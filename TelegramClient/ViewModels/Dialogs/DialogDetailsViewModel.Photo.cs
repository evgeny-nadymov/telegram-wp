// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.Storage.FileProperties;
using Telegram.Api.Helpers;
using Telegram.Logs;
#if WP81
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography.Core;
#endif
using Windows.Storage;
using Windows.Storage.Streams;
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.ViewModels.Media;
using Buffer = Windows.Storage.Streams.Buffer;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        private ImageEditorViewModel _imageEditor;

        public ImageEditorViewModel ImageEditor
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel ImageEditor");
                return _imageEditor;
            }
            protected set { _imageEditor = value; }
        }

        private MultiImageEditorViewModel _multiImageEditor;

        public MultiImageEditorViewModel MultiImageEditor
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel MultiImageEditor");
                return _multiImageEditor;
            }
            protected set { _multiImageEditor = value; }
        }

#if WP81

        public static async Task<Stream> GetPhotoThumbnailAsync(StorageFile file, ThumbnailMode mode, uint requestedSize, ThumbnailOptions options)
        {
            if (file == null) return null;

            try
            {
                var thumbnail = await file.GetThumbnailAsync(mode, requestedSize, options);
                if (thumbnail != null)
                {
                    return thumbnail.AsStream();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString());
            }

            try
            {
                var stream = await file.OpenReadAsync();
                var photo = await ResizeJpeg(stream, 99 * 2, null, null);
                if (photo != null && photo.Bytes != null)
                {
                    return new MemoryStream(photo.Bytes);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString());
            }

            return null;
        }

        public async Task<TLMessage25> GetPhotoMessage(StorageFile file)
        {
            //var stopwatch = Stopwatch.StartNew();
            var threadId = Thread.CurrentThread.ManagedThreadId;

            var volumeId = TLLong.Random();
            var localId = TLInt.Random();
            var secret = TLLong.Random();

            var fileLocation = new TLFileLocation
            {
                VolumeId = volumeId,
                LocalId = localId,
                Secret = secret,
                DCId = new TLInt(0),        //TODO: remove from here, replace with FileLocationUnavailable
                //Buffer = p.Bytes
            };

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                fileLocation.VolumeId,
                fileLocation.LocalId,
                fileLocation.Secret);

            var stream = await file.OpenReadAsync();

            //System.Diagnostics.Debug.WriteLine(threadId + " " + "GetPhotoMessage OpenRead " + stopwatch.Elapsed);
            var resizedPhoto = await ResizeJpeg(stream, Constants.DefaultImageSize, file.DisplayName, fileName);

            //System.Diagnostics.Debug.WriteLine(threadId + " " + "GetPhotoMessage ResizeJpeg " + stopwatch.Elapsed);

            var photoSize = new TLPhotoSize
            {
                Type = TLString.Empty,
                W = new TLInt(resizedPhoto.Width),
                H = new TLInt(resizedPhoto.Height),
                Location = fileLocation,
                Size = new TLInt(resizedPhoto.Bytes.Length)
            };

            volumeId = TLLong.Random();
            localId = TLInt.Random();
            secret = TLLong.Random();

            var previewFileLocation = new TLFileLocation
            {
                VolumeId = volumeId,
                LocalId = localId,
                Secret = secret,
                DCId = new TLInt(0),        //TODO: remove from here, replace with FileLocationUnavailable
                //Buffer = p.Bytes
            };

            var previewFileName = String.Format("{0}_{1}_{2}.jpg",
                previewFileLocation.VolumeId,
                previewFileLocation.LocalId,
                previewFileLocation.Secret);

            var photo = new TLPhoto56
            {
                Flags = new TLInt(0),
                Id = new TLLong(0),
                AccessHash = new TLLong(0),
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                Sizes = new TLVector<TLPhotoSizeBase> { photoSize },
            };

            // to improve performance for bulk photo preview generation
            Execute.BeginOnThreadPool(async () =>
            {
                stream = await file.OpenReadAsync();

                var previewPhoto = await ResizeJpeg(stream, 90, file.DisplayName, previewFileName);

                var previewPhotoSize = new TLPhotoSize
                {
                    Type = new TLString("s"),
                    W = new TLInt(previewPhoto.Width),
                    H = new TLInt(previewPhoto.Height),
                    Location = previewFileLocation,
                    Size = new TLInt(previewPhoto.Bytes.Length)
                };

                Execute.BeginOnUIThread(() =>
                {
                    photo.Sizes.Add(previewPhotoSize);
                });
            });

            var media = new TLMessageMediaPhoto75 { Flags = new TLInt(0), Photo = photo, Caption = TLString.Empty, File = resizedPhoto.File };

            return GetMessage(TLString.Empty, media);
        }

        private async void SendPhoto(IReadOnlyCollection<StorageFile> files)
        {
            //threadpool
            if (files == null || files.Count == 0) return;

            if (MultiImageEditor != null && MultiImageEditor.IsOpen)
            {
                BeginOnUIThread(async () => await MultiImageEditor.AddFiles(new List<StorageFile>(files)));

                return;
            }

            //var message = await GetPhotoMessage(files.First());

            if (MultiImageEditor == null)
            {
                MultiImageEditor = new MultiImageEditorViewModel(SendPhoto, GetUsernameHintsExternal, With)
                {
                    CurrentItem = new PhotoFile{ File = files.First() },
                    Files = files,
                    ContinueAction = ContinueSendPhoto,
                    GetPhotoMessage = file =>
                    {
                        var m = GetPhotoMessage(file).Result;
                        return m;
                    }
                };
                NotifyOfPropertyChange(() => MultiImageEditor);
            }
            else
            {
                MultiImageEditor.CurrentItem = new PhotoFile { File = files.First() };
                MultiImageEditor.Files = files;

                BeginOnUIThread(() => MultiImageEditor.OpenEditor());
            }

            // fast preview for first item
            Execute.BeginOnThreadPool(async () =>
            {
                var message = await GetPhotoMessage(files.First());
                Execute.BeginOnUIThread(() =>
                {
                    MultiImageEditor.CurrentItem.Message = message;
                    MultiImageEditor.NotifyOfPropertyChange(() => MultiImageEditor.CurrentItem);
                    MultiImageEditor.CurrentItem.NotifyOfPropertyChange(() => MultiImageEditor.CurrentItem.Self);
                    if (files.Count == 1)
                    {
                        MultiImageEditor.IsDoneEnabled = true;
                    }
                });
            });
        }

        public static async Task<Photo> ResizeJpeg(IRandomAccessStream chosenPhoto, uint size, string originalFileName, string localFileName, double? quality = null)
        {
            Photo photo = null;
            var orientedPixelHeight = 0u;
            var orientedPixelWidth = 0u;
            //using (var sourceStream = chosenPhoto)
            {
                var decoder = await BitmapDecoder.CreateAsync(chosenPhoto);
                if (decoder.DecoderInformation != null)
                {
                    var maxDimension = Math.Max(decoder.PixelWidth, decoder.PixelHeight);
                    var scale = (double)size / maxDimension;
                    if (scale < 1.0)
                    {
                        var orientedScaledHeight = (uint) (decoder.OrientedPixelHeight*scale);
                        var orientedScaledWidth = (uint) (decoder.OrientedPixelWidth*scale);
                        var scaledHeight = (uint) (decoder.PixelHeight*scale);
                        var scaledWidth = (uint) (decoder.PixelWidth*scale);

                        var transform = new BitmapTransform { ScaledHeight = scaledHeight, ScaledWidth = scaledWidth, InterpolationMode = BitmapInterpolationMode.Fant };
                        var pixelData = await decoder.GetPixelDataAsync(
                            decoder.BitmapPixelFormat,
                            decoder.BitmapAlphaMode,
                            transform,
                            ExifOrientationMode.RespectExifOrientation,
                            ColorManagementMode.DoNotColorManage);

                        using (var destinationStream = new InMemoryRandomAccessStream())
                        {
                            var propertySet = new BitmapPropertySet();
                            if (quality.HasValue && quality > 0.0 && quality <= 1.0)
                            {
                                var qualityValue = new BitmapTypedValue(quality, Windows.Foundation.PropertyType.Single);
                                propertySet.Add("ImageQuality", qualityValue);
                            }
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, destinationStream, propertySet);
                            encoder.SetPixelData(decoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied, orientedScaledWidth, orientedScaledHeight, decoder.DpiX, decoder.DpiY, pixelData.DetachPixelData());
                            await encoder.FlushAsync();

                            var reader = new DataReader(destinationStream.GetInputStreamAt(0));
                            var bytes = new byte[destinationStream.Size];
                            await reader.LoadAsync((uint) destinationStream.Size);
                            reader.ReadBytes(bytes);

                            photo = new Photo
                            {
                                Bytes = bytes,
                                Width = (int) orientedScaledWidth,
                                Height = (int) orientedScaledHeight,
                                FileName = originalFileName
                            };

                            if (!string.IsNullOrEmpty(localFileName))
                            {
                                photo.File = await SaveToLocalFolderAsync(destinationStream.AsStream(), localFileName);
                            }
                        }
                    }

                    orientedPixelHeight = decoder.OrientedPixelHeight;
                    orientedPixelWidth = decoder.OrientedPixelWidth;
                }
            }

            if (photo == null)
            {
                var reader = new DataReader(chosenPhoto.GetInputStreamAt(0));
                var bytes = new byte[chosenPhoto.Size];
                await reader.LoadAsync((uint)chosenPhoto.Size);
                reader.ReadBytes(bytes);

                photo = new Photo
                {
                    Bytes = bytes,
                    Width = (int)orientedPixelWidth,
                    Height = (int)orientedPixelHeight,
                    FileName = originalFileName
                };

                if (!string.IsNullOrEmpty(localFileName))
                {
                    photo.File = await SaveToLocalFolderAsync(chosenPhoto.AsStream(), localFileName);
                }
            }

            chosenPhoto.Dispose();

            return photo;
        }

        public static async Task<byte[]> ComputeMD5(IRandomAccessStream stream)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var inputStream = stream.GetInputStreamAt(0);
            uint capacity = 1024 * 1024;
            var buffer = new Buffer(capacity);
            var hash = alg.CreateHash();

            while (true)
            {
                await inputStream.ReadAsync(buffer, capacity, InputStreamOptions.None);
                if (buffer.Length > 0)
                    hash.Append(buffer);
                else
                    break;
            }

            return hash.GetValueAndReset().ToArray();

            //string hashText = CryptographicBuffer.EncodeToHexString(hash.GetValueAndReset()).ToUpper();
            //var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            ////IBuffer buff = 
            //var hashed = alg.HashData(str.)
            //var res = CryptographicBuffer.EncodeToHexString(hashed);
            //return res;
        }

        public static async Task<StorageFile> SaveToLocalFolderAsync(Stream file, string fileName)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var storageFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
            {
                await file.CopyToAsync(outputStream);
            }

            return storageFile;
        }
