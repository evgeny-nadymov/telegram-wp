// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Windows.Media;
using Windows.Storage.FileProperties;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using TelegramClient.Converters;
using TelegramClient.Resources;
#if WP8
using Windows.Storage;
#endif
#if WP81
using Windows.Foundation;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage.AccessCache;
#endif
using Telegram.Api.TL;
using TelegramClient.ViewModels.Media;
using Action = System.Action;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public EditVideoViewModel VideoEditor { get; set; }

        public void SendVideo(StorageFile file)
        {
            //threadpool
            if (file == null) return;

            if (VideoEditor == null)
            {
                VideoEditor = new EditVideoViewModel(ContinueSendVideo, GetUsernameHintsExternal, With);
                VideoEditor.SetVideoFile(file);
                NotifyOfPropertyChange(() => VideoEditor);
            }
            else
            {
                VideoEditor.SetVideoFile(file);

                BeginOnUIThread(() => VideoEditor.OpenEditor());
            }
        }

#if WP81

        private static async void TranscodeFile(MediaEncodingProfile profile, TimeSpan trimStartTime, TimeSpan trimStopTime, StorageFile srcFile, StorageFile destFile, Action<StorageFile, ulong> callback, Action<double> progressCallback, Action<IAsyncActionWithProgress<double>> faultCallback)
        {
            profile = profile ?? await MediaEncodingProfile.CreateFromFileAsync(srcFile);

            var transcoder = new MediaTranscoder
            {
                TrimStartTime = trimStartTime,
                TrimStopTime = trimStopTime
            };
            var prepareOp = await transcoder.PrepareFileTranscodeAsync(srcFile, destFile, profile);
            
            //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("TranscodeFile\nvideo=[{0}x{1} {2}]\naudio=[{3}]\ntrim_start={4} trim_end={5}", profile.Video.Width, profile.Video.Height, profile.Video.Bitrate, profile.Audio != null ? profile.Audio.Bitrate : 0, trimStartTime, trimStopTime));

            if (prepareOp.CanTranscode)
            {
                var transcodeOp = prepareOp.TranscodeAsync();
                transcodeOp.Progress += (result, progress) =>
                {
                    progressCallback.SafeInvoke(progress);
                };
                transcodeOp.Completed += async (o, e) =>
                {
                    var properties = await destFile.GetBasicPropertiesAsync();
                    var size = properties.Size;

                    TranscodeComplete(o, e, () => callback(destFile, size), faultCallback);
                };
            }
            else
            {
                faultCallback.SafeInvoke(null);

                switch (prepareOp.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.CodecWasNotFound, AppResources.Error, MessageBoxButton.OK));
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.ProfileIsInvalid, AppResources.Error, MessageBoxButton.OK));
                        break;
                    default:
                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.UnknownFailure, AppResources.Error, MessageBoxButton.OK));
                        break;
                }
            }
        }

        private static void TranscodeComplete(IAsyncActionWithProgress<double> asyncInfo, AsyncStatus asyncStatus, Action callback, Action<IAsyncActionWithProgress<double>> faultCallback)
        {
            asyncInfo.GetResults();
            if (asyncInfo.Status == AsyncStatus.Completed)
            {
                callback.SafeInvoke();
            }
            else if (asyncInfo.Status == AsyncStatus.Canceled)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("Transcode canceled result " + asyncInfo.Status);
                faultCallback.SafeInvoke(asyncInfo);
            }
            else
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("Transcode error result=" + asyncInfo.Status + " exception \n" + asyncInfo.ErrorCode);
                faultCallback.SafeInvoke(asyncInfo);
            }
        }

        private static async void GetCompressedFile(CompressingVideoFile file, Action<StorageFile, ulong> callback, Action<double> progressCallback, Action<IAsyncActionWithProgress<double>> faultCallback)
        {
            var fileName = Path.GetFileName(file.Source.Name);
            var videoParameters = string.Empty;
            if (file.EncodingProfile != null)
            {
                videoParameters = string.Format("{0}_{1}_{2}_{3}", file.EncodingProfile.Video.Width, file.EncodingProfile.Video.Height, file.EncodingProfile.Video.Bitrate, file.EncodingProfile.Video.FrameRate);
            }
            var audioParameters = string.Empty;
            if (file.EncodingProfile != null)
            {
                audioParameters = file.EncodingProfile.Audio != null ? file.EncodingProfile.Audio.Bitrate.ToString() : "0";
            }

            var hashString = string.Format("{0}_{1}_{2}_{3}", audioParameters, videoParameters, file.TrimStartTime, file.TrimStopTime);

            var transcodedFileName = string.Format("vid_{0}_{1}", hashString.GetHashCode(), fileName);

            //Telegram.Api.Helpers.Execute.ShowDebugMessage(transcodedFileName + Environment.NewLine + hashString);

            var fulltranscodedFileName = Path.Combine(KnownFolders.CameraRoll.Path, transcodedFileName);
            if (File.Exists(fulltranscodedFileName))
            {
                StorageFile transcodedFile = null;
                ulong transcodedLength = 0;
                try
                {
                    transcodedFile = await KnownFolders.CameraRoll.GetFileAsync(transcodedFileName);
                    if (transcodedFile != null)
                    {
                        transcodedLength = (ulong) new FileInfo(fulltranscodedFileName).Length;
                    }
                }
                catch (Exception ex)
                {
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("Get transcoded file ex: \n" + ex);    
                }

                if (transcodedFile != null && transcodedLength > 0)
                {
                    callback.SafeInvoke(transcodedFile, transcodedLength);
                    return;
                }
            }

            var dest = await KnownFolders.CameraRoll.CreateFileAsync(transcodedFileName, CreationCollisionOption.ReplaceExisting);
            TranscodeFile(file.EncodingProfile, file.TrimStartTime, file.TrimStopTime, file.Source, dest, callback, progressCallback, faultCallback);
        } 

        public void ContinueSendVideo(CompressingVideoFile videoFile)
        {
            if (videoFile == null) return;

            var file = videoFile.Source;
            if (file == null) return;

            if (!CheckDocumentSize(videoFile.Size))
            {
                MessageBox.Show(
                    string.Format(AppResources.MaximumFileSizeExceeded, MediaSizeConverter.Convert((int)Telegram.Api.Constants.MaximumUploadedFileSize)),
                    AppResources.Error,
                    MessageBoxButton.OK);
                return;
            }

            // to get access to the file with StorageFile.GetFileFromPathAsync in future
            AddFileToFutureAccessList(file);

            var documentAttributeVideo = new TLDocumentAttributeVideo
            {
                Duration = new TLInt((int) videoFile.Duration),
                W = new TLInt((int) videoFile.Width),
                H = new TLInt((int) videoFile.Height)
            };

            var documentAttributeFileName = new TLDocumentAttributeFileName
            {
                FileName = new TLString(file.Name)
            };

            var document = new TLDocument54
            {
                Id = TLLong.Random(),
                AccessHash = new TLLong(0),
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                MimeType = new TLString(file.ContentType),
                Size = new TLInt((int) videoFile.Size),
                Thumb = videoFile.ThumbPhoto ?? new TLPhotoSizeEmpty { Type = TLString.Empty },
                DCId = new TLInt(0),
                Version = new TLInt(0),
                Attributes = new TLVector<TLDocumentAttributeBase> { documentAttributeFileName, documentAttributeVideo }
            };

            if (videoFile.EncodingProfile != null 
                && videoFile.EncodingProfile.Audio == null
                && videoFile.Size < Telegram.Api.Constants.GifMaxSize
                && TLString.Equals(document.MimeType, new TLString("video/mp4"), StringComparison.OrdinalIgnoreCase))
            {
                document.Attributes.Add(new TLDocumentAttributeAnimated());
            }

            var media = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = document, IsoFileName = file.Path, File = file, Caption = TLString.Empty };

            var caption = string.IsNullOrEmpty(videoFile.Caption) ? TLString.Empty : new TLString(videoFile.Caption);
            var message = GetMessage(caption, media);

            if (videoFile.TimerSpan != null && videoFile.TimerSpan.Seconds > 0)
            {
                message.NotListened = true;
                var ttlMessageMedia = message.Media as ITTLMessageMedia;
                if (ttlMessageMedia != null)
                {
                    ttlMessageMedia.TTLSeconds = new TLInt(videoFile.TimerSpan.Seconds);
                }
            }

            _mentions = videoFile.Mentions;
            var processedText = string.Empty;
            var entities = GetEntities(message.Message.ToString(), out processedText);
            _mentions = null;

            if (entities.Count > 0)
            {
                message.Message = new TLString(processedText);
                message.Entities = new TLVector<TLMessageEntityBase>(entities);
            }

            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        m => SendVideoInternal(message, videoFile)));
            });
        }
