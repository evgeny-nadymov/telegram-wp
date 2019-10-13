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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Chats
{
    public class ChannelAdministratorsViewModel : ItemDetailsViewModelBase
    {
        public ObservableCollection<TLUserBase> Items { get; set; }

        private string _status;

        public string Status
        {
            get { return _status; }
            set { SetField(ref _status, value, () => Status); }
        }

        public string AddAdministratorHint
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && channel.IsMegaGroup
                    ? AppResources.GroupAdministratorsHint
                    : AppResources.ChannelAdministratorsHint;
            }
        }

        private string _selectedChatInviteItem;

        public string SelectedChatInviteItem
        {
            get { return _selectedChatInviteItem; }
            set { SetField(ref _selectedChatInviteItem, value, () => SelectedChatInviteItem); }
        }

        public void SetSelectedSound(string sound)
        {
            _selectedChatInviteItem = sound;
        }

        public List<string> ChatInviteItems { get; protected set; }

        public ChannelAdministratorsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            CurrentItem = StateService.CurrentChat;
            StateService.CurrentChat = null;

            ChatInviteItems = new List<string>
            {
                AppResources.AllMembers.ToLowerInvariant(),
                AppResources.OnlyAdmins.ToLowerInvariant()
            };
            var channel = CurrentItem as TLChannel44;
            _selectedChatInviteItem = channel != null && channel.IsDemocracy? ChatInviteItems[0] : ChatInviteItems[1];

            Items = new ObservableCollection<TLUserBase>();

            Status = AppResources.Loading;

            PropertyChanged += (sender, e) =>
            {
                if (Property.NameEquals(e.PropertyName, () => SelectedChatInviteItem))
                {
                    channel = CurrentItem as TLChannel44;
                    if (channel == null) return;
                    var enabled = SelectedChatInviteItem == ChatInviteItems[0];
                    IsWorking = true;
                    MTProtoService.ToggleInvitesAsync(channel.ToInputChannel(), new TLBool(enabled), 
                        result => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            Execute.ShowDebugMessage("channels.toggleInvites error " + error);
                        }));
                }
            };
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            StateService.UpdateChannelAdministrators = true;
        }

        public static TLChannelAdminRights GetDefaultAdminRights(bool supergroup, TLChannelParticipantRoleBase role)
        {
            if (role is TLChannelRoleEditor)
            {
                var rights = new TLChannelAdminRights { Flags = new TLInt(0) };
                if (supergroup)
                {
                    rights.ChannelInfo = true;
                    rights.DeleteMessages = true;
                    rights.BanUsers = true;
                    rights.InviteLinks = true;
                    rights.PinMessages = true;
                    rights.AddAdmins = false;
                }
                else
                {
                    rights.ChannelInfo = true;
                    rights.PostMessages = true;
                    rights.EditMessages = true;
                    rights.DeleteMessages = true;
                    rights.InviteUsers = true;
                    rights.AddAdmins = false;
                }
            }

            return new TLChannelAdminRights{Flags = new TLInt(0)};
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

                var rights = GetDefaultAdminRights(channel.IsMegaGroup, new TLChannelRoleEditor());

                IsWorking = true;
                MTProtoService.EditAdminAsync(channel, participant.ToInputUser(), rights,
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        var updates = result as TLUpdates;
                        if (updates != null)
                        {
                            var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                            if (updateNewMessage != null)
                            {
                                EventAggregator.Publish(updateNewMessage.Message);
                            }
                        }
                        Items.Insert(0, participant);
                        var channelParticipants = channel.ChannelParticipants;
                        if (channelParticipants != null)
                        {
                            var user = channelParticipants.Users.FirstOrDefault(x => x.Index == participant.Index);
                            if (user == null)
                            {
                                channelParticipants.Users.Add(participant);
                                channelParticipants.Participants.Add(new TLChannelParticipantAdmin
                                {
                                    Flags = new TLInt(0),
                                    CanEdit = true,
                                    UserId = participant.Id,
                                    InviterId = new TLInt(StateService.CurrentUserId),
                                    PromotedById = new TLInt(StateService.CurrentUserId),
                                    Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                                    AdminRights = rights
                                });
                                channelParticipants.Count = new TLInt(channelParticipants.Count.Value + 1);
                            }
                        }
                        Status = Items.Count > 0 ? string.Empty : AppResources.NoUsersHere;
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                        {
                            if (error.TypeEquals(ErrorType.ADMINS_TOO_MUCH))
                            {
                                MessageBox.Show(AppResources.AdminsTooMuch, AppResources.Error, MessageBoxButton.OK);
                            }
                        }

                        Execute.ShowDebugMessage("channels.editAdmin error " + error);
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
            Status = Items.Count > 0? string.Empty : AppResources.Loading;
            MTProtoService.GetParticipantsAsync(channel.ToInputChannel(), new TLChannelParticipantsAdmins(), new TLInt(0), new TLInt(200), new TLInt(0),
                result => BeginOnUIThread(() =>
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
                error => BeginOnUIThread(() =>
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
            StateService.CurrentRole = new TLChannelRoleEditor();
            StateService.CurrentAdminRights = GetDefaultAdminRights(channel.IsMegaGroup, new TLChannelRoleEditor());
            StateService.RemovedUsers = Items;
            StateService.RequestForwardingCount = false;
            NavigationService.UriFor<AddChatParticipantViewModel>().Navigate();
        }

        public void DeleteMember(TLUserBase user)
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null) return;

            if (user == null) return;

            var rights = new TLChannelAdminRights{ Flags = new TLInt(0) };

            IsWorking = true;
            MTProtoService.EditAdminAsync(channel, user.ToInputUser(), rights, 
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    Items.Remove(user);
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    Execute.ShowDebugMessage("channels.editAdmin error " + error);
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
