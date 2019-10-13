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
    public class TTLMediaToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TLMessageBase.HasTTL(value as TLMessageMediaBase) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TTLMediaToTimerStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ttlSeconds = value as TLInt;
            if (ttlSeconds != null && ttlSeconds.Value >= 0)
            {
                if (ttlSeconds.Value >= 60 * 60)
                {
                    return string.Format("{0}h", ttlSeconds.Value / (60 * 60));
                }

                if (ttlSeconds.Value >= 60)
                {
                    return string.Format("{0}m", ttlSeconds.Value / 60);
                }

                return string.Format("{0}s", ttlSeconds.Value);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
