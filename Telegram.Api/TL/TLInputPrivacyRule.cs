// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLInputPrivacyRuleBase : TLObject { }

    public class TLInputPrivacyValueAllowContacts : TLInputPrivacyRuleBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyValueAllowContacts;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLInputPrivacyValueAllowAll : TLInputPrivacyRuleBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyValueAllowAll;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLInputPrivacyValueAllowUsers : TLInputPrivacyRuleBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyValueAllowUsers;

        public TLVector<TLInputUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Users = GetObject<TLVector<TLInputUserBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Users.ToBytes());
        }
    }

    public class TLInputPrivacyValueDisallowContacts : TLInputPrivacyRuleBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyValueDisallowContacts;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLInputPrivacyValueDisallowAll : TLInputPrivacyRuleBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyValueDisallowAll;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLInputPrivacyValueDisallowUsers : TLInputPrivacyRuleBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyValueDisallowUsers;

        public TLVector<TLInputUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Users = GetObject<TLVector<TLInputUserBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Users.ToBytes());
        }
    }
}
