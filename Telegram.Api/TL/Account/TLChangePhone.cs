// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Account
{
    class TLChangePhone : TLObject
    {
        public const string Signature = "#70c32edb";

        public TLString PhoneNumber { get; set; }

        public TLString PhoneCodeHash { get; set; }

        public TLString PhoneCode { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneNumber.ToBytes(),
                PhoneCodeHash.ToBytes(),
                PhoneCode.ToBytes());
        }
    }
}