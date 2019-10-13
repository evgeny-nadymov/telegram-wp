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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Networking.Connectivity;
using Windows.Phone.Media.Devices;
using Windows.Phone.Networking.Voip;
using Windows.System;
using BugSense;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneVoIPApp.BackEnd;
using PhoneVoIPApp.UI;
using Telegram.Api;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Controls;
using TelegramClient.Helpers;
using TelegramClient.Helpers.TemplateSelectors;
using TelegramClient.ViewModels.Calls;
using TelegramClient.Views;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Calls;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;
using Caliburn.Micro;
using Microsoft.Devices;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Telegram.Api.Helpers;
using Telegram.Api.MD5;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.Location;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.Api.Transport;
using Telegram.Controls.VirtualizedView;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Debug;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Payments;
using Action = System.Action;
using Environment = System.Environment;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels
{
    public class ShellViewModel : Conductor<ViewModelBase>.Collection.OneActive,
        Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        Telegram.Api.Aggregator.IHandle<TLMessageCommon>,
        Telegram.Api.Aggregator.IHandle<TLDecryptedMessageBase>,
        Telegram.Api.Aggregator.IHandle<UpdatingEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChannelTooLong>,
        Telegram.Api.Aggregator.IHandle<UpdateCompletedEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateChannelsEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateContactRegistered>,
        Telegram.Api.Aggregator.IHandle<ExceptionInfo>,
        Telegram.Api.Aggregator.IHandle<TLUpdateStickerSets>,
        Telegram.Api.Aggregator.IHandle<TLUpdateFavedStickers>,
        Telegram.Api.Aggregator.IHandle<TLUpdateStickerSetsOrder>,
        Telegram.Api.Aggregator.IHandle<TLUpdateNewStickerSet>,
        Telegram.Api.Aggregator.IHandle<TLUpdateReadFeaturedStickers>,
        Telegram.Api.Aggregator.IHandle<PhoneCallStartedEventArgs>,
        Telegram.Api.Aggregator.IHandle<PhoneCallDiscardedEventArgs>,
        Telegram.Api.Aggregator.IHandle<PhoneCallRequestedEventArgs>,
        Telegram.Api.Aggregator.IHandle<PhoneCallStateChangedEventArgs>,
        Telegram.Api.Aggregator.IHandle<SignalBarsChangedEventArgs>,
        Telegram.Api.Aggregator.IHandle<LiveLocationAddedEventArgs>,
        Telegram.Api.Aggregator.IHandle<LiveLocationRemovedEventArgs>,
        Telegram.Api.Aggregator.IHandle<LiveLocationClearedEventArgs>,
        Telegram.Api.Aggregator.IHandle<LiveLocationLoadedEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEditMessage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEditChannelMessage>,
        Telegram.Api.Aggregator.IHandle<MTProtoProxyDisabledEventArgs>
    {

        //public static Stopwatch Timer = Stopwatch.StartNew();

        public static void StartNewTimer()
        {
            //Timer = Stopwatch.StartNew();
        }

        public static void WriteTimer(string str)
        {
            //System.Diagnostics.Debug.WriteLine(str + " elapsed=" + ShellViewModel.Timer.Elapsed);
        }

        public bool IsPasscodeEnabled { get; protected set; }

        public IStateService StateService
        {
            get { return _stateService; }
        }

        public Uri PasscodeImageSource
        {
            get
            {
                return PasscodeUtils.Locked
                    ? new Uri("/Images/Dialogs/passcode.close-WXGA.png", UriKind.Relative)
                    : new Uri("/Images/Dialogs/passcode.open-WXGA.png", UriKind.Relative);
            }
        }

        public Brush PasscodeImageBrush
        {
            get
            {
                return PasscodeUtils.Locked
                    ? (Brush)Application.Current.Resources["TelegramBadgeAccentBrush"]
                    : (Brush)Application.Current.Resources["TelegramBadgeSubtleBrush"];
            }
        }

        //public Visibility PasscodeImageVisibility
        //{
        //    get
        //    {
        //        return string.IsNullOrEmpty(_stateService.Passcode) ? Visibility.Collapsed : Visibility.Visible;                
        //    }
        //}

        public DialogsViewModel Dialogs { get; protected set; }

        public ContactsViewModel Contacts { get; protected set; }

        public CallsViewModel Calls { get; protected set; }

        private DebugViewModel _debug;

        private LongPollViewModel _longPoll;

        private IStateService _stateService;

        private ITelegramEventAggregator _eventAggregator;

        private readonly IMTProtoService _mtProtoService;

        public IMTProtoService MTProtoService { get { return _mtProtoService; } }

        private INavigationService _navigationService;

        private ICacheService _cacheService;

        private IPushService _pushService;

        private bool _registerDeviceOnce;

        private bool _isProxyEnabled;

        public bool IsProxyEnabled
        {
            get { return _isProxyEnabled; }
            set
            {
                if (_isProxyEnabled != value)
                {
                    _isProxyEnabled = value;
                    NotifyOfPropertyChange(() => IsProxyEnabled);
                }
            }
        }

        private bool _connecting;

        public bool Connecting
        {
            get { return _connecting; }
            set
            {
                if (_connecting != value)
                {
                    _connecting = value;
                    NotifyOfPropertyChange(() => Connecting);
                }
            }
        }

        private ConnectionType _connectionType = ConnectionType.Direct;

        public ConnectionType ConnectionType
        {
            get { return _connectionType; }
            set
            {
                if (_connectionType != value)
                {
                    _connectionType = value;
                    NotifyOfPropertyChange(() => ConnectionType);
                }
            }
        }

        public void OnAnimationComplete()
        {
            BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (!isAuthorized)
                {
                    Telegram.Logs.Log.Write("StartupViewModel ShellViewModel.OnAnimationComplete IsAuthorized=false");
                    TLUtils.IsLogEnabled = true;
                    Execute.BeginOnUIThread(() =>
                    {
                        _stateService.ClearNavigationStack = true;
                        _navigationService.UriFor<StartupViewModel>().Navigate();
                    });
                }
                else
                {
                    TLUtils.IsLogEnabled = false;

                    if (_registerDeviceOnce) return;

                    _pushService.RegisterDeviceAsync(() =>
                    {
                        _registerDeviceOnce = true;
                    });
                    _mtProtoService.CurrentUserId = new TLInt(_stateService.CurrentUserId);

                    ContactsHelper.UpdateDelayedContactsAsync(_cacheService, _mtProtoService);

                    UpdatePasscode();
                }
            });
        }

        public void UpdatePasscode()
        {
            IsPasscodeEnabled = PasscodeUtils.IsEnabled;
            NotifyOfPropertyChange(() => IsPasscodeEnabled);
        }

        public ShellViewModel(
#if DEBUG
LongPollViewModel longPoll,
            DebugViewModel debugViewModel,
#endif
 DialogsViewModel dialogs,
            CallsViewModel calls,
            ContactsViewModel contacts,
            IPushService pushService,
            ICacheService cacheService,
            IStateService stateService,
            IMTProtoService mtProtoService,
            ITelegramEventAggregator eventAggregator,
            INavigationService navigationService)
        {
            App.Log("start ShellViewModel .ctor ");
            _pushService = pushService;

            App.Log("start 1 ShellViewModel .ctor ");
            Dialogs = dialogs;
            Calls = calls;
            Contacts = contacts;
#if DEBUG
            _debug = debugViewModel;
            _longPoll = longPoll;
#endif

            App.Log("start 2 ShellViewModel .ctor ");
            _stateService = stateService;
            _eventAggregator = eventAggregator;
            _mtProtoService = mtProtoService;

            App.Log("start 3 ShellViewModel .ctor ");

            _mtProtoService.AuthorizationRequired += OnAuthorizationRequired;
            _mtProtoService.CheckDeviceLocked += OnCheckDeviceLocked;
            App.Log("start ShellViewModel .ctor Before NetworkInformation");

            App.Log("start ShellViewModel .ctor After NetworkInformation");
            _navigationService = navigationService;
            _cacheService = cacheService;

            _eventAggregator.Subscribe(this);

            //Execute.BeginOnThreadPool(TimeSpan.FromSeconds(5.0), () =>
            //{
            //    var storeUpdateService = new WindowsPhoneStoreUpdateService();
            //    storeUpdateService.CheckForUpdatedVersion("Text", "Title");
            //});

            App.Log("stop ShellViewModel .ctor");

            BeginOnThreadPool(() =>
            {
                var allStickers = stateService.GetAllStickers();
                var featuredStickers = stateService.GetFeaturedStickers();
            });
        }

        private void OnCheckDeviceLocked(object sender, System.EventArgs e)
        {
            UpdateDeviceLockedAsync(false);
        }

        private static int _previousPeriod = -2;

        public void UpdateDeviceLockedAsync(bool force = true)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (isAuthorized)
                {
                    UpdatePasscode();

                    Execute.BeginOnUIThread(() =>
                    {
                        var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                        if (frame != null && frame.IsPasscodeActive)
                        {
                            return;
                        }

                        var period = -1;
                        if (PasscodeUtils.IsEnabled)
                        {
                            period = PasscodeUtils.AutolockTimeout;

                            if (PasscodeUtils.Locked)
                            {
                                period = 0;
                            }
                        }

                        if (!force
                            && _previousPeriod == period
                            && (period == -1 || period == int.MaxValue))
                        {
                            return;
                        }

                        if (!force
                            && (period == TimeSpan.FromHours(1).TotalSeconds || period == TimeSpan.FromHours(5).TotalSeconds))
                        {
                            return;
                        }

                        MTProtoService.UpdateDeviceLockedAsync(new TLInt(period),
                            result =>
                            {
                                Execute.BeginOnUIThread(() =>
                                {
                                    _previousPeriod = period;
                                });

                                //Execute.ShowDebugMessage(string.Format("account.updateDeviceLocked {0} result {1}", period, result.Value));
                            },
                            error =>
                            {
                                Execute.ShowDebugMessage(string.Format("account.updateDeviceLocked {0} error {1}", period, error));
                            });
                    });
                }
            });
        }

        private void OnAuthorizationRequired(object sender, AuthorizationRequiredEventArgs e)
        {
            Telegram.Logs.Log.Write("StartupViewModel ShellViewModel.OnAuthorizationRequired " + e.MethodName + " " + e.Error + " " + e.AuthKeyId);

            var updateService = IoC.Get<IUpdatesService>();

            Execute.BeginOnUIThread(() =>
            {
                SettingsViewModel.LogOutCommon(
                    _eventAggregator,
                    _mtProtoService,
                    updateService,
                    _cacheService,
                    _stateService,
                    _pushService,
                    _navigationService);
            });
        }

        public void Load()
        {
            //Dialogs = IoC.Get<DialogsViewModel>();
            //Calls = IoC.Get<CallsViewModel>();
            //Contacts = IoC.Get<ContactsViewModel>();

            //Items.Add(Dialogs);
            //Items.Add(Calls);
            //Items.Add(Contacts);
            ////#if DEBUG
            ////            Items.Add(_debug);
            ////            Items.Add(_longPoll);
            ////#endif

            //ActivateItem(Dialogs);
        }

        protected override void OnInitialize()
        {
            App.Log("ShellViewModel.OnInitialize start");
            base.OnInitialize();

            Items.Add(Dialogs);
            Items.Add(Calls);
            Items.Add(Contacts);
#if DEBUG
            Items.Add(_debug);
            Items.Add(_longPoll);
#endif
            ActivateItem(Dialogs);

            App.Log("ShellViewModel.OnInitialize stop");
        }

        public static void Navigate(INavigationService navigationService)
        {
            var lastEntry = navigationService.BackStack.LastOrDefault();
            if (lastEntry != null && lastEntry.Source.ToString().Contains("ShellView.xaml"))
            {
                while (navigationService.BackStack.FirstOrDefault() != lastEntry)
                {
                    navigationService.RemoveBackEntry();
                }
                navigationService.GoBack();
            }
            else
            {
                while (navigationService.RemoveBackEntry() != null) { }
                IoC.Get<IStateService>().RemoveBackEntry = true;
                navigationService.UriFor<ShellViewModel>().Navigate();
            }
        }

        protected override void OnActivate()
        {
            _stateService.IsMainViewOpened = true;

            ThreadPool.QueueUserWorkItem(state =>
                _stateService.GetNotifySettingsAsync(settings =>
                {
                    var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);

                    if (isAuthorized && !settings.InvisibleMode)
                    {
                        _mtProtoService.RaiseSendStatus(new SendStatusEventArgs(new TLBool(false)));
                    }
                }));

            if (_stateService.FirstRun)
            {
                _stateService.FirstRun = false;

                Dialogs.FirstRun = true;
                Contacts.FirstRun = true;
                Calls.FirstRun = true;

                _pushService.RegisterDeviceAsync(() => { });
            }

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                _navigationService.RemoveBackEntry();
            }

            NavigateByUserNameAsync();

            base.OnActivate();
        }

        private void NavigateByUserNameAsync()
        {
            Execute.BeginOnThreadPool(() =>
            {
                if (_stateService.Domain != null)
                {
                    var domain = _stateService.Domain;
                    _stateService.Domain = null;

                    MTProtoService.ResolveUsernameAsync(new TLString(domain),
                    result => Execute.BeginOnUIThread(() =>
                    {
                        var peerUser = result.Peer as TLPeerUser;
                        if (peerUser != null)
                        {
                            var user = result.Users.FirstOrDefault();
                            if (user != null)
                            {
                                Contacts.OpenContactDetails(user);
                            }
                        }

                        var peerChannel = result.Peer as TLPeerChannel;
                        var peerChat = result.Peer as TLPeerChat;
                        if (peerChannel != null || peerChat != null)
                        {
                            var channel = result.Chats.FirstOrDefault();
                            if (channel != null)
                            {
                                Dialogs.OpenChatDetails(channel);
                            }
                        }
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST)
                            && TLRPCError.TypeEquals(error, ErrorType.QUERY_TOO_SHORT))
                        {
                            Execute.ShowDebugMessage("contacts.resolveUsername error " + error);
                        }
                        else if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                        {
                            Execute.ShowDebugMessage("contacts.resolveUsername error " + error);
                        }

                        Execute.ShowDebugMessage("contacts.resolveUsername error " + error);
                    }));
                }
            });
        }

        protected override void OnDeactivate(bool close)
        {
            _stateService.IsMainViewOpened = false;

            base.OnDeactivate(close);
        }


        public string SendingText;

        public void Send()
        {

        }

        public void OpenSettings()
        {
            var featuredStickers = StateService.GetFeaturedStickers();
            var allStickers = StateService.GetAllStickers();
            var masks = StateService.GetMasks();
            var user = _cacheService.GetUser(new TLInt(_stateService.CurrentUserId));
            var proxySettings = IoC.Get<ITransportService>().GetProxyConfig();

            _stateService.CurrentContact = user;
            _navigationService.UriFor<SettingsViewModel>().Navigate();
        }

        public void ComposeMessage()
        {
            Dialogs.CreateDialog();
        }

        public void AddContact()
        {
            Contacts.AddContact();
        }

        public void RefreshItems()
        {
            var itemsViewModel = ActiveItem as ItemsViewModelBase;
            if (itemsViewModel != null)
            {
                itemsViewModel.RefreshItems();
            }
        }

        public void Search()
        {
            var dialogs = ActiveItem as DialogsViewModel;
            if (dialogs != null)
            {
                dialogs.Search();
                return;
            }

            var contacts = ActiveItem as ContactsViewModel;
            if (contacts != null)
            {
                contacts.Search();
            }
        }

        public void About()
        {
            _navigationService.UriFor<AboutViewModel>().Navigate();
        }

        public void Add()
        {
            var itemsViewModel = ActiveItem as DialogsViewModel;
            if (itemsViewModel != null)
            {
                ComposeMessage();
            }
            else
            {
                AddContact();
            }
        }

        public void BeginOnThreadPool(System.Action action)
        {
            ThreadPool.QueueUserWorkItem(state => action());
        }

        public static CustomMessageBox ShowCustomMessageBox(string message, string caption, object rightButtonContent, object leftButtonContent = null, Action<CustomMessageBoxResult> dismissed = null, object content = null)
        {
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225 || Environment.OSVersion.Version.Major >= 10;

            var confirmation = new CustomMessageBox
            {
                Caption = caption,
                Message = message,
                Content = content,
                RightButtonContent = isFullHD && rightButtonContent is string ? rightButtonContent.ToString().ToLowerInvariant() : rightButtonContent,
                LeftButtonContent = isFullHD && leftButtonContent is string ? leftButtonContent.ToString().ToLowerInvariant() : leftButtonContent
            };

#if WP8
            if (isFullHD)
            {
                confirmation.Style = (Style)Application.Current.Resources["CustomMessageBoxFullHDStyle"];
            }
#endif
            confirmation.Dismissed += (sender, args) =>
            {
                dismissed.SafeInvoke(args.Result);
            };
            confirmation.Tap += (sender, args) =>
            {
                args.Handled = true;
            };
            confirmation.Show();

            return confirmation;
        }

        public void Resend(TLMessage25 message)
        {
            if (message.Media is TLMessageMediaEmpty)
            {
                DialogDetailsViewModel.SendInternal(message, _mtProtoService);
            }
        }

        public void ChangePasscodeState()
        {
            PasscodeUtils.ChangeLocked();
            NotifyOfPropertyChange(() => PasscodeImageSource);
            NotifyOfPropertyChange(() => PasscodeImageBrush);
        }

        public void Handle(TLDecryptedMessageBase message)
        {
            if (_stateService.SuppressNotifications) return;
            if (message.Out.Value) return;

            var from = _cacheService.GetUser(message.FromId);
            if (from == null) return;

            _stateService.GetNotifySettingsAsync(
               s =>
               {
                   try
                   {
                       var activeDialog = _stateService.ActiveDialog;
                       var toId = message.ChatId;
                       var fromId = message.FromId;
                       var suppressNotification = activeDialog is TLEncryptedChatBase && toId.Value == ((TLEncryptedChatBase)activeDialog).Id.Value;
                       if (suppressNotification) return;

                       var isDisplayedMessage = TLUtils.IsDisplayedDecryptedMessageInternal(message);
                       if (!isDisplayedMessage) return;

                       if (s.InAppMessagePreview)
                       {
                           Execute.BeginOnUIThread(() =>
                           {
                               var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                               if (frame != null)
                               {
                                   var shellView = frame.Content as ShellView;
                                   if (shellView == null)
                                   {
                                       var title = from.FullName;

                                       var text = DialogToBriefInfoConverter.Convert(message, true).Replace("\n", " ");

                                       var toast = new Telegram.Controls.Notifications.ToastPrompt
                                       {
                                           DataContext = from,
                                           TextOrientation = Orientation.Horizontal,
                                           Foreground = new SolidColorBrush(Colors.White),
                                           FontSize = (double)Application.Current.Resources["PhoneFontSizeSmall"],
                                           Title = title,
                                           Message = text,
                                           ImageHeight = 48,
                                           ImageWidth = 48,
                                           ImageSource = new BitmapImage(new Uri("/ToastPromptIcon.png", UriKind.Relative))
                                       };

                                       toast.Tap += (sender, args) =>
                                       {
                                           var dialogDetailsView = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as DialogDetailsView;
                                           if (dialogDetailsView != null)
                                           {
                                               TelegramTransitionService.SetNavigationOutTransition(dialogDetailsView, null);
                                           }

                                           var secretDialogDetailsView = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as SecretDialogDetailsView;
                                           if (secretDialogDetailsView != null)
                                           {
                                               TelegramTransitionService.SetNavigationOutTransition(secretDialogDetailsView, null);
                                           }

                                           var encryptedChat = _cacheService.GetEncryptedChat(message.ChatId);

                                           _stateService.Participant = from;
                                           _stateService.With = encryptedChat;
                                           _stateService.RemoveBackEntries = true;
                                           _navigationService.UriFor<SecretDialogDetailsViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
                                       };

                                       toast.Show();
                                   }
                               }
                           });
                       }

                       if (s.InAppVibration)
                       {
                           VibrateController.Default.Start(TimeSpan.FromMilliseconds(300));
                       }

                       if (s.InAppSound)
                       {
                           var sound = "Sounds/Default.wav";
                           //if (toId is TLPeerEncryptedChat && !string.IsNullOrEmpty(s.GroupSound))
                           //{
                           //    sound = "Sounds/" + s.GroupSound + ".wav";
                           //}
                           //else 
                           if (!string.IsNullOrEmpty(s.ContactSound))
                           {
                               sound = "Sounds/" + s.ContactSound + ".wav";
                           }

                           //if (toId is TLPeerChat && chat != null && chat.NotifySettings is TLPeerNotifySettings)
                           //{
                           //    sound = "Sounds/" + ((TLPeerNotifySettings)chat.NotifySettings).Sound.Value + ".wav";
                           //}
                           //else 
                           var notifySettings = from.NotifySettings as TLPeerNotifySettings;
                           if (from != null && notifySettings != null && !TLString.IsNullOrEmpty(notifySettings.Sound))
                           {
                               sound = "Sounds/" + notifySettings.Sound.Value + ".wav";
                           }

                           if (!Telegram.Api.Helpers.Utils.XapContentFileExists(sound))
                           {
                               sound = "Sounds/Default.wav";
                           }

                           var stream = TitleContainer.OpenStream(sound);
                           var effect = SoundEffect.FromStream(stream);

                           FrameworkDispatcher.Update();
                           effect.Play();
                       }

                   }
                   catch (Exception e)
                   {
                       TLUtils.WriteException("ShellViewModel.Handle(TLDecryptedMessage)", e);
                   }

               });
        }

        public void Handle(TLMessageCommon message)
        {
            if (LocationPicker != null)
            {
                var message70 = message as TLMessage70;
                if (message70 != null && !message70.Out.Value)
                {
                    var mediaGeoLive = message70.Media as TLMessageMediaGeoLive;
                    if (mediaGeoLive != null)
                    {
                        mediaGeoLive.EditDate = message70.EditDate;
                        mediaGeoLive.Date = message70.Date;
                        if (mediaGeoLive.Active)
                        {
                            if (LocationPicker != null)
                            {
                                var currentMessage = LocationPicker.MessageGeoLive as TLMessage70;
                                if (currentMessage != null)
                                {
                                    var currentPeerUser = currentMessage.ToId as TLPeerUser;
                                    var peerUser = message70.ToId as TLPeerUser;
                                    if (peerUser != null
                                        && currentPeerUser != null
                                        && message.FromId != null
                                        && currentPeerUser.Id.Value == message.FromId.Value)
                                    {
                                        Execute.BeginOnUIThread(() => LocationPicker.UpdateLiveLocation(message70));
                                        return;
                                    }

                                    var currentPeerChat = currentMessage.ToId as TLPeerChat;
                                    var peerChat = message70.ToId as TLPeerChannel;
                                    if (peerChat != null
                                        && currentPeerChat != null
                                        && peerChat.Id.Value == currentPeerChat.Id.Value)
                                    {
                                        Execute.BeginOnUIThread(() => LocationPicker.UpdateLiveLocation(message70));
                                        return;
                                    }

                                    var currentPeerChannel = currentMessage.ToId as TLPeerChannel;
                                    var peerChannel = message70.ToId as TLPeerChannel;
                                    if (peerChannel != null
                                        && currentPeerChannel != null
                                        && peerChannel.Id.Value == currentPeerChannel.Id.Value)
                                    {
                                        Execute.BeginOnUIThread(() => LocationPicker.UpdateLiveLocation(message70));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (_stateService.SuppressNotifications)
            {
                //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[StateService.SuppressNotification=true] msg_id={0}", message.Id));
                return;
            }
            if (message.Out.Value)
            {
                //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[message.Out=true] msg_id={0}", message.Id));
                return;
            }
            if (!message.Unread.Value)
            {
                //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[message.Unread=false] msg_id={0}", message.Id));
                return;
            }

            var message48 = message as TLMessage48;
            if (message48 != null && message48.Silent)
            {
                //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[message.Silent=true] msg_id={0}", message.Id));
                return;
            }

            TLUserBase from = null;
            if (message.FromId != null && message.FromId.Value >= 0)
            {
                from = _cacheService.GetUser(message.FromId);
                if (from == null)
                {
                    //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[CacheService.GetUser(message.FromId)=null] msg_id={0} from_id={1}", message.Id, message.FromId));
                    return;
                }
            }

            _stateService.GetNotifySettingsAsync(
                s =>
                {
                    try
                    {
                        var activeDialog = _stateService.ActiveDialog;
                        var toId = message.ToId;
                        var fromId = message.FromId;
                        var suppressNotification = false;
                        TLDialogBase dialog = null;

                        if (toId is TLPeerChat
                            && activeDialog is TLChat
                            && toId.Id.Value == ((TLChat)activeDialog).Id.Value)
                        {
                            //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[activeDialog(TLChat)=message.ToId] msg_id={0} to_id={1}", message.Id, message.ToId));
                            suppressNotification = true;
                        }
                        if (toId is TLPeerChannel
                            && activeDialog is TLChannel
                            && toId.Id.Value == ((TLChannel)activeDialog).Id.Value)
                        {
                            //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[activeDialog(TLChannel)=message.ToId] msg_id={0} to_id={1}", message.Id, message.ToId));
                            suppressNotification = true;
                        }
                        else if (toId is TLPeerUser
                            && activeDialog is TLUserBase
                            && ((from != null && from.IsSelf) || fromId.Value == ((TLUserBase)activeDialog).Id.Value))
                        {
                            //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[activeDialog(TLUser)=message.ToId] msg_id={0} to_id={1}", message.Id, message.ToId));
                            suppressNotification = true;
                        }

                        if (suppressNotification) return;

                        TLChatBase chat = null;
                        TLUserBase user = null;
                        TLChannel channel = null;
                        if (message.ToId is TLPeerChat)
                        {
                            chat = _cacheService.GetChat(message.ToId.Id);
                            dialog = _cacheService.GetDialog(new TLPeerChat { Id = message.ToId.Id });
                        }
                        else if (message.ToId is TLPeerChannel)
                        {
                            chat = _cacheService.GetChat(message.ToId.Id);
                            channel = chat as TLChannel;
                            dialog = _cacheService.GetDialog(new TLPeerChannel { Id = message.ToId.Id });
                        }
                        else
                        {
                            if (message.Out.Value)
                            {
                                user = _cacheService.GetUser(message.ToId.Id);
                                dialog = _cacheService.GetDialog(new TLPeerUser { Id = message.ToId.Id });
                            }
                            else
                            {
                                user = _cacheService.GetUser(message.FromId);
                                dialog = _cacheService.GetDialog(new TLPeerUser { Id = message.FromId });
                            }
                        }

                        var now = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
                        if (chat != null)
                        {
                            var notifySettingsBase = chat.NotifySettings;
                            if (notifySettingsBase == null)
                            {
                                notifySettingsBase = dialog != null ? dialog.NotifySettings : null;
                            }

                            if (notifySettingsBase == null)
                            {
                                if (channel != null)
                                {
                                    MTProtoService.GetFullChannelAsync(channel.ToInputChannel(),
                                        chatFull =>
                                        {
                                            chat.NotifySettings = chatFull.FullChat.NotifySettings;
                                            if (dialog != null)
                                            {
                                                dialog.NotifySettings = chatFull.FullChat.NotifySettings;
                                                Execute.BeginOnUIThread(() =>
                                                {
                                                    dialog.NotifyOfPropertyChange(() => dialog.NotifySettings);
                                                    dialog.NotifyOfPropertyChange(() => dialog.Self);
                                                });
                                            }
                                        });
                                }
                                else
                                {
                                    MTProtoService.GetFullChatAsync(chat.Id,
                                        chatFull =>
                                        {
                                            chat.NotifySettings = chatFull.FullChat.NotifySettings;
                                            if (dialog != null)
                                            {
                                                dialog.NotifySettings = chatFull.FullChat.NotifySettings;
                                                Execute.BeginOnUIThread(() =>
                                                {
                                                    dialog.NotifyOfPropertyChange(() => dialog.NotifySettings);
                                                    dialog.NotifyOfPropertyChange(() => dialog.Self);
                                                });
                                            }
                                        });
                                }
                            }

                            var alert = StateService.GetNotifySettings().GroupAlert;
                            var notifySettings = notifySettingsBase as TLPeerNotifySettings;
                            suppressNotification =
                                notifySettings == null
                                || notifySettings.MuteUntil != null && notifySettings.MuteUntil.Value > now.Value
                                || notifySettings.MuteUntil == null && !alert;
                            if (suppressNotification)
                            {
                                //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[muteUntil > now] msg_id={0} mute_until={1} now={2}", message.Id, notifySettings != null ? notifySettings.MuteUntil : null, now));
                            }
                        }

                        if (user != null)
                        {
                            var notifySettingsBase = user.NotifySettings;
                            if (notifySettingsBase == null)
                            {
                                notifySettingsBase = dialog != null ? dialog.NotifySettings : null;
                            }

                            if (notifySettingsBase == null)
                            {
                                MTProtoService.GetFullUserAsync(user.ToInputUser(),
                                    userFull =>
                                    {
                                        user.NotifySettings = userFull.NotifySettings;
                                        if (dialog != null)
                                        {
                                            dialog.NotifySettings = userFull.NotifySettings;
                                            Execute.BeginOnUIThread(() =>
                                            {
                                                dialog.NotifyOfPropertyChange(() => dialog.NotifySettings);
                                                dialog.NotifyOfPropertyChange(() => dialog.Self);
                                            });
                                        }
                                    });
                            }

                            var alert = StateService.GetNotifySettings().ContactAlert;
                            var notifySettings = notifySettingsBase as TLPeerNotifySettings;
                            suppressNotification = user.IsSelf
                                || notifySettings == null
                                || notifySettings.MuteUntil != null && notifySettings.MuteUntil.Value > now.Value
                                || notifySettings.MuteUntil == null && !alert;
                            if (suppressNotification)
                            {
                                //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[muteUntil > now] msg_id={0} mute_until={1} now={2}", message.Id, notifySettings != null ? notifySettings.MuteUntil : null, now));
                            }
                        }

                        if (suppressNotification) return;

                        if (dialog != null)
                        {
                            suppressNotification = CheckLastNotificationTime(dialog, now);
                        }

                        if (suppressNotification)
                        {
                            //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[CheckLastNotificationTime] msg_id={0}", message.Id));
                            return;
                        }

                        if (s.InAppMessagePreview)
                        {
                            Execute.BeginOnUIThread(() =>
                            {
                                var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                                if (frame != null)
                                {
                                    var webVerificationView = frame.Content as WebVerificationView;
                                    var shellView = frame.Content as ShellView;
                                    if (shellView == null && webVerificationView == null)
                                    {
                                        var title = message.ToId is TLPeerChat || message.ToId is TLPeerChannel
                                            ? from == null ? chat.FullName : string.Format("{0}@{1}", from.FullName, chat.FullName)
                                            : from.FullName;

                                        var text = DialogToBriefInfoConverter.Convert(message, true).Replace("\n", " ");
                                        var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
                                        var toast = new Telegram.Controls.Notifications.ToastPrompt
                                        {
                                            DataContext = message,//(message.ToId is TLPeerChat || message.ToId is TLPeerChannel)? (TLObject)chat : from,
                                            TextOrientation = Orientation.Horizontal,
                                            Foreground = new SolidColorBrush(Colors.White),
                                            FontSize = (double)Application.Current.Resources["PhoneFontSizeSmall"],
                                            Title = title,
                                            Message = text,
                                            ImageHeight = 48,
                                            ImageWidth = 48,
                                            ImageSource = new BitmapImage(new Uri("/ToastPromptIcon.png", UriKind.Relative)),
                                            Overlay = isLightTheme ? (Brush)Application.Current.Resources["InputBorderBrushLight"] : (Brush)Application.Current.Resources["InputBorderBrushDark"]
                                        };

                                        toast.Tap += (sender, args) =>
                                        {
                                            var dialogDetailsView = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as DialogDetailsView;
                                            if (dialogDetailsView != null)
                                            {
                                                TelegramTransitionService.SetNavigationOutTransition(dialogDetailsView, null);
                                            }

                                            var secretDialogDetailsView = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as SecretDialogDetailsView;
                                            if (secretDialogDetailsView != null)
                                            {
                                                TelegramTransitionService.SetNavigationOutTransition(secretDialogDetailsView, null);
                                            }

                                            _stateService.With = message.ToId is TLPeerChat || message.ToId is TLPeerChannel ? (TLObject)chat : from;
                                            _stateService.RemoveBackEntries = true;
                                            _navigationService.UriFor<DialogDetailsViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
                                        };

                                        toast.Show();
                                    }
                                }
                            });
                        }

                        if (_lastNotificationTime.HasValue)
                        {
                            var fromLastNotification = (DateTime.Now - _lastNotificationTime.Value).TotalSeconds;
                            if (fromLastNotification > 0.0 && fromLastNotification < 2.0)
                            {
                                suppressNotification = true;
                            }
                        }
                        _lastNotificationTime = DateTime.Now;

                        if (suppressNotification)
                        {
                            Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[lastNotificationTime] msg_id={0} last_notification_time={1}, now={2}", message.Id, _lastNotificationTime, DateTime.Now));
                            return;
                        }


                        if (s.InAppVibration)
                        {
                            VibrateController.Default.Start(TimeSpan.FromMilliseconds(300));
                        }

                        if (s.InAppSound)
                        {
                            var sound = "Sounds/Default.wav";
                            if (toId is TLPeerChat && !string.IsNullOrEmpty(s.GroupSound))
                            {
                                sound = "Sounds/" + s.GroupSound + ".wav";
                            }
                            else if (!string.IsNullOrEmpty(s.ContactSound))
                            {
                                sound = "Sounds/" + s.ContactSound + ".wav";
                            }
                            var chatNotifySettings = chat.NotifySettings as TLPeerNotifySettings;
                            var userNotifySettings = user.NotifySettings as TLPeerNotifySettings;
                            if (toId is TLPeerChat && chat != null && chatNotifySettings != null && !TLString.IsNullOrEmpty(chatNotifySettings.Sound))
                            {
                                sound = "Sounds/" + chatNotifySettings.Sound.Value + ".wav";
                            }
                            else if (toId is TLPeerUser && user != null && userNotifySettings != null && !TLString.IsNullOrEmpty(userNotifySettings.Sound))
                            {
                                sound = "Sounds/" + userNotifySettings.Sound.Value + ".wav";
                            }

                            if (!Telegram.Api.Helpers.Utils.XapContentFileExists(sound))
                            {
                                sound = "Sounds/Default.wav";
                            }

                            var stream = TitleContainer.OpenStream(sound);
                            var effect = SoundEffect.FromStream(stream);

                            FrameworkDispatcher.Update();
                            effect.Play();
                        }

                    }
                    catch (Exception e)
                    {
                        TLUtils.WriteLine(e.ToString(), LogSeverity.Error);
                    }

                });
        }

        private bool CheckLastNotificationTime(TLDialogBase dialog, TLInt now)
        {
            if (dialog != null)
            {
                var alert = dialog.Peer is TLPeerUser
                    ? StateService.GetNotifySettings().ContactAlert
                    : StateService.GetNotifySettings().GroupAlert;
                var notifySettings = dialog.NotifySettings as TLPeerNotifySettings;
                if (notifySettings != null
                    && (notifySettings.MuteUntil != null && notifySettings.MuteUntil.Value > now.Value      // muted chat
                        || notifySettings.MuteUntil == null && !alert))
                {
                    //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[CheckLastNotificationTime 1] mute_until={0} now={1}", notifySettings.MuteUntil, now));
                    dialog.LastNotificationTime = null;
                    dialog.UnmutedCount = 0;
                    return true;
                }

                if (dialog.LastNotificationTime == null)
                {
                    dialog.LastNotificationTime = DateTime.Now;
                    dialog.UnmutedCount = 1;
                    return false;
                }
                else
                {
                    var interval = (DateTime.Now - dialog.LastNotificationTime.Value).TotalSeconds;
                    if (interval <= Constants.NotificationInterval)
                    {
                        var unmutedCount = dialog.UnmutedCount;
                        if (unmutedCount < Constants.UnmutedCount)
                        {
                            dialog.UnmutedCount++;
                            return false;
                        }
                        else
                        {
                            //Telegram.Logs.Log.Write(string.Format("Cancel notification reason=[CheckLastNotificationTime 2] last_notificaiton_time={0} now={1} interval={2}", dialog.LastNotificationTime, DateTime.Now, interval));
                            dialog.UnmutedCount++;
                            return true;
                        }
                    }
                    else
                    {
                        dialog.LastNotificationTime = DateTime.Now;
                        dialog.UnmutedCount = 1;
                        return false;
                    }
                }
            }

            return false;
        }

        private DateTime? _lastNotificationTime;
        private ConnectionProfile _profile;
        private NetworkConnectivityLevel? _connectivityLevel;

        public void Review()
        {
            new MarketplaceReviewTask().Show();
        }

        public void OpenKey()
        {
            _mtProtoService.GetConfigInformationAsync(info =>
            {
                Execute.BeginOnUIThread(() =>
                {
                    MessageBox.Show(info);
                });
            });
        }

        public void GetCurrentPacketInfo()
        {
            var packetInfo = _mtProtoService.GetTransportInfo();

            MessageBox.Show(packetInfo);
        }

        public void PingDelayDisconnect(int disconnectDelay)
        {
            MTProtoService.PingDelayDisconnectAsync(TLLong.Random(), new TLInt(disconnectDelay),
                result => Execute.ShowDebugMessage("pingDelayDisconnect result: pong" + result.PingId.Value),
                error => Execute.ShowDebugMessage("pingDelayDisconnect error: " + error));
        }

        public void Handle(UpdatingEventArgs args)
        {
            if (_mtProtoService != null)
            {
                var timeout = 5.0;
#if DEBUG
                timeout = 5.0;
#endif
                _mtProtoService.SetMessageOnTime(timeout, AppResources.Updating + "...");
            }
        }

        public void Handle(UpdateCompletedEventArgs args)
        {
            if (_mtProtoService != null)
            {
                _mtProtoService.SetMessageOnTime(0.0, string.Empty);
            }

            UpdateChannels(args.UpdateChannelTooLongList);
        }

        private void UpdateChannels(IList<TLUpdateChannelTooLong> updateChannelTooLongList)
        {
            var info = new StringBuilder();
            if (updateChannelTooLongList != null)
            {
                foreach (var item in updateChannelTooLongList)
                {
                    info.AppendLine(item.ToString());
                }
            }
            Telegram.Logs.Log.Write("ShellViewModel.UpdateChannels start count=" + (updateChannelTooLongList != null ? updateChannelTooLongList.Count.ToString() : "null") + "\n" + info);

            if (updateChannelTooLongList != null)
            {
                foreach (var updateChannelTooLong in updateChannelTooLongList)
                {
                    Handle(updateChannelTooLong);
                }
            }
        }

        public void Handle(UpdateChannelsEventArgs args)
        {
            Telegram.Logs.Log.Write("ShellViewModel.Handle UpdateChannelsEventArgs start");

            UpdateChannels(args.UpdateChannelTooLongList);
        }

        public void Handle(DownloadableItem item)
        {
            var sticker = item.Owner as TLStickerItem;
            if (sticker != null)
            {
                sticker.NotifyOfPropertyChange(() => sticker.Self);
            }

            var document = item.Owner as TLDocument22;
            if (document != null)
            {
                var stickerSet = TelegramViewBase._stickerSet;
                if (stickerSet != null)
                {
                    for (var i = 0; i < stickerSet.Stickers.Count; i++)
                    {
                        var stickerItem = stickerSet.Stickers[i] as TLStickerItem;
                        if (stickerItem != null && stickerItem.Document == document)
                        {
                            stickerItem.NotifyOfPropertyChange(() => stickerItem.Document);
                        }
                    }
                }

                EmojiControl instance;
                if (EmojiControl.TryGetInstance(out instance))
                {
                    CheckEmojiSprites(document, instance.CurrentSprites);
                    CheckEmojiSprites(document, instance.SearchSprites);
                }
            }

            var inlineMediaResult = item.Owner as TLBotInlineMediaResult;
            if (inlineMediaResult != null)
            {
                inlineMediaResult.NotifyOfPropertyChange(() => inlineMediaResult.Self);
            }
        }

        private static void CheckEmojiSprites(TLDocument22 document, List<VListItemBase> sprites)
        {
            if (sprites != null)
            {
                for (var i = 0; i < sprites.Count; i++)
                {
                    var stickerSprite = sprites[i] as StickerSpriteItem;
                    if (stickerSprite != null && stickerSprite.Stickers != null)
                    {
                        for (var j = 0; j < stickerSprite.Stickers.Count; j++)
                        {
                            var stickerItem = stickerSprite.Stickers[j];
                            if (stickerItem != null && stickerItem.Document == document)
                            {
                                stickerItem.NotifyOfPropertyChange(() => stickerItem.Document);
                            }
                        }
                    }
                }
            }
        }

        public void Handle(TLUpdateContactRegistered contactRegistered)
        {
            StateService.GetNotifySettingsAsync(result =>
            {
                if (result.ContactJoined)
                {
                    var user = _cacheService.GetUser(contactRegistered.UserId);

                    if (user == null)
                    {
                        MTProtoService.GetFullUserAsync(new TLInputUser { UserId = contactRegistered.UserId, AccessHash = new TLLong(0) },
                            userFull =>
                            {
                                user = userFull.ToUser();
                                CreateContactRegisteredMessage(contactRegistered);
                            },
                            error =>
                            {

                            });
                    }
                    else
                    {
                        CreateContactRegisteredMessage(contactRegistered);
                    }
                }
            });
        }

        private void CreateContactRegisteredMessage(TLUpdateContactRegistered updateContactRegistered)
        {
            var user = _cacheService.GetUser(updateContactRegistered.UserId);

            if (user != null)
            {
                var currentUserId = MTProtoService.CurrentUserId;
                var message = new TLMessageService17
                {
                    Flags = new TLInt(0),
                    Id = new TLInt(0),
                    FromId = user.Id,
                    ToId = new TLPeerUser { Id = currentUserId },
                    Status = MessageStatus.Confirmed,
                    Out = TLBool.False,
                    Unread = TLBool.False,
                    Date = updateContactRegistered.Date,
                    Action = new TLMessageActionContactRegistered { UserId = user.Id },
                    RandomId = TLLong.Random()
                };

                _eventAggregator.Publish(user);

                var dialog = _cacheService.GetDialog(new TLPeerUser { Id = user.Id });
                if (dialog == null)
                {
                    _cacheService.SyncMessage(message, true, true,
                        cachedMessage =>
                        {
                            _eventAggregator.Publish(cachedMessage);
                        });
                }
            }
        }

        public void Handle(ExceptionInfo info)
        {
            BugSenseWrapper.LogError(info.Exception, info.Caption, new NotificationOptions { Type = enNotificationType.None });
        }

        public void Handle(TLUpdateChannelTooLong updateChannelTooLong)
        {
            //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={0} TLUpdateChannelTooLong", updateChannelTooLong.ChannelId));

            var channel = _cacheService.GetChat(updateChannelTooLong.ChannelId) as TLChannel49;
            if (channel != null && !channel.Min)
            {
                //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={0} TLUpdateChannelTooLong channel!=null", updateChannelTooLong.ChannelId));

                var pts = channel.Pts;
                if (pts == null)
                {
                    var dialogPts = _cacheService.GetDialog(new TLPeerChannel { Id = channel.Id }) as IDialogPts;
                    if (dialogPts != null)
                    {
                        pts = dialogPts.Pts;
                    }
                }

                if (pts != null)
                {
                    //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={1} TLUpdateChannelTooLong GetChannelDifference pts={0}", pts, updateChannelTooLong.ChannelId));
                    pts = new TLInt(pts.Value - 10 > 0 ? pts.Value - 10 : 1);
                    //Execute.ShowDebugMessage("updates.getChannelDifference channel_id=" + channel.Index + " pts=" + pts);

                    MTProtoService.GetChannelDifferenceAsync(true, channel.ToInputChannel(), new TLChannelMessagesFilterEmpty(), pts, new TLInt(1),
                        result =>
                        {
                            //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={1} TLUpdateChannelTooLong GetChannelDifference result pts={0}", result.Pts, updateChannelTooLong.ChannelId));
                        },
                        error =>
                        {
                            //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={1} TLUpdateChannelTooLong GetChannelDifference error={0}", error, updateChannelTooLong.ChannelId));
                        });
                }


                //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={0} TLUpdateChannelTooLong GetFullChannel start", updateChannelTooLong.ChannelId));
                _mtProtoService.GetFullChannelAsync(channel.ToInputChannel(),
                    messagesFull =>
                    {
                        //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={0} TLUpdateChannelTooLong GetFullChannel result", updateChannelTooLong.ChannelId));

                        var dialog = _cacheService.GetDialog(new TLPeerChannel { Id = channel.Id }) as TLDialog24;
                        if (dialog != null)
                        {
                            var channelFull = messagesFull.FullChat as TLChannelFull41;
                            if (channelFull != null)
                            {
                                //dialog.UnreadCount = channelFull.UnreadCount;
                                dialog.ReadInboxMaxId = channelFull.ReadInboxMaxId;
                                dialog.NotifySettings = channelFull.NotifySettings;
                                Execute.BeginOnUIThread(() =>
                                {
                                    dialog.NotifyOfPropertyChange(() => dialog.NotifySettings);
                                    dialog.NotifyOfPropertyChange(() => dialog.Self);
                                });

                                var dialogChannel = dialog as TLDialogChannel;
                                if (dialogChannel != null)
                                {
                                    dialogChannel.UnreadImportantCount = channelFull.UnreadImportantCount;
                                }

                                _cacheService.Commit();
                            }
                        }

                        //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={0} TLUpdateChannelTooLong GetChannelHistory start", updateChannelTooLong.ChannelId));

                        _mtProtoService.GetChannelHistoryAsync("ShellViewModel.Handle TLUpdateChannelTooLong", channel.ToInputPeer(), new TLPeerChannel { Id = channel.Id }, true, new TLInt(0), new TLInt(0), new TLInt(Constants.MessagesSlice),
                            result =>
                            {
                                //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={1} TLUpdateChannelTooLong GetChannelHistory result={0}", result.Messages.Count, updateChannelTooLong.ChannelId));

                                var topMessage = result.Messages.FirstOrDefault();
                                dialog = _cacheService.GetDialog(new TLPeerChannel { Id = channel.Id }) as TLDialog24;
                                if (dialog != null && topMessage != null)
                                {
                                    var channelFull = messagesFull.FullChat as TLChannelFull41;
                                    if (channelFull != null)
                                    {
                                        dialog.UnreadCount = channelFull.UnreadCount;
                                    }

                                    dialog.TopMessageId = topMessage.Id;
                                    var dialogChannel = dialog as TLDialogChannel;
                                    if (dialogChannel != null)
                                    {
                                        dialogChannel.TopImportantMessageId = topMessage.Id;
                                    }

                                    _cacheService.Commit();

                                    Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={0} TLUpdateChannelTooLong publish ChannelUpdateCompletedEventArgs", updateChannelTooLong.ChannelId));

                                    _eventAggregator.Publish(new ChannelUpdateCompletedEventArgs { ChannelId = channel.Id });
                                }
                            },
                            error =>
                            {
                                //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle GetChannelHistory error={0} channel_id={1}", error, updateChannelTooLong.ChannelId));
                            });
                    },
                    error =>
                    {
                        //Telegram.Logs.Log.Write(string.Format("  ShellViewModel.Handle channel_id={1} TLUpdateChannelTooLong GetFullChannel error={0}", error, updateChannelTooLong.ChannelId));
                    });
            }
            else
            {
                //var updatesService = IoC.Get<IUpdatesService>();
                //updatesService.LoadStateAndUpdate(() =>
                //{
                //    Execute.ShowDebugMessage("Handle(TLUpdateChannelTooLong) UpdatesService.LoadStateAndUpdateCompleted");
                //});
            }
        }

        public void Handle(TLUpdateFavedStickers updateFavedStickers)
        {
            StateService.GetAllStickersAsync(cachedStickers =>
            {
                var cachedStickers43 = cachedStickers as TLAllStickers43;
                if (cachedStickers43 != null)
                {
                    var hash = cachedStickers43.FavedStickers != null ? cachedStickers43.FavedStickers.Hash : new TLInt(0);

                    MTProtoService.GetFavedStickersAsync(hash,
                        result =>
                        {
                            var favedStickers = result as TLFavedStickers;
                            if (favedStickers != null)
                            {
                                cachedStickers43.FavedStickers = favedStickers;
                                StateService.SaveAllStickersAsync(cachedStickers43);

                                Execute.BeginOnUIThread(() =>
                                {
                                    EmojiControl emojiControl;
                                    if (EmojiControl.TryGetInstance(out emojiControl))
                                    {
                                        emojiControl.ResetFavedStickers();
                                    }
                                });
                            }
                        },
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.getFavedStickers error " + error);
                        });
                }
            });
        }

        public void Handle(TLUpdateStickerSets updateStickerSets)
        {
            StateService.GetMasksAsync(cachedMasks =>
            {
                var hash = cachedMasks != null ? cachedMasks.Hash : TLString.Empty;

                MTProtoService.GetMaskStickersAsync(hash,
                    result =>
                    {
                        var masks = result as TLAllStickers43;
                        if (masks != null)
                        {
                            var masks29 = cachedMasks as TLAllStickers29;
                            if (masks29 != null)
                            {
                                masks.ShowStickersTab = masks29.ShowStickersTab;
                                masks.RecentlyUsed = masks29.RecentlyUsed;
                                masks.Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
                            }
                            var cachedMasks43 = cachedMasks as TLAllStickers43;
                            if (cachedMasks43 != null)
                            {
                                masks.RecentStickers = cachedMasks43.RecentStickers;
                            }

                            StateService.SaveMasksAsync(masks);

                            Execute.BeginOnUIThread(() =>
                            {
                                _eventAggregator.Publish(new UpdateStickerSetsEventArgs(masks, true));

                                //EmojiControl emojiControl;
                                //if (EmojiControl.TryGetInstance(out emojiControl))
                                //{
                                //    emojiControl.ResetStickerSets();
                                //}
                            });
                        }
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getAllStickers error " + error);
                    });
            });

            StateService.GetAllStickersAsync(cachedStickers =>
            {
                MTProtoService.GetAllStickersAsync(cachedStickers.Hash,
                    result =>
                    {
                        var allStickers = result as TLAllStickers43;
                        if (allStickers != null)
                        {
                            var cachedStickers29 = cachedStickers as TLAllStickers29;
                            if (cachedStickers29 != null)
                            {
                                allStickers.ShowStickersTab = cachedStickers29.ShowStickersTab;
                                allStickers.RecentlyUsed = cachedStickers29.RecentlyUsed;
                                allStickers.Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
                            }
                            var cachedStickers43 = cachedStickers as TLAllStickers43;
                            if (cachedStickers43 != null)
                            {
                                allStickers.RecentStickers = cachedStickers43.RecentStickers;
                                allStickers.FavedStickers = cachedStickers43.FavedStickers;
                            }

                            StateService.SaveAllStickersAsync(allStickers);

                            Execute.BeginOnUIThread(() =>
                            {
                                _eventAggregator.Publish(new UpdateStickerSetsEventArgs(allStickers, false));

                                EmojiControl emojiControl;
                                if (EmojiControl.TryGetInstance(out emojiControl))
                                {
                                    emojiControl.ResetStickerSets();
                                }
                            });
                        }
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getAllStickers error " + error);
                    });
            });
        }

        public void Handle(TLUpdateStickerSetsOrder updateStickerSetsOrder)
        {
            var updateStickerSetsOrder56 = updateStickerSetsOrder as TLUpdateStickerSetsOrder56;
            if (updateStickerSetsOrder56 == null) return;

            if (updateStickerSetsOrder56.Masks)
            {
                StateService.GetMasksAsync(cachedMasks =>
                {
                    var masks = cachedMasks as TLAllStickers29;
                    if (masks != null)
                    {
                        var order = updateStickerSetsOrder.Order;

                        var setsDict = new Dictionary<long, TLStickerSetBase>();
                        for (var i = 0; i < masks.Sets.Count; i++)
                        {
                            setsDict[masks.Sets[i].Id.Value] = masks.Sets[i];
                        }

                        var sets = new TLVector<TLStickerSetBase>();
                        for (var i = 0; i < order.Count; i++)
                        {
                            TLStickerSetBase stickerSet;
                            if (setsDict.TryGetValue(order[i].Value, out stickerSet))
                            {
                                sets.Add(stickerSet);
                            }
                        }

                        masks.Sets = new TLVector<TLStickerSetBase>(sets);
                        var newHash = TLUtils.GetAllStickersHash(masks.Sets);
                        masks.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));

                        var documentSets = new Dictionary<long, TLVector<TLDocumentBase>>();
                        for (var i = 0; i < masks.Documents.Count; i++)
                        {
                            var document22 = masks.Documents[i] as TLDocument22;
                            if (document22 != null)
                            {
                                var stickerSetId = document22.StickerSet as TLInputStickerSetId;
                                if (stickerSetId != null)
                                {
                                    TLVector<TLDocumentBase> stickers;
                                    if (documentSets.TryGetValue(stickerSetId.Id.Value, out stickers))
                                    {
                                        stickers.Add(document22);
                                    }
                                    else
                                    {
                                        documentSets[stickerSetId.Id.Value] = new TLVector<TLDocumentBase> { document22 };
                                    }
                                }
                            }
                        }
                        var documents = new TLVector<TLDocumentBase>();
                        for (var i = 0; i < masks.Sets.Count; i++)
                        {
                            TLVector<TLDocumentBase> stickers;
                            if (documentSets.TryGetValue(masks.Sets[i].Id.Value, out stickers))
                            {
                                foreach (var sticker in stickers)
                                {
                                    documents.Add(sticker);
                                }
                            }
                        }
                        masks.Documents = documents;
                        StateService.SaveMasksAsync(masks);

                        Execute.BeginOnUIThread(() =>
                        {
                            _eventAggregator.Publish(new UpdateStickerSetsOrderEventArgs(masks, true));

                            //EmojiControl emojiControl;
                            //if (EmojiControl.TryGetInstance(out emojiControl))
                            //{
                            //    emojiControl.ReorderStickerSets();
                            //}
                        });
                    }
                });
            }
            else
            {
                StateService.GetAllStickersAsync(cachedStickers =>
                {
                    var allStickers = cachedStickers as TLAllStickers29;
                    if (allStickers != null)
                    {
                        var order = updateStickerSetsOrder.Order;

                        var setsDict = new Dictionary<long, TLStickerSetBase>();
                        for (var i = 0; i < allStickers.Sets.Count; i++)
                        {
                            setsDict[allStickers.Sets[i].Id.Value] = allStickers.Sets[i];
                        }

                        var sets = new TLVector<TLStickerSetBase>();
                        for (var i = 0; i < order.Count; i++)
                        {
                            TLStickerSetBase stickerSet;
                            if (setsDict.TryGetValue(order[i].Value, out stickerSet))
                            {
                                sets.Add(stickerSet);
                            }
                        }

                        allStickers.Sets = new TLVector<TLStickerSetBase>(sets);
                        var newHash = TLUtils.GetAllStickersHash(allStickers.Sets);
                        allStickers.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));

                        var documentSets = new Dictionary<long, TLVector<TLDocumentBase>>();
                        for (var i = 0; i < allStickers.Documents.Count; i++)
                        {
                            var document22 = allStickers.Documents[i] as TLDocument22;
                            if (document22 != null)
                            {
                                var stickerSetId = document22.StickerSet as TLInputStickerSetId;
                                if (stickerSetId != null)
                                {
                                    TLVector<TLDocumentBase> stickers;
                                    if (documentSets.TryGetValue(stickerSetId.Id.Value, out stickers))
                                    {
                                        stickers.Add(document22);
                                    }
                                    else
                                    {
                                        documentSets[stickerSetId.Id.Value] = new TLVector<TLDocumentBase> { document22 };
                                    }
                                }
                            }
                        }
                        var documents = new TLVector<TLDocumentBase>();
                        for (var i = 0; i < allStickers.Sets.Count; i++)
                        {
                            TLVector<TLDocumentBase> stickers;
                            if (documentSets.TryGetValue(allStickers.Sets[i].Id.Value, out stickers))
                            {
                                foreach (var sticker in stickers)
                                {
                                    documents.Add(sticker);
                                }
                            }
                        }
                        allStickers.Documents = documents;
                        StateService.SaveAllStickersAsync(allStickers);

                        Execute.BeginOnUIThread(() =>
                        {
                            _eventAggregator.Publish(new UpdateStickerSetsOrderEventArgs(allStickers, false));

                            EmojiControl emojiControl;
                            if (EmojiControl.TryGetInstance(out emojiControl))
                            {
                                emojiControl.ReorderStickerSets();
                            }
                        });
                    }
                });
            }
        }

        public void Handle(TLUpdateNewStickerSet updateNewStickerSet)
        {
            var stickerSet32 = updateNewStickerSet.Stickerset.Set as TLStickerSet32;
            if (stickerSet32 != null
                && stickerSet32.Masks)
            {
                StateService.GetMasksAsync(cachedMasks =>
                {
                    var masks29 = cachedMasks as TLAllStickers29;
                    if (masks29 != null)
                    {
                        var documents = new TLVector<TLStickerItem>();
                        foreach (var document in updateNewStickerSet.Stickerset.Documents)
                        {
                            documents.Add(new TLStickerItem { Document = document });
                        }

                        masks29.Sets.Insert(0, updateNewStickerSet.Stickerset.Set);
                        var newHash = TLUtils.GetAllStickersHash(masks29.Sets);
                        masks29.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));

                        for (int i = 0; i < updateNewStickerSet.Stickerset.Documents.Count; i++)
                        {
                            masks29.Documents.Insert(i, updateNewStickerSet.Stickerset.Documents[i]);
                        }

                        var packsDict = new Dictionary<string, TLStickerPack>();
                        for (var i = 0; i < masks29.Packs.Count; i++)
                        {
                            packsDict[masks29.Packs[i].Emoticon.ToString()] = masks29.Packs[i];
                        }

                        for (var i = 0; i < updateNewStickerSet.Stickerset.Documents.Count; i++)
                        {
                            var document22 = masks29.Documents[i] as TLDocument22;
                            if (document22 != null)
                            {
                                var documentAttributeSticker = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker29) as TLDocumentAttributeSticker29;
                                if (documentAttributeSticker != null)
                                {
                                    var emoticon = documentAttributeSticker.Alt.ToString();
                                    if (!string.IsNullOrEmpty(emoticon))
                                    {
                                        TLStickerPack pack;
                                        if (packsDict.TryGetValue(emoticon, out pack))
                                        {
                                            pack.Documents.Insert(0, document22.Id);
                                        }
                                        else
                                        {
                                            packsDict[emoticon] = new TLStickerPack { Emoticon = new TLString(emoticon), Documents = new TLVector<TLLong> { document22.Id } };
                                        }
                                    }
                                }
                            }
                        }

                        var packs = new TLVector<TLStickerPack>();
                        foreach (var pack in packsDict.Values)
                        {
                            packs.Add(pack);
                        }
                        masks29.Packs = packs;

                        StateService.SaveMasksAsync(masks29);

                        Execute.BeginOnUIThread(() =>
                        {
                            _eventAggregator.Publish(new UpdateNewStickerSetEventArgs(masks29, true));

                            //EmojiControl emojiControl;
                            //if (EmojiControl.TryGetInstance(out emojiControl))
                            //{
                            //    emojiControl.AddStickerSet(updateNewStickerSet.Stickerset);
                            //}
                        });
                    }
                });
            }
            else
            {
                StateService.GetAllStickersAsync(cachedStickers =>
                {
                    var allStickers = cachedStickers as TLAllStickers29;
                    if (allStickers != null)
                    {
                        var documents = new TLVector<TLStickerItem>();
                        foreach (var document in updateNewStickerSet.Stickerset.Documents)
                        {
                            documents.Add(new TLStickerItem { Document = document });
                        }

                        allStickers.Sets.Insert(0, updateNewStickerSet.Stickerset.Set);
                        var newHash = TLUtils.GetAllStickersHash(allStickers.Sets);
                        allStickers.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));

                        for (int i = 0; i < updateNewStickerSet.Stickerset.Documents.Count; i++)
                        {
                            allStickers.Documents.Insert(i, updateNewStickerSet.Stickerset.Documents[i]);
                        }

                        var packsDict = new Dictionary<string, TLStickerPack>();
                        for (var i = 0; i < allStickers.Packs.Count; i++)
                        {
                            packsDict[allStickers.Packs[i].Emoticon.ToString()] = allStickers.Packs[i];
                        }

                        for (var i = 0; i < updateNewStickerSet.Stickerset.Documents.Count; i++)
                        {
                            var document22 = allStickers.Documents[i] as TLDocument22;
                            if (document22 != null)
                            {
                                var documentAttributeSticker = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker29) as TLDocumentAttributeSticker29;
                                if (documentAttributeSticker != null)
                                {
                                    var emoticon = documentAttributeSticker.Alt.ToString();
                                    if (!string.IsNullOrEmpty(emoticon))
                                    {
                                        TLStickerPack pack;
                                        if (packsDict.TryGetValue(emoticon, out pack))
                                        {
                                            pack.Documents.Insert(0, document22.Id);
                                        }
                                        else
                                        {
                                            packsDict[emoticon] = new TLStickerPack { Emoticon = new TLString(emoticon), Documents = new TLVector<TLLong> { document22.Id } };
                                        }
                                    }
                                }
                            }
                        }

                        var packs = new TLVector<TLStickerPack>();
                        foreach (var pack in packsDict.Values)
                        {
                            packs.Add(pack);
                        }
                        allStickers.Packs = packs;

                        StateService.SaveAllStickersAsync(allStickers);

                        Execute.BeginOnUIThread(() =>
                        {
                            _eventAggregator.Publish(new UpdateNewStickerSetEventArgs(allStickers, false));

                            EmojiControl emojiControl;
                            if (EmojiControl.TryGetInstance(out emojiControl))
                            {
                                emojiControl.AddStickerSet(updateNewStickerSet.Stickerset);
                            }
                        });
                    }
                });
            }
        }

        public void RemoveMaskSet(TLStickerSetBase stickerSet, TLInputStickerSetBase inputStickerSet)
        {
            StateService.GetMasksAsync(cachedMasks =>
            {
                var masks = cachedMasks as TLAllStickers29;
                if (masks != null)
                {
                    for (var i = 0; i < masks.Sets.Count; i++)
                    {
                        if (masks.Sets[i].Id.Value == stickerSet.Id.Value)
                        {
                            masks.Sets.RemoveAt(i);
                            break;
                        }
                    }

                    var newHash = TLUtils.GetAllStickersHash(masks.Sets);
                    masks.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));

                    var documentsDict = new Dictionary<long, TLDocument22>();
                    for (int i = 0; i < masks.Documents.Count; i++)
                    {
                        var document22 = masks.Documents[i] as TLDocument22;
                        if (document22 != null)
                        {
                            var documentAttributeSticker = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker29) as TLDocumentAttributeSticker29;
                            if (documentAttributeSticker != null)
                            {
                                var stickerSetId = documentAttributeSticker.Stickerset as TLInputStickerSetId;
                                if (stickerSetId != null && stickerSetId.Id.Value == stickerSet.Id.Value)
                                {
                                    documentsDict[document22.Id.Value] = document22;
                                    masks.Documents.RemoveAt(i--);
                                    continue;
                                }
                                var stickerSetShortName = documentAttributeSticker.Stickerset as TLInputStickerSetShortName;
                                if (stickerSetShortName != null
                                    && TLString.Equals(stickerSetShortName.ShortName, stickerSet.ShortName, StringComparison.OrdinalIgnoreCase))
                                {
                                    documentsDict[document22.Id.Value] = document22;
                                    masks.Documents.RemoveAt(i--);
                                    continue;
                                }
                            }
                        }
                    }

                    for (var i = 0; i < masks.Packs.Count; i++)
                    {
                        for (var j = 0; j < masks.Packs[i].Documents.Count; j++)
                        {
                            if (documentsDict.ContainsKey(masks.Packs[i].Documents[j].Value))
                            {
                                masks.Packs[i].Documents.RemoveAt(j--);
                            }
                        }

                        if (masks.Packs[i].Documents.Count == 0)
                        {
                            masks.Packs.RemoveAt(i--);
                        }
                    }

                    StateService.SaveMasksAsync(masks);

                    //Execute.BeginOnUIThread(() =>
                    //{
                    //    EmojiControl emojiControl;
                    //    if (EmojiControl.TryGetInstance(out emojiControl))
                    //    {
                    //        emojiControl.RemoveStickerSet(inputStickerSet);
                    //    }
                    //});
                }
            });
        }

        public void RemoveStickerSet(TLStickerSetBase stickerSet, TLInputStickerSetBase inputStickerSet)
        {
            StateService.GetAllStickersAsync(cachedStickers =>
            {
                var allStickers = cachedStickers as TLAllStickers29;
                if (allStickers != null)
                {
                    for (var i = 0; i < allStickers.Sets.Count; i++)
                    {
                        if (allStickers.Sets[i].Id.Value == stickerSet.Id.Value)
                        {
                            allStickers.Sets.RemoveAt(i);
                            break;
                        }
                    }

                    var newHash = TLUtils.GetAllStickersHash(allStickers.Sets);
                    allStickers.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));

                    var documentsDict = new Dictionary<long, TLDocument22>();
                    for (int i = 0; i < allStickers.Documents.Count; i++)
                    {
                        var document22 = allStickers.Documents[i] as TLDocument22;
                        if (document22 != null)
                        {
                            var documentAttributeSticker = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker29) as TLDocumentAttributeSticker29;
                            if (documentAttributeSticker != null)
                            {
                                var stickerSetId = documentAttributeSticker.Stickerset as TLInputStickerSetId;
                                if (stickerSetId != null && stickerSetId.Id.Value == stickerSet.Id.Value)
                                {
                                    documentsDict[document22.Id.Value] = document22;
                                    allStickers.Documents.RemoveAt(i--);
                                    continue;
                                }
                                var stickerSetShortName = documentAttributeSticker.Stickerset as TLInputStickerSetShortName;
                                if (stickerSetShortName != null
                                    && TLString.Equals(stickerSetShortName.ShortName, stickerSet.ShortName, StringComparison.OrdinalIgnoreCase))
                                {
                                    documentsDict[document22.Id.Value] = document22;
                                    allStickers.Documents.RemoveAt(i--);
                                    continue;
                                }
                            }
                        }
                    }

                    for (var i = 0; i < allStickers.Packs.Count; i++)
                    {
                        for (var j = 0; j < allStickers.Packs[i].Documents.Count; j++)
                        {
                            if (documentsDict.ContainsKey(allStickers.Packs[i].Documents[j].Value))
                            {
                                allStickers.Packs[i].Documents.RemoveAt(j--);
                            }
                        }

                        if (allStickers.Packs[i].Documents.Count == 0)
                        {
                            allStickers.Packs.RemoveAt(i--);
                        }
                    }

                    StateService.SaveAllStickersAsync(allStickers);

                    Execute.BeginOnUIThread(() =>
                    {
                        EmojiControl emojiControl;
                        if (EmojiControl.TryGetInstance(out emojiControl))
                        {
                            emojiControl.RemoveStickerSet(inputStickerSet);
                        }
                    });
                }
            });
        }

        public void Handle(TLUpdateReadFeaturedStickers update)
        {
            var featuredStickers = StateService.GetFeaturedStickers();
            if (featuredStickers != null)
            {
                featuredStickers.Unread = new TLVector<TLLong>();

                StateService.SaveFeaturedStickersAsync(featuredStickers);

                Execute.BeginOnUIThread(() => _eventAggregator.Publish(new UpdateReadFeaturedStickersEventArgs(featuredStickers)));
            }
        }

        #region Voice calls

        private static TelegramMessageBox _phoneCallMessageBox;

        public void Handle(PhoneCallStateChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("  Handle state=" + args.CallState);

            Execute.BeginOnUIThread(() =>
            {
                if (_phoneCallMessageBox == null) return;

                var callView = _phoneCallMessageBox.Content as CallView;
                if (callView == null) return;

                var callViewModel = callView.ViewModel;

                if (callViewModel != null
                    && callViewModel.CallId != null
                    && args.Call != null
                    && args.Call.Id.Value == callViewModel.CallId.Value)
                {
                    callViewModel.ChangeCallState(args.CallState);
                }
            });
        }

        public void Handle(SignalBarsChangedEventArgs args)
        {
            Execute.BeginOnUIThread(() =>
            {
                if (_phoneCallMessageBox == null) return;

                var callView = _phoneCallMessageBox.Content as CallView;
                if (callView == null) return;

                var callViewModel = callView.ViewModel;
                if (callViewModel == null) return;

                callViewModel.Signal = args.Signal;
            });
        }

        public void Handle(PhoneCallStartedEventArgs args)
        {
            Execute.BeginOnUIThread(() =>
            {
                if (_phoneCallMessageBox == null) return;

                var callView = _phoneCallMessageBox.Content as CallView;
                if (callView == null) return;

                var callViewModel = callView.ViewModel;

                if (callViewModel != null
                    && (callViewModel.CallId == null || args.Call.Id.Value == callViewModel.CallId.Value))
                {
                    if (args.Emojis != null)
                    {
                        callViewModel.Emojis = string.Join("\u2009", args.Emojis);
                        callViewModel.NotifyOfPropertyChange(() => callViewModel.Emojis);
                    }
                }
            });
        }

        public void Handle(PhoneCallDiscardedEventArgs args)
        {
            Execute.BeginOnUIThread(() =>
            {
                var voipService = IoC.Get<IVoIPService>();

                var telegramTransitionFrame = Application.Current.RootVisual as TelegramTransitionFrame;
                if (telegramTransitionFrame != null)
                {
                    if (voipService.Call is TLPhoneCallDiscarded61)
                    {
                        telegramTransitionFrame.HideCallPlaceholder();
                    }
                    else
                    {
                        Telegram.Logs.Log.Write("Hadnle(PhoneCallDiscardedEventArg) didn't invoke HideCallPlaceholder voip.Call=" + voipService.Call);
                    }
                }

                if (_phoneCallMessageBox == null) return;

                var callView = _phoneCallMessageBox.Content as CallView;
                if (callView == null) return;

                var callViewModel = callView.ViewModel;

                if (callViewModel != null
                    && callViewModel.CallId != null
                    && args.DiscardedCall.Id.Value == callViewModel.CallId.Value)
                {
                    callViewModel.StopTimer();

                    if (args.Call is TLPhoneCallRequested64
                        || args.Call is TLPhoneCall
                        || args.Call is TLPhoneCallWaiting)
                    {
                        if (args.Call is TLPhoneCallWaiting && args.Outgoing)
                        {
                            callViewModel.ViewState = CallViewState.OutgoingCallBusy;
                        }
                        else
                        {
                            _phoneCallMessageBox.Dismiss();
                        }

                        var inputPhoneCall = args.Call as IInputPhoneCall;
                        if (inputPhoneCall != null)
                        {
                            if (args.DiscardedCall.NeedRating)
                            {
                                Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.5), () =>
                                {
                                    var content = new CallRatingControl();
                                    ShowCustomMessageBox(null, AppResources.AppName,
                                        AppResources.Send.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                                        dismissed =>
                                        {
                                            if (dismissed == CustomMessageBoxResult.RightButton)
                                            {
                                                if (content.Rating.Value > 0.0)
                                                {
                                                    var comment = content.Rating.Value < 5.0
                                                        ? new TLString(content.Comment.Text)
                                                        : TLString.Empty;

                                                    MTProtoService.SetCallRatingAsync(inputPhoneCall.ToInputPhoneCall(), new TLInt((int)content.Rating.Value), comment,
                                                        result =>
                                                        {

                                                        },
                                                        error =>
                                                        {
                                                            Execute.ShowDebugMessage("phone.setCallRating error " + error);
                                                        });
                                                }
                                            }
                                        },
                                        content);
                                });
                            }

                            if (args.DiscardedCall.NeedDebug)
                            {
                                var debug = voipService.GetDebugLog(args.DiscardedCall.Id.Value);
                                MTProtoService.SaveCallDebugAsync(inputPhoneCall.ToInputPhoneCall(),
                                    new TLDataJSON { Data = new TLString(debug) },
                                    result =>
                                    {

                                    },
                                    error =>
                                    {
                                        Execute.ShowDebugMessage("phone.saveCallDebugString error " + error);
                                    });
                            }
                        }
                    }
                    else
                    {
                        callViewModel.ViewState = args.Outgoing ? CallViewState.OutgoingCallBusy : CallViewState.IncomingCallBusy;
                    }
                }
            });
        }

        public void Handle(PhoneCallRequestedEventArgs args)
        {
            if (args == null) return;
            if (args.RequestedCall == null) return;

            var user = _cacheService.GetUser(args.RequestedCall.AdminId) as TLUser;
            if (user == null) return;

            Execute.BeginOnUIThread(() =>
            {
                var voipService = IoC.Get<IVoIPService>();

                if (voipService.AcceptedCallId == args.RequestedCall.Id.Value)
                {
                    ShowCallMessageBox(user, args.RequestedCall.Id, voipService, CallViewState.CallConnecting);

                    voipService.AcceptIncomingCall(args.RequestedCall);
                }
                else
                {
                    ShowCallMessageBox(user, args.RequestedCall.Id, voipService, CallViewState.IncomingCall);
                }
            });
        }

        public static void OpenCurrentCall()
        {
            if (_phoneCallMessageBox == null)
            {
                var callController = BackgroundProcessController.Instance.CallController;

                if (callController != null
                     && (callController.CallStatus == CallStatus.Held
                        || callController.CallStatus == CallStatus.InProgress))
                {
                    if (callController.Key != null)
                    {
                        var user = IoC.Get<ICacheService>().GetUser(new TLInt((int)callController.OtherPartyId)) as TLUser;
                        if (user != null)
                        {
                            ShowCallMessageBox(user, new TLLong(callController.CallId), IoC.Get<IVoIPService>(), CallViewState.Call, callController.CallStartTime.LocalDateTime);
                            return;
                        }
                    }
                }

                var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                if (frame != null)
                {
                    frame.HideCallPlaceholder();
                }

                return;
            }

            SystemTray.IsVisible = false;
            _phoneCallMessageBox.Show();
        }

        public static void StartVoiceCall(TLUser user, IVoIPService voipService, ICacheService cacheService)
        {
            if (user == null) return;
            if (user.IsBot) return;
            if (user.IsSelf) return;

            System.Diagnostics.Debug.WriteLine("StartVoiceCall user_id={0} call={1}", voipService.UserId, voipService.Call);

            if (voipService.UserId != null
                && (voipService.Call == null // first call is in progress
                    || (voipService.Call != null && !(voipService.Call is TLPhoneCallDiscarded61))) // any call is in progress
                )
            {
                if (user.Index == voipService.UserId.Value)
                {
                    OpenCurrentCall();
                    return;
                }

                var participant = cacheService.GetUser(voipService.UserId) as TLUser;
                if (participant != null)
                {
                    var result = MessageBox.Show(string.Format(AppResources.CallInProgressNotification, participant.FullName2), AppResources.AppName, MessageBoxButton.OK);
                }

                return;
            }

            var callViewModel = ShowCallMessageBox(user, null, voipService, CallViewState.OutgoingCall);

            voipService.StartOutgoingCall(user,
                result =>
                {
                    callViewModel.CallId = result;
                });
        }

        private static CallViewModel ShowCallMessageBox(TLUser user, TLLong callId, IVoIPService voipService, CallViewState viewState, DateTime? callStartTime = null)
        {
            var width = 480.0;
            var height = 800.0;
            var frame = Application.Current.RootVisual as TelegramTransitionFrame;
            if (frame != null)
            {
                var page = frame.Content as PhoneApplicationPage;
                if (page != null)
                {
                    width = page.ActualWidth;
                    height = page.ActualHeight;
                }

                if (frame.CallPlaceholder != null && frame.CallPlaceholder.ActualHeight > 0.0)
                {
                    height += frame.CallPlaceholder.ActualHeight;
                }
            }
            SystemTray.IsVisible = false;

            var callViewModel = new CallViewModel(user, voipService);
            callViewModel.CallId = callId;
            switch (viewState)
            {
                case CallViewState.Call:
                    callViewModel.Status = "";
                    callViewModel.StartTimer(callStartTime ?? DateTime.Now);
                    break;
                case CallViewState.IncomingCall:
                    callViewModel.Status = AppResources.IncomingCall;
                    break;
                case CallViewState.OutgoingCall:
                    callViewModel.Status = AppResources.OutgoingCall;
                    break;
            }
            callViewModel.ViewState = viewState;
            var callView = new CallView();
            callView.Width = width;
            callView.Height = height;
            callView.Margin = new Thickness(0.0);
            callView.DataContext = callViewModel;
            callView.GoToState(callViewModel.ViewState);

            var telegramMessageBox = new TelegramMessageBox { IsFullScreen = true };
            telegramMessageBox.Content = callView;
            telegramMessageBox.Dismissed += (sender, args) =>
            {
                if (_phoneCallMessageBox == telegramMessageBox)
                {
                    SystemTray.IsVisible = true;
                }

                //callViewModel.StopTimer();
            };
            callView.BottomCommand.Tap += (sender, args) =>
            {
                callView.BottomCommand.IsEnabled = false;
                callView.IgnoreButton.IsEnabled = false;

                if (voipService.Call != null
                    && (callView.ViewModel.CallId == null || callView.ViewModel.CallId.Value == voipService.Call.Id.Value))
                {
                    var requestedCall = voipService.Call as TLPhoneCallRequested64;
                    if (requestedCall != null)
                    {
                        callViewModel.ViewState = CallViewState.CallConnecting;

                        voipService.AcceptIncomingCall(requestedCall);
                    }

                    var waitingCall = voipService.Call as TLPhoneCallWaiting;
                    if (waitingCall != null)
                    {
                        voipService.HangUp();

                        _phoneCallMessageBox.Dismiss();
                    }

                    var call = voipService.Call as TLPhoneCall;
                    if (call != null)
                    {
                        voipService.HangUp();

                        _phoneCallMessageBox.Dismiss();
                    }

                    var discardedCall = voipService.Call as TLPhoneCallDiscarded61;
                    if (discardedCall != null)
                    {
                        callViewModel.Status = AppResources.OutgoingCall;
                        callViewModel.NotifyOfPropertyChange(() => callViewModel.Status);
                        callViewModel.ViewState = CallViewState.OutgoingCall;

                        voipService.StartOutgoingCall(user,
                            result =>
                            {
                                callViewModel.CallId = result;
                            });
                    }
                }
            };
            callView.IgnoreButton.Tap += (sender, e) =>
            {
                callView.BottomCommand.IsEnabled = false;
                callView.IgnoreButton.IsEnabled = false;

                var requestedCall = voipService.Call as TLPhoneCallRequested64;
                if (requestedCall != null)
                {
                    voipService.HangUp();
                }

                _phoneCallMessageBox.Dismiss();
            };
            callView.CaptionPanel.DoubleTap += (sender, e) =>
            {
                var caption = string.Format("libtgvoip v{0}", voipService.GetVersion());

                var content = new CallDebugControl { Height = 520.0 };
                content.Start();
                ShowCustomMessageBox(null, caption, AppResources.Close.ToLowerInvariant(), null,
                    dismissed =>
                    {
                        content.Stop();
                    },
                    content);
            };
            callView.ChatButton.Tap += (sender, e) =>
            {
                if (callView.ViewModel != null)
                {
                    callView.ViewModel.OpenChat();
                }

                _phoneCallMessageBox.Dismiss();
            };

            _phoneCallMessageBox = telegramMessageBox;

            telegramMessageBox.Show();

            if (frame != null)
            {
                frame.ShowCallPlaceholder(OpenCurrentCall);
            }

            return callViewModel;
        }

        public LiveLocationBadgeViewModel LiveLocationBadge { get; set; }

        public void Handle(LiveLocationAddedEventArgs args)
        {
            Execute.BeginOnUIThread(() =>
            {
                if (LiveLocationBadge == null)
                {
                    LiveLocationBadge = new LiveLocationBadgeViewModel(IoC.Get<ILiveLocationService>(), IoC.Get<ICacheService>(), true);
                    LiveLocationBadge.OpenMessage += OpenLiveLocationBadge;
                    LiveLocationBadge.Closed += CloseLiveLocationBadge;

                    NotifyOfPropertyChange(() => LiveLocationBadge);
                }

                LiveLocationBadge.UpdateLiveLocation(args.Message);
            });
        }

        public void Handle(LiveLocationRemovedEventArgs args)
        {
            if (args.Messages.Count == 0) return;

            Execute.BeginOnUIThread(() =>
            {
                if (LiveLocationBadge == null) return;

                LiveLocationBadge.RemoveLiveLocations(args.Messages);

                if (LiveLocationBadge.Messages.Count == 0)
                {
                    LiveLocationBadge = null;
                    NotifyOfPropertyChange(() => LiveLocationBadge);
                }
            });
        }

        public void Handle(LiveLocationClearedEventArgs args)
        {
            Execute.BeginOnUIThread(() =>
            {
                if (LiveLocationBadge == null) return;

                LiveLocationBadge = null;
                NotifyOfPropertyChange(() => LiveLocationBadge);
            });
        }

        public void Handle(LiveLocationLoadedEventArgs args)
        {
            if (args.Messages.Count == 0) return;

            Execute.BeginOnUIThread(() =>
            {
                if (LiveLocationBadge == null)
                {
                    LiveLocationBadge = new LiveLocationBadgeViewModel(IoC.Get<ILiveLocationService>(), IoC.Get<ICacheService>(), true);
                    LiveLocationBadge.OpenMessage += OpenLiveLocationBadge;
                    LiveLocationBadge.Closed += CloseLiveLocationBadge;

                    NotifyOfPropertyChange(() => LiveLocationBadge);
                }

                var messages = new List<TLMessageBase>();
                foreach (var m in args.Messages)
                {
                    messages.Add(m);
                }
                LiveLocationBadge.UpdateLiveLocations(messages);
            });
        }

        private void CloseLiveLocationBadge(object sender, System.EventArgs e)
        {
            var confirmation = MessageBox.Show(AppResources.StopLiveLocationAlertAll, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            var liveLocationService = IoC.Get<ILiveLocationService>();

            liveLocationService.StopAllAsync();
        }

        private void OpenLiveLocationBadge(object sender, System.EventArgs e)
        {
            if (LiveLocationBadge == null) return;
            if (LiveLocationBadge.Messages.Count == 0) return;

            if (LiveLocationBadge.Messages.Count == 1)
            {
                OpenLiveLocation(LiveLocationBadge.Messages[0]);
                return;
            }

            var message = string.Format(AppResources.SharingLiveLocationTitle, Language.Declension(
                LiveLocationBadge.Messages.Count,
                AppResources.ChatNominativeSingular,
                AppResources.ChatNominativePlural,
                AppResources.ChatGenitiveSingular,
                AppResources.ChatGenitivePlural).ToLower(CultureInfo.CurrentUICulture));

            var content = new LiveLocationsControl { DataContext = LiveLocationBadge };

            var messageBox = ShowCustomMessageBox(
                message,
                string.Empty,
                AppResources.Close,
                AppResources.StopAllLocationSharings,
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.LeftButton)
                    {
                        var liveLocationsService = IoC.Get<ILiveLocationService>();

                        liveLocationsService.StopAllAsync();
                    }
                },
                content);

            content.Tap += (o, args) =>
            {
                var element = args.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    var m = element.DataContext as TLMessage70;
                    if (m != null)
                    {
                        messageBox.Dismiss();
                        Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                        {
                            OpenLiveLocation(m);
                        });
                    }
                }
            };

            content.LiveLocationCompleted += (o, args) =>
            {
                var completedMessage = LiveLocationBadge.Messages.FirstOrDefault(x => ((TLMessage)x).Media == args.Media) as TLMessage;
                if (completedMessage != null)
                {
                    LiveLocationBadge.RemoveLiveLocations(new List<TLMessage> { completedMessage });
                }

                if (LiveLocationBadge.Messages.Count == 0)
                {
                    messageBox.Dismiss();
                }
            };
        }

        public MapViewModel LocationPicker { get; protected set; }

        private void OpenLiveLocation(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message == null) return;

            var liveLocationsService = IoC.Get<ILiveLocationService>();
            var messageGeoLive = liveLocationsService.Get(message.ToId, MTProtoService.CurrentUserId);
            if (messageGeoLive != null)
            {
                var cachedMessage = IoC.Get<ICacheService>().GetMessage(messageGeoLive.Id, message.ToId is TLPeerChannel ? message.ToId.Id : null) as TLMessage;
                if (cachedMessage != null)
                {
                    messageGeoLive = cachedMessage;
                }
            }

            if (LocationPicker == null)
            {
                LocationPicker = IoC.Get<MapViewModel>();
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = message.To;
                LocationPicker.MessageGeo = message;
                LocationPicker.MessageGeoLive = messageGeoLive;
                LocationPicker.ContinueAction = ContinueSendLocation;
                LocationPicker.StopLiveLocationAction = StopLiveLocation;
                LocationPicker.UpdateLiveLocationsAction = UpdateLiveLocations;
                LocationPicker.ParentHitTest = RestoreParentHitTest;
                NotifyOfPropertyChange(() => LocationPicker);
            }
            else
            {
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = message.To;
                LocationPicker.MessageGeo = message;
                LocationPicker.MessageGeoLive = messageGeoLive;
                Execute.BeginOnUIThread(() => LocationPicker.OpenEditor());
            }
        }

        private void RestoreParentHitTest(bool restore)
        {
            var shellView = GetView() as ShellView;
            if (shellView != null)
            {
                shellView.Items.IsHitTestVisible = restore;
                shellView.LiveLocationBadge.IsHitTestVisible = restore;
                shellView.ItemsHeaders.IsHitTestVisible = restore;
                shellView.AppBarPanel.Visibility = restore ? Visibility.Visible : Visibility.Collapsed;

                if (restore)
                {
                    LocationPicker = null;
                    NotifyOfPropertyChange(() => LocationPicker);
                }
            }
        }

        private void UpdateLiveLocations(Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback)
        {
            if (LocationPicker == null) return;

            var message = LocationPicker.MessageGeoLive as TLMessage70;
            if (message == null) return;

            MTProtoService.GetRecentLocationsAsync(MTProtoService.PeerToInputPeer(message.ToId), new TLInt(int.MaxValue), new TLInt(0),
                result => Execute.BeginOnUIThread(() =>
                {
                    callback.SafeInvoke(result);
                }),
                faultCallback);
        }

        private void StopLiveLocation(TLMessage70 message, Action callback)
        {
            if (message == null) return;

            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var newGeoPoint = new TLGeoPointEmpty();

            var liveLocationsService = IoC.Get<ILiveLocationService>();

            liveLocationsService.UpdateAsync(message, newGeoPoint, result =>
                Execute.BeginOnUIThread(() =>
                {
                    mediaGeoLive.Date = message.Date;
                    mediaGeoLive.EditDate = message.EditDate;
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error == null || error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        mediaGeoLive.Date = message.Date;
                        mediaGeoLive.EditDate = message.EditDate;
                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);

                        callback.SafeInvoke();
                    }
                }));
        }

        private void ContinueSendLocation(TLMessageMediaBase mediaBase)
        {
            if (LocationPicker == null) return;

            var message = LocationPicker.MessageGeoLive as TLMessage70;
            if (message == null) return;

            var mediaGeoLive = mediaBase as TLMessageMediaGeoLive;
            if (mediaGeoLive != null)
            {
                SendLiveLocation(message.ToId, mediaGeoLive);
            }
        }

        private void SendLiveLocation(TLPeerBase peer, TLMessageMediaGeoLive mediaGeoLive)
        {
            var date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);

            var message = TLUtils.GetMessage(
                new TLInt(StateService.CurrentUserId),
                peer,
                MessageStatus.Sending,
                TLBool.True,
                TLBool.True,
                date,
                TLString.Empty,
                mediaGeoLive,
                TLLong.Random(),
                new TLInt(0)
            );

            mediaGeoLive.Date = message.Date;
            mediaGeoLive.From = message.From;

            BeginOnThreadPool(() => IoC.Get<ICacheService>().SyncSendingMessage(message, null, SendLiveLocationInternal));
        }

        private void SendLiveLocationInternal(TLMessageCommon messageBase)
        {
            var message = messageBase as TLMessage34;
            if (message == null) return;

            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var inputMediaGeoLive = new TLInputMediaGeoLive
            {
                GeoPoint = new TLInputGeoPoint { Lat = geoPoint.Lat, Long = geoPoint.Long },
                Period = mediaGeoLive.Period
            };

            message.InputMedia = inputMediaGeoLive;

            UploadService.SendMediaInternal(message, MTProtoService, StateService, IoC.Get<ICacheService>());
        }

        public void Handle(TLUpdateEditMessage update)
        {
            EditLiveLocation(update.Message);
        }

        public void Handle(TLUpdateEditChannelMessage update)
        {
            EditLiveLocation(update.Message);
        }

        private void EditLiveLocation(TLMessageBase messageBase)
        {
            if (LocationPicker == null) return;

            var message70 = messageBase as TLMessage70;
            if (message70 != null)
            {
                var mediaGeoLive = message70.Media as TLMessageMediaGeoLive;
                if (mediaGeoLive != null)
                {
                    var currentMessage = LocationPicker.MessageGeoLive as TLMessage70;
                    if (currentMessage != null)
                    {
                        var currentPeerUser = currentMessage.ToId as TLPeerUser;
                        var peerUser = message70.ToId as TLPeerUser;
                        if (peerUser != null
                            && currentPeerUser != null
                            && message70.FromId != null
                            && currentPeerUser.Id.Value == message70.FromId.Value)
                        {
                            Execute.BeginOnUIThread(() => LocationPicker.UpdateLiveLocation(message70));
                            return;
                        }

                        var currentPeerChat = currentMessage.ToId as TLPeerChat;
                        var peerChat = message70.ToId as TLPeerChannel;
                        if (peerChat != null
                            && currentPeerChat != null
                            && peerChat.Id.Value == currentPeerChat.Id.Value)
                        {
                            Execute.BeginOnUIThread(() => LocationPicker.UpdateLiveLocation(message70));
                            return;
                        }

                        var currentPeerChannel = currentMessage.ToId as TLPeerChannel;
                        var peerChannel = message70.ToId as TLPeerChannel;
                        if (peerChannel != null
                            && currentPeerChannel != null
                            && peerChannel.Id.Value == currentPeerChannel.Id.Value)
                        {
                            Execute.BeginOnUIThread(() => LocationPicker.UpdateLiveLocation(message70));
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        public void OpenProxySettings()
        {
            _navigationService.UriFor<ProxyListViewModel>().Navigate();
        }

        public void Handle(MTProtoProxyDisabledEventArgs args)
        {
            Execute.BeginOnUIThread(() =>
            {
                ShowCustomMessageBox(AppResources.ProxyDisabledNotification, AppResources.AppName, AppResources.Ok, null, dismissed => { });
            });
        }

        public void CloseSearch()
        {
            var view = GetView() as ShellView;
            if (view != null)
            {
                view.CloseSearch();
            }
        }
    }

    public class UpdateRemoveStickerSetEventArgs
    {
        public TLStickerSetBase StickerSet { get; protected set; }

        public UpdateRemoveStickerSetEventArgs(TLStickerSetBase stickerSet)
        {
            StickerSet = stickerSet;
        }
    }

    public class UpdateStickerSetsEventArgs
    {
        public bool Masks { get; set; }

        public TLAllStickers29 AllStickers { get; protected set; }

        public UpdateStickerSetsEventArgs(TLAllStickers29 allStickers, bool masks)
        {
            AllStickers = allStickers;
            Masks = masks;
        }
    }

    public class UpdateStickerSetsOrderEventArgs
    {
        public bool Masks { get; set; }

        public TLAllStickers29 AllStickers { get; protected set; }

        public UpdateStickerSetsOrderEventArgs(TLAllStickers29 allStickers, bool masks)
        {
            AllStickers = allStickers;
            Masks = masks;
        }
    }

    public class UpdateNewStickerSetEventArgs
    {
        public bool Masks { get; set; }

        public TLAllStickers29 AllStickers { get; protected set; }

        public UpdateNewStickerSetEventArgs(TLAllStickers29 allStickers, bool masks)
        {
            AllStickers = allStickers;
            Masks = masks;
        }
    }

    public class UpdateReadFeaturedStickersEventArgs
    {
        public TLFeaturedStickers FeaturedStickers { get; protected set; }

        public UpdateReadFeaturedStickersEventArgs(TLFeaturedStickers featuredStickers)
        {
            FeaturedStickers = featuredStickers;
        }
    }
}