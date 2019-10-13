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
    public enum GetFeedFlags
    {
        OffsetPosition = 0x1,
        MaxPosition = 0x2,
        MinPosition = 0x4,
        OffsetToMaxRead = 0x8,
    }

    class TLGetFeed : TLObject
    {
        public const uint Signature = 0xb90f450;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool OffsetToMaxRead
        {
            get { return IsSet(Flags, (int) GetFeedFlags.OffsetToMaxRead); }
            set { SetUnset(ref _flags, value, (int) GetFeedFlags.OffsetToMaxRead); }
        }

        public TLInt FeedId { get; set; }

        private TLFeedPosition _offsetPosition;

        public TLFeedPosition OffsetPosition
        {
            get { return _offsetPosition; }
            set { SetField(out _offsetPosition, value, ref _flags, (int)GetFeedFlags.OffsetPosition); }
        }

        public TLInt AddOffset { get; set; }

        public TLInt Limit { get; set; }

        private TLFeedPosition _maxPosition;

        public TLFeedPosition MaxPosition
        {
            get { return _maxPosition; }
            set { SetField(out _maxPosition, value, ref _flags, (int)GetFeedFlags.MaxPosition); }
        }

        private TLFeedPosition _minPosition;

        public TLFeedPosition MinPosition
        {
            get { return _minPosition; }
            set { SetField(out _minPosition, value, ref _flags, (int)GetFeedFlags.MinPosition); }
        }

        public TLInt Hash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                FeedId.ToBytes(),
                ToBytes(OffsetPosition, Flags, (int)GetFeedFlags.OffsetPosition),
                AddOffset.ToBytes(),
                Limit.ToBytes(),
                ToBytes(MaxPosition, Flags, (int)GetFeedFlags.MaxPosition),
                ToBytes(MinPosition, Flags, (int)GetFeedFlags.MinPosition),
                Hash.ToBytes());
        }
    }
}
