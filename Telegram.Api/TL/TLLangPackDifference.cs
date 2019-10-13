// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public class TLLangPackDifference : TLObject
    {
        public const uint Signature = TLConstructors.TLLangPackDifference;

        public TLString LangCode { get; set; }

        public TLInt FromVersion { get; set; }

        public TLInt Version { get; set; }

        public TLVector<TLLangPackStringBase> Strings { get; set; } 

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            LangCode = GetObject<TLString>(bytes, ref position);
            FromVersion = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            Strings = GetObject<TLVector<TLLangPackStringBase>>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            LangCode.ToStream(output);
            FromVersion.ToStream(output);
            Version.ToStream(output);
            Strings.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            LangCode = GetObject<TLString>(input);
            FromVersion = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);
            Strings = GetObject<TLVector<TLLangPackStringBase>>(input);

            return this;
        }
    }
}
