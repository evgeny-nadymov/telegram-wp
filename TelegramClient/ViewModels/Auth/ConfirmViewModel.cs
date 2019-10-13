// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using ErrorType = Telegram.Api.TL.ErrorType;

namespace TelegramClient.ViewModels.Auth
{
    public class ConfirmViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<string>
    {
        private DateTime _startTime;

        private readonly DispatcherTimer _callTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };

        private int _timeCounter = Constants.SendCallDefaultTimeout;

        public int TimeCounter
        {
            get { return _timeCounter; }
            set
            {
                SetField(ref _timeCounter, value, () => TimeCounter);
            }
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

        public DebugViewModel Debug { get; private set; }

        public TLInt Timeout { get; set; }

        private bool _changePhoneNumber;

        private IExtendedDeviceInfoService _extendedDeviceInfoService;

        public int CodeLength { get; set; }

        private TLSentCodeTypeBase _type;

        private TLCodeTypeBase _nextType;

        private string _debugString;

        public string DebugString
        {
            get { return _debugString; }
            set { SetField(ref _debugString, value, () => DebugString); }
        }

        public string Caption
        {
            get { return TLString.IsNullOrEmpty(StateService.PhoneNumber) ? null : "+" + StateService.PhoneNumber; }
        }

        private TLSentCodeBase _sentCode;

        public ConfirmViewModel(IExtendedDeviceInfoService extendedDeviceInfoService, DebugViewModel debug, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _extendedDeviceInfoService = extendedDeviceInfoService;
#if DEBUG
            HelpVisibility = Visibility.Visible;
#endif
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

            UpdateDebugString();

            EventAggregator.Subscribe(this);
            SuppressUpdateStatus = true;

            Debug = debug;

            if (StateService.ChangePhoneNumber)
            {
                _changePhoneNumber = true;
                StateService.ChangePhoneNumber = false;
            }

            _sentCode = StateService.SentCode;
            StateService.SentCode = null;

            //_updatesService = updatesService;

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
                _timeCounter = Timeout == null ? 0 : (int)(Timeout.Value - (DateTime.Now - _startTime).TotalSeconds);

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
                SendMail();
                return;
            }

            _isResending = true;
            IsWorking = true;
            MTProtoService.ResendCodeAsync(StateService.PhoneNumber, StateService.PhoneCodeHash,
                sentCode => BeginOnUIThread(() =>
                {
                    _sentCode = sentCode;
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

                    UpdateDebugString();

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

        private void UpdateDebugString()
        {
#if DEBUG
            DebugString = string.Format("next_type={0} timeout={1} code_length={2}", _nextType, Timeout, CodeLength);
#endif
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

        private TLRPCError _lastError;

        public void Confirm()
        {
            if (_changePhoneNumber)
            {
                ConfirmChangePhoneNumber();
                return;
            }

            IsWorking = true;
            StateService.PhoneCode = new TLString(Code);
#if LOG_REGISTRATION
            TLUtils.WriteLog("auth.signIn");
#endif
            MTProtoService.SignInAsync(
                StateService.PhoneNumber, StateService.PhoneCodeHash, StateService.PhoneCode,
                auth => BeginOnUIThread(() =>
                {
#if LOG_REGISTRATION
                    TLUtils.WriteLog("auth.signIn result " + auth);
                    TLUtils.WriteLog("TLUtils.IsLogEnabled=false");
#endif

                    TLUtils.IsLogEnabled = false;
                    TLUtils.LogItems.Clear();

                    TimeCounterString = string.Empty;
                    HelpVisibility = Visibility.Collapsed;
                    _callTimer.Stop();

                    var result = MessageBox.Show(
                        AppResources.ConfirmPushMessage,
                        AppResources.ConfirmPushTitle,
                        MessageBoxButton.OKCancel);

                    if (result != MessageBoxResult.OK)
                    {
                        Notifications.Disable();
                    }
                    else
                    {
                        Notifications.Enable();
                    }

                    UpdateNotificationsAsync(MTProtoService, StateService);

                    MTProtoService.SetInitState();
                    CacheService.ClearConfigImportAsync();
                    //_updatesService.SetCurrentUser(auth.User);
                    _isProcessing = false;
                    StateService.CurrentUserId = auth.User.Index;
                    StateService.FirstRun = true;
                    SettingsHelper.SetValue(Constants.IsAuthorizedKey, true);

                    ShellViewModel.Navigate(NavigationService);

                    IsWorking = false;
                }),
                error => BeginOnUIThread(() =>
                {
#if LOG_REGISTRATION
                    TLUtils.WriteLog("auth.signIn error " + error);
#endif
                    _lastError = error;
                    IsWorking = false;
                    if (error.TypeEquals(ErrorType.PHONE_NUMBER_UNOCCUPIED))
                    {
                        _callTimer.Stop();
                        StateService.SentCode = _sentCode;
                        StateService.ClearNavigationStack = true;
                        NavigationService.UriFor<SignUpViewModel>().Navigate();
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_INVALID))
                    {
                        MessageBox.Show(AppResources.PhoneCodeInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_EMPTY))
                    {
                        MessageBox.Show(AppResources.PhoneCodeEmpty, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_EXPIRED))
                    {
                        MessageBox.Show(AppResources.PhoneCodeExpiredString, AppResources.Error, MessageBoxButton.OK);
                        OnBackKeyPress();
                        NavigationService.GoBack();
                    }
                    else if (error.TypeEquals(ErrorType.SESSION_PASSWORD_NEEDED))
                    {
                        IsWorking = true;
                        MTProtoService.GetPasswordAsync(
                            password => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                _callTimer.Stop();
                                StateService.Password = password;
                                StateService.RemoveBackEntry = true;
                                NavigationService.UriFor<ConfirmPasswordViewModel>().Navigate();
                            }),
                            error2 =>
                            {
                                IsWorking = false;
                                Telegram.Api.Helpers.Execute.ShowDebugMessage("account.getPassword error " + error);
                            });
                    }
                    else if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                    }
                    else
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.signIn error " + error);
                    }
                }));
        }


