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
    public class TLConfigSimple : TLObject
    {
        public const uint Signature = TLConstructors.TLConfigSimple;

        public TLInt Date { get; set; }

        public TLInt Expires { get; set; }

        public TLInt DCId { get; set; }

        public TLVector<TLIpPort> IpPortList { get; set; }

        public override string ToString()
        {
            return string.Format("TLConfigSimple date={0} expires={1} dc_id={2} ip_port_list=[{3}]", Date, Expires, DCId, string.Join(", ", IpPortList));
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);
            DCId = GetObject<TLInt>(bytes, ref position);
            IpPortList = GetObject<TLVector<TLIpPort>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Date = GetObject<TLInt>(input);
            Expires = GetObject<TLInt>(input);
            DCId = GetObject<TLInt>(input);
            IpPortList = GetObject<TLVector<TLIpPort>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Date.ToStream(output);
            Expires.ToStream(output);
            DCId.ToStream(output);
            IpPortList.ToStream(output);
        }
    }
}
