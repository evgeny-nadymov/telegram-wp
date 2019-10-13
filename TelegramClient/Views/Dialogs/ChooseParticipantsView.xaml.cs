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
using Telegram.Controls.Extensions;
using TelegramClient.Animation.Navigation;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.Views.Dialogs
{
    public partial class ChooseParticipantsView : IDisposable
    {
        private readonly IDisposable _keyPressSubscription;

        private FrameworkElement _lastTapedItem;

        public ChooseParticipantsViewModel ViewModel
        {
            get { return DataContext as ChooseParticipantsViewModel; }
        }

        public ChooseParticipantsView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            AnimationContext = LayoutRoot;

            var keyPressEvents = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                keh => { SearchBox.TextChanged += keh; },
                keh => { SearchBox.TextChanged -= keh; });

            _keyPressSubscription = keyPressEvents
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(e => ViewModel.Search());
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            var storyboard = new Storyboard();

            var layoutRoot = new DoubleAnimationUsingKeyFrames();
            layoutRoot.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.1), Value = 0.0 });
            layoutRoot.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 90.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            Storyboard.SetTarget(layoutRoot, LayoutRoot);
            Storyboard.SetTargetProperty(layoutRoot, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));
            storyboard.Children.Add(layoutRoot);

            var searchBox = new DoubleAnimationUsingKeyFrames();
            searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = -100.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 } });
            Storyboard.SetTarget(searchBox, TitlePanel);
            Storyboard.SetTargetProperty(searchBox, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(searchBox);

            var searchBoxOpacity = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.15),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 }
            };
            Storyboard.SetTarget(searchBoxOpacity, TitlePanel);
            Storyboard.SetTargetProperty(searchBoxOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(searchBoxOpacity);

            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                storyboard.Begin();
            }//);

            base.OnBackKeyPress(e);
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardIn
                || animationType == AnimationType.NavigateBackwardIn)
            {
                return new SwivelShowAnimator { RootElement = LayoutRoot };
            }
            else if (animationType == AnimationType.NavigateForwardOut)
            {
                if (toOrFrom != null
                    && toOrFrom.ToString().Contains("DialogDetailsView.xaml"))
                {
                    return null;

                    return new SlideDownAnimator{ RootElement = LayoutRoot };
                }

                return new SwivelHideAnimator { RootElement = LayoutRoot };              
            }

            return null;
        }

        protected override void AnimationsComplete(AnimationType animationType)
        {
            if (animationType == AnimationType.NavigateForwardIn)
            {
                var storyboard = new Storyboard();
                var searchBox = new DoubleAnimationUsingKeyFrames();
                searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -100.0 });
                searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(searchBox, TitlePanel);
                Storyboard.SetTargetProperty(searchBox, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(searchBox);

                var searchBoxOpacity = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.15),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 }
                };
                Storyboard.SetTarget(searchBoxOpacity, TitlePanel);
                Storyboard.SetTargetProperty(searchBoxOpacity, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(searchBoxOpacity);

                TitlePanel.Opacity = 0.0;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TitlePanel.Opacity = 1.0;
                    storyboard.Begin();
                });
            }

            base.AnimationsComplete(animationType);
        }

        public void Dispose()
        {
            _keyPressSubscription.Dispose();
        }

        private void LongListSelector_OnScrollingStarted(object sender, System.EventArgs e)
        {
            var focusElement = FocusManager.GetFocusedElement();
            if (focusElement == SearchBox)
            {
                Self.Focus();
            }
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            _lastTapedItem = sender as FrameworkElement;

            if (_lastTapedItem != null)
            {
                //foreach (var descendant in _lastTapedItem.GetVisualDescendants().OfType<HighlightingTextBlock>())
                //{
                //    if (GetIsAnimationTarget(descendant))
                //    {
                //        _lastTapedItem = descendant;
                //        break;
                //    }
                //}

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
        }



        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (!e.Cancel)
            {
                if (e.Uri != null
                    && e.Uri.ToString().Contains("DialogDetailsView.xaml"))
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
        }
    }
}