// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Helpers;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Dialogs;
using Execute = Caliburn.Micro.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public void OpenInlineBot(TLUserBase bot)
        {
            var view = GetView() as IDialogDetailsView;
            if (view != null) view.SetInlineBot(bot);
        }

        private string _botInlinePlaceholder;

        public string BotInlinePlaceholder
        {
            get { return _botInlinePlaceholder; }
            set { SetField(ref _botInlinePlaceholder, value, () => BotInlinePlaceholder); }
        }

        public TLUserBase _currentInlineBot;

        public TLUserBase CurrentInlineBot
        {
            get { return _currentInlineBot; }
            set
            {
                if (value != _currentInlineBot)
                {
                    _currentInlineBot = value;
                    SetBotInlinePlaceholder();
                }

                if (_currentInlineBot == null && _watcher != null)
                {
                    _watcher.Stop();
                    _disableWatching = true;
                }
            }
        }

        public void ResolveUsername(string text, string command = null)
        {
            if (IsEditingEnabled)
            {
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                CurrentInlineBot = null;

                return;
            }

            var username = new TLString(text.TrimStart('@'));

            var users = CacheService.GetUsers();
            for (var i = 0; i < users.Count; i++)
            {
                var user = users[i] as TLUser;
                if (user != null && user.IsInlineBot 
                    && TLString.Equals(user.UserName, username, StringComparison.OrdinalIgnoreCase))
                {
                    Execute.OnUIThread(() =>
                    {
                        if (!string.IsNullOrEmpty(command))
                        {
                            _currentInlineBot = user;
                            GetInlineBotResults(command);
                        }
                        else
                        {
                            CurrentInlineBot = user;
                            GetInlineBotResults(string.Empty);
                        }
                    });
                    return;
                }
            }

            CurrentInlineBot = null;
            NotifyOfPropertyChange(() => CurrentInlineBot);

            if (CurrentInlineBot == null)
            {
                IsWorking = true;
                MTProtoService.ResolveUsernameAsync(username,
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        if (!string.IsNullOrEmpty(command))
                        {
                            _currentInlineBot = result.Users.FirstOrDefault();
                            GetInlineBotResults(command);
                        }
                        else
                        {
                            CurrentInlineBot = result.Users.FirstOrDefault();
                            GetInlineBotResults(string.Empty);
                        }
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        Telegram.Api.Helpers.Execute.ShowDebugMessage("contacts.resolveUsername error " + error);
                    }));
            }
        }

        private bool SearchInlineBotResults(string text, out string searchText)
        {
            var searchInlineBotResults = false;
            searchText = string.Empty;
            if (CurrentInlineBot != null)
            {
                var user = CurrentInlineBot as IUserName;
                if (user != null)
                {
                    var username = user.UserName.ToString();
                    if (text != null
                        && text.TrimStart().StartsWith("@" + username, StringComparison.OrdinalIgnoreCase))
                    {
                        searchText = ReplaceFirst(text.TrimStart(), "@" + username, string.Empty);
                        if (searchText.StartsWith(" "))
                        {
                            searchText = ReplaceFirst(searchText, " ", string.Empty);
                            searchInlineBotResults = true;
                        }

                        if (!searchInlineBotResults)
                        {
                            if (string.Equals(text.TrimStart(), "@" + username, StringComparison.OrdinalIgnoreCase))
                            {
                                CurrentInlineBot = null;
                            }
                            else
                            {
                                SetBotInlinePlaceholder();
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(searchText))
                            {
                                SetBotInlinePlaceholder();
                            }
                            else
                            {
                                BotInlinePlaceholder = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        CurrentInlineBot = null;
                    }
                }
            }

            return searchInlineBotResults;
        }

        public void SetBotInlinePlaceholder()
        {
            var user45 = _currentInlineBot as TLUser45;
            if (user45 != null)
            {
                BotInlinePlaceholder = user45.BotInlinePlaceholder != null
                    ? user45.BotInlinePlaceholder.ToString()
                    : string.Empty;
            }
            else
            {
                BotInlinePlaceholder = string.Empty;
            }
        }

        public string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }


        public InlineBotResultsViewModel InlineBotResults { get; protected set; }

        private void CreateInlineBotResults()
        {
            if (InlineBotResults == null)
            {
                InlineBotResults = new InlineBotResultsViewModel(LoadNextInlineBotResults, Switch);
                NotifyOfPropertyChange(() => InlineBotResults);
            }
        }

        private void ClearInlineBotResults()
        {
            if (InlineBotResults != null)
            {
                InlineBotResults.SwitchPM = null;
                if (InlineBotResults.Results.Count > 0)
                {
                    InlineBotResults.Results.Clear();

                    //View.ResumeChatPlayers();
                }
            }

            _currentInlineBotResults = null;
        }
        
        private static readonly Dictionary<int, TLObject> _switchPMDict = new Dictionary<int, TLObject>();

        public void StartSwitchPMBotWithParam()
        {
            var switchPMWith = StateService.SwitchPMWith;
            StateService.SwitchPMWith = null;
            if (switchPMWith == null) return;

            var bot = With as TLUser;

            if (bot == null || !bot.IsBot || string.IsNullOrEmpty(bot.AccessToken)) return;

            _switchPMDict[bot.Index] = switchPMWith;

            var accessToken = new TLString(bot.AccessToken);
            bot.AccessToken = string.Empty;

            BeginOnUIThread(() =>
            {
                var text = new TLString("/start");

                var message = GetMessage(text, new TLMessageMediaEmpty());
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                NotifyOfPropertyChange(() => IsAppBarCommandVisible);

                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        result => StartBotInternal(result, accessToken)));
            });
        }

        public void Switch(TLInlineBotSwitchPM switchPM)
        {
            if (CurrentInlineBot == null) return;

            StateService.With = CurrentInlineBot;
            StateService.RemoveBackEntry = true;
            StateService.SwitchPMWith = With;
            StateService.Bot = CurrentInlineBot;
            StateService.AccessToken = switchPM.StartParam.ToString();
            NavigationService.UriFor<DialogDetailsViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
        }

        private readonly Dictionary<string, TLBotResults> _cachedInlineBotResults = new Dictionary<string, TLBotResults>();

        private const int InlineBotMaxResults = 5;

        private string _currentText;

        private TLBotResults _currentInlineBotResults;

        private GeoCoordinateWatcher _watcher;

        private string _readyText;

        private bool _suppressGetResultsOnReady;

        public void GetInlineBotResults(string text)
        {
            //Telegram.Api.Helpers.Execute.ShowDebugMessage("GetInlineBotResults text='" + text + "'");

            if (text == null) return;

            var currentInlineBot = CurrentInlineBot as TLUser;
            if (currentInlineBot == null) return;

            var username = currentInlineBot.UserName;
            if (TLString.IsNullOrEmpty(username)) return;

            var user45 = currentInlineBot as TLUser45;
            if (!CheckBotInlineGeoAccess(user45)) return;

            if (StartGeoWatching(text, user45)) return;

            var currentText = text;
            _currentText = text;

            ClearInlineBotResults();

            TLBotResults cachedResult;
            if (_cachedInlineBotResults.TryGetValue(string.Format("{0}_{1}", username, text), out cachedResult))
            {
                if (cachedResult != null)
                {
                    BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
                    {
                        if (CurrentInlineBot != currentInlineBot) return;
                        if (_currentText != currentText) return;

                        System.Diagnostics.Debug.WriteLine("    CachedResults location={0} text={1}", _watcher != null? _watcher.Position.Location : null, text);
                        CreateInlineBotResults();

                        InlineBotResults.Gallery = cachedResult.Gallery;
                        var cachedResult51 = cachedResult as TLBotResults51;
                        if (cachedResult51 != null)
                        {
                            InlineBotResults.SwitchPM = cachedResult51.SwitchPM;
                        }
                        for (var i = 0; i < cachedResult.Results.Count; i++)
                        {
                            if (i == InlineBotMaxResults) break;
                            InlineBotResults.Results.Add(cachedResult.Results[i]);
                        }

                        if (InlineBotResults.Results.Count > 0) View.PauseChatPlayers();

                        _currentInlineBotResults = cachedResult;
                    });
                }
            }
            else
            {
                BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
                {
                    if (CurrentInlineBot != currentInlineBot) return;
                    if (_currentText != currentText) return;

                    var geoPoint = GetGeoPoint(user45);

                    //System.Diagnostics.Debug.WriteLine("    GetInlineBotResults location={0} text={1}", geoPoint, text);
                    IsWorking = true;
                    //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("messages.getInlineBotResults username={0} query={1}", currentInlineBot.UserName, currentText));
                    MTProtoService.GetInlineBotResultsAsync(currentInlineBot.ToInputUser(), Peer, geoPoint, new TLString(currentText), TLString.Empty,
                        result => BeginOnUIThread(() =>
                        {
                            IsWorking = false;

                            CreateInlineBotResults();

                            if (CurrentInlineBot == currentInlineBot && _currentText == currentText)
                            {
                                InlineBotResults.Gallery = result.Gallery;
                                var cachedResult51 = result as TLBotResults51;
                                if (cachedResult51 != null)
                                {
                                    InlineBotResults.SwitchPM = cachedResult51.SwitchPM;
                                }
                                for (var i = 0; i < result.Results.Count; i++)
                                {
                                    if (i == InlineBotMaxResults) break;

                                    result.Results[i].QueryId = result.QueryId;
                                    InlineBotResults.Results.Add(result.Results[i]);
                                }

                                if (InlineBotResults.Results.Count > 0) View.PauseChatPlayers();
                            }

                            _currentInlineBotResults = result;

                            _cachedInlineBotResults[string.Format("{0}_{1}", username, currentText)] = result;
                        }),
                        error => BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.getInlineBotResults error " + error);
                        }));
                });
            }
        }

        private bool StartGeoWatching(string text, TLUser45 user45)
        {
            if (user45 != null && user45.IsBotInlineGeo && user45.BotInlineGeoAccess)
            {
                if (_watcher == null)
                {
                    _readyText = text;

                    _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                    _watcher.StatusChanged += (o, e) =>
                    {
                        if (_watcher.Status == GeoPositionStatus.Ready)
                        {
                            if (_suppressGetResultsOnReady)
                            {
                                _suppressGetResultsOnReady = false;
                                //System.Diagnostics.Debug.WriteLine("    ReGetInlineBotReesults suppress status={0} text={1}", _watcher.Status, text);
                                return;
                            }
                            //System.Diagnostics.Debug.WriteLine("    ReGetInlineBotReesults status={0} text={1}", _watcher.Status, text);
                            GetInlineBotResults(_readyText);
                            _readyText = null;
                        }
                        else if (_watcher.Status == GeoPositionStatus.Initializing)
                        {
                        }
                        else if (_watcher.Status == GeoPositionStatus.Disabled)
                        {
                            var result = MessageBox.Show(AppResources.LocationServicesDisabled, AppResources.AppName,
                                MessageBoxButton.OKCancel);
                            if (result == MessageBoxResult.OK)
                            {
#if WP8
                                Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
#endif
                            }
                        }
                    };
                    _watcher.Start(true);

                    return true;
                }

                if (_disableWatching)
                {
                    _disableWatching = false;

                    _watcher.Start(true);
                    if (_watcher.Position.Location != GeoCoordinate.Unknown)
                    {
                        _suppressGetResultsOnReady = true;
                    }
                }

                if (_watcher.Status == GeoPositionStatus.Initializing)
                {
                    if (_watcher.Position.Location == GeoCoordinate.Unknown)
                    {
                        //System.Diagnostics.Debug.WriteLine("    SetReadyText status={0} text={1}", _watcher.Status, text);
                        _readyText = text;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckBotInlineGeoAccess(TLUser45 user45)
        {
            if (user45 != null && user45.IsBotInlineGeo && !user45.BotInlineGeoAccess)
            {
                var now = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
                if (user45.NotifyGeoAccessDate == null
                    || user45.NotifyGeoAccessDate.Value + Constants.RecheckBotInlineGeoAccessInterval <= now.Value)
                {
                    user45.NotifyGeoAccessDate = now;

                    var result = MessageBox.Show(AppResources.ShareLocationInlineConfirmation, AppResources.ShareLocationCaption, MessageBoxButton.OKCancel);
                    if (result != MessageBoxResult.OK) return false;

                    user45.BotInlineGeoAccess = true;
                }
            }

            return true;
        }

        private TLInputGeoPointBase GetGeoPoint(TLUser45 user45)
        {
            TLInputGeoPointBase geoPoint = null;
            if (user45 != null && user45.IsBotInlineGeo)
            {
                geoPoint = new TLInputGeoPointEmpty();
                if (user45.BotInlineGeoAccess && _watcher != null && _watcher.Status == GeoPositionStatus.Ready && _watcher.Position.Location != GeoCoordinate.Unknown)
                {
                    geoPoint = new TLInputGeoPoint { Lat = new TLDouble(_watcher.Position.Location.Latitude), Long = new TLDouble(_watcher.Position.Location.Longitude) };
                }
            }
            return geoPoint;
        }

        public void ContinueInlineBotResults()
        {
            if (_currentInlineBotResults != null)
            {
                CreateInlineBotResults();

                if (InlineBotResults.Results.Count == InlineBotMaxResults)
                {
                    for (var i = InlineBotMaxResults; i < _currentInlineBotResults.Results.Count; i++)
                    {
                        _currentInlineBotResults.Results[i].QueryId = _currentInlineBotResults.QueryId;
                        InlineBotResults.Results.Add(_currentInlineBotResults.Results[i]);
                    }

                    if (InlineBotResults.Results.Count > 0) View.PauseChatPlayers();
                }
            }
        }

        private bool _isLoadingNextInlineBotResults;

        public void LoadNextInlineBotResults()
        {
            if (_isLoadingNextInlineBotResults) return;

            if (CurrentInlineBot == null) return;
            if (_currentInlineBotResults == null) return;
            if (_currentInlineBotResults.NextOffset == null) return;

            var currentInlineBot = CurrentInlineBot;
            var currentInlineBotResults = _currentInlineBotResults;
            var currentText = _currentText;

            var user = CurrentInlineBot as TLUser;
            if (user == null) return;

            var username = user.UserName;

            _isLoadingNextInlineBotResults = true;
            IsWorking = true;

            var geoPoint = GetGeoPoint(user as TLUser45);

            System.Diagnostics.Debug.WriteLine("GetInlineBotResults location={0} text={1}", geoPoint, currentText);
            //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("messages.getInlineBotResults username={0} query={1}", currentInlineBot.UserName, currentText));
            MTProtoService.GetInlineBotResultsAsync(currentInlineBot.ToInputUser(), Peer, geoPoint, new TLString(currentText), currentInlineBotResults.NextOffset,
                result => BeginOnUIThread(() =>
                {
                    _isLoadingNextInlineBotResults = false;
                    IsWorking = false;

                    if (CurrentInlineBot == currentInlineBot && currentInlineBotResults == _currentInlineBotResults && _currentText == currentText)
                    {
                        for (var i = 0; i < result.Results.Count; i++)
                        {
                            result.Results[i].QueryId = result.QueryId;
                            InlineBotResults.Results.Add(result.Results[i]);
                        }

                        if (InlineBotResults.Results.Count > 0) View.PauseChatPlayers();
                    }

                    TLBotResults cachedResult;
                    if (_cachedInlineBotResults.TryGetValue(string.Format("{0}_{1}", username, currentText), out cachedResult))
                    {
                        _cachedInlineBotResults[string.Format("{0}_{1}", username, currentText)] = MergeInlineBotResults(cachedResult, result);
                    }
                    else
                    {
                        _cachedInlineBotResults[string.Format("{0}_{1}", username, currentText)] = result;
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    _isLoadingNextInlineBotResults = false;
                    IsWorking = false;
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.getInlineBotResults error " + error);
                }));
        }

        private static TLBotResults MergeInlineBotResults(TLBotResults cachedResult, TLBotResults result)
        {
            //Telegram.Api.Helpers.Execute.ShowDebugMessage("MergeInlineBotResults " + cachedResult.QueryId + " " + result.QueryId);

            //if (cachedResult.QueryId.Value != result.QueryId.Value) return cachedResult;

            cachedResult.Flags = result.Flags;
            cachedResult.NextOffset = result.NextOffset;
            foreach (var resultItem in result.Results)
            {
                cachedResult.Results.Add(resultItem);
            }

            return cachedResult;
        }

        private static readonly object _inlineBotsSyncRoot = new object();

        private static IList<TLUserBase> _inlineBots;

        public static void SaveInlineBotsAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                var inlineBots = new TLVector<TLUserBase>(_inlineBots ?? new List<TLUserBase>());
                TLUtils.SaveObjectToMTProtoFile(_inlineBotsSyncRoot, Constants.InlineBotsFileName, inlineBots);
            });
        }

        public static IList<TLUserBase> GetInlineBots()
        {
            if (_inlineBots != null) return _inlineBots;

            var topPeers = IoC.Get<IStateService>().GetTopPeers() as TLTopPeers;
            if (topPeers != null)
            {
                var inlineBotsCategory = topPeers.Categories.FirstOrDefault(x => x.Category is TLTopPeerCategoryBotsInline);
                if (inlineBotsCategory != null)
                {
                    var inlineBots = new List<TLUserBase>();
                    foreach (var peer in inlineBotsCategory.Peers)
                    {
                        var user = IoC.Get<ICacheService>().GetUser(peer.Peer.Id);
                        if (user != null)
                        {
                            inlineBots.Add(user);
                        }
                    }

                    //if (inlineBots.Count > 0)
                    {
                        _inlineBots = inlineBots;

                        return _inlineBots;
                    }
                }
            }


            _inlineBots = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLUserBase>>(_inlineBotsSyncRoot, Constants.InlineBotsFileName) ?? new TLVector<TLUserBase>();

            return _inlineBots;
        }

        public static IList<TLUserBase> DeleteInlineBots()
        {
            _inlineBots = new TLVector<TLUserBase>();
            FileUtils.Delete(_inlineBotsSyncRoot, Constants.InlineBotsFileName);

            return _inlineBots;
        }

        public void SendBotInlineResult(TLBotInlineResultBase result)
        {
            _debugNotifyOfPropertyChanged = true;

            var currentInlineBot = CurrentInlineBot;
            if (currentInlineBot == null) return;

            var inlineBots = GetInlineBots();

            if (!inlineBots.Contains(currentInlineBot))
            {
                inlineBots.Insert(0, currentInlineBot);
                _cachedUsernameResults.Clear();
            }
            else
            {
                inlineBots.Remove(currentInlineBot);
                inlineBots.Insert(0, currentInlineBot);
                _cachedUsernameResults.Clear();
            }

            SaveInlineBotsAsync();

            if (_currentInlineBotResults == null) return;
            var queryId = _currentInlineBotResults.QueryId;

            ClearInlineBotResults();
            CurrentInlineBot = null;
            NotifyOfPropertyChange(() => BotInlinePlaceholder);
            Text = string.Empty;

            var message = GetMessage(TLString.Empty, new TLMessageMediaEmpty()) as TLMessage45;
            if (message == null) return;

            ProcessBotInlineResult(message, result, currentInlineBot.Id);

            if (message._media != null)
            {
                message.SetMedia();
            }

            if (Reply != null && IsWebPagePreview(Reply))
            {
                message._media = ((TLMessagesContainter)Reply).WebPageMedia;
                Reply = _previousReply;
            }

            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);

                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                var user = With as TLUser;
                if (user != null && user.IsBot && Items.Count == 1)
                {
                    NotifyOfPropertyChange(() => With);
                }

                BeginOnUIThread(() =>
                {
                    ProcessScroll();
                    View.ResumeChatPlayers();
                });

                _debugNotifyOfPropertyChanged = false;
                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        m => SendInternal(message, MTProtoService,
                            () => { },
                            () => Status = string.Empty)));
            });
        }

        private static bool BadInlineBotMessage(TLMessage45 message)
        {
            //var mediaDocument = message.Media as TLMessageMediaDocument;
            //if (mediaDocument != null)
            //{
            //    var document = mediaDocument.Document as TLDocument22;
            //    if (document != null && document.Size.Value == 0)
            //    {
            //        return true;
            //    }
            //}

            return false;
        }

        private void ProcessBotInlineResult(TLMessage45 message, TLBotInlineResultBase resultBase, TLInt botId)
        {
            message.InlineBotResultId = resultBase.Id;
            message.InlineBotResultQueryId = resultBase.QueryId;
            message.ViaBotId = botId;

            var botInlineMessageMediaVenue = resultBase.SendMessage as TLBotInlineMessageMediaVenue;
            var botInlineMessageMediaGeo = resultBase.SendMessage as TLBotInlineMessageMediaGeo;
            if (botInlineMessageMediaVenue != null)
            {
                message._media = new TLMessageMediaVenue72
                {
                    Title = botInlineMessageMediaVenue.Title,
                    Address = botInlineMessageMediaVenue.Address,
                    Provider = botInlineMessageMediaVenue.Provider,
                    VenueId = botInlineMessageMediaVenue.VenueId,
                    VenueType = TLString.Empty,
                    Geo = botInlineMessageMediaVenue.Geo
                };
            }
            else if (botInlineMessageMediaGeo != null)
            {
                message._media = new TLMessageMediaGeo { Geo = botInlineMessageMediaGeo.Geo };
            }

            var botInlineMessageMediaContact = resultBase.SendMessage as TLBotInlineMessageMediaContact;
            if (botInlineMessageMediaContact != null)
            {
                message._media = new TLMessageMediaContact82
                {
                    PhoneNumber = botInlineMessageMediaContact.PhoneNumber,
                    FirstName = botInlineMessageMediaContact.FirstName,
                    LastName = botInlineMessageMediaContact.LastName,
                    UserId = new TLInt(0),
                    VCard = TLString.Empty
                };
            }

            var mediaResult = resultBase as TLBotInlineMediaResult;
            if (mediaResult != null)
            {
                if (TLString.Equals(mediaResult.Type, new TLString("voice"), StringComparison.OrdinalIgnoreCase)
                    && mediaResult.Document != null)
                {
                    message._media = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = mediaResult.Document, Caption = TLString.Empty, NotListened = !(With is TLChannel) };

                    message.NotListened = !(With is TLChannel);
                }
                else if (TLString.Equals(mediaResult.Type, new TLString("audio"), StringComparison.OrdinalIgnoreCase)
                    && mediaResult.Document != null)
                {
                    message._media = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = mediaResult.Document, Caption = TLString.Empty };
                }
                else if (TLString.Equals(mediaResult.Type, new TLString("sticker"), StringComparison.OrdinalIgnoreCase)
                    && mediaResult.Document != null)
                {
                    message._media = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = mediaResult.Document, Caption = TLString.Empty };
                }
                else if (TLString.Equals(mediaResult.Type, new TLString("file"), StringComparison.OrdinalIgnoreCase)
                    && mediaResult.Document != null)
                {
                    message._media = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = mediaResult.Document, Caption = TLString.Empty };
                }
                else if (TLString.Equals(mediaResult.Type, new TLString("gif"), StringComparison.OrdinalIgnoreCase)
                    && mediaResult.Document != null)
                {
                    message._media = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = mediaResult.Document, Caption = TLString.Empty };
                }
                else if (TLString.Equals(mediaResult.Type, new TLString("photo"), StringComparison.OrdinalIgnoreCase)
                    && mediaResult.Photo != null)
                {
                    message._media = new TLMessageMediaPhoto75 { Flags = new TLInt(0), Photo = mediaResult.Photo, Caption = TLString.Empty };
                }
                else if (TLString.Equals(mediaResult.Type, new TLString("game"), StringComparison.OrdinalIgnoreCase))
                {
                    var game = new TLGame
                    {
                        Flags = new TLInt(0),
                        Id = new TLLong(0),
                        AccessHash = new TLLong(0),
                        ShortName = mediaResult.Id,
                        Title = mediaResult.Title ?? TLString.Empty,
                        Description = mediaResult.Description ?? TLString.Empty,
                        Photo = mediaResult.Photo ?? new TLPhotoEmpty {Id = new TLLong(0)},
                        Document = mediaResult.Document
                    };

                    message._media = new TLMessageMediaGame { Game = game, SourceMessage = message };
                }
            }

            var result = resultBase as TLBotInlineResult;
            if (result != null)
            {
                var isVoice = TLString.Equals(result.Type, new TLString("voice"), StringComparison.OrdinalIgnoreCase);
                var isAudio = TLString.Equals(result.Type, new TLString("audio"), StringComparison.OrdinalIgnoreCase);
                var isFile = TLString.Equals(result.Type, new TLString("file"), StringComparison.OrdinalIgnoreCase);

                if (isFile
                    || isAudio
                    || isVoice)
                {
                    var document = result.Document as TLDocument22;
                    if (document == null)
                    {
                        string fileName = null;
                        if (result.ContentUrl != null)
                        {
                            var fileUri = new Uri(result.ContentUrlString);
                            try
                            {
                                fileName = Path.GetFileName(fileUri.LocalPath);
                            }
                            catch (Exception ex)
                            {
                                
                            }

                            if (fileName == null)
                            {
                                fileName = "file.ext";
                            }
                        }

                        document = new TLDocument54
                        {
                            Id = new TLLong(0),
                            AccessHash = new TLLong(0),
                            Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                            FileName = new TLString(fileName),
                            MimeType = result.ContentType ?? TLString.Empty,
                            Size = new TLInt(0),
                            Thumb = new TLPhotoSizeEmpty { Type = TLString.Empty },
                            DCId = new TLInt(0),
                            Version = new TLInt(0)
                        };

                        if (isVoice || isAudio)
                        {
                            var documentAttributeAudio = new TLDocumentAttributeAudio46
                            {
                                Duration = result.Duration ?? new TLInt(0),
                                Title = result.Title ?? TLString.Empty,
                                Performer = null,
                                Voice = isVoice
                            };
                            document.Attributes.Add(documentAttributeAudio);
                        }

                        //message._status = MessageStatus.Failed;
                    }
                    var mediaDocument = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = document, Caption = TLString.Empty };

                    message._media = mediaDocument;

                    mediaDocument.NotListened = isVoice && !(With is TLChannel);
                    message.NotListened = isVoice && !(With is TLChannel);
                }
                else if (TLString.Equals(result.Type, new TLString("gif"), StringComparison.OrdinalIgnoreCase))
                {
                    var document = result.Document;
                    if (document != null)
                    {
                        var mediaDocument = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = document, Caption = TLString.Empty };

                        message._media = mediaDocument;
                    }
                }
                else if (TLString.Equals(result.Type, new TLString("photo"), StringComparison.OrdinalIgnoreCase))
                {
                    Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("w={0} h={1}\nthumb_url={2}\ncontent_url={3}", result.W, result.H, result.ThumbUrl, result.ContentUrl));

                    var location = new TLFileLocation{DCId = new TLInt(1), VolumeId = TLLong.Random(), LocalId = TLInt.Random(), Secret = TLLong.Random() };

                    var cachedSize = new TLPhotoCachedSize
                    {
                        Type = new TLString("s"),
                        W = result.W ?? new TLInt(90),
                        H = result.H ?? new TLInt(90),
                        Location = location,
                        Bytes = TLString.Empty,
                        TempUrl = result.ThumbUrlString ?? result.ContentUrlString
                        //Size = new TLInt(0)
                    };

                    var size = new TLPhotoSize
                    {
                        Type = new TLString("m"),
                        W = result.W ?? new TLInt(90),
                        H = result.H ?? new TLInt(90),
                        Location = location,
                        TempUrl = result.ContentUrlString,
                        Size = new TLInt(0)
                    };

                    if (!string.IsNullOrEmpty(result.ThumbUrlString))
                    {
                        //ServicePointManager.ServerCertificateValidationCallback += new 

                        var webClient = new WebClient();
                        webClient.OpenReadAsync(new Uri(result.ThumbUrlString, UriKind.Absolute));
                        webClient.OpenReadCompleted += (sender, args) =>
                        {
                            if (args.Cancelled) return;
                            if (args.Error != null) return;

                            var fileName = String.Format("{0}_{1}_{2}.jpg",
                                location.VolumeId,
                                location.LocalId,
                                location.Secret);

                            using (var stream = args.Result)
                            {
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(fileName)) return;
                                    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                                    {
                                        const int BUFFER_SIZE = 128*1024;
                                        var buf = new byte[BUFFER_SIZE];

                                        var bytesread = 0;
                                        while ((bytesread = stream.Read(buf, 0, BUFFER_SIZE)) > 0)
                                        {
                                            var position = stream.Position;
                                            stream.Position = position - 10;
                                            var tempBuffer = new byte[10];
                                            var resultOk = stream.Read(tempBuffer, 0, tempBuffer.Length);
                                            file.Write(buf, 0, bytesread);
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(result.ContentUrlString))
                            {
                                webClient.OpenReadAsync(new Uri(result.ContentUrlString, UriKind.Absolute));
                                webClient.OpenReadCompleted += (sender2, args2) =>
                                {
                                    if (args2.Cancelled) return;
                                    if (args2.Error != null) return;

                                    using (var stream = args2.Result)
                                    {
                                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                        {
                                            if (store.FileExists(fileName)) return;
                                            using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                                            {
                                                const int BUFFER_SIZE = 128 * 1024;
                                                var buf = new byte[BUFFER_SIZE];

                                                int bytesread = 0;
                                                while ((bytesread = stream.Read(buf, 0, BUFFER_SIZE)) > 0)
                                                {
                                                    file.Write(buf, 0, bytesread);
                                                }
                                            }
                                        }
                                    }
                                };
                            }
                        };
                    }

                    var photo = new TLPhoto56
                    {
                        Flags = new TLInt(0),
                        Id = TLLong.Random(),
                        AccessHash = TLLong.Random(),
                        Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                        Sizes = new TLVector<TLPhotoSizeBase> {cachedSize, size}
                    };

                    var mediaPhoto = new TLMessageMediaPhoto75 { Flags = new TLInt(0), Photo = photo, Caption = TLString.Empty };

                    message._media = mediaPhoto;
                }
            }

            var messageText = resultBase.SendMessage as TLBotInlineMessageText;
            if (messageText != null)
            {
                message.Message = messageText.Message;
                message.Entities = messageText.Entities;
                if (messageText.NoWebpage)
                {
                    
                }
            }

            var mediaAuto = resultBase.SendMessage as TLBotInlineMessageMediaAuto75;
            if (mediaAuto != null)
            {
                message.Message = mediaAuto.Caption;
                message.Entities = mediaAuto.Entities;
            }

            if (resultBase.SendMessage != null
                && resultBase.SendMessage.ReplyMarkup != null)
            {
                message.ReplyMarkup = resultBase.SendMessage.ReplyMarkup;
            }
        }
    }

    public class StartGifPlayerEventArgs : System.EventArgs
    {
        public TLMessage Message { get; set; }

        public TLDecryptedMessage DecryptedMessage { get; set; }

        public StartGifPlayerEventArgs(TLMessage message)
        {
            Message = message;
        }

        public StartGifPlayerEventArgs(TLDecryptedMessage decryptedMessage)
        {
            DecryptedMessage = decryptedMessage;
        }
    }
}
