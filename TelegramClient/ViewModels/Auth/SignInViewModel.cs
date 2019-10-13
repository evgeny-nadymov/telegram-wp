// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using ErrorType = Telegram.Api.TL.ErrorType;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Auth
{
    public class SignInViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<string>
    {
        private bool _isLoadingCountryInfo;

        public bool IsLoadingCountryInfo
        {
            get { return _isLoadingCountryInfo; }
            set { SetField(ref _isLoadingCountryInfo, value, () => IsLoadingCountryInfo); }
        }

        public bool IsInProgress
        {
            get { return _isLoadingCountryInfo || IsWorking; }
        }

        private DateTime? _startTime;

        private int _timeCounter;

        private readonly DispatcherTimer _showHelpTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.0) };

        private Country _selectedCountry;

        public Country SelectedCountry
        {
            get { return _selectedCountry; }
            set { SetField(ref _selectedCountry, value, () => SelectedCountry); }
        }

        public bool IsPhoneCodeInvalid
        {
            get { return string.IsNullOrEmpty(PhoneCode) || CountryUtils.CountriesSource.FirstOrDefault(x => x.PhoneCode == PhoneCode) == null; }
        }

        private string _phoneCode;

        public string PhoneCode
        {
            get { return _phoneCode; }
            set
            {
                SetField(ref _phoneCode, value, () => PhoneCode);
                Country country = null;
                foreach (var c in CountryUtils.CountriesSource)
                {
                    if (c.PhoneCode == PhoneCode)
                    {
                        if (c.PhoneCode == "1" && c.Code != "us")
                        {
                            continue;
                        }

                        if (c.PhoneCode == "7" && c.Code != "ru")
                        {
                            continue;
                        }

                        country = c;
                        break;
                    }
                }

                if (country != null)
                {
                    SelectedCountry = country;
                }

                NotifyOfPropertyChange(() => IsPhoneCodeInvalid);
            }
        }

        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { SetField(ref _phoneNumber, value, () => PhoneNumber); }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set { SetField(ref _status, value, () => Status); }
        }

        private Visibility _helpVisibility = Visibility.Collapsed;

        public Visibility HelpVisibility
        {
            get { return _helpVisibility; }
            set { SetField(ref _helpVisibility, value, () => HelpVisibility); }
        }

        public LogViewModel Log { get; private set; }

        private IExtendedDeviceInfoService _extendedDeviceInfoService;

        public SignInViewModel(
            IExtendedDeviceInfoService extendedDeviceInfoService,
#if PRIVATE_BETA
            LogViewModel log, 
#endif
 ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _extendedDeviceInfoService = extendedDeviceInfoService;

            HelpVisibility = PrivateBetaIdentityToVisibilityConverter.IsPrivateBeta
                ? Visibility.Visible
                : Visibility.Collapsed;

            EventAggregator.Subscribe(this);
            SuppressUpdateStatus = true;

#if PRIVATE_BETA
            Log = log;
#endif

            if (StateService.ChangePhoneNumber)
            {
                _changePhoneNumber = true;
                StateService.ChangePhoneNumber = false;
            }

            _showHelpTimer.Tick += (sender, args) =>
            {
                if (!_startTime.HasValue)
                {
                    _showHelpTimer.Stop();
                    return;
                }

                _timeCounter = (int)(Constants.ShowHelpTimeInSeconds - (DateTime.Now - _startTime.Value).TotalSeconds);
                if (_timeCounter <= 0)
                {
                    _timeCounter = 0;
                    HelpVisibility = Visibility.Visible;
                    _showHelpTimer.Stop();
                }
            };

            if (!string.IsNullOrEmpty(MTProtoService.Country))
            {
                OnGotUserCountry(this, new CountryEventArgs { Country = MTProtoService.Country });
            }

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => PhoneNumber)
                    || Property.NameEquals(args.PropertyName, () => PhoneCode))
                {
                    NotifyOfPropertyChange(() => CanSignIn);
                }
                else if (Property.NameEquals(args.PropertyName, () => IsLoadingCountryInfo)
                    || Property.NameEquals(args.PropertyName, () => IsWorking))
                {
                    NotifyOfPropertyChange(() => IsInProgress);
                }
            };
        }

        private void OnGotUserCountry(object sender, CountryEventArgs args)
        {
            Country country = null;
            foreach (var c in CountryUtils.CountriesSource)
            {
                if (string.Equals(c.Code, args.Country, StringComparison.OrdinalIgnoreCase))
                {
                    country = c;
                    break;
                }
            }

            if (country != null && SelectedCountry == null && string.IsNullOrEmpty(PhoneNumber))
            {
                OnCountrySelected(country);
            }
        }

        public bool CanSignIn
        {
            get
            {
                var phoneNumberLength = string.IsNullOrEmpty(PhoneNumber) ? 0 : PhoneNumber.Length;
                var phoneCodeLength = string.IsNullOrEmpty(PhoneCode) ? 0 : PhoneCode.Length;
                return !IsWorking && (phoneNumberLength + phoneCodeLength) >= 3;
            }
        }

        private TLRPCError _lastError;

        private TLSentCodeBase _sentCode;

        private bool _changePhoneNumber;

        public void SignIn()
        {
            if (_changePhoneNumber)
            {
                ChangePhoneNumber();
                return;
            }

#if LOG_REGISTRATION
            TLUtils.WriteLog("auth.sendCode");
#endif

            IsWorking = true;
            NotifyOfPropertyChange(() => CanSignIn);

            var phoneNumber = PhoneCode + PhoneNumber;
            _startTime = DateTime.Now;
            _showHelpTimer.Start();

            MTProtoService.SendCodeAsync(new TLString(phoneNumber), null,
                sentCode => BeginOnUIThread(() =>
                {
#if LOG_REGISTRATION
                    TLUtils.WriteLog("auth.sendCode result: " + sentCode);
#endif
                    _sentCode = sentCode;

                    _showHelpTimer.Stop();

                    StateService.SentCode = sentCode;
                    StateService.PhoneNumber = new TLString(phoneNumber);
                    StateService.PhoneNumberString = string.Format(AppResources.ConfirmMessage, PhoneNumberConverter.Convert(StateService.PhoneNumber));
                    StateService.PhoneCodeHash = sentCode.PhoneCodeHash;
                    StateService.PhoneRegistered = sentCode.PhoneRegistered;
                    StateService.SendCallTimeout = sentCode.SendCallTimeout;
                    var sentCode50 = sentCode as TLSentCode50;
                    if (sentCode50 != null)
                    {
                        StateService.Type = sentCode50.Type;
                        StateService.NextType = sentCode50.NextType;
                    }

                    NavigationService.UriFor<ConfirmViewModel>().Navigate();
                    IsWorking = false;
                    NotifyOfPropertyChange(() => CanSignIn);
                }),
                attemptNumber => BeginOnUIThread(() =>
                {
#if LOG_REGISTRATION
                    TLUtils.WriteLog("auth.sendCode attempt failed " + attemptNumber);
#endif
                    Execute.ShowDebugMessage("auth.sendCode attempt failed " + attemptNumber);
                }),
                error => BeginOnUIThread(() =>
                {
#if LOG_REGISTRATION
                    TLUtils.WriteLog("auth.sendCode error " + error);
#endif

                    _lastError = error;
                    IsWorking = false;
                    NotifyOfPropertyChange(() => CanSignIn);

                    if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                    {
                        MessageBox.Show(AppResources.PhoneNumberInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                    }
                    else
                    {
                        Execute.ShowDebugMessage("auth.sendCode error " + error);
                    }
                }));
        }

        private void ChangePhoneNumber()
        {
            IsWorking = true;
            NotifyOfPropertyChange(() => CanSignIn);
            var phoneNumber = PhoneCode + PhoneNumber;

            _startTime = DateTime.Now;
            _showHelpTimer.Start();
            MTProtoService.SendChangePhoneCodeAsync(new TLString(phoneNumber), null,
                sentCode => BeginOnUIThread(() =>
                {
                    _showHelpTimer.Stop();
                    StateService.SentCode = sentCode;
                    StateService.PhoneNumber = new TLString(phoneNumber);
                    StateService.PhoneNumberString = string.Format(AppResources.ConfirmMessage, PhoneNumberConverter.Convert(StateService.PhoneNumber));
                    StateService.PhoneCodeHash = sentCode.PhoneCodeHash;
                    StateService.PhoneRegistered = new TLBool(false);
                    StateService.SendCallTimeout = sentCode.SendCallTimeout;
                    StateService.ChangePhoneNumber = true;
                    var sentCode50 = sentCode as TLSentCode50;
                    if (sentCode50 != null)
                    {
                        StateService.Type = sentCode50.Type;
                        StateService.NextType = sentCode50.NextType;
                    }

                    NavigationService.UriFor<ConfirmViewModel>().Navigate();

                    IsWorking = false;
                    NotifyOfPropertyChange(() => CanSignIn);
                }),
                error => BeginOnUIThread(() =>
                {
                    _lastError = error;

                    IsWorking = false;
                    NotifyOfPropertyChange(() => CanSignIn);

                    if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                    {
                        MessageBox.Show(AppResources.PhoneNumberInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_NUMBER_OCCUPIED))
                    {
                        MessageBox.Show(string.Format(AppResources.NewNumberTaken, "+" + phoneNumber), AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                    }
                    else
                    {
                        Execute.ShowDebugMessage("account.sendChangePhoneCode error " + error);
                    }
                }));
        }

        public void SelectCountry()
        {
            NavigationService.UriFor<ChooseCountryViewModel>().Navigate();
        }

        protected override void OnActivate()
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            MTProtoService.GotUserCountry += OnGotUserCountry;

            if (StateService.ClearNavigationStack)
            {
                PhoneNumber = string.Empty;
                StateService.ClearNavigationStack = false;
                while (NavigationService.RemoveBackEntry() != null) { }
            }

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            if (StateService.SelectedCountry != null)
            {
                var country = StateService.SelectedCountry;
                StateService.SelectedCountry = null;
                OnCountrySelected(country);
            }

            _showHelpTimer.Start();
            base.OnActivate();
        }

        private void OnCountrySelected(Country country)
        {
            SelectedCountry = country;
            _phoneCode = SelectedCountry.PhoneCode;
            NotifyOfPropertyChange(() => PhoneCode);
            NotifyOfPropertyChange(() => IsPhoneCodeInvalid);
        }

        protected override void OnDeactivate(bool close)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;

            MTProtoService.GotUserCountry -= OnGotUserCountry;

            _showHelpTimer.Stop();
            base.OnDeactivate(close);
        }

        public void OnBackKeyPressed(CancelEventArgs args)
        {
#if LOG_REGISTRATION
            TLUtils.WriteLog("SignInViewModel.OnBackKeyPressed");
#endif

            if (IsWorking)
            {
                args.Cancel = true;
                IsWorking = false;
                MTProtoService.CancelSignInAsync();
                MTProtoService.ClearHistory("SignInViewModel", false);
                NotifyOfPropertyChange(() => CanSignIn);
            }
        }

        public string Email
        {
            get { return Constants.LogEmail; }
        }

        public void SendMail()
        {
            var logBuilder = new StringBuilder();
            foreach (var item in TLUtils.LogItems)
            {
                logBuilder.AppendLine(item);
            }

            var body = new StringBuilder();
            body.AppendLine();
            body.AppendLine();
            body.AppendLine("Page: Your Phone");
            body.AppendLine("Phone: " + "+" + PhoneCode + PhoneNumber);
            body.AppendLine("App version: " + _extendedDeviceInfoService.AppVersion);
            body.AppendLine("OS version: " + _extendedDeviceInfoService.SystemVersion);
            body.AppendLine("Device Name: " + _extendedDeviceInfoService.Model);
            body.AppendLine("Location: " + Telegram.Api.Helpers.Utils.CurrentUICulture());
            body.AppendLine("Wi-Fi: " + _extendedDeviceInfoService.IsWiFiEnabled);
            body.AppendLine("Mobile Network: " + _extendedDeviceInfoService.IsCellularDataEnabled);
            body.AppendLine("Last error: " + ((_lastError != null) ? _lastError.ToString() : null));
            body.AppendLine("Log" + Environment.NewLine + logBuilder);

            var appVersionBuilder = new StringBuilder();
            appVersionBuilder.Append(_extendedDeviceInfoService.AppVersion);
            if (_sentCode != null)
            {
                appVersionBuilder.Append(", Code");
            }

            var task = new EmailComposeTask();
            task.Body = body.ToString();
            task.To = Constants.LogEmail;
            task.Subject = string.Format("WP registration/login issue ({0}) {1}{2}", appVersionBuilder, PhoneCode, PhoneNumber);
            task.Show();
        }

        public void ChangeProxy()
        {
            StateService.SuppressProxySharing = true;
            NavigationService.UriFor<ProxyListViewModel>().Navigate();
        }

        public void Handle(string command)
        {
            if (string.Equals(command, Commands.LogOutCommand))
            {
                _startTime = null;
                HelpVisibility = Visibility.Collapsed;
                SelectedCountry = null;
                NotifyOfPropertyChange(() => IsPhoneCodeInvalid);
                PhoneNumber = string.Empty;
                IsWorking = false;
            }
        }
    }
}
