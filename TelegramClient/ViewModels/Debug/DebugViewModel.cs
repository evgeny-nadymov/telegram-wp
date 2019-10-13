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
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels
{
    public class DebugViewModel : ViewModelBase
    {
        public DebugViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            App.Log("DebugViewModel sstart .ctor");
            DisplayName = "debug";
            App.Log("DebugViewModel end .ctor");
        }

        public IList<string> Items { get { return TLUtils.DebugItems; } }

        public bool IsDebugEnabled
        {
            get { return TLUtils.IsDebugEnabled; }
            set { TLUtils.IsDebugEnabled = value; }
        }
       
         
        public void Send()
        {
            var body = new StringBuilder();
            foreach (var debugItem in TLUtils.DebugItems)
            {
                body.Append(debugItem + "\n");
            }
        }

        public void Clear()
        {
            TLUtils.DebugItems.Clear();
            NotifyOfPropertyChange(() => Items);
        }
    }
}
