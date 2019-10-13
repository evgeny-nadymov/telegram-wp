// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Networking.Voip;
using PhoneVoIPApp.UI;
using Telegram.Api.Aggregator;
using Telegram.Api.Services.DeviceInfo;
using Telegram.EmojiPanel;
using TelegramClient.Controls;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Calls;
using TelegramClient.ViewModels.Payments;
#if WP8
using Windows.Phone.System.Power;
#endif
#if WP81 && WNS_PUSH_SERVICE
using Windows.UI.Notifications;
#endif
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Connection;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.Location;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using Telegram.Api.Transport;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Auth;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Debug;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Feed;
using TelegramClient.ViewModels.Media;
using TelegramClient.ViewModels.Passport;
using TelegramClient.ViewModels.Search;
using EnterPasswordViewModel = TelegramClient.ViewModels.Passport.EnterPasswordViewModel;
using Execute = Telegram.Api.Helpers.Execute;
using ExtensionMethods = Caliburn.Micro.ExtensionMethods;

namespace TelegramClient
{
    public class Bootstrapper : PhoneBootstrapperBase
    {
        public Bootstrapper()
        {
            //LogManager.GetLog = type => new DebugLog(type);
            Initialize();
            //System.Diagnostics.Debug.WriteLine(System.Reflection.Assembly.GetCallingAssembly().FullName);
            //System.Diagnostics.Debug.WriteLine(Application.Current.GetType().Assembly);
            //System.Diagnostics.Debug.WriteLine(System.AppDomain.CurrentDomain.FriendlyName);
            //System.Diagnostics.Debug.WriteLine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase));
        }

        public static PhoneApplicationFrame PhoneFrame { get; protected set; }

        protected override PhoneApplicationFrame CreatePhoneApplicationFrame()
        {
            //return new PhoneApplicationFrame();
            PhoneFrame = new TelegramTransitionFrame{ Title = AppResources.Loading };
            //PhoneFrame.Loaded += InitializeMTProtoService;

            //PhoneFrame.FlowDirection = FlowDirection.RightToLeft;
            PhoneFrame.UriMapper = new TelegramUriMapper();
            //var stateService = IoC.Get<IStateService>();
            //var background = stateService.CurrentBackground;
            //var brush = new ImageBrush();
            //PhoneFrame.Navigate(new Uri("/Views/ShellView.xaml", UriKind.Relative));
            //brush.ImageSource = new BitmapImage(new Uri(background, UriKind.Relative));

            //frame.Background = brush;
            return PhoneFrame;
        }

        private PhoneContainer _container;

