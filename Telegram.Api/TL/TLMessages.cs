// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL
{
    [Flags]
    public enum MessagesFlags
    {
        Collapsed = 0x1
    }

    [Flags]
    public enum FeedMessagesFlags
    {
        MaxPosition = 0x1,
        MinPosition = 0x2,
        ReadMaxPosition = 0x4,
    }

    public abstract class TLMessagesBase : TLObject
    {
        public TLVector<TLMessageBase> Messages { get; set; }

        public TLVector<TLChatBase> Chats { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public abstract TLMessagesBase GetEmptyObject();
    }

    public class TLMessages : TLMessagesBase
    {
        public const uint Signature = TLConstructors.TLMessages;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLMessagesBase GetEmptyObject()
        {
            return new TLMessages
            {
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLMessagesSlice : TLMessages
    {
        public new const uint Signature = TLConstructors.TLMessagesSlice;

        public TLInt Count { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Count = GetObject<TLInt>(bytes, ref position);

            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLMessagesBase GetEmptyObject()
        {
            return new TLMessagesSlice
            {
                Count = Count,
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLChannelMessages : TLMessagesSlice
    {
        public new const uint Signature = TLConstructors.TLChannelMessages;

        public TLInt Flags { get; set; }

        public TLInt Pts { get; set; }

        public TLVector<TLMessageGroup> Collapsed { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            Count = GetObject<TLInt>(bytes, ref position);

            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            if (IsSet(Flags, (int) MessagesFlags.Collapsed))
            {
                Collapsed = GetObject<TLVector<TLMessageGroup>>(bytes, ref position);
            }
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLMessagesBase GetEmptyObject()
        {
            return new TLMessagesSlice
            {
                Count = Count,
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLChannelMessages53 : TLChannelMessages
    {
        public new const uint Signature = TLConstructors.TLChannelMessages53;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            Count = GetObject<TLInt>(bytes, ref position);

            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLMessagesBase GetEmptyObject()
        {
            return new TLMessagesSlice
            {
                Count = Count,
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLFeedMessagesNotModified : TLMessagesBase
    {
        public const uint Signature = TLConstructors.TLFeedMessagesNotModified;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = new TLVector<TLMessageBase>();
            Chats = new TLVector<TLChatBase>();
            Users = new TLVector<TLUserBase>();

            return this;
        }

        public override TLMessagesBase GetEmptyObject()
        {
            return new TLFeedMessages
            {
                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLFeedMessages : TLMessagesBase
    {
        public const uint Signature = TLConstructors.TLFeedMessages;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLFeedPosition _maxPosition;

        public TLFeedPosition MaxPosition
        {
            get { return _maxPosition; }
            set { SetField(out _maxPosition, value, ref _flags, (int) FeedMessagesFlags.MaxPosition); }
        }

        protected TLFeedPosition _minPosition;

        public TLFeedPosition MinPosition
        {
            get { return _minPosition; }
            set { SetField(out _minPosition, value, ref _flags, (int)FeedMessagesFlags.MinPosition); }
        }

        protected TLFeedPosition _readMaxPosition;

        public TLFeedPosition ReadMaxPosition
        {
            get { return _readMaxPosition; }
            set { SetField(out _readMaxPosition, value, ref _flags, (int)FeedMessagesFlags.ReadMaxPosition); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            _maxPosition = GetObject<TLFeedPosition>(Flags, (int)FeedMessagesFlags.MaxPosition, null, bytes, ref position);
            _minPosition = GetObject<TLFeedPosition>(Flags, (int)FeedMessagesFlags.MinPosition, null, bytes, ref position);
            _readMaxPosition = GetObject<TLFeedPosition>(Flags, (int)FeedMessagesFlags.ReadMaxPosition, null, bytes, ref position);

            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLMessagesBase GetEmptyObject()
        {
            return new TLFeedMessages
            {
                Flags = Flags,
                MaxPosition = MaxPosition,
                MinPosition = MinPosition,
                ReadMaxPosition = ReadMaxPosition,

                Messages = new TLVector<TLMessageBase>(Messages.Count),
                Chats = new TLVector<TLChatBase>(Chats.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }
}
