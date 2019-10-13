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

namespace TelegramClient.Converters
{
    public class DraftToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dialog53 = value as TLDialog53;
            if (dialog53 != null)
            {
                if (DialogToBriefInfoConverter.ShowDraft(dialog53))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowFromVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dialog = value as TLDialog;
            if (dialog != null)
            {
                var dialog53 = value as TLDialog53;
                if (dialog53 != null)
                {
                    if (DialogToBriefInfoConverter.ShowDraft(dialog53))
                    {
                        return Visibility.Collapsed;
                    }
                }

                var topMessage = dialog.TopMessage;
                if (topMessage != null)
                {
                    return topMessage.ShowFrom ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            var encryptedDialog = value as TLEncryptedDialog;
            if (encryptedDialog != null)
            {
                var topMessage = encryptedDialog.TopMessage;
                if (topMessage != null)
                {
                    return topMessage.ShowFrom ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DialogToMessageStatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dialog = value as TLDialog;
            if (dialog != null)
            {
                var dialog53 = value as TLDialog53;
                if (dialog53 != null)
                {
                    if (DialogToBriefInfoConverter.ShowDraft(dialog53))
                    {
                        return Visibility.Collapsed;
                    }
                }

                var dialog71 = value as TLDialog71;
                if (dialog71 != null)
                {
                    if (dialog71.IsPromo)
                    {
                        return Visibility.Collapsed;
                    }
                }

                var topMessage = dialog.TopMessage as TLMessageCommon;
                if (topMessage != null)
                {
                    if (!topMessage.Out.Value)
                    {
                        return Visibility.Collapsed;
                    }

                    var serviceMessage = topMessage as TLMessageService;
                    if (serviceMessage != null && serviceMessage.Action is TLMessageActionClearHistory)
                    {
                        return Visibility.Collapsed;
                    }
                }
            }

            var encryptedDialog = value as TLEncryptedDialog;
            if (encryptedDialog != null)
            {
                var topMessageCommon = encryptedDialog.TopMessage;
                if (topMessageCommon != null && !topMessageCommon.Out.Value)
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
