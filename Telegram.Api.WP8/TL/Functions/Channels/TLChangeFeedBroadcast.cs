// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Channels
{
    [Flags]
    public enum ChangeFeedBroadcastFlags
    {
        FeedId = 0x1
    }

    class TLChangeFeedBroadcast : TLObject
    {
        public const uint Signature = 0xffb37511;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputChannelBase Channel { get; set; }

        private TLInt _feedId;

        public TLInt FeedId
        {
            get { return _feedId; }
            set { SetField(out _feedId, value, ref _flags, (int)ChangeFeedBroadcastFlags.FeedId); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Channel.ToBytes(),
                ToBytes(FeedId, Flags, (int)ChangeFeedBroadcastFlags.FeedId));
        }
    }
}
