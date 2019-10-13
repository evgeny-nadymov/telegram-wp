// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Controls;
using VisualTreeExtensions = Telegram.Controls.Extensions.VisualTreeExtensions;

namespace TelegramClient.Views.Media
{
    public partial class FullMediaView
    {
        private readonly TelegramAppBarButton _searchButton = new TelegramAppBarButton
        {
            Text = AppResources.Search,
            IsEnabled = true,
            ImageSource = new BitmapImage(new Uri("/Images/ApplicationBar/ic_search_2x.png", UriKind.Relative))
        };

        private readonly TelegramAppBarButton _manageButton = new TelegramAppBarButton
        {
            Text = AppResources.Manage,
            IsEnabled = true,
            ImageSource = new BitmapImage(new Uri("/Images/W10M/ic_select_2x.png", UriKind.Relative))
        };

        private readonly TelegramAppBarButton _forwardButton = new TelegramAppBarButton
        {
            Text = AppResources.Forward,
            IsEnabled = true,
            ImageSource = new BitmapImage(new Uri("/Images/W10M/ic_share_2x.png", UriKind.Relative))
        };

        private readonly TelegramAppBarButton _deleteButton = new TelegramAppBarButton
        {
            Text = AppResources.Delete,
            IsEnabled = true,
            ImageSource = new BitmapImage(new Uri("/Images/W10M/ic_delete_2x.png", UriKind.Relative))
        };

        private readonly TelegramAppBarButton _cancelButton = new TelegramAppBarButton
        {
            Text = AppResources.Cancel,
            IsEnabled = true,
            ImageSource = new BitmapImage(new Uri("/Images/W10M/ic_cancel_2x.png", UriKind.Relative))
        };

        public FullMediaViewModel ViewModel 
        {
            get { return DataContext as FullMediaViewModel; }
        }

        public FullMediaView()
        {
            var timer = Stopwatch.StartNew();

            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            OptimizeFullHD();

            Grid.SetColumn(_searchButton, 3);
            Grid.SetColumn(_manageButton, 2);

            Grid.SetColumn(_deleteButton, 1);
            Grid.SetColumn(_forwardButton, 2);
            Grid.SetColumn(_cancelButton, 3);

            _searchButton.Tap += (sender, args) => ViewModel.Search();
            _manageButton.Tap += (sender, args) => ViewModel.Manage();
            _forwardButton.Tap += (sender, args) => ViewModel.Forward();
            _deleteButton.Tap += (sender, args) => ViewModel.Delete();
            _cancelButton.Tap += (sender, args) => CancelSelection();

            Loaded += (sender, args) =>
            {
                TimerString.Text = timer.Elapsed.ToString();

                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                ViewModel.Files.PropertyChanged += OnFilesPropertyChanged;
                ViewModel.Links.PropertyChanged += OnLinksPropertyChanged;
                ViewModel.Music.PropertyChanged += OnMusicPropertyChanged;

                if (ViewModel.ImageViewer != null)
                    ViewModel.ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
                if (ViewModel.AnimatedImageViewer != null)
                    ViewModel.AnimatedImageViewer.PropertyChanged += OnAnimatedImageViewerPropertyChanged;

                ViewModel.Files.Items.CollectionChanged += OnFilesCollectionChanged;
                ViewModel.Links.Items.CollectionChanged += OnLinksCollectionChanged;
                ViewModel.Music.Items.CollectionChanged += OnMusicCollectionChanged;

                BuildLocalizedAppBar();
                //ReturnItemsVisibility();
            };

            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                ViewModel.Files.PropertyChanged -= OnFilesPropertyChanged;
                ViewModel.Links.PropertyChanged -= OnLinksPropertyChanged;
                ViewModel.Music.PropertyChanged -= OnMusicPropertyChanged;

                if (ViewModel.ImageViewer != null)
                    ViewModel.ImageViewer.PropertyChanged -= OnImageViewerPropertyChanged;
                if (ViewModel.AnimatedImageViewer != null)
                    ViewModel.AnimatedImageViewer.PropertyChanged -= OnAnimatedImageViewerPropertyChanged;

                ViewModel.Files.Items.CollectionChanged -= OnFilesCollectionChanged;
                ViewModel.Links.Items.CollectionChanged -= OnLinksCollectionChanged;
                ViewModel.Music.Items.CollectionChanged -= OnMusicCollectionChanged;
            };

