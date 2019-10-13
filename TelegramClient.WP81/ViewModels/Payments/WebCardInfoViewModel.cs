// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using System.Windows;
using Windows.Data.Json;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Payments
{
    public class WebCardInfoViewModel : PaymentViewModelBase
    {
        public Uri Url { get; set; }

        private bool _removeCheckoutView;

        public WebCardInfoViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            if (PaymentInfo != null && PaymentInfo.Form != null && !TLString.IsNullOrEmpty(PaymentInfo.Form.Url))
            {
                Url = new Uri(PaymentInfo.Form.Url.ToString(), UriKind.Absolute);
            }

            _removeCheckoutView = StateService.RemoveCheckoutAndCardView;
            StateService.RemoveCheckoutAndCardView = false;
        }

        public void ScriptNotify(string value)
        {
            try
            {
                var obj = JsonObject.Parse(value);
                IJsonValue eventType;
                if (obj.TryGetValue("eventType", out eventType))
                {
                    if (string.Equals(eventType.GetString(), "payment_form_submit", StringComparison.OrdinalIgnoreCase))
                    {
                        IJsonValue eventData;
                        if (obj.TryGetValue("eventData", out eventData))
                        {
                            if (PaymentInfo != null)
                            {
                                var eventDataObject = eventData.GetObject();
                                if (eventDataObject != null)
                                {
                                    IJsonValue title;
                                    if (eventDataObject.TryGetValue("title", out title))
                                    {
                                        PaymentInfo.CredentialsTitle = title.Stringify().Trim('\"');
                                    }

                                    IJsonValue credentials;
                                    if (eventDataObject.TryGetValue("credentials", out credentials))
                                    {
                                        var paymentCredentials = new TLInputPaymentCredentials
                                        {
                                            Flags = new TLInt(0),
                                            Data = new TLDataJSON
                                            {
                                                Data = new TLString(credentials.Stringify())
                                            }
                                        };
                                        PaymentInfo.Credentials = paymentCredentials;
                                        NavigateToNextStep();
                                        return;
                                    }
                                }
                            }

                            return;
                        }

                        return;
                    }
                }

                MessageBox.Show(value);
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("WebViewModel.ScriptNotify {0} exception {1}", value, ex));
            }
        }

        public override void NavigateToNextStep()
        {
            if (_removeCheckoutView)
            {
                var backEntry = NavigationService.BackStack.FirstOrDefault();
                if (backEntry != null && backEntry.Source.ToString().Contains("CheckoutView.xaml"))
                {
                    NavigationService.RemoveBackEntry();
                }

                backEntry = NavigationService.BackStack.FirstOrDefault();
                if (backEntry != null && (backEntry.Source.ToString().Contains("SavedCardInfoView.xaml") || backEntry.Source.ToString().Contains("CardInfoView.xaml") || backEntry.Source.ToString().Contains("WebCardInfoView.xaml")))
                {
                    NavigationService.RemoveBackEntry();
                }
            }

            StateService.PaymentInfo = PaymentInfo;
            NavigationService.UriFor<CheckoutViewModel>().Navigate();
        }
    }
}
