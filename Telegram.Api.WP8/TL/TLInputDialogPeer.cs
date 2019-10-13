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
    public abstract class TLInputDialogPeerBase : TLObject { }

    public class TLInputDialogPeerFeed : TLInputDialogPeerBase
    {
        public const uint Signature = TLConstructors.TLInputDialogPeerFeed;

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
                TLUtils.SignatureToBytes(TLInputPeerUser.Signature),
                FeedId.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            FeedId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            FeedId = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputDialogPeerFeed feed_id={0}", FeedId);
        }
    }

    public class TLInputDialogPeer : TLInputDialogPeerBase
    {
        public const uint Signature = TLConstructors.TLInputDialogPeer;

        public TLInputPeerBase Peer { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLInputPeerBase>(bytes, ref position);

            return this;
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
            return string.Format("TLInputDialogPeer peer={0}", Peer);
        }
    }
}
