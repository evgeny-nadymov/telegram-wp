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
#if WP8
using Windows.Storage;
#endif
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
#if WP81
        public async void SendDocument(StorageFile file)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            if (file == null) return;

            var properties = await file.GetBasicPropertiesAsync();
            var size = properties.Size;

            if (!DialogDetailsViewModel.CheckDocumentSize(size))
            {
                MessageBox.Show(string.Format(AppResources.MaximumFileSizeExceeded, MediaSizeConverter.Convert((int)Telegram.Api.Constants.MaximumUploadedFileSize)), AppResources.Error, MessageBoxButton.OK);
                return;
            }

            DialogDetailsViewModel.AddFileToFutureAccessList(file);

            var thumb = await DialogDetailsViewModel.GetFileThumbAsync(file) as TLPhotoSize;

            var dcId = TLInt.Random();
            var id = TLLong.Random();
            var accessHash = TLLong.Random();

            var fileLocation = new TLEncryptedFile
            {
                Id = id,
                AccessHash = accessHash,
                DCId = dcId,
                Size = new TLInt((int)size),
                KeyFingerprint = new TLInt(0),
                FileName = new TLString(Path.GetFileName(file.Name))
            };

            var keyIV = GenerateKeyIV();

            var decryptedMediaDocumentBase= GetDecryptedMediaDocument(file, chat, thumb, size, keyIV, fileLocation);
            var decryptedMediaDocument45 = decryptedMediaDocumentBase as TLDecryptedMessageMediaDocument45;
            if (decryptedMediaDocument45 != null)
            {
                if (string.Equals(decryptedMediaDocument45.FileExt, "webp", StringComparison.OrdinalIgnoreCase)
                    && decryptedMediaDocument45.DocumentSize < Telegram.Api.Constants.StickerMaxSize)
                {
                    decryptedMediaDocument45.MimeType = new TLString("image/webp");
                    decryptedMediaDocument45.Attributes.Add(new TLDocumentAttributeSticker29 { Alt = TLString.Empty, Stickerset = new TLInputStickerSetEmpty() });

                    var fileName = decryptedMediaDocument45.GetFileName();
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, fileName, NameCollisionOption.ReplaceExisting);
                }
                else if (string.Equals(decryptedMediaDocument45.FileExt, "mp3", StringComparison.OrdinalIgnoreCase))
                {
                    var musicProperties = await file.Properties.GetMusicPropertiesAsync();
                    if (musicProperties != null)
                    {
                        var documentAttributeAudio = new TLDocumentAttributeAudio32 { Duration = new TLInt(0), Title = TLString.Empty, Performer = TLString.Empty };
                        documentAttributeAudio.Duration = new TLInt((int)musicProperties.Duration.TotalSeconds);
                        if (!string.IsNullOrEmpty(musicProperties.Title))
                        {
                            documentAttributeAudio.Title = new TLString(musicProperties.Title);
                        }
                        if (!string.IsNullOrEmpty(musicProperties.Artist))
                        {
                            documentAttributeAudio.Performer = new TLString(musicProperties.Artist);
                        }
                        decryptedMediaDocument45.Attributes.Add(documentAttributeAudio);
                    }
                }
            }
            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, decryptedMediaDocumentBase, chat, true);

            BeginOnUIThread(() =>
            {
                InsertSendingMessage(decryptedTuple.Item1);
                RaiseScrollToBottom();
                NotifyOfPropertyChange(() => DescriptionVisibility);

                BeginOnThreadPool(() =>
                    CacheService.SyncDecryptedMessage(decryptedTuple.Item1, chat,
                        cachedMessage => SendDocumentInternal(file, decryptedTuple.Item2)));
            });
        }

        private static TLDecryptedMessageMediaBase GetDecryptedMediaDocument(Photo p, TLEncryptedChat chat, TLString thumb, TLInt thumbW, TLInt thumbH, TLString mimeType, Telegram.Api.WindowsPhone.Tuple<TLString, TLString> keyIV, TLEncryptedFile fileLocation)
        {
            TLDecryptedMessageMediaBase decryptedMediaDocument;
            var chat17 = chat as TLEncryptedChat17;
            if (chat17 != null)
            {
                if (chat17.Layer.Value >= Constants.MinSecretChatWithCaptionsLayer)
                {
                    decryptedMediaDocument = new TLDecryptedMessageMediaDocument45
                    {
                        Thumb = thumb,
                        ThumbW = thumbW,
                        ThumbH = thumbH,
                        FileName = new TLString(Path.GetFileName(p.FileName)),
                        MimeType = mimeType,
                        Size = new TLInt(p.Bytes.Length),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        Caption = TLString.Empty,

                        File = fileLocation,
                        UploadingProgress = 0.001
                    };
                }
                else
                {
                    decryptedMediaDocument = new TLDecryptedMessageMediaDocument
                    {
                        Thumb = thumb,
                        ThumbW = thumbW,
                        ThumbH = thumbH,
                        FileName = new TLString(Path.GetFileName(p.FileName)),
                        MimeType = mimeType,
                        Size = new TLInt(p.Bytes.Length),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,

                        File = fileLocation,
                        UploadingProgress = 0.001
                    };
                }
            }
            else
            {
                decryptedMediaDocument = new TLDecryptedMessageMediaDocument
                {
                    Thumb = thumb,
                    ThumbW = thumbW,
                    ThumbH = thumbH,
                    FileName = new TLString(Path.GetFileName(p.FileName)),
                    MimeType = mimeType,
                    Size = new TLInt(p.Bytes.Length),
                    Key = keyIV.Item1,
                    IV = keyIV.Item2,

                    File = fileLocation,
                    UploadingProgress = 0.001
                };
            }
            return decryptedMediaDocument;
        }

        private static TLDecryptedMessageMediaBase GetDecryptedMediaDocument(StorageFile file, TLEncryptedChat chat, TLPhotoSize thumb, ulong size, Telegram.Api.WindowsPhone.Tuple<TLString, TLString> keyIV, TLEncryptedFile fileLocation)
        {
            TLDecryptedMessageMediaBase decryptedMediaDocument;
            var chat17 = chat as TLEncryptedChat17;
            if (chat17 != null)
            {
                if (chat17.Layer.Value >= Constants.MinSecretChatWithCaptionsLayer)
                {
                    decryptedMediaDocument = new TLDecryptedMessageMediaDocument45
                    {
                        Thumb = thumb != null ? thumb.Bytes : TLString.Empty,
                        ThumbW = thumb != null ? thumb.W : new TLInt(0),
                        ThumbH = thumb != null ? thumb.H : new TLInt(0),
                        FileName = new TLString(Path.GetFileName(file.Name)),
                        MimeType = new TLString(file.ContentType),
                        Size = new TLInt((int) size),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        Caption = TLString.Empty,

                        File = fileLocation,
                        StorageFile = file,
                        UploadingProgress = 0.001
                    };
                }
                else
                {
                    decryptedMediaDocument = new TLDecryptedMessageMediaDocument
                    {
                        Thumb = thumb != null ? thumb.Bytes : TLString.Empty,
                        ThumbW = thumb != null ? thumb.W : new TLInt(0),
                        ThumbH = thumb != null ? thumb.H : new TLInt(0),
                        FileName = new TLString(Path.GetFileName(file.Name)),
                        MimeType = new TLString(file.ContentType),
                        Size = new TLInt((int) size),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        File = fileLocation,
                        StorageFile = file,
                        UploadingProgress = 0.001
                    };
                }
            }
            else
            {
                decryptedMediaDocument = new TLDecryptedMessageMediaDocument
                {
                    Thumb = thumb != null ? thumb.Bytes : TLString.Empty,
                    ThumbW = thumb != null ? thumb.W : new TLInt(0),
                    ThumbH = thumb != null ? thumb.H : new TLInt(0),
                    FileName = new TLString(Path.GetFileName(file.Name)),
                    MimeType = new TLString(file.ContentType),
                    Size = new TLInt((int) size),
                    Key = keyIV.Item1,
                    IV = keyIV.Item2,
                    File = fileLocation,
                    StorageFile = file,
                    UploadingProgress = 0.001
                };
            }
            return decryptedMediaDocument;
        }
