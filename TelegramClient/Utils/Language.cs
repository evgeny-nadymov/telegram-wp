// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TelegramClient.Utils
{
    public enum Gender
    {
        Female,
        Male
    }

    public static class Language
    {
        public static string CapitalizeFirstLetter(string data)
        {
            var chars = data.ToCharArray();

            // Find the Index of the first letter
            var charac = data.FirstOrDefault(char.IsLetter);
            if (charac == default(char)) return data;
            var i = data.IndexOf(charac);

            // capitalize that letter
            chars[i] = char.ToUpper(chars[i]);

            return new string(chars);
        }

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

        /// <summary>
        /// Does a word declension after a number.
        /// </summary>
        /// <param name="number">           Number </param>
        /// <param name="nominative">       Nominative (собеседник, участник)  </param>
        /// <param name="genitiveSingular"> Genitive singular (собеседника, участника) </param>
        /// <param name="genitivePlural">   Genitive plural (собеседников, участников) </param>
        /// <returns></returns>   
        public static string RuDeclension(int number, string nominative, string genitiveSingular, string genitivePlural, string format)
        {
            var lastDigit = number%10;
            var lastTwoDigits = number%100;

            if (lastDigit == 1 && lastTwoDigits != 11)
            {
                return string.Format("{0} {1}", number, nominative);
            }

            if (lastDigit == 2 && lastTwoDigits != 12 || lastDigit == 3 && lastTwoDigits != 13 ||
                lastDigit == 4 && lastTwoDigits != 14)
            {
                return string.Format("{2}{0}{2} {1}", number, genitiveSingular, format);
            }

            return string.Format("{2}{0}{2} {1}", number, genitivePlural, format);
        }

        /// <summary>
        /// Does a word declension after a number.
        /// </summary>
        /// <param name="number">      Number </param>
        /// <param name="singular">    Singular (company) </param>
        /// <param name="plural">      Plural (companies) </param>
        /// <returns></returns>   
        public static string EnDeclension(int number, string singular, string plural, string format)
        {
            return string.Format("{2}{0}{2} {1}", number, number == 1 || number == 0 ? singular : plural, format);
        }

        public static string Declension(int number, string nominativeSingular, string nominativePlural, string genitiveSingular, string genitivePlural, string enException = null, string format = null)
        {
            var culture = CultureInfo.CurrentUICulture;
            var languageName = culture.IsNeutralCulture ? culture.EnglishName : culture.Parent.EnglishName;
            if (string.Equals(languageName, "Russian", StringComparison.OrdinalIgnoreCase))
            {
                return RuDeclension(number, nominativeSingular, genitiveSingular, genitivePlural, format);
            }

            if (!string.IsNullOrEmpty(enException)) return enException;

            return EnDeclension(number, nominativeSingular, nominativePlural, format);
        }

        /// <summary>
        /// Does a word declension after a number.
        /// </summary>
        /// <param name="number">           Number </param>
        /// <param name="nominative">       Nominative (собеседник, участник)  </param>
        /// <param name="genitiveSingular"> Genitive singular (собеседника, участника) </param>
        /// <param name="genitivePlural">   Genitive plural (собеседников, участников) </param>
        /// <returns></returns>   
        public static string RuDeclension2(int number, string nominative, string genitiveSingular, string genitivePlural)
        {
            var lastDigit = number % 10;
            var lastTwoDigits = number % 100;

            if (lastDigit == 1 && lastTwoDigits != 11)
            {
                return string.Format("{0}", nominative);
            }

            if (lastDigit == 2 && lastTwoDigits != 12 || lastDigit == 3 && lastTwoDigits != 13 ||
                lastDigit == 4 && lastTwoDigits != 14)
            {
                return string.Format("{0}", genitiveSingular);
            }

            return string.Format("{0}", genitivePlural);
        }

        /// <summary>
        /// Does a word declension after a number.
        /// </summary>
        /// <param name="number">      Number </param>
        /// <param name="singular">    Singular (company) </param>
        /// <param name="plural">      Plural (companies) </param>
        /// <returns></returns>   
        public static string EnDeclension2(int number, string singular, string plural)
        {
            return string.Format("{0}", number == 1 || number == 0 ? singular : plural);
        }

        public static string Declension2(int number, string nominativeSingular, string nominativePlural, string genitiveSingular, string genitivePlural, string enException = null)
        {
            var culture = CultureInfo.CurrentUICulture;
            var languageName = culture.IsNeutralCulture ? culture.EnglishName : culture.Parent.EnglishName;
            if (string.Equals(languageName, "Russian", StringComparison.OrdinalIgnoreCase))
            {
                return RuDeclension2(number, nominativeSingular, genitiveSingular, genitivePlural);
            }

            if (!string.IsNullOrEmpty(enException)) return enException;

            return EnDeclension2(number, nominativeSingular, nominativePlural);
        }

        public static string GenderString(Gender gender, string maleString, string femaleString)
        {
            return gender == Gender.Male ? maleString : femaleString;
        }

        public static void Test()
        {
            for (var i = 0; i < 105; i++)
            {
                Debug.WriteLine(RuDeclension(i, "собеседник", "собеседника", "собеседников", null));
            }

            Console.ReadLine();
        }
    }
}
