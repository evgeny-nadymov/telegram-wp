// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Org.BouncyCastle.Crypto;
using Telegram.Api;
using Telegram.Api.Services.Updates;
using Telegram.EmojiPanel;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Payments;
using TelegramClient.Views;
#if WP8
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
#endif
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Feed;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public void ReportMessage(TLMessageCommon message)
        {
            if (message == null) return;

            Report(Peer, new TLVector<TLInt> { message.Id });
        }

        public void OpenCropedMessage(TLMessage message)
        {
            if (message == null) return;

            StateService.MediaMessage = message;
            NavigationService.UriFor<MessageViewerViewModel>().Navigate();
        }

#if WP81
        private static StorageFile _fromFile;

        public static async void SaveFile(StorageFile toFile)
        {
            if (_fromFile == null) return;
            if (toFile == null) return;

            try
            {
                using (var streamSource = await _fromFile.OpenStreamForReadAsync())
                {
                    using (var streamDest = await toFile.OpenStreamForWriteAsync())
                    {
                        await streamSource.CopyToAsync(streamDest, 1024);
                    }
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage("SaveFile exception \n" + ex);
            }

            return;
        }
#endif



#if WP81

        public void SaveMedia(TLMessage message)
        {
            if (message == null) return;

            SaveMediaCommon(message);
        }
        public static async void SaveMediaCommon(TLMessage message)
        {
            var storageFile = await GetStorageFile(message.Media);
            if (storageFile == null)
            {
                return;
            }

            _fromFile = storageFile;

            var fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedFileName = storageFile.Name;
            fileSavePicker.FileTypeChoices.Add(storageFile.FileType ?? "file", new[] { storageFile.FileType });
            fileSavePicker.ContinuationData.Add("From", "DialogDetailsView");
            fileSavePicker.PickSaveFileAndContinue();
        }

        public string GetMediaFileName(TLMessage message)
        {
            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var file = mediaDocument.File;

                if (file == null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document != null)
                    {
                        var localFileName = document.GetFileName();
                        var globalFileName = mediaDocument.IsoFileName;
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
                        if (store.FileExists(localFileName))
                        {
                            return Path.GetFileName(localFileName);
                        }

                        if (store.FileExists(globalFileName))
                        {
                            return Path.GetFileName(globalFileName);
                        }

                        if (File.Exists(globalFileName))
                        {
                            return Path.GetFileName(globalFileName);
                        }

                    }
                }

                return Path.GetFileName(mediaDocument.File.Name);
            }

            var mediaVideo = message.Media as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                var file = mediaVideo.File;

                if (file == null)
                {
                    var video = mediaVideo.Video as TLVideo;
                    if (video != null)
                    {
                        var localFileName = video.GetFileName();
                        var globalFileName = mediaVideo.IsoFileName;
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
                        if (store.FileExists(localFileName))
                        {
                            return Path.GetFileName(localFileName);
                        }

                        if (store.FileExists(globalFileName))
                        {
                            return Path.GetFileName(globalFileName);
                        }

                        if (File.Exists(globalFileName))
                        {
                            return Path.GetFileName(globalFileName);
                        }
                    }
                }

                return Path.GetFileName(mediaVideo.File.Name);
            }

            return null;
        }
