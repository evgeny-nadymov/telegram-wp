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
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Payments
{
    public class WebVerificationViewModel : PaymentViewModelBase
    {

        public string Title
        {
            get
            {
                if (PaymentInfo != null)
                {
                    if (PaymentInfo.Receipt != null)
                    {
                        if (PaymentInfo.Receipt.Invoice.Test)
                        {
                            return AppResources.TestReceipt;
                        }

                        return AppResources.Receipt;
                    }

                    if (PaymentInfo.Form != null)
                    {
                        if (PaymentInfo.Form.Invoice.Test)
                        {
                            return AppResources.TestCheckout;
                        }

                        return AppResources.Checkout;
                    }
                }

                return AppResources.Checkout;
            }
        }

        public Uri Url { get; set; }

        public WebVerificationViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            if (StateService.Url != null)
            {
                Url = new Uri(StateService.Url, UriKind.Absolute);
                StateService.Url = null;
            }

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }
        }

        public override void NavigateToNextStep()
        {
            
        }
    }
}
