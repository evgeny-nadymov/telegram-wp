// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class CacheView
    {
        public CacheViewModel ViewModel
        {
            get { return DataContext as CacheViewModel; }
        }

        public CacheView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void CacheView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (ViewModel.ClearCacheSettings != null && ViewModel.ClearCacheSettings.IsOpen)
            {
                ViewModel.ClearCacheSettings.Close();
                e.Cancel = true;
                return;
            }
        }
    }
}