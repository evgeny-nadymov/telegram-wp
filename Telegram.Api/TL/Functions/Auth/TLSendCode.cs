// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Auth
{
    [Flags]
    public enum SendCode
    {
        AllowFlashcall = 0x1,      // 0
    }

    class TLSendCode : TLObject
    {
        public const uint Signature = 0x86aef0ec;

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
            set { SetField(out _currentNumber, value, ref _flags, (int)SendCode.AllowFlashcall); }
        }

        public TLInt ApiId { get; set; }

        public TLString ApiHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                PhoneNumber.ToBytes(),
                ToBytes(CurrentNumber, Flags, (int)SendCode.AllowFlashcall),
                ApiId.ToBytes(),
                ApiHash.ToBytes());
        }
    }
}
