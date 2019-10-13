// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Account
{
    class TLUpdatePasswordSettings : TLObject
    {
        public const uint Signature = 0xa59b102f;

        public TLInputCheckPasswordBase Password { get; set; }

        public TLPasswordInputSettings NewSettings { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Password.ToBytes(),
                NewSettings.ToBytes());
        }
    }
}