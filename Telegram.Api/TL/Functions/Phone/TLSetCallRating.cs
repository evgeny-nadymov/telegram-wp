// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Phone
{
    class TLSetCallRating : TLObject
    {
        public const uint Signature = 0x1c536a34;

        public TLInputPhoneCall Peer { get; set; }

        public TLInt Rating { get; set; }

        public TLString Comment { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes(),
                Rating.ToBytes(),
                Comment.ToBytes());
        }
    }
}
