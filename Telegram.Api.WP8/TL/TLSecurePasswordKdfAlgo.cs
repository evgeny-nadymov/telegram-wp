// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLSecurePasswordKdfAlgoBase : TLObject { }

    public class TLSecurePasswordKdfAlgoUnknown : TLSecurePasswordKdfAlgoBase
    {
        public const uint Signature = TLConstructors.TLSecurePasswordKdfAlgoUnknown;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000 : TLSecurePasswordKdfAlgoBase
    {
        public const uint Signature = TLConstructors.TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000;

        public TLString Salt { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Salt = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Salt.ToBytes());
        }
    }

    public class TLSecurePasswordKdfAlgoSHA512 : TLSecurePasswordKdfAlgoBase
    {
        public const uint Signature = TLConstructors.TLSecurePasswordKdfAlgoSHA512;

        public TLString Salt { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Salt = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Salt.ToBytes());
        }
    }
}
