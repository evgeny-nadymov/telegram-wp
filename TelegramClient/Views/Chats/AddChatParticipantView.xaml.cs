// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Microsoft.Phone.Shell;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Chats;

namespace TelegramClient.Views.Chats
{
    public partial class AddChatParticipantView
    {
        public AddChatParticipantViewModel ViewModel
        {
            get { return DataContext as AddChatParticipantViewModel; }
        }

        private readonly ApplicationBarIconButton _searchButton = new ApplicationBarIconButton
        {
            Text = AppResources.Search,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.search.rest.png", UriKind.Relative)
        };

        public AddChatParticipantView()
        {
            InitializeComponent();

            _searchButton.Click += (sender, args) => ViewModel.Search();

            Loaded += (o, e) => BuildLocalizedAppBar();
        }

        private bool _initialized;

        private void BuildLocalizedAppBar()
        {
            if (_initialized) return;

            _initialized = true;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_searchButton);
        }
    }
}