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
    public class TLTmpPassword : TLObject
    {
        public const uint Signature = TLConstructors.TLTmpPassword;

        public TLString TmpPassword { get; set; }

        public TLInt ValidUntil { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            TmpPassword = GetObject<TLString>(bytes, ref position);
            ValidUntil = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            TmpPassword = GetObject<TLString>(input);
            ValidUntil = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            TmpPassword.ToStream(output);
            ValidUntil.ToStream(output);
        }
    }
}