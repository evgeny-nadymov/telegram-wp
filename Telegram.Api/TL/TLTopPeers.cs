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
    public abstract class TLTopPeersBase : TLObject { }

    public class TLTopPeers : TLTopPeersBase
    {
        public const uint Signature = TLConstructors.TLTopPeers;

        public TLVector<TLTopPeerCategoryPeers> Categories { get; set; }

        public TLVector<TLChatBase> Chats { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Categories = GetObject<TLVector<TLTopPeerCategoryPeers>>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Categories.ToBytes(),
                Chats.ToBytes(),
                Users.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Categories = GetObject<TLVector<TLTopPeerCategoryPeers>>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Categories.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);
        }
    }

    public class TLTopPeersNotModified : TLTopPeersBase
    {
        public const uint Signature = TLConstructors.TLTopPeersNotModified;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLTopPeersDisabled : TLTopPeersBase
    {
        public const uint Signature = TLConstructors.TLTopPeersDisabled;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }
}
