// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLDecryptedMessageLayer : TLObject
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageLayer;

        public TLInt Layer { get; set; }

        public TLDecryptedMessageBase Message { get; set; }

        public override byte[] ToBytes()
        {
            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessageLayer.ToBytes layer={0} message=[{1}]", Layer, Message);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Layer.ToBytes(),
                Message.ToBytes());
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessageLayer.FromBytes layer={0} message=[{1}]", Layer, Message);

            Layer = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLDecryptedMessageBase>(bytes, ref position);

            return this;
        }
    }

    public class TLDecryptedMessageLayer17 : TLDecryptedMessageLayer
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageLayer17;

        public TLString RandomBytes { get; set; }

        public TLInt InSeqNo { get; set; }

        public TLInt OutSeqNo { get; set; }

        public override byte[] ToBytes()
        {
            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessageLayer17.ToBytes layer={0} in_seq_no={1} out_seq_no={2} message=[{3}]", Layer, InSeqNo, OutSeqNo, Message);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                RandomBytes.ToBytes(),
                Layer.ToBytes(),
                InSeqNo.ToBytes(),
                OutSeqNo.ToBytes(),
                Message.ToBytes());
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            RandomBytes = GetObject<TLString>(bytes, ref position);
            Layer = GetObject<TLInt>(bytes, ref position);
            InSeqNo = GetObject<TLInt>(bytes, ref position);
            OutSeqNo = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLDecryptedMessageBase>(bytes, ref position);

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessageLayer17.FromBytes layer={0} in_seq_no={1} out_seq_no={2} message=[{3}]", Layer, InSeqNo, OutSeqNo, Message);

            return this;
        }
    }
}
