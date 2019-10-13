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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Shell;

namespace TelegramClient.Views.Additional
{
    public partial class StartupView
    {
        private const double ScreenWidth = 480.0;

        private const double MarginWidth = 174.0;

        private const double ScrollIndicatorWidth = 12.0;

        private const int LastIndex = 5;

        private const double ScrollIndicatorTranslationX = (ScreenWidth - MarginWidth - MarginWidth - ScrollIndicatorWidth) / LastIndex;


        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedText", typeof(string), typeof(StartupView), new PropertyMetadata(default(string), OnFormattedTextChanged));

        public static void SetFormattedText(DependencyObject element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject element)
        {
            return (string)element.GetValue(FormattedTextProperty);
        }

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var r = d as TextBlock;
            if (r != null)
            {
                var text = e.NewValue as string;
                if (text != null)
                {
                    r.Inlines.Clear();
                    var splittedText = text.Split(new[] { "**" }, StringSplitOptions.None);
                    for (var i = 0; i < splittedText.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            var bold = new Run();
                            bold.FontWeight = FontWeights.SemiBold;
                            bold.Text = splittedText[i];
                            r.Inlines.Add(bold);
                        }
                        else
                        {
                            r.Inlines.Add(splittedText[i]);
                        }
                    }
                }
            }
        }

        public StartupView()
        {
            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            IndicatorBorder.Background = isLightTheme
                ? (Brush) Resources["IndicatorBrushLight"]
                : (Brush) Resources["IndicatorBrushDark"];
            PositionBorder.Background = isLightTheme
                ? (Brush) Resources["PositionBrushLight"]
                : (Brush) Resources["PositionBrushDark"];

            OptimizeFullHD();
        }

        private void OptimizeFullHD()
        {
            return;
            var appBar = new ApplicationBar();
            var appBarDefaultSize = appBar.DefaultSize;

            StartMessagingPanel.Height = appBarDefaultSize;
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

        private void StartView_OnSelectionCanceled(object sender, System.EventArgs e)
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
            if (BorderPosition.X < 0.0) BorderPosition.X = 0.0;
            if (BorderPosition.X > ScrollIndicatorTranslationX * LastIndex) BorderPosition.X = ScrollIndicatorTranslationX * LastIndex;
        }

        private void IndicatorBorder_OnTap(object sender, GestureEventArgs e)
        {
            var selectedIndex = StartView.SelectedIndex;
            if (selectedIndex >= StartView.Items.Count - 1)
            {
                StartView.SelectedIndex = 0;
            }
            else if (StartView.SelectedIndex < StartView.Items.Count - 1)
            {
                StartView.SelectedIndex++;
            }
        }
    }
}