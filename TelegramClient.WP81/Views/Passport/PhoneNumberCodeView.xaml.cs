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
using Telegram.Api.Helpers;
using TelegramClient.ViewModels.Passport;

namespace TelegramClient.Views.Passport
{
    public partial class PhoneNumberCodeView
    {
        public PhoneNumberCodeViewModel ViewModel
        {
            get { return DataContext as PhoneNumberCodeViewModel; }
        }

        public PhoneNumberCodeView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.3), () => Code.Focus());
        }

        private void ResendCode_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Resend();
        }

        private void DoneIcon_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.Confirm();
        }
    }
}