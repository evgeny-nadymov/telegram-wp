// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;

namespace Telegram.Api.TL
{
    public class TLIpPort : TLObject
    {
        public TLInt Ip { get; set; }

        public TLInt Port { get; set; }

        public override string ToString()
        {
            return string.Format("TLIpPort ip={0}({1}) port={2}", GetIpString(), Ip, Port);
        }

        public string GetIpString()
        {
            var ip = Ip.ToBytes();

            return string.Format("{0}.{1}.{2}.{3}", ip[3], ip[2], ip[1], ip[0]);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Ip = GetObject<TLInt>(bytes, ref position);
            Port = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Ip = GetObject<TLInt>(input);
            Port = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            Ip.ToStream(output);
            Port.ToStream(output);
        }
    }
}
