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
using System.Windows.Controls;
using System.Windows.Data;

namespace TelegramClient.Views.Controls
{
    public partial class ChatInviteControl
    {
        public ChatInviteControl()
        {
            InitializeComponent();
        }
    }

    public class ParticipantsCountToScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
            {
                return ScrollBarVisibility.Auto;
            }

            var count = (int)value;
            if (count <= 4)
            {
                return ScrollBarVisibility.Disabled;
            }

            return ScrollBarVisibility.Auto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ParticipantsCountToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
            {
                return HorizontalAlignment.Stretch;
            }

            var count = (int) value;
            if (count <= 4)
            {
                return HorizontalAlignment.Center;
            }

            return HorizontalAlignment.Stretch;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
