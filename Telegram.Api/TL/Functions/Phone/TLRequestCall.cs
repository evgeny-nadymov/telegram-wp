// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Phone
{
    class TLRequestCall : TLObject
    {
        public const uint Signature = 0x5b95b3d4;

        public TLInputUserBase UserId { get; set; }

        public TLInt RandomId { get; set; }

        public TLString GAHash { get; set; }

        public TLPhoneCallProtocol Protocol { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                UserId.ToBytes(),
                RandomId.ToBytes(),
                GAHash.ToBytes(),
                Protocol.ToBytes());
        }
    }
}
