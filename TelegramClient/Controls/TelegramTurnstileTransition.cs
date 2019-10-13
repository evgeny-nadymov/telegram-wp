// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;

namespace TelegramClient.Controls
{
    public class TelegramTurnstileTransition : TransitionElement
    {
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof (TurnstileTransitionMode), typeof (TelegramTurnstileTransition), null);

        public TurnstileTransitionMode Mode
        {
            get { return (TurnstileTransitionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        static TelegramTurnstileTransition()
        {
        }

        public override ITransition GetTransition(UIElement element)
        {
            return new Transition(element, TelegramTurnstileAnimations.GetAnimation(element, Mode));
        }
    }

    public static class TelegramTurnstileAnimations
    {
        public static Storyboard GetAnimation(UIElement element, TurnstileTransitionMode mode)
        {
            if (!(element.Projection is PlaneProjection))
            {
                element.Projection = new PlaneProjection { CenterOfRotationX = 0.0 };
            }

            if (mode == TurnstileTransitionMode.ForwardIn)
            {
                return ForwardIn(element);
            }
            
            if (mode == TurnstileTransitionMode.ForwardOut)
            {
                return ForwardOut(element);
            }
            
            if (mode == TurnstileTransitionMode.BackwardIn)
            {
                return BackwardIn(element);
            }
                
            return BackwardOut(element);
        }

        public static Storyboard ForwardIn(UIElement element)
        {
            var storyboard = new Storyboard();

            var rotationYAnimation = new DoubleAnimationUsingKeyFrames();
            rotationYAnimation.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -80 });
            rotationYAnimation.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 0, KeySpline = new KeySpline { ControlPoint1 = new Point(0.10000000149011612, 0.89999997615811421), ControlPoint2 = new Point(0.20000000298023224, 1) } });
            Storyboard.SetTargetProperty(rotationYAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationY)"));
            storyboard.Children.Add(rotationYAnimation);

            var globalOffsetXAnimation = new DoubleAnimationUsingKeyFrames();
            globalOffsetXAnimation.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 200 });
            globalOffsetXAnimation.KeyFrames.Add(new SplineDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 0, KeySpline = new KeySpline { ControlPoint1 = new Point(0.10000000149011612, 0.89999997615811421), ControlPoint2 = new Point(0.20000000298023224, 1) } });
            Storyboard.SetTargetProperty(globalOffsetXAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.LocalOffsetX)"));
            storyboard.Children.Add(globalOffsetXAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.01), Value = 1 });
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(storyboard, element);

            return storyboard;
        }

        public static Storyboard ForwardOut(UIElement element)
        {
            var storyboard = new Storyboard();

            var rotationYAnimation = new DoubleAnimationUsingKeyFrames();
            rotationYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0 });
            rotationYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.22), Value = 30, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5 } });
            Storyboard.SetTargetProperty(rotationYAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationY)"));
            storyboard.Children.Add(rotationYAnimation);

            var globalOffsetXAnimation = new DoubleAnimationUsingKeyFrames();
            globalOffsetXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0 });
            globalOffsetXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.22), Value = -100, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5 } });
            Storyboard.SetTargetProperty(globalOffsetXAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.GlobalOffsetX)"));
            storyboard.Children.Add(globalOffsetXAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.219), Value = 1 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.22), Value = 0 });
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(storyboard, element);

            return storyboard;
        }

        public static Storyboard BackwardIn(UIElement element)
        {
            var storyboard = new Storyboard();

            var rotationYAnimation = new DoubleAnimationUsingKeyFrames();
            rotationYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 50 });
            rotationYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6 } });
            Storyboard.SetTargetProperty(rotationYAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationY)"));
            storyboard.Children.Add(rotationYAnimation);

            var globalOffsetXAnimation = new DoubleAnimationUsingKeyFrames();
            globalOffsetXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -60 });
            globalOffsetXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6 } });
            Storyboard.SetTargetProperty(globalOffsetXAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.GlobalOffsetX)"));
            storyboard.Children.Add(globalOffsetXAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.001), Value = 1 });
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(storyboard, element);

            return storyboard;
        }

        public static Storyboard BackwardOut(UIElement element)
        {
            var storyboard = new Storyboard();

            var rotationYAnimation = new DoubleAnimationUsingKeyFrames();
            rotationYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0 });
            rotationYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = -60, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5 } });
            Storyboard.SetTargetProperty(rotationYAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationY)"));
            storyboard.Children.Add(rotationYAnimation);

            var globalOffsetXAnimation = new DoubleAnimationUsingKeyFrames();
            globalOffsetXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0 });
            globalOffsetXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = 60, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6 } });
            Storyboard.SetTargetProperty(globalOffsetXAnimation, new PropertyPath("(UIElement.Projection).(PlaneProjection.GlobalOffsetX)"));
            storyboard.Children.Add(globalOffsetXAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.19), Value = 1 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = 0 });
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(storyboard, element);

            return storyboard;
        }
    }
}
