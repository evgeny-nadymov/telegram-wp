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
    [Flags]
    public enum AuthorizationFormFlags
    {
        PrivacyPolicyUrl = 0x1,         // 0
        SelfieRequired = 0x2,           // 1
    }

    public class TLAuthorizationForm : TLObject
    {
        public const uint Signature = TLConstructors.TLAuthorizationForm;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool SelfieRequired { get { return IsSet(Flags, (int) AuthorizationFormFlags.SelfieRequired); } }

        public TLVector<TLSecureValueTypeBase> RequiredTypes { get; set; }

        public TLVector<TLSecureValue> Values { get; set; }

        public TLVector<TLSecureValueErrorBase> Errors { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        protected TLString _privacyPolicyUrl;

        public TLString PrivacyPolicyUrl
        {
            get { return _privacyPolicyUrl; }
            set { SetField(out _privacyPolicyUrl, value, ref _flags, (int)AuthorizationFormFlags.PrivacyPolicyUrl); }
        }

        #region Additional
        
        public TLInt BotId { get; set; }

        public TLString Scope { get; set; }

        public TLString PublicKey { get; set; }

        public TLString CallbackUrl { get; set; }

        public TLString Payload { get; set; }

        public TLPassportConfig Config { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            
            _flags = GetObject<TLInt>(bytes, ref position);        
            RequiredTypes = GetObject<TLVector<TLSecureValueTypeBase>>(bytes, ref position);
            Values = GetObject<TLVector<TLSecureValue>>(bytes, ref position);
            Errors = GetObject<TLVector<TLSecureValueErrorBase>>(bytes, ref position);     
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);
            _privacyPolicyUrl = GetObject<TLString>(Flags, (int)AuthorizationFormFlags.PrivacyPolicyUrl, null, bytes, ref position);

            return this;
        }
    }

    public class TLAuthorizationForm85 : TLAuthorizationForm
    {
        public new const uint Signature = TLConstructors.TLAuthorizationForm85;

        public TLVector<TLSecureRequiredTypeBase> NewRequiredTypes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            NewRequiredTypes = GetObject<TLVector<TLSecureRequiredTypeBase>>(bytes, ref position);
            Values = GetObject<TLVector<TLSecureValue>>(bytes, ref position);
            Errors = GetObject<TLVector<TLSecureValueErrorBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);
            _privacyPolicyUrl = GetObject<TLString>(Flags, (int)AuthorizationFormFlags.PrivacyPolicyUrl, null, bytes, ref position);

            return this;
        }
    }
}
