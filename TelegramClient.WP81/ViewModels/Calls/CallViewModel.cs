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
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using TelegramClient.Controls;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Calls;
using TelegramClient.Views.Contacts;
using TelegramClient.Views.Dialogs;

namespace TelegramClient.ViewModels.Calls
{
    public class CallViewModel : PropertyChangedBase
    {
        public TLUserBase User { get; set; }

        public string Status { get; set; }

        public string Emojis { get; set; }

        public string EmojiKeyDescription
        {
            get { return string.Format(AppResources.EmojiKeyDescription, User != null ? User.ShortName : string.Empty); }
        }

        private readonly DispatcherTimer _callTimer;

        private DateTime _callStartTime;

        private CallViewState _viewState;

        public CallViewState ViewState
        {
            get { return _viewState; }
            set
            {
                if (_viewState != value)
                {
                    _viewState = value;
                    NotifyOfPropertyChange(() => ViewState);
                }
            }
        }

        private int _signal;

        public int Signal
        {
            get { return _signal; }
            set
            {
                if (_signal != value)
                {
                    _signal = value;
                    NotifyOfPropertyChange(() => Signal);
                }
            }
        }

        private readonly IVoIPService _voipService;

        public TLLong CallId { get; set; }

        public CallViewModel(TLUserBase user, IVoIPService voipService)
        {
            _callTimer = new DispatcherTimer();
            _callTimer.Interval = TimeSpan.FromSeconds(0.33);
            _callTimer.Tick += CallTimer_Tick;
            _voipService = voipService;

            User = user;
        }

        public void ChangeCallState(PhoneCallState state)
        {
            System.Diagnostics.Debug.WriteLine(state);
            if (state == PhoneCallState.STATE_WAITING_INCOMING)
            {
                Status = AppResources.VoipIncoming;
            }
            else if (state == PhoneCallState.STATE_WAIT_INIT || state == PhoneCallState.STATE_WAIT_INIT_ACK)
            {
                Status = AppResources.VoipConnecting;
                ViewState = CallViewState.CallConnecting;
            }
            else if (state == PhoneCallState.STATE_EXCHANGING_KEYS)
            {
                Status = AppResources.VoipExchangingKeys;
                ViewState = CallViewState.CallConnecting;
            }
            else if (state == PhoneCallState.STATE_WAITING)
            {
                Status = AppResources.VoipWaiting;
            }
            else if (state == PhoneCallState.STATE_RINGING)
            {
                Status = AppResources.VoipRinging;
            }
            else if (state == PhoneCallState.STATE_REQUESTING)
            {
                Status = AppResources.VoipRequesting;
            }
            else if (state == PhoneCallState.STATE_HANGING_UP)
            {
                Status = AppResources.VoipHangingUp;
            }
            else if (state == PhoneCallState.STATE_ENDED)
            {
                Status = AppResources.VoipEnded;
            }
            else if (state == PhoneCallState.STATE_BUSY)
            {
                Status = AppResources.VoipBusy;
            }
            else if (state == PhoneCallState.STATE_ESTABLISHED)
            {
                StartTimer();
                ViewState = CallViewState.Call;
            }
            else if (state == PhoneCallState.STATE_FAILED)
            {
                Status = AppResources.VoipFailed;
            }

            NotifyOfPropertyChange(() => Status);
        }

        public void StartTimer()
        {
            _callStartTime = DateTime.Now;
            _callTimer.Start();
        }

        public void StartTimer(DateTime callStartTime)
        {
            _callStartTime = callStartTime;
            _callTimer.Start();
        }

        public void StopTimer()
        {
            _callTimer.Stop();
        }

        private void CallTimer_Tick(object sender, System.EventArgs eventArgs)
        {
            var callTime = DateTime.Now - _callStartTime;
            if (callTime.TotalHours >= 10.0)
            {
                Status = callTime.ToString(@"hh\:mm\:ss");
            }
            else if (callTime.TotalHours >= 1.0)
            {
                Status = callTime.ToString(@"h\:mm\:ss");
            }
            else if (callTime.TotalMinutes >= 10.0)
            {
                Status = callTime.ToString(@"mm\:ss");
            }
            else
            {
                Status = callTime.ToString(@"m\:ss");
            }

            NotifyOfPropertyChange(() => Status);
        }

        public void SwitchSpeaker(bool external)
        {
            _voipService.SwitchSpeaker(external);
        }

        public void Mute(bool muted)
        {
            _voipService.Mute(muted);
        }

        public void OpenChat()
        {
            if (User == null) return;

            var contactView = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as ContactView;
            if (contactView != null)
            {
                var withUser = contactView.ViewModel.CurrentContact as TLUser;
                if (withUser != null && withUser.Index == User.Index)
                {
                    var navigationService = IoC.Get<INavigationService>();
                    if (navigationService != null)
                    {
                        var backEntry = navigationService.BackStack.FirstOrDefault();
                        if (backEntry != null && backEntry.Source.ToString().Contains("DialogDetailsView.xaml"))
                        {
                            navigationService.GoBack();

                            return;
                        }
                    }
                }
            }

            var dialogDetailsView = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as DialogDetailsView;
            if (dialogDetailsView != null)
            {
                if (dialogDetailsView.ViewModel != null)
                {
                    var withUser = dialogDetailsView.ViewModel.With as TLUser;
                    if (withUser != null && withUser.Index == User.Index)
                    {
                        return;
                    }
                }

                TelegramTransitionService.SetNavigationOutTransition(dialogDetailsView, null);
            }

            var secretDialogDetailsView = ((PhoneApplicationFrame)Application.Current.RootVisual).Content as SecretDialogDetailsView;
            if (secretDialogDetailsView != null)
            {
                if (secretDialogDetailsView.ViewModel != null)
                {
                    var withUser = secretDialogDetailsView.ViewModel.With as TLUser;
                    if (withUser != null && withUser.Index == User.Index)
                    {
                        return;
                    }
                }

                TelegramTransitionService.SetNavigationOutTransition(secretDialogDetailsView, null);
            }

            IoC.Get<IStateService>().With = User;
            IoC.Get<IStateService>().RemoveBackEntries = true;
            IoC.Get<INavigationService>().UriFor<DialogDetailsViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
        }
    }
}
