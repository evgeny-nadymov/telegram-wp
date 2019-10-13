// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;

namespace TelegramClient.Views.Controls
{
    public partial class ProxyStatusControl
    {
        public static readonly DependencyProperty ConnectionTypeProperty = DependencyProperty.Register(
            "ConnectionType", typeof(ConnectionType), typeof(ProxyStatusControl), new PropertyMetadata(default(ConnectionType), OnConnectionTypeChanged));

        private static void OnConnectionTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var proxyStatusControl = d as ProxyStatusControl;
            if (proxyStatusControl != null)
            {
                proxyStatusControl.Progress.Style = (ConnectionType) e.NewValue == ConnectionType.Direct
                    ? (Style) proxyStatusControl.Resources["DirectProgressBarStyle"]
                    : (Style) proxyStatusControl.Resources["ProxyProgressBarStyle"];
            }
        }

        public ConnectionType ConnectionType
        {
            get { return (ConnectionType) GetValue(ConnectionTypeProperty); }
            set { SetValue(ConnectionTypeProperty, value); }
        }

        public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
            "IsIndeterminate", typeof(bool), typeof(ProxyStatusControl), new PropertyMetadata(true, OnIsIndeterminateChanged));

        private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ProxyStatusControl;
            if (control != null)
            {
                control.Progress.IsIndeterminate = (bool)e.NewValue;
            }
        }

        public bool IsIndeterminate
        {
            get { return (bool) GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public ProxyStatusControl()
        {
            InitializeComponent();
        }
    }

    public enum ConnectionType
    {
        Proxy,
        Direct
    }
}
