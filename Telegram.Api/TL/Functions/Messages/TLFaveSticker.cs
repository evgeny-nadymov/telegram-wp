// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Messages
{
    class TLFaveSticker : TLObject
    {
        public const uint Signature = 0xb9ffc55b;

        public TLInputDocumentBase Id { get; set; }

        public TLBool Unfave { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Unfave.ToBytes());
        }
    }
}
