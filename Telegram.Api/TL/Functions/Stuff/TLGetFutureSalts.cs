﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Stuff
{
    public class TLGetFutureSalts : TLObject
    {
        public const string Signature = "#b921bd04";

        public TLInt Num { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Num.ToBytes());
        }
    }
}
