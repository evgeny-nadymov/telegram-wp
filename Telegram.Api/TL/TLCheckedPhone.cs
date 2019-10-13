// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLCheckedPhoneBase : TLObject
    {
        public TLBool PhoneRegistered { get; set; }
    }

    public class TLCheckedPhone : TLCheckedPhoneBase
    {
        public const uint Signature = TLConstructors.TLCheckedPhone;

        public TLBool PhoneInvited { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneRegistered = GetObject<TLBool>(bytes, ref position);
            PhoneInvited = GetObject<TLBool>(bytes, ref position);

            return this;
        }
    }

    public class TLCheckedPhone24 : TLCheckedPhoneBase
    {
        public const uint Signature = TLConstructors.TLCheckedPhone24;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneRegistered = GetObject<TLBool>(bytes, ref position);

            return this;
        }
    }
}
