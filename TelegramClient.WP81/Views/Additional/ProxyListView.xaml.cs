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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.TL;
using Telegram.Api.Transport;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class ProxyListView
    {
        public ProxyListViewModel ViewModel
        {
            get { return DataContext as ProxyListViewModel; }
        }

        private TranslateTransform _frameTransform;

        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register(
            "RootFrameTransformProperty", typeof(double), typeof(ProxyListView), new PropertyMetadata(OnRootFrameTransformChanged));

        private static void OnRootFrameTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as ProxyListView;
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

        public ProxyListView()
        {
            InitializeComponent();

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
            ShareMenuIcon.Opacity = ViewModel != null && !ViewModel.SuppressSharing
                ? 1.0
                : 0.5;
        }

        private void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }

        private void Add_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.Add();
        }

        private void ShareIcon_OnTap(object sender, GestureEventArgs e)
        {
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

        private WeakEventListener<ProxyListView, object, NavigatingCancelEventArgs> _weakEventListener;

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

                var weakEventListener = new WeakEventListener<ProxyListView, object, NavigatingCancelEventArgs>(this, frame);
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

    public class ProxyTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mtProtoProxy = value as TLMTProtoProxy;
            if (mtProtoProxy != null)
            {
                return AppResources.MTProto;
            }

            return AppResources.Socks5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ProxyToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var transportService = IoC.Get<ITransportService>();
            if (transportService == null) return null;

            var proxyConfig = transportService.GetProxyConfig();
            if (proxyConfig == null) return null;

            var proxyBase = value as TLProxyBase;
            if (proxyBase != null)
            {
                switch (proxyBase.Status)
                {
                    case ProxyStatus.Available:
                        return proxyConfig.IsEnabled.Value && proxyBase.IsSelected ? AppResources.Connected : AppResources.Available;
                    case ProxyStatus.Unavailable:
                        return AppResources.Unavailable;
                    case ProxyStatus.Connecting:
                        return proxyConfig.IsEnabled.Value && proxyBase.IsSelected ? AppResources.Connecting + "..." : AppResources.Checking + "...";
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProxyToForegroundConverter : IValueConverter
    {
        public Brush AccentBrush { get; set; }

        public Brush AvailableBrush { get; set; }

        public Brush UnavailableBrush { get; set; }

        public Brush RegularBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var proxyBase = value as TLProxyBase;
            if (proxyBase != null)
            {
                if (proxyBase.IsSelected)
                {
                    return AccentBrush;
                }
                if (proxyBase.Status == ProxyStatus.Available)
                {
                    return AvailableBrush;
                }
                if (proxyBase.Status == ProxyStatus.Unavailable)
                {
                    return UnavailableBrush;
                }
            }

            return RegularBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}