#endif
        private UserActionViewModel _userAction;

        public UserActionViewModel UserAction
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel UserAction");
                return _userAction;
            }
            protected set
            {
                _userAction = value;
            }
        }

        public void ChangeUserAction()
        {
            if (UserActionViewModel.IsRequired(With))
            {
                if (UserAction == null)
                {
                    UserAction = new UserActionViewModel((TLUserBase)With);
                    UserAction.InvokeUserAction += (sender, args) => InvokeUserAction();
                    UserAction.InvokeUserAction2 += (sender, args) => InvokeUserAction2();
                    NotifyOfPropertyChange(() => UserAction);
                }
                else
                {
                    UserAction.SetUser((TLUserBase)With);
                }
            }
            else
            {
                if (UserAction != null)
                {
                    UserAction = null;
                    NotifyOfPropertyChange(() => UserAction);
                }
            }
        }

        public void InvokeUserAction()
        {
            var userBase = With as TLUserBase;
            if (userBase != null && userBase.IsRequest)
            {
                AddContact(userBase);
                return;
            }

            if (userBase != null && userBase.IsForeign)
            {
                ShareMyContactInfo();
                return;
            }
        }

        public void InvokeUserAction2()
        {
            ReportSpam();
        }

        private void ShareMyContactInfo()
        {
            var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
            if (currentUser == null) return;

            SendContact(currentUser);
        }

        public void AddContact(TLUserBase userRequest)
        {
            if (userRequest == null) return;

            var phone = userRequest.Phone;
            if (TLString.IsNullOrEmpty(phone)) return;

            IsWorking = true;
            ContactViewModel.ImportContactAsync(
                userRequest, phone, MTProtoService,
                result =>
                {
                    if (result.Users.Count > 0)
                    {
                        EventAggregator.Publish(new TLUpdateContactLink24 { UserId = result.Users[0].Id, MyLink = new TLContactLink(), ForeignLink = new TLContactLinkUnknown() });

                        var userBase = result.Users[0];
                        if (userBase != null && userBase.IsContact)
                        {
                            ContactsHelper.CreateContactAsync(DownloadFileManager, StateService, userBase);
                        }
                    }

                    if (UserAction != null)
                    {
                        UserAction.Remove();
                    }

                    BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("contacts.importContacts error " + error);
                }));
        }

        public void AppBarCommand()
        {

            if (IsBotStarting)
            {
                if (_bot == null || string.IsNullOrEmpty(_bot.AccessToken))
                {
                    _text = "/start";
                    Execute.BeginOnUIThread(() => SendInternal(false, false));
                }
                else
                {
                    var accessToken = new TLString(_bot.AccessToken);
                    _bot.AccessToken = string.Empty;

                    BeginOnUIThread(() =>
                    {
                        var text = With is TLUser
                            ? new TLString("/start")
                            : new TLString("/start@" + ((IUserName)_bot).UserName);

                        var message = GetMessage(text, new TLMessageMediaEmpty());
                        var previousMessage = InsertSendingMessage(message);
                        IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                        NotifyOfPropertyChange(() => With);

                        BeginOnThreadPool(() =>
                            CacheService.SyncSendingMessage(
                                message, previousMessage,
                                result => StartBotInternal(result, accessToken)));
                    });
                }

                return;
            }
            else if (IsChannel)
            {
                var channel = With as TLChannel;
                if (channel != null)
                {
                    if (channel.Left.Value)
                    {
                        IsWorking = true;
                        MTProtoService.JoinChannelAsync(channel,
                            result => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                Subtitle = GetSubtitle();
                                NotifyOfPropertyChange(() => With);
                                NotifyOfPropertyChange(() => IsAppBarCommandVisible);
                                NotifyOfPropertyChange(() => AppBarCommandString);

                                if (channel.IsMegaGroup)
                                {
                                    var updates = result as TLUpdates;
                                    if (updates != null)
                                    {
                                        var updateNewChannelMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                                        if (updateNewChannelMessage != null)
                                        {
                                            Items.Insert(0, updateNewChannelMessage.Message);
                                            CurrentDialog = CurrentDialog ?? CacheService.GetDialog(new TLPeerChannel { Id = channel.Id });
                                            EventAggregator.Publish(new TopMessageUpdatedEventArgs(CurrentDialog, updateNewChannelMessage.Message));
                                        }
                                        else
                                        {
                                            var currentUserId = new TLInt(StateService.CurrentUserId);
                                            var message = new TLMessageService49
                                            {
                                                Flags = new TLInt(0),
                                                Id = new TLInt(0),
                                                FromId = currentUserId,
                                                ToId = new TLPeerChannel { Id = channel.Id },
                                                Status = MessageStatus.Confirmed,
                                                Out = TLBool.True,
                                                Unread = TLBool.False,
                                                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                                                Action = new TLMessageActionChatAddUser41 { Users = new TLVector<TLInt> { currentUserId } },
                                                RandomId = TLLong.Random()
                                            };

                                            CacheService.SyncMessage(message, true, true,
                                                cachedMessage =>
                                                {
                                                    Items.Insert(0, message);
                                                });
                                        }
                                    }
                                }
                                else
                                {
                                    var dateIndex = 0;
                                    var dialog = CacheService.GetDialogs().LastOrDefault() as TLDialog;
                                    if (dialog != null && dialog.TopMessage != null)
                                    {
                                        dateIndex = dialog.TopMessage.DateIndex;
                                    }

                                    var message = Items.FirstOrDefault();
                                    if (message != null && message.DateIndex > dateIndex)
                                    {
                                        CacheService.SyncMessage(message, true, true,
                                            cachedMessage =>
                                            {

                                            });
                                    }
                                }
                                //var message = Items.FirstOrDefault();
                                //if (message != null)
                                //{
                                //    CacheService.DeleteChannelMessages(channel.Id, new TLVector<TLInt>{message.Id});
                                //    CacheService.SyncMessage(message, new TLPeerChannel{ Id = channel.Id },
                                //        m =>
                                //        {

                                //        });
                                //}
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                                {
                                    if (error.TypeEquals(ErrorType.CHANNELS_TOO_MUCH))
                                    {
                                        MessageBox.Show(AppResources.ChannelsTooMuch, AppResources.Error, MessageBoxButton.OK);
                                    }
                                }

                                Execute.ShowDebugMessage("channels.joinChannel error " + error);
                            }));


                        return;
                    }

                    var notifySettings = channel.NotifySettings as TLPeerNotifySettings;
                    if (notifySettings != null)
                    {
                        var currentMuteUntil = notifySettings.MuteUntil ?? (IoC.Get<IStateService>().GetNotifySettings().GroupAlert ? new TLInt(0) : new TLInt(int.MaxValue));

                        var muteUntil = currentMuteUntil.Value == 0 ? int.MaxValue : 0;

                        var inputSettings = new TLInputPeerNotifySettings78
                        {
                            Flags = new TLInt(0),
                            MuteUntil = new TLInt(muteUntil),
                            ShowPreviews = notifySettings.ShowPreviews,
                            Sound = notifySettings.Sound
                        };

                        IsWorking = true;
                        MTProtoService.UpdateNotifySettingsAsync(new TLInputNotifyPeer { Peer = channel.ToInputPeer() }, inputSettings,
                            result => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                notifySettings.MuteUntil = new TLInt(muteUntil);
                                NotifyOfPropertyChange(() => AppBarCommandString);
                                channel.NotifyOfPropertyChange(() => channel.NotifySettings);

                                var dialog = CacheService.GetDialog(new TLPeerChannel { Id = channel.Id });
                                if (dialog != null)
                                {
                                    dialog.NotifySettings = channel.NotifySettings;
                                    dialog.NotifyOfPropertyChange(() => dialog.NotifySettings);
                                    dialog.NotifyOfPropertyChange(() => dialog.Self);
                                    var settings = dialog.With as INotifySettings;
                                    if (settings != null)
                                    {
                                        settings.NotifySettings = channel.NotifySettings;
                                    }
                                }

                                CacheService.Commit();
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                Execute.ShowDebugMessage("account.updateNotifySettings error " + error);
                            }));
                    }
                }
            }
            else if (IsChannelForbidden)
            {
                var channelForbidden = With as TLChannelForbidden;
                if (channelForbidden != null)
                {
                    DeleteChannelInternal(channelForbidden.Id, true);
                }
            }
            else if (IsChatForbidden || IsChatDeactivated)
            {
                var chat = With as TLChatBase;
                if (chat != null)
                {
                    DialogsViewModel.DeleteAndExitDialogCommon((TLChatBase)With, MTProtoService, () =>
                    {
                        var dialog = CacheService.GetDialog(new TLPeerChat { Id = chat.Id });
                        if (dialog != null)
                        {
                            EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                            CacheService.DeleteDialog(dialog);
                            DialogsViewModel.UnpinFromStart(dialog);
                        }
                        BeginOnUIThread(() =>
                        {
                            if (NavigationService.CanGoBack)
                            {
                                NavigationService.GoBack();
                            }
                            else
                            {
                                NavigateToShellViewModel();
                            }
                        });
                    },
                        error =>
                        {
                            Execute.ShowDebugMessage("DeleteAndExitDialogCommon error " + error);
                        });
                }
            }
            else if (IsUserBlocked)
            {
                var user = With as TLUserBase;
                if (user != null)
                {
                    var confirmation = IsBot
                        ? MessageBoxResult.OK
                        : MessageBox.Show(AppResources.UnblockContactConfirmation, AppResources.AppName, MessageBoxButton.OKCancel);

                    if (confirmation == MessageBoxResult.OK)
                    {
                        IsWorking = true;
                        MTProtoService.UnblockAsync(user.ToInputUser(),
                            result => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                user.Blocked = TLBool.False;
                                CacheService.Commit();
                                Handle(new TLUpdateUserBlocked { UserId = user.Id, Blocked = TLBool.False });

                                if (IsBot)
                                {
                                    _text = "/start";
                                    Execute.BeginOnUIThread(() => SendInternal(false, false));
                                }
                            }),
                            error => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                Execute.ShowDebugMessage("contacts.Unblock error " + error);
                            }));
                    }
                }

                return;
            }
        }

        private TLUserBase GetStartingBot()
        {
            var user = With as TLUser;
            if (user != null && user.IsBot) return user;

            var chat = With as TLChatBase;
            if (chat != null)
            {
                return _bot;
            }

            return null;
        }

        private void StartBotInternal(TLMessageBase message, TLString accessToken)
        {
            var message31 = message as TLMessage31;
            if (message31 == null) return;

            var bot = GetStartingBot();
            if (bot == null) return;

            MTProtoService.StartBotAsync(bot.ToInputUser(), accessToken, message31,
                result =>
                {
                    bot.AccessToken = null;
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error.TypeEquals(ErrorType.PEER_FLOOD))
                    {
                        //MessageBox.Show(AppResources.PeerFloodSendMessage, AppResources.Error, MessageBoxButton.OK);
                        ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodSendMessage, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                            result =>
                            {
                                if (result == CustomMessageBoxResult.RightButton)
                                {
                                    TelegramViewBase.NavigateToUsername(MTProtoService, Constants.SpambotUsername, null, null, null);
                                }
                            });
                    }
                    else
                    {
                        Execute.ShowDebugMessage("messages.startBot error " + error);
                    }
                }));
        }

        public bool IsChatForbidden
        {
            get { return With is TLChatForbidden || (With is TLChat && ((TLChat)With).Left.Value); }
        }

        public bool IsChatDeactivated
        {
            get { return With is TLChat40 && (((TLChat40)With).Deactivated); }
        }

        public bool IsChannelForbidden
        {
            get { return With is TLChannelForbidden; }
        }

        public bool IsBot
        {
            get
            {
                var bot = _bot as TLUser;
                if (bot != null && bot.IsBot)
                {
                    return true;
                }

                var user = With as TLUser;
                if (user != null && user.IsBot)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsBotStarting
        {
            get
            {
                var bot = _bot as TLUser;
                if (bot != null && bot.IsBot && !string.IsNullOrEmpty(bot.AccessToken))
                {
                    return true;
                }

                var user = With as TLUser;
                if (user != null && user.IsBot && Items.Count == 0 && LazyItems.Count == 0)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsUserBlocked
        {
            get
            {
                var user = With as TLUserBase;
                if (user != null && user.Blocked != null && user.Blocked.Value)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsBroadcast
        {
            get { return With is TLBroadcastChat; }
        }

        public void OpenPeerDetails()
        {
            if (With is TLChatBase)
            {
                if (IsChatForbidden)
                {
                    return;
                }

                StateService.CurrentChat = (TLChatBase)With;
                NavigationService.UriFor<ChatViewModel>().Navigate();
            }
            else if (With is TLVector<TLChatBase>)
            {
                StateService.CurrentFeed = (TLVector<TLChatBase>)With;
                NavigationService.UriFor<FeedViewModel>().Navigate();
            }
            else
            {
                var user = With as TLUser;
                if (user != null && user.IsSelf)
                {
                    StateService.CurrentInputPeer = user;
                    NavigationService.UriFor<FullMediaViewModel>().Navigate();
                    return;
                }

                StateService.CurrentContact = (TLUserBase)With;
                NavigationService.UriFor<ContactViewModel>().Navigate();
            }
        }

        public void ForwardMessage(TLMessageBase message)
        {
            if (message == null) return;
            if (message.Index <= 0) return;

            ForwardMessagesCommon(new List<TLMessageBase> { message }, StateService, NavigationService);
        }

        public void ForwardMessages(List<TLMessageBase> selectedItems)
        {
            if (selectedItems.Count == 0) return;

            ForwardMessagesCommon(selectedItems, StateService, NavigationService);
        }

        public void FastForwardMessages(List<TLMessageBase> selectedItems)
        {
            if (selectedItems.Count == 0) return;

            string link = null;
            if (selectedItems.Count == 1)
            {
                var message = selectedItems.FirstOrDefault();
                if (message == null) return;
                if (message.Index == 0) return;

                var channel = With as TLChannel;
                if (channel != null)
                {
                    if (!TLString.IsNullOrEmpty(channel.UserName))
                    {
                        link = String.Format(Constants.UsernameLinkPlaceholder + "/{1}", channel.UserName, message.Id);
                    }
                }

                var dialogDetailsView = GetView() as DialogDetailsView;
                if (dialogDetailsView != null)
                {
                    dialogDetailsView.OpenShareMessagePicker(link, args =>
                    {
                        if (args.Dialogs.Count == 0) return;

                        var containers = new List<Tuple<TLPeerBase, TLMessage25, TLMessagesContainter>>();
                        foreach (var dialog in args.Dialogs)
                        {
                            var with = dialog.With;
                            if (with != null)
                            {
                                with.ClearBitmap();
                            }

                            TLMessage25 commentMessage = null;
                            if (!string.IsNullOrEmpty(args.Comment))
                            {
                                commentMessage = GetMessage(new TLString(args.Comment), new TLMessageMediaEmpty());
                                commentMessage.ToId = dialog.Peer;

                                System.Diagnostics.Debug.WriteLine("  comment message random_id=" + commentMessage.RandomIndex);
                            }
                            var container = GetForwardMessagesContainer(selectedItems, false, dialog.Peer);
                            containers.Add(new Tuple<TLPeerBase, TLMessage25, TLMessagesContainter>(dialog.Peer, commentMessage, container));
                            if (dialog == CurrentDialog)
                            {
                                if (commentMessage != null)
                                {
                                    Items.Insert(0, commentMessage);
                                }

                                var grouped = new List<TLMessageBase>();
                                foreach (var fwdMessage in container.FwdMessages)
                                {
                                    grouped.Add(fwdMessage);
                                }
                                ProcessGroupedMessages(grouped);

                                foreach (var m in grouped)
                                {
                                    CheckChannelMessage(m as TLMessage25);
                                    Items.Insert(0, m);
                                }
                                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                            }
                        }

                        Execute.BeginOnThreadPool(() =>
                        {
                            foreach (var pair in containers)
                            {
                                var peer = pair.Item1;
                                var comment = pair.Item2;
                                var container = pair.Item3;

                                var fwdMessages25 = container.FwdMessages.Reverse().ToList();
                                var sendingMessages = new TLVector<TLMessage>();
                                if (comment != null)
                                {
                                    sendingMessages.Add(comment);
                                }
                                foreach (var fwdMessage in fwdMessages25)
                                {
                                    sendingMessages.Add(fwdMessage);
                                }

                                if (fwdMessages25.Count > 0)
                                {
                                    CacheService.SyncSendingMessages(
                                        sendingMessages, null,
                                        result => SendForwardMessagesInternal(MTProtoService, MTProtoService.PeerToInputPeer(peer), comment, fwdMessages25, container.WithMyScore));
                                }
                            }
                        });
                    });

                    return;
                }
            }

            ForwardMessagesCommon(selectedItems, StateService, NavigationService);
        }

        public static void ForwardMessagesCommon(List<TLMessageBase> messages, IStateService stateService, INavigationService navigationService)
        {
            stateService.ForwardMessages = UngroupEnumerator(messages).Reverse().ToList();

            Execute.BeginOnUIThread(() => navigationService.UriFor<ChooseDialogViewModel>().Navigate());
        }

        public void CopyMessage(TLMessage message)
        {
            if (message == null) return;

            if (!TLString.IsNullOrEmpty(message.Message))
            {
                Clipboard.SetText(message.Message.ToString());
                return;
            }
        }

        public void DeleteMessage(TLMessageBase messageBase)
        {
            if (messageBase == null) return;

            var messages = new List<TLMessageBase>();
            var message = messageBase as TLMessage73;
            if (message != null)
            {
                var mediaGroup = message.Media as TLMessageMediaGroup;
                if (mediaGroup != null)
                {
                    foreach (var item in mediaGroup.Group)
                    {
                        messages.Add(item);
                    }
                }
            }

            if (messages.Count == 0)
            {
                messages.Add(message);
            }

            var channel = With as TLChannel;
            if (channel != null)
            {
                var messageCommon = messageBase as TLMessageCommon;
                if (messageCommon != null)
                {
                    if (messageCommon.ToId is TLPeerChat)
                    {
                        IsDeleteConfirmed(1, MTProtoService, CacheService, null, messages, revoke =>
                        {
                            DeleteMessages(MTProtoService, revoke, null, null, messages, null, DeleteMessagesInternal);
                        });
                        return;
                    }
                }

                TLUserBase from;
                if (IsMassDeleteReportSpamEnabled(channel, null, messages, out from))
                {
                    DeleteReportSpam(from, null, null, messages);
                }
                else
                {
                    IsDeleteConfirmed(1, MTProtoService, CacheService, null, messages, revoke =>
                    {
                        DeleteChannelMessages(MTProtoService, (TLChannel)With, null, null, messages, null, DeleteMessagesInternal);
                    });
                }

                return;
            }

            IsDeleteConfirmed(1, MTProtoService, CacheService, null, messages, revoke =>
            {
                if (With is TLBroadcastChat)
                {
                    DeleteMessagesInternal(null, messages);
                    return;
                }

                if (messageBase.Index == 0 && messageBase.RandomIndex != 0)
                {
                    DeleteMessagesInternal(null, messages);
                    return;
                }

                DeleteMessages(MTProtoService, revoke, null, null, messages, null, DeleteMessagesInternal);
            });
        }

        public void DeleteUploadingMessage(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message == null) return;

            var media = message.Media;
            if (media == null || media.UploadingProgress == 1.0) return;

            var message73 = messageBase as TLMessage73;
            if (message73 != null && message73.GroupedId != null)
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    var groupedMessage = Items[i] as TLMessage73;
                    if (groupedMessage != null
                        && groupedMessage.GroupedId != null
                        && groupedMessage.GroupedId.Value == message73.GroupedId.Value)
                    {
                        var mediaGroup = groupedMessage.Media as TLMessageMediaGroup;
                        if (mediaGroup != null)
                        {
                            mediaGroup.Group.Remove(messageBase);
                            mediaGroup.RaiseCalculate();

                            if (mediaGroup.Group.Count == 0)
                            {
                                groupedMessage.Status = MessageStatus.Failed;
                                Items.Remove(groupedMessage);
                            }
                            else if (mediaGroup.Group.Count == 1)
                            {
                                message73.GroupedId = null;
                                if (Items[i] == groupedMessage)
                                {
                                    Items.RemoveAt(i);
                                    Items.Insert(i, mediaGroup.Group[0]);
                                }
                            }
                            break;
                        }

                        message.Status = MessageStatus.Failed;
                        Items.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                message.Status = MessageStatus.Failed;
                Items.Remove(message);
            }

            MergeGroupMessages(new List<TLMessageBase> { message });

            IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
            NotifyOfPropertyChange(() => With);

            BeginOnThreadPool(() =>
            {
                CacheService.DeleteMessages(new TLVector<TLLong> { message.RandomId });
                CancelUploading(message);
            });
        }

        private void RemoveMessages(IList<TLMessageBase> messages)
        {
            var group = new Dictionary<long, TLMessageMediaGroup>();
            for (var i = 0; i < messages.Count; i++)
            {
                if (messages[i].Status == MessageStatus.Sending)
                {
                    messages[i].Status = MessageStatus.Failed;
                    CancelUploading(messages[i]);
                }

                for (var j = 0; j < Items.Count; j++)
                {
                    if (Items[j] == messages[i])
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
                                if (mediaGroup.Group[k] == messages[i])
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
            }

            foreach (var mediaGroup in group.Values)
            {
                mediaGroup.RaiseCalculate();
            }
        }

        private void DeleteMessagesInternal(TLMessageBase lastMessage, IList<TLMessageBase> messages)
        {
            var channel = With as TLChannel;
            TLPeerBase toPeer = null;
            var localIds = new TLVector<TLLong>();
            var remoteIds = new TLVector<TLInt>();
            var remoteChatIds = new TLVector<TLInt>();
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].RandomIndex != 0)
                {
                    localIds.Add(messages[i].RandomId);
                }

                if (messages[i].Index > 0)
                {
                    var messageCommon = messages[i] as TLMessageCommon;
                    if (channel != null && messageCommon != null && messageCommon.ToId is TLPeerChat)
                    {
                        remoteChatIds.Add(messageCommon.Id);
                        toPeer = messageCommon.ToId;
                    }
                    else
                    {
                        remoteIds.Add(messages[i].Id);
                    }
                }
            }

            BeginOnUIThread(() =>
            {
                RemoveMessages(messages);

                MergeGroupMessages(messages);

                BeginOnUIThread(() =>
                {
                    IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                    NotifyOfPropertyChange(() => With);
                    HideScrollToBottomButton();
                });
            });

            if (toPeer != null) CacheService.DeleteMessages(toPeer, lastMessage, remoteChatIds);
            CacheService.DeleteMessages(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), lastMessage, remoteIds);
            CacheService.DeleteMessages(localIds);
        }

        private void CancelUploading(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message == null) return;

            var media = message.Media;

            var mediaPhoto = media as TLMessageMediaPhoto;
            if (mediaPhoto != null && mediaPhoto.FileId != null)
            {
                IoC.Get<IUploadService>().Remove(message);
                UploadFileManager.CancelUploadFile(media.FileId);
            }

            var mediaVideo = media as TLMessageMediaVideo;
            if (mediaVideo != null && mediaVideo.FileId != null)
            {
                UploadVideoFileManager.CancelUploadFile(media.FileId);
            }

            var mediaDocument = media as TLMessageMediaDocument;
            if (mediaDocument != null && mediaDocument.FileId != null)
            {
                if (message.IsVoice())
                {
                    UploadAudioFileManager.CancelUploadFile(media.FileId);
                }
                else if (message.IsVideo())
                {
                    IoC.Get<IUploadService>().Remove(message);
                    UploadVideoFileManager.CancelUploadFile(media.FileId);
                }
                else
                {
                    UploadDocumentFileManager.CancelUploadFile(media.FileId);
                }
            }

            var mediaAudio = media as TLMessageMediaAudio;
            if (mediaAudio != null && mediaAudio.FileId != null)
            {
                UploadAudioFileManager.CancelUploadFile(media.FileId);
            }
        }

        private void DeleteFromItems(IList<TLMessageBase> messages)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                Items.Remove(messages[i]);
            }

            MergeGroupMessages(messages);

            IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
            NotifyOfPropertyChange(() => With);
        }

        public void DeleteMessages()
        {
            var randomItems = new List<TLMessageBase>();
            var items = new List<TLMessageBase>();

            TLMessageBase lastItem = null;

            foreach (var item in UngroupEnumerator(Items))
            {
                var message = item;
                if (message.IsSelected)
                {
                    if (message.Index == 0 && message.RandomIndex != 0)
                    {
                        randomItems.Add(message);
                        lastItem = null;
                    }
                    else if (message.Index != 0)
                    {
                        items.Add(message);
                        lastItem = null;
                    }
                }
                else
                {
                    if (lastItem == null)
                    {
                        lastItem = message;
                    }
                }
            }

            if (randomItems.Count > 0 || items.Count > 0)
            {
                IsSelectionEnabled = false;
            }

            var totalCount = randomItems.Count + items.Count;

            var channel = With as TLChannel;
            if (channel != null)
            {
                var chatMessages = new List<TLMessageBase>();
                var channelMessages = new List<TLMessageBase>();
                if (channel.MigratedFromChatId != null)
                {
                    foreach (var item in items)
                    {
                        var message = item as TLMessageCommon;
                        if (message != null && message.ToId is TLPeerChat)
                        {
                            chatMessages.Add(message);
                        }
                        else
                        {
                            channelMessages.Add(message);
                        }
                    }
                }

                if (chatMessages.Count > 0)
                {
                    IsDeleteConfirmed(totalCount, MTProtoService, CacheService, randomItems, items, revoke =>
                    {
                        DeleteChannelMessages(MTProtoService, channel, lastItem, null, channelMessages, null, DeleteMessagesInternal);
                        DeleteMessages(MTProtoService, revoke, lastItem, null, chatMessages, null, DeleteMessagesInternal);
                    });

                    return;
                }

                TLUserBase from;
                if (IsMassDeleteReportSpamEnabled(channel, randomItems, items, out from))
                {
                    DeleteReportSpam(from, lastItem, randomItems, items);
                }
                else
                {
                    IsDeleteConfirmed(totalCount, MTProtoService, CacheService, randomItems, items, revoke =>
                    {
                        DeleteChannelMessages(MTProtoService, channel, lastItem, randomItems, items, DeleteMessagesInternal, DeleteMessagesInternal);
                    });
                }

                return;
            }

            IsDeleteConfirmed(totalCount, MTProtoService, CacheService, randomItems, items, revoke =>
            {
                if (With is TLBroadcastChat)
                {
                    DeleteMessagesInternal(lastItem, randomItems);
                    DeleteMessagesInternal(lastItem, items);
                    return;
                }

                DeleteMessages(MTProtoService, revoke, lastItem, randomItems, items, DeleteMessagesInternal, DeleteMessagesInternal);
            });
        }

        public static void IsDeleteConfirmed(int totalCount, IMTProtoService mtProtoService, ICacheService cacheService, IList<TLMessageBase> localMessages, IList<TLMessageBase> remoteMessages, Action<bool> callback)
        {
            if (totalCount == 0) return;
            TLMessageBase m = null;
            if (localMessages != null && localMessages.Count > 0)
            {
                m = localMessages.FirstOrDefault();
            }
            if (m == null && remoteMessages != null && remoteMessages.Count > 0)
            {
                m = remoteMessages.FirstOrDefault();
            }
            if (m == null) return;

            var canRevoke = CanRevoke(mtProtoService, localMessages, remoteMessages);
            var message = totalCount == 1
                ? AppResources.DeleteMessageConfirmation
                : string.Format(AppResources.DeleteMessagesConfirmation, Utils.Language.Declension(
                    totalCount,
                    AppResources.MessageNominativeSingular,
                    AppResources.MessageNominativePlural,
                    AppResources.MessageGenitiveSingular,
                    AppResources.MessageGenitivePlural).ToLower(CultureInfo.CurrentUICulture));

            object content = null;
            var textBlock = new TextBlock { IsHitTestVisible = false };
            var checkBox = new CheckBox { IsChecked = false, IsHitTestVisible = false };
            textBlock.SetValue(TextBlock.FontSizeProperty, DependencyProperty.UnsetValue);
            if (!canRevoke)
            {
                var hint = GetHint(cacheService, m, totalCount);

                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
                textBlock.Text = Environment.NewLine + hint;

                content = textBlock;
            }
            else
            {
                var text = AppResources.DeleteForAll;

                var mCommon = m as TLMessageCommon;
                if (mCommon != null && mCommon.ToId is TLPeerUser)
                {
                    var userId = mCommon.Out.Value ? mCommon.ToId.Id : mCommon.FromId;
                    var user = cacheService.GetUser(userId) as TLUser;
                    if (user != null)
                    {
                        text = string.Format(AppResources.DeleteFor, user.ShortName);
                    }
                }

                textBlock.Margin = new Thickness(-18.0, 0.0, 12.0, 0.0);
                textBlock.Text = text;
                textBlock.VerticalAlignment = VerticalAlignment.Center;

                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0.0, -2.0, 0.0, -20.0), Background = new SolidColorBrush(Colors.Transparent) };
                panel.Tap += (sender, args) =>
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                };
                panel.Children.Add(checkBox);
                panel.Children.Add(textBlock);
                content = panel;
            }

            ShellViewModel.ShowCustomMessageBox(message, AppResources.Confirm,
                AppResources.Delete.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                        {
                            callback.SafeInvoke(checkBox.IsChecked.Value);
                        });
                    }
                },
                content);
        }

        private static string GetHint(ICacheService cacheService, TLMessageBase m, int totalCount)
        {
            var messageCommon = m as TLMessageCommon;
            if (messageCommon != null)
            {
                var peerChannel = messageCommon.ToId as TLPeerChannel;
                if (peerChannel != null)
                {
                    var channel = cacheService.GetChat(peerChannel.Id) as TLChannel;
                    if (channel != null)
                    {
                        if (channel.IsMegaGroup)
                        {
                            return totalCount == 1
                                ? AppResources.DeleteMessageForEveryoneInChatHint
                                : AppResources.DeleteMessagesForEveryoneInChatHint;
                        }
                        else
                        {
                            return totalCount == 1
                                ? AppResources.DeleteMessageForEveryoneInChannelHint
                                : AppResources.DeleteMessagesForEveryoneInChannelHint;
                        }
                    }
                }
            }

            return totalCount == 1
                    ? AppResources.DeleteMessageForYouHint
                    : AppResources.DeleteMessagesForYouHint;
        }

        public static void DeleteMessages(IMTProtoService mtProtoService, bool revoke, TLMessageBase lastItem, IList<TLMessageBase> localMessages, IList<TLMessageBase> remoteMessages, Action<TLMessageBase, IList<TLMessageBase>> localCallback = null, Action<TLMessageBase, IList<TLMessageBase>> remoteCallback = null)
        {
            if (localMessages != null && localMessages.Count > 0)
            {
                localCallback.SafeInvoke(lastItem, localMessages);
            }

            if (remoteMessages != null && remoteMessages.Count > 0)
            {
                mtProtoService.DeleteMessagesAsync(revoke, new TLVector<TLInt> { Items = remoteMessages.Select(x => x.Id).ToList() },
                    deletedIds =>
                    {
                        remoteCallback.SafeInvoke(lastItem, remoteMessages);
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.deleteMessages error " + error);
                    });
            }
        }

        private static bool CanRevoke(IMTProtoService mtProtoService, IList<TLMessageBase> localMessages, IList<TLMessageBase> remoteMessages)
        {
            TLInt revokeTimeLimit = null;
            TLInt revokePmTimeLimit = null;
            var revokePmInbox = false;
            var config = IoC.Get<ICacheService>().GetConfig() as TLConfig48;
            if (config != null)
            {
                revokeTimeLimit = config.EditTimeLimit;
                revokePmTimeLimit = config.EditTimeLimit;
                revokePmInbox = false;
            }
            var config76 = IoC.Get<ICacheService>().GetConfig() as TLConfig76;
            if (config76 != null)
            {
                revokeTimeLimit = config76.RevokeTimeLimit;
                revokePmTimeLimit = config76.RevokePmTimeLimit;
                revokePmInbox = config76.RevokePmInbox;
            }

            if (revokeTimeLimit != null)
            {
                var now = TLUtils.DateToUniversalTimeTLInt(mtProtoService.ClientTicksDelta, DateTime.Now);

                if (localMessages != null)
                {
                    foreach (var messageBase in localMessages)
                    {
                        var message = messageBase as TLMessage;
                        if (message == null
                            || (!message.Out.Value && !revokePmInbox)
                            || (message.ToId is TLPeerUser && message.DateIndex + revokePmTimeLimit.Value < now.Value)
                            || (message.ToId is TLPeerChat && message.DateIndex + revokeTimeLimit.Value < now.Value))
                        {
                            return false;
                        }

                        var peerChannel = message.ToId as TLPeerChannel;
                        if (peerChannel != null)
                        {
                            return false;
                        }

                        var peerUser = message.ToId as TLPeerUser;
                        if (peerUser != null)
                        {
                            if (peerUser.Id.Value == mtProtoService.CurrentUserId.Value
                                && message.FromId.Value == mtProtoService.CurrentUserId.Value)
                            {
                                return false;
                            }

                            var user = IoC.Get<ICacheService>().GetUser(peerUser.Id) as TLUser;
                            if (user != null && user.IsBot)
                            {
                                return false;
                            }
                        }
                    }
                }

                if (remoteMessages != null)
                {
                    foreach (var messageBase in remoteMessages)
                    {
                        var message = messageBase as TLMessage;
                        if (message == null
                            || (!message.Out.Value && !revokePmInbox)
                            || (message.ToId is TLPeerUser && message.DateIndex + revokePmTimeLimit.Value < now.Value)
                            || (message.ToId is TLPeerChat && message.DateIndex + revokeTimeLimit.Value < now.Value))
                        {
                            return false;
                        }

                        var peerChannel = message.ToId as TLPeerChannel;
                        if (peerChannel != null)
                        {
                            return false;
                        }

                        var peerUser = message.ToId as TLPeerUser;
                        if (peerUser != null)
                        {
                            if (peerUser.Id.Value == mtProtoService.CurrentUserId.Value
                                && message.FromId.Value == mtProtoService.CurrentUserId.Value)
                            {
                                return false;
                            }

                            var user = IoC.Get<ICacheService>().GetUser(peerUser.Id) as TLUser;
                            if (user != null && user.IsBot)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public static void DeleteChannelMessages(IMTProtoService mtProtoService, TLChannel channel, TLMessageBase lastItem, IList<TLMessageBase> localMessages, IList<TLMessageBase> remoteMessages, Action<TLMessageBase, IList<TLMessageBase>> localCallback = null, Action<TLMessageBase, IList<TLMessageBase>> remoteCallback = null)
        {
            if (localMessages != null && localMessages.Count > 0)
            {
                localCallback.SafeInvoke(lastItem, localMessages);
            }

            if (remoteMessages != null && remoteMessages.Count > 0)
            {
                mtProtoService.DeleteMessagesAsync(channel.ToInputChannel(), new TLVector<TLInt> { Items = remoteMessages.Select(x => x.Id).ToList() },
                    deletedIds =>
                    {
                        remoteCallback.SafeInvoke(lastItem, remoteMessages);
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("channels.deleteMessages error " + error);
                    });
            }
        }

        public void DeleteMessageById(TLMessageBase message, System.Action callback)
        {
            if (message == null) return;

            if ((message.Id == null || message.Id.Value == 0)
                && message.RandomIndex != 0)
            {
                CacheService.DeleteMessages(new TLVector<TLLong> { message.RandomId });
                callback.SafeInvoke();
                BeginOnUIThread(() =>
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].RandomIndex == message.RandomIndex)
                        {
                            Items.RemoveAt(i);
                            break;
                        }
                    }
                });
                return;
            }

            IsDeleteConfirmed(1, MTProtoService, CacheService, null, new List<TLMessageBase> { message }, revoke =>
            {
                MTProtoService.DeleteMessagesAsync(false, new TLVector<TLInt> { message.Id },
                    deletedIds =>
                    {
                        // duplicate: deleting performed through updates
                        CacheService.DeleteMessages(new TLVector<TLInt> { message.Id });

                        Handle(new MessagesRemovedEventArgs(CurrentDialog, new List<TLMessageBase> { message }));

                        callback.SafeInvoke();
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.deleteMessages error " + error);
                    });
            });
        }

        public void DeleteFile(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message != null)
            {
                var mediaDocument = message.Media as TLMessageMediaDocument45;
                if (mediaDocument != null && message.IsVideo())
                {
                    var video = mediaDocument.Video as TLDocument22;
                    if (video != null)
                    {
                        var fileName = video.GetFileName();

                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (store.FileExists(fileName))
                            {
                                store.DeleteFile(fileName);
                            }
                        }
                    }
                }

                var mediaVideo = message.Media as TLMessageMediaVideo;
                if (mediaVideo != null)
                {
                    var video = mediaVideo.Video as TLVideo;
                    if (video != null)
                    {
                        var fileName = video.GetFileName();

                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (store.FileExists(fileName))
                            {
                                store.DeleteFile(fileName);
                            }
                        }
                    }
                }
            }
        }