#endif

        private void SendVideo(RecordedVideo recorderVideo)
        {
            if (recorderVideo == null) return;

            long size = 0;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = storage.OpenFile(recorderVideo.FileName, FileMode.Open, FileAccess.Read))
                {
                    size = file.Length;
                }
            }

            long photoSize = 0;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = storage.OpenFile(recorderVideo.FileName + ".jpg", FileMode.Open, FileAccess.Read))
                {
                    photoSize = file.Length;
                }
            }

            var volumeId = TLLong.Random();
            var localId = TLInt.Random();
            var secret = TLLong.Random();

            var thumbLocation = new TLFileLocation //TODO: replace with TLFileLocationUnavailable
            {
                DCId = new TLInt(0),
                VolumeId = volumeId,
                LocalId = localId,
                Secret = secret,
            };

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                thumbLocation.VolumeId,
                thumbLocation.LocalId,
                thumbLocation.Secret);

            // заменяем имя на стандартное для всех каритинок
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                store.CopyFile(recorderVideo.FileName + ".jpg", fileName, true);
                store.DeleteFile(recorderVideo.FileName + ".jpg");
            }

            var thumbSize = new TLPhotoSize
            {
                W = new TLInt(640),
                H = new TLInt(480),
                Size = new TLInt((int) photoSize),
                Type = new TLString(""),
                Location = thumbLocation,
            };

            var documentAttributeVideo = new TLDocumentAttributeVideo
            {
                Duration = new TLInt((int)recorderVideo.Duration),
                W = new TLInt(640),
                H = new TLInt(480)
            };

            var document = new TLDocument54
            {
                Id = new TLLong(0),
                AccessHash = new TLLong(0),
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                MimeType = new TLString("video/mp4"),
                Size = new TLInt((int)size),
                Thumb = thumbSize,
                DCId = new TLInt(0),
                Version = new TLInt(0),
                Attributes = new TLVector<TLDocumentAttributeBase> { documentAttributeVideo }
            };

            var media = new TLMessageMediaDocument75
            {
                Flags = new TLInt(0),
                FileId = recorderVideo.FileId ?? TLLong.Random(),
                Document = document,
                IsoFileName = recorderVideo.FileName,
                Caption = TLString.Empty
            };

            var message = GetMessage(TLString.Empty, media);

            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        m => SendVideoInternal(message, null)));
            });
        }

