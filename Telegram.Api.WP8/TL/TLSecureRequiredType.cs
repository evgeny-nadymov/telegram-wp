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
    public enum SecureRequiredTypeFlags
    {
        NativeNames = 0x1,              // 0
        SelfieRequired = 0x2,           // 1
        TranslationRequired = 0x4,      // 2
    }

    public abstract class TLSecureRequiredTypeBase : TLObject { }
    
    public class TLSecureRequiredType : TLSecureRequiredTypeBase
    {
        public const uint Signature = TLConstructors.TLSecureRequiredType;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool NativeNames { get { return IsSet(Flags, (int)SecureRequiredTypeFlags.NativeNames); } }

        public bool SelfieRequired { get { return IsSet(Flags, (int)SecureRequiredTypeFlags.SelfieRequired); } }

        public bool TranslationRequired { get { return IsSet(Flags, (int)SecureRequiredTypeFlags.TranslationRequired); } }

        public TLSecureValueTypeBase Type { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            
            _flags = GetObject<TLInt>(bytes, ref position);
            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLSecureRequiredType type={0} native_names={1} selfie={2} translation={3}", Type, NativeNames, SelfieRequired, TranslationRequired);
        }
    }

    public class TLSecureRequiredTypeOneOf : TLSecureRequiredTypeBase
    {
        public const uint Signature = TLConstructors.TLSecureRequiredTypeOneOf;

        public TLVector<TLSecureRequiredTypeBase> Types { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Types = GetObject<TLVector<TLSecureRequiredTypeBase>>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLSecureRequiredTypeOneOf types=[{0}]", string.Join(",", Types));
        }
    }
}
