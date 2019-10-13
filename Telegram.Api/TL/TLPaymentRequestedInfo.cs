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
    public enum PaymentRequestedInfoFlags
    {
        Name = 0x1,             // 0
        Phone = 0x2,            // 1
        Email = 0x4,            // 2
        ShippingAddress = 0x8,  // 3
    }

    public class TLPaymentRequestedInfo : TLObject
    {
        public const uint Signature = TLConstructors.TLPaymentRequestedInfo;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLString _name;

        public TLString Name
        {
            get { return _name; }
            set { SetField(out _name, value, ref _flags, (int)PaymentRequestedInfoFlags.Name); }
        }

        protected TLString _phone;

        public TLString Phone
        {
            get { return _phone; }
            set { SetField(out _phone, value, ref _flags, (int)PaymentRequestedInfoFlags.Phone); }
        }

        protected TLString _email;

        public TLString Email
        {
            get { return _email; }
            set { SetField(out _email, value, ref _flags, (int)PaymentRequestedInfoFlags.Email); }
        }

        protected TLPostAddress _shippingAddress;

        public TLPostAddress ShippingAddress
        {
            get { return _shippingAddress; }
            set { SetField(out _shippingAddress, value, ref _flags, (int)PaymentRequestedInfoFlags.ShippingAddress); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _name = GetObject<TLString>(Flags, (int)PaymentRequestedInfoFlags.Name, null, bytes, ref position);
            _phone = GetObject<TLString>(Flags, (int)PaymentRequestedInfoFlags.Phone, null, bytes, ref position);
            _email = GetObject<TLString>(Flags, (int)PaymentRequestedInfoFlags.Email, null, bytes, ref position);
            _shippingAddress = GetObject<TLPostAddress>(Flags, (int)PaymentRequestedInfoFlags.ShippingAddress, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(Name, Flags, (int)PaymentRequestedInfoFlags.Name),
                ToBytes(Phone, Flags, (int)PaymentRequestedInfoFlags.Phone),
                ToBytes(Email, Flags, (int)PaymentRequestedInfoFlags.Email),
                ToBytes(ShippingAddress, Flags, (int)PaymentRequestedInfoFlags.ShippingAddress));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _name = GetObject<TLString>(Flags, (int)PaymentRequestedInfoFlags.Name, null, input);
            _phone = GetObject<TLString>(Flags, (int)PaymentRequestedInfoFlags.Phone, null, input);
            _email = GetObject<TLString>(Flags, (int)PaymentRequestedInfoFlags.Email, null, input);
            _shippingAddress = GetObject<TLPostAddress>(Flags, (int)PaymentRequestedInfoFlags.ShippingAddress, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, _name, Flags, (int)PaymentRequestedInfoFlags.Name);
            ToStream(output, _phone, Flags, (int)PaymentRequestedInfoFlags.Phone);
            ToStream(output, _email, Flags, (int)PaymentRequestedInfoFlags.Email);
            ToStream(output, _shippingAddress, Flags, (int)PaymentRequestedInfoFlags.ShippingAddress);
        }
    }
}
