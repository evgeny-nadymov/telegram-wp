// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TelegramClient.Controls;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Media;
using Execute = Telegram.Api.Helpers.Execute;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class SettingsView
    {
        public SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }

        private readonly AppBarButton _editButton = new AppBarButton
        {
            Text = AppResources.Edit,
            IconUri = new Uri("/Images/ApplicationBar/appbar.edit.png", UriKind.Relative)
        };

        private readonly AppBarMenuItem _askQuestionMenuItem = new AppBarMenuItem
        {
            Text = AppResources.AskAQuestion
        };

        private readonly AppBarMenuItem _telegramFAQMenuItem = new AppBarMenuItem
        {
            Text = AppResources.TelegramFAQ
        };

        private readonly AppBarMenuItem _privacyPolicyMenuItem = new AppBarMenuItem
        {
            Text = AppResources.PrivacyPolicy
        };

        private readonly AppBarMenuItem _logOutMenuItem = new AppBarMenuItem
        {
            Text = AppResources.LogOut
        };

        private readonly AppBarMenuItem _sendLogsMenuItem = new AppBarMenuItem
        {
            Text = AppResources.SendLogs
        };

        public SettingsView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            _editButton.Click += (sender, args) => ViewModel.EditProfile();
            _askQuestionMenuItem.Click += (sender, args) => ViewModel.Support();
            _telegramFAQMenuItem.Click += (sender, args) => ViewModel.OpenFAQ();
            _privacyPolicyMenuItem.Click += (sender, args) => ViewModel.OpenPrivacyPolicy();
            _logOutMenuItem.Click += (sender, args) => ViewModel.LogOut();
            _sendLogsMenuItem.Click += (sender, args) => ViewModel.SendLogs();

            Loaded += (sender, args) =>
            {
                RunAnimation();
                BuildLocalizedAppBar();

                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ProfilePhotoViewer))
            {
                ViewModel.ProfilePhotoViewer.PropertyChanged += OnProfilePhotoViewerPropertyChanged;
            }
        }

        private IApplicationBar _prevAppBar;

        private void OnProfilePhotoViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ProfilePhotoViewer.IsOpen))
            {
                ScrollViewer.IsHitTestVisible = !ViewModel.ProfilePhotoViewer.IsOpen;
                AppBarPanel.Visibility = !ViewModel.ProfilePhotoViewer.IsOpen
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (ViewModel.ProfilePhotoViewer.IsOpen)
                {
                    _prevAppBar = ApplicationBar;

                    var profilePhotoViewerView = ProfilePhotoViewer.Content as ProfilePhotoViewerView;
                    ApplicationBar = profilePhotoViewerView != null ? profilePhotoViewerView.ApplicationBar : null;
                }
                else
                {
                    // wait to finish closing profile viewer animation
                    Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25),
                        () =>
                        {
                            ApplicationBar = _prevAppBar;
                        });
                }
            }
        }

        private bool _isForwardInAnimation;

        private void RunAnimation()
        {
            if (_isForwardInAnimation)
            {
                _isForwardInAnimation = false;
                var forwardInAnimation = TelegramTurnstileAnimations.GetAnimation(LayoutRoot, TurnstileTransitionMode.ForwardIn);
                forwardInAnimation.Completed += (sender, args) =>
                {
                    ViewModel.ForwardInAnimationComplete();
                };
                Execute.BeginOnUIThread(forwardInAnimation.Begin);
            }
            else
            {
                LayoutRoot.Opacity = 1.0;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                LayoutRoot.Opacity = 0.0;
                _isForwardInAnimation = true;
            }

            base.OnNavigatedTo(e);
        }

        private void BuildLocalizedAppBar()
        {
            return;
            if (ApplicationBar == null)
            {
                ApplicationBar = new ApplicationBar{Opacity = 0.99};
                ApplicationBar.Buttons.Add(_editButton);

                ApplicationBar.MenuItems.Add(_askQuestionMenuItem);
                ApplicationBar.MenuItems.Add(_telegramFAQMenuItem);
                ApplicationBar.MenuItems.Add(_privacyPolicyMenuItem);
                ApplicationBar.MenuItems.Add(_sendLogsMenuItem);
                ApplicationBar.MenuItems.Add(_logOutMenuItem);
            }
        }

        private void SetPhotoButton_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.EditProfilePhoto());
        }

        private void EditNameButton_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.EditProfile());
        }

        private void SupportMenuItem_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.Support());
        }

        private void OpenFAQMenuItem_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.OpenFAQ());
        }

        private void PrivacyPolicyMenuItem_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.OpenPrivacyPolicy());
        }

        private void LogOutMenuItem_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () => ViewModel.LogOut());
        }

        private void SendLogsMenuItem_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.SendLogs());
        }

        private void SendCallLogsMenuItem_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.SendCallLogs());
        }

        private void ClearLogsMenuItem_OnTap(object sender, GestureEventArgs e)
        {
            AppBarPanel.Close();
            Execute.BeginOnUIThread(() => ViewModel.ClearLogs());
        }

        private void SettingsView_OnBackKeyPress(object sender, CancelEventArgs e)
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

            if (MorePanel.Visibility == Visibility.Visible)
            {
                AppBarPanel.Close();
                e.Cancel = true;
            }

            if (ViewModel.ProfilePhotoViewer != null
                && ViewModel.ProfilePhotoViewer.IsOpen)
            {
                ViewModel.ProfilePhotoViewer.CloseViewer();
                e.Cancel = true;
                return;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (MorePanel.Visibility == Visibility.Visible)
            {
                AppBarPanel.Close();
            }

            base.OnNavigatingFrom(e);
        }
    }
}