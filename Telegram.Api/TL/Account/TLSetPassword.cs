// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Account
{
    class TLSetPassword : TLObject
    {
        public const string Signature = "#dd2a4d8f";

        public TLString CurrentPasswordHash { get; set; }

        public TLString NewSalt { get; set; }

        public TLString NewPasswordHash { get; set; }

        public TLString Hint { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                CurrentPasswordHash.ToBytes(),
                NewSalt.ToBytes(),
                NewPasswordHash.ToBytes(),
                Hint.ToBytes());
        }
    }
}
