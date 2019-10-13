// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL
{
    [Flags]
    public enum SavedInfoFlags
    {
        SavedInfo = 0x1,                    // 0
        HasSavedCredentials = 0x2,          // 1
    }

    public class TLSavedInfo : TLObject
    {
        public const uint Signature = TLConstructors.TLSavedInfo;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool HasSavedCredentials
        {
            get { return IsSet(Flags, (int) SavedInfoFlags.HasSavedCredentials); }
            set { SetUnset(ref _flags, value, (int) SavedInfoFlags.HasSavedCredentials); }
        }

        private TLPaymentRequestedInfo _savedInfo;

        public TLPaymentRequestedInfo SavedInfo
        {
            get { return _savedInfo; }
            set { SetField(out _savedInfo, value, ref _flags, (int) SavedInfoFlags.SavedInfo); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _savedInfo = GetObject<TLPaymentRequestedInfo>(Flags, (int) SavedInfoFlags.SavedInfo, null, bytes, ref position);

            return this;
        }
    }
}