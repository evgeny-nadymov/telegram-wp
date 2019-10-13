// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TelegramClient.Converters
{
    public class MaskConverter : DependencyObject, IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var str = value.ToString();

            var mask = "●";

            if (parameter != null)
            {
                mask = parameter.ToString();
            }

            var builder = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                builder.Append(mask);
            }

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}