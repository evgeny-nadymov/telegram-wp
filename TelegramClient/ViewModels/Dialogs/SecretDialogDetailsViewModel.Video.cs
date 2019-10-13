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
using Telegram.Api.TL;
using TelegramClient.Helpers;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
        private void SendVideoInternal(byte[] data, TLObject obj)
        {
            var message = GetDecryptedMessage(obj);
            if (message == null) return;

            var mediaVideo = message.Media as TLDecryptedMessageMediaVideo;
            if (mediaVideo == null) return;

            var fileLocation = mediaVideo.File as TLEncryptedFile;
            if (fileLocation == null) return;

            var fileName = String.Format("{0}_{1}_{2}.mp4",
                fileLocation.Id,
                fileLocation.DCId,
                fileLocation.AccessHash);

            if (data == null)
            {
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var file = storage.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                    {
                        data = new byte[file.Length];
                        file.Read(data, 0, data.Length);
                    }
                }
            }

            var encryptedBytes = Telegram.Api.Helpers.Utils.AesIge(data, mediaVideo.Key.Data, mediaVideo.IV.Data, true);
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = storage.OpenFile("encrypted." + fileName, FileMode.Create, FileAccess.Write))
                {
                    file.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
            }

            UploadVideoFileManager.UploadFile(fileLocation.Id, obj, "encrypted." + fileName);
        }

        private void SendVideo(string videoFileName, long duration)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            if (string.IsNullOrEmpty(videoFileName)) return;

            long size = 0;
            byte[] data;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = storage.OpenFile(videoFileName, FileMode.Open, FileAccess.Read))
                {
                    size = file.Length;
                    data = new byte[size];
                    file.Read(data, 0, data.Length);
                }
            }

            byte[] thumb;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = storage.OpenFile(videoFileName + ".jpg", FileMode.Open, FileAccess.Read))
                {
                    thumb = new byte[file.Length];
                    file.Read(thumb, 0, thumb.Length);
                }
            }

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
                FileName = new TLString(""),
                Duration = new TLInt((int)duration)
            };

            var fileName = String.Format("{0}_{1}_{2}.mp4",
                fileLocation.Id,
                fileLocation.DCId,
                fileLocation.AccessHash);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                store.CopyFile(videoFileName, fileName, true);
                store.DeleteFile(videoFileName);
            }

            var keyIV = GenerateKeyIV();

            int thumbHeight;
            int thumbWidth;
            thumb = ImageUtils.CreateThumb(thumb, Constants.VideoPreviewMaxSize, Constants.VideoPreviewQuality, out thumbHeight, out thumbWidth);

            TLDecryptedMessageMediaVideo decryptedMediaVideo;
            var encryptedChat17 = chat as TLEncryptedChat17;
            if (encryptedChat17 != null)
            {
                if (encryptedChat17.Layer.Value >= Constants.MinSecretChatWithAudioAsDocumentsLayer)
                {
                    decryptedMediaVideo = new TLDecryptedMessageMediaVideo45
                    {
                        Thumb = TLString.FromBigEndianData(thumb),
                        ThumbW = new TLInt(thumbWidth),
                        ThumbH = new TLInt(thumbHeight),
                        Duration = new TLInt((int)duration),
                        MimeType = new TLString("video/mp4"),
                        W = new TLInt(640),
                        H = new TLInt(480),
                        Size = new TLInt((int)size),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        Caption = TLString.Empty,

                        File = fileLocation,

                        UploadingProgress = 0.001
                    };
                }
                else
                {
                    decryptedMediaVideo = new TLDecryptedMessageMediaVideo17
                    {
                        Thumb = TLString.FromBigEndianData(thumb),
                        ThumbW = new TLInt(thumbWidth),
                        ThumbH = new TLInt(thumbHeight),
                        Duration = new TLInt((int)duration),
                        MimeType = new TLString("video/mp4"),
                        W = new TLInt(640),
                        H = new TLInt(480),
                        Size = new TLInt((int)size),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,

                        File = fileLocation,

                        UploadingProgress = 0.001
                    };
                }
            }
            else
            {
                decryptedMediaVideo = new TLDecryptedMessageMediaVideo
                {
                    Thumb = TLString.FromBigEndianData(thumb),
                    ThumbW = new TLInt(thumbWidth),
                    ThumbH = new TLInt(thumbHeight),
                    Duration = new TLInt((int) duration),
                    //MimeType = new TLString("video/mp4"),
                    W = new TLInt(640),
                    H = new TLInt(480),
                    Size = new TLInt((int) size),
                    Key = keyIV.Item1,
                    IV = keyIV.Item2,

                    File = fileLocation,

                    UploadingProgress = 0.001
                };
            }

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, decryptedMediaVideo, chat, true);

            InsertSendingMessage(decryptedTuple.Item1);
            RaiseScrollToBottom();
            NotifyOfPropertyChange(() => DescriptionVisibility);

            BeginOnThreadPool(() => 
                CacheService.SyncDecryptedMessage(decryptedTuple.Item1, chat, 
                    cachedMessage => SendVideoInternal(data, decryptedTuple.Item2)));
        }
    }
}
