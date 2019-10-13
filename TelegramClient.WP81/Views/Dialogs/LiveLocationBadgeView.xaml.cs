// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.Views.Dialogs
{
    public partial class LiveLocationBadgeView
    {
        public LiveLocationBadgeViewModel ViewModel
        {
            get { return DataContext as LiveLocationBadgeViewModel; }
        }

        public LiveLocationBadgeView()
        {
            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                Border.Background = (Brush)Resources["InputBorderBrushLight"];
            }
            else
            {
                Border.Background = (Brush)Resources["InputBorderBrushDark"];
            }
        }

        private void Close_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.RaiseClosed();
        }

        private void Message_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.RaiseOpenMessage();
        }
    }
}
