// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class LockscreenView
    {
        public LockscreenViewModel ViewModel
        {
            get { return DataContext as LockscreenViewModel; }
        }

        public LockscreenView()
        {
            InitializeComponent();

            OptimizeFullHD();

            //Windows.UI.ViewManagement.InputPane.GetForCurrentView().Showing += MainPage_Showing;
            //Windows.UI.ViewManagement.InputPane.GetForCurrentView().Hiding += MainPage_Hiding;
        }

        public void OnPasscodeIncorrect(object sender, System.EventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                IncorrectPasscodeHint.Visibility = Visibility.Visible;

                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () =>
                {
                    IncorrectPasscodeHint.Visibility = Visibility.Collapsed;
                });
            });
        }

        private double _gotFocusTransformY = 0.0;

        private const double TransformYDefault = -122.0;
        private const double TransformY112 = -118;
        private const double TransformY150 = -96;
        private const double TransformY150Software = -118;
        private const double TransformY6Inch = -20.0;

        private void OptimizeFullHD()
        {
            var gotFocusTransformY = TransformYDefault;
#if WP8
            switch (Application.Current.Host.Content.ScaleFactor)
            {
                case 100:   //Lumia 820
                    gotFocusTransformY = TransformYDefault;
                    break;
                case 112:   //Lumia 535
                    gotFocusTransformY = TransformY112;
                    break;
                case 150:   //HTC 8X

                    // Lumia 730 softwarebuttons
                    var appBar = new ApplicationBar();
                    if (appBar.DefaultSize == 67.0)
                    {
                        gotFocusTransformY = TransformY150Software;
                        break;
                    }

                    gotFocusTransformY = TransformY150;
                    break;
                case 160:   //Lumia 925, 1020 (WXGA)
                    gotFocusTransformY = TransformYDefault;
                    break;
                case 225:   // Lumia 1520, Lumia 930
                    var deviceName = DeviceStatus.DeviceName;
                    if (!string.IsNullOrEmpty(deviceName))
                    {
                        deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();

                        //Lumia 1320, 1520
                        if (//deviceName.StartsWith("rm934")      // Lumia 1320
                            //|| deviceName.StartsWith("rm935")
                            //|| deviceName.StartsWith("rm936")
                            deviceName.StartsWith("rm937")   // Lumia 1520
                            || deviceName.StartsWith("rm938")
                            || deviceName.StartsWith("rm939")
                            || deviceName.StartsWith("rm940"))
                        {
                            var stateService = IoC.Get<IStateService>();

                            if (LockscreenViewModel.IsSimple(stateService))
                            {
                                gotFocusTransformY = -88.0;
                                break;
                            }

                            gotFocusTransformY = TransformY6Inch;
                            break;
                        }
                    }

                    // other FullHD
                    gotFocusTransformY = TransformYDefault;
                    break;
            }
#endif

            _gotFocusTransformY = gotFocusTransformY;
        }

        public PhoneApplicationPage ParentPage { get; set; }

        private void Passcode_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && ViewModel.Simple)
            {
                return;
            }

            if (ParentPage != null)
            {
                if (ParentPage.ApplicationBar != null)
                {
                    ParentPage.ApplicationBar.BackgroundColor = (Color) Application.Current.Resources["PhoneChromeColor"];
                }

                Clipboard.SetText(string.Empty);

                PasscodeBorder.Opacity = 1.0;
                var storyboard = new Storyboard();
                var translateYAnimation = new DoubleAnimation
                {
                    To = _gotFocusTransformY,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(storyboard, PasscodeTransform);
                Storyboard.SetTargetProperty(storyboard, new PropertyPath("Y"));
                storyboard.Children.Add(translateYAnimation);

                Telegram.Api.Helpers.Execute.BeginOnUIThread(storyboard.Begin);
            }
        }

        private void Passcode_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && ViewModel.Simple)
            {
                return;
            }

            if (ParentPage != null && Visibility == Visibility.Visible)
            {
                var focusedElement = FocusManager.GetFocusedElement();
                if (focusedElement == PasscodeNumberBorder || focusedElement == PasscodeNumeric)
                {
                    return;
                }

                if (ParentPage.ApplicationBar != null)
                {
                    var color = ParentPage.ApplicationBar.BackgroundColor;
                    color.A = 254;
                    ParentPage.ApplicationBar.BackgroundColor = color;
                }
                PasscodeBorder.Opacity = 1.0;
                var storyboard = new Storyboard();
                var translateYAnimation = new DoubleAnimation
                {
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(storyboard, PasscodeTransform);
                Storyboard.SetTargetProperty(storyboard, new PropertyPath("Y"));
                storyboard.Children.Add(translateYAnimation);

                storyboard.Begin();
                storyboard.Completed += OnLostFocusStoryboardCompleted;
            }
        }

        private void OnLostFocusStoryboardCompleted(object sender, System.EventArgs e)
        {
            //return;

            if (ParentPage.ApplicationBar != null
                && ParentPage.ApplicationBar.BackgroundColor.A == 254)
            {
                ParentPage.ApplicationBar.BackgroundColor = Colors.Transparent;
            }
        }

        public void FocusPasscode()
        {
            PasscodeTransform.Y = 0.0;
            OptimizeFullHD();
            if (ViewModel.Simple)
            {
                PasscodeTransform.Y = _gotFocusTransformY;
                PinKeyboard.Visibility = Visibility.Visible;
                Passcode.Visibility = Visibility.Collapsed;
                PasscodeNumericPanel.Visibility = Visibility.Visible;
                //Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.5), () => PasscodeNumeric.Focus());
            }
            else
            {
                PinKeyboard.Visibility = Visibility.Collapsed;
                Passcode.Visibility = Visibility.Visible;
                PasscodeNumericPanel.Visibility = Visibility.Collapsed;
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () => Passcode.Focus());
                //Passcode.Focus();
            }
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            if (ViewModel != null && ViewModel.Simple)
            {
                return;
            }

            PasscodeNumeric.Focus();
        }

        private void PasscodeNumeric_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key == Key.Back)
            {
                if (IncorrectPasscodeHint.Visibility == Visibility.Visible)
                {
                    IncorrectPasscodeHint.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Passcode_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(ViewModel.Passcode))
                {
                    ViewModel.Done();
                }
            }

            if (IncorrectPasscodeHint.Visibility == Visibility.Visible)
            {
                IncorrectPasscodeHint.Visibility = Visibility.Collapsed;
            }
        }

        private void PasscodeNumeric_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IncorrectPasscodeHint.Visibility == Visibility.Visible)
            {
                IncorrectPasscodeHint.Visibility = Visibility.Collapsed;
            }
        }
    }
}