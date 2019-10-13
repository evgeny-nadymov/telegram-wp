// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define INAPP_CAMERA
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.DeviceInfo;
using Telegram.Api.TL;
using TelegramClient.Extensions;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;
#if WP81
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
#endif
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Media;
using Binding = System.Windows.Data.Binding;
using Execute = Telegram.Api.Helpers.Execute;
using PhotoFile = TelegramClient.Views.Controls.PhotoFile;
using TaskResult = Microsoft.Phone.Tasks.TaskResult;

namespace TelegramClient.ViewModels.Additional
{
    public class ChooseAttachmentViewModel : ViewAware
    {

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    NotifyOfPropertyChange(() => IsOpen);
                }
            }
        }

        private readonly bool _contactEnabled;

        public Visibility OpenContactVisibility
        {
            get
            {
                return _contactEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility OpenCamcorderVisibility
        {
            get
            {
#if WP81
                return _contactEnabled ? Visibility.Visible : Visibility.Collapsed;
#else
                return Visibility.Collapsed;
#endif
            }
        }

        private readonly ICacheService _cacheService;

        private readonly IStateService _stateService;

        private readonly INavigationService _navigationService;

        private readonly ITelegramEventAggregator _eventAggregator;

        private readonly TLObject _with;

        public ObservableCollection<TLUserBase> InlineBots { get; set; }

        private readonly Action<TLUserBase> _openInlineBotAction;

        private readonly Action<StorageFile> _sendDocumentAction;

        private readonly Action<StorageFile> _sendVideoAction;

        private readonly Action<IReadOnlyList<StorageFile>> _sendPhotosAction;

        private readonly System.Action _sendLocationAction;

        private readonly System.Action _sendContactAction;

        public ChooseAttachmentViewModel(TLObject with,
            Action<TLUserBase> openInlineBotAction,
            Action<StorageFile> sendDocumentAction,
            Action<StorageFile> sendVideoAction,
            Action<IReadOnlyList<StorageFile>> sendPhotosAction,
            System.Action sendLocationAction,
            System.Action sendContactAction,
            ICacheService cacheService, ITelegramEventAggregator eventAggregator, INavigationService navigationService, IStateService stateService, bool contactEnabled = true)
        {
            InlineBots = new ObservableCollection<TLUserBase>();

            _with = with;
            _openInlineBotAction = openInlineBotAction;
            _sendDocumentAction = sendDocumentAction;
            _sendVideoAction = sendVideoAction;
            _sendPhotosAction = sendPhotosAction;
            _sendLocationAction = sendLocationAction;
            _sendContactAction = sendContactAction;

            _cacheService = cacheService;
            _stateService = stateService;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;

            _contactEnabled = contactEnabled;
            _eventAggregator.Subscribe(this);
        }

        public void OpenLocation()
        {
            IsOpen = false;

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                _sendLocationAction.SafeInvoke();
            });
        }

        public void OpenCamera()
        {
            IsOpen = false;

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), async () =>
            {
                var cameraSettings = IoC.Get<IStateService>().GetCameraSettings();
                if (cameraSettings != null && !cameraSettings.External)
                {

                    SystemTray.IsVisible = false;
                    var frame = Application.Current.RootVisual as PhoneApplicationFrame;

                    double width = 0.0;
                    double height = 0.0;
                    PhoneApplicationPage page = null;
                    if (frame != null)
                    {
                        page = frame.Content as PhoneApplicationPage;
                        if (page != null)
                        {
                            width = page.ActualWidth;
                            height = page.ActualHeight;
                            page.IsHitTestVisible = false;
                            //page.Visibility = Visibility.Collapsed;
                            page.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
                        }
                    }

                    var cameraPage = new CameraPage();

                    var binding = new Binding();
                    binding.Source = page;
                    binding.Path = new PropertyPath("Orientation");
                    cameraPage.SetBinding(CameraPage.ParentPageOrientationProperty, binding);

                    cameraPage.Width = width;
                    cameraPage.Height = height;

                    var telegramMessageBox = new TelegramMessageBox { IsFullScreen = true };
                    telegramMessageBox.Content = cameraPage;

                    var photoCaptured = false;
                    cameraPage.PhotoCaptured += (sender, args) =>
                    {
                        if (args.File != null)
                        {
                            _sendPhotosAction.SafeInvoke(new ReadOnlyCollection<StorageFile>(new[] { args.File }));
                        }
                        else
                        {
                            photoCaptured = true;
                            telegramMessageBox.Dismiss();
                        }
                    };
                    cameraPage.VideoCaptured += (sender, args) =>
                    {
                        telegramMessageBox.Dismiss();

                        var dialogDetails = page as IDialogDetailsView;
                        if (dialogDetails != null)
                        {
                            dialogDetails.CreateBitmapCache(() => { });
                        }

                        Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () => _sendVideoAction.SafeInvoke(args.File));
                    };
                    telegramMessageBox.Show();
                    telegramMessageBox.Dismissing += (sender, args) =>
                    {

                    };
                    telegramMessageBox.Dismissed += (sender, args) =>
                    {
                        SystemTray.IsVisible = true;

                        frame = Application.Current.RootVisual as PhoneApplicationFrame;
                        if (frame != null)
                        {
                            page = frame.Content as PhoneApplicationPage;
                            if (page != null)
                            {
                                page.IsHitTestVisible = true;
                                //page.Visibility = Visibility.Visible;
                                page.SupportedOrientations = SupportedPageOrientation.Portrait;
                            }
                        }

                        cameraPage.ClearValue(CameraPage.ParentPageOrientationProperty);
                        if (!photoCaptured) cameraPage.Dispose();
                    };
                }
                else
                {
                    if (Environment.OSVersion.Version.Major >= 10)
                    {
                        object ccu = null;
                        var _type10 = Type.GetType("Windows.Media.Capture.CameraCaptureUI, Windows, ContentType=WindowsRuntime");
                        if (_type10 != null)
                        {
                            ccu = Activator.CreateInstance(_type10);
                        }
                        if (ccu == null) return;
                        var mode = CameraCaptureUIMode.Photo;
                        var modeType = Type.GetType("Windows.Media.Capture.CameraCaptureUIMode, Windows, ContentType=WindowsRuntime");
                        object modeVal = Enum.ToObject(modeType, mode);
                        try
                        {
                            var file = await (Windows.Foundation.IAsyncOperation<StorageFile>)_type10.GetRuntimeMethod("CaptureFileAsync", new Type[] { modeType }).Invoke(ccu, new object[] { modeVal });
                            if (file != null)
                            {
                                _sendPhotosAction.SafeInvoke(new ReadOnlyCollection<StorageFile>(new[] { file }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Execute.ShowDebugMessage("CameraCaptureTask.OnCompleted ex " + ex);
                        }
                    }
                    else
                    {
                        ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                        var task = new CameraCaptureTask();
                        task.Completed += (o, e) =>
                        {
                            if (e.TaskResult != TaskResult.OK)
                            {
                                return;
                            }

                            try
                            {
                                var getFileTask = StorageFile.GetFileFromPathAsync(e.OriginalFileName).AsTask();
                                var file = getFileTask.Result;
                                App.Photos = new ReadOnlyCollection<StorageFile>(new[] { file });
                            }
                            catch (Exception ex)
                            {
                                Execute.ShowDebugMessage("CameraCaptureTask.OnCompleted ex " + ex);
                            }
                        };
                        task.Show();
                    }
                }
            });
        }

        public void OpenPhoto()
        {
            IsOpen = false;

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), async () =>
            {
#if WP81
                var photoPickerSettings = IoC.Get<IStateService>().GetPhotoPickerSettings();
                if (photoPickerSettings != null && photoPickerSettings.External)
                {
                    var from = _contactEnabled ? "DialogDetailsView" : "SecretDialogDetailsView";

                    ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                    var fileOpenPicker = new FileOpenPicker();
                    fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                    fileOpenPicker.FileTypeFilter.Clear();
                    fileOpenPicker.FileTypeFilter.Add(".bmp");
                    fileOpenPicker.FileTypeFilter.Add(".png");
                    fileOpenPicker.FileTypeFilter.Add(".jpeg");
                    fileOpenPicker.FileTypeFilter.Add(".jpg");
                    fileOpenPicker.ContinuationData.Add("From", from);
                    fileOpenPicker.ContinuationData.Add("Type", "Image");

                    if (Environment.OSVersion.Version.Major >= 10)
                    {
                        var result = await fileOpenPicker.PickMultipleFilesAsync();
                        if (result.Count > 0)
                        {
                            Execute.BeginOnThreadPool(() =>
                            {
                                _sendPhotosAction.SafeInvoke(result);
                            });
                        }
                    }
                    else
                    {
                        fileOpenPicker.PickMultipleFilesAndContinue();
                    }
                }
                else
                {
                    OpenPhotoPicker(false, (r1, r2) => _sendPhotosAction.SafeInvoke(r1));
                }
#else
                        ((App) Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                        var task = new PhotoChooserTask { ShowCamera = true };
                        task.Completed += (o, e) => Handle(_stateService, e.ChosenPhoto, e.OriginalFileName);
                        task.Show();
#endif
            });
        }

        private static OpenPhotoPicker _openPhotoPicker;

        public static void OpenPhotoPicker(bool isSingleItem, Action<IReadOnlyList<StorageFile>, IReadOnlyCollection<PhotoFile>> callback)
        {
            var isVisible = false;
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            PhoneApplicationPage page = null;
            if (frame != null)
            {
                page = frame.Content as PhoneApplicationPage;
                if (page != null)
                {
                    page.IsHitTestVisible = false;
                    var applicationBar = page.ApplicationBar;
                    if (applicationBar != null)
                    {
                        isVisible = applicationBar.IsVisible;
                        applicationBar.IsVisible = false;
                    }
                }
            }

            if (page == null) return;

            var popup = new Popup();
            var photoPicker = new OpenPhotoPicker
            {
                IsSingleItem = isSingleItem,
                Width = page.ActualWidth,
                Height = page.ActualHeight
            };
            _openPhotoPicker = photoPicker;
            page.SizeChanged += Page_SizeChanged;

            photoPicker.Close += (sender, args) =>
            {
                _openPhotoPicker = null;
                popup.IsOpen = false;
                popup.Child = null;

                frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    page = frame.Content as PhoneApplicationPage;
                    if (page != null)
                    {
                        page.SizeChanged -= Page_SizeChanged;
                        page.IsHitTestVisible = true;
                        var applicationBar = page.ApplicationBar;
                        if (applicationBar != null)
                        {
                            applicationBar.IsVisible = isVisible;
                        }
                    }
                }
            };
            photoPicker.Pick += (sender, args) =>
            {
                callback.SafeInvoke(args.Files, args.PhotoFiles);

                photoPicker.TryClose();
            };

            popup.Child = photoPicker;
            popup.IsOpen = true;
        }

        private static void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_openPhotoPicker != null)
            {
                _openPhotoPicker.Width = e.NewSize.Width;
                _openPhotoPicker.Height = e.NewSize.Height;
            }
        }

        public void OpenVideo()
        {
            IsOpen = false;

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), async () =>
            {
#if WP81
                if (_contactEnabled)
                {
                    ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                    var fileOpenPicker = new FileOpenPicker();
                    fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                    fileOpenPicker.FileTypeFilter.Add(".wmv");
                    fileOpenPicker.FileTypeFilter.Add(".mp4");
                    fileOpenPicker.FileTypeFilter.Add(".avi");

                    fileOpenPicker.ContinuationData.Add("From", "DialogDetailsView");
                    fileOpenPicker.ContinuationData.Add("Type", "Video");

                    if (Environment.OSVersion.Version.Major >= 10)
                    {
                        var result = await fileOpenPicker.PickSingleFileAsync();
                        if (result != null)
                        {
                            Execute.BeginOnThreadPool(() =>
                            {
                                _sendVideoAction.SafeInvoke(result);
                            });
                        }
                    }
                    else
                    {
                        fileOpenPicker.PickSingleFileAndContinue();
                    }
                }
                else
                {
                    _navigationService.UriFor<VideoCaptureViewModel>().Navigate();
                }
#else
                _navigationService.UriFor<VideoCaptureViewModel>().Navigate();
#endif
            });
        }

        public void OpenCamcorder()
        {
            IsOpen = false;
            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                _navigationService.UriFor<VideoCaptureViewModel>().Navigate();
            });
        }

        public void OpenContact()
        {
            IsOpen = false;

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                _sendContactAction.SafeInvoke();
            });
        }

        public void OpenDocument()
        {
            IsOpen = false;

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), async () =>
            {
#if WP81
                if (_contactEnabled)
                {
                    ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                    var fileOpenPicker = new FileOpenPicker();
                    //fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    fileOpenPicker.FileTypeFilter.Add("*");
                    fileOpenPicker.ContinuationData.Add("From", "DialogDetailsView");
                    fileOpenPicker.ContinuationData.Add("Type", "Document");

                    if (Environment.OSVersion.Version.Major >= 10)
                    {
                        var result = await fileOpenPicker.PickSingleFileAsync();
                        if (result != null)
                        {
                            Execute.BeginOnThreadPool(() =>
                            {
                                _sendDocumentAction.SafeInvoke(result);
                            });
                        }
                    }
                    else
                    {
                        fileOpenPicker.PickSingleFileAndContinue();
                    }
                }
                else
                {
                    ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                    var fileOpenPicker = new FileOpenPicker();
                    //fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    fileOpenPicker.FileTypeFilter.Add("*");
                    fileOpenPicker.ContinuationData.Add("From", "SecretDialogDetailsView");
                    fileOpenPicker.ContinuationData.Add("Type", "Document");

                    if (Environment.OSVersion.Version.Major >= 10)
                    {
                        var result = await fileOpenPicker.PickSingleFileAsync();
                        if (result != null)
                        {
                            Execute.BeginOnThreadPool(() =>
                            {
                                _sendDocumentAction.SafeInvoke(result);
                            });
                        }
                    }
                    else
                    {
                        fileOpenPicker.PickSingleFileAndContinue();
                    }
                }
#else
                    ((App) Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                    var task = new PhotoChooserTask { ShowCamera = true };
                    task.Completed += (o, e) => Handle(_stateService, e.ChosenPhoto, e.OriginalFileName);
                    task.Show();
#endif
            });
        }

        //private static bool GetAngleFromExif(Stream imageStream, out int angle)
        //{
        //    angle = 0;
        //    var position = imageStream.Position;

        //    imageStream.Position = 0;
        //    var info = ExifReader.ReadJpeg(imageStream);
        //    imageStream.Position = position;

        //    if (!info.IsValid)
        //    {
        //        return false;
        //    }

        //    var orientation = info.Orientation;
        //    switch (orientation)
        //    {
        //        case ExifOrientation.TopRight:
        //            angle = 90;
        //            break;
        //        case ExifOrientation.BottomRight:
        //            angle = 180;
        //            break;
        //        case ExifOrientation.BottomLeft:
        //            angle = 270;
        //            break;
        //        case ExifOrientation.TopLeft:
        //        case ExifOrientation.Undefined:
        //        default:
        //            angle = 0;
        //            break;
        //    }

        //    return true;
        //}

        private static WriteableBitmap RotateBitmap(WriteableBitmap source, int width, int height, int angle)
        {
            var target = new WriteableBitmap(width, height);
            int sourceIndex = 0;
            int targetIndex = 0;
            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    sourceIndex = x + y * source.PixelWidth;
                    switch (angle)
                    {
                        case 90:
                            targetIndex = (source.PixelHeight - y - 1)
                                + x * target.PixelWidth;
                            break;
                        case 180:
                            targetIndex = (source.PixelWidth - x - 1)
                                + (source.PixelHeight - y - 1) * source.PixelWidth;
                            break;
                        case 270:
                            targetIndex = y + (source.PixelWidth - x - 1)
                                * target.PixelWidth;
                            break;
                    }
                    target.Pixels[targetIndex] = source.Pixels[sourceIndex];
                }
            }
            return target;
        }

        private static WriteableBitmap DecodeImage(Stream imageStream, int angle)
        {
            var source = PictureDecoder.DecodeJpeg(imageStream);

            switch (angle)
            {
                case 90:
                case 270:
                    return RotateBitmap(source, source.PixelHeight, source.PixelWidth, angle);
                case 180:
                    return RotateBitmap(source, source.PixelWidth, source.PixelHeight, angle);
                default:
                    return source;
            }
        }


