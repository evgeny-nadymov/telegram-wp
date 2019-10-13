// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Telegram.Controls.Extensions;
using TelegramClient.Extensions;
using TelegramClient.Views;

namespace TelegramClient.Animation.Navigation
{
    
    public class AnimatedBasePage : TelegramViewBase
    {

        public static readonly DependencyProperty IsAnimationTargetProperty = DependencyProperty.RegisterAttached("IsAnimationTarget",
                                                                                                                typeof(bool),
                                                                                                                typeof(AnimatedBasePage),
                                                                                                                null);

        public static void SetIsAnimationTarget(UIElement element, bool value)
        {
            element.SetValue(IsAnimationTargetProperty, value);
        }

        public static bool GetIsAnimationTarget(UIElement element)
        {
            return (bool)element.GetValue(IsAnimationTargetProperty);
        }

        private static readonly Uri ExternalUri = new Uri(@"app://external/");

        public static readonly DependencyProperty AnimationContextProperty = DependencyProperty.Register(
            "AnimationContext", 
            typeof(FrameworkElement), 
            typeof(AnimatedBasePage), 
            new PropertyMetadata(null));
        
        public FrameworkElement AnimationContext
        {
            get { return (FrameworkElement)GetValue(AnimationContextProperty); }
            set { SetValue(AnimationContextProperty, value); }
        }

        private static Uri _fromUri;

        private bool _isAnimating;
        private static bool _isNavigating;
        private bool _needsOutroAnimation;
        private Uri _nextUri;
        private Uri _arrivedFromUri;
        private AnimationType _currentAnimationType;
        private NavigationMode? _currentNavigationMode;
        private bool _isActive;
        private bool _isForwardNavigation;
        private bool _loadingAndAnimatingIn;

        private static PageOrientation _lastOrientation;

        public AnimatedBasePage()
        {
            _isActive = true;

            _isForwardNavigation = true;

            OrientationChanged += Page_OrientationChanged;
        }

        void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            var newOrientation = e.Orientation;

            if (newOrientation == _lastOrientation) return;
            // Orientations are (clockwise) 'PortraitUp', 'LandscapeRight', 'LandscapeLeft'

            var transitionElement = new RotateTransition();

            switch (newOrientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeRight:
                    // Come here from PortraitUp (i.e. clockwise) or LandscapeLeft?
                    if (_lastOrientation == PageOrientation.PortraitUp)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In180Counterclockwise;
                    break;
                case PageOrientation.LandscapeLeft:
                    // Come here from LandscapeRight or PortraitUp?
                    if (_lastOrientation == PageOrientation.LandscapeRight)
                        transitionElement.Mode = RotateTransitionMode.In180Clockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    // Come here from LandscapeLeft or LandscapeRight?
                    if (_lastOrientation == PageOrientation.LandscapeLeft)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
            }
            
            // Execute the transition
            var phoneApplicationPage = (PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content;
            var transition = transitionElement.GetTransition(phoneApplicationPage);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();

            _lastOrientation = newOrientation;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            //if (_isNavigating)
            //{
            //    e.Cancel = true;
            //    return;
            //}

            if (!CanAnimate())
                return;

            //if (_isAnimating)
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (_loadingAndAnimatingIn)
            //{
            //    e.Cancel = true;
            //    return;
            //}

            if (!NavigationService.CanGoBack)
                return;
            
            if (!IsPopupOpen())
            {
                _isNavigating = true;
                e.Cancel = true;
                _needsOutroAnimation = false;
                _currentAnimationType = AnimationType.NavigateBackwardOut;
                _currentNavigationMode = NavigationMode.Back;

                RunAnimation();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.Cancel)
            {
                return;
            }

            _lastOrientation = Orientation;

            if (_isAnimating)
            {
                e.Cancel = true;
                return;
            }

            if (_loadingAndAnimatingIn)
            {
                e.Cancel = true;
                return;
            }

            _fromUri = NavigationService.CurrentSource;

            if (_needsOutroAnimation)
            {
                _needsOutroAnimation = false;

                if (!CanAnimate())
                    return;

                if (_isNavigating)
                {
                    e.Cancel = true;
                    return;
                }

                if (!NavigationService.CanGoBack && e.NavigationMode == NavigationMode.Back)
                    return;

                if (IsPopupOpen() && e.Uri != ExternalUri)
                {
                    return;
                }

                e.Cancel = true;
                _nextUri = e.Uri;

                switch (e.NavigationMode)
                {
                    case NavigationMode.New:
                        _currentAnimationType = AnimationType.NavigateForwardOut;
                        break;

                    case NavigationMode.Back:
                        _currentAnimationType = AnimationType.NavigateBackwardOut;
                        break;

                    case NavigationMode.Forward:
                        _currentAnimationType = AnimationType.NavigateForwardOut;
                        break;
                }
                _currentNavigationMode = e.NavigationMode;

                if (e.Uri != ExternalUri)
                    RunAnimation();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Orientation = _lastOrientation;

            _currentNavigationMode = null;

            //Debug.WriteLine("OnNavigatedTo: {0}", this);

            if (_nextUri != ExternalUri)
            {
                //this.InvokeOnLayoutUpdated(() => OnLayoutUpdated(this, null));
                _loadingAndAnimatingIn = true;
                Loaded += AnimatedBasePage_Loaded;

                if (AnimationContext != null)
                {
                    AnimationContext.Opacity = 0;
                }
            }

            _needsOutroAnimation = true;
        }

        void AnimatedBasePage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AnimatedBasePage_Loaded;
            OnLayoutUpdated();
        }

