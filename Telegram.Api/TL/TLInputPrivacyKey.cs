// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLInputPrivacyKeyBase : TLObject { }

    public class TLInputPrivacyKeyStatusTimestamp : TLInputPrivacyKeyBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyKeyStatusTimestamp;

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

    public class TLInputPrivacyKeyChatInvite : TLInputPrivacyKeyBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyKeyChatInvite;

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

    public class TLInputPrivacyKeyPhoneCall : TLInputPrivacyKeyBase
    {
        public const uint Signature = TLConstructors.TLInputPrivacyKeyPhoneCall;

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
}
