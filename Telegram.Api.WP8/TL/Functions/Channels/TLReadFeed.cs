// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Channels
{
    class TLReadFeed : TLObject
    {
        public const uint Signature = 0x9c3011d;

        public TLInt FeedId { get; set; }

        public TLFeedPosition MaxPosition { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                FeedId.ToBytes(),
                MaxPosition.ToBytes());
        }
    }
}
