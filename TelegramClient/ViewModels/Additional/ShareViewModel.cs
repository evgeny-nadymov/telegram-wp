// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Microsoft.Phone.Tasks;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class ShareViewModel
    {
        public string Caption { get; protected set; }

        public ShareViewModel(IStateService stateService)
        {
            _shareLink = stateService.ShareLink;
            stateService.ShareLink = null;

            _shareMessage = stateService.ShareMessage;
            stateService.ShareMessage = null;

            Caption = stateService.ShareCaption;
            stateService.ShareCaption = null;
        }

        private readonly string _shareLink;

        private readonly string _shareMessage;

        public void ShareLink()
        {
            if (string.IsNullOrEmpty(_shareLink)) return;

            var task = new ShareLinkTask
            {
                Title = AppResources.AppName,
                LinkUri = new Uri(_shareLink, UriKind.Absolute),
                Message = _shareMessage
            };
            task.Show();
        }

        public void ComposeSMS()
        {
            if (string.IsNullOrEmpty(_shareMessage)) return;

            var task = new SmsComposeTask
            {
                Body = _shareMessage
            };
            task.Show();
        }

        public void ComposeEmail()
        {
            if (string.IsNullOrEmpty(_shareMessage)) return;

            var task = new EmailComposeTask
            {
                Subject = AppResources.AppName,
                Body = _shareMessage
            };
            task.Show();
        }
    }
}
