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
    public class UserActionViewModel : TelegramPropertyChangedBase
    {
        public TLUserBase User { get; protected set; }

        public UserActionViewModel(TLUserBase user)
        {
            User = user;
        }

        public static bool IsRequired(TLObject obj)
        {
            var userBase = obj as TLUserBase;

            return 
                userBase != null
                && userBase.IsRequest
                && !userBase.RemoveUserAction && userBase.Index != Constants.TelegramNotificationsId;
        }

        public void SetUser(TLUserBase user)
        {
            User = user;
            NotifyOfPropertyChange(() => User);
        }

        public event EventHandler InvokeUserAction;

        protected virtual void RaiseInvokeUserAction()
        {
            var handler = InvokeUserAction;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler InvokeUserAction2;

        protected virtual void RaiseInvokeUserAction2()
        {
            var handler = InvokeUserAction2;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public void Invoke()
        {
            RaiseInvokeUserAction();
        }

        public void Invoke2()
        {
            RaiseInvokeUserAction2();
        }

        public void Remove()
        {
            if (User == null) return;

            User.RemoveUserAction = true;
        }
    }
}