#endif

        private void SendPhoto(Photo p)
        {
            var volumeId = TLLong.Random();
            var localId = TLInt.Random();
            var secret = TLLong.Random();

            var fileLocation = new TLFileLocation
            {
                VolumeId = volumeId,
                LocalId = localId,
                Secret = secret,
                DCId = new TLInt(0),        //TODO: remove from here, replace with FileLocationUnavailable
                //Buffer = p.Bytes
            };

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                fileLocation.VolumeId,
                fileLocation.LocalId,
                fileLocation.Secret);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = store.CreateFile(fileName))
                {
                    fileStream.Write(p.Bytes, 0, p.Bytes.Length);
                }
            }

            var photoSize = new TLPhotoSize
            {
                Type = TLString.Empty,
                W = new TLInt(p.Width),
                H = new TLInt(p.Height),
                Location = fileLocation,
                Size = new TLInt(p.Bytes.Length)
            };

            var photo = new TLPhoto56
            {
                Flags = new TLInt(0),
                Id = new TLLong(0),
                AccessHash = new TLLong(0),
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                Sizes = new TLVector<TLPhotoSizeBase> { photoSize },   
            };

            var media = new TLMessageMediaPhoto75 { Flags = new TLInt(0), Photo = photo, Caption = TLString.Empty };

            var message = GetMessage(TLString.Empty, media);

            if (ImageEditor == null)
            {
                ImageEditor = new ImageEditorViewModel
                {
                    CurrentItem = message,
                    ContinueAction = ContinueSendPhoto
                };
                NotifyOfPropertyChange(() => ImageEditor);
            }
            else
            {
                ImageEditor.CurrentItem = message;
            }

            BeginOnUIThread(() => ImageEditor.OpenEditor());
        }

        private void ContinueSendPhoto(TLMessage34 message)
        {
            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                BeginOnThreadPool(() =>
                 CacheService.SyncSendingMessage(
                     message, previousMessage,
                     m => UploadPhotoInternal(message)));
            });
        }

