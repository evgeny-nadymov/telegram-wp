// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace TelegramClient.Behaviors
{
    public class ProgressBarSmoother
    {
        public static double GetSmoothValue(DependencyObject obj)
        {
            return (double)obj.GetValue(SmoothValueProperty);
        }

        public static void SetSmoothValue(DependencyObject obj, double value)
        {
            obj.SetValue(SmoothValueProperty, value);
        }

        public static readonly DependencyProperty SmoothValueProperty =
            DependencyProperty.RegisterAttached("SmoothValue", typeof(double), typeof(ProgressBarSmoother), new PropertyMetadata(0.0, OnSmoothValueChanged));

        private static void OnSmoothValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if ((double)e.NewValue > (double)e.OldValue)
            //{
                
            //}

            var animation = new DoubleAnimation
            {
                To = (double) e.NewValue,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                //EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 }
            };

            Storyboard.SetTarget(animation, d);
            Storyboard.SetTargetProperty(animation, new PropertyPath(RangeBase.ValueProperty));
            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.Begin();
        }
    }
}
