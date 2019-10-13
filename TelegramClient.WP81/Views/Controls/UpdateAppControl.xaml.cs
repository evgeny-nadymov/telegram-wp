// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using Caliburn.Micro;
using TelegramClient.Services;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Controls
{
    public partial class UpdateAppControl
    {
        public event EventHandler TapBottomMenu;

        public bool ShowBottomMenu
        {
            get { return UpdatePanel.Visibility == Visibility.Visible; }
            set { UpdatePanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public UpdateAppControl()
        {
            InitializeComponent();
        }

        private void Update_OnTap(object sender, GestureEventArgs e)
        {
            RaiseTapBottomMenu();
            return;
            var storeUpdateService = IoC.Get<IWindowsPhoneStoreUpdateService>();
            storeUpdateService.CheckForUpdatedVersion("Text", "Title");
        }

        protected virtual void RaiseTapBottomMenu()
        {
            var handler = TapBottomMenu;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }
    }
}
