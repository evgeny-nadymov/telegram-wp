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
using Caliburn.Micro;
using Telegram.Api;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.Views.Controls;
#if WP8
using Windows.Storage;
using TelegramClient_Opus;
#endif

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel :
        Telegram.Api.Aggregator.IHandle<MessagesRemovedEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLEncryptedChatBase>,
        Telegram.Api.Aggregator.IHandle<TLDecryptedMessageBase>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEncryptedMessagesRead>,
        Telegram.Api.Aggregator.IHandle<UploadProgressChangedEventArgs>,
        Telegram.Api.Aggregator.IHandle<ProgressChangedEventArgs>,
        Telegram.Api.Aggregator.IHandle<DownloadingCanceledEventArgs>,
        Telegram.Api.Aggregator.IHandle<UploadingCanceledEventArgs>,
        Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        Telegram.Api.Aggregator.IHandle<SetMessagesTTLEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateContactLinkBase>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserStatus>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEncryptedChatTyping>,
        Telegram.Api.Aggregator.IHandle<TLUpdatePrivacy>,
        Telegram.Api.Aggregator.IHandle<TLUpdateWebPage>
    {

        public void Handle(TLUpdateWebPage updateWebPage)
        {
            Execute.BeginOnUIThread(() =>
            {
                var webPageBase = updateWebPage.WebPage;

                foreach (var webPageKeyValue in _webPagesCache)
                {
                    var mediaBase = webPageKeyValue.Value;
                    var webPageMessageMedia = mediaBase as TLMessageMediaWebPage;
                    if (webPageMessageMedia != null)
                    {
                        var webPage = webPageMessageMedia.WebPage;
                        if (webPage != null)
                        {
                            if (webPage.Id.Value == webPageBase.Id.Value)
                            {
                                webPageMessageMedia.WebPage = webPageBase;

                                if (string.Equals(Text, webPageKeyValue.Key))
                                {
                                    if (webPageBase is TLWebPage || webPageBase is TLWebPagePending)
                                    {
                                        SaveReply();

                                        Reply = new TLDecryptedMessagesContainter { WebPageMedia = webPageMessageMedia };
                                    }
                                    else
                                    {
                                        RestoreReply();
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                foreach (var item in Items)
                {
                    var message45 = item as TLDecryptedMessage45;
                    if (message45 != null)
                    {
                        var mediaWebPage = message45.Media as TLDecryptedMessageMediaWebPage;
                        if (mediaWebPage != null)
                        {
                            var webPage = mediaWebPage.WebPage;
                            if (webPage != null && webPage.Id.Value == updateWebPage.WebPage.Id.Value)
                            {
                                mediaWebPage.WebPage = updateWebPage.WebPage;
                                message45.NotifyOfPropertyChange(() => message45.MediaSelf);
                            }
                        }
                    }
                }
            });
        }

        public void Handle(MessagesRemovedEventArgs args)
        {
            //if (With == args.Dialog.With && args.DecryptedMessage != null)
            //{
            //    BeginOnUIThread(() =>
            //    {
            //        Items.Remove(args.Message);

            //        IsEmptyDialog = Items.Count == 0 && LazyItems.Count == 0;
            //    });
            //}
        }

        public void Handle(ProgressChangedEventArgs args)
        {
            var media = args.Item.Owner as TLDecryptedMessageMediaBase;
            if (media != null)
            {
                var delta = args.Progress - media.DownloadingProgress;

                if (delta > 0.0)
                {
                    media.DownloadingProgress = args.Progress;
                }
            }
        }

        public void Handle(DownloadableItem item)
        {
            var webPage = item.Owner as TLWebPage;
            if (webPage != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    var messages = UngroupEnumerator(Items).OfType<TLDecryptedMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLDecryptedMessageMediaWebPage;
                        if (media != null && media.WebPage == webPage)
                        {
                            media.NotifyOfPropertyChange(() => media.Photo);
                            media.NotifyOfPropertyChange(() => media.Self);
                            break;
                        }
                    }
                });
            }

            var decryptedMessage = item.Owner as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                var mediaExternalDocument = decryptedMessage.Media as TLDecryptedMessageMediaExternalDocument;
                if (mediaExternalDocument != null)
                {
                    decryptedMessage.NotifyOfPropertyChange(() => decryptedMessage.Self);
                }
            }

            var decryptedMedia = item.Owner as TLDecryptedMessageMediaBase;
            if (decryptedMedia != null)
            {
                decryptedMessage = UngroupEnumerator(Items).OfType<TLDecryptedMessage>().FirstOrDefault(x => x.Media == decryptedMedia);
                if (decryptedMessage != null)
                {
                    var mediaPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                    if (mediaPhoto != null)
                    {
                        mediaPhoto.DownloadingProgress = 1.0;
                        var fileName = item.IsoFileName;
                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            byte[] buffer;
                            using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                            {
                                buffer = new byte[file.Length];
                                file.Read(buffer, 0, buffer.Length);
                            }
                            var fileLocation = decryptedMessage.Media.File as TLEncryptedFile;
                            if (fileLocation == null) return;
                            var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, mediaPhoto.Key.Data,
                                mediaPhoto.IV.Data, false);

                            var newFileName = String.Format("{0}_{1}_{2}.jpg",
                                fileLocation.Id,
                                fileLocation.DCId,
                                fileLocation.AccessHash);

                            using (var file = store.OpenFile(newFileName, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                            }

                            store.DeleteFile(fileName);
                        }

                        if (!decryptedMedia.IsCanceled)
                        {
                            decryptedMedia.NotifyOfPropertyChange(() => decryptedMedia.Self);
                        }

                    }

                    var mediaVideo = decryptedMessage.Media as TLDecryptedMessageMediaVideo;
                    if (mediaVideo != null)
                    {
                        mediaVideo.DownloadingProgress = 1.0;
                        var fileName = item.IsoFileName;
                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            byte[] buffer;
                            using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                            {
                                buffer = new byte[file.Length];
                                file.Read(buffer, 0, buffer.Length);
                            }
                            var fileLocation = decryptedMessage.Media.File as TLEncryptedFile;
                            if (fileLocation == null) return;
                            var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, mediaVideo.Key.Data, mediaVideo.IV.Data, false);

                            var newFileName = String.Format("{0}_{1}_{2}.mp4",
                                fileLocation.Id,
                                fileLocation.DCId,
                                fileLocation.AccessHash);

                            using (var file = store.OpenFile(newFileName, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                            }

                            store.DeleteFile(fileName);
                        }
                    }

                    var mediaDocument = decryptedMessage.Media as TLDecryptedMessageMediaDocument;
                    if (mediaDocument != null)
                    {
                        if (decryptedMessage.IsVoice())
                        {
                            var fileLocation = decryptedMessage.Media.File as TLEncryptedFile;
                            if (fileLocation == null) return;

                            var fileName = item.IsoFileName;
                            var decryptedFileName = String.Format("audio{0}_{1}.mp3",
                                fileLocation.Id,
                                fileLocation.AccessHash);
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                byte[] buffer;
                                using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                {
                                    buffer = new byte[file.Length];
                                    file.Read(buffer, 0, buffer.Length);
                                }
                                var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, mediaDocument.Key.Data, mediaDocument.IV.Data, false);

                                using (var file = store.OpenFile(decryptedFileName, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                                }

                                store.DeleteFile(fileName);

                            }

                            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                            {
                                MessagePlayerControl.ConvertAndSaveOpusToWav(mediaDocument);

                                mediaDocument.DownloadingProgress = 1.0;
                            });
                        }
                        else if (decryptedMessage.IsVideo())
                        {
                            mediaDocument.DownloadingProgress = 1.0;
                            var fileName = item.IsoFileName;
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                byte[] buffer;
                                using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                {
                                    buffer = new byte[file.Length];
                                    file.Read(buffer, 0, buffer.Length);
                                }
                                var fileLocation = decryptedMessage.Media.File as TLEncryptedFile;
                                if (fileLocation == null) return;
                                var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, mediaDocument.Key.Data, mediaDocument.IV.Data, false);

                                var newFileName = String.Format("{0}_{1}_{2}.mp4",
                                    fileLocation.Id,
                                    fileLocation.DCId,
                                    fileLocation.AccessHash);

                                using (var file = store.OpenFile(newFileName, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                                }

                                store.DeleteFile(fileName);
                            }
                        }
                        else
                        {
                            mediaDocument.DownloadingProgress = 1.0;
                            var fileName = item.IsoFileName;
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                byte[] buffer;
                                using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                {
                                    buffer = new byte[file.Length];
                                    file.Read(buffer, 0, buffer.Length);
                                }
                                var fileLocation = decryptedMessage.Media.File as TLEncryptedFile;
                                if (fileLocation == null) return;
                                var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, mediaDocument.Key.Data, mediaDocument.IV.Data, false);

                                var newFileName = String.Format("{0}_{1}_{2}.{3}",
                                    fileLocation.Id,
                                    fileLocation.DCId,
                                    fileLocation.AccessHash,
                                    fileLocation.FileExt ?? mediaDocument.FileExt);

                                using (var file = store.OpenFile(newFileName, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                                }

                                store.DeleteFile(fileName);
                            }
                        }
                    }

                    var mediaAudio = decryptedMessage.Media as TLDecryptedMessageMediaAudio;
                    if (mediaAudio != null)
                    {
                        var fileLocation = decryptedMessage.Media.File as TLEncryptedFile;
                        if (fileLocation == null) return;

                        var fileName = item.IsoFileName;
                        var decryptedFileName = String.Format("audio{0}_{1}.mp3",
                            fileLocation.Id,
                            fileLocation.AccessHash);
                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            byte[] buffer;
                            using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                            {
                                buffer = new byte[file.Length];
                                file.Read(buffer, 0, buffer.Length);
                            }
                            var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, mediaAudio.Key.Data, mediaAudio.IV.Data, false);

                            using (var file = store.OpenFile(decryptedFileName, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                            }

                            store.DeleteFile(fileName);

                        }

                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                        {
                            MessagePlayerControl.ConvertAndSaveOpusToWav(mediaAudio);

                            mediaAudio.DownloadingProgress = 1.0;
                        });
                    }

                }
            }
        }

        public void Handle(UploadingCanceledEventArgs args)
        {
            var owner = args.Item.Owner;
            var messageLayer = owner as TLDecryptedMessageLayer;
            if (messageLayer != null)
            {
                owner = messageLayer.Message;
            }

            var message = owner as TLDecryptedMessage;
            if (message != null)
            {
                var photo = message.Media as TLDecryptedMessageMediaPhoto;
                if (photo != null)
                {
                    message.Media.UploadingProgress = 0.0;
                    message.Status = MessageStatus.Failed;
                }

                var video = message.Media as TLDecryptedMessageMediaVideo;
                if (video != null)
                {
                    message.Media.UploadingProgress = 0.0;
                    message.Status = MessageStatus.Failed;
                }

                var document = message.Media as TLDecryptedMessageMediaDocument;
                if (document != null)
                {
                    message.Media.UploadingProgress = 0.0;
                    message.Status = MessageStatus.Failed;
                }

                var audio = message.Media as TLDecryptedMessageMediaAudio;
                if (audio != null)
                {
                    message.Media.UploadingProgress = 0.0;
                    message.Status = MessageStatus.Failed;
                }
                message.NotifyOfPropertyChange(() => message.Status);
            }
        }

        public void Handle(DownloadingCanceledEventArgs args)
        {
            var media = args.Item.Owner as TLDecryptedMessageMediaBase;
            if (media != null)
            {
                media.DownloadingProgress = 0.0;
            }
        }

        public void Handle(SetMessagesTTLEventArgs args)
        {
            if (Chat != null
                && args.Chat.Id.Value == Chat.Id.Value)
            {
                Execute.BeginOnUIThread(() =>
                {
                    Items.Insert(0, args.Message);
                    NotifyOfPropertyChange(() => DescriptionVisibility);
                });
            }
        }

        public void Handle(TLUpdateContactLinkBase update)
        {
            if (With != null
                && With.Index == update.UserId.Value)
            {
                Execute.BeginOnUIThread(() =>
                {
                    Subtitle = GetSubtitle(With);
                    NotifyOfPropertyChange(() => Subtitle);
                    NotifyOfPropertyChange(() => With);
                });
            }
        }

        public void Handle(TLUpdateUserStatus updateUserStatus)
        {
            if (With != null
                && With.Index == updateUserStatus.UserId.Value)
            {
                Execute.BeginOnUIThread(() =>
                {
                    Subtitle = GetSubtitle(With);
                    NotifyOfPropertyChange(() => Subtitle);
                });
            }
        }

        public void Handle(TLUpdateEncryptedChatTyping encryptedChatTyping)
        {
            if (Chat != null
                && With != null
                && Chat.Index == encryptedChatTyping.ChatId.Value)
            {
                BeginOnThreadPool(() => InputTypingManager.AddTypingUser(With.Index, new TLSendMessageTypingAction()));
            }
        }

        public void Handle(UploadProgressChangedEventArgs args)
        {
            var message = GetDecryptedMessage(args.Item.Owner);
            if (message != null)
            {
                var media = message.Media;
                if (media != null)
                {
                    var delta = args.Progress - media.UploadingProgress;

                    if (delta > 0.0)
                    {
                        media.UploadingProgress = args.Progress;
                    }
                    return;
                }
            }

            var photo = args.Item.Owner as TLDecryptedMessageMediaPhoto;
            if (photo != null)
            {
                var delta = args.Progress - photo.UploadingProgress;

                if (delta > 0.0)
                {
                    photo.UploadingProgress = args.Progress;
                }
                return;
            }
        }

        public void Handle(TLDecryptedMessageBase decryptedMessage)
        {
            if (Chat != null
                && decryptedMessage.ChatId.Value == Chat.Id.Value)
            {
                System.Diagnostics.Debug.WriteLine("Handle random_id={0} date={1} qts={2}", decryptedMessage.RandomId, decryptedMessage.Date, decryptedMessage.Qts);

                var serviceMessage = decryptedMessage as TLDecryptedMessageService;
                if (serviceMessage != null)
                {
                    var action = serviceMessage.Action;

                    var typingAction = action as TLDecryptedMessageActionTyping;
                    if (typingAction != null)
                    {
                        var cancelAction = typingAction.Action as TLSendMessageCancelAction;
                        if (cancelAction != null)
                        {
                            InputTypingManager.RemoveTypingUser(With.Index);
                        }
                        else
                        {
                            InputTypingManager.AddTypingUser(With.Index, typingAction.Action);
                        }
                    }

                    var setMessageTTLAction = action as TLDecryptedMessageActionSetMessageTTL;
                    if (setMessageTTLAction != null)
                    {
                        Chat.MessageTTL = setMessageTTLAction.TTLSeconds;
                    }

                    var flushHistoryAction = action as TLDecryptedMessageActionFlushHistory;
                    if (flushHistoryAction != null)
                    {
                        Execute.BeginOnUIThread(() => Items.Clear());
                        CacheService.ClearDecryptedHistoryAsync(Chat.Id);
                    }

                    var readMessagesAction = action as TLDecryptedMessageActionReadMessages;
                    if (readMessagesAction != null)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            foreach (var randomId in readMessagesAction.RandomIds)
                            {
                                foreach (var item in UngroupEnumerator(Items))
                                {
                                    if (item.RandomId.Value == randomId.Value)
                                    {
                                        item.Status = MessageStatus.Read;
                                        if (item.TTL != null && item.TTL.Value > 0)
                                        {
                                            item.DeleteDate = new TLLong(DateTime.Now.Ticks + Chat.MessageTTL.Value * TimeSpan.TicksPerSecond);
                                        }

                                        var decryptedMessage17 = item as TLDecryptedMessage17;
                                        if (decryptedMessage17 != null)
                                        {
                                            var decryptedMediaPhoto = decryptedMessage17.Media as TLDecryptedMessageMediaPhoto;
                                            if (decryptedMediaPhoto != null)
                                            {
                                                if (decryptedMediaPhoto.TTLParams == null)
                                                {
                                                    var ttlParams = new TTLParams();
                                                    ttlParams.IsStarted = true;
                                                    ttlParams.Total = decryptedMessage17.TTL.Value;
                                                    ttlParams.StartTime = DateTime.Now;
                                                    ttlParams.Out = decryptedMessage17.Out.Value;

                                                    decryptedMediaPhoto.TTLParams = ttlParams;
                                                }
                                            }

                                            var decryptedMediaVideo17 = decryptedMessage17.Media as TLDecryptedMessageMediaVideo17;
                                            if (decryptedMediaVideo17 != null)
                                            {
                                                if (decryptedMediaVideo17.TTLParams == null)
                                                {
                                                    var ttlParams = new TTLParams();
                                                    ttlParams.IsStarted = true;
                                                    ttlParams.Total = decryptedMessage17.TTL.Value;
                                                    ttlParams.StartTime = DateTime.Now;
                                                    ttlParams.Out = decryptedMessage17.Out.Value;

                                                    decryptedMediaVideo17.TTLParams = ttlParams;
                                                }
                                            }

                                            var decryptedMediaAudio17 = decryptedMessage17.Media as TLDecryptedMessageMediaAudio17;
                                            if (decryptedMediaAudio17 != null)
                                            {
                                                if (decryptedMediaAudio17.TTLParams == null)
                                                {
                                                    var ttlParams = new TTLParams();
                                                    ttlParams.IsStarted = true;
                                                    ttlParams.Total = decryptedMessage17.TTL.Value;
                                                    ttlParams.StartTime = DateTime.Now;
                                                    ttlParams.Out = decryptedMessage17.Out.Value;

                                                    decryptedMediaAudio17.TTLParams = ttlParams;
                                                }
                                            }

                                            var decryptedMediaDocument45 = decryptedMessage17.Media as TLDecryptedMessageMediaDocument45;
                                            if (decryptedMediaDocument45 != null && (decryptedMessage17.IsVoice() || decryptedMessage17.IsVideo()))
                                            {
                                                if (decryptedMediaDocument45.TTLParams == null)
                                                {
                                                    var ttlParams = new TTLParams();
                                                    ttlParams.IsStarted = true;
                                                    ttlParams.Total = decryptedMessage17.TTL.Value;
                                                    ttlParams.StartTime = DateTime.Now;
                                                    ttlParams.Out = decryptedMessage17.Out.Value;

                                                    decryptedMediaDocument45.TTLParams = ttlParams;
                                                }

                                                var message45 = decryptedMessage17 as TLDecryptedMessage45;
                                                if (message45 != null)
                                                {
                                                    message45.SetListened();
                                                }
                                                decryptedMediaDocument45.NotListened = false;
                                                decryptedMediaDocument45.NotifyOfPropertyChange(() => decryptedMediaDocument45.NotListened);
                                            }
                                        }

                                        item.NotifyOfPropertyChange(() => item.Status);
                                        break;
                                    }
                                }
                            }
                        });
                    }

                    var deleteMessagesAction = action as TLDecryptedMessageActionDeleteMessages;
                    if (deleteMessagesAction != null)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            var group = new Dictionary<long, TLDecryptedMessageMediaGroup>();
                            foreach (var randomId in deleteMessagesAction.RandomIds)
                            {
                                for (var i = 0; i < Items.Count; i++)
                                {
                                    var groupedMessage = false;
                                    var message73 = Items[i] as TLDecryptedMessage73;
                                    if (message73 != null && message73.GroupedId != null)
                                    {
                                        var mediaGroup = message73.Media as TLDecryptedMessageMediaGroup;
                                        if (mediaGroup != null)
                                        {
                                            groupedMessage = true;
                                            for (var k = 0; k < mediaGroup.Group.Count; k++)
                                            {
                                                if (mediaGroup.Group[k].RandomId.Value == randomId.Value)
                                                {
                                                    mediaGroup.Group.RemoveAt(k);
                                                    if (mediaGroup.Group.Count == 0)
                                                    {
                                                        Items.Remove(message73);
                                                    }
                                                    else
                                                    {
                                                        group[message73.GroupedId.Value] = mediaGroup;
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (!groupedMessage && Items[i].RandomId.Value == randomId.Value)
                                    {
                                        Items.RemoveAt(i);
                                        break;
                                    }
                                }
                            }

                            foreach (var mediaGroup in group.Values)
                            {
                                mediaGroup.RaiseCalculate();
                            }

                            CacheService.DeleteDecryptedMessages(deleteMessagesAction.RandomIds);
                        });
                    }
                }

                if (!TLUtils.IsDisplayedDecryptedMessage(decryptedMessage))
                {
                    return;
                }

                ProcessMessages(new List<TLDecryptedMessageBase> { decryptedMessage });

                Execute.OnUIThread(() =>
                {
                    var addedGrouped = false;
                    var message73 = decryptedMessage as TLDecryptedMessage73;
                    if (message73 != null && message73.GroupedId != null && Items.Count > 0)
                    {
                        var previousMessage = Items[0] as TLDecryptedMessage73;
                        if (previousMessage != null
                            && previousMessage.GroupedId != null
                            && previousMessage.GroupedId.Value == message73.GroupedId.Value)
                        {
                            Items.RemoveAt(0);
                            var items = new List<TLDecryptedMessageBase>();
                            var mediaGroup = previousMessage.Media as TLDecryptedMessageMediaGroup;
                            if (mediaGroup != null)
                            {
                                items.Add(message73);

                                for (var i = mediaGroup.Group.Count - 1; i >= 0; i--)
                                {
                                    items.Add(mediaGroup.Group[i]);
                                }
                            }
                            else
                            {
                                items.Add(message73);
                                items.Add(previousMessage);
                            }

                            ProcessGroupedMessages(items);

                            for (var j = 0; j < items.Count; j++)
                            {
                                InsertMessageInOrder(items[j]);
                            }

                            addedGrouped = true;
                        }
                    }

                    var position = -1;
                    if (!addedGrouped)
                    {
                        position = InsertMessageInOrder(decryptedMessage);
                        System.Diagnostics.Debug.WriteLine("Handle.Insert random_id={0} date={1} position={2}", decryptedMessage.RandomId, decryptedMessage.Date, position);
                    }
                    else
                    {
                        position = 0;
                    }
                    NotifyOfPropertyChange(() => DescriptionVisibility);

                    if (position != -1)
                    {
                        ReadMessages(decryptedMessage);
                        if (decryptedMessage is TLDecryptedMessage)
                        {
                            InputTypingManager.RemoveTypingUser(With.Index);
                        }
                    }
                });
            }
        }

        private void SplitGroupedMessages(IList<TLLong> messages)
        {
            if (messages.Count == 0) return;

            var messagesDict = messages.ToDictionary(x => x.Value);
            for (var i = 0; i < Items.Count; i++)
            {
                var message = Items[i] as TLDecryptedMessage73;
                if (message != null)
                {
                    var mediaGroup = message.Media as TLDecryptedMessageMediaGroup;
                    if (mediaGroup != null)
                    {
                        var removed = false;
                        for (var j = 0; j < mediaGroup.Group.Count; j++)
                        {
                            if (messagesDict.ContainsKey(mediaGroup.Group[j].RandomIndex))
                            {
                                removed = true;
                                break;
                            }
                        }

                        if (removed)
                        {
                            Items.RemoveAt(i);

                            var group = new TLVector<TLDecryptedMessageBase>(mediaGroup.Group.Reverse().ToList());

                            ProcessGroupedMessages(group);

                            for (var j = 0; j < group.Count; j++)
                            {
                                Items.Insert(i++, group[j]);
                            }
                            i--;
                        }
                    }
                }
            }
        }

        public void Handle(TLEncryptedChatBase encryptedChat)
        {
            if (encryptedChat != null
                && Chat != null
                && encryptedChat.Id.Value == Chat.Id.Value)
            {
                Chat = encryptedChat;
                if (SecretChatDebug != null)
                {
                    SecretChatDebug.UpdateChat(encryptedChat);
                }
                NotifyOfPropertyChange(() => AppBarCommandString);
                NotifyOfPropertyChange(() => IsAppBarCommandVisible);
                NotifyOfPropertyChange(() => IsApplicationBarVisible);
            }
        }

        public void Handle(TLUpdateEncryptedMessagesRead update)
        {
            return; //UpdatesService.ProcessUpdateInternal уже обработали там

            if (update != null
                && Chat != null
                && update.ChatId.Value == Chat.Id.Value)
            {
                Execute.BeginOnUIThread(() =>
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Out.Value)
                        {
                            if (Items[i].Status == MessageStatus.Confirmed)
                            //&& Items[i].Date.Value <= update.MaxDate.Value) // здесь надо учитывать смещение по времени
                            {
                                Items[i].Status = MessageStatus.Read;
                                Items[i].NotifyOfPropertyChange("Status");
                                if (Items[i].TTL != null && Items[i].TTL.Value > 0)
                                {
                                    var decryptedMessage = Items[i] as TLDecryptedMessage17;
                                    if (decryptedMessage != null)
                                    {
                                        var decryptedPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                                        if (decryptedPhoto != null && Items[i].TTL.Value <= 60.0)
                                        {
                                            continue;
                                        }

                                        var decryptedVideo17 = decryptedMessage.Media as TLDecryptedMessageMediaVideo17;
                                        if (decryptedVideo17 != null && Items[i].TTL.Value <= 60.0)
                                        {
                                            continue;
                                        }

                                        var decryptedAudio17 = decryptedMessage.Media as TLDecryptedMessageMediaAudio17;
                                        if (decryptedAudio17 != null && Items[i].TTL.Value <= 60.0)
                                        {
                                            continue;
                                        }

                                        var decryptedDocument45 = decryptedMessage.Media as TLDecryptedMessageMediaDocument45;
                                        if (decryptedDocument45 != null && (Items[i].IsVoice() || Items[i].IsVideo()) && Items[i].TTL.Value <= 60.0)
                                        {
                                            continue;
                                        }
                                    }

                                    Items[i].DeleteDate = new TLLong(DateTime.Now.Ticks + Chat.MessageTTL.Value * TimeSpan.TicksPerSecond);
                                }
                            }
                            else if (Items[i].Status == MessageStatus.Read)
                            {
                                break;
                            }
                        }
                    }
                });
            }
        }

        public void Handle(TLUpdatePrivacy privacy)
        {
            if (privacy.Key is TLPrivacyKeyStatusTimestamp)
            {
                MTProtoService.GetFullUserAsync((With).ToInputUser(),
                    userFull =>
                    {
                        With = userFull.User;
                        NotifyOfPropertyChange(() => With);
                        Subtitle = GetSubtitle(With);
                        NotifyOfPropertyChange(() => Subtitle);
                    });
            }
        }
    }
}
