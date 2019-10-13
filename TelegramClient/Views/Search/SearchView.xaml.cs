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
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Extensions;
using TelegramClient.ViewModels.Search;
using Action = System.Action;
using Execute = Telegram.Api.Helpers.Execute;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Search
{
    public partial class SearchView
    {
        public SearchViewModel ViewModel
        {
            get { return DataContext as SearchViewModel; }
        }

        public SearchView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
            LayoutRoot.Opacity = 0.0;

            Loaded += SearchShellView_Loaded;
        }

        ~SearchView()
        {
            
        }

        public Action<Visibility> ClosePivotAction;

        private void HorizontalItem_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var with = frameworkElement.DataContext as TLObject;
            if (with == null) return;

            ClosePivotAction.SafeInvoke(Visibility.Visible);
            ViewModel.StateService.CollapseSearchControl = true;

            Execute.BeginOnUIThread(() =>
            {
                if (!ViewModel.OpenDialogDetails(with, false)) return;
            });
        }

        private void VerticalItem2_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var with = frameworkElement.DataContext as TLObject;
            if (with == null) return;

            ClosePivotAction.SafeInvoke(Visibility.Visible);
            ViewModel.StateService.CollapseSearchControl = true;

            Execute.BeginOnUIThread(() =>
            {
                if (!ViewModel.OpenDialogDetails(with, true)) return;

                _lastTapedItem = sender as FrameworkElement;

                if (_lastTapedItem != null)
                {
                    if (!(_lastTapedItem.RenderTransform is CompositeTransform))
                    {
                        _lastTapedItem.RenderTransform = new CompositeTransform();
                    }

                    var tapedItemContainer = _lastTapedItem.FindParentOfType<ListBoxItem>();
                    if (tapedItemContainer != null)
                    {
                        tapedItemContainer = tapedItemContainer.FindParentOfType<ListBoxItem>();
                    }

                    ShellView.StartContinuumForwardOutAnimation(_lastTapedItem, tapedItemContainer);
                }
            });
        }

        private void VerticalItem_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var with = frameworkElement.DataContext as TLObject;
            if (with == null) return;

            ClosePivotAction.SafeInvoke(Visibility.Visible);
            ViewModel.StateService.CollapseSearchControl = true;

            Execute.BeginOnUIThread(() =>
            {
                if (!ViewModel.OpenDialogDetails(with, true)) return;

                _lastTapedItem = sender as FrameworkElement;

                if (_lastTapedItem != null)
                {
                    if (!(_lastTapedItem.RenderTransform is CompositeTransform))
                    {
                        _lastTapedItem.RenderTransform = new CompositeTransform();
                    }

                    var tapedItemContainer = _lastTapedItem.FindParentOfType<ListBoxItem>();
                    if (tapedItemContainer != null)
                    {
                        tapedItemContainer = tapedItemContainer.FindParentOfType<ListBoxItem>();
                    }

                    ShellView.StartContinuumForwardOutAnimation(_lastTapedItem, tapedItemContainer);
                }
            });
        }

        private void Items_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            CloseKeyboard();
        }

        private void CloseKeyboard()
        {
            var focusElement = FocusManager.GetFocusedElement();
            if (focusElement != null
                && focusElement.GetType() == typeof(WatermarkedTextBox))
            {
                Self.Focus();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (!e.Cancel && e.Uri.OriginalString != "app://external/")
            {
                BeginCloseStoryboard();
            }
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
                if (string.IsNullOrEmpty(ViewModel.Text))
                {
                    SearchBox.Focus();
                }
                ViewModel.ForwardInAnimationComplete();
                ClosePivotAction.SafeInvoke(Visibility.Collapsed);
            };
            _openStoryboard = storyboard;
            Execute.BeginOnUIThread(storyboard.Begin);
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

        private static FrameworkElement _lastTapedItem;

        public static void StartContinuumForwardOutAnimation(FrameworkElement tapedItem, FrameworkElement tapedItemContainer)
        {
            _lastTapedItem = tapedItem;

            _lastTapedItem.CacheMode = new BitmapCache();

            var storyboard = new Storyboard();

            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 73.0 });
            Storyboard.SetTarget(timeline, tapedItem);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(timeline);

            var timeline2 = new DoubleAnimationUsingKeyFrames();
            timeline2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            timeline2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 425.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 } });
            Storyboard.SetTarget(timeline2, tapedItem);
            Storyboard.SetTargetProperty(timeline2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(timeline2);

            var timeline3 = new DoubleAnimationUsingKeyFrames();
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1.0 });
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = 1.0 });
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
            Storyboard.SetTarget(timeline3, tapedItem);
            Storyboard.SetTargetProperty(timeline3, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(timeline3);

            if (tapedItemContainer != null)
            {
                var timeline4 = new ObjectAnimationUsingKeyFrames();
                timeline4.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 999.0 });
                timeline4.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
                Storyboard.SetTarget(timeline4, tapedItemContainer);
                Storyboard.SetTargetProperty(timeline4, new PropertyPath("(Canvas.ZIndex)"));
                storyboard.Children.Add(timeline4);
            }

            storyboard.Begin();
        }

        private void ClearRecent_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.ClearSearchHistory();
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ViewModel.LoadNextSlice();
        }

        private void Expander_OnExpanded(object sender, RoutedEventArgs e)
        {
            //ShowText.Text = "Show less";
        }

        private void Expander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            //ShowText.Text = "Show more";
        }
    }
}