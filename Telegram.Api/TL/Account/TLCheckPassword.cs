// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Account
{
    class TLCheckPassword : TLObject
    {
        public const uint Signature = 0xd18b4d16;

        public TLInputCheckPasswordBase Password { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Password.ToBytes());
        }
    }

    class TLCheckPasswordOld : TLObject
    {
        public const string Signature = "#a63011e";

        public TLString PasswordHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PasswordHash.ToBytes());
        }
    }
}
