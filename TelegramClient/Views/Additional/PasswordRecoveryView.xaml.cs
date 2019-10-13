// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Input;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class PasswordRecoveryView
    {
        public PasswordRecoveryViewModel ViewModel
        {
            get { return DataContext as PasswordRecoveryViewModel; }
        }

        public PasswordRecoveryView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => CodeLabel.Focus();
        }

        private void Text_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.RecoverPassword();
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                
            }
            else if (e.Key == Key.Back)
            {

            }
            else
            {
                e.Handled = true;
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ForgotPassword();
        }
    }
}