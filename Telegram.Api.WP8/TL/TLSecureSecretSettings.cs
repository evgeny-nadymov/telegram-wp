// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLSecureSecretSettings : TLObject
    {
        public const uint Signature = TLConstructors.TLSecureSecretSettings;

        public TLSecurePasswordKdfAlgoBase SecureAlgo { get; set; }

        public TLString SecureSecret { get; set; }

        public TLLong SecureSecretId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            SecureAlgo = GetObject<TLSecurePasswordKdfAlgoBase>(bytes, ref position);
            SecureSecret = GetObject<TLString>(bytes, ref position);
            SecureSecretId = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                SecureAlgo.ToBytes(),
                SecureSecret.ToBytes(),
                SecureSecretId.ToBytes());
        }
    }
}
