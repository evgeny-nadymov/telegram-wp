// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.Views.Contacts
{
    public partial class ShareContactDetailsView
    {
        public ShareContactDetailsViewModel ViewModel
        {
            get { return DataContext as ShareContactDetailsViewModel; }
        }

        public ShareContactDetailsView()
        {
            InitializeComponent();

            LayoutRoot.Opacity = 0.0;

            Loaded += SearchShellView_Loaded;
        }

        public Action<Visibility> ClosePivotAction;

        public void SearchShellView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SearchShellView_Loaded;

            BeginOpenStoryboard(true);
        }

        private Storyboard _openStoryboard;

        public void BeginOpenStoryboard(bool initialize = false)
        {
            LayoutRoot.Opacity = 0.0;

            var translateYTo = 150.0;
            var duration = TimeSpan.FromSeconds(0.4);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var translateAnimation = new DoubleAnimationUsingKeyFrames();
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = duration,
                Value = 0.0,
                EasingFunction = easingFunction
            });
            Storyboard.SetTarget(translateAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = duration,
                Value = 1.0,
                EasingFunction = easingFunction
            });
            Storyboard.SetTarget(opacityAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            storyboard.Completed += (o, args) =>
            {
                ViewModel.ForwardInAnimationComplete();
                ClosePivotAction.SafeInvoke(Visibility.Collapsed);
            };
            _openStoryboard = storyboard;

            ViewModel.ForwardInAnimationBegin();
            Execute.BeginOnUIThread(storyboard.Begin);
        }

        public void BeginCloseStoryboard(Action callback = null)
        {
            if (_openStoryboard.GetCurrentState() == ClockState.Active)
            {
                _openStoryboard.Pause();
            }

            ClosePivotAction.SafeInvoke(Visibility.Visible);
            //LayoutRoot.Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];

            var translateYTo = 150.0;
            var duration = TimeSpan.FromSeconds(0.3);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var translateAnimation = new DoubleAnimationUsingKeyFrames();
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = duration,
                Value = translateYTo,
                EasingFunction = easingFunction
            });
            Storyboard.SetTarget(translateAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = duration,
                Value = 0.0,
                EasingFunction = easingFunction
            });
            Storyboard.SetTarget(opacityAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            if (callback != null)
            {
                storyboard.Completed += (o, e) => callback();
            }

            Execute.BeginOnUIThread(() =>
            {
                storyboard.Begin();
            });
        }

        private void PhoneButton_OnTap(object sender, GestureEventArgs e)
        {
            if (ViewModel.Mode == ShareContactDetailsMode.Share)
            {
                var element = sender as FrameworkElement;
                if (element != null)
                {
                    var userPhone = element.DataContext as TLUserPhone;
                    if (userPhone != null)
                    {
                        userPhone.IsSelected = !userPhone.IsSelected;

                        foreach (var item in ViewModel.Items)
                        {
                            var currentUserPhone = item as TLUserPhone;
                            if (currentUserPhone != null && currentUserPhone != userPhone)
                            {
                                currentUserPhone.IsSelected = false;
                            }
                        }
                    }
                }
                Send.IsEnabled = ViewModel.IsSharingEnabled;
            }
            else
            {
                var element = sender as FrameworkElement;
                if (element != null)
                {
                    var userPhone = element.DataContext as TLUserPhone;
                    if (userPhone != null)
                    {
                        ViewModel.Call(userPhone);
                    }
                }
            }
        }
    }

    public class UserPhoneKindToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kind = value as TLInt;
            if (kind != null)
            {
                switch (kind.Value)
                {
                    case 0:
                        return AppResources.PhoneKindHome;
                    case 1:
                        return AppResources.PhoneKindMobile;
                    case 2:
                        return AppResources.PhoneKindWork;
                    case 3:
                        return AppResources.PhoneKindOther;
                }
            }

            return AppResources.PhoneKindMobile;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
