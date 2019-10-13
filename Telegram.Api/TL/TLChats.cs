// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLChatsBase : TLObject
    {
        public TLVector<TLChatBase> Chats { get; set; }
    }

    public class TLChats : TLChatsBase
    {
        public const uint Signature = TLConstructors.TLChats;

        public TLVector<TLUserBase> Users { get; set; } 

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }
    }

    public class TLChats24 : TLChatsBase
    {
        public const uint Signature = TLConstructors.TLChats24;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);

            return this;
        }
    }

    public class TLChatsSlice : TLChatsBase
    {
        public const uint Signature = TLConstructors.TLChatsSlice;

        public TLInt Count { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Count = GetObject<TLInt>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }
    }

    public class TLChatsSlice59 : TLChatsBase
    {
        public const uint Signature = TLConstructors.TLChatsSlice59;

        public TLInt Count { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Count = GetObject<TLInt>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);

            return this;
        }
    }
}
