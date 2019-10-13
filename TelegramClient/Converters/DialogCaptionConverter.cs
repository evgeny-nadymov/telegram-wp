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
using Telegram.Api.Services.Cache;
//#if WP8
//using PhoneNumbers;
//#endif
using Telegram.Api.TL;
using TelegramClient.Resources;

namespace TelegramClient.Converters
{
    public class DialogCaptionConverter : IValueConverter
    {
        public static string Convert(object value)
        {
            if (value == null) return null;

            var channels = value as TLVector<TLChatBase>;
            if (channels != null)
            {
                return AppResources.Feed;
            }

            var user = value as TLUserBase;
            if (user != null)
            {
                if (user.Index == 333000)
                {
                    return AppResources.AppName;
                }

                if (user.Index == Constants.TelegramNotificationsId)
                {
                    return AppResources.TelegramNotifications;
                }

                if (user.IsRequest)
                {
#if WP8
                    //var phoneUtil = PhoneNumberUtil.GetInstance();
                    //try
                    //{
                    //    return phoneUtil.Format(phoneUtil.Parse("+" + user.Phone.Value, ""), PhoneNumberFormat.INTERNATIONAL);
                    //}
                    //catch (Exception e)
                    {
                        return "+" + user.Phone.Value;
                    }
#else
                    return "+" + user.Phone.Value;
#endif

                }

                if (user is TLUserEmpty
                    || user.IsDeleted)
                {
                    
                }

                return user.FullName.Trim();
            }

            var chat = value as TLChatBase;
            if (chat != null)
            {
                return chat.FullName.Trim();
            }

            var encryptedChat = value as TLEncryptedChatCommon;
            if (encryptedChat != null)
            {
                var currentUserId = IoC.Get<IMTProtoService>().CurrentUserId;
                var cache = IoC.Get<ICacheService>();

                if (currentUserId.Value == encryptedChat.AdminId.Value)
                {
                    var cachedParticipant = cache.GetUser(encryptedChat.ParticipantId);
                    return cachedParticipant != null ? cachedParticipant.FullName.Trim().ToUpperInvariant() : string.Empty;
                }

                var cachedAdmin = cache.GetUser(encryptedChat.AdminId);
                return cachedAdmin != null ? cachedAdmin.FullName.Trim() : string.Empty;
            }

            return value != null? value.ToString() : string.Empty;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool uppercase = false;
            if (parameter != null
                && string.Equals(parameter.ToString(), "uppercase", StringComparison.OrdinalIgnoreCase))
            {
                uppercase = true;
            }

            var result = Convert(value) ?? string.Empty;
            if (uppercase)
            {
                return result.ToUpperInvariant();
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ChatInviteSubtitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var chatInvite54 = value as TLChatInvite54;
            if (chatInvite54 == null) return null;
            string subtitleText;
            if (chatInvite54.Participants != null && chatInvite54.Participants.Count > 0)
            {
                subtitleText = string.Format(AppResources.ChatInviteSubtitle, Utils.Language.Declension(
                        chatInvite54.ParticipantsCount.Value,
                        AppResources.CompanyNominativeSingular,
                        AppResources.CompanyNominativePlural,
                        AppResources.CompanyGenitiveSingular,
                        AppResources.CompanyGenitivePlural))
                        .ToLower(CultureInfo.CurrentUICulture);
            }
            else
            {
                subtitleText = Utils.Language.Declension(
                    chatInvite54.ParticipantsCount.Value,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural)
                    .ToLower(CultureInfo.CurrentUICulture);
            }

            return subtitleText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
