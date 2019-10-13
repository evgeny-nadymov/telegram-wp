// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class BioViewModel : ViewModelBase
    {
        public string About { get; set; }

        private TLUser45 _currentUser;

        public BioViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _currentUser = stateService.CurrentContact as TLUser45;
            stateService.CurrentContact = null;

            About = _currentUser != null && _currentUser.About != null ? _currentUser.About.ToString() : string.Empty;
        }

        public void Done()
        {
            if (IsWorking) return;

            //_currentUser.About = new TLString(About);
            //_currentUser.NotifyOfPropertyChange(() => _currentUser.About);
            //NavigationService.GoBack();
            //return;
            IsWorking = true;
            MTProtoService.UpdateProfileAsync(null, null, new TLString(About), 
                user => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    _currentUser.About = new TLString(About);
                    _currentUser.NotifyOfPropertyChange(() => _currentUser.About);
                    NavigationService.GoBack();
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }));
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }
    }
}
