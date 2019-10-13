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
    public enum PaymentReceiptFlags
    {
        Info = 0x1,         // 0
        Shipping = 0x2,     // 1
    }

    public class TLPaymentReceipt : TLObject
    {
        public const uint Signature = TLConstructors.TLPaymentReceipt;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt Date { get; set; }

        public TLInt BotId { get; set; }

        public TLInvoice Invoice { get; set; }

        public TLInt ProviderId { get; set; }

        private TLPaymentRequestedInfo _info;

        public TLPaymentRequestedInfo SavedInfo
        {
            get { return _info; }
            set { SetField(out _info, value, ref _flags, (int) PaymentReceiptFlags.Info); }
        }

        private TLShippingOption _shipping;

        public TLShippingOption Shipping
        {
            get { return _shipping; }
            set { SetField(out _shipping, value, ref _flags, (int) PaymentReceiptFlags.Shipping); }
        }

        public TLString Currency { get; set; }

        public TLLong TotalAmount { get; set; }

        public TLString CredentialsTitle { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            BotId = GetObject<TLInt>(bytes, ref position);
            Invoice = GetObject<TLInvoice>(bytes, ref position);
            ProviderId = GetObject<TLInt>(bytes, ref position);
            _info = GetObject<TLPaymentRequestedInfo>(Flags, (int) PaymentReceiptFlags.Info, null, bytes, ref position);
            _shipping = GetObject<TLShippingOption>(Flags, (int) PaymentReceiptFlags.Shipping, null, bytes, ref position);
            Currency = GetObject<TLString>(bytes, ref position);
            TotalAmount = GetObject<TLLong>(bytes, ref position);
            CredentialsTitle = GetObject<TLString>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }
    }
}