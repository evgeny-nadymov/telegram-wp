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
using Caliburn.Micro;
using TelegramClient.Services;

namespace TelegramClient.Converters
{
    public class IdToPlaceholderBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long index;
            if (value is int)
            {
                index = (int)value;
            }
            else if (value is long)
            {
                index = (long)value;
            }
            else
            {
                return Application.Current.Resources["PhoneChromeBrush2"];
            }

            if (index == 0)
            {
                return Application.Current.Resources["PhoneChromeBrush2"];
            }
            if (index == -1)
            {
                return Application.Current.Resources["PhoneChromeBrush2"];
            }
            if (index == -2)
            {
                return Application.Current.Resources["PhoneAccentBrush"];
            }

            var currentUserId = IoC.Get<IStateService>().CurrentUserId;

            var number = Math.Abs(MD5Core.GetHash(string.Format("{0}{1}", value, currentUserId))[Math.Abs(index % 16)]) % 8;
            //switch (number)
            //{
            //    case 0:
            //        return Application.Current.Resources["Placeholder0Brush"];
            //    case 1:
            //        return Application.Current.Resources["Placeholder1Brush"];
            //    case 2:
            //        return Application.Current.Resources["Placeholder2Brush"];
            //    case 3:
            //        return Application.Current.Resources["Placeholder3Brush"];
            //    case 4:
            //        return Application.Current.Resources["Placeholder4Brush"];
            //    case 5:
            //        return Application.Current.Resources["Placeholder5Brush"];
            //}
            //var number = (int) value % 8;
            switch (number)
            {
                case 0:
                    return Application.Current.Resources["BlueBrush"];
                case 1:
                    return Application.Current.Resources["CyanBrush"];
                case 2:
                    return Application.Current.Resources["GreenBrush"];
                case 3:
                    return Application.Current.Resources["OrangeBrush"];
                case 4:
                    return Application.Current.Resources["PinkBrush"];
                case 5:
                    return Application.Current.Resources["PurpleBrush"];
                case 6:
                    return Application.Current.Resources["RedBrush"];
                case 7:
                    return Application.Current.Resources["YellowBrush"];
            }

            return Application.Current.Resources["PhoneChromeBrush2"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
