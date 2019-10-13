// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum InvoiceFlags
    {
        Test = 0x1,                         // 0
        NameRequested = 0x2,                // 1
        PhoneRequested = 0x4,               // 2
        EmailRequested = 0x8,               // 3
        ShippingAddressRequested = 0x10,    // 4
        Flexible = 0x20,                    // 5
    }

    public class TLInvoice : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInvoice;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString Currency { get; set; }

        public TLVector<TLLabeledPrice> Prices { get; set; }

        public bool Test { get { return IsSet(Flags, (int) InvoiceFlags.Test); } }

        public bool NameRequested { get { return IsSet(Flags, (int) InvoiceFlags.NameRequested); } }

        public bool PhoneRequested { get { return IsSet(Flags, (int) InvoiceFlags.PhoneRequested); } }

        public bool EmailRequested { get { return IsSet(Flags, (int) InvoiceFlags.EmailRequested); } }

        public bool ShippingAddressRequested { get { return IsSet(Flags, (int) InvoiceFlags.ShippingAddressRequested); } }

        public bool Flexible { get { return IsSet(Flags, (int) InvoiceFlags.Flexible); } }

        #region Additional

        public bool ReceiverRequested { get { return NameRequested || PhoneRequested || EmailRequested; } }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Currency = GetObject<TLString>(bytes, ref position);
            Prices = GetObject<TLVector<TLLabeledPrice>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Currency.ToBytes(),
                Prices.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Currency = GetObject<TLString>(input);
            Prices = GetObject<TLVector<TLLabeledPrice>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Currency.ToStream(output);
            Prices.ToStream(output);
        }
    }
}
