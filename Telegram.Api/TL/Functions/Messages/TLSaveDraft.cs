// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL.Functions.Messages
{
    public class TLSaveDraft : TLObject
    {
        public const uint Signature = 0xbc39e14b;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLInt _replyToMsgId;

        public TLInt ReplyToMsgId
        {
            get { return _replyToMsgId; }
            set { SetField(out _replyToMsgId, value, ref _flags, (int) SendFlags.ReplyToMsgId); }
        }

        public TLInputPeerBase Peer { get; set; }

        public TLString Message { get; set; }

        private TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int) SendFlags.Entities); }
        }

        public void DisableWebPagePreview()
        {
            Set(ref _flags, (int)SendFlags.NoWebpage);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(ReplyToMsgId, Flags, (int)SendFlags.ReplyToMsgId),
                Peer.ToBytes(),
                Message.ToBytes(),
                ToBytes(Entities, Flags, (int) SendFlags.Entities));
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            ToStream(output, ReplyToMsgId, Flags, (int) SendFlags.ReplyToMsgId);
            Peer.ToStream(output);
            Message.ToStream(output);
            ToStream(output, Entities, Flags, (int) SendFlags.Entities);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            ReplyToMsgId = GetObject<TLInt>(Flags, (int) SendFlags.ReplyToMsgId, null, input);
            Peer = GetObject<TLInputPeerBase>(input);
            Message = GetObject<TLString>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int) SendFlags.Entities, null, input);

            return this;
        }
    }
}
