// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class LogViewModel : ViewModelBase
    {
        public LogViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            DisplayName = "log";
        }

        public IList<string> Items { get { return TLUtils.LogItems; } }

        public bool IsLogEnabled
        {
            get { return TLUtils.IsLogEnabled; }
            set { TLUtils.IsLogEnabled = value; }
        }
       
         
        public void Send(string email, string subject, params string[] footer)
        {
            var body = new StringBuilder();
            foreach (var debugItem in TLUtils.LogItems)
            {
                body.AppendLine(debugItem);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            body.AppendLine(timestamp + ": Send log");
            body.AppendLine();
            foreach (var item in footer)
            {
                body.AppendLine(item);
            }

            var task = new EmailComposeTask();
            task.Body = body.ToString();
            task.To = email;
            task.Subject = subject;
            task.Show();
        }

        public void Clear()
        {
            TLUtils.LogItems.Clear();
            NotifyOfPropertyChange(() => Items);
        }
    }
}
