// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.ViewModels.Chats;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Chats
{
    public partial class ChannelAdministratorsView
    {
        public ChannelAdministratorsViewModel ViewModel
        {
            get { return DataContext as ChannelAdministratorsViewModel; }
        }

        public ChannelAdministratorsView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            
        }

        private void TelegramNavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            ViewModel.ForwardInAnimationComplete();
        }

        private void ContextMenu_OnLoaded(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu == null) return;

            var channel = ViewModel.CurrentItem as TLChannel;
            if (channel != null && !channel.Creator)
            {
                contextMenu.Visibility = Visibility.Collapsed;
                return;
            }

            var user = contextMenu.DataContext as TLUserBase;
            if (user == null) return;

            contextMenu.Visibility = user.Index == IoC.Get<IStateService>().CurrentUserId
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}