#if WP81
        public async void Resend(TLMessage45 message)
#else
        public void Resend(TLMessage25 message)
#endif
        {
            if (message == null) return;

            if (message.Index != 0)
            {
                var messageInfo = string.Format("Resend delivered message Id={0} RandomId={1} Status={2} Date={3}", message.Index, message.RandomIndex, message.Status, message.Date);
                Execute.ShowDebugMessage(messageInfo);

                message.Status = MessageStatus.Confirmed;
                CacheService.SyncSendingMessage(message, null, result => { });

                return;
            }

            if (message.RandomIndex == 0)
            {
                var messageInfo = string.Format("Resend with missing randomIndex message Id={0} RandomId={1} Status={2} Date={3}", message.Index, message.RandomIndex, message.Status, message.Date);
                Execute.ShowDebugMessage(messageInfo);

                message.RandomId = TLLong.Random();
            }

            // find common grouped message to resend
            var message73 = message as TLMessage73;
            if (message73 != null
                && message73.GroupedId != null)
            {
                var groupedMessage = FindGroupedMessage(message73) as TLMessage45;
                if (groupedMessage != null)
                {
                    message = groupedMessage;
                }
            }

            message.Status = MessageStatus.Sending;

            if (message73 != null
                && message73.GroupedId != null
                && message73.FwdHeader != null
                && message73.FwdMessageId != null)
            {
                var mediaGroup = message73.Media as TLMessageMediaGroup;
                if (mediaGroup != null)
                {
                    var messages = new List<TLMessage25>();
                    foreach (var item in UngroupEnumerator(mediaGroup.Group).OfType<TLMessage25>())
                    {
                        messages.Add(item);
                    }

                    var inputPeer = PeerToInputPeer(message.ToId);
                    SendForwardMessagesInternal(MTProtoService, inputPeer, null, messages);

                    return;
                }
            }

            var message48 = message as TLMessage48;
            if (message48 != null && message48.FwdHeader != null && message48.FwdMessageId != null)
            {
                var inputPeer = PeerToInputPeer(message.ToId);
                SendForwardMessagesInternal(MTProtoService, inputPeer, null, new List<TLMessage25> { message48 });
                return;
            }

            var message40 = message as TLMessage40;
            if (message40 != null && message40.FwdFromPeer != null && message40.FwdMessageId != null)
            {
                var inputPeer = PeerToInputPeer(message.ToId);
                SendForwardMessagesInternal(MTProtoService, inputPeer, null, new List<TLMessage25> { message40 });
                return;
            }

            if (message.Media is TLMessageMediaEmpty || (message.InlineBotResultId != null && message.InlineBotResultQueryId.Value != 0 && !TLString.IsNullOrEmpty(message.InlineBotResultId)))
            {
                if (BadInlineBotMessage(message))
                {
                    message.Status = MessageStatus.Failed;
                    return;
                }

                SendInternal(message, MTProtoService, null, () => Status = string.Empty);
            }
            else
            {
                if (message.Media is TLMessageMediaGroup)
                {
                    var mediaGroup = (TLMessageMediaGroup)message.Media;
                    var inputMedia = new TLVector<TLInputSingleMedia>();
                    foreach (var item in mediaGroup.Group.OfType<TLMessage>())
                    {
                        var singleMedia = item.InputMedia as TLInputSingleMedia;
                        if (singleMedia != null)
                        {
                            inputMedia.Add(singleMedia);
                        }
                    }
                    if (inputMedia.Count == mediaGroup.Group.Count)
                    {
                        UploadService.SendMultiMediaInternal(inputMedia, message, MTProtoService, StateService, CacheService);
                    }
                    else
                    {
                        IoC.Get<IUploadService>().AddGroup(message);
                        UploadPhotoInternal(mediaGroup.Group.OfType<TLMessage34>().ToList());
                    }
                }
                else if (message.Media is TLMessageMediaPhoto)
                {
                    if (message.InputMedia != null)
                    {
                        UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    }
                    else
                    {
                        UploadPhotoInternal(message);
                    }
                }
                else if (message.Media is TLMessageMediaAudio)
                {
#if WP8
                    if (message.InputMedia != null)
                    {
                        UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    }
                    else
                    {
                        SendAudioInternal(message);
                    }
#endif
                }
                else if (message.Media is TLMessageMediaVideo)
                {
#if WP81
                    var file = await GetStorageFile(message.Media);

                    if (file != null)
                    {
                        SendCompressedVideoInternal(message, file);
                    }
                    else
                    {
                        MessageBox.Show(AppResources.UnableToAccessDocument, AppResources.Error, MessageBoxButton.OK);
                        DeleteMessage(message);
                    }
#else
                    SendVideoInternal(message, null);
#endif
                }
                else if (message.Media is TLMessageMediaDocument)
                {
#if WP8
                    if (message.IsVoice())
                    {
                        if (message.InputMedia != null)
                        {
                            UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
                        }
                        else
                        {
                            SendAudioInternal(message);
                        }
                        return;
                    }

                    if (message.IsVideo())
                    {
#if WP81
                        var videoFile = await GetStorageFile(message.Media);

                        if (videoFile != null)
                        {
                            SendCompressedVideoInternal(message, videoFile);
                        }
                        else
                        {
                            MessageBox.Show(AppResources.UnableToAccessDocument, AppResources.Error, MessageBoxButton.OK);
                            DeleteMessage(message);
                        }
#else
                        SendVideoInternal(message, null);
#endif
                    }
#endif
#if WP81
                    var file = await GetStorageFile(message.Media);

                    if (file != null)
                    {
                        SendDocumentInternal(message, file);
                    }
                    else
                    {
                        MessageBox.Show(AppResources.UnableToAccessDocument, AppResources.Error, MessageBoxButton.OK);
                        DeleteMessage(message);
                    }
#else
                    SendDocumentInternal(message, null);
#endif
                }
                else if (message.Media is TLMessageMediaVenue)
                {
                    if (message.InputMedia != null)
                    {
                        UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    }
                    else
                    {
                        SendVenueInternal(message);
                    }
                }
                else if (message.Media is TLMessageMediaGeo)
                {
                    if (message.InputMedia != null)
                    {
                        UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    }
                    else
                    {
                        SendLocationInternal(message);
                    }
                }
                else if (message.Media is TLMessageMediaContact)
                {
                    if (message.InputMedia != null)
                    {
                        UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    }
                    else
                    {
                        SendContactInternal(message);
                    }
                }
            }
        }

        public TLMessageBase FindGroupedMessage(TLMessage73 message73)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var m = Items[i] as TLMessage73;
                if (m != null)
                {
                    var mGroup = m.Media as TLMessageMediaGroup;
                    if (mGroup != null)
                    {
                        for (var j = 0; j < mGroup.Group.Count; j++)
                        {
                            if (mGroup.Group[j] == message73)
                            {
                                return m;
                            }
                        }
                    }
                }
            }

            return null;
        }

