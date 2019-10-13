// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLRPCResult : TLObject
    {
        public const uint Signature = TLConstructors.TLRPCResult;

        public TLLong RequestMessageId { get; set; }

        public TLObject Object { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            RequestMessageId = GetObject<TLLong>(bytes, ref position);
            Object = GetObject<TLObject>(bytes, ref position);

            return this;
        }
    }
}
