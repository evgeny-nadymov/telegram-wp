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
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute; 

namespace TelegramClient.ViewModels.Search
{
    public class SearchDialogsViewModel : SearchItemsViewModelBase<TLObject>
    {
        public string Text { get; set; }

        public IList<TLDialogBase> LoadedDilaogs { get; set; }

        public bool ChatsOnly { get; set; }

        public Func<TLObject, bool> OpenDialogDetailsAction { get; set; } 

        public SearchDialogsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new ObservableCollection<TLObject>();
            DisplayName = LowercaseConverter.Convert(AppResources.Conversations);
        }

        public bool OpenDialogDetails(TLObject with)
        {
            return OpenDialogDetailsAction(with);
        }

        private string _lastText;

        private IList<TLUserBase> _usersSource;

        private IList<TLChatBase> _chatsSource;

        private SearchDialogsRequest _lastDialogsRequest;

        private readonly LRUCache<string, SearchDialogsRequest> _searchResultsCache = new LRUCache<string, SearchDialogsRequest>(Constants.MaxCacheCapacity); 

        public override void Search(string text)
        {
            if (!string.Equals(text, Text, StringComparison.OrdinalIgnoreCase)) return;

            _lastText = text;

            if (_lastDialogsRequest != null)
            {
                _lastDialogsRequest.Cancel();
            }

            var trimmedText = Text.Trim();

            if (string.IsNullOrEmpty(trimmedText))
            {
                Items.Clear();
                Status = string.IsNullOrEmpty(Text) ? AppResources.SearchAmongYourContacts : AppResources.NoResults;
                return;
            }

            SearchDialogsRequest nextDialogsRequest;
            if (!_searchResultsCache.TryGetValue(text, out nextDialogsRequest))
            {
                IList<TLUserBase> usersSource;

                if (_lastDialogsRequest != null
                    && text.IndexOf(_lastDialogsRequest.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    usersSource = _lastDialogsRequest.UsersSource;
                }
                else
                {
                    var source = _usersSource;

                    if (source == null)
                    {
                        source = CacheService.GetUsersForSearch(LoadedDilaogs)
                        .Where(x => x != null && !(x is TLUserEmpty) && !x.IsDeleted && x.Index != StateService.CurrentUserId)
                        .ToList();
                    }

                    _usersSource = source;

                    usersSource = _usersSource;
                }

                IList<TLChatBase> chatsSource;

                if (_lastDialogsRequest != null
                    && text.IndexOf(_lastDialogsRequest.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    chatsSource = _lastDialogsRequest.ChatsSource;
                }
                else
                {
                    _chatsSource = _chatsSource ??
                        CacheService.GetChats()
                        .Where(x =>
                        {
                            var chat = x as TLChat41;
                            var channel = x as TLChannel;
                            return channel != null || (chat != null && !chat.IsMigrated);
                        })
                        .OrderBy(x => x.FullName)
                        .ToList();

                    foreach (var chat in _chatsSource)
                    {
                        chat.FullNameWords = chat.FullName.Split(' ');
                    }

                    chatsSource = _chatsSource;
                }

                nextDialogsRequest = new SearchDialogsRequest(CacheService, text, usersSource, chatsSource);
            }

            IsWorking = true;
            nextDialogsRequest.ProcessAsync(ChatsOnly, results =>
                Execute.BeginOnUIThread(() =>
                {
                    if (nextDialogsRequest.IsCanceled) return;
                    if (!string.Equals(Text, nextDialogsRequest.Text, StringComparison.OrdinalIgnoreCase)) return;

                    const int firstSliceCount = 6;
                    Items.Clear();
                    var items = new List<TLObject>();
                    for (var i = 0; i < results.Count; i++)
                    {
                        if (i < firstSliceCount)
                        {
                            Items.Add(results[i]);
                        }
                        else
                        {
                            items.Add(results[i]);
                        }
                    }

                    IsWorking = false;
                    Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.NoResults : string.Empty;


                    Execute.BeginOnUIThread(() =>
                    {
                        if (!string.Equals(Text, nextDialogsRequest.Text, StringComparison.OrdinalIgnoreCase)) return;

                        foreach (var item in items)
                        {
                            Items.Add(item);
                        }
                    });
                }));

            _searchResultsCache[nextDialogsRequest.Text] = nextDialogsRequest;
            _lastDialogsRequest = nextDialogsRequest;
        }

        protected override void OnActivate()
        {
            if (_lastText == Text) return;
            Search(Text);

            base.OnActivate();
        }

        public event EventHandler CloseKeyboard;
 
        public void RaiseCloseKeyboard()
        {
            var handler = CloseKeyboard;
            if (handler != null)
            {
                CloseKeyboard(this, System.EventArgs.Empty);
            }
        }
    }

    public class SearchDialogsRequest
    {
        public volatile bool IsCanceled;

        public string Text { get; private set; }

        public IList<TLUserBase> UsersSource { get; private set; }

        public IList<TLChatBase> ChatsSource { get; private set; } 

        public IList<TLObject> Results { get; private set; }

        private ICacheService _cacheService;

        public SearchDialogsRequest(ICacheService cacheService, string text, IList<TLUserBase> usersSource, IList<TLChatBase> chatsSource)
        {
            _cacheService = cacheService;
            Text = text;
            UsersSource = usersSource;
            ChatsSource = chatsSource;
        }

        public void ProcessAsync(bool chatsOnly, Action<IList<TLObject>> callback)
        {
            if (Results != null)
            {
                IsCanceled = false;
                callback.SafeInvoke(Results);
                return;
            }

            var usersSource = UsersSource;
            var chatsSource = ChatsSource;
            Execute.BeginOnThreadPool(() =>
            {
                var useFastSearch = !Text.Contains(" ");

                var userResults = new List<TLUserBase>(usersSource.Count);
                if (!chatsOnly)
                {
                    foreach (var contact in usersSource)
                    {
                        //var userName = contact as IUserName;
                        //if (userName != null 
                        //    && userName.UserName != null
                        //    && (userName.UserName.ToString().StartsWith(Text, StringComparison.OrdinalIgnoreCase)
                        //        || userName.UserName.ToString().StartsWith(Text.TrimStart('@'), StringComparison.OrdinalIgnoreCase)))
                        //{
                        //    userResults.Add(contact);

                        //    continue;
                        //}
                        var added = false;
                        
                        if (contact.FirstName.ToString().StartsWith(Text, StringComparison.OrdinalIgnoreCase)
                            || contact.LastName.ToString().StartsWith(Text, StringComparison.OrdinalIgnoreCase)
                            || contact.FullName.StartsWith(Text, StringComparison.OrdinalIgnoreCase))
                        {
                            added = true;
                            userResults.Add(contact);
                        }

                        if (!added)
                        {
                            var userNameContact = contact as IUserName;
                            if (userNameContact != null)
                            {
                                var userName = userNameContact.UserName != null ? userNameContact.UserName.ToString() : string.Empty;
                                if (userName.StartsWith(Text, StringComparison.OrdinalIgnoreCase))
                                {
                                    userResults.Add(contact);
                                }
                            }
                        }
                    }
                }

                var chatsResults = new List<TLChatBase>(chatsSource.Count);

                foreach (var chat in chatsSource)
                {
                    var added = false;
                    if (!useFastSearch)
                    {
                        var fullName = chat.FullName;

                        var i = fullName.IndexOf(Text, StringComparison.OrdinalIgnoreCase);
                        if (i != -1)
                        {
                            while (i < fullName.Length && i != -1)
                            {
                                if (i == 0 || (i > 0 && fullName[i - 1] == ' '))
                                {
                                    added = true;
                                    chatsResults.Add(chat);
                                    break;
                                }
                                if (fullName.Length > i + 1)
                                {
                                    i = fullName.IndexOf(Text, i + 1, StringComparison.OrdinalIgnoreCase);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (chat.FullNameWords != null
                            && chat.FullNameWords.Any(x => x.StartsWith(Text, StringComparison.OrdinalIgnoreCase)))
                        {
                            added = true;
                            chatsResults.Add(chat);
                        }
                    }

                    if (!added)
                    {
                        var userNameChannel = chat as IUserName;
                        if (userNameChannel != null)
                        {
                            var userName = userNameChannel.UserName != null ? userNameChannel.UserName.ToString() : string.Empty;
                            if (userName.StartsWith(Text, StringComparison.OrdinalIgnoreCase))
                            {
                                chatsResults.Add(chat);
                            }
                        }
                    }
                }

                Results = new List<TLObject>(userResults.Count + chatsResults.Count);
                foreach (var userResult in userResults)
                {
                    Results.Add(userResult);
                    if (userResult.Dialog == null)
                    {
                        userResult.Dialog = _cacheService.GetDialog(new TLPeerUser { Id = userResult.Id });
                    }
                }
                foreach (var chatsResult in chatsResults)
                {
                    Results.Add(chatsResult);
                    if (chatsResult.Dialog == null)
                    {
                        chatsResult.Dialog = _cacheService.GetDialog(new TLPeerChat { Id = chatsResult.Id });
                    }
                }

                callback.SafeInvoke(Results);
            });
        }

        public void Cancel()
        {
            IsCanceled = true;
        }

        public void CancelAsync()
        {
            Execute.BeginOnThreadPool(Cancel);
        }
    }
}
