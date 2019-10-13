// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telegram.Api.TL;
using TelegramClient.Resources;

namespace TelegramClient.Views.Controls
{
    public partial class SecretPhotoPlaceholder
    {
        public static readonly DependencyProperty DownloadingProgressProperty = DependencyProperty.Register(
            "DownloadingProgress", typeof(double), typeof(SecretPhotoPlaceholder), new PropertyMetadata(default(double), OnProgressChanged));

        public double DownloadingProgress
        {
            get { return (double) GetValue(DownloadingProgressProperty); }
            set { SetValue(DownloadingProgressProperty, value); }
        }

        public static readonly DependencyProperty UploadingProgressProperty = DependencyProperty.Register(
            "UploadingProgress", typeof(double), typeof(SecretPhotoPlaceholder), new PropertyMetadata(default(double), OnProgressChanged));

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var secretPhotoPlaceholder = d as SecretPhotoPlaceholder;
            if (secretPhotoPlaceholder != null)
            {
                var newValue = (double)e.NewValue;

                System.Diagnostics.Debug.WriteLine("  progress=" + newValue);

                secretPhotoPlaceholder.Progress.Value = newValue;

                if (newValue >= 1.0)
                {
                    secretPhotoPlaceholder.Progress.Completed += secretPhotoPlaceholder.OnProgressCompleted;
                }
                else if (newValue > 0.0 && newValue < 1.0)
                {
                    secretPhotoPlaceholder.Icons.Visibility = Visibility.Collapsed;
                }
                else
                {
                    secretPhotoPlaceholder.Icons.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnProgressCompleted(object sender, System.EventArgs e)
        {
            Progress.Completed -= OnProgressCompleted;
            
            Icons.Visibility = Visibility.Visible;
        }

        public double UploadingProgress
        {
            get { return (double) GetValue(UploadingProgressProperty); }
            set { SetValue(UploadingProgressProperty, value); }
        }

        private bool _showHint = true;

        public bool ShowHint
        {
            get { return _showHint; }
            set { _showHint = value; }
        }

        private const double OpenDelaySeconds = 0.15;

        public static readonly DependencyProperty TTLParamsProperty = DependencyProperty.Register(
            "TTLParams", typeof (TTLParams), typeof (SecretPhotoPlaceholder), new PropertyMetadata(default(TTLParams), OnTTLParamsChanged));

        public TTLParams TTLParams
        {
            get { return (TTLParams) GetValue(TTLParamsProperty); }
            set { SetValue(TTLParamsProperty, value); }
        }

        private static void OnTTLParamsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var secretPhotoPlaceholder = (SecretPhotoPlaceholder)d;
            var oldTTLParams = e.OldValue as TTLParams;
            var newTTLParams = e.NewValue as TTLParams;

            if (newTTLParams != null)
            {
                if (newTTLParams.IsStarted)
                {
                    if (newTTLParams.Out)
                    {
                        secretPhotoPlaceholder.GasIcon.Visibility = Visibility.Collapsed;
                        secretPhotoPlaceholder.CheckIcon.Visibility = Visibility.Visible;

                        return;
                    }

                    var progressAnimation = secretPhotoPlaceholder.TimerProgressAnimation;
                    var elapsed = (DateTime.Now - newTTLParams.StartTime).TotalSeconds;

                    var remaining = newTTLParams.Total - elapsed;
                    if (remaining > 0)
                    {
                        progressAnimation.From = remaining / newTTLParams.Total * 359.0;
                        progressAnimation.Duration = TimeSpan.FromSeconds(remaining);

                        secretPhotoPlaceholder.GasIcon.Visibility = Visibility.Collapsed;
                        secretPhotoPlaceholder.TimerProgress.Visibility = Visibility.Visible;
                        secretPhotoPlaceholder.TimerStoryboard.Begin();
                    }
                    else
                    {
                        secretPhotoPlaceholder.GasIcon.Visibility = Visibility.Visible;
                        secretPhotoPlaceholder.CheckIcon.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    secretPhotoPlaceholder.GasIcon.Visibility = Visibility.Visible;
                    secretPhotoPlaceholder.CheckIcon.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                secretPhotoPlaceholder.GasIcon.Visibility = Visibility.Visible;
                secretPhotoPlaceholder.CheckIcon.Visibility = Visibility.Collapsed;
            }
        }

        public SecretPhotoPlaceholder()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.03);
            _timer.Tick += OnTimerTick;

            Loaded += (sender, args) =>
            {
                //if (TimerStoryboard.GetCurrentState() == ClockState.Active)
                //{
                //    var elapsed = (DateTime.Now - TTLParams.StartTime).TotalSeconds;

                //    var remaining = TTLParams.Total - elapsed;
                //    if (TimerStoryboard.Duration.TimeSpan.TotalSeconds > remaining)
                //    {
                //        TimerStoryboard.Seek(TimeSpan.FromSeconds(TimerStoryboard.Duration.TimeSpan.TotalSeconds - remaining));
                //    }
                //    else
                //    {
                //        TimerStoryboard.SkipToFill();
                //    }
                //}
            };
            Unloaded += (sender, args) =>
            {
                _timer.Stop();
                //TimerStoryboard.Stop();
            };
        }

        public event EventHandler StartTimer;

        protected virtual void RaiseStartTimer()
        {
            var handler = StartTimer;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void OnTimerTick(object sender, System.EventArgs e)
        {
            if (_leftButtonDownTime.HasValue)
            {
                if ((DateTime.Now - _leftButtonDownTime.Value).TotalSeconds > OpenDelaySeconds)
                {
                    _timer.Stop();
                    _leftButtonDownTime = null;

                    //StartTimerAnimation();
                    RaiseStartTimer();
                }
            }
            else
            {
                _timer.Stop();
            }
        }

        //private void StartTimerAnimation()
        //{
            
        //}

        public event EventHandler Elapsed;

        protected virtual void RaiseElapsed()
        {
            var handler = Elapsed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void TimerStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            RaiseElapsed();
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.2), () =>
            {
                GasIcon.Visibility = Visibility.Visible;
                CheckIcon.Visibility = Visibility.Collapsed;
            });
        }

        private DateTime? _leftButtonDownTime;

        private readonly DispatcherTimer _timer;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _leftButtonDownTime = DateTime.Now;
            _timer.Start();

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (ShowHint && _leftButtonDownTime.HasValue && (DateTime.Now - _leftButtonDownTime.Value).TotalSeconds < OpenDelaySeconds)
            {
                MessageBox.Show(AppResources.TapAndHoldToView);
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _leftButtonDownTime = null;

            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _leftButtonDownTime = null;

            base.OnMouseMove(e);
        }

        public event EventHandler CancelUploading;

        protected virtual void RaiseCancelUploading()
        {
            var handler = CancelUploading;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler CancelDownloading;

        protected virtual void RaiseCancelDownloading()
        {
            var handler = CancelDownloading;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void LayoutRoot_OnTap(object sender, GestureEventArgs e)
        {
            if (UploadingProgress > 0.0 && UploadingProgress < 1.0)
            {
                e.Handled = true;
                RaiseCancelUploading();
            }
            else if (DownloadingProgress > 0.0 && DownloadingProgress < 1.0)
            {
                e.Handled = true;
                RaiseCancelDownloading();
            }
        }
    }
}
