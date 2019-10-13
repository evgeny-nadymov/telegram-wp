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
    public class PrivateBetaToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if PRIVATE_BETA
            return Visibility.Visible;
#else
            return Visibility.Collapsed;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PrivateBetaIdentityToVisibilityConverter : IValueConverter
    {
        public static bool IsPrivateBeta
        {
            get
            {
#if WP8
                return Windows.ApplicationModel.Package.Current.Id.Name == "TelegramMessengerLLP.TelegramMessengerPreview";
#else
#if DEBUG
                return true;
#else
                return false;
#endif
#endif
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsPrivateBeta ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LogVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Telegram.Logs.Log.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
