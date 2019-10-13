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
    public class TLKeyboardButtonRow : TLObject
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonRow;

        public TLVector<TLKeyboardButtonBase> Buttons { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Buttons = GetObject<TLVector<TLKeyboardButtonBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Buttons.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Buttons = GetObject<TLVector<TLKeyboardButtonBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Buttons.ToBytes());
        }
    }
}
