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
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class PasscodeViewModel : ViewModelBase
    {
        private TimerSpan _selectedAutolockSpan;

        public TimerSpan SelectedAutolockSpan
        {
            get { return _selectedAutolockSpan; }
            set
            {
                if (value != null)
                {
                    PasscodeUtils.AutolockTimeout = value.Seconds;
                }
                SetField(ref _selectedAutolockSpan, value, () => SelectedAutolockSpan);
            }
        }

        public IList<TimerSpan> AutolockSpans { get; protected set; } 

        private bool _passcodeEnabled;

        public bool PasscodeEnabled
        {
            get { return _passcodeEnabled; }
            set { SetField(ref _passcodeEnabled, value, () => PasscodeEnabled); }
        }

        private const int AutolockSpanDefaultIndex = 3;

        public PasscodeViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            AutolockSpans = new List<TimerSpan>
            {
                new TimerSpan(AppResources.AutolockDisabled, string.Empty, int.MaxValue, AppResources.Disabled),
                new TimerSpan(AppResources.MinuteNominativeSingular,  "1", (int)TimeSpan.FromMinutes(1.0).TotalSeconds, string.Format(AppResources.PasscodeAutolockIn, string.Format("{0} {1}", "1", AppResources.MinuteNominativeSingular).ToLowerInvariant())),
                new TimerSpan(AppResources.MinuteGenitivePlural, "5", (int)TimeSpan.FromMinutes(5.0).TotalSeconds, string.Format(AppResources.PasscodeAutolockIn, string.Format("{0} {1}", "5", AppResources.MinuteGenitivePlural).ToLowerInvariant())),
                new TimerSpan(AppResources.HourNominativeSingular,  "1", (int)TimeSpan.FromHours(1.0).TotalSeconds, string.Format(AppResources.PasscodeAutolockIn, string.Format("{0} {1}", "1", AppResources.HourNominativeSingular).ToLowerInvariant())),
                new TimerSpan(AppResources.HourGenitivePlural, "5", (int)TimeSpan.FromHours(5.0).TotalSeconds, string.Format(AppResources.PasscodeAutolockIn, string.Format("{0} {1}", "5", AppResources.HourGenitivePlural).ToLowerInvariant())),               
            };
            _selectedAutolockSpan = AutolockSpans.FirstOrDefault(x => x.Seconds == PasscodeUtils.AutolockTimeout) ?? AutolockSpans[AutolockSpanDefaultIndex];

            _passcodeEnabled = PasscodeUtils.IsEnabled;

            PropertyChanged += (o, e) =>
            {
                if (Property.NameEquals(e.PropertyName, () => PasscodeEnabled))
                {
                    if (PasscodeEnabled)
                    {
                        ChangePasscode();

                        if (!PasscodeUtils.IsEnabled)
                        {
                            PasscodeUtils.Reset();
                            SelectedAutolockSpan = AutolockSpans[AutolockSpanDefaultIndex];
                        }
                    }
                    else
                    {
                        PasscodeUtils.Reset();
                        UpdateDeviceLockedAsync();
                    }
                }
                else if (Property.NameEquals(e.PropertyName, () => SelectedAutolockSpan))
                {
                    UpdateDeviceLockedAsync();
                }
            };
        }

        private void UpdateDeviceLockedAsync()
        {
            var shellViewModel = IoC.Get<ShellViewModel>();
            if (shellViewModel != null)
            {
                shellViewModel.UpdateDeviceLockedAsync();
            }
        }

        protected override void OnActivate()
        {
            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            PasscodeEnabled = PasscodeUtils.IsEnabled;

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            if (!PasscodeEnabled)
            {
                PasscodeUtils.Reset();
            } 

            base.OnDeactivate(close);
        }

        public void ChangePasscode()
        {
            StateService.SelectedAutolockTimeout = _selectedAutolockSpan != null ? _selectedAutolockSpan.Seconds : AutolockSpans[AutolockSpanDefaultIndex].Seconds;
            
            BeginOnUIThread(TimeSpan.FromSeconds(0.25), () => NavigationService.UriFor<ChangePasscodeViewModel>().Navigate());
        }
    }
}
