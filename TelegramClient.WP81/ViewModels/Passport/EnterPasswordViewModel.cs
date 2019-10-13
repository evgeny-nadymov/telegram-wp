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
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Passport
{
    public class EnterPasswordViewModel : PasswordViewModelBase
    {
        public TLUserBase Bot { get; set; }

        public string Hint
        {
            get
            {
                if (_authorizationForm == null)
                {
                    return AppResources.PassportSelfRequest;
                }
                
                return Bot == null
                    ? string.Empty
                    : string.Format(AppResources.PassportHint, Bot.FullName);
            }
        }

        public string BottomHint
        {
            get
            {
                if (_authorizationForm == null)
                {
                    return AppResources.PassportRequestPasswordInfo;
                }

                return null;
            }
        }

        public string RandomParam { get; set; }

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly IList<TLSecureValue> _secureValues;

        public static TLString Secret { get; protected set; }

        public EnterPasswordViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _authorizationForm = stateService.AuthorizationForm;
            stateService.AuthorizationForm = null;

            _secureValues = stateService.SecureValues;
            stateService.SecureValues = null;

            if (_authorizationForm != null)
            {
                Bot = _authorizationForm.Users.LastOrDefault();
            }
            else
            {
                Bot = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            var backEntry = NavigationService.BackStack.FirstOrDefault();
            while (backEntry != null
                && !backEntry.Source.ToString().EndsWith("ShellView.xaml")
                && !DialogDetailsViewModel.IsFirstEntryFromPeopleHub(backEntry, NavigationService.BackStack)
                && !DialogDetailsViewModel.IsFirstEntryFromTelegramUrl(backEntry, NavigationService.BackStack))
            {
                NavigationService.RemoveBackEntry();
                backEntry = NavigationService.BackStack.FirstOrDefault();
            }
        }

        protected override void OnSucceded(TLPasswordSettings passwordSettings)
        {
            NavigateToPassportCommon(
                passwordSettings, PasswordBase, new TLString(Password),
                _authorizationForm, _secureValues,
                MTProtoService, StateService, NavigationService);
        }

        public static void NavigateToPassportCommon(
            TLPasswordSettings passwordSettings, TLPassword passwordBase, TLString password, 
            TLAuthorizationForm authorizationForm, IList<TLSecureValue> secureValues, 
            IMTProtoService mtProtoService, IStateService stateService, INavigationService navigationService)
        {
            var passwordSettings83 = passwordSettings as TLPasswordSettings83;
            if (passwordSettings83 == null) return;

            var secureSettings = passwordSettings83.SecureSettings;

            passwordBase.Settings = passwordSettings83;

            var password84 = passwordBase as TLPassword84;
            if (password84 == null) return;

            var algo = password84.CurrentAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (algo == null) return;

            //passwordBase.CurrentPasswordHash = Utils.Password.GetHash(passwordBase.CurrentSalt, password);
            var passwordHash = SRP.GetX(password, algo);

            passwordBase.CurrentPasswordHash = passwordHash;
            
            // create secure_secret
            if (secureSettings == null || TLString.IsNullOrEmpty(secureSettings.SecureSecret))
            {
                AddSecureSecret(passwordSettings, passwordBase, password, authorizationForm, secureValues, mtProtoService, stateService, navigationService);
                return;
            }

            var values = authorizationForm != null ? authorizationForm.Values : secureValues;
            if (values == null) return;

            var decrypted = SetSecretAndDecryptSecureValues(passwordSettings83, password, values);
            if (!decrypted)
            {
                Execute.BeginOnUIThread(() =>
                {
                    ShellViewModel.ShowCustomMessageBox(
                        AppResources.PassportDataCorrupted, AppResources.AppName,
                        AppResources.Reset, AppResources.Cancel, 
                        dismissed =>
                        {
                            if (dismissed == CustomMessageBoxResult.RightButton)
                            {
                                ResetSecureSecret(passwordSettings, passwordBase, password, authorizationForm, secureValues, mtProtoService, stateService, navigationService);
                                values.Clear(); 
                            }
                        });
                });

                return;
            }

            stateService.RemoveBackEntry = true;
            stateService.AuthorizationForm = authorizationForm;
            stateService.SecureValues = secureValues;
            stateService.Password = passwordBase;
            Execute.BeginOnUIThread(() =>
            {
                if (authorizationForm != null)
                {
                    navigationService.UriFor<PassportViewModel>().Navigate();
                }
                else if (secureValues != null)
                {
                    navigationService.UriFor<PassportSettingsViewModel>().Navigate();
                }
            });
        }

        private static void ResetSecureSecret(
            TLPasswordSettings passwordSettings, TLPassword passwordBase, TLString password,
            TLAuthorizationForm authorizationForm, IList<TLSecureValue> secureValues,
            IMTProtoService mtProtoService, IStateService stateService, INavigationService navigationService)
        {
            var passwordSettings83 = passwordSettings as TLPasswordSettings83;
            if (passwordSettings83 == null) return;

            var password84 = passwordBase as TLPassword84;
            if (password84 == null) return;

            var newSecureSettings = new TLSecureSecretSettings
            {
                SecureAlgo = new TLSecurePasswordKdfAlgoUnknown(),
                SecureSecret = TLString.Empty,
                SecureSecretId = new TLLong(0)
            };

            var newSettings = new TLPasswordInputSettings83
            {
                Flags = new TLInt(0),
                NewSecureSettings = newSecureSettings,
            };

            mtProtoService.GetPasswordAsync(
                resul1 =>
                {
                    var srpParams = resul1 as IPasswordSRPParams;
                    if (srpParams == null) return;

                    var currentPasswordHash = passwordBase.CurrentPasswordHash ?? TLString.Empty;
                    mtProtoService.UpdatePasswordSettingsAsync(SRP.GetCheck(currentPasswordHash, srpParams.SRPId, srpParams.SRPB, srpParams.CurrentAlgo), newSettings,
                        result2 =>
                        {
                            passwordSettings83.SecureSettings = newSettings.NewSecureSettings;

                            NavigateToPassportCommon(
                                passwordSettings83, passwordBase, password,
                                authorizationForm, secureValues,
                                mtProtoService, stateService, navigationService);
                        },
                        error2 => { });
                },
                error1 => { });
        }

        private static void AddSecureSecret(
            TLPasswordSettings passwordSettings, TLPassword passwordBase, TLString password,
            TLAuthorizationForm authorizationForm, IList<TLSecureValue> secureValues,
            IMTProtoService mtProtoService, IStateService stateService, INavigationService navigationService)
        {
            var passwordSettings83 = passwordSettings as TLPasswordSettings83;
            if (passwordSettings83 == null) return;

            var password84 = passwordBase as TLPassword84;
            if (password84 == null) return;

            var newSettings = new TLPasswordInputSettings83 { Flags = new TLInt(0) };

            Utils.Password.AddRandomSecureSecret(newSettings, password84, password);

            mtProtoService.GetPasswordAsync(
                resul1 =>
                {
                    var srpParams = resul1 as IPasswordSRPParams;
                    if (srpParams == null) return;

                    var currentPasswordHash = passwordBase.CurrentPasswordHash ?? TLString.Empty;
                    mtProtoService.UpdatePasswordSettingsAsync(SRP.GetCheck(currentPasswordHash, srpParams.SRPId, srpParams.SRPB, srpParams.CurrentAlgo), newSettings,
                        result2 =>
                        {
                            passwordSettings83.SecureSettings = newSettings.NewSecureSettings;

                            NavigateToPassportCommon(
                                passwordSettings83, passwordBase, password,
                                authorizationForm, secureValues,
                                mtProtoService, stateService, navigationService);
                        },
                        error2 => { });
                },
                error1 => { });
        }

        protected static bool SetSecretAndDecryptSecureValues(TLPasswordSettings83 passwordSettings, TLString password, IList<TLSecureValue> values)
        {
            if (passwordSettings == null) return false;

            var secureSettings = passwordSettings.SecureSettings;
            if (secureSettings == null) return false;

            var secureAlgo = secureSettings.SecureAlgo;
            if (secureAlgo == null) return false;

            Secret = Utils.Passport.DecryptSecureSecret(
                secureSettings.SecureSecret,
                password,
                secureAlgo);

            // cannot decrypt secureSecret, corrupt data
            if (Secret == null) return false;

            if (values == null) return true;

            foreach (var value in values)
            {
                try
                {
                    if (value.Data != null)
                    {
                        var decryptedData = Utils.Passport.DecryptSecureValue(value, Secret);

                        if (!TLString.IsNullOrEmpty(decryptedData))
                        {
                            if (ResidentialAddressViewModel.IsValidDataType(value.Type))
                            {
                                value.Data.DecryptedData = JsonUtils.FromJSON<ResidentialAddressRootObject>(decryptedData.Data);
                            }
                            else if (PersonalDetailsViewModel.IsValidProofType(value.Type))
                            {
                                value.Data.DecryptedData = JsonUtils.FromJSON<PersonalDetailsDocumentRootObject>(decryptedData.Data);
                            }
                            else if (PersonalDetailsViewModel.IsValidDataType(value.Type))
                            {
                                value.Data.DecryptedData = JsonUtils.FromJSON<PersonalDetailsRootObject>(decryptedData.Data);
                            }
                            else
                            {
                                value.Data.DecryptedData = decryptedData;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Telegram.Api.Helpers.Execute.ShowDebugMessage(ex.ToString());
                }
            }

            return true;
        }
    }
}
