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
    class TLReadMessageContents : TLObject, IRandomId
    {
        public const uint Signature = 0x36a73f77;

        public TLVector<TLInt> Id { get; set; }

        public TLLong RandomId { get; set; }

        public TLReadMessageContents()
        {
            RandomId = TLLong.Random();
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLVector<TLInt>>(input);

            return this;
        }
    }
}
