// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.TL;

namespace TelegramClient.ViewModels.Dialogs
{
    public class PinnedMessageViewModel : TelegramPropertyChangedBase
    {
        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public TLMessageBase Message { get; protected set; }

        public PinnedMessageViewModel(TLMessageBase message)
        {
            Message = message;
        }

        public static bool IsRequired(TLObject obj)
        {
            var userBase = obj as TLUserBase;

            return 
                userBase != null
                && userBase.IsRequest
                && !userBase.RemoveUserAction && userBase.Index != Constants.TelegramNotificationsId;
        }

        public void SetUser(TLMessageBase message)
        {
            Message = message;
            NotifyOfPropertyChange(() => Message);
        }

        public event EventHandler UnpinMessage;

        public virtual void RaiseUnpinMessage()
        {
            var handler = UnpinMessage;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler OpenMessage;

        public virtual void RaiseOpenMessage()
        {
            var handler = OpenMessage;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public void Close()
        {
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);
        } 

        public event EventHandler Closed;

        public virtual void RaiseClosed()
        {
            var handler = Closed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }
    }
}
