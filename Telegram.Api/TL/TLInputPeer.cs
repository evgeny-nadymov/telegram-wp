using System.Linq;

namespace Telegram.Api.TL
{
    abstract class TLInputPeer : TLObject { }

    class TLInputPeerEmpty : TLInputPeer
    {
        public const uint Signature = TLConstructors.TLInputPeerEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLInputPeerEmpty--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override string ToString()
        {
            return "TLInputPeerEmpty";
        }
    }

    class TLInputPeerSelf : TLInputPeer
    {
        public const uint Signature = TLConstructors.TLInputPeerSelf;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLInputPeerSelf--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override string ToString()
        {
            return "TLInputPeerSelf";
        }
    }

    class TLInputPeerContact : TLInputPeer
    {
        public const uint Signature = TLConstructors.TLInputPeerContact;

        public TLInt UserId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLInputPeerContact--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature)
                .Concat(UserId.ToBytes())
                .ToArray();
        }

        public override string ToString()
        {
            return "UserId " + UserId;
        }
    }

    class TLInputPeerForeign : TLInputPeer
    {
        public const uint Signature = TLConstructors.TLInputPeerForeign;

        public TLInt UserId { get; set; }

        public TLLong AccessHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLInputPeerForeign--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature)
                .Concat(UserId.ToBytes())
                .Concat(AccessHash.ToBytes())
                .ToArray();
        }

        public override string ToString()
        {
            return "UserId " + UserId + " AccessHash " + AccessHash;
        }
    }

    class TLInputPeerChat : TLInputPeer
    {
        public const uint Signature = TLConstructors.TLInputPeerChat;

        public TLInt ChatId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLInputPeerChat--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChatId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature)
                .Concat(ChatId.ToBytes())
                .ToArray();
        }

        public override string ToString()
        {
            return "ChatId " + ChatId;
        }
    }
}
