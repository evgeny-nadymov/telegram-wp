// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
//#define PHOTO_CACHE_DISABLED
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Microsoft.Phone;
using Telegram.Api;
using Telegram.Api.Extensions;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Services;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Dialogs;
using TelegramClient.Views.Media;
#if WP81
using Windows.Graphics.Imaging;
#endif
#if WP8
using TelegramClient_WebP.LibWebP;
#endif

namespace TelegramClient.Converters
{
    public class InlineBotResultPhotoConverter : IValueConverter
    {
        public static BitmapImage ReturnOrEnqueueImage(TLFileLocation location, TLBotInlineResultBase owner, TLInt fileSize)
        {
            var fileName = String.Format("{0}_{1}_{2}.jpg",
                location.VolumeId,
                location.LocalId,
                location.Secret);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                {
                    if (fileSize != null)
                    {
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            var fileManager = IoC.Get<IFileManager>();
                            fileManager.DownloadFile(location, owner, fileSize, item => Execute.BeginOnUIThread(() =>
                            {
                                owner.NotifyOfPropertyChange(() => owner.Self);
                            }));
                        });
                    }
                }
                else
                {
                    BitmapImage imageSource;

                    try
                    {
                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            imageSource = image;
                        }
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

        public static TLPhotoSize GetPhotoSize(TLPhoto photo, object parameter = null)
        {
            var width = 311.0;
            double result;
            if (Double.TryParse((string)parameter, out result))
            {
                width = result;
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

            return size;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resultMedia = value as TLBotInlineMediaResult;
            if (resultMedia != null)
            {
                var photo = resultMedia.Photo as TLPhoto;
                if (photo != null)
                {
                    var size = GetPhotoSize(photo, parameter);

                    if (size != null)
                    {
                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            return ReturnOrEnqueueImage(location, resultMedia, size.Size);
                        }
                    }
                }

                var document = resultMedia.Document as TLDocument;
                if (document != null)
                {
                    var cachedSize = document.Thumb as TLPhotoCachedSize;
                    if (cachedSize != null)
                    {
                        BitmapImage imageSource;

                        try
                        {
                            var image = new BitmapImage();
                            image.SetSource(new MemoryStream(cachedSize.Bytes.Data));
                            imageSource = image;
                        }
                        catch (Exception)
                        {
                            return null;
                        }

                        return imageSource;
                    }

                    var size = document.Thumb as TLPhotoSize;
                    if (size != null)
                    {
                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            return ReturnOrEnqueueImage(location, resultMedia, size.Size);
                        }
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaPhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var photo = value as TLPhoto;
            if (photo != null)
            {
                var imageSource = GetImageSource(photo, 311.0);
                if (imageSource != null) return imageSource;

                imageSource = GetImageSource(photo, 800.0);
                if (imageSource != null) return imageSource;
            }

            return null;
        }

        private static object GetImageSource(TLPhoto photo, double width)
        {
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
                var location = size.Location as TLFileLocation;
                if (location != null)
                {
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(fileName))
                        {
                            BitmapImage imageSource;

                            try
                            {
                                using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                {
                                    stream.Seek(0, SeekOrigin.Begin);
                                    var image = new BitmapImage();
                                    image.SetSource(stream);
                                    imageSource = image;
                                }
                            }
                            catch (Exception)
                            {
                                return null;
                            }

                            return imageSource;
                        }
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timer = Stopwatch.StartNew();

            var photoSize = value as TLPhotoSize;
            if (photoSize != null)
            {
                var location = photoSize.Location as TLFileLocation;
                if (location != null)
                {
                    return DefaultPhotoConverter.ReturnImage(timer, location);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProfileSmallPhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timer = Stopwatch.StartNew();

            var userProfilePhoto = value as TLUserProfilePhoto;
            if (userProfilePhoto != null)
            {
                var location = userProfilePhoto.PhotoSmall as TLFileLocation;
                if (location != null)
                {
                    return DefaultPhotoConverter.ReturnOrEnqueueImage(timer, false, location, userProfilePhoto, new TLInt(0), null, true);
                }
            }

            var chatPhoto = value as TLChatPhoto;
            if (chatPhoto != null)
            {
                var location = chatPhoto.PhotoSmall as TLFileLocation;
                if (location != null)
                {
                    return DefaultPhotoConverter.ReturnOrEnqueueImage(timer, false, location, chatPhoto, new TLInt(0), null, true);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProfileBigPhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timer = Stopwatch.StartNew();

            var userProfilePhoto = value as TLUserProfilePhoto;
            if (userProfilePhoto != null)
            {
                var location = userProfilePhoto.PhotoBig as TLFileLocation;
                if (location != null)
                {
                    return DefaultPhotoConverter.ReturnOrEnqueueImage(timer, false, location, userProfilePhoto, new TLInt(0), null, true,
                        result => Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                        {
                            userProfilePhoto.NotifyOfPropertyChange(() => userProfilePhoto.Self);
                        }));
                }
            }

            var chatPhoto = value as TLChatPhoto;
            if (chatPhoto != null)
            {
                var location = chatPhoto.PhotoBig as TLFileLocation;
                if (location != null)
                {
                    return DefaultPhotoConverter.ReturnOrEnqueueImage(timer, false, location, chatPhoto, new TLInt(0), null, true,
                        result => Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                        {
                            chatPhoto.NotifyOfPropertyChange(() => chatPhoto.Self);
                        }));
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DefaultPhotoConverter : IValueConverter
    {
        public bool CheckChatSettings { get; set; }

        public static BitmapImage ReturnOrEnqueueImage(bool checkChatSettings, TLEncryptedFile location, TLObject owner, TLDecryptedMessageMediaBase mediaPhoto, bool isBackground)
        {
            var fileName = String.Format("{0}_{1}_{2}.jpg",
                location.Id,
                location.DCId,
                location.AccessHash);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                {
                    TLObject with = null;
                    if (checkChatSettings)
                    {
                        var navigationService = IoC.Get<INavigationService>();
                        var dialogDetailsView = navigationService.CurrentContent as SecretDialogDetailsView;
                        if (dialogDetailsView != null)
                        {
                            var dialogDetailsViewModel = dialogDetailsView.DataContext as SecretDialogDetailsViewModel;
                            if (dialogDetailsViewModel != null)
                            {
                                with = dialogDetailsViewModel.With;
                            }
                        }
                    }

                    var stateService = IoC.Get<IStateService>();
                    var chatSettings = stateService.GetChatSettings();
                    if (chatSettings != null)
                    {
                        if (with is TLUserBase && !chatSettings.AutoDownloadPhotoPrivateChats)
                        {
                            return null;
                        }

                        if (with is TLChatBase && !chatSettings.AutoDownloadPhotoGroups)
                        {
                            return null;
                        }
                    }

                    if (mediaPhoto != null) mediaPhoto.DownloadingProgress = 0.01;

                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                    {
                        var fileManager = IoC.Get<IEncryptedFileManager>();
                        fileManager.DownloadFile(location, owner);
                    });
                }
                else
                {
                    BitmapImage imageSource;

                    try
                    {
                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var image = new BitmapImage();
                            if (isBackground)
                            {
                                image.CreateOptions |= BitmapCreateOptions.BackgroundCreation;
                            }
                            image.SetSource(stream);
                            imageSource = image;
                        }
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

        public static BitmapImage ReturnImage(Stopwatch timer, TLFileLocation location)
        {
            //return null;

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                {

                }
                else
                {
                    BitmapImage imageSource;

                    try
                    {
                        //using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, store))
                        //{
                        //    stream.Seek(0, SeekOrigin.Begin);
                        //    var image = new BitmapImage();
                        //    image.SetSource(stream);
                        //    imageSource = image;
                        //}

                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var image = new BitmapImage();
                            image.CreateOptions = BitmapCreateOptions.DelayCreation |
                                                  BitmapCreateOptions.BackgroundCreation;
                            image.SetSource(stream);
                            imageSource = image;
                        }
                    }
                    catch (Exception ex)
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage(ex.ToString());

                        return null;
                    }


                    //TLUtils.WritePerformance("DefaultPhotoConverter time: " + timer.Elapsed);
                    return imageSource;
                }
            }

            return null;
        }

#if !PHOTO_CACHE_DISABLED
        private static LRUCache<string, WeakReference<BitmapImage>> _photoCache = new LRUCache<string, WeakReference<BitmapImage>>(100);
#endif

        public static BitmapImage ReturnOrEnqueueImage(Stopwatch timer, bool checkChatSettings, TLFileLocation location, TLObject owner, TLInt fileSize, TLMessageMediaPhoto mediaPhoto, bool isBackground = false, Action<DownloadableItem> callback = null)
        {
            var fileName = string.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                {
                    if (fileSize != null)
                    {
                        TLObject with = null;
                        if (checkChatSettings)
                        {
                            var navigationService = IoC.Get<INavigationService>();
                            var dialogDetailsView = navigationService.CurrentContent as DialogDetailsView;
                            if (dialogDetailsView != null)
                            {
                                var dialogDetailsViewModel = dialogDetailsView.DataContext as DialogDetailsViewModel;
                                if (dialogDetailsViewModel != null)
                                {
                                    with = dialogDetailsViewModel.With;
                                }
                            }
                        }

                        var stateService = IoC.Get<IStateService>();
                        var chatSettings = stateService.GetChatSettings();
                        if (chatSettings != null)
                        {
                            if (with is TLUserBase && !chatSettings.AutoDownloadPhotoPrivateChats)
                            {
                                return null;
                            }

                            if (with is TLChatBase && !chatSettings.AutoDownloadPhotoGroups)
                            {
                                return null;
                            }
                        }

                        if (location.DCId.Value == 0) return null;
                        if (mediaPhoto != null) mediaPhoto.DownloadingProgress = 0.01;

                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            var fileManager = IoC.Get<IFileManager>();
                            fileManager.DownloadFile(location, owner, fileSize, callback);
                        });
                    }
                }
                else
                {
                    BitmapImage imageSource;

                    try
                    {
                        BitmapImage image;
#if !PHOTO_CACHE_DISABLED
                        WeakReference<BitmapImage> reference;
                        if (_photoCache.TryGetValue(fileName, out reference) && reference.TryGetTarget(out image))
                        {
                            if (image.PixelHeight > 0 || image.PixelWidth > 0)
                            {
                                return image;
                            }
                        }
#endif

                        using (IsolatedStorageFileStream stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            image = new BitmapImage();
                            if (isBackground)
                            {
                                image.CreateOptions |= BitmapCreateOptions.BackgroundCreation;
                            }
                            image.SetSource(stream);
#if !PHOTO_CACHE_DISABLED
                            if (image.CreateOptions.HasFlag(BitmapCreateOptions.BackgroundCreation))
                            {
                                var wrapper = new BitmapImageWrapper(fileName, image);
                                wrapper.Subscribe(bi =>
                                {
                                    _photoCache[fileName] = new WeakReference<BitmapImage>(image);
                                });
                            }
                            else
                            {
                                _photoCache[fileName] = new WeakReference<BitmapImage>(image);
                            }
#endif
                            imageSource = image;
                        }
                    }
                    catch (Exception ex)
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage(ex.ToString());

                        return null;
                    }

                    return imageSource;
                }
            }

            return null;
        }

#if !PHOTO_CACHE_DISABLED
        public static void InvalidateCacheItem(string key)
        {
            _photoCache.Remove(key);
        }
#endif

        #region Profile Photo
#if !PHOTO_CACHE_DISABLED
        private static readonly LRUCache<string, WeakReference<BitmapImage>> _cachedSources = new LRUCache<string, WeakReference<BitmapImage>>(200);
#else
        private static readonly Dictionary<string, WeakReference> _cachedSources = new Dictionary<string, WeakReference>();
#endif

        public static BitmapSource ReturnOrEnqueueProfileImage(Stopwatch timer, TLFileLocation location, TLObject owner, TLInt fileSize, bool isBackground = false)
        {
            var fileName = String.Format("{0}_{1}_{2}.jpg",
                location.VolumeId,
                location.LocalId,
                location.Secret);

#if !PHOTO_CACHE_DISABLED
            BitmapImage bitmapImage;
            WeakReference<BitmapImage> weakImageSource;
            if (_cachedSources.TryGetValue(fileName, out weakImageSource) && weakImageSource.TryGetTarget(out bitmapImage))
            {
                return bitmapImage;
            }
#else
            BitmapSource bitmapImage;
            WeakReference weakImageSource;
            if (_cachedSources.TryGetValue(fileName, out weakImageSource))
            {
                if (weakImageSource.IsAlive)
                {
                    bitmapImage = weakImageSource.Target as BitmapSource;

                    //System.Diagnostics.Debug.WriteLine("DefaultPhotoConverter weakImageSource return elapsed=" + ShellViewModel.Timer.Elapsed);
                    return bitmapImage;
                }
            }
#endif

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                {
                    if (fileSize != null)
                    {
                        var fileManager = IoC.Get<IFileManager>();
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            fileManager.DownloadFile(location, owner, fileSize);
                        });
                    }
                }
                else
                {
                    try
                    {
                        using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var image = new BitmapImage();
                            if (isBackground)
                            {
                                image.CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation;
                            }
                            image.SetSource(stream);
                            bitmapImage = image;
#if !PHOTO_CACHE_DISABLED
                            if (image.CreateOptions.HasFlag(BitmapCreateOptions.BackgroundCreation))
                            {
                                var wrapper = new BitmapImageWrapper(fileName, bitmapImage);
                                wrapper.Subscribe(bi =>
                                {
                                    _cachedSources[fileName] = new WeakReference<BitmapImage>(image);
                                });
                            }
                            else
                            {
                                _cachedSources[fileName] = new WeakReference<BitmapImage>(image);
                            }
#else
                            _cachedSources[fileName] = new WeakReference(bitmapImage);
#endif
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    return bitmapImage;
                }
            }

            return null;
        }
        #endregion

#if WP8

        private static readonly Dictionary<string, WeakReference<WriteableBitmap>> _cachedWebPImages = new Dictionary<string, WeakReference<WriteableBitmap>>();

        private static ImageSource DecodeWebPImage(string cacheKey, byte[] buffer, System.Action faultCallback = null)
        {
            try
            {
                WeakReference<WriteableBitmap> reference;
                if (_cachedWebPImages.TryGetValue(cacheKey, out reference))
                {
                    WriteableBitmap cachedBitmap;
                    if (reference.TryGetTarget(out cachedBitmap))
                    {
                        return cachedBitmap;
                    }
                }

                var decoder = new WebPDecoder();
                int width = 0, height = 0;
                byte[] decoded = null;
                //try
                //{
                if (buffer == null)
                {
                    Telegram.Logs.Log.Write("DefaultPhotoConverter.DecodeWebPImage buffer=null");
                }
                else
                {
                    //buffer = null;
                    decoded = decoder.DecodeRgbA(buffer, out width, out height);
                }
                //}
                //catch (Exception ex)
                //{
                //    faultCallback.SafeInvoke();
                //    // не получается сконвертировать, битый файл
                //    //store.DeleteFile(documentLocalFileName);
                //    Telegram.Api.Helpers.Execute.ShowDebugMessage("WebPDecoder.DecodeRgbA ex " + ex);
                //}

                if (decoded == null) return null;

                var wb = new WriteableBitmap(width, height);
                for (var i = 0; i < decoded.Length / 4; i++)
                {
                    int r = decoded[4 * i];
                    int g = decoded[4 * i + 1];
                    int b = decoded[4 * i + 2];
                    int a = decoded[4 * i + 3];

                    a <<= 24;
                    r <<= 16;
                    g <<= 8;
                    int iPixel = a | r | g | b;

                    wb.Pixels[i] = iPixel;
                }

                _cachedWebPImages[cacheKey] = new WeakReference<WriteableBitmap>(wb);

                return wb;
            }
            catch (Exception ex)
            {
                TLUtils.WriteException("WebPDecode ex ", ex);
                //Telegram.Api.Helpers.Execute.BeginOnThreadPool(faultCallback);
                return null;
            }

            return null;
        }

        public static ImageSource ReturnOrEnqueueStickerPreview(TLFileLocation location, TLObject owner, TLInt fileSize)
        {
            var fileName =
                String.Format("{0}_{1}_{2}.jpg",
                location.VolumeId,
                location.LocalId,
                location.Secret);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                {
                    if (fileSize != null)
                    {
                        var fileManager = IoC.Get<IFileManager>();
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            fileManager.DownloadFile(location, owner, fileSize);
                        });
                    }
                }
                else
                {
                    byte[] buffer;
                    using (var file = store.OpenFile(fileName, FileMode.Open))
                    {
                        buffer = new byte[file.Length];
                        file.Read(buffer, 0, buffer.Length);
                    }

                    return DecodeWebPImage(fileName, buffer,
                        () =>
                        {
                            using (var localStore = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                localStore.DeleteFile(fileName);
                            }
                        });
                }
            }

            return null;
        }

        private static ImageSource ReturnOrEnqueueSticker(TLDecryptedMessageMediaExternalDocument document, TLObject owner)
        {
            if (document == null) return null;

            var documentLocalFileName = document.GetFileName();

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(documentLocalFileName))
                {
                    // 1. download full size
                    IoC.Get<IDocumentFileManager>().DownloadFileAsync(document.FileName, document.DCId, document.ToInputFileLocation(), owner, document.Size, progress => { });

                    // 2. download preview
                    var thumbCachedSize = document.Thumb as TLPhotoCachedSize;
                    if (thumbCachedSize != null)
                    {
                        var fileName = "cached" + document.GetFileName();
                        var buffer = thumbCachedSize.Bytes.Data;
                        if (buffer == null) return null;

                        return DecodeWebPImage(fileName, buffer, () => { });
                    }

                    var thumbPhotoSize = document.Thumb as TLPhotoSize;
                    if (thumbPhotoSize != null)
                    {
                        var location = thumbPhotoSize.Location as TLFileLocation;
                        if (location != null)
                        {
                            return ReturnOrEnqueueStickerPreview(location, owner, thumbPhotoSize.Size);
                        }
                    }
                }
                else
                {
                    if (document.Size.Value > 0
                        && document.Size.Value < Telegram.Api.Constants.StickerMaxSize)
                    {
                        byte[] buffer;
                        using (var file = store.OpenFile(documentLocalFileName, FileMode.Open))
                        {
                            buffer = new byte[file.Length];
                            file.Read(buffer, 0, buffer.Length);
                        }

                        return DecodeWebPImage(documentLocalFileName, buffer,
                            () =>
                            {
                                using (var localStore = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    localStore.DeleteFile(documentLocalFileName);
                                }
                            });
                    }
                }
            }

            return null;
        }

        public static ImageSource ReturnOrEnqueueSticker(TLDocument22 document, TLObject sticker)
        {
            if (document == null) return null;

            var documentLocalFileName = document.GetFileName();

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(documentLocalFileName))
                {
                    TLObject owner = document;
                    if (sticker != null)
                    {
                        owner = sticker;
                    }

                    // 1. download full size
                    IoC.Get<IDocumentFileManager>().DownloadFileAsync(document.FileName, document.DCId, document.ToInputFileLocation(), owner, document.Size, progress => { });

                    // 2. download preview
                    var thumbCachedSize = document.Thumb as TLPhotoCachedSize;
                    if (thumbCachedSize != null)
                    {
                        var fileName = "cached" + document.GetFileName();
                        var buffer = thumbCachedSize.Bytes.Data;
                        if (buffer == null) return null;

                        return DecodeWebPImage(fileName, buffer, () => { });
                    }

                    var thumbPhotoSize = document.Thumb as TLPhotoSize;
                    if (thumbPhotoSize != null)
                    {
                        var location = thumbPhotoSize.Location as TLFileLocation;
                        if (location != null)
                        {
                            return ReturnOrEnqueueStickerPreview(location, sticker, thumbPhotoSize.Size);
                        }
                    }
                }
                else
                {
                    if (document.DocumentSize > 0
                        && document.DocumentSize < Telegram.Api.Constants.StickerMaxSize)
                    {
                        byte[] buffer;
                        using (var file = store.OpenFile(documentLocalFileName, FileMode.Open))
                        {
                            buffer = new byte[file.Length];
                            file.Read(buffer, 0, buffer.Length);
                        }

                        return DecodeWebPImage(documentLocalFileName, buffer,
                            () =>
                            {
                                using (var localStore = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    localStore.DeleteFile(documentLocalFileName);
                                }
                            });
                    }
                }
            }

            return null;
        }

