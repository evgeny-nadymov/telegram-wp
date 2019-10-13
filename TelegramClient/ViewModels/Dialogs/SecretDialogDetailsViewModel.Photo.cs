// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Services;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
        public MultiImageEditorViewModel MultiImageEditor { get; protected set; }

        private void UploadPhotoInternal(byte[] data, TLObject obj)
        {
            var message = GetDecryptedMessage(obj);
            if (message == null) return;

            var media = message.Media as TLDecryptedMessageMediaPhoto;
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

            UploadFileManager.UploadFile(file.Id, obj, encryptedBytes);
        }

        private void SendPhoto(Photo p)
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
                KeyFingerprint = new TLInt(0)
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
            var thumb = ImageUtils.CreateThumb(p.Bytes, Constants.PhotoPreviewMaxSize, Constants.PhotoPreviewQuality, out thumbHeight, out thumbWidth);

            var decryptedMediaPhoto = new TLDecryptedMessageMediaPhoto
            {
                Thumb = TLString.FromBigEndianData(thumb),
                ThumbW = new TLInt(thumbWidth),
                ThumbH = new TLInt(thumbHeight),
                Key = keyIV.Item1,
                IV = keyIV.Item2,
                W = new TLInt(p.Width),
                H = new TLInt(p.Height),
                Size = new TLInt(p.Bytes.Length),

                File = fileLocation,

                UploadingProgress = 0.001
            };

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, decryptedMediaPhoto, chat, true);

            InsertSendingMessage(decryptedTuple.Item1);
            RaiseScrollToBottom();
            NotifyOfPropertyChange(() => DescriptionVisibility);

            BeginOnThreadPool(() => 
                CacheService.SyncDecryptedMessage(decryptedTuple.Item1, chat, 
                    cachedMessage => UploadPhotoInternal(p.Bytes, decryptedTuple.Item2)));
        }

        public async Task<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>> GetPhotoMessage(StorageFile file)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return null;

            if (file == null) return null;

            var properties = await file.GetBasicPropertiesAsync();
            var size = properties.Size;
            
            var thumb = await DialogDetailsViewModel.GetFileThumbAsync(file) as TLPhotoSize;

            var dcId = TLInt.Random();
            var id = TLLong.Random();
            var accessHash = TLLong.Random();

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                id,
                dcId,
                accessHash);

            var stream = await file.OpenReadAsync();
            var resizedPhoto = await DialogDetailsViewModel.ResizeJpeg(stream, Constants.DefaultImageSize, file.DisplayName, fileName);

            var keyIV = GenerateKeyIV();

            var fileLocation = new TLEncryptedFile
            {
                Id = id,
                AccessHash = accessHash,
                DCId = dcId,
                Size = new TLInt(resizedPhoto.Bytes.Length),
                KeyFingerprint = new TLInt(0),
                FileName = new TLString(Path.GetFileName(file.Name))
            };

            TLDecryptedMessageMediaPhoto decryptedMediaPhoto;
            var chat17 = chat as TLEncryptedChat17;
            if (chat17 != null && chat17.Layer.Value >= Constants.MinSecretChatWithCaptionsLayer)
            {
                decryptedMediaPhoto = new TLDecryptedMessageMediaPhoto45
                {
                    Thumb = thumb != null ? thumb.Bytes : TLString.Empty,
                    ThumbW = thumb != null ? thumb.W : new TLInt(0),
                    ThumbH = thumb != null ? thumb.H : new TLInt(0),
                    Size = new TLInt(resizedPhoto.Bytes.Length),
                    Key = keyIV.Item1,
                    IV = keyIV.Item2,
                    W = new TLInt(resizedPhoto.Width),
                    H = new TLInt(resizedPhoto.Height),
                    Caption = TLString.Empty,

                    File = fileLocation,
                    StorageFile = resizedPhoto.File,

                    UploadingProgress = 0.001
                };
            }
            else
            {
                decryptedMediaPhoto = new TLDecryptedMessageMediaPhoto
                {
                    Thumb = thumb != null ? thumb.Bytes : TLString.Empty,
                    ThumbW = thumb != null ? thumb.W : new TLInt(0),
                    ThumbH = thumb != null ? thumb.H : new TLInt(0),
                    Size = new TLInt(resizedPhoto.Bytes.Length),
                    Key = keyIV.Item1,
                    IV = keyIV.Item2,
                    W = new TLInt(resizedPhoto.Width),
                    H = new TLInt(resizedPhoto.Height),

                    File = fileLocation,
                    StorageFile = resizedPhoto.File,

                    UploadingProgress = 0.001
                };
            }

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, decryptedMediaPhoto, chat, true);

            return decryptedTuple;
        }

        private async void SendPhoto(IReadOnlyCollection<StorageFile> files)
        {
            var chat = Chat as TLEncryptedChat17;

            //threadpool
            if (files == null || files.Count == 0) return;

            if (MultiImageEditor != null && MultiImageEditor.IsOpen)
            {
                BeginOnUIThread(async () => await MultiImageEditor.AddFiles(new List<StorageFile>(files)));

                return;
            }

            //var decryptedTuple = await GetPhotoMessage(files.First());

            if (MultiImageEditor == null)
            {
                MultiImageEditor = new MultiImageEditorViewModel(SendPhoto, null, Chat)
                {
                    CurrentItem = new PhotoFile { File = files.First() },
                    Files = files,
                    ContinueAction = ContinueSendPhoto,
                    GetDecryptedPhotoMessage = file =>
                    {
                        var m = GetPhotoMessage(file).Result;
                        return m;
                    },
                    IsCaptionEnabled = chat != null && chat.Layer.Value >= Constants.MinSecretChatWithCaptionsLayer,
                    IsSecretChat = true
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
                    MultiImageEditor.CurrentItem.DecryptedTuple = message;
                    MultiImageEditor.NotifyOfPropertyChange(() => MultiImageEditor.CurrentItem);
                    MultiImageEditor.CurrentItem.NotifyOfPropertyChange(() => MultiImageEditor.CurrentItem.Self);
                    if (files.Count == 1)
                    {
                        MultiImageEditor.IsDoneEnabled = true;
                    }
                });
            });
        }

