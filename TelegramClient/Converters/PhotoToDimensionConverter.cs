// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Telegram.Api;
using Telegram.Api.TL;

namespace TelegramClient.Converters
{
    public class InvoiceToDimensionConverter : IValueConverter
    {
        public static double GetWebDocumentDimension(TLWebDocumentBase webDocument, bool isWidth)
        {
            const double width = Constants.DefaultMessageContentWidth;
            if (isWidth)
            {
                return width;
            }

            var attributes = webDocument as IAttributes;
            if (attributes == null)
            {
                return double.NaN;
            }

            return StickerToDimensionConverter.GetAttributesDimension(attributes, isWidth, width);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isWidth = (string.Equals((string)parameter, "Width", StringComparison.OrdinalIgnoreCase));

            var mediaInvoice = value as TLMessageMediaInvoice;
            if (mediaInvoice == null) return null;

            var webDocument = mediaInvoice.Photo;
            if (webDocument == null) return null;

            return GetWebDocumentDimension(webDocument, isWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GameToDimensionConverter : IValueConverter
    {
        public static double GetGameDimension(TLGame game, bool isWidth)
        {
            const double width = 311.0 - 12.0;
            if (isWidth)
            {
                return width;
            }

            var photo = game.Photo as TLPhoto;
            if (photo == null)
            {
                return double.NaN;
            }

            TLPhotoSize size = null;
            var sizes = photo.Sizes.OfType<TLPhotoSize>();
            foreach (var photoSize in sizes)
            {
                if (size == null
                    || Math.Abs(width - size.W.Value) > Math.Abs(width - photoSize.W.Value))
                {
                    size = photoSize;
                }
            }

            if (size != null)
            {
                return width / size.W.Value * size.H.Value; //* 0.75;
            }

            return double.NaN;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isWidth = (string.Equals((string) parameter, "Width", StringComparison.OrdinalIgnoreCase));

            var mediaGame = value as TLMessageMediaGame;
            if (mediaGame == null) return null;

            var game = mediaGame.Game;
            if (game == null) return null;

            return GetGameDimension(game, isWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WebPageToDimensionConverter : IValueConverter
    {
        public static double GetWebPageDimension(TLWebPage webPage, bool isWidth)
        {
            const double width = 311.0 - 12.0;
            if (isWidth)
            {
                return width;
            }

            var photo = webPage.Photo as TLPhoto;
            if (photo == null)
            {
                return double.NaN;
            }

            TLPhotoSize size = null;
            var sizes = photo.Sizes.OfType<TLPhotoSize>();
            foreach (var photoSize in sizes)
            {
                if (size == null
                    || Math.Abs(width - size.W.Value) > Math.Abs(width - photoSize.W.Value))
                {
                    size = photoSize;
                }
            }

            if (size != null)
            {
                return width / size.W.Value * size.H.Value; //* 0.75;
            }

            return double.NaN;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isWidth = (string.Equals((string) parameter, "Width", StringComparison.OrdinalIgnoreCase));

            var mediaWebPage = value as TLMessageMediaWebPage;
            if (mediaWebPage == null) return null;

            var webPage = mediaWebPage.WebPage as TLWebPage;
            if (webPage == null) return null;

            return GetWebPageDimension(webPage, isWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DocumentToDimensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const double width = Constants.DefaultMessageContentWidth - 12.0;
            if (string.Equals((string)parameter, "Width", StringComparison.OrdinalIgnoreCase))
            {
                return width;
            }

            var media = value as TLMessageMediaDocument;
            if (media == null)
            {
                return double.NaN;
            }

            var document = media.Document as TLDocument;
            if (document == null)
            {
                return double.NaN;
            }

            if (string.Equals(document.MimeType.ToString(), "image/webp", StringComparison.OrdinalIgnoreCase))
            {
                return double.NaN;
            }

            var size = document.Thumb as TLPhotoSize;
            if (size != null)
            {
                return width / size.W.Value * size.H.Value;
            }

            var cachedSize = document.Thumb as TLPhotoCachedSize;
            if (cachedSize != null)
            {
                return width / cachedSize.W.Value * cachedSize.H.Value;
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VideoToDimensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const double width = Constants.DefaultMessageContentWidth;
            if (string.Equals((string)parameter, "Width", StringComparison.OrdinalIgnoreCase))
            {
                return width;
            }

            var mediaDocument = value as TLMessageMediaDocument45;
            if (mediaDocument != null)
            {
                var video = mediaDocument.Video as TLDocument22;
                if (video == null)
                {
                    return double.NaN;
                }

                var size = video.Thumb as TLPhotoSize;
                if (size != null)
                {
                    return width / size.W.Value * size.H.Value;
                }

                var cachedSize = video.Thumb as TLPhotoCachedSize;
                if (cachedSize != null)
                {
                    return width / cachedSize.W.Value * cachedSize.H.Value;
                }
            }

            var media = value as TLMessageMediaVideo;
            if (media != null)
            {
                var video = media.Video as TLVideo;
                if (video == null)
                {
                    return double.NaN;
                }

                var size = video.Thumb as TLPhotoSize;
                if (size != null)
                {
                    return width / size.W.Value * size.H.Value;
                }

                var cachedSize = video.Thumb as TLPhotoCachedSize;
                if (cachedSize != null)
                {
                    return width / cachedSize.W.Value * cachedSize.H.Value;
                }
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PhotoToDimensionConverter : IValueConverter
    {
        private double _maxDimension = Constants.DefaultMessageContentWidth;

        public double MaxDimension
        {
            get { return _maxDimension; }
            set { _maxDimension = value; }
        }

        public bool IsScaledVerticalPhoto(double minRatio, TLInt heigth, TLInt width)
        {
            var ratio = (double)heigth.Value / width.Value;

            return ratio > minRatio;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var minVerticalRatioToScale = 1.2;
            var scale = 1.2; // must be less than minVerticalRatioToScale to avoid large square photos

            if (string.Equals((string)parameter, "Width", StringComparison.OrdinalIgnoreCase))
            {
                var decryptedMediaPhoto = value as TLDecryptedMessageMediaPhoto;
                if (decryptedMediaPhoto != null)
                {
                    if (decryptedMediaPhoto.H.Value > decryptedMediaPhoto.W.Value)
                    {
                        if (IsScaledVerticalPhoto(minVerticalRatioToScale, decryptedMediaPhoto.H, decryptedMediaPhoto.W))
                        {
                            return scale * MaxDimension / decryptedMediaPhoto.H.Value * decryptedMediaPhoto.W.Value;
                        }

                        return MaxDimension / decryptedMediaPhoto.H.Value * decryptedMediaPhoto.W.Value;
                    }

                    return MaxDimension;
                }

                var mediaPhoto = value as TLMessageMediaPhoto;
                if (mediaPhoto != null)
                {
                    value = mediaPhoto.Photo;                  
                }

                var photo = value as TLPhoto;
                if (photo != null)
                {
                    IPhotoSize size = null;
                    var sizes = photo.Sizes.OfType<IPhotoSize>();
                    foreach (var photoSize in sizes)
                    {
                        if (size == null
                            || Math.Abs(MaxDimension - size.H.Value) > Math.Abs(MaxDimension - photoSize.H.Value))
                        {
                            size = photoSize;
                        }
                    }

                    if (size != null)
                    {
                        if (size.H.Value > size.W.Value)
                        {
                            if (IsScaledVerticalPhoto(minVerticalRatioToScale, size.H, size.W))
                            {
                                return scale * MaxDimension / size.H.Value * size.W.Value;
                            }

                            return MaxDimension / size.H.Value * size.W.Value;
                        }

                        return MaxDimension;
                    }
                }

                var mediaDocument = value as TLMessageMediaDocument;
                if (mediaDocument != null)
                {
                    return new VideoToDimensionConverter().Convert(value, targetType, parameter, culture);
                }
            }

            {
                var decryptedMediaPhoto = value as TLDecryptedMessageMediaPhoto;
                if (decryptedMediaPhoto != null)
                {
                    if (decryptedMediaPhoto.H.Value > decryptedMediaPhoto.W.Value)
                    {
                        if (IsScaledVerticalPhoto(minVerticalRatioToScale, decryptedMediaPhoto.H, decryptedMediaPhoto.W))
                        {
                            return scale * MaxDimension;
                        }

                        return MaxDimension;
                    }

                    return MaxDimension / decryptedMediaPhoto.W.Value * decryptedMediaPhoto.H.Value;
                }

                var mediaPhoto = value as TLMessageMediaPhoto;
                if (mediaPhoto != null)
                {
                    value = mediaPhoto.Photo;
                }

                var photo = value as TLPhoto;
                if (photo != null)
                {
                    IPhotoSize size = null;
                    var sizes = photo.Sizes.OfType<IPhotoSize>();
                    foreach (var photoSize in sizes)
                    {
                        if (size == null
                            || Math.Abs(MaxDimension - size.W.Value) > Math.Abs(MaxDimension - photoSize.W.Value))
                        {
                            size = photoSize;
                        }
                    }

                    if (size != null)
                    {
                        if (size.H.Value > size.W.Value)
                        {
                            if (IsScaledVerticalPhoto(minVerticalRatioToScale, size.H, size.W))
                            {
                                return scale * MaxDimension;
                            }

                            return MaxDimension;
                        }

                        return MaxDimension / size.W.Value * size.H.Value;
                    }
                }

                var mediaDocument = value as TLMessageMediaDocument;
                if (mediaDocument != null)
                {
                    return new VideoToDimensionConverter().Convert(value, targetType, parameter, culture);
                }
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StickerPreviewToDimensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const double maxStickerDimension = 93.0;
            var isWidth = string.Equals((string)parameter, "Width", StringComparison.OrdinalIgnoreCase);

            var attributes = value as IAttributes;
            if (attributes != null)
            {
                return StickerToDimensionConverter.GetAttributesDimension(attributes, isWidth, maxStickerDimension);
            }

            return isWidth ? double.NaN : maxStickerDimension;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StickerToDimensionConverter : IValueConverter
    {
        public static double GetAttributesDimension(IAttributes sticker, bool isWidth, double maxStickerDimension)
        {
            TLDocumentAttributeImageSize imageSizeAttribute = null;
            for (var i = 0; i < sticker.Attributes.Count; i++)
            {
                imageSizeAttribute = sticker.Attributes[i] as TLDocumentAttributeImageSize;
                if (imageSizeAttribute != null)
                {
                    break;
                }
            }

            if (imageSizeAttribute != null)
            {
                var width = imageSizeAttribute.W.Value;
                var height = imageSizeAttribute.H.Value;

                var maxDimension = Math.Max(width, height);
                if (maxDimension > maxStickerDimension)
                {
                    var scaleFactor = maxStickerDimension / maxDimension;

                    return isWidth ? scaleFactor * width : scaleFactor * height;
                }

                return isWidth ? width : height;
            }

            return isWidth ? double.NaN : maxStickerDimension;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isWidth = string.Equals((string) parameter, "Width", StringComparison.OrdinalIgnoreCase);
            
            var attributes = value as IAttributes;
            if (attributes != null)
            {
                return GetAttributesDimension(attributes, isWidth, Constants.MaxStickerDimension);
            }

            return isWidth ? double.NaN : Constants.MaxStickerDimension;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GifToDimensionConverter : IValueConverter
    {
        public static double GetGifDimension(double maxGifDimension, IPhotoSize thumb, IAttributes attributes, bool isWidth)
        {
            TLDocumentAttributeVideo videoAttribute = null;
            if (attributes != null)
            {
                for (var i = 0; i < attributes.Attributes.Count; i++)
                {
                    videoAttribute = attributes.Attributes[i] as TLDocumentAttributeVideo;
                    if (videoAttribute != null)
                    {
                        break;
                    }
                }
            }

            if (videoAttribute != null)
            {
                var width = videoAttribute.W.Value;
                var height = videoAttribute.H.Value;

                var maxDimension = width;
                if (maxDimension > 0)
                {
                    var scaleFactor = maxGifDimension / maxDimension;

                    return isWidth ? scaleFactor * width : scaleFactor * height;
                }
            }

            if (thumb != null)
            {
                var width = thumb.W.Value;
                var height = thumb.H.Value;

                var maxDimension = width;
                if (maxDimension > 0)
                {
                    var scaleFactor = maxGifDimension / maxDimension;

                    return isWidth ? scaleFactor * width : scaleFactor * height;
                }
            }

            return maxGifDimension;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isWidth = string.Equals((string)parameter, "Width", StringComparison.OrdinalIgnoreCase);

            var attributes = value as IAttributes;
            var document = value as TLDocument;
            var thumb = document != null ? document.Thumb as IPhotoSize : null;
            return GetGifDimension(Constants.MaxGifDimension, thumb, attributes, isWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WebPageGifToDimensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isWidth = string.Equals((string)parameter, "Width", StringComparison.OrdinalIgnoreCase);

            var attributes = value as IAttributes;
            var document = value as TLDocument;
            var thumb = document != null ? document.Thumb as IPhotoSize : null;
            return GifToDimensionConverter.GetGifDimension(Constants.MaxGifDimension - 12.0 - 3.0 - 12.0, thumb, attributes, isWidth);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InlineBotResultToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value as TLBotInlineResult;
            if (result != null)
            {
                if (result.W != null && result.H != null)
                {
                    var w = (double) result.W.Value;
                    var h = (double) result.H.Value;
                    if (w > 0.0 && h > 0.0)
                    {
                        return w / h * Constants.DeafultInlineBotResultHeight;
                    }
                }
            }

            var mediaResult = value as TLBotInlineMediaResult;
            if (mediaResult != null)
            {
                var photo = mediaResult.Photo as TLPhoto;
                if (photo != null)
                {
                    var photoSize = photo.Sizes.FirstOrDefault(x => x is IPhotoSize) as IPhotoSize;
                    if (photoSize != null)
                    {
                        var w = (double)photoSize.W.Value;
                        var h = (double)photoSize.H.Value;
                        if (w > 0.0 && h > 0.0)
                        {
                            return w / h * Constants.DeafultInlineBotResultHeight;
                        }
                    }
                }

                var document = mediaResult.Document as TLDocument22;
                if (document != null)
                {
                    var videoAttribute = document.Attributes.FirstOrDefault(x => x is TLDocumentAttributeVideo) as TLDocumentAttributeVideo;
                    if (videoAttribute != null)
                    {
                        var w = (double)videoAttribute.W.Value;
                        var h = (double)videoAttribute.H.Value;
                        if (w > 0.0 && h > 0.0)
                        {
                            return w / h * Constants.DeafultInlineBotResultHeight;
                        }
                    }

                    var imageSizeAttribute = document.Attributes.FirstOrDefault(x => x is TLDocumentAttributeImageSize) as TLDocumentAttributeImageSize;
                    if (imageSizeAttribute != null)
                    {
                        var w = (double)imageSizeAttribute.W.Value;
                        var h = (double)imageSizeAttribute.H.Value;
                        if (w > 0.0 && h > 0.0)
                        {
                            return w / h * Constants.DeafultInlineBotResultHeight;
                        }
                    }

                    var thumb = document.Thumb as IPhotoSize;
                    if (thumb != null)
                    {
                        var w = (double)thumb.W.Value;
                        var h = (double)thumb.H.Value;
                        if (w > 0.0 && h > 0.0)
                        {
                            return w / h * Constants.DeafultInlineBotResultHeight;
                        }
                    }
                }
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
