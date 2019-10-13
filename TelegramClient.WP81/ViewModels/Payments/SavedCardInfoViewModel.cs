// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Security.Cryptography;
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

namespace TelegramClient.ViewModels.Payments
{
    public class SavedCardInfoViewModel : PaymentViewModelBase
    {
        private readonly TLPaymentSavedCredentialsCard _savedCredentialsCard;

        public string Title { get; set; }

        public string Password { get; set; }

        public string Error { get; set; }

        public string SavedCardHint
        {
            get { return string.Format(AppResources.SavedCardHint, Title); }
        }

        public SavedCardInfoViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            if (PaymentInfo != null && PaymentInfo.Form != null && PaymentInfo.Form.SavedCredentials != null)
            {
                _savedCredentialsCard = PaymentInfo.Form.SavedCredentials as TLPaymentSavedCredentialsCard;
                if (_savedCredentialsCard != null)
                {
                    Title = _savedCredentialsCard.Title.ToString();
                }
            }
        }

        public void ChangeCredentials()
        {
            if (PaymentInfo.Receipt != null) return;
            if (PaymentInfo.Form == null) return;

            StateService.PaymentInfo = PaymentInfo;

            if (PaymentInfo.Form.IsNativeProvider)
            {
                StateService.RemoveCheckoutAndCardView = true;
                NavigationService.UriFor<CardInfoViewModel>().Navigate();
            }
            else
            {
                StateService.RemoveCheckoutAndCardView = true;
                NavigationService.UriFor<WebCardInfoViewModel>().Navigate();
            }
        }

        public void Validate()
        {
            if (_savedCredentialsCard == null) return;

            if (string.IsNullOrEmpty(Password))
            {
                Error = "PASSWORD";
                NotifyOfPropertyChange(() => Error);
                return;
            }

            IsWorking = true;
            MTProtoService.GetPasswordAsync(
                result =>
                {
                    var password84 = result as TLPassword84;
                    if (password84 == null)
                    {
                        return;
                    }

                    var algo = password84.CurrentAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
                    if (algo == null)
                    {
                        return;
                    }
                    //var passwordHash = Utils.Password.GetHash(password.CurrentSalt, new TLString(Password));
                    var passwordHash = SRP.GetX(new TLString(Password), algo);
                    var password = SRP.GetCheck(passwordHash, password84.SRPId, password84.SRPB, algo);

                    MTProtoService.GetTmpPasswordAsync(
                        password, new TLInt(Constants.DefaultTmpPasswordLifetime),
                        result2 => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            PaymentInfo.Credentials = new TLInputPaymentCredentialsSaved
                            {
                                Id = _savedCredentialsCard.Id,
                                TmpPassword = result2.TmpPassword
                            };
                            PaymentInfo.CredentialsTitle = Title;
                            StateService.SaveTmpPassword(result2);
                            NavigateToNextStep();
                        }),
                        error2 => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            var messageBuilder = new StringBuilder();
                            messageBuilder.AppendLine("Method: account.getTmpPassword");
                            messageBuilder.AppendLine("Result: " + error2);

                            if (TLRPCError.CodeEquals(error2, ErrorCode.FLOOD))
                            {
                                Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.FloodWaitString, AppResources.Error, MessageBoxButton.OK));
                            }
                            else if (TLRPCError.CodeEquals(error2, ErrorCode.INTERNAL))
                            {
                                Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.ServerError, MessageBoxButton.OK));
                            }
                            else if (TLRPCError.CodeEquals(error2, ErrorCode.BAD_REQUEST))
                            {
                                if (TLRPCError.TypeEquals(error2, ErrorType.PASSWORD_HASH_INVALID))
                                {
                                    Error = string.Format("PASSWORD");
                                    NotifyOfPropertyChange(() => Error);
                                }
                                else
                                {
                                    Execute.BeginOnUIThread(() => MessageBox.Show(messageBuilder.ToString(), AppResources.Error, MessageBoxButton.OK));
                                }
                            }
                            else
                            {
                                Execute.ShowDebugMessage("account.getTmpPassword error " + error2);
                            }
                        }));
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }));
        }

        public override void NavigateToNextStep()
        {
            StateService.PaymentInfo = PaymentInfo;
            NavigationService.UriFor<CheckoutViewModel>().Navigate();
        }
    }
}
