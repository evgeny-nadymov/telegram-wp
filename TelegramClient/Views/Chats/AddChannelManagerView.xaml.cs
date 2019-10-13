// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Chats;

namespace TelegramClient.Views.Chats
{
    public partial class AddChannelManagerView
    {
        public AddChannelManagerViewModel ViewModel
        {
            get { return DataContext as AddChannelManagerViewModel; }
        }

        private readonly AppBarButton _doneButton = new AppBarButton
        {
            Text = AppResources.Done,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        public AddChannelManagerView()
        {
            InitializeComponent();
            _doneButton.Click += (sender, args) => ViewModel.Done();
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();

            Loaded += (o, e) => BuildLocalizedAppBar();
        }

        private bool _initialized;

        private void BuildLocalizedAppBar()
        {
            if (_initialized) return;

            _initialized = true;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_doneButton);
            ApplicationBar.Buttons.Add(_cancelButton);
        }
    }
}