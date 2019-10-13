// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLSecureCredentialsEncrypted : TLObject
    {
        public const uint Signature = TLConstructors.TLSecureCredentialsEncrypted;

        public TLString Data { get; set; }

        public TLString Secret { get; set; }

        public TLString Hash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Data = GetObject<TLString>(bytes, ref position);
            Hash = GetObject<TLString>(bytes, ref position);
            Secret = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Data.ToBytes(),
                Hash.ToBytes(),
                Secret.ToBytes());
        }
    }
}
