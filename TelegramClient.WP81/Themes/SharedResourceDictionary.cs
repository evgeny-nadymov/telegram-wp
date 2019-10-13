// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace TelegramClient.Themes
{
    public class SharedResourceDictionary : ResourceDictionary
    {
        public static Dictionary<Uri, ResourceDictionary> _sharedDictionaries = new Dictionary<Uri, ResourceDictionary>();

        private Uri _sourceUri;

        public new Uri Source
        {
            get { return _sourceUri; }
            set
            {
                _sourceUri = value;
                if (!_sharedDictionaries.ContainsKey(value))
                {
                    Application.LoadComponent(this, value);
                    _sharedDictionaries.Add(value, this);
                }
                else
                {
                    CopyInto(this, _sharedDictionaries[value]);
                }
            }
        }

        private static void CopyInto(ResourceDictionary copy, ResourceDictionary original)
        {
            foreach (var dictionary in original.MergedDictionaries)
            {
                var mergedCopy = new ResourceDictionary();
                CopyInto(mergedCopy, dictionary);
                copy.MergedDictionaries.Add(mergedCopy);
            }
            foreach (DictionaryEntry pair in original)
            {
                copy.Add(pair.Key, pair.Value);
            }
        }
    }
}
