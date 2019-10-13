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
    public enum GetRecentStickersFlags
    {
        Attached = 0x1
    }

    class TLGetRecentStickers : TLObject
    {
        public const uint Signature = 0x5ea192c9;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt Hash { get; set; }

        public void SetAttached()
        {
            Set(ref _flags, (int) GetRecentStickersFlags.Attached);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Hash.ToBytes());
        }
    }
}
