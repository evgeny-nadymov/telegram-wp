// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace TelegramClient.ViewModels.Payments.Stripe.JSON
{
    public class Error
    {
        public string message { get; set; }
        public string type { get; set; }
        public string param { get; set; }
        public string code { get; set; }
    }

    public class RootObject
    {
        public Error error { get; set; }
        public string id { get; set; }
        public string @object { get; set; }
        public Card card { get; set; }
        public string client_ip { get; set; }
        public int created { get; set; }
        public bool livemode { get; set; }
        public string type { get; set; }
        public bool used { get; set; }
    }

    public class Metadata
    {
    }

    public class Card
    {
        public string id { get; set; }
        public string @object { get; set; }
        public object address_city { get; set; }
        public string address_country { get; set; }
        public object address_line1 { get; set; }
        public object address_line1_check { get; set; }
        public object address_line2 { get; set; }
        public object address_state { get; set; }
        public string address_zip { get; set; }
        public string address_zip_check { get; set; }
        public string brand { get; set; }
        public string country { get; set; }
        public string cvc_check { get; set; }
        public object dynamic_last4 { get; set; }
        public int exp_month { get; set; }
        public int exp_year { get; set; }
        public string funding { get; set; }
        public string last4 { get; set; }
        public Metadata metadata { get; set; }
        public string name { get; set; }
        public object tokenization_method { get; set; }
    }
}
