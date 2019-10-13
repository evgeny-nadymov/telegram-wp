// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.Views.Dialogs;

namespace TelegramClient.ViewModels.Payments
{
    public abstract class PaymentViewModelBase : ViewModelBase
    {
        public PaymentInfo PaymentInfo { get; set; }

        protected PaymentViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            PaymentInfo = stateService.PaymentInfo;
            stateService.PaymentInfo = null;
        }

        public abstract void NavigateToNextStep();

        public static void NavigateToCardInfo(IDialogDetailsView view, PaymentInfo paymentInfo, Func<TLTmpPassword> getTmpPassword,
            INavigationService navigationService)
        {
            if (paymentInfo == null) return;

            var form = paymentInfo.Form;
            if (form == null) return;

            var useSavedCredentials = false;
            var savedCredentialsCard = form.SavedCredentials as TLPaymentSavedCredentialsCard;
            TLTmpPassword tmpPassword = null;
            if (savedCredentialsCard != null && !form.PasswordMissing)
            {
                tmpPassword = getTmpPassword.Invoke();
                if (tmpPassword != null)
                {
                    var now = TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now);
                    if (tmpPassword.ValidUntil.Value - 60 > now.Value)
                    {
                        useSavedCredentials = true;
                    }
                }
            }

            if (useSavedCredentials)
            {
                paymentInfo.Credentials = new TLInputPaymentCredentialsSaved
                {
                    Id = savedCredentialsCard.Id,
                    TmpPassword = tmpPassword.TmpPassword
                };
                paymentInfo.CredentialsTitle = savedCredentialsCard.Title.ToString();

                Navigate<CheckoutViewModel>(view, navigationService);
            }
            else if (savedCredentialsCard != null && !form.PasswordMissing)
            {
                Navigate<SavedCardInfoViewModel>(view, navigationService);
            }
            else
            {
                if (form.IsNativeProvider)
                {
                    Navigate<CardInfoViewModel>(view, navigationService);
                }
                else
                {
                    Navigate<WebCardInfoViewModel>(view, navigationService);
                }
            }
        }

        private static void Navigate<T>(IDialogDetailsView view, INavigationService navigationService)
        {
            if (view != null)
            {
                view.StopPlayersAndCreateBitmapCache(() =>
                {
                    navigationService.UriFor<T>().Navigate();
                });
            }
            else
            {
                navigationService.UriFor<T>().Navigate();
            }
        }
    }
}
