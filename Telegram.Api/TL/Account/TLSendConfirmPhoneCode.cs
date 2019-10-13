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
    public enum SendConfirmPhoneCodeFlags
    {
        AllowFlashcall = 0x1,      // 0
    }

    class TLSendConfirmPhoneCode : TLObject
    {
        public const uint Signature = 0x1516d7bd;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString Hash { get; set; }

        private TLBool _currentNumber;

        public TLBool CurrentNumber
        {
            get { return _currentNumber; }
            set { SetField(out _currentNumber, value, ref _flags, (int)SendConfirmPhoneCodeFlags.AllowFlashcall); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Hash.ToBytes(),
                ToBytes(CurrentNumber, Flags, (int)SendConfirmPhoneCodeFlags.AllowFlashcall));
        }
    }
}
