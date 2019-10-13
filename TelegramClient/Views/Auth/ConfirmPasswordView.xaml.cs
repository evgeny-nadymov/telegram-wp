// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Auth;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Auth
{
    public partial class ConfirmPasswordView
    {
        public ConfirmPasswordViewModel ViewModel
        {
            get { return DataContext as ConfirmPasswordViewModel; }
        }

        public ConfirmPasswordView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.3), () => PasswordBox.Focus());
        }

        private void Passcode_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Confirm();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if LOG_REGISTRATION
            TLUtils.WriteLog("ConfirmPasswordView.OnNavigatedTo");
#endif

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
#if LOG_REGISTRATION
            TLUtils.WriteLog("ConfirmPasswordView.OnNavigatedFrom");
#endif

            base.OnNavigatedFrom(e);
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            HintTextBlock.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void PasswordBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            HintTextBlock.Foreground = (Brush) Application.Current.Resources["PhoneTextBoxForegroundBrush"];
        }

        private void PasswordBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            HintTextBlock.Foreground = (Brush) Application.Current.Resources["PhoneForegroundBrush"];
        }
    }
}