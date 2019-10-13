// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLPQInnerData : TLObject
    {
        public const uint Signature = TLConstructors.TLPQInnerData;

        public TLString PQ { get; set; }

        public TLString P { get; set; }

        public TLString Q { get; set; }

        public TLInt128 Nonce { get; set; }

        public TLInt128 ServerNonce { get; set; }

        public TLInt256 NewNonce { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PQ.ToBytes(),
                P.ToBytes(),
                Q.ToBytes(),
                Nonce.ToBytes(),
                ServerNonce.ToBytes(),
                NewNonce.ToBytes());
        }
    }

    public class TLPQInnerDataDC : TLPQInnerData
    {
        public new const uint Signature = TLConstructors.TLPQInnerDataDC;

        public TLInt DCId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PQ.ToBytes(),
                P.ToBytes(),
                Q.ToBytes(),
                Nonce.ToBytes(),
                ServerNonce.ToBytes(),
                NewNonce.ToBytes(),
                DCId.ToBytes());
        }
    }
}
