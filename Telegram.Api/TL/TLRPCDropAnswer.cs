// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLRPCAnswerUnknown : TLObject
    {
        public const uint Signature = TLConstructors.TLRPCAnswerUnknown;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }

    public class TLRPCAnswerDroppedRunning : TLObject
    {
        public const uint Signature = TLConstructors.TLRPCAnswerDroppedRunning;
        
        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }

    public class TLRPCAnswerDropped : TLObject
    {
        public const uint Signature = TLConstructors.TLRPCAnswerDropped;

        public TLLong MsgId { get; set; }

        public TLInt SeqNo { get; set; }

        public TLInt Bytes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            MsgId = GetObject<TLLong>(bytes, ref position);
            SeqNo = GetObject<TLInt>(bytes, ref position);
            Bytes = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }
}
