// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;

namespace TelegramClient.ViewModels.Additional
{
    public class AccountSelfDestructsViewModel : ViewModelBase
    {
        private Period _selectedPeriod;

        public Period SelectedPeriod
        {
            get { return _selectedPeriod; }
            set { SetField(ref _selectedPeriod, value, () => SelectedPeriod); }
        }

        public List<Period> Periods { get; set; }

        private int _accountDaysTTL;

        public AccountSelfDestructsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _accountDaysTTL = StateService.AccountDaysTTL;
            StateService.AccountDaysTTL = 0;

            Periods = new List<Period>
            {
                new Period{Label = Language.Declension(
                    1,
                    AppResources.MonthNominativeSingular,
                    AppResources.MonthNominativePlural,
                    AppResources.MonthGenitiveSingular,
                    AppResources.MonthGenitivePlural).ToLower(CultureInfo.CurrentUICulture), Days = 30},
                new Period{Label = Language.Declension(
                    3,
                    AppResources.MonthNominativeSingular,
                    AppResources.MonthNominativePlural,
                    AppResources.MonthGenitiveSingular,
                    AppResources.MonthGenitivePlural).ToLower(CultureInfo.CurrentUICulture), Days = 90},
                new Period{Label = Language.Declension(
                    6,
                    AppResources.MonthNominativeSingular,
                    AppResources.MonthNominativePlural,
                    AppResources.MonthGenitiveSingular,
                    AppResources.MonthGenitivePlural).ToLower(CultureInfo.CurrentUICulture), Days = 180},
                new Period{Label = Language.Declension(
                    1,
                    AppResources.YearNominativeSingular,
                    AppResources.YearNominativePlural,
                    AppResources.YearGenitiveSingular,
                    AppResources.YearGenitivePlural).ToLower(CultureInfo.CurrentUICulture), Days = 365},
            };
            _selectedPeriod = GetSelectedPeriod(Periods, _accountDaysTTL, Periods[2]);

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => SelectedPeriod) && SelectedPeriod != null)
            {
                IsWorking = true;
                MTProtoService.SetAccountTTLAsync(
                    new TLAccountDaysTTL{Days = new TLInt(SelectedPeriod.Days)},
                    result =>
                    {
                        IsWorking = false;
                    },
                    error =>
                    {
                        IsWorking = false;
                        if (error.CodeEquals(ErrorCode.FLOOD))
                        {
                            MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                        }

                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.setAccountTTL error " + error);
                    });
            }
        }

        private static Period GetSelectedPeriod(IList<Period> periods, int accountDaysTTL, Period defaultPeriod)
        {
            if (accountDaysTTL == 0)
            {
                return defaultPeriod;
            }

            var diff = int.MaxValue;
            Period selectedPeriod = null;
            foreach (var period in periods)
            {
                var nextdDiff = Math.Abs(accountDaysTTL - period.Days);
                if (nextdDiff < diff)
                {
                    diff = nextdDiff;
                    selectedPeriod = period;
                }
            }

            return selectedPeriod ?? defaultPeriod;
        }
    }

    public class Period
    {
        public string Label { get; set; }

        public int Days { get; set; }

        public override string ToString()
        {
            return Label;
        }
    }
}
