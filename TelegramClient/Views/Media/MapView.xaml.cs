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
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Media;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Media.MapTileSources;
using TelegramClient.Views.Search;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Media
{
    public partial class MapView
    {
        public static readonly DependencyProperty ContactLocationStringProperty = DependencyProperty.Register(
            "ContactLocationString", typeof (string), typeof (MapView), new PropertyMetadata(default(string)));

        public string ContactLocationString
        {
            get { return (string) GetValue(ContactLocationStringProperty); }
            set { SetValue(ContactLocationStringProperty, value); }
        }

        public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register(
            "Distance", typeof (double), typeof (MapView), new PropertyMetadata(default(double)));

        public double Distance
        {
            get { return (double) GetValue(DistanceProperty); }
            set { SetValue(DistanceProperty, value); }
        }

        protected MapViewModel ViewModel
        {
            get { return DataContext as MapViewModel; }
        }

        GeoCoordinateWatcher _coordinatWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);

        private readonly AppBarButton _searchButton = new AppBarButton
        {
            Text = AppResources.Search,
            IsEnabled = false,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.search.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _directionsButton = new AppBarButton
        {
            Text = AppResources.Directions,
            IconUri = new Uri("/Images/ApplicationBar/appbar.map.direction.png", UriKind.Relative)
        };

        private readonly AppBarButton _centerMeButton = new AppBarButton
        {
            Text = AppResources.MyLocation,
            IsEnabled = false,
            IconUri = new Uri("/Images/ApplicationBar/appbar.map.centerme.png", UriKind.Relative)
        };

        private readonly AppBarButton _attachButton = new AppBarButton
        {
            Text = AppResources.Attach,
            IsEnabled = false,
            IconUri = new Uri("/Images/ApplicationBar/appbar.map.checkin.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        private readonly AppBarMenuItem _switchModeMenuItem = new AppBarMenuItem
        {
            Text = AppResources.SwitchMode
        };

        public MapView()
        {
            InitializeComponent();

            LayoutRoot.Opacity = 0.0;

            ContactLocation.Visibility = Visibility.Collapsed;
            CurrentLocation.Visibility = Visibility.Collapsed;

            _searchButton.Click += (sender, args) => ViewModel.SearchLocation(ContactLocation.Location);
            _attachButton.Click += (sender, args) => ViewModel.AttchLocation(ContactLocation.Location);
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();
            _centerMeButton.Click += (sender, args) =>
            {
                var stateService = ViewModel.StateService;
                stateService.GetNotifySettingsAsync(settings =>
                {
                    if (settings.LocationServices)
                    {
                        if (_coordinatWatcher.Position.Location == GeoCoordinate.Unknown)
                        {
                            return;
                        }

                        Map.AnimationLevel = AnimationLevel.Full;
                        Map.SetView(_coordinatWatcher.Position.Location, 16.0);
                    }
                });       
            };

            _switchModeMenuItem.Click += (sender, args) =>
            {
                var tileSource = MapLayer.TileSources.FirstOrDefault() as GoogleMapsTileSource;
                if (tileSource != null)
                {
                    if (tileSource.MapsTileSourceType == GoogleMapsTileSourceType.Street)
                    {
                        tileSource.MapsTileSourceType = GoogleMapsTileSourceType.Satellite;
                    }
                    else if (tileSource.MapsTileSourceType == GoogleMapsTileSourceType.Satellite)
                    {
                        tileSource.MapsTileSourceType = GoogleMapsTileSourceType.Hybrid;
                    }
                    else
                    {
                        tileSource.MapsTileSourceType = GoogleMapsTileSourceType.Street;
                    }

                    MapLayer.TileSources.Clear();
                    MapLayer.TileSources.Add(tileSource);
                }
            };

            _directionsButton.Click += (sender, args) => ViewModel.ShowMapsDirections();

            _coordinatWatcher.StatusChanged += CoordinatWatcher_StatusChanged;
            _coordinatWatcher.PositionChanged += CoordinatWatcher_PositionChanged;

            //Loaded += OnLoaded; Invoked on BeginOpenStoryboard
            //Unloaded += OnUnloaded; Invoked on BeginCloseStoryboard

            Loaded += OnLoadedOnce;
            Loaded += (o, e) =>
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
            Unloaded += (o, e) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            StartWatching();

            //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("MapView.Loaded [status={0}, accuracy={1}]", _coordinatWatcher.Status, _coordinatWatcher.Position.Location.HorizontalAccuracy));

            if (ViewModel.MessageGeo != null)
            {
                var message = ViewModel.MessageGeo as TLMessage;
                if (message != null)
                {
                    var mediaGeo = message.Media as TLMessageMediaGeo;
                    var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                    if (mediaGeo != null)
                    {
                        var geoPoint = mediaGeo.Geo as TLGeoPoint;
                        if (geoPoint == null) return;

                        ContactLocation.Template = mediaGeoLive != null
                            ? (ControlTemplate)Resources["LiveContactPushpinTemplate"]
                            : (ControlTemplate)Resources["ContactPushpinTemplate"];
                        ContactLocation.Visibility = Visibility.Visible;
                        ContactLocation.DataContext = message;
                        ContactLocation.Location = new GeoCoordinate
                        {
                            Latitude = geoPoint.Lat.Value,
                            Longitude = geoPoint.Long.Value
                        };

                        Map.AnimationLevel = AnimationLevel.UserInput;
                        Map.ZoomLevel = 16.0;
                        Map.Center = ContactLocation.Location;
                    }

                    if (mediaGeoLive != null)
                    {
                        ViewModel.UpdateLiveLocations();
                    }
                }

                var decryptedMessage = ViewModel.MessageGeo as TLDecryptedMessage;
                if (decryptedMessage != null)
                {
                    var mediaGeo = decryptedMessage.Media as TLDecryptedMessageMediaGeoPoint;
                    if (mediaGeo != null)
                    {
                        var geoPoint = mediaGeo.Geo as TLGeoPoint;
                        if (geoPoint == null) return;

                        ContactLocation.Template = (ControlTemplate)Resources["ContactPushpinTemplate"];
                        ContactLocation.Visibility = Visibility.Visible;
                        ContactLocation.DataContext = decryptedMessage;
                        ContactLocation.Location = new GeoCoordinate
                        {
                            Latitude = geoPoint.Lat.Value,
                            Longitude = geoPoint.Long.Value
                        };

                        Map.AnimationLevel = AnimationLevel.UserInput;
                        Map.ZoomLevel = 16.0;
                        Map.Center = ContactLocation.Location;
                    }
                }

                AppBarPanel.Visibility = Visibility.Collapsed;

                BuildLocalizedAppBar(false);
            }
            else
            {
                AppBarPanel.Visibility = Visibility.Visible;
                ContactLocation.Template = (ControlTemplate)Resources["ContactPushpinTemplate"];

                BuildLocalizedAppBar(true);
            }

            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () =>
            {
                Map.AnimationLevel = AnimationLevel.Full;
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            StopWatching();
            //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("MapView.Unloaded watcher.Stop [status={0}, accuracy={1}]", _coordinatWatcher.Status, _coordinatWatcher.Position.Location.HorizontalAccuracy));
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                if (ViewModel.IsOpen)
                {
                    BeginOpenStoryboard();
                }
                else
                {
                    BeginCloseStoryboard();
                }
            }
        }

        private void OnLoadedOnce(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoadedOnce;

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.OpenEditor());
        }

        private static readonly Uri ExternalUri = new Uri(@"app://external/");

        private bool _fromExternalUri;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back 
                && _fromExternalUri
//                && !_isWatching
                )
            {
                StartWatching();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _fromExternalUri = e.Uri == ExternalUri;

            base.OnNavigatedFrom(e);
        }

        private void StopWatching()
        {
            if (_coordinatWatcher != null)
            {
                _coordinatWatcher.PositionChanged -= CoordinatWatcher_PositionChanged;
                _coordinatWatcher.StatusChanged -= CoordinatWatcher_StatusChanged;
                _coordinatWatcher.Dispose();

                _coordinatWatcher = null;
            }
        }

        private void StartWatching()
        {
            var stateService = ViewModel.StateService;
            stateService.GetNotifySettingsAsync(
                settings =>
                {
                    if (!settings.LocationServices)
                    {
                        settings.AskAllowingLocationServices = true;
                        stateService.SaveNotifySettingsAsync(settings);

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            var result = MessageBox.Show(
                                AppResources.AllowLocationServiceText,
                                AppResources.AllowLocationServicesTitle,
                                MessageBoxButton.OKCancel);

                            if (result == MessageBoxResult.OK)
                            {
                                settings.LocationServices = true;
                                stateService.SaveNotifySettingsAsync(settings);

                                ContinueStartWatching();
                            }
                            else
                            {
                                //_isWatching = false;
                            }
                        });
                    }
                    else
                    {
                        ContinueStartWatching();
                    }
                });
        }

        private void OpenLocationSettings()
        {
#if WP8
            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
#endif
        }

        private void ContinueStartWatching()
        {
            if (_coordinatWatcher == null)
            {
                _coordinatWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
                _coordinatWatcher.PositionChanged += CoordinatWatcher_PositionChanged;
                _coordinatWatcher.StatusChanged += CoordinatWatcher_StatusChanged;
            }

            if (_coordinatWatcher.Permission != GeoPositionPermission.Granted)
            {
                _coordinatWatcher.Dispose();
                _coordinatWatcher.PositionChanged -= CoordinatWatcher_PositionChanged;
                _coordinatWatcher.StatusChanged -= CoordinatWatcher_StatusChanged;

                _coordinatWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
                _coordinatWatcher.PositionChanged += CoordinatWatcher_PositionChanged;
                _coordinatWatcher.StatusChanged += CoordinatWatcher_StatusChanged;
            }

            if (_coordinatWatcher.Permission != GeoPositionPermission.Granted)
            {
                UpdateAttaching(false);
            }
            else
            {
                UpdateAttaching(true);
                SetContactLocationString();
            }

            _coordinatWatcher.Start(true);
        }

        private void UpdateAttaching(bool isEnabled)
        {
            LocationButton.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            LiveLocationButton.Visibility = isEnabled && !(ViewModel.MessageGeoLive is TLMessageEmpty) ? Visibility.Visible : Visibility.Collapsed;
            LocationSettingsPanel.Visibility = isEnabled? Visibility.Collapsed : Visibility.Visible;
            _centerMeButton.IsEnabled = isEnabled;
            _attachButton.IsEnabled = isEnabled;
            _searchButton.IsEnabled = isEnabled;
        }

        //private bool _isWatching;

        private void CoordinatWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("MapView.PositionChanged [status={0}, accuracy={1}]", _coordinatWatcher.Status, _coordinatWatcher.Position.Location.HorizontalAccuracy));

            var distance = CurrentLocation.Location.GetDistanceTo(ContactLocation.Location);

            Distance = distance;
            CurrentLocation.Location = e.Position.Location;

            if (ViewModel.MessageGeo == null && distance < Constants.GlueGeoPointsDistance)
            {
                ContactLocation.Location = e.Position.Location;
            }

            SetContactLocationString();

            ViewModel.OnPositionChanged(e.Position);
        }

        private void CoordinatWatcher_StatusChanged(object o, GeoPositionStatusChangedEventArgs e)
        {
            //Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("MapView.StatusChanged [status={0}, accuracy={1}]", _coordinatWatcher.Status, _coordinatWatcher.Position.Location.HorizontalAccuracy));


            if (e.Status == GeoPositionStatus.Ready || _coordinatWatcher.Position.Location != GeoCoordinate.Unknown)
            {

                CurrentLocation.Visibility = Visibility.Visible;
                CurrentLocation.Location = _coordinatWatcher.Position.Location;
                if (!CurrentLocation.Location.IsUnknown)
                {
                    UpdateAttaching(true);
                }

                Distance = CurrentLocation.Location.GetDistanceTo(ContactLocation.Location);

                if (ViewModel.MessageGeo == null && !_contactLocationChoosen)
                {
                    ContactLocation.Visibility = Visibility.Visible;
                    ContactLocation.Location = _coordinatWatcher.Position.Location;
                    Map.AnimationLevel = AnimationLevel.UserInput;
                    Map.ZoomLevel = 16.0;
                    Map.Center = _coordinatWatcher.Position.Location;

                    SetContactLocationString();
                }

                if (ViewModel.MessageGeo == null)
                {
                    ViewModel.GetVenues(_coordinatWatcher.Position.Location);
                }
            }
        }


