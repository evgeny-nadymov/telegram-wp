// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Phone
{
    public class TLDiscardCall : TLObject
    {
        public const uint Signature = 0x78d413a6;

        public TLInputPhoneCall Peer { get; set; }

        public TLInt Duration { get; set; }

        public TLPhoneCallDiscardReasonBase Reason { get; set; }

        public TLLong ConnectionId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes(),
                Duration.ToBytes(),
                Reason.ToBytes(),
                ConnectionId.ToBytes());
        }
    }
}
