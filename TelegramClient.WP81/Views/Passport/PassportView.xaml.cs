// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Passport;
using TelegramClient.Views.Payments;

namespace TelegramClient.Views.Passport
{
    public partial class PassportView
    {
        public PassportViewModel ViewModel
        {
            get { return DataContext as PassportViewModel; }
        }

        public PassportView()
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
                    switch (ViewModel.Error)
                    {
                        case "REQUIRED_TYPE":
                            var type = ViewModel.RequiredTypes.FirstOrDefault(x => !x.IsCompleted);
                            if (type != null)
                            {
                                var control = Types.ItemContainerGenerator.ContainerFromItem(type) as FrameworkElement;
                                if (control != null)
                                {
                                    ShippingInfoView.Shake(control);
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void AboutIcon_OnTap(object sender, GestureEventArgs e)
        {
            ShellViewModel.ShowCustomMessageBox(
                AppResources.PassportInfo, AppResources.PassportInfoTitle,
                AppResources.Ok);
        }

        private void PassportView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups().ToList();
            var popup = popups.FirstOrDefault();
            if (popup != null)
            {
                e.Cancel = true;

                return;
            }

            if (!ViewModel.IsCancelConfirmed)
            {
                e.Cancel = true;
                ViewModel.Cancel();
            }
        }
    }
}