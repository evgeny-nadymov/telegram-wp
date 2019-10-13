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
using System.Windows.Media.Imaging;

namespace TelegramClient.Views.Media
{
    public partial class ColorPicker
    {
        public Color SelectedColor { get; set; }

        public event EventHandler<ColorEventArgs> SelectedColorChanged;

        protected virtual void RaiseSelectedColorChanged(ColorEventArgs e)
        {
            var handler = SelectedColorChanged;
            if (handler != null) handler(this, e);
        }

        private WriteableBitmap _bitmap;

        public ColorPicker()
        {
            InitializeComponent();

            SelectedColor = ((SolidColorBrush) PickerBorder.Fill).Color;

            Loaded += OnColorPickerLoaded;
        }

        private void OnColorPickerLoaded(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
            {
                _bitmap = new WriteableBitmap(GradientBorder, null);
            }
        }

        private double _minScale = 0.2;

        private double _maxScale = 0.95;

        public double CurrentScale
        {
            get { return PickerBorderScale.ScaleX; }
        }

        private void UIElement_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            TranslateTransform.TranslateX += e.DeltaManipulation.Translation.X * TranslateTransform.ScaleX;
            TranslateTransform.TranslateY += e.DeltaManipulation.Translation.Y * TranslateTransform.ScaleY;
            if (TranslateTransform.TranslateX < 0.0) TranslateTransform.TranslateX = 0.0;
            else if (TranslateTransform.TranslateX > ActualWidth - 2) TranslateTransform.TranslateX = ActualWidth - 2;

            //if (TranslateTransform.TranslateY > -75.0) TranslateTransform.TranslateY = -75.0;

            var minDisplacement = -100;
            if (TranslateTransform.TranslateY < minDisplacement)
            {
                var percent = Math.Abs((-TranslateTransform.TranslateY + minDisplacement) / 300.0);
                var scale = (_maxScale - _minScale) * percent + _minScale;
                System.Diagnostics.Debug.WriteLine("y={0} percent={1} t={2}", TranslateTransform.TranslateY, percent, scale);

                if (scale < _minScale) scale = _minScale;
                else if (scale > _maxScale) scale = _maxScale;
                PickerBorderScale.ScaleX = scale;
                PickerBorderScale.ScaleY = scale;
            }

            if (TranslateTransform.TranslateX > ActualWidth - 10)
            {
                PickerBorder.StrokeThickness = 0.5 / PickerBorderScale.ScaleX;
            }
            else
            {
                PickerBorder.StrokeThickness = 0.0;
            }

            var position = TranslateTransform.TranslateX / ActualWidth;
            
            var color = GetColor(position);
            if (!color.HasValue) return;

            PickerBorder.Fill = new SolidColorBrush(color.Value);
            SelectedColor = color.Value;
            RaiseSelectedColorChanged(new ColorEventArgs { Color = color.Value, Position = position });
        }

        public void SetPosition(double position)
        {
            var x = position * ActualWidth;
            TranslateTransform.TranslateX = x;
            if (TranslateTransform.TranslateX < 0.0) TranslateTransform.TranslateX = 0.0;
            if (TranslateTransform.TranslateX > ActualWidth - 2) TranslateTransform.TranslateX = ActualWidth - 2;

            position = TranslateTransform.TranslateX / ActualWidth;

            var color = GetColor(position);
            if (!color.HasValue) return;

            PickerBorder.Fill = new SolidColorBrush(color.Value);
        }

        private Color? GetColor(double position)
        {
            if (_bitmap == null) return null;

            var colorAsInt = GetPixel(_bitmap, (int) (_bitmap.PixelWidth*position), _bitmap.PixelHeight/2);

            var color = Color.FromArgb(
                (byte) ((colorAsInt >> 0x18) & 0xff),
                (byte) ((colorAsInt >> 0x10) & 0xff),
                (byte) ((colorAsInt >> 8) & 0xff),
                (byte) (colorAsInt & 0xff));

            return color;
        }

        private int GetPixel(WriteableBitmap bitmap, int x, int y)
        {
            if (bitmap == null) return 0;

            return bitmap.Pixels[y * _bitmap.PixelWidth + x];
        }

        private void PickerBorder_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var storyboard = new Storyboard();

            var toScaleX = 1.0;
            var scaleXAnimation = new DoubleAnimationUsingKeyFrames();
            scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toScaleX, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(scaleXAnimation, TranslateTransform);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleXAnimation);

            var toScaleY = 1.0;
            var scaleYAnimation = new DoubleAnimationUsingKeyFrames();
            scaleYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toScaleY, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(scaleYAnimation, TranslateTransform);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
            storyboard.Children.Add(scaleYAnimation);

            var toTranslateY = 0.0;
            var translateYAnimation = new DoubleAnimationUsingKeyFrames();
            translateYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toTranslateY, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(translateYAnimation, TranslateTransform);
            Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath("TranslateY"));
            storyboard.Children.Add(translateYAnimation);

            //var toBorderThickness = 8.0;
            //var borderThicknessAnimation = new DoubleAnimationUsingKeyFrames();
            //borderThicknessAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toBorderThickness, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            //Storyboard.SetTarget(borderThicknessAnimation, PickerBorder2);
            //Storyboard.SetTargetProperty(borderThicknessAnimation, new PropertyPath("StrokeThickness"));
            //storyboard.Children.Add(borderThicknessAnimation);

            storyboard.Begin();
        }

        private void PickerBorder_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            var storyboard = new Storyboard();

            var toScaleX = 2.0;
            var scaleXAnimation = new DoubleAnimationUsingKeyFrames();
            scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toScaleX, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(scaleXAnimation, TranslateTransform);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleXAnimation);

            var toScaleY = 2.0;
            var scaleYAnimation = new DoubleAnimationUsingKeyFrames();
            scaleYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toScaleY, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(scaleYAnimation, TranslateTransform);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
            storyboard.Children.Add(scaleYAnimation);

            var toTranslateY = -75.0;
            var translateYAnimation = new DoubleAnimationUsingKeyFrames();
            translateYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toTranslateY, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(translateYAnimation, TranslateTransform);
            Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath("TranslateY"));
            storyboard.Children.Add(translateYAnimation);

            //var toBorderThickness = 2.0;
            //var borderThicknessAnimation = new DoubleAnimationUsingKeyFrames();
            //borderThicknessAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toBorderThickness, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            //Storyboard.SetTarget(borderThicknessAnimation, PickerBorder2);
            //Storyboard.SetTargetProperty(borderThicknessAnimation, new PropertyPath("StrokeThickness"));
            //storyboard.Children.Add(borderThicknessAnimation);

            storyboard.Begin();
        }
    }

    public class ColorEventArgs : System.EventArgs
    {
        public Color Color { get; set; }

        public double Position { get; set; }
    }
}