#if WP8 && MULTIPLE_PHOTOS
        private void ContinueSendPhoto(IList<PhotoFile> photos)
        {
            var reply = Reply as TLMessage;

            var messages = new List<TLMessage>();
            foreach (var photo in photos)
            {
                var message = photo.Message as TLMessage73;
                if (message != null)
                {
                    if (reply != null)
                    {
                        message.ReplyToMsgId = reply.Id;
                        message.Reply = reply;
                    }

                    _mentions = photo.Mentions;
                    var processedText = string.Empty;
                    var entities = GetEntities(message.Message.ToString(), out processedText);
                    _mentions = null;

                    if (entities.Count > 0)
                    {
                        message.Message = new TLString(processedText);
                        message.Entities = new TLVector<TLMessageEntityBase>(entities);
                    }
                }

                messages.Add(photo.Message);
            }

            BeginOnUIThread(() => SendMessages(messages, UploadPhotoInternal));
        }
#endif

        private void UploadPhotoInternal(IList<TLMessage> messages)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];

                var fileId = TLLong.Random();
                message.Media.FileId = fileId;
                message.Media.UploadingProgress = 0.001;
                UploadFileManager.UploadFile(fileId, message, message.Media.File);
            }
        }

        private void UploadPhotoInternal(IList<TLMessage34> messages)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                messages[i].Status = MessageStatus.Sending;
                UploadPhotoInternal(messages[i]);
            }
        }

        private void UploadPhotoInternal(TLMessage34 message)
        {
            var photo = ((TLMessageMediaPhoto)message.Media).Photo as TLPhoto;
            if (photo == null) return;

            var photoSize = photo.Sizes[0] as TLPhotoSize;
            if (photoSize == null) return;

            var fileLocation = photoSize.Location;
            if (fileLocation == null) return;

            byte[] bytes = null;
            var fileName = String.Format("{0}_{1}_{2}.jpg",
                fileLocation.VolumeId,
                fileLocation.LocalId,
                fileLocation.Secret);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    if (fileStream.Length > 0)
                    {
                        bytes = new byte[fileStream.Length];
                        fileStream.Read(bytes, 0, bytes.Length);
                    }
                }
            }

            if (bytes == null) return;

            var md5Bytes = Telegram.Api.Helpers.Utils.ComputeMD5(bytes);
            var md5Checksum = BitConverter.ToInt64(md5Bytes, 0);
            
            StateService.GetServerFilesAsync(
                results =>
                {
                    var serverFile = results.FirstOrDefault(result => result.MD5Checksum.Value == md5Checksum);

#if MULTIPLE_PHOTOS
                    serverFile = null;
#endif

                    if (serverFile != null)
                    {
                        var media = serverFile.Media;
                        var captionMedia = media as IInputMediaCaption;
                        if (captionMedia == null)
                        {
                            var inputMediaPhoto = serverFile.Media as TLInputMediaPhoto;
                            if (inputMediaPhoto != null)
                            {
                                var inputMediaPhoto75 = new TLInputMediaPhoto75(inputMediaPhoto);
                                captionMedia = inputMediaPhoto75;
                                media = inputMediaPhoto75;
                                serverFile.Media = inputMediaPhoto75;
                                StateService.SaveServerFilesAsync(results);
                            }
                            var inputMediaUploadedPhoto = serverFile.Media as TLInputMediaUploadedPhoto;
                            if (inputMediaUploadedPhoto != null)
                            {
                                var inputMediaUploadedPhoto75 = new TLInputMediaUploadedPhoto75(inputMediaUploadedPhoto, null);
                                captionMedia = inputMediaUploadedPhoto75;
                                media = inputMediaUploadedPhoto75;
                                serverFile.Media = inputMediaUploadedPhoto75;
                                StateService.SaveServerFilesAsync(results);
                            }
                        }

                        if (captionMedia != null)
                        {
                            captionMedia.Caption = message.Message ?? TLString.Empty;
                        }

                        var ttlMedia = media as IInputTTLMedia;
                        if (ttlMedia != null)
                        {
                            ttlMedia.TTLSeconds = ((TLMessageMediaPhoto70) message.Media).TTLSeconds;
                        }

                        message.InputMedia = media;
                        UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    }
                    else
                    {
                        var fileId = TLLong.Random();
                        message.Media.FileId = fileId;
                        message.Media.UploadingProgress = 0.001;
                        UploadFileManager.UploadFile(fileId, message, bytes);
                    }
                });
        }
    }
}
