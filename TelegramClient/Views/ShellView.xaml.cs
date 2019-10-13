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
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Networking;
using Windows.System;
using FFmpegInterop;
using Microsoft.Phone.Info;
using Microsoft.Phone.Networking.Voip;
using Microsoft.Phone.Scheduler;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using PhoneVoIPApp.BackEnd;
using PhoneVoIPApp.UI;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using TelegramClient.Controls;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;
using TelegramClient.Views.Search;
#if WP8
using TelegramClient_WebP.LibWebP;
using Windows.Phone.PersonalInformation;
#endif
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using TelegramClient.Utils;
#if WP81
using Windows.ApplicationModel.Background;
using Windows.Storage.Pickers;
#endif
using Caliburn.Micro;
using libtgnet;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Extensions;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.Services.Connection;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Help;
using Telegram.Api.Transport;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Passport;
using TelegramClient.Views.Media;
using EnterPasswordViewModel = TelegramClient.ViewModels.Passport.EnterPasswordViewModel;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views
{
    public partial class ShellView
    {
        private static Brush _captionBrush;

        public static Brush CaptionBrush
        {
            get
            {
                if (_captionBrush != null)
                {
                    return _captionBrush;
                }

                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                _captionBrush = isLightTheme
                    ? (Brush)Application.Current.Resources["TelegramBrush"]
                    : (Brush)Application.Current.Resources["PhoneChromeBrush"];

                return _captionBrush;
            }
        }

        private ShellViewModel ViewModel { get { return DataContext as ShellViewModel; } }

        private bool _firstRun = true;

        private readonly ApplicationBarIconButton _addContactButton = new ApplicationBarIconButton
        {
            Text = AppResources.ComposeMessage,
            IconUri = new Uri("/Images/ApplicationBar/appbar.add.rest.png", UriKind.Relative)
        };

        private readonly ApplicationBarIconButton _searchButton = new ApplicationBarIconButton
        {
            Text = AppResources.Search,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.search.rest.png", UriKind.Relative)
        };

        private readonly ApplicationBarMenuItem _backMenuItem = new ApplicationBarMenuItem
        {
            Text = "to test mode"
        };

        private readonly ApplicationBarMenuItem _refreshMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.Refresh
        };

        private readonly ApplicationBarMenuItem _settingsItem = new ApplicationBarMenuItem
        {
            Text = AppResources.Settings,
        };

        private readonly ApplicationBarMenuItem _reviewItem = new ApplicationBarMenuItem
        {
            Text = AppResources.Review,
        };

        private readonly ApplicationBarMenuItem _aboutItem = new ApplicationBarMenuItem
        {
            Text = AppResources.About,
        };

#if DEBUG
        private readonly ApplicationBarMenuItem _openConfig = new ApplicationBarMenuItem
        {
            Text = "Open config",
        };

        private readonly ApplicationBarMenuItem _getCurrentPacketInfoItem = new ApplicationBarMenuItem
        {
            Text = "Info",
        };

        private readonly ApplicationBarMenuItem _showRegistrationLogItem = new ApplicationBarMenuItem
        {
            Text = "Log",
        };
#endif
        private readonly ApplicationBarMenuItem _importContactsItem = new ApplicationBarMenuItem
        {
            Text = "import contacts",
        };

        public ShellView()
        {
            App.Log("start ShellView.ctor");

            InitializeComponent();

            //ItemsHeaders.Items.Add(new HeaderItem { DisplayName = AppResources.ChatNominativePlural });
            //ItemsHeaders.Items.Add(new HeaderItem { DisplayName = AppResources.Calls });
            //ItemsHeaders.Items.Add(new HeaderItem { DisplayName = AppResources.Contacts });
            //ItemsHeaders.SelectedIndex = 0;

            Caption.Background = CaptionBrush;
            CaptionButtons.Background = CaptionBrush;

            App.Log("ShellView.InitializeComponent");

            OptimizeFullHD();

            //throw new Exception("test");

            _addContactButton.Click += (sender, args) => ViewModel.Add();
            _searchButton.Click += (sender, args) => ViewModel.Search();

            _backMenuItem.Click += async (sender, args) =>
            {
                //var openCVComponent = new OpenCVComponent.OpenCVLib();
                //openCVComponent.ProcessImageAsync("test.jpg");
                return;
                ulong fileSize2 = 0;
                var file2 = await Package.Current.InstalledLocation.GetFileAsync("2.mp4");
                var properties2 = await file2.GetBasicPropertiesAsync();
                fileSize2 = properties2.Size;
                var fileName2 = file2.Path;
                var _decoderParams2 = new int[3];
                var _handle2 = FFmpegGifDecoder.CreateDecoder(fileName2, _decoderParams2);
                var _w2 = _decoderParams2[0];
                var _h2 = _decoderParams2[1];
                //_t2.Change(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1));

                return;
                var dialogs = Items.Items.FirstOrDefault(x => x is DialogsViewModel) as DialogsViewModel;
                if (dialogs != null)
                {
                    dialogs.TestMode = !dialogs.TestMode;
                    _backMenuItem.Text = dialogs.TestMode ? "to normal mode" : "to test mode";
                }
            };
            _refreshMenuItem.Click += (sender, args) => ViewModel.RefreshItems();
            _settingsItem.Click += (sender, args) => ViewModel.OpenSettings();
            _reviewItem.Click += (sender, args) => ViewModel.Review();
            _aboutItem.Click += (sender, args) =>
            {
                ViewModel.About();
            };
            _importContactsItem.Click += (o, e) => { };//ViewModel.ImportContactsAsync();

            Items.SelectionChanged += (sender, args) =>
            {
                if (ApplicationBar == null) return;

                var contacts = Items.SelectedItem as ContactsViewModel;

                if (contacts != null)
                {
                    _addContactButton.Text = AppResources.Add;
                    return;
                }

                var dialogs = Items.SelectedItem as DialogsViewModel;

                if (dialogs != null)
                {
                    _addContactButton.Text = AppResources.ComposeMessage;
                    return;
                }
            };

            Loaded += InitializeMTProtoService;

            Loaded += (sender, args) =>
            {
                //if (ViewModel.Items.Count == 0)
                //{
                //    Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                //    {
                //        Items.SelectionChanged -= Items_OnSelectionChanged;
                //        //ItemsHeaders.SelectionChanged -= ItemsHeaders_OnSelectionChanged;

                //        ViewModel.Load();

                //        if (ItemsHeaders.Items.Count < ViewModel.Items.Count)
                //        {
                //            ItemsHeaders.Items.Clear();
                //            foreach (var item in ViewModel.Items)
                //            {
                //                ItemsHeaders.Items.Add(item);
                //            }
                //            ItemsHeaders.SelectedItem = Items.SelectedItem;
                //        }

                //        //ItemsHeaders.SelectionChanged += ItemsHeaders_OnSelectionChanged;
                //        Items.SelectionChanged += Items_OnSelectionChanged;
                //    });
                //}

                //MessageBox.Show("ShellViewModel.OnLoaded");
                TelegramPropertyChangedBase.LogNotify = false;
                App.Log("start ShellView.Loaded");
#if WP81
                NavigationService.PauseOnBack = true;
#endif
                Items.Opacity = 1.0;
                var result = RunAnimation((o, e) =>
                {
                    if (ViewModel.Dialogs.UpdateCompletedDialogs != null)
                    {
                        ViewModel.Dialogs.UpdateCompleted(ViewModel.Dialogs.UpdateCompletedDialogs);
                        ViewModel.Dialogs.UpdateCompletedDialogs = null;
                    }

                    ReturnItemsVisibility();
                });
                if (!result)
                {
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(ReturnItemsVisibility);
                }

                if (ViewModel != null) ViewModel.OnAnimationComplete();
                if (!_firstRun)
                {
                    return;
                }

                _firstRun = false;
                App.Log("end ShellView.Loaded");
            };

#if WP81
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(2.0), async () =>
            {
                foreach (var backgroundTaskRegistration in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks.Values)
                {
                    //Telegram.Logs.Log.Write("::Unregister background task " + backgroundTaskRegistration.Name);
                    backgroundTaskRegistration.Unregister(true);
                }

                var result = await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync();

                if (result == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity
                    || result == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
                {
                    //var builder0 = new BackgroundTaskBuilder();
                    //builder0.Name = Constants.PushNotificationsBackgroundTaskName + "VoIP";
                    //builder0.TaskEntryPoint = "PhoneVoIPApp.BackEnd.BackgroundTask"; //"PhoneVoIPApp.Agents.BackgroundTask";
                    //builder0.SetTrigger(new PushNotificationTrigger());
                    //builder0.Register();

                    var builder = new BackgroundTaskBuilder();
                    builder.Name = Constants.PushNotificationsBackgroundTaskName;
                    builder.TaskEntryPoint = "TelegramClient.Tasks.PushNotificationsBackgroundTask";
                    builder.SetTrigger(new PushNotificationTrigger());
                    builder.Register();
                    //Telegram.Logs.Log.Write("::Register background task " + builder.Name);

                    //var builder2 = new BackgroundTaskBuilder();
                    //builder2.Name = Constants.MessageSchedulerBackgroundTaskName;
                    //builder2.TaskEntryPoint = "TelegramClient.Tasks.MessageSchedulerBackgroundTask";
                    //builder2.SetTrigger(new SystemTrigger(SystemTriggerType.InternetAvailable, false));
                    ////builder2.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                    //builder2.Register();
                    ////Telegram.Logs.Log.Write("::Register background task " + builder2.Name);

                    //var builder3 = new BackgroundTaskBuilder();
                    //builder3.Name = Constants.TimerMessageSchedulerBackgroundTaskName;
                    //builder3.TaskEntryPoint = "TelegramClient.Tasks.MessageSchedulerBackgroundTask";
                    //builder3.SetTrigger(new TimeTrigger(30, false));
                    //builder3.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                    //builder3.Register();
                    ////Telegram.Logs.Log.Write("::Register background task " + builder3.Name);

                    var builder4 = new BackgroundTaskBuilder();
                    builder4.Name = Constants.BackgroundDifferenceLoaderTaskName;
                    builder4.TaskEntryPoint = "TelegramClient.Tasks.BackgroundDifferenceLoader";
                    builder4.SetTrigger(new PushNotificationTrigger());
                    builder4.Register();
                    //Telegram.Logs.Log.Write("::Register background task " + builder4.Name);

                    var type = typeof(IBackgroundTrigger).Assembly.GetType("Windows.ApplicationModel.Background.ToastNotificationActionTrigger");
                    if (type != null)
                    {
                        var trigger = (IBackgroundTrigger)Activator.CreateInstance(type);
                        var builder5 = new BackgroundTaskBuilder();
                        builder5.Name = Constants.InteractiveNotificationsBackgroundTaskName;   //"InteractiveNotificationsBackgroundTask";
                        builder5.TaskEntryPoint = "TelegramClient.Tasks.InteractiveNotificationsBackgroundTask";
                        builder5.SetTrigger(trigger);
                        builder5.Register();
                    }
                }
                else
                {
                    Telegram.Logs.Log.Write("::Background tasks are disabled result=" + result);
                    var messageBoxResult = MessageBox.Show(AppResources.BackgroudnTaskDisabledAlert, AppResources.Warning, MessageBoxButton.OKCancel);
                    if (messageBoxResult == MessageBoxResult.OK)
                    {
                        await Launcher.LaunchUriAsync(new Uri("ms-settings-power://"));
                    }
                }
            });
#endif

            BuildLocalizedAppBar();

            App.Log("end ShellView.ctor");
        }

        private void InitializeMTProtoService(object sender, RoutedEventArgs e)
        {
            Loaded -= InitializeMTProtoService;

            var transportService = IoC.Get<ITransportService>();
            transportService.TransportConnecting += OnTransportConnecting;
            transportService.TransportConnected += OnTransportConnected;

            var mtProtoService = IoC.Get<IMTProtoService>();
            mtProtoService.StartInitialize();
        }

        private void OnTransportConnected(object sender, TransportEventArgs e)
        {
            if (e.Transport.MTProtoType == MTProtoTransportType.Main)
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    ViewModel.Connecting = false;
                });
            }
        }

        private void OnTransportConnecting(object sender, TransportEventArgs e)
        {
            if (e.Transport.MTProtoType == MTProtoTransportType.Main)
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    var connectionType =
                        e.Transport.ProxyConfig != null
                        && !e.Transport.ProxyConfig.IsEmpty
                        && e.Transport.ProxyConfig.IsEnabled.Value
                            ? ConnectionType.Proxy
                            : ConnectionType.Direct;

                    var blockedMode = false;
                    if (connectionType == ConnectionType.Direct)
                    {
                        var config = IoC.Get<ICacheService>().GetConfig() as TLConfig78;
                        blockedMode = config != null && config.BlockedMode;
                    }

                    ViewModel.IsProxyEnabled = connectionType == ConnectionType.Proxy || blockedMode;
                    ViewModel.Connecting = true;
                    ViewModel.ConnectionType = connectionType;
                });
            }
        }

