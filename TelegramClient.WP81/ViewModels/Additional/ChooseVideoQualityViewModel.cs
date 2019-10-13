// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Linq;
using Caliburn.Micro;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class ChooseVideoQualityViewModel : Screen
    {
        public LoopingObservableCollection<TimerSpan> Items { get; private set; }

        private readonly IStateService _stateService;

        public string Subtitle { get; set; }

        public ChooseVideoQualityViewModel(IStateService stateService)
        {
            _stateService = stateService;

            var selectedTimerSpan = _stateService.SelectedTimerSpan;
            _stateService.SelectedTimerSpan = null;

            var timerSpans = _stateService.TimerSpans;
            _stateService.TimerSpans = null;

            TimerSpan defaultSpan = null;
            Items = new LoopingObservableCollection<TimerSpan>();
            if (timerSpans != null)
            {
                foreach (var timerSpan in timerSpans)
                {
                    Items.Add(timerSpan);
                }
            }
            else
            {
                Items.Add(new TimerSpan(string.Empty, AppResources.Auto, 0));
                Items.Add(new TimerSpan(string.Empty, "240", 240));
                Items.Add(new TimerSpan(string.Empty, "360", 360));
                Items.Add(new TimerSpan(string.Empty, "480", 480));
                Items.Add(new TimerSpan(string.Empty, "720", 720));
                Items.Add(new TimerSpan(string.Empty, "1080", 1080));
            }
            defaultSpan = Items[0];

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
        }

        public void Cancel()
        {

        }
    }
}
