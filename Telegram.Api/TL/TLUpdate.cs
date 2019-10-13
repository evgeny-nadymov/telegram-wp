// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    [Flags]
    public enum UpdateFlags
    {
        Geo = 0x1,          // 0
        MessageId = 0x2,    // 1 
    }

    [Flags]
    public enum UpdateServiceNotificationFlags
    {
        Popup = 0x1,        // 0
        InboxDate = 0x2,    // 1 
    }

    [Flags]
    public enum UpdateDilogPinnedFlags
    {
        Pinned = 0x1,       // 0
    }

    [Flags]
    public enum UpdatePinnedDialogsFlags
    {
        Order = 0x1,        // 0
    }

    [Flags]
    public enum UpdateBotPrecheckoutQueryFlags
    {
        Info = 0x1,                 // 0
        ShippingOptionId = 0x2,     // 1
    }

    [Flags]
    public enum UpdateReadFeedFlags
    {
        UnreadCount = 0x1,          // 0
    }

    [Flags]
    public enum UpdateDialogUnreadMarkFlags
    {
        Unread = 0x1,               // 0
    }

    public abstract class TLUpdateBase : TLObject
    {
        public abstract IList<TLInt> GetPts();
    }

    public interface IMultiPts
    {
        TLInt Pts { get; set; }

        TLInt PtsCount { get; set; }
    }

    public interface IMultiChannelPts
    {
        TLInt ChannelPts { get; set; }

        TLInt ChannelPtsCount { get; set; }
    }

    public class TLUpdateNewMessage : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateNewMessage;

        public TLMessageBase Message { get; set; }
        public TLInt Pts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLMessageBase>(bytes, ref position);

            var messageCommon = Message as TLMessageCommon;
            if (messageCommon != null) messageCommon.SetUnreadSilent(TLBool.True);

            Pts = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
            Pts.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLMessageBase>(input);
            Pts = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt> { Pts };
        }
    }

    public class TLUpdateNewMessage24 : TLUpdateNewMessage, IMultiPts
    {
        public new const uint Signature = TLConstructors.TLUpdateNewMessage24;

        public TLInt PtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLMessageBase>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            var messageCommon = Message as TLMessageCommon;
            if (messageCommon != null) messageCommon.SetUnreadSilent(TLBool.True);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLMessageBase>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdateChatParticipantAdd37 : TLUpdateChatParticipantAdd
    {
        public new const uint Signature = TLConstructors.TLUpdateChatParticipantAdd37;

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            InviterId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            UserId.ToStream(output);
            InviterId.ToStream(output);
            Date.ToStream(output);
            Version.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            InviterId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChatParticipantAdd : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChatParticipantAdd;

        public TLInt ChatId { get; set; }
        public TLInt UserId { get; set; }
        public TLInt InviterId { get; set; }
        public TLInt Version { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            InviterId = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            UserId.ToStream(output);
            InviterId.ToStream(output);
            Version.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            InviterId = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChatParticipantDelete : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChatParticipantDelete;

        public TLInt ChatId { get; set; }
        public TLInt UserId { get; set; }
        public TLInt Version { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            UserId.ToStream(output);
            Version.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateNewEncryptedMessage : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateNewEncryptedMessage;

        public TLEncryptedMessageBase Message { get; set; }
        public TLInt Qts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLEncryptedMessageBase>(bytes, ref position);
            Qts = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
            Qts.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLEncryptedMessageBase>(input);
            Qts = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateEncryption : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateEncryption;

        public TLEncryptedChatBase Chat { get; set; }
        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Chat = GetObject<TLEncryptedChatBase>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Chat.ToStream(output);
            Date.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Chat = GetObject<TLEncryptedChatBase>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateMessageId : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateMessageId;

        public TLInt Id { get; set; }

        public TLLong RandomId { get; set; }

        public override string ToString()
        {
            return string.Format("TLUpdateMessageId id={0} random_id={1}", Id, RandomId);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            RandomId = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            RandomId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            RandomId = GetObject<TLLong>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateReadMessages : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateReadMessages;

        public TLVector<TLInt> Messages { get; set; }
        public TLInt Pts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Messages.ToStream(output);
            Pts.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Messages = GetObject<TLVector<TLInt>>(input);
            Pts = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt> { Pts };
        }
    }

    public class TLUpdateReadMessages24 : TLUpdateReadMessages, IMultiPts
    {
        public new const uint Signature = TLConstructors.TLUpdateReadMessages24;

        public TLInt PtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Messages.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Messages = GetObject<TLVector<TLInt>>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdateReadMessagesContents : TLUpdateReadMessages, IMultiPts
    {
        public new const uint Signature = TLConstructors.TLUpdateReadMessagesContents;

        public TLInt PtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Messages.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Messages = GetObject<TLVector<TLInt>>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public abstract class TLUpdateReadHistory : TLUpdateBase, IMultiPts
    {
        public TLPeerBase Peer { get; set; }

        public TLInt MaxId { get; set; }

        public TLInt Pts { get; set; }

        public TLInt PtsCount { get; set; }
    }

    public class TLUpdateReadHistoryInbox : TLUpdateReadHistory
    {
        public const uint Signature = TLConstructors.TLUpdateReadHistoryInbox;

        public override string ToString()
        {
            return string.Format("TLUpdateReadHistoryInbox peer={0} max_id={1} pts={2} pts_count={3}", Peer, MaxId, Pts, PtsCount);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);
            MaxId = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);
            MaxId.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);
            MaxId = GetObject<TLInt>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdateReadHistoryOutbox : TLUpdateReadHistory
    {
        public const uint Signature = TLConstructors.TLUpdateReadHistoryOutbox;

        public override string ToString()
        {
            return string.Format("TLUpdateReadHistoryOutbox peer={0} max_id={1} pts={2} pts_count={3}", Peer, MaxId, Pts, PtsCount);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);
            MaxId = GetObject<TLInt>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);
            MaxId.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);
            MaxId = GetObject<TLInt>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdateEncryptedMessagesRead : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateEncryptedMessagesRead;

        public TLInt ChatId { get; set; }
        public TLInt MaxDate { get; set; }
        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            MaxDate = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            MaxDate.ToStream(output);
            Date.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            MaxDate = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }

        public override string ToString()
        {
            return string.Format("{0} ChatId={1} MaxDate={2} Date={3}", GetType().Name, ChatId, MaxDate, Date);
        }
    }

    public class TLUpdateDeleteMessages : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateDeleteMessages;

        public TLVector<TLInt> Messages { get; set; }
        public TLInt Pts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Messages.ToStream(output);
            Pts.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Messages = GetObject<TLVector<TLInt>>(input);
            Pts = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt> { Pts };
        }
    }

    public class TLUpdateDeleteMessages24 : TLUpdateDeleteMessages, IMultiPts
    {
        public new const uint Signature = TLConstructors.TLUpdateDeleteMessages24;

        public TLInt PtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Messages.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Messages = GetObject<TLVector<TLInt>>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdateRestoreMessages : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateRestoreMessages;

        public TLVector<TLInt> Messages { get; set; }
        public TLInt Pts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Messages.ToStream(output);
            Pts.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Messages = GetObject<TLVector<TLInt>>(input);
            Pts = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt> { Pts };
        }
    }

    public interface IUserTypingAction
    {
        TLSendMessageActionBase Action { get; set; }
    }

    public abstract class TLUpdateTypingBase : TLUpdateBase
    {
        public TLInt UserId { get; set; }
    }

    public class TLUpdateUserTyping : TLUpdateTypingBase
    {
        public const uint Signature = TLConstructors.TLUpdateUserTyping;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateUserTyping17 : TLUpdateUserTyping, IUserTypingAction
    {
        public new const uint Signature = TLConstructors.TLUpdateUserTyping17;

        public TLSendMessageActionBase Action { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Action = GetObject<TLSendMessageActionBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            Action.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Action = GetObject<TLSendMessageActionBase>(input);

            return this;
        }
    }


    public class TLUpdateChatUserTyping : TLUpdateTypingBase
    {
        public const uint Signature = TLConstructors.TLUpdateChatUserTyping;

        public TLInt ChatId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            UserId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChatUserTyping17 : TLUpdateChatUserTyping, IUserTypingAction
    {
        public new const uint Signature = TLConstructors.TLUpdateChatUserTyping17;

        public TLSendMessageActionBase Action { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Action = GetObject<TLSendMessageActionBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            UserId.ToStream(output);
            Action.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            Action = GetObject<TLSendMessageActionBase>(input);

            return this;
        }
    }

    public class TLUpdateEncryptedChatTyping : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateEncryptedChatTyping;

        public TLInt ChatId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChatParticipants : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChatParticipants;

        public TLChatParticipantsBase Participants { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Participants.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Participants = GetObject<TLChatParticipantsBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateUserStatus : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateUserStatus;

        public TLInt UserId { get; set; }
        public TLUserStatus Status { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Status = GetObject<TLUserStatus>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            Status.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Status = GetObject<TLUserStatus>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateUserName : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateUserName;

        public TLInt UserId { get; set; }
        public TLString FirstName { get; set; }
        public TLString LastName { get; set; }
        public TLString UserName { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            FirstName = GetObject<TLString>(bytes, ref position);
            LastName = GetObject<TLString>(bytes, ref position);
            UserName = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            FirstName.ToStream(output);
            LastName.ToStream(output);
            UserName.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            FirstName = GetObject<TLString>(input);
            LastName = GetObject<TLString>(input);
            UserName = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateUserPhoto : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateUserPhoto;

        public TLInt UserId { get; set; }

        public TLInt Date { get; set; }

        public TLPhotoBase Photo { get; set; }

        public TLBool Previous { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Photo = GetObject<TLPhotoBase>(bytes, ref position);
            Previous = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            Date.ToStream(output);
            Photo.ToStream(output);
            Previous.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            Photo = GetObject<TLPhotoBase>(input);
            Previous = GetObject<TLBool>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateContactRegistered : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateContactRegistered;

        public TLInt UserId { get; set; }
        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            Date.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public abstract class TLUpdateContactLinkBase : TLUpdateBase
    {
        public TLInt UserId { get; set; }
    }

    public class TLUpdateContactLink : TLUpdateContactLinkBase
    {
        public const uint Signature = TLConstructors.TLUpdateContactLink;

        public TLMyLinkBase MyLink { get; set; }
        public TLForeignLinkBase ForeignLink { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            MyLink = GetObject<TLMyLinkBase>(bytes, ref position);
            ForeignLink = GetObject<TLForeignLinkBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            MyLink.ToStream(output);
            ForeignLink.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            MyLink = GetObject<TLMyLinkBase>(input);
            ForeignLink = GetObject<TLForeignLinkBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateContactLink24 : TLUpdateContactLinkBase
    {
        public const uint Signature = TLConstructors.TLUpdateContactLink24;

        public TLContactLinkBase MyLink { get; set; }
        public TLContactLinkBase ForeignLink { get; set; }

        public override string ToString()
        {
            return string.Format("TLUpdateContactLink24 user_id={0} my_link={1} foreign_link={2}", UserId, MyLink, ForeignLink);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            MyLink = GetObject<TLContactLinkBase>(bytes, ref position);
            ForeignLink = GetObject<TLContactLinkBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            MyLink.ToStream(output);
            ForeignLink.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            MyLink = GetObject<TLContactLinkBase>(input);
            ForeignLink = GetObject<TLContactLinkBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateActivation : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateActivation;

        public TLInt UserId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateNewAuthorization : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateNewAuthorization;

        public TLLong AuthKeyId { get; set; }
        public TLInt Date { get; set; }
        public TLString Device { get; set; }
        public TLString Location { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            AuthKeyId = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Device = GetObject<TLString>(bytes, ref position);
            Location = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            AuthKeyId.ToStream(output);
            Date.ToStream(output);
            Device.ToStream(output);
            Location.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            AuthKeyId = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            Device = GetObject<TLString>(input);
            Location = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateDCOptions : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateDCOptions;

        public TLVector<TLDCOption> DCOptions { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            DCOptions = GetObject<TLVector<TLDCOption>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            DCOptions.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            DCOptions = GetObject<TLVector<TLDCOption>>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateNotifySettings : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateNotifySettings;

        public TLNotifyPeerBase Peer { get; set; }

        public TLPeerNotifySettingsBase NotifySettings { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLNotifyPeerBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);
            NotifySettings.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLNotifyPeerBase>(input);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateUserBlocked : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateUserBlocked;

        public TLInt UserId { get; set; }

        public TLBool Blocked { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Blocked = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            Blocked.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Blocked = GetObject<TLBool>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdatePrivacy : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdatePrivacy;

        public TLPrivacyKeyBase Key { get; set; }

        public TLVector<TLPrivacyRuleBase> Rules { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Key = GetObject<TLPrivacyKeyBase>(bytes, ref position);
            Rules = GetObject<TLVector<TLPrivacyRuleBase>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Key.ToStream(output);
            Rules.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Key = GetObject<TLPrivacyKeyBase>(input);
            Rules = GetObject<TLVector<TLPrivacyRuleBase>>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateUserPhone : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateUserPhone;

        public TLInt UserId { get; set; }

        public TLString Phone { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Phone = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            Phone.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Phone = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateServiceNotification : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateServiceNotification;

        public TLString Type { get; set; }

        public TLString Message { get; set; }

        public TLMessageMediaBase Media { get; set; }

        public virtual TLBool Popup { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLString>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            Media = GetObject<TLMessageMediaBase>(bytes, ref position);
            Popup = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Type.ToStream(output);
            Message.ToStream(output);
            Media.ToStream(output);
            Popup.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Type = GetObject<TLString>(input);
            Message = GetObject<TLString>(input);
            Media = GetObject<TLMessageMediaBase>(input);
            Popup = GetObject<TLBool>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateServiceNotification59 : TLUpdateServiceNotification
    {
        public new const uint Signature = TLConstructors.TLUpdateServiceNotification59;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool Popup
        {
            get { return new TLBool(IsSet(_flags, (int)UpdateServiceNotificationFlags.Popup)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)UpdateServiceNotificationFlags.Popup);
                }
            }
        }

        public TLInt InboxDate { get; set; }

        public TLVector<TLMessageEntityBase> Entities { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            InboxDate = GetObject<TLInt>(Flags, (int)UpdateServiceNotificationFlags.InboxDate, null, bytes, ref position);
            Type = GetObject<TLString>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            Media = GetObject<TLMessageMediaBase>(bytes, ref position);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, InboxDate, Flags, (int)UpdateServiceNotificationFlags.InboxDate);
            Type.ToStream(output);
            Message.ToStream(output);
            Media.ToStream(output);
            Entities.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            InboxDate = GetObject<TLInt>(Flags, (int)UpdateServiceNotificationFlags.InboxDate, null, input);
            Type = GetObject<TLString>(input);
            Message = GetObject<TLString>(input);
            Media = GetObject<TLMessageMediaBase>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateWebPage37 : TLUpdateWebPage, IMultiPts
    {
        public new const uint Signature = TLConstructors.TLUpdateWebPage37;

        public TLInt Pts { get; set; }

        public TLInt PtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            WebPage = GetObject<TLWebPageBase>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            WebPage.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            WebPage = GetObject<TLWebPageBase>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdateWebPage : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateWebPage;

        public TLWebPageBase WebPage { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            WebPage = GetObject<TLWebPageBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            WebPage.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            WebPage = GetObject<TLWebPageBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    [Flags]
    public enum UpdateChannelTooLongFlags
    {
        ChannelPts = 0x1
    }

    public class TLUpdateChannelTooLong49 : TLUpdateChannelTooLong
    {
        public new const uint Signature = TLConstructors.TLUpdateChannelTooLong49;

        public TLInt Flags { get; set; }

        public TLInt ChannelPts { get; set; }

        public override string ToString()
        {
            return string.Format("TLUpdateChannelTooLong49 channel_id={0} channel_pts={1} flags={2}", ChannelId, ChannelPts, Flags);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            ChannelId = GetObject<TLInt>(bytes, ref position);
            ChannelPts = GetObject<TLInt>(Flags, (int)UpdateChannelTooLongFlags.ChannelPts, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ChannelId.ToStream(output);
            ToStream(output, ChannelPts, Flags, (int)UpdateChannelTooLongFlags.ChannelPts);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            ChannelId = GetObject<TLInt>(input);
            ChannelPts = GetObject<TLInt>(Flags, (int)UpdateChannelTooLongFlags.ChannelPts, null, input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannelTooLong : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChannelTooLong;

        public TLInt ChannelId { get; set; }

        public override string ToString()
        {
            return "TLUpdateChannelTooLong channel_id=" + ChannelId;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannelGroup : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChannelGroup;

        public TLInt ChannelId { get; set; }

        public TLMessageGroup Group { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            Group = GetObject<TLMessageGroup>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            Group.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            Group = GetObject<TLMessageGroup>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateNewChannelMessage : TLUpdateBase, IMultiChannelPts
    {
        public const uint Signature = TLConstructors.TLUpdateNewChannelMessage;

        public TLMessageBase Message { get; set; }

        public TLInt ChannelPts { get; set; }

        public TLInt ChannelPtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLMessageBase>(bytes, ref position);

            var messageCommon = Message as TLMessageCommon;
            if (messageCommon != null) messageCommon.SetUnreadSilent(TLBool.True);

            ChannelPts = GetObject<TLInt>(bytes, ref position);
            ChannelPtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
            ChannelPts.ToStream(output);
            ChannelPtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLMessageBase>(input);
            ChannelPts = GetObject<TLInt>(input);
            ChannelPtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }

        public override string ToString()
        {
            var toId = Message is TLMessageCommon ? ((TLMessageCommon) Message).ToId : null;
            return string.Format("{0} message_id={1} channel=[{2}]", GetType(), Message.Index, toId);
        }
    }

    public class TLUpdateReadChannelInbox : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateReadChannelInbox;

        public TLInt ChannelId { get; set; }

        public TLInt MaxId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            MaxId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            MaxId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            MaxId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateDeleteChannelMessages : TLUpdateBase, IMultiChannelPts
    {
        public const uint Signature = TLConstructors.TLUpdateDeleteChannelMessages;

        public TLInt ChannelId { get; set; }

        public TLVector<TLInt> Messages { get; set; }

        public TLInt ChannelPts { get; set; }

        public TLInt ChannelPtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);
            ChannelPts = GetObject<TLInt>(bytes, ref position);
            ChannelPtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            Messages.ToStream(output);
            ChannelPts.ToStream(output);
            ChannelPtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            Messages = GetObject<TLVector<TLInt>>(input);
            ChannelPts = GetObject<TLInt>(input);
            ChannelPtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannelMessageViews : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChannelMessageViews;

        public TLInt ChannelId { get; set; }

        public TLInt Id { get; set; }

        public TLInt Views { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            Views = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            Id.ToStream(output);
            Views.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            Views = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannel : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChannel;

        public TLInt ChannelId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return "TLUpdateChannel channel_id=" + ChannelId;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChatAdmins : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChatAdmins;

        public TLInt ChatId { get; set; }

        public TLBool Enabled { get; set; }

        public TLInt Version { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            Enabled = GetObject<TLBool>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            Enabled.ToStream(output);
            Version.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            Enabled = GetObject<TLBool>(input);
            Version = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChatParticipantAdmin : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChatParticipantAdmin;

        public TLInt ChatId { get; set; }

        public TLInt UserId { get; set; }

        public TLBool IsAdmin { get; set; }

        public TLInt Version { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            IsAdmin = GetObject<TLBool>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChatId.ToStream(output);
            UserId.ToStream(output);
            IsAdmin.ToStream(output);
            Version.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            IsAdmin = GetObject<TLBool>(input);
            Version = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateNewStickerSet : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateNewStickerSet;

        public TLMessagesStickerSet Stickerset { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Stickerset = GetObject<TLMessagesStickerSet>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Stickerset.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Stickerset = GetObject<TLMessagesStickerSet>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    [Flags]
    public enum UpdateStickerSetsOrderFlags
    {
        Masks = 0x1
    }

    public class TLUpdateStickerSetsOrder56 : TLUpdateStickerSetsOrder
    {
        public new const uint Signature = TLConstructors.TLUpdateStickerSetsOrder56;

        public TLInt Flags { get; set; }

        public bool Masks { get { return IsSet(Flags, (int)UpdateStickerSetsOrderFlags.Masks); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Order = GetObject<TLVector<TLLong>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Order.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Order = GetObject<TLVector<TLLong>>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateStickerSetsOrder : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateStickerSetsOrder;

        public TLVector<TLLong> Order { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Order = GetObject<TLVector<TLLong>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Order.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Order = GetObject<TLVector<TLLong>>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateStickerSets : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateStickerSets;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateSavedGifs : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateSavedGifs;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotInlineQuery : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateBotInlineQuery;

        public TLLong QueryId { get; set; }

        public TLInt UserId { get; set; }

        public TLString Query { get; set; }

        public TLString Offset { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Query = GetObject<TLString>(bytes, ref position);
            Offset = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            QueryId.ToStream(output);
            UserId.ToStream(output);
            Query.ToStream(output);
            Offset.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            Query = GetObject<TLString>(input);
            Offset = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotInlineQuery51 : TLUpdateBotInlineQuery
    {
        public new const uint Signature = TLConstructors.TLUpdateBotInlineQuery51;

        public TLInt Flags { get; set; }

        public TLGeoPointBase Geo { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Query = GetObject<TLString>(bytes, ref position);
            Geo = GetObject<TLGeoPointBase>(Flags, (int)UpdateFlags.Geo, null, bytes, ref position);
            Offset = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            QueryId.ToStream(output);
            UserId.ToStream(output);
            Query.ToStream(output);
            ToStream(output, Geo, Flags, (int)UpdateFlags.Geo);
            Offset.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            Query = GetObject<TLString>(input);
            Geo = GetObject<TLGeoPointBase>(Flags, (int)UpdateFlags.Geo, null, input);
            Offset = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotInlineSend : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateBotInlineSend;

        public TLInt Flags { get; set; }

        public TLInt UserId { get; set; }

        public TLString Query { get; set; }

        public TLGeoPointBase Geo { get; set; }

        public TLString Id { get; set; }

        public TLInputBotInlineMessageId MessageId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Query = GetObject<TLString>(bytes, ref position);
            Geo = GetObject<TLGeoPointBase>(Flags, (int)UpdateFlags.Geo, null, bytes, ref position);
            Id = GetObject<TLString>(bytes, ref position);
            MessageId = GetObject<TLInputBotInlineMessageId>(Flags, (int)UpdateFlags.MessageId, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
            Query.ToStream(output);
            ToStream(output, Geo, Flags, (int)UpdateFlags.Geo);
            Id.ToStream(output);
            ToStream(output, MessageId, Flags, (int)UpdateFlags.MessageId);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Query = GetObject<TLString>(input);
            Geo = GetObject<TLGeoPointBase>(Flags, (int)UpdateFlags.Geo, null, input);
            Id = GetObject<TLString>(input);
            MessageId = GetObject<TLInputBotInlineMessageId>(Flags, (int)UpdateFlags.MessageId, null, input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateEditChannelMessage : TLUpdateBase, IMultiChannelPts
    {
        public const uint Signature = TLConstructors.TLUpdateEditChannelMessage;

        public TLMessageBase Message { get; set; }

        public TLInt ChannelPts { get; set; }

        public TLInt ChannelPtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            //Execute.ShowDebugMessage(string.Format("TLUpdateEditChannelMessage.FromBytes channel_pts={0} channel_pts_count={1} message={2}", ChannelPts, ChannelPtsCount, Message));

            Message = GetObject<TLMessageBase>(bytes, ref position);
            ChannelPts = GetObject<TLInt>(bytes, ref position);
            ChannelPtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
            ChannelPts.ToStream(output);
            ChannelPtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLMessageBase>(input);
            ChannelPts = GetObject<TLInt>(input);
            ChannelPtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannelPinnedMessage : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChannelPinnedMessage;

        public TLInt ChannelId { get; set; }

        public TLInt Id { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    [Flags]
    public enum UpdateBotCallbackQueryFlags
    {
        Data = 0x1,
        GameId = 0x2,
    }

    public class TLUpdateBotCallbackQuery56 : TLUpdateBotCallbackQuery
    {
        public new const uint Signature = TLConstructors.TLUpdateBotCallbackQuery56;

        public TLInt Flags { get; set; }

        public TLInt GameId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);
            MessageId = GetObject<TLInt>(bytes, ref position);
            Data = GetObject<TLString>(Flags, (int)UpdateBotCallbackQueryFlags.Data, null, bytes, ref position);
            GameId = GetObject<TLInt>(Flags, (int)UpdateBotCallbackQueryFlags.GameId, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            QueryId.ToStream(output);
            UserId.ToStream(output);
            Peer.ToStream(output);
            MessageId.ToStream(output);
            ToStream(output, Data, Flags, (int)UpdateBotCallbackQueryFlags.Data);
            ToStream(output, GameId, Flags, (int)UpdateBotCallbackQueryFlags.GameId);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);
            MessageId = GetObject<TLInt>(input);
            Data = GetObject<TLString>(Flags, (int)UpdateBotCallbackQueryFlags.Data, null, input);
            GameId = GetObject<TLInt>(Flags, (int)UpdateBotCallbackQueryFlags.GameId, null, input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotCallbackQuery : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateBotCallbackQuery;

        public TLLong QueryId { get; set; }

        public TLInt UserId { get; set; }

        public TLPeerBase Peer { get; set; }

        public TLInt MessageId { get; set; }

        public TLString Data { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);
            MessageId = GetObject<TLInt>(bytes, ref position);
            Data = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            QueryId.ToStream(output);
            UserId.ToStream(output);
            Peer.ToStream(output);
            MessageId.ToStream(output);
            Data.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);
            MessageId = GetObject<TLInt>(input);
            Data = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    [Flags]
    public enum UpdateInlineBotCallbackQueryFlags
    {
        Data = 0x1,
        GameId = 0x2
    }

    public class TLUpdateInlineBotCallbackQuery56 : TLUpdateInlineBotCallbackQuery
    {
        public new const uint Signature = TLConstructors.TLUpdateInlineBotCallbackQuery56;

        public TLInt Flags { get; set; }

        public TLInt GameId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            MessageId = GetObject<TLInputBotInlineMessageId>(bytes, ref position);
            Data = GetObject<TLString>(Flags, (int)UpdateInlineBotCallbackQueryFlags.Data, null, bytes, ref position);
            GameId = GetObject<TLInt>(Flags, (int)UpdateInlineBotCallbackQueryFlags.GameId, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            QueryId.ToStream(output);
            UserId.ToStream(output);
            MessageId.ToStream(output);
            ToStream(output, Data, Flags, (int)UpdateBotCallbackQueryFlags.Data);
            ToStream(output, GameId, Flags, (int)UpdateBotCallbackQueryFlags.GameId);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            MessageId = GetObject<TLInputBotInlineMessageId>(input);
            Data = GetObject<TLString>(Flags, (int)UpdateInlineBotCallbackQueryFlags.Data, null, input);
            GameId = GetObject<TLInt>(Flags, (int)UpdateInlineBotCallbackQueryFlags.GameId, null, input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateInlineBotCallbackQuery : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateInlineBotCallbackQuery;

        public TLLong QueryId { get; set; }

        public TLInt UserId { get; set; }

        public TLInputBotInlineMessageId MessageId { get; set; }

        public TLString Data { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            MessageId = GetObject<TLInputBotInlineMessageId>(bytes, ref position);
            Data = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            QueryId.ToStream(output);
            UserId.ToStream(output);
            MessageId.ToStream(output);
            Data.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            MessageId = GetObject<TLInputBotInlineMessageId>(input);
            Data = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateEditMessage : TLUpdateBase, IMultiPts
    {
        public const uint Signature = TLConstructors.TLUpdateEditMessage;

        public TLMessageBase Message { get; set; }

        public TLInt Pts { get; set; }

        public TLInt PtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLMessageBase>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);

            Execute.ShowDebugMessage(string.Format("TLUpdateEditMessage.FromBytes pts={0} pts_count={1} message={2}", Pts, PtsCount, Message));

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
            Pts.ToStream(output);
            PtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLMessageBase>(input);
            Pts = GetObject<TLInt>(input);
            PtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdateReadChannelOutbox : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateReadChannelOutbox;

        public TLInt ChannelId { get; set; }

        public TLInt MaxId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            MaxId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            MaxId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            MaxId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateDraftMessage : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateDraftMessage;

        public TLPeerBase Peer { get; set; }

        public TLDraftMessageBase Draft { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);
            Draft = GetObject<TLDraftMessageBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);
            Draft.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);
            Draft = GetObject<TLDraftMessageBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateReadFeaturedStickers : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateReadFeaturedStickers;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateRecentStickers : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateRecentStickers;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateConfig : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateConfig;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdatePtsChanged : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdatePtsChanged;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannelWebPage : TLUpdateWebPage, IMultiChannelPts
    {
        public new const uint Signature = TLConstructors.TLUpdateChannelWebPage;

        public TLInt ChannelId { get; set; }

        public TLInt ChannelPts { get; set; }

        public TLInt ChannelPtsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            WebPage = GetObject<TLWebPageBase>(bytes, ref position);
            ChannelPts = GetObject<TLInt>(bytes, ref position);
            ChannelPtsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            WebPage.ToStream(output);
            ChannelPts.ToStream(output);
            ChannelPtsCount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            WebPage = GetObject<TLWebPageBase>(input);
            ChannelPts = GetObject<TLInt>(input);
            ChannelPtsCount = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>(); //TLUtils.GetPtsRange(Pts, PtsCount);
        }
    }

    public class TLUpdatePhoneCall : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdatePhoneCall;

        public TLPhoneCallBase PhoneCall { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneCall = GetObject<TLPhoneCallBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PhoneCall.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            PhoneCall = GetObject<TLPhoneCallBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }

        public override string ToString()
        {
            return string.Format("TLUpdatePhoneCall PhoneCall={0}", PhoneCall);
        }
    }

    public abstract class TLUpdateDialogPinnedBase : TLUpdateBase
    {
        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Pinned
        {
            get { return IsSet(Flags, (int)UpdateDilogPinnedFlags.Pinned); }
            set { SetUnset(ref _flags, value, (int)UpdateDilogPinnedFlags.Pinned); }
        }
    }

    public class TLUpdateDialogPinned76 : TLUpdateDialogPinnedBase
    {
        public const uint Signature = TLConstructors.TLUpdateDialogPinned76;

        public TLDialogPeerBase Peer { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLDialogPeerBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Peer.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLDialogPeerBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateDialogPinned : TLUpdateDialogPinnedBase
    {
        public const uint Signature = TLConstructors.TLUpdateDialogPinned;

        public TLPeerBase Peer { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Peer.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public abstract class TLUpdatePinnedDialogsBase : TLUpdateBase
    {
        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }
    }

    public class TLUpdatePinnedDialogs76 : TLUpdatePinnedDialogsBase
    {
        public const uint Signature = TLConstructors.TLUpdatePinnedDialogs76;

        protected TLVector<TLDialogPeerBase> _order;

        public TLVector<TLDialogPeerBase> Order
        {
            get { return _order; }
            set { SetField(out _order, value, ref _flags, (int)UpdatePinnedDialogsFlags.Order); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Order = GetObject<TLVector<TLDialogPeerBase>>(Flags, (int)UpdatePinnedDialogsFlags.Order, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, Order, Flags, (int)UpdatePinnedDialogsFlags.Order);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Order = GetObject<TLVector<TLDialogPeerBase>>(Flags, (int)UpdatePinnedDialogsFlags.Order, null, input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdatePinnedDialogs : TLUpdatePinnedDialogsBase
    {
        public const uint Signature = TLConstructors.TLUpdatePinnedDialogs;

        protected TLVector<TLPeerBase> _order;

        public TLVector<TLPeerBase> Order
        {
            get { return _order; }
            set { SetField(out _order, value, ref _flags, (int)UpdatePinnedDialogsFlags.Order); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Order = GetObject<TLVector<TLPeerBase>>(Flags, (int)UpdatePinnedDialogsFlags.Order, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, Order, Flags, (int)UpdatePinnedDialogsFlags.Order);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Order = GetObject<TLVector<TLPeerBase>>(Flags, (int)UpdatePinnedDialogsFlags.Order, null, input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotWebhookJSON : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateBotWebhookJSON;

        public TLString DataJSON { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            DataJSON = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            DataJSON.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            DataJSON = GetObject<TLString>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotWebhookJSONQuery : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateBotWebhookJSONQuery;

        public TLLong QueryId { get; set; }

        public TLString DataJSON { get; set; }

        public TLInt Timeout { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            QueryId = GetObject<TLLong>(bytes, ref position);
            DataJSON = GetObject<TLString>(bytes, ref position);
            Timeout = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            QueryId.ToStream(output);
            DataJSON.ToStream(output);
            Timeout.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            QueryId = GetObject<TLLong>(input);
            DataJSON = GetObject<TLString>(input);
            Timeout = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotShippingQuery : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateBotShippingQuery;

        public TLLong QueryId { get; set; }

        public TLInt UserId { get; set; }

        public TLString Payload { get; set; }

        public TLPostAddress ShippingAddress { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Payload = GetObject<TLString>(bytes, ref position);
            ShippingAddress = GetObject<TLPostAddress>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            QueryId.ToStream(output);
            UserId.ToStream(output);
            Payload.ToStream(output);
            ShippingAddress.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            Payload = GetObject<TLString>(input);
            ShippingAddress = GetObject<TLPostAddress>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateBotPrecheckoutQuery : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateBotPrecheckoutQuery;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLLong QueryId { get; set; }

        public TLInt UserId { get; set; }

        public TLString Payload { get; set; }

        protected TLPaymentRequestedInfo _info;

        public TLPaymentRequestedInfo Info
        {
            get { return _info; }
            set { SetField(out _info, value, ref _flags, (int)UpdateBotPrecheckoutQueryFlags.Info); }
        }

        protected TLString _shippingOptionId;

        public TLString ShippingOptionId
        {
            get { return _shippingOptionId; }
            set { SetField(out _shippingOptionId, value, ref _flags, (int)UpdateBotPrecheckoutQueryFlags.ShippingOptionId); }
        }

        public TLString Currency { get; set; }

        public TLLong TotalAmount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Payload = GetObject<TLString>(bytes, ref position);
            _info = GetObject<TLPaymentRequestedInfo>(Flags, (int)UpdateBotPrecheckoutQueryFlags.Info, null, bytes, ref position);
            _shippingOptionId = GetObject<TLString>(Flags, (int)UpdateBotPrecheckoutQueryFlags.ShippingOptionId, null, bytes, ref position);
            Currency = GetObject<TLString>(bytes, ref position);
            TotalAmount = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            QueryId.ToStream(output);
            UserId.ToStream(output);
            Payload.ToStream(output);
            ToStream(output, Info, Flags, (int)UpdateBotPrecheckoutQueryFlags.Info);
            ToStream(output, ShippingOptionId, Flags, (int)UpdateBotPrecheckoutQueryFlags.ShippingOptionId);
            Currency.ToStream(output);
            TotalAmount.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            QueryId = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            Payload = GetObject<TLString>(input);
            _info = GetObject<TLPaymentRequestedInfo>(Flags, (int)UpdateBotPrecheckoutQueryFlags.Info, null, input);
            _shippingOptionId = GetObject<TLString>(Flags, (int)UpdateBotPrecheckoutQueryFlags.ShippingOptionId, null, input);
            Currency = GetObject<TLString>(input);
            TotalAmount = GetObject<TLLong>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateLangPackTooLong : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateLangPackTooLong;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateLangPack : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateLangPack;

        public TLLangPackDifference Difference { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Difference = GetObject<TLLangPackDifference>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Difference.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Difference = GetObject<TLLangPackDifference>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateFavedStickers : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateFavedStickers;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannelReadMessagesContents : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChannelReadMessagesContents;

        public TLInt ChannelId { get; set; }

        public TLVector<TLInt> Messages { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            Messages = GetObject<TLVector<TLInt>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            Messages.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            Messages = GetObject<TLVector<TLInt>>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateContactsReset : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateContactsReset;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateChannelAvailableMessages : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateChannelAvailableMessages;

        public TLInt ChannelId { get; set; }

        public TLInt AvailableMinId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);
            AvailableMinId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
            AvailableMinId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);
            AvailableMinId = GetObject<TLInt>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateReadFeed : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateReadFeed;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt FeedId { get; set; }

        public TLFeedPosition MaxPosition { get; set; }

        protected TLInt _unreadCount;

        public TLInt UnreadCount
        {
            get { return _unreadCount; }
            set { SetField(out _unreadCount, value, ref _flags, (int)UpdateReadFeedFlags.UnreadCount); }
        }

        protected TLInt _unreadMutedCount;

        public TLInt UnreadMutedCount
        {
            get { return _unreadMutedCount; }
            set { SetField(out _unreadMutedCount, value, ref _flags, (int)UpdateReadFeedFlags.UnreadCount); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            FeedId = GetObject<TLInt>(bytes, ref position);
            MaxPosition = GetObject<TLFeedPosition>(bytes, ref position);
            _unreadCount = GetObject<TLInt>(Flags, (int)UpdateReadFeedFlags.UnreadCount, null, bytes, ref position);
            _unreadMutedCount = GetObject<TLInt>(Flags, (int)UpdateReadFeedFlags.UnreadCount, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            FeedId.ToStream(output);
            MaxPosition.ToStream(output);
            ToStream(output, _unreadCount, Flags, (int)UpdateReadFeedFlags.UnreadCount);
            ToStream(output, _unreadMutedCount, Flags, (int)UpdateReadFeedFlags.UnreadCount);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            FeedId = GetObject<TLInt>(input);
            MaxPosition = GetObject<TLFeedPosition>(input);
            _unreadCount = GetObject<TLInt>(Flags, (int)UpdateReadFeedFlags.UnreadCount, null, input);
            _unreadMutedCount = GetObject<TLInt>(Flags, (int)UpdateReadFeedFlags.UnreadCount, null, input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }

    public class TLUpdateDialogUnreadMark : TLUpdateBase
    {
        public const uint Signature = TLConstructors.TLUpdateDialogUnreadMark;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Unread
        {
            get { return IsSet(_flags, (int)UpdateDialogUnreadMarkFlags.Unread); }
            set { SetUnset(ref _flags, value, (int)UpdateDialogUnreadMarkFlags.Unread); }
        }

        public TLDialogPeerBase Peer { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLDialogPeerBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Peer.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLDialogPeerBase>(input);

            return this;
        }

        public override IList<TLInt> GetPts()
        {
            return new List<TLInt>();
        }
    }
}