#if WP81
        private void OnBackgroundTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {

        }
#endif

        private double _applicationBarDefaultSize = 72.0;

        private double _iconSize = 32.0;

        private double _iconMargin = 20.0;

        private void OptimizeFullHD()
        {
#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            //if (!isFullHD) return;
#endif

            //Items.HeaderTemplate = (DataTemplate)Application.Current.Resources["FullHDPivotHeaderTemplate"];
        }

        private void ReturnItemsVisibility()
        {
            if (_searchView != null && SearchContentControl.Visibility == Visibility.Visible)
            {
                return;
            }

#if DEBUG
            var builder = new StringBuilder();
            builder.AppendLine(Items.SelectedIndex.ToString());
            foreach (ViewModelBase item in Items.Items)
            {
                builder.AppendLine(item.Visibility.ToString());
            }
            //Telegram.Api.Helpers.Execute.ShowDebugMessage(builder.ToString());
#endif

            foreach (ViewModelBase item in Items.Items)
            {
                item.Visibility = Visibility.Visible;
            }
        }

        private bool _isBackwardInAnimation;

        private bool RunAnimation(EventHandler callback)
        {
            if (_isBackwardInAnimation)
            {
                _isBackwardInAnimation = false;

                if (NextUri != null
                    && NextUri.ToString().Contains("DialogDetailsView.xaml"))
                {
                    var storyboard = new Storyboard();

                    var translateAnimation = new DoubleAnimationUsingKeyFrames();
                    translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 150.0 });
                    translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
                    Storyboard.SetTarget(translateAnimation, LayoutRoot);
                    Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                    storyboard.Children.Add(translateAnimation);

                    //LayoutRoot.Opacity = 1.0;
                    LayoutRoot.Opacity = 0.0;
                    var opacityAnimation = new DoubleAnimationUsingKeyFrames();
                    opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
                    opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 1.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
                    Storyboard.SetTarget(opacityAnimation, LayoutRoot);
                    Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
                    storyboard.Children.Add(opacityAnimation);
                    if (callback != null)
                    {
                        storyboard.Completed += callback;
                    }

                    Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
                    return true;
                }

                if (NextUri != null
                    && (NextUri.ToString().EndsWith("AboutView.xaml")
                        || NextUri.ToString().EndsWith("SettingsView.xaml")
                        || NextUri.ToString().EndsWith("ProxyView.xaml")
                        || NextUri.ToString().EndsWith("ProxyListView.xaml")
                        || NextUri.ToString().Contains("EnterPasswordView.xaml")
                        || NextUri.ToString().Contains("PasswordIntroView.xaml")))
                {
                    var storyboard = TelegramTurnstileAnimations.BackwardIn(LayoutRoot);
                    if (callback != null)
                    {
                        storyboard.Completed += callback;
                    }
                    //var rotationAnimation = new DoubleAnimationUsingKeyFrames();
                    //rotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 105.0 });
                    //rotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
                    //Storyboard.SetTarget(rotationAnimation, LayoutRoot);
                    //Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationY)"));
                    //storyboard.Children.Add(rotationAnimation);
                    //if (callback != null)
                    //{
                    //    storyboard.Completed += callback;
                    //}

                    LayoutRoot.Opacity = 1.0;

                    Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
                    return true;
                }
                else
                {
                    ((CompositeTransform)LayoutRoot.RenderTransform).TranslateY = 0.0;
                    LayoutRoot.Opacity = 1.0;
                    return false;
                }
            }
            else
            {
                ((CompositeTransform)LayoutRoot.RenderTransform).TranslateY = 0.0;
                LayoutRoot.Opacity = 1.0;
                return false;
            }

            return false;
        }

