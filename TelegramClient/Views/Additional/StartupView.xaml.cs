using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TelegramClient.Views.Additional
{
    public partial class StartupView
    {
        private const double ScreenWidth = 480.0;

        private const double MarginWidth = 24.0;

        private const double ScrollIndicatorWidth = 76.0;

        private const int LastIndex = 5;

        private const double ScrollIndicatorTranslationX = (ScreenWidth - MarginWidth - MarginWidth - ScrollIndicatorWidth) / LastIndex;

        public StartupView()
        {
            InitializeComponent();

            //Application.Current.Host.Content.ActualWidth
        }

        private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = StartView.SelectedIndex;

            var storyboard = new Storyboard();

            var timeline2 = new DoubleAnimationUsingKeyFrames();
            timeline2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = index * ScrollIndicatorTranslationX, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(timeline2, BorderPosition);
            Storyboard.SetTargetProperty(timeline2, new PropertyPath("X"));
            storyboard.Children.Add(timeline2);

            storyboard.Begin();
        }
    }
}