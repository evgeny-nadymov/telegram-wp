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
    public class TLFeedPosition : TLObject
    {
        public const uint Signature = TLConstructors.TLFeedPosition;
        
        public TLInt Date { get; set; }

        public TLPeerBase Peer { get; set; }

        public TLInt Id { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Date.ToBytes(),
                Peer.ToBytes(),
                Id.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Date = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);
            Id = GetObject<TLInt>(input);
            
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Date.ToStream(output);
            Peer.ToStream(output);
            Id.ToStream(output);
        }
    }
}
