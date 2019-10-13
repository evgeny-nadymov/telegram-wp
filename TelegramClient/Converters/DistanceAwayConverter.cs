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
using TelegramClient.Resources;

namespace TelegramClient.Converters
{
    public class DistanceAwayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double)) return value;

            var distance = (double) value;

            if (distance < 1000)
            {
                return string.Format(AppResources.DistanceAway, (int) distance + AppResources.MetersShort);
            }

            return string.Format(AppResources.DistanceAway, String.Format("{0:0.#}", distance / 1000) + AppResources.KilometersShort);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
