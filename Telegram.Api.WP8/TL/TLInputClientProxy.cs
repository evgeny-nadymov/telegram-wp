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
    public class TLInputClientProxy : TLObject
    {
        public const uint Signature = TLConstructors.TLInputClientProxy;

        public TLString Address { get; set; }

        public TLInt Port { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Address.ToBytes(),
                Port.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Address.ToStream(output);
            Port.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Address = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputClientProxy address={0} port={1}", Address, Port);
        }
    }
}