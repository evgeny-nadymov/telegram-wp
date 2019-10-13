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
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class AllowUsersViewModel :ItemsViewModelBase<TLUserBase>
    {
        private IPrivacyValueUsersRule _usersRule;

        public string Title { get; protected set; }

        public AllowUsersViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            if (StateService.UsersRule is TLPrivacyValueAllowUsers)
            {
                Title = AppResources.AlwaysShare.ToLowerInvariant();
            }
            else
            {
                Title = AppResources.NeverShare.ToLowerInvariant();
            }

            BeginOnThreadPool(() =>
            {
                _usersRule = StateService.UsersRule;
                StateService.UsersRule = null;

                if (_usersRule != null)
                {
                    foreach (var userId in _usersRule.Users)
                    {
                        var user = CacheService.GetUser(userId);
                        if (user != null)
                        {
                            Items.Add(user);
                        }
                    }

                    Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : string.Format("{0}", AppResources.NoUsersHere);
                }
            });
        }

        protected override void OnActivate()
        {
            BeginOnThreadPool(() =>
            {
                if (StateService.Participant != null)
                {
                    var participant = StateService.Participant;
                    StateService.Participant = null;
                    var addedUsers = new List<TLUserBase> { participant };


                    if (_usersRule != null)
                    {
                        var usersCache = new Dictionary<int, int>();
                        foreach (var userId in _usersRule.Users)
                        {
                            usersCache[userId.Value] = userId.Value;
                        }

                        foreach (var addedUser in addedUsers)
                        {
                            if (!usersCache.ContainsKey(addedUser.Index))
                            {
                                _usersRule.Users.Add(addedUser.Id);
                                Items.Add(addedUser);
                            }
                        }

                        Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : string.Format("{0}", AppResources.NoUsersHere);
                    }
                }
            });

            base.OnActivate();
        }

        public void DeleteUser(TLUserBase user)
        {
            if (user == null) return;
            if (_usersRule == null) return;

            Items.Remove(user);

            for (var i = 0; i < _usersRule.Users.Count; i++)
            {
                if (_usersRule.Users[i].Value == user.Index)
                {
                    _usersRule.Users.RemoveAt(i);
                    break;
                }
            }

            Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : string.Format("{0}", AppResources.NoUsersHere);
        }

        public void OpenUserDetails(TLUserBase user)
        {
            if (user == null) return;

            StateService.CurrentContact = user;
            NavigationService.UriFor<ContactViewModel>().Navigate();
        }

        public void SelectUsers()
        {
            if (_usersRule == null) return;

            StateService.SelectedUserIds = _usersRule.Users.Items;
            NavigationService.UriFor<AddChatParticipantViewModel>().Navigate();
        }

        protected override void OnDeactivate(bool close)
        {
            StateService.UsersRule = _usersRule;

            base.OnDeactivate(close);
        }
    }
}
