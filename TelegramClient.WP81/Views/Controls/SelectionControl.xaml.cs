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

namespace TelegramClient.Views.Controls
{
    public partial class SelectionControl
    {
        public static readonly DependencyProperty SelectedBorderBrushProperty = DependencyProperty.Register(
            "SelectedBorderBrush", typeof(Brush), typeof(SelectionControl), new PropertyMetadata(default(Brush), OnSelectedBorderBrushChanged));

        public Brush SelectedBorderBrush
        {
            get { return (Brush)GetValue(SelectedBorderBrushProperty); }
            set { SetValue(SelectedBorderBrushProperty, value); }
        }

        private static void OnSelectedBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SelectionControl;
            if (control != null)
            {
                if (control.IsSelected)
                {
                    control.Border.BorderBrush = e.NewValue as Brush;
                }
            }
        }

        public static readonly DependencyProperty UnselectedBorderBrushProperty = DependencyProperty.Register(
            "UnselectedBorderBrush", typeof(Brush), typeof(SelectionControl), new PropertyMetadata(default(Brush), OnUnselectedBorderBrushChanged));

        public Brush UnselectedBorderBrush
        {
            get { return (Brush)GetValue(UnselectedBorderBrushProperty); }
            set { SetValue(UnselectedBorderBrushProperty, value); }
        }

        private static void OnUnselectedBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SelectionControl;
            if (control != null)
            {
                if (!control.IsSelected)
                {
                    control.Border.BorderBrush = e.NewValue as Brush;
                }
            }
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
            "Index", typeof(int), typeof(SelectionControl), new PropertyMetadata(default(int), OnIndexChanged));

        private static void OnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SelectionControl;
            if (control != null)
            {
                var index = (int) e.NewValue;

                control.Label.Text = index.ToString();
                control.Label.Visibility = index > 0 ? Visibility.Visible : Visibility.Collapsed;
                control.Path.Visibility = Visibility.Collapsed;
            }
        }

        public int Index
        {
            get { return (int) GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof (bool), typeof (SelectionControl), new PropertyMetadata(default(bool), OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selectionControl = d as SelectionControl;
            if (selectionControl != null)
            {
                var isSelected = (bool) e.NewValue;

                if (!selectionControl.SuppressAnimation)
                {
                    var toValue = !isSelected ? 0.75 : 1.25;
                    var easingFunction = new ExponentialEase { Exponent = 2.0, EasingMode = EasingMode.EaseOut };
                    var storyboard = new Storyboard();
                    var scaleXAnimation = new DoubleAnimationUsingKeyFrames();
                    scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = toValue, EasingFunction = easingFunction });
                    scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 1.0, EasingFunction = easingFunction });
                    Storyboard.SetTarget(scaleXAnimation, selectionControl.ScaleTransform);
                    Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("ScaleX"));
                    storyboard.Children.Add(scaleXAnimation);
                    var scaleYAnimation = new DoubleAnimationUsingKeyFrames();
                    scaleYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = toValue, EasingFunction = easingFunction });
                    scaleYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 1.0, EasingFunction = easingFunction });
                    Storyboard.SetTarget(scaleYAnimation, selectionControl.ScaleTransform);
                    Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
                    storyboard.Children.Add(scaleYAnimation);

                    storyboard.Begin();
                }

                selectionControl.Border.BorderBrush = isSelected
                    ? selectionControl.SelectedBorderBrush ?? selectionControl.Border.BorderBrush
                    : selectionControl.UnselectedBorderBrush ?? selectionControl.Border.BorderBrush;

                selectionControl.Border.Background = isSelected
                    ? (Brush) selectionControl.Resources["CustomAccentBrush"]
                    : null;
                selectionControl.Path.Visibility = isSelected
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty SuppressAnimationProperty = DependencyProperty.Register(
            "SuppressAnimation", typeof (bool), typeof (SelectionControl), new PropertyMetadata(default(bool)));

        public bool SuppressAnimation
        {
            get { return (bool) GetValue(SuppressAnimationProperty); }
            set { SetValue(SuppressAnimationProperty, value); }
        }

        public SelectionControl()
        {
            InitializeComponent();

            Border.Background = null;
            Path.Visibility = Visibility.Collapsed;
        }
    }
}
