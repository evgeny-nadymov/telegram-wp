// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Org.BouncyCastle.Security;
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    public class TLInt128 : TLObject
    {
        public byte[] Value { get; set; }

        public override byte[] ToBytes()
        {
            return Value;
        }

        public static TLInt128 Random()
        {
            var randomNumber = new byte[16];
            var random = new SecureRandom();
            random.NextBytes(randomNumber);
            return new TLInt128{ Value = randomNumber };
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Value = bytes.SubArray(position, 16);
            position += 16;

            return this;
        }
    }
}
