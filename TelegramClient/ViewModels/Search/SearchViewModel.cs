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
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Contacts;
using Telegram.Controls.Utils;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Search
{
    public class RecentGroup
    {
        public TLObject Item1 { get; set; }
        public TLObject Item2 { get; set; }
        public TLObject Item3 { get; set; }
        public TLObject Item4 { get; set; }
    }

    public class SearchViewModel : 
        ItemsViewModelBase<TLObject>, 
        Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        Telegram.Api.Aggregator.IHandle<ClearTopPeersEventArgs>
    {
        public Func<TLObject, bool, bool> Callback;

        public string Watermark { get; set; }

        public bool SuppressMessagesSearch { get; set; }

        public IList<TLDialogBase> LoadedDilaogs { get; set; }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (value != _text)
                {
                    _text = value;
                    NotifyOfPropertyChange(() => ShowRecent);
                    Search();
                }
            }
        }

        private IList<TLUserBase> _usersSource;

        private IList<TLChatBase> _chatsSource;

        private SearchRequest _lastRequest;

        private readonly LRUCache<string, SearchRequest> _searchResultsCache = new LRUCache<string, SearchRequest>(Constants.MaxCacheCapacity);

        public SearchViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Watermark = AppResources.SearchChatsOrMessages;

            Recent = new ObservableCollection<TLObject>();

            TopPeers = new ObservableCollection<TLObject>();
            TopBots = new ObservableCollection<TLUserBase>();

            LoadedDilaogs = stateService.LoadedDialogs;
            stateService.LoadedDialogs = null;

            Status = string.Empty;

            EventAggregator.Subscribe(this);
        }

        public bool OpenDialogDetails(TLObject with)
        {
            return OpenDialogDetails(with, true);
        }

        public bool OpenDialogDetails(TLObject with, bool saveToRecent)
        {
            if (with == null) return false;

            var result = true;
            if (Callback != null)
            {
                result = Callback(with, saveToRecent);
            }
            else
            {
                var dialog = with as TLDialog;
                if (dialog != null)
                {
                    with = dialog.With;
                    var channel = with as TLChannel;
                    if (channel != null)
                    {
                        channel = CacheService.GetChat(channel.Id) as TLChannel; // MigratedFromChatId
                        if (channel != null)
                        {
                            with = channel;
                        }
                    }

                    StateService.Message = dialog.TopMessage;
                }

                with.ClearBitmap();
                //StateService.RemoveBackEntry = true;
                StateService.With = with;
                StateService.AnimateTitle = saveToRecent;
                NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
            }

            if (saveToRecent)
            {
                SaveRecentAsync(with);
            }

            return true;
        }

        public ObservableCollection<TLObject> TopPeers { get; set; }

        public ObservableCollection<TLUserBase> TopBots { get; set; }

        private static void AddTopPeerByRating(TLTopPeer peer, IList<TLTopPeer> peers)
        {
            var rating = peer.Rating.Value;
            for (var i = 0; i < peers.Count; i++)
            {
                if (peers[i].Rating.Value <= rating)
                {
                    peers.Insert(i, peer);
                    return;
                }
            }

            peers.Add(peer);
        }

        private bool _initialized;

        public void ForwardInAnimationComplete()
        {
            //if (_initialized)
            //{
            //    return;
            //}

            //_initialized = true;

            var topBots = new List<TLTopPeer>();
            var topPeers = new List<TLTopPeer>();
            var recent = new List<TLObject>();

            CreateTop(topBots, topPeers);
            UpdateTopAsync();
            CreateRecent(recent);

            var recentFirstSliceCount = 5;
            var topBotsFirstSliceCount = 5;
            var topPeersFirstSliceCount = 5;

            Recent.Clear();
            for (var i = 0; i < recentFirstSliceCount && i < recent.Count; i++)
            {
                Recent.Add(recent[i]);
            }

            TopPeers.Clear();
            for (var i = 0; i < topPeersFirstSliceCount && i < topPeers.Count; i++)
            {
                TopPeers.Add(topPeers[i].Object);
            }

            TopBots.Clear();
            for (var i = 0; i < topBotsFirstSliceCount && i < topBots.Count; i++)
            {
                TopBots.Add((TLUserBase)topBots[i].Object);
            }

            NotifyOfPropertyChange(() => ShowRecent);

            BeginOnUIThread(() =>
            {
                for (var i = recentFirstSliceCount; i < recent.Count; i++)
                {
                    Recent.Add(recent[i]);
                }

                for (var i = topPeersFirstSliceCount; i < topPeers.Count; i++)
                {
                    TopPeers.Add(topPeers[i].Object);
                }

                for (var i = topBotsFirstSliceCount; i < topBots.Count; i++)
                {
                    TopBots.Add((TLUserBase)topBots[i].Object);
                }
            });
        }

        private void CreateRecent(List<TLObject> recent)
        {
            _recentResults = _recentResults ?? TLUtils.OpenObjectFromMTProtoFile<TLVector<TLResultInfo>>(_recentSyncRoot, Constants.RecentSearchResultsFileName) ?? new TLVector<TLResultInfo>();

            foreach (var result in _recentResults)
            {
                if (result.Type.ToString() == "user")
                {
                    var user = CacheService.GetUser(result.Id);
                    if (user != null)
                    {
                        recent.Add(user);
                        if (user.Dialog == null)
                        {
                            user.Dialog = CacheService.GetDialog(new TLPeerUser {Id = user.Id});
                        }
                    }
                }

                if (result.Type.ToString() == "chat")
                {
                    var chat = CacheService.GetChat(result.Id);
                    if (chat != null)
                    {
                        var chat41 = chat as TLChat41;
                        if (chat41 == null || !chat41.IsMigrated)
                        {
                            recent.Add(chat);
                            if (chat.Dialog == null)
                            {
                                var dialog = CacheService.GetDialog(new TLPeerChat {Id = chat.Id});

                                chat.Dialog = dialog;
                            }
                        }
                    }
                }
            }
        }

        private void CreateTop(List<TLTopPeer> topBotsList, List<TLTopPeer> topPeersList)
        {
            _topPeers = StateService.GetTopPeers() as TLTopPeers;
            if (_topPeers == null) return;

            foreach (var categoryPeers in _topPeers.Categories)
            {
                if (categoryPeers.Category is TLTopPeerCategoryBotsInline)
                {
                    continue;
                }
                else if (categoryPeers.Category is TLTopPeerCategoryBotsPM)
                {
                    continue;

                    foreach (var topPeer in categoryPeers.Peers)
                    {
                        var user = CacheService.GetUser(topPeer.Peer.Id);
                        if (user != null)
                        {
                            AddTopPeerByRating(topPeer, topBotsList);

                            topPeer.Object = user;
                            if (user.Dialog == null)
                            {
                                user.Dialog = CacheService.GetDialog(new TLPeerUser { Id = user.Id });
                            }
                        }
                    }
                }
                else if (categoryPeers.Category is TLTopPeerCategoryCorrespondents)
                {
                    foreach (var topPeer in categoryPeers.Peers)
                    {
                        var user = CacheService.GetUser(topPeer.Peer.Id);
                        if (user != null)
                        {
                            AddTopPeerByRating(topPeer, topPeersList);

                            topPeer.Object = user;
                            if (user.Dialog == null)
                            {
                                user.Dialog = CacheService.GetDialog(new TLPeerUser { Id = user.Id });
                            }
                        }
                    }
                }
                else if (categoryPeers.Category is TLTopPeerCategoryGroups)
                {
                    continue;

                    foreach (var topPeer in categoryPeers.Peers)
                    {
                        var chat = CacheService.GetChat(topPeer.Peer.Id);
                        if (chat != null)
                        {
                            AddTopPeerByRating(topPeer, topPeersList);

                            topPeer.Object = chat;
                            if (chat.Dialog == null)
                            {
                                chat.Dialog = CacheService.GetDialog(new TLPeerChat { Id = chat.Id });
                            }
                        }
                    }
                }
                else if (categoryPeers.Category is TLTopPeerCategoryChannels)
                {
                    continue;

                    foreach (var topPeer in categoryPeers.Peers)
                    {
                        var channel = CacheService.GetChat(topPeer.Peer.Id);
                        if (channel != null)
                        {
                            AddTopPeerByRating(topPeer, topPeersList);

                            topPeer.Object = channel;
                            if (channel.Dialog == null)
                            {
                                channel.Dialog = CacheService.GetDialog(new TLPeerChannel { Id = channel.Id });
                            }
                        }
                    }
                }
            }
        }

        public void ResetTopPeerRating(TLObject obj)
        {
            var user = obj as TLUserBase;
            if (user != null)
            {
                var confirmation = MessageBox.Show(string.Format(AppResources.ConfirmResetTopPeerRating, user.FullName), AppResources.Confirm, MessageBoxButton.OKCancel);
                if (confirmation != MessageBoxResult.OK) return;

                var topPeerCategory = new TLTopPeerCategoryCorrespondents();

                IsWorking = true;
                MTProtoService.ResetTopPeerRatingAsync(topPeerCategory, user.ToInputPeer(),
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        var topPeers = StateService.GetTopPeers() as TLTopPeers;
                        if (topPeers != null)
                        {
                            var category = topPeers.Categories.FirstOrDefault(x => x.Category.GetType() == topPeerCategory.GetType());
                            if (category != null)
                            {
                                for (var i = 0; i < category.Peers.Count; i++)
                                {
                                    if (category.Peers[i].Peer.Id.Value == user.Index)
                                    {
                                        category.Peers.RemoveAt(i);
                                        break;
                                    }
                                }

                                for (var i = 0; i < topPeers.Users.Count; i++)
                                {
                                    if (topPeers.Users[i].Index == user.Index)
                                    {
                                        topPeers.Users.RemoveAt(i);
                                        break;
                                    }
                                }

                                for (var i = 0; i < TopPeers.Count; i++)
                                {
                                    var topUser = TopPeers[i] as TLUserBase;
                                    if (topUser != null)
                                    {
                                        if (topUser.Index == user.Index)
                                        {
                                            TopPeers.RemoveAt(i);
                                            break;
                                        }
                                    }
                                }

                                StateService.SaveTopPeers(topPeers);
                            }
                        }
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Execute.ShowDebugMessage("contacts.resetTopPeerRating error " + error);
                    }));
            }
        }

        private void UpdateTopAsync()
        {
            UpdateTopCommon(MTProtoService, StateService);
        }

        private static DateTime? _updateTopFloodWait;

        public static void UpdateTopCommon(IMTProtoService mtProtoService, IStateService stateService, Action<TLTopPeers> callback = null)
        {
            if (_updateTopFloodWait.HasValue && _updateTopFloodWait.Value > DateTime.Now)
            {
                return;
            }

            var hash = 0;
            if (_topPeers != null)
            {
                hash = TLUtils.GetTopPeersHash(_topPeers);
            }

            mtProtoService.GetTopPeersAsync(
                GetTopPeersFlags.BotsInline | GetTopPeersFlags.Correspondents,
                new TLInt(0), new TLInt(0), new TLInt(hash),
                result =>
                {
                    var topPeers = result as TLTopPeers;
                    if (topPeers != null)
                    {
                        _topPeers = topPeers;

                        stateService.SaveTopPeers(topPeers);
                    }

                    var topPeersDisabled = result as TLTopPeersDisabled;
                    if (topPeersDisabled != null)
                    {
                        _topPeers = null;

                        stateService.SaveTopPeers(topPeersDisabled);
                    }

                    callback.SafeInvoke(topPeers);
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error.CodeEquals(ErrorCode.FLOOD)
                        && error.Message.ToString().StartsWith("FLOOD_WAIT"))
                    {
                        var message = error.Message.ToString();
                        if (message.StartsWith("FLOOD_WAIT"))
                        {
                            var seconds = 0;
                            if (Int32.TryParse(message.Replace("FLOOD_WAIT_", string.Empty), out seconds))
                            {
                                _updateTopFloodWait = DateTime.Now.AddSeconds(seconds);
                            }
                        }
                    }
                    Execute.ShowDebugMessage("contacts.getTopPeers error " + error);
                }));
        }

        public void Search()
        {
            var text = Text;

            if (string.IsNullOrEmpty(text.Trim()))
            {
                Items.Clear();
                Status = string.IsNullOrEmpty(text.Trim()) ? Watermark : AppResources.NoResults;
                return;
            }

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
            {
                if (!string.Equals(text, Text, StringComparison.OrdinalIgnoreCase)) return;

                Search(Text);
            });
        }

        public void Search(string text)
        {
            if (!string.Equals(text, Text, StringComparison.OrdinalIgnoreCase)) return;

            if (_lastRequest != null)
            {
                _lastRequest.Cancel();
            }

            var trimmedText = Text.Trim();
            if (string.IsNullOrEmpty(trimmedText))
            {
                Items.Clear();
                Status = string.IsNullOrEmpty(Text) ? Watermark : AppResources.NoResults;

                return;
            }

            var nextRequest = GetNextRequest(text);

            IsWorking = true; 
            Status = Items.Count == 0 ? AppResources.Loading : string.Empty;
            nextRequest.ProcessAsync(results =>
                {
                    if (nextRequest.IsCanceled) return;
                    if (!string.Equals(Text, nextRequest.Text, StringComparison.OrdinalIgnoreCase)) return;

                    const int firstSliceCount = 6;
                    Items.Clear();
                    if (results.Count > 0)
                    {
                        Items.Add(new TLServiceText { Text = AppResources.ChatsAndContacts });
                    }
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
                    Status = Items.Count == 0 && items.Count == 0 ? AppResources.Loading : string.Empty;

                    Execute.BeginOnUIThread(() =>
                    {
                        if (nextRequest.IsCanceled) return;
                        if (!string.Equals(Text, nextRequest.Text, StringComparison.OrdinalIgnoreCase)) return;

                        foreach (var item in items)
                        {
                            Items.Add(item);
                        }

                        ProcessGlobalSearch(nextRequest);
                    });
                });

            _searchResultsCache[nextRequest.Text] = nextRequest;
            _lastRequest = nextRequest;
        }

        private SearchRequest GetNextRequest(string text)
        {
            SearchRequest nextRequest;
            if (!_searchResultsCache.TryGetValue(text, out nextRequest))
            {
                IList<TLUserBase> usersSource;

                if (_lastRequest != null
                    && text.IndexOf(_lastRequest.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    usersSource = _lastRequest.UsersSource;
                }
                else
                {
                    var source = _usersSource;

                    if (source == null)
                    {
                        source = CacheService.GetUsersForSearch(LoadedDilaogs)
                            .Where(x => x != null && !(x is TLUserEmpty) && !x.IsDeleted)// && x.Index != StateService.CurrentUserId)
                            .ToList();
                    }

                    _usersSource = source;

                    foreach (var user in _usersSource)
                    {
                        user.FullNameWords = user.FullName.Split(' ');
                    }

                    usersSource = _usersSource;
                }

                IList<TLChatBase> chatsSource;

                if (_lastRequest != null
                    && text.IndexOf(_lastRequest.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    chatsSource = _lastRequest.ChatsSource;
                }
                else
                {
                    _chatsSource = _chatsSource ??
                                   CacheService.GetChats()
                                       .OrderBy(x => x.FullName)
                                       .ToList();

                    foreach (var chat in _chatsSource)
                    {
                        chat.FullNameWords = chat.FullName.Split(' ');
                    }

                    chatsSource = _chatsSource;
                }

                nextRequest = new SearchRequest(CacheService, text, usersSource, chatsSource);
            }
            return nextRequest;
        }

        private void ProcessGlobalSearch(SearchRequest request)
        {
            if (request.GlobalResults != null)
            {
                if (!string.Equals(Text, request.Text, StringComparison.OrdinalIgnoreCase)) return;
                if (request.IsCanceled) return;

                if (request.GlobalResults.Count > 0)
                {
                    Items.Add(new TLServiceText { Text = AppResources.GlobalSearch });
                    foreach (var user in request.GlobalResults)
                    {
                        Items.Add(user);
                    }
                }

                Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;

                ProcessMessagesSearch(request);
            }
            else
            {
                if (request.Text.Length < Constants.UsernameMinLength)
                {
                    request.GlobalResults = new List<TLObject>();

                    ProcessMessagesSearch(request);
                    return;
                }

                IsWorking = true;
                MTProtoService.SearchAsync(new TLString(request.Text), new TLInt(100),
                    result => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        request.GlobalResults = new List<TLObject>();
                        var contactsFound74 = result as TLContactsFound74;
                        if (contactsFound74 != null)
                        {
                            if (contactsFound74.MyResults.Count > 0)
                            {
                                var users = new List<TLUserBase>();
                                var chats = new List<TLChatBase>();
                                var userIndex = contactsFound74.Users.ToDictionary(x => x.Index);
                                var chatIndex = contactsFound74.Chats.ToDictionary(x => x.Index);
                                foreach (var myResult in contactsFound74.MyResults)
                                {
                                    var peerUser = myResult as TLPeerUser;
                                    if (peerUser != null)
                                    {
                                        if (request.UserResultsIndex.ContainsKey(peerUser.Id.Value))
                                        {
                                            continue;
                                        }

                                        TLUserBase user;
                                        if (userIndex.TryGetValue(peerUser.Id.Value, out user))
                                        {
                                            users.Add(user);
                                            request.Results.Add(user);
                                            request.UserResultsIndex[user.Index] = user;
                                        }
                                    }

                                    var peerChannel = myResult as TLPeerChannel;
                                    if (peerChannel != null)
                                    {
                                        if (request.ChatResultsIndex.ContainsKey(peerChannel.Id.Value))
                                        {
                                            continue;
                                        }

                                        TLChatBase chat;
                                        if (chatIndex.TryGetValue(peerChannel.Id.Value, out chat))
                                        {
                                            chats.Add(chat);
                                            request.Results.Add(chat);
                                            request.ChatResultsIndex[chat.Index] = chat;
                                        }
                                    }

                                    var peerChat = myResult as TLPeerChat;
                                    if (peerChat != null)
                                    {
                                        if (request.ChatResultsIndex.ContainsKey(peerChat.Id.Value))
                                        {
                                            continue;
                                        }

                                        TLChatBase chat;
                                        if (chatIndex.TryGetValue(peerChat.Id.Value, out chat))
                                        {
                                            chats.Add(chat);
                                            request.Results.Add(chat);
                                            request.ChatResultsIndex[chat.Index] = chat;
                                        }
                                    }
                                }

                                if (!string.Equals(Text, request.Text, StringComparison.OrdinalIgnoreCase)) return;
                                if (request.IsCanceled) return;

                                if (users.Count > 0 || chats.Count > 0)
                                {
                                    if (Items.Count == 0)
                                    {
                                        Items.Add(new TLServiceText { Text = AppResources.ChatsAndContacts });
                                    }

                                    foreach (var chat in chats)
                                    {
                                        Items.Add(chat);
                                    }

                                    foreach (var user in users)
                                    {
                                        Items.Add(user);
                                    }
                                }
                            }
                        }

                        var contactsFound40 = result as TLContactsFound40;
                        if (contactsFound40 != null)
                        {
                            foreach (var chat in contactsFound40.Chats)
                            {
                                if (request.ChatResultsIndex != null && request.ChatResultsIndex.ContainsKey(chat.Index))
                                {
                                    continue;
                                }
                                chat.IsGlobalResult = true;
                                request.GlobalResults.Add(chat);
                            }

                            foreach (var user in contactsFound40.Users)
                            {
                                if (request.UserResultsIndex != null && request.UserResultsIndex.ContainsKey(user.Index))
                                {
                                    continue;
                                }
                                user.IsGlobalResult = true;
                                request.GlobalResults.Add(user);
                            }
                        }

                        if (!string.Equals(Text, request.Text, StringComparison.OrdinalIgnoreCase)) return;
                        if (request.IsCanceled) return;

                        var hasResults = request.GlobalResults.Count > 0;
                        if (hasResults)
                        {
                            Items.Add(new TLServiceText { Text = AppResources.GlobalSearch });
                            foreach (var r in request.GlobalResults)
                            {
                                Items.Add(r);
                            }
                        }

                        Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;

                        ProcessMessagesSearch(request);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;

                        Execute.ShowDebugMessage("contacts.search error " + error);
                    }));
            }
        }

        public void LoadNextSlice()
        {
            if (_lastRequest == null) return;

            ProcessMessagesSearch(_lastRequest, true);
        }

        public void ProcessMessagesSearch(SearchRequest request, bool nextSlice = false)
        {
            if (SuppressMessagesSearch) return;

            if (request.MessageResults != null && !nextSlice)
            {
                if (!string.Equals(Text, request.Text, StringComparison.OrdinalIgnoreCase)) return;
                if (request.IsCanceled) return;

                if (request.MessageResults.Count > 0)
                {
                    Items.Add(new TLServiceText { Text = AppResources.MessageSearch });
                    foreach (var result in request.MessageResults)
                    {
                        Items.Add(result);
                    }
                }

                Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;
            }
            else
            {
                if (IsWorking) return;
                if (request.IsLastSliceLoaded) return;

                var offsetId = 0;
                var offsetDate = 0;
                TLInputPeerBase offsetPeer = new TLInputPeerEmpty();
                if (request.MessageResults != null && request.MessageResults.Count > 0)
                {
                    var lastDialog = request.MessageResults.LastOrDefault(x => x is TLDialog) as TLDialog;
                    if (lastDialog != null)
                    {
                        var lastMessage = lastDialog.TopMessage as TLMessageCommon;
                        if (lastMessage != null)
                        {
                            offsetId = lastMessage.Index;
                            offsetDate = lastMessage.DateIndex;
                            if (lastMessage.ToId is TLPeerUser)
                            {
                                offsetPeer = !lastMessage.Out.Value
                                    ? DialogDetailsViewModel.PeerToInputPeer(new TLPeerUser { Id = lastMessage.FromId })
                                    : DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                            }
                            else
                            {
                                offsetPeer = DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                            }
                        }
                    }
                }

                IsWorking = true;
                //MTProtoService.SearchAsync(
                //    new TLInputPeerEmpty(),
                //    new TLString(request.Text),
                //    new TLInputMessagesFilterEmpty(),
                //    new TLInt(0), new TLInt(0), new TLInt(request.Offset), new TLInt(0), new TLInt(request.Limit),
                MTProtoService.SearchGlobalAsync(
                    new TLString(request.Text),
                    new TLInt(offsetDate), offsetPeer, new TLInt(offsetId), new TLInt(request.Limit),
                        result =>
                        {
                            CacheService.AddChats(result.Chats, results => { });
                            CacheService.AddUsers(result.Users, results => { });
                            
                            var items = new List<TLObject>();
                            var newMessages = result as TLMessages;
                            if (newMessages != null)
                            {
                                var usersCache = new Dictionary<int, TLUserBase>();
                                foreach (var user in newMessages.Users)
                                {
                                    usersCache[user.Index] = user;
                                }

                                var chatsCache = new Dictionary<int, TLChatBase>();
                                foreach (var chat in newMessages.Chats)
                                {
                                    chatsCache[chat.Index] = chat;
                                }

                                foreach (var message in newMessages.Messages.OfType<TLMessageCommon>())
                                {
                                    var dialog = new TLDialog { TopMessage = message };
                                    var peer = TLUtils.GetPeerFromMessage(message);
                                    if (peer is TLPeerUser)
                                    {
                                        TLUserBase user;
                                        if (!usersCache.TryGetValue(peer.Id.Value, out user))
                                        {
                                            continue;
                                        }
                                        dialog.With = user;
                                    }
                                    else if (peer is TLPeerChat)
                                    {
                                        TLChatBase chat;
                                        if (!chatsCache.TryGetValue(peer.Id.Value, out chat))
                                        {
                                            continue;
                                        }

                                        var chat41 = chat as TLChat41;
                                        if (chat41 != null && chat41.MigratedTo != null)
                                        {
                                            var inputChannel = chat41.MigratedTo as TLInputChannel;
                                            if (inputChannel != null)
                                            {
                                                var channel = CacheService.GetChat(inputChannel.ChannelId);
                                                if (channel != null)
                                                {
                                                    chat = channel;
                                                }
                                            }
                                        }

                                        dialog.With = chat;
                                    }
                                    else if (peer is TLPeerChannel)
                                    {
                                        TLChatBase chat;
                                        if (!chatsCache.TryGetValue(peer.Id.Value, out chat))
                                        {
                                            continue;
                                        }

                                        dialog.With = chat;
                                    }
                                    items.Add(dialog);
                                }
                            }

                            Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                if (request.MessageResults == null)
                                {
                                    request.MessageResults = new List<TLObject>();
                                }
                                foreach (var item in items)
                                {
                                    request.MessageResults.Add(item);
                                }
                                request.Offset += request.Limit;
                                request.IsLastSliceLoaded = result.Messages.Count < request.Limit;

                                if (!string.Equals(Text, request.Text, StringComparison.OrdinalIgnoreCase)) return;
                                if (request.IsCanceled) return;

                                if (items.Count > 0)
                                {
                                    if (request.Offset == request.Limit)
                                    {
                                        Items.Add(new TLServiceText { Text = AppResources.MessageSearch });
                                    }
                                    foreach (var item in items)
                                    {
                                        Items.Add(item);
                                    }
                                }

                                Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;
                            });
                        },
                        error => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;
                        }));
            }
        }

        #region Recents

        private static TLTopPeers _topPeers;

        private static readonly object _recentSyncRoot = new object();

        private static TLVector<TLResultInfo> _recentResults;

        public ObservableCollection<TLObject> Recent { get; set; }

        public bool ShowRecent
        {
            get { return (Recent.Count > 0 || TopBots.Count > 0 || TopPeers.Count > 0) && string.IsNullOrEmpty(Text); }
        }

        public void ClearRecent()
        {
            Recent.Clear();
            NotifyOfPropertyChange(() => ShowRecent);

            DeleteRecentAsync(false);
        }

        public void ClearRecent(TLObject obj)
        {
            Recent.Remove(obj);
            NotifyOfPropertyChange(() => ShowRecent);

            DeleteRecentAsync(obj);
        }

        public static void DeleteRecentAsync(bool deleteTop = true)
        {
            Execute.BeginOnThreadPool(() =>
            {
                _recentResults = new TLVector<TLResultInfo>();
                FileUtils.Delete(_recentSyncRoot, Constants.RecentSearchResultsFileName);

                if (deleteTop)
                {
                    _topPeers = null;
                    FileUtils.Delete(_recentSyncRoot, Constants.TopPeersFileName);
                }
            });
        }

        public static void DeleteRecentAsync(TLObject with)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var recentResults = _recentResults ?? new TLVector<TLResultInfo>();

                if (with != null)
                {
                    var id = GetId(with);
                    var type = GetType(with);
                    if (id == null || type == null) return;

                    for (var i = 0; i < recentResults.Count; i++)
                    {
                        if (recentResults[i].Id.Value == id.Value
                            && recentResults[i].Type.ToString() == type.ToString())
                        {
                            recentResults.RemoveAt(i);
                            break;
                        }
                    }
                }

                _recentResults = recentResults;

                TLUtils.SaveObjectToMTProtoFile(_recentSyncRoot, Constants.RecentSearchResultsFileName, recentResults);
            });
        }

        public void SaveRecentAsync(TLObject with)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var recentResults = _recentResults ?? new TLVector<TLResultInfo>();

                if (with != null)
                {
                    var id = GetId(with);
                    var type = GetType(with);
                    if (id == null || type == null) return;

                    var isAdded = false;
                    long maxCount = 0;
                    for (var i = 0; i < recentResults.Count; i++)
                    {
                        var recentResult = recentResults[i];

                        maxCount = maxCount > recentResult.Count.Value ? maxCount : recentResult.Count.Value;

                        if (recentResults[i].Id.Value == id.Value
                            && recentResults[i].Type.ToString() == type.ToString())
                        {
                            recentResults[i].Count = new TLLong(recentResults[i].Count.Value + 1);

                            var newPosition = i;
                            for (var j = i - 1; j >= 0; j--)
                            {
                                if (recentResults[j].Count.Value <= recentResults[i].Count.Value)
                                {
                                    newPosition = j;
                                }
                            }

                            if (i != newPosition)
                            {
                                recentResults.RemoveAt(i);
                                recentResults.Insert(newPosition, recentResult);
                            }
                            isAdded = true;
                            break;
                        }
                    }

                    if (!isAdded)
                    {
                        var recentResult = new TLResultInfo
                        {
                            Id = id,
                            Type = type,
                            Count = new TLLong(maxCount)
                        };

                        for (var i = 0; i < recentResults.Count; i++)
                        {
                            if (recentResults[i].Count.Value <= maxCount)
                            {
                                recentResults.Insert(i, recentResult);
                                isAdded = true;
                                break;
                            }
                        }

                        if (!isAdded)
                        {
                            recentResults.Add(recentResult);
                        }
                    }
                }

                _recentResults = recentResults;

                TLUtils.SaveObjectToMTProtoFile(_recentSyncRoot, Constants.RecentSearchResultsFileName, recentResults);
            });
        }

        private static TLString GetType(TLObject with)
        {
            return with is TLUserBase ? new TLString("user") : new TLString("chat");
        }

        private static TLInt GetId(TLObject with)
        {
            var user = with as TLUserBase;
            if (user != null)
            {
                return user.Id;
            }
            var chat = with as TLChatBase;
            if (chat != null)
            {
                return chat.Id;
            }

            return null;
        }

        public void ClearSearchHistory()
        {
            var result = MessageBox.Show(AppResources.ClearSearchHistoryConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                ClearRecent();
            }
        }
        #endregion

        public void Handle(DownloadableItem item)
        {
            BeginOnUIThread(() =>
            {
                var photo = item.Owner as TLUserProfilePhoto;
                if (photo != null)
                {
                    var user = (TLUserBase)Items.FirstOrDefault(x => x is TLUserBase && ((TLUserBase)x).Photo == photo);
                    if (user != null)
                    {
                        user.NotifyOfPropertyChange(() => user.Photo);
                        return;
                    }

                    var dialog = Items.FirstOrDefault(x => x is TLDialogBase && ((TLDialogBase)x).With is TLUserBase && ((TLUserBase)((TLDialog)x).With).Photo == photo);
                    if (dialog != null)
                    {
                        dialog.NotifyOfPropertyChange(() => ((TLDialogBase)dialog).With);
                        return;
                    }
                }

                var chatPhoto = item.Owner as TLChatPhoto;
                if (chatPhoto != null)
                {
                    var chat = (TLChat)Items.FirstOrDefault(x => x is TLChat && ((TLChat)x).Photo == chatPhoto);
                    if (chat != null)
                    {
                        chat.NotifyOfPropertyChange(() => chat.Photo);
                        return;
                    }

                    var dialog = Items.FirstOrDefault(x => x is TLDialogBase && ((TLDialogBase)x).With is TLChat && ((TLChat)((TLDialog)x).With).Photo == chatPhoto);
                    if (dialog != null)
                    {
                        dialog.NotifyOfPropertyChange(() => ((TLDialogBase)dialog).With);
                        return;
                    }
                }

                var channelPhoto = item.Owner as TLChatPhoto;
                if (channelPhoto != null)
                {

                    var channel = (TLChannel)Items.FirstOrDefault(x => x is TLChannel && ((TLChannel)x).Photo == channelPhoto);
                    if (channel != null)
                    {
                        channel.NotifyOfPropertyChange(() => channel.Photo);
                        return;
                    }

                    var dialog = Items.FirstOrDefault(x => x is TLDialogBase && ((TLDialogBase)x).With is TLChannel && ((TLChannel)((TLDialog)x).With).Photo == channelPhoto);
                    if (dialog != null)
                    {
                        dialog.NotifyOfPropertyChange(() => ((TLDialogBase)dialog).With);
                        return;
                    }
                    return;
                }
            });
        }

        public void Handle(ClearTopPeersEventArgs args)
        {
            TopPeers.Clear();
        }
    }

    public class SearchRequest
    {
        public bool IsCanceled;

        public string TransliterateText { get; private set; }

        public string Text { get; private set; }

        public IList<TLUserBase> UsersSource { get; private set; }

        public IList<TLChatBase> ChatsSource { get; private set; }

        public IList<TLObject> Results { get; private set; }

        public Dictionary<int, TLUserBase> UserResultsIndex { get; private set; }

        public Dictionary<int, TLChatBase> ChatResultsIndex { get; private set; } 

        public IList<TLObject> GlobalResults { get; set; }

        public IList<TLObject> MessageResults { get; set; }

        public int Offset { get; set; }

        public int Limit { get { return 20; } }

        public bool IsLastSliceLoaded { get; set; }

        private readonly ICacheService _cacheService;

        public SearchRequest(ICacheService cacheService, string text, IList<TLUserBase> usersSource, IList<TLChatBase> chatsSource)
        {
            _cacheService = cacheService;
            Text = text;
            TransliterateText = Language.Transliterate(text);
            UsersSource = usersSource;
            ChatsSource = chatsSource;
        }

        private static bool IsUserValid(TLUserBase contact, string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            return contact.FirstName.ToString().StartsWith(text, StringComparison.OrdinalIgnoreCase)
                || contact.LastName.ToString().StartsWith(text, StringComparison.OrdinalIgnoreCase)
                || contact.FullName.StartsWith(text, StringComparison.OrdinalIgnoreCase)
                || (contact.FullNameWords != null && contact.FullNameWords.Any(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool IsChatValid(TLChatBase chat, string text, bool useFastSearch)
        {
            if (string.IsNullOrEmpty(text)) return false;

            if (!useFastSearch)
            {
                var fullName = chat.FullName;

                var i = fullName.IndexOf(text, StringComparison.OrdinalIgnoreCase);
                if (i != -1)
                {
                    while (i < fullName.Length && i != -1)
                    {
                        if (i == 0 || (i > 0 && fullName[i - 1] == ' '))
                        {
                            return true;
                        }
                        if (fullName.Length > i + 1)
                        {
                            i = fullName.IndexOf(text, i + 1, StringComparison.OrdinalIgnoreCase);
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
                    && chat.FullNameWords.Any(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUsernameValid(IUserName userNameContact, string text)
        {
            if (text.Length >= Constants.UsernameMinLength)
            {
                if (userNameContact != null)
                {
                    var userName = userNameContact.UserName != null ? userNameContact.UserName.ToString() : string.Empty;
                    if (userName.StartsWith(text.TrimStart('@'), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ProcessAsync(Action<IList<TLObject>> callback)
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
                foreach (var contact in usersSource)
                {
                    if (IsUserValid(contact, Text)
                        || IsUserValid(contact, TransliterateText)
                        || IsUsernameValid(contact as IUserName, Text))
                    {
                        userResults.Add(contact);
                    }
                }

                var chatsResults = new List<TLChatBase>(chatsSource.Count);
                foreach (var chat in chatsSource)
                {
                    if (IsChatValid(chat, Text, useFastSearch)
                        || IsChatValid(chat, TransliterateText, useFastSearch)
                        || IsUsernameValid(chat as IUserName, Text))
                    {
                        var channel = chat as TLChannel;
                        if (channel != null && channel.Left.Value)
                        {
                            continue;
                        }

                        var channelForbidden = chat as TLChannelForbidden;
                        if (channelForbidden != null)
                        {
                            continue;
                        }

                        var chat41 = chat as TLChat41;
                        if (chat41 != null && chat41.IsMigrated)
                        {
                            continue;
                        }

                        chatsResults.Add(chat);
                    }
                }

                Results = new List<TLObject>(userResults.Count + chatsResults.Count);
                UserResultsIndex = new Dictionary<int, TLUserBase>();
                ChatResultsIndex = new Dictionary<int, TLChatBase>();
                foreach (var userResult in userResults)
                {
                    Results.Add(userResult);
                    UserResultsIndex[userResult.Index] = userResult;
                    if (userResult.Dialog == null)
                    {
                        userResult.Dialog = _cacheService.GetDialog(new TLPeerUser { Id = userResult.Id });
                    }
                }
                foreach (var chatResult in chatsResults)
                {
                    Results.Add(chatResult);
                    ChatResultsIndex[chatResult.Index] = chatResult;
                    if (chatResult.Dialog == null)
                    {
                        var dialog = _cacheService.GetDialog(new TLPeerChat { Id = chatResult.Id });

                        chatResult.Dialog = dialog;
                    }
                }

                Execute.BeginOnUIThread(() => callback.SafeInvoke(Results));
            });
        }

        public void Cancel()
        {
            IsCanceled = true;
        }

        public void LoadNextSlice(System.Action callback)
        {
        }
    }
}
