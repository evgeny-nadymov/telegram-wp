// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum ProxyDataPromoCustomFlags
    {
        //Channel = 0x1,
        Notified = 0x2,
    }

    public abstract class TLProxyDataBase : TLObject
    {
        public TLInt Expires { get; set; }

        public abstract TLProxyDataBase GetEmptyObject();
    }

    public class TLProxyDataEmpty : TLProxyDataBase
    {
        public const uint Signature = TLConstructors.TLProxyDataEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Expires = GetObject<TLInt>(bytes, ref position);

            return this;
        }
        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Expires.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Expires.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Expires = GetObject<TLInt>(input);

            return this;
        }

        public override TLProxyDataBase GetEmptyObject()
        {
            return new TLProxyDataEmpty { Expires = Expires };
        }
    }

    public class TLProxyDataPromo : TLProxyDataBase
    {
        public const uint Signature = TLConstructors.TLProxyDataPromo;

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public bool Notified
        {
            get { return IsSet(_customFlags, (int) ProxyDataPromoCustomFlags.Notified); }
            set { SetUnset(ref _customFlags, value, (int) ProxyDataPromoCustomFlags.Notified); }
        }

        public TLPeerBase Peer { get; set; }

        public TLVector<TLChatBase> Chats { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Expires = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);
            Chats = GetObject<TLVector<TLChatBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);
            CustomFlags = new TLLong(0);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Expires.ToBytes(),
                Peer.ToBytes(),
                Chats.ToBytes(),
                Users.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Expires.ToStream(output);
            Peer.ToStream(output);
            Chats.ToStream(output);
            Users.ToStream(output);

            CustomFlags.NullableToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Expires = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);
            Chats = GetObject<TLVector<TLChatBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override TLProxyDataBase GetEmptyObject()
        {
            return new TLProxyDataPromo
            {
                Expires = Expires,
                Peer = Peer,
                Chats = new TLVector<TLChatBase>(),
                Users = new TLVector<TLUserBase>(),
                CustomFlags = new TLLong(0)
            };
        }
    }
}
