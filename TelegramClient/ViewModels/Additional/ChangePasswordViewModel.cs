// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class ChangePasswordViewModel : ChangePasswordViewModelBase
    {
        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                SetField(ref _password, value, () => Password);
                NotifyOfPropertyChange(() => CanChangePassword);
            }
        }

        private string _confirmPassword;

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                SetField(ref _confirmPassword, value, () => ConfirmPassword);
                NotifyOfPropertyChange(() => CanChangePassword);
            }
        }

        public ChangePasswordViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {

        }

        protected override void OnActivate()
        {
            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            base.OnActivate();
        }

        public bool CanChangePassword
        {
            get { return !string.IsNullOrEmpty(_password) && string.Equals(_password, _confirmPassword); }
        }

        public void ChangePassword()
        {
            if (!CanChangePassword) return;

            var password84 = _passwordBase as TLPassword84;
            if (password84 == null) return;

            var newAlgo = password84.NewAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (newAlgo == null) return;

            Utils.Password.AddClientSalt(newAlgo);

            //var newPasswordHash = SRP.GetX(new TLString(_password), newAlgo);
            //var newPasswordInputHash = SRP.GetVBytes(new TLString(_password), newAlgo);
            //Utils.Password.GetHash(newSalt, new TLString(_password));

            var inputSettings = new TLPasswordInputSettings83
            {
                Flags = new TLInt(0),
                NewAlgo = newAlgo,
                NewPasswordHash = null,
                Hint = TLString.Empty,
                NewPassword = _password
            };

            // reencrypt secure_secret
            var passwordSecret = _passwordBase as IPasswordSecret;
            if (passwordSecret != null)
            {
                TLPasswordSettings83 passwordSettings = null;
                var password = _passwordBase as TLPassword81;
                if (password != null)
                {
                    passwordSettings = password.Settings as TLPasswordSettings83;
                }
                if (passwordSettings == null || passwordSettings.SecureSettings == null)
                {
                    Utils.Password.AddRandomSecureSecret(inputSettings, passwordSecret, new TLString(_password));
                }
                else
                {
                    var secret = Utils.Passport.DecryptSecureSecret(passwordSettings.SecureSettings.SecureSecret, new TLString(PasswordViewModel.Password), passwordSettings.SecureSettings.SecureAlgo);
                    Utils.Password.AddSecureSecret(secret, inputSettings, passwordSecret, new TLString(_password));
                }
            }

            PasswordViewModel.TempNewPassword = Password;

            StateService.Password = _passwordBase;
            StateService.NewPasswordSettings = inputSettings;
            StateService.RemoveBackEntry = true;
            StateService.AuthorizationForm = _authorizationForm;
            StateService.SecureValues = _secureValues;
            NavigationService.UriFor<ChangePasswordHintViewModel>().Navigate();

            return;
        }
    }

    public abstract class ChangePasswordViewModelBase : ViewModelBase
    {

        private bool _hasError;

        public bool HasError
        {
            get { return _hasError; }
            set { SetField(ref _hasError, value, () => HasError); }
        }

        private string _error;

        public string Error
        {
            get { return _error; }
            set { SetField(ref _error, value, () => Error); }
        }

        protected TLPasswordInputSettings _newPasswordSettings;

        protected TLPasswordBase _passwordBase;

        protected TLAuthorizationForm _authorizationForm;

        protected IList<TLSecureValue> _secureValues;

        protected ChangePasswordViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _passwordBase = StateService.Password;
            StateService.Password = null;

            _newPasswordSettings = StateService.NewPasswordSettings;
            StateService.NewPasswordSettings = null;

            _authorizationForm = StateService.AuthorizationForm;
            StateService.AuthorizationForm = null;

            _secureValues = StateService.SecureValues;
            StateService.SecureValues = null;
        }

        protected void UpdatePasswordSettings(TLPassword84 password, TLPasswordInputSettings newSettings)
        {
            IsWorking = true;
            BeginOnThreadPool(() =>
            {
                MTProtoService.GetPasswordAsync(
                    result1 =>
                    {
                        var srpParams = result1 as IPasswordSRPParams;
                        if (srpParams == null)
                        {
                            BeginOnUIThread(() => IsWorking = false);
                            return;
                        }

                        var newSettings83 = newSettings as TLPasswordInputSettings83;
                        if (newSettings83 == null) return;

                        // calculate new password hash if password will be changed
                        TLString newPasswordHash = null;
                        var newAlgo = newSettings83.NewAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
                        if (newAlgo != null)
                        {
                            if (string.IsNullOrEmpty(newSettings83.NewPassword)) return;

                            newPasswordHash = SRP.GetX(new TLString(newSettings83.NewPassword), newAlgo); 

                            newSettings83.NewPasswordHash = SRP.GetVBytes(new TLString(newSettings83.NewPassword), newAlgo);
                        }

                        var currentPasswordHash = password.CurrentPasswordHash ?? TLString.Empty;
                        MTProtoService.UpdatePasswordSettingsAsync(SRP.GetCheck(currentPasswordHash, srpParams.SRPId, srpParams.SRPB, srpParams.CurrentAlgo), newSettings,
                           result2 =>
                           {
                               IsWorking = false;
                               MTProtoService.GetPasswordAsync(
                                   result3 => BeginOnUIThread(() =>
                                   {
                                       EventAggregator.Publish(result3);

                                       var password84 = result3 as TLPassword84;
                                       if (password84 != null)
                                       {
                                           password84.CurrentPasswordHash = newPasswordHash ?? password84.CurrentPasswordHash;
                                       }

                                       MessageBox.Show(AppResources.PasswordActive, AppResources.Success, MessageBoxButton.OK);

                                       if (password84 != null && (_authorizationForm != null || _secureValues != null))
                                       {
                                           MTProtoService.GetPasswordSettingsAsync(SRP.GetCheck(password84.CurrentPasswordHash, password84.SRPId, password84.SRPB, password84.CurrentAlgo),
                                               result4 => BeginOnUIThread(() =>
                                               {
                                                   Passport.EnterPasswordViewModel.NavigateToPassportCommon(
                                                       result4, password84, new TLString(PasswordViewModel.TempNewPassword),
                                                       _authorizationForm, _secureValues,
                                                       MTProtoService, StateService, NavigationService);
                                               }),
                                               error4 => BeginOnUIThread(() =>
                                               {
                                                   if (error4.TypeEquals(ErrorType.PASSWORD_HASH_INVALID))
                                                   {
                                                       StateService.AuthorizationForm = _authorizationForm;
                                                       StateService.Password = password84;
                                                       NavigationService.UriFor<Passport.EnterPasswordViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
                                                   }
                                               }));
                                       }
                                       else
                                       {
                                           StateService.Password = result3;
                                           NavigationService.GoBack();
                                       }
                                   }),
                                   error3 => BeginOnUIThread(() =>
                                   {
                                       Execute.ShowDebugMessage("account.getPassword error " + error3);
                                   }));
                           },
                           error2 => BeginOnUIThread(() =>
                           {
                               IsWorking = false;
                               var messageBuilder = new StringBuilder();
                               //messageBuilder.AppendLine(AppResources.Error);
                               //messageBuilder.AppendLine();
                               messageBuilder.AppendLine("Method: account.updatePasswordSettings");
                               messageBuilder.AppendLine("Result: " + error2);

                               if (TLRPCError.CodeEquals(error2, ErrorCode.FLOOD))
                               {
                                   HasError = true;
                                   Error = AppResources.FloodWaitString;
                                   Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.FloodWaitString, AppResources.Error, MessageBoxButton.OK));
                               }
                               else if (TLRPCError.CodeEquals(error2, ErrorCode.INTERNAL))
                               {
                                   HasError = true;
                                   Error = AppResources.ServerError;
                                   Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.ServerError, MessageBoxButton.OK));
                               }
                               else if (TLRPCError.CodeEquals(error2, ErrorCode.BAD_REQUEST))
                               {
                                   if (TLRPCError.TypeEquals(error2, ErrorType.PASSWORD_HASH_INVALID))
                                   {
                                       HasError = true;
                                       Error = string.Format("{0} {1}", error2.Code, error2.Message);
                                       Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                   }
                                   else if (TLRPCError.TypeEquals(error2, ErrorType.NEW_PASSWORD_BAD))
                                   {
                                       HasError = true;
                                       Error = string.Format("{0} {1}", error2.Code, error2.Message);
                                       Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                   }
                                   else if (TLRPCError.TypeEquals(error2, ErrorType.NEW_SALT_INVALID))
                                   {
                                       HasError = true;
                                       Error = string.Format("{0} {1}", error2.Code, error2.Message);
                                       Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                   }
                                   else if (TLRPCError.TypeEquals(error2, ErrorType.EMAIL_INVALID))
                                   {
                                       HasError = true;
                                       Error = AppResources.EmailInvalid;
                                       Telegram.Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.EmailInvalid, AppResources.Error, MessageBoxButton.OK));
                                   }
                                   else if (TLRPCError.TypeEquals(error2, ErrorType.EMAIL_UNCONFIRMED))
                                   {
                                       HasError = false;
                                       Error = string.Empty;
                                       MTProtoService.GetPasswordAsync(
                                            result3 => BeginOnUIThread(() =>
                                            {
                                                IsWorking = false;
                                                password = result3 as TLPassword84;
                                                if (password != null)
                                                {
                                                    password.CurrentPasswordHash = currentPasswordHash; //EMAIL_UNCONFIRMED - new settings are not active yet
                                                }

                                                MessageBox.Show(AppResources.CompletePasswordHint, AppResources.AlmostThere, MessageBoxButton.OK);

                                                if (_authorizationForm != null || _secureValues != null)
                                                {
                                                    StateService.AuthorizationForm = _authorizationForm;
                                                    StateService.SecureValues = _secureValues;
                                                    StateService.Password = result3;
                                                    StateService.RemoveBackEntry = true;
                                                    NavigationService.UriFor<PasswordViewModel>().Navigate();
                                                }
                                                else
                                                {
                                                    StateService.Password = result3;
                                                    NavigationService.GoBack();
                                                }
                                            }),
                                            error3 => Execute.BeginOnUIThread(() =>
                                            {
                                                IsWorking = false;
                                                Execute.ShowDebugMessage("account.getPassword error " + error3);
                                            }));
                                   }
                                   else
                                   {
                                       HasError = true;
                                       Error = string.Format("{0} {1}", error2.Code, error2.Message);
                                       Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                   }
                               }
                               else
                               {
                                   HasError = true;
                                   Error = string.Empty;
                                   Execute.ShowDebugMessage("account.updatePasswordSettings error " + error2);
                               }
                           }));
                    },
                    error1 => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                    }));
            });
        }
    }
}
