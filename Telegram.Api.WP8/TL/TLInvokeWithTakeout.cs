// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLInvokeWithTakeout : TLObject
    {
        public const uint Signature = 0xaca9fd2e;

        public TLLong TakeoutId { get; set; }

        public TLObject Object { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                TakeoutId.ToBytes(),
                Object.ToBytes());
        }
    }
}
