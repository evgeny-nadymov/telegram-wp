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
using System.Windows.Media.Animation;
using TelegramClient.Animation.Navigation;

namespace TelegramClient.Views.Additional
{
    public partial class ShareView
    {
        public ShareView()
        {
            InitializeComponent();

            AnimationContext = LayoutRoot;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            var storyboard = new Storyboard();

            //var socialNetworksRotationAnimation = new DoubleAnimationUsingKeyFrames();
            //socialNetworksRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
            //socialNetworksRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 90.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            //Storyboard.SetTarget(socialNetworksRotationAnimation, SocialNetworks);
            //Storyboard.SetTargetProperty(socialNetworksRotationAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));
            //storyboard.Children.Add(socialNetworksRotationAnimation);

            //var smsRotationAnimation = new DoubleAnimationUsingKeyFrames();
            //smsRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.10), Value = 0.0 });
            //smsRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 90.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            //Storyboard.SetTarget(smsRotationAnimation, Sms);
            //Storyboard.SetTargetProperty(smsRotationAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));
            //storyboard.Children.Add(smsRotationAnimation);

            //var emailRotationAnimation = new DoubleAnimationUsingKeyFrames();
            //emailRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.20), Value = 0.0 });
            //emailRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.45), Value = 90.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            //Storyboard.SetTarget(emailRotationAnimation, Email);
            //Storyboard.SetTargetProperty(emailRotationAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));
            //storyboard.Children.Add(emailRotationAnimation);

            

            var rootRotationAnimation = new DoubleAnimationUsingKeyFrames();
            rootRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            rootRotationAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 90.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            Storyboard.SetTarget(rootRotationAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(rootRotationAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)"));
            storyboard.Children.Add(rootRotationAnimation);

            var rootOpacityAnimation = new DoubleAnimationUsingKeyFrames();
            rootOpacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1.0 });
            rootOpacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            Storyboard.SetTarget(rootOpacityAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(rootOpacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(rootOpacityAnimation);

            storyboard.Begin();

            //e.Cancel = true;

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
                return new SwivelHideAnimator { RootElement = LayoutRoot };
            }

            return null;
        }

        protected override void AnimationsComplete(AnimationType animationType)
        {
            if (animationType == AnimationType.NavigateForwardIn)
            {
                //var storyboard = new Storyboard();
                //var searchBox = new DoubleAnimationUsingKeyFrames();
                //searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -100.0 });
                //searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                //Storyboard.SetTarget(searchBox, SearchBox);
                //Storyboard.SetTargetProperty(searchBox, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                //storyboard.Children.Add(searchBox);

                //var searchBoxOpacity = new DoubleAnimation
                //{
                //    From = 0.0,
                //    To = 1.0,
                //    Duration = TimeSpan.FromSeconds(0.15),
                //    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 }
                //};
                //Storyboard.SetTarget(searchBoxOpacity, SearchBox);
                //Storyboard.SetTargetProperty(searchBoxOpacity, new PropertyPath("(UIElement.Opacity)"));
                //storyboard.Children.Add(searchBoxOpacity);

                //SearchBox.Opacity = 0.0;
                //Deployment.Current.Dispatcher.BeginInvoke(() =>
                //{
                //    SearchBox.Opacity = 1.0;
                //    storyboard.Begin();
                //});
            }

            base.AnimationsComplete(animationType);
        }
    }
}