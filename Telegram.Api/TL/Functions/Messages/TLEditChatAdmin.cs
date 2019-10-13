// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Messages
{
    class TLEditChatAdmin : TLObject
    {
        public const uint Signature = 0xa9e69f2e;

        public TLInt ChatId { get; set; }

        public TLInputUserBase UserId { get; set; }

        public TLBool IsAdmin { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                ChatId.ToBytes(),
                UserId.ToBytes(),
                IsAdmin.ToBytes());
        }
    }
}
