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
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Media;

namespace TelegramClient.Views.Contacts
{
    public partial class ContactView
    {
        public ContactViewModel ViewModel { get { return DataContext as ContactViewModel; } }

        private readonly AppBarButton _editButton = new AppBarButton
        {
            Text = AppResources.Edit,
            IconUri = new Uri("/Images/ApplicationBar/appbar.edit.png", UriKind.Relative)
        };

        private readonly AppBarButton _shareButton = new AppBarButton
        {
            Text = AppResources.Share,
            IconUri = new Uri("/Images/ApplicationBar/appbar.share.png", UriKind.Relative)
        };

        private readonly ListBoxItem _reportMenuItem = new ListBoxItem
        {
            Content = AppResources.Report,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _addToGroupMenuItem = new ListBoxItem
        {
            Content = AppResources.AddToGroup,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _blockMenuItem = new ListBoxItem
        {
            Content = AppResources.BlockContact,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _unblockMenuItem = new ListBoxItem
        {
            Content = AppResources.UnblockContact,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _addMenuItem = new ListBoxItem
        {
            Content = AppResources.AddContact,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private readonly ListBoxItem _deleteMenuItem = new ListBoxItem
        {
            Content = AppResources.DeleteContact,
            Padding = new Thickness(18.0),
            FontSize = 23.0
        };

        private IApplicationBar _prevAppBar;

        public ContactView()
        {
            var timer = Stopwatch.StartNew();

            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                //NotificationsSwitch.Style = (Style)Application.Current.Resources["ProfileLightToggleSwitch"];
                LayoutRoot.Background = ShellView.CaptionBrush;
                AppBarPanel.MorePanelBackgroundBrush = ShellView.CaptionBrush;
                AppBarPanel.BackgroundBrush = ShellView.CaptionBrush;
                foreach (var button in AppBarPanel.Buttons)
                {
                    var appBarButton = button as TelegramAppBarButton;
                    if (appBarButton != null)
                    {
                        appBarButton.LabelForeground = new SolidColorBrush(Colors.White);
                    }
                }
                AppBarPanel.MoreButton.LabelForeground = new SolidColorBrush(Colors.White);
                AppBarMenuItemsPlaceholder.Foreground = new SolidColorBrush(Colors.White);
            }

            OptimizeFullHD();

            _editButton.Click += (sender, args) => ViewModel.Edit();
            _shareButton.Click += (sender, args) => ViewModel.Share();

            _reportMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    ViewModel.Report();
                });
            };
            _addToGroupMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    ViewModel.AddToGroup();
                });
            };
            _blockMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    ViewModel.BlockContact();
                });
            };
            _unblockMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    ViewModel.UnblockContact();
                });
            };
            _addMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    ViewModel.AddContact();
                });
            };
            _deleteMenuItem.Tap += (sender, args) =>
            {
                AppBarPanel.Close();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    ViewModel.DeleteContact();
                });
            };

            Loaded += (sender, args) =>
            {
                _blockMenuItem.Content = ViewModel.IsBot ? AppResources.StopBot : AppResources.BlockContact;
                _unblockMenuItem.Content = ViewModel.IsBot ? AppResources.RestartBot : AppResources.UnblockContact;

                if (ViewModel.ProfilePhotoViewer != null)
                    ViewModel.ProfilePhotoViewer.PropertyChanged += OnProfileViewerPropertyChanged;

                ViewModel.BlockedStatusChanged += OnBlockedStatusChanged;
                ViewModel.ImportStatusChanged += OnImportStatusChanged;
                ViewModel.PropertyChanged += OnContactDetailsPropertyChanges;

                BuildLocalizedAppBar();
            };

            Unloaded += (sender, args) =>
            {
                if (ViewModel.ProfilePhotoViewer != null)
                    ViewModel.ProfilePhotoViewer.PropertyChanged -= OnProfileViewerPropertyChanged;

                ViewModel.BlockedStatusChanged -= OnBlockedStatusChanged;
                ViewModel.ImportStatusChanged -= OnImportStatusChanged;
                ViewModel.PropertyChanged -= OnContactDetailsPropertyChanges;
            };
        }

        private void OptimizeFullHD()
        {
#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            //if (!isFullHD) return;
#endif

            //Items.HeaderTemplate = (DataTemplate)Application.Current.Resources["FullHDPivotHeaderTemplate"];
        }

        private void OnContactDetailsPropertyChanges(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ProfilePhotoViewer))
            {
                ViewModel.ProfilePhotoViewer.PropertyChanged += OnProfileViewerPropertyChanged;
            }
        }

        private void OnImportStatusChanged(object sender, ImportEventArgs e)
        {
            Execute.OnUIThread(() =>
            {
                //if (ApplicationBar == null) return;
                if (ViewModel.CurrentContact.IsSelf) return;

                ViewModel.CurrentContact.NotifyOfPropertyChange(() => ViewModel.CurrentContact.FullName);

                if (e.Imported)
                {
                    AppBarMenuItems.Children.Remove(_addMenuItem);
                    AppBarMenuItems.Children.Insert(0, _deleteMenuItem);
                }
                else
                {
                    AppBarMenuItems.Children.Remove(_deleteMenuItem);
                    if (ViewModel.CurrentContact.HasPhone) AppBarMenuItems.Children.Insert(0, _addMenuItem);
                }
            });
        }

        private void OnBlockedStatusChanged(object sender, BlockedEventArgs e)
        {
            Execute.OnUIThread(() =>
            {
                //if (ApplicationBar == null) return;
                if (ViewModel.CurrentContact.IsSelf) return;

                if (e.Blocked)
                {
                    AppBarMenuItems.Children.Remove(_blockMenuItem);
                    AppBarMenuItems.Children.Add(_unblockMenuItem);
                }
                else
                {
                    AppBarMenuItems.Children.Remove(_unblockMenuItem);
                    AppBarMenuItems.Children.Add(_blockMenuItem);
                }
            });
        }

        private void OnProfileViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ProfilePhotoViewer.IsOpen))
            {
                ContentPanel.IsHitTestVisible = !ViewModel.ProfilePhotoViewer.IsOpen;
                AppBarPanel.Visibility = !ViewModel.ProfilePhotoViewer.IsOpen
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                ViewModel.NotifyOfPropertyChange(() => ViewModel.IsViewerOpen);

                if (ViewModel.ProfilePhotoViewer.IsOpen)
                {
                    _prevAppBar = ApplicationBar;

                    var profilePhotoViewerView = ProfilePhotoViewer.Content as ProfilePhotoViewerView;
                    ApplicationBar = profilePhotoViewerView != null? profilePhotoViewerView.ApplicationBar : null;
                }
                else
                {
                    // wait to finish closing profile viewer animation
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25),
                        () =>
                        {
                            if (_prevAppBar != null)
                            {
                                ApplicationBar = _prevAppBar;
                            }
                        });
                }
            }
        }

        private ApplicationBar _applicationBar;

        private void BuildLocalizedAppBar()
        {
            if (_applicationBar != null) return;

            var contact = ViewModel.CurrentContact;
            if (contact != null && contact.IsDeleted)
            {
                AppBarPanel.Visibility = Visibility.Collapsed;
                return;
            }

            _applicationBar = new ApplicationBar { BackgroundColor = ((SolidColorBrush)ShellView.CaptionBrush).Color, ForegroundColor = Colors.White, Opacity = 0.99999 };
            _applicationBar.Buttons.Add(_shareButton);
            _applicationBar.Buttons.Add(_editButton);

            var bot = ViewModel.CurrentContact as TLUser;
            if (bot != null && bot.IsBot)
            {
                if (!bot.IsBotGroupsBlocked)
                {
                    AppBarMenuItems.Children.Add(_addToGroupMenuItem);
                }

                AppBarMenuItems.Children.Add(_reportMenuItem);
            }

            if (ViewModel.CurrentContact != null && ViewModel.CurrentContact.IsContact)
            {
                if (!ViewModel.CurrentContact.IsSelf)
                {
                    AppBarMenuItems.Children.Add(_deleteMenuItem);
                }
            }
            else if (ViewModel.HasPhone)
            {
                AppBarMenuItems.Children.Add(_addMenuItem);
            }
        }

        private void ContactView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (ViewModel.ProfilePhotoViewer != null 
                && ViewModel.ProfilePhotoViewer.IsOpen)
            {
                ViewModel.ProfilePhotoViewer.CloseViewer();
                e.Cancel = true;
                return;
            }
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

        private void EditButton_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                ViewModel.Edit();
            });
        }

        private void ShareButton_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                ViewModel.Share();
            });
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var span = element.DataContext as TimerSpan;
            if (span == null) return;

            ViewModel.SelectSpan(span);
        }

        private void ToggleSwitch_OnChecked(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectSpan(ViewModel.Spans.First());
        }

        private void ToggleSwitch_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectSpan(ViewModel.Spans.Last());
        }

        private void CopyLink_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyLink();
        }

        private void CopyPhone_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyPhone();
        }

        private void CopyBotInfo_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyBotInfo();
        }

        private void CopyBio_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyBio();
        }
    }
}