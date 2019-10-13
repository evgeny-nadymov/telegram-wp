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
    public class TLTopPeerCategoryPeers : TLObject
    {
        public const uint Signature = TLConstructors.TLTopPeerCategoryPeers;

        public TLTopPeerCategoryBase Category { get; set; }

        public TLInt Count { get; set; }

        public TLVector<TLTopPeer> Peers { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Category = GetObject<TLTopPeerCategoryBase>(bytes, ref position);
            Count = GetObject<TLInt>(bytes, ref position);
            Peers = GetObject<TLVector<TLTopPeer>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Category.ToBytes(),
                Count.ToBytes(),
                Peers.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Category = GetObject<TLTopPeerCategoryBase>(input);
            Count = GetObject<TLInt>(input);
            Peers = GetObject<TLVector<TLTopPeer>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Category.ToStream(output);
            Count.ToStream(output);
            Peers.ToStream(output);
        }
    }
}
