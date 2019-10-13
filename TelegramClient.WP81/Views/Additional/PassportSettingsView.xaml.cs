// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Passport;
using TelegramClient.Views.Passport;

namespace TelegramClient.Views.Additional
{
    public partial class PassportSettingsView
    {
        public PassportSettingsView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void AboutIcon_OnTap(object sender, GestureEventArgs e)
        {
            ShellViewModel.ShowCustomMessageBox(
                AppResources.PassportInfo, AppResources.PassportInfoTitle,
                AppResources.Ok);
        }

        private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
            ImagePlaceholder.Opacity = 1.0;
        }
    }

    public class SecureRequiredTypeToCaptionConverter : IValueConverter
    {
        private static readonly Dictionary<Type, string> _dict = new Dictionary<Type, string>
        {
            { typeof(TLSecureValueTypePersonalDetails), AppResources.PassportPersonalDetails },
            { typeof(TLSecureValueTypePassport), AppResources.Passport },
            { typeof(TLSecureValueTypeDriverLicense), AppResources.PassportDriverLicence },
            { typeof(TLSecureValueTypeIdentityCard), AppResources.PassportIdentityCard },
            { typeof(TLSecureValueTypeInternalPassport), AppResources.PassportInternal },
            { typeof(TLSecureValueTypeAddress), AppResources.PassportAddress },
            { typeof(TLSecureValueTypeUtilityBill), AppResources.PassportUtilityBill },
            { typeof(TLSecureValueTypeBankStatement), AppResources.PassportBankStatement },
            { typeof(TLSecureValueTypeRentalAgreement), AppResources.PassportTenancyAgreement },
            { typeof(TLSecureValueTypePassportRegistration), AppResources.PassportRegistration },
            { typeof(TLSecureValueTypeTemporaryRegistration), AppResources.PassportTemporaryRegistration },
            { typeof(TLSecureValueTypePhone), AppResources.PassportPhone },
            { typeof(TLSecureValueTypeEmail), AppResources.PassportEmail },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var secureValueType = value as TLSecureValueTypeBase;
            if (secureValueType != null)
            {
                return Convert(secureValueType);
            }

            return null;
        }

        public static string Convert(TLSecureValueTypeBase value)
        {
            string caption;
            if (value != null && _dict.TryGetValue(value.GetType(), out caption))
            {
                return caption;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SecureRequiredTypeToHintConverter : IValueConverter
    {
        private static readonly Dictionary<Type, string> _dict = new Dictionary<Type, string>
        {
            { typeof(TLSecureValueTypePersonalDetails), AppResources.PassportPersonalDetailsInfo },
            { typeof(TLSecureValueTypePassport), AppResources.PassportIdentityPassport },
            { typeof(TLSecureValueTypeDriverLicense), AppResources.PassportIdentityDriverLicence },
            { typeof(TLSecureValueTypeIdentityCard), AppResources.PassportIdentityID },
            { typeof(TLSecureValueTypeInternalPassport), AppResources.PassportIdentityInternalPassport },
            { typeof(TLSecureValueTypeAddress), AppResources.PassportAddressNoUploadInfo },
            { typeof(TLSecureValueTypeUtilityBill), AppResources.PassportAddBillInfo },
            { typeof(TLSecureValueTypeBankStatement), AppResources.PassportAddBankInfo },
            { typeof(TLSecureValueTypeRentalAgreement), AppResources.PassportAddAgreementInfo },
            { typeof(TLSecureValueTypePassportRegistration), AppResources.PassportAddPassportRegistrationInfo },
            { typeof(TLSecureValueTypeTemporaryRegistration), AppResources.PassportAddTemporaryRegistrationInfo },
            { typeof(TLSecureValueTypePhone), AppResources.PassportPhoneInfo },
            { typeof(TLSecureValueTypeEmail), AppResources.PassportEmailInfo },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var secureValueType = value as TLSecureValueTypeBase;
            if (secureValueType != null)
            {
                return Convert(secureValueType);
            }

            return null;
        }

        public static string Convert(TLSecureValueTypeBase value)
        {
            string caption;
            if (value != null && _dict.TryGetValue(value.GetType(), out caption))
            {
                return caption;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SecureValueToHintConverter : IValueConverter
    {
        private static readonly Dictionary<Type, Func<TLSecureValue, string>> _dict = new Dictionary<Type, Func<TLSecureValue, string>>
        {
            { typeof(TLSecureValueTypePersonalDetails), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypePassport), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeDriverLicense), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeIdentityCard), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeInternalPassport), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeAddress), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeUtilityBill), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeBankStatement), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeRentalAgreement), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypePassportRegistration), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeTemporaryRegistration), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypePhone), x => { return AppResources.PassportPersonalDetails; } },
            { typeof(TLSecureValueTypeEmail), x => { return AppResources.PassportPersonalDetails; } },
        };

        public static string Convert(TLSecureValue secureValue)
        {
            var personalDetails = secureValue.Type as TLSecureValueTypePersonalDetails;
            if (personalDetails != null)
            {
                var rootObject = secureValue.Data.DecryptedData as PersonalDetailsRootObject;
                if (rootObject != null)
                {
                    return rootObject.ToString(
                        GenderToStringConverter.Convert,
                        x =>
                        {
                            var country = CountryUtils.GetCountryByCode(x);
                            return country != null ? country.Name : null;
                        });
                }
            }

            var passport = secureValue.Type as TLSecureValueTypePassport;
            if (passport != null)
            {
                if (secureValue.Data.DecryptedData != null)
                {
                    return secureValue.Data.DecryptedData.ToString();
                }
            }

            var driverLicence = secureValue.Type as TLSecureValueTypeDriverLicense;
            if (driverLicence != null)
            {
                if (secureValue.Data.DecryptedData != null)
                {
                    return secureValue.Data.DecryptedData.ToString();
                }
            }

            var identityCard = secureValue.Type as TLSecureValueTypeIdentityCard;
            if (identityCard != null)
            {
                if (secureValue.Data.DecryptedData != null)
                {
                    return secureValue.Data.DecryptedData.ToString();
                }
            }

            var internalPassport = secureValue.Type as TLSecureValueTypeInternalPassport;
            if (internalPassport != null)
            {
                if (secureValue.Data.DecryptedData != null)
                {
                    return secureValue.Data.DecryptedData.ToString();
                }
            }

            var address = secureValue.Type as TLSecureValueTypeAddress;
            if (address != null)
            {
                var rootObject = secureValue.Data.DecryptedData as ResidentialAddressRootObject;
                if (rootObject != null)
                {
                    return rootObject.ToString(x =>
                    {
                        var country = CountryUtils.GetCountryByCode(x);
                        return country != null ? country.Name : null;
                    });
                }
            }

            var utilityBill = secureValue.Type as TLSecureValueTypeUtilityBill;
            if (utilityBill != null)
            {
                return AppResources.PassportDocuments;
            }

            var bankStatement = secureValue.Type as TLSecureValueTypeBankStatement;
            if (bankStatement != null)
            {
                return AppResources.PassportDocuments;
            }

            var rentalAgreement = secureValue.Type as TLSecureValueTypeRentalAgreement;
            if (rentalAgreement != null)
            {
                return AppResources.PassportDocuments;
            }

            var passportRegistration = secureValue.Type as TLSecureValueTypePassportRegistration;
            if (passportRegistration != null)
            {
                return AppResources.PassportDocuments;
            }

            var temporaryRegistration = secureValue.Type as TLSecureValueTypeTemporaryRegistration;
            if (temporaryRegistration != null)
            {
                return AppResources.PassportDocuments;
            }

            var phone = secureValue.Type as TLSecureValueTypePhone;
            if (phone != null)
            {
                var plainData = secureValue.PlainData as TLSecurePlainPhone;
                if (plainData != null && !TLString.IsNullOrEmpty(plainData.Phone))
                {
                    return plainData.Phone.ToString().StartsWith("+") ? plainData.Phone.ToString() : "+" + plainData.Phone;
                }
            }

            var email = secureValue.Type as TLSecureValueTypeEmail;
            if (email != null)
            {
                var plainData = secureValue.PlainData as TLSecurePlainEmail;
                if (plainData != null && !TLString.IsNullOrEmpty(plainData.Email))
                {
                    return plainData.Email.ToString();
                }
            }

            return null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var secureValue = value as TLSecureValue;
            if (secureValue == null) return null;

            return Convert(secureValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}