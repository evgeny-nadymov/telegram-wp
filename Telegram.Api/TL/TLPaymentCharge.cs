// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;

namespace Telegram.Api.TL
{
    public class TLPaymentCharge : TLObject
    {
        public const uint Signature = TLConstructors.TLPaymentCharge;

        public TLString Id { get; set; }

        public TLString ProviderChargeId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLString>(bytes, ref position);
            ProviderChargeId = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLString>(input);
            ProviderChargeId = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            Id.ToStream(output);
            ProviderChargeId.ToStream(output);
        }
    }
}
