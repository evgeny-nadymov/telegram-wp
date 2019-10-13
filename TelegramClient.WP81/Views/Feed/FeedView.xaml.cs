// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Feed;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Media;

namespace TelegramClient.Views.Feed
{
    public partial class FeedView
    {
        private FeedViewModel ViewModel { get { return DataContext as FeedViewModel; } }

        private readonly TelegramAppBarButton _setPhotoButton = new TelegramAppBarButton
        {
            Text = AppResources.SetPhoto,
            ImageSource = new BitmapImage(new Uri("/Images/W10M/ic_photo_2x.png", UriKind.Relative))
        };

        private readonly TelegramAppBarButton _editButton = new TelegramAppBarButton
        {
            Text = AppResources.Edit,
            ImageSource = new BitmapImage(new Uri("/Images/W10M/ic_edit_2x.png", UriKind.Relative))
        };

        private readonly TelegramAppBarButton _addButton = new TelegramAppBarButton
        {
            Text = AppResources.Add,
            ImageSource = new BitmapImage(new Uri("/Images/ApplicationBar/ic_plus_2x.png", UriKind.Relative))
        };

        private readonly ListBoxItem _reportMenuItem = new ListBoxItem
        {
            Content = AppResources.Report,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _setAdminsMenuItem = new ListBoxItem
        {
            Content = AppResources.SetAdmins,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _convertToSupergroupMenuItem = new ListBoxItem
        {
            Content = AppResources.ConvertToSupergroup,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _deleteAndExitMenuItem = new ListBoxItem
        {
            Content = AppResources.DeleteAndExitGroup,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        public FeedView()
        {
            var timer = Stopwatch.StartNew();

            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                NotificationsSwitch.Style = (Style)Application.Current.Resources["ProfileLightToggleSwitch"];
                LayoutRoot.Background = ShellView.CaptionBrush;
                AppBarPanel.MorePanelBackgroundBrush = ShellView.CaptionBrush;
                AppBarPanel.BackgroundBrush = ShellView.CaptionBrush;

                _setPhotoButton.LabelForeground = new SolidColorBrush(Colors.White);
                _editButton.LabelForeground = new SolidColorBrush(Colors.White);
                _addButton.LabelForeground = new SolidColorBrush(Colors.White);
                AppBarPanel.MoreButton.LabelForeground = new SolidColorBrush(Colors.White);
                AppBarMenuItemsPlaceholder.Foreground = new SolidColorBrush(Colors.White);
            }

            OptimizeFullHD();

            _setPhotoButton.Tap += SetPhoto_OnTap;
            _editButton.Tap += EditButton_OnTap;
            _addButton.Tap += AddButton_OnTap;

            _deleteAndExitMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    //ViewModel.DeleteAndExitGroup();
                });
            };
            _setAdminsMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    //ViewModel.SetAdmins();
                });
            };
            _convertToSupergroupMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    //ViewModel.ConvertToSupergroup();
                });
            };

            Loaded += (sender, args) =>
            {
                //if (ViewModel.ProfilePhotoViewer != null)
                //    ViewModel.ProfilePhotoViewer.PropertyChanged += OnProfileViewerPropertyChanged;

                //ViewModel.PropertyChanged += OnChatDetailsPropertyChanged;
                BuildLocalizedAppBar();
            };

            Unloaded += (sender, args) =>
            {
                //ViewModel.PropertyChanged -= OnChatDetailsPropertyChanged;

                //if (ViewModel.ProfilePhotoViewer != null)
                //    ViewModel.ProfilePhotoViewer.PropertyChanged -= OnProfileViewerPropertyChanged;
            };
        }

        private void OptimizeFullHD()
        {
#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            //if (!isFullHD) return;
#endif
        }

        private void OnChatDetailsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (Property.NameEquals(e.PropertyName, () => ViewModel.ProfilePhotoViewer))
            //{
            //    ViewModel.ProfilePhotoViewer.PropertyChanged += OnProfileViewerPropertyChanged;
            //}
            //else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsChannelAdministrator))
            //{
            //    UpdateLocalizedAppBar();
            //}
            //else if (Property.NameEquals(e.PropertyName, () => ViewModel.CanEditChat))
            //{
            //    UpdateLocalizedAppBar();
            //}
        }

        private IApplicationBar _prevAppBar;

        private void OnProfileViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (Property.NameEquals(e.PropertyName, () => ViewModel.ProfilePhotoViewer.IsOpen))
            //{
            //    LayoutRoot2.IsHitTestVisible = !ViewModel.ProfilePhotoViewer.IsOpen;
            //    AppBarPanel.Visibility = !ViewModel.ProfilePhotoViewer.IsOpen
            //        ? Visibility.Visible
            //        : Visibility.Collapsed;

            //    ViewModel.NotifyOfPropertyChange(() => ViewModel.IsViewerOpen);

            //    if (ViewModel.ProfilePhotoViewer.IsOpen)
            //    {
            //        _prevAppBar = ApplicationBar;

            //        var profilePhotoViewerView = ProfilePhotoViewer.Content as ProfilePhotoViewerView;
            //        ApplicationBar = profilePhotoViewerView != null ? profilePhotoViewerView.ApplicationBar : null;
            //    }
            //    else
            //    {
            //        // wait to finish closing profile viewer animation
            //        Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            //        {
            //            if (_prevAppBar != null)
            //            {
            //                ApplicationBar = _prevAppBar;
            //            }
            //        });
            //    }
            //}
        }

        private ApplicationBar _applicationBar;

        private void BuildLocalizedAppBar()
        {
            if (_applicationBar != null) return;

            _applicationBar = new ApplicationBar { BackgroundColor = ((SolidColorBrush)ShellView.CaptionBrush).Color, ForegroundColor = Colors.White, Opacity = 0.99999 };
            var gridColumn = 3;
            //if (ViewModel.CanAddChannelParticipants || ViewModel.CanAddChatParticipants)
            //{
            //    Grid.SetColumn(_addButton, gridColumn--);
            //    AppBarPanel.Buttons.Add(_addButton);
            //    //_applicationBar.Buttons.Add(_addButton);
            //}

            //if (ViewModel.CanEditChannel || ViewModel.CanEditChat)
            //{
            //    Grid.SetColumn(_editButton, gridColumn--);
            //    AppBarPanel.Buttons.Add(_editButton);

            //    Grid.SetColumn(_setPhotoButton, gridColumn--);
            //    AppBarPanel.Buttons.Add(_setPhotoButton);
            //    //_applicationBar.Buttons.Add(_editButton);
            //}

            //if (ViewModel.IsReportButtonEnabled)
            //{
            //    AppBarMenuItems.Children.Add(_reportMenuItem);
            //}

            //if (ViewModel.IsDeleteAndExitVisible)
            //{
            //    _deleteAndExitMenuItem.Content = ViewModel.DeleteAndExitGroupString;
            //    AppBarMenuItems.Children.Add(_deleteAndExitMenuItem);
            //}

            //var chat = ViewModel.Chat as TLChat40;
            //if (chat != null && chat.Creator)
            //{
            //    AppBarMenuItems.Children.Add(_setAdminsMenuItem);
            //    AppBarMenuItems.Children.Add(_convertToSupergroupMenuItem);
            //}

            UpdateLocalizedAppBar();
        }

        private void UpdateLocalizedAppBar()
        {
            if (_applicationBar == null) return;

            //var channel = ViewModel.Chat as TLChannel;
            //if (channel != null)
            //{
            //    //AppBarPanel.Visibility = (ViewModel.ChatDetails.CanEditChannel || ViewModel.ChatDetails.CanAddChannelParticipants)
            //    //    ? Visibility.Visible
            //    //    : Visibility.Collapsed;
            //    //_applicationBar.IsVisible = ViewModel.ChatDetails.CanEditChannel || ViewModel.ChatDetails.CanAddChannelParticipants;
            //    return;
            //}

            //var chat = ViewModel.Chat as TLChat40;
            //if (chat != null)
            //{
            //    //AppBarPanel.Visibility = ViewModel.ChatDetails.CanEditChat
            //    //    ? Visibility.Visible
            //    //    : Visibility.Collapsed;
            //    //_applicationBar.IsVisible = ViewModel.ChatDetails.CanEditChat;
            //    return;
            //}
        }

        private void ChatView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups().ToList();
            var popup = popups.FirstOrDefault();
            if (popup != null)
            {
                e.Cancel = true;

                var multiplePhotoChooser = popup.Child as OpenPhotoPicker;
                if (multiplePhotoChooser != null)
                {
                    multiplePhotoChooser.TryClose();
                }

                var cropControl = popup.Child as CropControl;
                if (cropControl != null)
                {
                    cropControl.TryClose();
                }

                return;
            }

            //if (ViewModel.ProfilePhotoViewer != null
            //    && ViewModel.ProfilePhotoViewer.IsOpen)
            //{
            //    ViewModel.ProfilePhotoViewer.CloseViewer();
            //    e.Cancel = true;
            //    return;
            //}
        }

        private bool _once;

        private void TelegramNavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                _once = true;
                ViewModel.ForwardInAnimationComplete();
            }
            //else
            //{
            //    ViewModel.UpdateTitles();
            //}
        }

        private void AppBarPanel_OnPanelOpened(object sender, System.EventArgs e)
        {
            MorePanelBackground.Visibility = Visibility.Visible;
        }

        private void AppBarPanel_OnPanelClosed(object sender, System.EventArgs e)
        {
            MorePanelBackground.Visibility = Visibility.Collapsed;
        }

        private void MorePanelBackground_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
        }

        private void SetPhoto_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                //ViewModel.SetPhoto();
            });
        }

        private void EditButton_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                //ViewModel.Edit();
            });
        }

        private void AddButton_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                //ViewModel.AddParticipant();
            });
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {

            //ContextMenuService.GetContextMenu((DependencyObject)sender).IsOpen = true;
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var span = element.DataContext as TimerSpan;
            if (span == null) return;

            //ViewModel.SelectSpan(span);
        }

        private void CopyLink_OnClick(object sender, RoutedEventArgs e)
        {
            //ViewModel.CopyLink();
        }

        private void ToggleSwitch_OnChecked(object sender, RoutedEventArgs e)
        {
            //ViewModel.SelectSpan(ViewModel.Spans.First());
        }

        private void ToggleSwitch_OnUnchecked(object sender, RoutedEventArgs e)
        {
            //ViewModel.SelectSpan(ViewModel.Spans.Last());
        }

        private void DeleteMenuItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var user = element.DataContext as TLUserBase;
            if (user == null) return;

            element.Visibility = Visibility.Visible;

            //var chat = ViewModel.CurrentItem as TLChat41;
            //if (chat != null && chat.AdminsEnabled.Value && !chat.Admin.Value && !chat.Creator)
            //{
            //    element.Visibility = Visibility.Collapsed;
            //    return;
            //}

            //var channel = ViewModel.CurrentItem as TLChannel;
            //if (channel != null)
            //{
            //    if (!channel.IsEditor && !channel.Creator)
            //    {
            //        element.Visibility = Visibility.Collapsed;
            //        return;
            //    }

            //    if (channel.ChannelParticipants != null)
            //    {
            //        var participants = channel.ChannelParticipants.Participants;
            //        var creator = participants.FirstOrDefault(x => x is TLChannelParticipantCreator);
            //        if (creator != null && creator.UserId.Value == user.Index)
            //        {
            //            element.Visibility = Visibility.Collapsed;
            //            return;
            //        }

            //    }

            //    return;
            //}


        }
    }
}