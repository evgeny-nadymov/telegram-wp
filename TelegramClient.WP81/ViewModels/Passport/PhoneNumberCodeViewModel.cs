using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Passport
{
    public class PhoneNumberCodeViewModel : ViewModelBase
    {
        private DateTime _startTime;

        private readonly DispatcherTimer _callTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };

        private int _timeCounter = Constants.SendCallDefaultTimeout;

        public int TimeCounter
        {
            get { return _timeCounter; }
            set { SetField(ref _timeCounter, value, () => TimeCounter); }
        }

        private string _timeCounterString = " ";

        public string TimeCounterString
        {
            get { return _timeCounterString; }
            set { SetField(ref _timeCounterString, value, () => TimeCounterString); }
        }

        private string _code;

        public string Code
        {
            get { return _code; }
            set { SetField(ref _code, value, () => Code); }
        }

        private Visibility _helpVisibility = Visibility.Collapsed;

        public Visibility HelpVisibility
        {
            get { return _helpVisibility; }
            set { SetField(ref _helpVisibility, value, () => HelpVisibility); }
        }

        private Visibility _resendCodeVisibility = Visibility.Collapsed;

        public Visibility ResendCodeVisibility
        {
            get { return _resendCodeVisibility; }
            set { SetField(ref _resendCodeVisibility, value, () => ResendCodeVisibility); }
        }

        public string Subtitle { get; set; }

        public TLInt Timeout { get; set; }

        public int CodeLength { get; set; }

        private TLSentCodeTypeBase _type;

        private TLCodeTypeBase _nextType;

        public string Caption
        {
            get { return TLString.IsNullOrEmpty(StateService.PhoneNumber) ? null : "+" + StateService.PhoneNumber; }
        }

        private readonly TLSecureValue _phoneNumberValue;

        private readonly TLPasswordBase _passwordBase;

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly IList<TLSecureValue> _secureValues;

        private readonly TLSecureValueTypeBase _secureType;

        private readonly SecureRequiredType _secureRequiredType;

        public PhoneNumberCodeViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _passwordBase = stateService.Password;
            stateService.Password = null;

            _authorizationForm = stateService.AuthorizationForm;
            stateService.AuthorizationForm = null;

            _secureValues = stateService.SecureValues;
            stateService.SecureValues = null;

            _secureType = stateService.SecureType;
            stateService.SecureType = null;

            _secureRequiredType = stateService.SecureRequiredType;
            stateService.SecureRequiredType = null;

            _phoneNumberValue = _secureRequiredType != null ? _secureRequiredType.DataValue : null;

            _type = stateService.Type;
            stateService.Type = null;

            _nextType = stateService.NextType;
            stateService.NextType = null;

            Subtitle = GetSubtitle();

            var length = _type as ILength;
            CodeLength = length != null ? length.Length.Value : Constants.DefaultCodeLength;

            Timeout = stateService.SendCallTimeout;
            ResendCodeVisibility = stateService.SendCallTimeout != null && stateService.SendCallTimeout.Value > 0
                ? Visibility.Collapsed
                : Visibility.Visible;

            SuppressUpdateStatus = true;

            EventAggregator.Subscribe(this);

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Code))
                {
                    NotifyOfPropertyChange(() => CanConfirm);

                    if (!string.IsNullOrEmpty(Code) && Code.Length == CodeLength)
                    {
                        Confirm();
                    }
                }
            };

            _callTimer.Tick += (sender, args) =>
            {
                _timeCounter = Timeout == null? 0 : (int)(Timeout.Value - (DateTime.Now - _startTime).TotalSeconds);

                if (_timeCounter > 0)
                {
#if DEBUG
                    TimeCounterString = _timeCounter.ToString(CultureInfo.InvariantCulture);
#endif

                    if (_nextType is TLCodeTypeCall)
                    {
                        TimeCounterString = string.Format(AppResources.WeWillCallYou, TimeSpan.FromSeconds(TimeCounter).ToString(@"m\:ss"));
                    }
                }
                else   
                {
                    _timeCounter = 0;
                    if (_nextType is TLCodeTypeCall)
                    {
                        TimeCounterString = AppResources.TelegramDialedYourNumber;
                    }

                    HelpVisibility = Visibility.Visible;
                    ResendCodeVisibility = Visibility.Visible;
                    _callTimer.Stop();
                }

                NotifyOfPropertyChange(() => TimeCounter);
            };
        }

        private bool _isResending;

        public void Resend()
        {
            if (_isResending)
            {
                return;
            }

            if (_nextType == null)
            {
                return;
            }

            _isResending = true;
            IsWorking = true;
            MTProtoService.ResendCodeAsync(StateService.PhoneNumber, StateService.PhoneCodeHash,
                sentCode => BeginOnUIThread(() =>
                {
                    _isResending = false;
                    IsWorking = false;

                    StateService.PhoneCodeHash = sentCode.PhoneCodeHash;
                    StateService.PhoneRegistered = sentCode.PhoneRegistered;

                    Timeout = sentCode.SendCallTimeout;
                    ResendCodeVisibility = Timeout != null && Timeout.Value > 0
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                    var sentCode50 = sentCode as TLSentCode50;
                    if (sentCode50 != null)
                    {
                        _type = sentCode50.Type;
                        _nextType = sentCode50.NextType;

                        Subtitle = GetSubtitle();
                        NotifyOfPropertyChange(() => Subtitle);

                        var length = _type as ILength;
                        CodeLength = length != null ? length.Length.Value : Constants.DefaultCodeLength;
                        NotifyOfPropertyChange(() => CodeLength);
                    }

                    _startTime = DateTime.Now;
                    _callTimer.Start();
                }),
                error => BeginOnUIThread(() =>
                {
                    _isResending = false;
                    IsWorking = false;
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("auth.resendCode error " + error);
                }));
        }

        private string GetSubtitle()
        {
            if (_type is TLSentCodeTypeApp)
            {
                return AppResources.CodeSentToTelegramApp;
            }

            if (_type is TLSentCodeTypeSms)
            {
                return string.Format(AppResources.ConfirmMessage, PhoneNumberConverter.Convert(StateService.PhoneNumber));
            }

            if (_type is TLSentCodeTypeCall)
            {
                return string.Format(AppResources.CodeSentViaCallingPhone, PhoneNumberConverter.Convert(StateService.PhoneNumber));
            }

            return string.Empty;
        }

        public bool CanConfirm
        {
            get { return !string.IsNullOrEmpty(Code); }
        }

        public void Confirm()
        {
            IsWorking = true;
            StateService.PhoneCode = new TLString(Code);
            MTProtoService.VerifyPhoneAsync(
                StateService.PhoneNumber, 
                StateService.PhoneCodeHash, 
                StateService.PhoneCode,
                auth => BeginOnUIThread(() =>
                {
                    TimeCounterString = string.Empty;
                    HelpVisibility = Visibility.Collapsed;
                    _callTimer.Stop();

                    _isProcessing = false;

                    var phoneNumberValue = _phoneNumberValue;
                    if (phoneNumberValue == null)
                    {
                        var secureRequiredType = _secureRequiredType != null ? _secureRequiredType.DataRequiredType as TLSecureRequiredType : null;
                        var secureType = secureRequiredType != null && PhoneNumberViewModel.IsValidType(secureRequiredType.Type)
                            ? secureRequiredType.Type
                            : null;

                        // add new phone number from passport settings
                        if (_secureType != null && PhoneNumberViewModel.IsValidType(_secureType))
                        {
                            phoneNumberValue = new TLSecureValue85
                            {
                                Flags = new TLInt(0),
                                Type = _secureType
                            };
                        }
                        // add new phone number from authorization form
                        else if (secureType != null)
                        {
                            phoneNumberValue = new TLSecureValue85
                            {
                                Flags = new TLInt(0),
                                Type = secureType
                            };
                        }
                        else
                        {
                            return;
                        }
                    }

                    IsWorking =
                    PhoneNumberViewModel.SavePhoneAsync(
                        StateService.PhoneNumber, _passwordBase as TLPassword, MTProtoService,
                        result => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            if (_authorizationForm != null)
                            {
                                _authorizationForm.Values.Remove(_phoneNumberValue);
                                _authorizationForm.Values.Add(result);
                            }

                            phoneNumberValue.Update(result);
                            phoneNumberValue.NotifyOfPropertyChange(() => phoneNumberValue.Self);

                            if (_secureType != null)
                            {
                                EventAggregator.Publish(new AddSecureValueEventArgs { Values = new List<TLSecureValue> { phoneNumberValue } });
                            }
                            
                            if (_secureRequiredType != null)
                            {
                                _secureRequiredType.UpdateValue();
                            }

                            NavigationService.RemoveBackEntry();
                            NavigationService.GoBack();
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;

                            if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                                && error.TypeEquals(ErrorType.PHONE_VERIFICATION_NEEDED))
                            {
                                MTProtoService.SendVerifyPhoneCodeAsync(StateService.PhoneNumber, null,
                                    sentCode => BeginOnUIThread(() =>
                                    {
                                        StateService.PhoneCodeHash = sentCode.PhoneCodeHash;
                                        StateService.PhoneRegistered = sentCode.PhoneRegistered;

                                        Timeout = sentCode.SendCallTimeout;
                                        ResendCodeVisibility = Timeout != null && Timeout.Value > 0
                                            ? Visibility.Collapsed
                                            : Visibility.Visible;

                                        var sentCode50 = sentCode as TLSentCode50;
                                        if (sentCode50 != null)
                                        {
                                            _type = sentCode50.Type;
                                            _nextType = sentCode50.NextType;

                                            Subtitle = GetSubtitle();
                                            NotifyOfPropertyChange(() => Subtitle);

                                            var length = _type as ILength;
                                            CodeLength = length != null ? length.Length.Value : Constants.DefaultCodeLength;
                                            NotifyOfPropertyChange(() => CodeLength);
                                        }
                                    }),
                                    error2 => BeginOnUIThread(() =>
                                    {
                                        if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                                        {
                                            ShellViewModel.ShowCustomMessageBox(AppResources.PhoneNumberInvalidString, AppResources.Error, AppResources.Ok);
                                        }
                                        else if (error.CodeEquals(ErrorCode.FLOOD))
                                        {
                                            ShellViewModel.ShowCustomMessageBox(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, AppResources.Ok);
                                        }
                                        else
                                        {
                                            Telegram.Api.Helpers.Execute.ShowDebugMessage("account.sendVerifyPhoneCode error " + error);
                                        }
                                    }));
                            }
                        }));
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (error.TypeEquals(ErrorType.PHONE_CODE_INVALID))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.PhoneCodeInvalidString, AppResources.Error, AppResources.Ok);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_EMPTY))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.PhoneCodeEmpty, AppResources.Error, AppResources.Ok);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.PhoneNumberInvalidString, AppResources.Error, AppResources.Ok);
                    }
                    else if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, AppResources.Ok);
                    }
                    else
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.verifyPhone error " + error);
                    }
                }));
        }

        private bool _isProcessing;

        protected override void OnActivate()
        {
            if (_isProcessing) return;

            _isProcessing = true;

            Subtitle = GetSubtitle();

            if (Timeout != null)
            {
                TimeCounter = Timeout.Value;
            }
            _startTime = DateTime.Now;
            _callTimer.Start();

            base.OnActivate();
        }

        public void OnBackKeyPress()
        {
            _isProcessing = false;
            Code = string.Empty;
            _callTimer.Stop();
            TimeCounterString = " ";
#if DEBUG
            HelpVisibility = Visibility.Visible;
#else
            HelpVisibility = Visibility.Collapsed;
#endif
        }
    }
}
