// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using Telegram.Controls;
using TelegramClient.Animation.Navigation;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Search;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Search
{
    public partial class SearchLinksView
    {
        private readonly IDisposable _keyPressSubscription;

        public SearchLinksViewModel ViewModel
        {
            get { return DataContext as SearchLinksViewModel; }
        }

        public SearchLinksView()
        {
            InitializeComponent();

            AnimationContext = LayoutRoot;

            var keyPressEvents = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                keh => { SearchBox.TextChanged += keh; },
                keh => { SearchBox.TextChanged -= keh; });

            _keyPressSubscription = keyPressEvents
                .Throttle(TimeSpan.FromSeconds(0.10))
                .ObserveOnDispatcher()
                .Subscribe(e => ViewModel.Search());

            Loaded += (o, e) =>
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };

            Unloaded += (o, e) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsWorking))
            {
                if (!ViewModel.IsWorking)
                {
                    RestoreTapedItem();
                }
            }
        }

        private void RestoreTapedItem()
        {
            if (_lastTapedItem != null)
            {
                if (_lastTapedItem != null)
                {
                    _lastStoryboard.Stop();
                }

                _lastTapedItem.Opacity = 1.0;
                var compositeTransform = _lastTapedItem.RenderTransform as CompositeTransform;
                if (compositeTransform != null)
                {
                    compositeTransform.TranslateX = 0.0;
                    compositeTransform.TranslateY = 0.0;
                }
            }
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardIn
                || animationType == AnimationType.NavigateBackwardIn)
            {
                return new SlideUpAnimator { RootElement = LayoutRoot };
            }

            return null;
        }

        protected override void AnimationsComplete(AnimationType animationType)
        {
            if (animationType == AnimationType.NavigateForwardIn)
            {
                if (string.IsNullOrEmpty(ViewModel.Text))
                {
                    SearchBox.Focus();
                }
            }

            base.AnimationsComplete(animationType);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_lastTapedItem != null)
            {
                _lastTapedItem.Opacity = 1.0;
                var compositeTransform = _lastTapedItem.RenderTransform as CompositeTransform;
                if (compositeTransform != null)
                {
                    compositeTransform.TranslateX = 0.0;
                    compositeTransform.TranslateY = 0.0;
                }
            }

            base.OnBackKeyPress(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (!e.Cancel && e.Uri.OriginalString != "app://external/")
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
            }
        }

        public void Dispose()
        {
            _keyPressSubscription.Dispose();
        }

        private FrameworkElement _lastTapedItem;

        private Storyboard _lastStoryboard;

        private void MainItemGrid_OnTextBlockTap(object sender, System.Windows.Input.GestureEventArgs args)
        {
            MainItem_OnTapCommon<TextBlock>(sender, args);
        }

        private void MainItemGrid_OnHighlightingTextBlockTap(object sender, System.Windows.Input.GestureEventArgs args)
        {
            MainItem_OnTapCommon<HighlightingTextBlock>(sender, args);
        }

        private void MainItem_OnTapCommon<T>(object sender, GestureEventArgs args)
            where T : FrameworkElement
        {
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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

        private void Items_OnScrollingStateChanged(object sender, ScrollingStateChangedEventArgs e)
        {
            if (e.NewValue)
            {
                var focusElement = FocusManager.GetFocusedElement();
                if (focusElement == SearchBox)
                {
                    Self.Focus();
                }
            }
        }

        private void DeleteMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var channel = ViewModel.CurrentItem as TLChannel;
            menuItem.Visibility = (channel == null || channel.Creator)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}