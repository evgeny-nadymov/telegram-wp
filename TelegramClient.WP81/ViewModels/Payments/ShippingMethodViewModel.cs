// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Payments
{
    public class ShippingMethodViewModel : PaymentViewModelBase
    {
        public ShippingMethodViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            if (PaymentInfo != null
                && PaymentInfo.Form != null
                && PaymentInfo.ValidatedInfo != null
                && PaymentInfo.ValidatedInfo.ShippingOptions != null)
            {
                for (var i = 0; i < PaymentInfo.ValidatedInfo.ShippingOptions.Count; i++)
                {
                    PaymentInfo.ValidatedInfo.ShippingOptions[i].IsSelected = i == 0;
                    foreach (var price in PaymentInfo.ValidatedInfo.ShippingOptions[i].Prices)
                    {
                        price.Currency = PaymentInfo.Form.Invoice.Currency;
                    }
                }
            }
        }

        public void Validate()
        {
            NavigateToNextStep();
        }

        public override void NavigateToNextStep()
        {
            StateService.PaymentInfo = PaymentInfo;
            NavigateToCardInfo(null, StateService.PaymentInfo, () => StateService.GetTmpPassword(), NavigationService);
        }
    }
}
