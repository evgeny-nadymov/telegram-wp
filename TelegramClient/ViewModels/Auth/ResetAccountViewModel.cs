// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using Language = TelegramClient.Utils.Language;

namespace TelegramClient.ViewModels.Auth
{
    public class ResetAccountViewModel : ViewModelBase
    {
        private bool _isTimerVisible = true;

        public bool IsTimerVisible
        {
            get { return _isTimerVisible; }
            set { _isTimerVisible = value; }
        }

        public string ConfirmWaitString { get; set; }

        public string Subtitle
        {
            get { return string.Format(AppResources.ResetAccountDescription, PhoneNumberConverter.Convert(StateService.PhoneNumber)); }
        }

        private TimeSpan _confirmWait;

        private DateTime _startTime;

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private readonly TLSentCodeBase _sentCode;

        public ResetAccountViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _timer.Tick += OnTimerTick;

            var confirmWait = StateService.ConfirmWait;
            StateService.ConfirmWait = 0;

            _sentCode = StateService.SentCode;
            StateService.SentCode = null;

            UpdateInterval(confirmWait);
        }

        private void UpdateInterval(int interval)
        {
            if (interval > 0)
            {
                IsTimerVisible = true;
                NotifyOfPropertyChange(() => IsTimerVisible);
            }

            _confirmWait = TimeSpan.FromSeconds(interval);
            _timer.Interval = interval < 3600.0 ? TimeSpan.FromSeconds(0.25) : TimeSpan.FromSeconds(1.0);

            _startTime = DateTime.Now;

            UpdateConfirmWaitString();
        }

        private void OnTimerTick(object sender, System.EventArgs e)
        {
            UpdateConfirmWaitString();
        }

        private void UpdateConfirmWaitString()
        {
            var interval = _confirmWait.TotalSeconds - (DateTime.Now - _startTime).TotalSeconds;
            if (interval <= 1.0)
            {
                IsTimerVisible = false;
                NotifyOfPropertyChange(() => IsTimerVisible);
            }
            else if (interval < 0.0)
            {
                interval = 0.0;
                _timer.Stop();
            }
            else if (interval < 3600.0)
            {
                _timer.Interval = TimeSpan.FromSeconds(0.25);
            }

            var confirmWait = TimeSpan.FromSeconds(interval);

            var builder = new StringBuilder();
            if (confirmWait.Days > 0)
            {
                var days = Language.Declension(
                    confirmWait.Days,
                    AppResources.DayNominativeSingular,
                    AppResources.DayNominativePlural,
                    AppResources.DayGenitiveSingular,
                    AppResources.DayGenitivePlural,
                    confirmWait.Days < 2
                        ? string.Format("**{1}** {0}", AppResources.DayNominativeSingular, Math.Abs(confirmWait.Days)).ToLowerInvariant()
                        : string.Format("**{1}** {0}", AppResources.DayNominativePlural, Math.Abs(confirmWait.Days))).ToLowerInvariant();

                builder.Append(string.Format("{0} ", days));
            }
            if (confirmWait.Hours > 0 || confirmWait.Days > 0)
            {
                var hours = Language.Declension(
                    confirmWait.Hours,
                    AppResources.HourNominativeSingular,
                    AppResources.HourNominativePlural,
                    AppResources.HourGenitiveSingular,
                    AppResources.HourGenitivePlural,
                    confirmWait.Hours < 2
                        ? string.Format("**{1}** {0}", AppResources.HourNominativeSingular, Math.Abs(confirmWait.Hours)).ToLowerInvariant()
                        : string.Format("**{1}** {0}", AppResources.HourNominativePlural, Math.Abs(confirmWait.Hours))).ToLowerInvariant();

                builder.Append(string.Format("{0} ", hours));
            }
            if (confirmWait.Minutes > 0 || confirmWait.Hours > 0 || confirmWait.Days > 0)
            {
                var minutes = Language.Declension(
                    confirmWait.Minutes,
                    AppResources.MinuteNominativeSingular,
                    AppResources.MinuteNominativePlural,
                    AppResources.MinuteGenitiveSingular,
                    AppResources.MinuteGenitivePlural,
                    confirmWait.Minutes < 2
                        ? string.Format("**{1}** {0}", AppResources.MinuteNominativeSingular, Math.Abs(confirmWait.Minutes)).ToLowerInvariant()
                        : string.Format("**{1}** {0}", AppResources.MinuteNominativePlural, Math.Abs(confirmWait.Minutes))).ToLowerInvariant();

                builder.Append(string.Format("{0} ", minutes));
            }
            if (confirmWait.TotalSeconds < 3600)
            {
                var seconds = Language.Declension(
                    confirmWait.Seconds,
                    AppResources.SecondNominativeSingular,
                    AppResources.SecondNominativePlural,
                    AppResources.SecondGenitiveSingular,
                    AppResources.SecondGenitivePlural,
                    confirmWait.Seconds < 2
                        ? string.Format("**{1}** {0}", AppResources.SecondNominativeSingular, Math.Abs(confirmWait.Seconds)).ToLowerInvariant()
                        : string.Format("**{1}** {0}", AppResources.SecondNominativePlural, Math.Abs(confirmWait.Seconds))).ToLowerInvariant();

                builder.Append(string.Format("{0} ", seconds));
            }

            ConfirmWaitString = builder.ToString();
            NotifyOfPropertyChange(() => ConfirmWaitString);
            NotifyOfPropertyChange(() => CanResetAccount);
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            _timer.Start();
        }

        protected override void OnDeactivate(bool close)
        {
            _timer.Stop();

            base.OnDeactivate(close);
        }

        public bool CanResetAccount
        {
            get { return (DateTime.Now - _startTime).TotalSeconds >= _confirmWait.TotalSeconds; }
        }

        public void ResetAccount()
        {
            IsWorking = true;
            MTProtoService.DeleteAccountAsync(
                new TLString("Forgot password"),
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    StateService.RemoveBackEntry = true;
                    StateService.SentCode = _sentCode;
                    NavigationService.UriFor<SignUpViewModel>().Navigate();
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    if (error.TypeEquals("2FA_RECENT_CONFIRM"))
                    {
                        MessageBox.Show(AppResources.ResetAccountError, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeStarsWith("2FA_CONFIRM_WAIT"))
                    {
                        var message = error.Message.ToString().Replace("2FA_CONFIRM_WAIT_", string.Empty);
                        try
                        {
                            var confirmWait = Convert.ToInt32(message);
                            UpdateInterval(confirmWait);
                            _timer.Start();
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    else
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.deleteAccount error " + error);
                    }
                }));
        }

        public void LogOut()
        {
            StateService.ClearNavigationStack = true;
            NavigationService.UriFor<SignInViewModel>().Navigate();
        }
    }
}
