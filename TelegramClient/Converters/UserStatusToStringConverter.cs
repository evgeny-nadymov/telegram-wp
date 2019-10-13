// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Utils;

namespace TelegramClient.Converters
{
    public class UserStatusToStringConverter : IValueConverter
    {
        public static string GetShortTimePattern(ref CultureInfo ci)
        {
            if (ci.DateTimeFormat.ShortTimePattern.Contains("H"))
            {
                return "H:mm";
            }

            ci.DateTimeFormat.AMDesignator = "am";
            ci.DateTimeFormat.PMDesignator = "pm";
            return "h:mmt";
        }

        public static string Convert(TLUserStatus status)
        {
            if (status == null)
            {
                return AppResources.LastSeenLongTimeAgo;
            }

            if (!(status is TLUserStatusEmpty))
            {
                if (status is TLUserStatusOnline)
                {
                    return LowercaseConverter.Convert(AppResources.Online);
                }

                if (status is TLUserStatusRecently)
                {
                    return LowercaseConverter.Convert(AppResources.LastSeenRecently);
                }

                if (status is TLUserStatusLastMonth)
                {
                    return LowercaseConverter.Convert(AppResources.LastSeenWithinMonth);
                }

                if (status is TLUserStatusLastWeek)
                {
                    return LowercaseConverter.Convert(AppResources.LastSeenWithinWeek);
                }

                if (status is TLUserStatusOffline)
                {
                    var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                    var shortTimePattern = GetShortTimePattern(ref cultureInfo);

                    var clientDelta = IoC.Get<IMTProtoService>().ClientTicksDelta;
                    //var utc0SecsLong = (((TLUserStatusOffline)status).WasOnline).Value * 4294967296 - clientDelta;
                    var utc0SecsInt = ((TLUserStatusOffline)status).WasOnline.Value - clientDelta / 4294967296.0;

                    //var utc0SecsInt = (((TLUserStatusOffline) status).WasOnline).Value;
                    var lastSeen = Telegram.Api.Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);

                    var lastSeenTimeSpan = DateTime.Now - lastSeen;

                    // Just now
                    if (lastSeenTimeSpan.TotalMinutes <= 1)
                    {
                        return AppResources.LastSeenJustNow.ToLowerInvariant();
                    }

                    // Up to one hour
                    if (lastSeenTimeSpan < TimeSpan.FromMinutes(60.0))
                    {
                        var minutes = Language.Declension(
                            lastSeenTimeSpan.Minutes == 0 ? 1 : lastSeenTimeSpan.Minutes,
                            AppResources.MinuteAccusative,
                            null,
                            AppResources.MinuteGenitiveSingular,
                            AppResources.MinuteGenitivePlural,
                            lastSeenTimeSpan.Minutes < 2
                                ? string.Format("{1} {0}", AppResources.MinuteNominativeSingular, 1).ToLowerInvariant()
                                : string.Format("{1} {0}", AppResources.MinuteNominativePlural, Math.Abs(lastSeenTimeSpan.Minutes))).ToLowerInvariant();

                        return string.Format(AppResources.LastSeen, minutes).ToLowerInvariant();
                    }

                    // Today
                    if (lastSeen.Date == DateTime.Now.Date)
                    {
                        return string.Format(
                            AppResources.LastSeenTodayAt.ToLowerInvariant(),
                            new DateTime(lastSeen.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo));
                    }

                    // Yesterday
                    if (lastSeen.Date.AddDays(1.0) == DateTime.Now.Date)
                    {
                        return string.Format(
                            AppResources.LastSeenYesterdayAt.ToLowerInvariant(),
                            new DateTime(lastSeen.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo));
                    }

                    // this year
                    if (lastSeen.Date.AddDays(365) >= DateTime.Now.Date)
                    {
                        return string.Format(
                            AppResources.LastSeenAtDate.ToLowerInvariant(),
                            lastSeen.ToString(AppResources.UserStatusDayFormat, cultureInfo),
                            new DateTime(lastSeen.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo));
                    }

                    return string.Format(
                        AppResources.LastSeenAtDate.ToLowerInvariant(),
                        lastSeen.ToString(AppResources.UserStatusYearDayFormat, cultureInfo),
                        new DateTime(lastSeen.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo));
                }
                
            }

            return LowercaseConverter.Convert(AppResources.LastSeenLongTimeAgo);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userBase = value as TLUserBase;
            if (userBase == null) return null;

            var user = userBase as TLUser;
            if (user != null)
            {
                if (user.IsBot)
                {
                    if (user.IsBotAllHistory)
                    {
                        return AppResources.SeesAllMessages.ToLowerInvariant();
                    }

                    return AppResources.OnlySeesMessagesStartingWithSlash.ToLowerInvariant();
                }

                //if (user.IsSelf)
                //{
                //    return AppResources.ChatWithYourself.ToLowerInvariant();
                //}
            }

