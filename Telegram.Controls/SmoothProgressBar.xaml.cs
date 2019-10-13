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

namespace Telegram.Controls
{
    public partial class SmoothProgressBar
    {
        private bool _useAnimations = true;

        public bool UseAnimations
        {
            get { return _useAnimations; }
            set { _useAnimations = value; }
        }

        public static readonly DependencyProperty CommandTextProperty = DependencyProperty.Register(
            "CommandText", typeof (string), typeof (SmoothProgressBar), new PropertyMetadata(default(string)));

        public string CommandText
        {
            get { return (string) GetValue(CommandTextProperty); }
            set { SetValue(CommandTextProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof (double), typeof (SmoothProgressBar), new PropertyMetadata(default(double), OnValueChanged));

        private Storyboard _previousStoryboard;

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var smoothProgressBar = (SmoothProgressBar) d;
            if ((double) e.NewValue > 0.0)
            {
                smoothProgressBar.Visibility = Visibility.Visible;
            }

            //smoothProgressBar.Progress.Value = (double)e.NewValue;
            //if ((double)e.NewValue <= 0.0 || (double)e.NewValue >= 1.0)
            //{
            //    smoothProgressBar.Visibility = Visibility.Collapsed;
            //}
            //return;
            var newValue = (double)e.NewValue;
            if ((double) e.OldValue > 0.0 && newValue == 0.0)
            {
               newValue = 1.0;
            }

            var animation = new DoubleAnimation
            {
                To = newValue,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
            };

            if (smoothProgressBar.UseAnimations)
            {
                Storyboard.SetTarget(animation, smoothProgressBar.Progress);
                Storyboard.SetTargetProperty(animation, new PropertyPath(RangeBase.ValueProperty));
                var sb = new Storyboard();
                sb.Children.Add(animation);
                if ((double)e.NewValue <= 0.0 || (double)e.NewValue >= 1.0)
                {
                    sb.Completed += (sender, args) =>
                    {
                        smoothProgressBar.Visibility = Visibility.Collapsed;
                    };
                }

                if (smoothProgressBar._previousStoryboard != null)
                {
                    smoothProgressBar._previousStoryboard.Stop();
                }
                smoothProgressBar._previousStoryboard = sb;

                sb.Begin();
            }
            else
            {
                smoothProgressBar.Value = newValue;
                if ((double)e.NewValue <= 0.0 || (double)e.NewValue >= 1.0)
                {
                    smoothProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        public double Value
        {
            get { return (double) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public SmoothProgressBar()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;
        }
    }
}
