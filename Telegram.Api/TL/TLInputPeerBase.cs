// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using System.Linq;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLInputPeerBase : TLObject { }

    public class TLInputPeerEmpty : TLInputPeerBase
    {
        public const uint Signature = TLConstructors.TLInputPeerEmpty;

        #region Additional
        public TLInt UserId { get; set; }

        public TLInt SelfId { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override string ToString()
        {
            return "TLInputPeerEmpty";
        }
    }

    public class TLInputPeerSelf : TLInputPeerBase
    {
        public const uint Signature = TLConstructors.TLInputPeerSelf;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override string ToString()
        {
            return "TLInputPeerSelf";
        }
    }

    public class TLInputPeerContact : TLInputPeerBase
    {
        public const uint Signature = TLConstructors.TLInputPeerContact;

        public TLInt UserId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(TLInputPeerUser.Signature),
                UserId.ToBytes(),
                new TLLong(0).ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            UserId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputPeerContact user_id={0}", UserId);
        }
    }

    public class TLInputPeerForeign : TLInputPeerBase
    {
        public const uint Signature = TLConstructors.TLInputPeerForeign;

        public TLInt UserId { get; set; }

        public TLLong AccessHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(TLInputPeerUser.Signature),
                UserId.ToBytes(),
                AccessHash.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            UserId.ToStream(output);
            AccessHash.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputPeerForeign user_id={0} access_hash={1}", UserId, AccessHash);
        }
    }

    public class TLInputPeerChat : TLInputPeerBase
    {
        public const uint Signature = TLConstructors.TLInputPeerChat;

        public TLInt ChatId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                ChatId.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            ChatId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputPeerChat chat_id={0}", ChatId);
        }
    }

    public class TLInputPeerBroadcast : TLInputPeerBase
    {
        public const uint Signature = TLConstructors.TLInputPeerBroadcast;

        public TLInt ChatId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                ChatId.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            ChatId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputPeerBroadcast chat_id={0}", ChatId);
        }
    }

    public class TLInputPeerUser : TLInputPeerBase
    {
        public const uint Signature = TLConstructors.TLInputPeerUser;

        public TLInt UserId { get; set; }

        public TLLong AccessHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                UserId.ToBytes(),
                AccessHash.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            UserId.ToStream(output);
            AccessHash.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputPeerUser user_id={0} access_hash={1}", UserId, AccessHash);
        }
    }

    public class TLInputPeerChannel : TLInputPeerBroadcast
    {
        public new const uint Signature = TLConstructors.TLInputPeerChannel;

        public TLLong AccessHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                ChatId.ToBytes(),
                AccessHash.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            ChatId.ToStream(output);
            AccessHash.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputPeerChannel channel_id={0} access_hash={1}", ChatId, AccessHash);
        }
    }

    public class TLInputPeerFeed : TLInputPeerBroadcast
    {
        public new const uint Signature = TLConstructors.TLInputPeerFeed;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                ChatId.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            ChatId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            ChatId = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputPeerFeed feed_id={0}", ChatId);
        }
    }
}
