// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLServerDHInnerData : TLObject
    {
        public const uint Signature = TLConstructors.TLServerDHInnerData;

        public TLInt128 Nonce { get; set; }

        public TLInt128 ServerNonce { get; set; }

        public TLInt G { get; set; }

        public TLString DHPrime { get; set; }

        public TLString GA { get; set; }

        public TLInt ServerTime { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Nonce.ToBytes(),
                ServerNonce.ToBytes(),
                G.ToBytes(),
                DHPrime.ToBytes(),
                GA.ToBytes(),
                ServerTime.ToBytes());
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Nonce = GetObject<TLInt128>(bytes, ref position);
            ServerNonce = GetObject<TLInt128>(bytes, ref position);
            G = GetObject<TLInt>(bytes, ref position);
            DHPrime = GetObject<TLString>(bytes, ref position);
            GA = GetObject<TLString>(bytes, ref position);
            ServerTime = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }
}
