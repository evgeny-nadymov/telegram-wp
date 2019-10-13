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
    public partial class PasswordEmailView
    {
        public PasswordEmailViewModel ViewModel
        {
            get { return DataContext as PasswordEmailViewModel; }
        }

        public PasswordEmailView()
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
                var type = ViewModel.Error;
                if (!string.IsNullOrEmpty(type))
                {
                    switch (type)
                    {
                        case "invalid_password":
                            ShippingInfoView.Shake(PasswordLabel, PasswordLabel.Input);
                            break;
                        case "invalid_confirmpassword":
                            ShippingInfoView.Shake(ConfirmPasswordLabel, ConfirmPasswordLabel.Input);
                            break;
                        case "invalid_email":
                            ShippingInfoView.Shake(RecoveryEmailLabel, RecoveryEmailLabel.Input);
                            break;
                    }
                }
            }
        }

        private void PasswordLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmPasswordLabel.Focus();
            }
        }

        private void ConfirmPasswordLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RecoveryEmailLabel.Focus();
            }
        }

        private void RecoveryEmailLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Password = PasswordLabel.TextBox.Password;
                ViewModel.ConfirmPassword = ConfirmPasswordLabel.TextBox.Password;
                ViewModel.RecoveryEmail = RecoveryEmailLabel.TextBox.Text;

                ViewModel.Create();
            }
        }
    }
}