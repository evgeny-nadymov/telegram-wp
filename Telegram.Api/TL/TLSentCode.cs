// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Text;

namespace Telegram.Api.TL
{
    [Flags]
    public enum SentCodeFlags
    {
        PhoneRegistered = 0x1,      // 0
        NextType = 0x2,             // 1
        Timeout = 0x4,              // 2
        TermsOfService = 0x8,       // 3
    }

    public abstract class TLSentCodeBase : TLObject
    {
        public virtual TLBool PhoneRegistered { get; set; }

        public TLString PhoneCodeHash { get; set; }

        public TLInt SendCallTimeout { get; set; }

        public TLBool IsPassword { get; set; }
    }

    public class TLSentCode80 : TLSentCode50
    {
        public new const uint Signature = TLConstructors.TLSentCode80;

        protected TLTermsOfServiceBase _termsOfService;

        public TLTermsOfServiceBase TermsOfService
        {
            get { return _termsOfService; }
            set { SetField(out _termsOfService, value, ref _flags, (int)SentCodeFlags.TermsOfService); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Type = GetObject<TLSentCodeTypeBase>(bytes, ref position);
            PhoneCodeHash = GetObject<TLString>(bytes, ref position);
            NextType = GetObject<TLCodeTypeBase>(Flags, (int)SentCodeFlags.NextType, null, bytes, ref position);
            SendCallTimeout = GetObject<TLInt>(Flags, (int)SentCodeFlags.Timeout, null, bytes, ref position);
            _termsOfService = GetObject<TLTermsOfServiceBase>(Flags, (int)SentCodeFlags.TermsOfService, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("SentCode80");
            sb.AppendLine(string.Format("PhoneRegistered " + PhoneRegistered));
            sb.AppendLine(string.Format("Type " + Type));
            sb.AppendLine(string.Format("PhoneCodeHash " + PhoneCodeHash));
            sb.AppendLine(string.Format("NextType " + NextType));
            sb.AppendLine(string.Format("SendCallTimeout " + SendCallTimeout));
            sb.AppendLine(string.Format("TermsOfService " + TermsOfService));

            return sb.ToString();
        }
    }

    public class TLSentCode50 : TLSentCodeBase
    {
        public const uint Signature = TLConstructors.TLSentCode50;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool PhoneRegistered
        {
            get { return new TLBool(IsSet(Flags, (int)SentCodeFlags.PhoneRegistered)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)SentCodeFlags.PhoneRegistered);
                }
            }
        }

        public TLSentCodeTypeBase Type { get; set; }

        public TLCodeTypeBase NextType { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Type = GetObject<TLSentCodeTypeBase>(bytes, ref position);
            PhoneCodeHash = GetObject<TLString>(bytes, ref position);
            NextType = GetObject<TLCodeTypeBase>(Flags, (int)SentCodeFlags.NextType, null, bytes, ref position);
            SendCallTimeout = GetObject<TLInt>(Flags, (int)SentCodeFlags.Timeout, null, bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("SentCode50");
            sb.AppendLine(string.Format("PhoneRegistered " + PhoneRegistered));
            sb.AppendLine(string.Format("Type " + Type));
            sb.AppendLine(string.Format("PhoneCodeHash " + PhoneCodeHash));
            sb.AppendLine(string.Format("NextType " + NextType));
            sb.AppendLine(string.Format("SendCallTimeout " + SendCallTimeout));

            return sb.ToString();
        }
    }

    public class TLSentCode : TLSentCodeBase
    {
        public const uint Signature = TLConstructors.TLSentCode;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneRegistered = GetObject<TLBool>(bytes, ref position);
            PhoneCodeHash = GetObject<TLString>(bytes, ref position);
            SendCallTimeout = GetObject<TLInt>(bytes, ref position);
            IsPassword = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneRegistered.ToBytes(),
                PhoneCodeHash.ToBytes(),
                SendCallTimeout.ToBytes(),
                IsPassword.ToBytes());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("SentCode");
            sb.AppendLine(string.Format("PhoneRegistered " + PhoneRegistered));
            sb.AppendLine(string.Format("PhoneCodeHash " + PhoneCodeHash));
            sb.AppendLine(string.Format("SendCallTimeout " + SendCallTimeout));
            sb.AppendLine(string.Format("IsPassword " + IsPassword));

            return sb.ToString();
        }
    }

    public class TLSentAppCode : TLSentCodeBase
    {
        public const uint Signature = TLConstructors.TLSentAppCode;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneRegistered = GetObject<TLBool>(bytes, ref position);
            PhoneCodeHash = GetObject<TLString>(bytes, ref position);
            SendCallTimeout = GetObject<TLInt>(bytes, ref position);
            IsPassword = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneRegistered.ToBytes(),
                PhoneCodeHash.ToBytes(),
                SendCallTimeout.ToBytes(),
                IsPassword.ToBytes());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("SentAppCode");
            sb.AppendLine(string.Format("PhoneRegistered " + PhoneRegistered));
            sb.AppendLine(string.Format("PhoneCodeHash " + PhoneCodeHash));
            sb.AppendLine(string.Format("SendCallTimeout " + SendCallTimeout));
            sb.AppendLine(string.Format("IsPassword " + IsPassword));

            return sb.ToString();
        }
    }
}
