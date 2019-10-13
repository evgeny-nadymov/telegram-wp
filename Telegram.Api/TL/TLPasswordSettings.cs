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
    public enum PasswordSettingsFlags
    {
        Email = 0x1,                // 0
        SecureSettings = 0x2,       // 1
    }

    public class TLPasswordSettings : TLObject
    {
        public const uint Signature = TLConstructors.TLPasswordSettings;

        public virtual TLString Email { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Email = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLPasswordSettings81 : TLPasswordSettings
    {
        public new const uint Signature = TLConstructors.TLPasswordSettings81;

        public TLString SecureSalt { get; set; }

        public TLString SecureSecret { get; set; }

        public TLLong SecureSecretId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Email = GetObject<TLString>(bytes, ref position);
            SecureSalt = GetObject<TLString>(bytes, ref position);
            SecureSecret = GetObject<TLString>(bytes, ref position);
            SecureSecretId = GetObject<TLLong>(bytes, ref position);

            return this;
        }
    }

    public class TLPasswordSettings83 : TLPasswordSettings81
    {
        public new const uint Signature = TLConstructors.TLPasswordSettings83;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLString _email;

        public override TLString Email
        {
            get { return _email; }
            set { SetField(out _email, value, ref _flags, (int)PasswordSettingsFlags.Email); }
        }

        protected TLSecureSecretSettings _secureSettings;

        public TLSecureSecretSettings SecureSettings
        {
            get { return _secureSettings; }
            set { SetField(out _secureSettings, value, ref _flags, (int)PasswordSettingsFlags.SecureSettings); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            _email = GetObject<TLString>(Flags, (int)PasswordSettingsFlags.Email, null, bytes, ref position);
            _secureSettings = GetObject<TLSecureSecretSettings>(Flags, (int)PasswordSettingsFlags.SecureSettings, null, bytes, ref position);
            
            return this;
        }
    }
}