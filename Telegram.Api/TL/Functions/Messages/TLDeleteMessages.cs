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
    public enum DeleteMessagesFlags
    {
        Revoke = 0x1,           // 0
    }

    class TLDeleteMessages : TLObject
    {
        public const uint Signature = 0xe58e95d2;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLVector<TLInt> Id { get; set; }

        public bool Revoke
        {
            get { return IsSet(Flags, (int) DeleteMessagesFlags.Revoke); }
            set { SetUnset(ref _flags, value, (int) DeleteMessagesFlags.Revoke); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes());
        }
    }
}
