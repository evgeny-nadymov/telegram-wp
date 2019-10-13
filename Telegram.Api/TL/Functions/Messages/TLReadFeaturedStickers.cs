// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL.Functions.Messages
{
    class TLReadFeaturedStickers : TLObject
    {
        public const uint Signature = 0x5b118126;

        public TLVector<TLLong> Id { get; set; } 

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(TLUtils.SignatureToBytes(Signature), 
                Id.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLVector<TLLong>>(input);

            return this;
        }
    }
}
