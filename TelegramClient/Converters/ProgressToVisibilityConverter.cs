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

namespace TelegramClient.Converters
{
    public class ProgressToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible;

            if (!(value is double)) return Visibility.Collapsed;

            var progress = (double)value;
            if (Math.Abs(progress) < 0.00001)
            {
                isVisible = false;
            }
            else
            {
                isVisible = Math.Abs(progress - 1.0) > 0.00001;              
            }

            if (parameter is string
                && string.Equals((string) parameter, "invert", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProgressBetween0and1ToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible;

            var progress = (double)value;
            isVisible = progress > 0.0 && progress < 1.0;

            if (parameter is string
                && string.Equals((string)parameter, "invert", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
