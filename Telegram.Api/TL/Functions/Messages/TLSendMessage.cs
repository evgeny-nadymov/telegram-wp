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

namespace Telegram.Api.TL.Functions.Messages
{
    [Flags]
    public enum SendFlags
    {
        ReplyToMsgId = 0x1,
        NoWebpage = 0x2,
        ReplyMarkup = 0x4,
        Entities = 0x8,
        Channel = 0x10,
        Silent = 0x20,
        Background = 0x40,
        ClearDraft = 0x80,
        WithMyScore = 0x100,
        Grouped = 0x200
    }

    public interface IRandomId
    {
        TLLong RandomId { get; }
    }

    public class TLSendMessage : TLObject, IRandomId
    {
        public const uint Signature = 0xfa88427a;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputPeerBase Peer { get; set; }

        private TLInt _replyToMsgId;

        public TLInt ReplyToMsgId
        {
            get { return _replyToMsgId; }
            set { SetField(out _replyToMsgId, value, ref _flags, (int) SendFlags.ReplyToMsgId); }
        }

        public TLString Message { get; set; }

        public TLLong RandomId { get; set; }

        private TLReplyKeyboardBase _replyMarkup;

        public TLReplyKeyboardBase ReplyMarkup
        {
            get { return _replyMarkup; }
            set { SetField(out _replyMarkup, value, ref _flags, (int) SendFlags.ReplyMarkup); }
        }

        private TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int) SendFlags.Entities); }
        }

        public void NoWebpage()
        {
            Set(ref _flags, (int) SendFlags.NoWebpage);
        }

        public void SetChannelMessage()
        {
            Set(ref _flags, (int) SendFlags.Channel);
        }

        public void SetSilent()
        {
            Set(ref _flags, (int) SendFlags.Silent);
        }

        public void ClearDraft()
        {
            Set(ref _flags, (int) SendFlags.ClearDraft);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes(),
                ToBytes(ReplyToMsgId, Flags, (int) SendFlags.ReplyToMsgId),
                Message.ToBytes(),
                RandomId.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int) SendFlags.ReplyMarkup),
                ToBytes(Entities, Flags, (int) SendFlags.Entities)
                );
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Peer.ToStream(output);
            ToStream(output, ReplyToMsgId, Flags, (int) SendFlags.ReplyToMsgId);
            Message.ToStream(output);
            RandomId.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int) SendFlags.ReplyMarkup);
            ToStream(output, Entities, Flags, (int) SendFlags.Entities);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLInputPeerBase>(input);
            ReplyToMsgId = GetObject<TLInt>(Flags, (int) SendFlags.ReplyToMsgId, null, input);
            Message = GetObject<TLString>(input);
            RandomId = GetObject<TLLong>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int) SendFlags.ReplyMarkup, null, input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int) SendFlags.Entities, null, input);

            return this;
        }
    }
}
