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
using Telegram.Api;
using Telegram.Api.TL;

namespace TelegramClient.Helpers.TemplateSelectors
{
    public class DocumentTemplateSelector : IValueConverter
    {
        public DataTemplate ThumbDocumentTemplate { get; set; }

        public DataTemplate DocumentTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mediaDocument = value as TLMessageMediaDocument;
            if (mediaDocument == null)
            {
                var decryptedMediaDocument = value as TLDecryptedMessageMediaDocument;
                if (decryptedMediaDocument != null)
                {
                    if (decryptedMediaDocument.Thumb.Data.Length == 0
                        || (decryptedMediaDocument.ThumbH.Value == 0 && decryptedMediaDocument.ThumbW.Value == 0))
                    {
                        return DocumentTemplate;
                    }

                    return ThumbDocumentTemplate;
                }

                return null;
            }

            var document = mediaDocument.Document as TLDocument;
            if (document != null)
            {
                var emptyThumb = document.Thumb as TLPhotoSizeEmpty;
                if (emptyThumb != null)
                {
                    return DocumentTemplate;
                }

                if (string.Equals(document.MimeType.ToString(), "image/webp", StringComparison.OrdinalIgnoreCase))
                {
                    return DocumentTemplate;
                }

                return ThumbDocumentTemplate;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LinkTemplateSelector : IValueConverter
    {
        public DataTemplate LinkTemplate { get; set; }

        public DataTemplate EmptyLinkTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageBase = value as TLMessage;
            if (messageBase != null)
            {
                var mediaWebPage = messageBase.Media as TLMessageMediaWebPage;
                if (mediaWebPage != null)
                {
                    var webPage = mediaWebPage.WebPage as TLWebPage;
                    if (webPage != null)
                    {
                        return LinkTemplate;
                    }

                    var webPagePending = mediaWebPage.WebPage as TLWebPagePending;
                    if (webPagePending != null)
                    {
                        return EmptyLinkTemplate;
                    }
                }
            }

            return EmptyLinkTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
