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
using System.Windows.Media;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.Converters
{
    public class MessageStateToForegroundConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var accentColor = (Brush)Application.Current.Resources["TelegramBadgeAccentBrush"];
            var foregroundColor = (Brush)Application.Current.Resources["TelegramBadgeSubtleBrush"];

            var dialog = value as TLDialog;
            if (dialog != null)
            {
                var notifySettings = dialog.NotifySettings as TLPeerNotifySettings;
                if (notifySettings != null)
                {
                    var muteUntil = notifySettings.MuteUntil;
                    if (muteUntil == null)
                    {
                        var alert = dialog.Peer is TLPeerUser
                            ? IoC.Get<IStateService>().GetNotifySettings().ContactAlert
                            : IoC.Get<IStateService>().GetNotifySettings().GroupAlert;

                        return alert ? accentColor : foregroundColor;
                    }

                    var clientDelta = IoC.Get<IMTProtoService>().ClientTicksDelta;
                    //var utc0SecsLong = notifySettings.MuteUntil.Value * 4294967296 - clientDelta;
                    var utc0SecsInt = muteUntil.Value - clientDelta / 4294967296.0;

                    var muteUntilDateTime = Telegram.Api.Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);

                    if (muteUntilDateTime > DateTime.Now)
                    {
                        return foregroundColor;
                    }
                }

                return accentColor;
            }

            return accentColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class DialogToForegroundConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var accentColor = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
            var foregroundColor = new SolidColorBrush((Color)Application.Current.Resources["PhoneForegroundColor"]);

            var encryptedDialog = value as TLEncryptedDialog;
            if (encryptedDialog != null)
            {

                return accentColor;
            }

            return foregroundColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
