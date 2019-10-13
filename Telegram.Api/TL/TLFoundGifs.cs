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
    public class TLFoundGifs : TLObject
    {
        public const uint Signature = TLConstructors.TLFoundGifs;

        public TLInt NextOffset { get; set; }

        public TLVector<TLFoundGifBase> Results { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NextOffset = GetObject<TLInt>(bytes, ref position);
            Results = GetObject<TLVector<TLFoundGifBase>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            NextOffset = GetObject<TLInt>(input);
            Results = GetObject<TLVector<TLFoundGifBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            NextOffset.ToStream(output);
            Results.ToStream(output);
        }
    }
}
