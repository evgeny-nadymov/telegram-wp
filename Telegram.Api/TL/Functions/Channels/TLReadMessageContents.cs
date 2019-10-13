// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;
using Telegram.Api.TL.Functions.Messages;

namespace Telegram.Api.TL.Functions.Channels
{
    class TLReadMessageContents : TLObject, IRandomId
    {
        public const uint Signature = 0xeab5dc38;

        public TLInputChannelBase Channel { get; set; }

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
                Channel.ToBytes(),
                Id.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Channel.ToStream(output);
            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Channel = GetObject<TLInputChannelBase>(input);
            Id = GetObject<TLVector<TLInt>>(input);

            return this;
        }
    }
}
