// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Media;

namespace TelegramClient.Views.Media
{
    public partial class ImageEditorView
    {
        private readonly AppBarButton _doneButton = new AppBarButton
        {
            Text = AppResources.Done,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };
      
        public ImageEditorViewModel ViewModel
        {
            get { return DataContext as ImageEditorViewModel; }
        }

        public ImageEditorView()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;

            BuildLocalizedAppBar();

            OptimizeFullHD();

            _runOnce = true;

            Loaded += (o, e) =>
            {
                if (_runOnce && ViewModel.CurrentItem != null)
                {
                    _runOnce = false;
                    BeginOpenStoryboard();
                }

                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
            Unloaded += (o, e) => ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        private bool _runOnce;

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                if (ViewModel.IsOpen)
                {
                    _runOnce = false;
                    BeginOpenStoryboard();
                }
                else
                {
                    BeginCloseStoryboard();
                }
            }
        }

        private void OptimizeFullHD()
        {
#if WP8
            var appBar = ApplicationBar;
            if (appBar == null)
            {
                appBar = new ApplicationBar();
            }

            var appBarDefaultSize = appBar.DefaultSize;
            var appBarDifference = appBarDefaultSize - 72.0;

            ApplicationBarPlaceholder.Height = appBarDefaultSize;
#endif
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null)
            {
                return;
            }

            ApplicationBar = new ApplicationBar { Opacity = 0.9999 };
            //ApplicationBar.BackgroundColor = Colors.Black;
            //ApplicationBar.ForegroundColor = Colors.White;
            ApplicationBar.StateChanged += (o, e) =>
            {
                ApplicationBar.Opacity = e.IsMenuVisible ? 0.9999 : 0.0;
            };

            _doneButton.Click += (sender, args) => ViewModel.Done();
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();

            ApplicationBar.Buttons.Add(_doneButton);
            ApplicationBar.Buttons.Add(_cancelButton);
        }


        private void BeginCloseStoryboard()
        {
            SystemTray.IsVisible = true;
            ApplicationBar.IsVisible = false;

            var direction = CloseDirection.Down;
            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = ImagesGrid.ActualHeight / 2 + rootFrameHeight / 2;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = direction == CloseDirection.Down ? translateYTo : -translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
            opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = 0 });
            Storyboard.SetTarget(opacityImageAniamtion, BackgroundBorder);
            Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityImageAniamtion);

            storyboard.Begin();
            storyboard.Completed += (o, args) =>
            {
                Visibility = Visibility.Collapsed;
            };
        }

        private void BeginOpenStoryboard()
        {
            SystemTray.IsVisible = false;
            ApplicationBar.IsVisible = true;

            var transparentBlack = Colors.Black;
            transparentBlack.A = 0;

            CaptionWatermark.Visibility = Visibility.Visible;
            Visibility = Visibility.Visible;
            ImagesGrid.Opacity = 1.0;
            ImagesGrid.RenderTransform = new CompositeTransform();
            BackgroundBorder.Opacity = 1.0;


            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            ((CompositeTransform)ImagesGrid.RenderTransform).TranslateY = translateYTo;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            storyboard.Completed += (sender, args) =>
            {
                //Deployment.Current.Dispatcher.BeginInvoke(() => OpenApplicationPanelAnimation.Begin());
            };
            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private void Caption_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var height = GetKeyboardHeightDifference();
            CaptionWatermark.Visibility = Visibility.Collapsed;
            KeyboardPlaceholder.Height = EmojiControl.PortraitOrientationHeight - ApplicationBarPlaceholder.ActualHeight;
            ImagesGrid.Margin = new Thickness(0.0, 0.0, 0.0, -height);

            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };
            var storyboard = new Storyboard();
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = -height / 2.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private double GetKeyboardHeightDifference()
        {
            var heightDifference = EmojiControl.PortraitOrientationHeight - ApplicationBarPlaceholder.ActualHeight;

            return heightDifference;
        }

        private void Caption_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var height = GetKeyboardHeightDifference();
            CaptionWatermark.Visibility = string.IsNullOrEmpty(Caption.Text) ? Visibility.Visible : Visibility.Collapsed;
            KeyboardPlaceholder.Height = 0.0;
            ImagesGrid.Margin = new Thickness(0.0);

            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };
            var storyboard = new Storyboard();
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -height / 2.0 });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.20), Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private void Caption_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CaptionWatermark.Visibility = string.IsNullOrEmpty(Caption.Text) && FocusManager.GetFocusedElement() != Caption? Visibility.Visible : Visibility.Collapsed;
        }

        private void Caption_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Done();
            }
        }
    }
}
