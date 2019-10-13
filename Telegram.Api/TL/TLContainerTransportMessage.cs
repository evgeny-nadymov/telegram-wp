// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL
{
    public class TLTransportMessageWithIdBase : TLObject
    {
        public TLLong MessageId { get; set; }
    }

    public class TLContainerTransportMessage : TLTransportMessageWithIdBase
    {
        public TLInt SeqNo { get; set; }
        public TLInt MessageDataLength { get; set; }
        public TLObject MessageData { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            MessageId = GetObject<TLLong>(bytes, ref position);
            SeqNo = GetObject<TLInt>(bytes, ref position);
            MessageDataLength = GetObject<TLInt>(bytes, ref position);
            MessageData = GetObject<TLObject>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            var objectBytes = MessageData.ToBytes();

            return TLUtils.Combine(
                MessageId.ToBytes(),
                SeqNo.ToBytes(),
                BitConverter.GetBytes(objectBytes.Length),
                objectBytes);
        }
    }

    public class TLTransportMessage : TLContainerTransportMessage
    {
        public TLLong Salt { get; set; }
        public TLLong SessionId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Salt = GetObject<TLLong>(bytes, ref position);
            SessionId = GetObject<TLLong>(bytes, ref position);
            
            MessageId = GetObject<TLLong>(bytes, ref position);
            SeqNo = GetObject<TLInt>(bytes, ref position);
            MessageDataLength = GetObject<TLInt>(bytes, ref position);
            MessageData = GetObject<TLObject>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            var objectBytes = MessageData.ToBytes();

            return TLUtils.Combine(
               Salt.ToBytes(),
               SessionId.ToBytes(),
               MessageId.ToBytes(),
               SeqNo.ToBytes(),
               BitConverter.GetBytes(objectBytes.Length),
               objectBytes);
        }
    }
}