        public void OnLayoutUpdated()
        {
            //Debug.WriteLine("OnLayoutUpdated: {0}", this);

            if (_isForwardNavigation)
            {
                _currentAnimationType = AnimationType.NavigateForwardIn;
                _arrivedFromUri = _fromUri != null ? new Uri(_fromUri.OriginalString, UriKind.Relative) : null;
            }
            else
            {
                _currentAnimationType = AnimationType.NavigateBackwardIn;
            }

            if (CanAnimate())
            {
                RunAnimation();
            }
            else
            {
                if (AnimationContext != null)
                    AnimationContext.Opacity = 1;

                OnTransitionAnimationCompleted();
            }

            //OnFirstLayoutUpdated(!_isForwardNavigation, _fromUri);

            if (_isForwardNavigation)
                _isForwardNavigation = false;
        }

        protected virtual void OnFirstLayoutUpdated(bool isBackNavigation, Uri from) { }

        private void RunAnimation()
        {
            _isAnimating = true;

            AnimatorHelperBase animation;

            switch (_currentAnimationType)
            {
                case AnimationType.NavigateForwardIn:
                    animation = GetAnimation(_currentAnimationType, _fromUri);
                    break;
                case AnimationType.NavigateBackwardOut:
                    animation = GetAnimation(_currentAnimationType, _arrivedFromUri);
                    break;
                default:
                    animation = GetAnimation(_currentAnimationType, _nextUri);
                    break;
            }

            Dispatcher.BeginInvoke(() =>
            {
                if (animation == null)
                {
                    AnimationContext.Opacity = 1;
                    OnTransitionAnimationCompleted();
                }
                else
                {
                    AnimatorHelperBase transitionAnimation = animation;
                    AnimationContext.Opacity = 1;
                    transitionAnimation.Begin(OnTransitionAnimationCompleted);
                }

                //Debug.WriteLine("{0} - {1} - {2} - {3}", this, _currentAnimationType, _currentAnimationType == AnimationType.NavigateForwardOut || _currentAnimationType == AnimationType.NavigateBackwardIn ? _nextUri : _fromUri, transitionAnimation);
            });
        }

        private bool CanAnimate()
        {
            return (_isActive 
                && !_isNavigating 
                && AnimationContext != null);
        }

        void OnTransitionAnimationCompleted()
        {
            _isAnimating = false;
            _loadingAndAnimatingIn = false;

            try
            {
                Dispatcher.BeginInvoke(() =>
                {
                    //Debug.WriteLine("{0} - Animation complete: {1}", this, _currentAnimationType);
                    //Debug.WriteLine("nav mode : {0}", _currentNavigationMode);
                    switch (_currentNavigationMode)
                    {
                        case NavigationMode.Forward:
                            Application.Current.GoForward();
                            break;

                        case NavigationMode.Back:
                            Application.Current.GoBack();
                            break;

                        case NavigationMode.New:
                            Application.Current.Navigate(_nextUri);
                            break;
                    }
                    _isNavigating = false;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnTransitionAnimationCompleted Exception on {0}: {1}", this, ex);
            }

            AnimationsComplete(_currentAnimationType);
        }

        public AnimatorHelperBase GetContinuumAnimation(FrameworkElement element, AnimationType animationType)
        {
            var movedText = element;

            var isTarget = false;
            
            if (element != null)
            {
                isTarget = GetIsAnimationTarget(element);
            } 

            if (!isTarget && element != null)
            {
                foreach (var descendant in element.GetVisualDescendants().OfType<FrameworkElement>())
                {
                    if (GetIsAnimationTarget(descendant))
                    {
                        movedText = descendant;
                        break;
                    }
                }
            }

            if (movedText != null)
            {
                if (animationType == AnimationType.NavigateForwardIn)
                {
                    return new ContinuumForwardInAnimator() { RootElement = movedText, LayoutRoot = AnimationContext };
                }
                if (animationType == AnimationType.NavigateForwardOut)
                {
                    return new ContinuumForwardOutAnimator() { RootElement = movedText, LayoutRoot = AnimationContext };
                }
                if (animationType == AnimationType.NavigateBackwardIn)
                {
                    return new ContinuumBackwardInAnimator() { RootElement = movedText, LayoutRoot = AnimationContext };
                }
                if (animationType == AnimationType.NavigateBackwardOut)
                {
                    return new ContinuumBackwardOutAnimator() { RootElement = movedText, LayoutRoot = AnimationContext };
                }
            }
            return null;
        }

        protected virtual void AnimationsComplete(AnimationType animationType) { }

        protected virtual AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            AnimatorHelperBase animation;

            switch (animationType)
            {
                case AnimationType.NavigateBackwardIn:
                    animation = new TurnstileBackwardInAnimator();
                    break;

                case AnimationType.NavigateBackwardOut:
                    animation = new TurnstileBackwardOutAnimator();
                    break;

                case AnimationType.NavigateForwardIn:
                    animation = new TurnstileForwardInAnimator();
                    break;

                default:
                    animation = new TurnstileForwardOutAnimator();
                    break;
            }

            animation.RootElement = AnimationContext;
            return animation;
        }

        protected virtual bool IsPopupOpen()
        {
            return false;
        }

        public void CancelAnimation()
        {
            _isActive = false;
        }

        public void ResumeAnimation()
        {
            _isActive = true;
        }
    }
}
