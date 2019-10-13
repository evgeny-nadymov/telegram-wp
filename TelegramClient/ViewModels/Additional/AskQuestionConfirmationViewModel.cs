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
using Telegram.Api.Extensions;
using Telegram.Api.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class AskQuestionConfirmationViewModel : TelegramPropertyChangedBase
    {
        public string TelegramFaq
        {
            get { return Constants.TelegramFaq; }
        }

        public string TelegramTroubleshooting
        {
            get { return Constants.TelegramTroubleshooting; }
        }

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            protected set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    NotifyOfPropertyChange(() => IsOpen);
                }
            }
        }

        private Action<MessageBoxResult> _callback;

        public void Open(Action<MessageBoxResult> callback)
        {
            IsOpen = true;
            _callback = callback;
        }

        public void Close(MessageBoxResult result)
        {
            IsOpen = false;
            _callback.SafeInvoke(result);
        }
    }
}