#if WP81

        public static async Task<Photo> ResizeJpeg(IRandomAccessStream stream, uint size, string originalFileName)
        {
            Photo photo;
            using (var sourceStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(sourceStream);

                if (decoder.DecoderInformation != null
                    && decoder.DecoderInformation.CodecId == BitmapDecoder.JpegDecoderId)
                {
                    var maxDimension = Math.Max(decoder.PixelWidth, decoder.PixelHeight);
                    var scale = (double)size / maxDimension;
                    var orientedScaledHeight = (uint)(decoder.OrientedPixelHeight * scale);
                    var orientedScaledWidth = (uint)(decoder.OrientedPixelWidth * scale);
                    var scaledHeight = (uint)(decoder.PixelHeight * scale);
                    var scaledWidth = (uint)(decoder.PixelWidth * scale);

                    var transform = new BitmapTransform { ScaledHeight = scaledHeight, ScaledWidth = scaledWidth, InterpolationMode = BitmapInterpolationMode.Fant };
                    var pixelData = await decoder.GetPixelDataAsync(
                        decoder.BitmapPixelFormat,
                        decoder.BitmapAlphaMode,
                        transform,
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.DoNotColorManage);

                    using (var destinationStream = new InMemoryRandomAccessStream())
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, destinationStream);
                        encoder.SetPixelData(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode, orientedScaledWidth, orientedScaledHeight, decoder.DpiX, decoder.DpiY, pixelData.DetachPixelData());
                        await encoder.FlushAsync();

                        var reader = new DataReader(destinationStream.GetInputStreamAt(0));
                        var bytes = new byte[destinationStream.Size];
                        await reader.LoadAsync((uint)destinationStream.Size);
                        reader.ReadBytes(bytes);

                        photo = new Photo
                        {
                            Bytes = bytes,
                            Width = (int)orientedScaledWidth,
                            Height = (int)orientedScaledHeight,
                            FileName = originalFileName
                        };
                    }
                }
                else
                {
                    var reader = new DataReader(stream.GetInputStreamAt(0));
                    var bytes = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);

                    photo = new Photo
                    {
                        Bytes = bytes,
                        Width = (int)decoder.OrientedPixelWidth,
                        Height = (int)decoder.OrientedPixelHeight,
                        FileName = originalFileName
                    };
                }
            }

            return photo;
        }

        public static async Task Handle(IStateService stateService, IRandomAccessStream chosenPhoto, string originalFileName)
        {

            var log = new StringBuilder();
            var stopwatch = Stopwatch.StartNew();

            var photo = await ResizeJpeg(chosenPhoto, 1280, originalFileName);
            log.AppendLine("save_jpeg " + stopwatch.Elapsed);
            stateService.Photo = photo;

            //Execute.ShowDebugMessage(log.ToString());
        }

