// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;

namespace TelegramClient.Views.Dialogs
{
    public partial class SecretChatDebugView
    {
        private bool _once;

        public SecretChatDebugView()
        {
            InitializeComponent();

            SearchLabel.Visibility = Visibility.Collapsed;

            Loaded += (sender, args) =>
            {
                if (!_once)
                {
                    _once = true;
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() => OpenStoryboard.Begin());
                }
            };
        }
    }
}