        protected override void Configure()
        {
            App.Log("start Bootstrapper.Configure ");
            _container = new PhoneContainer();
//            _container.Activated += instance =>
//            {
//#if DEBUG
//                Debug.WriteLine("Bootstrapper.Activated " + (instance != null ? instance.ToString() : "null"));
//#endif
//            };

            if (!DesignerProperties.IsInDesignTool)
            {
                _container.RegisterPhoneServices(RootFrame);
            }

            _container.PerRequest<SecretContactDetailsViewModel>();
            _container.PerRequest<SecretMediaViewModel>();
            _container.PerRequest<SecretContactViewModel>();
            

            _container.Singleton<ShellViewModel>();
            _container.Singleton<DialogsViewModel>();
            _container.Singleton<ContactsViewModel>();

            _container.Singleton<LogViewModel>();
#if DEBUG
            _container.Singleton<DebugViewModel>();
            _container.Singleton<LongPollViewModel>();
            _container.Singleton<PerformanceViewModel>();
#endif
            _container.PerRequest<EncryptionKeyViewModel>();
            _container.PerRequest<AboutViewModel>();
            _container.PerRequest<CacheViewModel>();
            _container.PerRequest<StartupViewModel>();
            _container.PerRequest<SignInViewModel>();
            _container.PerRequest<SignUpViewModel>();
            _container.PerRequest<ConfirmViewModel>();
            _container.PerRequest<ContactViewModel>();
            _container.PerRequest<ContactInfoViewModel>();
            _container.PerRequest<ChatViewModel>();
            _container.PerRequest<AddChatParticipantViewModel>();
            _container.PerRequest<AddChannelManagerViewModel>();
            _container.PerRequest<AddSecretChatParticipantViewModel>();
            _container.PerRequest<EditChatViewModel>();
            _container.PerRequest<EditContactViewModel>();
            _container.PerRequest<EditCurrentUserViewModel>();
            _container.PerRequest<EditUsernameViewModel>();
            _container.PerRequest<EditPhoneNumberViewModel>();
            _container.PerRequest<ChangePhoneNumberViewModel>();
            _container.PerRequest<MediaViewModel<TLUserBase>>();
            _container.PerRequest<MediaViewModel<TLChatBase>>();
            _container.PerRequest<MediaViewModel<IInputPeer>>();
            _container.PerRequest<FullMediaViewModel>();
            _container.PerRequest<LinksViewModel<TLUserBase>>();
            _container.PerRequest<LinksViewModel<TLChatBase>>();
            _container.PerRequest<LinksViewModel<IInputPeer>>();
            _container.PerRequest<FilesViewModel<TLUserBase>>();
            _container.PerRequest<FilesViewModel<TLChatBase>>();
            _container.PerRequest<FilesViewModel<IInputPeer>>();
            _container.PerRequest<MusicViewModel<TLUserBase>>();
            _container.PerRequest<MusicViewModel<TLChatBase>>();
            _container.PerRequest<MusicViewModel<IInputPeer>>();
            _container.PerRequest<ImageViewerViewModel>();
            _container.PerRequest<AnimatedImageViewerViewModel>();
            _container.PerRequest<DecryptedImageViewerViewModel>();
            _container.PerRequest<ProfilePhotoViewerViewModel>();
            _container.PerRequest<ShareViewModel>();
            _container.PerRequest<DialogDetailsViewModel>();
            _container.PerRequest<SecretDialogDetailsViewModel>();
            _container.PerRequest<CreateDialogViewModel>();
            _container.PerRequest<CreateBroadcastViewModel>();
            _container.PerRequest<CreateChannelStep1ViewModel>();
            _container.PerRequest<CreateChannelStep2ViewModel>();
            _container.PerRequest<CreateChannelStep3ViewModel>();
            _container.PerRequest<ChooseParticipantsViewModel>();
            _container.PerRequest<SettingsViewModel>();
            _container.PerRequest<NotificationsViewModel>();
            _container.PerRequest<BlockedContactsViewModel>();
            _container.PerRequest<ChooseBackgroundViewModel>();
            _container.PerRequest<ChooseAttachmentViewModel>();
            _container.PerRequest<AskQuestionConfirmationViewModel>();
            _container.PerRequest<AddChatParticipantConfirmationViewModel>();
            _container.PerRequest<MapViewModel>();
            _container.PerRequest<SearchShellViewModel>();
            _container.PerRequest<SearchDialogsViewModel>();
            _container.PerRequest<SearchMessagesViewModel>();
            _container.PerRequest<DialogSearchMessagesViewModel>();
            _container.PerRequest<SearchContactsViewModel>();
            _container.PerRequest<SearchFilesViewModel>();
            _container.PerRequest<SearchLinksViewModel>();
            _container.PerRequest<SearchMusicViewModel>();
            _container.PerRequest<SearchViewModel>();
            _container.PerRequest<ShareContactViewModel>();
            _container.PerRequest<VideoPlayerViewModel>();
            _container.PerRequest<LastSeenViewModel>();
            _container.PerRequest<SecretChatsViewModel>();
            _container.PerRequest<GroupsViewModel>();
            _container.PerRequest<PrivacySecurityViewModel>();
            _container.PerRequest<AccountSelfDestructsViewModel>();
            _container.PerRequest<AllowUsersViewModel>();
            _container.PerRequest<ChooseTTLViewModel>();
            _container.PerRequest<ChooseNotificationSpanViewModel>();
            _container.PerRequest<MessageViewerViewModel>();
            _container.PerRequest<FastDialogDetailsViewModel>();
            _container.PerRequest<FeaturedStickersViewModel>();
            _container.PerRequest<ArchivedStickersViewModel>();
            _container.PerRequest<CallsPrivacyViewModel>();

            _container.PerRequest<PasscodeViewModel>();
            _container.PerRequest<ChangePasscodeViewModel>();
            _container.PerRequest<EnterPasscodeViewModel>();
            _container.PerRequest<LockscreenViewModel>();

            _container.PerRequest<ConfirmPasswordViewModel>();
            _container.PerRequest<PasswordViewModel>();
            _container.PerRequest<ChangePasswordViewModel>();
            _container.PerRequest<ChangePasswordHintViewModel>();
            _container.PerRequest<ChangePasswordEmailViewModel>();
            _container.PerRequest<ViewModels.Additional.EnterPasswordViewModel>();
            _container.PerRequest<PasswordRecoveryViewModel>();
            _container.PerRequest<CancelConfirmResetViewModel>();
            _container.PerRequest<ResetAccountViewModel>();
            _container.PerRequest<FeedViewModel>();
            _container.PerRequest<ShareContactDetailsViewModel>();

            //_container.PerRequest<FastDialogDetailsViewModel>();

            _container.PerRequest<SessionsViewModel>();

#if WP81
            _container.PerRequest<EditVideoViewModel>();
#endif
            _container.PerRequest<ChooseCountryViewModel>();
            _container.PerRequest<VideoCaptureViewModel>();
            _container.PerRequest<PrivacyStatementViewModel>();
            _container.PerRequest<ChooseDialogViewModel>();
            _container.PerRequest<SnapshotsViewModel>();
            _container.PerRequest<UsernameHintsViewModel>();
            _container.PerRequest<HashtagHintsViewModel>();
            _container.PerRequest<InlineBotResultsViewModel>();
            _container.PerRequest<UserActionViewModel>();
            _container.PerRequest<PinnedMessageViewModel>();
            _container.PerRequest<ImageEditorViewModel>();
#if WP8
            _container.PerRequest<MultiImageEditorViewModel>();
#endif
            _container.PerRequest<InviteLinkViewModel>();
            _container.PerRequest<SearchVenuesViewModel>();
            _container.PerRequest<StickersViewModel>();
            _container.PerRequest<SecretChatDebugViewModel>();
            _container.PerRequest<CommandHintsViewModel>();
            _container.PerRequest<DialogSearchMessagesViewModel>();
            _container.PerRequest<ChannelAdministratorsViewModel>();
            _container.PerRequest<ChannelMembersViewModel>();
            _container.PerRequest<ChannelIntroViewModel>();
            _container.PerRequest<AddAdminsViewModel>();
            _container.PerRequest<ChannelBlockedContactsViewModel>();
            _container.PerRequest<ChatSettingsViewModel>();
            _container.PerRequest<ClearCacheSettingsViewModel>();
            _container.PerRequest<ConvertToSupergroupViewModel>();
            _container.PerRequest<MassDeleteReportSpamViewModel>();
            _container.PerRequest<EditGroupTypeViewModel>();
            _container.PerRequest<StickerHintsViewModel>();
            _container.PerRequest<WebViewModel>();
            _container.PerRequest<MasksViewModel>();
            _container.PerRequest<GroupsInCommonViewModel>();
            _container.PerRequest<WebCardInfoViewModel>();
            _container.PerRequest<WebVerificationViewModel>();
            _container.PerRequest<CardInfoViewModel>();
            _container.PerRequest<SavedCardInfoViewModel>();
            _container.PerRequest<ShippingInfoViewModel>();
            _container.PerRequest<ShippingMethodViewModel>();
            _container.PerRequest<CheckoutViewModel>();
            _container.PerRequest<CallsViewModel>();
            _container.PerRequest<CameraViewModel>();
            _container.PerRequest<PhotoPickerViewModel>();
            _container.PerRequest<ProxyViewModel>();
            _container.PerRequest<CallsSecurityViewModel>();
            _container.PerRequest<BioViewModel>();
            _container.PerRequest<EmojiHintsViewModel>();
            _container.PerRequest<GroupStickersViewModel>();
            _container.PerRequest<PasswordEmailViewModel>();
            _container.PerRequest<LiveLocationBadgeViewModel>();
            _container.PerRequest<LoggedInViewModel>();
            _container.PerRequest<PassportViewModel>();
            _container.PerRequest<PersonalDetailsViewModel>();
            _container.PerRequest<ResidentialAddressViewModel>();
            _container.PerRequest<PhoneNumberViewModel>();
            _container.PerRequest<PhoneNumberCodeViewModel>();
            _container.PerRequest<EmailViewModel>();
            _container.PerRequest<EmailCodeViewModel>();
            _container.PerRequest<EnterPasswordViewModel>();
            _container.PerRequest<ProxyListViewModel>();
            _container.PerRequest<ContactsSecurityViewModel>();
            _container.PerRequest<SearchSharedContactsViewModel>();
            _container.PerRequest<PassportSettingsViewModel>();
            _container.PerRequest<PasswordIntroViewModel>();

            _container.Singleton<IVoIPService, VoIPService>();
            _container.Singleton<ITelegramEventAggregator, TelegramEventAggregator>();
            _container.Singleton<IConnectionService, ConnectionService>();
            _container.Singleton<ICommonErrorHandler, CommonErrorHandler>();
            _container.Singleton<IMTProtoService, MTProtoService>();
            _container.Singleton<IStateService, StateService>();
            _container.Singleton<ITransport, HttpTransport>();
            _container.Singleton<ICacheService, InMemoryCacheService>();
            _container.Singleton<IUpdatesService, UpdatesService>();
            _container.Singleton<IFileManager, FileManager>();
            _container.Singleton<IVideoFileManager, VideoFileManager>();
            _container.Singleton<IEncryptedFileManager, EncryptedFileManager>();
            _container.Singleton<IUploadFileManager, UploadFileManager>();
            _container.Singleton<ITransportService, TransportService>();
            _container.Singleton<IProxyChecker, ProxyChecker>();

            _container.Singleton<IDeviceInfoService, PhoneInfoService>();
            _container.Singleton<IExtendedDeviceInfoService, PhoneInfoService>();

#if WP81 && WNS_PUSH_SERVICE
            _container.Singleton<IPushService, WNSPushService>();
            _container.Singleton<IWindowsPhoneStoreUpdateService, WindowsPhoneStoreUpdateService>();
#else
            _container.Singleton<IPushService, PushService>();
#endif
            _container.Singleton<IHttpDocumentFileManager, HttpDocumentFileManager>();
            _container.Singleton<IUploadVideoFileManager, UploadVideoFileManager>();
            _container.Singleton<IDocumentFileManager, DocumentFileManager>();
            _container.Singleton<IAudioFileManager, AudioFileManager>();
            _container.Singleton<IUploadAudioFileManager, UploadAudioFileManager>();
            _container.Singleton<IUploadDocumentFileManager, UploadDocumentFileManager>();
            _container.Singleton<IUploadService, UploadService>();
            _container.Singleton<ILiveLocationService, LiveLocationService>();
            _container.Singleton<IPublicConfigService, PublicConfigService>();
            _container.Singleton<IWindowsPhoneStoreUpdateService, WindowsPhoneStoreUpdateService>();
            _container.Singleton<IWebFileManager, WebFileManager>();

            SetupViewLocator();

            // avoid xaml ui designer crashes 
            if (Caliburn.Micro.Execute.InDesignMode) return;

            StartBugsenseAsync();
            AddCustomConventions();

            App.Log("end Bootstrapper.Configure");
        }

