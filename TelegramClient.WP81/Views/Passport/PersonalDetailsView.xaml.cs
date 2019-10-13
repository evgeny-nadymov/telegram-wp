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
using System.Windows.Data;
using System.Windows.Input;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Passport;
using TelegramClient.Views.Payments;

namespace TelegramClient.Views.Passport
{
    public partial class PersonalDetailsView
    {
        public PersonalDetailsViewModel ViewModel
        {
            get { return DataContext as PersonalDetailsViewModel; }
        }

        public PersonalDetailsView()
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
                        case ErrorType.ADDRESS_RESIDENCE_COUNTRY_INVALID:
                            ScrollIntoViewAndShake(SelectResidenceCountry);
                            break;
                        case ErrorType.FIRSTNAME_INVALID:
                            ScrollIntoViewAndShake(FirstNameLabel);
                            break;
                        case ErrorType.MIDDLENAME_INVALID:
                            ScrollIntoViewAndShake(MiddleNameLabel);
                            break;
                        case ErrorType.LASTNAME_INVALID:
                            ScrollIntoViewAndShake(LastNameLabel);
                            break;
                        case ErrorType.BIRTHDATE_INVALID:
                            ScrollIntoViewAndShake(SelectBirthDate);
                            break;
                        case ErrorType.GENDER_INVALID:
                            ScrollIntoViewAndShake(SelectGender);
                            break;
                        case ErrorType.DOCUMENT_NUMBER_INVALID:
                            ScrollIntoViewAndShake(DocumentNumberLabel);
                            break;
                        case ErrorType.EXPIRYDATE_INVALID:
                            ScrollIntoViewAndShake(SelectExpiryDate);
                            break;
                        case ErrorType.FIRSTNAMENATIVE_INVALID:
                            ScrollIntoViewAndShake(FirstNameNativeLabel);
                            break;
                        case ErrorType.MIDDLENAMENATIVE_INVALID:
                            ScrollIntoViewAndShake(MiddleNameNativeLabel);
                            break;
                        case ErrorType.LASTNAMENATIVE_INVALID:
                            ScrollIntoViewAndShake(LastNameNativeLabel);
                            break;
                        case ErrorType.FRONT_SIDE_REQUIRED:
                            ScrollIntoViewAndShake(AttachFrontSide);
                            ScrollIntoViewAndShake(FrontSide);
                            break;
                        case ErrorType.REVERSE_SIDE_REQUIRED:
                            ScrollIntoViewAndShake(AttachReverseSide);
                            ScrollIntoViewAndShake(ReverseSide);
                            break;
                        case ErrorType.SELFIE_REQUIRED:
                            ScrollIntoViewAndShake(AttachSelfie);
                            ScrollIntoViewAndShake(Selfie);
                            break;
                        case ErrorType.FILE_ERROR:
                            var frontSide = ViewModel.FrontSide as ISecureFileError;
                            var reverseSide = ViewModel.ReverseSide as ISecureFileError;
                            var selfie = ViewModel.Selfie as ISecureFileError;
                            if (frontSide != null && !string.IsNullOrEmpty(frontSide.Error))
                            {
                                ScrollIntoViewAndShake(FrontSide);
                            }
                            else if (reverseSide != null && !string.IsNullOrEmpty(reverseSide.Error))
                            {
                                ScrollIntoViewAndShake(ReverseSide);
                            }
                            else if (selfie != null && !string.IsNullOrEmpty(selfie.Error))
                            {
                                ScrollIntoViewAndShake(Selfie);
                            }
                            break;
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

    public class GenderToStringConverter : IValueConverter
    {
        public static string Convert(string gender)
        {
            if (string.Equals(gender, "male", StringComparison.OrdinalIgnoreCase))
            {
                return AppResources.PassportMale;
            }
            if (string.Equals(gender, "female", StringComparison.OrdinalIgnoreCase))
            {
                return AppResources.PassportFemale;
            }

            return gender;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gender = value as string;
            if (gender != null)
            {
                return Convert(gender);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}