#endif

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //System.Diagnostics.Debug.WriteLine("DefaultPhotoConverter elapsed=" + ShellViewModel.Timer.Elapsed);
            if (value == null) return null;

            var timer = Stopwatch.StartNew();

            var encryptedPhoto = value as TLEncryptedFile;
            if (encryptedPhoto != null)
            {
                var isBackground = parameter != null && Equals(parameter, "Background");

                return ReturnOrEnqueueImage(CheckChatSettings, encryptedPhoto, encryptedPhoto, null, isBackground);
            }

            var userProfilePhoto = value as TLUserProfilePhoto;
            if (userProfilePhoto != null)
            {
                var location = userProfilePhoto.PhotoSmall as TLFileLocation;
                if (location != null)
                {
                    return ReturnOrEnqueueProfileImage(timer, location, userProfilePhoto, new TLInt(0), false);
                }
            }

            var chatPhoto = value as TLChatPhoto;
            if (chatPhoto != null)
            {
                var location = chatPhoto.PhotoSmall as TLFileLocation;
                if (location != null)
                {
                    return ReturnOrEnqueueProfileImage(timer, location, chatPhoto, new TLInt(0), false);
                }
            }

            var decrypteMedia = value as TLDecryptedMessageMediaBase;
            if (decrypteMedia != null)
            {
                var decryptedMediaVideo = value as TLDecryptedMessageMediaVideo;
                if (decryptedMediaVideo != null)
                {
                    var buffer = decryptedMediaVideo.Thumb.Data;

                    if (buffer.Length > 0
                        && decryptedMediaVideo.ThumbW.Value > 0
                        && decryptedMediaVideo.ThumbH.Value > 0)
                    {
                        try
                        {
                            var memoryStream = new MemoryStream(buffer);
                            var bitmap = PictureDecoder.DecodeJpeg(memoryStream);

                            bitmap.BoxBlur(37);

                            var blurredStream = new MemoryStream();
                            bitmap.SaveJpeg(blurredStream, decryptedMediaVideo.ThumbW.Value, decryptedMediaVideo.ThumbH.Value, 0, 70);

                            return ImageUtils.CreateImage(blurredStream.ToArray());
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    return ImageUtils.CreateImage(buffer);
                }

                var decryptedMediaDocument = value as TLDecryptedMessageMediaDocument;
                if (decryptedMediaDocument != null)
                {
                    var location = decryptedMediaDocument.File as TLEncryptedFile;
                    if (location != null)
                    {
                        var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.Id,
                        location.DCId,
                        location.AccessHash);

                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (store.FileExists(fileName))
                            {
                                BitmapImage imageSource;

                                try
                                {
                                    using (var stream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                    {
                                        stream.Seek(0, SeekOrigin.Begin);
                                        var image = new BitmapImage();
                                        image.SetSource(stream);
                                        imageSource = image;
                                    }
                                }
                                catch (Exception)
                                {
                                    return null;
                                }

                                return imageSource;
                            }
                        }
                    }

                    var buffer = decryptedMediaDocument.Thumb.Data;
                    return ImageUtils.CreateImage(buffer);
                }

                var decryptedMediaPhoto = value as TLDecryptedMessageMediaPhoto;
                if (decryptedMediaPhoto != null)
                {
                    if (decryptedMediaPhoto.Bitmap != null)
                    {
                        var bitmap = decryptedMediaPhoto.Bitmap;
                        decryptedMediaPhoto.Bitmap = null;
                        return bitmap;
                    }
                }

                var file = decrypteMedia.File as TLEncryptedFile;
                if (file != null)
                {
                    if (!decrypteMedia.IsCanceled)
                    {
                        var isBackground = parameter != null && Equals(parameter, "Background");
                        return ReturnOrEnqueueImage(CheckChatSettings, file, decrypteMedia, decrypteMedia, isBackground);
                    }
                }
            }

            TLDecryptedMessageMediaExternalDocument decryptedMediaExternalDocument;
            var decryptedMessage = value as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                decryptedMediaExternalDocument = decryptedMessage.Media as TLDecryptedMessageMediaExternalDocument;
                if (decryptedMediaExternalDocument != null)
                {
#if WP8
                    return ReturnOrEnqueueSticker(decryptedMediaExternalDocument, decryptedMessage);
#endif

                    return null;
                }

            }

            decryptedMediaExternalDocument = value as TLDecryptedMessageMediaExternalDocument;
            if (decryptedMediaExternalDocument != null)
            {
#if WP8
                return ReturnOrEnqueueSticker(decryptedMediaExternalDocument, decryptedMediaExternalDocument);
#endif

                return null;
            }

            var photoMedia = value as TLMessageMediaPhoto;
            if (photoMedia != null)
            {
                if (photoMedia.Bitmap != null)
                {
                    var bitmap = photoMedia.Bitmap;
                    photoMedia.Bitmap = null;
                    return bitmap;
                }

                value = photoMedia.Photo;
            }

            var photo = value as TLPhoto;
            if (photo != null)
            {
                bool isBackground;
                var size = GetPhotoSize(parameter, photo, out isBackground);

                if (size != null)
                {
                    if (!string.IsNullOrEmpty(size.TempUrl))
                    {
                        if (photoMedia != null) photoMedia.DownloadingProgress = 0.01;
                        return size.TempUrl;
                    }

                    var location = size.Location as TLFileLocation;
                    if (location != null)
                    {
                        if (photoMedia == null
                            || !photoMedia.IsCanceled)
                        {
                            Action<DownloadableItem> callback = null;
                            if (photoMedia == null)
                            {
                                callback = item => Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                                {
                                    photo.NotifyOfPropertyChange(() => photo.Self);
                                });
                            }

                            return ReturnOrEnqueueImage(timer, CheckChatSettings, location, photo, size.Size, photoMedia, isBackground, callback);
                        }
                    }
                }
            }

