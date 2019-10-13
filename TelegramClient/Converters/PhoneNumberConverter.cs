// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
//#if WP8
//using PhoneNumbers;
//#endif
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Utils;

namespace TelegramClient.Converters
{
    public class SimplePhoneNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var phone = value as TLString;
            if (phone == null) return value;

            var phoneString = phone.ToString();

            return phoneString.StartsWith("+") ? phoneString : "+" + phoneString ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PhoneNumberConverter : IValueConverter
    {
        public static string Convert(TLString phone)
        {
//#if WP8
//            var phoneUtil = PhoneNumberUtil.GetInstance();
//            try
//            {
//                return phoneUtil.Format(phoneUtil.Parse("+" + phone.Value, ""), PhoneNumberFormat.INTERNATIONAL).Replace('-', ' ');
//            }
//            catch (Exception e)
//            {
//                return "+" + phone.Value;
//            }

//#endif
            return "+" + phone.Value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var phone = value as TLString;
            if (phone == null) return value;

            return Convert(phone);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PhoneCallToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageService = value as TLMessageService;
            if (messageService != null)
            {
                var actionPhoneCall = messageService.Action as TLMessageActionPhoneCall;
                if (actionPhoneCall != null)
                {
                    var reason = actionPhoneCall.Reason;
                    var duration = actionPhoneCall.Duration;
                    if (duration != null)
                    {
                        return messageService.Out.Value
                            ? AppResources.OutgoingCall
                            : AppResources.IncomingCall;
                    }

                    var missed = reason as TLPhoneCallDiscardReasonMissed;
                    if (missed != null)
                    {
                        if (messageService.Out.Value)
                        {
                            return AppResources.CanceledCall;
                        }
                        else
                        {
                            return AppResources.MissedCall;
                        }
                    }

                    var busy = reason as TLPhoneCallDiscardReasonBusy;
                    if (busy != null)
                    {
                        if (messageService.Out.Value)
                        {
                            return AppResources.OutgoingCall;
                        }
                        else
                        {
                            return AppResources.DeclinedCall;
                        }
                    }

                    return messageService.Out.Value
                        ? AppResources.OutgoingCall
                        : AppResources.IncomingCall;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PhoneCallToSubtitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageService = value as TLMessageService;
            if (messageService != null)
            {
                var actionPhoneCall = messageService.Action as TLMessageActionPhoneCall;
                if (actionPhoneCall != null)
                {
                    var duration = actionPhoneCall.Duration;
                    var messageDateTimeConverter = (TLIntToDateTimeConverter)Application.Current.Resources["MessageDateTimeConverter"];
                    var timeString = messageDateTimeConverter.Convert(messageService.Date, null, null, null);
                    var durationString = string.Empty;
                    if (duration != null)
                    {
                        var durationTimeSpan = TimeSpan.FromSeconds(duration.Value);
                        if (durationTimeSpan.TotalSeconds > 60.0)
                        {
                            durationString =
                                Language.Declension(
                                    (int)durationTimeSpan.TotalMinutes,
                                    AppResources.MinuteNominativeSingular,
                                    AppResources.MinuteNominativePlural,
                                    AppResources.MinuteGenitiveSingular,
                                    AppResources.MinuteGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                        }
                        else
                        {
                            durationString =
                                Language.Declension(
                                    durationTimeSpan.Seconds,
                                    AppResources.SecondNominativeSingular,
                                    AppResources.SecondNominativePlural,
                                    AppResources.SecondGenitiveSingular,
                                    AppResources.SecondGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                        }
                    }

                    return timeString + (!string.IsNullOrEmpty(durationString) ? ", " + durationString : string.Empty);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
