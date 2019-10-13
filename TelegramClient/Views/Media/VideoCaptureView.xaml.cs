// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Telegram.Api.Helpers;
#if WP8
using Windows.Storage;
#endif
using Caliburn.Micro;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Services;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Media;
using Execute = Caliburn.Micro.Execute;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Media
{
    public partial class VideoCaptureView
    {
        public VideoCaptureViewModel ViewModel { get { return DataContext as VideoCaptureViewModel; } }

        private readonly AppBarButton _startRecordingButton = new AppBarButton
        {
            Text = AppResources.Record,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.video.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _stopRecordingPlaybackButton = new AppBarButton
        {
            Text = AppResources.Stop,
            IconUri = new Uri("/Images/ApplicationBar/appbar.stop.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _startPlaybackButton = new AppBarButton
        {
            Text = AppResources.Play,
            IconUri = new Uri("/Images/Audio/appbar.transport.play.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _pausePlaybackButton = new AppBarButton
        {
            Text = AppResources.Pause,
            IconUri = new Uri("/Images/Audio/appbar.transport.pause.rest.png ", UriKind.Relative)
        };

        private readonly AppBarButton _attachButton = new AppBarButton
        {
            Text = AppResources.Attach,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar == null)
            {
                ApplicationBar = new ApplicationBar();
            }
            else
            {
                return;
            }
            ApplicationBar.Opacity = 0.5;
            ApplicationBar.IsVisible = false;

            //ApplicationBar.Buttons.Add(_startRecordingButton);
            //ApplicationBar.Buttons.Add(_stopRecordingPlaybackButton);
            ApplicationBar.Buttons.Add(_attachButton);
            ApplicationBar.Buttons.Add(_cancelButton);
        }


        private DateTime _startTime;

        private int _timerCounter;

        public static readonly DependencyProperty TimerStringProperty =
            DependencyProperty.Register("TimerString", typeof(string), typeof(VideoCaptureView), new PropertyMetadata("00:00"));

        public string TimerString
        {
            get { return (string)GetValue(TimerStringProperty); }
            set { SetValue(TimerStringProperty, value); }
        }

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.25) };

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
            switch (e.Orientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeLeft:
                    //VideoPlayerTransform.Rotation = 0;
                    ViewfinderTransform.Rotation = 0;
                    TimerTransform.Rotation = 0;
                    break;
                case PageOrientation.LandscapeRight:
                    //VideoPlayerTransform.Rotation =180;
                    ViewfinderTransform.Rotation = 180;
                    TimerTransform.Rotation = 180;
                    break;
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    //VideoPlayerTransform.Rotation = 90;
                    ViewfinderTransform.Rotation = 90;
                    break;
                case PageOrientation.PortraitDown:
                    //VideoPlayerTransform.Rotation = 270;
                    ViewfinderTransform.Rotation = 270;
                    break;
            }
        }

        // Source and device for capturing video.
        private CaptureSource _captureSource;
        private VideoCaptureDevice _videoCaptureDevice;

        // Media details for storing the recording.        
        private IsolatedStorageFileStream _fileStream;
        private FileSink _fileSink;
        private readonly string _videoFileName = string.Empty;

        // For managing button and application state.
        private enum ButtonState { Initialized, Ready, Recording, Playback, Paused, NoChange, CameraNotSupported };
        private ButtonState _currentAppState;

        public VideoCaptureView()
        {
            InitializeComponent();

            if (Execute.InDesignMode) return;

            _timer.Tick += OnTimerTick;

            var guid = IoC.Get<IStateService>().CurrentUserId + "_" + Guid.NewGuid() + ".mp4";
            _videoFileName = guid;

            _attachButton.Click += OnAttachButtonClick;
            _cancelButton.Click += OnCancelButtonClick;
            // Prepare ApplicationBar and buttons.
            BuildLocalizedAppBar();

            Loaded += (sender, args) =>
            {
                FileUtils.SwitchIdleDetectionMode(false);
            };
            Unloaded += (sender, args) =>
            {
                FileUtils.SwitchIdleDetectionMode(true);
            };
        }

        private void OnCancelButtonClick(object sender, System.EventArgs e)
        {
            ViewModel.VideoIsoFile = null;
            TimerString = "00:00";
            UpdateUI(ButtonState.Initialized, AppResources.TapRecordToStartRecording);
        }

        private void OnAttachButtonClick(object sender, System.EventArgs e)
        {
            ViewModel.Attach(_videoFileName, _fileId, _uploadableParts);
        }

        private void OnTimerTick(object sender, System.EventArgs e)
        {
            _timerCounter = (int)(DateTime.Now - _startTime).TotalSeconds;
            TimerString = TimeSpan.FromSeconds(_timerCounter).ToString(@"mm\:ss");

#if WP8
            //UploadFileAsync();
#endif
        }

        private long _uploadingLength;
        private volatile bool _isPartReady;
        private TLLong _fileId;
        private readonly List<UploadablePart> _uploadableParts = new List<UploadablePart>();

#if WP8
        private void UploadFileAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                if (!_isPartReady) return;

                _isPartReady = false;

                var uploadablePart = GetUploadablePart(_videoFileName, _uploadingLength, _uploadableParts.Count);
                if (uploadablePart == null)
                {
                    _isPartReady = true;
                    return;
                }

                _uploadableParts.Add(uploadablePart);
                _uploadingLength += uploadablePart.Count;

                //Execute.BeginOnUIThread(() => VibrateController.Default.Start(TimeSpan.FromSeconds(0.02)));

                var mtProtoService = IoC.Get<IMTProtoService>();
                mtProtoService.SaveFilePartAsync(_fileId, uploadablePart.FilePart,
                    TLString.FromBigEndianData(uploadablePart.Bytes),
                    result =>
                    {
                        if (result.Value)
                        {
                            uploadablePart.Status = PartStatus.Processed;
                        }
                    },
                    error => Telegram.Api.Helpers.Execute.ShowDebugMessage("upload.saveFilePart error " + error));

                _isPartReady = true;
            });
        }

        private static UploadablePart GetUploadablePart(string fileName, long position, int partId)
        {
            var fullFilePath = ApplicationData.Current.LocalFolder.Path + "\\" + fileName;
            var fi = new FileInfo(fullFilePath);
            if (!fi.Exists)
            {
                return null;
            }

            const int minPartLength = 1024;
            const int maxPartLength = 4 * 1024;

            var recordingLength = fi.Length - position;
            if (recordingLength < minPartLength)
            {
                return null;
            }

            var subpartsCount = (int)recordingLength / minPartLength;
            var uploadingBufferSize = Math.Min(maxPartLength, subpartsCount * minPartLength);
            var uploadingBuffer = new byte[uploadingBufferSize];

            try
            {
                using (var fileStream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Position = position;
                    fileStream.Read(uploadingBuffer, 0, uploadingBufferSize);
                }
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("read file " + fullFilePath + " exception " + ex);
                return null;
            }

            return new UploadablePart(null, new TLInt(partId), uploadingBuffer, position, uploadingBufferSize);
        }
