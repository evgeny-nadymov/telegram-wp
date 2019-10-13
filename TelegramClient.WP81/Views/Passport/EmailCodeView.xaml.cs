// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Windows.Input;
using Telegram.Api.Helpers;
using TelegramClient.ViewModels.Passport;

namespace TelegramClient.Views.Passport
{
    public partial class EmailCodeView
    {
        public EmailCodeViewModel ViewModel
        {
            get { return DataContext as EmailCodeViewModel; }
        }

        public EmailCodeView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.3), () => Code.Focus());
        }

        private void DoneIcon_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.Confirm();
        }
    }
}