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
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class ChooseNotificationSpanViewModel : ViewModelBase
    {
         public LoopingObservableCollection<TimerSpan> Items { get; private set; }

         public ChooseNotificationSpanViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) : 
            base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new LoopingObservableCollection<TimerSpan>
            {
                new TimerSpan(AppResources.Enabled, string.Empty, 0),
                new TimerSpan(AppResources.HourNominativeSingular,  "1", (int)TimeSpan.FromHours(1.0).TotalSeconds),
                new TimerSpan(AppResources.HourGenitivePlural, "8", (int)TimeSpan.FromHours(8.0).TotalSeconds),
                new TimerSpan(AppResources.DayNominativePlural, "2", (int)TimeSpan.FromDays(2.0).TotalSeconds),
                new TimerSpan(AppResources.Disabled, string.Empty, int.MaxValue),
            };

            if (StateService.SelectedTimerSpan == null)
            {
                Items.SelectedItem = Items[0];
            }
            else
            {
                var selectedItem = Items.FirstOrDefault(x => x.Seconds == StateService.SelectedTimerSpan.Seconds);
                Items.SelectedItem = selectedItem ?? Items[0];
            }
            StateService.SelectedTimerSpan = null;
        }

        public void Done()
        {
            StateService.SelectedTimerSpan = (TimerSpan)Items.SelectedItem;
            NavigationService.GoBack();
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }
    }
}