#if DEBUG
        ~MapView()
        {
            //TLUtils.WritePerformance("++MapView dstr");
        }
#endif

        private void BuildLocalizedAppBar(bool attaching)
        {

            HeaderGrid.Visibility = attaching ? Visibility.Collapsed : Visibility.Visible;
            FooterGrid.Visibility = attaching ? Visibility.Collapsed : Visibility.Visible;

            if (ApplicationBar == null)
            {
                ApplicationBar = new ApplicationBar{ Opacity = 1.0 };
                var foregroundColor = Colors.White;
                foregroundColor.A = 254;
                ApplicationBar.ForegroundColor = foregroundColor;

                var backgroundColor = new Color();
                backgroundColor.A = 255;
                backgroundColor.R = 31;
                backgroundColor.G = 31;
                backgroundColor.B = 31;
                ApplicationBar.BackgroundColor = backgroundColor;
                if (attaching)
                {
                    ApplicationBar.Buttons.Add(_searchButton);               
                }
                else
                {
#if WP8
                    ApplicationBar.Buttons.Add(_directionsButton);
#endif
                }
                //var color = new Color { A = 217 };
                //ApplicationBar.BackgroundColor = color;
                ApplicationBar.Buttons.Add(_centerMeButton);
                ApplicationBar.MenuItems.Add(_switchModeMenuItem);
            }
        }

        private bool _contactLocationChoosen;

        private void GestureListener_Hold(object sender, GestureEventArgs e)
        {
            if (ViewModel.MessageGeo != null) return;

            var point = new Point(e.GetPosition(Map).X, e.GetPosition(Map).Y);
            var location = Map.ViewportPointToLocation(point);
            _contactLocationChoosen = true;
            ContactLocation.Visibility = Visibility.Visible;
            ContactLocation.Location = location;

            var distance = CurrentLocation.Location.GetDistanceTo(ContactLocation.Location);
            if (distance < Constants.GlueGeoPointsDistance)
            {
                ContactLocation.Location = CurrentLocation.Location;
            }

            UpdateAttaching(true);
            SetContactLocationString();
        }

        public void SetContactLocationString()
        {
            if (ContactLocation.Location.IsUnknown
                || (ContactLocation.Location.Latitude == 0.0 && ContactLocation.Location.Longitude == 0.0))
            {
                ContactLocationString = AppResources.Loading;
                return;
            }

            ContactLocationString = string.Format("({0}, {1})", ContactLocation.Location.Latitude.ToString("###.#######", new CultureInfo("en-us")), ContactLocation.Location.Longitude.ToString("###.#######", new CultureInfo("en-us")));            
        }

        private void AttachLocation_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModel.AttchLocation(ContactLocation.Location);
        }

        private void StopLiveLocation_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.StopLiveLocation();
        }

        private void AttachLiveLocation_OnTap(object sender, GestureEventArgs e)
        {
            var message = ViewModel.MessageGeoLive as TLMessage;
            if (message != null)
            {
                var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                if (mediaGeoLive != null && mediaGeoLive.Active)
                {
                    StopLiveLocation_OnTap(null, null);
                    return;
                }
            }

            var list = new List<TimerSpan>
            {
                new TimerSpan(AppResources.MinuteGenitivePlural, "15", (int) TimeSpan.FromMinutes(15.0).TotalSeconds),
                new TimerSpan(AppResources.HourNominativeSingular, "1", (int) TimeSpan.FromHours(1.0).TotalSeconds),
                new TimerSpan(AppResources.HourGenitivePlural, "8", (int) TimeSpan.FromHours(8.0).TotalSeconds)
            };

            var contact = ViewModel.With as TLUserBase;
            var subtitle = contact != null
                ? string.Format(AppResources.ShareLiveLocationToContactTimerSubtitle, contact.FullName2)
                : AppResources.ShareLiveLocationToChatTimerSubtitle;

            var chooseTTLView = new ChooseGeoLivePeriodView
            {
                Margin = new Thickness(0.0, -34.0, 0.0, -6.0),
                Subtitle = { Text = subtitle }
            };
            ShellViewModel.ShowCustomMessageBox(null, null, AppResources.Share.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                result =>
                {
                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        var selectedItem = list[0];
                        if (chooseTTLView.Period1Hour.IsChecked == true)
                        {
                            selectedItem = list[1];
                        }
                        else if (chooseTTLView.Period8Hours.IsChecked == true)
                        {
                            selectedItem = list[2];
                        }

                        Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                        {
                            ViewModel.AttachGeoLive(ContactLocation.Location, selectedItem);
                        });
                    }
                },
                chooseTTLView);

            //ViewModel.AttchLiveLocation(ContactLocation.Location);
        }

        private void LocationSettings_OnClick(object sender, RoutedEventArgs e)
        {
            OpenLocationSettings();
        }

        private void BeginOpenStoryboard()
        {
            ViewModel.RestoreParentHitTest(false);

            CurrentLocation.Template = (ControlTemplate)Resources["PushpinTemplate"];
            ContactLocation.Visibility = Visibility.Visible;
            for (int i = 0; i < Map.Children.Count; i++)
            {
                var pushpin = Map.Children[i] as Pushpin;
                if (pushpin != null && pushpin.Tag == "live")
                {
                    Map.Children.RemoveAt(i--);
                }
            }

            OnLoaded(null, null);

            Venues.ScrollToTop();
            SearchPlaceholder.Content = null;

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            var storyboard = new Storyboard();
            var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(translateAnimaiton, LayoutRoot);
            Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateAnimaiton);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                LayoutRoot.Opacity = 1.0;
                LayoutRoot.Visibility = Visibility.Visible;
                storyboard.Begin();
            });

            storyboard.Completed += (sender, args) =>
            {
                Map.Visibility = Visibility.Visible;
            };
        }

        private void BeginCloseStoryboard()
        {
            OnUnloaded(null, null);

            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, LayoutRoot);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateImageAniamtion);

            //var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            //translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            //translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            //Storyboard.SetTarget(translateBarAniamtion, Bar);
            //Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            //storyboard.Children.Add(translateBarAniamtion);
            storyboard.Completed += (sender, args) =>
            {
                LayoutRoot.Visibility = Visibility.Collapsed;
                ViewModel.RestoreParentHitTest(true);
            };
            storyboard.Begin();
        }

        public static readonly DependencyProperty ImageOpacityMaskProperty = DependencyProperty.RegisterAttached(
            "ImageOpacityMask", typeof(ImageSource), typeof(MapView), new PropertyMetadata(default(ImageSource), OnImageOpacityMaskChanged));

        private static void OnImageOpacityMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var border = d as Border;
            if (border != null)
            {
                var imageSource = e.NewValue as ImageSource;
                if (imageSource != null)
                {
                    var brush = new ImageBrush { ImageSource = imageSource };
                    brush.ImageOpened += (sender, args) =>
                    {
                        border.Opacity = 1.0;
                    };

                    border.Opacity = 0.0;
                    border.OpacityMask = brush;
                }
            }
        }

        public static void SetImageOpacityMask(DependencyObject element, Brush value)
        {
            element.SetValue(ImageOpacityMaskProperty, value);
        }

        public static ImageSource GetImageOpacityMask(DependencyObject element)
        {
            return (ImageSource)element.GetValue(ImageOpacityMaskProperty);
        }

        private void ContextMenu_OnHold(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void ContextMenu_OnTap(object sender, GestureEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void SwitchMapsTileSource(GoogleMapsTileSourceType type)
        {
            var tileSource = MapLayer.TileSources.FirstOrDefault() as GoogleMapsTileSource;
            if (tileSource != null)
            {
                if (tileSource.MapsTileSourceType == type)
                {
                    return;
                }

                tileSource.MapsTileSourceType = type;

                MapLayer.TileSources.Clear();
                MapLayer.TileSources.Add(tileSource);
            }
        }

        private void Map_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchMapsTileSource(GoogleMapsTileSourceType.Street);
        }

        private void Satellite_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchMapsTileSource(GoogleMapsTileSourceType.Satellite);
        }

        private void Hybrid_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchMapsTileSource(GoogleMapsTileSourceType.Hybrid);
        }

        private void NavigateButton_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.ShowMapsDirections();
        }

        private void CenterMeButton_OnTap(object sender, GestureEventArgs e)
        {
            var stateService = ViewModel.StateService;
            stateService.GetNotifySettingsAsync(settings =>
            {
                if (settings.LocationServices)
                {
                    if (_coordinatWatcher.Position.Location == GeoCoordinate.Unknown)
                    {
                        return;
                    }

                    Map.AnimationLevel = AnimationLevel.Full;
                    Map.SetView(_coordinatWatcher.Position.Location, 16.0);
                }
            });  
        }

        private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
            CenterMeBorder.Opacity = 1.0;
        }

        private void Search_OnTap(object sender, GestureEventArgs e)
        {
            var view = new SearchVenuesView();
            var viewModel = IoC.Get<SearchVenuesViewModel>();
            viewModel.Location = ContactLocation.Location;
            viewModel.AttachAction = venue =>
            {
                ViewModel.AttachVenue(venue);
            };
            view.DataContext = viewModel;

            SearchPlaceholder.Content = view;

            //ViewModel.SearchLocation(ContactLocation.Location);
        }

        private void MediaGeo_OnTap(object sender, GestureEventArgs e)
        {
            if (ViewModel.MessageGeo == null) return;

            var message = ViewModel.MessageGeo as TLMessage;
            if (message != null)
            {
                var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                if (mediaGeoLive != null)
                {
                    var footerMessage = ViewModel.FooterMessage as TLMessage;
                    if (footerMessage == null)
                    {
                        AttachLiveLocation_OnTap(null, null);
                    }
                    else
                    {
                        mediaGeoLive = footerMessage.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null && mediaGeoLive.Active)
                        {
                            StopLiveLocation_OnTap(null, null);
                        }
                        else
                        {
                            AttachLiveLocation_OnTap(null, null);
                        }
                    }
                    return;
                }

                var mediaGeo = message.Media as TLMessageMediaGeo;
                if (mediaGeo != null)
                {
                    var geoPoint = mediaGeo.Geo as TLGeoPoint;
                    if (geoPoint == null) return;

                    Map.AnimationLevel = AnimationLevel.Full;
                    Map.SetView(new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value), 16.0);

                    return;
                }
            }

            var decryptedMessage = ViewModel.MessageGeo as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                var mediaGeo = decryptedMessage.Media as TLDecryptedMessageMediaGeoPoint;
                if (mediaGeo != null)
                {
                    var geoPoint = mediaGeo.Geo as TLGeoPoint;
                    if (geoPoint == null) return;

                    Map.AnimationLevel = AnimationLevel.Full;
                    Map.SetView(new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value), 16.0);

                    return;
                }
            }
        }

        private void Map_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                Map_OnClick(null, null);
            });
        }

        private void Satellite_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                Satellite_OnClick(null, null);
            });
        }

        private void Hybrid_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                Hybrid_OnClick(null, null);
            });
        }

        public void UpdateLiveLocation(TLMessage48 message)
        {
            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            if (message.FromId == null) return;

            for (var i = 0; i < Map.Children.Count; i++)
            {
                var child = Map.Children[i];
                var pushpin = child as Pushpin;
                if (pushpin != null
                    && string.Equals(pushpin.Tag, "live"))
                {
                    var pushpinMessage = pushpin.DataContext as TLMessage;
                    if (pushpinMessage != null
                        && pushpinMessage.FromId != null
                        && pushpinMessage.FromId.Value == message.FromId.Value)
                    {
                        var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
                        if (geoPoint != null)
                        {
                            pushpin.Location = new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value);
                        }
                        else
                        {
                            Map.Children.RemoveAt(i);
                        }
                        return;
                    }
                }
            }

            if (ContactLocation.Visibility == Visibility.Visible)
            {
                var contactMessage = ContactLocation.DataContext as TLMessage;
                if (contactMessage != null && contactMessage.FromId != null && contactMessage.FromId.Value == message.FromId.Value)
                {
                    var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
                    if (geoPoint != null)
                    {
                        ContactLocation.Visibility = Visibility.Visible;
                        ContactLocation.Location = new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value);
                    }
                    else
                    {
                        ContactLocation.Visibility = Visibility.Collapsed;
                    }
                    return;
                }
            }

            mediaGeoLive.Date = message.Date;
            mediaGeoLive.EditDate = message.EditDate;
            mediaGeoLive.From = message.From;
            if (mediaGeoLive.Active)
            {
                var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
                if (geoPoint != null)
                {
                    var location = new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value);

                    var pushPin = new Pushpin
                    {
                        CacheMode = new BitmapCache(),
                        Template = (ControlTemplate)Resources["LiveContactPushpinTemplate"],
                        DataContext = message,
                        Tag = "live",
                        Location = location
                    };

                    if (message.FromId.Value == IoC.Get<IStateService>().CurrentUserId)
                    {
                        //locations.Add(CurrentLocation.Location);
                        pushPin.SetBinding(Pushpin.LocationDependencyProperty, new Binding { Source = CurrentLocation, Path = new PropertyPath("Location") });
                    }
                    else
                    {
                        //locations.Add(location);
                    }

                    Map.Children.Add(pushPin);
                }
            }
        }

        public void UpdateLiveLocations()
        {
            if (ViewModel.LiveLocations.Count > 0)
            {
                var locations = new List<GeoCoordinate>();
                ContactLocation.Visibility = Visibility.Collapsed;
                foreach (var messageBase in ViewModel.LiveLocations)
                {
                    var message = messageBase as TLMessage48;
                    if (message != null)
                    {
                        var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null)
                        {
                            mediaGeoLive.Date = message.Date;
                            mediaGeoLive.EditDate = message.EditDate;
                            mediaGeoLive.From = message.From;
                            if (mediaGeoLive.Active)
                            {
                                var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
                                if (geoPoint != null)
                                {
                                    var location = new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value);

                                    var pushPin = new Pushpin
                                    {
                                        CacheMode = new BitmapCache(),
                                        Template = (ControlTemplate)Resources["LiveContactPushpinTemplate"],
                                        DataContext = message,
                                        Tag = "live",
                                        Location = location
                                    };

                                    if (message.FromId.Value == IoC.Get<IStateService>().CurrentUserId)
                                    {
                                        locations.Add(CurrentLocation.Location);
                                        pushPin.SetBinding(Pushpin.LocationDependencyProperty, new Binding {Source = CurrentLocation, Path = new PropertyPath("Location")});
                                    }
                                    else
                                    {
                                        locations.Add(location);
                                    }

                                    Map.Children.Add(pushPin);
                                }
                            }
                        }
                    }
                }

                if (locations.Count > 0)
                {
                    Map.AnimationLevel = AnimationLevel.UserInput;
                    Map.ZoomLevel = 16.0;

                    if (locations.Count == 1)
                    {
                        Map.Center = locations[0];
                    }
                    else
                    {
                        Map.Center = locations[0];
                        return;
                        double num1 = -90.0;
                        double num2 = 90.0;
                        double num3 = 180.0;
                        double num4 = -180.0;
                        foreach (GeoCoordinate location in locations)
                        {
                            num1 = Math.Max(num1, location.Latitude);
                            num2 = Math.Min(num2, location.Latitude);
                            num3 = Math.Min(num3, location.Longitude);
                            num4 = Math.Max(num4, location.Longitude);
                        }

                        var locationRect = new LocationRect(num1, num3, num2, num4);

                        var boundedRect = new LocationRect(locationRect.Center, locationRect.Width * 1.2, locationRect.Height * 1.4);

                        Deployment.Current.Dispatcher.BeginInvoke(() => Map.SetView(boundedRect))
                        ;
                    }
                }
                else
                {
                    ContactLocation.Visibility = Visibility.Visible;
                }
            }
        }
    }
}