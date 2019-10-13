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
    public class TLSendMultiMedia : TLObject, IRandomId
    {
        public const uint Signature = 0x2095512f;

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
                    SetField(out _replyToMsgId, value, ref _flags, (int) SendFlags.ReplyToMsgId);
                }
            }
        }

        public TLVector<TLInputSingleMedia> MultiMedia { get; set; }

        public TLLong RandomId
        {
            get
            {
                long hash = 19;

                unchecked
                {
                    if (MultiMedia != null)
                    {
                        for (var i = 0; i < MultiMedia.Count; i++)
                        {
                            hash = hash * 31 + MultiMedia[i].RandomId.Value;
                        }
                    }
                }

                return new TLLong(hash);
            }
            set { }
        }

        public void SetSilent()
        {
            Set(ref _flags, (int)SendFlags.Silent);
        }

        public void SetBackground()
        {
            Set(ref _flags, (int)SendFlags.Background);
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
                MultiMedia.ToBytes()
            );
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Peer.ToStream(output);
            ToStream(output, ReplyToMsgId, Flags, (int)SendFlags.ReplyToMsgId);
            MultiMedia.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLInputPeerBase>(input);
            ReplyToMsgId = GetObject<TLInt>(Flags, (int) SendFlags.ReplyToMsgId, null, input);
            MultiMedia = GetObject<TLVector<TLInputSingleMedia>>(input);

            return this;
        }
    }
}
