// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Media;

namespace TelegramClient.Views.Controls
{
    public partial class OpacityMaskBorder
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof (ImageSource), typeof (OpacityMaskBorder), new PropertyMetadata(default(ImageSource), OnImageSourceChanged));

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var opacityMaskBorder = d as OpacityMaskBorder;
            if (opacityMaskBorder != null)
            {
                opacityMaskBorder.ImageBrush.ImageSource = e.NewValue as ImageSource;
            }
        }

        public static readonly DependencyProperty BorderBackgroundProperty = DependencyProperty.Register(
            "BorderBackground", typeof (Brush), typeof (OpacityMaskBorder), new PropertyMetadata(default(Brush), OnBorderBackgroundChanged));

        private static void OnBorderBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var opacityMaskBorder = d as OpacityMaskBorder;
            if (opacityMaskBorder != null)
            {
                opacityMaskBorder.Border.Background = e.NewValue as Brush;
            }
        }

        public Brush BorderBackground
        {
            get { return (Brush) GetValue(BorderBackgroundProperty); }
            set { SetValue(BorderBackgroundProperty, value); }
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public OpacityMaskBorder()
        {
            InitializeComponent();
        }

        private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
            var opacityMaskBorder = sender as OpacityMaskBorder;
            if (opacityMaskBorder != null)
            {
                opacityMaskBorder.Border.Opacity = 1.0;
            }
        }

        private void ImageBrush_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            
        }
    }
}
