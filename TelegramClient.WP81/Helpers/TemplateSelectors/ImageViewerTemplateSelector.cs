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

namespace TelegramClient.Helpers.TemplateSelectors
{
    public class ImageViewerTemplateSelector : IValueConverter
    {
        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as TLMessage;
            if (message != null)
            {
                if (message.Media is TLMessageMediaVideo || message.IsVideo())
                {
                    return VideoTemplate;
                }

                return PhotoTemplate;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DecryptedImageViewerTemplateSelector : IValueConverter
    {
        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as TLDecryptedMessage;
            if (message != null)
            {
                if (message.Media is TLDecryptedMessageMediaVideo || message.IsVideo())
                {
                    return VideoTemplate;
                }

                return PhotoTemplate;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
