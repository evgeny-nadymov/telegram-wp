// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Additional;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Auth
{
    public class ConfirmPasswordViewModel : ViewModelBase
    {
        private Visibility _resetAccountVisibility = Visibility.Collapsed;

        public Visibility ResetAccountVisibility
        {
            get { return _resetAccountVisibility; }
            set { SetField(ref _resetAccountVisibility, value, () => ResetAccountVisibility); }
        }

        private string _code;

        public string Code
        {
            get { return _code; }
            set { SetField(ref _code, value, () => Code); }
        }

        private TLPassword _password;

        public string PasswordHint { get; set; }

        private readonly TLSentCodeBase _sentCode;

        public ConfirmPasswordViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _password = StateService.Password as TLPassword;
            StateService.Password = null;

            _sentCode = StateService.SentCode;
            StateService.SentCode = null;

            if (_password != null)
            {
                PasswordHint = !TLString.IsNullOrEmpty(_password.Hint) ? _password.Hint.ToString() : string.Empty;
            }

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Code))
                {
                    NotifyOfPropertyChange(() => CanConfirm);
                }
            };
        }

        public bool CanConfirm
        {
            get { return !string.IsNullOrEmpty(Code); }
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


        public void Confirm()
        {
            if (_password == null) return;
            if (!CanConfirm) return;

            var password84 = _password as TLPassword84;
            if (password84 == null)
            {
                return;
            }

            var algo = password84.CurrentAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (algo == null)
            {
                return;
            }

            IsWorking = true;
#if LOG_REGISTRATION
            TLUtils.WriteLog("auth.checkPassword");
#endif

            BeginOnThreadPool(() =>
            {
                //var passwordHash = Utils.Password.GetHash(_password.CurrentSalt, new TLString(Code));
                var passwordHash = SRP.GetX(new TLString(Code), algo);
                var password = SRP.GetCheck(passwordHash, password84.SRPId, password84.SRPB, algo);

                MTProtoService.CheckPasswordAsync(
                    password,
                    auth => BeginOnUIThread(() =>
                    {
#if LOG_REGISTRATION
                        TLUtils.WriteLog("auth.checkPassword result " + auth);
                        TLUtils.WriteLog("TLUtils.IsLogEnabled=false");
#endif

                        TLUtils.IsLogEnabled = false;
                        TLUtils.LogItems.Clear();

                        var result = MessageBox.Show(
                            AppResources.ConfirmPushMessage,
                            AppResources.ConfirmPushTitle,
                            MessageBoxButton.OKCancel);

                        if (result != MessageBoxResult.OK)
                        {
                            Notifications.Disable();
                        }
                        else
                        {
                            Notifications.Enable();
                        }

                        ConfirmViewModel.UpdateNotificationsAsync(MTProtoService, StateService);

                        MTProtoService.SetInitState();
                        StateService.CurrentUserId = auth.User.Index;
                        StateService.FirstRun = true;
                        SettingsHelper.SetValue(Constants.IsAuthorizedKey, true);

                        ShellViewModel.Navigate(NavigationService);

                        IsWorking = false;
                    }),
                    error => BeginOnUIThread(() =>
                    {
#if LOG_REGISTRATION
                        TLUtils.WriteLog("auth.checkPassword error " + error);
#endif
                        IsWorking = false;
                        if (error.TypeEquals(ErrorType.PASSWORD_HASH_INVALID))
                        {
                            MTProtoService.GetPasswordAsync(
                                result1 => BeginOnUIThread(() =>
                                {
                                    _password = result1 as TLPassword84;
                                    MessageBox.Show(AppResources.PasswordInvalidString, AppResources.Error, MessageBoxButton.OK);
                                }),
                                error1 => BeginOnUIThread(() =>
                                {
                                    MessageBox.Show(AppResources.PasswordInvalidString, AppResources.Error, MessageBoxButton.OK);
                                }));
                        }
                        else if (error.CodeEquals(ErrorCode.FLOOD))
                        {
                            MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (error.TypeEquals(ErrorType.SRP_ID_INVALID))
                        {
                            IsWorking = true;
                            MTProtoService.GetPasswordAsync(
                                result1 => BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                    _password = result1 as TLPassword84;
                                    Confirm();
                                }),
                                error1 => BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                }));
                        }
                        else
                        {
                            Execute.ShowDebugMessage("account.checkPassword error " + error);
                        }
                    }));
            });
        }

        public void ResetAccount()
        {
            BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
            {
                var r = MessageBox.Show(AppResources.ResetMyAccountConfirmation, AppResources.Warning, MessageBoxButton.OKCancel);
                if (r != MessageBoxResult.OK) return;

                IsWorking = true;
                MTProtoService.DeleteAccountAsync(
                    new TLString("Forgot password"),
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        StateService.RemoveBackEntry = true;
                        StateService.SentCode = _sentCode;
                        NavigationService.UriFor<SignUpViewModel>().Navigate();
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        if (error.TypeEquals("2FA_RECENT_CONFIRM"))
                        {
                            MessageBox.Show(AppResources.ResetAccountError, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (error.TypeStarsWith("2FA_CONFIRM_WAIT"))
                        {
                            var message = error.Message.ToString().Replace("2FA_CONFIRM_WAIT_", string.Empty);
                            try
                            {
                                var confirmWait = Convert.ToInt32(message);
                                StateService.ConfirmWait = confirmWait;
                                StateService.SentCode = _sentCode;
                                NavigationService.UriFor<ResetAccountViewModel>().Navigate();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                        else
                        {
                            Execute.ShowDebugMessage("account.deleteAccount error " + error);
                        }
                    }));
            });
        }

        public void ForgotPassword()
        {
            if (_password == null) return;

            BeginOnUIThread(() =>
            {
                if (_password.HasRecovery.Value)
                {
                    IsWorking = true;
                    MTProtoService.RequestPasswordRecoveryAsync(
                        result => BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            _password.EmailUnconfirmedPattern = result.EmailPattern;
                            _password.IsAuthRecovery = true;

                            MessageBox.Show(string.Format(AppResources.SentRecoveryCodeMessage, result.EmailPattern), AppResources.AppName, MessageBoxButton.OK);

                            StateService.Password = _password;
                            StateService.RemoveBackEntry = true;
                            NavigationService.UriFor<PasswordRecoveryViewModel>().Navigate();

                            ResetAccountVisibility = Visibility.Visible;
                        }),
                        error => BeginOnUIThread(() =>
                        {
                            IsWorking = false;

                            var messageBuilder = new StringBuilder();
                            messageBuilder.AppendLine(AppResources.Error);
                            messageBuilder.AppendLine();
                            messageBuilder.AppendLine("Method: account.requestPasswordRecovery");
                            messageBuilder.AppendLine("Result: " + error);

                            if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                            {
                                Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK));
                            }
                            else if (TLRPCError.CodeEquals(error, ErrorCode.INTERNAL))
                            {
                                Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.ServerError, MessageBoxButton.OK));
                            }
                            else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                            {
                                if (TLRPCError.TypeEquals(error, ErrorType.PASSWORD_EMPTY))
                                {
                                    Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                }
                                else if (TLRPCError.TypeEquals(error, ErrorType.PASSWORD_RECOVERY_NA))
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
                                Execute.ShowDebugMessage("account.requestPasswordRecovery error " + error);
                            }

                            ResetAccountVisibility = Visibility.Visible;
                        }));
                }
                else
                {
                    MessageBox.Show(AppResources.NoRecoveryEmailMessage, AppResources.Sorry, MessageBoxButton.OK);
                    ResetAccountVisibility = Visibility.Visible;
                }
            });
        }
    }
}
