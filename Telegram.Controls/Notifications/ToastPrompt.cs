// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Clarity.Phone.Extensions;
using Coding4Fun.Toolkit.Controls;
using Coding4Fun.Toolkit.Controls.Binding;
using Coding4Fun.Toolkit.Controls.Common;

namespace Telegram.Controls.Notifications
{
    public class ToastPrompt : PopUp<string, PopUpResult>, IDisposable, IImageSourceFull
    {
        protected Image ToastImage;
        private const string ToastImageName = "ToastImage";
        private Timer _timer;

        private TranslateTransform _translate;

        public ToastPrompt()
        {
            DefaultStyleKey = typeof(ToastPrompt);

            IsAppBarVisible = true;
            IsBackKeyOverride = true;
            IsCalculateFrameVerticalOffset = true;

            IsOverlayApplied = false;

            AnimationType = DialogService.AnimationTypes.Swivel;
            //AnimationType = DialogService.AnimationTypes.SlideHorizontal;

            ManipulationStarted += ToastPromptManipulationStarted;
            ManipulationDelta += ToastPromptManipulationDelta;
            ManipulationCompleted += ToastPromptManipulationCompleted;

            Opened += ToastPromptOpened;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SetRenderTransform();

            ToastImage = GetTemplateChild(ToastImageName) as Image;

            if (ToastImage != null && ImageSource != null)
            {
                ToastImage.Source = ImageSource;
                SetImageVisibility(ImageSource);
            }

            SetTextOrientation(TextWrapping);
        }

        public override void Show()
        {
            if (!IsTimerEnabled)
                return;

            base.Show();

            SetRenderTransform();
            PreventScrollBinding.SetIsEnabled(this, true);
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        #region Control Events
        void ToastPromptManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            PauseTimer();
        }

        void ToastPromptManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            _translate.X += e.DeltaManipulation.Translation.X;

            if (_translate.X < 0)
                _translate.X = 0;
        }

        void ToastPromptManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (e.TotalManipulation.Translation.X > 200 || e.FinalVelocities.LinearVelocity.X > 1000)
            {
                OnCompleted(new PopUpEventArgs<string, PopUpResult> { PopUpResult = PopUpResult.UserDismissed });
            }
            else if (e.TotalManipulation.Translation.X < 20)
            {
                OnCompleted(new PopUpEventArgs<string, PopUpResult> { PopUpResult = PopUpResult.Ok });
            }
            else
            {
                _translate.X = 0;
                StartTimer();
            }
        }

        void ToastPromptOpened(object sender, EventArgs e)
        {
            StartTimer();
        }

        void TimerTick(object state)
        {
            Dispatcher.BeginInvoke(() => OnCompleted(new PopUpEventArgs<string, PopUpResult> { PopUpResult = PopUpResult.NoResponse }));
        }

        public override void OnCompleted(PopUpEventArgs<string, PopUpResult> result)
        {
            if (PopUpService != null && result.PopUpResult == PopUpResult.UserDismissed)
            {
                PopUpService.AnimationType = DialogService.AnimationTypes.SlideHorizontal;   
            }

            PreventScrollBinding.SetIsEnabled(this, false);

            PauseTimer();
            Dispose();


            base.OnCompleted(result);
        }
        #endregion

        #region helper methods
        private void SetImageVisibility(ImageSource source)
        {
            ToastImage.Visibility = (source == null) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetTextOrientation(TextWrapping value)
        {
            if (value == TextWrapping.Wrap)
            {
                TextOrientation = Orientation.Vertical;
            }
        }

        private void StartTimer()
        {
            if (_timer == null)
                _timer = new Timer(TimerTick);

            _timer.Change(TimeSpan.FromMilliseconds(MillisecondsUntilHidden), TimeSpan.FromMilliseconds(-1));
        }

        private void PauseTimer()
        {
            if (_timer != null)
                _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        }

        private void SetRenderTransform()
        {
            _translate = new TranslateTransform();
            RenderTransform = _translate;
        }
        #endregion

        #region Dependency Property Callbacks
        private static void OnTextWrapping(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sender = o as ToastPrompt;

            if (sender == null || sender.ToastImage == null)
                return;

            sender.SetTextOrientation((TextWrapping)e.NewValue);
        }

        private static void OnImageSource(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sender = o as ToastPrompt;

            if (sender == null || sender.ToastImage == null)
                return;

            sender.SetImageVisibility(e.NewValue as ImageSource);
        }
        #endregion

        #region Dependency Properties / Properties

        public int MillisecondsUntilHidden
        {
            get { return (int)GetValue(MillisecondsUntilHiddenProperty); }
            set { SetValue(MillisecondsUntilHiddenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MillisecondsUntilHidden.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MillisecondsUntilHiddenProperty =
            DependencyProperty.Register("MillisecondsUntilHidden", typeof(int), typeof(ToastPrompt), new PropertyMetadata(4000));

        public bool IsTimerEnabled
        {
            get { return (bool)GetValue(IsTimerEnabledProperty); }
            set { SetValue(IsTimerEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTimerEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTimerEnabledProperty =
            DependencyProperty.Register("IsTimerEnabled", typeof(bool), typeof(ToastPrompt), new PropertyMetadata(true));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ToastPrompt), new PropertyMetadata(""));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ToastPrompt), new PropertyMetadata(""));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ToastPrompt),
            new PropertyMetadata(OnImageSource));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stretch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ToastPrompt),
            new PropertyMetadata(Stretch.None));

        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(double), typeof(ToastPrompt), new PropertyMetadata(double.NaN));

        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double), typeof(ToastPrompt), new PropertyMetadata(double.NaN));

        public Orientation TextOrientation
        {
            get { return (Orientation)GetValue(TextOrientationProperty); }
            set { SetValue(TextOrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextOrientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextOrientationProperty =
            DependencyProperty.Register("TextOrientation", typeof(Orientation), typeof(ToastPrompt), new PropertyMetadata(Orientation.Horizontal));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextWrapping.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(ToastPrompt), new PropertyMetadata(TextWrapping.NoWrap, OnTextWrapping));

        #endregion
    }
}
