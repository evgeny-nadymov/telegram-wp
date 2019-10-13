// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows.Input;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class ChangePasswordView
    {
        public ChangePasswordViewModel ViewModel
        {
            get { return DataContext as ChangePasswordViewModel; }
        }

        public ChangePasswordView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) =>
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() => PasswordBox.Focus());
            };
        }

        private void Password_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmOrCompletePasscode();
            }
            else if (e.Key == Key.Back)
            {
                if (ConfirmPasswordBox.IsFocused
                    && ConfirmPasswordBox.Length == 0)
                {
                    PasswordBox.Focus();
                }
            }
        }

        private void ConfirmOrCompletePasscode()
        {
            if (PasswordBox.IsFocused)
            {
                if (PasswordBox.Length > 0)
                {
                    ConfirmPasswordBox.Focus();
                }
            }
            else if (ConfirmPasswordBox.IsFocused)
            {
                ViewModel.ChangePassword();
            }
        }
    }
}