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
    public enum PaymentFormFlags
    {
        SavedInfo = 0x1,            // 0
        SavedCredentials = 0x2,     // 1
        CanSaveCredentials = 0x4,   // 2
        PasswordMissing = 0x8,      // 3
        Native = 0x10,              // 4
    }

    public class TLPaymentForm : TLObject
    {
        public const uint Signature = TLConstructors.TLPaymentForm;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool CanSaveCredentials
        {
            get { return IsSet(Flags, (int) PaymentFormFlags.CanSaveCredentials); }
            set { SetUnset(ref _flags, value, (int) PaymentFormFlags.CanSaveCredentials); }
        }

        public bool PasswordMissing
        {
            get { return IsSet(Flags, (int) PaymentFormFlags.PasswordMissing); }
            set { SetUnset(ref _flags, value, (int) PaymentFormFlags.PasswordMissing); }
        }

        public TLInt BotId { get; set; }

        public TLInvoice Invoice { get; set; }

        public TLInt ProviderId { get; set; }

        public TLString Url { get; set; }

        private TLString _nativeProvider;

        public TLString NativeProvider
        {
            get { return _nativeProvider; }
            set { SetField(out _nativeProvider, value, ref _flags, (int) PaymentFormFlags.Native); }
        }

        private TLDataJSON _nativeParams;

        public TLDataJSON NativeParams
        {
            get { return _nativeParams; }
            set { SetField(out _nativeParams, value, ref _flags, (int) PaymentFormFlags.Native); }
        }

        private TLPaymentRequestedInfo _savedInfo;

        public TLPaymentRequestedInfo SavedInfo
        {
            get { return _savedInfo; }
            set { SetField(out _savedInfo, value, ref _flags, (int) PaymentFormFlags.SavedInfo); }
        }

        private TLPaymentSavedCredentials _savedCredentials;

        public TLPaymentSavedCredentials SavedCredentials
        {
            get { return _savedCredentials; }
            set { SetField(out _savedCredentials, value, ref _flags, (int) PaymentFormFlags.SavedCredentials); }
        }

        public TLVector<TLUserBase> Users { get; set; }

        public bool IsNativeProvider
        {
            get
            {
                if (TLString.Equals(NativeProvider, new TLString("stripe"), StringComparison.Ordinal)
                    && NativeParams != null)
                {
                    return true;
                }

                return false;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            BotId = GetObject<TLInt>(bytes, ref position);
            Invoice = GetObject<TLInvoice>(bytes, ref position);
            ProviderId = GetObject<TLInt>(bytes, ref position);
            Url = GetObject<TLString>(bytes, ref position);
            _nativeProvider = GetObject<TLString>(Flags, (int) PaymentFormFlags.Native, null, bytes, ref position);
            _nativeParams = GetObject<TLDataJSON>(Flags, (int) PaymentFormFlags.Native, null, bytes, ref position);
            _savedInfo = GetObject<TLPaymentRequestedInfo>(Flags, (int) PaymentFormFlags.SavedInfo, null, bytes, ref position);
            _savedCredentials = GetObject<TLPaymentSavedCredentials>(Flags, (int) PaymentFormFlags.SavedCredentials, null, bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }
    }
}
