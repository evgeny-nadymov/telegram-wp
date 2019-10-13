// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class AboutViewModel : ViewModelBase
    {
        public Uri ApplicationIconSource
        {
            get
            {
#if WP81
                return new Uri("/ApplicationIcon106.png", UriKind.Relative);
#elif WP8
                return new Uri("/ApplicationIcon210.png", UriKind.Relative);
#endif
                return new Uri("/ApplicationIcon99.png", UriKind.Relative);
            }
        }

        public string Version
        {
            get
            {
                return _extendedDeviceInfoService.AppVersion
#if WP81
                +" WP8.1";
#elif WP8
                + " WP8";
#else
                + " WP7";
#endif
            }
        }

        private readonly IExtendedDeviceInfoService _extendedDeviceInfoService;

        public AboutViewModel(IExtendedDeviceInfoService extendedDeviceInfoService, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _extendedDeviceInfoService = extendedDeviceInfoService;

            var datePicker = new DatePicker();
        }

        public void OpenPrivacyStatement()
        {
            NavigationService.UriFor<PrivacyStatementViewModel>().Navigate();
        }

        public void OpenSpecialThanks()
        {
            NavigationService.UriFor<SpecialThanksViewModel>().Navigate();
        }
    }
}
