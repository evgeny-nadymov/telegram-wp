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
    public class TLSendMedia : TLObject, IRandomId
    {
        public const uint Signature = 0xb8d1262b;

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
                var replyToMsgId = value != null && value.Value > 0 ? value : null;

                SetField(out _replyToMsgId, replyToMsgId, ref _flags, (int)SendFlags.ReplyToMsgId);
            }
        }

        public TLInputMediaBase Media { get; set; }

        public TLString Message { get; set; }

        public TLLong RandomId { get; set; }

        private TLReplyKeyboardBase _replyMarkup;

        public TLReplyKeyboardBase ReplyMarkup
        {
            get { return _replyMarkup; }
            set { SetField(out _replyMarkup, value, ref _flags, (int)SendFlags.ReplyMarkup); }
        }

        protected TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int)SendFlags.Entities); }
        }

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
                Media.ToBytes(),
                Message.ToBytes(),
                RandomId.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)SendFlags.ReplyMarkup),
                ToBytes(Entities, Flags, (int)SendFlags.Entities)
            );
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Peer.ToStream(output);
            ToStream(output, ReplyToMsgId, Flags, (int)SendFlags.ReplyToMsgId);
            Media.ToStream(output);
            Message.ToStream(output);
            RandomId.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)SendFlags.ReplyMarkup);
            ToStream(output, Entities, Flags, (int)SendFlags.Entities);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLInputPeerBase>(input);
            ReplyToMsgId = GetObject<TLInt>(Flags, (int)SendFlags.ReplyToMsgId, null, input);
            Media = GetObject<TLInputMediaBase>(input);
            Message = GetObject<TLString>(input);
            RandomId = GetObject<TLLong>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)SendFlags.ReplyMarkup, null, input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)SendFlags.Entities, null, input);

            return this;
        }
    }
}
