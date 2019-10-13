// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum ChannelDifferenceFlags
    {
        Final = 0x1,
        Timeout = 0x2
    }

    public abstract class TLChannelDifferenceBase : TLObject
    {
        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt Pts { get; set; }

        protected TLInt _timeout;

        public TLInt Timeout
        {
            get { return _timeout; }
            set { SetField(out _timeout, value, ref _flags, (int) ChannelDifferenceFlags.Timeout); }
        }
    }

    public class TLChannelDifferenceEmpty : TLChannelDifferenceBase
    {
        public const uint Signature = TLConstructors.TLChannelDifferenceEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            _timeout = GetObject<TLInt>(Flags, (int) ChannelDifferenceFlags.Timeout, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Pts.ToStream(output);
            ToStream(output, Timeout, Flags, (int) ChannelDifferenceFlags.Timeout);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Pts = GetObject<TLInt>(input);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLChannelDifferenceEmpty flags={0} pts={1} timeout={2}", Flags, Pts, Timeout);
        }
    }

    public class TLChannelDifferenceTooLong : TLChannelDifferenceBase
    {
        public const uint Signature = TLConstructors.TLChannelDifferenceTooLong;

        public TLInt TopMessage { get; set; }

        public TLInt TopImportantMessage { get; set; }

        public TLInt ReadInboxMaxId { get; set; }

        public TLInt UnreadCount { get; set; }

        public TLInt UnreadMentionsCount { get; set; }

        public TLVector<TLMessageBase> Messages { get; set; }

        public TLVector<TLChatBase> Chats { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, bytes, ref position);
            TopMessage = GetObject<TLInt>(bytes, ref position);
            TopImportantMessage = GetObject<TLInt>(bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadMentionsCount = GetObject<TLInt>(bytes, ref position);
            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Pts.ToStream(output);
            ToStream(output, Timeout, Flags, (int)ChannelDifferenceFlags.Timeout);
            TopMessage.ToStream(output);
            TopImportantMessage.ToStream(output);
            ReadInboxMaxId.ToStream(output);
            UnreadCount.ToStream(output);
            UnreadMentionsCount.ToStream(output);
            Messages.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Pts = GetObject<TLInt>(input);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, input);
            TopMessage = GetObject<TLInt>(input);
            TopImportantMessage = GetObject<TLInt>(input);
            ReadInboxMaxId = GetObject<TLInt>(input);
            UnreadCount = GetObject<TLInt>(input);
            UnreadMentionsCount = GetObject<TLInt>(input);
            Messages = GetObject<TLVector<TLMessageBase>>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLChannelDifferenceTooLong flags={0} pts={1} timeout={2} new_messages={3}", Flags, Pts, Timeout, Messages.Count);
        }
    }

    public class TLChannelDifferenceTooLong53 : TLChannelDifferenceTooLong
    {
        public new const uint Signature = TLConstructors.TLChannelDifferenceTooLong53;

        public TLInt ReadOutboxMaxId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, bytes, ref position);
            TopMessage = GetObject<TLInt>(bytes, ref position);
            TopImportantMessage = new TLInt(0);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadMentionsCount = new TLInt(0);
            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Pts.ToStream(output);
            ToStream(output, Timeout, Flags, (int)ChannelDifferenceFlags.Timeout);
            TopMessage.ToStream(output);
            ReadInboxMaxId.ToStream(output);
            ReadOutboxMaxId.ToStream(output);
            UnreadCount.ToStream(output);
            Messages.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Pts = GetObject<TLInt>(input);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, input);
            TopMessage = GetObject<TLInt>(input);
            TopImportantMessage = new TLInt(0);
            ReadInboxMaxId = GetObject<TLInt>(input);
            ReadOutboxMaxId = GetObject<TLInt>(input);
            UnreadCount = GetObject<TLInt>(input);
            UnreadMentionsCount = new TLInt(0);
            Messages = GetObject<TLVector<TLMessageBase>>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLChannelDifferenceTooLong53 flags={0} pts={1} timeout={2} new_messages={3}", Flags, Pts, Timeout, Messages.Count);
        }
    }

    public class TLChannelDifferenceTooLong71 : TLChannelDifferenceTooLong53
    {
        public new const uint Signature = TLConstructors.TLChannelDifferenceTooLong71;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, bytes, ref position);
            TopMessage = GetObject<TLInt>(bytes, ref position);
            TopImportantMessage = new TLInt(0);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadMentionsCount = GetObject<TLInt>(bytes, ref position);
            Messages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Pts.ToStream(output);
            ToStream(output, Timeout, Flags, (int)ChannelDifferenceFlags.Timeout);
            TopMessage.ToStream(output);
            ReadInboxMaxId.ToStream(output);
            ReadOutboxMaxId.ToStream(output);
            UnreadCount.ToStream(output);
            UnreadMentionsCount.ToStream(output);
            Messages.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Pts = GetObject<TLInt>(input);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, input);
            TopMessage = GetObject<TLInt>(input);
            TopImportantMessage = new TLInt(0);
            ReadInboxMaxId = GetObject<TLInt>(input);
            ReadOutboxMaxId = GetObject<TLInt>(input);
            UnreadCount = GetObject<TLInt>(input);
            UnreadMentionsCount = GetObject<TLInt>(input);
            Messages = GetObject<TLVector<TLMessageBase>>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLChannelDifferenceTooLong71 flags={0} pts={1} timeout={2} new_messages={3}", Flags, Pts, Timeout, Messages.Count);
        }
    }

    public class TLChannelDifference : TLChannelDifferenceBase
    {
        public const uint Signature = TLConstructors.TLChannelDifference;

        public TLVector<TLMessageBase> NewMessages { get; set; }

        public TLVector<TLUpdateBase> OtherUpdates { get; set; }

        public TLVector<TLChatBase> Chats { get; set; } 

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, bytes, ref position);
            NewMessages = GetObject<TLVector<TLMessageBase>>(bytes, ref position);
            OtherUpdates = GetObject<TLVector<TLUpdateBase>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Pts.ToStream(output);
            ToStream(output, Timeout, Flags, (int)ChannelDifferenceFlags.Timeout);
            NewMessages.ToStream(output);
            OtherUpdates.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Pts = GetObject<TLInt>(input);
            _timeout = GetObject<TLInt>(Flags, (int)ChannelDifferenceFlags.Timeout, null, input);
            NewMessages = GetObject<TLVector<TLMessageBase>>(input);
            OtherUpdates = GetObject<TLVector<TLUpdateBase>>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLChannelDifference flags={0} pts={1} timeout={2} new_messages={3} other_updates={4}", Flags, Pts, Timeout, NewMessages.Count, OtherUpdates.Count);
        }
    }
}
