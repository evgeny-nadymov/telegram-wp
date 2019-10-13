// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using TelegramClient.ViewModels.Chats;

namespace TelegramClient.Views.Chats
{
    public partial class GroupsInCommonView
    {
        public GroupsInCommonViewModel ViewModel
        {
            get { return DataContext as GroupsInCommonViewModel; }
        }

        public GroupsInCommonView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }
    }
}