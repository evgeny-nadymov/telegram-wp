// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLPeerDialogs : TLObject
    {
        public const uint Signature = TLConstructors.TLPeerDialogs;

        public TLVector<TLDialogBase> Dialogs { get; set; } 

        public TLVector<TLMessageBase> Messages { get; set; }

        public TLVector<TLChatBase> Chats { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public TLState State { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Dialogs = GetObject<TLVector<TLDialogBase>>(bytes, ref position);
            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);
            State = GetObject<TLState>(bytes, ref position);

            return this;
        }

        public TLDialogsBase ToDialogs()
        {
            return new TLDialogs
            {
                Dialogs = Dialogs,
                Messages = Messages,
                Chats = Chats,
                Users = Users
            };
        }
    }
}
