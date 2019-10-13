// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TelegramClient.Views.Controls
{
    public partial class Progress
    {
        public Brush BackgroundBrush
        {
            get { return BackgrondElement.Background; }
            set { BackgrondElement.Background = value; }
        }

        public static readonly DependencyProperty CancelVisibilityProperty = DependencyProperty.Register(
            "CancelVisibility", typeof (Visibility), typeof (Progress), new PropertyMetadata(default(Visibility), OnCancelVisibilityChanged));

        private static void OnCancelVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progress = (Progress)d;
            if (progress != null)
            {
                progress.CancelButton.Visibility = (Visibility) e.NewValue;
            }
        }

        public Visibility CancelVisibility
        {
            get { return (Visibility) GetValue(CancelVisibilityProperty); }
            set { SetValue(CancelVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(Progress), new PropertyMetadata(default(double), OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progress = (Progress)d;
            if (progress != null)
            {
                double newAngle = 0.0;
                if (e.OldValue != null
                    && e.NewValue != null
                    && (double) e.OldValue > 0.0
                    && (double) e.OldValue < 1.0
                    && (double) e.NewValue == 0.0)
                {
                    newAngle = 0.0; //359.0;
                }
                else
                {
                    newAngle = (double)e.NewValue * 359.0;
                    if (newAngle < 0.0)
                    {
                        newAngle = 0.0;
                    }
                    else if (newAngle > 359.0)
                    {
                        newAngle = 359.0;
                    }
                }

                if (newAngle != progress.Indicator.Angle)
                {
                    if (newAngle > 0.0 && newAngle < 359.0)
                    {
                        progress.Visibility = Visibility.Visible;
                    }

                    progress._angleStoryboard.Stop();
                    if (newAngle > progress.Indicator.Angle)
                    {
                        var doubleAnimation = (DoubleAnimation)progress._angleStoryboard.Children[0];
                        doubleAnimation.To = newAngle;
                        progress._angleStoryboard.Begin();
                    }
                    else
                    {
                        progress.Indicator.Angle = newAngle;
                        if (progress.Value >= 1.0 || progress.Value <= 0.0)
                        {
                            progress.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private readonly Storyboard _angleStoryboard;

        private readonly Storyboard _foreverStoryboard;

        public Progress()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;

            _foreverStoryboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(3.0)
            };
            Storyboard.SetTarget(animation, Rotation);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(RotateTransform.Angle)"));
            _foreverStoryboard.Children.Add(animation);
            _foreverStoryboard.Completed += OnForeverStoryboardCompleted;


            _angleStoryboard = new Storyboard();
            var angleAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.25),
            };
            Storyboard.SetTarget(angleAnimation, Indicator);
            Storyboard.SetTargetProperty(angleAnimation, new PropertyPath("Angle"));
            _angleStoryboard.Children.Add(angleAnimation);
            _angleStoryboard.Completed += OnAngleStoryboardCompleted;

            Loaded += (sender, args) =>
            {
                _foreverStoryboard.RepeatBehavior = RepeatBehavior.Forever;
                _foreverStoryboard.Begin();
            };
            Unloaded += (sender, args) =>
            {
                _foreverStoryboard.RepeatBehavior = new RepeatBehavior(1.0);
            };

        }

        private void OnAngleStoryboardCompleted(object sender, System.EventArgs e)
        {
            if (Value >= 1.0 || Value <= 0.0)
            {
                Visibility = Visibility.Collapsed;
                RaiseCompleted();
            }
        }

        public event EventHandler Completed;

        protected virtual void RaiseCompleted()
        {
            var handler = Completed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void OnForeverStoryboardCompleted(object sender, System.EventArgs e)
        {


        }

        private void Border_OnTap(object sender, GestureEventArgs e)
        {
            if (Value > 0.0 && Value < 1.0)
            {
                RaiseCancel(e);
                Visibility = Visibility.Collapsed;
            }
        }

        public event EventHandler<GestureEventArgs> Cancel;

        protected virtual void RaiseCancel(GestureEventArgs args)
        {
            var handler = Cancel;
            if (handler != null) handler(this, args);
        }
    }

    public class ProgressPieSlice : Path
    {
        private bool m_HasLoaded = false;

        public ProgressPieSlice()
        {
            Loaded += (s, e) =>
            {
                m_HasLoaded = true;
                UpdatePath();
            };
        }

        // StartAngle
        public static readonly DependencyProperty StartAngleProperty
            = DependencyProperty.Register("StartAngle", typeof(double), typeof(ProgressPieSlice),
            new PropertyMetadata(DependencyProperty.UnsetValue, (s, e) => { Changed(s as ProgressPieSlice); }));
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        // Angle
        public static readonly DependencyProperty AngleProperty
            = DependencyProperty.Register("Angle", typeof(double), typeof(ProgressPieSlice),
            new PropertyMetadata(DependencyProperty.UnsetValue, (s, e) => { Changed(s as ProgressPieSlice); }));
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // Radius
        public static readonly DependencyProperty RadiusProperty
            = DependencyProperty.Register("Radius", typeof(double), typeof(ProgressPieSlice),
            new PropertyMetadata(DependencyProperty.UnsetValue, (s, e) => { Changed(s as ProgressPieSlice); }));
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        private static void Changed(ProgressPieSlice pieSlice)
        {
            if (pieSlice.m_HasLoaded)
                pieSlice.UpdatePath();
        }

        public void UpdatePath()
        {
            // ensure variables
            if (GetValue(StartAngleProperty) == DependencyProperty.UnsetValue)
                throw new ArgumentNullException("Start Angle is required");
            if (GetValue(RadiusProperty) == DependencyProperty.UnsetValue)
                throw new ArgumentNullException("Radius is required");
            if (GetValue(AngleProperty) == DependencyProperty.UnsetValue)
                throw new ArgumentNullException("Angle is required");

            Width = Height = 2 * Radius + StrokeThickness;
            var endAngle = StartAngle + Angle;

            // path container
            var figure = new PathFigure
            {
                StartPoint = new Point(Radius + StrokeThickness / 2.0, 0 + StrokeThickness / 2.0),
                IsClosed = false,
            };

            // outer arc
            var arcX = Radius + Math.Sin(endAngle * Math.PI / 180) * Radius + StrokeThickness / 2.0;
            var arcY = Radius - Math.Cos(endAngle * Math.PI / 180) * Radius + StrokeThickness / 2.0;
            var arcSize = new Size(Radius, Radius);
            var arcPoint = new Point(arcX, arcY);
            var arc = new ArcSegment
            {
                IsLargeArc = Angle >= 180.0,
                Point = arcPoint,
                Size = arcSize,
                SweepDirection = SweepDirection.Clockwise,
            };
            figure.Segments.Add(arc);

            // finalé
            Data = new PathGeometry { Figures = { figure } };

            InvalidateArrange();
        }
    }
}
