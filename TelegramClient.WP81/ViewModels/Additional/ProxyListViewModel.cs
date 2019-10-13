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
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.TL;
using Telegram.Api.Transport;
using TelegramClient.Helpers;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Additional;
using Execute = Caliburn.Micro.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class ProxyListViewModel : ItemsViewModelBase<TLProxyBase>
    {
        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetField(ref _isEnabled, value, () => IsEnabled); }
        }

        private bool _useForCalls;

        public bool UseForCalls
        {
            get { return _useForCalls; }
            set { SetField(ref _useForCalls, value, () => UseForCalls); }
        }

        private readonly ITransportService _transportService;

        public bool SuppressSharing { get; protected set; }

        private TLProxyConfig76 _proxyConfig;

        private readonly IProxyChecker _proxyChecker;

        public ProxyListViewModel(IProxyChecker proxyChecker, ITransportService transportService, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            SuppressSharing = StateService.SuppressProxySharing;
            StateService.SuppressProxySharing = false;

            _proxyChecker = proxyChecker;
            _transportService = transportService;

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => IsEnabled))
                {
                    var proxy = _proxyConfig.GetProxy();
                    _proxyConfig.IsEnabled = new TLBool(IsEnabled);
                    if (proxy != null)
                    {
                        proxy.NotifyOfPropertyChange(() => proxy.Self);
                    }
                    SetReconnect(!_proxyConfig.IsEmpty, _proxyConfig, null);
                }
                else if (Property.NameEquals(args.PropertyName, () => UseForCalls))
                {
                    var proxy = _proxyConfig.GetProxy();
                    _proxyConfig.UseForCalls = new TLBool(UseForCalls);
                    if (proxy != null)
                    {
                        proxy.NotifyOfPropertyChange(() => proxy.Self);
                    }
                    SetReconnect(false, _proxyConfig, null);
                }
            };
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            _proxyConfig = _transportService.GetProxyConfig() as TLProxyConfig76;
            if (_proxyConfig != null)
            {
                _isEnabled = _proxyConfig.IsEnabled.Value;
                _useForCalls = _proxyConfig.UseForCalls.Value;

                var now = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
                Items.Clear();
                for (var i = 0; i < _proxyConfig.Items.Count; i++)
                {
                    var item = _proxyConfig.Items[i];
                    item.IsSelected = i == _proxyConfig.SelectedIndex.Value;
                    Items.Add(item);

                    if (CheckProxy(item))
                    {
                        item.Status = ProxyStatus.Connecting;
                        item.Ping = null;
                        _proxyChecker.CheckAsync(item, 10.0,
                            (proxyItem, ping) => Execute.BeginOnUIThread(() =>
                            {
                                proxyItem.Proxy.CheckTime = now;
                                proxyItem.Proxy.Ping = ping;
                                proxyItem.Proxy.Status = item.Ping != null ? ProxyStatus.Available : ProxyStatus.Unavailable;

                                Set(_proxyConfig);
                            }));
                    }
                }
            }

            _transportService.TransportConnected += OnTransportConnected;
            _transportService.TransportConnecting += OnTransportConnecting;
        }

        private bool CheckProxy(TLProxyBase item)
        {
            var lastCheckTime = item.CheckTime;
            if (lastCheckTime == null)
            {
                return true;
            }

            var now = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
            if (lastCheckTime.Value + 60 * 2 < now.Value)
            {
                return true;
            }

            return false;
        }

        private void OnTransportConnecting(object sender, TransportEventArgs e)
        {
            if (e.Transport.MTProtoType == MTProtoTransportType.Main
                && e.Transport.ProxyConfig != null
                && !e.Transport.ProxyConfig.IsEmpty
                && e.Transport.ProxyConfig.IsEnabled.Value)
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    if (_proxyConfig != null)
                    {
                        foreach (var item in _proxyConfig.Items)
                        {
                            if (item == e.Transport.ProxyConfig.GetProxy())
                            {
                                item.Status = ProxyStatus.Connecting;
                                item.Ping = null;
                            }
                        }
                    }
                });
            }
        }

        private void OnTransportConnected(object sender, TransportEventArgs e)
        {
            if (e.Transport.MTProtoType == MTProtoTransportType.Main
                && e.Transport.ProxyConfig != null
                && !e.Transport.ProxyConfig.IsEmpty
                && e.Transport.ProxyConfig.IsEnabled.Value)
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    if (_proxyConfig != null)
                    {
                        for (var i = 0; i < _proxyConfig.Items.Count; i++)
                        {
                            var item = _proxyConfig.Items[i];
                            if (item == e.Transport.ProxyConfig.GetProxy())
                            {
                                var now = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
                                item.CheckTime = now;
                                item.Status = ProxyStatus.Available;
                                item.Ping = e.Transport.Ping > 0 ? new TLInt((int) e.Transport.Ping) : null;
                                if (item.Ping == null)
                                {
                                    PingMainTransportAsync(item, e.Transport);
                                }

                                Set(_proxyConfig);
                            }
                        }
                    }
                });
            }
        }

        private void PingMainTransportAsync(TLProxyBase item, ITransport transport)
        {
            MTProtoService.PingAsync(TLLong.Random(),
                pong => Execute.BeginOnUIThread(() =>
                {
                    item.Ping = transport.Ping > 0
                        ? new TLInt((int)transport.Ping)
                        : null;

                    Set(_proxyConfig);
                }));
        }

        protected override void OnDeactivate(bool close)
        {
            _transportService.TransportConnected -= OnTransportConnected;
            _transportService.TransportConnecting -= OnTransportConnecting;

            base.OnDeactivate(close);
        }

        public void Select(TLProxyBase proxy)
        {
            var proxyConfig = _proxyConfig;
            if (proxyConfig == null) return;
            if (proxyConfig.IsEnabled.Value && proxyConfig.GetProxy() == proxy) return;

            var currentProxy = proxyConfig.GetProxy();
            proxyConfig.IsEnabled = TLBool.True;
            if (currentProxy != null)
            {
                currentProxy.NotifyOfPropertyChange(() => currentProxy.Self);
            }
            IsEnabled = true;
            proxyConfig.SelectedIndex = new TLInt(proxyConfig.Items.IndexOf(proxy));
            foreach (var item in proxyConfig.Items)
            {
                item.IsSelected = item == proxy;
                item.Ping = item.IsSelected ? null : item.Ping;
                item.Status = item.IsSelected ? ProxyStatus.Connecting : (item.Status == ProxyStatus.Connecting ? ProxyStatus.Unavailable : item.Status);
            }

            SetReconnect(true, proxyConfig, proxy);
        }

        private static void Set(TLProxyConfigBase proxyConfig)
        {
            var transportService = IoC.Get<ITransportService>();
            transportService.SetProxyConfig(proxyConfig);
        }

        private static void SetReconnect(bool reconnect, TLProxyConfigBase proxyConfig, TLProxyBase proxy)
        {
            Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("SetRecconect recconect={0} proxyConfig={1}", reconnect, proxyConfig));
            if (proxyConfig == null) return;

            Set(proxyConfig);

            var transportService = IoC.Get<ITransportService>();
            var mtProtoService = IoC.Get<IMTProtoService>();
            var cacheService = IoC.Get<ICacheService>();
            var eventAggregator = IoC.Get<ITelegramEventAggregator>();

            if (reconnect)
            {
                var promoDialogs = cacheService.GetDialogs().OfType<TLDialog71>().Where(x => x.IsPromo).ToList();
                foreach (var dialog in promoDialogs)
                {
                    cacheService.UpdateDialogPromo(dialog, false);
                }

                cacheService.SyncProxyData(null, result => { });

                transportService.Close();
                mtProtoService.PingAsync(TLLong.Random(), 
                    result => Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                    {
                        if (proxy != null)
                        {
                            var now = TLUtils.DateToUniversalTimeTLInt(mtProtoService.ClientTicksDelta, DateTime.Now);
                            proxy.CheckTime = now;
                            proxy.Status = ProxyStatus.Available;
                            proxy.Ping = mtProtoService.GetActiveTransport().Ping > 0
                                ? new TLInt((int)mtProtoService.GetActiveTransport().Ping)
                                : null;

                            Set(proxyConfig);
                        }

                        var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                        if (isAuthorized)
                        {
                            mtProtoService.GetProxyDataAsync(
                                result2 =>
                                {
                                    var proxyDataPromo = result2 as TLProxyDataPromo;
                                    if (proxyDataPromo != null)
                                    {
                                        mtProtoService.GetPromoDialogAsync(mtProtoService.PeerToInputPeer(proxyDataPromo.Peer),
                                            result3 =>
                                            {
                                                if (result3.Dialogs.Count > 0)
                                                {
                                                    eventAggregator.Publish(new ProxyDataChangedEventArgs(proxyDataPromo));
                                                }
                                            });
                                    }
                                    else
                                    {

                                    }

                                    Telegram.Api.Helpers.Execute.ShowDebugMessage(result2.ToString());
                                });
                        }
                    }));
            }

            eventAggregator.Publish(new ProxyChangedEventArgs(proxyConfig));
        }

        public static void ApplySettings(TLProxyConfigBase proxyConfigBase, bool isEnabled, TLProxyBase proxy)
        {
            var proxyConfig = proxyConfigBase ?? TLProxyConfigBase.Empty;

            var proxyConfig76 = proxyConfig as TLProxyConfig76;
            if (proxyConfig76 == null) return;

            proxyConfig.IsEnabled = new TLBool(isEnabled);

            var added = false;
            for (var i = 0; i < proxyConfig76.Items.Count; i++)
            {
                var proxyItem = proxyConfig76.Items[i];
                if (TLProxyBase.ProxyEquals(proxyItem, proxy))
                {
                    proxyConfig76.SelectedIndex = new TLInt(i);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                proxyConfig76.Items.Insert(0, proxy);
                proxyConfig76.SelectedIndex = new TLInt(0);
            }

            SetReconnect(true, proxyConfig76, proxy);
        }

        public void Add()
        {
            StateService.SuppressProxySharing = SuppressSharing;
            NavigationService.UriFor<ProxyViewModel>().Navigate();
        }

        public void Delete(TLProxyBase proxy)
        {
            var proxyConfig = _proxyConfig;
            if (proxyConfig == null) return;

            var selectedProxy = proxyConfig.Items[proxyConfig.SelectedIndex.Value];
            var reconnect = selectedProxy == proxy;
            
            Items.Remove(proxy);
            proxyConfig.Items.Remove(proxy);
            proxyConfig.SelectedIndex = new TLInt(proxyConfig.Items.IndexOf(selectedProxy));
            if (proxyConfig.SelectedIndex.Value < 0)
            {
                if (proxyConfig.Items.Count > 0)
                {
                    proxyConfig.SelectedIndex = new TLInt(0);
                    selectedProxy = proxyConfig.Items[proxyConfig.SelectedIndex.Value];
                }
                else if (proxyConfig.IsEnabled.Value)
                {
                    proxyConfig.IsEnabled = TLBool.False; 
                    IsEnabled = false;  // SetReconnect will be invoked here

                    return;
                }
            }

            foreach (var item in proxyConfig.Items)
            {
                item.IsSelected = item == selectedProxy;
            }

            SetReconnect(reconnect, proxyConfig, selectedProxy);
        }

        public void Open(TLProxyBase proxy)
        {
            StateService.Proxy = proxy;
            StateService.SuppressProxySharing = SuppressSharing;
            NavigationService.UriFor<ProxyViewModel>().Navigate();
        }

        public void Done()
        {
            var proxyConfig = _proxyConfig ?? TLProxyConfigBase.Empty as TLProxyConfig76;
            if (proxyConfig == null) return;

            proxyConfig.IsEnabled = new TLBool(IsEnabled);
            proxyConfig.UseForCalls = new TLBool(UseForCalls);

            SetReconnect(true, proxyConfig, proxyConfig.GetProxy());

            NavigationService.GoBack();
        }

        public void Share()
        {
            if (SuppressSharing) return;
            if (_proxyConfig == null || _proxyConfig.IsEmpty) return;

            var view = GetView() as ProxyListView;
            if (view != null)
            {
                view.OpenShareMessagePicker(string.Empty,
                    args =>
                    {
                        if (args.Dialogs.Count == 0) return;

                        var prefix = Constants.DefaultMeUrlPrefix;
                        var config63 = CacheService.GetConfig() as TLConfig63;
                        if (config63 != null && !TLString.IsNullOrEmpty(config63.MeUrlPrefix))
                        {
                            prefix = config63.MeUrlPrefix.ToString();
                        }

                        var text = string.IsNullOrEmpty(args.Comment)
                            ? GetProxyListString(prefix)
                            : args.Comment + "\n\n" + GetProxyListString(prefix);

                        var messages = new List<TLMessage25>();
                        foreach (var dialog in args.Dialogs)
                        {
                            var with = dialog.With;
                            if (with != null)
                            {
                                with.ClearBitmap();
                            }

                            var date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);

                            var message = TLUtils.GetMessage(
                                new TLInt(StateService.CurrentUserId),
                                dialog.Peer,
                                MessageStatus.Sending,
                                TLBool.True,
                                TLBool.True,
                                date,
                                new TLString(text),
                                new TLMessageMediaEmpty(),
                                TLLong.Random(),
                                new TLInt(0)
                            );

                            messages.Add(message);
                        }

                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            foreach (var message in messages)
                            {
                                CacheService.SyncSendingMessage(
                                    message, null,
                                    result => DialogDetailsViewModel.SendInternal(message, MTProtoService));
                            }
                        });
                    });
            }

            return;
            //StateService.Url = GetProxyListString(prefix);
            //NavigationService.UriFor<ChooseDialogViewModel>().Navigate();
        }

        private string GetProxyListString(string prefix)
        {
            var url = string.Empty;
            
            foreach (var item in _proxyConfig.Items)
            {
                if (!item.IsEmpty)
                {
                    url += string.Format("{0}\n\n", item.GetUrl(prefix));
                }
            }

            return url.Trim('\n');
        }
    }
}
