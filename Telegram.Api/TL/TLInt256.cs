// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    public class TLInt256 : TLObject
    {
        public byte[] Value { get; set; }

        public override byte[] ToBytes()
        {
            return Value;
        }

        public static TLInt256 Random()
        {
            var randomNumber = new byte[32];
            var random = new Random();
            random.NextBytes(randomNumber);
            return new TLInt256 { Value = randomNumber };
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Value = bytes.SubArray(position, 32);
            position += 32;

            return this;
        }
    }
}
