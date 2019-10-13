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
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Telegram.Api.TL;

namespace TelegramClient.Behaviors
{
    public class SelectionBehavior : Behavior<CheckBox>
    {
        public static readonly DependencyProperty IsSelectionEnabledProperty = DependencyProperty.Register(
            "IsSelectionEnabled", typeof (bool), typeof (SelectionBehavior), new PropertyMetadata(default(bool), OnSelectionChanged));

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as SelectionBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                if ((bool) e.NewValue)
                {
                    var storyboard = new Storyboard();
                    if (!(behavior.AssociatedObject.RenderTransform is CompositeTransform))
                    {
                        behavior.AssociatedObject.RenderTransform = new CompositeTransform();
                    }
                    var continuumLayoutRootY = new DoubleAnimationUsingKeyFrames();
                    continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -80.0 });
                    continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = -80.0 });
                    continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.55), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                    Storyboard.SetTarget(continuumLayoutRootY, behavior.AssociatedObject);
                    Storyboard.SetTargetProperty(continuumLayoutRootY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                    storyboard.Children.Add(continuumLayoutRootY);

                    behavior.AssociatedObject.Visibility = Visibility.Visible;
                    storyboard.Begin();
                }
                else
                {
                    behavior.AssociatedObject.Visibility = Visibility.Collapsed;
                    //var storyboard = new Storyboard();
                    //if (!(behavior.AssociatedObject.RenderTransform is CompositeTransform))
                    //{
                    //    behavior.AssociatedObject.RenderTransform = new CompositeTransform();
                    //}
                    //var translateX = new DoubleAnimationUsingKeyFrames();
                    //translateX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
                    //translateX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = -50.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 } });
                    //Storyboard.SetTarget(translateX, behavior.AssociatedObject);
                    //Storyboard.SetTargetProperty(translateX, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                    //storyboard.Children.Add(translateX);

                    //var visibility = new ObjectAnimationUsingKeyFrames();
                    //visibility.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = Visibility.Collapsed });
                    //Storyboard.SetTarget(visibility, behavior.AssociatedObject);
                    //Storyboard.SetTargetProperty(visibility, new PropertyPath("Visibility"));
                    //storyboard.Children.Add(visibility);

                    //storyboard.Begin();
                }
            }
        }

        public bool IsSelectionEnabled
        {
            get { return (bool) GetValue(IsSelectionEnabledProperty); }
            set { SetValue(IsSelectionEnabledProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_OnLoaded;

            base.OnAttached();
        }

        private void AssociatedObject_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(AssociatedObject.RenderTransform is CompositeTransform))
            {
                AssociatedObject.RenderTransform = new CompositeTransform();
            }
            var transform = AssociatedObject.RenderTransform as CompositeTransform;
            transform.TranslateX = IsSelectionEnabled ? 0.0 : -68.0;

            //var message = AssociatedObject.DataContext as TLMessage;
            //if (message != null)
            //{
            //    TLUtils.WriteLine(string.Format("SelectionsBehavior.OnLoaded {0} {1}", message.Index, message.Message), LogSeverity.Error);
            //}
            //else
            //{
            //    TLUtils.WriteLine(string.Format("SelectionsBehavior.OnLoaded {0}", "null"), LogSeverity.Error);
            //}
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_OnLoaded;

            base.OnDetaching();
        }
    }

    public class NewSelectionBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty IsSelectionEnabledProperty = DependencyProperty.Register(
            "IsSelectionEnabled", typeof(bool), typeof(NewSelectionBehavior), new PropertyMetadata(default(bool), OnSelectionChanged));

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as NewSelectionBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                if ((bool)e.NewValue)
                {
                    //behavior.AssociatedObject.CacheMode = behavior.AssociatedObject.CacheMode ?? new BitmapCache();

                    var storyboard = new Storyboard();
                    if (!(behavior.AssociatedObject.RenderTransform is CompositeTransform))
                    {
                        behavior.AssociatedObject.RenderTransform = new CompositeTransform();
                    }
                    var translateXAnimation = new DoubleAnimationUsingKeyFrames();
                    translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -68.0 });
                    translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.55), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                    Storyboard.SetTarget(translateXAnimation, behavior.AssociatedObject);
                    Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                    storyboard.Children.Add(translateXAnimation);

                    //behavior.AssociatedObject.Visibility = Visibility.Visible;
                    storyboard.Begin();
                    //Deployment.Current.Dispatcher.BeginInvoke(() => storyboard.Begin());
                }
                else
                {
                    var storyboard = new Storyboard();
                    if (!(behavior.AssociatedObject.RenderTransform is CompositeTransform))
                    {
                        behavior.AssociatedObject.RenderTransform = new CompositeTransform();
                    }
                    var translateXAnimation = new DoubleAnimationUsingKeyFrames();
                    translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
                    translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.55), Value = -68.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                    Storyboard.SetTarget(translateXAnimation, behavior.AssociatedObject);
                    Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                    storyboard.Children.Add(translateXAnimation);

                    //behavior.AssociatedObject.Visibility = Visibility.Visible;
                    storyboard.Begin();
                    //Deployment.Current.Dispatcher.BeginInvoke(() => storyboard.Begin());
                }
            }
        }

        public bool IsSelectionEnabled
        {
            get { return (bool)GetValue(IsSelectionEnabledProperty); }
            set { SetValue(IsSelectionEnabledProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_OnLoaded;

            base.OnAttached();
        }

        private void AssociatedObject_OnLoaded(object sender, RoutedEventArgs e)
        {
            var transform = AssociatedObject.RenderTransform as CompositeTransform;
            if (transform != null)
            {
                transform.TranslateX = IsSelectionEnabled ? 0.0 : -68.0;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_OnLoaded;

            base.OnDetaching();
        }
    }
}