        protected override void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);

            

#if LOG_REGISTRATION
            TLUtils.WriteLog("Unhandled exception " + e);
#endif

            e.Handled = true;
        }

        private static void AddCustomConventions()
        {
            // App Bar Conventions

            // ... the rest of your conventions
        }

        protected override object GetInstance(Type service, string key)
        {
            object instance = null;
            var stopwatch = Stopwatch.StartNew();
            //App.Log("  Bootstrapper.GetInstance start " + (service != null ? service.ToString() : "null") + " " + stopwatch.Elapsed);
            instance = _container.GetInstance(service, key);
            //App.Log("  Bootstrapper.GetInstance stop " + (service != null ? service.ToString() : "null") + " " + stopwatch.Elapsed);

            return instance;
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            Telegram.Logs.Log.Write("Startup");
#if LOG_REGISTRATION
            TLUtils.WriteLog("App startup ");
#endif

            base.OnStartup(sender, e);
        }

        protected override void OnLaunch(object sender, LaunchingEventArgs e)
        {

            App.Log("start Bootstrapper.OnLaunch ");

            Telegram.Logs.Log.Write("\nLaunch");
#if LOG_REGISTRATION
            TLUtils.WriteLog("App launch");
#endif
            EnterAppMutex();

            App.Log("Bootstrapper.OnLaunch EnterAppMutex");
            //var cacheService = IoC.Get<ICacheService>();
            //cacheService.Init();

            App.Log("Bootstrapper.OnLaunch cacheService.Init");
            var transportService = IoC.Get<ITransportService>();
            transportService.TransportConnecting += OnTransportConnecting;
            transportService.TransportConnected += OnTransportConnected;

            TLUtils.WritePerformance(">>OnLaunch");

            App.Log("Bootstrapper.OnLaunch transportService");
            LoadStateAndUpdateAsync();

            App.Log("Bootstrapper.OnLaunch LoadStateAndUpdateAsync");
            ShowBatterySaverAlertAsync();

            App.Log("Bootstrapper.OnLaunch ShowBatterySaverAlertAsync");
            StartAgent();

            App.Log("Bootstrapper.OnLaunch StartAgent");
            SetInitTextScaleFactor();

            App.Log("Bootstrapper.OnLaunch SetInitTextScaleFactor");
#if WP81
            var shareTargetLaunch = e as ShareLaunchingEventArgs;
            if (shareTargetLaunch != null)
            {
                (App.Current as App).ShareOperation = shareTargetLaunch.ShareTargetActivatedEventArgs.ShareOperation;
            }
#endif

            base.OnLaunch(sender, e);
            App.Log("end Bootstrapper.OnLaunch");
        }

        protected override void OnActivate(object sender, ActivatedEventArgs e)
        {

            Telegram.Logs.Log.Write("Activate");
#if LOG_REGISTRATION
            TLUtils.WriteLog("App activate IsAppInstancePreserved=" + e.IsApplicationInstancePreserved);
#endif
            EnterAppMutex();
            CheckPasscode();

            if (!e.IsApplicationInstancePreserved)
            {
                var cacheService = IoC.Get<ICacheService>();
                cacheService.Init();
                Execute.ShowDebugMessage("Init database");
            }

            TLUtils.WritePerformance(">>OnActivate IsAppInstancePreserved " + e.IsApplicationInstancePreserved);

            LoadStateAndUpdateAsync();

            ShowBatterySaverAlertAsync();

            CheckTextScaleFactorAsync();

            base.OnActivate(sender, e);
        }

        private void OnTransportConnected(object sender, TransportEventArgs e)
        {
            var mtProtoService = IoC.Get<IMTProtoService>();
            if (mtProtoService != null)
            {
                var transport = e.Transport;
                if (transport == null)
                {
                    return;
                }

                if (transport.MTProtoType != MTProtoTransportType.Main)
                {
                    return;
                }

                mtProtoService.SetMessageOnTime(0.0, string.Empty);
            }
        }

        private void OnTransportConnecting(object sender, TransportEventArgs e)
        {
            var mtProtoService = IoC.Get<IMTProtoService>();
            if (mtProtoService != null)
            {
                var transport = e.Transport;

                if (transport == null)
                {
                    return;
                }

                if (transport.MTProtoType != MTProtoTransportType.Main)
                {
                    return;
                }

#if DEBUG
                var proxy =
                    transport.ProxyConfig != null
                    && transport.ProxyConfig.IsEnabled.Value
                    && !transport.ProxyConfig.IsEmpty
                    ? transport.ProxyConfig.GetProxy()
                    : null;

                var transportIdString = string.Format("({0} {1}_{2}{3})", transport.Id, transport.DCId, transport.ActualHost, proxy != null ? "_" + proxy.Server : "");
#else
                var transportIdString = string.Empty;
#endif

                var proxyConfig = transport.ProxyConfig;
                if (proxyConfig != null && proxyConfig.IsEnabled.Value && !proxyConfig.IsEmpty)
                {
                    mtProtoService.SetMessageOnTime(25.0, string.Format("{0}{1}...", AppResources.ConnectingToProxy, transportIdString));
                }
                else
                {
                    mtProtoService.SetMessageOnTime(25.0, string.Format("{0}{1}...", AppResources.Connecting, transportIdString));
                }
            }
        }

        protected override void OnClose(object sender, ClosingEventArgs e)
        {
            var updatesService = IoC.Get<IUpdatesService>();

            Telegram.Logs.Log.Write("Close state=" + updatesService.GetState());
#if LOG_REGISTRATION
            TLUtils.WriteLog("App close");
#endif
            ReleaseAppMutex();
            //BackgroundProcessController.Instance.CallController.StartMTProtoUpdater();
            try
            {
                if (BackgroundProcessController.Instance.CallController != null)
                {
                    BackgroundProcessController.Instance.CallController.SetStatusCallback(null);
                }
            }
            catch (Exception ex)
            {
                TLUtils.WriteException("OnDeactivate", ex);
            }
            BackgroundProcessController.Instance.DisconnectUi();

            UpdateMainTile();

            if (PasscodeUtils.IsEnabled)
            {
                var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                if (frame != null && !frame.IsLockScreenOpen())
                {
                    PasscodeUtils.CloseTime = DateTime.Now;
                }
            }

            var cacheService = IoC.Get<ICacheService>();
            cacheService.TryCommit();

            updatesService.SaveState();

            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
            if (isAuthorized)
            {
                var mtProtoService = IoC.Get<IMTProtoService>();
                mtProtoService.UpdateStatusAsync(new TLBool(true), result => CloseTransport());
            }
            else
            {
                CloseTransport();
            }

            base.OnClose(sender, e);
        }

