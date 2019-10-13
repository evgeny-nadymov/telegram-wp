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
    public enum ValidateRequestedInfoFlags
    {
        Save = 0x1
    }

    class TLValidateRequestedInfo : TLObject
    {
        public const uint Signature = 0x770a8e74;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Save
        {
            get { return IsSet(Flags, (int) ValidateRequestedInfoFlags.Save); }
            set { SetUnset(ref _flags, value, (int) ValidateRequestedInfoFlags.Save); }
        }

        public TLInt MsgId { get; set; }

        public TLPaymentRequestedInfo Info { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            MsgId = GetObject<TLInt>(bytes, ref position);
            Info = GetObject<TLPaymentRequestedInfo>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                MsgId.ToBytes(),
                Info.ToBytes());
        }
    }
}
