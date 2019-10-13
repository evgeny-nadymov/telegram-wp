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
    public enum PhoneCallProtocolFlags
    {
        UdpP2P = 0x1,           // 0
        UdpReflector = 0x2,     // 1
    }

    public class TLPhoneCallProtocol : TLPhoneCallBase
    {
        public const uint Signature = TLConstructors.TLPhoneCallProtocol;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool UdpP2P
        {
            get { return IsSet(Flags, (int) PhoneCallProtocolFlags.UdpP2P); }
            set { SetUnset(ref _flags, value, (int) PhoneCallProtocolFlags.UdpP2P); }
        }

        public bool UdpReflector 
        {
            get { return IsSet(Flags, (int) PhoneCallProtocolFlags.UdpReflector); }
            set { SetUnset(ref _flags, value, (int) PhoneCallProtocolFlags.UdpReflector); }
        }

        public TLInt MinLayer { get; set; }

        public TLInt MaxLayer { get; set; }

        public override string ToString()
        {
            return string.Format("TLPhoneCallProtocol min_layer={0} max_layer={1}", MinLayer, MaxLayer);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            MinLayer = GetObject<TLInt>(bytes, ref position);
            MaxLayer = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                MinLayer.ToBytes(),
                MaxLayer.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            MinLayer.ToStream(output);
            MaxLayer.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            MinLayer = GetObject<TLInt>(input);
            MaxLayer = GetObject<TLInt>(input);

            return this;
        }
    }
}
