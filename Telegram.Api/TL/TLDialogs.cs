// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLDialogsBase : TLObject
    {
        public TLVector<TLDialogBase> Dialogs { get; set; }

        public TLVector<TLMessageBase> Messages { get; set; }

        public TLVector<TLChatBase> Chats { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public abstract TLDialogsBase GetEmptyObject();

        public TLPeerDialogs ToPeerDialogs(TLState state)
        {
            return new TLPeerDialogs
            {
                Dialogs = Dialogs,
                Messages = Messages,
                Chats = Chats,
                Users = Users,
                State = state
            };
        }
    }
    public class TLDialogsNotModified : TLDialogsBase
    {
        public const uint Signature = TLConstructors.TLDialogsNotModified;

        public TLInt Count { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Count = GetObject<TLInt>(bytes, ref position);
            Dialogs = new TLVector<TLDialogBase>();
            Messages = new TLVector<TLMessageBase>();
            Chats = new TLVector<TLChatBase>();
            Users = new TLVector<TLUserBase>();

            return this;
        }

        public override TLDialogsBase GetEmptyObject()
        {
            return new TLDialogs
            {
                Dialogs = new TLVector<TLDialogBase>(Dialogs.Count),
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLDialogs : TLDialogsBase
    {
        public const uint Signature = TLConstructors.TLDialogs;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Dialogs = GetObject<TLVector<TLDialogBase>>(bytes, ref position);
            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLDialogsBase GetEmptyObject()
        {
            return new TLDialogs
            {
                Dialogs = new TLVector<TLDialogBase>(Dialogs.Count),
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLDialogsSlice : TLDialogsBase
    {
        public const uint Signature = TLConstructors.TLDialogsSlice;

        public TLInt Count { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Count = GetObject<TLInt>(bytes, ref position);
            Dialogs = GetObject<TLVector<TLDialogBase>>(bytes, ref position);
            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLDialogsBase GetEmptyObject()
        {
            return new TLDialogsSlice
            {
                Count = Count,
                Dialogs = new TLVector<TLDialogBase>(Dialogs.Count),
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }
}
