// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using TelegramClient.ViewModels.Payments.Stripe.JSON;

namespace TelegramClient.ViewModels.Payments.Stripe
{
    public class StripeToken
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public Error Error { get; set; }

        public string Content { get; set; }
    }
}