#if WP8
        private static readonly Mutex _appOpenMutex = new Mutex(false, Telegram.Api.Constants.TelegramMessengerMutexName);
#endif

        private static bool EnterAppMutex()
        {
#if WP8
            return _appOpenMutex.WaitOne();
#endif

            return true;
        }

        private static void ReleaseAppMutex()
        {
#if WP8
            _appOpenMutex.ReleaseMutex();
#endif
        }

        private void CheckPasscode()
        {
            if (PasscodeUtils.IsLockscreenRequired)
            {
                var chooseFileInfo = ((App) Application.Current).ChooseFileInfo;
                if (chooseFileInfo == null || DateTime.Now > chooseFileInfo.Time.AddSeconds(120.0))
                {
                    var frame = RootFrame as TelegramTransitionFrame;
                    if (frame != null)
                    {
                        frame.OpenLockscreen();
                    }
                }
            }

            ((App) Application.Current).ChooseFileInfo = null;
        }

        private double _previousScaleFactor = 1.0;

        private void CheckTextScaleFactorAsync()
        {
            var scaledText = Application.Current.Resources["ScaledText"] as ScaledText;
            if (scaledText != null)
            {
                if (scaledText.TextScaleFactor != _previousScaleFactor)
                {
                    _previousScaleFactor = scaledText.TextScaleFactor;
                    BrowserNavigationService.FontScaleFactor = scaledText.TextScaleFactor;
                    scaledText.NotifyOfPropertyChange(() => scaledText.TextScaleFactor);
                }
            }
        }

        private void SetInitTextScaleFactor()
        {
            var scaledText = Application.Current.Resources["ScaledText"] as ScaledText;
            if (scaledText != null)
            {
                BrowserNavigationService.FontScaleFactor = scaledText.TextScaleFactor;
                _previousScaleFactor = scaledText.TextScaleFactor;
            }
        }

        protected override void OnDeactivate(object sender, DeactivatedEventArgs args)
        {
            var updatesService = IoC.Get<IUpdatesService>();
            Telegram.Logs.Log.Write("Deactivate state=" + updatesService.GetState());
#if LOG_REGISTRATION
            TLUtils.WriteLog("App deactivate");
#endif
            ReleaseAppMutex();
            //BackgroundProcessController.Instance.CallController.StartMTProtoUpdater();
            try
            {
                if (BackgroundProcessController.Instance.CallController != null)
                {
                    BackgroundProcessController.Instance.CallController.SetStatusCallback(null);
                }
            }
            catch (Exception ex)
            {
                TLUtils.WriteException("OnDeactivate", ex);
            }
            BackgroundProcessController.Instance.DisconnectUi();

            UpdateMainTile();

            var cacheService = IoC.Get<ICacheService>();
            cacheService.TryCommit();

            updatesService.SaveState();
            updatesService.CancelUpdating();


            
            var mtProtoService = IoC.Get<IMTProtoService>();
            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
            if (isAuthorized)
            {
                var manualResetEvent2 = new ManualResetEvent(false);
                mtProtoService.UpdateStatusAsync(TLBool.True,
                    result =>
                    {
                        manualResetEvent2.Set();
                    },
                    error =>
                    {
                        manualResetEvent2.Set();
                    });
                manualResetEvent2.WaitOne(200);

                ((App) Application.Current).Offline = true;

                if (PasscodeUtils.IsEnabled)
                {
                    var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                    if (frame != null && !frame.IsLockScreenOpen())
                    {
                        PasscodeUtils.CloseTime = DateTime.Now;
                    }
                }
            }
            //manualResetEvent2.WaitOne(20000);

            var manualResetEvent = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
#if LOG_REGISTRATION
                    TLUtils.WriteLog("OnDeactivate MTProtoService.ClearHistory");
#endif
                    mtProtoService.ClearHistory("OnDeactivate", true);
                }
                catch (Exception e)
                {
                    TLUtils.WriteException(e);
                }
                finally
                {
                    manualResetEvent.Set();
                }
            });
            manualResetEvent.WaitOne(5000);

            base.OnDeactivate(sender, args);
        }

        private void ShowBatterySaverAlertAsync()
        {
            Execute.BeginOnThreadPool(() =>
            {
                if (!_showBatterySaverOnce)
                {
                    _showBatterySaverOnce = CheckBatterySaverState();
                }
            });
        }

        private bool _showBatterySaverOnce;

        private bool CheckBatterySaverState()
        {
#if WP8
            // The minimum phone version that supports the PowerSavingModeEnabled property
            var targetVersion = new Version(8, 0, 10492);

            //if (Environment.OSVersion.Version >= targetVersion)
            {
                // Use reflection to get the PowerSavingModeEnabled value
                //var powerSaveEnabled = (bool) typeof(PowerManager).GetProperty("PowerSavingModeEnabled").GetValue(null, null);
                var powerSaveOn = PowerManager.PowerSavingMode == PowerSavingMode.On;
                if (powerSaveOn)
                {
                    Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.BatterySaverModeAlert, AppResources.Warning, MessageBoxButton.OK));
                    return true;
                }

                return false;
            }
