// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Passport
{
    public class PhoneNumberViewModel : ViewModelBase
    {
        private Country _selectedCountry;

        public Country SelectedCountry
        {
            get { return _selectedCountry; }
            set { SetField(ref _selectedCountry, value, () => SelectedCountry); }
        }

        public bool IsPhoneCodeInvalid
        {
            get { return string.IsNullOrEmpty(PhoneCode) || CountryUtils.CountriesSource.FirstOrDefault(x => x.PhoneCode == PhoneCode) == null; }
        }

        private string _phoneCode;

        public string PhoneCode
        {
            get { return _phoneCode; }
            set
            {
                SetField(ref _phoneCode, value, () => PhoneCode);
                Country country = null;
                foreach (var c in CountryUtils.CountriesSource)
                {
                    if (c.PhoneCode == PhoneCode)
                    {
                        if (c.PhoneCode == "1" && c.Code != "us")
                        {
                            continue;
                        }

                        if (c.PhoneCode == "7" && c.Code != "ru")
                        {
                            continue;
                        }

                        country = c;
                        break;
                    }
                }

                if (country != null)
                {
                    SelectedCountry = country;
                }

                NotifyOfPropertyChange(() => IsPhoneCodeInvalid);
            }
        }

        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { SetField(ref _phoneNumber, value, () => PhoneNumber); }
        }

        public string UseCurrentPhoneCommand
        {
            get { return string.Format(AppResources.PassportPhoneUseSame, "+" + CurrentPhone); }
        }

        public string CurrentPhone { get; set; }

        public TLRPCError Error { get; set; }
        
        public static bool IsValidType(TLSecureValueTypeBase type)
        {
            return type is TLSecureValueTypePhone;
        }

        private readonly TLSecureValue _phoneNumberValue;

        private readonly TLPasswordBase _passwordBase;

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly IList<TLSecureValue> _secureValues;

        private readonly TLSecureValueTypeBase _secureType;

        private readonly SecureRequiredType _secureRequiredType;

        public PhoneNumberViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _passwordBase = stateService.Password;
            stateService.Password = null;

            _authorizationForm = stateService.AuthorizationForm;
            stateService.AuthorizationForm = null;

            _secureValues = stateService.SecureValues;
            stateService.SecureValues = null;

            _secureType = stateService.SecureType;
            stateService.SecureType = null;

            _secureRequiredType = stateService.SecureRequiredType;
            stateService.SecureRequiredType = null;

            var user = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
            if (user != null && user.HasPhone)
            {
                CurrentPhone = user.Phone.ToString();
            }

            _phoneNumberValue = _secureRequiredType != null ? _secureRequiredType.DataValue : null;
        }

        protected override void OnActivate()
        {
            if (StateService.SelectedCountry != null)
            {
                var country = StateService.SelectedCountry;
                StateService.SelectedCountry = null;
                OnCountrySelected(country);
            }
            base.OnActivate();
        }

        public void SelectCountry()
        {
            StateService.HideCountryCode = true;
            NavigationService.UriFor<ChooseCountryViewModel>().Navigate();
        }

        private void OnCountrySelected(Country country)
        {
            SelectedCountry = country;
            _phoneCode = SelectedCountry.PhoneCode;
            NotifyOfPropertyChange(() => PhoneCode);
        }

        public void UseCurrentPhone()
        {
            if (!IsPhoneNumberValid(CurrentPhone)) return;

            SavePhoneAsync(new TLString(CurrentPhone));
        }

        public void Done()
        {
            if (!IsPhoneNumberValid(PhoneCode + PhoneNumber)) return;
            if (IsWorking) return;

            SavePhoneAsync(new TLString(PhoneCode + PhoneNumber));
        }

        public bool IsPhoneNumberValid(string phoneNumber)
        {
            var phoneNumberLength = string.IsNullOrEmpty(phoneNumber) ? 0 : phoneNumber.Length;
            return !IsWorking && phoneNumberLength >= 3;
        }

        public static bool SavePhoneAsync(TLString phone, TLPassword password, IMTProtoService mtProtoService, Action<TLSecureValue> callback, Action<TLRPCError> faultCallback = null)
        {
            if (password == null)
            {
                return false;
            }

            var passwordSettings = password.Settings as TLPasswordSettings83;
            if (passwordSettings == null)
            {
                return false;
            }

            var inputSecureValue = new TLInputSecureValue85
            {
                Flags = new TLInt(0),
                Type = new TLSecureValueTypePhone(),
                PlainData = new TLSecurePlainPhone { Phone = phone }
            };

            var secureSecretId = passwordSettings.SecureSettings.SecureSecretId;

            mtProtoService.SaveSecureValueAsync(
                inputSecureValue, secureSecretId,
                callback.SafeInvoke,
                faultCallback.SafeInvoke);

            return true;
        }

        private void SavePhoneAsync(TLString phone)
        {
            var phoneNumberValue = _phoneNumberValue;
            if (phoneNumberValue == null)
            {
                var secureRequiredType = _secureRequiredType != null ? _secureRequiredType.DataRequiredType as TLSecureRequiredType : null;
                var secureType = secureRequiredType != null && IsValidType(secureRequiredType.Type)
                    ? secureRequiredType.Type
                    : null;

                // add new phone number from passport settings
                if (_secureType != null && IsValidType(_secureType))
                {
                    phoneNumberValue = new TLSecureValue85
                    {
                        Flags = new TLInt(0),
                        Type = _secureType
                    };
                }
                // add new phone number from authorization form
                else if (secureType != null)
                {
                    phoneNumberValue = new TLSecureValue85
                    {
                        Flags = new TLInt(0),
                        Type = secureType
                    };
                }
                else
                {
                    return;
                }
            }

            IsWorking =
                SavePhoneAsync(
                    phone, _passwordBase as TLPassword, MTProtoService,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        if (_authorizationForm != null)
                        {
                            _authorizationForm.Values.Remove(phoneNumberValue);
                            _authorizationForm.Values.Add(result);
                        }

                        phoneNumberValue.Update(result);
                        phoneNumberValue.NotifyOfPropertyChange(() => phoneNumberValue.Self);

                        if (_secureType != null)
                        {
                            EventAggregator.Publish(new AddSecureValueEventArgs { Values = new List<TLSecureValue> { phoneNumberValue } });
                        }

                        if (_secureRequiredType != null)
                        {
                            _secureRequiredType.UpdateValue();
                        }

                        NavigationService.GoBack();
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                            && error.TypeEquals(ErrorType.PHONE_VERIFICATION_NEEDED))
                        {
                            MTProtoService.SendVerifyPhoneCodeAsync(phone, null,
                                result2 => BeginOnUIThread(() =>
                                {
                                    StateService.PhoneNumber = phone;
                                    StateService.PhoneNumberString = string.Format(AppResources.ConfirmMessage, PhoneNumberConverter.Convert(StateService.PhoneNumber));
                                    StateService.PhoneCodeHash = result2.PhoneCodeHash;
                                    StateService.PhoneRegistered = result2.PhoneRegistered;
                                    StateService.SendCallTimeout = result2.SendCallTimeout;
                                    var sentCode50 = result2 as TLSentCode50;
                                    if (sentCode50 != null)
                                    {
                                        StateService.Type = sentCode50.Type;
                                        StateService.NextType = sentCode50.NextType;
                                    }
                                    StateService.Password = _passwordBase;
                                    StateService.AuthorizationForm = _authorizationForm;
                                    StateService.SecureValues = _secureValues;
                                    StateService.SecureType = _secureType;
                                    StateService.SecureRequiredType = _secureRequiredType;
                                    NavigationService.UriFor<PhoneNumberCodeViewModel>().Navigate();
                                }),
                                error2 => BeginOnUIThread(() =>
                                {
                                    if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                                    {
                                        ShellViewModel.ShowCustomMessageBox(AppResources.PhoneNumberInvalidString, AppResources.Error, AppResources.Ok);
                                    }
                                    else if (error.CodeEquals(ErrorCode.FLOOD))
                                    {
                                        ShellViewModel.ShowCustomMessageBox(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, AppResources.Ok);
                                    }
                                    else
                                    {
                                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.sendVerifyPhoneCode error " + error);
                                    }
                                }));
                        }
                        else if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                        {
                            ShellViewModel.ShowCustomMessageBox(AppResources.PhoneNumberInvalidString, AppResources.Error, AppResources.Ok);
                        }
                        else if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                        {
                            ShellViewModel.ShowCustomMessageBox(
                               "account.saveSecureValue" + Environment.NewLine + error.Message,
                               AppResources.AppName,
                               AppResources.Ok);
                        }
                    }));
        }
    }
}
