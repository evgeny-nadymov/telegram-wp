// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using System.Windows.Input;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Payments;

namespace TelegramClient.Views.Payments
{
    public partial class SavedCardInfoView
    {
        public SavedCardInfoViewModel ViewModel
        {
            get { return DataContext as SavedCardInfoViewModel; }
        }

        public SavedCardInfoView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) =>
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.Error))
            {
                if (ViewModel.Error != null)
                {
                    var type = ViewModel.Error;
                    switch (type)
                    {
                        case "PASSWORD":
                            ShippingInfoView.Shake(PasswordLabel, PasswordLabel.Input);
                            break;
                    }
                }
            }
        }

        private void PasswordLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Password = PasswordLabel.TextBox.Password;
                ViewModel.Validate();
            }
        }
    }
}