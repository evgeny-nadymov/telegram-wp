// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Caliburn.Micro;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;

namespace TelegramClient.Models
{
    public class UsersByLastName : List<AlphaKeyGroup<TLUserBase>>
    {
        private readonly Dictionary<string, AlphaKeyGroup<TLUserBase>> _groups;

        private const string Groups = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя#abcdefghijklmnopqrstuvwxyz";

        public int SumCount
        {
            get
            {
                var count = 0;
                ForEach(x => count += x.Count);
                return count;
            }
        }

        public void AddUser(TLUserBase user)
        {
            var key = TLUserBase.GetLastNameKey(user);
            var hasItem = _groups[key].IndexOf(user) != -1;
            if (!hasItem)
            {
                _groups[key].Add(user);
                //_groups[key].Sort(TLUserBase.CompareByLastName);
            }
        }

        public void RemoveUser(TLUserBase user)
        {
            var key = TLUserBase.GetLastNameKey(user);
            _groups[key].Remove(user);
        }

        public UsersByLastName(int hintsCount = 0, bool online = false, Dictionary<int, int> excludeUids = null)
        {
            var people = IoC.Get<ICacheService>().GetContacts();
            people.Sort(TLUserBase.CompareByLastName);

            _groups = new Dictionary<string, AlphaKeyGroup<TLUserBase>>();

            //if (hintsCount > 0)
            //{
            //    var hints = people.Take(hintsCount).ToList();
            //    var hintsGroup = new UsersInGroup("hints");
            //    Add(hintsGroup);
            //    _groups["hints"] = hintsGroup;
            //    foreach (var hint in hints)
            //    {
            //        _groups["hints"].Add(hint);
            //    }
            //}

            foreach (var c in Groups)
            {
                var group = new AlphaKeyGroup<TLUserBase>(c.ToString());
                Add(group);
                _groups[c.ToString()] = group;
            }

            foreach (var person in people)
            {
                _groups[TLUserBase.GetLastNameKey(person)].Add(person);
            }

            //CacheService.Database.Storage.Commit();
        }
    }
}
