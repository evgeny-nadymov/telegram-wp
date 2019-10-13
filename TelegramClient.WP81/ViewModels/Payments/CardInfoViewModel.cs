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
using System.Windows.Threading;
using Windows.Data.Json;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Payments.Stripe;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Payments
{
    public class CardInfoViewModel : PaymentViewModelBase, Telegram.Api.Aggregator.IHandle<TLPasswordBase>
    {
        public bool NeedCountry { get; set; }

        public bool NeedZip { get; set; }

        public bool NeedCardholderName { get; set; }

        private readonly string _publishableKey ;

        public string CardNumber { get; set; }

        public string ExpirationDate { get; set; }

        public string CardholderName { get; set; }

        public string SecurityCode { get; set; }

        private Country _selectedCountry;

        public Country SelectedCountry
        {
            get { return _selectedCountry; }
            set { SetField(ref _selectedCountry, value, () => SelectedCountry); }
        }

        public string PostCode { get; set; }

        public Visibility SavePaymentInformationVisibility { get; set; }

        public bool SavePaymentInformation { get; set; }
         
        public string Error { get; set; }

        private bool _removeCheckoutView;

        private TLPasswordBase _password;

        private readonly DispatcherTimer _checkPasswordSettingsTimer = new DispatcherTimer();

        private void StartTimer()
        {
            if (_password != null && !TLString.IsNullOrEmpty(_password.EmailUnconfirmedPattern))
            {
                _checkPasswordSettingsTimer.Start();
            }
        }

        private void StopTimer()
        {
            _checkPasswordSettingsTimer.Stop();
        }

        public CardInfoViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _checkPasswordSettingsTimer.Tick += OnCheckPasswordSettings;
            _checkPasswordSettingsTimer.Interval = TimeSpan.FromSeconds(5.0);

            EventAggregator.Subscribe(this);

            if (PaymentInfo != null && PaymentInfo.Form != null && PaymentInfo.Form.IsNativeProvider)
            {
                try
                {
                    var jsonObject = JsonObject.Parse(PaymentInfo.Form.NativeParams.Data.ToString());

                    NeedCountry = jsonObject.GetNamedBoolean("need_country", false);
                    NeedZip = jsonObject.GetNamedBoolean("need_zip", false);
                    NeedCardholderName = jsonObject.GetNamedBoolean("need_cardholder_name", false);

                    _publishableKey = jsonObject.GetNamedString("publishable_key", string.Empty);
                }
                catch (Exception ex)
                {
                    Telegram.Logs.Log.Write(string.Format("Can't read Form.NativeParams ex={0}\nparams={1}", ex, PaymentInfo.Form.NativeParams.Data));
                }

                SavePaymentInformation = PaymentInfo.Form.CanSaveCredentials;
                SavePaymentInformationVisibility = PaymentInfo.Form.PasswordMissing ||
                    PaymentInfo.Form.CanSaveCredentials ? Visibility.Visible : Visibility.Collapsed;
            }

            _removeCheckoutView = StateService.RemoveCheckoutAndCardView;
            StateService.RemoveCheckoutAndCardView = false;
        }

        private void OnCheckPasswordSettings(object sender, System.EventArgs e)
        {
            Execute.ShowDebugMessage("account.getPasswordSettings");

            MTProtoService.GetPasswordAsync(
                result => BeginOnUIThread(() =>
                {
                    var password = result as TLPassword;
                    if (password != null && password.HasRecovery.Value)
                    {
                        var currentPassword = _password as TLPassword;
                        if (currentPassword != null)
                        {
                            password.CurrentPasswordHash = currentPassword.CurrentPasswordHash;
                        }

                        _password = password;

                        Handle(password);

                        StopTimer();
                    }
                }),
                error =>
                {
                    Execute.ShowDebugMessage("account.getPasswordSettings error " + error);
                });
        }

        public void SelectCountry()
        {
            StateService.HideCountryCode = true;
            NavigationService.UriFor<ChooseCountryViewModel>().Navigate();
        }

        protected override void OnActivate()
        {
            StartTimer();

            if (StateService.SelectedCountry != null)
            {
                var country = StateService.SelectedCountry;
                StateService.SelectedCountry = null;
                OnCountrySelected(country);
            }
            if (StateService.Password != null)
            {
                _password = StateService.Password;
                StateService.Password = null;

                Handle(_password);

                StartTimer();
            }

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            StopTimer();

            base.OnDeactivate(close);
        }

        private void OnCountrySelected(Country country)
        {
            SelectedCountry = country;
        }

        public async void Validate()
        {
            if (PaymentInfo.Form.SavedCredentials != null && !SavePaymentInformation && PaymentInfo.Form.CanSaveCredentials)
            {
                PaymentInfo.Form.SavedCredentials = null;

                StateService.SaveTmpPassword(null);
                MTProtoService.ClearSavedInfoAsync(false, true,
                    result =>
                    {
                        
                    },
                    error =>
                    {
                        
                    });
            }

            var month = 0;
            var year = 0;

            try
            {
                if (ExpirationDate != null)
                {
                    var args = ExpirationDate.Split('/');
                    if (args.Length == 2)
                    {
                        month = int.Parse(args[0]);
                        year = int.Parse(args[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                Error = "CARD_EXPIRE_DATE_INVALID";
                NotifyOfPropertyChange(() => Error);
                return;
            }

            var card = new Card(
                CardNumber,
                month,
                year,
                SecurityCode,
                CardholderName,
                null, null, null, null,
                PostCode,
                SelectedCountry != null ? _selectedCountry.Code.ToUpperInvariant() : null,
                null);

            if (!card.ValidateNumber())
            {
                Error = "CARD_NUMBER_INVALID";
                NotifyOfPropertyChange(() => Error);
                return;
            }

            if (!card.ValidateExpireDate())
            {
                Error = "CARD_EXPIRE_DATE_INVALID";
                NotifyOfPropertyChange(() => Error);
                return;
            }

            if (NeedCardholderName && string.IsNullOrWhiteSpace(CardholderName))
            {
                Error = "CARD_HOLDER_NAME_INVALID";
                NotifyOfPropertyChange(() => Error);
                return;
            }
            if (!card.ValidateCVC())
            {
                Error = "CARD_CVC_INVALID";
                NotifyOfPropertyChange(() => Error);
                return;
            }
            if (NeedCountry && _selectedCountry == null)
            {
                Error = "CARD_COUNTRY_INVALID";
                NotifyOfPropertyChange(() => Error);
                return;
            }
            if (NeedZip && string.IsNullOrWhiteSpace(PostCode))
            {
                Error = "CARD_ZIP_INVALID";
                NotifyOfPropertyChange(() => Error);
                return;
            }

            IsWorking = true;

            using (var stripe = new StripeClient(_publishableKey))
            {
                var token = await stripe.CreateTokenAsync(card);

                IsWorking = false;

                if (token != null)
                {
                    if (token.Error != null)
                    {
                        Error = token.Error.code;
                        NotifyOfPropertyChange(() => Error);
                        return;
                    }

                    if (!string.IsNullOrEmpty(token.Id)
                        && !string.IsNullOrEmpty(token.Type))
                    {
                        var title = card.GetBrand() + " *" + card.GetLast4();
                        var credentials = string.Format("{{\"type\":\"{0}\", \"id\":\"{1}\"}}", token.Type, token.Id);

                        PaymentInfo.CredentialsTitle = title;
                        var paymentCredentials = new TLInputPaymentCredentials
                        {
                            Flags = new TLInt(0),
                            Data = new TLDataJSON
                            {
                                Data = new TLString(credentials)
                            },
                            Save = SavePaymentInformation
                        };
                        PaymentInfo.Credentials = paymentCredentials;
                        NavigateToNextStep();
                        return;
                    }
                }

                Error = "invalid_button";
                NotifyOfPropertyChange(() => Error);
            }
        }

        public void OpenTwoStepVerification()
        {
            MTProtoService.GetPasswordAsync(
                result => BeginOnUIThread(() =>
                {
                    if (result != null && result.HasPassword)
                    {
                        StateService.Password = result;
                        NavigationService.UriFor<EnterPasswordViewModel>().Navigate();
                        return;
                    }

                    StateService.Password = result;
                    NavigationService.UriFor<PasswordEmailViewModel>().Navigate();
                }),
                error =>
                {
                    Execute.ShowDebugMessage("account.getPassword error " + error);
                });
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

        public void Handle(TLPasswordBase passwordBase)
        {
            if (PaymentInfo != null && PaymentInfo.Form != null)
            {
                PaymentInfo.Form.PasswordMissing = passwordBase == null || !passwordBase.HasPassword;
                PaymentInfo.Form.NotifyOfPropertyChange(() => PaymentInfo.Form.PasswordMissing);
                SavePaymentInformation = PaymentInfo.Form.CanSaveCredentials;
                NotifyOfPropertyChange(() => SavePaymentInformation);
                SavePaymentInformationVisibility = PaymentInfo.Form.PasswordMissing ||
                    PaymentInfo.Form.CanSaveCredentials ? Visibility.Visible : Visibility.Collapsed;
                NotifyOfPropertyChange(() => SavePaymentInformationVisibility);
            }
        }
    }
}
