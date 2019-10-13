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
    public enum PasswordInputSettingsFlags
    {
        Password = 0x1,
        Email = 0x2,
        NewSecureSecret = 0x4,
    }

    public class TLPasswordInputSettings : TLObject
    {
        public const uint Signature = TLConstructors.TLPasswordInputSettings;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; } 
            set { _flags = value; }
        }

        private TLString _newSalt;

        public virtual TLString NewSalt
        {
            get { return _newSalt; }
            set { SetField(out _newSalt, value, ref _flags, (int)PasswordInputSettingsFlags.Password); }
        }

        protected TLString _newPasswordHash;

        public TLString NewPasswordHash
        {
            get { return _newPasswordHash; }
            set { SetField(out _newPasswordHash, value, ref _flags, (int)PasswordInputSettingsFlags.Password); }
        }

        protected TLString _hint;

        public TLString Hint
        {
            get { return _hint; }
            set { SetField(out _hint, value, ref _flags, (int)PasswordInputSettingsFlags.Password); }
        }

        protected TLString _email;

        public TLString Email
        {
            get { return _email; }
            set { SetField(out _email, value, ref _flags, (int)PasswordInputSettingsFlags.Email); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            NewSalt = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            NewPasswordHash = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            Hint = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            Email = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Email, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(NewSalt, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(NewPasswordHash, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(Hint, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(Email, Flags, (int)PasswordInputSettingsFlags.Email));
        }
    }

    public class TLPasswordInputSettings81 : TLPasswordInputSettings
    {
        public new const uint Signature = TLConstructors.TLPasswordInputSettings81;

        private TLString _newSecureSalt;

        public virtual TLString NewSecureSalt
        {
            get { return _newSecureSalt; }
            set { SetField(out _newSecureSalt, value, ref _flags, (int)PasswordInputSettingsFlags.NewSecureSecret); }
        }

        private TLString _newSecureSecret;

        public virtual TLString NewSecureSecret
        {
            get { return _newSecureSecret; }
            set { SetField(out _newSecureSecret, value, ref _flags, (int)PasswordInputSettingsFlags.NewSecureSecret); }
        }

        private TLLong _newSecureSecretId;

        public virtual TLLong NewSecureSecretId
        {
            get { return _newSecureSecretId; }
            set { SetField(out _newSecureSecretId, value, ref _flags, (int)PasswordInputSettingsFlags.NewSecureSecret); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            NewSalt = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            NewPasswordHash = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            Hint = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            Email = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Email, null, bytes, ref position);
            NewSecureSalt = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.NewSecureSecret, null, bytes, ref position);
            NewSecureSecret = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.NewSecureSecret, null, bytes, ref position);
            NewSecureSecretId = GetObject<TLLong>(Flags, (int)PasswordInputSettingsFlags.NewSecureSecret, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(NewSalt, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(NewPasswordHash, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(Hint, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(Email, Flags, (int)PasswordInputSettingsFlags.Email),
                ToBytes(NewSecureSalt, Flags, (int)PasswordInputSettingsFlags.NewSecureSecret),
                ToBytes(NewSecureSecret, Flags, (int)PasswordInputSettingsFlags.NewSecureSecret),
                ToBytes(NewSecureSecretId, Flags, (int)PasswordInputSettingsFlags.NewSecureSecret));
        }
    }

    public class TLPasswordInputSettings83 : TLPasswordInputSettings81
    {
        public new const uint Signature = TLConstructors.TLPasswordInputSettings83;

        private TLPasswordKdfAlgoBase _newAlgo;

        public TLPasswordKdfAlgoBase NewAlgo    
        {
            get { return _newAlgo; }
            set { SetField(out _newAlgo, value, ref _flags, (int)PasswordInputSettingsFlags.Password); }
        }

        private TLSecureSecretSettings _newSecureSettings;

        public TLSecureSecretSettings NewSecureSettings
        {
            get { return _newSecureSettings; }
            set { SetField(out _newSecureSettings, value, ref _flags, (int)PasswordInputSettingsFlags.NewSecureSecret); }
        }
        
        #region Additional
        public string NewPassword { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _newAlgo = GetObject<TLPasswordKdfAlgoBase>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            _newPasswordHash = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            _hint = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Password, null, bytes, ref position);
            _email = GetObject<TLString>(Flags, (int)PasswordInputSettingsFlags.Email, null, bytes, ref position);
            _newSecureSettings = GetObject<TLSecureSecretSettings>(Flags, (int)PasswordInputSettingsFlags.NewSecureSecret, null, bytes, ref position);
            
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(NewAlgo, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(NewPasswordHash, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(Hint, Flags, (int)PasswordInputSettingsFlags.Password),
                ToBytes(Email, Flags, (int)PasswordInputSettingsFlags.Email),
                ToBytes(NewSecureSettings, Flags, (int)PasswordInputSettingsFlags.NewSecureSecret));
        }
    }
}