// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.IO;
using Windows.Data.Json;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLPassportConfigBase : TLObject { }

    public class TLPassportConfigNotModified : TLPassportConfigBase
    {
        public const uint Signature = TLConstructors.TLPassportConfigNotModified;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLPassportConfig : TLPassportConfigBase
    {
        public const uint Signature = TLConstructors.TLPassportConfig;

        public TLInt Hash { get; set; }

        public TLDataJSON CountriesLangs { get; set; }

        public JsonObject CountriesLangsObject { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLInt>(bytes, ref position);
            CountriesLangs = GetObject<TLDataJSON>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLInt>(input);
            CountriesLangs = GetObject<TLDataJSON>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Hash.ToStream(output);
            CountriesLangs.ToStream(output);
        }
    }
}
