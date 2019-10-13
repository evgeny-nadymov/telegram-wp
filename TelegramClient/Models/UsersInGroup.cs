using System.Collections.Generic;
using Telegram.Api.TL;

namespace TelegramClient.Models
{
    public class UsersInGroup : List<TLUserBase>
    {
        public UsersInGroup(string category)
        {
            Key = category;
        }

        public string Key { get; set; }

        public bool HasItems { get { return Count > 0; } }
    }
}
