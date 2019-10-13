// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class CameraView
    {
        public CameraViewModel ViewModel
        {
            get { return DataContext as CameraViewModel; }
        }

        public CameraView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                var privacyRule = radioButton.DataContext as TLPrivacyRuleBase;
                if (privacyRule != null)
                {
                    ViewModel.SelectedMainRule = privacyRule;
                }
            }
        }
    }
}