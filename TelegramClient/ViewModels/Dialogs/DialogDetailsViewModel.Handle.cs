// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Telegram.Api;
using Telegram.Api.Extensions;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;
#if WP8
using Windows.Storage;
using TelegramClient_Opus;
#endif
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel :
        Telegram.Api.Aggregator.IHandle<TLUpdateContactLinkBase>,
        Telegram.Api.Aggregator.IHandle<TLMessageCommon>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserTyping>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatUserTyping>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserStatus>,
        Telegram.Api.Aggregator.IHandle<DialogRemovedEventArgs>,
        Telegram.Api.Aggregator.IHandle<MessagesRemovedEventArgs>,
        Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        //Telegram.Api.Aggregator.IHandle<UploadableItem>,
        Telegram.Api.Aggregator.IHandle<ProgressChangedEventArgs>,
        Telegram.Api.Aggregator.IHandle<UploadProgressChangedEventArgs>,
        Telegram.Api.Aggregator.IHandle<UploadingCanceledEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateCompletedEventArgs>,
        Telegram.Api.Aggregator.IHandle<ChannelUpdateCompletedEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdatePrivacy>,
        Telegram.Api.Aggregator.IHandle<DeleteMessagesEventArgs>,
        Telegram.Api.Aggregator.IHandle<ChannelAvailableMessagesEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateNotifySettings>,
        Telegram.Api.Aggregator.IHandle<TLUpdateWebPage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserBlocked>,
        Telegram.Api.Aggregator.IHandle<TLAllStickersBase>,
        Telegram.Api.Aggregator.IHandle<TLChannel>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChannel>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChannelPinnedMessage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateDraftMessage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateReadMessagesContents>,
        Telegram.Api.Aggregator.IHandle<ForwardGroupedEventArgs>
    //Telegram.Api.Aggregator.IHandle<TopMessageUpdatedEventArgs>
    {

        private int _addedCount = 0;

        private void InsertMessage(TLMessageCommon messageCommon)
        {
            ProcessMessages(new List<TLMessageBase> { messageCommon });

            Execute.BeginOnUIThread(() =>
            {
                var addedGrouped = false;
                var message73 = messageCommon as TLMessage73;
                if (message73 != null && message73.GroupedId != null && Items.Count > 0)
                {
                    var previousMessage = Items[0] as TLMessage73;
                    if (previousMessage != null
                        && previousMessage.GroupedId != null
                        && previousMessage.GroupedId.Value == message73.GroupedId.Value)
                    {
                        Items.RemoveAt(0);
                        var items = new List<TLMessageBase>();
                        var mediaGroup = previousMessage.Media as TLMessageMediaGroup;
                        if (mediaGroup != null)
                        {
                            items.Add(messageCommon);
                            foreach (var item in mediaGroup.Group)
                            {
                                items.Add(item);
                            }
                        }
                        else
                        {
                            items.Add(messageCommon);
                            items.Add(previousMessage);
                        }

                        items = items.OrderByDescending(x => x.Index).ToList();

                        ProcessGroupedMessages(items);

                        for (var j = 0; j < items.Count; j++)
                        {
                            TLDialog.InsertMessageInOrder(Items, items[j]);
                        }

                        addedGrouped = true;
                    }
                }
                var position = -1;
                if (!addedGrouped)
                {
                    position = TLDialog.InsertMessageInOrder(Items, messageCommon);
                    _addedCount++;
                }
                else
                {
                    position = 0;
                }

                if (position != -1)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var serviceMessage = Items[i] as TLMessageService;
                        if (serviceMessage != null && serviceMessage.Action is TLMessageActionUnreadMessages)
                        {
                            Items.RemoveAt(i--);
                        }
                    }

                    if (!messageCommon.Out.Value)
                    {
                        if (messageCommon.Unread.Value
                            && View != null
                            && View.IsScrollToBottomButtonVisible)
                        {
                            Counter++;
                        }

                        var message25 = messageCommon as TLMessage25;
                        if (message25 != null
                            && message25.Unread.Value
                            && message25.IsMention
                            && View != null
                            && View.IsScrollToBottomButtonVisible)
                        {
                            var dialog = CurrentDialog as TLDialog71;
                            if (dialog != null)
                            {
                                dialog.UnreadMentions = dialog.UnreadMentions ?? new TLVector<TLMessageBase>();
                                var added = false;
                                for (var j = 0; j < dialog.UnreadMentions.Count; j++)
                                {
                                    if (dialog.UnreadMentions[j].Index == message25.Index)
                                    {
                                        added = true;
                                        break;
                                    }
                                    if (dialog.UnreadMentions[j].Index < message25.Index)
                                    {
                                        dialog.UnreadMentions.Insert(j, message25);
                                        added = true;
                                        break;
                                    }
                                }
                                if (!added)
                                {
                                    dialog.UnreadMentions.Add(message25);
                                }

                                MentionsCounter++;

                                Execute.BeginOnUIThread(() =>
                                {
                                    View.ShowMentionButton();
                                });
                            }
                        }
                    }
                    else
                    {
                        Counter = 0;
                    }

                    var message = messageCommon as TLMessage;
                    if (message != null && !message.Out.Value)
                    {
                        var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null && mediaGeoLive.Active)
                        {
                            if (LocationPicker != null)
                            {
                                LocationPicker.UpdateLiveLocation(message);
                            }
                            if (LiveLocationBadge != null)
                            {
                                LiveLocationBadge.UpdateLiveLocation(message);
                            }
                        }
                    }

                    var message31 = messageCommon as TLMessage31;
                    if (message31 != null && !message31.Out.Value && message31.ReplyMarkup != null)
                    {
                        var fromId = message31.FromId;
                        var user = CacheService.GetUser(fromId) as TLUser;
                        if (user != null && user.IsBot)
                        {
                            SetReplyMarkup(message31);
                        }
                    }

                    Execute.BeginOnThreadPool(() =>
                    {
                        MarkAsRead(messageCommon);

                        if (messageCommon is TLMessage)
                        {
                            InputTypingManager.RemoveTypingUser(messageCommon.FromId.Value);
                        }
                    });
                }
            });
        }

        public void Handle(TLMessageCommon message)
        {
            if (message == null) return;
#if WP8
            if (!_isFirstSliceLoaded)
            {
                Execute.ShowDebugMessage("DialogDetailsViewModel.Handle(TLMessageCommon) _isFirstSliceLoaded=false");
                return;
            }
#endif

            //if (message.FromId.Value == StateService.CurrentUserId)
            //{
            //    if (message.IsSticker())
            //    {
            //        EmojiControl emojiControl;
            //        if (EmojiControl.TryGetInstance(out emojiControl))
            //        {
            //            var messageCommon = message as TLMessage;
            //            if (messageCommon != null)
            //            {
            //                var mediaDocument = messageCommon.Media as TLMessageMediaDocument;
            //                if (mediaDocument != null)
            //                {
            //                    var args = new StickerSelectedEventArgs { Sticker = new TLStickerItem { Document = mediaDocument.Document } };
            //                    emojiControl.UpdateRecentStickers(args);
            //                }
            //            }
            //        }
            //    }
            //}

            if (With is TLUserBase
                && message.ToId is TLPeerUser
                && !message.Out.Value
                && ((TLUserBase)With).Id.Value == message.FromId.Value)
            {
                InsertMessage(message);

                // switchPM
                if (_isActive)
                {
                    var message31 = message as TLMessage31;
                    if (message31 != null)
                    {
                        var keyboardRows = message31.ReplyMarkup as IReplyKeyboardRows;
                        if (keyboardRows != null)
                        {
                            var keyboardButtonSwitchInline =
                                keyboardRows.Rows.SelectMany(x => x.Buttons)
                                    .FirstOrDefault(x => x is TLKeyboardButtonSwitchInline);
                            if (keyboardButtonSwitchInline != null)
                            {
                                Send(message, keyboardButtonSwitchInline, true);
                            }
                        }
                    }
                }
            }
            else if (With is TLUserBase
                    && message.ToId is TLPeerUser
                    && message.Out.Value
                    && ((TLUserBase)With).Id.Value == message.ToId.Id.Value)
            {
                InsertMessage(message);
            }
            else if (With is TLChatBase
                    && ((message.ToId is TLPeerChat && ((TLChatBase)With).Id.Value == message.ToId.Id.Value)
                        || (message.ToId is TLPeerChannel && ((TLChatBase)With).Id.Value == message.ToId.Id.Value)))
            {
                InsertMessage(message);
                NotifyOfPropertyChange(() => With); // Title and appbar changing

                var messageService = message as TLMessageService;
                if (messageService != null)
                {
                    var chatMigrateToAction = messageService.Action as TLMessageActionChatMigrateTo;
                    if (chatMigrateToAction != null)
                    {
                        var channel = CacheService.GetChat(chatMigrateToAction.ChannelId) as TLChannel;
                        if (channel != null)
                        {
                            channel.MigratedFromChatId = ((TLChatBase)With).Id;
                            channel.MigratedFromMaxId = messageService.Id;
                            BeginOnUIThread(() =>
                            {
                                StateService.With = channel;
                                StateService.RemoveBackEntries = true;
                                NavigationService.Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                            });
                        }

                        return;
                    }

                    var deleteUserAction = messageService.Action as TLMessageActionChatDeleteUser;
                    if (deleteUserAction != null)
                    {
                        // delete replyMarkupKeyboard
                        var userId = deleteUserAction.UserId;
                        if (ReplyMarkupMessage != null && ReplyMarkupMessage.FromId.Value == userId.Value)
                        {
                            SetReplyMarkup(null);
                        }

                        // remove botInfo
                        GetFullInfo();
                    }

                    // add botInfo
                    var addUserAction = messageService.Action as TLMessageActionChatAddUserBase;
                    if (addUserAction != null)
                    {
                        GetFullInfo();
                    }

                    // Update number of participants
                    Subtitle = GetSubtitle();
                }
            }
            else if (With is TLBroadcastChat
                     && message.ToId is TLPeerBroadcast
                     && ((TLChatBase)With).Id.Value == message.ToId.Id.Value)
            {
                if (message is TLMessageService) // Update number of participants
                {
                    Subtitle = GetSubtitle();
                }
            }

            IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
        }

        private void MarkAsRead(TLMessageCommon message)
        {
            if (!_isActive) return;

            if (message != null
                && !message.Out.Value
                && message.Unread.Value)
            //&& !message.IsAudioVideoMessage())
            {
                StateService.GetNotifySettingsAsync(settings =>
                {
                    if (settings.InvisibleMode) return;

                    CurrentDialog = CurrentDialog ?? CacheService.GetDialog(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));

                    var dialog = CurrentDialog as TLDialog;
                    if (dialog != null)
                    {
                        var topMessage = dialog.TopMessage as TLMessageCommon;
                        //SetRead(topMessage, d => new TLInt(Math.Max(0, d.UnreadCount.Value - 1)));
                        SetRead(topMessage);
                    }

                    var channel = With as TLChannel;
                    if (channel != null)
                    {
                        MTProtoService.ReadHistoryAsync(channel, message.Id,
                            result =>
                            {
                                Execute.ShowDebugMessage("channels.readHistory result=" + result);
                                //SetRead(topMessage, d => new TLInt(0));
                            },
                            error =>
                            {
                                Execute.ShowDebugMessage("channels.readHistory error " + error);
                            });
                    }
                    else
                    {
                        MTProtoService.ReadHistoryAsync(Peer, message.Id, new TLInt(0),
                            affectedHistory =>
                            {
                                //SetRead(topMessage, d => new TLInt(0));
                            },
                            error =>
                            {
                                Execute.ShowDebugMessage("messages.readHistory error " + error);
                            });
                    }
                });
            }
        }

        private void SetRead(TLMessageCommon topMessage)
        {
            Execute.BeginOnUIThread(() =>
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    var message = Items[i] as TLMessageCommon;
                    if (IsIncomingUnread(message))
                    {
                        message.SetUnread(TLBool.False);
                    }
                }

                if (IsIncomingUnread(topMessage))
                {
                    topMessage.SetUnread(TLBool.False);
                }

                CurrentDialog.UnreadCount = new TLInt(0);
                CurrentDialog.NotifyOfPropertyChange(() => CurrentDialog.UnreadCount);

                var dialog = CurrentDialog as TLDialog;
                if (dialog != null)
                {
                    dialog.NotifyOfPropertyChange(() => dialog.TopMessage);
                }
                var dialog71 = CurrentDialog as TLDialog71;
                if (dialog71 != null)
                {
                    dialog71.UnreadMark = false;
                    dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMark);
                }

                CurrentDialog.NotifyOfPropertyChange(() => CurrentDialog.Self);

                CacheService.Commit();
            });
        }

        private static bool IsIncomingUnread(TLMessageCommon message)
        {
            return message != null
                   && !message.Out.Value
                   && message.Unread.Value;
            //&& !message.IsAudioVideoMessage();
        }

        public void Handle(TLUpdateUserTyping userTyping)
        {
            var user = With as TLUserBase;
            if (user != null
                && !user.IsSelf
                && user.Index == userTyping.UserId.Value)
            {
                HandleTypingCommon(userTyping);
            }
        }

        public void Handle(TLUpdateChatUserTyping chatUserTyping)
        {
            var chat = With as TLChatBase;
            if (chat != null
                && chat.Index == chatUserTyping.ChatId.Value)
            {
                HandleTypingCommon(chatUserTyping);
            }
        }

        private void HandleTypingCommon(TLUpdateTypingBase updateTyping)
        {
            TLSendMessageActionBase action = new TLSendMessageTypingAction();
            var updateUserTyping17 = updateTyping as IUserTypingAction;
            if (updateUserTyping17 != null)
            {
                action = updateUserTyping17.Action;
            }

            if (action is TLSendMessageCancelAction)
            {
                InputTypingManager.RemoveTypingUser(updateTyping.UserId.Value);
            }
            else
            {
                InputTypingManager.AddTypingUser(updateTyping.UserId.Value, action);
            }
        }

        public void Handle(TLUpdateUserStatus updateUserStatus)
        {
            var userBase = With as TLUserBase;
            if (userBase != null
                && userBase.Index == updateUserStatus.UserId.Value)
            {
                BeginOnUIThread(() =>
                {
                    Subtitle = GetSubtitle();
                });
            }
        }

        public void Handle(TLUpdateContactLinkBase update)
        {
            var userBase = With as TLUserBase;
            if (userBase != null
                && userBase.Index == update.UserId.Value)
            {
                BeginOnUIThread(() =>
                {
                    Subtitle = GetSubtitle();
                    NotifyOfPropertyChange(() => With);

                    ChangeUserAction();
                });
            }
        }

        public void Handle(DialogRemovedEventArgs args)
        {
            if (With == args.Dialog.With)
            {
                BeginOnUIThread(() =>
                {
                    LazyItems.Clear();
                    Items.Clear();
                    IsEmptyDialog = true;
                });
            }
        }

        public void Handle(ChannelAvailableMessagesEventArgs args)
        {
            if (With == args.Dialog.With)
            {
                BeginOnUIThread(() =>
                {
                    var group = new Dictionary<long, TLMessageMediaGroup>();
                    for (var j = 0; j < Items.Count; j++)
                    {
                        var messageCommon = Items[j] as TLMessageCommon;
                        if (messageCommon != null
                            && messageCommon.ToId is TLPeerChannel
                            && messageCommon.Index <= args.AvailableMinId.Value)
                        {
                            Items.RemoveAt(j);
                            break;
                        }

                        var message73 = Items[j] as TLMessage73;
                        if (message73 != null && message73.GroupedId != null)
                        {
                            var mediaGroup = message73.Media as TLMessageMediaGroup;
                            if (mediaGroup != null)
                            {
                                for (var k = 0; k < mediaGroup.Group.Count; k++)
                                {
                                    messageCommon = mediaGroup.Group[k] as TLMessageCommon;
                                    if (messageCommon != null
                                        && messageCommon.ToId is TLPeerChannel
                                        && messageCommon.Index <= args.AvailableMinId.Value)
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
                    }

                    foreach (var mediaGroup in group.Values)
                    {
                        mediaGroup.RaiseCalculate();
                    }

                    IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                });
            }
        }

        public void Handle(MessagesRemovedEventArgs args)
        {
            if (With == args.Dialog.With && args.Messages != null && args.Messages.Count > 0)
            {
                BeginOnUIThread(() =>
                {
                    RemoveMessages(args.Messages);

                    IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                });
            }
        }

        public void Handle(DownloadableItem item)
        {
            Execute.BeginOnUIThread(() =>
            {
                var updated = false;

                var items = UngroupEnumerator(Items).ToList();

                var userProfilePhoto = item.Owner as TLUserProfilePhoto;
                if (userProfilePhoto != null)
                {
                    //handled with DialogsViewModel
                    return;
                }

                var chatPhoto = item.Owner as TLChatPhoto;
                if (chatPhoto != null)
                {
                    var channel = With as TLChannel;
                    if (channel != null)
                    {
                        channel.NotifyOfPropertyChange(() => channel.Photo);
                        updated = true;
                    }

                    var serviceMessages = items.OfType<TLMessageService>();
                    foreach (var serviceMessage in serviceMessages)
                    {
                        var editPhoto = serviceMessage.Action as TLMessageActionChatEditPhoto;
                        if (editPhoto != null && editPhoto.Photo == chatPhoto)
                        {
                            editPhoto.NotifyOfPropertyChange(() => editPhoto.Photo);
                            updated = true;
                            break;
                        }
                    }
                }

                var message = item.Owner as TLMessage;
                if (message != null)
                {
                    var messages = items.OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaVideo;
                        if (media != null && m == item.Owner)
                        {
                            m.Media.IsCanceled = false;
                            m.Media.LastProgress = 0.0;
                            m.Media.DownloadingProgress = 1.0;
                            m.NotifyOfPropertyChange(() => m.Self);
                            m.Media.IsoFileName = item.IsoFileName;
                            updated = true;
                            break;
                        }

                        var mediaGame = m.Media as TLMessageMediaGame;
                        if (mediaGame != null && m == item.Owner)
                        {
                            m.Media.IsCanceled = false;
                            m.Media.LastProgress = 0.0;
                            m.Media.DownloadingProgress = 1.0;
                            m.Media.NotifyOfPropertyChange(() => m.Media.Self); // update download icon for documents
                            m.NotifyOfPropertyChange(() => m.Self);
                            m.Media.IsoFileName = item.IsoFileName;
                            updated = true;
                            break;
                        }

                        var mediaDocument = m.Media as TLMessageMediaDocument;
                        if (mediaDocument != null && m == item.Owner)
                        {
                            if (m.IsVoice())
                            {
                                MessagePlayerControl.ConvertAndSaveOpusToWav(null, mediaDocument);
                            }

                            m.Media.IsCanceled = false;
                            m.Media.LastProgress = 0.0;
                            m.Media.DownloadingProgress = 1.0;
                            m.Media.NotifyOfPropertyChange(() => m.Media.Self); // update download icon for documents
                            m.NotifyOfPropertyChange(() => m.Self);
                            m.Media.IsoFileName = item.IsoFileName;
                            updated = true;
                            break;
                        }

                        var mediaWebPage = m.Media as TLMessageMediaWebPage;
                        if (mediaWebPage != null && m == item.Owner)
                        {
                            m.Media.IsCanceled = false;
                            m.Media.LastProgress = 0.0;
                            m.Media.DownloadingProgress = 1.0;
                            m.Media.NotifyOfPropertyChange(() => m.Media.Self);
                            m.NotifyOfPropertyChange(() => m.Self);
                            m.Media.IsoFileName = item.IsoFileName;
                            updated = true;
                            break;
                        }

                        var audioMedia = m.Media as TLMessageMediaAudio;
                        if (audioMedia != null && m == item.Owner)
                        {
                            MessagePlayerControl.ConvertAndSaveOpusToWav(audioMedia);

                            m.Media.IsCanceled = false;
                            m.Media.LastProgress = 0.0;
                            m.Media.DownloadingProgress = 1.0;
                            m.Media.IsoFileName = item.IsoFileName;
                            updated = true;
                            break;
                        }

                    }
                    return;
                }

                var photo = item.Owner as TLPhoto;
                if (photo != null)
                {
                    var isUpdated = false;
                    var messages = items.OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var mediaPhoto = m.Media as TLMessageMediaPhoto;
                        if (mediaPhoto != null && mediaPhoto.Photo == photo)
                        {
                            mediaPhoto.DownloadingProgress = 1.0;

                            if (!mediaPhoto.IsCanceled)
                            {
                                mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.Photo);
                                mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.Self);
                                updated = true;
                            }
                            isUpdated = true;
                            break;
                        }

                        var mediaWebPage = m.Media as TLMessageMediaWebPage;
                        if (mediaWebPage != null && mediaWebPage.Photo == photo)
                        {
                            mediaWebPage.NotifyOfPropertyChange(() => mediaWebPage.Photo);
                            mediaWebPage.NotifyOfPropertyChange(() => mediaWebPage.Self);
                            updated = true;
                            isUpdated = false;
                            break;
                        }
                    }

                    if (isUpdated) return;

                    var serviceMessages = items.OfType<TLMessageService>();
                    foreach (var serviceMessage in serviceMessages)
                    {
                        var editPhoto = serviceMessage.Action as TLMessageActionChatEditPhoto;
                        if (editPhoto != null && editPhoto.Photo == photo)
                        {
                            editPhoto.NotifyOfPropertyChange(() => editPhoto.Photo);
                            isUpdated = true;
                            updated = true;
                            break;
                        }
                    }
                }

                var document = item.Owner as TLDocument;
                if (document != null)
                {
                    var messages = items.OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaDocument;
                        if (media != null && TLDocumentBase.DocumentEquals(media.Document, document))
                        {
                            if (m.IsVoice())
                            {
                                media.NotifyOfPropertyChange(() => media.Document);
                            }
                            else if (m.IsVideo())
                            {
                                media.NotifyOfPropertyChange(() => media.Video);
                            }
                            else
                            {
                                media.NotifyOfPropertyChange(() => media.Document);
                            }
                            updated = true;
                            break;
                        }
                    }

                    if (PinnedMessage != null && PinnedMessage.Message != null)
                    {
                        var m = PinnedMessage.Message as TLMessage;
                        if (m != null)
                        {
                            var media = m.Media as TLMessageMediaDocument;
                            if (media != null && TLDocumentBase.DocumentEquals(media.Document, document))
                            {
                                if (m.IsVoice())
                                {
                                    media.NotifyOfPropertyChange(() => media.Document);
                                }
                                else if (m.IsVideo())
                                {
                                    media.NotifyOfPropertyChange(() => media.Video);
                                }
                                else
                                {
                                    media.NotifyOfPropertyChange(() => media.Document);
                                }
                                updated = true;
                            }
                        }
                    }
                }

                var video = item.Owner as TLVideo;
                if (video != null)
                {
                    var messages = items.OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaVideo;
                        if (media != null && media.Video == video)
                        {
                            media.NotifyOfPropertyChange(() => media.Video);
                            updated = true;
                            break;
                        }
                    }
                }

                var audio = item.Owner as TLAudio;
                if (audio != null)
                {
                    var messages = items.OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaAudio;
                        if (media != null && media.Audio == audio)
                        {
                            media.NotifyOfPropertyChange(() => media.Audio);
                            updated = true;
                            break;
                        }
                    }
                }

                var webPage = item.Owner as TLWebPage;
                if (webPage != null)
                {
                    var messages = items.OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaWebPage;
                        if (media != null && media.WebPage.Id.Value == webPage.Id.Value)
                        {
                            media.NotifyOfPropertyChange(() => media.Photo);
                            media.NotifyOfPropertyChange(() => media.Self);
                            updated = true;
                            //break;
                        }
                    }
                }

                var game = item.Owner as TLGame;
                if (game != null)
                {
                    var messages = items.OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaGame;
                        if (media != null && media.Game.Id.Value == game.Id.Value)
                        {
                            media.NotifyOfPropertyChange(() => media.Photo);
                            media.NotifyOfPropertyChange(() => media.Self);
                            updated = true;
                            //break;
                        }
                    }
                }
                //if (!updated)
                //{
                //    if (item.Owner is TLStickerItem || item.Owner is TLBotInlineMediaResult)
                //    {
                //        return;
                //    }

                //    Execute.ShowDebugMessage("DialogDetailsViewModel.Handle DownloadableItem not handled owner=" + item.Owner);
                //}
            });
        }

        public void Handle(ProgressChangedEventArgs args)
        {
            var photo = args.Item.Owner as TLPhoto;
            if (photo != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    var messages = UngroupEnumerator(Items).OfType<TLMessage>();
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaPhoto;
                        if (media != null && media.Photo == photo)
                        {
                            var delta = args.Progress - media.DownloadingProgress;

                            if (delta > 0.0)
                            {
                                media.DownloadingProgress = args.Progress;

                            }
                            break;
                        }
                    }
                });

                return;
            }

            var message = args.Item.Owner as TLMessage;
            if (message != null)
            {
                //var photo = message.Media as TLMessageMediaPhoto;
                //if (photo != null && !photo.IsCanceled)
                //{
                //    var delta = args.Progress - photo.DownloadingProgress;

                //    if (delta > 0.0)
                //    {
                //        photo.DownloadingProgress = args.Progress;

                //    }
                //    return;
                //}

                var video = message.Media as TLMessageMediaVideo;
                if (video != null && !video.IsCanceled)
                {
                    var delta = args.Progress - video.DownloadingProgress;

                    if (delta > 0.0)
                    {
                        video.DownloadingProgress = args.Progress;

                    }
                    return;
                }

                var audio = message.Media as TLMessageMediaAudio;
                if (audio != null && !audio.IsCanceled)
                {
                    var delta = args.Progress - audio.DownloadingProgress;

                    if (delta > 0.0)
                    {
                        audio.DownloadingProgress = args.Progress;
                    }
                    return;
                }

                var game = message.Media as TLMessageMediaGame;
                if (game != null && !game.IsCanceled)
                {
                    var delta = args.Progress - game.DownloadingProgress;

                    if (delta > 0.0)
                    {
                        game.DownloadingProgress = args.Progress;
                    }
                    return;
                }

                var document = message.Media as TLMessageMediaDocument;
                if (document != null && !document.IsCanceled)
                {
                    var delta = args.Progress - document.DownloadingProgress;

                    if (delta > 0.0)
                    {
                        document.DownloadingProgress = args.Progress;
                    }
                    return;
                }

                var webPage = message.Media as TLMessageMediaWebPage;
                if (webPage != null && !webPage.IsCanceled)
                {
                    var delta = args.Progress - webPage.DownloadingProgress;

                    if (delta > 0.0)
                    {
                        webPage.DownloadingProgress = args.Progress;
                    }
                    return;
                }
            }
        }

        public void Handle(UploadProgressChangedEventArgs args)
        {
            var message = args.Item.Owner as TLMessage;
            if (message != null)
            {
                var photo = message.Media as TLMessageMediaPhoto;
                if (photo != null)
                {
                    var delta = args.Progress - photo.UploadingProgress;

                    if (delta > 0.0)
                    {
                        photo.UploadingProgress = args.Progress;
                    }

                    UploadTypingManager.SetTyping(new TLSendMessageUploadPhotoAction28 { Progress = new TLInt((int)(args.Progress * 100.0)) });

                    return;
                }

                var document = message.Media as TLMessageMediaDocument;
                if (document != null)
                {
                    var delta = args.Progress - document.UploadingProgress;

                    if (delta > 0.0)
                    {
                        document.UploadingProgress = args.Progress;
                    }

                    UploadTypingManager.SetTyping(new TLSendMessageUploadDocumentAction28 { Progress = new TLInt((int)(args.Progress * 100.0)) });

                    return;
                }

                var video = message.Media as TLMessageMediaVideo;
                if (video != null)
                {
                    var delta = args.Progress - video.UploadingProgress;

                    if (delta > 0.0)
                    {
                        video.UploadingProgress = args.Progress;
                    }

                    UploadTypingManager.SetTyping(new TLSendMessageUploadVideoAction28 { Progress = new TLInt((int)(args.Progress * 100.0)) });

                    return;
                }
            }
        }

        public void Handle(UploadingCanceledEventArgs args)
        {
            var message = args.Item.Owner as TLMessage;
            if (message != null)
            {
                var photo = message.Media as TLMessageMediaPhoto;
                if (photo != null)
                {
                    message.Media.UploadingProgress = 0.0;
                    message.Status = MessageStatus.Failed;
                }

                var document = message.Media as TLMessageMediaDocument;
                if (document != null)
                {
                    message.Media.UploadingProgress = 0.0;
                    message.Status = MessageStatus.Failed;
                }

                var video = message.Media as TLMessageMediaVideo;
                if (video != null)
                {
                    message.Media.UploadingProgress = 0.0;
                    message.Status = MessageStatus.Failed;
                }

                UploadTypingManager.CancelTyping();
            }
        }

        public void Handle(TopMessageUpdatedEventArgs args)
        {
#if WP8
            if (!_isFirstSliceLoaded)
            {
                Execute.ShowDebugMessage("DialogDetailsViewModel.Handle(TLMessageCommon) _isFirstSliceLoaded=false");
                return;
            }
#endif

            var serviceMessage = args.Message as TLMessageService;
            if (serviceMessage == null) return;
            Handle(serviceMessage);
        }

        public void Handle(ChannelUpdateCompletedEventArgs eventArgs)
        {
            Telegram.Logs.Log.Write(string.Format("DialogDetailsViewModel.Handle ChannelUpdateCompletedEventArgs start"));
            if (Peer == null) return; // tombstoning 
            if (!_isUpdated) return;

            var channel = With as TLChannel;
            if (channel != null && channel.Index == eventArgs.ChannelId.Value)
            {
                Telegram.Logs.Log.Write(string.Format("DialogDetailsViewModel.Handle ChannelUpdateCompletedEventArgs UpdateCompletedEventArgs"));
                Handle(new UpdateCompletedEventArgs());
            }
            Telegram.Logs.Log.Write(string.Format("DialogDetailsViewModel.Handle ChannelUpdateCompletedEventArgs stop"));
        }

        private int _counter;

        public int Counter
        {
            get { return _counter; }
            set { SetField(ref _counter, value, () => Counter); }
        }

        public void Handle(UpdateCompletedEventArgs args)
        {
            Telegram.Logs.Log.Write("DialogDetailsViewModel.Handle UpdateCompletedEventArgs start");

            if (Peer == null) return; // tombstoning 
            if (!_isUpdated) return;

            Telegram.Logs.Log.Write("DialogDetailsViewModel.Handle UpdateCompletedEventArgs GetHistoryAsync");

            CacheService.GetHistoryAsync(
                TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                messages =>
                {
                    Telegram.Logs.Log.Write("DialogDetailsViewModel.Handle UpdateCompletedEventArgs ProcessRepliesAndAudio");

                    ProcessMessages(messages);

                    Execute.BeginOnUIThread(() =>
                    {
                        Telegram.Logs.Log.Write("DialogDetailsViewModel.Handle UpdateCompletedEventArgs start UI");
                        var messageIndexes = new Dictionary<int, int>();
                        for (var i = 0; i < Items.Count; i++)
                        {
                            messageIndexes[Items[i].Index] = Items[i].Index;
                        }
                        var topMessage = Items.FirstOrDefault(x => x.Index != 0);
                        var lastMessage = Items.LastOrDefault(x => x.Index != 0);


                        var newMessages = new List<TLMessageBase>();
                        var newMessagesAtMiddle = new List<TLMessageBase>();
                        foreach (var message in messages)
                        {

                            if (message.Index != 0)  //возможно, это новое сообщение
                            {
                                if (topMessage == null && lastMessage == null)  // в имеющемся списке нет сообщений с индексом
                                {
                                    newMessages.Add(message);
                                }
                                else
                                {
                                    if (topMessage != null && message.Index > topMessage.Index)  // до первого сообщения с индексом в списке 
                                    {
                                        newMessages.Add(message);
                                    }
                                    else if (lastMessage != null
                                        && !messageIndexes.ContainsKey(message.Index)
                                        && message.Index < lastMessage.Index)  // в середине списка до последнего сообщения с индексом
                                    {
                                        Execute.ShowDebugMessage("Catch middle message: " + message);
                                        newMessagesAtMiddle.Add(message);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                            }
                        }

                        if (newMessages.Count > 0)
                        {
                            Subtitle = GetSubtitle();

                            for (var i = newMessagesAtMiddle.Count; i > 0; i--)
                            {
                                TLDialog.InsertMessageInOrder(Items, newMessagesAtMiddle[i]);
                            }

                            AddUnreadHistory(newMessages);
                        }
                        Telegram.Logs.Log.Write("DialogDetailsViewModel.Handle UpdateCompletedEventArgs stop UI");
                    });

                }, int.MaxValue);
        }

        private static bool UseSeparator(IList<TLMessageBase> messages)
        {
            if (messages.Count > 0)
            {
                var firstMessage = messages[0] as TLMessageCommon;
                if (firstMessage != null
                    && !firstMessage.Out.Value
                    && firstMessage.Unread.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddUnreadHistory(IList<TLMessageBase> newMessages)
        {
            var useSeparator = UseSeparator(newMessages);

            var counters = AddSeparator(useSeparator, newMessages);

            const int firstSliceCount = 0;
            var count = 0;
            var secondSlice = new List<TLMessageBase>();
            for (var i = newMessages.Count; i > 0; i--)
            {
                if (count < firstSliceCount || !useSeparator)
                {
                    count++;
                    Items.Insert(0, newMessages[i - 1]);
                }
                else
                {
                    secondSlice.Add(newMessages[i - 1]);
                }
            }

            if (secondSlice.Count == 0)
            {
                ContinueAddUnreadHistory();
            }
            else
            {
                InsertAndHoldPosition(secondSlice, () =>
                {
                    ContinueAddUnreadHistory();

                    BeginOnUIThread(() =>
                    {
                        Counter = counters.Item1;
                        MentionsCounter += counters.Item2;
                        ShowScrollToBottomButton();
                    });
                });
            }
        }

        private void InsertAndHoldPosition(IEnumerable<TLMessageBase> items, System.Action callback)
        {
            BeginOnUIThread(() =>
            {
                HoldScrollingPosition = true;
                BeginOnUIThread(() =>
                {
                    foreach (var message in items)
                    {
                        Items.Insert(0, message);
                    }
                    HoldScrollingPosition = false;

                    callback.SafeInvoke();
                });
            });
        }

        private void ContinueAddUnreadHistory()
        {
            UpdateReplyMarkup(Items);

            if (_isActive)
            {
                ReadHistoryAsync();
            }
        }

        private Tuple<int, int> AddSeparator(bool useSeparator, IList<TLMessageBase> newMessages)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var serviceMessage = Items[i] as TLMessageService;
                if (serviceMessage != null && serviceMessage.Action is TLMessageActionUnreadMessages)
                {
                    Items.RemoveAt(i--);
                }
            }

            if (useSeparator)
            {
                var position = -1;
                var counter = 0;
                var mentionsCounter = 0;
                var mentions = new List<TLMessageBase>();
                for (var i = 0; i < newMessages.Count; i++)
                {
                    var messageCommon = newMessages[i] as TLMessageCommon;
                    if (messageCommon != null
                        && !messageCommon.Out.Value
                        && messageCommon.Unread.Value)
                    {
                        position = i;
                        counter++;
                        var message = messageCommon as TLMessage25;
                        if (message != null && message.IsMention)
                        {
                            mentionsCounter++;
                            mentions.Add(message);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (position != -1)
                {
                    var separator = new TLMessageService17
                    {
                        FromId = new TLInt(StateService.CurrentUserId),
                        ToId = TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                        Status = With is TLBroadcastChat
                            ? MessageStatus.Broadcast
                            : MessageStatus.Sending,
                        Out = TLBool.True,
                        Unread = TLBool.True,
                        Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                        Action = new TLMessageActionUnreadMessages(),
                        //IsAnimated = true,
                        RandomId = TLLong.Random()
                    };

                    newMessages.Insert(position + 1, separator);
                }

                if (mentions.Count > 0)
                {
                    var dialog = CurrentDialog as TLDialog71;
                    if (dialog != null)
                    {
                        dialog.UnreadMentions = dialog.UnreadMentions ?? new TLVector<TLMessageBase>();
                        for (var i = mentions.Count - 1; i >= 0; i--)
                        {
                            var added = false;
                            for (var j = 0; j < dialog.UnreadMentions.Count; j++)
                            {
                                if (dialog.UnreadMentions[j].Index == mentions[i].Index)
                                {
                                    added = true;
                                    break;
                                }
                                if (dialog.UnreadMentions[j].Index < mentions[i].Index)
                                {
                                    dialog.UnreadMentions.Insert(j, mentions[i]);
                                    added = true;
                                    break;
                                }
                            }
                            if (!added)
                            {
                                dialog.UnreadMentions.Add(mentions[i]);
                            }
                        }
                        dialog.UnreadMentionsCount = new TLInt(dialog.UnreadMentions.Count);
                    }
                }

                return new Tuple<int, int>(counter, mentionsCounter);
            }

            return new Tuple<int, int>(0, 0);
        }

        private void UpdateReplyMarkup(IList<TLMessageBase> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var message31 = items[i] as TLMessage31;
                if (message31 != null && !message31.Out.Value)
                {
                    if (message31.ReplyMarkup != null)
                    {
                        var fromId = message31.FromId;
                        var user = CacheService.GetUser(fromId) as TLUser;
                        if (user != null && user.IsBot)
                        {
                            SetReplyMarkup(message31, true);
                            break;
                        }
                    }
                }
            }
        }

        public void Handle(TLUpdatePrivacy privacy)
        {
            if (privacy.Key is TLPrivacyKeyStatusTimestamp)
            {
                GetFullInfo();
            }
        }

        public void Handle(DeleteMessagesEventArgs args)
        {
            if (With == args.Owner)
            {
                BeginOnUIThread(() =>
                {
                    for (var j = 0; j < args.Messages.Count; j++)
                    {
                        for (var i = 0; i < Items.Count; i++)
                        {
                            if (Items[i].Index == args.Messages[j].Index)
                            {
                                Items.RemoveAt(i);
                                break;
                            }
                        }
                    }
                });
            }
        }

        public void Handle(TLUpdateNotifySettings updateNotifySettings)
        {
            // threadpool
            var notifyPeer = updateNotifySettings.Peer as TLNotifyPeer;
            if (notifyPeer != null)
            {
                var peer = notifyPeer.Peer;
                var chat = With as TLChatBase;
                var user = With as TLUserBase;
                var channel = With as TLChannel;

                if (peer is TLPeerChat
                    && chat != null
                    && peer.Id.Value == chat.Index)
                {
                    chat.NotifySettings = updateNotifySettings.NotifySettings;
                    With.NotifyOfPropertyChange(() => chat.NotifySettings);
                }
                else if (peer is TLPeerUser
                    && user != null
                    && peer.Id.Value == user.Index)
                {
                    user.NotifySettings = updateNotifySettings.NotifySettings;
                    With.NotifyOfPropertyChange(() => chat.NotifySettings);
                }
                else if (peer is TLPeerChannel
                    && channel != null
                    && peer.Id.Value == channel.Index)
                {
                    channel.NotifySettings = updateNotifySettings.NotifySettings;
                    With.NotifyOfPropertyChange(() => channel.NotifySettings);
                    NotifyOfPropertyChange(() => AppBarCommandString);
                }
            }
        }

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

                                        Reply = new TLMessagesContainter { WebPageMedia = webPageMessageMedia };
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
            });
        }

        public void Handle(TLUpdateUserBlocked updateUserBlocked)
        {
            var user = With as TLUserBase;
            if (user != null && user.Id.Value == updateUserBlocked.UserId.Value)
            {
                NotifyOfPropertyChange(() => With);
            }
        }

        public void Handle(TLAllStickersBase allStickersBase)
        {
            var allStickers = allStickersBase as TLAllStickers;
            if (allStickers != null)
            {
                Stickers = allStickers;
            }
        }

        public void Handle(TLChannel channel)
        {
            var currentChannel = With as TLChannel;
            if (currentChannel != null && currentChannel.Index == channel.Index)
            {

            }
        }

        public void Handle(TLUpdateChannel updateChannel)
        {
            var currentChannel = With as TLChatBase;
            if (currentChannel != null && currentChannel.Index == updateChannel.ChannelId.Value)
            {
                var channelForbidden = CacheService.GetChat(updateChannel.ChannelId) as TLChannelForbidden;
                if (channelForbidden != null)
                {
                    With = channelForbidden;
                    NotifyOfPropertyChange(() => With);
                }
            }
        }

        public void Handle(TLUpdateChannelPinnedMessage updateChannelPinnedMessage)
        {
            var currentChannel = With as TLChannel49;
            if (currentChannel != null && currentChannel.Index == updateChannelPinnedMessage.ChannelId.Value)
            {
                ShowPinnedMessage(currentChannel);
            }
        }

        public void Handle(TLUpdateDraftMessage update)
        {
            var peer = update.Peer;
            var chat = With as TLChatBase;
            var user = With as TLUserBase;
            var channel = With as TLChannel;

            if (peer is TLPeerChat
                && chat != null
                && peer.Id.Value == chat.Index)
            {
                UpdateDraftMessage(update.Draft);
            }
            else if (peer is TLPeerUser
                && user != null
                && peer.Id.Value == user.Index)
            {
                UpdateDraftMessage(update.Draft);
            }
            else if (peer is TLPeerChannel
                && channel != null
                && peer.Id.Value == channel.Index)
            {
                UpdateDraftMessage(update.Draft);
            }
        }

        private bool _suppressTyping;

        private void UpdateDraftMessage(TLDraftMessageBase draftBase)
        {
            Execute.BeginOnUIThread(() =>
            {
                _suppressTyping = true;
                var draft = draftBase as TLDraftMessage;
                if (draft != null)
                {
                    if (draft.ReplyToMsgId != null && draft.ReplyToMsgId.Value > 0)
                    {
                        var message = Items.FirstOrDefault(x => x.Index == draft.ReplyToMsgId.Value);
                        if (message == null)
                        {
                            var channel = With as TLChannel;
                            if (channel != null)
                            {
                                message = CacheService.GetMessage(draft.ReplyToMsgId, channel.Id);
                            }
                            else
                            {
                                message = CacheService.GetMessage(draft.ReplyToMsgId);
                            }
                        }

                        if (message != null)
                        {
                            ReplyMessage(message);
                        }
                    }
                    else
                    {
                        Reply = null;
                    }

                    var text = draft.Message.ToString();
                    if (string.Equals(text, Text) && !draft.NoWebpage)
                    {
                        GetWebPagePreviewAsync(text);
                    }
                    else
                    {
                        Text = GetDraftText(draft);
                    }
                    return;
                    if (!string.IsNullOrEmpty(Text))
                    {
                        var view = GetView() as IDialogDetailsView;
                        if (view != null)
                        {
                            view.MoveCurretToEnd();
                        }
                    }
                }
                else
                {
                    Text = "";
                    Reply = null;
                }
            });
        }

        private string GetDraftText(TLDraftMessage draft)
        {
            var entities = draft.Entities;
            var text = draft.Message.ToString();
            if (entities == null || entities.Count == 0) return text;

            ClearMentions();

            var builder = new StringBuilder();

            //text
            for (var i = 0; i < entities.Count; i++)
            {
                // prefix i
                var offset = i == 0 ? 0 : entities[i - 1].Offset.Value + entities[i - 1].Length.Value;
                var length = i == 0 ? entities[i].Offset.Value : entities[i].Offset.Value - offset;
                var prefix = text.Substring(offset, length);

                builder.Append(prefix);

                // entity i
                var entityText = text.Substring(entities[i].Offset.Value, entities[i].Length.Value);
                var entityMentionName = entities[i] as TLMessageEntityMentionName;
                var inputEntityMentionName = entities[i] as TLInputMessageEntityMentionName;
                if (entityMentionName != null)
                {
                    var user = CacheService.GetUser(entityMentionName.UserId);
                    if (user != null)
                    {
                        AddMention(user);
                        entityText = string.Format("@({0})", entityText);
                    }
                }
                else if (inputEntityMentionName != null)
                {
                    var inputUser = inputEntityMentionName.User as TLInputUser;
                    if (inputUser != null)
                    {
                        var user = CacheService.GetUser(inputUser.UserId);
                        if (user != null)
                        {
                            AddMention(user);
                            entityText = string.Format("@({0})", entityText);
                        }
                    }
                }
                builder.Append(entityText);
            }

            //postfix
            var lastEntity = entities[entities.Count - 1];
            var postfix = text.Substring(lastEntity.Offset.Value + lastEntity.Length.Value);
            builder.Append(postfix);

            return builder.ToString();
        }

        public void Handle(TLUpdateReadMessagesContents update)
        {
            // UI thread here
            var channel = With as TLChannel;
            if (channel != null) return;

            SplitGroupedMessages(update.Messages);
        }

        public void Handle(TLUpdateChannelReadMessagesContents update)
        {
            // UI thread here
            var channel = With as TLChannel;
            if (channel == null) return;
            if (channel.Index != update.ChannelId.Value) return;

            SplitGroupedMessages(update.Messages);
        }

        private void SplitGroupedMessages(IList<TLInt> messages)
        {
            var messagesDict = messages.ToDictionary(x => x.Value);
            for (var i = 0; i < Items.Count; i++)
            {
                var message = Items[i] as TLMessage73;
                if (message != null)
                {
                    var mediaGroup = message.Media as TLMessageMediaGroup;
                    if (mediaGroup != null)
                    {
                        var removed = false;
                        for (var j = 0; j < mediaGroup.Group.Count; j++)
                        {
                            if (messagesDict.ContainsKey(mediaGroup.Group[j].Index))
                            {
                                removed = true;
                                break;
                            }
                        }

                        if (removed)
                        {
                            Items.RemoveAt(i);

                            var group = new TLVector<TLMessageBase>(mediaGroup.Group.Reverse().ToList());

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

        public void Handle(ForwardGroupedEventArgs args)
        {
            // UI thread
            var lastMessage = args.Messages.LastOrDefault();
            if (lastMessage != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var message73 = Items[i] as TLMessage73;
                    if (message73 != null
                        && message73.GroupedId != null)
                    {
                        var mediaGroup = message73.Media as TLMessageMediaGroup;
                        if (mediaGroup != null)
                        {
                            for (int j = 0; j < mediaGroup.Group.Count; j++)
                            {
                                if (mediaGroup.Group[j] == lastMessage)
                                {
                                    message73.Status = lastMessage.Status;
                                    message73.Date = lastMessage.Date;
                                    message73.Id = lastMessage.Id;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static class Wav
    {
        public static MemoryStream GetWavAsMemoryStream(this byte[] data, int sampleRate, int audioChannels = 1, int bitsPerSample = 16)
        {
            MemoryStream memoryStream = new MemoryStream();
            Wav.WriteHeader(memoryStream, sampleRate, audioChannels, bitsPerSample);
            Wav.SeekPastHeader(memoryStream);
            memoryStream.Write(data, 0, data.Length);
            Wav.UpdateHeader(memoryStream);
            return memoryStream;
        }
        public static MemoryStream GetWavAsMemoryStream(this Stream data, int sampleRate, int audioChannels = 1, int bitsPerSample = 16)
        {
            MemoryStream memoryStream = new MemoryStream();
            Wav.WriteHeader(memoryStream, sampleRate, audioChannels, bitsPerSample);
            Wav.SeekPastHeader(memoryStream);
            data.Position = 0L;
            data.CopyTo(memoryStream);
            Wav.UpdateHeader(memoryStream);
            return memoryStream;
        }
        public static byte[] GetWavAsByteArray(this byte[] data, int sampleRate, int audioChannels = 1, int bitsPerSample = 16)
        {
            return data.GetWavAsMemoryStream(sampleRate, audioChannels, bitsPerSample).ToArray();
        }
        public static byte[] GetWavAsByteArray(this Stream data, int sampleRate, int audioChannels = 1, int bitsPerSample = 16)
        {
            return data.GetWavAsMemoryStream(sampleRate, audioChannels, bitsPerSample).ToArray();
        }
        public static void WriteHeader(Stream stream, int sampleRate, int audioChannels = 1, int bitsPerSample = 16)
        {
            int num = bitsPerSample / 8;
            Encoding uTF = Encoding.UTF8;
            long position = stream.Position;
            stream.Seek(0L, 0);
            stream.Write(uTF.GetBytes("RIFF"), 0, 4);
            stream.Write(BitConverter.GetBytes(0), 0, 4);
            stream.Write(uTF.GetBytes("WAVE"), 0, 4);
            stream.Write(uTF.GetBytes("fmt "), 0, 4);
            stream.Write(BitConverter.GetBytes(16), 0, 4);
            stream.Write(BitConverter.GetBytes(1), 0, 2);
            stream.Write(BitConverter.GetBytes((short)audioChannels), 0, 2);
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
            stream.Write(BitConverter.GetBytes(sampleRate * num * audioChannels), 0, 4);
            stream.Write(BitConverter.GetBytes((short)num), 0, 2);
            stream.Write(BitConverter.GetBytes((short)bitsPerSample), 0, 2);
            stream.Write(uTF.GetBytes("data"), 0, 4);
            stream.Write(BitConverter.GetBytes(0), 0, 4);
            Wav.UpdateHeader(stream);
            stream.Seek(position, 0);
        }
        public static void SeekPastHeader(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new Exception("Can't seek stream to update wav header");
            }
            stream.Seek(44L, 0);
        }
        public static void UpdateHeader(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new Exception("Can't seek stream to update wav header");
            }
            long position = stream.Position;
            stream.Seek(4L, 0);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 8), 0, 4);
            stream.Seek(40L, 0);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 44), 0, 4);
            stream.Seek(position, 0);
        }
    }
}
