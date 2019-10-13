// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.IO;

namespace Telegram.Api.TL
{
    public class TLPostAddress : TLObject
    {
        public const uint Signature = TLConstructors.TLPostAddress;

        public TLString StreetLine1 { get; set; }

        public TLString StreetLine2 { get; set; }

        public TLString City { get; set; }

        public TLString State { get; set; }

        public TLString CountryIso2 { get; set; }

        public TLString PostCode { get; set; }

        public override string ToString()
        {
            var list = new List<TLString>();
            if (!TLString.IsNullOrEmpty(StreetLine1)) list.Add(StreetLine1);
            if (!TLString.IsNullOrEmpty(StreetLine2)) list.Add(StreetLine2);
            if (!TLString.IsNullOrEmpty(City)) list.Add(City);
            if (!TLString.IsNullOrEmpty(State)) list.Add(State);
            if (!TLString.IsNullOrEmpty(CountryIso2)) list.Add(CountryIso2);
            if (!TLString.IsNullOrEmpty(PostCode)) list.Add(PostCode);

            return string.Join(", ", list);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            StreetLine1 = GetObject<TLString>(bytes, ref position);
            StreetLine2 = GetObject<TLString>(bytes, ref position);
            City = GetObject<TLString>(bytes, ref position);
            State = GetObject<TLString>(bytes, ref position);
            CountryIso2 = GetObject<TLString>(bytes, ref position);
            PostCode = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                StreetLine1.ToBytes(),
                StreetLine2.ToBytes(),
                City.ToBytes(),
                State.ToBytes(),
                CountryIso2.ToBytes(),
                PostCode.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            StreetLine1 = GetObject<TLString>(input);
            StreetLine2 = GetObject<TLString>(input);
            City = GetObject<TLString>(input);
            State = GetObject<TLString>(input);
            CountryIso2 = GetObject<TLString>(input);
            PostCode = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            StreetLine1.ToStream(output);
            StreetLine2.ToStream(output);
            City.ToStream(output);
            State.ToStream(output);
            CountryIso2.ToStream(output);
            PostCode.ToStream(output);
        }
    }
}
