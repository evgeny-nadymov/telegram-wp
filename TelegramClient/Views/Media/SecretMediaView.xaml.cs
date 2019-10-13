// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Threading;
using System.Windows;

namespace TelegramClient.Views.Media
{
    public partial class SecretMediaView
    {
        public SecretMediaView()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(500);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {

                        Items.Visibility = Visibility.Visible;
                    });
                });
            };
        }
    }
}