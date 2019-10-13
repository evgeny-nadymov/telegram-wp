// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Caliburn.Micro;
using Microsoft.Phone.Controls.Primitives;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class ChooseTTLViewModel : Screen
    {
        public LoopingObservableCollection<TimerSpan> Items { get; private set; }

        private readonly INavigationService _navigationService;

        private readonly IStateService _stateService;

        public string Subtitle { get; set; }

        public ChooseTTLViewModel(INavigationService navigationService, IStateService stateService)
        {
            _navigationService = navigationService;
            _stateService = stateService;

            var isEncryptedTimer = _stateService.IsEncryptedTimer;
            _stateService.IsEncryptedTimer = false;

            var selectedTimerSpan = _stateService.SelectedTimerSpan;
            _stateService.SelectedTimerSpan = null;

            Items = new LoopingObservableCollection<TimerSpan>();
            TimerSpan defaultSpan = null;
            if (isEncryptedTimer)
            {
                Items.Add(new TimerSpan(AppResources.OffMasculine, string.Empty, 0));
                for (var i = 1; i <= 15; i++)
                {
                    Items.Add( new TimerSpan(
                        Language.Declension2(
                                i,
                                AppResources.SecondNominativeSingular,
                                AppResources.SecondNominativePlural,
                                AppResources.SecondGenitiveSingular,
                                AppResources.SecondGenitivePlural),  
                                i.ToString(), i));
                }
                Items.Add(new TimerSpan(AppResources.SecondGenitivePlural, "30", 30));
                Items.Add(new TimerSpan(AppResources.MinuteNominativeSingular, "1", 60));
                Items.Add(new TimerSpan(AppResources.HourNominativeSingular, "1", (int) TimeSpan.FromHours(1.0).TotalSeconds));
                Items.Add(new TimerSpan(AppResources.DayNominativeSingular, "1", (int) TimeSpan.FromDays(1.0).TotalSeconds));
                Items.Add(new TimerSpan(AppResources.WeekNominativeSingular, "1", (int) TimeSpan.FromDays(7.0).TotalSeconds));

                defaultSpan = Items[0];
            }
            else
            {
                Items.Add(new TimerSpan(AppResources.OffMasculine, string.Empty, 0));
                for (var i = 1; i <= 30; i++)
                {
                    Items.Add(new TimerSpan(
                        Language.Declension2(
                                i,
                                AppResources.SecondNominativeSingular,
                                AppResources.SecondNominativePlural,
                                AppResources.SecondGenitiveSingular,
                                AppResources.SecondGenitivePlural), 
                                i.ToString(), i));
                }
                Items.Add(new TimerSpan(AppResources.SecondGenitivePlural, "35", 35));
                Items.Add(new TimerSpan(AppResources.SecondGenitivePlural, "40", 40));
                Items.Add(new TimerSpan(AppResources.SecondGenitivePlural, "45", 45));
                Items.Add(new TimerSpan(AppResources.SecondGenitivePlural, "50", 50));
                Items.Add(new TimerSpan(AppResources.SecondGenitivePlural, "55", 55));
                Items.Add(new TimerSpan(AppResources.MinuteNominativeSingular, "1", 60));

                defaultSpan = Items[10];
            }

            if (selectedTimerSpan != null)
            {
                Items.SelectedItem = Items.FirstOrDefault(x => x.Seconds == selectedTimerSpan.Seconds);
            }
            if (Items.SelectedItem == null)
            {
                Items.SelectedItem = defaultSpan;
            }
        }

        public void Done()
        {
            _stateService.SelectedTimerSpan = (TimerSpan)Items.SelectedItem;
            _navigationService.GoBack();
        }

        public void Cancel()
        {
            _navigationService.GoBack();
        }
    }

    public class LoopingObservableCollection<T> : ObservableCollection<T>, ILoopingSelectorDataSource
    {
        public object GetNext(object relativeTo)
        {

            if (relativeTo == null) return this[0];

            var item = (T)relativeTo;
            var position = IndexOf(item);
            if (position + 1 == Count)
            {
                return this[0];
            }

            return this[position + 1];
        }

        public object GetPrevious(object relativeTo)
        {
            if (relativeTo == null) return this[Count - 1];

            var item = (T)relativeTo;
            var position = IndexOf(item);
            if (position == 0)
            {
                return this[Count - 1];
            }

            return this[position - 1];
        }

        public object SelectedItem { get; set; }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
    }
}
