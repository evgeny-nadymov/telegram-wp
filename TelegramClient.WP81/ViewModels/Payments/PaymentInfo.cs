// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.TL;

namespace TelegramClient.ViewModels.Payments
{
    public class PaymentInfo
    {
        public TLMessage Message { get; set; }

        public TLPaymentForm Form { get; set; }

        public TLValidatedRequestedInfo ValidatedInfo { get; set; }

        public string CredentialsTitle { get; set; }

        public TLInputPaymentCredentialsBase Credentials { get; set; }

        public TLPaymentResultBase Result { get; set; }

        public TLPaymentReceipt Receipt { get; set; }

        public TLObject With { get; set; }
    }
}