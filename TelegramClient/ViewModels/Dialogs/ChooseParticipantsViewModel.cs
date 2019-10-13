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
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public class ChooseParticipantsViewModel : ItemsViewModelBase<TLUserBase>
    {
        public string Text { get; set; }

        private SearchUsersRequest _lastSearchRequest;

        private List<TLUserBase> _source;

        private readonly LRUCache<string, SearchUsersRequest> _searchResultsCache = new LRUCache<string, SearchUsersRequest>(Constants.MaxCacheCapacity);

        private volatile bool _isFullResults;

        public TLUserBase CurrentUser { get; protected set; }

        public ChooseParticipantsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new ObservableCollection<TLUserBase>();
            Status = AppResources.Loading;
            BeginOnThreadPool(() =>
            {
                if (_source == null)
                {
                    var source= new List<TLUserBase>();
                    var contacts = CacheService.GetContacts().OrderBy(x => x.FullName);
                    foreach (var contact in contacts)
                    {
                        if (contact is TLUserEmpty)
                        {
                            continue;
                        }
                        var user = contact as TLUser;
                        if (user != null && user.IsSelf)
                        {
                            CurrentUser = user;
                            //continue;
                        }

                       source.Add(contact);
                    }

                    _source = source;
                }

                Status = string.Empty;
                NotifyOfPropertyChange(() => CurrentUser);

                var count = 0;
                const int firstSliceCount = 10;
                var secondSlice = new List<TLUserBase>();
                foreach (var contact in _source)
                {
                    if (count < firstSliceCount)
                    {
                        LazyItems.Add(contact);
                    }
                    else
                    {
                        secondSlice.Add(contact);
                    }
                    count++;
                }

                if (Items.Count == 0 && LazyItems.Count == 0)
                {
                    Status = AppResources.NoUsersHere;
                }

                BeginOnUIThread(() => PopulateItems(() =>
                {
                    _isFullResults = true;
                    foreach (var item in secondSlice)
                    {
                        Items.Add(item);
                    }
                }));
            });
        }

        #region Action
        public void UserAction(TLUserBase user)
        {
            OpenUserChat(user);
        }

        public void OpenUserChat(TLUserBase user)
        {
            if (user == null) return;

            StateService.RemoveBackEntry = true;
            StateService.With = user;
            StateService.AnimateTitle = true;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        public void NewGroup()
        {
            StateService.RemoveBackEntry = true;
            NavigationService.UriFor<CreateDialogViewModel>().Navigate();
        }

        public void NewSecretChat()
        {
            StateService.RemoveBackEntry = true;
            NavigationService.UriFor<AddSecretChatParticipantViewModel>().Navigate();
        }

        public void NewBroadcastList()
        {
            StateService.RemoveBackEntry = true;
            NavigationService.UriFor<CreateBroadcastViewModel>().Navigate();
        }

        public void NewChannel()
        {
            ChannelIntroViewModel.CheckIntroEnabledAsync(
                enabled => BeginOnUIThread(() =>
                {
                    if (enabled)
                    {
                        StateService.RemoveBackEntry = true;
                        NavigationService.UriFor<ChannelIntroViewModel>().Navigate();
                    }
                    else
                    {
                        StateService.RemoveBackEntry = true;
                        NavigationService.UriFor<CreateChannelStep1ViewModel>().Navigate();
                    }
                }));
        }
        #endregion

        public void Search()
        {
            if (_lastSearchRequest != null)
            {
                _lastSearchRequest.Cancel();
            }

            var text = Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                if (_isFullResults) return;

                LazyItems.Clear();               
                Items.Clear();

                foreach (var contact in _source)
                {
                    Items.Add(contact);
                }

                _isFullResults = true;

                return;
            }

            var nextSearchRequest = CreateSearchRequest(text);

            _isFullResults = false;
            IsWorking = true;
            nextSearchRequest.ProcessAsync(results =>
                Execute.BeginOnUIThread(() =>
                {
                    if (nextSearchRequest.IsCanceled) return;

                    Items.Clear();
                    LazyItems.Clear();

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
                    Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;

                    PopulateItems();
                }));

            _searchResultsCache[nextSearchRequest.Text] = nextSearchRequest;
            _lastSearchRequest = nextSearchRequest;
        }

        private SearchUsersRequest CreateSearchRequest(string text)
        {
            SearchUsersRequest request;
            if (!_searchResultsCache.TryGetValue(text, out request))
            {
                IList<TLUserBase> source;

                if (_lastSearchRequest != null
                    && text.IndexOf(_lastSearchRequest.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    source = _lastSearchRequest.Source;
                }
                else
                {
                    _source = _source ??
                              CacheService.GetContacts()
                                  .Where(x => !(x is TLUserEmpty) && x.Index != StateService.CurrentUserId)
                                  .OrderBy(x => x.FullName)
                                  .ToList();

                    source = _source;
                }

                request = new SearchUsersRequest(text, source);
            }
            return request;
        }
    }
}
