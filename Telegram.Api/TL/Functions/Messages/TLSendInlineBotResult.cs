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
    class TLSendInlineBotResult : TLObject, IRandomId
    {
        public const uint Signature = 0xb16e06fe;

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
            set
            {
                if (value != null && value.Value > 0)
                {
                    Set(ref _flags, (int)SendFlags.ReplyToMsgId);
                    _replyToMsgId = value;
                }
                else
                {
                    Unset(ref _flags, (int)SendFlags.ReplyToMsgId);
                }
            }
        }

        public TLLong RandomId { get; set; }

        public TLLong QueryId { get; set; }

        public TLString Id { get; set; }

        public void SetChannelMessage()
        {
            Set(ref _flags, (int)SendFlags.Channel);
        }

        public void SetSilent()
        {
            Set(ref _flags, (int)SendFlags.Silent);
        }

        public void SetClearDraft()
        {
            Set(ref _flags, (int)SendFlags.ClearDraft);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes(),
                ToBytes(ReplyToMsgId, Flags, (int)SendFlags.ReplyToMsgId),
                RandomId.ToBytes(),
                QueryId.ToBytes(),
                Id.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Peer.ToStream(output);
            ToStream(output, ReplyToMsgId, Flags, (int)SendFlags.ReplyToMsgId);
            RandomId.ToStream(output);
            QueryId.ToStream(output);
            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLInputPeerBase>(input);
            if (IsSet(Flags, (int)SendFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }
            RandomId = GetObject<TLLong>(input);
            QueryId = GetObject<TLLong>(input);
            Id = GetObject<TLString>(input);

            return this;
        }
    }
}
