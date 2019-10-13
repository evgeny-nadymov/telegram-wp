// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace TelegramClient.ViewModels.Payments.Stripe
{
    public class DateUtils
    {
        /// <summary>
        /// Determines whether or not the input year has already passed.
        /// </summary>
        /// <param name="year">the input year, as a two or four-digit integer</param>
        /// <returns><see langword="true"> if the year has passed, <see langword="false"> otherwise.</returns>
        public static bool HasYearPassed(int year)
        {
            var normalized = NormalizeYear(year);
            return normalized < DateTime.Now.Year;
        }

        /// <summary>
        /// Determines whether the input year-month pair has passed.
        /// </summary>
        /// <param name="year">the input year, as a two or four-digit integer</param>
        /// <param name="month">the input month</param>
        /// <returns><see langword="true"> if the input time has passed, <see langword="false"> otherwise.</returns>
        public static bool HasMonthPassed(int year, int month)
        {
            if (HasYearPassed(year))
            {
                return true;
            }

            // Expires at end of specified month, Calendar month starts at 0
            return NormalizeYear(year) == DateTime.Now.Year
                    && month < (DateTime.Now.Month + 1);
        }

        // Convert two-digit year to full year if necessary
        private static int NormalizeYear(int year)
        {
            if (year < 100 && year >= 0)
            {
                var currentYear = DateTime.Now.Year.ToString();
                var prefix = currentYear.Substring(0, currentYear.Length - 2);
                year = int.Parse(String.Format("{0}{1}", prefix, year));
            }

            return year;
        }
    }
}
