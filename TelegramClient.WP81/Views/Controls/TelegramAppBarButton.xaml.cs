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
    public partial class TelegramAppBarButton
    {
        public static readonly DependencyProperty LabelForegroundProperty = DependencyProperty.Register(
            "LabelForeground", typeof(Brush), typeof(TelegramAppBarButton), new PropertyMetadata(default(Brush), OnLabelForegroundChanged));

        private static void OnLabelForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramApplicationBarButton = d as TelegramAppBarButton;
            if (telegramApplicationBarButton != null)
            {
                telegramApplicationBarButton.LabelBlock.Foreground = (Brush)e.NewValue;
                telegramApplicationBarButton.Button.Background = (Brush)e.NewValue;
            }
        }

        public Brush LabelForeground
        {
            get { return (Brush) GetValue(LabelForegroundProperty); }
            set { SetValue(LabelForegroundProperty, value); }
        }

        private static void SetDefaultValues()
        {
            var applicationBar = TelegramApplicationBar.ApplicationBar;
            if (applicationBar.DefaultSize < TelegramApplicationBar.ApplicaitonBarDefaultSize1X)
            {
                var scaleFactor = applicationBar.DefaultSize / TelegramApplicationBar.ApplicaitonBarDefaultSize1X;
                _iconMargin = 20.0 * scaleFactor;
                _iconSize = applicationBar.DefaultSize - 2.0 * _iconMargin;
                _iconLabelFontSize = 18.0 * scaleFactor;
            }
            else
            {
                _iconSize = 32.0;
                _iconMargin = 20.0;
                _iconLabelFontSize = 18.0;
            }
        }

        private static double? _iconLabelFontSize;

        public static double IconLabelFontSize
        {
            get
            {
                if (_iconLabelFontSize == null)
                {
                    SetDefaultValues();
                }

                return _iconLabelFontSize.Value;
            }
        }

        private static double? _iconSize;

        public static double IconSize
        {
            get
            {
                if (_iconSize == null)
                {
                    SetDefaultValues();
                }

                return _iconSize.Value;
            }
        }

        private static double? _iconMargin;

        public static double IconMargin
        {
            get
            {
                if (_iconMargin == null)
                {
                    SetDefaultValues();
                }

                return _iconMargin.Value;
            }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof (ImageSource), typeof (TelegramAppBarButton), new PropertyMetadata(default(ImageSource), OnImageSourceChanged));

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramApplicationBarButton = d as TelegramAppBarButton;
            if (telegramApplicationBarButton != null)
            {
                telegramApplicationBarButton.ImageBrush.ImageSource = e.NewValue as ImageSource;
            }
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (TelegramAppBarButton), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramApplicationBarButton = d as TelegramAppBarButton;
            if (telegramApplicationBarButton != null)
            {
                telegramApplicationBarButton.LabelBlock.Text = e.NewValue as string;
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TelegramAppBarButton()
        {
            InitializeComponent();

            Button.Margin = new Thickness(20.0, IconMargin, 20.0, IconMargin);
            Button.Width = IconSize;
            Button.Height = IconSize;

            LabelBlock.FontSize = IconLabelFontSize;

            IsEnabledChanged += OnIsEnabledChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            if (!IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", false);
                LayoutRoot.IsHitTestVisible = false;
            }
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, (bool) e.NewValue ? "Normal" : "Disabled", false);
            LayoutRoot.IsHitTestVisible = (bool) e.NewValue;
        }

        public void HideLabel()
        {
            VisualStateManager.GoToState(this, "Closed", false);
        }

        public void ShowLabel()
        {
            VisualStateManager.GoToState(this, "Opened", false);
        }
    }
}
