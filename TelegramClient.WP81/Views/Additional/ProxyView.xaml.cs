// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Telegram.Api.Extensions;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Additional;
using TelegramClient.Views.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class ProxyView
    {
        public ProxyViewModel ViewModel
        {
            get { return DataContext as ProxyViewModel; }
        }

        private TranslateTransform _frameTransform;

        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register(
            "RootFrameTransformProperty", typeof(double), typeof(ProxyView), new PropertyMetadata(OnRootFrameTransformChanged));

        private static void OnRootFrameTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as ProxyView;
            if (view != null)
            {
                view._frameTransform.Y = 0;
            }
        }

        public double RootFrameTransform
        {
            get { return (double)GetValue(RootFrameTransformProperty); }
            set { SetValue(RootFrameTransformProperty, value); }
        }

        private void SetRootFrameBinding()
        {
            var frame = (Frame)Application.Current.RootVisual;
            _frameTransform = ((TranslateTransform)((TransformGroup)frame.RenderTransform).Children[0]);
            var binding = new Binding("Y")
            {
                Source = _frameTransform
            };
            SetBinding(RootFrameTransformProperty, binding);
        }

        private void RemoveRootFrameBinding()
        {
            ClearValue(RootFrameTransformProperty);
        }

        public ProxyView()
        {
            InitializeComponent();

            ServerLabel.TextBox.TextChanged += (sender, args) =>
            {
                UpdateShareButton();
            };
            PortLabel.TextBox.TextChanged += (sender, args) =>
            {
                UpdateShareButton();
            };

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) =>
            {
                UpdateShareButton();
                SetRootFrameBinding();
            };
            Unloaded += (sender, args) =>
            {
                RemoveRootFrameBinding();
            };
        }

        private void UpdateShareButton()
        {
            ShareMenuIcon.Opacity = ViewModel != null && !ViewModel.SuppressSharing && ViewModel.IsDoneEnabled
                ? 1.0
                : 0.5;
        }

        private void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }

        private void ServerLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PortLabel.Focus();
            }
        }

        private void PortLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                || (e.Key >= Key.D0 && e.Key <= Key.D9))
            {

            }
            else if (e.Key == Key.Enter)
            {
                if (ViewModel.IsSocks5Proxy)
                {
                    UsernameLabel.Focus();
                }
                else
                {
                    SecretLabel.Focus();
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Username_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordLabel.Focus();
            }
        }

        private void Done_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Done();
            }
        }

        private void ShareIcon_OnTap(object sender, GestureEventArgs e)
        {
            if (ShareMenuIcon.Opacity < 1.0) return;

            ViewModel.Share();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups().ToList();
            var popup = popups.FirstOrDefault();
            if (popup != null)
            {
                e.Cancel = true;

                var shareMessagePicker = popup.Child as ShareMessagePicker;
                if (shareMessagePicker != null)
                {
                    shareMessagePicker.TryClose();
                }

                return;
            }

            base.OnBackKeyPress(e);
        }

        private ShareMessagePicker _shareMessagePicker;

        private WeakEventListener<ProxyView, object, NavigatingCancelEventArgs> _weakEventListener;

        public void OpenShareMessagePicker(string link, Action<PickDialogEventArgs> callback = null)
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

                var weakEventListener = new WeakEventListener<ProxyView, object, NavigatingCancelEventArgs>(this, frame);
                frame.Navigating += weakEventListener.OnEvent;

                weakEventListener.OnEventAction = (view, o, args) =>
                {
                    view.Frame_Navigating(o, args);
                };
                weakEventListener.OnDetachAction = (listener, source) =>
                {
                    var f = source as PhoneApplicationFrame;
                    if (f != null)
                    {
                        f.Navigating -= listener.OnEvent;
                    }
                };

                _weakEventListener = weakEventListener;
            }

            if (page == null) return;

            var popup = new Popup();
            var sharePicker = new ShareMessagePicker
            {
                Width = page.ActualWidth,
                Height = page.ActualHeight,
                Link = link
            };
            _shareMessagePicker = sharePicker;
            page.SizeChanged += Page_SizeChanged;

            sharePicker.Close += (sender, args) =>
            {
                _shareMessagePicker = null;
                _weakEventListener.Detach();
                _weakEventListener = null;

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
            _shareMessagePicker.Pick += (sender, args) =>
            {
                callback.SafeInvoke(args);
            };

            popup.Child = sharePicker;
            popup.IsOpen = true;
        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_shareMessagePicker != null)
            {
                _shareMessagePicker.ForceClose();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}