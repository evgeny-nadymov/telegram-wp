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
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Utils;

namespace TelegramClient.Converters
{
    public class ChannelParticipantsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var channel = value as TLChannel73;
            if (channel != null && channel.ParticipantsCount != null)
            {
                return ", " +  Language.Declension(
                    channel.ParticipantsCount.Value,
                    AppResources.SubscriberNominativeSingular,
                    AppResources.SubscriberNominativePlural,
                    AppResources.SubscriberGenitiveSingular,
                    AppResources.SubscriberGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
