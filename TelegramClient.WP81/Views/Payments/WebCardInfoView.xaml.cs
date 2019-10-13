// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using TelegramClient.ViewModels.Payments;

namespace TelegramClient.Views.Payments
{
    public partial class WebCardInfoView
    {
        public WebCardInfoViewModel ViewModel
        {
            get { return DataContext as WebCardInfoViewModel; }
        }

        public WebCardInfoView()
        {
            InitializeComponent();

            CaptionBorder.Background = ShellView.CaptionBrush;
        }

        private void WebBrowser_OnScriptNotify(object sender, NotifyEventArgs e)
        {
            ViewModel.ScriptNotify(e.Value);
        }

        private void WebPaymentView_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            var contentScaleFactor = Application.Current.Host.Content.ScaleFactor / 100.0;
            var systemTrayHeight = 72.0 / contentScaleFactor;
            CaptionTransform.X = e.Orientation == PageOrientation.LandscapeLeft ? systemTrayHeight : 0.0;
        }

        private void Browser_OnNavigating(object sender, NavigatingEventArgs e)
        {
            ViewModel.IsWorking = true;
        }

        private void Browser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            ViewModel.IsWorking = false;
        }
    }
}