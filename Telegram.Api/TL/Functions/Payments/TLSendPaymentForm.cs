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
    public enum SendPaymentFormFlags
    {
        RequestedInfoId = 0x1,
        ShippingOptionId = 0x2
    }

    class TLSendPaymentForm : TLObject
    {
        public const uint Signature = 0x2b8879b3;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt MsgId { get; set; }

        private TLString _requestedInfoId;

        public TLString RequestedInfoId
        {
            get { return _requestedInfoId; }
            set { SetField(out _requestedInfoId, value, ref _flags, (int) SendPaymentFormFlags.RequestedInfoId); }
        }

        private TLString _shippingOptionId;

        public TLString ShippingOptionId
        {
            get { return _shippingOptionId; }
            set { SetField(out _shippingOptionId, value, ref _flags, (int)SendPaymentFormFlags.ShippingOptionId); }
        }

        public TLInputPaymentCredentialsBase Credentials { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                MsgId.ToBytes(),
                ToBytes(RequestedInfoId, Flags, (int) SendPaymentFormFlags.RequestedInfoId),
                ToBytes(ShippingOptionId, Flags, (int) SendPaymentFormFlags.ShippingOptionId),
                Credentials.ToBytes());
        }
    }
}