#if DEBUG
        private void GetCurrentPacketInfoItemOnClick(object sender, System.EventArgs eventArgs)
        {
            ViewModel.GetCurrentPacketInfo();
        }
#endif

        private void BuildLocalizedAppBar()
        {
            return;

            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            //ApplicationBar.Opacity = 0.99;

            ApplicationBar.Buttons.Add(_addContactButton);
            ApplicationBar.Buttons.Add(_searchButton);

#if DEBUG
            ApplicationBar.MenuItems.Add(_backMenuItem);
#endif

            ApplicationBar.MenuItems.Add(_refreshMenuItem);
            ApplicationBar.MenuItems.Add(_settingsItem);
            ApplicationBar.MenuItems.Add(_reviewItem);
            ApplicationBar.MenuItems.Add(_aboutItem);
            //ApplicationBar.MenuItems.Add(_backMenuItem);
            //#if DEBUG
            //            ApplicationBar.MenuItems.Add(_importContactsItem);
            //            ApplicationBar.MenuItems.Add(_openConfig);
            //            ApplicationBar.MenuItems.Add(_getCurrentPacketInfoItem);
            //            ApplicationBar.MenuItems.Add(_showRegistrationLogItem);
            //#endif
        }

        private bool _fromExternal;

        private static readonly Uri ExternalUri = new Uri(@"app://external/");

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //MessageBox.Show(string.Format("ShellViewModel.OnNavigatedFrom mode={0} uri={1}", e.Uri, e.NavigationMode));

            ShellViewModel.WriteTimer("ShellView start OnNavigatedFrom");

            AppBarPanel.Close();

            _fromExternal = e.Uri == ExternalUri;
            var selectedIndex = Items.SelectedIndex;
            for (var i = 0; i < Items.Items.Count; i++)
            {
                if (selectedIndex != i)
                {
                    ((ViewModelBase)Items.Items[i]).Visibility = Visibility.Collapsed;
                }
            }

            base.OnNavigatedFrom(e);

            ShellViewModel.WriteTimer("ShellView stop OnNavigatedFrom");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                //if (e.EndsWith("DialogDetailsView.xaml"))
                {
                    //LayoutRoot.Opacity = 0.0;
                    _isBackwardInAnimation = true;
                }
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                // share photo 
                string fileId;
                if (NavigationContext.QueryString.TryGetValue("FileId", out fileId))
                {
                    IoC.Get<IStateService>().FileId = fileId;
                    while (NavigationService.RemoveBackEntry() != null) { }
                }
            }
            else if (e.NavigationMode == NavigationMode.Refresh)
            {
                // share photo 
                string fileId;
                if (NavigationContext.QueryString.TryGetValue("FileId", out fileId))
                {
                    IoC.Get<IStateService>().FileId = fileId;
                }
            }

            string phoneCallId;
            if (NavigationContext.QueryString.TryGetValue("phone_call_id", out phoneCallId))
            {
                IoC.Get<IStateService>().PhoneCallId = phoneCallId;
            }

            if (_lastTapedItem != null)
            {
                var transform = _lastTapedItem.RenderTransform as CompositeTransform;
                if (transform != null)
                {
                    transform.TranslateX = 0.0;
                    transform.TranslateY = 0.0;
                }
                _lastTapedItem.Opacity = 1.0;
            }

            base.OnNavigatedTo(e);
        }

        private static FrameworkElement _lastTapedItem;
        public Uri NextUri;

        public static Storyboard StartContinuumForwardOutAnimation(FrameworkElement tapedItem, FrameworkElement tapedItemContainer = null, bool saveLastTapedItem = true)
        {
            if (saveLastTapedItem)
            {
                _lastTapedItem = tapedItem;
                _lastTapedItem.CacheMode = new BitmapCache();
            }

            var storyboard = new Storyboard();

            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 73.0 });
            Storyboard.SetTarget(timeline, tapedItem);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(timeline);

            var timeline2 = new DoubleAnimationUsingKeyFrames();
            timeline2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            timeline2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 425.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 } });
            Storyboard.SetTarget(timeline2, tapedItem);
            Storyboard.SetTargetProperty(timeline2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(timeline2);

            var timeline3 = new DoubleAnimationUsingKeyFrames();
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1.0 });
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = 1.0 });
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
            Storyboard.SetTarget(timeline3, tapedItem);
            Storyboard.SetTargetProperty(timeline3, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(timeline3);

            if (tapedItemContainer != null)
            {
                var timeline4 = new ObjectAnimationUsingKeyFrames();
                timeline4.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 999.0 });
                timeline4.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
                Storyboard.SetTarget(timeline4, tapedItemContainer);
                Storyboard.SetTargetProperty(timeline4, new PropertyPath("(Canvas.ZIndex)"));
                storyboard.Children.Add(timeline4);
            }

            storyboard.Begin();

            return storyboard;
        }

        public void CloseSearch()
        {
            if (_searchView != null && SearchContentControl.Visibility == Visibility.Visible)
            {
                _searchView.ClosePivotAction.SafeInvoke(Visibility.Visible);
                SearchContentControl.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //MessageBox.Show(string.Format("ShellViewModel.OnNavigatingFrom mode={0} uri={1}", e.Uri, e.NavigationMode));

            ShellViewModel.WriteTimer("ShellView.OnNavigatingFrom start");

            base.OnNavigatingFrom(e);

            ShellViewModel.WriteTimer("ShellView.OnNavigatingFrom base.OnNavigatingFrom");

            NextUri = e.Uri;

            var collapseSearchControl = IoC.Get<IStateService>().CollapseSearchControl;
            IoC.Get<IStateService>().CollapseSearchControl = false;

            if (!e.Cancel)
            {
                if (e.Uri.ToString().Contains("DialogDetailsView.xaml"))
                {
                    var storyboard = new Storyboard();

                    var translateAnimation = new DoubleAnimationUsingKeyFrames();
                    translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
                    translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                    Storyboard.SetTarget(translateAnimation, LayoutRoot);
                    Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                    storyboard.Children.Add(translateAnimation);

                    var opacityAnimation = new DoubleAnimationUsingKeyFrames();
                    opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
                    opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                    Storyboard.SetTarget(opacityAnimation, LayoutRoot);
                    Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
                    storyboard.Children.Add(opacityAnimation);

                    storyboard.Begin();

                    if (collapseSearchControl)
                    {
                        storyboard.Completed += (o, args) =>
                        {
                            SearchContentControl.Visibility = Visibility.Collapsed;
                        };
                    }
                }
                else if (e.Uri.ToString().EndsWith("AboutView.xaml")
                    || e.Uri.ToString().EndsWith("SettingsView.xaml")
                    || e.Uri.ToString().EndsWith("ProxyView.xaml")
                    || e.Uri.ToString().EndsWith("ProxyListView.xaml")
                    || e.Uri.ToString().Contains("EnterPasswordView.xaml")
                    || e.Uri.ToString().Contains("PasswordIntroView.xaml"))
                {
                    var storyboard = TelegramTurnstileAnimations.ForwardOut(LayoutRoot);

                    //var translateAnimation = new DoubleAnimationUsingKeyFrames();
                    //translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
                    //translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 105.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                    //Storyboard.SetTarget(translateAnimation, LayoutRoot);
                    //Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationY)"));
                    //storyboard.Children.Add(translateAnimation);

                    //var opacityAnimation = new DoubleAnimationUsingKeyFrames();
                    //opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
                    //opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                    //Storyboard.SetTarget(opacityAnimation, LayoutRoot);
                    //Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
                    //storyboard.Children.Add(opacityAnimation);

                    storyboard.Begin();
                }

            }
            ShellViewModel.WriteTimer("ShellView.OnNavigatingFrom stop");
        }

        private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
            PasscodeIcon.Opacity = 1.0;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var textBlock = (TextBlock)sender;
            if (textBlock != null)
            {
                MessageBox.Show(textBlock.ActualHeight.ToString());
            }
        }

        private void ShellView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups().ToList();
            var popup = popups.FirstOrDefault();
            if (popup != null)
            {
                e.Cancel = true;

                return;
            }

            if (_searchView != null && SearchContentControl.Visibility == Visibility.Visible)
            {
                _searchView.BeginCloseStoryboard(() =>
                {
                    Self.Focus();
                    SearchContentControl.Visibility = Visibility.Collapsed;
                    ReturnItemsVisibility();
                });
                e.Cancel = true;
                return;
            }

            if (ViewModel.LocationPicker != null
                && ViewModel.LocationPicker.IsOpen)
            {
                ViewModel.LocationPicker.CloseEditor();
                e.Cancel = true;

                return;
            }

            //var fastDialogDetailsView = FastDialogDetails.Content as FastDialogDetailsView;
            //if (fastDialogDetailsView != null)
            //{
            //    fastDialogDetailsView.Close(ViewModel.CloseFastDialogDetails);

            //    e.Cancel = true;
            //}
        }

        private void Add_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                ViewModel.Add();
            });
        }

        private SearchView _searchView;

        private void Search_OnTap(object sender, GestureEventArgs e)
        {
            IoC.Get<IStateService>().GetContactsSettings();

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                MorePanel.Visibility = Visibility.Collapsed;
                AppBarPanel.Visibility = Visibility.Collapsed;

                if (_searchView == null)
                {
                    var searchViewModel = new SearchViewModel(
                        IoC.Get<ICacheService>(), IoC.Get<ICommonErrorHandler>(),
                        IoC.Get<IStateService>(), IoC.Get<INavigationService>(),
                        IoC.Get<IMTProtoService>(), IoC.Get<ITelegramEventAggregator>());

                    _searchView = new SearchView();
                    _searchView.ClosePivotAction = visibility =>
                    {
                        Items.IsHitTestVisible = visibility == Visibility.Visible;
                        LiveLocationBadge.IsHitTestVisible = visibility == Visibility.Visible;
                        AppBarPanel.Visibility = visibility;
                    };
                    ViewModelBinder.Bind(searchViewModel, _searchView, null);

                    SearchContentControl.Visibility = Visibility.Visible;
                    SearchContentControl.Content = _searchView;
                }
                else
                {
                    var searchViewModel = _searchView.DataContext as SearchViewModel;
                    if (searchViewModel != null)
                    {
                        searchViewModel.Text = string.Empty;
                        searchViewModel.NotifyOfPropertyChange(() => searchViewModel.Text);
                    }
                    SearchContentControl.Visibility = Visibility.Visible;
                    _searchView.BeginOpenStoryboard();
                }
            });
        }

        private void Settings_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                ViewModel.OpenSettings();
            });
        }

        private void About_OnTap(object sender, GestureEventArgs e)
        {
            //MessagePlayerControl.Player.Stop();

            //libtgvoip.VoIPControllerWrapper.SwitchSpeaker(false);
            //MessagePlayerControl.Player.MediaOpened += StartOnMediaOpened;
            //MessagePlayerControl.Player.MediaFailed += (o, args) =>
            //{

            //};
            //MessagePlayerControl.Player.Source = new Uri("/Sounds/Sound1.wav", UriKind.Relative);
            //MessagePlayerControl.Player.Play();
            AppBarPanel.Close();
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                //if (_socket != null) _socket.Close();
                ViewModel.About();

            });
        }

        //private void StartOnMediaOpened(object sender, RoutedEventArgs e)
        //{
        //    //MessagePlayerControl.Player.MediaOpened -= StartOnMediaOpened;
        //    MessagePlayerControl.Player.Play();
        //}

        //private void ItemsHeaders_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Items.SelectedIndex = ItemsHeaders.SelectedIndex;
        //}

        private void Items_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ItemsHeaders.Items.Count > Items.SelectedIndex)
            //{
            //    ItemsHeaders.SelectedIndex = Items.SelectedIndex;
            //}

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                foreach (var addedItem in e.AddedItems)
                {
                    var viewModelBase = addedItem as ViewModelBase;
                    if (viewModelBase != null)
                    {
                        viewModelBase.Visibility = Visibility.Visible;
                    }
                }
            }

            //if (Items.SelectedIndex >= 0
            //    && Items.SelectedIndex < Items.Items.Count)
            //{
            //    ((ViewModelBase)Items.Items[Items.SelectedIndex]).Visibility = Visibility.Visible;
            //}
        }

        //static byte[] PEM(string type, string pem)
        //{
        //    string header = String.Format("-----BEGIN {0}-----", type);
        //    string footer = String.Format("-----END {0}-----", type);
        //    int start = pem.IndexOf(header) + header.Length;
        //    int end = pem.IndexOf(footer, start);
        //    string base64 = pem.Substring(start, (end - start));
        //    return Convert.FromBase64String(base64);
        //}

        //static X509Certificate LoadCertificateFile(string filename)
        //{
        //    X509Certificate x509 = null;
        //    using (FileStream fs = File.OpenRead(filename))
        //    {
        //        byte[] data = new byte[fs.Length];
        //        fs.Read(data, 0, data.Length);
        //        if (data[0] != 0x30)
        //        {
        //            // maybe it's ASCII PEM base64 encoded ? 
        //            data = PEM("CERTIFICATE", data);
        //        }
        //        if (data != null)
        //            x509 = new X509Certificate(data);
        //    }

        //    return x509;
        //} 

        private async void Test2_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            var frame = Application.Current.RootVisual as TelegramTransitionFrame;
            if (frame != null)
            {
                frame.ShowBlockingPlaceholder(() =>
                {
                    Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9WZDNCRDZHS0"));
                });
            }
        }

        private ConnectionSocketWrapper _socket;

        private async void Test_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            var passportConfig = IoC.Get<IStateService>().GetPassportConfig();
            var passportConfigHash = passportConfig != null ? passportConfig.Hash : new TLInt(0);

            IoC.Get<IMTProtoService>().GetPassportDataAsync(
                (result1, result2) => Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    if (result1.HasPassword)
                    {
                        IoC.Get<IStateService>().Password = result1;
                        IoC.Get<IStateService>().SecureValues = result2;
                        IoC.Get<INavigationService>().UriFor<EnterPasswordViewModel>().Navigate();
                        return;
                    }

                    if (!result1.HasPassword)
                    {
                        if (!TLString.IsNullOrEmpty(result1.EmailUnconfirmedPattern))
                        {
                            IoC.Get<IStateService>().Password = result1;
                            IoC.Get<IStateService>().SecureValues = result2;
                            IoC.Get<INavigationService>().UriFor<PasswordViewModel>().Navigate();
                        }
                        else
                        {
                            IoC.Get<IStateService>().Password = result1;
                            IoC.Get<IStateService>().SecureValues = result2;
                            IoC.Get<INavigationService>().UriFor<PasswordIntroViewModel>().Navigate();
                        }
                        return;
                    }
                }),
                error =>
                {

                });
            return;
            ShellViewModel.ShowCustomMessageBox(null, null, "update (29MB)", "cancel",
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9WZDNCRDZHS0"));
                    }
                },
                new UpdateAppControl { Margin = new Thickness(-12.0, -32.0, -12.0, -12.0), ShowBottomMenu = false });
            return;
            //ViewModel.Handle(new MTProtoProxyDisabledEventArgs());

            /*ViewModel.MTProtoService.GetProxyDataAsync(
                result =>
                {
                    Telegram.Api.Helpers.Execute.ShowDebugMessage(result.ToString());
                },
                error =>
                {
                    
                });*/

            //return;

            //var channels = new TLVector<TLInputChannelBase>();


            //ViewModel.MTProtoService.SetFeedBroadcastsAsync(new TLInt(1),
            //    channels,
            //    TLBool.False,
            //    result =>
            //    {

            //    },
            //    error =>
            //    {

            //    });

            //AppBarPanel.Close();
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.WaitForFullGCComplete();
            //GC.Collect();
            //return;
            //#if DEBUG
            //            ViewModel.MTProtoService.CheckPublicConfig();
            //#endif
            //var key = new byte[32];
            //var iv = new byte[16];
            //var ecount = new byte[16];
            //var data = new byte[1024];
            //uint num = 0;

            //var result = ConnectionSocketWrapper.AesCtr(data, new TCPO2StateWrapper {Ecount = ecount, IV = iv, Key = key, Num = 0});
            //var result2 = Telegram.Api.Helpers.Utils.AES_ctr128_encrypt2(data, key, ref iv, ref ecount, ref num);

            //for (int i = 0; i < result.Length; i++)
            //{
            //    if (result[i] != result2[i])
            //    {

            //    }
            //}

            //return;

            var connectionSettings = new ConnectionSettings
            {
                ProtocolDCId = 4,
                ProtocolSecret = null,
                Host = "104.233.13.83",
                Port = 443,
                IPv4 = true
            };
            var dcOption = TLUtils.GetDCOption(IoC.Get<ICacheService>().GetConfig(), new TLInt(connectionSettings.ProtocolDCId));

            var request = new TLGetConfig();//new TLPing {PingId = TLLong.Random()};
            var message = ViewModel.MTProtoService.GetEncryptedTransportMessage(dcOption.AuthKey, dcOption.Salt, request);
            var packet = message.ToBytes();

            if (_socket == null)
            {
                var proxyConfig = IoC.Get<ITransportService>().GetProxyConfig();
                var socks5Proxy = proxyConfig.GetProxy() as TLSocks5Proxy;
                if (socks5Proxy != null)
                {
                    var proxy = new HostName(socks5Proxy.Server.ToString());
                    var proxySettings = new ProxySettings
                    {
                        Type = ProxyType.Socks5,
                        Host = socks5Proxy.Server.ToString(),
                        Port = socks5Proxy.Port.Value,
                        IPv4 = proxy.Type == HostNameType.Ipv4,
                        Username = socks5Proxy.Username.ToString(),
                        Password = socks5Proxy.Password.ToString()
                    };
                }

                var socket = new ConnectionSocketWrapper(connectionSettings, null);
                socket.Closed += wrapper =>
                {
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("Connection closed");
                };
                socket.PacketReceived += (wrapper, bytes) =>
                {
                    var position = 0;
                    var encryptedMessage = (TLEncryptedTransportMessage)new TLEncryptedTransportMessage().FromBytes(bytes, ref position);

                    encryptedMessage.Decrypt(dcOption.AuthKey);

                    position = 0;
                    var transportMessage = TLObject.GetObject<TLTransportMessage>(encryptedMessage.Data, ref position);
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("PacketReceived length=" + bytes.Length);
                };
                socket.Connect();

                Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                {
                    socket.StartReceive();
                });

                _socket = socket;
            }
            _socket.SendPacket(packet);
            //var bufferSize = socket.GetReceiveBufferSize();
            //var bufferSize2 = socket.GetSendBufferSize();
            //ViewModel.MTProtoService.GetCdnConfigAsync(
            //    result =>
            //    {

            //    },
            //    error =>
            //    {

            //    });

            //var publicConfigService = new PublicConfigService();
            //publicConfigService.GetAsync(
            //    result =>
            //    {

            //    },
            //    error =>
            //    {

            //    });

            //return;
            //var xml = "<RSAKeyValue><Modulus>yr+18Rex2ohtVy8sroGPBwXD3DOoKCSpjDqYoXgCqB7ioln4eDCFfOBUlfXUEvM/fnKCpF46VkAftlb4VuPDeQSS/ZxZYEGqHaywlroVnXHIjgqoxiAd192xRGreuXIaUKmkwlM9JID9WS2jUsTpzQ91L8MEPLJ/4zrBwZua8W5fECwCCh2c9G5IzzBm+otMS/YKwmR1olzRCyEkyAEjXWqBI9Ftv5eG8m0VkBzOG655WIYdyV0HfDK/NWcvGqa0w/nriMD6mDjKOryamw0OP9QuYgMN0C9xMW9y8SmP4h92OAWodTYgY1hZCxdv6cs5UnW9+PWvS+WIbkh+GaWYxw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

            //var provider = new RSACryptoServiceProvider();
            //provider.FromXmlString(xml);
            //var parameters = provider.ExportParameters(false);
            //var modulus = parameters.Modulus;
            //var exponent = parameters.Exponent;

            //var request = WebRequest.Create("https://google.com/");
            //request.Headers["Host"] = "dns-telegram.appspot.com";

            //string dataString;
            //using (var response = await request.GetResponseAsync())
            //{
            //    using (var s = response.GetResponseStream())
            //    {
            //        using (var readStream = new StreamReader(s))
            //        {
            //            dataString = readStream.ReadToEnd().Replace("\r\n", string.Empty).Replace("\n", string.Empty);
            //        }
            //    }
            //}

            //var data = Convert.FromBase64String(dataString);

            //var builder = new StringBuilder();
            //foreach (var d in data)
            //{
            //    builder.Append(", " + d);
            //}
            //System.Diagnostics.Debug.WriteLine(builder.ToString());

            //var dataBI = new BigInteger(data.Reverse().Concat(new byte[] { 0x00 }).ToArray());
            //var exponentBI = new BigInteger(exponent.Reverse().Concat(new byte[] { 0x00 }).ToArray());
            //var modulusBI = new BigInteger(modulus.Reverse().Concat(new byte[] { 0x00 }).ToArray());

            //var authKey = BigInteger.ModPow(dataBI, exponentBI, modulusBI).ToByteArray();
            //if (authKey[authKey.Length - 1] == 0x00)
            //{
            //    authKey = authKey.SubArray(0, authKey.Length - 1);
            //}

            //authKey = authKey.Reverse().ToArray();

            //if (authKey.Length > 256)
            //{
            //    var correctedAuth = new byte[256];
            //    Array.Copy(authKey, authKey.Length - 256, correctedAuth, 0, 256);
            //    authKey = correctedAuth;
            //}
            //else if (authKey.Length < 256)
            //{
            //    var correctedAuth = new byte[256];
            //    Array.Copy(authKey, 0, correctedAuth, 256 - authKey.Length, authKey.Length);
            //    for (var i = 0; i < 256 - authKey.Length; i++)
            //    {
            //        authKey[i] = 0;
            //    }
            //    authKey = correctedAuth;
            //}

            //var key = authKey.SubArray(0, 32);
            //var iv = authKey.SubArray(16, 16);
            //var encryptedData = authKey.SubArray(32, authKey.Length - 32);

            //var cipher = CipherUtilities.GetCipher("AES/CBC/NOPADDING");
            //var param = new KeyParameter(key);
            //cipher.Init(false, new ParametersWithIV(param, iv));
            //var decryptedData = cipher.DoFinal(encryptedData);

            //var hash = Telegram.Api.Helpers.Utils.ComputeSHA256(decryptedData.SubArray(0, 208));
            //for (var i = 0; i < 16; i++)
            //{
            //    if (hash[i] != decryptedData[208 + i])
            //    {
            //        return;
            //    }
            //}

            //var position = 4;
            //var configSimple = TLObject.GetObject<TLConfigSimple>(decryptedData, ref position);

            //MessageBox.Show(configSimple.ToString());

            return;
        }

        private void ProxyStatus_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.OpenProxySettings();
        }

        private void TogglePasscodeState_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.ChangePasscodeState();
        }
    }

    public class HeaderItem
    {
        public string DisplayName { get; set; }
    }
}