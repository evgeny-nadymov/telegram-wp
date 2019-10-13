using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        }

        private void StartView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = StartView.SelectedIndex;

            var storyboard = new Storyboard();

            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = index * ScrollIndicatorTranslationX, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(timeline, BorderPosition);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("X"));
            storyboard.Children.Add(timeline);

            storyboard.Begin();
        }

        private void StartView_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (StartView.SelectedIndex == 0 && e.DeltaManipulation.Translation.X > 0.0 && BorderPosition.X == 0.0)
            {
                return;
            }

            if (StartView.SelectedIndex == LastIndex && e.DeltaManipulation.Translation.X < 0.0 && BorderPosition.X == ScrollIndicatorTranslationX * LastIndex)
            {
                return;
            }

            BorderPosition.X -= e.DeltaManipulation.Translation.X / ScreenWidth * ScrollIndicatorTranslationX;
        }
    }
}