#if WP81
        public static async Task<StorageFile> GetStorageFile(TLMessageMediaBase media)
        {
            if (media == null) return null;
            if (media.File != null)
            {
                if (File.Exists(media.File.Path))
                {
                    return media.File;
                }
            }

            var file = await GetFileFromFolder(media.IsoFileName);

            if (file == null)
            {
                file = await GetFileFromLocalFolder(media.IsoFileName);
            }

            if (file == null)
            {
                var mediaPhoto = media as TLMessageMediaPhoto;
                if (mediaPhoto != null)
                {
                    var photo = mediaPhoto.Photo as TLPhoto;
                    if (photo != null)
                    {
                        file = await GetFileFromLocalFolder(photo.GetFileName());
                    }
                }
            }

            if (file == null)
            {
                var mediaDocument = media as TLMessageMediaDocument;
                if (mediaDocument != null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document != null)
                    {
                        file = await GetFileFromLocalFolder(document.GetFileName());
                    }
                }
            }

            if (file == null)
            {
                var mediaVideo = media as TLMessageMediaVideo;
                if (mediaVideo != null)
                {
                    var video = mediaVideo.Video as TLVideo;
                    if (video != null)
                    {
                        file = await GetFileFromLocalFolder(video.GetFileName());
                    }
                }
            }

            return file;
        }

        public static async Task<StorageFile> GetFileFromLocalFolder(string fileName)
        {
            StorageFile file = null;
            try
            {
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!string.IsNullOrEmpty(fileName)
                    && store.FileExists(fileName))
                {
                    file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(string.Format("LocalFolder.GetFileAsync({0}) exception ", fileName) + ex);
            }

            return file;
        }

        public static async Task<StorageFile> GetFileFromFolder(string fileName)
        {
            StorageFile file = null;
            try
            {
                if (!string.IsNullOrEmpty(fileName)
                    && File.Exists(fileName))
                {
                    file = await StorageFile.GetFileFromPathAsync(fileName);
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(string.Format("GetFileFromPathAsync({0}) exception ", fileName) + ex);
            }

            return file;
        }
#endif
        public bool CanSend
        {
            get
            {
                var text = GetTrimmedText(Text);

                return !string.IsNullOrEmpty(text) || Reply is TLMessagesContainter;
            }
        }

        public static string GetTrimmedText(string input)
        {
            return input != null ? input.Trim().Replace("\r", "\n").Replace("--", "—") : null;
        }

        public void Send(TLMessageBase message, TLKeyboardButtonBase keyboardButtonBase, bool fromNewMessage = false)
        {
            if (keyboardButtonBase == null) return;

            var keyboardButtonSwitchInline55 = keyboardButtonBase as TLKeyboardButtonSwitchInline55;
            if (keyboardButtonSwitchInline55 != null
                && keyboardButtonSwitchInline55.IsSamePeer)
            {
                var bot = GetBot(message);
                if (bot != null && bot.IsBot)
                {
                    _text = string.Format("@{0} {1}", bot.UserName, keyboardButtonSwitchInline55.Query);

                    var chat = With as TLChatBase;
                    if (chat != null)
                    {
                        _reply = message;
                    }

                    Execute.BeginOnUIThread(() => SendInternal(true, true));
                }

                return;
            }

            var keyboardButtonSwitchInline = keyboardButtonBase as TLKeyboardButtonSwitchInline;
            if (keyboardButtonSwitchInline != null)
            {
                var bot = GetBot(message);
                if (bot != null && bot.IsBot)
                {
                    TLObject with;
                    if (_switchPMDict.TryGetValue(bot.Index, out with))
                    {
                        _switchPMDict.Remove(bot.Index);

                        View.StopPlayersAndCreateBitmapCache(() =>
                        {
                            keyboardButtonSwitchInline.Bot = bot;
                            StateService.SwitchInlineButton = keyboardButtonSwitchInline;
                            StateService.RemoveBackEntry = true;
                            StateService.With = with;
                            NavigationService.UriFor<DialogDetailsViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
                        });
                    }
                    else
                    {
                        if (fromNewMessage) return;

                        View.StopPlayersAndCreateBitmapCache(() =>
                        {
                            keyboardButtonSwitchInline.Bot = bot;
                            StateService.SwitchInlineButton = keyboardButtonSwitchInline;
                            NavigationService.UriFor<ChooseDialogViewModel>().Navigate();
                        });
                    }
                }

                return;
            }

            var keyboardButtonUrl = keyboardButtonBase as TLKeyboardButtonUrl;
            if (keyboardButtonUrl != null)
            {
                var navstr = keyboardButtonUrl.Url.ToString();

                if (navstr.ToLowerInvariant().Contains("telegram.me")
                    || navstr.ToLowerInvariant().Contains("t.me"))
                {
                    OnTelegramLinkAction(this, new TelegramEventArgs { Uri = navstr });
                }
                else
                {
                    var result = MessageBox.Show(string.Format(AppResources.OpenUrlConfirmation, navstr), AppResources.Confirm, MessageBoxButton.OKCancel);
                    if (result != MessageBoxResult.OK) return;

                    var task = new WebBrowserTask();
                    task.URL = HttpUtility.UrlEncode(navstr);
                    task.Show();
                }

                return;
            }

            var keyboardButtonGame = keyboardButtonBase as TLKeyboardButtonGame;
            if (keyboardButtonGame != null)
            {
                if (message != null)
                {
                    var bot = GetBot(message);
                    if (bot == null) return;

                    var m = message as TLMessage;
                    if (m != null)
                    {
                        var mediaGame = m.Media as TLMessageMediaGame;
                        if (mediaGame != null)
                        {
                            OpenGame(message, bot, mediaGame.Game);
                        }
                    }
                    return;
                }

                return;
            }

            var keyboardButtonBuy = keyboardButtonBase as TLKeyboardButtonBuy;
            if (keyboardButtonBuy != null)
            {
                if (message != null)
                {
                    var bot = GetBot(message);
                    if (bot == null) return;

                    var m = message as TLMessage;
                    if (m != null)
                    {
                        var mediaInvoice = m.Media as TLMessageMediaInvoice;
                        if (mediaInvoice != null)
                        {
                            OpenInvoice(m, mediaInvoice);
                        }
                    }
                    return;
                }

                return;
            }

            var keyboardButtonCallback = keyboardButtonBase as TLKeyboardButtonCallback;
            if (keyboardButtonCallback != null)
            {
                if (message != null)
                {
                    IsWorking = true;
                    MTProtoService.GetBotCallbackAnswerAsync(Peer, message.Id, keyboardButtonCallback.Data, null,
                        result => BeginOnUIThread(() =>
                        {
                            IsWorking = false;

                            if (!TLString.IsNullOrEmpty(result.Message))
                            {
                                if (result.Alert)
                                {
                                    MessageBox.Show(result.Message.ToString());
                                }
                                else
                                {
                                    MTProtoService.SetMessageOnTime(2.0, result.Message.ToString());
                                }
                            }
                            else
                            {
                                var botCallbackAnswer54 = result as TLBotCallbackAnswer54;
                                if (botCallbackAnswer54 != null)
                                {
                                    if (!TLString.IsNullOrEmpty(botCallbackAnswer54.Url))
                                    {
                                        var user = message.ViaBot as TLUser45;
                                        if (user != null && user.IsBot)
                                        {
                                            if (!user.BotOpenUrlPermission)
                                            {
                                                var r = MessageBox.Show(string.Format(AppResources.BotOpenUrlConfirmation, "@" + user.UserName, botCallbackAnswer54.Url), AppResources.Confirm, MessageBoxButton.OKCancel);
                                                if (r != MessageBoxResult.OK) return;

                                                user.BotOpenUrlPermission = true;
                                            }
                                        }
                                        else
                                        {
                                            var r = MessageBox.Show(string.Format(AppResources.OpenUrlConfirmation, botCallbackAnswer54.Url), AppResources.Confirm, MessageBoxButton.OKCancel);
                                            if (r != MessageBoxResult.OK) return;
                                        }

                                        var task = new WebBrowserTask();
                                        task.URL = HttpUtility.UrlEncode(botCallbackAnswer54.Url.ToString());
                                        task.Show();
                                    }
                                }
                            }
                        }),
                        error =>
                        {
                            IsWorking = false;
                            Execute.ShowDebugMessage("messages.getCallbackAnswer error " + error);
                        });
                }

                return;
            }

            var keyboardButtonRequestPhone = keyboardButtonBase as TLKeyboardButtonRequestPhone;
            if (keyboardButtonRequestPhone != null)
            {
                var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
                if (currentUser != null)
                {
                    var bot = With as TLUser;
                    if (bot != null)
                    {
                        var confirmation = MessageBox.Show(AppResources.SharePhoneNumberConfirmation, AppResources.SharePhoneNumberCaption, MessageBoxButton.OKCancel);
                        if (confirmation == MessageBoxResult.OK)
                        {
                            SendContact(currentUser);
                        }
                    }
                }

                return;
            }

            var keyboardButtonRequestGeoLocation = keyboardButtonBase as TLKeyboardButtonRequestGeoLocation;
            if (keyboardButtonRequestGeoLocation != null)
            {
                var confirmation = MessageBox.Show(AppResources.ShareLocationConfirmation, AppResources.ShareLocationCaption, MessageBoxButton.OKCancel);
                if (confirmation == MessageBoxResult.OK)
                {
                    var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                    var locationSent = false;
                    //System.Diagnostics.Debug.WriteLine("Watcher location={0} status={1}", watcher.Position.Location, watcher.Status);
                    watcher.StatusChanged += (o, e) =>
                    {
                        //System.Diagnostics.Debug.WriteLine("Watcher.StatusChanged location={0} status={1}", watcher.Position.Location, watcher.Status);

                        var location = watcher.Position.Location;

                        if (watcher.Status == GeoPositionStatus.Ready)
                        {
                            if (locationSent)
                            {
                                watcher.Dispose();
                                return;
                            }

                            if (location == GeoCoordinate.Unknown)
                            {
                                MessageBox.Show(AppResources.UnableToDetermineLocation, AppResources.AppName, MessageBoxButton.OKCancel);
                            }
                            else
                            {
                                SendLocation(new TLMessageMediaGeo { Geo = new TLGeoPoint { Lat = new TLDouble(location.Latitude), Long = new TLDouble(location.Longitude) } });
                            }

                            watcher.Dispose();
                        }
                        else if (watcher.Status == GeoPositionStatus.Initializing)
                        {
                            if (location != GeoCoordinate.Unknown)
                            {
                                if (locationSent) return;

                                locationSent = true;
                                SendLocation(new TLMessageMediaGeo { Geo = new TLGeoPoint { Lat = new TLDouble(location.Latitude), Long = new TLDouble(location.Longitude) } });
                            }
                        }
                        else if (watcher.Status == GeoPositionStatus.Disabled)
                        {
                            var result = MessageBox.Show(AppResources.LocationServicesDisabled, AppResources.AppName, MessageBoxButton.OKCancel);
                            if (result == MessageBoxResult.OK)
                            {
#if WP8
                                Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
#endif
                            }
                            watcher.Dispose();
                        }
                    };
                    watcher.Start(true);
                }

                return;
            }

            _text = keyboardButtonBase.Text.ToString();
            var message31 = ReplyMarkupMessage;
            if (message31 != null && message31.ReplyMarkup != null)
            {
                message31.ReplyMarkup.HasResponse = true;
            }
            Execute.BeginOnUIThread(() => SendInternal(true, true));
        }

        public void OpenGame(TLMessageMediaGame mediaGame)
        {
            var message =
                Items.OfType<TLMessage>()
                    .FirstOrDefault(x => x.Media is TLMessageMediaGame && ((TLMessageMediaGame)x.Media) == mediaGame);

            if (message == null) return;
            var bot = GetBot(message);
            if (bot == null) return;

            OpenGame(message, bot, mediaGame.Game);
        }

        private void OpenGame(TLMessageBase message, TLUser bot, TLGame game)
        {
            var user45 = bot as TLUser45;
            if (user45 != null && !user45.IsVerified && !user45.BotPassTelegramNameToWebPagesPermission)
            {
                var confirmation = MessageBox.Show(string.Format(AppResources.OpenWebPagesViaBotConfirmation, bot.FullName2), AppResources.Confirm, MessageBoxButton.OKCancel);
                if (confirmation != MessageBoxResult.OK) return;

                user45.BotPassTelegramNameToWebPagesPermission = true;
            }

            IsWorking = true;
            MTProtoService.GetBotCallbackAnswerAsync(Peer, message.Id, null, TLBool.True,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var botCallbackAnswer54 = result as TLBotCallbackAnswer54;
                    if (botCallbackAnswer54 != null)
                    {
                        View.StopPlayersAndCreateBitmapCache(() =>
                        {
                            StateService.InputPeer = Peer;
                            StateService.Game = game;
                            StateService.BotCallbackAnswer = result;
                            StateService.ForwardMessages = new List<TLMessageBase> { message };
                            StateService.SharedContact = bot;
                            NavigationService.UriFor<WebViewModel>().Navigate();
                        });
                    }
                }),
                error =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.getCallbackAnswer error " + error);
                });
        }

        public void OpenInvoice(TLMessage message, TLMessageMediaInvoice mediaInvoice)
        {
            if (message == null) return;

            if (mediaInvoice.ReceiptMsgId != null)
            {
                OpenReceipt(message, mediaInvoice.ReceiptMsgId);

                return;
            }

            IsWorking = true;
            MTProtoService.GetPaymentFormAsync(message.Id,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    StateService.PaymentInfo = new PaymentInfo
                    {
                        Message = message,
                        Form = result,
                        With = With
                    };

                    if (result.Invoice.NameRequested
                        || result.Invoice.PhoneRequested
                        || result.Invoice.EmailRequested
                        || result.Invoice.ShippingAddressRequested)
                    {
                        View.StopPlayersAndCreateBitmapCache(() =>
                        {
                            NavigationService.UriFor<ShippingInfoViewModel>().Navigate();
                        });
                    }
                    else
                    {
                        PaymentViewModelBase.NavigateToCardInfo(View, StateService.PaymentInfo, () => StateService.GetTmpPassword(), NavigationService);
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (error.TypeEquals(ErrorType.INVOICE_ALREADY_PAID))
                    {
                        OpenReceipt(message, mediaInvoice.ReceiptMsgId);
                    }
                }));
        }

        private void OpenReceipt(TLMessage message, TLInt receiptMsgId)
        {
            if (receiptMsgId == null) return;

            IsWorking = true;
            MTProtoService.GetPaymentReceiptAsync(receiptMsgId,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    StateService.PaymentInfo = new PaymentInfo
                    {
                        Message = message,
                        Receipt = result
                    };
                    View.StopPlayersAndCreateBitmapCache(() =>
                    {
                        NavigationService.UriFor<CheckoutViewModel>().Navigate();
                    });
                }),
                error => Execute.BeginOnUIThread(() => { IsWorking = false; }));
        }

        private TLUser GetBot(TLMessageBase message)
        {
            var bot = With as TLUser;
            if (bot == null || !bot.IsBot)
            {
                bot = message.ViaBot as TLUser;
            }
            if (bot == null || !bot.IsBot)
            {
                var m = message as TLMessage;
                if (m != null)
                {
                    bot = m.From as TLUser;
                }
            }
            return bot;
        }

        public void Send(TLString command)
        {
            if (TLString.IsNullOrEmpty(command)) return;

            _text = command.ToString();
            Execute.BeginOnUIThread(() => SendInternal(false, true));
        }

        internal Stopwatch _debugTimer;

        private void SendInternal(bool useReplyMarkup, bool scrollToBottom)
        {
            _debugNotifyOfPropertyChanged = true;
            _debugTimer = Stopwatch.StartNew();
            var elapsed = new List<TimeSpan>();

            if (!CanSend) return;

            var text = GetTrimmedText(Text) ?? string.Empty;

            if (ProcessSpecialCommands(text)) return;

            //check maximum message length
            if (text.Length > Constants.MaximumMessageLength)
            {
                MessageBox.Show(String.Format(AppResources.MaximumMessageLengthExceeded, Constants.MaximumMessageLength), AppResources.Error, MessageBoxButton.OK);

                return;
            }

            // 0
            elapsed.Add(_debugTimer.Elapsed);

            elapsed.Add(_debugTimer.Elapsed);
            if (string.IsNullOrEmpty(text))
            {
                var messagesContainer = Reply as TLMessagesContainter;
                if (messagesContainer != null)
                {
                    var fwdMessages25 = messagesContainer.FwdMessages;
                    var fwdMessages = new TLVector<TLMessage>();
                    for (var i = 0; i < fwdMessages25.Count; i++)
                    {
                        fwdMessages.Add(fwdMessages25[i]);
                    }

                    if (fwdMessages25.Count > 0)
                    {
                        SendMessages(fwdMessages, m => SendForwardMessagesInternal(MTProtoService, Peer, null, fwdMessages25, messagesContainer.WithMyScore));
                    }
                }
            }
            else
            {
                // 2
                elapsed.Add(_debugTimer.Elapsed);

                string processedText;
                var entities = GetEntities(text, out processedText);

                var message = GetMessage(new TLString(processedText), new TLMessageMediaEmpty());
                if (entities != null)
                {
                    var message48 = message as TLMessage48;
                    if (message48 != null)
                    {
                        message48.Entities = new TLVector<TLMessageEntityBase>(entities);
                    }
                }

                if (Reply != null && IsWebPagePreview(Reply))
                {
                    message._media = ((TLMessagesContainter)Reply).WebPageMedia;
                    Reply = _previousReply;
                }
                else
                {
                    TLMessageMediaBase media;
                    if (_webPagesCache.TryGetValue(text, out media))
                    {
                        var webPageMessageMedia = media as TLMessageMediaWebPage;
                        if (webPageMessageMedia != null)
                        {
                            var webPage = webPageMessageMedia.WebPage;
                            if (webPage != null)
                            {
                                message.NoWebpage = true;
                            }
                        }
                    }
                }

                // 3
                elapsed.Add(_debugTimer.Elapsed);
                Text = string.Empty;

                // 4
                elapsed.Add(_debugTimer.Elapsed);
                var previousMessage = InsertSendingMessage(message, useReplyMarkup);

                // 5
                elapsed.Add(_debugTimer.Elapsed);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                // 6
                elapsed.Add(_debugTimer.Elapsed);
                var user = With as TLUser;
                if (user != null && user.IsBot && Items.Count == 1)
                {
                    NotifyOfPropertyChange(() => With);
                }

                // 7
                elapsed.Add(_debugTimer.Elapsed);
                if (scrollToBottom)
                {
                    ProcessScroll();
                }

                // 8
                elapsed.Add(_debugTimer.Elapsed);

                var sb = new StringBuilder();
                for (var i = 0; i < elapsed.Count; i++)
                {
                    sb.AppendLine(i + " " + elapsed[i]);
                }
                System.Diagnostics.Debug.WriteLine(sb);

                BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    var shellViewModel = IoC.Get<ShellViewModel>();
                    shellViewModel.CloseSearch();
                });

                _debugNotifyOfPropertyChanged = false;
                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        m => SendInternal(message, MTProtoService, null, () => Status = string.Empty)));
            }
        }

        public IList<TLMessageEntityBase> GetEntities(string text, out string processedText)
        {
            var entities = new List<TLMessageEntityBase>();

            var startIndex = -1;
            var entityString = string.Empty;
            var entityLength = 0;

            for (var i = 0; i < text.Length; i++)
            {
                // mention name
                if (text[i] == '@')
                {
                    startIndex = i;

                    entityString = GetMentionNameEntityString(text, i, out entityLength);
                    if (!string.IsNullOrEmpty(entityString))
                    {
                        var user = _mentions.FirstOrDefault();
                        if (user != null)
                        {
                            _mentions.RemoveAt(0);

                            var entity = new TLInputMessageEntityMentionName
                            {
                                Length = new TLInt(entityLength + 3),
                                Offset = new TLInt(startIndex),
                                User = user.ToInputUser(),
                                Name = entityString
                            };

                            entities.Add(entity);

                            i = i + entityLength + 2;
                        }
                    }

                    entityString = GetMentionEntityString(text, i, out entityLength);
                    if (!string.IsNullOrEmpty(entityString))
                    {
                        var entity = new TLMessageEntityMention
                        {
                            Length = new TLInt(entityLength + 1),
                            Offset = new TLInt(startIndex)
                        };

                        entities.Add(entity);

                        i = i + entityLength + 0;
                    }
                }

                // bold
                else if (text[i] == '*' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    startIndex = i;

                    entityString = GetTwoSymbolEntityString('*', text, i, out entityLength);
                    if (!string.IsNullOrEmpty(entityString))
                    {
                        var entity = new TLMessageEntityBold
                        {
                            Length = new TLInt(entityLength + 4),
                            Offset = new TLInt(startIndex)
                        };

                        entities.Add(entity);

                        i = i + entityLength + 3;
                    }
                }

                // italic
                else if (text[i] == '_' && i + 1 < text.Length && text[i + 1] == '_')
                {
                    startIndex = i;

                    entityString = GetTwoSymbolEntityString('_', text, i, out entityLength);
                    if (!string.IsNullOrEmpty(entityString))
                    {
                        var entity = new TLMessageEntityItalic
                        {
                            Length = new TLInt(entityLength + 4),
                            Offset = new TLInt(startIndex)
                        };

                        entities.Add(entity);

                        i = i + entityLength + 3;
                    }
                }

                // code
                else if (text[i] == '`')
                {
                    startIndex = i;

                    entityString = GetCodeEntityString(text, i, out entityLength);
                    if (!string.IsNullOrEmpty(entityString))
                    {
                        var entity = new TLMessageEntityCode
                        {
                            Length = new TLInt(entityLength + 2),
                            Offset = new TLInt(startIndex)
                        };

                        entities.Add(entity);

                        i = i + entityLength + 1;
                    }
                }

                // pre
                else if (text[i] == '`' && i + 2 < text.Length && text[i + 1] == '`' && text[i + 2] == '`')
                {
                    startIndex = i;

                    entityString = GetPreEntityString(text, i, out entityLength);
                    if (!string.IsNullOrEmpty(entityString))
                    {
                        var entity = new TLMessageEntityPre
                        {
                            Length = new TLInt(entityLength + 6),
                            Offset = new TLInt(startIndex),
                            Language = TLString.Empty
                        };

                        entities.Add(entity);

                        i = i + entityLength + 5;
                    }
                }
            }

            var count = entities.Count;
            if (count > 0)
            {
                var bulder = new StringBuilder();

                //text
                for (var i = 0; i < entities.Count; i++)
                {
                    // prefix i
                    var prefixOffset = i == 0 ? 0 : entities[i - 1].Offset.Value + entities[i - 1].Length.Value;
                    var prefixLength = i == 0 ? entities[i].Offset.Value : entities[i].Offset.Value - prefixOffset;
                    var prefix = text.Substring(prefixOffset, prefixLength);
                    bulder.Append(prefix);

                    // entity i
                    var entityMentionName = entities[i] as TLInputMessageEntityMentionName;
                    if (entityMentionName != null)
                    {
                        bulder.Append(entityMentionName.Name);
                        continue;
                    }
                    var entityMention = entities[i] as TLMessageEntityMention;
                    if (entityMention != null)
                    {
                        bulder.Append(text.Substring(entityMention.Offset.Value, entityMention.Length.Value));
                        continue;
                    }
                    var entityBold = entities[i] as TLMessageEntityBold;
                    if (entityBold != null)
                    {
                        bulder.Append(text.Substring(entityBold.Offset.Value + 2, entityBold.Length.Value - 4));
                        continue;
                    }
                    var entityItalic = entities[i] as TLMessageEntityItalic;
                    if (entityItalic != null)
                    {
                        bulder.Append(text.Substring(entityItalic.Offset.Value + 2, entityItalic.Length.Value - 4));
                        continue;
                    }
                    var entityCode = entities[i] as TLMessageEntityCode;
                    if (entityCode != null)
                    {
                        bulder.Append(text.Substring(entityCode.Offset.Value + 1, entityCode.Length.Value - 2));
                        continue;
                    }
                    var entityPre = entities[i] as TLMessageEntityPre;
                    if (entityPre != null)
                    {
                        bulder.Append(text.Substring(entityPre.Offset.Value + 3, entityPre.Length.Value - 6));
                        continue;
                    }

                    bulder.Append(text.Substring(entities[i].Offset.Value, entities[i].Length.Value));
                }

                //postfix
                var lastEntity = entities[count - 1];
                var postfix = text.Substring(lastEntity.Offset.Value + lastEntity.Length.Value);
                bulder.Append(postfix);
                processedText = bulder.ToString();

                var removedLength = 0;
                var totalRemovedLength = 0;
                for (var i = 0; i < entities.Count; i++)
                {
                    var entityName = entities[i] as TLInputMessageEntityMentionName;
                    if (entityName != null)
                    {
                        removedLength = 3;  // @(...)
                    }
                    var entityMention = entities[i] as TLMessageEntityMention;
                    if (entityMention != null)
                    {
                        removedLength = 0;  // @...
                    }
                    var bold = entities[i] as TLMessageEntityBold;
                    if (bold != null)
                    {
                        removedLength = 4;  // **...**
                    }
                    var italic = entities[i] as TLMessageEntityItalic;
                    if (italic != null)
                    {
                        removedLength = 4;  // __...__
                    }
                    var code = entities[i] as TLMessageEntityCode;
                    if (code != null)
                    {
                        removedLength = 2;  // `...`
                    }
                    var pre = entities[i] as TLMessageEntityPre;
                    if (pre != null)
                    {
                        removedLength = 6;  // ```...```
                    }
                    entities[i].Offset = new TLInt(entities[i].Offset.Value - totalRemovedLength);
                    entities[i].Length = new TLInt(entities[i].Length.Value - removedLength);
                    totalRemovedLength += removedLength;
                }
            }
            else
            {
                processedText = text;
            }

            ClearMentions();

            return entities;
        }

        private string GetMentionEntityString(string text, int startPosition, out int l)
        {
            l = 0;

            if (text.Length < startPosition + 3) return string.Empty;
            if (text[startPosition] != '@') return string.Empty;

            var length = 0;
            for (var i = startPosition + 1; i < text.Length; i++)
            {
                if (BrowserNavigationService.IsValidUsernameSymbol(text[i]))
                {
                    length++;
                }
                else
                {
                    break;
                }
            }

            if (length <= 3 || length > 32)
            {
                return string.Empty;
            }

            l = length;
            return text.Substring(startPosition, length + 1);
        }

        private string GetMentionNameEntityString(string text, int startPosition, out int l)
        {
            l = 0;

            if (text.Length < startPosition + 2) return string.Empty;
            if (text[startPosition] != '@') return string.Empty;
            if (text[startPosition + 1] != '(') return string.Empty;

            var length = 0;
            var hasCloseSymbol = false;
            for (var i = startPosition + 2; i < text.Length; i++)
            {
                if (text[i] != ')')
                {
                    length++;
                }
                else
                {
                    hasCloseSymbol = true;
                    break;
                }
            }

            if (!hasCloseSymbol)
            {
                return string.Empty;
            }

            l = length;
            return text.Substring(startPosition + 2, length);
        }

        private string GetCodeEntityString(string text, int startPosition, out int l)
        {
            l = 0;

            if (text.Length < startPosition + 2) return string.Empty;
            if (text[startPosition] != '`') return string.Empty;

            var length = 0;
            var hasCloseSymbol = false;
            for (var i = startPosition + 1; i < text.Length; i++)
            {
                if (text[i] != '`')
                {
                    length++;
                }
                else
                {
                    hasCloseSymbol = true;
                    break;
                }
            }

            if (!hasCloseSymbol)
            {
                return string.Empty;
            }

            l = length;
            return text.Substring(startPosition + 1, length);
        }

        private string GetPreEntityString(string text, int startPosition, out int l)
        {
            l = 0;

            if (text.Length < startPosition + 6) return string.Empty;
            if (text[startPosition] != '`') return string.Empty;
            if (text[startPosition + 1] != '`') return string.Empty;
            if (text[startPosition + 2] != '`') return string.Empty;

            var length = 0;
            var hasCloseSymbol = false;
            for (var i = startPosition + 3; i < text.Length; i++)
            {
                if (text[i] != '`')
                {
                    length++;
                }
                else
                {
                    hasCloseSymbol = i + 2 < text.Length && text[i + 1] == '`' && text[i + 2] == '`';
                    break;
                }
            }

            if (!hasCloseSymbol)
            {
                return string.Empty;
            }

            l = length;
            return text.Substring(startPosition + 1, length);
        }

        private string GetTwoSymbolEntityString(char symbol, string text, int startPosition, out int l)
        {
            l = 0;

            if (text.Length < startPosition + 5) return string.Empty;
            if (text[startPosition] != symbol) return string.Empty;
            if (text[startPosition + 1] != symbol) return string.Empty;

            var length = 0;
            var hasCloseSymbol = false;
            for (var i = startPosition + 2; i < text.Length; i++)
            {
                if (text[i] != symbol)
                {
                    length++;
                }
                else
                {
                    hasCloseSymbol = i + 1 < text.Length && text[i + 1] == symbol;
                    break;
                }
            }

            if (!hasCloseSymbol)
            {
                return string.Empty;
            }

            l = length;
            return text.Substring(startPosition + 2, length);
        }

        private string GetIndexString(string text, int startPosition, out int l)
        {
            l = 0;

            if (text.Length < startPosition + 1) return string.Empty;

            var length = 0;
            for (var i = startPosition; i < text.Length; i++)
            {
                if (text[i] >= '0' && text[i] <= '9')
                {
                    length++;
                }
                else if (text[i] == ' ')
                {
                    break;
                }
                else
                {
                    return string.Empty;
                }
            }

            l = length;
            return text.Substring(startPosition, length);
        }

        private void SendMessages(IList<TLMessage> messages, Action<IList<TLMessage>> callback)
        {
            var previousMessage = Items.FirstOrDefault();

            if (messages.Count == 1)
            {
                InsertSendingMessage(messages[0] as TLMessage25);
            }
            else
            {
                var uploadService = IoC.Get<IUploadService>();

                var mediaMessage = messages.FirstOrDefault() as TLMessage73;
                var groupedId = mediaMessage != null ? mediaMessage.GroupedId : null;
                if (groupedId != null)
                {
                    var messageMediaGroup = new TLMessageMediaGroup { Group = new TLVector<TLMessageBase>() };
                    var message = (TLMessage73)GetMessage(TLString.Empty, messageMediaGroup);
                    message.Status = MessageStatus.Sending;
                    message.ReplyToMsgId = mediaMessage.ReplyToMsgId;
                    message.Reply = mediaMessage.Reply;
                    message.GroupedId = groupedId;

                    for (var i = 0; i < messages.Count; i++)
                    {
                        if (i % Constants.MaxGroupedMediaCount == 0)
                        {
                            if (messageMediaGroup.Group.Count > 0)
                            {
                                uploadService.AddGroup(message);
                                Items.Insert(0, message);
                                if (message.GroupedId != null)
                                {
                                    _group[message.GroupedId.Value] = message;
                                }
                            }

                            mediaMessage = messages[i] as TLMessage73;
                            groupedId = mediaMessage != null ? mediaMessage.GroupedId : null;

                            messageMediaGroup = new TLMessageMediaGroup { Group = new TLVector<TLMessageBase>() };
                            message = (TLMessage73)GetMessage(TLString.Empty, messageMediaGroup);
                            message.Status = MessageStatus.Sending;
                            message.ReplyToMsgId = mediaMessage.ReplyToMsgId;
                            message.Reply = mediaMessage.Reply;
                            message.GroupedId = groupedId;
                        }

                        messageMediaGroup.Group.Add(messages[i]);
                    }

                    if (messageMediaGroup.Group.Count > 0)
                    {
                        uploadService.AddGroup(message);
                        Items.Insert(0, message);
                        if (message.GroupedId != null)
                        {
                            _group[message.GroupedId.Value] = message;
                        }
                    }
                }
                else
                {
                    foreach (var message in messages)
                    {
                        CheckChannelMessage(message as TLMessage25);
                        Items.Insert(0, message);
                    }
                }

                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                Reply = null;
            }

            BeginOnThreadPool(() =>
                CacheService.SyncSendingMessages(
                    messages, previousMessage,
                    callback.SafeInvoke));
        }

        private bool _debugNotifyOfPropertyChanged;

        public override void NotifyOfPropertyChange(string propertyName)
        {
            if (_debugNotifyOfPropertyChanged)
            {
                System.Diagnostics.Debug.WriteLine(propertyName);
                //Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(propertyName));
            }

            base.NotifyOfPropertyChange(propertyName);
        }

        public void Send()
        {
            SendInternal(false, false);
        }

        private TLMessageBase InsertSendingMessage(TLMessage25 message, bool useReplyMarkup = false)
        {
            CheckChannelMessage(message);

            TLMessageBase previousMessage;
#if WP8
            if (_isFirstSliceLoaded)
            {
                if (useReplyMarkup
                    && ReplyMarkupMessage != null)
                {
                    var chatBase = With as TLChatBase;
                    if (chatBase != null)
                    {
                        message.ReplyToMsgId = ReplyMarkupMessage.Id;
                        message.Reply = ReplyMarkupMessage;
                    }

                    BeginOnUIThread(() =>
                    {
                        if (Reply != null)
                        {
                            Reply = null;
                            SetReplyMarkup(null);
                        }
                    });
                }

                var messagesContainer = Reply as TLMessagesContainter;
                if (Reply != null)
                {
                    if (Reply.Index != 0)
                    {
                        message.ReplyToMsgId = Reply.Id;
                        message.Reply = Reply;
                    }
                    else
                    {
                        if (messagesContainer != null)
                        {
                            if (!string.IsNullOrEmpty(message.Message.ToString()))
                            {
                                message.Reply = Reply;
                            }
                        }
                    }

                    var message31 = Reply as TLMessage31;
                    if (message31 != null)
                    {
                        var replyMarkup = message31.ReplyMarkup;
                        if (replyMarkup != null)
                        {
                            replyMarkup.HasResponse = true;
                        }
                    }

                    BeginOnUIThread(() =>
                    {
                        //var emptyMedia = message.Media as TLMessageMediaEmpty;
                        //if (emptyMedia != null)
                        {
                            Reply = null;
                        }
                    });
                }

                previousMessage = Items.FirstOrDefault();
                Items.Insert(0, message);

                if (messagesContainer != null)
                {
                    var message48 = message as TLMessage48;
                    if (message48 != null && message48.FwdHeader == null)
                    {
                        foreach (var fwdMessage in messagesContainer.FwdMessages)
                        {
                            CheckChannelMessage(fwdMessage as TLMessage25);
                            Items.Insert(0, fwdMessage);
                        }
                    }
                }

                for (var i = 1; i < Items.Count; i++)
                {
                    var serviceMessage = Items[i] as TLMessageService;
                    if (serviceMessage != null)
                    {
                        var unreadMessagesAction = serviceMessage.Action as TLMessageActionUnreadMessages;
                        if (unreadMessagesAction != null)
                        {
                            Items.RemoveAt(i);
                            break;
                        }
                    }
                }

                Execute.BeginOnUIThread(RaiseScrollToBottom);
            }
            else
            {

                var messagesContainer = Reply as TLMessagesContainter;
                if (Reply != null)
                {
                    if (Reply.Index != 0)
                    {
                        message.ReplyToMsgId = Reply.Id;
                        message.Reply = Reply;
                    }
                    else
                    {
                        if (messagesContainer != null)
                        {
                            if (!string.IsNullOrEmpty(message.Message.ToString()))
                            {
                                message.Reply = Reply;
                            }
                        }
                    }

                    Reply = null;
                }

                Items.Clear();
                Items.Add(message);
                var messages = CacheService.GetHistory(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));
                previousMessage = messages.FirstOrDefault();
                for (var i = 0; i < messages.Count; i++)
                {
                    Items.Add(messages[i]);
                }

                if (messagesContainer != null)
                {
                    if (!string.IsNullOrEmpty(message.Message.ToString()))
                    {
                        foreach (var fwdMessage in messagesContainer.FwdMessages)
                        {
                            Items.Insert(0, fwdMessage);
                        }
                    }
                }

                for (var i = 1; i < Items.Count; i++)
                {
                    var serviceMessage = Items[i] as TLMessageService;
                    if (serviceMessage != null)
                    {
                        var unreadMessagesAction = serviceMessage.Action as TLMessageActionUnreadMessages;
                        if (unreadMessagesAction != null)
                        {
                            Items.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
#else
            var messagesContainer = Reply as TLMessagesContainter;
            if (Reply != null)
            {
                if (Reply.Index != 0)
                {
                    message.ReplyToMsgId = Reply.Id;
                    message.Reply = Reply;
                }
                else
                {
                    if (messagesContainer != null)
                    {
                        if (!string.IsNullOrEmpty(message.Message.ToString()))
                        {
                            message.Reply = Reply;
                        }
                    }
                }

                Reply = null;
            }

            previousMessage = Items.FirstOrDefault();
            Items.Insert(0, message);

            if (messagesContainer != null)
            {
                if (!string.IsNullOrEmpty(message.Message.ToString()))
                {
                    foreach (var fwdMessage in messagesContainer.FwdMessages)
                    {
                        Items.Insert(0, fwdMessage);
                    }
                }
            }

            for (var i = 1; i < Items.Count; i++)
            {
                var serviceMessage = Items[i] as TLMessageService;
                if (serviceMessage != null)
                {
                    var unreadMessagesAction = serviceMessage.Action as TLMessageActionUnreadMessages;
                    if (unreadMessagesAction != null)
                    {
                        Items.RemoveAt(i);
                        break;
                    }
                }
            }

            Execute.BeginOnUIThread(RaiseScrollToBottom);
#endif
            return previousMessage;
        }

        private bool ProcessSpecialCommands(string text)
        {
            if (string.IsNullOrEmpty(text) || text[0] != '/') return false;

            if (string.Equals(text, "/tlg_stgs", StringComparison.OrdinalIgnoreCase))
            {

                Execute.BeginOnThreadPool(async () =>
                {
                    using (var fileStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("__ApplicationSettings"))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            var line = streamReader.ReadToEnd() ?? string.Empty;

                            try
                            {
                                Execute.BeginOnUIThread(() =>
                                {
                                    MessageBox.Show(line);
                                    Clipboard.SetText(line);
                                });
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                });

                Text = string.Empty;
                return true;
            }

            if (string.Equals(text, "/tlg_msgs_err", StringComparison.OrdinalIgnoreCase))
            {
                ShowLastSyncErrors(info =>
                {
                    try
                    {
                        Clipboard.SetText(info);
                    }
                    catch (Exception ex)
                    {

                    }
                });
                Text = string.Empty;
                return true;
            }

            if (text != null
                && text.StartsWith("/tlg_msgs", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parameters = text.Split(' ');
                    var limit = 15;
                    if (parameters.Length > 1)
                    {
                        limit = Convert.ToInt32(parameters[1]);
                    }

                    ShowMessagesInfo(limit, info =>
                    {
                        try
                        {
                            Clipboard.SetText(info);
                        }
                        catch (Exception ex)
                        {

                        }
                    });
                    Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Execute.BeginOnUIThread(() => MessageBox.Show("Unknown command"));
                }
                return true;
            }

            if (string.Equals(text, "/tlg_cfg", StringComparison.OrdinalIgnoreCase))
            {
                ShowConfigInfo(info =>
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        try
                        {

                            MessageBox.Show(info);
                            Clipboard.SetText(info);
                        }
                        catch (Exception ex)
                        {
                        }

                    });
                });
                Text = string.Empty;
                return true;
            }

            if (string.Equals(text, "/tlg_tr", StringComparison.OrdinalIgnoreCase))
            {
                ShowTransportInfo(info =>
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        try
                        {

                            MessageBox.Show(info);
                            Clipboard.SetText(info);
                        }
                        catch (Exception ex)
                        {
                        }
                    });
                });

                Text = string.Empty;
                return true;
            }

            if (text != null
                && text.StartsWith("/tlg_del_c", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parameters = text.Split(' ');
                    var id = -1;
                    if (parameters.Length > 1)
                    {
                        id = Convert.ToInt32(parameters[1]);
                    }

                    var chat = CacheService.GetChat(new TLInt(id));
                    if (chat != null)
                    {
                        CacheService.DeleteChat(new TLInt(id));
                        CacheService.Commit();
                    }
                    Execute.BeginOnUIThread(() => MessageBox.Show("Complete"));
                    Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Execute.BeginOnUIThread(() => MessageBox.Show("Unknown command"));
                }

                return true;
            }

            if (text != null
                && text.StartsWith("/tlg_del_u", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parameters = text.Split(' ');
                    var id = -1;
                    if (parameters.Length > 1)
                    {
                        id = Convert.ToInt32(parameters[1]);
                    }

                    var user = CacheService.GetUser(new TLInt(id));
                    if (user != null)
                    {
                        CacheService.DeleteUser(new TLInt(id));
                        CacheService.Commit();
                    }
                    Execute.BeginOnUIThread(() => MessageBox.Show("Complete"));
                    Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Execute.BeginOnUIThread(() => MessageBox.Show("Unknown command"));
                }
                return true;
            }

            if (text != null
                && text.StartsWith("/tlg_up_tr", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parameters = text.Split(' ');
                    var dcOption = new TLDCOption78
                    {
                        Flags = new TLInt(0),
                        Id = new TLInt(Convert.ToInt32(parameters[1])),
                        IpAddress = new TLString(parameters[2]),
                        Port = new TLInt(Convert.ToInt32(parameters[3]))
                    };

                    MTProtoService.UpdateTransportInfoAsync(dcOption, dcOption.IpAddress, dcOption.Port,
                        result =>
                        {
                            Execute.BeginOnUIThread(() => MessageBox.Show("Complete /tlg_up_tr"));
                        });

                    Text = string.Empty;

                    //ShowTransportInfo(info =>
                    //{
                    //    Execute.BeginOnUIThread(() =>
                    //    {
                    //        try
                    //        {

                    //            MessageBox.Show(info);
                    //            Clipboard.SetText(info);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //        }

                    //        Text = string.Empty;
                    //    });
                    //});
                }
                catch (Exception ex)
                {
                    Execute.BeginOnUIThread(() => MessageBox.Show("Unknown command"));
                }
                return true;
            }

            return false;
        }

        public event EventHandler<ScrollToEventArgs> ScrollTo;

        protected virtual void RaiseScrollTo(ScrollToEventArgs args)
        {
            var handler = ScrollTo;
            if (handler != null) handler(this, args);
        }

        public event EventHandler ScrollToBottom;

        protected virtual void RaiseScrollToBottom()
        {
            var handler = ScrollToBottom;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public static TLInputPeerBase PeerToInputPeer(TLPeerBase peer)
        {
            if (peer is TLPeerUser)
            {
                var cachedUser = IoC.Get<ICacheService>().GetUser(peer.Id);
                if (cachedUser != null)
                {
                    var userForeign = cachedUser as TLUserForeign;
                    var userRequest = cachedUser as TLUserRequest;
                    var user = cachedUser as TLUser;

                    if (userForeign != null)
                    {
                        return new TLInputPeerUser { UserId = userForeign.Id, AccessHash = userForeign.AccessHash };
                    }

                    if (userRequest != null)
                    {
                        return new TLInputPeerUser { UserId = userRequest.Id, AccessHash = userRequest.AccessHash };
                    }

                    if (user != null)
                    {
                        return user.ToInputPeer();
                    }

                    return new TLInputPeerUser { UserId = peer.Id, AccessHash = new TLLong(0) };
                }

                return new TLInputPeerUser { UserId = peer.Id, AccessHash = new TLLong(0) };
            }

            if (peer is TLPeerChannel)
            {
                var channel = IoC.Get<ICacheService>().GetChat(peer.Id) as TLChannel;
                if (channel != null)
                {
                    return new TLInputPeerChannel { ChatId = peer.Id, AccessHash = channel.AccessHash };
                }
            }

            if (peer is TLPeerChat)
            {
                return new TLInputPeerChat { ChatId = peer.Id };
            }

            return new TLInputPeerBroadcast { ChatId = peer.Id };
        }

        public static void SendInternal(TLMessage25 message, IMTProtoService mtProtoService, System.Action callback = null, System.Action faultCallback = null)
        {
            var cacheService = IoC.Get<ICacheService>();

            var inputPeer = PeerToInputPeer(message.ToId);

            if (inputPeer is TLInputPeerBroadcast && !(inputPeer is TLInputPeerChannel))
            {
                var broadcast = cacheService.GetBroadcast(message.ToId.Id);
                var contacts = new TLVector<TLInputUserBase>();

                foreach (var participantId in broadcast.ParticipantIds)
                {
                    var contact = IoC.Get<ICacheService>().GetUser(participantId);
                    contacts.Add(contact.ToInputUser());
                }

                mtProtoService.SendBroadcastAsync(contacts, new TLInputMediaEmpty(), message,
                    result =>
                    {
                        message.Status = MessageStatus.Confirmed;
                        callback.SafeInvoke();
                    },
                    () =>
                    {
                        message.Status = MessageStatus.Confirmed;
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.sendBroadcast error " + error);

                        if (message.Status == MessageStatus.Broadcast)
                        {
                            message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                        }

                        faultCallback.SafeInvoke();
                    });
            }
            else
            {
                var message45 = message as TLMessage45;
                if (message45 != null
                    && message45.InlineBotResultQueryId != null
                    && !TLString.IsNullOrEmpty(message45.InlineBotResultId))
                {

                    mtProtoService.SendInlineBotResultAsync(
                        message45,
                        result =>
                        {
                            Execute.BeginOnUIThread(() =>
                            {
                                var mediaGame = message.Media as TLMessageMediaGame;
                                if (mediaGame != null)
                                {
                                    mediaGame.NotifyOfPropertyChange(() => mediaGame.Message);
                                    mediaGame.NotifyOfPropertyChange(() => mediaGame.MessageVisibility);
                                    mediaGame.NotifyOfPropertyChange(() => mediaGame.DescriptionVisibility);
                                }

                                message.NotifyOfPropertyChange(() => message.Media);
                                message.NotifyOfPropertyChange(() => message45.ReplyMarkup);
                            });

                            callback.SafeInvoke();
                        },
                        () =>
                        {
                            message.Status = MessageStatus.Confirmed;
                        },
                        error => Execute.BeginOnUIThread(() =>
                        {
                            if (error.TypeEquals(ErrorType.PEER_FLOOD))
                            {
                                //MessageBox.Show(AppResources.PeerFloodSendMessage, AppResources.Error, MessageBoxButton.OK);
                                ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodSendMessage, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                                    result =>
                                    {
                                        if (result == CustomMessageBoxResult.RightButton)
                                        {
                                            TelegramViewBase.NavigateToUsername(mtProtoService, Constants.SpambotUsername, null, null, null);
                                        }
                                    });
                            }
                            else if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                            {
                                MessageBox.Show("messages.sendInlineBotResult error " + error, AppResources.Error, MessageBoxButton.OK);
                            }
                            else
                            {
                                Execute.ShowDebugMessage("messages.sendInlineBotResult error " + error);
                            }

                            if (message.Status == MessageStatus.Sending)
                            {
                                message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                            }

                            faultCallback.SafeInvoke();
                        }));
                }
                else
                {
                    mtProtoService.SendMessageAsync(
                        (TLMessage36)message,
                        result =>
                        {
                            callback.SafeInvoke();
                        },
                        () =>
                        {
                            message.Status = MessageStatus.Confirmed;
                        },
                        error => Execute.BeginOnUIThread(() =>
                        {
                            if (error.TypeEquals(ErrorType.PEER_FLOOD))
                            {
                                //MessageBox.Show(AppResources.PeerFloodSendMessage, AppResources.Error, MessageBoxButton.OK);
                                ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodSendMessage, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                                    result =>
                                    {
                                        if (result == CustomMessageBoxResult.RightButton)
                                        {
                                            TelegramViewBase.NavigateToUsername(mtProtoService, Constants.SpambotUsername, null, null, null);
                                        }
                                    });
                            }
                            else if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                            {
                                MessageBox.Show("messages.sendMessage error " + error, AppResources.Error, MessageBoxButton.OK);
                            }
                            else
                            {
                                Execute.ShowDebugMessage("messages.sendMessage error " + error);
                            }

                            if (message.Status == MessageStatus.Sending)
                            {
                                message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                            }

                            faultCallback.SafeInvoke();
                        }));
                }

                SendForwardedMessages(mtProtoService, inputPeer, message);
            }
        }

        public void OpenFwdContactDetails(TLObject obj)
        {
            var messageForwarded = obj as TLMessageForwarded;
            if (messageForwarded != null)
            {
                if (messageForwarded.FwdFrom == null) return;

                StateService.CurrentContact = messageForwarded.FwdFrom;
                NavigationService.UriFor<ContactViewModel>().Navigate();
            }

            var message25 = obj as TLMessage25;
            if (message25 != null)
            {
                if (message25.FwdFrom == null) return;

                var fwdHeader = message25.FwdFrom as TLMessageFwdHeader;
                if (fwdHeader != null)
                {
                    if (fwdHeader.ChannelId != null)
                    {
                        var channel = CacheService.GetChat(fwdHeader.ChannelId) as TLChannel;
                        if (channel != null)
                        {
                            if (With == channel)
                            {
                                OpenMessage(message25, fwdHeader.ChannelPost);

                                return;
                            }

                            if (fwdHeader.ChannelPost != null)
                            {
                                StateService.Post = fwdHeader.ChannelPost.Value.ToString(CultureInfo.InvariantCulture);
                            }
                            StateService.With = channel;
                            StateService.RemoveBackEntries = true;
                            NavigationService.Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                            return;
                        }
                    }
                    else if (fwdHeader.FromId != null)
                    {
                        var user = CacheService.GetUser(fwdHeader.FromId);
                        if (user != null)
                        {
                            StateService.CurrentContact = user;
                            NavigationService.UriFor<ContactViewModel>().Navigate();
                            return;
                        }
                    }
                }

                var fwdFromUser = message25.FwdFrom as TLUserBase;
                if (fwdFromUser != null)
                {
                    StateService.CurrentContact = fwdFromUser;
                    NavigationService.UriFor<ContactViewModel>().Navigate();
                    return;
                }

                var fwdFromChannel = message25.FwdFrom as TLChannel;
                if (fwdFromChannel != null)
                {
                    StateService.With = fwdFromChannel;
                    StateService.RemoveBackEntries = true;
                    NavigationService.Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                    return;
                }

                var fwdFromChat = message25.FwdFrom as TLChatBase;
                if (fwdFromChat != null)
                {
                    StateService.CurrentChat = fwdFromChat;
                    NavigationService.UriFor<ChatViewModel>().Navigate();
                    return;
                }
            }
        }

        public void ShowUserProfile(TLMessage message)
        {
            if (message == null) return;

            StateService.CurrentContact = message.From as TLUserBase;
            Execute.BeginOnUIThread(() => NavigationService.UriFor<ContactViewModel>().Navigate());
        }

        public void CancelUploading(TLMessageMediaBase media)
        {
            TLMessage message = null;
            foreach (var item in UngroupEnumerator(Items))
            {
                var messageCommon = item as TLMessage;
                if (messageCommon != null && messageCommon.Media == media)
                {
                    message = messageCommon;
                    break;
                }
            }

            if (message != null)
            {
                DeleteUploadingMessage(message);
            }
        }

        public void CancelVideoDownloading(TLMessageMediaBase mediaVideo)
        {
            BeginOnThreadPool(() =>
            {
                BeginOnUIThread(() =>
                {
                    var message = Items.OfType<TLMessage>().FirstOrDefault(x => x.Media == mediaVideo);
                    DownloadVideoFileManager.CancelDownloadFileAsync(message);

                    mediaVideo.IsCanceled = true;
                    mediaVideo.LastProgress = mediaVideo.DownloadingProgress;
                    mediaVideo.DownloadingProgress = 0.0;
                });
            });
        }

        public void CancelDocumentDownloading(TLMessageMediaBase mediaDocument)
        {
            if (mediaDocument.DownloadingProgress > 0.0)
            {
                BeginOnUIThread(() =>
                {
                    var message = Items.OfType<TLMessage>().FirstOrDefault(x => x.Media == mediaDocument);
                    DownloadDocumentFileManager.CancelDownloadFileAsync(message);

                    mediaDocument.IsCanceled = true;
                    mediaDocument.LastProgress = mediaDocument.DownloadingProgress;
                    mediaDocument.DownloadingProgress = 0.0;
                });
            }
            else if (mediaDocument.UploadingProgress > 0.0)
            {
                CancelUploading(mediaDocument);
            }
        }

        public void CancelDownloading(TLPhotoBase photo)
        {
            BeginOnThreadPool(() =>
            {
                DownloadFileManager.CancelDownloadFile(photo);
            });
        }

        public void CancelPhotoDownloading(TLMessageMediaPhoto mediaPhoto)
        {
            BeginOnUIThread(() =>
            {
                DownloadFileManager.CancelDownloadFile(mediaPhoto.Photo);


                mediaPhoto.IsCanceled = true;
                mediaPhoto.LastProgress = mediaPhoto.DownloadingProgress;
                mediaPhoto.DownloadingProgress = 0.0;
            });
        }

        public void CancelAudioDownloading(TLMessageMediaBase mediaBase)
        {
            var mediaDocument = mediaBase as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                if (mediaDocument.UploadingProgress > 0.0)
                {
                    CancelUploading(mediaDocument);
                }
                else
                {
                    BeginOnUIThread(() =>
                    {
                        DownloadAudioFileManager.CancelDownloadFile(mediaDocument);

                        mediaDocument.IsCanceled = true;
                        mediaDocument.LastProgress = mediaDocument.DownloadingProgress;
                        mediaDocument.DownloadingProgress = -0.01;
                    });
                }
                return;
            }

            var mediaAudio = mediaBase as TLMessageMediaAudio;
            if (mediaAudio != null)
            {
                if (mediaAudio.UploadingProgress > 0.0)
                {
                    CancelUploading(mediaAudio);
                }
                else
                {
                    BeginOnUIThread(() =>
                    {
                        DownloadAudioFileManager.CancelDownloadFile(mediaAudio);

                        mediaAudio.IsCanceled = true;
                        mediaAudio.LastProgress = mediaAudio.DownloadingProgress;
                        mediaAudio.DownloadingProgress = -0.01;
                    });
                }
                return;
            }
        }

        public void OpenChatPhoto()
        {
            var user = With as TLUserBase;
            if (user != null)
            {
                var photo = user.Photo as TLUserProfilePhoto;
                if (photo != null)
                {
                    //StateService.CurrentPhoto = photo;
                    //NavigationService.UriFor<ProfilePhotoViewerViewModel>().Navigate();
                    return;
                }
            }

            var chat = With as TLChat;
            if (chat != null)
            {
                var photo = chat.Photo as TLChatPhoto;
                if (photo != null)
                {
                    //StateService.CurrentPhoto = photo;
                    //NavigationService.UriFor<ProfilePhotoViewerViewModel>().Navigate();
                    return;
                }
            }
        }

        public void CancelDownloading()
        {
            BeginOnThreadPool(() =>
            {
                BeginOnUIThread(() =>
                {
                    foreach (var item in Items.OfType<TLMessage>())
                    {
                        var mediaPhoto = item.Media as TLMessageMediaPhoto;
                        if (mediaPhoto != null)
                        {
                            CancelDownloading(mediaPhoto.Photo);
                        }
                    }
                });
            });
        }

        public void PinToStart()
        {
            DialogsViewModel.PinToStartCommon(new TLDialog24 { With = With });
        }

        public void ProcessScroll()
        {
            // replies
            if (_previousScrollPosition != null && Items.IndexOf(_previousScrollPosition) != -1)
            {
                RaiseScrollTo(new ScrollToEventArgs(_previousScrollPosition));
                _previousScrollPosition = null;
                return;
            }


            // unread separator
            if (!_isFirstSliceLoaded)
            {
                Items.Clear();
                var messages = CacheService.GetHistory(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));
                ProcessMessages(messages);

                const int maxCount = 10;
                for (var i = 0; i < messages.Count && i < maxCount; i++)
                {
                    Items.Add(messages[i]);
                }

                //wait to complete animation for hiding ScrollToBottomButton
                BeginOnUIThread(TimeSpan.FromSeconds(0.35), () =>
                {
                    for (var i = maxCount; i < messages.Count; i++)
                    {
                        Items.Add(messages[i]);
                    }
                    _isFirstSliceLoaded = true;

                    UpdateItemsAsync(0, 0, Constants.FileSliceLength, false);
                });
            }
            else
            {
                RaiseScrollToBottom();
            }
        }

        public void Help()
        {
            _text = "/help";
            Send();
        }

        public void OpenStickerSettings()
        {
            NavigationService.UriFor<StickersViewModel>().Navigate();
        }

        public void Call()
        {
            ShellViewModel.StartVoiceCall(With as TLUser, IoC.Get<IVoIPService>(), IoC.Get<ICacheService>());
        }

        public void OnOpenPhone(object sender, TelegramPhoneEventArgs e)
        {
            if (e == null) return;
            if (string.IsNullOrEmpty(e.Phone)) return;

            var phone = e.Phone.StartsWith("+") ? e.Phone : "+" + e.Phone;

            var task = new PhoneCallTask
            {
                DisplayName = "",
                PhoneNumber = phone
            };
            task.Show();
        }
    }

    public class ScrollToEventArgs : System.EventArgs
    {
        public TLUserBase User { get; set; }

        public TLMessageBase Message { get; set; }

        public TLDecryptedMessageBase DecryptedMessage { get; set; }

        public ScrollToEventArgs(TLUserBase user)
        {
            User = user;
        }

        public ScrollToEventArgs(TLMessageBase message)
        {
            Message = message;
        }

        public ScrollToEventArgs(TLDecryptedMessageBase message)
        {
            DecryptedMessage = message;
        }
    }
}
