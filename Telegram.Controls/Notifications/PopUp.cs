// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Clarity.Phone.Extensions;
using Coding4Fun.Toolkit.Controls;
using Coding4Fun.Toolkit.Controls.Common;
using Microsoft.Phone.Controls;

namespace Telegram.Controls.Notifications
{
    public abstract class PopUp<T, TPopUpResult> : Control
    {
        internal DialogService PopUpService;
        private PhoneApplicationPage _startingPage;
        private bool _alreadyFired;

        public event EventHandler<PopUpEventArgs<T, TPopUpResult>> Completed;
        public event EventHandler Opened;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (PopUpService != null)
            {
                // template hasn't been applied yet
                // overlay is null until now
                PopUpService.BackgroundBrush = Overlay;

                PopUpService.ApplyOverlayBackground();
                PopUpService.SetAlignmentsOnOverlay(HorizontalAlignment, VerticalAlignment);
            }
        }

        public virtual void Show()
        {
            if (IsOpen)
                return;

            if (_alreadyFired)
                throw new InvalidOperationException("Invalid control state, do not reuse object after calling Show()");

            if (PopUpService == null)
            {
                PopUpService = new DialogService
                {
                    AnimationType = AnimationType,
                    Child = this,
                    IsBackKeyOverride = IsBackKeyOverride,
                    IsOverlayApplied = IsOverlayApplied,
                    MainBodyDelay = MainBodyDelay,
                };
            }

            // this will happen if the user comes in OnNavigate or 
            // something where the DOM hasn't been created yet.
            if (PopUpService.Page == null)
            {
                Dispatcher.BeginInvoke(Show);

                return;
            }

            if (IsCalculateFrameVerticalOffset)
            {
                PopUpService.ControlVerticalOffset = -FrameTransform;
            }

            PopUpService.Closed -= PopUpClosed;
            PopUpService.Opened -= PopUpOpened;

            PopUpService.Closed += PopUpClosed;
            PopUpService.Opened += PopUpOpened;


            if (!IsAppBarVisible && PopUpService.Page.ApplicationBar != null && PopUpService.Page.ApplicationBar.IsVisible)
            {
                PopUpService.Page.ApplicationBar.IsVisible = false;

                IsSetAppBarVisibiilty = true;
            }

            _startingPage = PopUpService.Page;

            PopUpService.Show();
        }



        protected virtual TPopUpResult GetOnClosedValue()
        {
            return default(TPopUpResult);
        }

        public void Hide()
        {
            PopUpClosed(this, null);
        }



        #region Control Events
        void PopUpOpened(object sender, EventArgs e)
        {
            if (Opened != null)
                Opened(sender, e);
        }

        void PopUpClosed(object sender, EventArgs e)
        {
            if (!_alreadyFired)
            {
                OnCompleted(new PopUpEventArgs<T, TPopUpResult> { PopUpResult = GetOnClosedValue() });
                return;
            }

            ResetWorldAndDestroyPopUp();
        }

        public virtual void OnCompleted(PopUpEventArgs<T, TPopUpResult> result)
        {
            _alreadyFired = true;

            if (Completed != null)
                Completed(this, result);

            if (PopUpService != null)
                PopUpService.Hide();

            if (PopUpService != null && PopUpService.BackButtonPressed)
                ResetWorldAndDestroyPopUp();
        }
        #endregion

        #region helper methods
        private void ResetWorldAndDestroyPopUp()
        {
            if (PopUpService != null)
            {
                if (!IsAppBarVisible && IsSetAppBarVisibiilty)
                {
                    _startingPage.ApplicationBar.IsVisible = IsSetAppBarVisibiilty;
                }

                _startingPage = null;

                PopUpService.Child = null;
                PopUpService = null;
            }
        }
        #endregion

        #region Dependency Property Callbacks
        static void OnFrameTransformPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var sender = source as PopUp<T, TPopUpResult>;

            if (sender == null || sender.PopUpService == null)
                return;

            if (!sender.IsCalculateFrameVerticalOffset)
                return;

            sender.PopUpService.ControlVerticalOffset = -sender.FrameTransform;
            sender.PopUpService.CalculateVerticalOffset();
        }
        #endregion

        #region Dependency Properties / Properties
        public bool IsOpen { get { return PopUpService != null && PopUpService.IsOpen; } }
        public bool IsAppBarVisible { get; set; }

        // adjust for SIP
        private bool _isCalculateFrameVerticalOffset;

        protected bool IsCalculateFrameVerticalOffset
        {
            get { return _isCalculateFrameVerticalOffset; }
            set
            {
                _isCalculateFrameVerticalOffset = value;

                if (_isCalculateFrameVerticalOffset)
                {
                    var bind = new System.Windows.Data.Binding("Y");

                    var rootFrame = ApplicationSpace.RootFrame;

                    if (rootFrame != null)
                    {
                        var transGroup = rootFrame.RenderTransform as TransformGroup;

                        if (transGroup != null)
                        {
                            bind.Source = transGroup.Children.FirstOrDefault(t => t is TranslateTransform);
                            SetBinding(FrameTransformProperty, bind);
                        }
                    }
                }
            }
        }

        public bool IsOverlayApplied
        {
            get { return _isOverlayApplied; }
            set { _isOverlayApplied = value; }
        }
        private bool _isOverlayApplied = true;

        internal bool IsSetAppBarVisibiilty { get; set; }
        internal TimeSpan MainBodyDelay { get; set; }

        protected internal bool IsBackKeyOverride { get; set; }
        protected DialogService.AnimationTypes AnimationType { get; set; }

        double FrameTransform
        {
            get { return (double)GetValue(FrameTransformProperty); }
            set { SetValue(FrameTransformProperty, value); }
        }

        static readonly DependencyProperty FrameTransformProperty = DependencyProperty.Register(
              "FrameTransform",
              typeof(double),
              typeof(PopUp<T, TPopUpResult>),
              new PropertyMetadata(0.0, OnFrameTransformPropertyChanged));

        public Brush Overlay
        {
            get { return (Brush)GetValue(OverlayProperty); }
            set { SetValue(OverlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Overlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverlayProperty =
            DependencyProperty.Register("Overlay", typeof(Brush), typeof(PopUp<T, TPopUpResult>), new PropertyMetadata(default(SolidColorBrush)));
        #endregion
    }
}
