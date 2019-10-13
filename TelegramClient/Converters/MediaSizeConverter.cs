// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows.Data;

namespace TelegramClient.Converters
{
    public class MediaSizeConverter : IValueConverter
    {
        public static string Convert(int bytesCount)
        {
            if (bytesCount < 1024)
            {
                return string.Format("{0} B", bytesCount);
            }

            if (bytesCount < 1024 * 1024)
            {
                return string.Format("{0} KB", ((double)bytesCount / 1024).ToString("0.0", CultureInfo.InvariantCulture));
            }

            if (bytesCount < 1024 * 1024 * 1024)
            {
                return string.Format("{0} MB", ((double)bytesCount / 1024 / 1024).ToString("0.0", CultureInfo.InvariantCulture));
            }

            return string.Format("{0} GB", ((double)bytesCount / 1024 / 1024 / 1024).ToString("0.0", CultureInfo.InvariantCulture));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int)) return string.Empty;

            var bytesCount = (int)value;

            return Convert(bytesCount);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
