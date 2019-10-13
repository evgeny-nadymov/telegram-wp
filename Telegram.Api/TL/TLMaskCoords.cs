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
    public class TLMaskCoords : TLObject
    {
        public const uint Signature = TLConstructors.TLMaskCoords;

        public TLInt N { get; set; }

        public TLDouble X { get; set; }

        public TLDouble Y { get; set; }

        public TLDouble Zoom { get; set; }

        public TLMaskCoords()
        {

        }

        public TLMaskCoords(int n, double x, double y, double zoom)
        {
            N = new TLInt(n);
            X = new TLDouble(x);
            Y = new TLDouble(y);
            Zoom = new TLDouble(zoom);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                N.ToBytes(),
                X.ToBytes(),
                Y.ToBytes(),
                Zoom.ToBytes()
            );
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            N = GetObject<TLInt>(bytes, ref position);
            X = GetObject<TLDouble>(bytes, ref position);
            Y = GetObject<TLDouble>(bytes, ref position);
            Zoom = GetObject<TLDouble>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            N.ToStream(output);
            X.ToStream(output);
            Y.ToStream(output);
            Zoom.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            N = GetObject<TLInt>(input);
            X = GetObject<TLDouble>(input);
            Y = GetObject<TLDouble>(input);
            Zoom = GetObject<TLDouble>(input);

            return this;
        }
    }
}