#endif

            return true;
        }

        private void LoadStateAndUpdateAsync()
        {
            Execute.BeginOnThreadPool(() =>
            {
                try
                {
                    BackgroundProcessController.Instance.ConnectUi();
                }
                catch (Exception ex)
                {
                    TLUtils.WriteException("LoadStateAndUpdateAsync 1", ex);
                }
                //BackgroundProcessController.Instance.CallController.StopMTProtoUpdater();

                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (!isAuthorized) return;

                Execute.BeginOnUIThread(() =>
                {
                    //MessageBox.Show("LoadStateAndUpdateAsync");

                    var mtProtoService = IoC.Get<IMTProtoService>();
                    var stateService = IoC.Get<IStateService>();
                    var updatesService = IoC.Get<IUpdatesService>();
                    var voipService = IoC.Get<IVoIPService>();
                    var liveLocationsService = IoC.Get<ILiveLocationService>();

                    mtProtoService.CurrentUserId = new TLInt(stateService.CurrentUserId);
                    updatesService.GetCurrentUserId = () => mtProtoService.CurrentUserId;
                    updatesService.GetStateAsync = mtProtoService.GetStateAsync;
                    updatesService.GetDHConfigAsync = mtProtoService.GetDHConfigAsync;
                    updatesService.GetDifferenceAsync = mtProtoService.GetDifferenceAsync;
                    updatesService.AcceptEncryptionAsync = mtProtoService.AcceptEncryptionAsync;
                    updatesService.SendEncryptedServiceAsync = mtProtoService.SendEncryptedServiceAsync;
                    updatesService.SetMessageOnTimeAsync = mtProtoService.SetMessageOnTime;
                    updatesService.RemoveFromQueue = mtProtoService.RemoveFromQueue;
                    updatesService.UpdateChannelAsync = mtProtoService.UpdateChannelAsync;
                    updatesService.GetFullChatAsync = mtProtoService.GetFullChatAsync;
                    updatesService.GetFullUserAsync = mtProtoService.GetFullUserAsync;
                    updatesService.GetChannelMessagesAsync = mtProtoService.GetMessagesAsync;
                    updatesService.GetPinnedDialogsAsync = mtProtoService.GetPinnedDialogsAsync;
                    updatesService.GetMessagesAsync = mtProtoService.GetMessagesAsync;
                    updatesService.GetPeerDialogsAsync = mtProtoService.GetPeerDialogsAsync;
                    updatesService.GetPromoDialogAsync = mtProtoService.GetPromoDialogAsync;

                    stateService.SuppressNotifications = true;

                    long acceptedCallId = -1;
                    try
                    {
                        if (BackgroundProcessController.Instance.CallController != null)
                        {
                            BackgroundProcessController.Instance.CallController.SetStatusCallback((VoIPService)voipService);
                            acceptedCallId = BackgroundProcessController.Instance.CallController.AcceptedCallId;
                            BackgroundProcessController.Instance.CallController.AcceptedCallId = -1;
                            voipService.AcceptedCallId = acceptedCallId;
                            if (acceptedCallId != -1)
                            {
                                BackgroundProcessController.Instance.CallController.EndCall();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TLUtils.WriteException("LoadStateAndUpdateAsync 2", ex);
                    }

                    liveLocationsService.LoadAndUpdateAllAsync();

                    var timer = Stopwatch.StartNew();
                    TLUtils.WritePerformance(">>UpdateService.LoadStateAndUpdate start");
                    //mtProtoService.SetMessageOnTime(60.0 * 5, AppResources.Updating + "...");
                    updatesService.LoadStateAndUpdate(acceptedCallId,
                        () =>
                        {
                            //mtProtoService.SetMessageOnTime(0.0, string.Empty);
                            TLUtils.WritePerformance(">>UpdateService.LoadStateAndUpdate stop " + timer.Elapsed);
                            stateService.SuppressNotifications = false;
                        });
                });
            });
        }

        private void RestoreConnectionAsync()
        {
            Execute.BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (!isAuthorized) return;

                Execute.BeginOnUIThread(() =>
                {
                    var mtProtoService = IoC.Get<IMTProtoService>();
                    //mtProtoService.PingAsync(TLLong.Random(), result => { }, error => { });
                    mtProtoService.UpdateStatusAsync(TLBool.False, result => { }, error => { });
                });
            });
        }

        private void CloseTransport()
        {
            var transportService = IoC.Get<ITransportService>();
            transportService.Close();
        }

        public static void UpdateMainTile()
        {
#if WNS_PUSH_SERVICE
            try
            {
                ToastNotificationManager.History.Clear();
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write("ClearNotifications ex " + ex);
                Execute.ShowDebugMessage("Clear notifications history exception\n" + ex);
            }

            try
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write("ClearTile ex " + ex);
                Execute.ShowDebugMessage("Clear tile exception\n" + ex);
            }
