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
    public class CallsSecurityViewModel : ViewModelBase
    {
        private TLCallsSecurity _callsSecurity;

        public TLCallsSecurity CallsSecurity
        {
            get { return _callsSecurity; }
            set { SetField(ref _callsSecurity, value, () => CallsSecurity); }
        }

        public CallsSecurityViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _callsSecurity = StateService.GetCallsSecurity(true);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            StateService.SaveCallsSecurity(_callsSecurity);
        }
    }
}
