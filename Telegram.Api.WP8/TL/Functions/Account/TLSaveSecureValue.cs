// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Account
{
    class TLSaveSecureValue : TLObject
    {
        public const uint Signature = 0x899fe31d;

        public TLInputSecureValue Value { get; set; }

        public TLLong SecureSecretId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Value.ToBytes(),
                SecureSecretId.ToBytes());
        }
    }
}
