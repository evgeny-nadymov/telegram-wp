// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.Devices;

namespace TelegramClient.Views.Controls
{
    public partial class VideoTimelineControl
    {
        public static readonly DependencyProperty TrimRightProperty = DependencyProperty.Register(
            "TrimRight", typeof(TimeSpan?), typeof(VideoTimelineControl), new PropertyMetadata(default(TimeSpan?)));

        public TimeSpan? TrimRight
        {
            get { return (TimeSpan?)GetValue(TrimRightProperty); }
            set { SetValue(TrimRightProperty, value); }
        }

        public static readonly DependencyProperty TrimLeftProperty = DependencyProperty.Register(
            "TrimLeft", typeof(TimeSpan?), typeof(VideoTimelineControl), new PropertyMetadata(default(TimeSpan?)));

        public TimeSpan? TrimLeft
        {
            get { return (TimeSpan?)GetValue(TrimLeftProperty); }
            set { SetValue(TrimLeftProperty, value); }
        }

        public event EventHandler TrimChanged;

        protected virtual void RaiseTrimChanged()
        {
            var handler = TrimChanged;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<ThumbnailChangedEventArgs> ThumbnailChanged;

        protected virtual void RaiseThumbnailChanged(ThumbnailChangedEventArgs e)
        {
            var handler = ThumbnailChanged;
            if (handler != null) handler(this, e);
        }

        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
            "File", typeof(StorageFile), typeof(VideoTimelineControl), new PropertyMetadata(default(StorageFile), OnFileChanged));

        private static void OnFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as VideoTimelineControl;
            if (control != null)
            {
                control.OnFileChangedInternal(e.NewValue as StorageFile);
            }
        }

        private VideoProperties _videoProperties;

