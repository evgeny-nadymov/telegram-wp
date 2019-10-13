// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace Telegram.Api.TL
{
    [DataContract]
    public class TLDouble : TLObject
    {
        public TLDouble() { }

        public TLDouble(double value)
        {
            Value = value;
        }

        [DataMember]
        public Double Value { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Value = BitConverter.ToDouble(bytes, position);
            position += 8;

            return this;
        }

        public override byte[] ToBytes()
        {
            return BitConverter.GetBytes(Value);
        }

        public override TLObject FromStream(Stream input)
        {
            var buffer = new byte[8];
            input.Read(buffer, 0, 8);
            Value = BitConverter.ToDouble(buffer, 0);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(BitConverter.GetBytes(Value), 0, 8);
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
