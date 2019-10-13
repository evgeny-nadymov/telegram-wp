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
using Telegram.Api.TL;

namespace TelegramClient.Converters
{
    public class FileSizeConverter : IValueConverter
    {
        public static string Convert(long bytesCount)
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
            if (value is long)
            {
                return Convert((long) value);
            }

            if (value is int)
            {
                return Convert((int) value);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageViewsConverter : IValueConverter
    {
        public static string Convert(long viewsCount)
        {
#if DEBUG
            return string.Format("{0}", viewsCount);
#endif

            if (viewsCount < 1000)
            {
                return string.Format("{0}", viewsCount);
            }

            if (viewsCount < 1000 * 1000)
            {
                return string.Format("{0}K", ((double)viewsCount / 1000).ToString("0.0", CultureInfo.InvariantCulture));
            }

            if (viewsCount < 1000 * 1000 * 1000)
            {
                return string.Format("{0}M", ((double)viewsCount / 1000 / 1000).ToString("0.0", CultureInfo.InvariantCulture));
            }

            return string.Format("{0}B", ((double)viewsCount / 1000 / 1000 / 1000).ToString("0.0", CultureInfo.InvariantCulture));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TLInt)
            {
                return Convert(((TLInt)value).Value);
            }

            if (value is TLLong)
            {
                return Convert(((TLLong)value).Value);
            }

            if (value is long)
            {
                return Convert((long)value);
            }

            if (value is int)
            {
                return Convert((int)value);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
