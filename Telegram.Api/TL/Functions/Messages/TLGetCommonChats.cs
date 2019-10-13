// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Messages
{
    class TLGetCommonChats : TLObject
    {
        public const uint Signature = 0xd0a48c4;

        public TLInputUserBase User { get; set; }

        public TLInt MaxId { get; set; }

        public TLInt Limit { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                User.ToBytes(),
                MaxId.ToBytes(),
                Limit.ToBytes());
        }
    }
}
