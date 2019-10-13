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
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Search
{
    public class SearchSharedContactsViewModel : ItemsViewModelBase<TLUserBase>
    {
        private string _text;

        public string Text
        {
            get { return _text; }
            set { SetField(ref _text, value, () => Text); }
        }

        public Action<TLUserBase> AttachAction;

        public SearchSharedContactsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new ObservableCollection<TLUserBase>();
            Status = AppResources.NoResults;

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Text))
                {
                    if (string.IsNullOrEmpty(Text))
                    {
                        Search();
                    }
                    else
                    {
                        var text = Text;
                        BeginOnUIThread(TimeSpan.FromSeconds(0.2), () =>
                        {
                            if (!string.Equals(Text, text)) return;

                            Search();
                        });
                    }
                }
            };
        }

        public void UserAction(TLUserBase user)
        {
            if (user == null) return;

            AttachAction.SafeInvoke(user);
        }

        private SearchUsersRequest _lastUsersRequest;

        public IList<TLUserBase> Source { get; set; }

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
                System.Diagnostics.Debug.WriteLine("EmptyText text={0} canceled={1} items_count={2} current_text={3}", text, _lastUsersRequest != null ? _lastUsersRequest.IsCanceled.ToString() : "null", LazyItems.Count, Text);

                LazyItems.Clear();
                Items.Clear();
                Status = string.Empty;
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
                    source = Source;
                }

                nextUsersRequest = new SearchUsersRequest(text, source);
            }

            IsWorking = true;
            nextUsersRequest.ProcessAsync(results =>
                Execute.BeginOnUIThread(() =>
                {
                    if (nextUsersRequest.IsCanceled) return;

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
                    Status = Items.Count > 0 ? string.Empty : AppResources.NoResults;

                    IsWorking = false;
                    if (LazyItems.Count > 0)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            if (nextUsersRequest.IsCanceled) return;

                            System.Diagnostics.Debug.WriteLine("ContinueResults text={0} canceled={1} items_count={2} current_text={3}", nextUsersRequest.Text, nextUsersRequest.IsCanceled, LazyItems.Count, Text);

                            foreach (var item in LazyItems)
                            {
                                Items.Add(item);
                            }
                            LazyItems.Clear();
                            Status = Items.Count > 0 ? string.Empty : AppResources.NoResults;
                        });
                    }
                }));
            _searchResultsCache[nextUsersRequest.Text] = nextUsersRequest;
            _lastUsersRequest = nextUsersRequest;
        }
    }
}
