// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.TL;

namespace Telegram.Api.Services.Cache.EventArgs
{
    public class TopMessageUpdatedEventArgs : System.EventArgs
    {
        public TLPeerBase Peer { get; protected set; }

        public TLDialogBase Dialog { get; protected set; }

        public TLMessageBase Message { get; protected set; }

        public TLDecryptedMessageBase DecryptedMessage { get; protected set; }

        public bool NotifyPinned { get; set; }

        public TopMessageUpdatedEventArgs(TLPeerBase peer)
        {
            Peer = peer;
        }

        public TopMessageUpdatedEventArgs(TLDialogBase dialog, TLMessageBase message)
        {
            Dialog = dialog;
            Message = message;
        }

        public TopMessageUpdatedEventArgs(TLDialogBase dialog, TLDecryptedMessageBase message)
        {
            Dialog = dialog;
            DecryptedMessage = message;
        }
    }
}