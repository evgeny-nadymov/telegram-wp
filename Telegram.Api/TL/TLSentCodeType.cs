// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public interface ILength
    {
        TLInt Length { get; set; }
    }

    public abstract class TLSentCodeTypeBase : TLObject
    {
    }

    public class TLSentCodeTypeApp : TLSentCodeTypeBase, ILength
    {
        public const uint Signature = TLConstructors.TLSentCodeTypeApp;

        public TLInt Length { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Length = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Length.ToBytes());
        }
    }

    public class TLSentCodeTypeSms : TLSentCodeTypeBase, ILength
    {
        public const uint Signature = TLConstructors.TLSentCodeTypeSms;

        public TLInt Length { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Length = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Length.ToBytes());
        }
    }

    public class TLSentCodeTypeCall : TLSentCodeTypeBase, ILength
    {
        public const uint Signature = TLConstructors.TLSentCodeTypeCall;

        public TLInt Length { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Length = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Length.ToBytes());
        }
    }

    public class TLSentCodeTypeFlashCall : TLSentCodeTypeBase
    {
        public const uint Signature = TLConstructors.TLSentCodeTypeFlashCall;

        public TLString Pattern { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Pattern = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Pattern.ToBytes());
        }
    }
}
