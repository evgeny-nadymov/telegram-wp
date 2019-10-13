// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Telegram.Api.TL;

namespace TelegramClient.Models
{
    public class ContactsByLastName : List<AlphaKeyGroup<TLUserBase>>
    {
        private const string Groups = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя#abcdefghijklmnopqrstuvwxyz";

        public ContactsByLastName(List<TLUserBase> contacts)
        {
            var people = contacts;
            people.Sort(TLUserBase.CompareByLastName);

            /*if (!people.Any())
            {
                people.Add(new User { FirstName = "Андрей", LastName = "Рогозов", PhoneFirstName = "Андрей", PhoneLastName = "" });
                people.Add(new User { PhoneFirstName = "Алексей", PhoneLastName = "Степанов" });
            }*/

            var groups = new Dictionary<string, AlphaKeyGroup<TLUserBase>>();

            foreach (var c in Groups)
            {
                var group = new AlphaKeyGroup<TLUserBase>(c.ToString());
                Add(group);
                groups[c.ToString()] = group;
            }

            foreach (var person in people)
            {
                groups[TLUserBase.GetLastNameKey(person)].Add(person);
            }
        }
    }
}
