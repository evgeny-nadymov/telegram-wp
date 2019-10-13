// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using TelegramClient.ViewModels.Debug;

namespace TelegramClient.Views.Debug
{
    public partial class LongPollView
    {
        public LongPollView()
        {
            InitializeComponent();
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            //((LongPollViewModel)DataContext).Clear();
        }

        //private void ButtonUp_OnClick(object sender, RoutedEventArgs e)
        //{
            
        //}

        private void ButtonDown_OnClick(object sender, RoutedEventArgs e)
        {
            //ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ScrollableHeight);
        }
    }
}
