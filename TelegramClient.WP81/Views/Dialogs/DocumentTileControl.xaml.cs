// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Media;

namespace TelegramClient.Views.Dialogs
{
    public partial class DocumentTileControl
    {
        public static readonly DependencyProperty TileBrushProperty = DependencyProperty.Register(
            "TileBrush", typeof (Brush), typeof (DocumentTileControl), new PropertyMetadata(default(Brush), OnTileBrushChanged));

        private static void OnTileBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var documentTileControl = d as DocumentTileControl;
            if (documentTileControl != null)
            {
                documentTileControl.Ellipse.Fill = e.NewValue as Brush;
            }
        }

        public Brush TileBrush
        {
            get { return (Brush) GetValue(TileBrushProperty); }
            set { SetValue(TileBrushProperty, value); }
        }

        public static readonly DependencyProperty MusicProperty = DependencyProperty.Register(
            "Music", typeof (bool), typeof (DocumentTileControl), new PropertyMetadata(default(bool), OnMusicChanged));

        private static void OnMusicChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var documentTileControl = d as DocumentTileControl;
            if (documentTileControl != null)
            {
                var music = (bool) e.NewValue;

                documentTileControl.MusicIcon.Visibility = music ? Visibility.Visible : Visibility.Collapsed;
                documentTileControl.DocumentIcon.Visibility = music ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool Music
        {
            get { return (bool) GetValue(MusicProperty); }
            set { SetValue(MusicProperty, value); }
        }

        public static readonly DependencyProperty DownloadIconVisibilityProperty = DependencyProperty.Register(
            "DownloadIconVisibility", typeof (Visibility), typeof (DocumentTileControl), new PropertyMetadata(Visibility.Collapsed, OnDownloadIconVisibilityChanged));

        private static void OnDownloadIconVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var documentTileControl = d as DocumentTileControl;
            if (documentTileControl != null)
            {
                var visibility = (Visibility) e.NewValue;

                documentTileControl.DownloadIcon.Visibility = visibility;
                documentTileControl.Icon.Visibility = documentTileControl.DownloadIcon.Visibility == Visibility.Collapsed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                //documentTileControl.
            }
        }

        public Visibility DownloadIconVisibility
        {
            get { return (Visibility) GetValue(DownloadIconVisibilityProperty); }
            set { SetValue(DownloadIconVisibilityProperty, value); }
        }

        public static readonly DependencyProperty UploadingProgressProperty = DependencyProperty.Register(
            "UploadingProgress", typeof (double), typeof (DocumentTileControl), new PropertyMetadata(default(double), OnProgressChanged));

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var documentTileControl = d as DocumentTileControl;
            if (documentTileControl != null)
            {
                var newValue = (double) e.NewValue;

                //System.Diagnostics.Debug.WriteLine("  progress=" + newValue);

                documentTileControl.Progress.Value = newValue;

                documentTileControl.Progress.CancelVisibility = newValue == 1.0
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                var hasImageSource = documentTileControl.Image.Source != null;
                documentTileControl.Icon.Visibility = newValue > 0.0 && newValue < 1.0 && !hasImageSource
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                documentTileControl.DownloadIcon.Visibility = newValue > 0.0 && newValue < 1.0
                    ? Visibility.Collapsed
                    : documentTileControl.DownloadIconVisibility;

                if (documentTileControl.DownloadIcon.Visibility == Visibility.Visible
                    && documentTileControl.Icon.Visibility == Visibility.Visible)
                {
                    documentTileControl.Icon.Visibility = Visibility.Collapsed;
                }

            }
        }

        public double UploadingProgress
        {
            get { return (double) GetValue(UploadingProgressProperty); }
            set { SetValue(UploadingProgressProperty, value); }
        }

        public static readonly DependencyProperty DownloadingProgressProperty = DependencyProperty.Register(
            "DownloadingProgress", typeof (double), typeof (DocumentTileControl), new PropertyMetadata(default(double), OnProgressChanged));

        public double DownloadingProgress
        {
            get { return (double) GetValue(DownloadingProgressProperty); }
            set { SetValue(DownloadingProgressProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (DocumentTileControl), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var documentTileControl = d as DocumentTileControl;
            if (documentTileControl != null)
            {
                //documentTileControl.Label.Text = e.NewValue as string ?? string.Empty;
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof (ImageSource), typeof (DocumentTileControl), new PropertyMetadata(default(ImageSource), OnImageSourceChanged));

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var documentTileControl = d as DocumentTileControl;
            if (documentTileControl != null)
            {
                var source = e.NewValue as ImageSource;
                if (source != null)
                {
                    documentTileControl.Image.Visibility = Visibility.Visible;
                    documentTileControl.Image.Source = source;
                    documentTileControl.Ellipse.Visibility = Visibility.Collapsed;
                    documentTileControl.Icon.Visibility = Visibility.Collapsed;
                    documentTileControl.DownloadEllipse.Opacity = 0.3;
                }
                else
                {
                    documentTileControl.Image.Visibility = Visibility.Visible;
                    documentTileControl.Image.Source = null;
                    documentTileControl.Ellipse.Visibility = Visibility.Visible;
                    documentTileControl.Icon.Visibility = documentTileControl.DownloadIcon.Visibility == Visibility.Collapsed
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    documentTileControl.DownloadEllipse.Opacity = 0.0;
                }
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public DocumentTileControl()
        {
            InitializeComponent();
        }
    }
}