#endif

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Execute.InDesignMode) return;
            // Initialize the video recorder.

            CameraButtons.ShutterKeyPressed += OnShutterKeyPressed;

            ThreadPool.QueueUserWorkItem(state => Deployment.Current.Dispatcher.BeginInvoke(InitializeVideoRecorder));
        }

        public void InitializeVideoRecorder()
        {
            if (_captureSource == null)
            {
                // Create the VideoRecorder objects.
                _captureSource = new CaptureSource();
                _captureSource.VideoCaptureDevice.DesiredFormat = GetDesiredFormat();
                _fileSink = new FileSink();
                _videoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();

                // Add eventhandlers for _captureSource.
                _captureSource.CaptureFailed += OnCaptureFailed;
                _captureSource.CaptureImageCompleted += OnThumbCompleted;
                // Initialize the camera if it exists on the device.

                if (_videoCaptureDevice != null)
                {
                    // Create the VideoBrush for the viewfinder.
                    //ViewfinderBrush = new VideoBrush();
                    //ViewfinderBrush.Transform = new CompositeTransform{CenterX = 0.5, CenterY = 0.5};
                    ViewfinderBrush.SetSource(_captureSource);

                    // Display the viewfinder image on the rectangle.
                    Viewfinder.Fill = ViewfinderBrush;

                    // Start video capture and display it on the viewfinder.
                    _captureSource.Start();

                    // Set the button state and the message.
                    UpdateUI(ButtonState.Initialized, AppResources.TapRecordToStartRecording);
                }
                else
                {
                    // Disable buttons when the camera is not supported by the device.
                    UpdateUI(ButtonState.CameraNotSupported, AppResources.ACameraIsNotSupportedOnThisDevice);
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CameraButtons.ShutterKeyPressed -= OnShutterKeyPressed;

            base.OnNavigatingFrom(e);
        }

        private void OnShutterKeyPressed(object sender, System.EventArgs e)
        {
            UIElement_OnTap(sender, null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Dispose of camera and media objects.
            ThreadPool.QueueUserWorkItem(state => Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                DisposeVideoPlayer();
                DisposeVideoRecorder();
            }));

            base.OnNavigatedFrom(e);
        }

        private void DisposeVideoRecorder()
        {
            if (_captureSource != null)
            {
                // Stop _captureSource if it is running.
                if (_captureSource.VideoCaptureDevice != null
                    && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();
                }

                // Remove the event handlers for capturesource and the shutter button.
                _captureSource.CaptureFailed -= OnCaptureFailed;
                _captureSource.CaptureImageCompleted -= OnThumbCompleted;
                // Remove the video recording objects.
                _captureSource = null;
                _videoCaptureDevice = null;
                _fileSink = null;
                ViewfinderBrush = null;
            }
        }

        private void DisposeVideoPlayer()
        {
            //if (VideoPlayer != null)
            //{
            //    // Stop the VideoPlayer MediaElement.
            //    VideoPlayer.Stop();

            //    // Remove playback objects.
            //    VideoPlayer.Source = null;
            //    _fileStream = null;

            //    // Remove the event handler.
            //    VideoPlayer.MediaEnded -= VideoPlayerMediaEnded;
            //}
        }

        // If recording fails, display an error message.
        private void OnCaptureFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate()
            {
                DebugText.Text = AppResources.Error.ToUpperInvariant() + ": " + e.ErrorException.Message;
            });
        }

        // Display the viewfinder when playback ends.
        public void VideoPlayerMediaEnded(object sender, RoutedEventArgs e)
        {
            // Remove the playback objects.
            DisposeVideoPlayer();

            StartVideoPreview();
        }


        // Set the recording state: display the video on the viewfinder.
        private void StartVideoPreview()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.Sleep(100);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        // Display the video on the viewfinder.
                        if (_captureSource.VideoCaptureDevice != null
                        && _captureSource.State == CaptureState.Stopped)
                        {
                            // Add _captureSource to videoBrush.
                            //ViewfinderBrush = new VideoBrush();
                            //ViewfinderBrush.Transform = new CompositeTransform { CenterX = 0.5, CenterY = 0.5 };
                            ViewfinderBrush.SetSource(_captureSource);

                            // Add videoBrush to the visual tree.
                            //Viewfinder.Fill = ViewfinderBrush;

                            _captureSource.Start();

                            // Set the button states and the message.
                            UpdateUI(ButtonState.Ready, AppResources.ReadyToRecord);
                        }
                    }
                    // If preview fails, display an error.
                    catch (Exception e)
                    {
                        DebugText.Text = AppResources.Error.ToUpperInvariant() + ": " + e.Message;
                    }
                });
            });
        }

        // Update the buttons and text on the UI thread based on app state.
        private void UpdateUI(ButtonState currentButtonState, string statusMessage)
        {
            // Run code on the UI thread.
            Dispatcher.BeginInvoke(delegate
            {

                switch (currentButtonState)
                {
                    // When the camera is not supported by the device.
                    case ButtonState.CameraNotSupported:
                        ApplicationBar.IsVisible = false;
                        _startRecordingButton.IsEnabled = false;
                        _attachButton.IsEnabled = false;
                        _cancelButton.IsEnabled = false;
                        _stopRecordingPlaybackButton.IsEnabled = false;
                        _startPlaybackButton.IsEnabled = false;
                        _pausePlaybackButton.IsEnabled = false;
                        break;

                    // First launch of the application, so no video is available.
                    case ButtonState.Initialized:
                        ApplicationBar.IsVisible = false;
                        _startRecordingButton.IsEnabled = true;
                        _attachButton.IsEnabled = false;
                        _cancelButton.IsEnabled = false;
                        _stopRecordingPlaybackButton.IsEnabled = false;
                        _startPlaybackButton.IsEnabled = false;
                        _pausePlaybackButton.IsEnabled = false;
                        break;

                    // Ready to record, so video is available for viewing.
                    case ButtonState.Ready:
                        ApplicationBar.IsVisible = true;
                        _startRecordingButton.IsEnabled = true;
                        _attachButton.IsEnabled = true;
                        _cancelButton.IsEnabled = true;
                        _stopRecordingPlaybackButton.IsEnabled = false;
                        _startPlaybackButton.IsEnabled = true;
                        _pausePlaybackButton.IsEnabled = false;
                        break;

                    // Video recording is in progress.
                    case ButtonState.Recording:
                        ApplicationBar.IsVisible = false;
                        _startRecordingButton.IsEnabled = false;
                        _attachButton.IsEnabled = false;
                        _cancelButton.IsEnabled = false;
                        _stopRecordingPlaybackButton.IsEnabled = true;
                        _startPlaybackButton.IsEnabled = false;
                        _pausePlaybackButton.IsEnabled = false;
                        break;

                    // Video playback is in progress.
                    case ButtonState.Playback:
                        ApplicationBar.IsVisible = true;
                        _startRecordingButton.IsEnabled = false;
                        _attachButton.IsEnabled = true;
                        _cancelButton.IsEnabled = true;
                        _stopRecordingPlaybackButton.IsEnabled = true;
                        _startPlaybackButton.IsEnabled = false;
                        _pausePlaybackButton.IsEnabled = true;
                        break;

                    // Video playback has been paused.
                    case ButtonState.Paused:
                        ApplicationBar.IsVisible = true;
                        _startRecordingButton.IsEnabled = false;
                        _attachButton.IsEnabled = true;
                        _cancelButton.IsEnabled = true;
                        _stopRecordingPlaybackButton.IsEnabled = true;
                        _startPlaybackButton.IsEnabled = true;
                        _pausePlaybackButton.IsEnabled = false;
                        break;

                    default:
                        break;
                }

                // Display a message.
                DebugText.Text = statusMessage;

                // Note the current application state.
                _currentAppState = currentButtonState;
            });
        }


        private void StopPlaybackRecording_Click(object sender, System.EventArgs e)
        {
            Timer.Opacity = 1.0;
            // Avoid duplicate taps.
            _stopRecordingPlaybackButton.IsEnabled = false;

            // Stop during video recording.
            if (_currentAppState == ButtonState.Recording)
            {
                StopVideoRecording();

                // Set the button state and the message.
                UpdateUI(ButtonState.NoChange, AppResources.PressBackKeyToSendVideo);

                ViewModel.VideoIsoFile = _videoFileName;
            }

            // Stop during video playback.
            else
            {
                // Remove playback objects.
                DisposeVideoPlayer();

                StartVideoPreview();

                // Set the button state and the message.
                UpdateUI(ButtonState.NoChange, AppResources.PlaybackStoped);

                ViewModel.VideoIsoFile = string.Empty;
            }
        }

        // Set the recording state: stop recording.
        private void StopVideoRecording()
        {
            //var partsInfo = new StringBuilder();
            //foreach (var part in _uploadableParts)
            //{
            //    partsInfo.AppendLine(part.ToString());
            //}
            //Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(partsInfo.ToString()));

            ViewModel.Duration = _timerCounter;
            //var totalSeconds = 
            try
            {

                // Stop recording.
                if (_captureSource.VideoCaptureDevice != null
                && _captureSource.State == CaptureState.Started)
                {
                    _timer.Stop();
                    _captureSource.Stop();

                    // Disconnect _fileSink.
                    _fileSink.CaptureSource = null;
                    _fileSink.IsolatedStorageFileName = null;

                    // Set the button states and the message.
                    UpdateUI(ButtonState.NoChange, AppResources.PreparingViewfinder);

                    StartVideoPreview();
                }
            }
            // If stop fails, display an error.
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    DebugText.Text = AppResources.Error.ToUpperInvariant() + ": " + e.Message;
                });
            }
        }

        private void StartRecording_Click(object sender, System.EventArgs e)
        {
            Timer.Opacity = 0.3;
            // Avoid duplicate taps.
            _startRecordingButton.IsEnabled = false;

            StartVideoRecording();
        }

        // Set recording state: start recording.
        private void StartVideoRecording()
        {
            _fileId = TLLong.Random();
            _isPartReady = true;
            _uploadingLength = 0;
            _uploadableParts.Clear();

            try
            {
                // Connect _fileSink to _captureSource.
                if (_captureSource.VideoCaptureDevice != null
                    && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();
                    Viewfinder.Fill = new SolidColorBrush(Colors.Black);
                    // Connect the input and output of _fileSink.
                    _fileSink.CaptureSource = _captureSource;
                    _fileSink.IsolatedStorageFileName = _videoFileName;
                }


                ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(100);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {

                            // Begin recording.
                            if (_captureSource.VideoCaptureDevice != null
                                && _captureSource.State == CaptureState.Stopped)
                            {
                                Viewfinder.Fill = null;
                                _captureSource.Start();
                                Viewfinder.Fill = ViewfinderBrush;
                                _timerCounter = 0;
                                _startTime = DateTime.Now;
                                _timer.Start();
                            }

                            _captureSource.CaptureImageAsync();

                            // Set the button states and the message.
                            UpdateUI(ButtonState.Recording, AppResources.Recording);

                        }
                        catch (Exception e)
                        {
                            DebugText.Text = AppResources.Error.ToUpperInvariant() + ": " + e.Message;
                        }
                    });
                });


            }

            // If recording fails, display an error.
            catch (Exception e)
            {
                DebugText.Text = AppResources.Error.ToUpperInvariant() + ": " + e.Message;
            }
        }

        private VideoFormat GetDesiredFormat()
        {
            return _captureSource.VideoCaptureDevice.SupportedFormats.FirstOrDefault(format => format.PixelWidth == 640 && format.PixelHeight == 480);
        }

        private void OnThumbCompleted(object sender, CaptureImageCompletedEventArgs e)
        {
            var snapShot = e.Result;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = storage.OpenFile(_videoFileName + ".jpg", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    snapShot.SaveJpeg(file, 90, 67, 0, 70);
                }
            }
        }

        private void StartPlayback_Click(object sender, System.EventArgs e)
        {
            // Avoid duplicate taps.
            _startPlaybackButton.IsEnabled = false;

            // Start video playback when the file stream exists.
            if (_fileStream != null)
            {
                // VideoPlayer.Play();
            }
            // Start the video for the first time.
            else
            {
                // Stop the capture source.
                _captureSource.Stop();

                // Remove VideoBrush from the tree.
                //Viewfinder.Fill = null;

                // Create the file stream and attach it to the MediaElement.
                _fileStream = new IsolatedStorageFileStream(_videoFileName,
                                        FileMode.Open, FileAccess.Read,
                                        IsolatedStorageFile.GetUserStoreForApplication());

                //VideoPlayer.SetSource(_fileStream);

                // Add an event handler for the end of playback.
                //VideoPlayer.MediaEnded += new RoutedEventHandler(VideoPlayerMediaEnded);

                // Start video playback.
                //VideoPlayer.Play();
            }

            // Set the button state and the message.
            UpdateUI(ButtonState.Playback, AppResources.PlaybackStarted);
        }

        private void PausePlayback_Click(object sender, System.EventArgs e)
        {
            // Avoid duplicate taps.
            _pausePlaybackButton.IsEnabled = false;

            // If mediaElement exists, pause playback.
            //if (VideoPlayer != null)
            {
                //VideoPlayer.Pause();
            }

            // Set the button state and the message.
            UpdateUI(ButtonState.Paused, AppResources.PlaybackPaused);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_stopRecordingPlaybackButton.IsEnabled)
            {
                StopPlaybackRecording_Click(this, null);
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            if (!string.IsNullOrEmpty(ViewModel.VideoIsoFile))
            {
                return;
            }

            if (_startRecordingButton.IsEnabled)
            {
                StartRecording_Click(sender, e);
            }
            else if (_stopRecordingPlaybackButton.IsEnabled)
            {
                StopPlaybackRecording_Click(sender, e);
            }
        }
    }
}