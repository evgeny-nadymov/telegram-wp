// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Auth
{
    class TLBindTempAuthKey : TLObject
    {
        public const uint Signature = 0xcdd42a05;

        public TLLong PermAuthKeyId { get; set; }

        public TLLong Nonce { get; set; }

        public TLInt ExpiresAt { get; set; }

        public TLString EncryptedMessage { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PermAuthKeyId.ToBytes(),
                Nonce.ToBytes(),
                ExpiresAt.ToBytes(),
                EncryptedMessage.ToBytes());
        }
    }
}
