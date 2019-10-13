// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Phone.Media.Capture;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Caliburn.Micro;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Telegram.Api.TL;
using TelegramClient.Resources;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using MediaStreamType = Windows.Media.Capture.MediaStreamType;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;

namespace TelegramClient.Views.Controls
{
    public partial class CameraPage
    {
        public event EventHandler<FileEventArgs> VideoCaptured;

        protected virtual void RaiseVideoCaptured(FileEventArgs e)
        {
            var handler = VideoCaptured;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<FileEventArgs> PhotoCaptured;

        protected virtual void RaisePhotoCaptured(FileEventArgs e)
        {
            var handler = PhotoCaptured;
            if (handler != null) handler(this, e);
        }

        private bool _mediaCaptureManagerInitialized;

        private LowLagPhotoCapture _lowLagPhotoCapture;

        private LowLagMediaRecording _lowLagMediaRecording;

        private MediaCapture _mediaCaptureManager;

        private MediaCapturePreviewSink _previewSink;

        private MediaEncodingProfile _currentProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);

        private PageOrientation _previousOrientation;

        public CameraPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            _timer.Tick += TimerOnTick;

            LayoutUpdated += OnLayoutUpdated;

            _previousOrientation = PageOrientation.PortraitUp;
            Loaded += (sender, args) =>
            {
                CameraButtons.ShutterKeyPressed += CameraButtonsOnShutterKeyPressed;
                CameraButtons.ShutterKeyHalfPressed += CameraButtonsOnShutterKeyHalfPressed;
                //Windows.Graphics.Display.DisplayOrientations.
                DisplayProperties.OrientationChanged += DisplayProperties_OrientationChanged;
            };
            Unloaded += (sender, args) =>
            {
                CameraButtons.ShutterKeyPressed -= CameraButtonsOnShutterKeyPressed;
                CameraButtons.ShutterKeyHalfPressed -= CameraButtonsOnShutterKeyHalfPressed;
                DisplayProperties.OrientationChanged -= DisplayProperties_OrientationChanged;
            };
        }

        private void DisplayProperties_OrientationChanged(object sender)
        {
            System.Diagnostics.Debug.WriteLine("current={0} native={1}", DisplayProperties.CurrentOrientation, DisplayProperties.NativeOrientation);
        }

        ~CameraPage()
        {
            
        }

        public static readonly DependencyProperty ParentPageOrientationProperty = DependencyProperty.Register(
            "ParentPageOrientation", typeof (PageOrientation), typeof (CameraPage), new PropertyMetadata(default(PageOrientation), OnParentPageOrientationChanged));

