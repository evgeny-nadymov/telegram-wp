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
using Microsoft.Phone.Shell;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class WebView
    {
        private readonly ApplicationBarMenuItem _shareMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.Share,
        };

        private readonly ApplicationBarMenuItem _copyLinkMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.CopyLink,
        };

        public WebViewModel ViewModel
        {
            get { return DataContext as WebViewModel; }
        }

        public WebView()
        {
            InitializeComponent();

            CaptionBorder.Background = ShellView.CaptionBrush;

            _shareMenuItem.Click += (sender, args) => ViewModel.Share();
            _copyLinkMenuItem.Click += (sender, args) => ViewModel.CopyLink();

            BuildApplicationBar();

            //var text = "<html><body><script>function myfunc() { if (window.external && 'notify' in window.external) { window.external.notify(JSON.stringify({eventType: 'event_type', eventData: 'event_data'})); } } function logLastEvent (eventType, eventData) { document.getElementById('game_last_event').innerText = eventType + ', ' + JSON.stringify(eventData); } </script><code id=\"game_last_event\">no events yet, undefined</code> <button onclick=\"javascript:myfunc() \">Test</button> </body> </html>";
            //Browser.NavigateToString(text);
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Minimized };
            ApplicationBar.MenuItems.Add(_shareMenuItem);
            ApplicationBar.MenuItems.Add(_copyLinkMenuItem);
        }

        private void WebBrowser_OnScriptNotify(object sender, NotifyEventArgs e)
        {
            ViewModel.ScriptNotify(e.Value);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //Browser
            //Browser.NavigateToString("javascript: TelegramGameProxy.receiveEvent('ok_start', {test: 1});");
            Browser.InvokeScript("TelegramGameProxy_receiveEvent", "game_start", Input.Text);
            //Browser.InvokeScript("TelegramGameProxy.receiveEvent", "game_start", "{test: 1}");
        }

        private void Browser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            //Browser.IsScriptEnabled = true;
        }

        private void WebView_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            var contentScaleFactor = Application.Current.Host.Content.ScaleFactor / 100.0;
            var systemTrayHeight = 72.0 / contentScaleFactor;
            CaptionTransform.X = e.Orientation == PageOrientation.LandscapeLeft ? systemTrayHeight : 0.0;
        }
    }
}