            Items.SelectionChanged += (sender, args) =>
            {
                if (_applicationBar == null) return;

                _applicationBar.IsVisible = false;
                AppBarPanel.Visibility = Visibility.Collapsed;

                if (Items.SelectedItem is FilesViewModel<IInputPeer>
                    || Items.SelectedItem is LinksViewModel<IInputPeer>
                    || Items.SelectedItem is MusicViewModel<IInputPeer>)
                {
                    if (Items.SelectedItem is FilesViewModel<IInputPeer>)
                    {
                        OnFilesCollectionChanged(sender, null);
                    }
                    else if (Items.SelectedItem is LinksViewModel<IInputPeer>)
                    {
                        OnLinksCollectionChanged(sender, null);
                    }
                    else
                    {
                        OnMusicCollectionChanged(sender, null);
                    }
                    AppBarPanel.Visibility = Visibility.Visible;
                    AppBarPanel.Buttons.Clear();
                    AppBarPanel.Buttons.Add(_searchButton);
                    AppBarPanel.Buttons.Add(_manageButton);

                    _applicationBar.IsVisible = true;
                    //_applicationBar.Buttons.Clear();
                    //_applicationBar.Buttons.Add(_searchButton);
                    //_applicationBar.Buttons.Add(_manageButton);
                }
            };
        }

        ~FullMediaView()
        {
            
        }

        private void OptimizeFullHD()
        {
#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            //if (!isFullHD) return;
#endif

            Items.HeaderTemplate = (DataTemplate)Application.Current.Resources["FullHDPivotHeaderTemplate"];
        }

