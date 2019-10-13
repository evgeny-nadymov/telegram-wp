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
    public enum ValidatedRequestedInfoFlags
    {
        Id = 0x1,                   // 0
        ShippingOptions = 0x2,      // 1
    }

    public class TLValidatedRequestedInfo : TLObject
    {
        public const uint Signature = TLConstructors.TLValidatedRequestedInfo;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLString _id;

        public TLString Id
        {
            get { return _id; }
            set { SetField(out _id, value, ref _flags, (int) ValidatedRequestedInfoFlags.Id); }
        }

        private TLVector<TLShippingOption> _shippingOptions;

        public TLVector<TLShippingOption> ShippingOptions
        {
            get { return _shippingOptions; }
            set { SetField(out _shippingOptions, value, ref _flags, (int) ValidatedRequestedInfoFlags.ShippingOptions); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _id = GetObject<TLString>(Flags, (int) ValidatedRequestedInfoFlags.Id, null, bytes, ref position);
            _shippingOptions = GetObject<TLVector<TLShippingOption>>(Flags, (int)ValidatedRequestedInfoFlags.ShippingOptions, null, bytes, ref position);

            return this;
        }
    }
}
