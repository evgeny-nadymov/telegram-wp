// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLPaymentResultBase : TLObject { }

    public class TLPaymentResult : TLPaymentResultBase
    {
        public const uint Signature = TLConstructors.TLPaymentResult;

        public TLUpdatesBase Updates { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Updates = GetObject<TLUpdatesBase>(bytes, ref position);

            return this;
        }
    }

    public class TLPaymentVerificationNeeded : TLPaymentResultBase
    {
        public const uint Signature = TLConstructors.TLPaymentVerificationNeeded;

        public TLString Url { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}