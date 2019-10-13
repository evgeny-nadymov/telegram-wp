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
using TelegramClient.Resources;

namespace TelegramClient.Converters
{
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mediaDocument = value as TLMessageMediaDocument;
            if (mediaDocument == null)
            {
                return null;
            }

            var document = mediaDocument.Document as TLDocument;
            if (document == null)
            {
                return null;
            }

            var fileName = document.FileName.ToString();
            if (string.IsNullOrEmpty(fileName))
            {
                return AppResources.Document.ToLowerInvariant();
            }

            return fileName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
