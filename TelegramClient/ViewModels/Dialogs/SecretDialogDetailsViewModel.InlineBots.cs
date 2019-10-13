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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Logs;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views.Dialogs;
using Execute = Caliburn.Micro.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    partial class SecretDialogDetailsViewModel
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
            var chat = Chat as TLEncryptedChat17;
            if (chat == null || chat.Layer.Value < Constants.MinSecretChatWithInlineBotsLayer) return;

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
            if (InlineBotResults != null && InlineBotResults.Results.Count > 0)
            {
                InlineBotResults.Results.Clear();

                //View.ResumeChatPlayers();
            }

            _currentInlineBotResults = null;
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

        private bool _inlineBotsNotification;

        private readonly object _inlineBotsNotificationSyncRoot = new object();

        private void CheckInlineBotsNotification()
        {
            if (_inlineBotsNotification) return;

            _inlineBotsNotification = TLUtils.OpenObjectFromMTProtoFile<TLBool>(_inlineBotsNotificationSyncRoot, Constants.InlineBotsNotificationFileName) != null;

            if (!_inlineBotsNotification)
            {
                MessageBox.Show(AppResources.InlineBotsNotification, AppResources.AppName, MessageBoxButton.OK);

                TLUtils.SaveObjectToMTProtoFile(_inlineBotsNotificationSyncRoot, Constants.InlineBotsNotificationFileName, TLBool.True);

                _inlineBotsNotification = true;
            }
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
                        CheckInlineBotsNotification();

                        if (CurrentInlineBot != currentInlineBot) return;
                        if (_currentText != currentText) return;

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

                    CheckInlineBotsNotification();

                    var geoPoint = GetGeoPoint(user45);

                    IsWorking = true;
                    //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("messages.getInlineBotResults username={0} query={1}", currentInlineBot.UserName, currentText));
                    MTProtoService.GetInlineBotResultsAsync(currentInlineBot.ToInputUser(), new TLInputPeerSelf(), geoPoint, new TLString(currentText), TLString.Empty,
                        result => BeginOnUIThread(() =>
                        {
                            IsWorking = false;

                            foreach (var inlineResult in result.Results)
                            {
                                inlineResult.QueryId = result.QueryId;
                            }

                            ClearUnsupportedResults(result);

                            _cachedInlineBotResults[string.Format("{0}_{1}", username, currentText)] = result;

                            if (CurrentInlineBot != currentInlineBot) return;
                            if (_currentText != currentText) return;

                            CreateInlineBotResults();

                            InlineBotResults.Gallery = result.Gallery;
                            var cachedResult51 = cachedResult as TLBotResults51;
                            if (cachedResult51 != null)
                            {
                                InlineBotResults.SwitchPM = cachedResult51.SwitchPM;
                            }
                            for (var i = 0; i < result.Results.Count; i++)
                            {
                                if (i == InlineBotMaxResults) break;
                                InlineBotResults.Results.Add(result.Results[i]);
                            }

                            if (InlineBotResults.Results.Count > 0) View.PauseChatPlayers();

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

        private static void ClearUnsupportedResults(TLBotResults result)
        {
            for (var i = 0; i < result.Results.Count; i++)
            {
                if (TLString.Equals(result.Results[i].Type, new TLString("audio"), StringComparison.OrdinalIgnoreCase))
                {
                    result.Results.RemoveAt(i--);
                }
                else if (TLString.Equals(result.Results[i].Type, new TLString("voice"), StringComparison.OrdinalIgnoreCase))
                {
                    result.Results.RemoveAt(i--);
                }
                else if (TLString.Equals(result.Results[i].Type, new TLString("sticker"), StringComparison.OrdinalIgnoreCase))
                {
                    result.Results.RemoveAt(i--);
                }
                else if (TLString.Equals(result.Results[i].Type, new TLString("file"), StringComparison.OrdinalIgnoreCase))
                {
                    result.Results.RemoveAt(i--);
                }
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
                            var result = MessageBox.Show(AppResources.LocationServicesDisabled, AppResources.AppName, MessageBoxButton.OKCancel);
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
            //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("messages.getInlineBotResults username={0} query={1}", currentInlineBot.UserName, currentText));
            MTProtoService.GetInlineBotResultsAsync(currentInlineBot.ToInputUser(), new TLInputPeerSelf(), geoPoint, new TLString(currentText), currentInlineBotResults.NextOffset,
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

        public async void SendBotInlineResult(TLBotInlineResultBase result)
        {
            var currentInlineBot = CurrentInlineBot;
            if (currentInlineBot == null) return;

            var inlineBots = DialogDetailsViewModel.GetInlineBots();
            if (!inlineBots.Contains(currentInlineBot))
            {
                inlineBots.Insert(0, currentInlineBot);
                //_cachedUsernameResults.Clear();
            }
            else
            {
                inlineBots.Remove(currentInlineBot);
                inlineBots.Insert(0, currentInlineBot);
                //_cachedUsernameResults.Clear();
            }

            DialogDetailsViewModel.SaveInlineBotsAsync();

            if (_currentInlineBotResults == null) return;
            var queryId = _currentInlineBotResults.QueryId;

            ClearInlineBotResults();
            CurrentInlineBot = null;
            NotifyOfPropertyChange(() => BotInlinePlaceholder);
            Text = string.Empty;

            BeginOnUIThread(async () =>
            {
                var chat = Chat as TLEncryptedChat;
                if (chat == null) return;

                var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, new TLDecryptedMessageMediaEmpty(), chat, GetDelaySeq(result));

                var processed = await ProcessBotInlineResult(decryptedTuple, result, currentInlineBot, ContinueProcessBotInlineResult);

                if (processed == null) return;

                Items.Insert(0, decryptedTuple.Item1);

                NotifyOfPropertyChange(() => DescriptionVisibility);

                BeginOnUIThread(() =>
                {
                    ProcessScroll();
                    View.ResumeChatPlayers();
                });

                if (processed == true)
                {
                    SendEncrypted(chat, decryptedTuple.Item2, MTProtoService, CacheService);
                }
                else
                {
                    CacheService.SyncDecryptedMessage(decryptedTuple.Item1, Chat, cachedMessage => { });
                }
            });
        }

        private bool GetDelaySeq(TLBotInlineResultBase result)
        {
            if (result.SendMessage is TLBotInlineMessageMediaContact)
            {
                return false;
            }
            if (result.SendMessage is TLBotInlineMessageMediaVenue)
            {
                return false;
            }
            if (result.SendMessage is TLBotInlineMessageMediaGeo)
            {
                return false;
            }

            return true;
        }

        private async void ContinueProcessBotInlineResult(Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject> tuple, TLInt fileSize, string fileName)
        {
            var sourceFileName = fileName;

            var dcId = TLInt.Random();
            var id = TLLong.Random();
            var accessHash = TLLong.Random();

            var fileLocation = new TLEncryptedFile
            {
                Id = id,
                AccessHash = accessHash,
                DCId = dcId,
                Size = fileSize,
                KeyFingerprint = new TLInt(0)
            };

            var destinationFileExt = Path.GetExtension(sourceFileName);
            var destinationFileName = String.Format("{0}_{1}_{2}{3}",
                fileLocation.Id,
                fileLocation.DCId,
                fileLocation.AccessHash,
                destinationFileExt);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                store.CopyFile(sourceFileName, destinationFileName);
            }

            var decryptedMessage = tuple.Item1 as TLDecryptedMessage;
            if (decryptedMessage == null) return;

            var mediaPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                if (mediaPhoto.ThumbW.Value == mediaPhoto.ThumbH.Value
                    && mediaPhoto.ThumbW.Value == 0)
                {
                    var preview = await GetLocalFilePreviewBytes(fileName);
                    if (preview != null)
                    {
                        mediaPhoto.Thumb = preview.Bytes;
                        mediaPhoto.ThumbH = preview.H;
                        mediaPhoto.ThumbW = preview.W;
                    }
                }

                mediaPhoto.File = fileLocation;

                var storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(destinationFileName);
                if (storageFile == null) return;
                Execute.BeginOnUIThread(() =>
                {
                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.ThumbSelf);
                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.Self);

                    mediaPhoto.DownloadingProgress = 0.0;
                    mediaPhoto.UploadingProgress = 0.001;
                    UploadDocumentFileManager.UploadFile(fileLocation.Id, tuple.Item2, storageFile, mediaPhoto.Key, mediaPhoto.IV);
                });
            }

            var mediaDocument = decryptedMessage.Media as TLDecryptedMessageMediaDocument;
            if (mediaDocument != null)
            {
                if (mediaDocument.ThumbW.Value == mediaDocument.ThumbH.Value
                    && mediaDocument.ThumbW.Value == 0)
                {
                    if (fileName.EndsWith("png") || fileName.EndsWith(".jpg"))
                    {
                        var preview = await GetLocalFilePreviewBytes(fileName);
                        if (preview != null)
                        {
                            mediaDocument.Thumb = preview.Bytes;
                            mediaDocument.ThumbH = preview.H;
                            mediaDocument.ThumbW = preview.W;
                        }
                    }
                }

                mediaDocument.File = fileLocation;

                var storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(destinationFileName);
                if (storageFile == null) return;
                Execute.BeginOnUIThread(() =>
                {
                    mediaDocument.NotifyOfPropertyChange(() => mediaDocument.Document);
                    mediaDocument.NotifyOfPropertyChange(() => mediaDocument.Self);

                    mediaDocument.DownloadingProgress = 0.0;
                    mediaDocument.UploadingProgress = 0.001;
                    UploadDocumentFileManager.UploadFile(fileLocation.Id, tuple.Item2, storageFile, mediaDocument.Key, mediaDocument.IV);
                });
            }
        }

        private async void ResendInlineBotResult(TLDecryptedMessage45 message, Action<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>, TLInt, string> downloadFileCallback)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null)
            {
                message.Status = MessageStatus.Failed;
                message.NotifyOfPropertyChange(() => message.Status);
                return;
            }

            var tuple = GetDecryptedMessageAndObject(message, chat);
            if (tuple == null)
            {
                message.Status = MessageStatus.Failed;
                message.NotifyOfPropertyChange(() => message.Status);
                return;
            }

            var resultBase = message.InlineBotResult;

            var mediaResultDocument = resultBase as TLBotInlineMediaResult;
            if (mediaResultDocument != null && mediaResultDocument.Document != null)
            {
                var document = mediaResultDocument.Document as TLDocument22;
                if (document == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);
                    return;
                }

                var fileLocation = document.ToInputFileLocation();
                if (fileLocation == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);
                    return;
                }

                var mediaDocument = message.Media as TLDecryptedMessageMediaDocument45;
                if (mediaDocument == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);
                    return;
                }

                var fileExtension = Path.GetExtension(document.FileName.ToString());
                var fileName = string.Format("document{0}_{1}{2}", fileLocation.Id, fileLocation.AccessHash, fileExtension);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(fileName))
                    {
                        message.Media.DownloadingProgress = 0.001;
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            var owner = mediaDocument;
                            var fileManager = IoC.Get<IDocumentFileManager>();
                            fileManager.DownloadFileAsync(
                                document.FileName,
                                document.DCId,
                                document.ToInputFileLocation(),
                                owner,
                                document.Size,
                                null,
                                item => downloadFileCallback.SafeInvoke(tuple, mediaDocument.Size, fileName));
                        });
                    }
                    else
                    {
                        downloadFileCallback.SafeInvoke(tuple, mediaDocument.Size, fileName);
                    }
                }
            }

            var mediaResultPhoto = resultBase as TLBotInlineMediaResult;
            if (mediaResultPhoto != null)
            {
                var photo = mediaResultPhoto.Photo as TLPhoto;
                if (photo == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);
                    return;
                }

                var maxSize = GetPhotoMaxSize(photo);
                if (maxSize == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);
                    return;
                }

                var maxSizeLocation = maxSize.Location as TLFileLocation;
                if (maxSizeLocation == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);
                    return;
                }

                var mediaPhoto = message.Media as TLDecryptedMessageMediaPhoto;
                if (mediaPhoto == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);
                    return;
                }

                var fileName = String.Format("{0}_{1}_{2}.jpg",
                    maxSizeLocation.VolumeId,
                    maxSizeLocation.LocalId,
                    maxSizeLocation.Secret);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(fileName))
                    {
                        message.Media.DownloadingProgress = 0.001;
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            var owner = photo;
                            var fileManager = IoC.Get<IFileManager>();
                            fileManager.DownloadFile(maxSizeLocation, owner, mediaPhoto.Size, item => downloadFileCallback.SafeInvoke(tuple, mediaPhoto.Size, fileName));
                        });
                    }
                    else
                    {
                        downloadFileCallback.SafeInvoke(tuple, mediaPhoto.Size, fileName);
                    }
                }
            }

            var result = resultBase as TLBotInlineResult;
            if (result != null)
            {
                if (TLString.Equals(result.Type, new TLString("gif"), StringComparison.OrdinalIgnoreCase))
                {
                    var contentUrl = result.ContentUrl;
                    if (contentUrl == null)
                    {
                        message.Status = MessageStatus.Failed;
                        message.NotifyOfPropertyChange(() => message.Status);
                        return;
                    }

                    var mediaDocument = message.Media as TLDecryptedMessageMediaDocument45;
                    if (mediaDocument == null)
                    {
                        message.Status = MessageStatus.Failed;
                        message.NotifyOfPropertyChange(() => message.Status);
                        return;
                    }

                    var previewFileName = result.ThumbUrlString ?? result.ContentUrlString;
                    if (previewFileName == null)
                    {
                        message.Status = MessageStatus.Failed;
                        message.NotifyOfPropertyChange(() => message.Status);
                        return;
                    }

                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                    var httpFileManager = IoC.Get<IHttpDocumentFileManager>();
                    var previewFileExtension = Path.GetExtension(previewFileName);
                    var destinationPreviewFileName = string.Format("preview{0}_{1}{2}", result.Id, result.QueryId, previewFileExtension);
                    if (!store.FileExists(destinationPreviewFileName))
                    {
                        httpFileManager.DownloadFileAsync(previewFileName, destinationPreviewFileName, result,
                            async item =>
                            {
                                var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                                if (cachedSize != null)
                                {
                                    mediaDocument.Thumb = cachedSize.Bytes;
                                    mediaDocument.ThumbW = cachedSize.W;
                                    mediaDocument.ThumbH = cachedSize.H;
                                }

                                Execute.BeginOnUIThread(() =>
                                {
                                    mediaDocument.NotifyOfPropertyChange(() => mediaDocument.Document);
                                });
                            },
                            error =>
                            {

                            });
                    }
                    else
                    {
                        var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                        if (cachedSize != null)
                        {
                            mediaDocument.Thumb = cachedSize.Bytes;
                            mediaDocument.ThumbW = cachedSize.W;
                            mediaDocument.ThumbH = cachedSize.H;
                        }
                    }

                    var contentFileName = result.ContentUrlString ?? result.ThumbUrlString;
                    if (contentFileName == null)
                    {
                        message.Status = MessageStatus.Failed;
                        message.NotifyOfPropertyChange(() => message.Status);
                        return;
                    }

                    var maxSizeLocation = new TLFileLocation
                    {
                        DCId = new TLInt(1),
                        VolumeId = TLLong.Random(),
                        LocalId = TLInt.Random(),
                        Secret = TLLong.Random()
                    };

                    var maxSize = new TLPhotoSize
                    {
                        Type = TLString.Empty,
                        Location = maxSizeLocation,
                        W = result.W,
                        H = result.H,
                        Size = new TLInt(0)
                    };

                    var fileExtension = Path.GetExtension(contentUrl.ToString());
                    var fileName = String.Format("{0}_{1}_{2}{3}",
                        maxSizeLocation.VolumeId,
                        maxSizeLocation.LocalId,
                        maxSizeLocation.Secret,
                        fileExtension);

                    var destinationContentFileName = string.Format("content{0}_{1}{2}", result.Id, result.QueryId, fileExtension);
                    //if (string.Equals(contentFileName, previewFileName, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    store.CopyFile(destinationPreviewFileName, fileName);

                    //    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                    //    {
                    //        mediaDocument.Size = new TLInt((int)file.Length);
                    //        maxSize.Size = new TLInt((int)file.Length);
                    //    }

                    //    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                    //}
                    //else
                    {
                        if (!store.FileExists(destinationContentFileName))
                        {
                            mediaDocument.DownloadingProgress = 0.001;
                            httpFileManager.DownloadFileAsync(contentFileName, destinationContentFileName, result,
                                item =>
                                {
                                    store.CopyFile(item.IsoFileName, fileName);
                                    store.DeleteFile(item.IsoFileName);

                                    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                                    {
                                        mediaDocument.Size = new TLInt((int)file.Length);
                                        maxSize.Size = new TLInt((int)file.Length);
                                    }

                                    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                                },
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    message.Status = MessageStatus.Failed;
                                    message.NotifyOfPropertyChange(() => message.Status);

                                    mediaDocument.DownloadingProgress = 0.0;
                                }));
                        }
                        else
                        {
                            store.CopyFile(destinationContentFileName, fileName);
                            store.DeleteFile(destinationContentFileName);

                            using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                mediaDocument.Size = new TLInt((int)file.Length);
                                maxSize.Size = new TLInt((int)file.Length);
                            }

                            downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                        }
                    }
                }
                else if (TLString.Equals(result.Type, new TLString("photo"), StringComparison.OrdinalIgnoreCase))
                {
                    var mediaPhoto = message.Media as TLDecryptedMessageMediaPhoto45;
                    if (mediaPhoto == null)
                    {
                        message.Status = MessageStatus.Failed;
                        message.NotifyOfPropertyChange(() => message.Status);
                        return;
                    }

                    var previewFileName = result.ThumbUrlString ?? result.ContentUrlString;
                    if (previewFileName == null)
                    {
                        message.Status = MessageStatus.Failed;
                        message.NotifyOfPropertyChange(() => message.Status);
                        return;
                    }

                    var destinationPreviewFileName = string.Format("preview{0}_{1}.jpg", result.Id, result.QueryId);
                    var httpFileManager = IoC.Get<IHttpDocumentFileManager>();
                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                    if (!store.FileExists(destinationPreviewFileName))
                    {
                        httpFileManager.DownloadFileAsync(previewFileName, destinationPreviewFileName, result,
                            async item =>
                            {
                                var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                                if (cachedSize != null)
                                {
                                    mediaPhoto.Thumb = cachedSize.Bytes;
                                    mediaPhoto.ThumbW = cachedSize.W;
                                    mediaPhoto.ThumbH = cachedSize.H;
                                }

                                Execute.BeginOnUIThread(() =>
                                {
                                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.ThumbSelf);
                                });
                            },
                            error =>
                            {

                            });
                    }
                    else
                    {
                        var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                        if (cachedSize != null)
                        {
                            mediaPhoto.Thumb = cachedSize.Bytes;
                            mediaPhoto.ThumbW = cachedSize.W;
                            mediaPhoto.ThumbH = cachedSize.H;
                        }
                    }

                    var contentFileName = result.ContentUrlString ?? result.ThumbUrlString;
                    if (contentFileName == null)
                    {
                        message.Status = MessageStatus.Failed;
                        message.NotifyOfPropertyChange(() => message.Status);
                        return;
                    }

                    var maxSizeLocation = new TLFileLocation
                    {
                        DCId = new TLInt(1),
                        VolumeId = TLLong.Random(),
                        LocalId = TLInt.Random(),
                        Secret = TLLong.Random()
                    };

                    var maxSize = new TLPhotoSize
                    {
                        Type = TLString.Empty,
                        Location = maxSizeLocation,
                        W = result.W,
                        H = result.H,
                        Size = new TLInt(0)
                    };

                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        maxSizeLocation.VolumeId,
                        maxSizeLocation.LocalId,
                        maxSizeLocation.Secret);

                    var destinationContentFileName = string.Format("content{0}_{1}.jpg", result.Id, result.QueryId);
                    //if (string.Equals(contentFileName, previewFileName, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    store.CopyFile(destinationPreviewFileName, fileName);

                    //    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                    //    {
                    //        mediaPhoto.Size = new TLInt((int) file.Length);
                    //        maxSize.Size = new TLInt((int) file.Length);
                    //    }

                    //    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                    //}
                    //else
                    {
                        if (!store.FileExists(destinationContentFileName))
                        {
                            mediaPhoto.DownloadingProgress = 0.001;
                            httpFileManager.DownloadFileAsync(contentFileName, destinationContentFileName, result,
                                item =>
                                {
                                    store.CopyFile(item.IsoFileName, fileName);
                                    store.DeleteFile(item.IsoFileName);

                                    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                                    {
                                        mediaPhoto.Size = new TLInt((int)file.Length);
                                        maxSize.Size = new TLInt((int)file.Length);
                                    }

                                    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                                },
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    message.Status = MessageStatus.Failed;
                                    message.NotifyOfPropertyChange(() => message.Status);

                                    mediaPhoto.DownloadingProgress = 0.0;
                                }));
                        }
                        else
                        {
                            store.CopyFile(destinationContentFileName, fileName);
                            store.DeleteFile(destinationContentFileName);

                            using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                mediaPhoto.Size = new TLInt((int)file.Length);
                                maxSize.Size = new TLInt((int)file.Length);
                            }

                            downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                        }
                    }
                }
            }
        }

        private static async Task<bool?> ProcessBotInlineResult(Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject> tuple, TLBotInlineResultBase resultBase, TLUserBase bot, Action<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>, TLInt, string> downloadFileCallback)
        {
            var processed = true;
            var message = tuple.Item1 as TLDecryptedMessage45;
            if (message == null) return null;

            var userName = bot as IUserName;
            if (userName != null)
            {
                message.ViaBotName = userName.UserName;
            }
            
            message.InlineBotResult = resultBase;


            var botInlineMessageMediaVenue = resultBase.SendMessage as TLBotInlineMessageMediaVenue;
            var botInlineMessageMediaGeo = resultBase.SendMessage as TLBotInlineMessageMediaGeo;
            if (botInlineMessageMediaVenue != null)
            {
                var geoPoint = botInlineMessageMediaVenue.Geo as TLGeoPoint;

                message.Media = new TLDecryptedMessageMediaVenue
                {
                    Title = botInlineMessageMediaVenue.Title,
                    Address = botInlineMessageMediaVenue.Address,
                    Provider = botInlineMessageMediaVenue.Provider,
                    VenueId = botInlineMessageMediaVenue.VenueId,
                    Lat = geoPoint != null ? geoPoint.Lat : new TLDouble(0.0),
                    Long = geoPoint != null ? geoPoint.Long : new TLDouble(0.0),
                };
                message.SetMedia();
            }
            else if (botInlineMessageMediaGeo != null)
            {
                var geoPoint = botInlineMessageMediaGeo.Geo as TLGeoPoint;

                message.Media = new TLDecryptedMessageMediaGeoPoint
                {
                    Lat = geoPoint != null ? geoPoint.Lat : new TLDouble(0.0),
                    Long = geoPoint != null ? geoPoint.Long : new TLDouble(0.0),
                };
                message.SetMedia();
            }

            var botInlineMessageMediaContact = resultBase.SendMessage as TLBotInlineMessageMediaContact;
            if (botInlineMessageMediaContact != null)
            {
                message.Media = new TLDecryptedMessageMediaContact
                {
                    PhoneNumber = botInlineMessageMediaContact.PhoneNumber,
                    FirstName = botInlineMessageMediaContact.FirstName,
                    LastName = botInlineMessageMediaContact.LastName,
                    UserId = new TLInt(0)
                };
                message.SetMedia();
            }

            var mediaResultDocument = resultBase as TLBotInlineMediaResult;
            if (mediaResultDocument != null && mediaResultDocument.Document != null)
            {
                var document = mediaResultDocument.Document as TLDocument22;
                if (document != null)
                {
                    var fileLocation = document.ToInputFileLocation();
                    if (fileLocation == null) return null;

                    var keyIV = GenerateKeyIV();
                    var mediaDocument = new TLDecryptedMessageMediaDocument45
                    {
                        Thumb = TLString.Empty,
                        ThumbW = new TLInt(0),
                        ThumbH = new TLInt(0),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        MimeType = document.MimeType,
                        Size = document.Size,
                        Attributes = document.Attributes,
                        Caption = TLString.Empty,
                    };

                    var inlinePreviewSize = document.Thumb as IPhotoSize;
                    if (inlinePreviewSize != null)
                    {
                        var preview = await GetLocalFilePreview(inlinePreviewSize);
                        if (preview != null)
                        {
                            mediaDocument.Thumb = preview.Bytes;
                            mediaDocument.ThumbH = preview.H;
                            mediaDocument.ThumbW = preview.W;
                        }
                    }

                    message.Media = mediaDocument;
                    message.SetMedia();

                    var fileExtension = Path.GetExtension(document.FileName.ToString());
                    var fileName = string.Format("document{0}_{1}{2}", fileLocation.Id, fileLocation.AccessHash, fileExtension);

                    processed = false;
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName))
                        {
                            message.Media.DownloadingProgress = 0.001;
                            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                            {
                                var owner = mediaDocument;
                                var fileManager = IoC.Get<IDocumentFileManager>();
                                fileManager.DownloadFileAsync(
                                    document.FileName,
                                    document.DCId,
                                    document.ToInputFileLocation(),
                                    owner,
                                    document.Size,
                                    null,
                                    item => downloadFileCallback.SafeInvoke(tuple, mediaDocument.Size, fileName));
                            });
                        }
                        else
                        {
                            downloadFileCallback.SafeInvoke(tuple, mediaDocument.Size, fileName);
                        }
                    }
                }
            }

            var mediaResultPhoto = resultBase as TLBotInlineMediaResult;
            if (mediaResultPhoto != null && mediaResultPhoto.Photo != null)
            {
                var photo = mediaResultPhoto.Photo as TLPhoto;
                if (photo != null)
                {
                    var maxSize = GetPhotoMaxSize(photo);
                    if (maxSize == null) return null;

                    var keyIV = GenerateKeyIV();
                    var mediaPhoto = new TLDecryptedMessageMediaPhoto45
                    {
                        Thumb = TLString.Empty,
                        ThumbW = new TLInt(0),
                        ThumbH = new TLInt(0),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        W = maxSize.W,
                        H = maxSize.H,
                        Size = maxSize.Size,
                        Caption = TLString.Empty,
                    };

                    var inlinePreviewSize = photo.Sizes.FirstOrDefault(x => x is TLPhotoCachedSize) as IPhotoSize ??
                                            InlineBotResultPhotoConverter.GetPhotoSize(photo);
                    if (inlinePreviewSize != null)
                    {
                        var preview = await GetLocalFilePreview(inlinePreviewSize);
                        if (preview != null)
                        {
                            mediaPhoto.Thumb = preview.Bytes;
                            mediaPhoto.ThumbH = preview.H;
                            mediaPhoto.ThumbW = preview.W;
                        }
                    }  

                    var maxSizeLocation = maxSize.Location as TLFileLocation;
                    if (maxSizeLocation == null) return null;
                    
                    message.Media = mediaPhoto;
                    message.SetMedia();

                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        maxSizeLocation.VolumeId,
                        maxSizeLocation.LocalId,
                        maxSizeLocation.Secret);

                    processed = false;
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName))
                        {
                            message.Media.DownloadingProgress = 0.001;
                            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                            {
                                var owner = photo;
                                var fileManager = IoC.Get<IFileManager>();
                                fileManager.DownloadFile(maxSizeLocation, owner, maxSize.Size, item => downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName));
                            });
                        }
                        else
                        {
                            downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                        }
                    }
                }
            }

            var result = resultBase as TLBotInlineResult;
            if (result != null)
            {
                if (TLString.Equals(result.Type, new TLString("gif"), StringComparison.OrdinalIgnoreCase))
                {
                    var contentUrl = result.ContentUrl;
                    if (contentUrl == null) return null;

                    var keyIV = GenerateKeyIV();
                    var mediaDocument = new TLDecryptedMessageMediaDocument45
                    {
                        Thumb = TLString.Empty,
                        ThumbW = new TLInt(0),
                        ThumbH = new TLInt(0),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        MimeType = result.ContentType,
                        Size = new TLInt(1),
                        Attributes = new TLVector<TLDocumentAttributeBase>(),
                        Caption = TLString.Empty,
                    };
                    if (result.W != null && result.H != null && result.W.Value > 0 && result.H.Value > 0)
                    {
                        var duration = result.Duration ?? new TLInt(0);

                        mediaDocument.Attributes.Add(new TLDocumentAttributeVideo { W = result.W, H = result.H, Duration = duration });
                        mediaDocument.Attributes.Add(new TLDocumentAttributeAnimated());
                        mediaDocument.Attributes.Add(new TLDocumentAttributeFileName{ FileName = new TLString(Path.GetFileName(contentUrl.ToString())) });
                    }

                    var previewFileName = result.ThumbUrlString ?? result.ContentUrlString;
                    if (previewFileName == null) return null;

                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                    var httpFileManager = IoC.Get<IHttpDocumentFileManager>();
                    var previewFileExtension = Path.GetExtension(previewFileName);
                    var destinationPreviewFileName = string.Format("preview{0}_{1}{2}", result.Id, result.QueryId, previewFileExtension);
                    if (!store.FileExists(destinationPreviewFileName))
                    {
                        httpFileManager.DownloadFileAsync(previewFileName, destinationPreviewFileName, result,
                            async item =>
                            {
                                var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                                if (cachedSize != null)
                                {
                                    mediaDocument.Thumb = cachedSize.Bytes;
                                    mediaDocument.ThumbW = cachedSize.W;
                                    mediaDocument.ThumbH = cachedSize.H;
                                }

                                Execute.BeginOnUIThread(() =>
                                {
                                    message.NotifyOfPropertyChange(() => message.MediaSelf);
                                });
                            },
                            error =>
                            {

                            });
                    }
                    else
                    {
                        var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                        if (cachedSize != null)
                        {
                            mediaDocument.Thumb = cachedSize.Bytes;
                            mediaDocument.ThumbW = cachedSize.W;
                            mediaDocument.ThumbH = cachedSize.H;
                        }
                    }

                    var contentFileName = result.ContentUrlString ?? result.ThumbUrlString;
                    if (contentFileName == null) return null;

                    processed = false;
                    message.Media = mediaDocument;
                    message.SetMedia();

                    var maxSizeLocation = new TLFileLocation
                    {
                        DCId = new TLInt(1),
                        VolumeId = TLLong.Random(),
                        LocalId = TLInt.Random(),
                        Secret = TLLong.Random()
                    };

                    var maxSize = new TLPhotoSize
                    {
                        Type = TLString.Empty,
                        Location = maxSizeLocation,
                        W = result.W,
                        H = result.H,
                        Size = new TLInt(0)
                    };

                    var fileExtension = Path.GetExtension(contentUrl.ToString());
                    var fileName = String.Format("{0}_{1}_{2}{3}",
                        maxSizeLocation.VolumeId,
                        maxSizeLocation.LocalId,
                        maxSizeLocation.Secret,
                        fileExtension);

                    var destinationContentFileName = string.Format("content{0}_{1}{2}", result.Id, result.QueryId, fileExtension);
                    //if (string.Equals(contentFileName, previewFileName, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    store.CopyFile(destinationPreviewFileName, fileName);

                    //    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                    //    {
                    //        mediaDocument.Size = new TLInt((int)file.Length);
                    //        maxSize.Size = new TLInt((int)file.Length);
                    //    }

                    //    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                    //}
                    //else
                    {
                        if (!store.FileExists(destinationContentFileName))
                        {
                            mediaDocument.DownloadingProgress = 0.001;
                            httpFileManager.DownloadFileAsync(contentFileName, destinationContentFileName, result,
                                item =>
                                {
                                    store.CopyFile(item.IsoFileName, fileName);
                                    store.DeleteFile(item.IsoFileName);

                                    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                                    {
                                        mediaDocument.Size = new TLInt((int)file.Length);
                                        maxSize.Size = new TLInt((int)file.Length);
                                    }

                                    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                                },
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    message.Status = MessageStatus.Failed;
                                    message.NotifyOfPropertyChange(() => message.Status);

                                    mediaDocument.DownloadingProgress = 0.0;
                                }));
                        }
                        else
                        {
                            store.CopyFile(destinationContentFileName, fileName);
                            store.DeleteFile(destinationContentFileName);

                            using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                mediaDocument.Size = new TLInt((int)file.Length);
                                maxSize.Size = new TLInt((int)file.Length);
                            }

                            downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                        }
                    }
                }
                else if (TLString.Equals(result.Type, new TLString("photo"), StringComparison.OrdinalIgnoreCase))
                {
                    var keyIV = GenerateKeyIV();
                    var mediaPhoto = new TLDecryptedMessageMediaPhoto45
                    {
                        Thumb = TLString.Empty,
                        ThumbW = new TLInt(0),
                        ThumbH = new TLInt(0),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        W = result.W,
                        H = result.H,
                        Size = new TLInt(1),
                        Caption = TLString.Empty,
                    };

                    var previewFileName = result.ThumbUrlString ?? result.ContentUrlString;
                    if (previewFileName == null) return null;

                    var destinationPreviewFileName = string.Format("preview{0}_{1}.jpg", result.Id, result.QueryId);
                    var httpFileManager = IoC.Get<IHttpDocumentFileManager>();
                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                    if (!store.FileExists(destinationPreviewFileName))
                    {
                        httpFileManager.DownloadFileAsync(previewFileName, destinationPreviewFileName, result,
                            async item =>
                            {
                                var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                                if (cachedSize != null)
                                {
                                    mediaPhoto.Thumb = cachedSize.Bytes;
                                    mediaPhoto.ThumbW = cachedSize.W;
                                    mediaPhoto.ThumbH = cachedSize.H;
                                }

                                Execute.BeginOnUIThread(() =>
                                {
                                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.ThumbSelf);
                                });
                            },
                            error =>
                            {

                            });
                    }
                    else
                    {
                        var cachedSize = await GetLocalFilePreviewBytes(destinationPreviewFileName);
                        if (cachedSize != null)
                        {
                            mediaPhoto.Thumb = cachedSize.Bytes;
                            mediaPhoto.ThumbW = cachedSize.W;
                            mediaPhoto.ThumbH = cachedSize.H;
                        }
                    }

                    var contentFileName = result.ContentUrlString ?? result.ThumbUrlString;
                    if (contentFileName == null) return null;

                    processed = false;
                    message.Media = mediaPhoto;
                    message.SetMedia();

                    var maxSizeLocation = new TLFileLocation
                    {
                        DCId = new TLInt(1),
                        VolumeId = TLLong.Random(),
                        LocalId = TLInt.Random(),
                        Secret = TLLong.Random()
                    };

                    var maxSize = new TLPhotoSize
                    {
                        Type = TLString.Empty,
                        Location = maxSizeLocation,
                        W = result.W,
                        H = result.H,
                        Size = new TLInt(0)
                    };

                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        maxSizeLocation.VolumeId,
                        maxSizeLocation.LocalId,
                        maxSizeLocation.Secret);

                    var destinationContentFileName = string.Format("content{0}_{1}.jpg", result.Id, result.QueryId);
                    //if (string.Equals(contentFileName, previewFileName, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    store.CopyFile(destinationPreviewFileName, fileName);

                    //    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                    //    {
                    //        mediaPhoto.Size = new TLInt((int) file.Length);
                    //        maxSize.Size = new TLInt((int) file.Length);
                    //    }

                    //    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                    //}
                    //else
                    {
                        if (!store.FileExists(destinationContentFileName))
                        {
                            mediaPhoto.DownloadingProgress = 0.001;
                            httpFileManager.DownloadFileAsync(contentFileName, destinationContentFileName, result,
                                item =>
                                {
                                    store.CopyFile(item.IsoFileName, fileName);
                                    store.DeleteFile(item.IsoFileName);

                                    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                                    {
                                        mediaPhoto.Size = new TLInt((int)file.Length);
                                        maxSize.Size = new TLInt((int)file.Length);
                                    }

                                    downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                                },
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    message.Status = MessageStatus.Failed;
                                    message.NotifyOfPropertyChange(() => message.Status);

                                    mediaPhoto.DownloadingProgress = 0.0;
                                }));
                        }
                        else
                        {
                            store.CopyFile(destinationContentFileName, fileName);
                            store.DeleteFile(destinationContentFileName);

                            using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                mediaPhoto.Size = new TLInt((int)file.Length);
                                maxSize.Size = new TLInt((int)file.Length);
                            }

                            downloadFileCallback.SafeInvoke(tuple, maxSize.Size, fileName);
                        }
                    }
                }
            }

            var messageText = resultBase.SendMessage as TLBotInlineMessageText;
            if (messageText != null)
            {
                message.Message = messageText.Message;

                var message45 = message;
                message45.Entities = messageText.Entities;
                if (messageText.NoWebpage)
                {

                }
            }

            var mediaAuto = resultBase.SendMessage as TLBotInlineMessageMediaAuto;
            if (mediaAuto != null)
            {
                var caption = message.Media as IMediaCaption;
                if (caption != null)
                {
                    caption.Caption = mediaAuto.Caption;
                }
            }

            return processed;
        }

        private static TLPhotoSize GetPhotoMaxSize(TLPhoto photo)
        {
            TLPhotoSize maxSize = null;
            var sizes = photo.Sizes.OfType<TLPhotoSize>();
            foreach (var photoSize in sizes)
            {
                if (maxSize == null
                    || maxSize.W.Value < photoSize.W.Value)
                {
                    maxSize = photoSize;
                }
            }
            
            return maxSize;
        }

        private static async Task<TLPhotoCachedSize> GetLocalFilePreview(IPhotoSize photoSize)
        {
            var cachedSize = photoSize as TLPhotoCachedSize;
            if (cachedSize != null) return cachedSize;

            var size = photoSize as TLPhotoSize;
            if (size != null)
            {
                var location = size.Location;
                if (location != null)
                {
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);

                    cachedSize = await GetLocalFilePreviewBytes(fileName);
                }
            }

            return cachedSize;
        }

        private static async Task<TLPhotoCachedSize> GetLocalFilePreviewBytes(string fileName)
        {
            TLPhotoCachedSize cachedSize = null;
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                {
                }
                else
                {
                    try
                    {
                        var file = await DialogDetailsViewModel.GetFileFromLocalFolder(fileName);
                        var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 90, ThumbnailOptions.ResizeThumbnail);
                        if (thumbnail != null)
                        {
                            var thumbnailStream = thumbnail.AsStream();
                            using (var ms = new MemoryStream())
                            {
                                thumbnailStream.CopyTo(ms);
                                cachedSize = new TLPhotoCachedSize
                                {
                                    Bytes = TLString.FromBigEndianData(ms.ToArray()),
                                    W = new TLInt((int) thumbnail.OriginalWidth),
                                    H = new TLInt((int) thumbnail.OriginalHeight),
                                    Type = new TLString("s"),
                                    Location =
                                        new TLFileLocationUnavailable
                                        {
                                            VolumeId = new TLLong(0),
                                            LocalId = new TLInt(0),
                                            Secret = new TLLong(0)
                                        }
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.ToString());
                    }
                }
            }

            return cachedSize;
        }
    }
}
