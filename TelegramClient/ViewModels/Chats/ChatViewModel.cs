// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views;
using TelegramClient.Views.Contacts;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Chats
{
    public class ChatViewModel : ItemsViewModelBase<TLUserBase>,
        Telegram.Api.Aggregator.IHandle<TLMessageBase>,
        Telegram.Api.Aggregator.IHandle<UploadableItem>,
        Telegram.Api.Aggregator.IHandle<TLUpdateNotifySettings>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChannel>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatParticipants>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatAdmins>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatParticipantAdmin>,
        Telegram.Api.Aggregator.IHandle<TLChannel>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserStatus>
    {
        public bool IsUpgradeAvailable { get; set; }

        public bool IsActivateAvailable
        {
            get
            {
                var chat = CurrentItem as TLChat40;
                return chat != null && chat.Creator && chat.Deactivated;
            }
        }

        public string UpgradeDescription
        {
            get { return string.Format(AppResources.UpgradeToSupergroupDescription, 1000); }
        }

        public string MembersSubtitle
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                if (channel != null)
                {
                    var count = channel.ParticipantsCount;
                    if (count != null)
                    {
                        return count.Value == 0
                            ? AppResources.NoUsers.ToLowerInvariant()
                            : Language.Declension(
                                count.Value,
                                AppResources.UserNominativeSingular,
                                AppResources.UserNominativePlural,
                                AppResources.UserGenitiveSingular,
                                AppResources.UserGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                    }
                }

                return null;
            }
        }

        public string AdministratorsSubtitle
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                if (channel != null)
                {
                    var count = channel.AdminsCount;
                    if (count != null)
                    {
                        return count.Value == 0
                            ? AppResources.NoUsers.ToLowerInvariant()
                            : Language.Declension(
                                count.Value,
                                AppResources.UserNominativeSingular,
                                AppResources.UserNominativePlural,
                                AppResources.UserGenitiveSingular,
                                AppResources.UserGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                    }
                }

                return null;
            }
        }

        public string KickedUsersSubtitle
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                if (channel != null)
                {
                    var count = channel.KickedCount;
                    if (count != null)
                    {
                        return count.Value == 0
                            ? AppResources.NoUsers.ToLowerInvariant()
                            : Language.Declension(
                                count.Value,
                                AppResources.UserNominativeSingular,
                                AppResources.UserNominativePlural,
                                AppResources.UserGenitiveSingular,
                                AppResources.UserGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                    }
                }

                return null;
            }
        }

        public string DeleteAndExitGroupString
        {
            get
            {
                if (IsMegaGroup) return AppResources.LeaveGroup;
                if (IsChannel) return AppResources.LeaveChannel;

                return AppResources.DeleteAndExitGroup;
            }
        }

        public bool IsDeleteAndExitVisible
        {
            get
            {
                var channel = _currentItem as TLChannel;
                if (channel != null && !channel.Creator && !channel.Left.Value)
                {
                    return true;
                }

                var broadcast = _currentItem as TLBroadcastChat;
                if (broadcast == null)
                {
                    return true;
                }

                return false;
            }
        }

        private TimerSpan _selectedSpan;

        public TimerSpan SelectedSpan
        {
            get { return _selectedSpan; }
            set
            {
                _selectedSpan = value;

                if (_selectedSpan != null)
                {
                    if (_selectedSpan.Seconds == 0
                        || _selectedSpan.Seconds == int.MaxValue)
                    {
                        MuteUntil = _selectedSpan.Seconds;
                    }
                    else
                    {
                        var now = DateTime.Now;
                        var muteUntil = now.AddSeconds(_selectedSpan.Seconds);

                        MuteUntil = muteUntil < now ? 0 : TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, muteUntil).Value;
                    }
                }
            }
        }

        public IList<TimerSpan> Spans { get; protected set; }

        private TLChatBase _currentItem;

        public TLChatBase CurrentItem
        {
            get { return _currentItem; }
            set { SetField(ref _currentItem, value, () => CurrentItem); }
        }

        public bool IsChannel
        {
            get { return CurrentItem is TLChannel; }
        }

        public bool IsMegaGroup
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return (channel != null && channel.IsMegaGroup);
            }
        }

        public bool CanViewParticipants
        {
            get
            {
                return !IsChannel || IsMegaGroup;
            }
        }

        public bool IsChannelAdministrator
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && (channel.Creator || channel.IsEditor);
            }
        }

        public bool IsChannelParticipantsButtonEnabled
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && !channel.IsMegaGroup && (channel.Creator || channel.IsEditor);
            }
        }

        public bool IsReportButtonEnabled
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && !TLString.IsNullOrEmpty(channel.UserName) && !channel.Creator;
            }
        }

        public bool CanEditChat
        {
            get
            {
                var chat = CurrentItem as TLChat40;

                return chat != null && (chat.Creator || chat.Admin.Value || !chat.AdminsEnabled.Value);
            }
        }

        public bool CanAddChatParticipants
        {
            get
            {
                var chat = CurrentItem as TLChat40;

                return chat != null && (chat.Creator || chat.Admin.Value || !chat.AdminsEnabled.Value);
            }
        }

        public bool CanEditChannel
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && (channel.Creator || (channel.IsMegaGroup && channel.IsEditor));
            }
        }

        public bool CanAddChannelParticipants
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && (channel.Creator || channel.IsDemocracy || (channel.IsMegaGroup && channel.IsEditor));
            }
        }

        public string Link
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                if (channel != null && !TLString.IsNullOrEmpty(channel.UserName))
                {
                    return "t.me/" + channel.UserName;
                }

                return string.Empty;
            }
        }

        private bool _suppressUpdating = true;

        private int _muteUntil;

        public int MuteUntil
        {
            get { return _muteUntil; }
            set { SetField(ref _muteUntil, value, () => MuteUntil); }
        }

        private string _selectedSound;

        public string SelectedSound
        {
            get { return _selectedSound; }
            set { SetField(ref _selectedSound, value, () => SelectedSound); }
        }

        public void SetSelectedSound(string sound)
        {
            _selectedSound = sound;
        }

        public List<string> Sounds { get; protected set; }

        private string _subtitle;

        public string Subtitle
        {
            get { return _subtitle; }
            set { SetField(ref _subtitle, value, () => Subtitle); }
        }

        private string _subtitle2;

        public string Subtitle2
        {
            get { return _subtitle2; }
            set { SetField(ref _subtitle2, value, () => Subtitle2); }
        }

        private string _subtitle3;

        public string Subtitle3
        {
            get { return _subtitle3; }
            set { SetField(ref _subtitle3, value, () => Subtitle3); }
        }

        private readonly IUploadFileManager _uploadManager;

        public ProfilePhotoViewerViewModel ProfilePhotoViewer { get; set; }

        public bool IsViewerOpen
        {
            get { return ProfilePhotoViewer != null && ProfilePhotoViewer.IsOpen; }
        }

        public TLChatBase Chat { get { return _currentItem; } }

        private readonly IStateService _stateService;

        private readonly INavigationService _navigationService;

        private readonly ITelegramEventAggregator _eventAggregator;

        private readonly IMTProtoService _mtProtoService;

        private readonly ICacheService _cacheService;

        public ChatViewModel(
            IUploadFileManager uploadManager, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService,
            INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            //tombstoning
            if (stateService.CurrentChat == null)
            {
                ShellViewModel.Navigate(navigationService);
                return;
            }

            _currentItem = stateService.CurrentChat;
            stateService.CurrentChat = null;

            _stateService = stateService;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;
            _mtProtoService = mtProtoService;
            _cacheService = cacheService;

            _eventAggregator.Subscribe(this);

            Execute.BeginOnThreadPool(() => _cacheService.GetConfigAsync(result =>
            {
                var config = result as TLConfig41;
                if (config != null)
                {
                    var chat41 = Chat as TLChat41;

                    if (chat41 != null && chat41.Creator && !chat41.IsMigrated
#if !DEBUG
                        && chat41.ParticipantsCount.Value >= config.BroadcastSizeMax.Value
#endif
)
                    {
                        IsUpgradeAvailable = true;
                        NotifyOfPropertyChange(() => IsUpgradeAvailable);
                    }
                }
            }));


            Spans = new List<TimerSpan>
            {
                new TimerSpan(AppResources.Enabled, string.Empty, 0, AppResources.Enabled),
                new TimerSpan(AppResources.HourNominativeSingular,  "1", (int)TimeSpan.FromHours(1.0).TotalSeconds, string.Format(AppResources.MuteFor, string.Format("{0} {1}", "1", AppResources.HourNominativeSingular).ToLowerInvariant())),
                new TimerSpan(AppResources.HourGenitivePlural, "8", (int)TimeSpan.FromHours(8.0).TotalSeconds, string.Format(AppResources.MuteFor, string.Format("{0} {1}", "8", AppResources.HourGenitivePlural).ToLowerInvariant())),
                new TimerSpan(AppResources.DayNominativePlural, "2", (int)TimeSpan.FromDays(2.0).TotalSeconds, string.Format(AppResources.MuteFor, string.Format("{0} {1}", "2", AppResources.DayNominativePlural).ToLowerInvariant())),
                new TimerSpan(AppResources.Disabled, string.Empty, int.MaxValue, AppResources.Disabled),
            };
            _selectedSpan = Spans[0];

            _notificationTimer = new DispatcherTimer();
            _notificationTimer.Interval = TimeSpan.FromSeconds(Constants.NotificationTimerInterval);
            _notificationTimer.Tick += OnNotificationTimerTick;

            _uploadManager = uploadManager;
            eventAggregator.Subscribe(this);

            DisplayName = LowercaseConverter.Convert(AppResources.Profile);

            Items = new ObservableCollection<TLUserBase>();


            UpdateNotificationSettings();

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => MuteUntil)
                    && !_suppressUpdating)
                {
                    UpdateNotifySettingsAsync();
                }

                if (Property.NameEquals(args.PropertyName, () => SelectedSound)
                   && !_suppressUpdating)
                {
                    NotificationsViewModel.PlaySound(SelectedSound);

                    UpdateNotifySettingsAsync();
                }
            };
        }

        protected override void OnActivate()
        {
            if (_stateService != null
                && _stateService.Participant != null)
            {
                var participant = _stateService.Participant;
                _stateService.Participant = null;

                var forwardingMessagesCount = _stateService.ForwardingMessagesCount;
                _stateService.ForwardingMessagesCount = 0;

                var broadcastChat = Chat as TLBroadcastChat;
                var channel = Chat as TLChannel;
                if (broadcastChat != null && channel == null)
                {
                    var serviceMessage = new TLMessageService17();
                    serviceMessage.ToId = new TLPeerBroadcast { Id = Chat.Id };
                    serviceMessage.FromId = new TLInt(_stateService.CurrentUserId);
                    serviceMessage.Out = new TLBool(true);
                    serviceMessage.SetUnread(new TLBool(false));
                    serviceMessage.Date = TLUtils.DateToUniversalTimeTLInt(_mtProtoService.ClientTicksDelta, DateTime.Now);
                    serviceMessage.Action = new TLMessageActionChatAddUser41 { Users = new TLVector<TLInt> { participant.Id } };

                    broadcastChat.ParticipantIds.Add(participant.Id);

                    _cacheService.SyncBroadcast(broadcastChat,
                        result =>
                        {
                            _eventAggregator.Publish(serviceMessage);
                        });
                    //ChatDetails.UpdateTitles();
                }
                else
                {
                    if (channel != null)
                    {
                        _mtProtoService.InviteToChannelAsync(channel.ToInputChannel(), new TLVector<TLInputUserBase> { participant.ToInputUser() },
                            statedMessage => Execute.BeginOnUIThread(() =>
                            {
                                var updates = statedMessage as TLUpdates;
                                if (updates != null)
                                {
                                    var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                                    if (updateNewMessage != null)
                                    {
                                        _eventAggregator.Publish(updateNewMessage.Message);
                                    }
                                }

                                UpdateTitles();
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                if (error.TypeEquals(ErrorType.PEER_FLOOD))
                                {
                                    //MessageBox.Show(AppResources.PeerFloodAddContact, AppResources.Error, MessageBoxButton.OK);
                                    ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodAddContact, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                                        result =>
                                        {
                                            if (result == CustomMessageBoxResult.RightButton)
                                            {
                                                TelegramViewBase.NavigateToUsername(_mtProtoService, Constants.SpambotUsername, null, null, null);
                                            }
                                        });
                                }
                                else if (error.TypeEquals(ErrorType.USERS_TOO_MUCH))
                                {
                                    MessageBox.Show(AppResources.UsersTooMuch, AppResources.Error, MessageBoxButton.OK);
                                }
                                else if (error.TypeEquals(ErrorType.USER_CHANNELS_TOO_MUCH))
                                {
                                    MessageBox.Show(AppResources.UserChannelsTooMuch, AppResources.Error, MessageBoxButton.OK);
                                }
                                else if (error.TypeEquals(ErrorType.BOTS_TOO_MUCH))
                                {
                                    MessageBox.Show(AppResources.BotsTooMuch, AppResources.Error, MessageBoxButton.OK);
                                }
                                else if (error.TypeEquals(ErrorType.USER_NOT_MUTUAL_CONTACT))
                                {
                                    MessageBox.Show(AppResources.UserNotMutualContact, AppResources.Error, MessageBoxButton.OK);
                                }

                                Execute.ShowDebugMessage("channels.inviteToChannel error " + error);
                            }));

                        return;
                    }

                    _mtProtoService.AddChatUserAsync(Chat.Id, participant.ToInputUser(), new TLInt(forwardingMessagesCount),
                        statedMessage =>
                        {
                            var updates = statedMessage as TLUpdates;
                            if (updates != null)
                            {
                                var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                                if (updateNewMessage != null)
                                {
                                    _eventAggregator.Publish(updateNewMessage.Message);
                                }
                            }

                            UpdateTitles();
                        },
                        error => Execute.BeginOnUIThread(() =>
                        {
                            if (error.TypeEquals(ErrorType.PEER_FLOOD))
                            {
                                //MessageBox.Show(AppResources.PeerFloodAddContact, AppResources.Error, MessageBoxButton.OK);

                                ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodAddContact, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                                    result =>
                                    {
                                        if (result == CustomMessageBoxResult.RightButton)
                                        {
                                            TelegramViewBase.NavigateToUsername(_mtProtoService, Constants.SpambotUsername, null, null, null);
                                        }
                                    });
                            }

                            Execute.ShowDebugMessage("messages.addChatUser error " + error);
                        }));
                }
            }

            if (_stateService != null
                && _stateService.SelectedTimerSpan != null)
            {
                SelectedSpan = _stateService.SelectedTimerSpan;
                _stateService.SelectedTimerSpan = null;
            }

            if (_stateService != null && _stateService.UpdateChannelAdministrators)
            {
                var channel = CurrentItem as TLChannel;
                if (channel != null && channel.IsMegaGroup)
                {
                    UpdateChannelAdministrators(channel);
                    UpdateSubtitles();
                    return;
                }
                _stateService.UpdateChannelAdministrators = false;
            }

            StartTimer();

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            StopTimer();

            base.OnDeactivate(close);
        }

        public void Edit()
        {
            _stateService.CurrentChat = Chat;
            _navigationService.UriFor<EditChatViewModel>().Navigate();
        }

        public void SetAdmins()
        {
            _stateService.CurrentChat = Chat;
            _navigationService.UriFor<AddAdminsViewModel>().Navigate();
        }

        public void ConvertToSupergroup()
        {
            _stateService.CurrentChat = Chat;
            _navigationService.UriFor<ConvertToSupergroupViewModel>().Navigate();
        }

        public void CopyLink()
        {
            if (string.IsNullOrEmpty(Link)) return;

            Clipboard.SetText("https://" + Link);
        }

        private void UpdateUsers(List<TLUserBase> users, System.Action callback)
        {
            const int firstSliceCount = 3;
            var secondSlice = new List<TLUserBase>();
            for (var i = 0; i < users.Count; i++)
            {
                if (i < firstSliceCount)
                {
                    //users[i].IsAdmin = false;
                    Items.Add(users[i]);
                }
                else
                {
                    secondSlice.Add(users[i]);
                }
            }

            Execute.BeginOnUIThread(() =>
            {
                foreach (var user in secondSlice)
                {
                    //user.IsAdmin = false;
                    Items.Add(user);
                }
                callback.SafeInvoke();
            });
        }

        private void UpdateNotifySettingsAsync()
        {
            if (CurrentItem == null) return;

            var notifySettings = new TLInputPeerNotifySettings78
            {
                Flags = new TLInt(0),
                MuteUntil = new TLInt(MuteUntil),
                ShowPreviews = TLBool.True,
                Sound = string.IsNullOrEmpty(SelectedSound) ? new TLString("default") : new TLString(SelectedSound)
            };

            IsWorking = true;
            MTProtoService.UpdateNotifySettingsAsync(
                CurrentItem.ToInputNotifyPeer(), notifySettings,
                result =>
                {
                    IsWorking = false;
                    CurrentItem.NotifySettings = new TLPeerNotifySettings78
                    {
                        Flags = new TLInt(0),
                        MuteUntil = new TLInt(MuteUntil),
                        ShowPreviews = notifySettings.ShowPreviews,
                        Sound = notifySettings.Sound
                    };

                    var channel = CurrentItem as TLChannel;
                    var peer = channel != null
                        ? (TLPeerBase)new TLPeerChannel { Id = CurrentItem.Id }
                        : new TLPeerChat { Id = CurrentItem.Id };
                    var dialog = CacheService.GetDialog(peer);

                    if (dialog != null)
                    {
                        dialog.NotifySettings = CurrentItem.NotifySettings;
                        dialog.NotifyOfPropertyChange(() => dialog.NotifySettings);
                        dialog.NotifyOfPropertyChange(() => dialog.Self);
                        var settings = dialog.With as INotifySettings;
                        if (settings != null)
                        {
                            settings.NotifySettings = CurrentItem.NotifySettings;
                        }
                    }

                    CacheService.Commit();
                },
                error =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("account.updateNotifySettings error: " + error);
                });
        }

        public void UpdateChannelAdministrators(TLChannel channel)
        {
            StateService.UpdateSubtitle = true;

            IsWorking = true;
            MTProtoService.GetParticipantsAsync(channel.ToInputChannel(), new TLChannelParticipantsAdmins(), new TLInt(0), new TLInt(200), new TLInt(0),
                result =>
                {
                    var channelParticipants = result as TLChannelParticipants;
                    if (channelParticipants != null)
                    {
                        var admins = new Dictionary<int, int>();

                        foreach (var participant in channelParticipants.Participants)
                        {
                            var creator = participant as TLChannelParticipantCreator;
                            if (creator != null)
                            {
                                admins[creator.UserId.Value] = creator.UserId.Value;
                            }

                            var editor = participant as TLChannelParticipantAdmin;
                            if (editor != null)
                            {
                                admins[editor.UserId.Value] = editor.UserId.Value;
                            }
                        }

                        Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            foreach (var user in Items)
                            {
                                user.IsAdmin = admins.ContainsKey(user.Index);
                                user.NotifyOfPropertyChange(() => user.IsAdmin);
                            }

                            var usersCache = new Dictionary<int, int>();
                            foreach (var item in Items)
                            {
                                usersCache[item.Index] = item.Index;
                            }

                            foreach (var admin in channelParticipants.Users)
                            {
                                if (!usersCache.ContainsKey(admin.Index))
                                {
                                    admin.IsAdmin = true;

                                    InsertInDescOrder(Items, admin);
                                }
                            }

                            Status = Items.Count > 0 ? string.Empty : AppResources.NoUsersHere;
                        });
                    }
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Status = string.Empty;

                    Execute.ShowDebugMessage("channels.getParticipants error " + error);
                }));
        }

        private readonly object _participantsSyncRoot = new object();

        public void UpdateChannelItems(TLChannel channel)
        {
            IsWorking = true;
            MTProtoService.GetParticipantsAsync(channel.ToInputChannel(), new TLChannelParticipantsRecent(), new TLInt(0), new TLInt(200), new TLInt(0),
                result =>
                {
                    var channelParticipants = result as TLChannelParticipants;
                    if (channelParticipants != null)
                    {
                        channel.ChannelParticipants = channelParticipants;

                        var admins = new Dictionary<int, int>();

                        foreach (var participant in channelParticipants.Participants)
                        {
                            var creator = participant as TLChannelParticipantCreator;
                            if (creator != null)
                            {
                                admins[creator.UserId.Value] = creator.UserId.Value;
                            }

                            var editor = participant as TLChannelParticipantAdmin;
                            if (editor != null)
                            {
                                admins[editor.UserId.Value] = editor.UserId.Value;
                            }
                        }

                        var users = channelParticipants.Users.OrderByDescending(x => x.StatusValue).ToList();

                        Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            Items.Clear();
                            foreach (var user in users)
                            {
                                user.IsAdmin = admins.ContainsKey(user.Index);
                                Items.Add(user);
                            }
                            Status = Items.Count > 0 ? string.Empty : AppResources.NoUsersHere;

                            UpdateNotificationSettings();
                            UpdateTitles();
                        });
                    }
                },
                error =>
                {
                    if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST)
                        && TLRPCError.TypeEquals(error, ErrorType.CHAT_ADMIN_REQUIRED))
                    {
                        channel.ChannelParticipants = null;
                        FileUtils.Delete(_participantsSyncRoot, channel.ChannelParticipantsFileName);
                    }

                    Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Status = string.Empty;

                        Execute.ShowDebugMessage("channels.getParticipants error " + error);
                    });
                });
        }

        private void UpdateItems()
        {
            var channel = CurrentItem as TLChannel;
            if (channel != null && channel.IsMegaGroup)
            {
                UpdateChannelItems(channel);

                return;
            }

            if (CurrentItem is TLBroadcastChat)
            {
                return;
            }

            IsWorking = true;
            MTProtoService.GetFullChatAsync(CurrentItem.Id,
                chatFull =>
                {
                    IsWorking = false;

                    var newUsersCache = new Dictionary<int, TLUserBase>();
                    foreach (var user in chatFull.Users)
                    {
                        newUsersCache[user.Index] = user;
                    }

                    var participants = chatFull.FullChat.Participants as IChatParticipants;
                    if (participants != null)
                    {
                        var usersCache = Items.ToDictionary(x => x.Index);

                        var onlineUsers = 0;

                        var chat40 = CurrentItem as TLChat40;
                        var adminsEnabled = chat40 != null && chat40.AdminsEnabled.Value;
                        foreach (var participant in participants.Participants)
                        {
                            var user = newUsersCache[participant.UserId.Value];

                            user.IsAdmin = IsChatAdmin(participant, adminsEnabled);

                            if (user.Status is TLUserStatusOnline)
                            {
                                onlineUsers++;
                            }

                            if (!usersCache.ContainsKey(user.Index))
                            {
                                BeginOnUIThread(() => InsertInDescOrder(Items, user));
                            }
                        }
                        CurrentItem.UsersOnline = onlineUsers;
                    }

                    var chatFull28 = chatFull.FullChat as TLChatFull28;
                    if (chatFull28 != null)
                    {
                        CurrentItem.ExportedInvite = chatFull28.ExportedInvite;
                    }

                    UpdateNotificationSettings();
                    UpdateTitles();
                },
                error =>
                {
                    IsWorking = false;
                });
        }

        protected override void OnInitialize()
        {
            if (Chat == null) return;

            UpdateNotificationSettings();
            UpdateSubtitles();

            var notifySettings = Chat.NotifySettings as TLPeerNotifySettings;
            if (notifySettings != null)
            {
                var sound = notifySettings.Sound == null ? null : _stateService.Sounds.FirstOrDefault(x => string.Equals(x, notifySettings.Sound.Value, StringComparison.OrdinalIgnoreCase));
                SetSelectedSound(sound ?? _stateService.Sounds[0]);
            }
            NotifyOfPropertyChange(() => Chat);

            base.OnInitialize();
        }

        private void InsertInDescOrder(IList<TLUserBase> users, TLUserBase user)
        {
            var added = false;
            for (var i = 0; i < users.Count; i++)
            {
                if (users[i].StatusValue <= user.StatusValue)
                {
                    users.Insert(i, user);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                users.Add(user);
            }
        }

        private string GetChatSubtitle3()
        {
            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                if (!TLString.IsNullOrEmpty(channel.About))
                {
                    return channel.About.ToString();
                }
            }

            return string.Empty;
        }

        private string GetChatSubtitle2()
        {
            if (IsChannel)
            {
                return string.Empty;
            }

            var usersCount = Items.Count;
            var onlineUsersCount = Items.Count(x => x.Status is TLUserStatusOnline);

            var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
            var isCurrentUserOnline = currentUser != null && currentUser.Status is TLUserStatusOnline;
            if (usersCount == 1 || (onlineUsersCount == 1 && isCurrentUserOnline))
            {
                onlineUsersCount = 0;
            }

            return onlineUsersCount > 0 ? string.Format("{0} {1}", onlineUsersCount, AppResources.Online.ToLowerInvariant()) : string.Empty;
        }

        private string GetChatSubtitle()
        {
            //if (IsChannel)
            //{
            //    return "channel";
            //}

            var chat = CurrentItem as TLChat;
            if (chat != null)
            {
                var participantsCount = chat.ParticipantsCount.Value;

                return Language.Declension(
                    participantsCount,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }

            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                if (channel.ParticipantsCount != null)
                {
                    var participantsCount = channel.ParticipantsCount.Value;

                    return Language.Declension(
                        participantsCount,
                        AppResources.CompanyNominativeSingular,
                        AppResources.CompanyNominativePlural,
                        AppResources.CompanyGenitiveSingular,
                        AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                }

                if (channel.ChannelParticipants != null)
                {
                    var participantsCount = channel.ChannelParticipants.Count.Value;

                    return Language.Declension(
                        participantsCount,
                        AppResources.CompanyNominativeSingular,
                        AppResources.CompanyNominativePlural,
                        AppResources.CompanyGenitiveSingular,
                        AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                }

                if (channel.IsMegaGroup)
                {
                    return AppResources.Loading.ToLowerInvariant();
                }

                return channel.IsPublic
                    ? AppResources.PublicChannel.ToLowerInvariant()
                    : AppResources.PrivateChannel.ToLowerInvariant();
            }

            var broadcastChat = CurrentItem as TLBroadcastChat;
            if (broadcastChat != null)
            {
                var participantsCount = broadcastChat.ParticipantIds.Count;

                return Language.Declension(
                    participantsCount,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }



            return string.Empty;
        }

        public void UpdateNotificationSettings()
        {
            var chat = CurrentItem;
            if (chat != null && chat.NotifySettings != null)
            {
                var notifySettings = chat.NotifySettings as TLPeerNotifySettings;
                if (notifySettings != null)
                {
                    _suppressUpdating = true;

                    MuteUntil = notifySettings.MuteUntil != null ? notifySettings.MuteUntil.Value : (StateService.GetNotifySettings().GroupAlert ? 0 : int.MaxValue);
                    var sound = notifySettings.Sound == null ? null : StateService.Sounds.FirstOrDefault(x => string.Equals(x, notifySettings.Sound.Value, StringComparison.OrdinalIgnoreCase));
                    SelectedSound = sound ?? StateService.Sounds[0];

                    _suppressUpdating = false;
                }
            }
        }

        #region Notification timer
        private readonly DispatcherTimer _notificationTimer;

        public void StartTimer()
        {
            _notificationTimer.Start();
        }

        public void StopTimer()
        {
            _notificationTimer.Stop();
        }

        private void OnNotificationTimerTick(object sender, System.EventArgs e)
        {
            if (MuteUntil > 0 && MuteUntil < int.MaxValue)
            {
                NotifyOfPropertyChange(() => MuteUntil);
            }
        }
        #endregion

        #region Actions

        public void SelectSpan(TimerSpan selectedSpan)
        {
            SelectedSpan = selectedSpan;
        }

        public void SelectNotificationSpan()
        {
            //StateService.SelectedTimerSpan = SelectedSpan;
            NavigationService.UriFor<ChooseNotificationSpanViewModel>().Navigate();
        }

        public void OpenLink()
        {
            StateService.ShareLink = "https://" + Link;
            StateService.ShareMessage = "https://" + Link;
            StateService.ShareCaption = AppResources.Share;
            NavigationService.UriFor<ShareViewModel>().Navigate();
        }

        public void AddParticipant()
        {
            if (CurrentItem.IsForbidden) return;

            var chat = CurrentItem as TLChat40;
            var channel = CurrentItem as TLChannel;

            StateService.IsInviteVisible = (chat != null && chat.Creator) || (channel != null && channel.IsMegaGroup && channel.Creator);
            StateService.CurrentChat = CurrentItem;
            StateService.RemovedUsers = Items;
            StateService.RequestForwardingCount = CurrentItem is TLChat;
            NavigationService.UriFor<AddChatParticipantViewModel>().Navigate();
        }

        public void DeleteParticipant(TLUserBase user)
        {
            if (CurrentItem.IsForbidden) return;
            if (user == null) return;

            var broadcast = CurrentItem as TLBroadcastChat;
            var channel = CurrentItem as TLChannel;
            if (broadcast != null && channel == null)
            {
                var broadcastChat = (TLBroadcastChat)CurrentItem;

                var serviceMessage = new TLMessageService17
                {
                    ToId = new TLPeerBroadcast { Id = broadcastChat.Id },
                    FromId = new TLInt(StateService.CurrentUserId),
                    Out = new TLBool(true),
                    Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                    Action = new TLMessageActionChatDeleteUser { UserId = user.Id }
                };
                serviceMessage.SetUnread(new TLBool(false));

                for (var i = 0; i < broadcastChat.ParticipantIds.Count; i++)
                {
                    if (user.Id.Value == broadcastChat.ParticipantIds[i].Value)
                    {
                        broadcastChat.ParticipantIds.RemoveAt(i);
                        break;
                    }
                }

                broadcastChat.ParticipantIds.Remove(user.Id);

                CacheService.SyncBroadcast(broadcastChat,
                    result =>
                    {
                        EventAggregator.Publish(serviceMessage);
                        UpdateTitles();
                    });
            }
            else
            {
                if (user.Index == StateService.CurrentUserId)
                {
                    DeleteAndExitGroup();
                    return;
                }

                if (channel != null)
                {
                    IsWorking = true;
                    MTProtoService.KickFromChannelAsync(channel, user.ToInputUser(), TLBool.True,
                        updatesBase =>
                        {
                            IsWorking = false;
                            BeginOnUIThread(() => Items.Remove(user));

                            var updates = updatesBase as TLUpdates;
                            if (updates != null)
                            {
                                var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                                if (updateNewMessage != null)
                                {
                                    EventAggregator.Publish(updateNewMessage.Message);
                                }
                            }
                            UpdateTitles();
                        },
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.deleteChatUser error " + error);
                            IsWorking = false;
                        });
                    return;
                }

                IsWorking = true;
                MTProtoService.DeleteChatUserAsync(CurrentItem.Id, user.ToInputUser(),
                    statedMessage =>
                    {
                        IsWorking = false;
                        BeginOnUIThread(() => Items.Remove(user));

                        var updates = statedMessage as TLUpdates;
                        if (updates != null)
                        {
                            var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                            if (updateNewMessage != null)
                            {
                                EventAggregator.Publish(updateNewMessage.Message);
                            }
                        }
                        UpdateTitles();
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.deleteChatUser error " + error);
                        IsWorking = false;
                    });
            }
        }

        public void MessageUser(TLUserBase user)
        {
            if (user == null) return;

            StateService.With = user;
            StateService.RemoveBackEntries = true;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        public void ViewUser(TLUserBase user)
        {
            if (user == null) return;

            StateService.CurrentContact = user;
            NavigationService.UriFor<ContactViewModel>().Navigate();
        }

        private TLPhotoBase GetPhoto()
        {
            var chat = CurrentItem as TLChat;
            if (chat != null) return chat.Photo;

            var channel = CurrentItem as TLChannel;
            if (channel != null) return channel.Photo;

            return null;
        }

        public void OpenPhoto()
        {
            var chat = CurrentItem as TLChat;
            var channel = CurrentItem as TLChannel;
            if (chat == null && channel == null) return;

            var photoBase = GetPhoto();

            var photo = photoBase as TLChatPhoto;
            if (photo != null)
            {
                StateService.CurrentPhoto = photo;
                StateService.CurrentChat = CurrentItem;

                if (ProfilePhotoViewer == null)
                {
                    ProfilePhotoViewer = new ProfilePhotoViewerViewModel(StateService, MTProtoService);
                    NotifyOfPropertyChange(() => ProfilePhotoViewer);
                }

                BeginOnUIThread(() => ProfilePhotoViewer.OpenViewer());
                return;
            }

            var photoEmpty = photoBase as TLChatPhotoEmpty;
            if (photoEmpty != null)
            {
                if ((chat != null && !chat.Left.Value)
                    || (channel != null && channel.Creator))
                {
                    EditChatActions.EditPhoto(result =>
                    {
                        var fileId = TLLong.Random();
                        IsWorking = true;
                        _uploadManager.UploadFile(fileId, CurrentItem, result);
                    });
                }
            }
        }

        public void SetPhoto()
        {
            var chat = CurrentItem as TLChat;
            var channel = CurrentItem as TLChannel;
            if (chat == null && channel == null) return;

            if ((chat != null && !chat.Left.Value)
                || (channel != null && channel.Creator))
            {
                EditChatActions.EditPhoto(result =>
                {
                    var fileId = TLLong.Random();
                    IsWorking = true;
                    _uploadManager.UploadFile(fileId, CurrentItem, result);
                });
            }
        }

        public void OpenMedia()
        {
            if (CurrentItem == null) return;

            StateService.CurrentInputPeer = CurrentItem;
            NavigationService.UriFor<FullMediaViewModel>().Navigate();
        }

        public void Report()
        {
            Report(CurrentItem.ToInputPeer());
        }

        public void OpenMembers()
        {
            StateService.CurrentChat = CurrentItem;
            NavigationService.UriFor<ChannelMembersViewModel>().Navigate();
        }

        public void OpenAdministrators()
        {
            StateService.CurrentChat = CurrentItem;
            NavigationService.UriFor<ChannelAdministratorsViewModel>().Navigate();
        }

        public void OpenKickedUsers()
        {
            StateService.CurrentChat = CurrentItem;
            NavigationService.UriFor<ChannelBlockedContactsViewModel>().Navigate();
        }

        public void UpgradeGroup()
        {
            var confirmation = MessageBox.Show(AppResources.UpgradeToSupergroupConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            IsWorking = true;
            MTProtoService.MigrateChatAsync(
                CurrentItem.Id,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var updates = result as TLUpdates;
                    if (updates != null)
                    {
                        var channel = updates.Chats.FirstOrDefault(x => x is TLChannel) as TLChannel;
                        if (channel != null)
                        {
                            var migratedFromMaxId = new TLInt(0);
                            var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                            if (updateNewMessage != null)
                            {
                                migratedFromMaxId = updateNewMessage.Message.Id;
                            }
                            channel.MigratedFromChatId = CurrentItem.Id;
                            channel.MigratedFromMaxId = migratedFromMaxId;

                            var addedChannel = CacheService.GetDialog(new TLPeerChannel { Id = channel.Id });
                            if (addedChannel != null)
                            {
                                EventAggregator.Publish(new DialogAddedEventArgs(addedChannel));
                            }
                            var removedChat = CacheService.GetDialog(new TLPeerChat { Id = CurrentItem.Id });
                            if (removedChat != null)
                            {
                                EventAggregator.Publish(new DialogRemovedEventArgs(removedChat));
                            }

                            StateService.With = channel;
                            StateService.RemoveBackEntries = true;
                            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
                        }
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.migrateChat error " + error);
                }));

            return;
        }

        public void DeleteAndExitGroup()
        {
            MessageBoxResult confirmation;

            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                confirmation = IsMegaGroup
                    ? MessageBox.Show(AppResources.LeaveGroupConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel)
                    : MessageBox.Show(AppResources.LeaveChannelConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
                if (confirmation != MessageBoxResult.OK) return;

                IsWorking = true;
                MTProtoService.LeaveChannelAsync(
                    channel,
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        NavigationService.RemoveBackEntry();

                        var dialog = CacheService.GetDialog(new TLPeerChannel { Id = CurrentItem.Id });
                        if (dialog != null)
                        {
                            CacheService.DeleteDialog(dialog);
                            DialogsViewModel.UnpinFromStart(dialog);
                            EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                        }

                        NavigationService.GoBack();
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        if (error.TypeEquals(ErrorType.CHANNEL_PRIVATE)
                            || error.TypeEquals(ErrorType.USER_NOT_PARTICIPANT))
                        {
                            NavigationService.RemoveBackEntry();

                            var dialog = CacheService.GetDialog(new TLPeerChannel { Id = CurrentItem.Id });
                            if (dialog != null)
                            {
                                CacheService.DeleteDialog(dialog);
                                DialogsViewModel.UnpinFromStart(dialog);
                                EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                            }

                            NavigationService.GoBack();
                        }
                        Execute.ShowDebugMessage("cnannels.leaveChannel error " + error);
                    }));

                return;
            }

            confirmation = MessageBox.Show(AppResources.DeleteAndExitConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            DialogsViewModel.DeleteAndExitDialogCommon(
                CurrentItem,
                MTProtoService,
                () => BeginOnUIThread(() =>
                {
                    NavigationService.RemoveBackEntry();

                    var dialog = CacheService.GetDialog(new TLPeerChat { Id = CurrentItem.Id });
                    if (dialog != null)
                    {
                        CacheService.DeleteDialog(dialog);
                        DialogsViewModel.UnpinFromStart(dialog);
                        EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                    }
                    NavigationService.GoBack();
                }),
                error =>
                {
                    Execute.ShowDebugMessage("DeleteAndExitGroupCommon error " + error);
                });
        }

        public void UpdateTitles()
        {
            NotifyOfPropertyChange(() => MembersSubtitle);
            NotifyOfPropertyChange(() => AdministratorsSubtitle);
            NotifyOfPropertyChange(() => KickedUsersSubtitle);
        }

        public void UpdateSubtitles()
        {
            Subtitle = GetChatSubtitle();
            Subtitle2 = GetChatSubtitle2();
            Subtitle3 = GetChatSubtitle3();
        }
        #endregion

        public void Handle(TLMessageBase message)
        {
            var serviceMessage = message as TLMessageService;
            if (serviceMessage != null)
            {
                var editTitleAction = serviceMessage.Action as TLMessageActionChatEditTitle;
                if (editTitleAction != null && serviceMessage.ToId.Id.Value == Chat.Index)
                {
                    NotifyOfPropertyChange(() => Chat);
                }

                var channel = CurrentItem as TLChannel;
                if (channel != null && channel.Index == serviceMessage.ToId.Id.Value)
                {
                    var chatDeletePhotoAction = serviceMessage.Action as TLMessageActionChatDeletePhoto;
                    if (chatDeletePhotoAction != null)
                    {
                        channel.NotifyOfPropertyChange(() => channel.Photo);
                        return;
                    }

                    var chatDeleteUserAction = serviceMessage.Action as TLMessageActionChatDeleteUser;
                    if (chatDeleteUserAction != null)
                    {
                        HandleDeleteUserAction(chatDeleteUserAction);
                        return;
                    }

                    var chatAddUserAction = serviceMessage.Action as TLMessageActionChatAddUser41;
                    if (chatAddUserAction != null)
                    {
                        HandleAddUserAction(chatAddUserAction);
                        return;
                    }
                }

                var chat = CurrentItem as TLChat;
                if (chat != null && chat.Index == serviceMessage.ToId.Id.Value)
                {
                    var chatDeletePhotoAction = serviceMessage.Action as TLMessageActionChatDeletePhoto;
                    if (chatDeletePhotoAction != null)
                    {
                        chat.NotifyOfPropertyChange(() => chat.Photo);
                        return;
                    }

                    var chatDeleteUserAction = serviceMessage.Action as TLMessageActionChatDeleteUser;
                    if (chatDeleteUserAction != null)
                    {
                        HandleDeleteUserAction(chatDeleteUserAction);
                        return;
                    }

                    var chatAddUserAction = serviceMessage.Action as TLMessageActionChatAddUser41;
                    if (chatAddUserAction != null)
                    {
                        HandleAddUserAction(chatAddUserAction);
                        return;
                    }
                }


                var broadcastChat = CurrentItem as TLBroadcastChat;
                if (broadcastChat != null && broadcastChat.Index == serviceMessage.ToId.Id.Value)
                {
                    var chatDeleteUserAction = serviceMessage.Action as TLMessageActionChatDeleteUser;
                    if (chatDeleteUserAction != null)
                    {
                        HandleDeleteUserAction(chatDeleteUserAction);
                        return;
                    }

                    var chatAddUserAction = serviceMessage.Action as TLMessageActionChatAddUser41;
                    if (chatAddUserAction != null)
                    {
                        HandleAddUserAction(chatAddUserAction);
                        return;
                    }
                }
            }
        }

        private void HandleAddUserAction(TLMessageActionChatAddUser41 chatAddUserAction)
        {
            var cachedUsers = new List<TLUserBase>();
            foreach (var userId in chatAddUserAction.Users)
            {
                var cachedUser = CacheService.GetUser(userId);
                if (cachedUser != null)
                {
                    cachedUsers.Add(cachedUser);
                }
            }

            if (cachedUsers.Count > 0)
            {
                BeginOnUIThread(() =>
                {
                    foreach (var cachedUser in cachedUsers)
                    {
                        InsertInDescOrder(Items, cachedUser);
                    }
                    UpdateTitles();
                });
            }
        }

        private void HandleDeleteUserAction(TLMessageActionChatDeleteUser chatDeleteUserAction)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Index == chatDeleteUserAction.UserId.Value)
                {
                    BeginOnUIThread(() =>
                    {
                        Items.RemoveAt(i);
                        UpdateTitles();
                    });
                    break;
                }
            }
        }

        public void Handle(UploadableItem item)
        {
            if (item.Owner == CurrentItem)
            {
                IsWorking = false;
            }
        }

        public void Handle(TLUpdateNotifySettings updateNotifySettings)
        {
            var notifyPeer = updateNotifySettings.Peer as TLNotifyPeer;
            if (notifyPeer != null)
            {
                var peer = notifyPeer.Peer;
                if (peer is TLPeerChat
                    && peer.Id.Value == CurrentItem.Index)
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        CurrentItem.NotifySettings = updateNotifySettings.NotifySettings;
                        var notifySettings = updateNotifySettings.NotifySettings as TLPeerNotifySettings;
                        if (notifySettings != null)
                        {
                            _suppressUpdating = true;
                            MuteUntil = notifySettings.MuteUntil != null ? notifySettings.MuteUntil.Value : (StateService.GetNotifySettings().GroupAlert ? 0 : int.MaxValue);
                            _suppressUpdating = false;
                        }
                    });
                }
            }
        }

        public void ForwardInAnimationComplete()
        {
            Items.Clear();
            LazyItems.Clear();

            var channel = CurrentItem as TLChannel;
            if (channel != null && channel.IsMegaGroup)
            {
                var participants = channel.ChannelParticipants;
                if (participants != null)
                {
                    var admins = new Dictionary<int, int>();

                    foreach (var participant in participants.Participants)
                    {
                        var creator = participant as TLChannelParticipantCreator;
                        if (creator != null)
                        {
                            admins[creator.UserId.Value] = creator.UserId.Value;
                        }

                        var editor = participant as TLChannelParticipantAdmin;
                        if (editor != null)
                        {
                            admins[editor.UserId.Value] = editor.UserId.Value;
                        }
                    }

                    var users = new List<TLUserBase>();
                    foreach (var user in participants.Users)
                    {
                        user.IsAdmin = admins.ContainsKey(user.Index);
                        users.Add(user);
                    }
                    users = users.OrderByDescending(x => x.StatusValue).ToList();

                    UpdateUsers(users, UpdateItems);

                    return;
                }
                else
                {
                    UpdateItems();
                }

                return;
            }

            var chat = CurrentItem;
            if (chat != null)
            {
                var participants = chat.Participants as TLChatParticipants40;
                if (participants != null)
                {
                    var users = new List<TLUserBase>(participants.Participants.Count);
                    var chat40 = CurrentItem as TLChat40;
                    var adminsEnabled = chat40 != null && chat40.AdminsEnabled.Value;
                    foreach (var participant in participants.Participants)
                    {
                        var user = CacheService.GetUser(participant.UserId);
                        if (user != null)
                        {
                            user.IsAdmin = IsChatAdmin(participant, adminsEnabled);

                            var canDeleteUserFromChat = false;

                            var inviter = participant as IInviter;
                            if (inviter != null
                                && inviter.InviterId.Value == StateService.CurrentUserId)
                            {
                                canDeleteUserFromChat = true;
                            }

                            var creator = participant as TLChatParticipantCreator;
                            if (creator != null
                                && creator.UserId.Value == StateService.CurrentUserId)
                            {
                                canDeleteUserFromChat = true;
                            }

                            if (participant.UserId.Value == StateService.CurrentUserId)
                            {
                                canDeleteUserFromChat = true;
                            }

                            user.DeleteActionVisibility = canDeleteUserFromChat
                                ? Visibility.Visible
                                : Visibility.Collapsed;

                            users.Add(user);
                        }
                    }
                    users = users.OrderByDescending(x => x.StatusValue).ToList();

                    UpdateUsers(users, UpdateItems);

                    return;
                }
                else
                {
                    UpdateItems();
                }
            }

            var broadcastChat = CurrentItem as TLBroadcastChat;
            if (broadcastChat != null && channel == null)
            {
                var users = new List<TLUserBase>(broadcastChat.ParticipantIds.Count);
                var count = 0;
                foreach (var participantId in broadcastChat.ParticipantIds)
                {
                    var user = CacheService.GetUser(participantId);
                    if (user != null)
                    {
                        user.DeleteActionVisibility = Visibility.Visible;
                        if (count < 4)
                        {
                            user.IsAdmin = false;
                            Items.Add(user);
                            count++;
                        }
                        else
                        {
                            users.Add(user);
                        }
                    }
                }
                users = users.OrderByDescending(x => x.StatusValue).ToList();

                UpdateUsers(users, UpdateItems);

                return;
            }
        }

        public void Handle(TLUpdateChannel updateChannel)
        {
            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                if (channel.Id.Value == updateChannel.ChannelId.Value)
                {
                    NotifyOfPropertyChange(() => IsChannelAdministrator);
                }
            }
        }

        public void Handle(TLUpdateChatParticipants updateChatParticipants)
        {
            if (updateChatParticipants.Participants.ChatId.Value == CurrentItem.Index)
            {
                var chat40 = CurrentItem as TLChat40;
                var adminsEnabled = chat40 != null && chat40.AdminsEnabled.Value;

                var adminsCache = GetAdminsCache(updateChatParticipants.Participants, adminsEnabled);

                Execute.BeginOnUIThread(() =>
                {
                    foreach (var item in Items)
                    {
                        item.IsAdmin = adminsCache.ContainsKey(item.Index);
                        item.NotifyOfPropertyChange(() => item.IsAdmin);
                    }

                    NotifyOfPropertyChange(() => CanEditChat);
                });
            }
        }

        public void Handle(TLUpdateChatAdmins updateChatAdmins)
        {
            if (updateChatAdmins.ChatId.Value == CurrentItem.Index)
            {
                var adminsCache = GetAdminsCache(CurrentItem.Participants, updateChatAdmins.Enabled.Value);

                Execute.BeginOnUIThread(() =>
                {
                    foreach (var item in Items)
                    {
                        item.IsAdmin = adminsCache.ContainsKey(item.Index);
                        item.NotifyOfPropertyChange(() => item.IsAdmin);
                    }

                    NotifyOfPropertyChange(() => CanEditChat);
                });
            }
        }

        private Dictionary<int, int> GetAdminsCache(TLChatParticipantsBase participants, bool adminsEnabled)
        {
            var adminsCache = new Dictionary<int, int>();
            var chatParticipants = participants as TLChatParticipants40;
            if (chatParticipants != null)
            {
                foreach (var participant in chatParticipants.Participants)
                {
                    if (IsChatAdmin(participant, adminsEnabled))
                    {
                        adminsCache[participant.UserId.Value] = participant.UserId.Value;
                    }
                }
            }
            return adminsCache;
        }

        private static bool IsChatAdmin(TLChatParticipantBase participant, bool adminsEnabled)
        {
            if (adminsEnabled)
            {
                if (participant is TLChatParticipantAdmin || participant is TLChatParticipantCreator)
                {
                    return true;
                }
            }
            else
            {
                if (participant is TLChatParticipantCreator)
                {
                    return true;
                }
            }

            return false;
        }

        public void Handle(TLUpdateChatParticipantAdmin updateChatParticipantAdmin)
        {
            if (updateChatParticipantAdmin.ChatId.Value == CurrentItem.Index)
            {
                Execute.BeginOnUIThread(() =>
                {
                    var user = Items.FirstOrDefault(x => x.Index == updateChatParticipantAdmin.UserId.Value);
                    if (user != null)
                    {
                        var chat40 = CurrentItem as TLChat40;
                        var adminsEnabled = chat40 != null && chat40.AdminsEnabled.Value;
                        if (adminsEnabled)
                        {
                            user.IsAdmin = updateChatParticipantAdmin.IsAdmin.Value;
                            user.NotifyOfPropertyChange(() => user.IsAdmin);
                        }
                        else
                        {
                            user.IsAdmin = false;
                            user.NotifyOfPropertyChange(() => user.IsAdmin);
                        }
                    }

                    NotifyOfPropertyChange(() => CanEditChat);
                });
            }
        }

        public void Handle(TLChannel channel)
        {
            if (CurrentItem is TLChannel && CurrentItem.Index == channel.Index)
            {
                Execute.BeginOnUIThread(UpdateTitles);
            }
        }

        public void Handle(TLUpdateUserStatus updateUserStatus)
        {
            var channel = CurrentItem as TLChannel;
            if (channel != null && channel.IsMegaGroup)
            {
                Execute.BeginOnUIThread(() =>
                {
                    TLUserBase user = null;
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Index == updateUserStatus.UserId.Value)
                        {
                            user = Items[i];
                            user._status = updateUserStatus.Status;

                            Items.RemoveAt(i);
                            UpdateTitles();
                            break;
                        }
                    }

                    if (user != null)
                    {
                        InsertInDescOrder(Items, user);
                        user.NotifyOfPropertyChange(() => user.StatusCommon);
                    }
                });

                return;
            }

            var chat41 = CurrentItem as TLChat41;
            if (chat41 != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    TLUserBase user = null;
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Index == updateUserStatus.UserId.Value)
                        {
                            user = Items[i];
                            user._status = updateUserStatus.Status;

                            Items.RemoveAt(i);
                            UpdateTitles();
                            break;
                        }
                    }

                    if (user != null)
                    {
                        InsertInDescOrder(Items, user);
                        user.NotifyOfPropertyChange(() => user.StatusCommon);
                    }
                });

                return;
            }
        }
    }
}
