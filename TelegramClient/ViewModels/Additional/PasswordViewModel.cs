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
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class PasswordViewModel : ViewModelBase
    {
        public Visibility ChangePasswordVisibility
        {
            get
            {
                if (_password is TLPassword && _password.HasPassword)
                {
                    return Visibility.Visible;
                }

                if (_password is TLPassword && !_password.HasPassword && TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern))
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public Visibility CompletePasswordVisibility
        {
            get
            {
                if (_password is TLPassword && _password.HasPassword)
                {
                    return Visibility.Collapsed;
                }

                if (_password is TLPassword && !_password.HasPassword && TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern))
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        private TLPasswordBase _password;

        private bool _passwordEnabled;

        public bool PasswordEnabled
        {
            get { return _passwordEnabled; }
            set { SetField(ref _passwordEnabled, value, () => PasswordEnabled); }
        }

        private Visibility _recoveryEmailUnconfirmedVisibility;

        public Visibility RecoveryEmailUnconfirmedVisibility
        {
            get { return _recoveryEmailUnconfirmedVisibility; }
            set
            {
                SetField(ref _recoveryEmailUnconfirmedVisibility, value, () => RecoveryEmailUnconfirmedVisibility);
                NotifyOfPropertyChange(() => RecoveryEmailUnconfirmedHint);
            }
        }

        public string RecoveryEmailUnconfirmedHint
        {
            get
            {
                if (!TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern))
                {
                    return string.Format(AppResources.RecoveryEmailPending, _password.EmailUnconfirmedPattern);
                }

                var password = _password as TLPassword;
                if (password != null && password.HasRecovery.Value)
                {
                    return AppResources.RecoveryEmailComplete;
                }

                return string.Empty;
            }
        }

        public string CompletePasswordLabel
        {
            get
            {
                if (!TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern))
                {
                    return string.Format(AppResources.StepsToCompleteToTwoStepsVerificationSetup, _password.EmailUnconfirmedPattern);
                }

                return AppResources.StepsToCompleteToTwoStepsVerificationSetup;
            }
        }

        public string RecoveryEmailLabel
        {
            get
            {
                if (_password != null)
                {
                    if (!TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern))
                    {
                        return AppResources.ChangeRecoveryEmail;
                    }

                    var password = _password as TLPassword;
                    if (password != null && password.HasRecovery.Value)
                    {
                        return AppResources.ChangeRecoveryEmail;
                    }
                }
                return AppResources.SetRecoveryEmail;
            }
        }

        private bool _suppressPasswordEnabled;

        private readonly DispatcherTimer _checkPasswordSettingsTimer = new DispatcherTimer();

        private void StartTimer()
        {
            if (!TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern))
            {
                _checkPasswordSettingsTimer.Start();
            }
        }

        private void StopTimer()
        {
            _checkPasswordSettingsTimer.Stop();
        }

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly IList<TLSecureValue> _secureValues;

        public static string Password { get; set; }

        public static string TempNewPassword { get; set; }

        public static TLString Secret { get; protected set; }

        public PasswordViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _checkPasswordSettingsTimer.Tick += OnCheckPasswordSettings;
            _checkPasswordSettingsTimer.Interval = TimeSpan.FromSeconds(5.0);

            _password = StateService.Password;
            StateService.Password = null;

            _authorizationForm = StateService.AuthorizationForm;
            StateService.AuthorizationForm = null;

            _secureValues = StateService.SecureValues;
            StateService.SecureValues = null;

            if (_password != null)
            {
                PasswordEnabled = _password.IsAvailable;
                RecoveryEmailUnconfirmedVisibility = !TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern) || (_password is TLPassword && ((TLPassword)_password).HasRecovery.Value)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            PropertyChanged += (o, e) =>
            {
                if (Property.NameEquals(e.PropertyName, () => PasswordEnabled) && !_suppressPasswordEnabled)
                {
                    if (PasswordEnabled)
                    {
                        ChangePassword();
                    }
                    else
                    {
                        ClearPassword();
                    }
                }
            };
        }

        public void ChangePasswordEnabled()
        {
            if (!PasswordEnabled)
            {
                PasswordEnabled = true;
                return;
            }

            var message = AppResources.TurnPasswordOffQuestion;
            var password = _password as TLPassword81;
            if (password != null && password.HasSecureValues)
            {
                message = string.Format("{0}\n\n{1}", message, AppResources.TurnPasswordOffPassport);
            }

            ShellViewModel.ShowCustomMessageBox(
                message, AppResources.AppName,
                AppResources.Ok, AppResources.Cancel,
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        PasswordEnabled = false;
                    }
                });
        }

        private void OnCheckPasswordSettings(object sender, System.EventArgs e)
        {
            Execute.ShowDebugMessage("account.getPasswordSettings");

            MTProtoService.GetPasswordAsync(
                result => BeginOnUIThread(() =>
                {
                    var password84 = result as TLPassword84;
                    if (password84 != null && password84.HasRecovery.Value)
                    {
                        var algo = password84.CurrentAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
                        if (algo == null) return;

                        password84.CurrentPasswordHash = SRP.GetX(new TLString(TempNewPassword), algo);
                        //password.CurrentPasswordHash = Utils.Password.GetHash(password.CurrentSalt, new TLString(TempNewPassword));

                        _password = password84;

                        RecoveryEmailUnconfirmedVisibility = !TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern) || (_password is TLPassword && ((TLPassword)_password).HasRecovery.Value)
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        NotifyOfPropertyChange(() => RecoveryEmailLabel);
                        NotifyOfPropertyChange(() => CompletePasswordLabel);
                        NotifyOfPropertyChange(() => ChangePasswordVisibility);
                        NotifyOfPropertyChange(() => CompletePasswordVisibility);

                        StopTimer();

                        if (_authorizationForm != null || _secureValues != null)
                        {
                            MTProtoService.GetPasswordSettingsAsync(SRP.GetCheck(password84.CurrentPasswordHash, password84.SRPId, password84.SRPB, algo),
                                result2 => BeginOnUIThread(() =>
                                {
                                    Passport.EnterPasswordViewModel.NavigateToPassportCommon(
                                        result2, password84, new TLString(TempNewPassword), 
                                        _authorizationForm, _secureValues, 
                                        MTProtoService, StateService, NavigationService);
                                }),
                                error => BeginOnUIThread(() =>
                                {
                                    if (error.TypeEquals(ErrorType.PASSWORD_HASH_INVALID))
                                    {
                                        StateService.AuthorizationForm = _authorizationForm;
                                        StateService.Password = result;
                                        NavigationService.UriFor<Passport.EnterPasswordViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
                                    }
                                }));
                        }
                    }
                }),
                error =>
                {
                    Execute.ShowDebugMessage("account.getPasswordSettings error " + error);
                });
        }

        protected override void OnActivate()
        {
            StartTimer();

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            if (StateService.Password == null)
            {
                _suppressPasswordEnabled = true;
                if (_password != null)
                {
                    PasswordEnabled = _password.IsAvailable;
                    RecoveryEmailUnconfirmedVisibility = !TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern) || (_password is TLPassword && ((TLPassword)_password).HasRecovery.Value)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    NotifyOfPropertyChange(() => RecoveryEmailLabel);
                    NotifyOfPropertyChange(() => CompletePasswordLabel);
                    NotifyOfPropertyChange(() => ChangePasswordVisibility);
                    NotifyOfPropertyChange(() => CompletePasswordVisibility);
                }
                _suppressPasswordEnabled = false;
            }
            else
            {
                _password = StateService.Password;
                StateService.Password = null; 

                StartTimer();

                RecoveryEmailUnconfirmedVisibility = !TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern) || (_password is TLPassword && ((TLPassword)_password).HasRecovery.Value)
                     ? Visibility.Visible
                     : Visibility.Collapsed;
                NotifyOfPropertyChange(() => RecoveryEmailLabel);
                NotifyOfPropertyChange(() => CompletePasswordLabel);
                NotifyOfPropertyChange(() => ChangePasswordVisibility);
                NotifyOfPropertyChange(() => CompletePasswordVisibility);
            }

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            StopTimer();

            base.OnDeactivate(close);
        }

        private void ClearPassword()
        {
            var password = _password as TLPassword84;
            if (password == null) return;

            IsWorking = true;
            BeginOnThreadPool(() =>
            {
                var newSettings = new TLPasswordInputSettings83
                {
                    Flags = new TLInt(0),
                    NewAlgo = new TLPasswordKdfAlgoUnknown(),
                    NewPasswordHash = TLString.Empty,
                    Hint = TLString.Empty,
                    Email = TLString.Empty
                };

                MTProtoService.GetPasswordAsync(
                    result1 => 
                    {
                        var srpParams = result1 as IPasswordSRPParams;
                        if (srpParams == null)
                        {
                            BeginOnUIThread(() => IsWorking = false);
                            return;
                        }

                        var currentPasswordHash = password.CurrentPasswordHash ?? TLString.Empty;
                        MTProtoService.UpdatePasswordSettingsAsync(SRP.GetCheck(currentPasswordHash, srpParams.SRPId, srpParams.SRPB, srpParams.CurrentAlgo), newSettings,
                            result => BeginOnUIThread(() =>
                            {
                                StopTimer();

                                IsWorking = false;
                                var noPassword = new TLPassword84
                                {
                                    Flags = new TLInt(0),
                                    EmailUnconfirmedPattern = TLString.Empty,
                                    NewSecureSalt = TLString.Empty,
                                    SecretRandom = TLString.Empty
                                };

                                var password81 = _password as TLPassword84;
                                if (password81 != null)
                                {
                                    noPassword.NewAlgo = password81.NewAlgo;
                                    noPassword.NewSecureSalt = password81.NewSecureSalt;
                                    noPassword.SecretRandom = password81.SecretRandom;
                                }

                                _password = noPassword;
                                if (_password != null)
                                {
                                    PasswordEnabled = _password.IsAvailable;
                                    RecoveryEmailUnconfirmedVisibility = !TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern) || (_password is TLPassword && ((TLPassword)_password).HasRecovery.Value)
                                        ? Visibility.Visible
                                        : Visibility.Collapsed;
                                    NotifyOfPropertyChange(() => RecoveryEmailLabel);
                                    NotifyOfPropertyChange(() => CompletePasswordLabel);
                                    NotifyOfPropertyChange(() => ChangePasswordVisibility);
                                    NotifyOfPropertyChange(() => CompletePasswordVisibility);
                                }
                                EventAggregator.Publish(_password);
                            }),
                            error => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                var messageBuilder = new StringBuilder();
                                //messageBuilder.AppendLine(AppResources.ServerErrorMessage);
                                //messageBuilder.AppendLine();
                                messageBuilder.AppendLine("Method: account.updatePasswordSettings");
                                messageBuilder.AppendLine("Result: " + error);

                                if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                                {
                                    Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.FloodWaitString, AppResources.Error, MessageBoxButton.OK));
                                }
                                else if (TLRPCError.CodeEquals(error, ErrorCode.INTERNAL))
                                {
                                    Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.ServerError, MessageBoxButton.OK));
                                }
                                else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                                {
                                    if (TLRPCError.TypeEquals(error, ErrorType.PASSWORD_HASH_INVALID))
                                    {
                                        Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                    }
                                    else if (TLRPCError.TypeEquals(error, ErrorType.NEW_PASSWORD_BAD))
                                    {
                                        Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                    }
                                    else if (TLRPCError.TypeEquals(error, ErrorType.NEW_SALT_INVALID))
                                    {
                                        Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                    }
                                    else if (TLRPCError.TypeEquals(error, ErrorType.EMAIL_INVALID))
                                    {
                                        Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.EmailInvalid, AppResources.Error, MessageBoxButton.OK));
                                    }
                                    else if (TLRPCError.TypeEquals(error, ErrorType.EMAIL_UNCONFIRMED))
                                    {
                                        Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                    }
                                    else
                                    {
                                        Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                    }
                                }
                                else
                                {
                                    Execute.ShowDebugMessage("account.updatePasswordSettings error " + error);
                                }
                            }));
                    },
                    error1 => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                    }));
            });
        }

        public void AbortPassword()
        {
            ClearPassword();
        }

        public void ChangePassword()
        {
            StateService.Password = _password;
            NavigationService.UriFor<ChangePasswordViewModel>().Navigate();
        }

        public void ChangeRecoveryEmail()
        {
            StateService.Password = _password;
            NavigationService.UriFor<ChangePasswordEmailViewModel>().Navigate();
        }
    }
}
