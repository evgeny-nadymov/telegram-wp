// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;

namespace Telegram.Controls.Utils
{
    public static class Currency
    {
        public static int GetPow(string currency)
        {
            if (currency == "CLF")
            {
                return 4;
            }

            string[] cur3 =
            {
                "BHD", "IQD", "JOD", "KWD", "LYD", "OMR", "TND"
            };
            if (cur3.Contains(currency))
            {
                return 3;
            }

            string[] cur0 =
            {
                "BIF", "BYR", "CLP", "CVE", "DJF", "GNF", "ISK", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "UYI", "VND", "VUV", "XAF", "XOF", "XPF"
            };
            if (cur0.Contains(currency)) 
            {
                return 0;
            }

            if (currency == "MRO") 
            {
                return 1;
            }

            return 2;
        }

        private static Dictionary<string, string> _dict = new Dictionary<string, string>
            {
                {"AED", "د.إ"},
                {"AFN", "؋"},
                {"ARS", "$"},
                {"AUD", "$"},
                {"AZN", "₼"},
                {"BND", "B$"},
                {"BRL", "R$"},
                {"CAD", "$"},
                {"CHF", "Fr"},
                {"CLP", "$"},
                {"CNY", "¥"},
                {"COP", "$"},
                {"EGP", "E£"},
                {"EUR", "€"},
                {"GBP", "£"},
                {"HKD", "$"},
                {"IDR", "Rp"},
                {"ILS", "₪"},
                {"INR", "₹"},
                {"ISK", "kr"},
                {"JPY", "¥"},
                {"KRW", "₩"},
                {"KZT", "₸"},
                {"MXN", "$"},
                {"MYR", "RM"},
                {"NOK", "kr"},
                {"NZD", "$"},
                {"PHP", "₱"},
                {"RUB", "₽"},
                {"SAR", "SR"},
                {"SEK", "kr"},
                {"SGD", "$"},
                {"TRY", "₺"},
                {"TTD", "$"},
                {"TWD", "$"},
                {"TZS", "TSh"},
                {"UAH", "₴"},
                {"UGX", "USh"},
                {"USD", "$"},
                {"UYU", "$"},
                {"VND", "₫"},
                {"YER", "﷼"},
                {"ZAR", "R"},
                {"IRR", "﷼"},
                {"IQD", "ع.د"},
                {"VEF", "Bs.F."}
            };

        public static string GetSymbol(string currency)
        {
            string symbol;
            if (_dict.TryGetValue(currency, out symbol))
            {
                return symbol;
            }

            return currency;
        }

        public static string GetString(long totalAmount, string currency)
        {
            return string.Format("{0:0.00} {1}", totalAmount / Math.Pow(10.0, GetPow(currency)), GetSymbol(currency));
        }
    }
}
