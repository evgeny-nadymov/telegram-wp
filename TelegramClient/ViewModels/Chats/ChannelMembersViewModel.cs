// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Chats
{
    public class ChannelMembersViewModel : ItemDetailsViewModelBase
    {
        public bool IsPublic
        {
            get
            {
                var channel = CurrentItem as TLChannel;

                return channel != null && channel.IsPublic;
            }
        }

        public ObservableCollection<TLUserBase> Items { get; set; }

        private string _status;

        public string Status
        {
            get { return _status; }
            set { SetField(ref _status, value, () => Status); }
        }

        public ChannelMembersViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            CurrentItem = StateService.CurrentChat;
            StateService.CurrentChat = null;

            Items = new ObservableCollection<TLUserBase>();

            Status = AppResources.Loading;
        }

        public void ForwardInAnimationComplete()
        {
            UpdateItems();

            if (StateService.Participant != null)
            {
                var participant = StateService.Participant;
                StateService.Participant = null;

                var channel = CurrentItem as TLChannel;
                if (channel == null) return;

                IsWorking = true;
                MTProtoService.InviteToChannelAsync(channel.ToInputChannel(), new TLVector<TLInputUserBase>{ participant.ToInputUser() }, 
                    result => Execute.BeginOnUIThread(() =>
                    {
                        if (channel.ParticipantsCount != null)
                        {
                            channel.ParticipantsCount = new TLInt(channel.ParticipantsCount.Value + 1);
                        }
                        CacheService.Commit();

                        IsWorking = false;

                        Items.Insert(0, participant);
                        Status = Items.Count > 0 ? string.Empty : AppResources.NoUsersHere;
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        if (error.TypeEquals(ErrorType.PEER_FLOOD))
                        {
                            //MessageBox.Show(AppResources.PeerFloodAddContact, AppResources.Error, MessageBoxButton.OK);
                            ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodAddContact, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                                result =>
                                {
                                    if (result == CustomMessageBoxResult.RightButton)
                                    {
                                        TelegramViewBase.NavigateToUsername(MTProtoService, Constants.SpambotUsername, null, null, null);
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
            }
        }

        private bool _once;

        private void UpdateItems()
        {
            if (_once) return;
            var channel = CurrentItem as TLChannel;
            if (channel == null) return;

            IsWorking = true;
            Status = Items.Count > 0 ? string.Empty : AppResources.Loading;
            MTProtoService.GetParticipantsAsync(channel.ToInputChannel(), new TLChannelParticipantsRecent(), new TLInt(0), new TLInt(200), new TLInt(0), 
                result => Execute.BeginOnUIThread(() =>
                {
                    var channelParticipants = result as TLChannelParticipants;
                    if (channelParticipants != null)
                    {
                        _once = true;
                        IsWorking = false;
                        Items.Clear();
                        foreach (var user in channelParticipants.Users)
                        {
                            Items.Add(user);
                        }
                        Status = Items.Count > 0 ? string.Empty : AppResources.NoUsersHere;
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Status = string.Empty;

                    Execute.ShowDebugMessage("channels.getParticipants error " + error);
                }));
        }

        public void AddMember()
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || channel.IsForbidden) return;

            StateService.IsInviteVisible = false;
            StateService.CurrentChat = channel;
            StateService.RemovedUsers = Items;
            StateService.RequestForwardingCount = false;
            NavigationService.UriFor<AddChatParticipantViewModel>().Navigate();
        }

        public void DeleteMember(TLUserBase user)
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || channel.IsForbidden) return;

            if (user == null) return;

            IsWorking = true;
            MTProtoService.KickFromChannelAsync(channel, user.ToInputUser(), TLBool.True,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    Items.Remove(user);
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    Execute.ShowDebugMessage("channels.getParticipants error " + error);
                }));
        }

        public void Invite()
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || channel.IsForbidden) return;

            StateService.CurrentChat = channel;
            NavigationService.UriFor<InviteLinkViewModel>().Navigate();
        }
    }
}
