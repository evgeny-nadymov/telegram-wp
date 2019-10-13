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
using Org.BouncyCastle.Security;

namespace Telegram.Api.TL
{
    [DataContract]
    public class TLLong : TLObject
    {
        [DataMember]
        public Int64 Value { get; set; }

        public TLLong()
        {
            
        }

        public TLLong(long value)
        {
            Value = value;
        }

        private static readonly object _randomSyncRoot = new object();

        private static Random _random;

        public static TLLong Random()
        {
            //System.Diagnostics.Debug.WriteLine("TLLong.Random 1");
            var randomNumber = new byte[8];
            lock (_randomSyncRoot)
            {
                //System.Diagnostics.Debug.WriteLine("TLLong.Random 2");
                if (_random == null)
                {
                    //System.Diagnostics.Debug.WriteLine("TLLong.Random 3");
                    _random = new Random(); // Note: SecureRandom doesnt work with Creators Update
                    //System.Diagnostics.Debug.WriteLine("TLLong.Random 4");
                }

                //System.Diagnostics.Debug.WriteLine("TLLong.Random 5");
                _random.NextBytes(randomNumber);
            }

            //System.Diagnostics.Debug.WriteLine("TLLong.Random 6");

            return new TLLong { Value = BitConverter.ToInt64(randomNumber, 0) };
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Value = BitConverter.ToInt64(bytes, position);
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
            Value = BitConverter.ToInt64(buffer, 0);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(BitConverter.GetBytes(Value), 0, 8);
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);// + " " + TLUtils.MessageIdString(this);
        }
    }
}
