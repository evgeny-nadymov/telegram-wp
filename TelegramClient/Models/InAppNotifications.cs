// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Runtime.Serialization;

namespace TelegramClient.Models
{
    [DataContract]
    public class InAppNotifications
    {
        [DataMember(Name = "V")]
        public bool InAppVibration { get; set; }

        [DataMember(Name = "S")]
        public bool InAppSound { get; set; }

        [DataMember(Name = "P")]
        public bool InAppMessagePreview { get; set; }
    }
}
