// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define EXTENDED_LENGTH
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace TelegramClient.Views.Controls
{
    public partial class RibbonImageControl
    {
        public static readonly DependencyProperty PhotoWidthProperty = DependencyProperty.Register(
            "PhotoWidth", typeof(double), typeof(RibbonImageControl), new PropertyMetadata(default(double), OnPhotoWidthChanged));

        private static void OnPhotoWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RibbonImageControl;
            if (control != null)
            {
                control.SetPhotoDimenstion((double) e.NewValue, control.PhotoHeight);
            }
        }

        private void SetPhotoDimenstion(double width, double height)
        {
            if (height > 0.0 && width > 0.0)
            {
                var minWidth = _minExpandedWidth;
                var maxWidth = 2 * _minExpandedWidth;
                Photo.Width = _height / height * width;

                if (Photo.Width < minWidth) Photo.Width = minWidth;
                if (Photo.Width > maxWidth) Photo.Width = maxWidth;
            }
        }

        public double PhotoWidth
        {
            get { return (double) GetValue(PhotoWidthProperty); }
            set { SetValue(PhotoWidthProperty, value); }
        }

        public static readonly DependencyProperty PhotoHeightProperty = DependencyProperty.Register(
            "PhotoHeight", typeof(double), typeof(RibbonImageControl), new PropertyMetadata(default(double), OnPhotoHeightChanged));

        private static void OnPhotoHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RibbonImageControl;
            if (control != null)
            {
                control.SetPhotoDimenstion(control.PhotoWidth, (double) e.NewValue);
            }
        }

        public double PhotoHeight
        {
            get { return (double) GetValue(PhotoHeightProperty); }
            set { SetValue(PhotoHeightProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(RibbonImageControl), new PropertyMetadata(default(ImageSource), OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RibbonImageControl;
            if (control != null)
            {
                control.Photo.Source = e.NewValue as ImageSource;

                var bitmapImage = control.Photo.Source as BitmapImage;
                if (bitmapImage != null && (bitmapImage.PixelHeight > 0 || bitmapImage.PixelWidth > 0))
                {
                    control.Photo.Opacity = 1.0;
                }
                else
                {
                    control.Photo.Opacity = 0.0;
                }
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public RibbonImageControl()
        {
            InitializeComponent();

            Photo.Width = MinExpandedWidth;
            Photo.Height = _height;
        }

        private void Photo_OnImageOpened(object sender, RoutedEventArgs e)
        {
            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 1.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(.25);

            Storyboard.SetTarget(opacityAnimation, Photo);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(opacityAnimation);

            storyboard.Begin();
        }

        private double _height = 60.0;

        private static double _minExpandedWidth = 40.0;

        public static double MinExpandedWidth
        {
            get { return _minExpandedWidth; }
        }

        public double ExpandedWidth
        {
            get
            {
#if EXTENDED_LENGTH
                var minWidth = MinExpandedWidth;
                var maxWidth = MaxExpandedWidth;
                return Photo.Width < minWidth ? minWidth : (Photo.Width > maxWidth) ? maxWidth : Photo.Width;
#else
                return MinExpandedWidth;
#endif
            }
        }

        public static double MaxExpandedWidth
        {
            get { return _minExpandedWidth * 2.0; }
        }

        public void Prepare()
        {
            if (Width > MinExpandedWidth)
            {
                Width = MinExpandedWidth;
            }
        }
    }
}
