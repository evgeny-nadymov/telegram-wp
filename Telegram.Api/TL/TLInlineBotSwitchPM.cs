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
    public class TLInlineBotSwitchPM : TLObject
    {
        public const uint Signature = TLConstructors.TLInlineBotSwitchPM;

        public TLString Text { get; set; }

        public TLString StartParam { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);
            StartParam = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                StartParam.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);
            StartParam = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Text.ToStream(output);
            StartParam.ToStream(output);
        }
    }
}
