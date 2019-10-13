// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Account
{
    public class TLUnregisterDevice : TLObject
    {
        public const string Signature = "#65c55b40";

        public TLInt TokenType { get; set; }

        public TLString Token { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                TokenType.ToBytes(),
                Token.ToBytes());
        }

        public override string ToString()
        {
            return string.Format("token_type={0}\ntoken={1}", TokenType, Token);
        }
    }
}