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
    public class StringFormatConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty StringProperty =
            DependencyProperty.Register("String", typeof (string), typeof (StringFormatConverter), new PropertyMetadata(default(string)));

        public string String
        {
            get { return (string) GetValue(StringProperty); }
            set { SetValue(StringProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(String, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
