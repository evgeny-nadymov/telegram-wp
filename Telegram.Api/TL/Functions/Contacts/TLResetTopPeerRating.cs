// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Contacts
{
    class TLResetTopPeerRating : TLObject
    {
        public const uint Signature = 0x1ae373ac;

        public TLTopPeerCategoryBase Category { get; set; }

        public TLInputPeerBase Peer { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Category.ToBytes(),
                Peer.ToBytes());
        }
    }
}
