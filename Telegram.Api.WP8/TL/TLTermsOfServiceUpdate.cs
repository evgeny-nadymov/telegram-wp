// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLTermsOfServiceUpdateBase : TLObject
    {
        public TLInt Expires { get; set; }
    }

    public class TLTermsOfServiceUpdate : TLTermsOfServiceUpdateBase
    {
        public const uint Signature = TLConstructors.TLTermsOfServiceUpdate;

        public TLTermsOfServiceBase TermsOfService { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Expires = GetObject<TLInt>(bytes, ref position);
            TermsOfService = GetObject<TLTermsOfServiceBase>(bytes, ref position);

            return this;
        }
    }

    public class TLTermsOfServiceUpdateEmpty : TLTermsOfServiceUpdateBase
    {
        public const uint Signature = TLConstructors.TLTermsOfServiceUpdateEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Expires = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }
}
