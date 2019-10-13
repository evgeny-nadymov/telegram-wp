// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.Services;
using TelegramClient.ViewModels.Auth;

namespace TelegramClient.ViewModels.Additional
{
    public class StartupViewModel : Screen
    {
        private readonly INavigationService _navigationService;

        private readonly IStateService _stateService;

        public StartupViewModel(INavigationService navigationService, IStateService stateService)
        {
            _navigationService = navigationService;
            _stateService = stateService;
        }

        public void StartMessaging()
        {
            _stateService.ClearNavigationStack = true;
            _navigationService.UriFor<SignInViewModel>().Navigate();
        }

        protected override void OnActivate()
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            if (_stateService.ClearNavigationStack)
            {
                _stateService.ClearNavigationStack = false;
                while (_navigationService.RemoveBackEntry() != null) { }
            }
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;

            base.OnDeactivate(close);
        }
    }
}
