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
using Telegram.Api.TL;

namespace TelegramClient.Helpers.TemplateSelectors
{
    public class LocationTemplateSelector : IValueConverter
    {
        public DataTemplate StartSharingLiveGeoTemplate { get; set; }

        public DataTemplate StopSharingLiveGeoTemplate { get; set; }

        public DataTemplate GeoTemplate { get; set; }

        public DataTemplate ForwardedGeoTemplate { get; set; }

        public DataTemplate VenueTemplate { get; set; }

        public DataTemplate EmptyTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var emptyMessage = value as TLMessageEmpty;
            if (emptyMessage != null)
            {
                return EmptyTemplate;
            }

            var message = value as TLMessage70;
            if (message != null)
            {
                var messageFwdHeader = message.FwdFrom as TLMessageFwdHeader70;
                if (messageFwdHeader != null && messageFwdHeader.From != null)
                {
                    return ForwardedGeoTemplate;
                }

                if (message.Media is TLMessageMediaVenue)
                {
                    return VenueTemplate;
                }

                var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                if (mediaGeoLive != null)
                {
                    mediaGeoLive.Date = message.Date;
                    mediaGeoLive.EditDate = message.EditDate;

                    return mediaGeoLive.Active
                        ? StopSharingLiveGeoTemplate
                        : StartSharingLiveGeoTemplate;
                }

                if (message.Media is TLMessageMediaGeo)
                {
                    return GeoTemplate;
                }
            }

            var decryptedMessage = value as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                if (decryptedMessage.Media is TLDecryptedMessageMediaVenue)
                {
                    return VenueTemplate;
                }

                if (decryptedMessage.Media is TLDecryptedMessageMediaGeoPoint)
                {
                    return GeoTemplate;
                }
            }

            return StartSharingLiveGeoTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LiveLocationTemplateSelector : IValueConverter
    {
        public DataTemplate StartSharingLiveGeoTemplate { get; set; }

        public DataTemplate StopSharingLiveGeoTemplate { get; set; }

        public DataTemplate EmptyTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var emptyMessage = value as TLMessageEmpty;
            if (emptyMessage != null)
            {
                return EmptyTemplate;
            }

            var message = value as TLMessage25;
            if (message != null)
            {
                var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                if (mediaGeoLive != null)
                {
                    return mediaGeoLive.Active
                        ? StopSharingLiveGeoTemplate
                        : StartSharingLiveGeoTemplate;
                }
            }

            return StartSharingLiveGeoTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