#if WP8
            var inlineMediaResult = value as TLBotInlineMediaResult;
            if (inlineMediaResult != null)
            {
                if (TLString.Equals(inlineMediaResult.Type, new TLString("sticker"), StringComparison.OrdinalIgnoreCase))
                {
                    var document22 = inlineMediaResult.Document as TLDocument22;
                    if (document22 == null) return null;

                    var thumbCachedSize = document22.Thumb as TLPhotoCachedSize;
                    if (thumbCachedSize != null)
                    {
                        var fileName = "cached" + document22.GetFileName();
                        var buffer = thumbCachedSize.Bytes.Data;
                        if (buffer == null) return null;

                        return DecodeWebPImage(fileName, buffer, () => { });
                    }

                    var thumbPhotoSize = document22.Thumb as TLPhotoSize;
                    if (thumbPhotoSize != null)
                    {
                        var location = thumbPhotoSize.Location as TLFileLocation;
                        if (location != null)
                        {
                            return ReturnOrEnqueueStickerPreview(location, inlineMediaResult, thumbPhotoSize.Size);
                        }
                    }

                    if (TLMessageBase.IsSticker(document22))
                    {
                        return ReturnOrEnqueueSticker(document22, inlineMediaResult);
                    }
                }
            }

            var sticker = value as TLStickerItem;
            if (sticker != null)
            {
                var document22 = sticker.Document as TLDocument22;
                if (document22 == null) return null;

                var thumbCachedSize = document22.Thumb as TLPhotoCachedSize;
                if (thumbCachedSize != null)
                {
                    var fileName = "cached" + document22.GetFileName();
                    var buffer = thumbCachedSize.Bytes.Data;
                    if (buffer == null) return null;

                    return DecodeWebPImage(fileName, buffer, () => { });
                }

                var thumbPhotoSize = document22.Thumb as TLPhotoSize;
                if (thumbPhotoSize != null)
                {
                    var location = thumbPhotoSize.Location as TLFileLocation;
                    if (location != null)
                    {
                        return ReturnOrEnqueueStickerPreview(location, sticker, thumbPhotoSize.Size);
                    }
                }

                if (TLMessageBase.IsSticker(document22))
                {
                    return ReturnOrEnqueueSticker(document22, sticker);
                }
            }
