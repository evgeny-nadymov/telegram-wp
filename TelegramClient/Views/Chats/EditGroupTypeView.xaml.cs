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
using TelegramClient.ViewModels.Chats;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Chats
{
    public partial class EditGroupTypeView
    {
        public EditGroupTypeViewModel ViewModel
        {
            get { return DataContext as EditGroupTypeViewModel; }
        }

        public EditGroupTypeView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) =>
            {
                ViewModel.EmptyUserName += OnEmptyUserName;
            };

            Unloaded += (sender, args) =>
            {
                ViewModel.EmptyUserName -= OnEmptyUserName;
            };
        }

        private void OnEmptyUserName(object sender, System.EventArgs e)
        {
            UserName.Focus();
        }

        private void CopyInvite_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyInvite();
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            ContextMenuService.GetContextMenu((DependencyObject)sender).IsOpen = true;
        }

        private void ScrollViewer_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (FocusManager.GetFocusedElement() == UserName)
            {
                ScrollViewer.Focus();
            }
        }

        private void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }
    }
}