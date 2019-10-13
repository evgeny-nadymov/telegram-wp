// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.ViewModels.Media;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Media
{
    public partial class FilesView
    {
        public FilesViewModel<IInputPeer> ViewModel { get { return DataContext as FilesViewModel<IInputPeer>; } }  

        public FilesView()
        {
            InitializeComponent();
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ((ISliceLoadable)DataContext).LoadNextSlice();
        }

        private void Files_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ((ISliceLoadable)DataContext).LoadNextSlice();
        }

        private void DeleteMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var channel = ViewModel.CurrentItem as TLChannel;
            menuItem.Visibility = (channel == null || channel.Creator)
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void SelectionBorder_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;

            var element = sender as FrameworkElement;
            if (element != null)
            {
                var message = element.DataContext as TLMessage;
                if (message != null)
                {
                    message.IsSelected = !message.IsSelected;
                    ViewModel.ChangeGroupActionStatus();
                }
            }
        }
    }
}