#if WP81
        private void SendCompressedVideoInternal(TLMessage message, StorageFile file)
#else
        private void SendCompressedVideoInternal(TLMessage message, object o)
#endif
        {
            var documentMedia = message.Media as TLMessageMediaDocument45;
            if (documentMedia != null)
            {
                var fileName = documentMedia.IsoFileName;
                if (string.IsNullOrEmpty(fileName)) return;

                var video = documentMedia.Document as TLDocument22;
                if (video == null) return;


                byte[] thumbBytes = null;
                var photoThumb = video.Thumb as TLPhotoSize;
                if (photoThumb != null)
                {
                    var location = photoThumb.Location as TLFileLocation;
                    if (location == null) return;

                    var thumbFileName = String.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);

                    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var thumbFile = storage.OpenFile(thumbFileName, FileMode.Open, FileAccess.Read))
                        {
                            thumbBytes = new byte[thumbFile.Length];
                            thumbFile.Read(thumbBytes, 0, thumbBytes.Length);
                        }
                    }
                }

                var fileId = message.Media.FileId ?? TLLong.Random();
                message.Media.FileId = fileId;
                message.Media.UploadingProgress = 0.001;
                //return;
#if WP81
                if (file != null)
                {
                    UploadVideoFileManager.UploadFile(fileId, message.IsGif(), message, file);
                }
                else if (!string.IsNullOrEmpty(fileName))
                {
                    UploadVideoFileManager.UploadFile(fileId, message, fileName);
                }
                else
                {
                    return;
                }
#else
                UploadVideoFileManager.UploadFile(fileId, message, fileName);
