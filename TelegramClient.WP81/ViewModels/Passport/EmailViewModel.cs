// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Passport
{
    public class EmailViewModel : ViewModelBase
    {
        public string UseCurrentEmailCommand
        {
            get { return string.Format(AppResources.PassportPhoneUseSame, CurrentEmail); }
        }

        public string Email { get; set; }

        public string CurrentEmail { get; set; }

        public static bool IsValidType(TLSecureValueTypeBase type)
        {
            return type is TLSecureValueTypeEmail;
        }

        private readonly TLSecureValue _emailValue;

        private readonly TLPasswordBase _passwordBase;

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly IList<TLSecureValue> _secureValues;

        private readonly TLSecureValueTypeBase _secureType;

        private readonly SecureRequiredType _secureRequiredType;

        public EmailViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
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

            var password = _passwordBase as TLPassword81;
            if (password != null)
            {
                var passwordSettings = password.Settings as TLPasswordSettings83;
                if (passwordSettings != null)
                {
                    CurrentEmail = TLString.IsNullOrEmpty(passwordSettings.Email)? string.Empty : passwordSettings.Email.ToString();
                }
            }

            _emailValue = _secureRequiredType != null ? _secureRequiredType.DataValue : null;
        }

        public void UseCurrentEmail()
        {
            if (!IsEmailValid(CurrentEmail)) return;

            SaveEmailAsync(new TLString(CurrentEmail));
        }

        public void Done()
        {
            if (!IsEmailValid(Email)) return;
            if (IsWorking) return;

            SaveEmailAsync(new TLString(Email));
        }

        public bool IsEmailValid(string email)
        {
            return !IsWorking && !string.IsNullOrEmpty(email);
        }

        public static bool SaveEmailAsync(TLString email, TLPassword password, IMTProtoService mtProtoService, Action<TLSecureValue> callback, Action<TLRPCError> faultCallback = null)
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
                Type = new TLSecureValueTypeEmail(),
                PlainData = new TLSecurePlainEmail { Email = email }
            };

            var secureSecretId = passwordSettings.SecureSettings.SecureSecretId;

            mtProtoService.SaveSecureValueAsync(
                inputSecureValue, secureSecretId,
                callback.SafeInvoke,
                faultCallback.SafeInvoke);

            return true;
        }

        private void SaveEmailAsync(TLString email)
        {
            var emailValue = _emailValue;
            if (emailValue == null)
            {
                var secureRequiredType = _secureRequiredType != null ? _secureRequiredType.DataRequiredType as TLSecureRequiredType : null;
                var secureType = secureRequiredType != null && IsValidType(secureRequiredType.Type)
                    ? secureRequiredType.Type
                    : null;

                // add new email from passport settings
                if (_secureType != null && IsValidType(_secureType))
                {
                    emailValue = new TLSecureValue85
                    {
                        Flags = new TLInt(0),
                        Type = _secureType
                    };
                }
                // add new email from authorization form
                else if (secureType != null)
                {
                    emailValue = new TLSecureValue85
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
                SaveEmailAsync(
                    email, _passwordBase as TLPassword, MTProtoService,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        if (_authorizationForm != null)
                        {
                            _authorizationForm.Values.Remove(emailValue);
                            _authorizationForm.Values.Add(result);
                        }

                        emailValue.Update(result);
                        emailValue.NotifyOfPropertyChange(() => emailValue.Self);

                        if (_secureType != null)
                        {
                            EventAggregator.Publish(new AddSecureValueEventArgs { Values = new List<TLSecureValue> { emailValue } });
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
                            && error.TypeEquals(ErrorType.EMAIL_VERIFICATION_NEEDED))
                        {
                            MTProtoService.SendVerifyEmailCodeAsync(email,
                                result2 => BeginOnUIThread(() =>
                                {
                                    StateService.SentEmailCode = result2;
                                    StateService.CurrentEmail = email;

                                    StateService.Password = _passwordBase;
                                    StateService.AuthorizationForm = _authorizationForm;
                                    StateService.SecureValues = _secureValues;
                                    StateService.SecureType = _secureType;
                                    StateService.SecureRequiredType = _secureRequiredType;
                                    NavigationService.UriFor<EmailCodeViewModel>().Navigate();
                                }),
                                error2 => BeginOnUIThread(() =>
                                {
                                    if (error.TypeEquals(ErrorType.EMAIL_INVALID))
                                    {
                                        ShellViewModel.ShowCustomMessageBox(AppResources.EmailInvalidString, AppResources.Error, AppResources.Ok);
                                    }
                                    else if (error.CodeEquals(ErrorCode.FLOOD))
                                    {
                                        ShellViewModel.ShowCustomMessageBox(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, AppResources.Ok);
                                    }
                                    else
                                    {
                                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.sendVerifyEmailCode error " + error);
                                    }
                                }));
                        }
                        else if (error.TypeEquals(ErrorType.EMAIL_INVALID))
                        {
                            ShellViewModel.ShowCustomMessageBox(AppResources.EmailInvalidString, AppResources.Error, AppResources.Ok);
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
