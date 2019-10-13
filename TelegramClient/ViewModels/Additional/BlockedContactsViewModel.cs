// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class BlockedContactsViewModel : ItemsViewModelBase<TLUserBase>, Telegram.Api.Aggregator.IHandle<TLUpdateUserBlocked>
    {
        public BlockedContactsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (StateService.Participant != null)
            {
                var blockedUser = StateService.Participant;
                StateService.Participant = null;

                MTProtoService.BlockAsync(blockedUser.ToInputUser(),
                    result =>
                    {
                        Items.Add(blockedUser);
                        Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : string.Format("{0}", AppResources.NoUsersHere);
                    });
            }
        }

        protected override void OnInitialize()
        {
            Status = AppResources.Loading;
            MTProtoService.GetBlockedAsync(new TLInt(0), new TLInt(int.MaxValue),
                result =>
                {
                    var contacts = result as TLContactsBlocked;
                    if (contacts != null)
                    {
                        foreach (var user in contacts.Users)
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

                        Status = Items.Count > 0 || LazyItems.Count > 0? string.Empty : string.Format("{0}", AppResources.NoUsersHere);
                        BeginOnUIThread(PopulateItems);
                    }
                },
                error => BeginOnUIThread(() => Status = string.Empty));

            base.OnInitialize();
        }

        public void UnblockContact(TLUserBase user)
        {
            if (user == null) return;

            var inputUser = user.ToInputUser();
            MTProtoService.UnblockAsync(
                inputUser,
                result =>
                {
                    Items.Remove(user);

                    Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : string.Format("{0}", AppResources.NoUsersHere);
                },
                error =>
                {
                    
                });
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

        public void AddContact()
        {
            StateService.RemovedUsers = Items;
            NavigationService.UriFor<AddChatParticipantViewModel>().Navigate();
        }

        public void Handle(TLUpdateUserBlocked update)
        {
            var user = CacheService.GetUser(update.UserId);
            if (user != null)
            {
                UpdateBlockedList(user, update);
            }
            else
            {
                MTProtoService.GetFullUserAsync(
                    new TLInputUser { UserId = update.UserId, AccessHash = new TLLong(0) }, 
                    fullUser => UpdateBlockedList(fullUser.User, update));
            }
        }

        private void UpdateBlockedList(TLUserBase user, TLUpdateUserBlocked update)
        {
            if (update.Blocked.Value)
            {
                Items.Add(user);
            }
            else
            {
                Items.Remove(user);
            }
            Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : string.Format("{0}", AppResources.NoUsersHere);
        }
    }
}
