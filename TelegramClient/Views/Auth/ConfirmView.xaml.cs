// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Navigation;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Auth;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Auth
{
    public partial class ConfirmView
    {
        public ConfirmViewModel ViewModel
        {
            get { return DataContext as ConfirmViewModel; }
        }

        public ConfirmView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.3), () => Code.Focus());
        }

        private void ResendCode_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Resend();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SendMail();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if LOG_REGISTRATION
            TLUtils.WriteLog("ConfirmView.OnNavigatedTo");
#endif

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
#if LOG_REGISTRATION
            TLUtils.WriteLog("ConfirmView.OnNavigatedFrom");
#endif

            base.OnNavigatedFrom(e);
        }
    }
}