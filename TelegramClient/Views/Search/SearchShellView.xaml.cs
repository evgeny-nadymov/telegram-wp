// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Extensions;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Search
{
    public partial class SearchShellView
    {
        public SearchShellViewModel ViewModel
        {
            get { return DataContext as SearchShellViewModel; }
        }

        public SearchShellView()
        {
            InitializeComponent();

            OptimizeFullHD();

            Loaded += SearchShellView_Loaded;
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var obj = element.DataContext as TLObject;
            if (obj == null) return;

            if (!ViewModel.OpenDialogDetails(obj)) return;

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

                StartContinuumForwardOutAnimation(_lastTapedItem, tapedItemContainer);
            }
        }

        private void Items_OnScrollingStateChanged(object sender, ScrollingStateChangedEventArgs e)
        {
            if (e.NewValue)
            {
                var focusElement = FocusManager.GetFocusedElement();
                if (focusElement != null
                    && focusElement.GetType() == typeof(WatermarkedTextBox))
                {
                    Self.Focus();
                }
            }
        }

        private void OptimizeFullHD()
        {
            //var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            //if (!isFullHD) return;

            Items.Margin = new Thickness(Items.Margin.Left, Items.Margin.Top + 12.0, Items.Margin.Right, Items.Margin.Bottom);
            Items.HeaderTemplate = (DataTemplate)Application.Current.Resources["FullHDPivotHeaderTemplate"];
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

        private void SearchShellView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isBackwardIn)
            {
                return;
            }

            LayoutRoot.Opacity = 0.0;

            var storyboard = new Storyboard();

            var translateAnimation = new DoubleAnimationUsingKeyFrames();
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 150.0 });
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(translateAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 1.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
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
            };

            Execute.BeginOnUIThread(storyboard.Begin);
        }

        private bool _isBackwardIn;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                _isBackwardIn = true;
            }

            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
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
            //});
            

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

        private void MainItemGrid_OnHold(object sender, GestureEventArgs e)
        {
            ViewModel.ClearSearchHistory();
        }
    }
}