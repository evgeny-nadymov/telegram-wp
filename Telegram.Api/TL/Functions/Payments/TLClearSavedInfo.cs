// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Payments
{
    [Flags]
    public enum ClearSavedInfoFlags
    {
        Credentials = 0x1,
        Info = 0x2
    }

    class TLClearSavedInfo : TLObject
    {
        public const uint Signature = 0xd83d70c1;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Credentials
        {
            get { return IsSet(Flags, (int) ClearSavedInfoFlags.Credentials); }
            set { SetUnset(ref _flags, value, (int) ClearSavedInfoFlags.Credentials); }
        }

        public bool Info
        {
            get { return IsSet(Flags, (int) ClearSavedInfoFlags.Info); }
            set { SetUnset(ref _flags, value, (int) ClearSavedInfoFlags.Info); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes());
        }
    }
}
