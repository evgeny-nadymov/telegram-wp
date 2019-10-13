// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.TL;

namespace TelegramClient.EventArgs
{
    public class UpdateChatTitleEventArgs
    {
        public TLChatBase Chat { get; set; }

        public string Title { get; set; }

        public UpdateChatTitleEventArgs(TLChatBase chat, string title)
        {
            Chat = chat;
            Title = title;
        }
    }
}
