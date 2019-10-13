// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLBadServerSalt : TLObject
    {
        public const uint Signature = TLConstructors.TLBadServerSalt;

        public TLLong BadMessageId { get; set; }

        public TLInt BadMessageSeqNo { get; set; }

        public TLInt ErrorCode { get; set; }

        public TLLong NewServerSalt { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            BadMessageId = GetObject<TLLong>(bytes, ref position);
            BadMessageSeqNo = GetObject<TLInt>(bytes, ref position);
            ErrorCode = GetObject<TLInt>(bytes, ref position);
            NewServerSalt = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLBadServerSalt msg_id={0} msg_seq_no={1} error_code={2} new_salt={3}", BadMessageId, BadMessageSeqNo, ErrorCode, NewServerSalt);
        }
    }
}