#endif

        private void SendDocument(Photo p)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            var dcId = TLInt.Random();
            var id = TLLong.Random();
            var accessHash = TLLong.Random();

            var fileLocation = new TLEncryptedFile
            {
                Id = id,
                AccessHash = accessHash,
                DCId = dcId,
                Size = new TLInt(p.Bytes.Length),
                KeyFingerprint = new TLInt(0),
                FileName = new TLString(Path.GetFileName(p.FileName))
            };

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                fileLocation.Id,
                fileLocation.DCId,
                fileLocation.AccessHash);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = store.CreateFile(fileName))
                {
                    fileStream.Write(p.Bytes, 0, p.Bytes.Length);
                }
            }

            var keyIV = GenerateKeyIV();

            int thumbHeight;
            int thumbWidth;
            var thumb = ImageUtils.CreateThumb(p.Bytes, Constants.DocumentPreviewMaxSize, Constants.DocumentPreviewQuality, out thumbHeight, out thumbWidth);

            var decryptedMediaDocument = GetDecryptedMediaDocument(p, chat, TLString.FromBigEndianData(thumb), new TLInt(thumbWidth), new TLInt(thumbHeight), new TLString("image/jpeg"), keyIV, fileLocation);

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, decryptedMediaDocument, chat, true);

            InsertSendingMessage(decryptedTuple.Item1);
            RaiseScrollToBottom();
            NotifyOfPropertyChange(() => DescriptionVisibility);

            BeginOnThreadPool(() => 
                CacheService.SyncDecryptedMessage(decryptedTuple.Item1, chat, 
                    cachedMessage => SendDocumentInternal(p.Bytes, decryptedTuple.Item2)));
        }

