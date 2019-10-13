// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Text;

namespace Telegram.Controls.Utils
{
    public static class Language
    {
        private static readonly Dictionary<char, string> _ruEnTable = new Dictionary<char, string>
        {
            {'а', "a"}, {'б', "b"}, 
            {'в', "v"}, {'г', "g"}, 
            {'д', "d"}, {'е', "e"},
            {'ё', "e"}, {'ж', "zh"}, 
            {'з', "z"}, {'и', "i"}, 
            {'й', "j"}, {'к', "k"},
            {'л', "l"}, {'м', "m"}, 
            {'н', "n"}, {'о', "o"}, 
            {'п', "p"}, {'р', "r"},
            {'с', "s"}, {'т', "t"}, 
            {'у', "u"}, {'ф', "f"}, 
            {'х', "kh"}, {'ц', "tc"},
            {'ч', "ch"}, {'ш', "sh"}, 
            {'щ', "shch"}, {'ъ', ""}, 
            {'ы', "y"}, {'ь', ""},
            {'э', "e"}, {'ю', "iu"}, 
            {'я', "ia"}
        };

        private static readonly Dictionary<char, string> _enRuTable = new Dictionary<char, string>
        {
            {'a', "а"}, {'b', "б"}, 
            {'c', "ц"}, {'d', "д"}, 
            {'e', "е"}, {'f', "ф"},
            {'g', "г"}, {'h', "х"}, 
            {'i', "и"}, {'j', "й"}, 
            {'k', "к"}, {'l', "л"},
            {'m', "м"}, {'n', "н"}, 
            {'o', "о"}, {'p', "п"}, 
            {'q', "к"}, {'r', "р"},
            {'s', "с"}, {'t', "т"}, 
            {'u', "ю"}, {'v', "в"}, 
            {'w', "в"}, {'x', "х"},
            {'y', "й"}, {'z', "з"}
        };

        public static string Transliterate(string str)
        {
            var enCount = 0;
            var ruCount = 0;
            var count = 0;
            const int maxCount = 7;
            foreach (var alpha in str)
            {
                if (count > maxCount) break;
                if (_enRuTable.ContainsKey(alpha))
                {
                    enCount++;
                }
                else if (_ruEnTable.ContainsKey(alpha))
                {
                    ruCount++;
                }
                count++;
            }

            if (enCount > ruCount)
            {
                return TransliterateToRussian(str);
            }

            return TransliterateToEnglish(str);
        }

        public static string TransliterateToRussian(string str)
        {
            var enStr = new StringBuilder();
            foreach (var alpha in str)
            {
                if (_enRuTable.ContainsKey(alpha))
                {
                    enStr.Append(_enRuTable[alpha]);
                }
            }

            return enStr.ToString();
        }

        public static string TransliterateToEnglish(string str)
        {
            var enStr = new StringBuilder();
            foreach (var alpha in str)
            {
                if (_ruEnTable.ContainsKey(alpha))
                {
                    enStr.Append(_ruEnTable[alpha]);
                }
            }

            return enStr.ToString();
        }
    }
}
