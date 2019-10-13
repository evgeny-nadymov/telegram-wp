// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows.Data;
using Caliburn.Micro;
using Telegram.Api.Services;
using TelegramClient.Resources;
using TelegramClient.Utils;

namespace TelegramClient.Converters
{
    public class MuteUntilToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
            {
                return null;
            }

            var muteUntil = (int)value;

            // Enabled
            if (muteUntil == 0)
            {
                return AppResources.Enabled;
            }

            // Disabled
            if (muteUntil == int.MaxValue)
            {
                return AppResources.Disabled;
            }

            // Other
            var clientDelta = IoC.Get<IMTProtoService>().ClientTicksDelta;
            //var utc0SecsLong = muteUntil * 4294967296 - clientDelta;
            var utc0SecsInt = muteUntil - clientDelta / 4294967296.0;

            var muteUntilDateTime = Telegram.Api.Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);

            var now = DateTime.Now;
            var muteUntilTimeSpan = muteUntilDateTime - now;

            // Enabled
            if (muteUntilDateTime < now)
            {
                return AppResources.Enabled;
            }

            // Up to 1 hour
            var totalMinutes = (int)Math.Ceiling(muteUntilTimeSpan.TotalMinutes);
            if (totalMinutes < 60)
            {
                var minutes = Language.Declension(
                    totalMinutes == 0 ? 1 : totalMinutes,
                    AppResources.MinuteAccusative,
                    null,
                    AppResources.MinuteGenitiveSingular,
                    AppResources.MinuteGenitivePlural,
                    totalMinutes < 2
                        ? string.Format("{1} {0}", AppResources.MinuteNominativeSingular, 1).ToLowerInvariant()
                        : string.Format("{1} {0}", AppResources.MinuteNominativePlural, Math.Abs(totalMinutes))).ToLowerInvariant();

                return string.Format(AppResources.UnmuteIn, minutes);
            }

            // Up to 1 day
            var totalHours = (int)Math.Ceiling(muteUntilTimeSpan.TotalHours);
            if (totalHours < 24)
            {
                var hours = Language.Declension(
                    totalHours == 0 ? 1 : totalHours,
                    AppResources.HourNominativeSingular,
                    null,
                    AppResources.HourGenitiveSingular,
                    AppResources.HourGenitivePlural,
                    totalHours < 2
                        ? string.Format("{1} {0}", AppResources.HourNominativeSingular, 1).ToLowerInvariant()
                        : string.Format("{1} {0}", AppResources.HourNominativePlural, Math.Abs(totalHours))).ToLowerInvariant();

                return string.Format(AppResources.UnmuteIn, hours);
            }

            // Other
            var totalDays = (int)Math.Ceiling(muteUntilTimeSpan.TotalDays);

            if (totalDays >= 365)
            {
                return AppResources.Disabled;
            }

            var days = Language.Declension(
                    totalDays == 0 ? 1 : totalDays,
                    AppResources.DayNominativeSingular,
                    null,
                    AppResources.DayGenitiveSingular,
                    AppResources.DayGenitivePlural,
                    totalDays < 2
                        ? string.Format("{1} {0}", AppResources.DayNominativeSingular, 1).ToLowerInvariant()
                        : string.Format("{1} {0}", AppResources.DayNominativePlural, Math.Abs(totalDays))).ToLowerInvariant();

            return string.Format(AppResources.UnmuteIn, days);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MuteUntilToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
            {
                return null;
            }

            var muteUntil = (int)value;

            // Enabled
            if (muteUntil == 0)
            {
                return true;
            }

            // Disabled
            if (muteUntil == int.MaxValue)
            {
                return false;
            }

            // Other
            var clientDelta = IoC.Get<IMTProtoService>().ClientTicksDelta;
            //var utc0SecsLong = muteUntil * 4294967296 - clientDelta;
            var utc0SecsInt = muteUntil - clientDelta / 4294967296.0;

            var muteUntilDateTime = Telegram.Api.Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);
            var now = DateTime.Now;

            // Enabled
            if (muteUntilDateTime < now)
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
