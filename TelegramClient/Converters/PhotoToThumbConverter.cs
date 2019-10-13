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
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Microsoft.Phone;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Helpers;

namespace TelegramClient.Converters
{
    public class PhotoToThumbConverter : IValueConverter
    {
        public bool Secret { get; set; }

        private readonly TelegramClient_WebP.ImageUtils _imageUtils = new TelegramClient_WebP.ImageUtils();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isBlurEnabled = parameter == null || !string.Equals(parameter.ToString(), "noblur", StringComparison.OrdinalIgnoreCase);

            var options = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation;

            var decryptedMediaPhoto = value as TLDecryptedMessageThumbMediaBase;
            if (decryptedMediaPhoto != null)
            {
                var buffer = decryptedMediaPhoto.Thumb.Data;

                if (buffer.Length > 0
                    && decryptedMediaPhoto.ThumbW.Value > 0
                    && decryptedMediaPhoto.ThumbH.Value > 0)
                {
                    if (!isBlurEnabled)
                    {
                        return ImageUtils.CreateImage(buffer, options);
                    }
                    else
                    {
                        try
                        {
                            var memoryStream = new MemoryStream(buffer);
                            var bitmap = PictureDecoder.DecodeJpeg(memoryStream);

                            BlurBitmap(bitmap, Secret);

                            var blurredStream = new MemoryStream();
                            bitmap.SaveJpeg(blurredStream, decryptedMediaPhoto.ThumbW.Value, decryptedMediaPhoto.ThumbH.Value, 0, 100);

                            return ImageUtils.CreateImage(blurredStream, options);
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }

                return null;
            }

            var mediaDocument = value as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var document = mediaDocument.Document as TLDocument;
                if (document != null)
                {
                    var size = document.Thumb as TLPhotoSize;
                    if (size != null)
                    {
                        if (!string.IsNullOrEmpty(size.TempUrl))
                        {
                            return size.TempUrl;
                        }

                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            var fileName = String.Format("{0}_{1}_{2}.jpg",
                                location.VolumeId,
                                location.LocalId,
                                location.Secret);

                            if (!isBlurEnabled)
                            {
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(fileName))
                                    {
                                        try
                                        {
                                            using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                            {
                                                return ImageUtils.CreateImage(stream, options);
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    else
                                    {
                                        var fileManager = IoC.Get<IFileManager>();
                                        fileManager.DownloadFile(location, document, size.Size,
                                            item =>
                                            {
                                                mediaDocument.NotifyOfPropertyChange(() => mediaDocument.ThumbSelf);
                                            });
                                    }
                                }
                            }
                            else
                            {
                                BitmapImage preview;
                                if (TryGetDocumentPreview(document.Id, out preview, options))
                                {
                                    return preview;
                                }

                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(fileName))
                                    {
                                        try
                                        {
                                            using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                            {
                                                var bitmap = PictureDecoder.DecodeJpeg(stream);

                                                BlurBitmap(bitmap, Secret);

                                                var blurredStream = new MemoryStream();
                                                bitmap.SaveJpeg(blurredStream, size.W.Value, size.H.Value, 0, 100);

                                                return bitmap;
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                }

                                var fileManager = IoC.Get<IFileManager>();
                                fileManager.DownloadFile(location, document, size.Size,
                                    item =>
                                    {
                                        Execute.BeginOnUIThread(() =>
                                        {
                                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                            {
                                                if (store.FileExists(fileName))
                                                {
                                                    try
                                                    {
                                                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                                        {
                                                            var bitmap = PictureDecoder.DecodeJpeg(stream);

                                                            BlurBitmap(bitmap, Secret);

                                                            var blurredStream = new MemoryStream();
                                                            bitmap.SaveJpeg(blurredStream, size.W.Value, size.H.Value, 0, 100);

                                                            var previewfileName = string.Format("preview_document{0}.jpg", document.Id);
                                                            SaveFile(previewfileName, blurredStream);

                                                            mediaDocument.NotifyOfPropertyChange(() => mediaDocument.ThumbSelf);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                            }
                                        });
                                    });
                            }
                        }
                    }

                    var cachedSize = document.Thumb as TLPhotoCachedSize;
                    if (cachedSize != null)
                    {
                        if (!string.IsNullOrEmpty(cachedSize.TempUrl))
                        {
                            return cachedSize.TempUrl;
                        }

                        var buffer = cachedSize.Bytes.Data;

                        if (buffer != null && buffer.Length > 0)
                        {
                            if (!isBlurEnabled)
                            {
                                return ImageUtils.CreateImage(buffer, options);
                            }
                            else
                            {
                                BitmapImage preview;
                                if (TryGetDocumentPreview(document.Id, out preview, options))
                                {
                                    return preview;
                                }

                                try
                                {
                                    var bitmap = PictureDecoder.DecodeJpeg(new MemoryStream(buffer));

                                    BlurBitmap(bitmap, Secret);

                                    var blurredStream = new MemoryStream();
                                    bitmap.SaveJpeg(blurredStream, cachedSize.W.Value, cachedSize.H.Value, 0, 100);

                                    var fileName = string.Format("preview_document{0}.jpg", document.Id);

                                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => SaveFile(fileName, blurredStream));

                                    return ImageUtils.CreateImage(blurredStream, options);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }

                        return null;
                    }

                    return null;
                }
            }

            var mediaPhoto = value as TLMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                var photo = mediaPhoto.Photo as TLPhoto;
                if (photo != null)
                {
                    var size = photo.Sizes.FirstOrDefault(x => TLString.Equals(x.Type, new TLString("s"), StringComparison.OrdinalIgnoreCase)) as TLPhotoSize;
                    if (size != null)
                    {
                        if (!string.IsNullOrEmpty(size.TempUrl))
                        {
                            return size.TempUrl;
                        }

                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            var fileName = String.Format("{0}_{1}_{2}.jpg",
                                location.VolumeId,
                                location.LocalId,
                                location.Secret);

                            if (!isBlurEnabled)
                            {
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(fileName))
                                    {
                                        try
                                        {
                                            using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                            {
                                                return ImageUtils.CreateImage(stream, options);
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    else
                                    {
                                        var fileManager = IoC.Get<IFileManager>();
                                        fileManager.DownloadFile(location, photo, size.Size,
                                            item =>
                                            {
                                                mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.ThumbSelf);
                                            });
                                    }
                                }
                            }
                            else
                            {
                                BitmapImage preview;
                                if (TryGetPhotoPreview(photo.Id, out preview, options))
                                {
                                    return preview;
                                }

                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(fileName))
                                    {
                                        try
                                        {
                                            using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                            {
                                                var bitmap = PictureDecoder.DecodeJpeg(stream);

                                                BlurBitmap(bitmap, Secret);

                                                var blurredStream = new MemoryStream();
                                                bitmap.SaveJpeg(blurredStream, size.W.Value, size.H.Value, 0, 100);

                                                return bitmap;
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                }

                                if (location.DCId.Value == 0) return null;

                                var fileManager = IoC.Get<IFileManager>();
                                fileManager.DownloadFile(location, photo, size.Size,
                                    item =>
                                    {
                                        Execute.BeginOnUIThread(() =>
                                        {
                                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                            {
                                                if (store.FileExists(fileName))
                                                {
                                                    try
                                                    {
                                                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                                        {
                                                            var bitmap = PictureDecoder.DecodeJpeg(stream);

                                                            BlurBitmap(bitmap, Secret);

                                                            var blurredStream = new MemoryStream();
                                                            bitmap.SaveJpeg(blurredStream, size.W.Value, size.H.Value, 0, 100);

                                                            var previewfileName = string.Format("preview{0}.jpg", photo.Id);
                                                            SaveFile(previewfileName, blurredStream);

                                                            mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.ThumbSelf);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                            }
                                        });
                                    });
                            }
                        }
                    }

                    var cachedSize = (TLPhotoCachedSize)photo.Sizes.FirstOrDefault(x => x is TLPhotoCachedSize);
                    if (cachedSize != null)
                    {
                        if (!string.IsNullOrEmpty(cachedSize.TempUrl))
                        {
                            return cachedSize.TempUrl;
                        }

                        var buffer = cachedSize.Bytes.Data;

                        if (buffer != null && buffer.Length > 0)
                        {
                            if (!isBlurEnabled)
                            {
                                return ImageUtils.CreateImage(buffer, options);
                            }
                            else
                            {
                                BitmapImage preview;
                                if (TryGetPhotoPreview(photo.Id, out preview, options))
                                {
                                    return preview;
                                }

                                try
                                {
                                    var bitmap = PictureDecoder.DecodeJpeg(new MemoryStream(buffer));

                                    BlurBitmap(bitmap, Secret);

                                    var blurredStream = new MemoryStream();
                                    bitmap.SaveJpeg(blurredStream, cachedSize.W.Value, cachedSize.H.Value, 0, 100);

                                    var fileName = string.Format("preview{0}.jpg", photo.Id);

                                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => SaveFile(fileName, blurredStream));

                                    return ImageUtils.CreateImage(blurredStream, options);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }

                        return null;
                    }

                    return null;
                }
            }

            var mediaGame = value as TLMessageMediaGame;
            if (mediaGame != null)
            {
                var photo = mediaGame.Photo as TLPhoto;
                if (photo != null)
                {
                    var size = photo.Sizes.FirstOrDefault(x => TLString.Equals(x.Type, new TLString("s"), StringComparison.OrdinalIgnoreCase)) as TLPhotoSize;
                    if (size != null)
                    {
                        if (!string.IsNullOrEmpty(size.TempUrl))
                        {
                            return size.TempUrl;
                        }

                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            var fileName = String.Format("{0}_{1}_{2}.jpg",
                                location.VolumeId,
                                location.LocalId,
                                location.Secret);

                            if (!isBlurEnabled)
                            {
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(fileName))
                                    {
                                        try
                                        {
                                            using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                            {
                                                return ImageUtils.CreateImage(stream, options);
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    else
                                    {
                                        var fileManager = IoC.Get<IFileManager>();
                                        fileManager.DownloadFile(location, photo, size.Size,
                                            item =>
                                            {
                                                mediaGame.NotifyOfPropertyChange(() => mediaGame.ThumbSelf);
                                            });
                                    }
                                }
                            }
                            else
                            {
                                BitmapImage preview;
                                if (TryGetPhotoPreview(photo.Id, out preview, options))
                                {
                                    return preview;
                                }

                                var fileManager = IoC.Get<IFileManager>();
                                fileManager.DownloadFile(location, photo, size.Size,
                                    item =>
                                    {
                                        Execute.BeginOnUIThread(() =>
                                        {
                                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                            {
                                                if (store.FileExists(fileName))
                                                {
                                                    try
                                                    {
                                                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                                        {
                                                            var bitmap = PictureDecoder.DecodeJpeg(stream);

                                                            BlurBitmap(bitmap, Secret);

                                                            var blurredStream = new MemoryStream();
                                                            bitmap.SaveJpeg(blurredStream, size.W.Value, size.H.Value, 0, 100);

                                                            var previewfileName = string.Format("preview{0}.jpg", photo.Id);
                                                            SaveFile(previewfileName, blurredStream);

                                                            mediaGame.NotifyOfPropertyChange(() => mediaGame.ThumbSelf);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                }
                                            }
                                        });
                                    });
                            }
                        }
                    }

                    var cachedSize = (TLPhotoCachedSize) photo.Sizes.FirstOrDefault(x => x is TLPhotoCachedSize);
                    if (cachedSize != null)
                    {
                        if (!string.IsNullOrEmpty(cachedSize.TempUrl))
                        {
                            return cachedSize.TempUrl;
                        }

                        var buffer = cachedSize.Bytes.Data;

                        if (buffer != null && buffer.Length > 0)
                        {
                            if (!isBlurEnabled)
                            {
                                return ImageUtils.CreateImage(buffer, options);
                            }
                            else
                            {
                                BitmapImage preview;
                                if (TryGetPhotoPreview(photo.Id, out preview, options))
                                {
                                    return preview;
                                }

                                try
                                {
                                    var bitmap = PictureDecoder.DecodeJpeg(new MemoryStream(buffer));

                                    BlurBitmap(bitmap, Secret);

                                    var blurredStream = new MemoryStream();
                                    bitmap.SaveJpeg(blurredStream, cachedSize.W.Value, cachedSize.H.Value, 0, 100);

                                    var fileName = string.Format("preview{0}.jpg", photo.Id);

                                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => SaveFile(fileName, blurredStream));

                                    return ImageUtils.CreateImage(blurredStream);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }

                        return null;
                    }

                    return null;
                }
            }

            return null;
        }

        public static bool TryGetDocumentPreview(TLLong documentId, out BitmapImage preview, BitmapCreateOptions options)
        {
            preview = null;

            var fileName = string.Format("preview_document{0}.jpg", documentId);
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(fileName))
                {
                    try
                    {
                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            preview = ImageUtils.CreateImage(stream, options);
                            return true;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return false;
        }

        public static bool TryGetPhotoPreview(TLLong photoId, out BitmapImage preview, BitmapCreateOptions options)
        {
            preview = null;

            var fileName = string.Format("preview{0}.jpg", photoId);
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(fileName))
                {
                    try
                    {
                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            preview = ImageUtils.CreateImage(stream, options);
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }

            return false;
        }

        public static void SaveFile(string fileName, MemoryStream blurredStream)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using (var stream = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        var buffer = blurredStream.ToArray();
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void BlurBitmap(WriteableBitmap bitmap, bool secret)
        {
            //bitmap.BoxBlur(7);
            var pix = new byte[bitmap.Pixels.Length*4];
            for (var j = 0; j < bitmap.Pixels.Length; j++)
            {
                pix[j*4] = (byte) bitmap.Pixels[j]; //r
                pix[j*4 + 1] = (byte) (bitmap.Pixels[j] >> 8); //g
                pix[j*4 + 2] = (byte) (bitmap.Pixels[j] >> 16); //b
                pix[j*4 + 3] = (byte) (bitmap.Pixels[j] >> 24); //a
            }
            var pixels = secret
                ? _imageUtils.FastSecretBlur(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.PixelWidth * 4, pix)
                : _imageUtils.FastBlur(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.PixelWidth*4, pix);
            for (var j = 0; j < bitmap.Pixels.Length; j++)
            {
                bitmap.Pixels[j] = pixels[j*4] + (pixels[j*4 + 1] << 8) + (pixels[j*4 + 2] << 16) + (pixels[j*4 + 3] << 24);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
