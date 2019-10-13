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
    public class TLResultInfo : TLObject
    {
        public const uint Signature = TLConstructors.TLResultInfo;

        public TLString Type { get; set; }

        public TLInt Id { get; set; }

        public TLLong Count { get; set; }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Type.ToStream(output);
            Id.ToStream(output);
            Count.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Type = GetObject<TLString>(input);
            Id = GetObject<TLInt>(input);
            Count = GetObject<TLLong>(input);

            return this;
        }
    }
}
