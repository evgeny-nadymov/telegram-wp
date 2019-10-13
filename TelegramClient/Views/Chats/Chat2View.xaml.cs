// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Diagnostics;
using System.Windows;

namespace TelegramClient.Views.Chats
{
    public partial class Chat2View
    {
        public static readonly DependencyProperty TimerProperty = DependencyProperty.Register(
            "Timer", typeof(string), typeof(Chat2View), new PropertyMetadata(default(string)));

        public string Timer
        {
            get { return (string) GetValue(TimerProperty); }
            set { SetValue(TimerProperty, value); }
        }

        public Chat2View()
        {
            var timer = Stopwatch.StartNew();
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                Timer = timer.Elapsed.ToString();
            };
        }
    }
}