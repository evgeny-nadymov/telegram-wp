// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;

namespace TelegramClient.Views.Calls
{
    public partial class CallRatingControl
    {
        public CallRatingControl()
        {
            InitializeComponent();

            Comment.FontSize = 20;
            Comment.TextBox.FontSize = 20;
            Comment.Input.TextWrapping = TextWrapping.Wrap;
            Comment.Input.AcceptsReturn = true;
        }

        private void Rating_OnValueChanged(object sender, System.EventArgs e)
        {
            if (Comment == null) return;
            if (Rating == null) return;

            Comment.Visibility = Rating.Value < 5.0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