#endif

            var document = value as TLDocument;
            if (document != null)
            {
#if WP8
                if (TLMessageBase.IsSticker(document))
                {
                    if (parameter != null &&
                        string.Equals(parameter.ToString(), "ignoreStickers", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    return ReturnOrEnqueueSticker((TLDocument22)document, null);
                }
#endif

                var thumbPhotoSize = document.Thumb as TLPhotoSize;
                if (thumbPhotoSize != null)
                {
                    var location = thumbPhotoSize.Location as TLFileLocation;
                    if (location != null)
                    {
                        return ReturnOrEnqueueImage(timer, false, location, document, thumbPhotoSize.Size, null);
                    }
                }

                var thumbCachedSize = document.Thumb as TLPhotoCachedSize;
                if (thumbCachedSize != null)
                {
                    var buffer = thumbCachedSize.Bytes.Data;

                    return ImageUtils.CreateImage(buffer);
                }
            }

            var videoMedia = value as TLMessageMediaVideo;
            if (videoMedia != null)
            {
                value = videoMedia.Video;
            }

            var video = value as TLVideo;
            if (video != null)
            {
                var thumbPhotoSize = video.Thumb as TLPhotoSize;

                if (thumbPhotoSize != null)
                {
                    var location = thumbPhotoSize.Location as TLFileLocation;
                    if (location != null)
                    {
                        return ReturnOrEnqueueImage(timer, false, location, video, thumbPhotoSize.Size, null);
                    }
                }

                var thumbCachedSize = video.Thumb as TLPhotoCachedSize;
                if (thumbCachedSize != null)
                {
                    var buffer = thumbCachedSize.Bytes.Data;
                    return ImageUtils.CreateImage(buffer);
                }
            }

            var invoiceMedia = value as TLMessageMediaInvoice;
            if (invoiceMedia != null)
            {

            }

            var webPageMedia = value as TLMessageMediaWebPage;
            if (webPageMedia != null)
            {
                value = webPageMedia.WebPage;
            }

            var decryptedWebPageMedia = value as TLDecryptedMessageMediaWebPage;
            if (decryptedWebPageMedia != null)
            {
                value = decryptedWebPageMedia.WebPage;
            }

            var webPage = value as TLWebPage;
            if (webPage != null)
            {
                var webPagePhoto = webPage.Photo as TLPhoto;
                if (webPagePhoto != null)
                {
                    var width = 311.0;
                    double result;
                    if (Double.TryParse((string)parameter, out result))
                    {
                        width = result;
                    }

                    TLPhotoSize size = null;
                    var sizes = webPagePhoto.Sizes.OfType<TLPhotoSize>();
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
                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            return ReturnOrEnqueueImage(timer, false, location, webPage, size.Size, null);
                        }
                    }
                }
            }

            var gameMedia = value as TLMessageMediaGame;
            if (gameMedia != null)
            {
                value = gameMedia.Game;
            }

            //var decryptedWebPageMedia = value as TLDecryptedMessageMediaWebPage;
            //if (decryptedWebPageMedia != null)
            //{
            //    value = decryptedWebPageMedia.WebPage;
            //}

            var game = value as TLGame;
            if (game != null)
            {
                var gamePhoto = game.Photo as TLPhoto;
                if (gamePhoto != null)
                {
                    var width = 311.0;
                    double result;
                    if (Double.TryParse((string)parameter, out result))
                    {
                        width = result;
                    }

                    TLPhotoSize size = null;
                    var sizes = gamePhoto.Sizes.OfType<TLPhotoSize>();
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
                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            return ReturnOrEnqueueImage(timer, false, location, game, size.Size, null);
                        }
                    }
                }
            }

            return null;
        }

        private static TLPhotoSize GetPhotoSize(object parameter, TLPhoto photo, out bool isBackground)
        {
            var widthParameter = string.Empty;
            isBackground = false;
            if (parameter != null)
            {
                var p = parameter.ToString().Split('_');
                if (p.Length > 0)
                {
                    widthParameter = p[0];
                }
                if (p.Length > 1)
                {
                    isBackground = string.Equals(p[1], "background", StringComparison.OrdinalIgnoreCase);
                }
            }

            var width = 311.0;
            double result;
            if (!string.IsNullOrEmpty(widthParameter) && double.TryParse(widthParameter, out result))
            {
                width = result;
            }

            TLPhotoSize size = null;
            var sizes = photo.Sizes.OfType<TLPhotoSize>();
            foreach (var photoSize in sizes)
            {
                if (photoSize.W.Value > 90
                    && (size == null || Math.Abs(width - size.W.Value) > Math.Abs(width - photoSize.W.Value)))
                {
                    size = photoSize;
                }
            }

            return size;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageViewerPhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var photoMedia = value as TLMessageMediaPhoto;
            if (photoMedia != null)
            {
                value = photoMedia.Photo;
            }

            var webPageMedia = value as TLMessageMediaWebPage;
            if (webPageMedia != null)
            {
                var webPage = webPageMedia.WebPage as TLWebPage;
                if (webPage != null)
                {
                    value = webPage.Photo;
                }
            }

            var photo = value as TLPhoto;
            if (photo != null)
            {
                var width = 311.0;
                BitmapSource image = ReturnImageBySize(photo, width);
                if (image != null) return image;

                if (webPageMedia != null)
                {
                    width = 99.0;
                    image = ReturnImageBySize(photo, width);
                    if (image != null) return image;
                }
                else if (photoMedia != null)
                {
                    image = new PhotoToThumbConverter().Convert(photoMedia, targetType, parameter, culture) as BitmapSource;
                }
                return image;
            }

            var documentMedia = value as TLMessageMediaDocument;
            if (documentMedia != null)
            {
                return new PhotoToThumbConverter().Convert(documentMedia, targetType, parameter, culture);
            }

            return null;
        }

        private static BitmapImage ReturnImageBySize(TLPhoto photo, double width)
        {
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
                var location = size.Location as TLFileLocation;
                if (location != null)
                {
                    var timer = Stopwatch.StartNew();
                    var image = DefaultPhotoConverter.ReturnImage(timer, location);
                    if (image != null)
                    {
                        return image;
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class PhotoThumbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return new DefaultPhotoConverter().Convert(value, targetType, parameter, culture);

            BitmapImage image = null;

            if (value != null)
            {
                var photoFile = value as PhotoFile;
                if (photoFile != null)
                {
                    var thumbnail = photoFile.Thumbnail;
                    if (thumbnail != null)
                    {
                        image = new BitmapImage();
                        image.CreateOptions = BitmapCreateOptions.None;
                        image.SetSource(thumbnail);

                        Deployment.Current.Dispatcher.BeginInvoke(() => MultiImageEditorView.ImageOpened(photoFile));
                    }
                    else
                    {
                        //Task.Factory.StartNew(async () =>
                        //{
                        //    thumbnail = await photoFile.File.GetThumbnailAsync(ThumbnailMode.ListView, 99, ThumbnailOptions.None);
                        //    photoFile.Thumbnail = thumbnail;
                        //    Deployment.Current.Dispatcher.BeginInvoke(() => photoFile.RaisePropertyChanged("Self"));
                        //});
                    }
                }
            }



            return (image);
        }

        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    throw new NotImplementedException();
        //}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

            //var thumbnail = await item.File.GetThumbnailAsync(ThumbnailMode.ListView, 99, ThumbnailOptions.None);
            //item.Thumbnail = thumbnail;
            //item.RaisePropertyChanged("Thumbnail");
        }
    }

    public class PhotoFileToTemplateConverter : IValueConverter
    {
        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate ButtonTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var photoFile = value as PhotoFile;
            if (photoFile != null)
            {
                return photoFile.IsButton ? ButtonTemplate : PhotoTemplate;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// To avoid memory leaks with ImageOpened, ImageFailed events
    /// Check for details:
    /// http://blogs.developpeur.org/kookiz/archive/2013/02/17/wpdev-memory-leak-with-bitmapimage.aspx 
    public class BitmapImageWrapper
    {
        public BitmapImage Image { get; protected set; }

        public string FileName { get; protected set; }

        private Action<BitmapImageWrapper> _imageOpened;

        private Action<BitmapImageWrapper> _imageFailed;

        public BitmapImageWrapper(string fileName, BitmapImage image)
        {
            FileName = fileName;
            Image = image;
        }

        //~BitmapImageWrapper()
        //{

        //}

        public void Subscribe(Action<BitmapImageWrapper> imageOpened, Action<BitmapImageWrapper> imageFailed = null)
        {
            Image.ImageOpened += OnImageOpened;
            Image.ImageFailed += OnImageFailed;

            _imageOpened = imageOpened;
            _imageFailed = imageFailed;
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Image.ImageOpened -= OnImageOpened;
            Image.ImageFailed -= OnImageFailed;

            _imageFailed.SafeInvoke(this);
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            Image.ImageOpened -= OnImageOpened;
            Image.ImageFailed -= OnImageFailed;

            _imageOpened.SafeInvoke(this);
        }
    }
}
