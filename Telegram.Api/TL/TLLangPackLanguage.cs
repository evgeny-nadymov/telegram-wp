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
    public class TLLangPackLanguage : TLObject
    {
        public const uint Signature = TLConstructors.TLLangPackLanguage;

        public TLString Name { get; set; }

        public TLString NativeName { get; set; }

        public TLString LangCode { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Name = GetObject<TLString>(bytes, ref position);
            NativeName = GetObject<TLString>(bytes, ref position);
            LangCode = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Name.ToStream(output);
            NativeName.ToStream(output);
            LangCode.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Name = GetObject<TLString>(input);
            NativeName = GetObject<TLString>(input);
            LangCode = GetObject<TLString>(input);

            return this;
        }
    }
}
