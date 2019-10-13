// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Account
{
    [Flags]
    public enum SendChangePhoneCode
    {
        AllowFlashcall = 0x1,      // 0
    }

    class TLSendChangePhoneCode : TLObject
    {
        public const uint Signature = 0x8e57deb;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString PhoneNumber { get; set; }

        private TLString _currentNumber;

        public TLString CurrentNumber
        {
            get { return _currentNumber; }
            set { SetField(out _currentNumber, value, ref _flags, (int)SendChangePhoneCode.AllowFlashcall); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                PhoneNumber.ToBytes(),
                ToBytes(CurrentNumber, Flags, (int)SendChangePhoneCode.AllowFlashcall));
        }
    }
}
