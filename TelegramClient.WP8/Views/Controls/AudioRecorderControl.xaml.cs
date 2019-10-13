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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Storage;
using Caliburn.Micro;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels;
using TelegramClient_Opus;
using Action = System.Action;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Controls
{
    public partial class AudioRecorderControl
    {
        public bool UploadFileDuringRecording { get; set; }

        public string RecorderImageSource
        {
            get
            {
                var currentBackground = IoC.Get<IStateService>().CurrentBackground;
                if (currentBackground != null && currentBackground.Name != "Empty")
                {
                    return "/Images/Audio/microphone.light.png";
                }

                var isLightTheme = (Visibility) Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/Audio/microphone.dark.png";
                }

                return "/Images/Audio/microphone.light.png";
            }
        }

        private bool _isLogEnabled = false;

        private void Log(string str)
        {
            if (!_isLogEnabled) return;

            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("  HH:mm:ss.fff ") + str);
        }

        private DateTime? _lastTypingTime;
        private bool _isHintStoryboardPlaying;
        private bool _isSliding;
        private Microphone _microphone;
        private byte[] _buffer;
        private TimeSpan _duration;
        private TimeSpan _recordedDuration;
        private DateTime _startTime;
        private volatile bool _stopRequested;
        private volatile bool _cancelRequested;
        private MemoryStream _stream;
        private XnaAsyncDispatcher _asyncDispatcher;
        private string _fileName = "audio.mp3";

        private WindowsPhoneRuntimeComponent _component;

        protected WindowsPhoneRuntimeComponent Component
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return null;

                _component = _component ?? new WindowsPhoneRuntimeComponent();

                return _component;
            }
        }

        private DateTime? _lastUpdateTime;

        private long _uploadingLength;
        private volatile bool _isPartReady;
        private int _skipBuffersCount;
        private TLLong _fileId;
        private readonly List<UploadablePart> _uploadableParts = new List<UploadablePart>();

        private void OnTimerTick()
        {
            //if (_lastUpdateTime.HasValue && (DateTime.Now - _lastUpdateTime.Value).TotalMilliseconds < 50.0) return;

            _lastUpdateTime = DateTime.Now;

            if (Duration != null) Duration.Text = (DateTime.Now - _startTime).ToString(@"mm\:ss\.ff");
        }

        public AudioRecorderControl()
        {
            ShellViewModel.WriteTimer("AudioRecorderControl ctor");

            InitializeComponent();

            ShellViewModel.WriteTimer("AudioRecorderControl ctor InitializeComponent");

            ShellViewModel.WriteTimer("AudioRecorderControl ctor asyncDispatcher");

            RecordImage.Source = new BitmapImage(new Uri(RecorderImageSource, UriKind.Relative));

            Loaded += (o, e) =>
            {
                if (_microphone != null)
                {
                    System.Diagnostics.Debug.WriteLine("+OnBufferReady");
                    _microphone.BufferReady += Microphone_OnBufferReady;
                }
            };
            Unloaded += (o, e) =>
            {
                if (_microphone != null)
                {
                    System.Diagnostics.Debug.WriteLine("-OnBufferReady");
                    _microphone.BufferReady -= Microphone_OnBufferReady;
                }
            };

            ShellViewModel.WriteTimer("AudioRecorderControl end ctor");
        }

        private void Microphone_OnBufferReady(object sender, System.EventArgs e)
        {
            const int skipStartBuffersCount = 1;

            if (Component == null) return;

            var dataLength = _microphone.GetData(_buffer);
            if (_skipBuffersCount < skipStartBuffersCount)
            {
                _skipBuffersCount++;
                return;
            }
            
            const int frameLength = 1920;
            var partsCount = dataLength / frameLength;
            _stream.Write(_buffer, 0, _buffer.Length);
            for (var i = 0; i < partsCount; i++)
            {
                var count = frameLength * (i + 1) > _buffer.Length ? _buffer.Length - frameLength * i : frameLength;
                var result = Component.WriteFrame(_buffer.SubArray(frameLength * i, count), count);
            }

            if (_stopRequested || _cancelRequested)
            {
                _microphone.Stop();
                _asyncDispatcher.StopService();
                Component.StopRecord();

                if (UploadFileDuringRecording)
                {
                    UploadAudioFileAsync(true);
                }

                if (_stopRequested)
                {
                    if ((DateTime.Now - _startTime).TotalMilliseconds < 1000.0)
                    {
                        _stopRequested = false;
                        _cancelRequested = false;
                        //Log("HintStoryboard_OnCompleted._stopRequested=false");

                        _isHintStoryboardPlaying = true;
                        HintStoryboard.Begin();
                        return;
                    }

                    RaiseAudioRecorded(_stream, (DateTime.Now - _startTime - TimeSpan.FromTicks(_microphone.BufferDuration.Ticks * skipStartBuffersCount)).TotalSeconds, _fileName, _fileId, _uploadableParts);
                    return;
                }

                if (_cancelRequested)
                {
                    RaiseRecordCanceled();
                    return;
                }
            }
            else
            {
                var now = DateTime.Now;
                if (!_lastTypingTime.HasValue
                    || _lastTypingTime.Value.AddSeconds(1.0) < now)
                {
                    _lastTypingTime = DateTime.Now;
                    RaiseRecordingAudio();
                }

                if (UploadFileDuringRecording)
                {
                    UploadAudioFileAsync(false);
                }
            }
        }

        private void UploadAudioFileAsync(bool isLastPart)
        {
            Execute.BeginOnThreadPool(() =>
            {
                if (!_isPartReady) return;

                _isPartReady = false;

                var uploadablePart = GetUploadablePart(_fileName, _uploadingLength, _uploadableParts.Count, isLastPart);
                if (uploadablePart == null)
                {
                    _isPartReady = true;
                    return;
                }

                _uploadableParts.Add(uploadablePart);
                _uploadingLength += uploadablePart.Count;

                //Execute.BeginOnUIThread(() => VibrateController.Default.Start(TimeSpan.FromSeconds(0.02)));

                if (!isLastPart)
                {
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
                        error => Execute.ShowDebugMessage("upload.saveFilePart error " + error));
                }

                _isPartReady = true;
            });
        }

        private static UploadablePart GetUploadablePart(string fileName, long position, int partId, bool isLastPart = false)
        {
            var fullFilePath = ApplicationData.Current.LocalFolder.Path + "\\" + fileName;
            var fi = new FileInfo(fullFilePath);
            if (!fi.Exists)
            {
                return null;
            }

            const int minPartLength = 1024;
            const int maxPartLength = 16 * 1024;

            var recordingLength = fi.Length - position;
            if (!isLastPart && recordingLength < minPartLength)
            {
                return null;
            }

            var subpartsCount = (int)recordingLength / minPartLength;
            var uploadingBufferSize = 0;
            if (isLastPart)
            {
                if (recordingLength > 0)
                {
                    uploadingBufferSize = Math.Min(maxPartLength, (int)recordingLength);
                }
            }
            else
            {
                uploadingBufferSize = Math.Min(maxPartLength, subpartsCount * minPartLength);
            }
            if (uploadingBufferSize == 0)
            {
                return null;
            }

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
                Execute.ShowDebugMessage("read file " + fullFilePath + " exception " + ex);
                return null;
            }

            return new UploadablePart(null, new TLInt(partId), uploadingBuffer, position, uploadingBufferSize);
        }

        private void RecordButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsHitTestVisible) return;
            var microphoneState = _microphone != null ? _microphone.State : (MicrophoneState?)null;
            if (microphoneState == MicrophoneState.Started) return;
            if (_isHintStoryboardPlaying) return;

            Log(string.Format("microphone_state={0} storyboard_state={1}", microphoneState, _isHintStoryboardPlaying));

            if (Component == null) return;
            if (_asyncDispatcher == null)
            {
                _asyncDispatcher = new XnaAsyncDispatcher(TimeSpan.FromMilliseconds(33), OnTimerTick);
            }

            if (SliderPanel == null)
            {
                SliderPanel = CreateSliderPanel();
                LayoutRoot.Children.Add(SliderPanel);
            }
            if (TimerPanel == null)
            {
                TimerPanel = CreateTimerPanel();
                LayoutRoot.Children.Add(TimerPanel);
            }

            if (_microphone == null)
            {
                _microphone = Microphone.Default;

                ShellViewModel.WriteTimer("AudioRecorderControl ctor microphone");

                if (_microphone == null)
                {
                    RecordButton.Visibility = Visibility.Collapsed;
                    Visibility = Visibility.Collapsed;
                    IsHitTestVisible = false;

                    return;
                }

                try
                {
                    _microphone.BufferDuration = TimeSpan.FromMilliseconds(240);
                    _duration = _microphone.BufferDuration;
                    _buffer = new byte[_microphone.GetSampleSizeInBytes(_microphone.BufferDuration)];
                }
                catch (Exception ex)
                {
                    TLUtils.WriteException(ex);

                    RecordButton.Visibility = Visibility.Collapsed;
                    Visibility = Visibility.Collapsed;
                    IsHitTestVisible = false;

                    return;
                }

                _microphone.BufferReady += Microphone_OnBufferReady;
            }

            _skipBuffersCount = 0;
            _fileId = TLLong.Random();
            _fileName = _fileId.Value + ".mp3";
            _isPartReady = true;
            _uploadingLength = 0;
            _uploadableParts.Clear();

            _isSliding = true;
            _stopRequested = false;
            _cancelRequested = false;

            RaiseRecordStarted();

            if (Duration != null) Duration.Text = "00:00.00";
            if (Slider != null) ((TranslateTransform)Slider.RenderTransform).X = 0.0;
            Component.StartRecord(ApplicationData.Current.LocalFolder.Path + "\\" + _fileName);

            _stream = new MemoryStream();
            _startTime = DateTime.Now; 
            VibrateController.Default.Start(TimeSpan.FromMilliseconds(25));

            Execute.BeginOnUIThread(TimeSpan.FromMilliseconds(25.0), () =>
            {
                if (!_isSliding)
                {
                    if (_stopRequested)
                    {
                        _stopRequested = false;
                        _cancelRequested = false;

                        _isHintStoryboardPlaying = true;
                        HintStoryboard.Begin();
                        return;
                    }
                    Log("_isSliding=false return");
                    return;
                }

                if (Slider != null) Slider.Visibility = Visibility.Visible;
                if (TimerPanel != null) TimerPanel.Visibility = Visibility.Visible;
                
                _asyncDispatcher.StartService(null);
                _microphone.Start();
                
                StartRecordingStoryboard();
            });
        }

        private void StartRecordingStoryboard()
        {
            var storyboard = new Storyboard();
            
            var timerAnimation = new DoubleAnimationUsingKeyFrames();
            timerAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -100.0 });
            timerAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            Storyboard.SetTarget(timerAnimation, TimerPanel);
            Storyboard.SetTargetProperty(timerAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(timerAnimation);

            var sliderAnimation = new DoubleAnimationUsingKeyFrames();
            sliderAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -100.0 });
            sliderAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            Storyboard.SetTarget(sliderAnimation, Slider);
            Storyboard.SetTargetProperty(sliderAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(sliderAnimation);

            storyboard.Begin();
        }

        private TextBlock Duration;

        private Grid SliderPanel;

        private StackPanel Slider;

        private Grid TimerPanel;

        private Grid CreateTimerPanel()
        {
            var timerPanel = new Grid{ Visibility = Visibility.Collapsed};
            timerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            timerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            timerPanel.RenderTransform = new TranslateTransform();

            Duration = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12.0, 4.0, -2.0, 0.0),
                Text = "00:00.00",
                FontSize = (double)Application.Current.Resources["PhoneFontSizeMediumLarge"],
                FontFamily = new FontFamily("Curier new")
            };
            var border = new Border
            {
                Margin = new Thickness(6.0, 2.0, 12.0, 0.0),
                Width = 6.0,
                Height = 6.0,
                Background = new SolidColorBrush(Colors.Red),
                CornerRadius = new CornerRadius(3.0)
            };
            Grid.SetColumn(border, 1);

            timerPanel.Children.Add(Duration);
            timerPanel.Children.Add(border);

            Grid.SetColumn(timerPanel, 2);

            return timerPanel;
        }

        private Grid CreateSliderPanel()
        {
            var clipGrid = new Grid { Background = new SolidColorBrush(Colors.Transparent) };
            Helpers.Clip.SetToBounds(clipGrid, true);
            Grid.SetColumn(clipGrid, 1);

            Slider = new StackPanel
            {
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(6.0, 0.0, 0.0, 0.0),
                RenderTransform = new TranslateTransform()
            };

            var textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Text = AppResources.Cancel.ToUpperInvariant(),
                FontSize = (double) Application.Current.Resources["PhoneFontSizeMediumLarge"]
            };

            var path = new System.Windows.Shapes.Path
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6.0, 0.0, 6.0, 0.0),
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeThickness = 3.0,
                Stroke = Foreground,
                RenderTransform = new TranslateTransform { X = 6.0, Y = 2.5 }
            };
            var dataBinding = new System.Windows.Data.Binding { Source = "M 0,0 7,12 0,24" };
            path.SetBinding(System.Windows.Shapes.Path.DataProperty, dataBinding);

            Slider.Children.Add(textBlock);
            Slider.Children.Add(path);

            clipGrid.Children.Add(Slider);

            return clipGrid;
        }

        private void RecordButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Log("RecordButton_OnMouseLeftButtonUp");
            StopRecording();
        }

        public event EventHandler<AudioEventArgs> AudioRecorded;

        protected virtual void RaiseAudioRecorded(MemoryStream stream, double duration, string fileName, TLLong fileId, IList<UploadablePart> parts)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("parts");
            foreach (var part in parts)
            {
                stringBuilder.AppendLine(string.Format("file_part={0} position={1} count={2} status={3}", part.FilePart, part.Position, part.Count, part.Status));
            }

            Telegram.Logs.Log.Write(string.Format("AudioRecorderControl.AudioRecorded duration={0} file_name={1}\n{2}", duration, fileName, stringBuilder));

            var handler = AudioRecorded;
            if (handler != null) handler(this, new AudioEventArgs(stream, duration, fileName, fileId, parts));
        }

        public event EventHandler<System.EventArgs> RecordCanceled;

        protected virtual void RaiseRecordCanceled()
        {
            Telegram.Logs.Log.Write("AudioRecorderControl.AudioRecordCanceled");

            var handler = RecordCanceled;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<System.EventArgs> RecordStarted;

        protected virtual void RaiseRecordStarted()
        {
            Telegram.Logs.Log.Write("AudioRecorderControl.AudioRecordStarted");

            var handler = RecordStarted;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<System.EventArgs> RecordingAudio;

        protected virtual void RaiseRecordingAudio()
        {
            var handler = RecordingAudio;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void CancelRecording()
        {
            if (Slider != null) Slider.Visibility = Visibility.Collapsed;
            if (TimerPanel != null) TimerPanel.Visibility = Visibility.Collapsed;

            if (!_stopRequested)
            {
                _cancelRequested = true;
            }
            _isSliding = false;
            _lastTypingTime = null;
        }

        private void StopRecording()
        {
            VibrateController.Default.Start(TimeSpan.FromMilliseconds(25));

            if (Slider != null) Slider.Visibility = Visibility.Collapsed;
            if (TimerPanel != null) TimerPanel.Visibility = Visibility.Collapsed;
            _stopRequested = true;
            _isSliding = false;
            _lastTypingTime = null;
        }

        private void LayoutRoot_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!_isSliding) return;
            if (Slider == null) return;


            var transform = (TranslateTransform)Slider.RenderTransform;
            transform.X += e.DeltaManipulation.Translation.X;

            if (transform.X < 0)
            {
                transform.X = 0;
            }

            if (transform.X > 200)
            {
                //SliderTransform.X = 0;
                _isSliding = false;

                CancelRecordingStoryboard.Begin();
            }
        }

        private void LayoutRoot_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!_cancelRequested && Hint.Visibility == Visibility.Collapsed)
            {
                StopRecording();
            }
        }

        private void HintStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            _isHintStoryboardPlaying = false;

            RaiseRecordCanceled();
        }

        private void CancelRecordingStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            CancelRecording();
        }
    }

    public class XnaAsyncDispatcher : IApplicationService
    {

        private readonly DispatcherTimer _timer;
        private readonly Action _tickAction;
        public XnaAsyncDispatcher(TimeSpan dispatchInterval, Action tickAction = null)
        {
            FrameworkDispatcher.Update();
            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;
            _timer.Interval = dispatchInterval;

            _tickAction = tickAction;
        }
        public void StartService(ApplicationServiceContext context)
        {
            _timer.Start();
        }

        public void StopService()
        {
            _timer.Stop();
        }

        private void TimerTick(object sender, System.EventArgs eventArgs)
        {
            try
            {
                FrameworkDispatcher.Update();
            }
            catch (Exception e)
            {
#if DEBUG
                MessageBox.Show(e.ToString());
#endif
            }
            if (_tickAction != null) _tickAction();
        }
    }

    public class AudioEventArgs : System.EventArgs
    {
        public MemoryStream PcmStream { get; set; }

        public string OggFileName { get; set; }

        public double Duration { get; set; }

        public TLLong FileId { get; set; }

        public IList<UploadablePart> Parts { get; set; }

        public AudioEventArgs(MemoryStream stream, double duration, string fileName, TLLong fileId, IList<UploadablePart> parts)
        {
            PcmStream = stream;
            Duration = duration;
            OggFileName = fileName;
            FileId = fileId;
            Parts = parts;
        }
    }
}
