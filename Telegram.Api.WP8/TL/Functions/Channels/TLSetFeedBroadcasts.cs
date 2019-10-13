// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Channels
{
    class TLSetFeedBroadcasts : TLObject
    {
        public const uint Signature = 0xb5287d9a;

        public TLInt FeedId { get; set; }

        public TLVector<TLInputChannelBase> Channels { get; set; }

        public TLBool AlsoNewlyJoined { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                FeedId.ToBytes(),
                Channels.ToBytes(),
                AlsoNewlyJoined.ToBytes());
        }
    }
}
