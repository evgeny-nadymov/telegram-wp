// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TelegramClient.Converters
{
    public class OverlayAccentBrushConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register("AccentColor", typeof (Color), typeof (OverlayAccentBrushConverter), new PropertyMetadata(default(Color)));

        public Color AccentColor
        {
            get { return (Color) GetValue(AccentColorProperty); }
            set { SetValue(AccentColorProperty, value); }
        }

        private Color? _prevColor;

        private Brush _prevBrush;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_prevColor.HasValue && _prevColor.Value != AccentColor)
            {
                _prevBrush = null;
            }

            if (_prevBrush == null)
            {
                var prevMixColor = Utils.ColorUtils.MergeColors(AccentColor, Color.FromArgb(100, 0, 0, 0));
                _prevBrush = new SolidColorBrush(prevMixColor);
                _prevColor = AccentColor;
            }

            return _prevBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
