// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class SnapshotsView
    {
        public SnapshotsViewModel ViewModel
        {
            get { return DataContext as SnapshotsViewModel; }
        }

        private readonly AppBarButton _createSnapshot = new AppBarButton
        {
            Text = AppResources.Add,
            IconUri = new Uri("/Images/ApplicationBar/appbar.add.rest.png", UriKind.Relative)
        };

        private readonly AppBarMenuItem _createTempSnapshot = new AppBarMenuItem
        {
            Text = "Create snapshot with diff",
        };

        public SnapshotsView()
        {
            InitializeComponent();

            _createSnapshot.Click += (sender, args) => ViewModel.Create();
            _createTempSnapshot.Click += (sender, args) => ViewModel.CreateTemp();

            Loaded += (sender, args) =>
            {
                BuildLocalizedAppBar();
            };
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_createSnapshot);
            ApplicationBar.MenuItems.Add(_createTempSnapshot);
        }
    }
}