        public static void UpdateNotificationsAsync(IMTProtoService mtProtoService, IStateService stateService)
        {
            stateService.GetNotifySettingsAsync(s =>
            {
                mtProtoService.GetNotifySettingsAsync(new TLInputNotifyUsers(),
                    settings =>
                    {
                        var notifySettings = settings as TLPeerNotifySettings;
                        if (notifySettings != null)
                        {
                            s.ContactAlert = notifySettings.MuteUntil == null || notifySettings.MuteUntil.Value == 0;
                            s.ContactMessagePreview = notifySettings.ShowPreviews != null && notifySettings.ShowPreviews.Value;

                            var sound = notifySettings.Sound == null ? null : stateService.Sounds.FirstOrDefault(x => string.Equals(x, notifySettings.Sound.Value, StringComparison.OrdinalIgnoreCase));
                            s.ContactSound = sound ?? stateService.Sounds[0];

                            stateService.SaveNotifySettingsAsync(s);
                        }
                    },
                    error =>
                    {

                    });

                mtProtoService.GetNotifySettingsAsync(new TLInputNotifyChats(),
                    settings =>
                    {
                        var notifySettings = settings as TLPeerNotifySettings;
                        if (notifySettings != null)
                        {
                            s.GroupAlert = notifySettings.MuteUntil == null || notifySettings.MuteUntil.Value == 0;
                            s.GroupMessagePreview = notifySettings.ShowPreviews != null && notifySettings.ShowPreviews.Value;

                            var sound = notifySettings.Sound == null ? null : stateService.Sounds.FirstOrDefault(x => string.Equals(x, notifySettings.Sound.Value, StringComparison.OrdinalIgnoreCase));
                            s.GroupSound = sound ?? stateService.Sounds[0];

                            stateService.SaveNotifySettingsAsync(s);
                        }
                    },
                    error =>
                    {

                    });
            });
        }

        private void ConfirmChangePhoneNumber()
        {
            IsWorking = true;
            StateService.PhoneCode = new TLString(Code);

            MTProtoService.ChangePhoneAsync(
                StateService.PhoneNumber, StateService.PhoneCodeHash, StateService.PhoneCode,
                auth => BeginOnUIThread(() =>
                {
                    TLUtils.IsLogEnabled = false;
                    TLUtils.LogItems.Clear();

                    TimeCounterString = string.Empty;
                    HelpVisibility = Visibility.Collapsed;
                    _callTimer.Stop();


                    _isProcessing = false;
                    auth.NotifyOfPropertyChange(() => auth.Phone);
                    NavigationService.RemoveBackEntry();
                    NavigationService.GoBack();
                    IsWorking = false;
                }),
                error => BeginOnUIThread(() =>
                {
                    _lastError = error;
                    IsWorking = false;
                    if (error.TypeEquals(ErrorType.PHONE_NUMBER_UNOCCUPIED))
                    {
                        _callTimer.Stop();
                        StateService.SentCode = _sentCode;
                        StateService.ClearNavigationStack = true;
                        NavigationService.UriFor<SignUpViewModel>().Navigate();
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_INVALID))
                    {
                        MessageBox.Show(AppResources.PhoneCodeInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_EMPTY))
                    {
                        MessageBox.Show(AppResources.PhoneCodeEmpty, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_EXPIRED))
                    {
                        MessageBox.Show(AppResources.PhoneCodeExpiredString, AppResources.Error, MessageBoxButton.OK);
                        OnBackKeyPress();
                        NavigationService.GoBack();
                    }
                    else if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                    }
                    else
                    {
#if DEBUG
                        MessageBox.Show(error.ToString());
#endif
                    }
                }));
        }

        public void Handle(string command)
        {
            if (string.Equals(command, Commands.LogOutCommand))
            {
                Code = string.Empty;
                IsWorking = false;
                HelpVisibility = Visibility.Collapsed;
            }
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
            body.AppendLine("Page: Confirm Code");
            body.AppendLine("Phone: " + "+" + StateService.PhoneNumber);
            body.AppendLine("App version: " + _extendedDeviceInfoService.AppVersion);
            body.AppendLine("OS version: " + _extendedDeviceInfoService.SystemVersion);
            body.AppendLine("Device Name: " + _extendedDeviceInfoService.Model);
            body.AppendLine("Location: " + Telegram.Api.Helpers.Utils.CurrentUICulture());
            body.AppendLine("Wi-Fi: " + _extendedDeviceInfoService.IsWiFiEnabled);
            body.AppendLine("Mobile Network: " + _extendedDeviceInfoService.IsCellularDataEnabled);
            body.AppendLine("Last error: " + ((_lastError != null) ? _lastError.ToString() : null));
            body.AppendLine("Log" + Environment.NewLine + logBuilder);

            var task = new EmailComposeTask();
            task.Body = body.ToString();
            task.To = Constants.LogEmail;
            task.Subject = "WP registration/login issue (" + _extendedDeviceInfoService.AppVersion + ", Code) " + StateService.PhoneNumber;
            task.Show();
        }

        protected override void OnDeactivate(bool close)
        {
            //_callTimer.Stop();
            base.OnDeactivate(close);
        }
    }
}
