// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Runtime.Serialization;
using Telegram.Api.TL;

namespace TelegramClient.Models
{
    [DataContract]
    public class Country : TLObject
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "phoneCode")]
        public string PhoneCode { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        public string GetKey()
        {
            return Name.Substring(0, 1).ToLowerInvariant();
        } 

    }
}
