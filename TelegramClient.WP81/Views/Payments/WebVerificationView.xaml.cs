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
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Payments;

namespace TelegramClient.Views.Payments
{
    public partial class WebVerificationView
    {
        public WebVerificationViewModel ViewModel
        {
            get { return DataContext as WebVerificationViewModel; }
        }

        public WebVerificationView()
        {
            InitializeComponent();

            CaptionBorder.Background = ShellView.CaptionBrush;
        }

        private void WebBrowser_OnScriptNotify(object sender, NotifyEventArgs e)
        {

        }

        private void WebPaymentView_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            var contentScaleFactor = Application.Current.Host.Content.ScaleFactor / 100.0;
            var systemTrayHeight = 72.0 / contentScaleFactor;
            CaptionTransform.X = e.Orientation == PageOrientation.LandscapeLeft ? systemTrayHeight : 0.0;
        }

        private void Browser_OnNavigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("tg://"))
            {
                e.Cancel = true;

                if (e.Uri.ToString().StartsWith("tg://resolve/?domain="))
                {
                    var previousEntry = NavigationService.BackStack.FirstOrDefault();
                    if (previousEntry != null
                        && previousEntry.Source.ToString().Contains("DialogDetailsView.xaml"))
                    {
                        var user = ViewModel.PaymentInfo.With as TLUser45;
                        if (user != null && user.UserName != null)
                        {
                            Dictionary<string, string> uriParams = null;
                            try
                            {
                                uriParams = TelegramUriMapper.ParseQueryString(e.Uri.ToString());
                            }
                            catch (Exception ex)
                            {
                                Execute.ShowDebugMessage("Parse uri exception " + e.Uri + ex);
                            }
                            if (uriParams != null)
                            {
                                if (e.Uri.ToString().Contains("domain"))
                                {
                                    // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&start=<access_token>
                                    // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&startgroup=<access_token>
                                    // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&post=<post_number>
                                    // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&game=<game>
                                    var domain = uriParams["domain"];

                                    if (string.Equals(user.UserName.ToString(), domain, StringComparison.OrdinalIgnoreCase))
                                    {
                                        NavigationService.GoBack();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ViewModel.IsWorking = true;
                }
            }
        }

        private void Browser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            ViewModel.IsWorking = false;
        }
    }
}