#if WP8 && MULTIPLE_PHOTOS
        private void ContinueSendPhoto(IList<PhotoFile> photos)
        {
            var reply = Reply as TLDecryptedMessage;

            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            var messages = new List<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>>();
            foreach (var photo in photos)
            {
                if (reply != null)
                {
                    var message = photo.DecryptedTuple.Item1 as TLDecryptedMessage45;
                    if (message != null)
                    {
                        message.ReplyToRandomMsgId = reply.RandomId;
                        message.Reply = reply;
                    }
                }

                messages.Add(photo.DecryptedTuple);
            }

            BeginOnUIThread(() => SendMessages(messages, UploadPhotoInternal));
        }
#endif

        private void SendMessages(IList<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>> messages, Action<IList<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>>> callback)
        {
            var uploadService = IoC.Get<IUploadService>();

            var tuple = messages.FirstOrDefault();
            var mediaMessage = tuple != null? tuple.Item1 as TLDecryptedMessage73 : null;
            var groupedId = mediaMessage != null ? mediaMessage.GroupedId : null;
            if (groupedId != null)
            {
                var messageMediaGroup = new TLDecryptedMessageMediaGroup { Group = new TLVector<TLDecryptedMessageBase>() };
                var message = new TLDecryptedMessage73
                {
                    Flags = new TLInt(0),
                    Media = messageMediaGroup,
                    Message = TLString.Empty,
                    RandomId = mediaMessage.RandomId,
                    RandomBytes = TLString.Random(Telegram.Api.Constants.MinRandomBytesLength),
                    ChatId = mediaMessage.ChatId,
                    FromId = mediaMessage.FromId,
                    Out = mediaMessage.Out,
                    Unread = mediaMessage.Unread,
                    Date = mediaMessage.Date,
                    TTL = mediaMessage.TTL,
                    Status = MessageStatus.Sending,
                    ReplyToRandomMsgId = mediaMessage.ReplyToRandomMsgId,
                    Reply = mediaMessage.Reply,
                    GroupedId = groupedId
                };
                message.SetMedia();

                for (var i = 0; i < messages.Count; i++)
                {
                    if (i % Constants.MaxGroupedMediaCount == 0)
                    {
                        if (messageMediaGroup.Group.Count > 0)
                        {
                            uploadService.AddGroup(message);
                            Items.Insert(0, message);
                        }

                        mediaMessage = messages[i].Item1 as TLDecryptedMessage73;
                        groupedId = mediaMessage != null ? mediaMessage.GroupedId : null;

                        messageMediaGroup = new TLDecryptedMessageMediaGroup { Group = new TLVector<TLDecryptedMessageBase>() };
                        message = new TLDecryptedMessage73
                        {
                            Flags = new TLInt(0),
                            Media = messageMediaGroup,
                            Message = TLString.Empty,
                            RandomId = mediaMessage.RandomId,
                            RandomBytes = TLString.Random(Telegram.Api.Constants.MinRandomBytesLength),
                            ChatId = mediaMessage.ChatId,
                            FromId = mediaMessage.FromId,
                            Out = mediaMessage.Out,
                            Unread = mediaMessage.Unread,
                            Date = mediaMessage.Date,
                            TTL = mediaMessage.TTL,
                            Status = MessageStatus.Sending,
                            ReplyToRandomMsgId = mediaMessage.ReplyToRandomMsgId,
                            Reply = mediaMessage.Reply,
                            GroupedId = groupedId
                        };
                        message.SetMedia();
                    }

                    messageMediaGroup.Group.Add(messages[i].Item1);
                }

                if (messageMediaGroup.Group.Count > 0)
                {
                    uploadService.AddGroup(message);
                    Items.Insert(0, message);
                }
            }
            else
            {
                foreach (var message in messages)
                {
                    Items.Insert(0, message.Item1);
                }
            }

            //IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
            Reply = null;

            RaiseScrollToBottom();
            NotifyOfPropertyChange(() => DescriptionVisibility);

            BeginOnThreadPool(() => CacheService.SyncDecryptedMessages(messages, Chat, callback.SafeInvoke));
        }

        private async void UploadPhotoInternal(IList<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>> messages)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                var obj = messages[i].Item2;

                var message = GetDecryptedMessage(obj);
                if (message == null) return;

                var media = message.Media as TLDecryptedMessageMediaPhoto;
                if (media == null) return;

                var file = media.File as TLEncryptedFile;
                if (file == null) return;

                var fileName = String.Format("{0}_{1}_{2}.jpg",
                    file.Id,
                    file.DCId,
                    file.AccessHash);

                var storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                if (storageFile == null) return;

                System.Diagnostics.Debug.WriteLine("Upload photo random_id={0} name={1}", messages[i].Item1.RandomId, storageFile.DisplayName);
                UploadDocumentFileManager.UploadFile(file.Id, obj, storageFile, media.Key, media.IV);
            }
        }

        public static Telegram.Api.WindowsPhone.Tuple<TLString, TLString> GenerateKeyIV()
        {
            var random = new Random();

            var key = new byte[32];
            var iv = new byte[32];
            random.NextBytes(key);
            random.NextBytes(iv);

            return new Telegram.Api.WindowsPhone.Tuple<TLString, TLString>(TLString.FromBigEndianData(key), TLString.FromBigEndianData(iv));
        }
    }
}
