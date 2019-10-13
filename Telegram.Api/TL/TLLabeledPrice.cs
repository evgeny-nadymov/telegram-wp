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
    public class TLLabeledPrice : TLObject
    {
        public const uint Signature = TLConstructors.TLLabeledPrice;

        public TLString Label { get; set; }

        public TLLong Amount { get; set; }

        #region Additional
        public TLString Currency { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Label = GetObject<TLString>(bytes, ref position);
            Amount = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Label.ToBytes(),
                Amount.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Label = GetObject<TLString>(input);
            Amount = GetObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Label.ToStream(output);
            Amount.ToStream(output);
        }
    }
}