        private void OnFilesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.Files.IsSelectionEnabled))
            {
                if (ViewModel.Files.IsSelectionEnabled)
                {
                    SwitchToSelectionMode(ViewModel.Files.IsGroupActionEnabled);
                }
                else
                {
                    SwithToNormalMode();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.Files.IsGroupActionEnabled))
            {
                var isGroupActionEnabled = ViewModel.Files.IsGroupActionEnabled;

                _forwardButton.IsEnabled = isGroupActionEnabled;
                _deleteButton.IsEnabled = isGroupActionEnabled;
            }
        }

        private void OnLinksPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.Links.IsSelectionEnabled))
            {
                if (ViewModel.Links.IsSelectionEnabled)
                {
                    SwitchToSelectionMode(ViewModel.Links.IsGroupActionEnabled);
                }
                else
                {
                    SwithToNormalMode();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.Links.IsGroupActionEnabled))
            {
                var isGroupActionEnabled = ViewModel.Links.IsGroupActionEnabled;

                _forwardButton.IsEnabled = isGroupActionEnabled;
                _deleteButton.IsEnabled = isGroupActionEnabled;
            }
        }



        private void OnMusicPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.Music.IsSelectionEnabled))
            {
                if (ViewModel.Music.IsSelectionEnabled)
                {
                    SwitchToSelectionMode(ViewModel.Music.IsGroupActionEnabled);
                }
                else
                {
                    SwithToNormalMode();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.Music.IsGroupActionEnabled))
            {
                var isGroupActionEnabled = ViewModel.Music.IsGroupActionEnabled;

                _forwardButton.IsEnabled = isGroupActionEnabled;
                _deleteButton.IsEnabled = isGroupActionEnabled;
            }
        }

        private PivotHeadersControl _pivotHeadersControl;

        private void SwithToNormalMode()
        {
            if (AppBarPanel == null || AppBarPanel.Buttons == null) return;

            AppBarPanel.Buttons.Clear();
            AppBarPanel.Buttons.Add(_searchButton);
            AppBarPanel.Buttons.Add(_manageButton);

            //_applicationBar.Buttons.Clear();
            //_applicationBar.Buttons.Add(_searchButton);
            //_applicationBar.Buttons.Add(_manageButton);

#if WP8
            ShowPivotHeadersControl();
#endif
        }

        private void SwitchToSelectionMode(bool isGroupActionEnabled)
        {
            if (AppBarPanel == null || AppBarPanel.Buttons == null) return;

            var channel = ViewModel.CurrentItem as TLChannel;
            //var isGroupActionEnabled = ViewModel.Files.IsGroupActionEnabled;

            _forwardButton.IsEnabled = isGroupActionEnabled;
            _deleteButton.IsEnabled = isGroupActionEnabled;


            AppBarPanel.Buttons.Clear();
            AppBarPanel.Buttons.Add(_forwardButton);
            if (channel == null || channel.Creator)
            {
                AppBarPanel.Buttons.Add(_deleteButton);
            }
            AppBarPanel.Buttons.Add(_cancelButton);


            //_applicationBar.Buttons.Clear();
            //_applicationBar.Buttons.Add(_forwardButton);

            //if (channel == null || channel.Creator)
            //{
            //    _applicationBar.Buttons.Add(_deleteButton);
            //}
#if WP8
            ViewModel.SetSelectedCount(0);
            HidePivotHeadersControl();
#endif
        }

        private void HidePivotHeadersControl()
        {
            _pivotHeadersControl = _pivotHeadersControl ?? VisualTreeExtensions.FindChildOfType<PivotHeadersControl>(Items);
            if (_pivotHeadersControl == null) return;

            _pivotHeadersControl.RenderTransform = new TranslateTransform();
            _pivotHeadersControl.CacheMode = new BitmapCache();

            Items.IsLocked = true;

            var storyboard = new Storyboard { BeginTime = TimeSpan.FromSeconds(0.2) };

            var transformAnimaion2 = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.2), From = 0.0, To = -72.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } };
            Storyboard.SetTarget(transformAnimaion2, _pivotHeadersControl);
            Storyboard.SetTargetProperty(transformAnimaion2, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(transformAnimaion2);

            var opacityAnimaion2 = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.2), From = 1.0, To = 0.0 };
            Storyboard.SetTarget(opacityAnimaion2, _pivotHeadersControl);
            Storyboard.SetTargetProperty(opacityAnimaion2, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion2);

            var opacityAnimaion = new DoubleAnimation { Duration = TimeSpan.FromSeconds(0.2), From = 0.0, To = 1.0 };
            Storyboard.SetTarget(opacityAnimaion, SelectionCaption);
            Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion);

            Execute.BeginOnUIThread(storyboard.Begin);
        }

        private void ShowPivotHeadersControl()
        {
            _pivotHeadersControl = _pivotHeadersControl ?? VisualTreeExtensions.FindChildOfType<PivotHeadersControl>(Items);
            if (_pivotHeadersControl == null) return;

            var storyboard = new Storyboard();

            var transformAnimaion2 = new DoubleAnimation { From = -72.0, To = 0.0, Duration = TimeSpan.FromSeconds(0.2), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } };
            Storyboard.SetTarget(transformAnimaion2, _pivotHeadersControl);
            Storyboard.SetTargetProperty(transformAnimaion2, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(transformAnimaion2);

            var opacityAnimaion2 = new DoubleAnimation { From = 0.0, To = 1.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion2, _pivotHeadersControl);
            Storyboard.SetTargetProperty(opacityAnimaion2, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion2);

            var opacityAnimaion = new DoubleAnimation { From = 1.0, To = 0.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion, SelectionCaption);
            Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion);

            storyboard.Completed += (o, e) =>
            {
                Items.IsLocked = false;
            };

            Execute.BeginOnUIThread(storyboard.Begin);
        }

        private void ReturnItemsVisibility()
        {
            var selectedIndex = Items.SelectedIndex;
            ((ViewModelBase)Items.Items[selectedIndex]).Visibility = Visibility.Visible;

            Execute.BeginOnUIThread(() =>
            {
                foreach (ViewModelBase item in Items.Items)
                {
                    item.Visibility = Visibility.Visible;
                }
            });
        }

        private ApplicationBar _applicationBar;

        private void BuildLocalizedAppBar()
        {
            if (_applicationBar != null) return;

            _applicationBar = new ApplicationBar();
            _applicationBar.IsVisible = false;
            AppBarPanel.Visibility = Visibility.Collapsed;
        }

        private IApplicationBar _prevApplicationBar;

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.AnimatedImageViewer)
                && ViewModel.AnimatedImageViewer != null)
            {
                ViewModel.AnimatedImageViewer.PropertyChanged += OnAnimatedImageViewerPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer)
                && ViewModel.ImageViewer != null)
            {
                ViewModel.ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
            }
        }

        private void OnImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer.IsOpen))
            {
                ViewModel.NotifyOfPropertyChange(() => ViewModel.IsViewerOpen);

                if (ViewModel.ImageViewer.IsOpen)
                {
                    _prevApplicationBar = ApplicationBar;
                    ApplicationBar = ((ImageViewerView)ImageViewer.Content).ApplicationBar;
                }
                else
                {
                    if (_prevApplicationBar != null)
                    {
                        ApplicationBar = _prevApplicationBar;
                    }
                }
            }
        }

        private void OnAnimatedImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AppBarPanel.Visibility = !ViewModel.AnimatedImageViewer.IsOpen ? Visibility.Visible : Visibility.Collapsed;
            _applicationBar.IsVisible = !ViewModel.AnimatedImageViewer.IsOpen;
        }

        private void OnFilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _searchButton.IsEnabled = ViewModel.Files.Items.Count > 0;
            _manageButton.IsEnabled = ViewModel.Files.Items.Count > 0;
            ViewModel.Files.NotifyOfPropertyChange(() => ViewModel.Files.IsEmptyList);
        }

        private void OnLinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _searchButton.IsEnabled = ViewModel.Links.Items.Count > 0;
            _manageButton.IsEnabled = ViewModel.Links.Items.Count > 0;
            ViewModel.Links.NotifyOfPropertyChange(() => ViewModel.Links.IsEmptyList);
        }

        private void OnMusicCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _searchButton.IsEnabled = ViewModel.Music.Items.Count > 0;
            _manageButton.IsEnabled = ViewModel.Music.Items.Count > 0;
            ViewModel.Music.NotifyOfPropertyChange(() => ViewModel.Music.IsEmptyList);
        }

        private void FullMediaView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (ViewModel.ImageViewer != null
                && ViewModel.ImageViewer.IsOpen)
            {
                ViewModel.ImageViewer.CloseViewer();
                e.Cancel = true;
                return;
            }

            if (ViewModel.AnimatedImageViewer != null
                && ViewModel.AnimatedImageViewer.IsOpen)
            {
                ViewModel.AnimatedImageViewer.CloseViewer();
                e.Cancel = true;
                return;
            }

            if (CancelSelection())
            {
                e.Cancel = true;
                return;
            }

            ViewModel.CancelLoading();
        }

        private bool CancelSelection()
        {
            if (ViewModel.Files.IsSelectionEnabled)
            {
                ViewModel.Files.IsSelectionEnabled = false;
                return true;
            }

            if (ViewModel.Links.IsSelectionEnabled)
            {
                ViewModel.Links.IsSelectionEnabled = false;
                return true;
            }

            if (ViewModel.Music.IsSelectionEnabled)
            {
                ViewModel.Music.IsSelectionEnabled = false;
                return true;
            }

            return false;
        }

        private static readonly Uri ExternalUri = new Uri(@"app://external/");

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.Uri != ExternalUri)
            {
                var selectedIndex = Items.SelectedIndex;
                for (var i = 0; i < Items.Items.Count; i++)
                {
                    //if (selectedIndex != i)
                    {
                        ((ViewModelBase)Items.Items[i]).Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private bool _once;

        private void TelegramNavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                ViewModel.ForwardInAnimationComplete();
                _once = true;
            }

            //MessageBox.Show("EndTransition");
            ReturnItemsVisibility();
        }

        private void Select_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                ViewModel.Manage();
            });
        }

        private void Search_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                ViewModel.Search();
            });
        }
    }
}