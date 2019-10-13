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
    public class TLReadChannelHistory : TLObject, IRandomId
    {
        public const uint Signature = 0xcc104937;

        public TLInputChannelBase Channel { get; set; }

        public TLInt MaxId { get; set; }
        
        public TLLong RandomId { get; set; }

        public TLReadChannelHistory()
        {
            RandomId = TLLong.Random();
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Channel.ToBytes(),
                MaxId.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Channel.ToStream(output);
            MaxId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Channel = GetObject<TLInputChannelBase>(input);
            MaxId = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLReadChannelHistory max_id={0} peer=[{1}]", MaxId, Channel);
        }
    }
}
