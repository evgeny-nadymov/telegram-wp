// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace TelegramClient.Views.Additional
{
    public partial class InputMessageHint
    {
        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
            "Hint", typeof (string), typeof (InputMessageHint), new PropertyMetadata(OnHintChanged));

        private static void OnHintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as InputMessageHint;
            if (control != null)
            {
                control._storyboard.Stop();
                control.HintTextBlock.Text = (string) e.NewValue;
                control._storyboard.Begin();
            }
        }

        public string Hint
        {
            get { return (string) GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        private readonly Storyboard _storyboard;

        public InputMessageHint()
        {
            InitializeComponent();

            _storyboard = new Storyboard();
            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1.0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(2.0), Value = 1.0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(2.5), Value = 0.0 });
            Storyboard.SetTarget(opacityAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            _storyboard.Children.Add(opacityAnimation);
            _storyboard.Completed += (sender, args) =>
            {
                RaiseClosed();
            };
        }

        public InputMessageHint(bool useFadeIn)
        {
            InitializeComponent();

            _storyboard = new Storyboard();
            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            if (useFadeIn)
            {
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(2.5), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(3.0), Value = 0.0 });
            }
            else
            {
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(2.0), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(2.5), Value = 0.0 });
            }
            Storyboard.SetTarget(opacityAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            _storyboard.Children.Add(opacityAnimation);
            _storyboard.Completed += (sender, args) =>
            {
                RaiseClosed();
            };
        }

        public event EventHandler Closed;

        protected virtual void RaiseClosed()
        {
            EventHandler handler = Closed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }
    }
}
