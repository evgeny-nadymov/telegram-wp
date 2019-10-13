// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Phone
{
    class TLConfirmCall : TLObject
    {
        public const uint Signature = 0x2efe1722;

        public TLInputPhoneCall Peer { get; set; }

        public TLString GA { get; set; }

        public TLLong KeyFingerprint { get; set; }

        public TLPhoneCallProtocol Protocol { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes(),
                GA.ToBytes(),
                KeyFingerprint.ToBytes(),
                Protocol.ToBytes());
        }
    }
}