#if WP81
        private void SendDocumentInternal(StorageFile storageFile, TLObject obj)
        {
            var message = GetDecryptedMessage(obj);
            if (message == null) return;

            var media = message.Media as TLDecryptedMessageMediaDocument;
            if (media == null) return;

            var file = media.File as TLEncryptedFile;
            if (file == null) return;

            UploadDocumentFileManager.UploadFile(file.Id, obj, storageFile, media.Key, media.IV);
        }
#endif

        private void SendDocumentInternal(byte[] data, TLObject obj)
        {
            var message = GetDecryptedMessage(obj);
            if (message == null) return;

            var media = message.Media as TLDecryptedMessageMediaDocument;
            if (media == null) return;
            var file = media.File as TLEncryptedFile;
            if (file == null) return;

            if (data == null)
            {
                var fileName = String.Format("{0}_{1}_{2}.jpg",
                    file.Id,
                    file.DCId,
                    file.AccessHash);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fileStream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                    {
                        data = new byte[fileStream.Length];
                        fileStream.Read(data, 0, data.Length);
                    }
                }
            }

            var encryptedBytes = Telegram.Api.Helpers.Utils.AesIge(data, media.Key.Data, media.IV.Data, true);
            UploadDocumentFileManager.UploadFile(file.Id, obj, encryptedBytes);
        }

        public void SendSticker(TLDocument22 document)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            TLDocumentAttributeSticker attributeSticker = null;
            var chat17 = Chat as TLEncryptedChat17;
            if (chat17 != null)
            {
                if (chat17.Layer.Value >= Constants.MinSecretChatWithCaptionsLayer)
                {
                    var attributeSticker29 = new TLDocumentAttributeSticker29 { Alt = TLString.Empty, Stickerset = new TLInputStickerSetEmpty() };
                    var attributes = document.Attributes;
                    if (attributes != null)
                    {
                        var attribute = document.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker) as TLDocumentAttributeSticker29;
                        if (attribute != null)
                        {
                            attributeSticker29.Alt = attribute.Alt;

                            var stickerSet = attribute.Stickerset as TLInputStickerSetShortName;
                            if (stickerSet == null)
                            {
                                var stickerSetId = attribute.Stickerset as TLInputStickerSetId;
                                if (stickerSetId != null)
                                {
                                    var allStickers = StateService.GetAllStickers() as TLAllStickers29;
                                    if (allStickers != null)
                                    {
                                        var set = allStickers.Sets.FirstOrDefault(x => x.Id.Value == stickerSetId.Id.Value);
                                        if (set != null && !TLString.IsNullOrEmpty(set.ShortName))
                                        {
                                            attributeSticker29.Stickerset = new TLInputStickerSetShortName { ShortName = set.ShortName };
                                        }
                                    }
                                }
                            }
                        }
                    }
                    attributeSticker = attributeSticker29;
                }
                else if (chat17.Layer.Value >= Constants.MinSecretChatWithStickersLayer)
                {
                    attributeSticker = new TLDocumentAttributeSticker();
                }
            }

            var stickerAttributes = new TLVector<TLDocumentAttributeBase>
            {
                new TLDocumentAttributeImageSize {H = document.ImageSizeH, W = document.ImageSizeW},
                new TLDocumentAttributeFileName {FileName = new TLString("sticker.webp")}
            };
            if (attributeSticker != null)
            {
                stickerAttributes.Add(attributeSticker);
            }
            var decryptedMediaExternalDocument = new TLDecryptedMessageMediaExternalDocument
            {
                Id = document.Id,
                AccessHash = document.AccessHash,
                Date= document.Date,
                MimeType = document.MimeType,
                Size = document.Size,
                Thumb = document.Thumb,
                DCId = document.DCId,
                Attributes = stickerAttributes
            };

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, decryptedMediaExternalDocument, chat);

            BeginOnUIThread(() =>
            {
                InsertSendingMessage(decryptedTuple.Item1);
                RaiseScrollToBottom();
                NotifyOfPropertyChange(() => DescriptionVisibility);
                Text = string.Empty;

                BeginOnThreadPool(() =>
                {
                    SendEncrypted(chat, decryptedTuple.Item2, MTProtoService, CacheService);
                });
            });
        }
    }
}
