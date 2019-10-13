// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class SessionsView
    {
        public SessionsView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            ContextMenuService.GetContextMenu((DependencyObject)sender).IsOpen = true;       
        }
    }
}