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
    public enum SearchStickerSetsFlags
    {
        ExcludeFeatured = 0x1,
    }

    class TLSearchStickerSets : TLObject
    {
        public const uint Signature = 0xc2b7d08b;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool ExcludeFeatured
        {
            get { return IsSet(Flags, (int)SearchStickerSetsFlags.ExcludeFeatured); }
            set { SetUnset(ref _flags, value, (int)SearchStickerSetsFlags.ExcludeFeatured); }
        }

        public TLString Q { get; set; }

        public TLInt Hash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Q.ToBytes(),
                Hash.ToBytes());
        }
    }
}
