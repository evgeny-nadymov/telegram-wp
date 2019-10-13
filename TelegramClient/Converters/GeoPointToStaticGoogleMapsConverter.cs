// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.Converters
{
    public class GeoPointToStaticGoogleMapsConverter : IValueConverter
    {
        private const int DefaultWidth = 311;
        private const int DefaultHeight = 150;

        public int Width { get; set; }

        public int Height { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var geoPoint = value as TLGeoPoint;

            if (geoPoint == null) return null;

            var width = Width != 0 ? Width : DefaultWidth;
            var height = Height != 0 ? Height : DefaultHeight;
            return string.Format(Constants.StaticGoogleMap, geoPoint.Lat.Value.ToString(new CultureInfo("en-US")), geoPoint.Long.Value.ToString(new CultureInfo("en-US")), width, height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GeoPointToStaticGoogleMapsConverter2 : IValueConverter
    {
        private static readonly Dictionary<string, WeakReference> _cachedSources = new Dictionary<string, WeakReference>();

        public static BitmapSource ReturnOrEnqueueGeoPointImage(TLGeoPoint geoPoint, int width, int height, TLObject owner)
        {
            var destFileName = geoPoint.GetFileName();

            BitmapSource imageSource;
            WeakReference weakImageSource;
            if (_cachedSources.TryGetValue(destFileName, out weakImageSource))
            {
                if (weakImageSource.IsAlive)
                {
                    imageSource = weakImageSource.Target as BitmapSource;

                    return imageSource;
                }
            }

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(destFileName))
                {
                    var sourceUri = string.Format(Constants.StaticGoogleMap, geoPoint.Lat.Value.ToString(new CultureInfo("en-US")), geoPoint.Long.Value.ToString(new CultureInfo("en-US")), width, height);

                    var fileManager = IoC.Get<IHttpDocumentFileManager>();

                    fileManager.DownloadFileAsync(sourceUri, destFileName, owner, item =>
                    {
                        var messageMediaGeoPoint = owner as IMessageMediaGeoPoint;
                        if (messageMediaGeoPoint != null)
                        {
                            var newGeoPoint = messageMediaGeoPoint.Geo as TLGeoPoint;
                            if (newGeoPoint != null)
                            {
                                var newFileName = newGeoPoint.GetFileName();
                                var oldFileName = destFileName;

                                using (var store2 = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store2.FileExists(oldFileName))
                                    {
                                        if (!string.IsNullOrEmpty(oldFileName) && !string.Equals(oldFileName, newFileName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            store2.CopyFile(oldFileName, newFileName, true);
                                            store2.DeleteFile(oldFileName);
                                        }
                                    }
                                }
                            }

                            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                            {
                                var messageMediaBase = owner as TLMessageMediaBase;
                                if (messageMediaBase != null)
                                {
                                    messageMediaBase.NotifyOfPropertyChange(() => messageMediaBase.Self);
                                }
                                var decryptedMessageMediaBase = owner as TLDecryptedMessageMediaBase;
                                if (decryptedMessageMediaBase != null)
                                {
                                    decryptedMessageMediaBase.NotifyOfPropertyChange(() => decryptedMessageMediaBase.Self);
                                }
                            });
                        }
                    });
                }
                else
                {
                    try
                    {
                        using (var stream = store.OpenFile(destFileName, FileMode.Open, FileAccess.Read))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            imageSource = image;
                        }

                        _cachedSources[destFileName] = new WeakReference(imageSource);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    return imageSource;
                }
            }

            return null;
        }

        private const int DefaultWidth = 311;
        private const int DefaultHeight = 150;

        public int Width { get; set; }

        public int Height { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mediaGeo = value as TLMessageMediaGeo;
            if (mediaGeo != null)
            {
                var geoPoint = mediaGeo.Geo as TLGeoPoint;
                if (geoPoint == null) return null;

                var width = Width != 0 ? Width : DefaultWidth;
                var height = Height != 0 ? Height : DefaultHeight;

                return ReturnOrEnqueueGeoPointImage(geoPoint, width, height, mediaGeo);
            }

            var decryptedMediaGeo = value as TLDecryptedMessageMediaGeoPoint;
            if (decryptedMediaGeo != null)
            {
                var geoPoint = decryptedMediaGeo.Geo as TLGeoPoint;
                if (geoPoint == null) return null;

                var width = Width != 0 ? Width : DefaultWidth;
                var height = Height != 0 ? Height : DefaultHeight;

                return ReturnOrEnqueueGeoPointImage(geoPoint, width, height, decryptedMediaGeo);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GeoPointToStaticGoogleMapsConverter3 : IValueConverter
    {
        private static readonly Dictionary<string, WeakReference> _cachedSources = new Dictionary<string, WeakReference>();

        public static BitmapSource ReturnOrEnqueueGeoPointImage(TLGeoPoint geoPoint, int width, int height, TLObject owner)
        {

            var destFileName = geoPoint.GetFileName();

            BitmapSource imageSource;
            WeakReference weakImageSource;
            if (_cachedSources.TryGetValue(destFileName, out weakImageSource))
            {
                if (weakImageSource.IsAlive)
                {
                    imageSource = weakImageSource.Target as BitmapSource;

                    return imageSource;
                }
            }

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(destFileName))
                {
                    var config = IoC.Get<ICacheService>().GetConfig() as TLConfig82;
                    if (config == null) return null;

                    var fileManager = IoC.Get<IWebFileManager>();

                    var geoPoint82 = geoPoint as TLGeoPoint82;
                    var accessHash = geoPoint82 != null ? geoPoint82.AccessHash : new TLLong(0); 

                    var inputLocation = new TLInputWebFileGeoPointLocation
                    {
                        GeoPoint = new TLInputGeoPoint { Long = geoPoint.Long, Lat = geoPoint.Lat },
                        AccessHash = accessHash,
                        W = new TLInt(width),
                        H = new TLInt(height),
                        Zoom = new TLInt(15),
                        Scale = new TLInt(2)
                    };

                    fileManager.DownloadFile(config.WebfileDCId, inputLocation, destFileName, owner, item =>
                    {
                        var messageMediaGeoPoint = owner as IMessageMediaGeoPoint;
                        if (messageMediaGeoPoint != null)
                        {
                            var newGeoPoint = messageMediaGeoPoint.Geo as TLGeoPoint;
                            if (newGeoPoint != null)
                            {
                                var newFileName = newGeoPoint.GetFileName();
                                var oldFileName = destFileName;

                                using (var store2 = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store2.FileExists(oldFileName))
                                    {
                                        if (!string.IsNullOrEmpty(oldFileName) && !string.Equals(oldFileName, newFileName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            store2.CopyFile(oldFileName, newFileName, true);
                                            store2.DeleteFile(oldFileName);
                                        }
                                    }
                                }
                            }

                            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                            {
                                var messageMediaBase = owner as TLMessageMediaBase;
                                if (messageMediaBase != null)
                                {
                                    messageMediaBase.NotifyOfPropertyChange(() => messageMediaBase.Self);
                                }
                                var decryptedMessageMediaBase = owner as TLDecryptedMessageMediaBase;
                                if (decryptedMessageMediaBase != null)
                                {
                                    decryptedMessageMediaBase.NotifyOfPropertyChange(() => decryptedMessageMediaBase.Self);
                                }
                            });
                        }
                    });
                }
                else
                {
                    try
                    {
                        using (var stream = store.OpenFile(destFileName, FileMode.Open, FileAccess.Read))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            imageSource = image;
                        }

                        _cachedSources[destFileName] = new WeakReference(imageSource);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    return imageSource;
                }
            }

            return null;
        }

        private const int DefaultWidth = 311;
        private const int DefaultHeight = 150;

        public int Width { get; set; }

        public int Height { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mediaGeo = value as TLMessageMediaGeo;
            if (mediaGeo != null)
            {
                var geoPoint = mediaGeo.Geo as TLGeoPoint;
                if (geoPoint == null) return null;

                var width = Width != 0 ? Width : DefaultWidth;
                var height = Height != 0 ? Height : DefaultHeight;

                return ReturnOrEnqueueGeoPointImage(geoPoint, width, height, mediaGeo);
            }

            var decryptedMediaGeo = value as TLDecryptedMessageMediaGeoPoint;
            if (decryptedMediaGeo != null)
            {
                var geoPoint = decryptedMediaGeo.Geo as TLGeoPoint;
                if (geoPoint == null) return null;

                var width = Width != 0 ? Width : DefaultWidth;
                var height = Height != 0 ? Height : DefaultHeight;

                return ReturnOrEnqueueGeoPointImage(geoPoint, width, height, decryptedMediaGeo);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
