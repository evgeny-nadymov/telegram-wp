// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Windows.Input;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.Views.Contacts
{
    public partial class ContactInfoView
    {
        private ContactInfoViewModel ViewModel { get { return DataContext as ContactInfoViewModel; } }

        #region Application Bar
        private readonly AppBarButton _editButton = new AppBarButton
        {
            Text = AppResources.Edit,
            IconUri = new Uri("/Images/ApplicationBar/appbar.edit.png", UriKind.Relative)
        };

        private readonly AppBarButton _shareButton = new AppBarButton
        {
            Text = AppResources.Share,
            IconUri = new Uri("/Images/ApplicationBar/appbar.share.png", UriKind.Relative)
        };

        private readonly AppBarButton _searchButton = new AppBarButton
        {
            Text = AppResources.Search,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.search.rest.png", UriKind.Relative)
        };

        private readonly AppBarMenuItem _blockMenuItem = new AppBarMenuItem
        {
            Text = AppResources.BlockContact
        };

        private readonly AppBarMenuItem _unblockMenuItem = new AppBarMenuItem
        {
            Text = AppResources.UnblockContact
        };

        private readonly AppBarMenuItem _addMenuItem = new AppBarMenuItem
        {
            Text = AppResources.AddContact
        };

        private readonly AppBarMenuItem _deleteMenuItem = new AppBarMenuItem
        {
            Text = AppResources.DeleteContact
        };
        #endregion

        public ContactInfoView()
        {
            var timer = Stopwatch.StartNew();
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                TimerString.Text = timer.Elapsed.ToString();

                BuildLocalizedAppBar();
            };
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_shareButton);
            ApplicationBar.Buttons.Add(_editButton);

            if (ViewModel.CurrentItem != null && ViewModel.CurrentItem.IsContact)
            {
                ApplicationBar.MenuItems.Add(_deleteMenuItem);
            }
            else
            {
                ApplicationBar.MenuItems.Add(_addMenuItem);
            }
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            

        }
    }
}