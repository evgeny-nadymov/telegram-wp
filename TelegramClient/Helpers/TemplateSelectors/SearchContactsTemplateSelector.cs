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
using TelegramClient.ViewModels.Search;

namespace TelegramClient.Helpers.TemplateSelectors
{
    public class SearchTemplateSelector : IValueConverter
    {
        public DataTemplate UserTemplate { get; set; }

        public DataTemplate TextTemplate { get; set; }

        public DataTemplate DialogTemplate { get; set; }

        public DataTemplate MessageTemplate { get; set; }

        public object Convert(object item, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(item is TLObject)) return null;

            var serviceText = item as TLServiceText;
            if (serviceText != null)
            {
                return TextTemplate;
            }

            var userBase = item as TLUserBase;
            if (userBase != null && userBase.IsGlobalResult)
            {
                var username = userBase as IUserName;
                if (username != null && !TLString.IsNullOrEmpty(username.UserName))
                {
                    return UserTemplate;
                }

                return DialogTemplate;
            }

            var chatBase = item as TLChatBase;
            if (chatBase != null && chatBase.IsGlobalResult)
            {
                return UserTemplate;
            }

            var dialog = item as TLDialog;
            if (dialog != null && dialog.TopMessage != null && dialog.Messages == null)
            {
                return MessageTemplate;
            }

            return DialogTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SearchContactsTemplateSelector : IValueConverter
    {
        public DataTemplate ContactTemplate { get; set; }

        public DataTemplate UserStatusTemplate { get; set; }

        public DataTemplate UserUsernameTemplate { get; set; }

        public DataTemplate TextTemplate { get; set; }

        public object Convert(object item, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(item is TLObject)) return null;

            var serviceText = item as TLServiceText;
            if (serviceText != null)
            {
                return TextTemplate;
            }

            var userBase = item as TLUserBase;
            if (userBase != null && userBase.IsContact)
            {
                return ContactTemplate;
            }

            var username = userBase as IUserName;
            if (username != null && !TLString.IsNullOrEmpty(username.UserName))
            {
                return UserUsernameTemplate;
            }

            return UserStatusTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
