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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Org.BouncyCastle.Security;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.EmojiPanel;
using TelegramClient.Converters;
using TelegramClient.Views;
using TelegramClient.Views.Dialogs;
#if WP8
using System.Threading.Tasks;
using Windows.Storage;
#endif
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Extensions;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel :
        ItemsViewModelBase<TLDecryptedMessageBase>
    {
        private IDialogDetailsView _view;

        public IDialogDetailsView View
        {
            get
            {
                _view = _view ?? GetView() as IDialogDetailsView;

                return _view;
            }
        }

        private void ShowScrollToBottomButton()
        {
            if (View != null) View.ShowScrollToBottomButton();
        }

        private void HideScrollToBottomButton()
        {
            if (View != null) View.HideScrollToBottomButton();
        }

        public void AddToFavedStickers(TLDecryptedMessageBase messageBase)
        {
            if (messageBase == null) return;

            var message = messageBase as TLDecryptedMessage;
            if (message == null) return;

            var mediaDocument = message.Media as TLDecryptedMessageMediaExternalDocument;
            if (mediaDocument == null) return;

            //var document = mediaDocument.Document as TLDocument22;
            //if (document != null)
            //{
            //    var favedStickers = StateService.GetFavedStickers();
            //    if (favedStickers != null)
            //    {
            //        var exists = favedStickers.Documents.FirstOrDefault(x => x.Index == document.Index) != null;
            //        if (exists)
            //        {
            //            favedStickers.RemoveSticker(document);
            //        }
            //        else
            //        {
            //            favedStickers.AddSticker(document);
            //        }
            //        var allStickers = StateService.GetAllStickers();
            //        var allStickers43 = allStickers as TLAllStickers43;
            //        if (allStickers43 != null)
            //        {
            //            allStickers43.FavedStickers = favedStickers;
            //            StateService.SaveAllStickersAsync(allStickers43);
            //        }
            //        StateService.SaveFavedStickersAsync(favedStickers);

            //        EmojiControl emojiControl;
            //        if (EmojiControl.TryGetInstance(out emojiControl))
            //        {
            //            emojiControl.UpdateFavedStickers(document);
            //        }
            //    }
            //}
        }

        public void AddToStickers(TLDecryptedMessageBase messageBase)
        {
            if (messageBase == null) return;

            var message = messageBase as TLDecryptedMessage;
            if (message == null) return;

            var mediaDocument = message.Media as TLDecryptedMessageMediaExternalDocument;
            if (mediaDocument == null) return;

            var inputStickerSet = mediaDocument.StickerSet;
            if (inputStickerSet != null)
            {
                TelegramViewBase.NavigateToStickers(MTProtoService, StateService, inputStickerSet);
            }
        }

        public bool IsGroupActionEnabled
        {
            get { return UngroupEnumerator(Items).Any(x => x.IsSelected); }
        }

        public string ScrollButtonImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/ApplicationBar/appbar.next.light.png";
                }

                return "/Images/ApplicationBar/appbar.next.png";
            }
        }

        public Brush WatermarkForeground
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    if (StateService.IsEmptyBackground)
                    {
                        var color = Colors.Black;
                        color.A = 153;
                        return new SolidColorBrush(color);
                    }
                }

                return (Brush)Application.Current.Resources["PhoneContrastForegroundBrush"];
            }
        }

        public Uri EncryptedImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                return isLightTheme ?
                    new Uri("/Images/Dialogs/secretchat-white-WXGA.png", UriKind.Relative) :
                    new Uri("/Images/Dialogs/secretchat-black-WXGA.png", UriKind.Relative);
            }
        }

        private bool _isSelectionEnabled;

        public bool IsSelectionEnabled
        {
            get { return _isSelectionEnabled; }
            set
            {
                SetField(ref _isSelectionEnabled, value, () => IsSelectionEnabled);

                if (!value)
                {
                    foreach (var item in Items)
                    {
                        item.IsSelected = false;
                        var message73 = item as TLDecryptedMessage73;
                        if (message73 != null)
                        {
                            var mediaGroup = message73.Media as TLDecryptedMessageMediaGroup;
                            if (mediaGroup != null)
                            {
                                foreach (var m in mediaGroup.Group)
                                {
                                    m.IsSelected = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private string _text = "";

        public string Text
        {
            get { return _text; }
            set
            {
                var notify = value != _text;
                SetField(ref _text, value, () => Text);
                if (notify)
                {
                    SaveUnsendedTextAsync(_text);
                    NotifyOfPropertyChange(() => CanSend);
                }
            }
        }

        private readonly object _unsendedTextRoot = new object();

        private void SaveUnsendedTextAsync(string text)
        {
            BeginOnThreadPool(() =>
            {
                var inputPeer = With as IInputPeer;
                if (inputPeer == null) return;

                if (string.IsNullOrEmpty(text))
                {
                    FileUtils.Delete(_unsendedTextRoot, "ec" + Chat.Id + ".dat");
                    return;
                }

                TLUtils.SaveObjectToMTProtoFile(_unsendedTextRoot, "ec" + Chat.Id + ".dat", new TLString(text));
            });
        }

        private void LoadUnsendedTextAsync(Action<string> callback)
        {
            BeginOnThreadPool(() =>
            {
                var inputPeer = With as IInputPeer;
                if (inputPeer == null) return;

                var result = TLUtils.OpenObjectFromMTProtoFile<TLString>(_unsendedTextRoot, "ec" + Chat.Id + ".dat");
                callback.SafeInvoke(result != null ? result.ToString() : "");
            });
        }

        public TLUserBase With { get; protected set; }

        public TLEncryptedChatBase Chat { get; protected set; }

        private Typing _typing;

        public Typing Typing
        {
            get { return _typing; }
            set { SetField(ref _typing, value, () => Typing); }
        }

        public string Subtitle { get; protected set; }

        public string AppBarCommandString
        {
            get
            {
                var chatWaiting = Chat as TLEncryptedChatWaiting;
                if (chatWaiting != null)
                {
                    return string.Format(AppResources.WaitingForUserToGetOnline, With.FirstName) + "...";
                }

                var chatDiscarded = Chat as TLEncryptedChatDiscarded;
                if (chatDiscarded != null)
                {
                    return AppResources.Delete;
                }

                return string.Empty;
            }
        }

        public bool IsAppBarCommandVisible
        {
            get
            {
                var chatWaiting = Chat as TLEncryptedChatWaiting;
                if (chatWaiting != null)
                {
                    return true;
                }

                var chatDiscarded = Chat as TLEncryptedChatDiscarded;
                if (chatDiscarded != null)
                {
                    return true;
                }

                var chatEmpty = Chat as TLEncryptedChatEmpty;
                if (chatEmpty != null)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsApplicationBarVisible
        {
            get { return Chat is TLEncryptedChat; }
        }

        public Visibility DescriptionVisibility
        {
            get
            {
                var isEmtpyDialog = Items == null || Items.Count == 0;
                var hasEmptyServiceMessage = false;
                if (!isEmtpyDialog && Items.Count == 1)
                {
                    var serviceMessage = Items[0] as TLDecryptedMessageService;
                    if (serviceMessage != null)
                    {
                        var serviceAction = serviceMessage.Action as TLDecryptedMessageActionEmpty;
                        if (serviceAction != null)
                        {
                            hasEmptyServiceMessage = true;
                        }
                    }
                }

                return (isEmtpyDialog || hasEmptyServiceMessage) && LazyItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ChooseAttachmentViewModel ChooseAttachment { get; set; }

        public bool IsChooseAttachmentOpen { get { return ChooseAttachment != null && ChooseAttachment.IsOpen; } }

        public DecryptedImageViewerViewModel ImageViewer { get; protected set; }

        public IUploadFileManager UploadFileManager
        {
            get { return IoC.Get<IUploadFileManager>(); }
        }

        public IUploadVideoFileManager UploadVideoFileManager
        {
            get { return IoC.Get<IUploadVideoFileManager>(); }
        }

        public IUploadDocumentFileManager UploadDocumentFileManager
        {
            get { return IoC.Get<IUploadDocumentFileManager>(); }
        }

        public string RandomParam { get; set; }

        private readonly DispatcherTimer _selfDestructTimer;

        private readonly object _typingUsersSyncRoot = new object();

        private readonly Dictionary<int, DateTime> _typingUsersCache = new Dictionary<int, DateTime>();

        private readonly IUploadService _uploadService;

        public SecretDialogDetailsViewModel(IUploadService uploadService, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _uploadService = uploadService;

            Items = new ObservableCollection<TLDecryptedMessageBase>();
            Items.CollectionChanged += (sender, args) =>
            {

            };

            Chat = GetChat();
            StateService.With = null;
            if (Chat == null) return;

            With = GetParticipant();
            StateService.Participant = null;
            if (With == null) return;

            eventAggregator.Subscribe(this);

            Status = string.Format(AppResources.SecretChatCaption, With.FirstName);
            Subtitle = GetSubtitle(With);

            _selfDestructTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0) };
            _selfDestructTimer.Tick += OnSelfDestructTimerTick;

            LoadUnsendedTextAsync(unsendedText =>
            {
                Text = unsendedText;
                if (unsendedText == null) return;

                var trimmedText = unsendedText.TrimStart();
                if (trimmedText.StartsWith("@"))
                {
                    var words = trimmedText.Split(' ');
                    if (words.Length >= 1)
                    {
                        if (BrowserNavigationService.IsValidUsername(words[0]))
                        {
                            if (trimmedText.Length > words[0].Length)
                            {
                                ResolveUsername(words[0], trimmedText.Substring(words[0].Length + 1));
                            }
                        }
                    }
                }
            });

            BeginOnThreadPool(() =>
            {
                GetAllStickersAsync();

                var cachedMessages = CacheService.GetDecryptedHistory(Chat.Index).ToList();

                ProcessMessages(cachedMessages);

                foreach (var cachedMessage in cachedMessages)
                {
                    var isDisplayedMessage = TLUtils.IsDisplayedDecryptedMessage(cachedMessage);

                    if (isDisplayedMessage)
                    {
                        LazyItems.Add(cachedMessage);
                    }
                }

                if (LazyItems.Count > 0)
                {
                    BeginOnUIThread(() => PopulateItems(() =>
                    {
                        ReadMessages();
                        NotifyOfPropertyChange(() => DescriptionVisibility);
                    }));
                }
                else
                {
                    ReadMessages();
                    NotifyOfPropertyChange(() => DescriptionVisibility);
                }

                NotifyOfPropertyChange(() => DescriptionVisibility);
            });

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Text))
                {
                    if (!string.IsNullOrEmpty(Text))
                    {
                        var chat17 = Chat as TLEncryptedChat17;
                        if (chat17 != null && chat17.Layer.Value >= Constants.MinSecretChatWithRepliesLayer)
                        {
                            GetWebPagePreviewAsync(Text);
                        }

                        if (chat17 != null)
                        {
                            if (chat17.Layer.Value >= Constants.MinSecretChatWithInlineBotsLayer)
                            {
                                string searchText;
                                var searchInlineBotResults = SearchInlineBotResults(Text, out searchText);
                                if (searchInlineBotResults)
                                {
                                    GetInlineBotResults(searchText);
                                }
                                else
                                {
                                    ClearInlineBotResults();
                                }
                            }

                            if (chat17.Layer.Value >= Constants.MinSecretChatWithStickersLayer)
                            {
                                string searchText;
                                var searchStickerHints = SearchStickerHints(Text, out searchText);
                                if (searchStickerHints)
                                {
                                    GetStickerHints(searchText);
                                }
                                else
                                {
                                    ClearStickerHints();
                                }
                            }
                        }
                    }
                    else
                    {
                        ClearInlineBotResults();
                        ClearStickerHints();
                    }

                    TextTypingManager.SetTyping();
                }
            };
        }

        private TLEncryptedChatBase GetChat()
        {
            if (StateService.With == null)
            {
                if (!string.IsNullOrEmpty(StateService.ChatId))
                {
                    var chatIdString = StateService.ChatId;
                    StateService.ChatId = null;

                    int chatId;
                    try
                    {
                        chatId = Convert.ToInt32(chatIdString);
                    }
                    catch (Exception e)
                    {
                        NavigateToShellViewModel();
                        return null;
                    }
                    var chat = CacheService.GetEncryptedChat(new TLInt(chatId));
                    if (chat != null)
                    {
                        return chat;
                    }

                    NavigateToShellViewModel();
                    return null;
                }

                NavigateToShellViewModel();
                return null;
            }

            return StateService.With as TLEncryptedChatBase;
        }

        private TLUserBase GetParticipant()
        {
            if (StateService.Participant == null)
            {
                if (!string.IsNullOrEmpty(StateService.UserId))
                {
                    var userIdString = StateService.UserId;
                    StateService.UserId = null;

                    int userId;
                    try
                    {
                        userId = Convert.ToInt32(userIdString);
                    }
                    catch (Exception e)
                    {
                        NavigateToShellViewModel();
                        return null;
                    }
                    var user = CacheService.GetUser(new TLInt(userId));
                    if (user != null)
                    {
                        return user;
                    }

                    NavigateToShellViewModel();
                    return null;
                }

                NavigateToShellViewModel();
                return null;
            }

            return StateService.Participant;
        }

        public void NavigateToShellViewModel()
        {
            ShellViewModel.Navigate(NavigationService);
        }

        private void SetTypingInternal(TLBool typing)
        {
            var chat = Chat as TLEncryptedChatCommon;
            if (chat == null) return;

            MTProtoService.SetEncryptedTypingAsync(new TLInputEncryptedChat { AccessHash = chat.AccessHash, ChatId = chat.Id }, typing, result => { });
        }

        //~SecretDialogDetailsViewModel()
        //{

        //}

        private void StartTimer()
        {
            if (_isActive)
            {
                _selfDestructTimer.Start();

                return;
            }

            //if (Chat != null && Chat.MessageTTL != null && IsActive)
            //{
            //    var seconds = Chat.MessageTTL.Value;

            //    if (seconds > 0)
            //    {
            //        if (seconds < 10)
            //        {
            //            _selfDestructTimer.Interval = TimeSpan.FromSeconds(1.0);
            //        }
            //        else if (seconds < 90)
            //        {
            //            _selfDestructTimer.Interval = TimeSpan.FromSeconds(5.0);
            //        }
            //        else
            //        {
            //            _selfDestructTimer.Interval = TimeSpan.FromSeconds(30.0);
            //        }

            //        _selfDestructTimer.Start();
            //    }
            //}
        }

        private void StopTimer()
        {
            _selfDestructTimer.Stop();
            _previousCheckTime = null;
            _screenshotsCount = null;
        }

        private bool _isActive;

        protected override void OnActivate()
        {
            if (With == null) return;

            BeginOnThreadPool(() =>
            {
                Thread.Sleep(500);
                //ReadMessages();
                BeginOnUIThread(StartTimer);
                InputTypingManager.Start();
            });

            base.OnActivate();
        }

        private bool _disableWatching;

        protected override void OnDeactivate(bool close)
        {
            StopTimer();
            InputTypingManager.Stop();

            if (_watcher != null)
            {
                _watcher.Stop();
                _disableWatching = true;
            }

            base.OnDeactivate(close);
        }

        private DateTime? _previousCheckTime;

        private int? _screenshotsCount;

        private void OnSelfDestructTimerTick(object state, System.EventArgs eventArgs)
        {
            var now = DateTime.Now.Ticks;

            var group = new Dictionary<long, TLDecryptedMessageMediaGroup>();
            var randomId = new TLVector<TLLong>();
            for (var i = 0; i < Items.Count; i++)
            {
                var message = Items[i] as TLDecryptedMessage;
                if (message != null)
                {
                    var groupedMessage = false;
                    var mediaGroup = message.Media as TLDecryptedMessageMediaGroup;
                    if (mediaGroup != null)
                    {
                        groupedMessage = true;
                        for (var k = 0; k < mediaGroup.Group.Count; k++)
                        {
                            var item73 = mediaGroup.Group[k] as TLDecryptedMessage73;
                            if (item73 != null
                                && item73.Status == MessageStatus.Read
                                && item73.DeleteIndex > 0)
                            {
                                randomId.Insert(0, item73.RandomId);

                                var diffTicks = now - item73.DeleteDate.Value;
                                if (diffTicks > 0)
                                {
                                    mediaGroup.Group.RemoveAt(k);
                                    if (mediaGroup.Group.Count == 0)
                                    {
                                        Items.Remove(message);
                                    }
                                    else
                                    {
                                        group[item73.GroupedId.Value] = mediaGroup;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    if (!groupedMessage
                        && message.Status == MessageStatus.Read
                        && message.DeleteIndex > 0)
                    {
                        var diffTicks = now - message.DeleteDate.Value;
                        if (diffTicks > 0)
                        {
                            var deleted = false;
                            var message17 = message as TLDecryptedMessage17;
                            if (message17 != null)
                            {
                                var mediaAudio17 = message17.Media as TLDecryptedMessageMediaAudio17;
                                if (mediaAudio17 != null)
                                {
                                    if (!message.Out.Value)
                                    {
                                        deleted = true;
                                        DeleteMessage(false, message);
                                    }
                                }

                                var mediaDocument45 = message17.Media as TLDecryptedMessageMediaDocument45;
                                if (mediaDocument45 != null && message.IsVoice())
                                {
                                    if (!message.Out.Value)
                                    {
                                        deleted = true;
                                        DeleteMessage(false, message);
                                    }
                                }
                            }

                            if (!deleted)
                            {
                                Items.RemoveAt(i--);
                            }

                            randomId.Insert(0, message.RandomId);
                        }
                    }
                }
            }

            foreach (var mediaGroup in group.Values)
            {
                mediaGroup.RaiseCalculate();
            }

            CacheService.DeleteDecryptedMessages(randomId);

            if (randomId.Count > 0 && Items.Count < Constants.MessagesSlice)
            {
                IList<TLDecryptedMessageBase> history;

                if (Items.Count > 0)
                {
                    history = CacheService.GetDecryptedHistory(Chat.Index, Items[Items.Count - 1].RandomId.Value);
                }
                else
                {
                    history = CacheService.GetDecryptedHistory(Chat.Index);
                }

                if (history != null)
                {
                    foreach (var item in history)
                    {
                        if (TLUtils.IsDisplayedDecryptedMessage(item))
                        {
                            Items.Add(item);
                        }
                    }
                }
            }

            //#if WP8
            //            if (!_previousCheckTime.HasValue || DateTime.Now > _previousCheckTime.Value.AddSeconds(10.0))
            //            {
            //                BeginOnThreadPool(async () =>
            //                {
            //                    try
            //                    {
            //                        var screenshotsFolder = await KnownFolders.PicturesLibrary.GetFolderAsync("Screenshots");
            //                        var screenshotsFiles = await screenshotsFolder.GetFilesAsync();
            //                        var previousScreenshotsCount = _screenshotsCount;
            //                        _screenshotsCount = screenshotsFiles.Count;
            //                        _previousCheckTime = DateTime.Now;
            //                        if (_screenshotsCount > previousScreenshotsCount)
            //                        {
            //                            var chat = Chat as TLEncryptedChat;
            //                            if (chat == null) return;

            //                            var screenshotAction = new TLDecryptedMessageActionScreenshotMessages();
            //                            screenshotAction.RandomIds = new TLVector<TLLong>();

            //                            var decryptedTuple = GetDecryptedServiceMessageAndObject(screenshotAction, chat,
            //                                MTProtoService.CurrentUserId, CacheService);

            //                            Execute.BeginOnUIThread(() =>
            //                            {
            //                                Items.Insert(0, decryptedTuple.Item1);
            //                                NotifyOfPropertyChange(() => DescriptionVisibility);
            //                            });

            //                            SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
            //                                result =>
            //                                {

            //                                });
            //                        }
            //                    }
            //                    catch (FileNotFoundException ex)
            //                    {
            //                        Execute.ShowDebugMessage("OnSelfDestructTimerTick check screenshot ex " + ex);
            //                        // Screenshots folder doesn't exist
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        Execute.ShowDebugMessage("OnSelfDestructTimerTick check screenshot ex " + ex);
            //                    }
            //                });
            //            }
            //#endif


            if (Items.Count == 0)
            {
                NotifyOfPropertyChange(() => DescriptionVisibility);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            NotifyOfPropertyChange(() => AppBarCommandString);

            base.OnViewLoaded(view);
        }

        public void CopyMessage(TLDecryptedMessage message)
        {
            if (message == null) return;

            Clipboard.SetText(message.Message.ToString());
        }

        public void OnForwardInAnimationComplete()
        {
            if (ChooseAttachment == null)
            {
                ChooseAttachment = new ChooseAttachmentViewModel(With, OpenInlineBot, SendDocument, null, SendPhoto, SendLocation, null, CacheService, EventAggregator, NavigationService, StateService, false);
                NotifyOfPropertyChange(() => ChooseAttachment);
            }

            if (StateService.RemoveBackEntry)
            {
                NavigationService.RemoveBackEntry();
                StateService.RemoveBackEntry = false;
            }

            if (StateService.RemoveBackEntries)
            {
                var backEntry = NavigationService.BackStack.FirstOrDefault();
                while (backEntry != null && !backEntry.Source.ToString().Contains("ShellView.xaml"))
                {
                    NavigationService.RemoveBackEntry();
                    backEntry = NavigationService.BackStack.FirstOrDefault();
                }

                StateService.RemoveBackEntries = false;
            }

            //if (PrivateBetaIdentityToVisibilityConverter.IsPrivateBeta)
            {
                SecretChatDebug = new SecretChatDebugViewModel(Chat, Rekey);
                NotifyOfPropertyChange(() => SecretChatDebug);
            }
        }

        private void Rekey()
        {
            var chat = Chat as TLEncryptedChat20;
            if (chat == null) return;
            if (chat.PFS_ExchangeId != null) return;

            var layer = chat.Layer;
            if (layer.Value < 20) return;

            var aBytes = new byte[256];
            var random = new SecureRandom();
            random.NextBytes(aBytes);
            var p = chat.P;
            var g = chat.G;

            var gaBytes = Telegram.Api.Services.MTProtoService.GetGB(aBytes, g, p);
            var ga = TLString.FromBigEndianData(gaBytes);

            var randomId = TLLong.Random();
            chat.PFS_A = TLString.FromBigEndianData(aBytes);
            chat.PFS_ExchangeId = randomId;
            var actionRequestKey = new TLDecryptedMessageActionRequestKey { ExchangeId = randomId, GA = ga };
            var decryptedTuple = GetDecryptedServiceMessageAndObject(actionRequestKey, chat, MTProtoService.CurrentUserId, CacheService);
            decryptedTuple.Item1.Unread = TLBool.False;

            if (TLUtils.IsDisplayedDecryptedMessage(decryptedTuple.Item1))
            {
                Items.Insert(0, decryptedTuple.Item1);
            }

            SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                result =>
                {

                });
        }

        public void OpenPeerDetails()
        {
            StateService.CurrentContact = With;
            if (Chat != null)
            {
                StateService.CurrentEncryptedChat = Chat;
                StateService.CurrentKey = Chat.Key;
                StateService.CurrentKeyFingerprint = Chat.KeyFingerprint;
                StateService.CurrentDecryptedMediaMessages =
                UngroupEnumerator(Items).OfType<TLDecryptedMessage>().Where(x => x.Media is TLDecryptedMessageMediaPhoto || x.Media is TLDecryptedMessageMediaVideo).ToList();
            }
            NavigationService.UriFor<SecretContactViewModel>().Navigate();
        }

        private void ReadMessages()
        {
            BeginOnUIThread(() =>
            {
                var unreadMessages = CacheService.GetUnreadDecryptedHistory(Chat.Index);
                for (var i = 0; i < Items.Count; i++)
                {
                    if (!Items[i].Out.Value)
                    {
                        if (Items[i].Unread.Value)
                        {
                            unreadMessages.Add(Items[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (unreadMessages.Count > 0)
                {
                    ReadMessages(unreadMessages.ToArray());
                }
            });
        }

        private void ReadMessages(params TLDecryptedMessageBase[] messages)
        {
            if (!_isActive) return;

            var chat = Chat as TLEncryptedChatCommon;
            if (chat == null) return;

            var maxDate = messages.Max(x => x.Date.Value);

            SetRead(messages);

            MTProtoService.ReadEncryptedHistoryAsync(
                new TLInputEncryptedChat { ChatId = Chat.Id, AccessHash = chat.AccessHash },
                new TLInt(maxDate),
                result =>
                {
                    //SetRead(messages);
                },
                error =>
                {
                    Execute.ShowDebugMessage("messages.readEncryptedHistory error: " + error);
                });
        }

        private void SetRead(params TLDecryptedMessageBase[] messages)
        {
            var dialog = CacheService.GetEncryptedDialog(Chat.Id) as TLEncryptedDialog;

            // input messages, no need to update UI
            messages.ForEach(x =>
            {
                if (x.TTL != null && x.TTL.Value > 0)
                {
                    var decryptedMessage = x as TLDecryptedMessage17;
                    if (decryptedMessage != null)
                    {
                        var decryptedPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                        if (decryptedPhoto != null && x.TTL.Value <= 60.0)
                        {
                            return;
                        }

                        var decryptedVideo17 = decryptedMessage.Media as TLDecryptedMessageMediaVideo17;
                        if (decryptedVideo17 != null && x.TTL.Value <= 60.0)
                        {
                            return;
                        }

                        var decryptedAudio17 = decryptedMessage.Media as TLDecryptedMessageMediaAudio17;
                        if (decryptedAudio17 != null && x.TTL.Value <= 60.0)
                        {
                            return;
                        }

                        var decryptedDocument45 = decryptedMessage.Media as TLDecryptedMessageMediaDocument45;
                        if (decryptedDocument45 != null && (x.IsVoice() || x.IsVideo()) && x.TTL.Value <= 60.0)
                        {
                            return;
                        }

                        var decryptedGroup = decryptedMessage.Media as TLDecryptedMessageMediaGroup;
                        if (decryptedGroup != null && x.TTL.Value <= 60.0)
                        {
                            return;
                        }
                    }
                    x.DeleteDate = new TLLong(DateTime.Now.Ticks + x.TTL.Value * TimeSpan.TicksPerSecond);
                }
                x.Unread = TLBool.False;
                x.Status = MessageStatus.Read;
                //CacheService.SyncDecryptedMessage(x, Chat, r => { }); local messages, will commit changes with dialog update
            });

            CacheService.Commit();

            Execute.BeginOnUIThread(() =>
            {
                if (dialog != null)
                {
                    dialog.UnreadCount = new TLInt(0);

                    dialog.NotifyOfPropertyChange(() => dialog.UnreadCount);
                    dialog.NotifyOfPropertyChange(() => dialog.TopMessage);
                    dialog.NotifyOfPropertyChange(() => dialog.Self);

                    CacheService.Commit();
                }
            });
        }

        public bool SliceLoaded { get; set; }

        public void LoadNextSlice()
        {
            if (LazyItems.Count > 0 || IsLastSliceLoaded) return;

            var lastItem = Items.LastOrDefault();

            long lastRandomId = 0;
            if (lastItem != null)
            {
                lastRandomId = lastItem.RandomIndex;
            }

            var slice = CacheService.GetDecryptedHistory(Chat.Index, lastRandomId);

            ProcessMessages(slice);

            SliceLoaded = true;

            if (slice.Count == 0)
            {
                IsLastSliceLoaded = true;
            }
            foreach (var message in slice)
            {
                if (TLUtils.IsDisplayedDecryptedMessage(message))
                {
                    Items.Add(message);
                }
            }


            NotifyOfPropertyChange(() => DescriptionVisibility);
        }

        public void OnBackwardInAnimationComplete()
        {
            SendMedia();

            ReadMessages();
        }

        public SecretChatDebugViewModel SecretChatDebug { get; protected set; }

        public void DeleteChat()
        {
            var chatDiscarded = Chat as TLEncryptedChatDiscarded;
            if (chatDiscarded == null) return;

            var dialog = CacheService.GetEncryptedDialog(Chat.Id);
            if (dialog != null)
            {
                EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                CacheService.DeleteDialog(dialog);
            }
            BeginOnUIThread(() => NavigationService.GoBack());
        }

        public void CancelVideoDownloading(TLDecryptedMessageMediaVideo mediaVideo)
        {
            mediaVideo.DownloadingProgress = 0.0;

            var fileManager = IoC.Get<IEncryptedFileManager>();
            fileManager.CancelDownloadFile(mediaVideo);

            DeleteSendingMessage(mediaVideo);
        }

        public void CancelDocumentDownloading(TLDecryptedMessageMediaDocument mediaDocument)
        {
            mediaDocument.DownloadingProgress = 0.0;

            var fileManager = IoC.Get<IEncryptedFileManager>();
            fileManager.CancelDownloadFile(mediaDocument);

            DeleteSendingMessage(mediaDocument);
        }

        public void CancelPhotoDownloading(TLDecryptedMessageMediaPhoto mediaPhoto)
        {
            mediaPhoto.IsCanceled = true;
            mediaPhoto.DownloadingProgress = 0.0;

            var fileManager = IoC.Get<IEncryptedFileManager>();
            fileManager.CancelDownloadFile(mediaPhoto);

            DeleteSendingMessage(mediaPhoto);
        }

        public void CancelAudioDownloading(TLDecryptedMessageMediaBase mediaBase)
        {
            var mediaDocument = mediaBase as TLDecryptedMessageMediaDocument;
            if (mediaDocument != null)
            {
                if (mediaDocument.UploadingProgress > 0.0)
                {
                    CancelUploading(mediaDocument);
                }
                else
                {
                    mediaDocument.IsCanceled = true;
                    mediaDocument.DownloadingProgress = -0.01;

                    var fileManager = IoC.Get<IEncryptedFileManager>();
                    fileManager.CancelDownloadFile(mediaDocument);
                }
            }

            var mediaAudio = mediaBase as TLDecryptedMessageMediaAudio;
            if (mediaAudio != null)
            {
                if (mediaAudio.UploadingProgress > 0.0)
                {
                    CancelUploading(mediaAudio);
                }
                else
                {
                    mediaAudio.IsCanceled = true;
                    mediaAudio.DownloadingProgress = -0.01;

                    var fileManager = IoC.Get<IEncryptedFileManager>();
                    fileManager.CancelDownloadFile(mediaAudio);
                }
            }
        }

        private void DeleteSendingMessage(TLDecryptedMessageMediaBase mediaDocument)
        {
            TLDecryptedMessage message = null;
            for (var i = 0; i < Items.Count; i++)
            {
                var messageCommon = Items[i] as TLDecryptedMessage;
                if (messageCommon != null && messageCommon.Media == mediaDocument)
                {
                    message = messageCommon;
                    break;
                }
            }

            if (message != null && message.Status == MessageStatus.Sending)
            {
                DeleteDownloadingMessage(message);
            }
        }

        public void CancelUploading(TLDecryptedMessageMediaBase media)
        {
            TLDecryptedMessage message = null;
            foreach (var item in UngroupEnumerator(Items))
            {
                var messageCommon = item as TLDecryptedMessage;
                if (messageCommon != null && messageCommon.Media == media)
                {
                    message = messageCommon;
                    break;
                }
            }

            if (message != null)
            {
                DeleteUploadingMessage(message);
            }
        }

        private void DeleteDownloadingMessage(TLDecryptedMessageBase messageBase)
        {
            var message = messageBase as TLDecryptedMessage;
            if (message == null) return;

            var media = message.Media;
            if (media == null || media.DownloadingProgress == 1.0) return;

            message.Status = MessageStatus.Failed;
            Items.Remove(message);

            NotifyOfPropertyChange(() => DescriptionVisibility);

            BeginOnThreadPool(() =>
            {
                CacheService.DeleteDecryptedMessages(new TLVector<TLLong> { message.RandomId });
                //CancelDownloading(message);
            });
        }

        private void CancelDownloading(TLDecryptedMessageBase messageBase)
        {
            var message = messageBase as TLDecryptedMessage;
            if (message == null) return;

            var media = message.Media;

            var mediaPhoto = media as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null && mediaPhoto.File != null)
            {
                var file = mediaPhoto.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    var fileManager = IoC.Get<IEncryptedFileManager>();
                    fileManager.CancelDownloadFile(mediaPhoto);
                }
            }

            var mediaVideo = media as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null && mediaVideo.File != null)
            {
                var file = mediaVideo.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    var fileManager = IoC.Get<IEncryptedFileManager>();
                    fileManager.CancelDownloadFile(mediaVideo);
                }
            }

            var mediaDocument = media as TLDecryptedMessageMediaDocument;
            if (mediaDocument != null && mediaDocument.File != null)
            {
                var file = mediaDocument.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    var fileManager = IoC.Get<IEncryptedFileManager>();
                    fileManager.CancelDownloadFile(mediaDocument);
                }
            }

            var mediaAudio = media as TLDecryptedMessageMediaAudio;
            if (mediaAudio != null && mediaAudio.File != null)
            {
                var file = mediaAudio.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    var fileManager = IoC.Get<IEncryptedFileManager>();
                    fileManager.CancelDownloadFile(mediaAudio);
                }
            }
        }

        private void DeleteUploadingMessage(TLDecryptedMessageBase messageBase)
        {
            var message = messageBase as TLDecryptedMessage;
            if (message == null) return;

            var media = message.Media;
            if (media == null || media.UploadingProgress == 1.0) return;

            var message73 = messageBase as TLDecryptedMessage73;
            if (message73 != null && message73.GroupedId != null)
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    var groupedMessage = Items[i] as TLDecryptedMessage73;
                    if (groupedMessage != null
                        && groupedMessage.GroupedId != null
                        && groupedMessage.GroupedId.Value == message73.GroupedId.Value)
                    {
                        var mediaGroup = groupedMessage.Media as TLDecryptedMessageMediaGroup;
                        if (mediaGroup != null)
                        {
                            mediaGroup.Group.Remove(messageBase);
                            mediaGroup.RaiseCalculate();

                            if (mediaGroup.Group.Count == 0)
                            {
                                groupedMessage.Status = MessageStatus.Failed;
                                Items.Remove(groupedMessage);
                            }
                            else if (mediaGroup.Group.Count == 1)
                            {
                                message73.GroupedId = null;
                                if (Items[i] == groupedMessage)
                                {
                                    Items.RemoveAt(i);
                                    Items.Insert(i, mediaGroup.Group[0]);
                                }
                            }
                            break;
                        }

                        message.Status = MessageStatus.Failed;
                        Items.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                message.Status = MessageStatus.Failed;
                Items.Remove(message);
            }

            NotifyOfPropertyChange(() => DescriptionVisibility);

            BeginOnThreadPool(() =>
            {
                CacheService.DeleteDecryptedMessages(new TLVector<TLLong> { message.RandomId });
                CancelUploading(message);
            });
        }

        private void CancelUploading(TLDecryptedMessageBase messageBase)
        {
            var message = messageBase as TLDecryptedMessage;
            if (message == null) return;

            var media = message.Media;

            var mediaPhoto = media as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null && mediaPhoto.File != null)
            {
                var file = mediaPhoto.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    UploadDocumentFileManager.CancelUploadFile(file.Id);
                }
            }

            var mediaVideo = media as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null && mediaVideo.File != null)
            {
                var file = mediaVideo.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    UploadVideoFileManager.CancelUploadFile(file.Id);
                }
            }

            var mediaDocument = media as TLDecryptedMessageMediaDocument;
            if (mediaDocument != null && mediaDocument.File != null)
            {
                var file = mediaDocument.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    if (messageBase.IsVoice())
                    {
                        UploadFileManager.CancelUploadFile(file.Id);
                    }
                    else if (messageBase.IsVideo())
                    {
                        UploadVideoFileManager.CancelUploadFile(file.Id);
                    }
                    else
                    {
                        UploadDocumentFileManager.CancelUploadFile(file.Id);
                    }
                }
            }

            var mediaAudio = media as TLDecryptedMessageMediaAudio;
            if (mediaAudio != null && mediaAudio.File != null)
            {
                var file = mediaAudio.File as TLEncryptedFile;
                if (file != null && file.Id != null)
                {
                    UploadFileManager.CancelUploadFile(file.Id);
                }
            }
        }

#if WP81
        public static async Task<StorageFile> GetStorageFile(TLDecryptedMessageMediaBase media)
        {
            if (media == null) return null;
            if (media.StorageFile != null)
            {
                if (File.Exists(media.StorageFile.Path))
                {
                    return media.StorageFile;
                }
            }

            var mediaDocument = media as TLDecryptedMessageMediaDocument;
            if (mediaDocument != null)
            {
                var file = media.File as TLEncryptedFile;
                if (file == null) return null;

                var fileName = String.Format("{0}_{1}_{2}.{3}",
                    file.Id,
                    file.DCId,
                    file.AccessHash,
                    mediaDocument.FileExt);

                return await DialogDetailsViewModel.GetFileFromLocalFolder(fileName);
            }

            var mediaVideo = media as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null)
            {
                var file = media.File as TLEncryptedFile;
                if (file == null) return null;

                var fileName = String.Format("{0}_{1}_{2}.{3}",
                    file.Id,
                    file.DCId,
                    file.AccessHash,
                    "mp4");

                return await DialogDetailsViewModel.GetFileFromLocalFolder(fileName);
            }

            return null;
        }
#endif


#if WP81
        public async void Resend(TLDecryptedMessage message)
#else
        public void Resend(TLDecryptedMessage message)
#endif
        {
            if (message == null) return;

            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            TLObject obj = message;
            var decryptedMessage17 = message as TLDecryptedMessage17;
            if (decryptedMessage17 != null)
            {
                var encryptedChat17 = chat as TLEncryptedChat17;
                if (encryptedChat17 == null) return;

                var messageLayer17 = TLUtils.GetDecryptedMessageLayer(encryptedChat17.Layer, decryptedMessage17.InSeqNo, decryptedMessage17.OutSeqNo, decryptedMessage17);

                obj = messageLayer17;
            }

            // find common grouped message to resend
            var message73 = message as TLDecryptedMessage73;
            if (message73 != null
                && message73.GroupedId != null)
            {
                var mediaGroup = message73.Media as TLDecryptedMessageMediaGroup;
                if (mediaGroup == null)
                {
                    var groupedMessage = FindGroupedMessage(message73) as TLDecryptedMessage73;
                    if (groupedMessage != null)
                    {
                        message = groupedMessage;
                    }
                }
            }

            message.Status = MessageStatus.Sending;
            message.NotifyOfPropertyChange(() => message.Status);

            var message45 = message as TLDecryptedMessage45;
            if (message45 != null && message45.InlineBotResult != null)
            {
                ResendInlineBotResult(message45, ContinueProcessBotInlineResult);
                return;
            }

            if (message.Media is TLDecryptedMessageMediaEmpty)
            {
                SendEncrypted(chat, obj, MTProtoService, CacheService);
            }
            else
            {
                message.Media.UploadingProgress = 0.001;
                if (message.Media is TLDecryptedMessageMediaGroup)
                {
                    var mediaGroup = (TLDecryptedMessageMediaGroup)message.Media;
                    var inputMedia = new TLVector<TLInputEncryptedFileBase>();
                    foreach (var item in mediaGroup.Group)
                    {
                        if (item.InputFile != null)
                        {
                            inputMedia.Add(item.InputFile);
                        }
                    }
                    if (inputMedia.Count == mediaGroup.Group.Count)
                    {
                        SendEncryptedMultiMediaInternal(chat, message, MTProtoService, CacheService);
                    }
                    else
                    {
                        var chat17 = Chat as TLEncryptedChat17;
                        if (chat17 == null) return;

                        var messages = new List<Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>>();
                        foreach (var item in mediaGroup.Group)
                        {
                            var item73 = item as TLDecryptedMessage73;
                            if (item73 == null) return;

                            item73.Media.UploadingProgress = 0.001;
                            var decryptedMessageLayer17 = TLUtils.GetDecryptedMessageLayer(chat17.Layer, new TLInt(-1), new TLInt(-1), item);
                            messages.Add(new Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>(item, decryptedMessageLayer17));
                        }

                        IoC.Get<IUploadService>().AddGroup(message);
                        UploadPhotoInternal(messages);
                    }
                }
                else if (message.Media is TLDecryptedMessageMediaPhoto)
                {
                    UploadPhotoInternal(null, obj);
                }
                else if (message.Media is TLDecryptedMessageMediaVideo)
                {
                    SendVideoInternal(null, obj);
                }
                else if (message.Media is TLDecryptedMessageMediaDocument)
                {
                    if (message.IsVoice())
                    {
#if WP8
                        SendAudioInternal(obj);
#endif
                    }
                    else if (message.IsVideo())
                    {
#if WP8
                        SendVideoInternal(null, obj);
#endif
                    }
                    else
                    {
#if WP81
                        var file = await GetStorageFile(message.Media);

                        if (file != null)
                        {
                            SendDocumentInternal(file, message);
                        }
                        else
                        {
                            MessageBox.Show(AppResources.UnableToAccessDocument, AppResources.Error, MessageBoxButton.OK);
                            message.Status = MessageStatus.Failed;
                            DeleteMessage(false, message);
                            return;
                        }
#else
                        SendDocumentInternal(null, message);
#endif
                    }
                }
                else if (message.Media is TLDecryptedMessageMediaAudio)
                {
#if WP8
                    SendAudioInternal(obj);
#endif
                }
                else if (message.Media is TLDecryptedMessageMediaGeoPoint)
                {
                    SendEncrypted(chat, obj, MTProtoService, CacheService);
                }
                else if (message.Media is TLDecryptedMessageMediaContact)
                {

                }
            }

            message.NotifyOfPropertyChange(() => message.Status);
        }

        public TLDecryptedMessageBase FindGroupedMessage(TLDecryptedMessage73 message73)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var m = Items[i] as TLDecryptedMessage73;
                if (m != null)
                {
                    var mGroup = m.Media as TLDecryptedMessageMediaGroup;
                    if (mGroup != null)
                    {
                        for (var j = 0; j < mGroup.Group.Count; j++)
                        {
                            if (mGroup.Group[j] == message73)
                            {
                                return m;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void ChangeSelection(TLDecryptedMessageBase messageBase)
        {
            if (messageBase == null) return;

            messageBase.IsSelected = !messageBase.IsSelected;

            var message = messageBase as TLDecryptedMessage73;
            if (message != null && message.GroupedId != null)
            {
                var mediaGroup = message.Media as TLDecryptedMessageMediaGroup;
                if (mediaGroup != null)
                {
                    foreach (var m in mediaGroup.Group)
                    {
                        m.IsSelected = messageBase.IsSelected;
                    }
                }
            }

            NotifyOfPropertyChange(() => IsGroupActionEnabled);
        }

        private void RemoveMessages(IList<TLDecryptedMessageBase> messages)
        {
            var group = new Dictionary<long, TLDecryptedMessageMediaGroup>();
            for (var i = 0; i < messages.Count; i++)
            {
                if (messages[i].Status == MessageStatus.Sending)
                {
                    messages[i].Status = MessageStatus.Failed;
                    CancelUploading(messages[i]);
                }

                for (var j = 0; j < Items.Count; j++)
                {
                    if (Items[j] == messages[i])
                    {
                        Items.RemoveAt(j);
                        break;
                    }

                    var message73 = Items[j] as TLDecryptedMessage73;
                    if (message73 != null && message73.GroupedId != null)
                    {
                        var mediaGroup = message73.Media as TLDecryptedMessageMediaGroup;
                        if (mediaGroup != null)
                        {
                            for (var k = 0; k < mediaGroup.Group.Count; k++)
                            {
                                if (mediaGroup.Group[k] == messages[i])
                                {
                                    mediaGroup.Group.RemoveAt(k);
                                    if (mediaGroup.Group.Count == 0)
                                    {
                                        Items.Remove(message73);
                                    }
                                    else
                                    {
                                        group[message73.GroupedId.Value] = mediaGroup;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var mediaGroup in group.Values)
            {
                mediaGroup.RaiseCalculate();
            }
        }

        private void DeleteMessagesWithCallback(bool confirmation, IList<TLDecryptedMessageBase> messages, System.Action callback = null)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            var items = new List<TLDecryptedMessageBase>();
            var localItems = new List<TLDecryptedMessageBase>();
            var remoteItems = new List<TLDecryptedMessageBase>();

            foreach (var item in messages)
            {
                items.Add(item);

                if (item.Status == MessageStatus.Sending
                    || item.Status == MessageStatus.Failed)
                {
                    localItems.Add(item);
                }
                else
                {
                    remoteItems.Add(item);
                }
            }

            if (localItems.Count > 0 || remoteItems.Count > 0)
            {
                IsSelectionEnabled = false;
            }

            var totalCount = localItems.Count + remoteItems.Count;

            IsDeleteConfirmed(confirmation, totalCount, localItems, remoteItems, revoke =>
            {
                if (remoteItems.Count > 0)
                {
                    var messageId = new TLVector<TLLong> { Items = remoteItems.Select(x => x.RandomId).ToList() };

                    var action = new TLDecryptedMessageActionDeleteMessages { RandomIds = messageId };

                    var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);
#if DEBUG
                    Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                    SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                        result => BeginOnUIThread(() =>
                        {
                            RemoveMessages(items);

                            NotifyOfPropertyChange(() => DescriptionVisibility);
                            CacheService.DeleteDecryptedMessages(new TLVector<TLLong>(items.Select(x => x.RandomId).ToList()));
                            callback.SafeInvoke();
                        }));
                }
                else
                {
                    RemoveMessages(items);

                    NotifyOfPropertyChange(() => DescriptionVisibility);
                    CacheService.DeleteDecryptedMessages(new TLVector<TLLong>(items.Select(x => x.RandomId).ToList()));
                    callback.SafeInvoke();
                }
            });
        }

        public void DeleteMessages()
        {
            var messages = UngroupEnumerator(Items).Where(x => x.IsSelected).ToList();

            DeleteMessagesWithCallback(true, messages);
        }

        public void DeleteMessage(bool confirmation, TLObject obj)
        {
            DeleteMessageWithCallback(confirmation, obj);
        }

        public void DeleteMessageWithCallback(bool confirmation, TLObject obj, System.Action callback = null)
        {
            var mediaPhoto = obj as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                var mediaMessage = UngroupEnumerator(Items).OfType<TLDecryptedMessage>().FirstOrDefault(x => x.Media == mediaPhoto);
                if (mediaMessage == null) return;

                DeleteMessagesWithCallback(confirmation, new List<TLDecryptedMessageBase> { mediaMessage }, callback);
            }

            var message = obj as TLDecryptedMessage;
            if (message != null)
            {
                DeleteMessagesWithCallback(confirmation, new List<TLDecryptedMessageBase> { message }, callback);
            }
        }

        public void IsDeleteConfirmed(bool confirmation, int totalCount, IList<TLDecryptedMessageBase> localMessages, IList<TLDecryptedMessageBase> remoteMessages, Action<bool> callback)
        {
            if (!confirmation)
            {
                callback.SafeInvoke(true);
                return;
            }

            if (totalCount == 0) return;
            TLDecryptedMessageBase m = null;
            if (localMessages != null && localMessages.Count > 0)
            {
                m = localMessages.FirstOrDefault();
            }
            if (m == null && remoteMessages != null && remoteMessages.Count > 0)
            {
                m = remoteMessages.FirstOrDefault();
            }
            if (m == null) return;

            var message = totalCount == 1
                ? AppResources.DeleteMessageConfirmation
                : string.Format(AppResources.DeleteMessagesConfirmation, Utils.Language.Declension(
                    totalCount,
                    AppResources.MessageNominativeSingular,
                    AppResources.MessageNominativePlural,
                    AppResources.MessageGenitiveSingular,
                    AppResources.MessageGenitivePlural))
                    .ToLower(CultureInfo.CurrentUICulture);

            ShellViewModel.ShowCustomMessageBox(message, AppResources.Confirm,
                AppResources.Delete.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                        {
                            callback.SafeInvoke(true);
                        });
                    }
                });
        }

        private int InsertMessageInOrder(TLDecryptedMessageBase message)
        {
            if (Chat != null && Chat.MessageTTL != null)
            {
                message.TTL = Chat.MessageTTL;
            }

            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].DateIndex == message.DateIndex
                    && Items[i].QtsIndex == message.QtsIndex
                    && Items[i].RandomIndex == message.RandomIndex)
                {
                    return -1;
                }

                if (Items[i].DateIndex < message.DateIndex)
                {
                    Items.Insert(i, message);
                    return i;
                }

                if (Items[i].QtsIndex < message.QtsIndex)
                {
                    Items.Insert(i, message);
                    return i;
                }
            }

            Items.Add(message);
            return Items.Count - 1;
        }

        private Typing GetTyping(IList<Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>> typingUsers)
        {
            return DialogsViewModel.GetTyping(TLUtils.InputPeerToPeer(With.ToInputPeer(), StateService.CurrentUserId), typingUsers, CacheService.GetUser, null);
        }

        private string GetSubtitle(TLUserBase user)
        {
            Typing = null;

            return DialogDetailsViewModel.GetUserStatus(user);
        }

        public void OpenMediaContact(TLUserBase user, TLString phone)
        {
            if (user == null) return;

            StateService.CurrentContact = user;
            StateService.CurrentContactPhone = phone;
            NavigationService.UriFor<ContactViewModel>().Navigate();
        }

        public void OnNavigatedTo()
        {
            _isActive = true;
            StateService.ActiveDialog = Chat;
        }

        public void OnNavigatedFrom()
        {
            _isActive = false;
            StateService.ActiveDialog = null;
        }

        private InputTypingManager _inputTypingManager;

        public InputTypingManager InputTypingManager
        {
            get
            {
                return _inputTypingManager =
                    _inputTypingManager ??
                    new InputTypingManager(
                        users =>
                        {
                            var typing = GetTyping(users);
                            Subtitle = typing != null ? typing.ToString() : null;
                            NotifyOfPropertyChange(() => Subtitle);
                            Typing = typing;
                        },
                        () =>
                        {
                            Subtitle = GetSubtitle(With);
                            NotifyOfPropertyChange(() => Subtitle);
                        });
            }
        }

        private OutputTypingManager _textTypingManager;

        public OutputTypingManager TextTypingManager
        {
            get
            {
                return _textTypingManager =
                    _textTypingManager ??
                    new OutputTypingManager(With.ToInputPeer(), Constants.SetTypingIntervalInSeconds,
                        action => SetTypingInternal(TLBool.True),
                        () => SetTypingInternal(TLBool.False));
            }
        }

        public void OpenStickerSettings()
        {
            NavigationService.UriFor<StickersViewModel>().Navigate();
        }

        private void InsertSendingMessage(TLDecryptedMessageBase messageBase)
        {
            var message = messageBase as TLDecryptedMessage45;
            if (message != null)
            {
                if (Reply != null)
                {
                    if (Reply.RandomIndex != 0)
                    {
                        message.ReplyToRandomMsgId = Reply.RandomId;
                        message.Reply = Reply;
                    }

                    BeginOnUIThread(() =>
                    {
                        Reply = null;
                    });
                }
            }

            Items.Insert(0, messageBase);
        }

        public bool StartGifPlayers { get; set; }

        private void ReadMessageContents(TLDecryptedMessage45 message)
        {
            if (message == null) return;

            if (message.RandomIndex != 0 && !message.Out.Value && message.NotListened)
            {
                var chat = Chat as TLEncryptedChat;
                if (chat == null) return;

                var messageId = new TLVector<TLLong> { message.RandomId };

                var action = new TLDecryptedMessageActionReadMessages { RandomIds = messageId };

                var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);

#if DEBUG
                Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                    result =>
                    {
                        message.SetListened();
                        message.Media.NotListened = false;
                        message.Media.NotifyOfPropertyChange(() => message.Media.NotListened);

                        CacheService.Commit();
                    });
            }
        }

        public static IEnumerable<TLDecryptedMessageBase> UngroupEnumerator(IEnumerable<TLDecryptedMessageBase> source)
        {
            foreach (var messageBase in source)
            {
                var message = messageBase as TLDecryptedMessage73;
                if (message != null && message.GroupedId != null)
                {
                    var mediaGroup = message.Media as TLDecryptedMessageMediaGroup;
                    if (mediaGroup != null)
                    {
                        for (var i = mediaGroup.Group.Count - 1; i >= 0; i--)
                        {
                            yield return mediaGroup.Group[i];
                        }
                    }
                    else
                    {
                        yield return messageBase;
                    }
                }
                else
                {
                    yield return messageBase;
                }
            }
        }
    }
}
