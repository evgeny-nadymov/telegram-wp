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
using System.Threading;
using System.Threading.Tasks;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.TL;
using Telegram.Api.Transport; 

namespace TelegramClient.Services
{
    public class ProxyChecker : IProxyChecker
    {
        private object _syncRoot = new object();

        private readonly ITransportService _transportService;

        private readonly IMTProtoService _mtProtoService;

        private List<ProxyItem> _list = new List<ProxyItem>();

        //private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(4);

        public ProxyChecker(ITransportService transportService, IMTProtoService mtProtoService)
        {
            _transportService = transportService;
            _mtProtoService = mtProtoService;
        }

        private TLInt CheckProxyInternal(ProxyItem item)
        {
            TLInt ping = null;
            var handler = new ManualResetEvent(false);
            _mtProtoService.PingProxyAsync(item.Proxy,
                result =>
                {
                    ping = result;
                    handler.Set();
                },
                error =>
                {
                    handler.Set();
                });

#if DEBUG
            var complete = handler.WaitOne(Timeout.Infinite);
#else
            var complete = handler.WaitOne(TimeSpan.FromSeconds(item.Timeout));
#endif

            return ping;
        }

        public void CheckAsync(TLProxyBase proxy, double timeout, Action<ProxyItem, TLInt> callback)
        {
            var proxyItem = new ProxyItem
            {
                Proxy = proxy,
                Timeout = timeout
            };

            var cts = new CancellationTokenSource();
            var task = new Task(() =>
            {
                var ping = CheckProxyInternal(proxyItem);
                _list.Remove(proxyItem);
                if (!cts.Token.IsCancellationRequested)
                {
                    callback.SafeInvoke(proxyItem, ping);
                }
            });

            proxyItem.TokenSource = cts;
            proxyItem.Task = task;

            _list.Add(proxyItem);

            task.Start();
        }

        public void CancelAsync(TLProxyBase proxy)
        {
            var item = _list.FirstOrDefault(x => x.Proxy == proxy);
            if (item != null)
            {
                _list.Remove(item);
                item.TokenSource.Cancel();
            }
        }
    }

    public class ProxyItem
    {
        public TLProxyBase Proxy { get; set; }

        public double Timeout { get; set; }

        public CancellationTokenSource TokenSource { get; set; }

        public Task Task { get; set; }
    }
}
