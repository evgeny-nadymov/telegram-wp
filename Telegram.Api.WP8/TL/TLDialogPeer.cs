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
    public abstract class TLDialogPeerBase : TLObject { }

    public class TLDialogPeerFeed : TLDialogPeerBase
    {
        public const uint Signature = TLConstructors.TLDialogPeerFeed;

        public TLInt FeedId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            FeedId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                FeedId.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            FeedId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(FeedId.ToBytes());
        }

        public override string ToString()
        {
            return "TLDialogPeer feed_id=" + FeedId;
        }
    }

    public class TLDialogPeer : TLDialogPeerBase
    {
        public const uint Signature = TLConstructors.TLDialogPeer;

        public TLPeerBase Peer { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Peer.ToBytes());
        }

        public override string ToString()
        {
            return "TLDialogPeer peer=" + Peer;
        }
    }
}
