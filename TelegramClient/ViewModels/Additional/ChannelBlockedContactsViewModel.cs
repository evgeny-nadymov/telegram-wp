// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class ChannelBlockedContactsViewModel : ItemsViewModelBase<TLUserBase>
    {
        private bool _isEmptyList = true;

        public bool IsEmptyList
        {
            get { return _isEmptyList; }
            set { SetField(ref _isEmptyList, value, () => IsEmptyList); }
        }

        public TLChannel Channel { get; set; }

        public ChannelBlockedContactsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Channel = StateService.CurrentChat as TLChannel;
            StateService.CurrentChat = null;

            eventAggregator.Subscribe(this);
        }

        protected override void OnInitialize()
        {
            if (Channel == null) return;

            //Status = AppResources.Loading;
            IsWorking = true;
            MTProtoService.GetParticipantsAsync(Channel.ToInputChannel(), new TLChannelParticipantsKicked68 { Q = TLString.Empty }, new TLInt(0), new TLInt(100), new TLInt(0),
                result =>
                {
                    var channelParticipants = result as TLChannelParticipants;
                    if (channelParticipants != null)
                    {

                        var kickedParticipants = new Dictionary<int, int>();
                        foreach (var participant in channelParticipants.Participants)
                        {
                            var kickedParticipant = participant as TLChannelParticipantBanned;
                            if (kickedParticipant != null)
                            {
                                kickedParticipants[kickedParticipant.UserId.Value] = kickedParticipant.UserId.Value;
                            }
                        }

                        var contacts = channelParticipants;
                        foreach (var user in contacts.Users)
                        {
                            if (kickedParticipants.ContainsKey(user.Index))
                            {
                                var cachedUser = CacheService.GetUser(new TLInt(user.Index));
                                if (cachedUser != null)
                                {
                                    LazyItems.Add(cachedUser);
                                }
                                else
                                {
                                    LazyItems.Add(user);
                                }
                            }
                        }

                        BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            IsEmptyList = Items.Count == 0 && LazyItems.Count == 0;
                            Status = string.Empty;
                            PopulateItems();
                        });
                    }
                },
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Status = string.Empty;
                }));

            base.OnInitialize();
        }

        public void UnblockContact(TLUserBase user)
        {
            if (user == null) return;

            IsWorking = true;
            MTProtoService.KickFromChannelAsync(Channel,
                user.ToInputUser(),
                TLBool.False,
                result => BeginOnUIThread(() =>
                {
                    Items.Remove(user);

                    IsWorking = false;
                    IsEmptyList = Items.Count == 0 && LazyItems.Count == 0;
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }));
        }

        public void UserAction(TLUserBase user)
        {
            if (user == null) return;

            OpenContactDetails(user);
        }

        public void OpenContactDetails(TLUserBase user)
        {
            if (user == null) return;

            StateService.CurrentContact = user;
            NavigationService.UriFor<ContactViewModel>().Navigate();
        }
    }
}
