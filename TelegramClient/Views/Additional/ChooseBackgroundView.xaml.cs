// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class ChooseBackgroundView
    {
        public ChooseBackgroundViewModel ViewModel
        {
            get { return DataContext as ChooseBackgroundViewModel; }
        }

        public ChooseBackgroundView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void TelegramNavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            ViewModel.OnForwardInAnimationComplete();
        }

        private void TelegramNavigationOutTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            LayoutRoot.Children.Clear();
        }
    }
}