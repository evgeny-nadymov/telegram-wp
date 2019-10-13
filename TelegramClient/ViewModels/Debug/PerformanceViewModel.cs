// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Debug
{
    public class PerformanceViewModel : ViewModelBase
    {
        public PerformanceViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            DisplayName = "performance";
        }

        public IList<string> Items { get { return TLUtils.PerformanceItems; } }

        public bool IsDebugEnabled
        {
            get { return TLUtils.IsPerformanceLogEnabled; }
            set { TLUtils.IsPerformanceLogEnabled = value; }
        }

        public void Clear()
        {
            TLUtils.PerformanceItems.Clear();
            NotifyOfPropertyChange(() => Items);
        }
    }
}
