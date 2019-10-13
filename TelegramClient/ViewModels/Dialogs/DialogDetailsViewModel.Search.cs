// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Telegram.Api.TL;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        private DialogSearchMessagesViewModel _searchMessages;

        public DialogSearchMessagesViewModel SearchMessages
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel SearchMessages");
                return _searchMessages;
            }
            protected set
            {
                _searchMessages = value;
            }
        }

        public void Search()
        {
            if (SearchMessages == null)
            {
                SearchMessages = new DialogSearchMessagesViewModel(With, Search, SearchUser, SearchUp, SearchDown, GetChatUsers);
                NotifyOfPropertyChange(() => SearchMessages);
            }

            BeginOnUIThread(() =>
            {
                if (!SearchMessages.IsOpen)
                {
                    _currentResultIndex = default(int);
                    _currentResults = null;

                    SearchMessages.Open();
                }
                else
                {
                    SearchMessages.Close();
                }
            });
        }

        private IList<TLUserBase> GetChatUsers()
        {
            var chat = With as TLChat41;
            if (chat != null)
            {
                var participants = chat.Participants as TLChatParticipants40;
                if (participants != null)
                {
                    var result = new List<TLUserBase>();
                    foreach (var participant in participants.Participants)
                    {
                        var user = CacheService.GetUser(participant.UserId);
                        if (user != null)
                        {
                            result.Add(user);
                        }
                    }

                    return result;
                }
            }

            var channel = With as TLChannel68;
            if (channel != null)
            {
                var participants = channel.ChannelParticipants as TLChannelParticipants;
                if (participants != null)
                {
                    var result = new List<TLUserBase>();
                    foreach (var participant in participants.Participants)
                    {
                        var user = CacheService.GetUser(participant.UserId);
                        if (user != null)
                        {
                            result.Add(user);
                        }
                    }

                    return result;
                }
            }

            return new List<TLUserBase>();
        }

        private void SearchUser(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                var users = GetChatUsers();
                SearchMessages.Hints.Clear();
                foreach (var user in users)
                {
                    SearchMessages.Hints.Add(user);
                }

                return;
            }

            var channel = With as TLChannel;
            if (channel == null || !channel.IsMegaGroup) return;

            if (SearchMessages.From != null) return;
            var key = GetKey(text, SearchMessages.Date, SearchMessages.From);
            //System.Diagnostics.Debug.WriteLine("key=" + key);

            BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                if (SearchMessages.From != null) return;
                var currentKey = GetKey(SearchMessages.Text, SearchMessages.Date, SearchMessages.From);
                //System.Diagnostics.Debug.WriteLine("current_key=" + currentKey + " key=" + key);
                if (!string.Equals(key, currentKey)) return;

                System.Diagnostics.Debug.WriteLine("channel.getPartisipants q=" + text);
                MTProtoService.GetParticipantsAsync(
                    channel.ToInputChannel(),
                    new TLChannelParticipantsSearch{ Q = new TLString(text)}, 
                    new TLInt(0), 
                    new TLInt(50),
                    new TLInt(0), 
                    result => Execute.BeginOnUIThread(() =>
                    {
                        var channelParticipants = result as TLChannelParticipants;
                        if (channelParticipants != null)
                        {
                            currentKey = GetKey(SearchMessages.Text, SearchMessages.Date, SearchMessages.From);
                            //System.Diagnostics.Debug.WriteLine("channel.getPartisipants q=" + text + " results=" + result.Users.Count + " current_key=" + currentKey);
                            if (!string.Equals(key, currentKey)) return;

                            SearchMessages.Hints.Clear();
                            foreach (var user in channelParticipants.Users)
                            {
                                SearchMessages.Hints.Add(user);
                            }
                        }
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        currentKey = GetKey(SearchMessages.Text, SearchMessages.Date, SearchMessages.From);
                        //System.Diagnostics.Debug.WriteLine("channel.getPartisipants q=" + text + " error current_key=" + currentKey);
                        if (!string.Equals(key, currentKey)) return;

                        SearchMessages.Hints.Clear();
                    }));

            });
        }

        private static readonly Dictionary<string, TLMessagesBase> _searchResults = new Dictionary<string, TLMessagesBase>();

        private int _currentResultIndex;

        private TLMessagesBase _currentResults;

        private void Search(string text, DateTime? date, TLUserBase from)
        {
            if (IsEmptyKey(text, date, from))
            {
                SearchMessages.ResultLoaded(0, 0);
                return;
            }

            var key = GetKey(text, date, from);

            BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
            {
                var currentKey = GetKey(SearchMessages.Text, SearchMessages.Date, SearchMessages.From);
                if (!string.Equals(key, currentKey)) return;


                TLMessagesBase cachedResults;
                if (_searchResults.TryGetValue(key, out cachedResults))
                {
                    ContinueSearch(cachedResults);

                    return;
                }

                SearchAsync(new TLString(text), date, from, new TLInt(0), new TLInt(Constants.SearchMessagesSliceLimit));
            });
        }

        private void SearchAsync(TLString text, DateTime? date, TLUserBase from, TLInt maxId, TLInt limit)
        {
            var maxDate = date != null
                ? TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, date.Value.AddDays(1).AddSeconds(-1))
                : null;

            if (TLString.IsNullOrEmpty(text) && date.HasValue)
            {
                IsWorking = true;
                MTProtoService.GetHistoryAsync(
                    Stopwatch.StartNew(), 
                    Peer, 
                    TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), 
                    false, maxDate ?? new TLInt(0), 
                    new TLInt(0), 
                    maxId, 
                    new TLInt(1), 
                    result => BeginOnUIThread(() =>
                    {
                        var messagesSlice = result as TLMessagesSlice;
                        if (messagesSlice != null)
                        {
                            messagesSlice.Count = new TLInt(messagesSlice.Messages.Count);
                        }

                        IsWorking = false;

                        ProcessMessages(result.Messages);

                        var key = GetKey(text.ToString(), date, from);

                        TLMessagesBase cachedResult;
                        if (_searchResults.TryGetValue(key, out cachedResult))
                        {
                            var lastId = cachedResult.Messages.Last().Id;
                            if (lastId.Value == maxId.Value)
                            {
                                var cachedUsersIndex = new Dictionary<int, int>();
                                foreach (var cachedUser in cachedResult.Users)
                                {
                                    cachedUsersIndex[cachedUser.Index] = cachedUser.Index;
                                }
                                foreach (var user in result.Users)
                                {
                                    if (!cachedUsersIndex.ContainsKey(user.Index))
                                    {
                                        cachedResult.Users.Add(user);
                                    }
                                }
                                var cachedChatsIndex = new Dictionary<int, int>();
                                foreach (var cachedChat in cachedResult.Chats)
                                {
                                    cachedChatsIndex[cachedChat.Index] = cachedChat.Index;
                                }
                                foreach (var chat in result.Chats)
                                {
                                    if (!cachedChatsIndex.ContainsKey(chat.Index))
                                    {
                                        cachedResult.Chats.Add(chat);
                                    }
                                }
                                foreach (var message in result.Messages)
                                {
                                    cachedResult.Messages.Add(message);
                                }

                                SearchMessages.ResultLoaded(_currentResultIndex, cachedResult.Messages.Count);
                            }
                        }
                        else
                        {
                            _searchResults[key] = result;
                            var currentKey = GetKey(SearchMessages.Text, SearchMessages.Date, SearchMessages.From); 
                            if (string.Equals(key, currentKey, StringComparison.Ordinal))
                            {
                                ContinueSearch(result);
                            }
                        }

                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Execute.ShowDebugMessage("messages.search error " + error);
                    }));
            }
            else
            {
                IsWorking = true;
                MTProtoService.SearchAsync(
                    Peer, 
                    text, 
                    from == null ? null : from.ToInputUser(),
                    new TLInputMessagesFilterEmpty(), 
                    new TLInt(0), 
                    maxDate ?? new TLInt(0), 
                    new TLInt(0), 
                    maxId, 
                    limit,
                    new TLInt(0),
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        ProcessMessages(result.Messages);

                        var key = GetKey(text.ToString(), date, from);

                        TLMessagesBase cachedResult;
                        if (_searchResults.TryGetValue(key, out cachedResult))
                        {
                            var lastId = cachedResult.Messages.Last().Id;
                            if (lastId.Value == maxId.Value)
                            {
                                var cachedUsersIndex = new Dictionary<int, int>();
                                foreach (var cachedUser in cachedResult.Users)
                                {
                                    cachedUsersIndex[cachedUser.Index] = cachedUser.Index;
                                }
                                foreach (var user in result.Users)
                                {
                                    if (!cachedUsersIndex.ContainsKey(user.Index))
                                    {
                                        cachedResult.Users.Add(user);
                                    }
                                }
                                var cachedChatsIndex = new Dictionary<int, int>();
                                foreach (var cachedChat in cachedResult.Chats)
                                {
                                    cachedChatsIndex[cachedChat.Index] = cachedChat.Index;
                                }
                                foreach (var chat in result.Chats)
                                {
                                    if (!cachedChatsIndex.ContainsKey(chat.Index))
                                    {
                                        cachedResult.Chats.Add(chat);
                                    }
                                }
                                foreach (var message in result.Messages)
                                {
                                    cachedResult.Messages.Add(message);
                                }

                                SearchMessages.ResultLoaded(_currentResultIndex, cachedResult.Messages.Count);
                            }
                        }
                        else
                        {
                            _searchResults[key] = result;
                            var currentKey = GetKey(SearchMessages.Text, SearchMessages.Date, SearchMessages.From);
                            if (string.Equals(key, currentKey, StringComparison.Ordinal))
                            {
                                ContinueSearch(result);
                            }
                        }

                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Execute.ShowDebugMessage("messages.search error " + error);
                    }));
            }
        }

        private static bool IsEmptyKey(string text, DateTime? date, TLUserBase from)
        {
            return string.IsNullOrEmpty(text) && (date == null || date == DateTime.Now.Date) && from == null;
        }

        private static string GetKey(string text, DateTime? date, TLUserBase from)
        {
            return string.Format("{0}_{1}_{2}", text, date == null || date == DateTime.Now.Date ? string.Empty : date.Value.ToString(), from != null ? from.Id.Value.ToString() : string.Empty);
        }

        private void ContinueSearch(TLMessagesBase result)
        {
            _currentResults = result;

            if (result.Messages.Count > 0)
            {
                _currentResults = result;

                LoadResult(0);
            }
            else
            {
                SearchMessages.ResultLoaded(0, 0);
            }
        }

        private void LoadResult(int resultIndex)
        {
            TLUtils.WriteLine(string.Format("LoadResult index={0}", resultIndex), LogSeverity.Error);

            _currentResultIndex = resultIndex;
            var message = _currentResults.Messages[_currentResultIndex];
            var nextMessage = _currentResults.Messages.Count > _currentResultIndex + 1? _currentResults.Messages[_currentResultIndex + 1] : null;

            Items.Clear();
            Items.Add(message);
            //HighlightMessage(message);
            LoadResultHistory(message);
            if (nextMessage != null)
            {
                PreloadResultHistory(nextMessage);
            }

            SliceLoaded = false;
            _isFirstSliceLoaded = false;
            _isLastMigratedHistorySliceLoaded = false;
            IsLastSliceLoaded = false;

            SearchMessages.ResultLoaded(resultIndex, _currentResults.Messages.Count);

            if (resultIndex >= _currentResults.Messages.Count - 3)
            {
                var messagesSlice = _currentResults as TLMessagesSlice;
                if (messagesSlice != null && messagesSlice.Count.Value > messagesSlice.Messages.Count)
                {
                    var maxId = messagesSlice.Messages.Last().Id;
                    SearchAsync(new TLString(SearchMessages.Text), SearchMessages.Date, SearchMessages.From, maxId, new TLInt(Constants.SearchMessagesSliceLimit));
                }
                else
                {
                    var channel = With as TLChannel;
                    if (channel != null && channel.MigratedFromChatId != null)
                    {
                        
                    }
                }
            }

            //if (ScrollToBottomVisibility == Visibility.Collapsed)
            //{
            //    Execute.BeginOnUIThread(() => ScrollToBottomVisibility = Visibility.Visible);
            //}
        }

        private static readonly Dictionary<int, IList<TLMessageBase>> _resultHistoryCache = new Dictionary<int, IList<TLMessageBase>>(); 

        private void LoadResultHistory(TLMessageBase message)
        {
            var maxId = message.Id;
            var offset = new TLInt(-10);
            var limit = new TLInt(Constants.MessagesSlice + 10);

            IList<TLMessageBase> resultHistory;
            if (_resultHistoryCache.TryGetValue(maxId.Value, out resultHistory))
            {
                ContinueLoadResultHistory(limit, maxId, resultHistory);

                return;
            }

            var messageCommon = message as TLMessageCommon;
            if (messageCommon != null)
            {
                if (Peer is TLInputPeerChannel)
                {
                    if (messageCommon.ToId is TLPeerChat)
                    {
                        var chat = CacheService.GetChat(messageCommon.ToId.Id);
                        if (chat != null)
                        {
                            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(), chat.ToInputPeer(), new TLPeerChat { Id = chat.Id }, false, new TLInt(0), offset, maxId, limit,
                                result =>
                                {
                                    ProcessMessages(result.Messages);

                                    BeginOnUIThread(() =>
                                    {
                                        _resultHistoryCache[maxId.Value] = result.Messages;
                                        if (_currentResults == null
                                            || _currentResults.Messages[_currentResultIndex] == message)
                                        {
                                            ContinueLoadResultHistory(limit, maxId, result.Messages);
                                        }
                                    });
                                },
                                error => BeginOnUIThread(() =>
                                {
                                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                                }));
                        }

                        return;
                    }
                }
            }

            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(), Peer, TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), false, new TLInt(0), offset, maxId, limit,
                result => 
                {
                    ProcessMessages(result.Messages);

                    BeginOnUIThread(() =>
                    {
                        _resultHistoryCache[maxId.Value] = result.Messages;
                        if (_currentResults == null
                            || _currentResults.Messages[_currentResultIndex] == message)
                        {
                            ContinueLoadResultHistory(limit, maxId, result.Messages);
                        }
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        private bool _loadMessageHistory;

        private TLInt _messageId;

        private Tuple<TLInt, TLInt, TLMessagesBase> _messageIdSlice;

        private void LoadResultHistory(TLInt messageId)
        {
            var maxId = messageId;
            var offset = new TLInt(-10);
            var limit = new TLInt(Constants.MessagesSlice + 10);

            _loadMessageHistory = true;
            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(), Peer, TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), false, new TLInt(0), offset, maxId, limit,
                result =>
                {
                    ProcessMessages(result.Messages);

                    BeginOnUIThread(() =>
                    {
                        _loadMessageHistory = false;
                        _resultHistoryCache[maxId.Value] = result.Messages;
                        if (_currentResults == null)
                        {
                            if (_isForwardInAnimationComplete)
                            {
                                _isUpdated = true;
                                _messageId = null;
                                ContinueLoadResultHistory(limit, maxId, result.Messages);
                            }
                            else
                            {
                                _messageIdSlice = new Tuple<TLInt, TLInt, TLMessagesBase>(maxId, limit, result);
                            }
                        }
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    _loadMessageHistory = false;

                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        private void PreloadResultHistory(TLMessageBase message)
        {
            var maxId = message.Id;
            var offset = new TLInt(-10);
            var limit = new TLInt(Constants.MessagesSlice + 10);

            IList<TLMessageBase> resultHistory;
            if (_resultHistoryCache.TryGetValue(maxId.Value, out resultHistory))
            {
                return;
            }

            var messageCommon = message as TLMessageCommon;
            if (messageCommon != null)
            {
                if (Peer is TLInputPeerChannel)
                {
                    if (messageCommon.ToId is TLPeerChat)
                    {
                        var chat = CacheService.GetChat(messageCommon.ToId.Id);
                        if (chat != null)
                        {
                            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(), chat.ToInputPeer(), new TLPeerChat { Id = chat.Id }, false, new TLInt(0), offset, maxId, limit,
                                result => 
                                {
                                    ProcessMessages(result.Messages);
                                    BeginOnUIThread(() =>
                                    {
                                        _resultHistoryCache[maxId.Value] = result.Messages;
                                    });
                                },
                                error => BeginOnUIThread(() =>
                                {
                                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                                }));
                        }

                        return;
                    }
                }
            }

            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(), Peer, TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), false, new TLInt(0), offset, maxId, limit,
                result =>
                {
                    ProcessMessages(result.Messages);
                    BeginOnUIThread(() =>
                    {
                        _resultHistoryCache[maxId.Value] = result.Messages;
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        private void ContinueLoadResultHistory(TLInt limit, TLInt maxId, IList<TLMessageBase> resultMessages)
        {
            var upperSlice = new List<TLMessageBase>();
            var bottomSlice = new List<TLMessageBase>();
            for (var i = 0; i < resultMessages.Count; i++)
            {
                var resultMessage = resultMessages[i];

                if (resultMessage.Index < maxId.Value)
                {
                    upperSlice.Add(resultMessage);
                }
                else if (resultMessage.Index == maxId.Value)
                {
                    if (Items.Count == 0)
                    {
                        upperSlice.Add(resultMessage);
                    }
                }
                else if (resultMessage.Index > maxId.Value)
                {
                    bottomSlice.Add(resultMessage);
                }
            }

            if (Items.Count <= 1)
            {
                var firstSliceCount = 10;
                foreach (var upperMessage in upperSlice.Take(firstSliceCount))
                {
                    upperMessage._isAnimated = false;//isAnimated;
                    if (!SkipMessage(upperMessage))
                    {
                        Items.Add(upperMessage);
                    }
                }
                if (bottomSlice.Count > 0 || upperSlice.Count > firstSliceCount)
                {
                    BeginOnUIThread(() =>
                    {
                        foreach (var upperMessage in upperSlice.Skip(firstSliceCount))
                        {
                            upperMessage._isAnimated = false;//isAnimated;
                            if (!SkipMessage(upperMessage))
                            {
                                Items.Add(upperMessage);
                            }
                        }

                        HoldScrollingPosition = true;
                        for (var i = bottomSlice.Count; i > 0; i--)
                        {
                            var message = bottomSlice[i - 1];
                            Items.Insert(0, message);
                        }
                        HoldScrollingPosition = false;
                    });
                }
            }

            if (limit.Value > upperSlice.Count + bottomSlice.Count)
            {
                AppendMigratedHistory(Items);
            }
        }

        private void SearchUp()
        {
            if (_currentResults == null) return;
            if (_currentResults.Messages.Count == _currentResultIndex + 1) return;

            BeginOnUIThread(() => LoadResult(_currentResultIndex + 1));
        }

        private void SearchDown()
        {
            if (_currentResults == null) return;
            if (_currentResultIndex <= 0) return;

            BeginOnUIThread(() => LoadResult(_currentResultIndex - 1));
        }
    }
}
