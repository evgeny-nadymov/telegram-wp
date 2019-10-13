// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Account
{
    class TLConfirmPhone : TLObject
    {
        public const uint Signature = 0x5f2178c3;

        public TLString PhoneCodeHash { get; set; }

        public TLString PhoneCode { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneCodeHash.ToBytes(),
                PhoneCode.ToBytes());
        }
    }
}