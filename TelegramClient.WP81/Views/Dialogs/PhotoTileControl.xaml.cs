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

namespace TelegramClient.Views.Dialogs
{
    public partial class PhotoTileControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof (ImageSource), typeof (PhotoTileControl), new PropertyMetadata(default(ImageSource), OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var photoTileControl = d as PhotoTileControl;
            if (photoTileControl != null)
            {
                photoTileControl.Photo.Source = e.NewValue as ImageSource;
            }
        }

        public static readonly DependencyProperty ThumbSourceProperty = DependencyProperty.Register(
            "ThumbSource", typeof (ImageSource), typeof (PhotoTileControl), new PropertyMetadata(default(ImageSource), OnThumbSourceChanged));

        private static void OnThumbSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var photoTileControl = d as PhotoTileControl;
            if (photoTileControl != null)
            {
                if (photoTileControl._lastThumbDataContext == null
                    || photoTileControl.DataContext != photoTileControl._lastThumbDataContext.Target)
                {
                    photoTileControl._lastThumbDataContext = new WeakReference(photoTileControl.DataContext);
                    photoTileControl.Photo.Source = e.NewValue as ImageSource;
                }
            }
        }

        public ImageSource ThumbSource
        {
            get { return (ImageSource) GetValue(ThumbSourceProperty); }
            set { SetValue(ThumbSourceProperty, value); }
        }

        public ImageSource Source
        {
            get { return (ImageSource) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private WeakReference _lastThumbDataContext;

        public PhotoTileControl()
        {
            InitializeComponent();
        }
    }
}
