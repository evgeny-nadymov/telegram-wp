// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Messages
{
    [Flags]
    public enum GetArchivedStickersFlags
    {
        Masks = 0x1,
    }

    class TLGetArchivedStickers : TLObject
    {
        public const uint Signature = 0x57f17692;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLLong OffsetId { get; set; }

        public TLInt Limit { get; set; }

        public void SetMasks()
        {
            Set(ref _flags, (int) GetArchivedStickersFlags.Masks);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                OffsetId.ToBytes(),
                Limit.ToBytes());
        }
    }
}