#endif

                if (thumbBytes != null)
                {
                    var fileId2 = TLLong.Random();
                    UploadFileManager.UploadFile(fileId2, message.Media, thumbBytes);
                }

                return;
            }

            var videoMedia = message.Media as TLMessageMediaVideo;
            if (videoMedia != null)
            {
                var fileName = videoMedia.IsoFileName;
                if (string.IsNullOrEmpty(fileName)) return;

                var video = videoMedia.Video as TLVideo;
                if (video == null) return;


                byte[] thumbBytes = null;
                var photoThumb = video.Thumb as TLPhotoSize;
                if (photoThumb != null)
                {
                    var location = photoThumb.Location as TLFileLocation;
                    if (location == null) return;

                    var thumbFileName = String.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);

                    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var thumbFile = storage.OpenFile(thumbFileName, FileMode.Open, FileAccess.Read))
                        {
                            thumbBytes = new byte[thumbFile.Length];
                            thumbFile.Read(thumbBytes, 0, thumbBytes.Length);
                        }
                    }
                }

                var fileId = message.Media.FileId ?? TLLong.Random();
                message.Media.FileId = fileId;
                message.Media.UploadingProgress = 0.001;

#if WP81
                if (file != null)
                {
                    UploadVideoFileManager.UploadFile(fileId, message.IsGif(), message, file);
                }
                else if (!string.IsNullOrEmpty(fileName))
                {
                    UploadVideoFileManager.UploadFile(fileId, message, fileName);
                }
                else
                {
                    return;
                }
#else
                UploadVideoFileManager.UploadFile(fileId, message, fileName);
#endif

                if (thumbBytes != null)
                {
                    var fileId2 = TLLong.Random();
                    UploadFileManager.UploadFile(fileId2, message.Media, thumbBytes);
                }

                return;
            }
        }

#if WP81
        private void SendVideoInternal(TLMessage message, CompressingVideoFile file)
        {
            if (file.EncodingProfile != null
                || file.TrimStartTime.Ticks > 0 
                || file.TrimStopTime.Ticks > 0)
            {
                message.Status = MessageStatus.Compressing;
                message.Media.UploadingProgress = 0.001;
                message.Media.CompressingProgress = 0.0;

                GetCompressedFile(
                    file,
                    async (compressedFile, compressedSize) =>
                    {
                        if (message.Status == MessageStatus.Failed)
                        {
                            return;
                        }
                        //var videoProperties = await file.Source.Properties.GetVideoPropertiesAsync();
                        //var videoProperties2 = await compressedFile.Properties.GetVideoPropertiesAsync();

                        message.Media.IsoFileName = compressedFile.Path;
                        message.Media.File = compressedFile;
                        message.Media.CompressingProgress = 0.0;
                        message.Status = MessageStatus.Sending;
                        var mediaDocument = message.Media as TLMessageMediaDocument45;
                        if (mediaDocument != null)
                        {
                            var document = mediaDocument.Document as TLDocument;
                            if (document != null)
                            {
                                if (file.EncodingProfile != null
                                    && file.EncodingProfile.Audio == null
                                    && compressedSize < Telegram.Api.Constants.GifMaxSize)
                                {
                                    // copy to local storage
                                    var localFile = await compressedFile.CopyAsync(ApplicationData.Current.LocalFolder,
                                        document.GetFileName(), NameCollisionOption.ReplaceExisting);
                                    message.Media.IsoFileName = localFile.Path;
                                    message.Media.File = localFile;
                                }

                                document.Size = new TLInt((int) compressedSize);
                                SendCompressedVideoInternal(message, compressedFile);
                            }
                        }

                        var mediaVideo = message.Media as TLMessageMediaVideo;
                        if (mediaVideo != null)
                        {
                            var video = mediaVideo.Video as TLVideo;
                            if (video != null)
                            {
                                video.Size = new TLInt((int) compressedSize);
                                SendCompressedVideoInternal(message, compressedFile);
                            }
                        }
                    },
                    progress =>
                    {
                        message.Media.CompressingProgress = progress / 100.0;
                    },
                    error =>
                    {
                        message.Status = MessageStatus.Failed;
                        message.Media.CompressingProgress = 0.0;
                    });
            }
            else
            {
                SendCompressedVideoInternal(message, file.Source);
            }
        }
#else
        private void SendVideoInternal(TLMessage message, object videoFile)
        {
            SendCompressedVideoInternal(message, videoFile);
        }
#endif
    }
}
