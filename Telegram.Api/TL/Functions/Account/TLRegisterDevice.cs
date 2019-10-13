// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Account
{
    public class TLRegisterDevice : TLObject
    {
        public const uint Signature = 0x5cbea590;

        public TLInt TokenType { get; set; }

        public TLString Token { get; set; }

        public TLBool AppSandbox { get; set; }

        public TLString Secret { get; set; }

        public TLVector<TLInt> OtherUids { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                TokenType.ToBytes(),
                Token.ToBytes(),
                AppSandbox.ToBytes(),
                Secret.ToBytes(),
                OtherUids.ToBytes());
        }

        public override string ToString()
        {
            return string.Format("token_type={0}\ntoken={1}", TokenType, Token);
        }
    }
}
