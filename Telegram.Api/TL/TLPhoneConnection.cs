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
    public class TLPhoneConnection : TLObject
    {
        public const uint Signature = TLConstructors.TLPhoneConnection;

        public TLString Ip { get; set; }

        public TLString IpV6 { get; set; }

        public TLInt Port { get; set; }

        public TLString PeerTag { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Ip = GetObject<TLString>(bytes, ref position);
            IpV6 = GetObject<TLString>(bytes, ref position);
            Port = GetObject<TLInt>(bytes, ref position);
            PeerTag = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Ip.ToBytes(),
                IpV6.ToBytes(),
                Port.ToBytes(),
                PeerTag.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Ip.ToStream(output);
            IpV6.ToStream(output);
            Port.ToStream(output);
            PeerTag.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Ip = GetObject<TLString>(input);
            IpV6 = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);
            PeerTag = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLPhoneConnection61 : TLPhoneConnection
    {
        public new const uint Signature = TLConstructors.TLPhoneConnection61;

        public TLLong Id { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            Ip = GetObject<TLString>(bytes, ref position);
            IpV6 = GetObject<TLString>(bytes, ref position);
            Port = GetObject<TLInt>(bytes, ref position);
            PeerTag = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Ip.ToBytes(),
                IpV6.ToBytes(),
                Port.ToBytes(),
                PeerTag.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
            Ip.ToStream(output);
            IpV6.ToStream(output);
            Port.ToStream(output);
            PeerTag.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            Ip = GetObject<TLString>(input);
            IpV6 = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);
            PeerTag = GetObject<TLString>(input);

            return this;
        }
    }
}
