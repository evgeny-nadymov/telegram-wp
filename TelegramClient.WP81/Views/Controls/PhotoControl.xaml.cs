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
    public partial class PhotoControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof (ImageSource), typeof (PhotoControl), new PropertyMetadata(default(ImageSource), OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var photoControl = d as PhotoControl;
            if (photoControl != null)
            {
                var source = (ImageSource) e.NewValue;

                photoControl.ImageControl.Source = source;
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
            "Index", typeof(int), typeof(PhotoControl), new PropertyMetadata(default(int), OnIndexChanged));

        private static void OnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var photoControl = d as PhotoControl;
            if (photoControl != null)
            {
                var index = (int) e.NewValue;
                var isSelected = index > 0;

                photoControl.SelectionControl.SuppressAnimation = photoControl.File != null && photoControl.File.SuppressAnimation;
                photoControl.SelectionControl.IsSelected = isSelected;
                photoControl.SelectionControl.Index = index;

                photoControl.SelectionBorder.Visibility = isSelected
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public int Index
        {
            get { return (int) GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof (bool), typeof (PhotoControl), new PropertyMetadata(default(bool), OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var photoControl = d as PhotoControl;
            if (photoControl != null)
            {
                var isSelected = (bool) e.NewValue;

                photoControl.SelectionControl.SuppressAnimation = photoControl.File != null && photoControl.File.SuppressAnimation;
                photoControl.SelectionControl.IsSelected = isSelected;

                photoControl.SelectionBorder.Visibility = isSelected
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
            "File", typeof (PhotoFile), typeof (PhotoControl), new PropertyMetadata(default(PhotoFile), OnFileChanged));

        private static void OnFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var photoControl = d as PhotoControl;
            if (photoControl != null)
            {
                var file = e.NewValue as PhotoFile;

                photoControl.LayoutRoot.Visibility = file != null
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public PhotoFile File
        {
            get { return (PhotoFile) GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        public PhotoControl()
        {
            InitializeComponent();

            LayoutRoot.Visibility = Visibility.Collapsed;
            SelectionBorder.Visibility = Visibility.Collapsed;
            SelectionControl.IsSelected = false;
        }
    }
}
