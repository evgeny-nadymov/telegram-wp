// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.UI.ViewManagement;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Media
{
    public partial class EditVideoView : IDisposable
    {
        private IDisposable _trimEventsSubscription;

        public EditVideoViewModel ViewModel
        {
            get { return DataContext as EditVideoViewModel; }
        }

        public EditVideoView()
        {
            InitializeComponent();

            Caption2.Background = ShellView.CaptionBrush;

            Visibility = Visibility.Collapsed;

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                TopBorder.Fill = (Brush)Resources["InputBorderBrushLight"];
            }
            else
            {
                TopBorder.Fill = (Brush)Resources["InputBorderBrushDark"];
            }

            var trimEvents = Observable.FromEventPattern<EventHandler, System.EventArgs>(
                keh => { Timeline.TrimChanged += keh; },
                keh => { Timeline.TrimChanged -= keh; });

            _trimEventsSubscription = trimEvents
                .Sample(TimeSpan.FromSeconds(0.25))
                .ObserveOnDispatcher()
                .Subscribe(args => ViewModel.UpdateEditedVideoDuration());

            Loaded += OnLoadedOnce;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            InputPane.GetForCurrentView().Showing += InputPane_Showing;
            InputPane.GetForCurrentView().Hiding += InputPane_Hiding;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            InputPane.GetForCurrentView().Showing -= InputPane_Showing;
            InputPane.GetForCurrentView().Hiding -= InputPane_Hiding;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            CaptionWatermark.Visibility = string.IsNullOrEmpty(Caption.Text) ? Visibility.Visible : Visibility.Collapsed;
            KeyboardPlaceholder.Height = 0.0;
            KeyboardPlaceholder.Visibility = Visibility.Collapsed;
            ImagesGrid.Margin = new Thickness(0.0);
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var keyboardHeight = 480.0 / args.OccludedRect.Width * args.OccludedRect.Height - AppBar.ActualHeight + 18.0;

            var height = GetKeyboardHeightDifference(keyboardHeight);
            CaptionWatermark.Visibility = Visibility.Collapsed;
            KeyboardPlaceholder.Height = keyboardHeight;
            KeyboardPlaceholder.Visibility = Visibility.Visible;
            ImagesGrid.Margin = new Thickness(0.0, 0.0, 0.0, -height);
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
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.PreviewPhoto))
            {
                if (ViewModel.PreviewPhoto != null)
                {
                    var binding = new Binding("PreviewPhoto");
                    binding.Converter = (IValueConverter)Application.Current.Resources["PhotoConverter"];
                    Preview.SetBinding(Image.SourceProperty, binding);
                }
            }
        }

        private void BeginCloseStoryboard()
        {
            SystemTray.IsVisible = true;
            //ApplicationBar.IsVisible = false;

            LayoutRoot.CacheMode = new BitmapCache();
            Bar.CacheMode = new BitmapCache();
            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, LayoutRoot);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            //var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
            //opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0 });
            //Storyboard.SetTarget(opacityImageAniamtion, BackgroundBorder);
            //Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
            //storyboard.Children.Add(opacityImageAniamtion);

            var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateBarAniamtion, Bar);
            Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateBarAniamtion);

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() => storyboard.Begin());
            storyboard.Completed += (o, args) =>
            {
                LayoutRoot.CacheMode = null;
                Bar.CacheMode = null;
                Visibility = Visibility.Collapsed;
            };
        }

        private void BeginOpenStoryboard()
        {
            SystemTray.IsVisible = false;
            //ApplicationBar.IsVisible = true;

            var transparentBlack = Colors.Black;
            transparentBlack.A = 0;

            //CaptionWatermark.Visibility = Visibility.Visible;
            Visibility = Visibility.Visible;
            //ImagesGrid.Opacity = 1.0;
            //ImagesGrid.RenderTransform = new CompositeTransform();
            //BackgroundBorder.Opacity = 1.0;


            LayoutRoot.CacheMode = new BitmapCache();
            Bar.CacheMode = new BitmapCache();
            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            ((CompositeTransform)LayoutRoot.RenderTransform).TranslateY = translateYTo;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, LayoutRoot);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = translateYTo });
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateBarAniamtion, Bar);
            Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateBarAniamtion);

            storyboard.Completed += (sender, args) =>
            {
                LayoutRoot.CacheMode = null;
                Bar.CacheMode = null;
            };
            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private void OnLoadedOnce(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoadedOnce;

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.OpenEditor());
        }

        private void Timeline_OnThumbnailChanged(object sender, ThumbnailChangedEventArgs e)
        {
            var bitmapImage = new BitmapImage();
            Preview.Source = bitmapImage;

            bitmapImage.SetSource(e.Thumbnail.AsStream());
        }

        private void Mute_OnClick(object sender, GestureEventArgs e)
        {
            var isMuted = !ViewModel.IsMuteEnabled;
            if (isMuted)
            {
                ViewModel.SelectedQuality = ViewModel.QualityList.Count == 1
                    ? ViewModel.QualityList[0]
                    : ViewModel.QualityList.FirstOrDefault(x => x.Seconds == 360) ??
                      ViewModel.QualityList.FirstOrDefault(x => x.Seconds == 240);
                ViewModel.NotifyOfPropertyChange(() => ViewModel.QualityButtonImageSource);
            }
            ViewModel.IsMuteEnabled = isMuted;
            ViewModel.Compression = !ViewModel.IsMuteEnabled && ViewModel.QualityList.Count > 1;
        }

        private void Timer_OnClick(object sender, GestureEventArgs e)
        {
            var stateService = IoC.Get<IStateService>();
            stateService.SelectedTimerSpan = ViewModel.TimerSpan;
            var chooseTTLViewModel = IoC.Get<ChooseTTLViewModel>();
            chooseTTLViewModel.Subtitle = AppResources.SelfDestructTimerVideoSubtitle;

            var chooseTTLView = new ChooseTTLView { Height = 330.0, DataContext = chooseTTLViewModel, Margin = new Thickness(0.0, -34.0, 0.0, -6.0) };
            ShellViewModel.ShowCustomMessageBox(null, null, AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                result =>
                {
                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        var selector = chooseTTLView.Selector;

                        ViewModel.TimerSpan = ((TimerSpan)selector.DataSource.SelectedItem);
                        ViewModel.NotifyOfPropertyChange(() => ViewModel.TimerSpan);
                    }
                },
                chooseTTLView);
        }

        private void Done_Click(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }

        private void Caption_OnLostFocus(object sender, RoutedEventArgs e)
        {
            //CaptionWatermark.Visibility = string.IsNullOrEmpty(Caption.Text) ? Visibility.Visible : Visibility.Collapsed;
            //KeyboardPlaceholder.Height = 0.0;
            //ImagesGrid.Margin = new Thickness(0.0);
        }

        private void Caption_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CaptionWatermark.Visibility = string.IsNullOrEmpty(Caption.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Caption_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Done();
            }
        }

        private double GetKeyboardHeightDifference(double keyboardHeight)
        {
            var heightDifference = keyboardHeight - Timeline.ActualHeight;

            return heightDifference;
        }

        private void Caption_OnGotFocus(object sender, RoutedEventArgs e)
        {
            //var height = GetKeyboardHeightDifference();
            //KeyboardPlaceholder.Height = EmojiControl.PortraitOrientationHeight - ApplicationBarPlaceholder.ActualHeight;
            //ImagesGrid.Margin = new Thickness(0.0, 0.0, 0.0, -height);
        }

        private void Quality_OnClick(object sender, GestureEventArgs e)
        {
            var stateService = IoC.Get<IStateService>();
            stateService.SelectedTimerSpan = ViewModel.SelectedQuality;
            stateService.TimerSpans = ViewModel.QualityList;
            var chooseVideoQualityViewModel = new ChooseVideoQualityViewModel(stateService);

            var chooseTTLView = new ChooseVideoQualityView { DataContext = chooseVideoQualityViewModel, Margin = new Thickness(0.0, -34.0, 0.0, -6.0) };
            ShellViewModel.ShowCustomMessageBox(null, null, AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                result =>
                {
                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        var selector = chooseTTLView.Selector;

                        ViewModel.SelectedQuality = (TimerSpan)selector.DataSource.SelectedItem;
                        ViewModel.NotifyOfPropertyChange(() => ViewModel.SelectedQuality);
                        ViewModel.NotifyOfPropertyChange(() => ViewModel.QualityButtonImageSource);
                    }
                },
                chooseTTLView);
        }

        public void Dispose()
        {
            if (_trimEventsSubscription != null) _trimEventsSubscription.Dispose();
        }

        private void UsernameHint_OnTap(object sender, GestureEventArgs e)
        {
            Caption.Focus();

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var user = frameworkElement.DataContext as IUserName;
                if (user != null)
                {
                    var userBase = user as TLUserBase;

                    var index = 0;
                    for (var i = Caption.Text.Length - 1; i >= 0; i--)
                    {
                        if (Caption.Text[i] == '@')
                        {
                            index = i;
                            break;
                        }
                    }

                    if (TLString.IsNullOrEmpty(user.UserName))
                    {
                        if (userBase != null)
                        {
                            ViewModel.AddMention(userBase);

                            Caption.Text = string.Format("{0}({1})", Caption.Text.Substring(0, index + 1), userBase.FullName);
                            Caption.SelectionStart = Caption.Text.Length - userBase.FullName.Length - 1;
                            Caption.SelectionLength = userBase.FullName.Length;
                        }
                    }
                    else
                    {
                        Caption.Text = string.Format("{0}{1} ", Caption.Text.Substring(0, index + 1), user.UserName);
                        Caption.SelectionStart = Caption.Text.Length;
                        Caption.SelectionLength = 0;
                    }
                }
            }
        }

        private void UsernameHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueLoadMentionHints();
        }
    }

    public class BooleanToBrushConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            "AccentBrush", typeof(Brush), typeof(BooleanToBrushConverter), new PropertyMetadata(default(Brush)));

        public Brush AccentBrush
        {
            get { return (Brush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        public static readonly DependencyProperty NormalBrushProperty = DependencyProperty.Register(
            "NormalBrush", typeof(Brush), typeof(BooleanToBrushConverter), new PropertyMetadata(default(Brush)));

        public Brush NormalBrush
        {
            get { return (Brush)GetValue(NormalBrushProperty); }
            set { SetValue(NormalBrushProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return AccentBrush;
            }

            return NormalBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}