#endif

        public static void Handle(IStateService stateService, Stream chosenPhoto, string originalFileName)
        {
            //var log = new StringBuilder();
            //var stopwatch = Stopwatch.StartNew();
            //WriteableBitmap writeableBitmap;
            //int angle;
            //var result = GetAngleFromExif(chosenPhoto, out angle);
            //log.AppendLine("get_angle result=" + angle + " " + stopwatch.Elapsed);
            //if (result)
            //{
            //    writeableBitmap = DecodeImage(chosenPhoto, angle);
            //    log.AppendLine("decode_image " + stopwatch.Elapsed);
            //}
            //else
            //{
            //    var bitmap = new BitmapImage { CreateOptions = BitmapCreateOptions.None };
            //    bitmap.SetSource(chosenPhoto);
            //    writeableBitmap = new WriteableBitmap(bitmap);
            //    log.AppendLine("writeable_bitmap " + stopwatch.Elapsed);
            //}

            //var maxDimension = Math.Max(writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
            //var scale = 1280.0 / maxDimension;
            //var newHeight = writeableBitmap.PixelHeight * scale;
            //var newWidth = writeableBitmap.PixelWidth * scale;
            //var ms = new MemoryStream();
            //writeableBitmap.SaveJpeg(ms, (int)newWidth, (int)newHeight, 0, 87);

            //log.AppendLine("save_jpeg " + stopwatch.Elapsed);

            //stateService.Photo = new Photo
            //{
            //    FileName = originalFileName,
            //    Bytes = ms.ToArray(),
            //    Width = (int)newWidth,
            //    Height = (int)newHeight
            //};

            //Execute.ShowDebugMessage(log.ToString());
        }

        public void Open()
        {
            IsOpen = true;

            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                var view = frame.Content as IDialogDetailsView;
                if (view != null)
                {
                    view.PauseChatPlayers();
                }
            }
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void OpenInlineBot(TLUserBase bot)
        {
            IsOpen = false;
            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () => _openInlineBotAction.SafeInvoke(bot));
        }

        private static DateTime? _lastUpdateTopPeerTime;

        public bool LoadInlineBots(System.Action callback)
        {
            var topPeers = IoC.Get<IStateService>().GetTopPeers() as TLTopPeers;
            if (topPeers != null)
            {
                if (_lastUpdateTopPeerTime == null || _lastUpdateTopPeerTime.Value.AddSeconds(300.0) < DateTime.Now)
                {
                    SearchViewModel.UpdateTopCommon(IoC.Get<IMTProtoService>(), IoC.Get<IStateService>(), result => Execute.BeginOnUIThread(() =>
                    {
                        _lastUpdateTopPeerTime = DateTime.Now;
                    }));
                }
            }

            if (InlineBots.Count > 0) return false;

            if (topPeers != null)
            {
                LoadInlineBotsInternal(topPeers);
                return false;
            }
            else
            {
                SearchViewModel.UpdateTopCommon(IoC.Get<IMTProtoService>(), IoC.Get<IStateService>(), result => Execute.BeginOnUIThread(() =>
                {
                    _lastUpdateTopPeerTime = DateTime.Now;
                    LoadInlineBotsInternal(result, callback);
                }));
                return true;
            }
        }

        private void LoadInlineBotsInternal(TLTopPeers topPeers, System.Action callback = null)
        {
            var inlineBotsCategory = topPeers.Categories.FirstOrDefault(x => x.Category is TLTopPeerCategoryBotsInline);
            if (inlineBotsCategory != null)
            {
                var inlineBots = new List<TLUserBase>();
                foreach (var peer in inlineBotsCategory.Peers)
                {
                    var user = IoC.Get<ICacheService>().GetUser(peer.Peer.Id);
                    if (user != null)
                    {
                        inlineBots.Add(user);
                    }
                }

                const int firstSliceCount = 5;
                for (var i = 0; i < inlineBots.Count && InlineBots.Count < firstSliceCount; i++)
                {
                    InlineBots.Add(inlineBots[i]);
                }

                var view = GetView() as IChooseAttachmentView;
                if (view != null)
                {
                    view.HideInlineBots();
                    view.ShowHint(AppResources.OpenInlineBotsHint);
                }

                callback.SafeInvoke();

                Execute.BeginOnUIThread(() =>
                {
                    for (var i = firstSliceCount; i < inlineBots.Count; i++)
                    {
                        InlineBots.Add(inlineBots[i]);
                    }
                });
            }
        }

        public void ResetTopPeerRating(TLObject obj)
        {
            var user = obj as TLUser;
            if (user == null) return;

            var confirmation = MessageBox.Show(string.Format(AppResources.ConfirmResetTopPeerRating, user.FullName), AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            var topPeerCategory = new TLTopPeerCategoryBotsInline();

            IoC.Get<IMTProtoService>().ResetTopPeerRatingAsync(topPeerCategory, user.ToInputPeer(),
                result => Execute.BeginOnUIThread(() =>
                {
                    var topPeers = IoC.Get<IStateService>().GetTopPeers() as TLTopPeers;
                    if (topPeers != null)
                    {
                        var category = topPeers.Categories.FirstOrDefault(x => x.Category.GetType() == topPeerCategory.GetType());
                        if (category != null)
                        {
                            for (var i = 0; i < category.Peers.Count; i++)
                            {
                                if (category.Peers[i].Peer.Id.Value == user.Index)
                                {
                                    category.Peers.RemoveAt(i);
                                    break;
                                }
                            }

                            for (var i = 0; i < topPeers.Users.Count; i++)
                            {
                                if (topPeers.Users[i].Index == user.Index)
                                {
                                    topPeers.Users.RemoveAt(i);
                                    break;
                                }
                            }

                            for (var i = 0; i < InlineBots.Count; i++)
                            {
                                var topUser = InlineBots[i];
                                if (topUser != null)
                                {
                                    if (topUser.Index == user.Index)
                                    {
                                        InlineBots.RemoveAt(i);
                                        break;
                                    }
                                }
                            }

                            IoC.Get<IStateService>().SaveTopPeers(topPeers);
                        }
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("contacts.resetTopPeerRating error " + error);
                }));
        }
    }

    /// <summary>
    /// Determines whether the user interface for capturing from the attached camera allows capture of photos, videos, or both photos and videos.
    /// </summary>
    public enum CameraCaptureUIMode
    {
        /// <summary>
        /// Either a photo or video can be captured.
        /// </summary>
        PhotoOrVideo = 0,

        /// <summary>
        /// The user can only capture a photo.
        /// </summary>
        Photo = 1,

        /// <summary>
        /// The user can only capture a video. 
        /// </summary>
        Video = 2,
    }
}
