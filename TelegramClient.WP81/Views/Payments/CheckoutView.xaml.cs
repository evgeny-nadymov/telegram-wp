// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using TelegramClient.Views.Controls;

namespace TelegramClient.Views.Payments
{
    public partial class CheckoutView
    {
        public CheckoutView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            ValidatePanel.Height = TelegramApplicationBar.ApplicationBar.DefaultSize;
        }
    }
}