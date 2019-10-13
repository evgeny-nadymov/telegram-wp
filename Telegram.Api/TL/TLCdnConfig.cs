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
    public class TLCdnConfig : TLObject
    {
        public const uint Signature = TLConstructors.TLCdnConfig;

        public TLVector<TLCdnPublicKey> PublicKeys { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PublicKeys = GetObject<TLVector<TLCdnPublicKey>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PublicKeys = GetObject<TLVector<TLCdnPublicKey>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PublicKeys.ToStream(output);
        }
    }
}
