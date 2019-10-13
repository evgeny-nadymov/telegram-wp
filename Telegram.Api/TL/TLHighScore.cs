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
    public class TLHighScore : TLObject
    {
        public const uint Signature = TLConstructors.TLHighScore;

        public TLInt Pos { get; set; }

        public TLInt UserId { get; set; }

        public TLInt Score { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Pos.ToBytes(),
                UserId.ToBytes(),
                Score.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Pos = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            Score = GetObject<TLInt>(input);

            return this;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Pos = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Score = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Pos.ToStream(output);
            UserId.ToStream(output);
            Score.ToStream(output);
        }
    }
}
