// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;

namespace Telegram.Api.TL
{
    public class TLBadMessageNotification : TLObject
    {
        public const uint Signature = TLConstructors.TLBadMessageNotification;

        public TLLong BadMessageId { get; set; }

        public TLInt BadMessageSequenceNumber { get; set; }

        public TLInt ErrorCode { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            BadMessageId = GetObject<TLLong>(bytes, ref position);
            BadMessageSequenceNumber = GetObject<TLInt>(bytes, ref position);
            ErrorCode = GetObject<TLInt>(bytes, ref position);
            
            return this;
        }

        public override string ToString()
        {
            return string.Format("TLBadMessageNotification msg_id={0} msg_seq_no={1} error_code={2}", BadMessageId, BadMessageSequenceNumber, ErrorCode);
        }
    }
}