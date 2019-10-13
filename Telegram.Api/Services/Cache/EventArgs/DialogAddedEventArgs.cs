// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Telegram.Api.TL;

namespace Telegram.Api.Services.Cache.EventArgs
{
    public class MessagesRemovedEventArgs
    {
        public TLDialogBase Dialog { get; protected set; }

        public IList<TLMessageBase> Messages { get; protected set; }

        public TLDecryptedMessageBase DecryptedMessage { get; protected set; }

        public MessagesRemovedEventArgs(TLDialogBase dialog, TLMessageBase message)
        {
            Dialog = dialog;
            Messages = new List<TLMessageBase> {message};
        }

        public MessagesRemovedEventArgs(TLDialogBase dialog, IList<TLMessageBase> messages)
        {
            Dialog = dialog;
            Messages = messages;
        }

        public MessagesRemovedEventArgs(TLDialogBase dialog, TLDecryptedMessageBase message)
        {
            Dialog = dialog;
            DecryptedMessage = message;
        }
    }

    public class DialogAddedEventArgs
    {
        public TLDialogBase Dialog { get; protected set; }

        public DialogAddedEventArgs(TLDialogBase dialog)
        {
            Dialog = dialog;
        }
    }

    public class DialogRemovedEventArgs
    {
        public TLDialogBase Dialog { get; protected set; }

        public DialogRemovedEventArgs(TLDialogBase dialog)
        {
            Dialog = dialog;
        }
    }

    public class ChannelAvailableMessagesEventArgs
    {
        public TLDialogBase Dialog { get; protected set; }

        public TLInt AvailableMinId { get; set; }

        public ChannelAvailableMessagesEventArgs(TLDialogBase dialog, TLInt availableMinId)
        {
            Dialog = dialog;
            AvailableMinId = availableMinId;
        }
    }
}