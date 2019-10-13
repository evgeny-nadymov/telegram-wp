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
    class TLReadMentions : TLObject, IRandomId
    {
        public const uint Signature = 0xf0189d3;

        public TLInputPeerBase Peer { get; set; }

        public TLLong RandomId { get; set; }

        public TLReadMentions()
        {
            RandomId = TLLong.Random();
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Peer.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLInputPeerBase>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLReadMentions peer=[{0}]", Peer);
        }
    }
}
