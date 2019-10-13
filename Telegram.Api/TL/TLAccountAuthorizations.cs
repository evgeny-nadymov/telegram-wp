// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLAccountAuthorizations : TLObject
    {
        public const uint Signature = TLConstructors.TLAccountAuthorizations;

        public TLVector<TLAccountAuthorization> Authorizations { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Authorizations = GetObject<TLVector<TLAccountAuthorization>>(bytes, ref position);

            return this;
        }
    }
}
