// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Auth
{
    public class TLSignUp : TLObject
    {
        public const string Signature = "#1b067634";

        public TLString PhoneNumber { get; set; }

        public TLString PhoneCodeHash { get; set; }

        public TLString PhoneCode { get; set; }

        public TLString FirstName { get; set; }

        public TLString LastName { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneNumber.ToBytes(),
                PhoneCodeHash.ToBytes(),
                PhoneCode.ToBytes(),
                FirstName.ToBytes(),
                LastName.ToBytes());
        }
    }
}
