// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLPasswordKdfAlgoBase : TLObject { }

    public class TLPasswordKdfAlgoUnknown : TLPasswordKdfAlgoBase
    {
        public const uint Signature = TLConstructors.TLPasswordKdfAlgoUnknown;

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

    public class TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow : TLPasswordKdfAlgoBase
    {
        public const uint Signature = TLConstructors.TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;

        public TLString Salt1 { get; set; }

        public TLString Salt2 { get; set; }

        public TLInt G { get; set; }

        public TLString P { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Salt1 = GetObject<TLString>(bytes, ref position);
            Salt2 = GetObject<TLString>(bytes, ref position);
            G = GetObject<TLInt>(bytes, ref position);
            P = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Salt1.ToBytes(),
                Salt2.ToBytes(),
                G.ToBytes(),
                P.ToBytes());
        }
    }
}
