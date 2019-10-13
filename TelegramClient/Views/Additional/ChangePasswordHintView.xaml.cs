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
    public partial class ChangePasswordHintView 
    {
        public ChangePasswordHintViewModel ViewModel
        {
            get { return DataContext as ChangePasswordHintViewModel; }
        }

        public ChangePasswordHintView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (o, e) =>
            {
                PasswordHintLabel.Input.Focus();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    PasswordHintLabel.Input.SelectAll();
                });
            };
        }

        private void PasswordHint_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.ChangePasswordHint();
            }
        }
    }
}