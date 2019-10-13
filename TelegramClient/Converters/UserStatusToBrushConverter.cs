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
    public class UserStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userBase = value as TLUserBase;
            if (userBase == null) return Application.Current.Resources["PhoneSubtleBrush"];

            var user = userBase as TLUser;
            if (user != null)
            {
                if (user.IsBot)
                {
                    return Application.Current.Resources["PhoneSubtleBrush"];
                }
            }

            var statusOnline = userBase.Status as TLUserStatusOnline;
            if (statusOnline != null) return Application.Current.Resources["TelegramAccentBrush"];

            return Application.Current.Resources["PhoneSubtleBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
