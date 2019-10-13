// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class EncryptionKeyView
    {
        public EncryptionKeyViewModel ViewModel
        {
            get { return DataContext as EncryptionKeyViewModel; }
        }

        public EncryptionKeyView()
        {
            //var timer = Stopwatch.StartNew();

            InitializeComponent();

            Loaded += (sender, args) =>
            {
                //TimerText.Text = timer.Elapsed.ToString();
            };
        }

        private void NavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            ViewModel.AnimationComplete();
        }
    }
}