        private static void OnParentPageOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cameraPage = d as CameraPage;
            if (cameraPage != null)
            {
                cameraPage.MainPage_OnOrientationChanged(cameraPage, new OrientationChangedEventArgs((PageOrientation)e.NewValue));
            }
        }

        public PageOrientation ParentPageOrientation
        {
            get { return (PageOrientation) GetValue(ParentPageOrientationProperty); }
            set { SetValue(ParentPageOrientationProperty, value); }
        }

        private void OnLayoutUpdated(object sender, System.EventArgs e)
        {
            LayoutUpdated -= OnLayoutUpdated;

            InitializeCameraAsync();
        }

        private void CameraButtonsOnShutterKeyHalfPressed(object sender, System.EventArgs eventArgs)
        {
            Focus_OnClick(null, null);
        }

        private void CameraButtonsOnShutterKeyPressed(object sender, System.EventArgs eventArgs)
        {
            RecordButton_OnClick(null, null);
        }

        private void DisposeInternal()
        {
            _initialized = false;
            if (_mediaCaptureManager != null)
            {
                _mediaCaptureManager.RecordLimitationExceeded -= RecordLimitationExceeded;
                _mediaCaptureManager.Failed -= Failed;
                _mediaCaptureManager.Dispose();
            }
            if (_previewSink != null) _previewSink.Dispose();

            //Preview.Background = null;
        }

        private DeviceInformation _frontCamera;

        private DeviceInformation _rearCamera;

        private bool _isRearCamera = true;

        private async Task InitializeCameraAsync()
        {
            RecordButton.IsEnabled = true;
            SwitchModeButton.IsEnabled = true;
            FlashButton.IsEnabled = true;

            SwitchMode(CameraMode.Photo);

            Focus.IsHitTestVisible = false;
            Focus.Opacity = 0.0;
            _mediaCaptureManager = new MediaCapture();

            if (_rearCamera == null && _frontCamera == null)
            {
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                foreach (var device in devices)
                {
                    switch (device.EnclosureLocation.Panel)
                    {
                        case Windows.Devices.Enumeration.Panel.Front:
                            _frontCamera = device;

                            break;
                        case Windows.Devices.Enumeration.Panel.Back:
                            _rearCamera = device;

                            break;
                        default:
                            //you can also check for Top, Left, right and Bottom
                            break;
                    }
                }
            }

            if (_frontCamera == null || _rearCamera == null)
            {
                SwitchCameraButton.IsEnabled = false;
            }
            if (_isRearCamera && _rearCamera == null)
            {
                _isRearCamera = false;
            }
            if (!_isRearCamera && _frontCamera == null)
            {
                _isRearCamera = true;
            }

            var currentCamera = _isRearCamera ? _rearCamera : _frontCamera;

            _mediaCaptureManagerInitialized = false;

            var photoCaptureSource = PhotoCaptureSource.Auto;
            try
            {
                //if (_isRearCamera)
                //{
                //    throw new Exception("Initialization exception");
                //}

                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = currentCamera.Id,
                    PhotoCaptureSource = photoCaptureSource,
                    //StreamingCaptureMode = StreamingCaptureMode.Video,
                    //AudioDeviceId = ""
                    //VideoSource = screenCapture.VideoSource
                };

                await _mediaCaptureManager.InitializeAsync(settings);

                _mediaCaptureManagerInitialized = true;
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }

            if (!_mediaCaptureManagerInitialized)
            {
                Preview.Background = null;
                RecordButton.IsEnabled = false;
                SwitchModeButton.IsEnabled = false;
                FlashButton.IsEnabled = false;
                MessageBox.Show(AppResources.AccessCameraException, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            if (!string.IsNullOrEmpty(_mediaCaptureManager.MediaCaptureSettings.VideoDeviceId)
                && !string.IsNullOrEmpty(_mediaCaptureManager.MediaCaptureSettings.AudioDeviceId))
            {
                //hook into MediaCapture events
                _mediaCaptureManager.RecordLimitationExceeded += RecordLimitationExceeded;
                _mediaCaptureManager.Failed += Failed;

                //device initialized successfully
            }
            else
            {
                Preview.Background = null;
                RecordButton.IsEnabled = false;
                SwitchModeButton.IsEnabled = false;
                FlashButton.IsEnabled = false;
                MessageBox.Show(AppResources.AccessCameraException, AppResources.Error, MessageBoxButton.OK);
                _mediaCaptureManagerInitialized = false;
                return;
                //no cam found
            }

            //return;
            // List of supported video preview formats to be used by the default preview format selector.

            //var previewFormat = _currentProfile.Video;
            var mediaStreamType = MediaStreamType.VideoPreview;
            var previewFormat = GetStreamFormat(_currentProfile.Video, mediaStreamType, true);

            var scale = previewFormat.Width / Application.Current.Host.Content.ActualHeight;
            var width = Application.Current.Host.Content.ActualHeight;
            var height = previewFormat.Height / scale;

            Preview.Width = width * 0.75;
            Preview.Height = height * 0.75;


            await _mediaCaptureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(mediaStreamType, previewFormat);

            if (_mediaCaptureManager.MediaCaptureSettings.VideoDeviceCharacteristic != VideoDeviceCharacteristic.AllStreamsIdentical
                && _mediaCaptureManager.MediaCaptureSettings.VideoDeviceCharacteristic != VideoDeviceCharacteristic.PreviewRecordStreamsIdentical
                && _mediaCaptureManager.MediaCaptureSettings.VideoDeviceCharacteristic != VideoDeviceCharacteristic.RecordPhotoStreamsIdentical)
            {
                mediaStreamType = MediaStreamType.VideoRecord;
                var recordFormat = GetStreamFormat(previewFormat, mediaStreamType, true);
                await _mediaCaptureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(mediaStreamType, recordFormat);

                if (photoCaptureSource != PhotoCaptureSource.VideoPreview)
                {
                    mediaStreamType = MediaStreamType.Photo;
                    recordFormat = GetStreamFormat(previewFormat, mediaStreamType);
                    await _mediaCaptureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(mediaStreamType, recordFormat);
                }
            }

            //return;
            var previewSink = new MediaCapturePreviewSink();
            _previewSink = previewSink;
            await _mediaCaptureManager.StartPreviewToCustomSinkAsync(new MediaEncodingProfile { Video = previewFormat }, previewSink);


            // Set the source of the VideoBrush used for your preview
            var videoBrush = new VideoBrush();
            //videoBrush.Stretch = Stretch.UniformToFill;
            Preview.Background = videoBrush;

            videoBrush.SetSource(previewSink);


            var scaleX = Application.Current.Host.Content.ActualHeight / Preview.Width;
            var scaleY = Application.Current.Host.Content.ActualWidth / Preview.Height;
            var scaleMax = Math.Max(scaleX, scaleY);
            Transform.ScaleX = scaleMax;
            Transform.ScaleY = scaleMax;

            if (_isRearCamera)
            {
                Transform.ScaleX = Math.Abs(Transform.ScaleX);
            }
            else
            {
                Transform.ScaleX = -Math.Abs(Transform.ScaleX);
            }

            try
            {
                _flashControlMode = _isRearCamera ? FlashControlMode.Auto : FlashControlMode.Disabled; //GetFlashControlMode(_mediaCaptureManager.VideoDeviceController.FlashControl);
                SwitchFlashControlMode(_flashControlMode, _mediaCaptureManager.VideoDeviceController.FlashControl);
                FlashButton.IsEnabled = _mediaCaptureManager.VideoDeviceController.FlashControl.Supported;
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }

            try
            {
                if (_mediaCaptureManager.VideoDeviceController.FocusControl.Supported)
                {
                    var modes = _mediaCaptureManager.VideoDeviceController.FocusControl.SupportedFocusModes;
                    if (modes.Contains(FocusMode.Continuous))
                    {
                        _mediaCaptureManager.VideoDeviceController.FocusControl.Configure(new FocusSettings
                        {
                            Mode = FocusMode.Continuous,
                            DisableDriverFallback = true
                        });
                    }
                    else
                    {
                        _mediaCaptureManager.VideoDeviceController.FocusControl.Configure(new FocusSettings
                        {
                            Mode = FocusMode.Auto
                        });
                    }

                    await _mediaCaptureManager.VideoDeviceController.FocusControl.FocusAsync();
                }
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }

            try
            {
                _mediaCaptureManager.VideoDeviceController.LowLagPhoto.ThumbnailEnabled = false;
                _lowLagPhotoCapture = await _mediaCaptureManager.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateJpeg());
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }
        }

        private static void HandleCameraException(Exception ex)
        {
            TLUtils.WriteException("In-App Camera", ex);
        }

        private VideoEncodingProperties GetStreamFormat(VideoEncodingProperties encodingProperties, MediaStreamType mediaStreamType, bool debug = false)
        {
            var supportedVideoFormats = new List<string> { "nv12", "rgb32" };

            // Find the supported preview format
            var properties = _mediaCaptureManager.VideoDeviceController.GetAvailableMediaStreamProperties(mediaStreamType)
                .OfType<VideoEncodingProperties>()
                .Where(p => p != null && !String.IsNullOrEmpty(p.Subtype) && supportedVideoFormats.Contains(p.Subtype.ToLower()))
                .ToList();

            if (properties == null || properties.Count == 0) return null;

            var aspectRatio = (double)encodingProperties.Height / encodingProperties.Width;
            var currentFormat = properties.First();
            var widthDelta = encodingProperties.Width >= currentFormat.Width ? encodingProperties.Width - currentFormat.Width : currentFormat.Width - encodingProperties.Width;
            foreach (var p in properties)
            {
                var propertyRatio = (double)p.Height / p.Width;
                var propertyWidthDelta = encodingProperties.Width >= p.Width ? encodingProperties.Width - p.Width : p.Width - encodingProperties.Width;
                if (Math.Abs(aspectRatio - propertyRatio) <= 0.05
                    && propertyWidthDelta < widthDelta)
                {
                    widthDelta = propertyWidthDelta;
                    currentFormat = p;
                }
            }

            var builder = new StringBuilder();
            builder.AppendLine(string.Format("mediaStreamType={0}", mediaStreamType));
            builder.AppendLine(string.Format("CAPTUREPROFILE={0}x{1} {2} {3}", encodingProperties.Width, encodingProperties.Height, (double)encodingProperties.Height / encodingProperties.Width, encodingProperties.FrameRate.Numerator / encodingProperties.FrameRate.Denominator));
            foreach (var p in properties)
            {
                builder.AppendLine(string.Format("availableFormat={0}x{1} {2} {3}", p.Width, p.Height, (double)p.Height / p.Width, p.FrameRate.Numerator / p.FrameRate.Denominator));
            }
            builder.AppendLine(string.Format("SELECTEDFORMAT={0}x{1} {2} {3}", currentFormat.Width, currentFormat.Height, (double)currentFormat.Height / currentFormat.Width, currentFormat.FrameRate.Numerator / currentFormat.FrameRate.Denominator));

            if (debug)
            {
                SetDebugText(builder.ToString(), 45000);
            }

            DebugWriteLine("mediaStreamType={0}", mediaStreamType);
            DebugWriteLine("captureProfile={0}x{1} {2} {3}", encodingProperties.Width, encodingProperties.Height, (double)encodingProperties.Height / encodingProperties.Width, encodingProperties.FrameRate.Numerator / encodingProperties.FrameRate.Denominator);
            foreach (var p in properties)
            {
                DebugWriteLine("availableFormat={0}x{1} {2} {3}", p.Width, p.Height, (double)p.Height / p.Width, p.FrameRate.Numerator / p.FrameRate.Denominator);
            }
            DebugWriteLine("selectedFormat={0}x{1} {2} {3}", currentFormat.Width, currentFormat.Height, (double)currentFormat.Height / currentFormat.Width, currentFormat.FrameRate.Numerator / currentFormat.FrameRate.Denominator);

            return currentFormat;
        }

        private static void DebugWriteLine(string str, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(str, args);
        }

        private async void Focus_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_mediaCaptureManagerInitialized)
            {
                return;
            }

            if (!_mediaCaptureManager.VideoDeviceController.FocusControl.Supported)
            {
                return;
            }

            try
            {
                await _mediaCaptureManager.VideoDeviceController.FocusControl.FocusAsync();
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }
        }

        #region Flash Control
        private FlashControlMode _flashControlMode;

        private void Flash_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_mediaCaptureManagerInitialized)
            {
                return;
            }

            var flashControl = _mediaCaptureManager.VideoDeviceController.FlashControl;
            if (!flashControl.Supported) return;

            _flashControlMode = GetNextFlashControlMode(_flashControlMode);
            SwitchFlashControlMode(_flashControlMode, flashControl);
        }

        private FlashControlMode GetFlashControlMode(FlashControl flashControl)
        {
            if (!flashControl.Supported)
            {
                return FlashControlMode.Disabled;
            }

            if (!flashControl.Enabled)
            {
                return FlashControlMode.Disabled;
            }

            if (flashControl.Auto)
            {
                return FlashControlMode.Auto;
            }

            return FlashControlMode.Enabled;
        }

        private FlashControlMode GetNextFlashControlMode(FlashControlMode flashControlMode)
        {
            if (flashControlMode == FlashControlMode.Auto)
            {
                return FlashControlMode.Disabled;
            }

            if (flashControlMode == FlashControlMode.Disabled)
            {
                return FlashControlMode.Enabled;
            }

            return FlashControlMode.Auto;
        }

        private void SwitchFlashControlMode(FlashControlMode mode, FlashControl flashControl)
        {
            if (mode == FlashControlMode.Auto)
            {
                if (flashControl.Supported)
                {
                    flashControl.Enabled = true;
                    flashControl.Auto = true;
                    if (flashControl.AssistantLightSupported)
                    {
                        flashControl.AssistantLightEnabled = false;
                    }
                    FlashButtonImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_flash_auto_2x.png", UriKind.Relative));
                }
            }
            else if (mode == FlashControlMode.Disabled)
            {
                if (flashControl.Supported)
                {
                    flashControl.Enabled = false;
                    flashControl.Auto = false;
                    if (flashControl.AssistantLightSupported)
                    {
                        flashControl.AssistantLightEnabled = false;
                    }
                }
                FlashButtonImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_flash_disabled_2x.png", UriKind.Relative));
            }
            else
            {
                if (flashControl.Supported)
                {
                    flashControl.Enabled = true;
                    flashControl.Auto = false;
                    if (flashControl.AssistantLightSupported)
                    {
                        flashControl.AssistantLightEnabled = true;
                    }
                    FlashButtonImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_flash_enabled_2x.png", UriKind.Relative));
                }
            }
        }
        #endregion

        private void Failed(MediaCapture sender, MediaCaptureFailedEventArgs erroreventargs)
        {
            if (_isRecording)
            {
                StopRecording();
            }
        }

        private void RecordLimitationExceeded(MediaCapture sender)
        {
            if (_isRecording)
            {
                StopRecording();
            }
        }

        public void MainPage_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft)
            {
            //    Width = Application.Current.Host.Content.ActualHeight;
            //    Height = Application.Current.Host.Content.ActualWidth;

            //    FlashButton.VerticalAlignment = VerticalAlignment.Top;
            //    FlashButton.HorizontalAlignment = HorizontalAlignment.Center;

            //    BottomBorder.HorizontalAlignment = HorizontalAlignment.Right;
            //    BottomBorder.VerticalAlignment = VerticalAlignment.Stretch;
            //    BottomBorderTranslate.X = 72.0;
            //    BottomBorderTranslate.Y = 0.0;

                ContentPanel.Children.Remove(ProgressText);
                ButtonsPanel.Children.Remove(ProgressText);
                ContentPanel.Children.Add(ProgressText);
                ProgressText.Margin = new Thickness(0.0);
                ProgressTextTransform.Rotation = 90.0;
                ProgressTextTransform.TranslateX = -200.0;

                //    ButtonsPanel.ColumnDefinitions.Clear();
                //    ButtonsPanel.RowDefinitions.Clear();
                //    ButtonsPanel.RowDefinitions.Add(new RowDefinition());
                //    ButtonsPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                //    ButtonsPanel.RowDefinitions.Add(new RowDefinition());

                //    Grid.SetRow(SwitchModeButton, 2);
                //    Grid.SetRow(ProgressCircle, 1);
                //    Grid.SetRow(RecordButton, 1);
                //    Grid.SetRow(SwitchCameraButton, 0);

                //    Transform.Rotation = 0;

                //    ButtonsPanel.HorizontalAlignment = HorizontalAlignment.Right;
                //    ButtonsPanel.VerticalAlignment = VerticalAlignment.Center;
            }
            else if (e.Orientation == PageOrientation.LandscapeRight)
            {
            //    Width = Application.Current.Host.Content.ActualHeight;
            //    Height = Application.Current.Host.Content.ActualWidth;

            //    FlashButton.VerticalAlignment = VerticalAlignment.Bottom;
            //    FlashButton.HorizontalAlignment = HorizontalAlignment.Center;

            //    BottomBorder.HorizontalAlignment = HorizontalAlignment.Left;
            //    BottomBorder.VerticalAlignment = VerticalAlignment.Stretch;
            //    BottomBorderTranslate.X = -72.0;
            //    BottomBorderTranslate.Y = 0.0;

                ContentPanel.Children.Remove(ProgressText);
                ButtonsPanel.Children.Remove(ProgressText);
                ContentPanel.Children.Add(ProgressText);
                ProgressText.Margin = new Thickness(0.0);
                ProgressTextTransform.Rotation = -90.0;
                ProgressTextTransform.TranslateX = 200.0;

            //    ButtonsPanel.ColumnDefinitions.Clear();
            //    ButtonsPanel.RowDefinitions.Clear();
            //    ButtonsPanel.RowDefinitions.Add(new RowDefinition());
            //    ButtonsPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            //    ButtonsPanel.RowDefinitions.Add(new RowDefinition());

            //    Grid.SetRow(SwitchModeButton, 0);
            //    Grid.SetRow(ProgressCircle, 1);
            //    Grid.SetRow(RecordButton, 1);
            //    Grid.SetRow(SwitchCameraButton, 2);

            //    Transform.Rotation = 180;

            //    ButtonsPanel.HorizontalAlignment = HorizontalAlignment.Left;
            //    ButtonsPanel.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
            //    Width = Application.Current.Host.Content.ActualWidth;
            //    Height = Application.Current.Host.Content.ActualHeight;

            //    FlashButton.VerticalAlignment = VerticalAlignment.Center;
            //    FlashButton.HorizontalAlignment = HorizontalAlignment.Right;

            //    BottomBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            //    BottomBorder.VerticalAlignment = VerticalAlignment.Bottom;
            //    BottomBorderTranslate.X = 0.0;
            //    BottomBorderTranslate.Y = 72.0;

                ContentPanel.Children.Remove(ProgressText);
                ButtonsPanel.Children.Remove(ProgressText);
                ButtonsPanel.Children.Add(ProgressText);
                ProgressText.Margin = new Thickness(-8.0);
                ProgressTextTransform.Rotation = 0.0;
                ProgressTextTransform.TranslateX = 0.0;

            //    ButtonsPanel.ColumnDefinitions.Clear();
            //    ButtonsPanel.RowDefinitions.Clear();
            //    ButtonsPanel.RowDefinitions.Add(new RowDefinition());
            //    ButtonsPanel.RowDefinitions.Add(new RowDefinition());
            //    ButtonsPanel.ColumnDefinitions.Add(new ColumnDefinition());
            //    ButtonsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            //    ButtonsPanel.ColumnDefinitions.Add(new ColumnDefinition());

            //    Grid.SetRow(SwitchModeButton, 1);
            //    Grid.SetRow(ProgressCircle, 1);
            //    Grid.SetRow(RecordButton, 1);
            //    Grid.SetRow(SwitchCameraButton, 1);

            //    Transform.Rotation = 90;

            //    RotateTransform.Angle = 0;
            //    ButtonsPanel.VerticalAlignment = VerticalAlignment.Bottom;
            //    ButtonsPanel.HorizontalAlignment = HorizontalAlignment.Center;
            }

            StartRotateAnimation(e.Orientation);

            //SetFocusTranslation(_previousOrientation, e.Orientation);

            _previousOrientation = e.Orientation;
        }

        private void SetFocusTranslation(PageOrientation previousOrientation, PageOrientation orientation)
        {
            double translateX = 0.0;
            double translateY = 0.0;

            var contentWidth = (_previousOrientation & PageOrientation.Portrait) == PageOrientation.Portrait
                    ? Application.Current.Host.Content.ActualWidth
                    : Application.Current.Host.Content.ActualHeight;
            var contentHeight = (_previousOrientation & PageOrientation.Portrait) == PageOrientation.Portrait
                ? Application.Current.Host.Content.ActualHeight
                : Application.Current.Host.Content.ActualWidth;

            if (previousOrientation == PageOrientation.PortraitUp)
            {
                if (orientation == PageOrientation.LandscapeLeft)
                {
                    translateX = FocusTransform.TranslateY;
                    translateY = contentHeight - FocusTransform.TranslateX - Focus.ActualHeight;
                }
                else if (orientation == PageOrientation.LandscapeRight)
                {
                    translateX = contentWidth - FocusTransform.TranslateY - Focus.ActualWidth;
                    translateY = FocusTransform.TranslateX;
                }
            }
            else if (previousOrientation == PageOrientation.LandscapeLeft)
            {
                if (orientation == PageOrientation.PortraitUp)
                {
                    translateX = contentWidth - FocusTransform.TranslateY - Focus.ActualWidth;
                    translateY = FocusTransform.TranslateX;
                }
                else if (orientation == PageOrientation.LandscapeRight)
                {
                    translateX = contentWidth - FocusTransform.TranslateX - Focus.ActualHeight;
                    translateY = contentHeight - FocusTransform.TranslateY - Focus.ActualWidth;
                }
            }
            else if (previousOrientation == PageOrientation.LandscapeRight)
            {
                if (orientation == PageOrientation.PortraitUp)
                {
                    translateX = FocusTransform.TranslateY;
                    translateY = contentHeight - FocusTransform.TranslateX - Focus.ActualHeight;
                }
                else if (orientation == PageOrientation.LandscapeLeft)
                {
                    translateX = contentWidth - FocusTransform.TranslateX - Focus.ActualHeight;
                    translateY = contentHeight - FocusTransform.TranslateY - Focus.ActualWidth;
                }
            }

            FocusTransform.TranslateX = translateX;
            FocusTransform.TranslateY = translateY;
        }

        private Storyboard _rotateStoryboard;

        private void StartRotateAnimation(PageOrientation orientation)
        {
            if (_rotateStoryboard != null)
            {
                _rotateStoryboard.Stop();
            }

            double from = 0.0;
            if (orientation == PageOrientation.PortraitUp)
            {
                from = 0.0;
            }
            else if (orientation == PageOrientation.LandscapeLeft)
            {
                from = 90.0;
            }
            else if (orientation == PageOrientation.LandscapeRight)
            {
                from = -90.0;
            }

            _rotateStoryboard = new Storyboard();

            var progressCircleAnimation = new DoubleAnimation { To = from, Duration = TimeSpan.FromSeconds(0.3) };
            Storyboard.SetTarget(progressCircleAnimation, ProgressCircleTransform);
            Storyboard.SetTargetProperty(progressCircleAnimation, new PropertyPath("Angle"));
            _rotateStoryboard.Children.Add(progressCircleAnimation);

            var recordAnimation = new DoubleAnimation { To = from, Duration = TimeSpan.FromSeconds(0.3) };
            Storyboard.SetTarget(recordAnimation, RecordTransform);
            Storyboard.SetTargetProperty(recordAnimation, new PropertyPath("Angle"));
            _rotateStoryboard.Children.Add(recordAnimation);

            var switchCameraAnimation = new DoubleAnimation { To = from, Duration = TimeSpan.FromSeconds(0.3) };
            Storyboard.SetTarget(switchCameraAnimation, SwitchCameraTransform);
            Storyboard.SetTargetProperty(switchCameraAnimation, new PropertyPath("Angle"));
            _rotateStoryboard.Children.Add(switchCameraAnimation);

            var switchModeAnimation = new DoubleAnimation { To = from, Duration = TimeSpan.FromSeconds(0.3) };
            Storyboard.SetTarget(switchModeAnimation, SwitchModeTransform);
            Storyboard.SetTargetProperty(switchModeAnimation, new PropertyPath("Angle"));
            _rotateStoryboard.Children.Add(switchModeAnimation);

            var focusAnimation = new DoubleAnimation { To = from, Duration = TimeSpan.FromSeconds(0.3) };
            Storyboard.SetTarget(focusAnimation, FlashTransform);
            Storyboard.SetTargetProperty(focusAnimation, new PropertyPath("Rotation"));
            _rotateStoryboard.Children.Add(focusAnimation);

            Deployment.Current.Dispatcher.BeginInvoke(() => _rotateStoryboard.Begin());
        }

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        private bool _isSwitchingMode;

        private async void Switch_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isSwitchingMode)
            {
                return;
            }

            try
            {
                _isSwitchingMode = true;
                if (_previewSink != null) _previewSink.Dispose();
                if (_mediaCaptureManager != null) _mediaCaptureManager.Dispose();

                _isRearCamera = !_isRearCamera;

                await InitializeCameraAsync();
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }
            finally
            {
                _isSwitchingMode = false;
            }
        }

        private bool _isCapturingPhoto;

        private async void RecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            DebugWriteLine("RecordButton_OnClick " + _mode);

            if (!_mediaCaptureManagerInitialized)
            {
                return;
            }

            if (_isHolded)
            {
                return;
            }

            if (_mode == CameraMode.Recording)
            {
                StopRecording();
            }
            else if (_mode == CameraMode.Video)
            {
                await StartRecording();
            }
            else
            {
                if (_isCapturingPhoto) return;

                _isCapturingPhoto = true;
                try
                {
                    LayoutRoot.IsHitTestVisible = false;
                    var stopwatch = Stopwatch.StartNew();
                    //declare string for filename
                    string captureFileName = string.Empty;
                    //declare image format
                    //ImageEncodingProperties format = ImageEncodingProperties.CreateJpeg();
                    //format.Width = 3840;
                    //format.Height = 2160;

                    DebugWriteLine("Before CapturePhotoToStreamAsync " + stopwatch.Elapsed);
                    //generate stream from MediaCapture

                    var storyboard = new Storyboard();
                    var translateXAnimation = new DoubleAnimation { From = 0.0, To = FlashButton.ActualWidth + FlashButton.Margin.Right + FlashButton.Margin.Left, Duration = TimeSpan.FromSeconds(1.5), EasingFunction = new ExponentialEase{ Exponent = 5.0, EasingMode = EasingMode.EaseOut }};
                    Storyboard.SetTarget(translateXAnimation, FlashTransform);
                    Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("TranslateX"));
                    storyboard.Children.Add(translateXAnimation);

                    var translateYAnimation = new DoubleAnimation { From = 0.0, To = ButtonsPanel.ActualHeight, Duration = TimeSpan.FromSeconds(1.5), EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } };
                    Storyboard.SetTarget(translateYAnimation, ButtonsPanelTransform);
                    Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath("TranslateY"));
                    storyboard.Children.Add(translateYAnimation);

                    storyboard.Begin();
                    storyboard.Completed += (o, args) =>
                    {
                        RaisePhotoCaptured(new FileEventArgs { File = null });
                    };

                    var rotation = GetRotation();
                    //await _mediaCaptureManager.StopPreviewAsync();
                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(async () =>
                    {
                        var capturedPhoto = await _lowLagPhotoCapture.CaptureAsync();

                        var elapsed2 = stopwatch.Elapsed;
                        await _mediaCaptureManager.StopPreviewAsync();
                        var elapsed = stopwatch.Elapsed;
                        DebugWriteLine("CapturePhotoToStreamAsync " + elapsed);

                        var text = string.Format("StopPreview {3}\nCaptured {0}x{1} {2}", 0, 0, elapsed, elapsed2);
                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() => SetDebugText(text));
                        //SetDebugText(text);
                        //return;
                        using (var imageStream = new InMemoryRandomAccessStream())
                        {

                            await RandomAccessStream.CopyAsync(capturedPhoto.Frame, imageStream);

                            //create decoder and encoder
                            var dec = await BitmapDecoder.CreateAsync(imageStream);
                            var enc = await BitmapEncoder.CreateForTranscodingAsync(imageStream, dec);

                            // fix for front camera mirroring
                            if (!_isRearCamera)
                            {
                                if (rotation == BitmapRotation.None)
                                {
                                    rotation = BitmapRotation.Clockwise180Degrees;
                                }
                                else if (rotation == BitmapRotation.Clockwise180Degrees)
                                {
                                    rotation = BitmapRotation.None;
                                }
                                enc.BitmapTransform.Flip = BitmapFlip.Vertical;
                            }

                            //roate the image
                            enc.BitmapTransform.Rotation = rotation;

                            //write changes to the image stream
                            await enc.FlushAsync();

                            var elapsed3 = stopwatch.Elapsed;
                            DebugWriteLine("FlushAsync " + stopwatch.Elapsed);

                            //save the image

                            var folder = KnownFolders.CameraRoll;
                            var capturefile = await folder.CreateFileAsync("photo_" + DateTime.Now.Ticks + ".jpg", CreationCollisionOption.ReplaceExisting);

                            //store stream in file
                            using (var fileStream = await capturefile.OpenStreamForWriteAsync())
                            {
                                try
                                {
                                    //because of using statement stream will be closed automatically after copying finished
                                    await RandomAccessStream.CopyAsync(imageStream, fileStream.AsOutputStream());
                                    DebugWriteLine("CopyAsync " + stopwatch.Elapsed);
                                }
                                catch (Exception ex)
                                {
                                    HandleCameraException(ex);
                                }
                            }

                            text = string.Format("StopPreview {3}\nCaptured {0}x{1} {2}\nRotate {4}\nSave {5}", capturedPhoto.Frame.Width, capturedPhoto.Frame.Height, elapsed, elapsed2, elapsed3, stopwatch.Elapsed);

                            Execute.BeginOnUIThread(() =>
                            {
                                SetDebugText(text);
                                RaisePhotoCaptured(new FileEventArgs { File = capturefile });
                            });

                            Dispose();
                        }
                    });
                }
                catch (Exception ex)
                {
                    HandleCameraException(ex);
                }
                finally
                {
                    _isCapturingPhoto = false;
                    LayoutRoot.IsHitTestVisible = true;
                }
            }
        }

        private void SetDebugText(string text, int millisecondsTimeout = 5000)
        {
#if DEBUG
            TimerLabel.Visibility = Visibility.Visible;
            TimerLabel.Text = text;
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.Sleep(5000);
                Deployment.Current.Dispatcher.BeginInvoke(() => { TimerLabel.Visibility = Visibility.Collapsed; });
            });
