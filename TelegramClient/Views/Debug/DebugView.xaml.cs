// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using TelegramClient.ViewModels;

namespace TelegramClient.Views
{
    public partial class DebugView
    {
        public DebugView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ((DebugViewModel)DataContext).Send();
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            ((DebugViewModel)DataContext).Clear();
        }

        private void ButtonDown_OnClick(object sender, RoutedEventArgs e)
        {
            //ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ScrollableHeight);
        }

        private void ButtonUp_OnClick(object sender, RoutedEventArgs e)
        {
           // ScrollViewer.ScrollToVerticalOffset(0.0);
        }
    }
}