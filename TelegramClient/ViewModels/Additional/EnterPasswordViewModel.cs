// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
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
    public abstract class PasswordViewModelBase : ViewModelBase
    {
        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                SetField(ref _password, value, () => Password);
                NotifyOfPropertyChange(() => CanDone);
            }
        }

        public string PasswordHint { get; set; }

        protected TLPassword PasswordBase;

        private TLString _email;

        protected PasswordViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            PasswordBase = StateService.Password as TLPassword;

            if (PasswordBase != null)
            {
                PasswordHint = !TLString.IsNullOrEmpty(PasswordBase.Hint) ? PasswordBase.Hint.ToString() : string.Empty;
            }
        }

        public bool CanDone
        {
            get { return !string.IsNullOrEmpty(Password); }
        }

        protected virtual void OnSucceded(TLPasswordSettings passwordSettings)
        {
            
        }

        public void Done()
        {
            if (IsWorking) return;

            var password84 = PasswordBase as TLPassword84;
            if (password84 == null) return;

            var algo = password84.CurrentAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (algo == null) return;

            IsWorking = true;
            BeginOnThreadPool(() =>
            {
                var passwordHash = SRP.GetX(new TLString(_password), algo);

                MTProtoService.GetPasswordSettingsAsync(SRP.GetCheck(passwordHash, password84.SRPId, password84.SRPB, algo),
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;

                        PasswordViewModel.Password = _password;

                        PasswordBase.CurrentPasswordHash = passwordHash;
                        PasswordBase.Settings = result;

                        OnSucceded(result);
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        if (error.TypeEquals(ErrorType.PASSWORD_HASH_INVALID))
                        {
                            MTProtoService.GetPasswordAsync(
                                result1 => BeginOnUIThread(() =>
                                {
                                    PasswordBase = result1 as TLPassword84;
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
                                    PasswordBase = result1 as TLPassword84;
                                    Done();
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

        public void ForgotPassword()
        {
            if (PasswordBase == null) return;

            if (PasswordBase.HasRecovery.Value)
            {
                IsWorking = true;
                MTProtoService.RequestPasswordRecoveryAsync(
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        PasswordBase.EmailUnconfirmedPattern = result.EmailPattern;

                        MessageBox.Show(string.Format(AppResources.SentRecoveryCodeMessage, result.EmailPattern), AppResources.AppName, MessageBoxButton.OK);

                        StateService.Password = PasswordBase;
                        StateService.RemoveBackEntry = true;
                        NavigationService.UriFor<PasswordRecoveryViewModel>().Navigate();
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
                    }));
            }
            else
            {
                MessageBox.Show(AppResources.NoRecoveryEmailMessage, AppResources.Sorry, MessageBoxButton.OK);
            }
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }
    }

    public class EnterPasswordViewModel : PasswordViewModelBase
    {
        public EnterPasswordViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
        }

        protected override void OnSucceded(TLPasswordSettings passwordSettings)
        {
            StateService.RemoveBackEntry = true;
            StateService.Password = PasswordBase;
            NavigationService.UriFor<PasswordViewModel>().Navigate();
        }
    }
}
