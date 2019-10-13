// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Search;

namespace TelegramClient.ViewModels.Chats
{
    public class AddAdminsViewModel :
        ItemsViewModelBase<TLUserBase>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatParticipants>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatAdmins>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatParticipantAdmin>
    {
        private string _text;

        public string Text
        {
            get { return _text; }
            set { SetField(ref _text, value, () => Text); }
        }

        private bool _adminsEnabled;

        public bool AdminsEnabled
        {
            get { return _adminsEnabled; }
            set
            {
                SetField(ref _adminsEnabled, value, () => AdminsEnabled);
                ToggleChatAdmins();
            }
        }

        public bool IsEnabled
        {
            get { return !IsWorking; }
        }

        public string AddAdminsDescription
        {
            get { return IsEnabled ? AppResources.AdminsDescription : AppResources.MembersDescription; }
        }

        private readonly TLChatBase _currentChat;

        public AddAdminsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Status = AppResources.Loading;

            _currentChat = StateService.CurrentChat;
            StateService.CurrentChat = null;

            var chat = _currentChat as TLChat40;
            _adminsEnabled = chat != null && chat.AdminsEnabled.Value;

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => AdminsEnabled))
                {
                    if (!AdminsEnabled)
                    {
                        foreach (var item in Items)
                        {
                            item.IsSelected = !AdminsEnabled;
                        }
                    }
                    else
                    {
                        UpdateAdmins(_currentChat.Participants as IChatParticipants);
                    }

                    NotifyOfPropertyChange(() => AddAdminsDescription);
                }
            };
        }

        public void ToggleChatAdmins()
        {
            if (IsWorking) return;

            Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("messages.toggleChatAdmins chat_id={0} enabled={1}", _currentChat.Id, AdminsEnabled));
            IsWorking = true;
            NotifyOfPropertyChange(() => IsEnabled);
            MTProtoService.ToggleChatAdminsAsync(_currentChat.Id, new TLBool(AdminsEnabled), 
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    NotifyOfPropertyChange(() => IsEnabled);
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    NotifyOfPropertyChange(() => IsEnabled);

                    Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.toggleChatAdmins error " + error);
                }));
        }

        public void EditChatAdmin(TLUserBase user)
        {
            if (user == null) return;
            if (user.Index == StateService.CurrentUserId) return;
            if (!AdminsEnabled) return;

            IsWorking = true;
            user.IsSelected = !user.IsSelected;
            Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("messages.editChatAdmin chat_id={0} user_id={1} is_admin={2}", _currentChat.Id, user.Index, user.IsSelected));
            MTProtoService.EditChatAdminAsync(_currentChat.Id, user.ToInputUser(), new TLBool(user.IsSelected),
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.editChatAdmin error " + error);
                }));
        }

        public void ForwardInAnimationComplete()
        {
            Items.Clear();
            LazyItems.Clear();

            var chat = _currentChat;
            if (chat != null)
            {
                var participants = chat.Participants as TLChatParticipants40;
                if (participants != null)
                {
                    var users = new List<TLUserBase>(participants.Participants.Count);
                    foreach (var participant in participants.Participants)
                    {
                        var user = CacheService.GetUser(participant.UserId);
                        if (user != null)
                        {
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
                            if (AdminsEnabled)
                            {
                                user.IsSelected = participant is TLChatParticipantAdmin ||
                                                  participant is TLChatParticipantCreator;
                            }
                            else
                            {
                                user.IsSelected = true;
                            }
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
        }

        private void UpdateUsers(List<TLUserBase> users, System.Action callback)
        {
            const int firstSliceCount = 3;
            var secondSlice = new List<TLUserBase>();
            for (var i = 0; i < users.Count; i++)
            {
                if (i < firstSliceCount)
                {
                    Items.Add(users[i]);
                }
                else
                {
                    secondSlice.Add(users[i]);
                }
            }
            Status = Items.Count > 0 ? string.Empty : AppResources.Loading;

            Execute.BeginOnUIThread(() =>
            {
                foreach (var user in secondSlice)
                {
                    Items.Add(user);
                }
                callback.SafeInvoke();
            });
        }

        private void UpdateItems()
        {
            if (_currentChat == null) return;

            IsWorking = true;
            Status = Items.Count > 0 ? string.Empty : AppResources.Loading;
            MTProtoService.GetFullChatAsync(_currentChat.Id,
                chatFull =>
                {
                    IsWorking = false;
                    Status = string.Empty;

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
                        foreach (var participant in participants.Participants)
                        {
                            var user = newUsersCache[participant.UserId.Value];
                            if (user.Status is TLUserStatusOnline)
                            {
                                onlineUsers++;
                            }

                            if (AdminsEnabled)
                            {
                                user.IsSelected = participant is TLChatParticipantAdmin ||
                                                  participant is TLChatParticipantCreator;
                            }
                            else
                            {
                                user.IsSelected = true;
                            }
                            if (!usersCache.ContainsKey(user.Index))
                            {
                                BeginOnUIThread(() => InsertInDescOrder(Items, user));
                            }
                        }
                        _currentChat.UsersOnline = onlineUsers;
                    }

                    var chatFull28 = chatFull.FullChat as TLChatFull28;
                    if (chatFull28 != null)
                    {
                        _currentChat.ExportedInvite = chatFull28.ExportedInvite;
                    }
                },
                error =>
                {
                    IsWorking = false;
                    Status = string.Empty;
                });
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



        #region Searching

        private SearchUsersRequest _lastUsersRequest;

        private List<TLUserBase> _source;

        private readonly LRUCache<string, SearchUsersRequest> _searchResultsCache = new LRUCache<string, SearchUsersRequest>(Constants.MaxCacheCapacity);

        public void Search()
        {
            if (_lastUsersRequest != null)
            {
                _lastUsersRequest.Cancel();
            }

            var text = Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                LazyItems.Clear();
                Items.Clear();
                foreach (var item in _source)
                {
                    Items.Add(item);
                }
                //Status = string.IsNullOrEmpty(Text) ? AppResources.SearchAmongYourContacts : AppResources.NoResults;
                return;
            }

            SearchUsersRequest nextUsersRequest;
            if (!_searchResultsCache.TryGetValue(text, out nextUsersRequest))
            {
                IList<TLUserBase> source;

                if (_lastUsersRequest != null
                    && text.IndexOf(_lastUsersRequest.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    source = _lastUsersRequest.Source;
                }
                else
                {
                    _source = _source ??
                        Items
                        .OrderBy(x => x.FullName)
                        .ToList();

                    source = _source;
                }

                nextUsersRequest = new SearchUsersRequest(text, source);
            }

            IsWorking = true;
            nextUsersRequest.ProcessAsync(results =>
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    if (nextUsersRequest.IsCanceled) return;

                    //Status = string.Empty;
                    Items.Clear();
                    LazyItems.Clear();
                    if (results.Count > 0)
                    {
                        //Items.Add(new TLServiceText { Text = AppResources.Contacts });
                    }
                    for (var i = 0; i < results.Count; i++)
                    {
                        if (i < 6)
                        {
                            Items.Add(results[i]);
                        }
                        else
                        {
                            LazyItems.Add(results[i]);
                        }
                    }

                    IsWorking = false;
                    //Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;

                    //return;

                    if (LazyItems.Count > 0)
                    {
                        PopulateItems();
                    }
                }));
            _searchResultsCache[nextUsersRequest.Text] = nextUsersRequest;
            _lastUsersRequest = nextUsersRequest;
        }

        #endregion




        public void Handle(TLUpdateChatParticipants updateChatParticipants)
        {
            if (_currentChat.Index != updateChatParticipants.Participants.ChatId.Value)
            {
                return;
            }

            Execute.BeginOnUIThread(() =>
            {
                UpdateAdmins(updateChatParticipants.Participants as IChatParticipants);
            });
        }

        private void UpdateAdmins(IChatParticipants participants)
        {
            if (participants == null) return;
            
            var admins = new Dictionary<int, bool>();
            foreach (var participant in participants.Participants)
            {
                if (participant is TLChatParticipant)
                {
                    admins[participant.UserId.Value] = false;
                }
                else
                {
                    admins[participant.UserId.Value] = true;
                }
            }

            foreach (var item in Items)
            {
                bool isAdmin;
                if (admins.TryGetValue(item.Index, out isAdmin))
                {
                    item.IsSelected = isAdmin;
                }
            }
        }

        public void Handle(TLUpdateChatAdmins updateChatAdmins)
        {
            if (_currentChat.Index != updateChatAdmins.ChatId.Value)
            {
                return;
            }

            _adminsEnabled = updateChatAdmins.Enabled.Value;
            NotifyOfPropertyChange(() => AdminsEnabled);            
        }

        public void Handle(TLUpdateChatParticipantAdmin updateChatParticipantAdmin)
        {
            if (_currentChat.Index != updateChatParticipantAdmin.ChatId.Value)
            {
                return;
            }

            Execute.BeginOnUIThread(() =>
            {
                foreach (var item in Items)
                {
                    if (item.Index == updateChatParticipantAdmin.UserId.Value)
                    {
                        item.IsSelected = updateChatParticipantAdmin.IsAdmin.Value;
                        break;
                    }
                }
            });
        }
    }
}
