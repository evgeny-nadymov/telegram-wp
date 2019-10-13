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
using System.Net;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls.Utils;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Payments
{
    public class CheckoutViewModel : PaymentViewModelBase
    {
        public TLInvoice Invoice { get; set; }

        public TLPaymentRequestedInfo SavedInfo { get; set; }

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

        public string PaymentProvider
        {
            get
            {
                if (PaymentInfo != null)
                {
                    if (PaymentInfo.Receipt != null)
                    {
                        var providerId = PaymentInfo.Receipt.ProviderId;
                        var paymentProvider = PaymentInfo.Receipt.Users.FirstOrDefault(x => x.Id.Value == providerId.Value) as TLUser;
                        if (paymentProvider != null)
                        {
                            return paymentProvider.FullName2;
                        }

                        return string.Empty;
                    }

                    if (PaymentInfo.Form != null)
                    {
                        var providerId = PaymentInfo.Form.ProviderId;
                        var paymentProvider = PaymentInfo.Form.Users.FirstOrDefault(x => x.Id.Value == providerId.Value) as TLUser;
                        if (paymentProvider != null)
                        {
                            return paymentProvider.FullName2;
                        }

                        return string.Empty;
                    }
                }

                return AppResources.Checkout;
            }
        }

        public List<TLLabeledPrice> Prices { get; set; }

        public long TotalAmount
        {
            get
            {
                if (Prices != null)
                {
                    return Prices.Sum(x => x.Amount.Value);
                }

                return 0;
            }
        }

        public TLLabeledPrice Total
        {
            get
            {
                var price = new TLLabeledPrice
                {
                    Amount = new TLLong(TotalAmount),
                    Label = new TLString(AppResources.Total),
                    Currency = PaymentInfo != null && PaymentInfo.Form != null ? PaymentInfo.Form.Invoice.Currency : null
                };

                if (PaymentInfo != null)
                {
                    if (PaymentInfo.Form != null)
                    {
                        price.Currency = PaymentInfo.Form.Invoice.Currency;
                    }
                    else if (PaymentInfo.Receipt != null)
                    {
                        price.Currency = PaymentInfo.Receipt.Invoice.Currency;
                    }
                }

                return price;
            }
        }

        public string PayString
        {
            get
            {
                return string.Format("{0} {1}", AppResources.Pay, new LabeledPriceToStringConverter().Convert(Total, null, null, null));
            }
        }

        private TLUserBase _bot;

        public TLUserBase Bot
        {
            get
            {
                if (_bot == null && PaymentInfo != null)
                {
                    if (PaymentInfo.Form != null)
                    {
                        _bot = CacheService.GetUser(PaymentInfo.Form.BotId);
                    }
                    else if (PaymentInfo.Receipt != null)
                    {
                        _bot = CacheService.GetUser(PaymentInfo.Receipt.BotId);
                    }
                }

                return _bot;
            }
        }

        public TLShippingOption ShippingOption { get; set; }

        public CheckoutViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Prices = new List<TLLabeledPrice>();

            if (PaymentInfo != null)
            {
                if (PaymentInfo.Form != null)
                {
                    Invoice = PaymentInfo.Form.Invoice;
                    SavedInfo = PaymentInfo.Form.SavedInfo;
                    foreach (var price in PaymentInfo.Form.Invoice.Prices)
                    {
                        price.Currency = PaymentInfo.Form.Invoice.Currency;
                        Prices.Add(price);
                    }

                    if (PaymentInfo.ValidatedInfo != null
                        && PaymentInfo.ValidatedInfo.ShippingOptions != null)
                    {
                        foreach (var shippingOption in PaymentInfo.ValidatedInfo.ShippingOptions)
                        {
                            if (shippingOption.IsSelected)
                            {
                                ShippingOption = shippingOption;
                                foreach (var price in shippingOption.Prices)
                                {
                                    price.Currency = PaymentInfo.Form.Invoice.Currency;
                                    Prices.Add(price);
                                }
                                break;
                            }
                        }
                    }
                }
                else if (PaymentInfo.Receipt != null)
                {
                    Invoice = PaymentInfo.Receipt.Invoice;
                    SavedInfo = PaymentInfo.Receipt.SavedInfo;
                    PaymentInfo.CredentialsTitle = PaymentInfo.Receipt.CredentialsTitle.ToString();

                    foreach (var price in PaymentInfo.Receipt.Invoice.Prices)
                    {
                        price.Currency = PaymentInfo.Receipt.Invoice.Currency;
                        Prices.Add(price);
                    }

                    var shippingOption = PaymentInfo.Receipt.Shipping;
                    if (shippingOption != null)
                    {
                        ShippingOption = shippingOption;
                        foreach (var price in shippingOption.Prices)
                        {
                            price.Currency = PaymentInfo.Receipt.Invoice.Currency;
                            Prices.Add(price);
                        }
                    }
                }
            }
        }

        public void Validate()
        {
            if (PaymentInfo == null) return;
            if (PaymentInfo.Form == null) return;
            if (PaymentInfo.Message == null) return;
            if (PaymentInfo.Credentials == null) return;

            var paymentCredentials = PaymentInfo.Credentials;
            TLString validatedInfoId = null;
            TLString shippingOptionId = null;
            if (PaymentInfo.ValidatedInfo != null)
            {
                validatedInfoId = PaymentInfo.ValidatedInfo.Id;

                if (PaymentInfo.ValidatedInfo.ShippingOptions != null)
                {
                    foreach (var shippingOption in PaymentInfo.ValidatedInfo.ShippingOptions)
                    {
                        if (shippingOption.IsSelected)
                        {
                            shippingOptionId = shippingOption.Id;
                            break;
                        }
                    }
                }
            }

            var mediaInvoice = PaymentInfo.Message.Media as TLMessageMediaInvoice;
            if (mediaInvoice == null) return;

            var bot = CacheService.GetUser(PaymentInfo.Form.BotId) as TLUser45;
            if (bot == null) return;

            if (!bot.IsVerified && !bot.BotPaymentsPermission)
            {
                MessageBox.Show(string.Format(AppResources.PaymentsWarning, bot.FullName), string.Empty, MessageBoxButton.OK);
                bot.BotPaymentsPermission = true;
            }

            var confirmation = MessageBox.Show(string.Format(AppResources.TransactionConfirmation, Currency.GetString(TotalAmount, PaymentInfo.Form.Invoice.Currency.ToString()), bot.FullName, mediaInvoice.Title), AppResources.TransactionReview, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            IsWorking = true;
            MTProtoService.SendPaymentFormAsync(PaymentInfo.Message.Id, validatedInfoId, shippingOptionId, paymentCredentials,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    PaymentInfo.Result = result;
                    NavigateToNextStep();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }));
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

        public override void NavigateToNextStep()
        {
            var paymentResult = PaymentInfo.Result as TLPaymentResult;
            if (paymentResult != null)
            {
                var backEnrty = NavigationService.BackStack.FirstOrDefault();
                while (backEnrty != null
                    && !backEnrty.Source.ToString().Contains("DialogDetailsView.xaml")
                    && !backEnrty.Source.ToString().Contains("ShellView.xaml"))
                {
                    NavigationService.RemoveBackEntry();
                    backEnrty = NavigationService.BackStack.FirstOrDefault();
                }
                if (backEnrty != null && backEnrty.Source.ToString().Contains("DialogDetailsView.xaml"))
                {
                    StateService.ShowScrollDownButton = true;
                }
                NavigationService.GoBack();

                return;
            }

            var paymentVerificationNeeded = PaymentInfo.Result as TLPaymentVerificationNeeded;
            if (paymentVerificationNeeded != null)
            {
                if (!TLString.IsNullOrEmpty(paymentVerificationNeeded.Url))
                {
                    var backEnrty = NavigationService.BackStack.FirstOrDefault();
                    while (backEnrty != null
                        && !backEnrty.Source.ToString().Contains("DialogDetailsView.xaml")
                        && !backEnrty.Source.ToString().Contains("ShellView.xaml"))
                    {
                        NavigationService.RemoveBackEntry();
                        backEnrty = NavigationService.BackStack.FirstOrDefault();
                    }
                    if (backEnrty != null && backEnrty.Source.ToString().Contains("DialogDetailsView.xaml"))
                    {
                        StateService.ShowScrollDownButton = true;
                    }

                    StateService.RemoveBackEntry = true;
                    StateService.Url = paymentVerificationNeeded.Url.ToString();
                    StateService.PaymentInfo = PaymentInfo;
                    NavigationService.UriFor<WebVerificationViewModel>().Navigate();
                }
                return;
            }
        }
    }
}
