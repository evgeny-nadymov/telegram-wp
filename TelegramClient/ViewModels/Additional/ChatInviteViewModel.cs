// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.Aggregator;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;

namespace TelegramClient.ViewModels.Additional
{
    public class ChatInviteViewModel : IHandle<DownloadableItem>
    {
        public TLChatInvite54 ChatInvite { get; set; }

        private readonly ITelegramEventAggregator _eventAggregator;

        public ChatInviteViewModel(ITelegramEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe(this);
        }

        public void Handle(DownloadableItem item)
        {
            var chatPhoto = item.Owner as TLChatPhoto;
            if (chatPhoto != null && chatPhoto == ChatInvite.Photo)
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    ChatInvite.NotifyOfPropertyChange(() => ChatInvite.Photo);
                });
            }
        }
    }
}