#else
            // no way to clear toast history with MPNS
            //try
            //{
            //    ToastNotificationManager.History.Clear();
            //}
            //catch (Exception ex)
            //{
            //    Telegram.Logs.Log.Write("ClearNotifications ex " + ex);
            //    Execute.ShowDebugMessage("Clear notifications history exception\n" + ex);
            //}

            var tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile == null) return;

#if WP8
            var tileData = new IconicTileData { Count = 0, WideContent1 = "", WideContent2 = "", WideContent3 = "" };
#else
            var tileData = new StandardTileData{ Count = 0 };
#endif
            try
            { 
                tile.Update(tileData);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage("Tile.Update exception\n" + ex);
            }
#endif
        }

        private static void StartBugsenseAsync()
        {
            Execute.BeginOnUIThread(TimeSpan.FromSeconds(3.0), BugSenseWrapper.Init);
        }

        // The name of the incoming call task.
        private const string incomingCallTaskName = "PhoneVoIPApp.IncomingCallTask";

        public static void InitHttpNotificationTask()
        {
            try
            {
                // Obtain a reference to the existing task, if any.
                var incomingCallTask = ScheduledActionService.Find(incomingCallTaskName) as VoipHttpIncomingCallTask;
                if (incomingCallTask != null)
                {
                    if (incomingCallTask.IsScheduled == false)
                    {
                        // The incoming call task has been unscheduled due to OOM or throwing an unhandled exception twice in a row
                        ScheduledActionService.Remove(incomingCallTaskName);
                    }
                    else
                    {
                        // The incoming call task has been scheduled and is still scheduled so there is nothing more to do
                        return;
                    }
                }

                // Create a new incoming call task.
                incomingCallTask = new VoipHttpIncomingCallTask(incomingCallTaskName, Constants.ToastNotificationChannelName);
                incomingCallTask.Description = "Incoming call task";

                foreach (var action in ScheduledActionService.GetActions<ScheduledAction>())
                {
                    ScheduledActionService.Remove(action.Name);
                }

                ScheduledActionService.Add(incomingCallTask);
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write("InitHttpNotificationTask ScheduledActionService.Add exception\n" + ex);
            }
        }

        private static void StartAgent()
        {
            InitHttpNotificationTask();

            return;

            if (ScheduledActionService.Find(Constants.ScheduledAgentTaskName) != null)
            {
                ScheduledActionService.Remove(Constants.ScheduledAgentTaskName);
            }

            var task = new PeriodicTask(Constants.ScheduledAgentTaskName)
            {
                Description = AppResources.TileUpdaterTaskDescription
            };

            try
            {
                ScheduledActionService.Add(task);

#if DEBUG
            // If we're debugging, attempt to start the task immediately 
            // this break our service and updates
            try
            {
                ScheduledActionService.LaunchForTest(Constants.ScheduledAgentTaskName, new TimeSpan(0, 0, 10));
            }
            catch (Exception e)
            {
                TLUtils.WriteException(e);
            }
#endif
            }
            catch (Exception e)
            {
                TLUtils.WriteException(e);
            }

            //SettingsHelper.SetValue(Constants.UnreadCountKey, 0);

        }

        private static void SetupViewLocator()
        {
            ViewLocator.DeterminePackUriFromType = (viewModelType, viewType) =>
            {
                var assemblyName = ExtensionMethods.GetAssemblyName(viewType.Assembly);
                var uri = viewType.FullName.Replace(assemblyName
#if WP8 //NOTE: at WP8 project deafult assembly name TelegramClient.WP8 instead of TelegramClient at WP7
.Replace(".WP8", string.Empty)
#endif
, string.Empty).Replace(".", "/") + ".xaml";

                if (!ExtensionMethods.GetAssemblyName(Application.Current.GetType().Assembly).Equals(assemblyName))
                {
                    return "/" + assemblyName + ";component" + uri;
                }

                return uri;
            };
        }
    }
}
