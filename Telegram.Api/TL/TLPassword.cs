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
    public enum PasswordFlags
    {
        HasRecovery = 0x1,              // 0
        HasSecureValues = 0x2,          // 1
        HasPassword = 0x4,              // 2
        Hint = 0x8,                     // 3
        EmailUnconfirmedPattern = 0x10, // 4
    }

    public interface IPasswordSecret
    {
        TLString SecretRandom { get; set; }

        TLSecurePasswordKdfAlgoBase NewSecureAlgo { get; set; }
    }

    public interface IPasswordSRPParams
    {
        TLString SRPB { get; set; }

        TLLong SRPId { get; set; }

        TLPasswordKdfAlgoBase CurrentAlgo { get; set; }
    }

    public abstract class TLPasswordBase : TLObject
    {
        public TLString NewSalt { get; set; }

        public virtual TLString EmailUnconfirmedPattern { get; set; }

        public bool IsAvailable { get { return HasPassword || !TLString.IsNullOrEmpty(EmailUnconfirmedPattern); } } 

        public bool IsAuthRecovery { get; set; }

        public abstract bool HasPassword { get; }

        public TLString CurrentPasswordHash { get; set; }
    }

    public class TLPassword : TLPasswordBase
    {
        public const uint Signature = TLConstructors.TLPassword;

        public TLString CurrentSalt { get; set; }

        public virtual TLString Hint { get; set; }

        public virtual TLBool HasRecovery { get; set; }

        public override bool HasPassword { get { return true; } }

        #region Additional

        public TLPasswordSettings Settings { get; set; }

        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            CurrentSalt = GetObject<TLString>(bytes, ref position);
            NewSalt = GetObject<TLString>(bytes, ref position);
            Hint = GetObject<TLString>(bytes, ref position);
            HasRecovery = GetObject<TLBool>(bytes, ref position);
            EmailUnconfirmedPattern = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLPassword81 : TLPassword
    {
        public new const uint Signature = TLConstructors.TLPassword81;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool HasRecovery
        {
            get { return new TLBool(IsSet(_flags, (int) PasswordFlags.HasRecovery)); }
            set { SetUnset(ref _flags, value != null && value.Value, (int)PasswordFlags.HasRecovery); }
        }

        public bool HasSecureValues
        {
            get { return IsSet(_flags, (int) PasswordFlags.HasSecureValues); }
        }

        public TLString NewSecureSalt { get; set; }

        public TLString SecretRandom { get; set; }

        #region Additional
        public TLString CurrentSecret { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            CurrentSalt = GetObject<TLString>(bytes, ref position);
            NewSalt = GetObject<TLString>(bytes, ref position);
            NewSecureSalt = GetObject<TLString>(bytes, ref position);
            SecretRandom = GetObject<TLString>(bytes, ref position);
            Hint = GetObject<TLString>(bytes, ref position);
            EmailUnconfirmedPattern = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLPassword83 : TLPassword81
    {
        public new const uint Signature = TLConstructors.TLPassword83;

        public override bool HasPassword { get { return IsSet(_flags, (int)PasswordFlags.HasPassword); } }

        protected TLPasswordKdfAlgoBase _currentAlgo;

        public TLPasswordKdfAlgoBase CurrentAlgo
        {
            get { return _currentAlgo; }
            set { SetField(out _currentAlgo, value, ref _flags, (int)PasswordFlags.HasPassword); }
        }

        protected TLString _hint;

        public override TLString Hint
        {
            get { return _hint; }
            set { SetField(out _hint, value, ref _flags, (int)PasswordFlags.Hint); }
        }

        protected TLString _emailUnconfirmedPattern;

        public override TLString EmailUnconfirmedPattern
        {
            get { return _emailUnconfirmedPattern; }
            set { SetField(out _emailUnconfirmedPattern, value, ref _flags, (int)PasswordFlags.EmailUnconfirmedPattern); }
        }

        public TLPasswordKdfAlgoBase NewAlgo { get; set; }

        public TLSecurePasswordKdfAlgoBase NewSecureAlgo { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            _currentAlgo = GetObject<TLPasswordKdfAlgoBase>(Flags, (int) PasswordFlags.HasPassword, null, bytes, ref position);
            _hint = GetObject<TLString>(Flags, (int)PasswordFlags.Hint, null, bytes, ref position);
            _emailUnconfirmedPattern = GetObject<TLString>(Flags, (int)PasswordFlags.EmailUnconfirmedPattern, null, bytes, ref position);
            NewAlgo = GetObject<TLPasswordKdfAlgoBase>(bytes, ref position);
            NewSecureAlgo = GetObject<TLSecurePasswordKdfAlgoBase>(bytes, ref position);
            SecretRandom = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLPassword84 : TLPassword83, IPasswordSecret, IPasswordSRPParams
    {
        public new const uint Signature = TLConstructors.TLPassword84;

        public TLString SRPB { get; set; }

        public TLLong SRPId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            _currentAlgo = GetObject<TLPasswordKdfAlgoBase>(Flags, (int)PasswordFlags.HasPassword, null, bytes, ref position);
            SRPB = GetObject<TLString>(Flags, (int)PasswordFlags.HasPassword, null, bytes, ref position);
            SRPId = GetObject<TLLong>(Flags, (int)PasswordFlags.HasPassword, null, bytes, ref position);            
            _hint = GetObject<TLString>(Flags, (int)PasswordFlags.Hint, null, bytes, ref position);
            _emailUnconfirmedPattern = GetObject<TLString>(Flags, (int)PasswordFlags.EmailUnconfirmedPattern, null, bytes, ref position);
            NewAlgo = GetObject<TLPasswordKdfAlgoBase>(bytes, ref position);
            NewSecureAlgo = GetObject<TLSecurePasswordKdfAlgoBase>(bytes, ref position);
            SecretRandom = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLNoPassword : TLPasswordBase
    {
        public const uint Signature = TLConstructors.TLNoPassword;

        public override bool HasPassword { get { return false; } }

        #region Additional

        public TLPasswordSettings Settings { get; set; }

        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NewSalt = GetObject<TLString>(bytes, ref position);
            EmailUnconfirmedPattern = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLNoPassword81 : TLNoPassword
    {
        public new const uint Signature = TLConstructors.TLNoPassword81;

        public TLString NewSecureSalt { get; set; }

        public TLString SecretRandom { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            NewSalt = GetObject<TLString>(bytes, ref position);
            NewSecureSalt = GetObject<TLString>(bytes, ref position);
            SecretRandom = GetObject<TLString>(bytes, ref position);
            EmailUnconfirmedPattern = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}
