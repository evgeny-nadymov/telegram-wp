// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class BlockedContactsView
    {

        private readonly AppBarButton _addContactButton = new AppBarButton
        {
            Text = AppResources.Add,
            IconUri = new Uri("/Images/ApplicationBar/appbar.add.rest.png", UriKind.Relative)
        };

        public BlockedContactsView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            //AnimationContext = LayoutRoot;

            _addContactButton.Click += (sender, args) => ((BlockedContactsViewModel) DataContext).AddContact();
            Loaded += (sender, args) =>
            {
                BuildLocalizedAppBar();
            };
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_addContactButton);
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            //ContextMenuService.GetContextMenu((DependencyObject)sender).IsOpen = true;       
        }
    }
}