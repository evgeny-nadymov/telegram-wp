// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class EnterPasswordView
    {
        public EnterPasswordViewModel ViewModel
        {
            get { return DataContext as EnterPasswordViewModel; }
        }

        public EnterPasswordView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => PasswordBox.Focus();
        }

        private void Passcode_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Done();
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            HintTextBlock.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void PasswordBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            HintTextBlock.Foreground = (Brush)Application.Current.Resources["PhoneTextBoxForegroundBrush"];
        }

        private void PasswordBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            HintTextBlock.Foreground = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
        }
    }
}