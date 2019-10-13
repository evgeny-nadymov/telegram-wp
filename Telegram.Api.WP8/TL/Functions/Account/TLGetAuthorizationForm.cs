// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Account
{
    class TLGetAuthorizationForm : TLObject
    {
        public const uint Signature = 0xb86ba8e1;

        public TLInt BotId { get; set; }

        public TLString Scope { get; set; }

        public TLString PublicKey { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                BotId.ToBytes(),
                Scope.ToBytes(),
                PublicKey.ToBytes());
        }
    }
}
