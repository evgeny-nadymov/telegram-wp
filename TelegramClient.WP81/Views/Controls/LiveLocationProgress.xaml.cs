// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Resources;

namespace TelegramClient.Views.Controls
{
    public partial class LiveLocationProgress
    {
        public static List<LiveLocationProgress> Controls = new List<LiveLocationProgress>();

        public static readonly DependencyProperty MediaProperty = DependencyProperty.Register(
            "Media", typeof(TLMessageMediaBase), typeof(LiveLocationProgress), new PropertyMetadata(default(TLMessageMediaBase), OnMediaChanged));

        private static void OnMediaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progress = d as LiveLocationProgress;
            if (progress != null)
            {
                var mediaGeoLive = e.NewValue as TLMessageMediaGeoLive;
                if (mediaGeoLive != null)
                {
                    progress.UpdateAnimation(mediaGeoLive);
                }
            }
        }

        public void Update()
        {
            UpdateAnimation(Media as TLMessageMediaGeoLive);
        }

        private void UpdateAnimation(TLMessageMediaGeoLive mediaGeoLive)
        {
            if (mediaGeoLive == null || mediaGeoLive.Date == null)
            {
                Label.Text = "0";
                Progress.Angle = 0.0;
                if (_storyboard.GetCurrentState() == ClockState.Active)
                {
                    _storyboard.SkipToFill(); // will invoke storyboard.Completed
                }
                return;
            }

            var date = TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now);

            var lastPartSeconds = (double)mediaGeoLive.Date.Value + mediaGeoLive.Period.Value - date.Value;
            if (lastPartSeconds <= 0)
            {
                Label.Text = "0";
                Progress.Angle = 0.0;
                if (_storyboard.GetCurrentState() == ClockState.Active)
                {
                    _storyboard.SkipToFill(); // will invoke storyboard.Completed
                }
                return;
            }
            if (lastPartSeconds > mediaGeoLive.Period.Value)
            {
                lastPartSeconds = mediaGeoLive.Period.Value - 1.0;
            }

            Label.Text = GetLabelText(lastPartSeconds);

            var percent = lastPartSeconds / mediaGeoLive.Period.Value;
            var angle = 359.0 * percent;

            _progressAnimation.From = angle;
            _progressAnimation.Duration = TimeSpan.FromSeconds(lastPartSeconds);

            Progress.Angle = angle;
            _storyboard.Begin();

            SetNextTimer(lastPartSeconds);
        }

        private string GetLabelText(double lastPartTotalSeconds)
        {
            if (lastPartTotalSeconds < 0)
            {
                return "0";
            }
            if (lastPartTotalSeconds < 60)
            {
                return Math.Ceiling(lastPartTotalSeconds).ToString();
            }
            if (lastPartTotalSeconds < 60 * 60)
            {
                return Math.Ceiling(lastPartTotalSeconds / 60).ToString();
            }

            return string.Format("{0}{1}", Math.Ceiling(lastPartTotalSeconds / 60 / 60), AppResources.HourShort.ToLowerInvariant());
        }

        public TLMessageMediaBase Media
        {
            get { return (TLMessageMediaBase) GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }

        public event EventHandler Completed;

        protected virtual void RaiseCompleted()
        {
            var handler = Completed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private readonly Storyboard _storyboard;

        private readonly DoubleAnimation _progressAnimation;

        private readonly Timer _timer;

        public LiveLocationProgress()
        {
            InitializeComponent();

            _timer = new Timer(Timer_OnTick);

            _storyboard = new Storyboard();
            _storyboard.Completed += (sender, args) =>
            {
                RaiseCompleted();
            };
            _progressAnimation = new DoubleAnimation{ To = 0.0 };
            Storyboard.SetTarget(_progressAnimation, Progress);
            Storyboard.SetTargetProperty(_progressAnimation, new PropertyPath(ProgressPieSlice.AngleProperty));
            _storyboard.Children.Add(_progressAnimation);

            Loaded += (sender, args) =>
            {
                Controls.Add(this);

                UpdateAnimation(Media as TLMessageMediaGeoLive);
            };
            Unloaded += (sender, args) =>
            {
                Controls.Remove(this);

                _storyboard.Stop();
                _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            };
        }

        private void Timer_OnTick(object state)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                var mediaGeoLive = Media as TLMessageMediaGeoLive;
                if (mediaGeoLive == null || mediaGeoLive.Date == null)
                {
                    Label.Text = "0";
                    Progress.Angle = 0.0;
                    
                    return;
                }

                var date = TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now);

                var lastPartSeconds = (double)mediaGeoLive.Date.Value + mediaGeoLive.Period.Value - date.Value;
                if (lastPartSeconds <= 0)
                {
                    Label.Text = "0";
                    Progress.Angle = 0.0;

                    return;
                }

                Label.Text = GetLabelText(lastPartSeconds);

                SetNextTimer(lastPartSeconds);
            });
        }

        private void SetNextTimer(double lastPartSeconds)
        {
            if (lastPartSeconds < 60)
            {
                _timer.Change(TimeSpan.FromSeconds(0.5), Timeout.InfiniteTimeSpan);
                return;
            }
            if (lastPartSeconds < 60 * 2)
            {
                _timer.Change(TimeSpan.FromSeconds(lastPartSeconds - 60.0), Timeout.InfiniteTimeSpan);
                return;
            }

            if (lastPartSeconds <= 60 * 60)
            {
                _timer.Change(TimeSpan.FromSeconds(lastPartSeconds - (int)(lastPartSeconds / 60.0) * 60.0), Timeout.InfiniteTimeSpan);
                return;
            }

            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }
}
