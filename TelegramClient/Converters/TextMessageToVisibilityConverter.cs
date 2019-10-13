// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Data;
using Telegram.Api.TL;

namespace TelegramClient.Converters
{
    public class TextMessageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as TLMessage;
            if (message == null) return Visibility.Collapsed;

            if (TLString.IsNullOrEmpty(message.Message)) return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaFileAvailableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as TLMessage;
            if (message == null) return Visibility.Collapsed;


#if WP8
            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var file = mediaDocument.File;

                if (file == null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document != null)
                    {
                        var localFileName = document.GetFileName() ?? string.Empty;
                        var globalFileName = mediaDocument.IsoFileName ?? string.Empty;
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
                        if (store.FileExists(localFileName) || store.FileExists(globalFileName) 
#if WP81
                            || File.Exists(globalFileName)
#endif
                            )
                        {
                            return Visibility.Visible;
                        }
                    }
                }

                return file != null? Visibility.Visible : Visibility.Collapsed;
            }

            var mediaVideo = message.Media as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                var file = mediaVideo.File;

                if (file == null)
                {
                    var video = mediaVideo.Video as TLVideo;
                    if (video != null)
                    {
                        var localFileName = video.GetFileName() ?? string.Empty;
                        var globalFileName = mediaVideo.IsoFileName ?? string.Empty;
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
                        if (store.FileExists(localFileName) || store.FileExists(globalFileName) 
#if WP81
                            || File.Exists(globalFileName)
#endif
                            )
                        {
                            return Visibility.Visible;
                        }
                    }
                }

                return file != null ? Visibility.Visible : Visibility.Collapsed;
            }
#endif

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
