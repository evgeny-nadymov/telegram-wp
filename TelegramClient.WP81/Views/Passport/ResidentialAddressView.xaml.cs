// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Passport;
using TelegramClient.Views.Payments;

namespace TelegramClient.Views.Passport
{
    public partial class ResidentialAddressView
    {
        public ResidentialAddressViewModel ViewModel
        {
            get { return DataContext as ResidentialAddressViewModel; }
        }

        public ResidentialAddressView()
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
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer)
                && ViewModel.ImageViewer != null)
            {
                ViewModel.ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.Error))
            {
                if (ViewModel.Error != null)
                {
                    var type = ViewModel.Error.GetErrorType();
                    switch (type)
                    {
                        case ErrorType.ADDRESS_COUNTRY_INVALID:
                            ScrollIntoViewAndShake(SelectCountry);
                            break;
                        case ErrorType.ADDRESS_CITY_INVALID:
                            ScrollIntoViewAndShake(CityLabel.Input, CityLabel.Input);
                            break;
                        case ErrorType.ADDRESS_POSTCODE_INVALID:
                            ScrollIntoViewAndShake(PostCodeLabel.Input, PostCodeLabel.Input);
                            break;
                        case ErrorType.ADDRESS_STATE_INVALID:
                            ScrollIntoViewAndShake(StateLabel.Input, StateLabel.Input);
                            break;
                        case ErrorType.ADDRESS_STREET_LINE2_INVALID:
                            ScrollIntoViewAndShake(StreetLine2Label.Input, StreetLine2Label.Input);
                            break;
                        case ErrorType.ADDRESS_STREET_LINE1_INVALID:
                            ScrollIntoViewAndShake(StreetLine1Label.Input, StreetLine1Label.Input);
                            break;
                        case ErrorType.FILES_EMPTY:
                            ScrollIntoViewAndShake(AttachDocument);
                            break;
                        case ErrorType.FILE_ERROR:
                        {
                            var file = ViewModel.Files.OfType<ISecureFileError>().FirstOrDefault(x => !string.IsNullOrEmpty(x.Error));
                            if (file != null)
                            {
                                var control = Files.ItemContainerGenerator.ContainerFromItem(file) as FrameworkElement;
                                if (control != null)
                                {
                                    ScrollIntoViewAndShake(control);
                                }
                            }
                            break;
                        }
                        case ErrorType.TRANSLATION_EMPTY:
                            ScrollIntoViewAndShake(AttachTranslation);
                            break;
                        case ErrorType.TRANSLATION_ERROR:
                            if (ViewModel.Translations != null)
                            {
                                var file = ViewModel.Translations.OfType<ISecureFileError>().FirstOrDefault(x => !string.IsNullOrEmpty(x.Error));
                                if (file != null)
                                {
                                    var control = Translations.ItemContainerGenerator.ContainerFromItem(file) as FrameworkElement;
                                    if (control != null)
                                    {
                                        ScrollIntoViewAndShake(control);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void ScrollIntoViewAndShake(FrameworkElement element)
        {
            var duration = TimeSpan.FromSeconds(0.15);

            Scroll.ScrollIntoView(element, 0.0, 128.0, duration);
            Telegram.Api.Helpers.Execute.BeginOnUIThread(duration, () => ShippingInfoView.Shake(element));
        }

        private void ScrollIntoViewAndShake(FrameworkElement element, TextBox focusElement)
        {
            var duration = TimeSpan.FromSeconds(0.15);

            Scroll.ScrollIntoView(element, 0.0, 128.0, duration);
            Telegram.Api.Helpers.Execute.BeginOnUIThread(duration, () => ShippingInfoView.Shake(element, focusElement));
        }

        private void OnImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer.IsOpen))
            {
                Scroll.IsHitTestVisible = !ViewModel.ImageViewer.IsOpen;
            }
        }

        private void DoneIcon_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }
    }

    public class FileUploadingToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool) value ? 0.5 : 1.0;
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}