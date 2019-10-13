// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class ChooseCountryView
    {
        public ChooseCountryViewModel ViewModel { get { return DataContext as ChooseCountryViewModel; } }

        public ChooseCountryView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private bool _once;

        private void TelegramNavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            if (_once) return;

            _once = true;
            ViewModel.ForwardInAnimationComplete();
        }

        private void Search_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.IsSearching = !ViewModel.IsSearching;

            if (ViewModel.IsSearching)
            {
                SearchBox.Focus();
            }
            else
            {
                Page.Focus();
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (ViewModel.IsSearching)
            {
                ViewModel.IsSearching = false;
                e.Cancel = true;
                return;
            }

            base.OnBackKeyPress(e);
        }
    }
}