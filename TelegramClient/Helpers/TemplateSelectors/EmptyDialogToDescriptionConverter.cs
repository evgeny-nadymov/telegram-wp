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
    public class EmptyDialogToDescriptionConverter : IValueConverter
    {
        public DataTemplate UserSelfTemplate { get; set; }

        public DataTemplate UserTemplate { get; set; }

        public DataTemplate SupportTemplate { get; set; }

        public DataTemplate BotTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userBase = value as TLUserBase;
            if (userBase != null)
            {
                if (userBase.Index == 333000)
                {
                    return SupportTemplate;
                }
                if (userBase.IsSelf)
                {
                    return UserSelfTemplate;
                }
            }

            var user = value as TLUser;
            if (user != null && user.IsBot)
            {
                var botInfo = user.BotInfo as TLBotInfo;
                if (botInfo != null && !TLString.IsNullOrEmpty(botInfo.Description))
                {
                    return BotTemplate;
                }

                return new DataTemplate();
            }

            return UserTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