        private void OnFileChangedInternal(StorageFile storageFile)
        {
            Photos.Children.Clear();
            LeftTransform.X = -MaxTranslateX;
            LeftOpacityBorderTransform.X = -465.0;
            Left.IsHitTestVisible = false;
            RightTransform.X = MaxTranslateX;
            RightOpacityBorderTransform.X = 465.0;
            Right.IsHitTestVisible = false;
            _videoProperties = null;
            _composition = null;
            TrimRight = null;
            TrimLeft = null;
            _lastPosition = null;
            _isManipulating = false;

            if (storageFile != null)
            {
                Telegram.Api.Helpers.Execute.BeginOnThreadPool(TimeSpan.FromSeconds(1.0), async () =>
                {
                    _videoProperties = storageFile.Properties.GetVideoPropertiesAsync().AsTask().Result;
                    if (_videoProperties == null) return;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Left.IsHitTestVisible = true;
                        Right.IsHitTestVisible = true;
                    });

                    _composition = new MediaComposition();
                    var clip = await MediaClip.CreateFromFileAsync(storageFile);
                    _composition.Clips.Add(clip);

                    var scaleFactor = 100.0 / Math.Min(_videoProperties.Width, _videoProperties.Height);
                    var thumbnailWidth = _videoProperties.Orientation == VideoOrientation.Normal || _videoProperties.Orientation == VideoOrientation.Rotate180 ? (int)(_videoProperties.Width * scaleFactor) : (int)(_videoProperties.Height * scaleFactor);
                    var thumbnailHeight = _videoProperties.Orientation == VideoOrientation.Normal || _videoProperties.Orientation == VideoOrientation.Rotate180 ? (int)(_videoProperties.Height * scaleFactor) : (int)(_videoProperties.Width * scaleFactor);
                    for (var i = 0; i < 9; i++)
                    {
                        var timeStamp = new TimeSpan(_videoProperties.Duration.Ticks / 9 * i);

                        var photo = await _composition.GetThumbnailAsync(timeStamp, thumbnailWidth, thumbnailHeight, VideoFramePrecision.NearestKeyFrame);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            var stream = photo.AsStream();
                            var bitmapImage = new BitmapImage();
                            var image = new Image
                            {
                                CacheMode = new BitmapCache(),
                                Stretch = Stretch.UniformToFill,
                                Width = 50.0,
                                Height = 50.0,
                                Source = bitmapImage
                            };
                            Photos.Children.Add(image);

                            bitmapImage.SetSource(stream);
                        });
                    }
                });
            }
        }

        public StorageFile File
        {
            get { return (StorageFile)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        public VideoTimelineControl()
        {
            InitializeComponent();

            Unloaded += (o, e) =>
            {
                _isManipulating = false;
            };
        }

        private MediaComposition _composition;

        private const double MaxTranslateX = 218;

        private void Left_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            LeftTransform.X += e.DeltaManipulation.Translation.X;
            if (LeftTransform.X <= -MaxTranslateX) LeftTransform.X = -MaxTranslateX;
            if (LeftTransform.X >= RightTransform.X - (Left.ActualWidth + Right.ActualWidth) / 2) LeftTransform.X = RightTransform.X - (Left.ActualWidth + Right.ActualWidth) / 2;

            LeftOpacityBorderTransform.X = LeftTransform.X - (465.0 - MaxTranslateX);

            var trimLeft = new TimeSpan((long)((LeftTransform.X + MaxTranslateX) / (2 * MaxTranslateX) * _videoProperties.Duration.Ticks));
            if (trimLeft.Ticks < 0)
            {
                trimLeft = TimeSpan.Zero;
            }

            SetThumbnailPosition(trimLeft);

            TrimLeft = trimLeft;
            RaiseTrimChanged();
        }

        private void Right_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            RightTransform.X += e.DeltaManipulation.Translation.X;
            if (RightTransform.X >= MaxTranslateX) RightTransform.X = MaxTranslateX;
            if (RightTransform.X <= LeftTransform.X + (Left.ActualWidth + Right.ActualWidth) / 2) RightTransform.X = LeftTransform.X + (Left.ActualWidth + Right.ActualWidth) / 2;

            RightOpacityBorderTransform.X = RightTransform.X + (465.0 - MaxTranslateX);

            var trimRight = new TimeSpan((long)((RightTransform.X + MaxTranslateX) / (2 * MaxTranslateX) * _videoProperties.Duration.Ticks));
            if (trimRight.Ticks >= _videoProperties.Duration.Ticks)
            {
                trimRight = new TimeSpan(_videoProperties.Duration.Ticks - 1);
            }
            SetThumbnailPosition(trimRight);

            TrimRight = trimRight;
            RaiseTrimChanged();
        }

        private TimeSpan? _lastPosition;

        private bool _isManipulating;

        private void SetThumbnailPosition(TimeSpan position)
        {
            _lastPosition = position;
        }

        private void Slider_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _isManipulating = true;

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(async () =>
            {
                TimeSpan processedPosition;
                while (_isManipulating)
                {
                    //return;
                    var position = _lastPosition;
                    if (position != null && processedPosition != position)
                    {
                        var thumbnailWidth = _videoProperties.Orientation == VideoOrientation.Normal || _videoProperties.Orientation == VideoOrientation.Rotate180 ? (int)_videoProperties.Width : (int)_videoProperties.Height;
                        var thumbnailHeight = _videoProperties.Orientation == VideoOrientation.Normal || _videoProperties.Orientation == VideoOrientation.Rotate180 ? (int)_videoProperties.Height : (int)_videoProperties.Width;
                        var photo = await _composition.GetThumbnailAsync(position.Value,
                            0,
                            0,
                            VideoFramePrecision.NearestFrame);

                        processedPosition = position.Value;
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            RaiseThumbnailChanged(new ThumbnailChangedEventArgs { Thumbnail = photo });
                        });
                    }
                    //Thread.Sleep(100);
//#if DEBUG
//                    VibrateController.Default.Start(TimeSpan.FromMilliseconds(50));
//#endif
                }
            });
        }

        private void Slider_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _isManipulating = false;
        }
    }

    public class ThumbnailChangedEventArgs : System.EventArgs
    {
        public ImageStream Thumbnail { get; set; }
    }
}
