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
using Org.BouncyCastle.Security;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Search
{
    public class SearchContactsViewModel : ItemsViewModelBase<TLObject>, Telegram.Api.Aggregator.IHandle<DownloadableItem>
    {
        public bool IsNotWorking { get { return !IsWorking; } }

        public Visibility ProgressVisibility { get { return IsWorking ? Visibility.Visible : Visibility.Collapsed; } }

        private AddChatParticipantConfirmationViewModel _confirmation;

        public AddChatParticipantConfirmationViewModel Confirmation
        {
            get { return _confirmation = _confirmation ?? new AddChatParticipantConfirmationViewModel(); }
        }

        public bool RequestForwardingCount { get; protected set; }

        public bool NavigateToDialogDetails { get; protected set; }

        public bool NavigateToSecretChat { get; protected set; }

        public string Text { get; set; }

        public string TrimmedText { get; set; }

        private string _domain;

        private TLDHConfig _dhConfig;

        private TLUserBase _contact;

        private readonly TLChatBase _currentChat;

        private readonly TLChannelAdminRights _currentAdminRights;

        private readonly TLChannelParticipantRoleBase _currentRole;

        public TLUserBase Contact { get { return _contact; } }

        public SearchContactsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Items = new ObservableCollection<TLObject>();
            Status = string.Empty;

            _currentChat = StateService.CurrentChat;
            StateService.CurrentChat = null;

            if (StateService.CurrentAdminRights != null)
            {
                _currentAdminRights = StateService.CurrentAdminRights;
                StateService.CurrentAdminRights = null;
            }

            if (StateService.CurrentRole != null)
            {
                _currentRole = StateService.CurrentRole;
                StateService.CurrentRole = null;
            }

            if (StateService.RequestForwardingCount)
            {
                RequestForwardingCount = true;
                StateService.RequestForwardingCount = false;
            }

            if (StateService.NavigateToDialogDetails)
            {
                NavigateToDialogDetails = true;
                StateService.NavigateToDialogDetails = false;
            }

            if (StateService.NavigateToSecretChat)
            {
                NavigateToSecretChat = true;
                StateService.NavigateToSecretChat = false;
                _dhConfig = StateService.DHConfig;
                StateService.DHConfig = null;

                if (_dhConfig == null)
                {
                    Execute.BeginOnThreadPool(GetDHConfig);
                }
            }
        }

        public void CancelSecretChat()
        {
            _contact = null;

            IsWorking = false;
            NotifyOfPropertyChange(() => IsNotWorking);
            NotifyOfPropertyChange(() => ProgressVisibility);
        }

        private volatile bool _isGettingConfig;

        public void GetDHConfig()
        {
            _isGettingConfig = true;
            MTProtoService.GetDHConfigAsync(new TLInt(0), new TLInt(0),
                result =>
                {
                    var dhConfig = result as TLDHConfig;
                    if (dhConfig == null) return;
                    if (!TLUtils.CheckPrime(dhConfig.P.Data, dhConfig.G.Value))
                    {
                        return;
                    }

                    var aBytes = new byte[256];
                    var random = new SecureRandom();
                    random.NextBytes(aBytes);
                    var gaBytes = Telegram.Api.Services.MTProtoService.GetGB(aBytes, dhConfig.G, dhConfig.P);

                    dhConfig.A = TLString.FromBigEndianData(aBytes);
                    dhConfig.GA = TLString.FromBigEndianData(gaBytes);

                    _isGettingConfig = false;

                    Execute.BeginOnUIThread(() =>
                    {
                        _dhConfig = dhConfig;

                        if (_contact != null)
                        {
                            UserAction(_contact);
                        }
                    });
                },
                error =>
                {
                    _isGettingConfig = false; 
                    IsWorking = false;
                    NotifyOfPropertyChange(() => IsNotWorking);
                    NotifyOfPropertyChange(() => ProgressVisibility);
                    Execute.ShowDebugMessage("messages.getDhConfig error: " + error);
                });
        }


        protected override void OnActivate()
        {
            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            if (StateService.NavigateToSecretChat)
            {
                StateService.NavigateToSecretChat = false;
                NavigationService.RemoveBackEntry();
            }

            base.OnActivate();
        }

        public void UserAction(TLObject obj)
        {
            var chatBase = obj as TLChatBase;
            if (chatBase != null)
            {
                StateService.With = chatBase;
                StateService.AnimateTitle = true;
                NavigationService.UriFor<DialogDetailsViewModel>().Navigate();

                return;
            }

            var userBase = obj as TLUserBase;
            if (userBase == null) return;

            if (NavigateToSecretChat)
            {
                CreateSecretChatAction(userBase);
            }
            else if (NavigateToDialogDetails)
            {
                StateService.With = userBase;
                StateService.AnimateTitle = true;
                NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
            }
            else
            {
                var user = userBase as TLUser;
                if (user != null && user.IsBot)
                {
                    if (user.IsBotGroupsBlocked)
                    {
                        MessageBox.Show(AppResources.AddBotToGroupsError, AppResources.Error, MessageBoxButton.OK);
                        return;
                    }
                    var userName = user.FirstName;
                    if (TLString.IsNullOrEmpty(userName))
                    {
                        userName = user.LastName;
                    }
                    var confirmation = MessageBox.Show(string.Format(AppResources.AddUserToTheGroup, userName, _currentChat.FullName), AppResources.AppName, MessageBoxButton.OKCancel);
                    if (confirmation == MessageBoxResult.OK)
                    {
                        NavigateBackward(userBase);
                    }

                    return;
                }

                var channel = _currentChat as TLChannel;
                if (channel != null)
                {
                    IsWorking = true;
                    MTProtoService.GetParticipantAsync(channel.ToInputChannel(), userBase.ToInputUser(),
                        result => BeginOnUIThread(() =>
                        {
                            IsWorking = false;

                            var participantKicked = result.Participant as TLChannelParticipantBanned;
                            var participant = result.Participant as TLChannelParticipant;
                            if (participant != null || participantKicked != null)
                            {
                                //if (_currentRole is TLChannelRoleEditor)
                                {
                                    if (participantKicked != null)
                                    {
                                        var confirmation = channel.IsMegaGroup
                                            ? MessageBox.Show(string.Format(AppResources.InviteContactToGroupConfirmation, userBase.FullName), AppResources.AppName, MessageBoxButton.OKCancel)
                                            : MessageBox.Show(string.Format(AppResources.InviteContactConfirmation, userBase.FullName), AppResources.AppName, MessageBoxButton.OKCancel);
                                        if (confirmation != MessageBoxResult.OK)
                                        {
                                            return;
                                        }
                                    }

                                    NavigateBackward(userBase);
                                }
                            }
                        }),
                        error => BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            Execute.ShowDebugMessage("channels.getParticipant error " + error);

                            if (error.TypeEquals(ErrorType.USER_NOT_PARTICIPANT))
                            {
                                if (_currentAdminRights != null 
                                    && (_currentAdminRights.InviteUsers || _currentAdminRights.InviteLinks))
                                {
                                    var confirmation = channel.IsMegaGroup
                                        ? MessageBox.Show(string.Format(AppResources.InviteContactToGroupConfirmation, userBase.FullName), AppResources.AppName, MessageBoxButton.OKCancel)
                                        : MessageBox.Show(string.Format(AppResources.InviteContactConfirmation, userBase.FullName), AppResources.AppName, MessageBoxButton.OKCancel);
                                    if (confirmation != MessageBoxResult.OK)
                                    {
                                        return;
                                    }
                                }

                                NavigateBackward(userBase);
                            }
                        }));

                    return;
                }

                if (RequestForwardingCount)
                {
                    _confirmation.Open(userBase, _currentChat, 
                        result =>
                        {
                            if (result.Result == MessageBoxResult.OK)
                            {
                                NavigateBackward(userBase, result.ForwardingMessagesCount);
                            }
                        });
                }
                else
                {
                    NavigateBackward(userBase);
                }
            }
        }

        private void CreateSecretChatAction(TLUserBase user)
        {
            if (user == null) return;

            if (_dhConfig == null)
            {
                IsWorking = true;
                NotifyOfPropertyChange(() => IsNotWorking);
                NotifyOfPropertyChange(() => ProgressVisibility);
                _contact = user;

                //try to get dhConfig once again
                if (!_isGettingConfig)
                {
                    GetDHConfig();
                }

                return;
            }

            IsWorking = false;
            NotifyOfPropertyChange(() => IsNotWorking);
            NotifyOfPropertyChange(() => ProgressVisibility);

            AddSecretChatParticipantViewModel.CreateSecretChatCommon(user, _dhConfig, MTProtoService, CacheService, NavigationService, StateService, EventAggregator);
        }

        private void NavigateBackward(TLUserBase user, int forwardingMessagesCount = 0)
        {
            StateService.Participant = user;
            StateService.ForwardingMessagesCount = forwardingMessagesCount;
            NavigationService.GoBack();
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
                Status = string.IsNullOrEmpty(Text)? string.Empty : AppResources.NoResults;
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
                        CacheService.GetContacts()
                        .Where(x => !(x is TLUserEmpty) && x.Index != StateService.CurrentUserId)
                        .OrderBy(x => x.FullName)
                        .ToList();

                    source = _source;
                }

                nextUsersRequest = new SearchUsersRequest(text, source);
            }
            
            IsWorking = true;
            nextUsersRequest.ProcessAsync(results => 
                Execute.BeginOnUIThread(() =>
                {
                    if (nextUsersRequest.IsCanceled) return;

                    Status = string.Empty;
                    Items.Clear();
                    LazyItems.Clear();
                    if (results.Count > 0)
                    {
                        Items.Add(new TLServiceText { Text = AppResources.Contacts });
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
                        PopulateItems(() => ProcessGlobalSearch(nextUsersRequest));
                    }
                    else
                    {
                        ProcessGlobalSearch(nextUsersRequest);
                    }
                }));
            _searchResultsCache[nextUsersRequest.Text] = nextUsersRequest;
            _lastUsersRequest = nextUsersRequest;
        }

        private void ProcessGlobalSearch(SearchUsersRequest nextUsersRequest)
        {
            if (nextUsersRequest.GlobalResults != null)
            {
                if (nextUsersRequest.GlobalResults.Count > 0)
                {
                    BeginOnUIThread(() =>
                    {
                        if (nextUsersRequest.IsCanceled) return;

                        Items.Add(new TLServiceText { Text = AppResources.GlobalSearch });
                        foreach (var user in nextUsersRequest.GlobalResults)
                        {
                            Items.Add(user);
                        }
                        //Status = Items.Count == 0 ? AppResources.NoResults : string.Empty;
                    });
                }
            }
            else
            {
                //if (nextUsersRequest.Text.Length < 5)
                //{
                //    nextUsersRequest.GlobalResults = new List<TLObject>();
                //    return;
                //}


                MTProtoService.SearchAsync(new TLString(nextUsersRequest.Text), new TLInt(100),
                    result => Execute.BeginOnUIThread(() =>
                    {
                        nextUsersRequest.GlobalResults = new List<TLObject>();
                        foreach (var user in result.Users)
                        {
                            nextUsersRequest.GlobalResults.Add(user);
                        }
                        var contactsFound40 = result as TLContactsFound40;
                        if (contactsFound40 != null)
                        {
                            foreach (var chat in contactsFound40.Chats)
                            {
                                nextUsersRequest.GlobalResults.Add(chat);
                            }
                        }

                        if (nextUsersRequest.IsCanceled) return;

                        var items = new List<TLUserBase>();
                        foreach (var user in result.Users)
                        {
                            if (!nextUsersRequest.ResultsIndex.ContainsKey(user.Index))
                            {
                                items.Add(user);
                            }
                        }

                        if (items.Count > 0)
                        {
                            Items.Add(new TLServiceText { Text = AppResources.GlobalSearch });
                            foreach (var user in items)
                            {
                                Items.Add(user);
                            }
                        }
                        
                    }),
                    error =>
                    {
                        if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST)
                            && TLRPCError.TypeEquals(error, ErrorType.QUERY_TOO_SHORT))
                        {
                            nextUsersRequest.GlobalResults = new List<TLObject>();
                        }
                        else if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                        {
                            nextUsersRequest.GlobalResults = new List<TLObject>();
                            BeginOnUIThread(() => MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK));
                        }

                        Execute.ShowDebugMessage("contacts.search error " + error);
                    });
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
                    }
                    return;
                }
            });
        }
    }

    public class TLServiceText : TLObject
    {
        public string Text { get; set; }

        public string FullName { get { return Text; } }
    }

    public class SearchUsersRequest
    {
        public volatile bool IsCanceled;

        public string Text { get; private set; }

        public IList<TLUserBase> Source { get; private set; } 

        public IList<TLUserBase> Results { get; private set; }

        public Dictionary<int, TLUserBase> ResultsIndex { get; private set; }

        public IList<TLObject> GlobalResults { get; set; } 

        public SearchUsersRequest(string text, IList<TLUserBase> source)
        {
            Text = text;
            Source = source;
        }

        public void ProcessAsync(Action<IList<TLUserBase>> callback)
        {
            if (Results != null)
            {
                IsCanceled = false;
                callback.SafeInvoke(Results);
                return;
            }

            var source = Source;
            Execute.BeginOnThreadPool(() =>
            {
                var items = new List<TLUserBase>(source.Count);
                var dict = new Dictionary<int, TLUserBase>();
                foreach (var contact in source)
                {
                    var userNameContact = contact as IUserName;
                    var userName = userNameContact != null ? userNameContact.UserName.ToString() : null;
                    if (contact.FirstName.ToString().StartsWith(Text, StringComparison.OrdinalIgnoreCase)
                        || contact.LastName.ToString().StartsWith(Text, StringComparison.OrdinalIgnoreCase)
                        || contact.FullName.StartsWith(Text, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(userName, Text, StringComparison.OrdinalIgnoreCase))
                    {
                        items.Add(contact);
                        dict[contact.Index] = contact;
                    }
                }

                Results = items;
                ResultsIndex = dict;
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
