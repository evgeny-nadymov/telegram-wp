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
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public class CreateDialogViewModel : ItemsViewModelBase<TLUserBase>
    {
        private string _title;

        public string Title
        {
            get { return _title; }
            set { SetField(ref _title, value, () => Title); }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set { SetField(ref _text, value, () => Text); }
        }

        private readonly IList<TLUserBase> _selectedUsers = new ObservableCollection<TLUserBase>(); 

        public IList<TLUserBase> SelectedUsers
        {
            get { return _selectedUsers; }
        }

        private readonly Dictionary<int, TLUserBase> _selectedUsersCache = new Dictionary<int, TLUserBase>(); 

        private SearchUsersRequest _lastSearchRequest;

        protected List<TLUserBase> _source;

        private readonly Dictionary<string, SearchUsersRequest> _searchResultsCache = new Dictionary<string, SearchUsersRequest>();

        private volatile bool _isFullResults;

        private string _limitString;

        public string LimitString
        {
            get { return _limitString; }
            set { SetField(ref _limitString, value, () => LimitString); }
        }

        public ObservableCollection<TLUserBase> GroupedUsers { get; set; }


        private TLInt _broadcastSizeMax;

        public TLInt BroadcastSizeMax
        {
            get
            {
                if (_broadcastSizeMax == null)
                {
                    var config = CacheService.GetConfig();

                    _broadcastSizeMax = config != null ? config.BroadcastSizeMax : new TLInt(5000);
                }

                return _broadcastSizeMax;
            }
        }

        private TLInt _chatSizeMax;

        public TLInt ChatSizeMax
        {
            get
            {
                if (_chatSizeMax == null)
                {
                    var config = CacheService.GetConfig();
                    
                    _chatSizeMax = config != null? config.ChatSizeMax : new TLInt(200);
                }

                return _chatSizeMax;
            }
        }

        public CreateDialogViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            GroupedUsers = new ObservableCollection<TLUserBase>();
            //GroupedUsers = new ObservableCollection<AlphaKeyGroup<TLUserBase>>();

            UpdateLimitString();

            BeginOnThreadPool(() =>
            {
                //Thread.Sleep(300);
                _source = _source ??
                    CacheService.GetContacts()
                    .Where(x => !(x is TLUserEmpty) && x.Index != StateService.CurrentUserId)
                    .OrderBy(x => x.FullName)
                    .ToList();

                Status = string.Empty;

                var count = 0;
                const int firstSliceCount = 10;
                var secondSlice = new List<TLUserBase>();
                foreach (var contact in _source)
                {
                    contact._isSelected = false;
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

                if (_source.Count == 0)
                {
                    Status = AppResources.NoUsersHere;
                }


                BeginOnUIThread(() => PopulateItems(() =>
                {
                    foreach (var item in secondSlice)
                    {
                        Items.Add(item);
                    }
                }));
                Thread.Sleep(500);
                BeginOnUIThread(() =>
                {
                    foreach (var item in _source)
                    {
                        GroupedUsers.Add(item);
                    }
                });
                //var groups = AlphaKeyGroup<TLUserBase>.CreateGroups(
                //    _source,
                //    Thread.CurrentThread.CurrentUICulture,
                //    x => x.FullName,
                //    false);

                //foreach (var @group in groups)
                //{
                //    var gr = new AlphaKeyGroup<TLUserBase>(@group.Key);
                //    foreach (var u in @group.OrderBy(x => x.FullName))
                //    {
                //        gr.Add(u);
                //    }

                //    BeginOnUIThread(() =>
                //    {
                //        GroupedUsers.Add(gr);
                //    });
                //}

                
            });
        }

        private void UpdateLimitString()
        {
            LimitString = string.Format("({0}/{1})", SelectedUsers.Count, BroadcastSizeMax);
        }

        public event EventHandler<ScrollToEventArgs> ScrollTo;

        protected virtual void RaiseScrollTo(ScrollToEventArgs e)
        {
            var handler = ScrollTo;
            if (handler != null) handler(this, e);
        }

        public void ChooseContact(TLUserBase user)
        {
            if (user == null) return;

            if (user.IsSelected)
            {
                OnContactUnchecked(user);
            }
            else
            {
                OnContactChecked(user);
            }
        }

        public void OnContactChecked(TLUserBase user)
        {
            if (user == null) return;
            if (SelectedUsers.Count == ChatSizeMax.Value - 1)
            {
                MessageBox.Show(AppResources.ChatMaxSizeNotification, AppResources.AppName, MessageBoxButton.OK);
                return;
            }

            user.IsSelected = true;
            SelectedUsers.Add(user);
            _selectedUsersCache[user.Index] = user;
            NotifyOfPropertyChange(() => SelectedUsers);

            Text = string.Empty;
            UpdateLimitString();

            RaiseScrollTo(new ScrollToEventArgs(user));
        }

        public void OnContactUnchecked(TLUserBase user)
        {
            if (user == null) return;

            user.IsSelected = false;
            SelectedUsers.Remove(user);
            _selectedUsersCache.Remove(user.Index);
            NotifyOfPropertyChange(() => SelectedUsers);

            UpdateLimitString();
        }

        public virtual void Create()
        {
            if (string.IsNullOrEmpty(Title))
            {
                MessageBox.Show(AppResources.PleaseEnterGroupSubject, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            var participants = new TLVector<TLInputUserBase>();
            foreach (var item in SelectedUsers)
            {
                participants.Add(item.ToInputUser());
            }

            if (participants.Count == 0)
            {
                MessageBox.Show(AppResources.PleaseChooseAtLeastOneParticipant, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            IsWorking = true;
            MTProtoService.CreateChatAsync(participants, new TLString(Title),
                statedMessage =>
                {
                    IsWorking = false;
                    foreach (var item in Items)
                    {
                        item.IsSelected = false;
                    }

                    var updates = statedMessage as TLUpdates;
                    if (updates != null)
                    {
                        StateService.With = updates.Chats.First();
                        StateService.RemoveBackEntry = true;
                        BeginOnUIThread(() => NavigationService.UriFor<DialogDetailsViewModel>().Navigate());
                    }
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                    }
                }));
        }

        protected override void OnActivate()
        {
            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            base.OnActivate();
        }

        public void Search(string text)
        {
            if (_lastSearchRequest != null)
            {
                _lastSearchRequest.Cancel();
            }

            var trimmedText = text.Trim();

            if (string.IsNullOrEmpty(trimmedText))
            {
                if (_isFullResults) return;

                LazyItems.Clear();
                Items.Clear();

                //foreach (var contact in _source)
                //{
                //    Items.Add(contact);
                //}

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

        public void DeleteLastUser()
        {
            var user = SelectedUsers.LastOrDefault();
            if (user != null)
            {
                OnContactUnchecked(user);
            }
        }
    }
}
