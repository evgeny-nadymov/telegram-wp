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
    public enum ReorderStickerSetsFlags
    {
        Masks = 0x1,
    }

    class TLReorderStickerSets : TLObject
    {
        public const uint Signature = 0x78337739;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLVector<TLLong> Order { get; set; }

        public void SetMasks()
        {
            Set(ref _flags, (int) ReorderStickerSetsFlags.Masks);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Order.ToBytes());
        }
    }
}
