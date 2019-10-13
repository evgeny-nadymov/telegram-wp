// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Media
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        public string IsoFileName { get; set; }

        public VideoPlayerViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            IsoFileName = StateService.IsoFileName;
            StateService.IsoFileName = null;
        }
    }
}
