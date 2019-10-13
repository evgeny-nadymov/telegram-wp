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

namespace TelegramClient.Converters
{
    public class StatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MessageStatus)) return null;

            var status = (MessageStatus) value;

            if (status == MessageStatus.Sending)
            {
                return new Uri("/Images/Messages/message.state.sending-WXGA.png", UriKind.Relative);
            }

            if (status == MessageStatus.Confirmed)
            {
                return new Uri("/Images/Messages/message.state.sent-WXGA.png", UriKind.Relative);
            }

            if (status == MessageStatus.Read)
            {
                return new Uri("/Images/Messages/message.state.read-WXGA.png", UriKind.Relative);
            }

            //if (status == MessageStatus.Broadcast)
            //{
            //    return new Uri("/Images/Messages/message.state.broadcast.png", UriKind.Relative);
            //}

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