#endif
        }

        private BitmapRotation GetRotation()
        {
            if (_previousOrientation == PageOrientation.LandscapeLeft)
            {
                return BitmapRotation.None;
            }
            else if (_previousOrientation == PageOrientation.LandscapeRight)
            {
                return BitmapRotation.Clockwise180Degrees;
            }
            else
            {
                if (_isRearCamera)
                {
                    return BitmapRotation.Clockwise90Degrees;
                }
                else
                {
                    return BitmapRotation.Clockwise270Degrees;
                }
            }
        }

        private int GetVideoRotation()
        {
            if (_previousOrientation == PageOrientation.LandscapeLeft)
            {
                return 0;
            }
            else if (_previousOrientation == PageOrientation.LandscapeRight)
            {
                return 180;
            }
            else
            {
                if (_isRearCamera)
                {
                    return 90;
                }
                else
                {
                    return 270;
                }
            }
        }

        private bool _isFocusing;

        private async void Preview_OnTap(object sender, GestureEventArgs e)
        {
            if (!_mediaCaptureManager.VideoDeviceController.FocusControl.Supported)
            {
                return;
            }

            if (_isFocusing)
            {
                return;
            }

            try
            {
                _isFocusing = true;
                if (_mediaCaptureManager.VideoDeviceController.RegionsOfInterestControl.AutoFocusSupported
                    && _mediaCaptureManager.VideoDeviceController.RegionsOfInterestControl.MaxRegions > 0)
                {
                    var properties = _mediaCaptureManager.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                    var previewWidth = properties.Width;
                    var previewHeight = properties.Height;

                    var scaleFactor = Math.Max(previewWidth, previewHeight) / Math.Max(Preview.ActualWidth, Preview.ActualHeight);

                    var previewPosition = e.GetPosition(Preview);
                    var absolutePosition = e.GetPosition(ContentPanel);
                    System.Diagnostics.Debug.WriteLine("absolutePosition x={0} y={1}", absolutePosition.X, absolutePosition.Y);
                    var focusTransformX = absolutePosition.X - Focus.ActualWidth / 2.0;
                    var focusTransformY = absolutePosition.Y - Focus.ActualHeight / 2.0;

                    var contentWidth = (_previousOrientation & PageOrientation.Portrait) == PageOrientation.Portrait
                        ? Application.Current.Host.Content.ActualWidth
                        : Application.Current.Host.Content.ActualHeight;
                    var contentHeight = (_previousOrientation & PageOrientation.Portrait) == PageOrientation.Portrait
                        ? Application.Current.Host.Content.ActualHeight
                        : Application.Current.Host.Content.ActualWidth;

                    if (focusTransformX < 0)
                    {
                        focusTransformX = 0;
                    }
                    else if (focusTransformX > (contentWidth - Focus.ActualWidth))
                    {
                        focusTransformX = (contentWidth - Focus.ActualWidth);
                    }
                    if (focusTransformY < 0)
                    {
                        focusTransformY = 0;
                    }
                    else if (focusTransformY > (contentHeight - Focus.ActualHeight))
                    {
                        focusTransformY = (contentHeight - Focus.ActualHeight);
                    }
                    FocusTransform.TranslateX = focusTransformX;
                    FocusTransform.TranslateY = focusTransformY;
                    System.Diagnostics.Debug.WriteLine("focusTransform x={0} y={1}", focusTransformX, focusTransformY);

                    Focus.IsHitTestVisible = true;
                    Focus.Opacity = 1.0;

                    //System.Diagnostics.Debug.WriteLine("preview w={0} h={1}", previewWidth, previewHeight);
                    //System.Diagnostics.Debug.WriteLine("pos={0} w={1} h={2}", pos, Preview.ActualWidth, Preview.ActualHeight);

                    var point1 = new Point((previewPosition.X - 25) * scaleFactor, (previewPosition.Y - 25) * scaleFactor);
                    if (point1.X < 0)
                        point1.X = 0;
                    if (point1.Y < 0)
                        point1.Y = 0;

                    var point2 = new Point((previewPosition.X + 25) * scaleFactor, (previewPosition.Y + 25) * scaleFactor);
                    if (point2.X > previewWidth)
                        point2.X = previewWidth;
                    if (point2.Y > previewHeight)
                        point2.Y = previewHeight;

                    var region = new RegionOfInterest();
                    region.Bounds = new Rect(point1, point2);
                    System.Diagnostics.Debug.WriteLine("bounds x={0} y={1} of with={2} height={3}", region.Bounds.X, region.Bounds.Y, previewWidth, previewHeight);
                    region.BoundsNormalized = false;
                    region.AutoFocusEnabled = true;
                    region.AutoExposureEnabled = false; //this will make exposure for roi
                    region.AutoWhiteBalanceEnabled = false; //this will make wb for roi

                    _mediaCaptureManager.VideoDeviceController.FocusControl.Configure(new FocusSettings
                    {
                        Mode = FocusMode.Single
                    });

                    await _mediaCaptureManager.VideoDeviceController.RegionsOfInterestControl.ClearRegionsAsync();
                    await _mediaCaptureManager.VideoDeviceController.RegionsOfInterestControl.SetRegionsAsync(new List<RegionOfInterest> { region }, true);

                    //note: before focusing, make sure or set single focus mode. That part of code not here.
                    await _mediaCaptureManager.VideoDeviceController.FocusControl.FocusAsync();
                }
                else
                {
                    var modes = _mediaCaptureManager.VideoDeviceController.FocusControl.SupportedFocusModes;
                    if (modes.Contains(FocusMode.Continuous))
                    {
                        _mediaCaptureManager.VideoDeviceController.FocusControl.Configure(new FocusSettings
                        {
                            Mode = FocusMode.Continuous,
                            DisableDriverFallback = true
                        });
                    }
                    else
                    {
                        _mediaCaptureManager.VideoDeviceController.FocusControl.Configure(new FocusSettings
                        {
                            Mode = FocusMode.Auto
                        });
                    }

                    await _mediaCaptureManager.VideoDeviceController.FocusControl.FocusAsync();
                }
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }
            finally
            {
                _isFocusing = false;
            }
        }

        private void Preview_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation == null) return;

            var zoomControl = _mediaCaptureManager.VideoDeviceController.ZoomControl;
            if (!zoomControl.Supported) return;

            var zoomFactor = zoomControl.Value * e.PinchManipulation.DeltaScale;

            if (zoomFactor < zoomControl.Min) zoomFactor = zoomControl.Min;
            if (zoomFactor > zoomControl.Max) zoomFactor = zoomControl.Max;
            zoomFactor = zoomFactor - (zoomFactor % zoomControl.Step);

            _mediaCaptureManager.VideoDeviceController.ZoomControl.Value = (float)zoomFactor;
        }

        private bool _isHolded = false;

        private bool _isRecording = false;
        private bool _initialized;
        private DateTime _startRecordTime;

        private DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.3) };


        private void TimerOnTick(object sender, System.EventArgs eventArgs)
        {
            if (!_isRecording) return;

            var time = (DateTime.Now - _startRecordTime);

            ProgressText.Text = time.ToString(@"mm\:ss");
        }

        private async void RecordButton_OnHold(object sender, GestureEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("RecordButton_OnHold " + _mode);
            if (!_mediaCaptureManagerInitialized)
            {
                return;
            }

            _isHolded = true;

            await StartRecording();
        }

        private Storyboard _progressStoryboard;

        private async Task StartRecording()
        {
            SwitchIdleDetectionMode(false);

            if (_isHolded)
            {
                Overlay.Background = new SolidColorBrush(Colors.White);
            }

            _isRecordCanceled = false;
            _isRecording = true;
            var videoFolder = KnownFolders.CameraRoll;
            _captureVideofile = await videoFolder.CreateFileAsync("video_" + DateTime.Now.Ticks + ".mp4", CreationCollisionOption.ReplaceExisting);

            var profile = _currentProfile;
            profile.Video.Properties.Remove(RotationKey);
            profile.Video.Properties.Add(RotationKey, PropertyValue.CreateInt32(GetVideoRotation()));

            System.Diagnostics.Debug.WriteLine("profile={0}x{1}", profile.Video.Width, profile.Video.Height);

            //await _mediaCaptureManager.StartRecordToStorageFileAsync(profile, _captureVideofile);
            await _lowLagPhotoCapture.FinishAsync();

            _lowLagMediaRecording = await _mediaCaptureManager.PrepareLowLagRecordToStorageFileAsync(profile, _captureVideofile);
            await _lowLagMediaRecording.StartAsync();


            SetDebugText("Recording...", 30000);
            _timer.Start();
            _startRecordTime = DateTime.Now;

            _progressStoryboard = new Storyboard();
            var progressAnimation = new DoubleAnimation { From = 0.0, To = 359.9, Duration = TimeSpan.FromSeconds(30.0) };
            Storyboard.SetTarget(progressAnimation, ProgressRing);
            Storyboard.SetTargetProperty(progressAnimation, new PropertyPath("Angle"));
            _progressStoryboard.Children.Add(progressAnimation);

            _progressStoryboard.Begin();
            _progressStoryboard.Completed += OnProgressStoryboardCompleted;

            SwitchMode(CameraMode.Recording);
        }

        private void OnProgressStoryboardCompleted(object sender, System.EventArgs e)
        {
            if (_isRecording)
            {
                StopRecording();
            }
        }

        private void RecordButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            DebugWriteLine("RecordButton_OnMouseLeave " + _mode);
            if (_isRecording && _isHolded)
            {
                StopRecording();
            }

            _isHolded = false;
        }

        private async Task StopRecording()
        {
            SwitchIdleDetectionMode(true);

            Overlay.Background = new SolidColorBrush(Colors.Transparent);

            _isRecording = false;

            TimerLabel.Visibility = Visibility.Collapsed;
            _timer.Stop();
            ProgressText.Text = "00:00";

            SwitchMode(CameraMode.Video);

            _progressStoryboard.Stop();

            //await _mediaCaptureManager.StopRecordAsync();
            await _lowLagMediaRecording.StopAsync();
            await _lowLagMediaRecording.FinishAsync();
            _lowLagPhotoCapture = await _mediaCaptureManager.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateJpeg());

            if (_isRecordCanceled)
            {
                _isRecordCanceled = false;
                await _captureVideofile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            else
            {
                RaiseVideoCaptured(new FileEventArgs{ File = _captureVideofile });
            }
        }

        private CameraMode _mode = CameraMode.Photo;

        private void SwitchMode(CameraMode mode)
        {
            _mode = mode;

            switch (mode)
            {
                case CameraMode.Photo:
                    ProgressText.Foreground = new SolidColorBrush(Colors.White);
                    ProgressText.Visibility = Visibility.Collapsed;
                    SwitchCameraButton.Visibility = Visibility.Visible;
                    SwitchModeButton.Visibility = Visibility.Visible;
                    FlashButton.Visibility = Visibility.Visible;
                    SwitchModeImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_camera_video_2x.png", UriKind.Relative));
                    RecordImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_camera_photo_2x.png", UriKind.Relative));
                    ProgressCircle.Opacity = 0.0;
                    return;
                case CameraMode.Video:
                    ProgressText.Foreground = new SolidColorBrush(Colors.White);
                    ProgressText.Visibility = Visibility.Visible;
                    SwitchCameraButton.Visibility = Visibility.Visible;
                    SwitchModeButton.Visibility = Visibility.Visible;
                    FlashButton.Visibility = Visibility.Collapsed;
                    SwitchModeImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_camera_photo_2x.png", UriKind.Relative));
                    RecordImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_camera_video_2x.png", UriKind.Relative));
                    ProgressCircle.Opacity = 0.0;
                    return;
                case CameraMode.Recording:
                    ProgressText.Foreground = new SolidColorBrush(Colors.White);
                    ProgressText.Visibility = Visibility.Visible;
                    SwitchCameraButton.Visibility = Visibility.Collapsed;
                    SwitchModeButton.Visibility = Visibility.Collapsed;
                    FlashButton.Visibility = Visibility.Collapsed;
                    RecordImage.Source = new BitmapImage(new Uri("/Images/W10M/ic_camera_stop_2x.png", UriKind.Relative));
                    ProgressCircle.Opacity = 1.0;
                    return;
            }
        }

        private void SwitchMode_OnClick(object sender, RoutedEventArgs e)
        {
            if (_mode == CameraMode.Photo)
            {
                SwitchMode(CameraMode.Video);
            }
            else if (_mode == CameraMode.Video)
            {
                SwitchMode(CameraMode.Photo);
            }
        }

        private bool _isRecordCanceled;
        private StorageFile _captureVideofile;

        private void RecordButton_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!_isHolded)
            {
                return;
            }

            if (e.CumulativeManipulation == null)
            {
                return;
            }

            var distance =
                Math.Sqrt(e.CumulativeManipulation.Translation.X * e.CumulativeManipulation.Translation.X +
                          e.CumulativeManipulation.Translation.Y * e.CumulativeManipulation.Translation.Y);

            System.Diagnostics.Debug.WriteLine(e.CumulativeManipulation.Translation);

            if (distance >= 70.0)
            {
                _isRecordCanceled = true;
                ProgressText.Foreground = (Brush)Resources["CancelForeground"];
            }
            else
            {
                _isRecordCanceled = false;
                ProgressText.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private async void Focus_OnTap(object sender, GestureEventArgs e)
        {
            if (!_mediaCaptureManager.VideoDeviceController.FocusControl.Supported)
            {
                return;
            }

            if (_isFocusing)
            {
                return;
            }

            Focus.Opacity = 0.0;
            Focus.IsHitTestVisible = false;

            try
            {
                _isFocusing = true;

                await _mediaCaptureManager.VideoDeviceController.RegionsOfInterestControl.ClearRegionsAsync();

                var modes = _mediaCaptureManager.VideoDeviceController.FocusControl.SupportedFocusModes;
                if (modes.Contains(FocusMode.Continuous))
                {
                    _mediaCaptureManager.VideoDeviceController.FocusControl.Configure(new FocusSettings
                    {
                        Mode = FocusMode.Continuous,
                        DisableDriverFallback = true
                    });
                }
                else
                {
                    _mediaCaptureManager.VideoDeviceController.FocusControl.Configure(new FocusSettings
                    {
                        Mode = FocusMode.Auto
                    });
                }
                await _mediaCaptureManager.VideoDeviceController.FocusControl.FocusAsync();
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }
            finally
            {
                _isFocusing = false;
            }
        }

        public static void SwitchIdleDetectionMode(bool enabled)
        {
#if WINDOWS_PHONE
            var mode = enabled ? IdleDetectionMode.Enabled : IdleDetectionMode.Disabled;
            try
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = mode;
            }
            catch (Exception ex)
            {
                HandleCameraException(ex);
            }
#endif
        }

        private void SwitchCameraButton_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            DebugWriteLine("SwitchCameraButton_OnManipulationCompleted");
        }

        private void SwitchCameraButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            DebugWriteLine("SwitchCameraButton_OnMouseLeave");
        }

        private void CameraPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            Preview.Background = null;
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        public void DisposeAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                DisposeInternal();
            });
        }
    }

    public enum CameraMode
    {
        Photo,
        Video,
        Recording,
    }

    public enum FlashControlMode
    {
        Auto,
        Disabled,
        Enabled,
    }

    public class FileEventArgs : System.EventArgs
    {
        public StorageFile File { get; set; }
    }
}