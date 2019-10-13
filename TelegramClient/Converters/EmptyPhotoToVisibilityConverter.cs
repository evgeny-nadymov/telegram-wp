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
using Telegram.Api.TL;

namespace TelegramClient.Converters
{
    public class EmptyChatPhotoToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isEmpty = value is TLChatPhotoEmpty;

            if (parameter != null
                && string.Equals(parameter.ToString(), "invert", StringComparison.OrdinalIgnoreCase))
            {
                isEmpty = !isEmpty;
            }

            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyUserProfilePhotoToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isEmpty = value is TLUserProfilePhotoEmpty;

            if (parameter != null
                && string.Equals(parameter.ToString(), "invert", StringComparison.OrdinalIgnoreCase))
            {
                isEmpty = !isEmpty;
            }

            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
