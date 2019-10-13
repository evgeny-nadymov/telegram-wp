// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLPrivacyRules : TLObject
    {
        public const uint Signature = TLConstructors.TLPrivacyRules;

        public TLVector<TLPrivacyRuleBase> Rules { get; set; }

        public TLVector<TLUserBase> Users { get; set; } 

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Rules = GetObject<TLVector<TLPrivacyRuleBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }
    }
}
