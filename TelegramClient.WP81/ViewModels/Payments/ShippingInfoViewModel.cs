// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Payments
{
    public class ShippingInfoViewModel : PaymentViewModelBase
    {
        public bool IsPhoneCodeInvalid
        {
            get { return string.IsNullOrEmpty(PhoneCode) || CountryUtils.CountriesSource.FirstOrDefault(x => x.PhoneCode == PhoneCode) == null; }
        }

        public string Name { get; set; }

        public string PhoneCode { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        #region Shipping Address

        public string StreetLine1 { get; set; }

        public string StreetLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        private Country _selectedCountry;

        public Country SelectedCountry
        {
            get { return _selectedCountry; }
            set { SetField(ref _selectedCountry, value, () => SelectedCountry); }
        }

        public string PostCode { get; set; }
        #endregion

        public bool SaveShippingInformation { get; set; }

        public TLRPCError Error { get; set; }

        public ShippingInfoViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            if (PaymentInfo != null && PaymentInfo.Form != null)
            {
                var savedInfo = PaymentInfo.Form.SavedInfo;
                if (savedInfo != null)
                {
                    Name = savedInfo.Name != null? savedInfo.Name.ToString() : null;
                    var phone = TLString.IsNullOrEmpty(savedInfo.Phone) ? string.Empty : savedInfo.Phone.ToString();
                    if (!string.IsNullOrEmpty(phone))
                    {
                        var codeCounty = CountryUtils.CountriesSource.FirstOrDefault(x => phone.StartsWith(x.PhoneCode, StringComparison.OrdinalIgnoreCase));
                        if (codeCounty != null)
                        {
                            PhoneCode = codeCounty.PhoneCode;
                            var index = phone.IndexOf(PhoneCode, StringComparison.OrdinalIgnoreCase);
                            var phoneNumber = (index < 0)
                                ? phone
                                : phone.Remove(index, PhoneCode.Length);
                            PhoneNumber = phoneNumber;
                        }
                        else
                        {
                            PhoneNumber = phone;
                        }
                    }
                    Email = savedInfo.Email != null ? savedInfo.Email.ToString() : null;

                    if (savedInfo.ShippingAddress != null)
                    {
                        var country = CountryUtils.CountriesSource.FirstOrDefault(x => string.Equals(savedInfo.ShippingAddress.CountryIso2.ToString(), x.Code, StringComparison.OrdinalIgnoreCase));

                        StreetLine1 = savedInfo.ShippingAddress.StreetLine1.ToString();
                        StreetLine2 = savedInfo.ShippingAddress.StreetLine2.ToString();
                        City = savedInfo.ShippingAddress.City.ToString();
                        State = savedInfo.ShippingAddress.State.ToString();
                        _selectedCountry = country;
                        PostCode = savedInfo.ShippingAddress.PostCode.ToString();
                    }
                }
            }

            SaveShippingInformation = true;
        }

        public void SelectCountry()
        {
            StateService.HideCountryCode = true;
            NavigationService.UriFor<ChooseCountryViewModel>().Navigate();
        }

        protected override void OnActivate()
        {
            if (StateService.SelectedCountry != null)
            {
                var country = StateService.SelectedCountry;
                StateService.SelectedCountry = null;
                OnCountrySelected(country);
            }
            base.OnActivate();
        }

        private void OnCountrySelected(Country country)
        {
            SelectedCountry = country;
        }

        public void Validate()
        {
            if (PaymentInfo == null) return;
            if (PaymentInfo.Message == null) return;
            if (PaymentInfo.Form == null) return;

            var info = new TLPaymentRequestedInfo
            {
                Flags = new TLInt(0)
            };
            if (PaymentInfo.Form.Invoice.NameRequested)
            {
                info.Name = new TLString(Name);
            }
            if (PaymentInfo.Form.Invoice.PhoneRequested)
            {
                info.Phone = new TLString(PhoneCode + PhoneNumber);
            }
            if (PaymentInfo.Form.Invoice.EmailRequested)
            {
                info.Email = new TLString(Email);
            }
            if (PaymentInfo.Form.Invoice.ShippingAddressRequested)
            {
                info.ShippingAddress = new TLPostAddress
                {
                    StreetLine1 = new TLString(StreetLine1),
                    StreetLine2 = new TLString(StreetLine2),
                    City = new TLString(City),
                    State = new TLString(State),
                    CountryIso2 = new TLString(SelectedCountry != null ? SelectedCountry.Code : string.Empty),
                    PostCode = new TLString(PostCode)
                };
            }

            IsWorking = true;
            MTProtoService.ValidateRequestedInfoAsync(
                SaveShippingInformation,
                PaymentInfo.Message.Id,
                info,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    PaymentInfo.ValidatedInfo = result;
                    PaymentInfo.Form.SavedInfo = info;

                    if (!SaveShippingInformation
                        && PaymentInfo != null
                        && PaymentInfo.Form != null
                        && PaymentInfo.Form.SavedInfo != null)
                    {
                        IsWorking = true;
                        MTProtoService.ClearSavedInfoAsync(false, true,
                            result2 =>Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                NavigateToNextStep();
                            }),
                            error2 => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                Telegram.Api.Helpers.Execute.ShowDebugMessage("payments.clearInfo error " + error2);
                            }));
                    }
                    else
                    {
                        NavigateToNextStep();
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Error = error;
                    NotifyOfPropertyChange(() => Error);
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("payments.validateRequestedInfo error " + error);
                }));
        }

        public override void NavigateToNextStep()
        {
            StateService.PaymentInfo = PaymentInfo;

            if (PaymentInfo.Form.Invoice.Flexible
                && PaymentInfo.ValidatedInfo.ShippingOptions != null
                && PaymentInfo.ValidatedInfo.ShippingOptions.Count > 0)
            {
                NavigationService.UriFor<ShippingMethodViewModel>().Navigate();
            }
            else
            {
                NavigateToCardInfo(null, StateService.PaymentInfo, () => StateService.GetTmpPassword(), NavigationService);
            }
        }
    }
}
