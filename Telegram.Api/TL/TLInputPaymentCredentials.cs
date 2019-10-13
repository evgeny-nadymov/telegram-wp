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
    public enum InputPaymentCredentials
    {
        Save = 0x1,            // 0
    }

    public abstract class TLInputPaymentCredentialsBase : TLObject { }

    public class TLInputPaymentCredentialsSaved : TLInputPaymentCredentialsBase
    {
        public const uint Signature = TLConstructors.TLInputPaymentCredentialsSaved;

        public TLString Id { get; set; }

        public TLString TmpPassword { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                TmpPassword.ToBytes());
        }
    }

    public class TLInputPaymentCredentials : TLInputPaymentCredentialsBase
    {
        public const uint Signature = TLConstructors.TLInputPaymentCredentials;
        
        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Save
        {
            get { return IsSet(Flags, (int) InputPaymentCredentials.Save); }
            set { SetUnset(ref _flags, value, (int) InputPaymentCredentials.Save); }
        }

        public TLDataJSON Data { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Data.ToBytes());
        }
    }

    public class TLInputPaymentCredentialsApplePay : TLInputPaymentCredentialsBase
    {
        public const uint Signature = TLConstructors.TLInputPaymentCredentialsApplePay;

        public TLDataJSON PaymentData { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PaymentData.ToBytes());
        }
    }

    public class TLInputPaymentCredentialsAndroidPay : TLInputPaymentCredentialsBase
    {
        public const uint Signature = TLConstructors.TLInputPaymentCredentialsAndroidPay;

        public TLDataJSON PaymentToken { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PaymentToken.ToBytes());
        }
    }

    public class TLInputPaymentCredentialsAndroidPay74 : TLInputPaymentCredentialsAndroidPay
    {
        public new const uint Signature = TLConstructors.TLInputPaymentCredentialsAndroidPay74;

        public TLString GoogleTransactionId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PaymentToken.ToBytes(),
                GoogleTransactionId.ToBytes());
        }
    }
}