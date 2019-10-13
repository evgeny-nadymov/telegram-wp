// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLFeedBroadcastsBase : TLObject
    {
        public TLVector<TLInt> Channels { get; set; }
    }

    public class TLFeedBroadcastsUngrouped : TLFeedBroadcastsBase
    {
        public const uint Signature = TLConstructors.TLFeedBroadcastsUngrouped;
        
        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Channels = GetObject<TLVector<TLInt>>(bytes, ref position);

            return this;
        }
    }

    public class TLFeedBroadcasts : TLFeedBroadcastsBase
    {
        public const uint Signature = TLConstructors.TLFeedBroadcasts;

        public TLInt FeedId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            FeedId = GetObject<TLInt>(bytes, ref position);
            Channels = GetObject<TLVector<TLInt>>(bytes, ref position);

            return this;
        }
    }
}
