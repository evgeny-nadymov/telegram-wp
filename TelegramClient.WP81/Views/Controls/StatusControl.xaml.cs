// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Media;
using Telegram.Api.TL;

namespace TelegramClient.Views.Controls
{
    public partial class StatusControl
    {
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof (MessageStatus), typeof (StatusControl), new PropertyMetadata(default(MessageStatus), OnStatusChanged));

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var statusControl = d as StatusControl;
            if (statusControl != null)
            {
                statusControl.Sending.Visibility = (MessageStatus)e.NewValue == MessageStatus.Sending || (MessageStatus)e.NewValue == MessageStatus.Compressing
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                statusControl.Confirmed.Visibility = (MessageStatus)e.NewValue == MessageStatus.Confirmed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                statusControl.Read.Visibility = (MessageStatus)e.NewValue == MessageStatus.Read
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                statusControl.Failed.Visibility = (MessageStatus)e.NewValue == MessageStatus.Failed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof (Brush), typeof (StatusControl), new PropertyMetadata(default(Brush), OnFillChanged));

        private static void OnFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var statusControl = d as StatusControl;
            if (statusControl != null)
            {
                statusControl.SendingEllipse.Stroke = e.NewValue as Brush;
                statusControl.SendingPolyline.Stroke = e.NewValue as Brush;

                statusControl.ReadPolyline1.Fill = e.NewValue as Brush;
                statusControl.ReadPolyline2.Fill = e.NewValue as Brush;

                statusControl.Confirmed.Fill = e.NewValue as Brush;
            }
        }

        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public MessageStatus Status
        {
            get { return (MessageStatus) GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public StatusControl()
        {
            InitializeComponent();
        }
    }
}