            var status = userBase.Status;
            if (status == null) return LowercaseConverter.Convert(AppResources.LastSeenLongTimeAgo);

            return Convert(status);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EditDateToStringConverter : IValueConverter
    {
        public static string GetShortTimePattern(ref CultureInfo ci)
        {
            if (ci.DateTimeFormat.ShortTimePattern.Contains("H"))
            {
                return "H:mm";
            }

            ci.DateTimeFormat.AMDesignator = "am";
            ci.DateTimeFormat.PMDesignator = "pm";
            return "h:mmt";
        }

        public static string Convert(TLInt editDate)
        {
            if (editDate == null || editDate.Value == 0)
            {
                return AppResources.UpdatedJustNow.ToLowerInvariant();
            }

            var cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            var shortTimePattern = GetShortTimePattern(ref cultureInfo);

            var clientDelta = IoC.Get<IMTProtoService>().ClientTicksDelta;
            //var utc0SecsLong = editDate.Value * 4294967296 - clientDelta;
            var utc0SecsInt = editDate.Value - clientDelta / 4294967296.0;

            //var utc0SecsInt = (((TLUserStatusOffline) status).WasOnline).Value;
            var editDateTime = Telegram.Api.Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);

            var editTimeSpan = DateTime.Now - editDateTime;

            // Just now
            if (editTimeSpan.TotalMinutes <= 1)
            {
                return AppResources.UpdatedJustNow.ToLowerInvariant();
            }

            // Up to one hour
            if (editTimeSpan < TimeSpan.FromMinutes(60.0))
            {
                var minutes = Language.Declension(
                    editTimeSpan.Minutes == 0 ? 1 : editTimeSpan.Minutes,
                    AppResources.MinuteAccusative,
                    null,
                    AppResources.MinuteGenitiveSingular,
                    AppResources.MinuteGenitivePlural,
                    editTimeSpan.Minutes < 2
                        ? string.Format("{1} {0}", AppResources.MinuteNominativeSingular, 1).ToLowerInvariant()
                        : string.Format("{1} {0}", AppResources.MinuteNominativePlural, Math.Abs(editTimeSpan.Minutes))).ToLowerInvariant();

                return string.Format(AppResources.UpdatedAgo, minutes).ToLowerInvariant();
            }

            // Today
            if (editDateTime.Date == DateTime.Now.Date)
            {
                return string.Format(
                    AppResources.UpdatedTodayAt,
                    new DateTime(editDateTime.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo)).ToLowerInvariant();
            }

            // Yesterday
            if (editDateTime.Date.AddDays(1.0) == DateTime.Now.Date)
            {
                return string.Format(
                    AppResources.UpdatedYesterdayAt,
                    new DateTime(editDateTime.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo)).ToLowerInvariant();
            }

            // this year
            if (editDateTime.Date.AddDays(365) >= DateTime.Now.Date)
            {
                return string.Format(
                    AppResources.UpdatedAtDate,
                    editDateTime.ToString(AppResources.UserStatusDayFormat, cultureInfo),
                    new DateTime(editDateTime.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo)).ToLowerInvariant();
            }

            return string.Format(
                AppResources.UpdatedAtDate,
                editDateTime.ToString(AppResources.UserStatusYearDayFormat, cultureInfo),
                new DateTime(editDateTime.TimeOfDay.Ticks).ToString(shortTimePattern, cultureInfo)).ToLowerInvariant();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value as TLInt);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SearchResultStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userBase = value as TLUserBase;
            if (userBase != null)
            {
                var user = userBase as TLUser;
                if (user != null)
                {
                    if (user.IsBot)
                    {
                        return AppResources.Bot.ToLowerInvariant();
                    }
                }

                var status = userBase.Status;
                if (status == null) return LowercaseConverter.Convert(AppResources.LastSeenLongTimeAgo);

                return UserStatusToStringConverter.Convert(status);
            }

            var channel = value as TLChannel;
            if (channel != null)
            {
                return null;
            }

            var chat = value as TLChat;
            if (chat != null)
            {
                var participantsCount = chat.ParticipantsCount.Value;

                return Language.Declension(
                    participantsCount,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViaBotToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userBase = value as IUserName;
            if (userBase == null) return null;

            return string.Format(AppResources.Via.ToLowerInvariant(), "@" + userBase.UserName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DecryptedViaBotToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viaBot = value as TLString;
            if (viaBot == null) return null;

            return string.Format(AppResources.Via, "@" + viaBot).ToLowerInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
