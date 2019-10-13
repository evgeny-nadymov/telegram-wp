// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Users
{
    class TLSetSecureValueErrors : TLObject
    {
        public const uint Signature = 0x90c894b5;

        public TLInputUserBase Id { get; set; }

        public TLVector<TLSecureValueErrorBase> Errors { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Errors.ToBytes());
        }
    }
}
