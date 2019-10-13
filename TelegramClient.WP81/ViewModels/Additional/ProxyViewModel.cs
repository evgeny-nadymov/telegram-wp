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
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Api.Transport;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Additional;

namespace TelegramClient.ViewModels.Additional
{
    public class ProxyViewModel : ViewModelBase
    {
        private bool _isSocks5Proxy = true;

        public bool IsSocks5Proxy
        {
            get { return _isSocks5Proxy; }
            set { SetField(ref _isSocks5Proxy, value, () => IsSocks5Proxy); }
        }

        public string Server { get; set; }

        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Secret { get; set; }

        private readonly ITransportService _transportService;

        public bool SuppressSharing { get; protected set; }

        private TLProxyBase _proxy;

        public ProxyViewModel(ITransportService transportService, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            SuppressSharing = StateService.SuppressProxySharing;
            StateService.SuppressProxySharing = false;

            _transportService = transportService;

            _proxy = StateService.Proxy;
            StateService.Proxy = null;

            if (_proxy != null)
            {
                Server = _proxy.Server.ToString();
                Port = _proxy.Port.Value >= 0 ? _proxy.Port.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;

                var socks5Proxy = _proxy as TLSocks5Proxy;
                if (socks5Proxy != null)
                {
                    Username = socks5Proxy.Username.ToString();
                    Password = socks5Proxy.Password.ToString();
                    _isSocks5Proxy = true;
                }

                var mtProtoProxy = _proxy as TLMTProtoProxy;
                if (mtProtoProxy != null)
                {
                    Secret = mtProtoProxy.Secret.ToString();
                    _isSocks5Proxy = false;
                }
            }
        }

        public bool IsDoneEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Port)) return false;
                if (!IsSocks5Proxy && string.IsNullOrEmpty(Secret)) return false;

                return true;
            }
        }

        public void Done()
        {
            var proxyConfig = _transportService.GetProxyConfig() as TLProxyConfig76 ?? TLProxyConfigBase.Empty as TLProxyConfig76;
            if (proxyConfig == null) return;
            if (!IsDoneEnabled) return;

            if (IsSocks5Proxy)
            {
                TLSocks5Proxy proxy = null;
                // new proxy
                if (_proxy == null)
                {
                    proxy = new TLSocks5Proxy { CustomFlags = new TLLong(0) };
                    proxy.Server = new TLString(Server);
                    var port = 0;
                    proxy.Port = int.TryParse(Port, out port) ? new TLInt(port) : new TLInt(0);
                    proxy.Username = new TLString(Username);
                    proxy.Password = new TLString(Password);

                    _proxy = proxy;
                    proxyConfig.Items.Add(_proxy);
                    proxyConfig.SelectedIndex = new TLInt(proxyConfig.Items.IndexOf(_proxy));
                    proxyConfig.IsEnabled = TLBool.True;

                    _transportService.SetProxyConfig(proxyConfig);
                    _transportService.Close();
                    MTProtoService.PingAsync(TLLong.Random(), result => { });

                    EventAggregator.Publish(new ProxyChangedEventArgs(proxyConfig));
                    NavigationService.GoBack();

                    return;
                }

                proxy = _proxy as TLSocks5Proxy;
                // change type
                if (proxy == null)
                {
                    proxy = new TLSocks5Proxy { CustomFlags = new TLLong(0) };
                    proxy.Server = new TLString(Server);
                    var port = 0;
                    proxy.Port = int.TryParse(Port, out port) ? new TLInt(port) : new TLInt(0);
                    proxy.Username = new TLString(Username);
                    proxy.Password = new TLString(Password);

                    _proxy = proxy;
                    proxyConfig.Items.Add(_proxy);
                    proxyConfig.SelectedIndex = new TLInt(proxyConfig.Items.IndexOf(_proxy));

                    _transportService.SetProxyConfig(proxyConfig);
                    _transportService.Close();
                    MTProtoService.PingAsync(TLLong.Random(), result => { });

                    EventAggregator.Publish(new ProxyChangedEventArgs(proxyConfig));
                    NavigationService.GoBack();

                    return;
                }
                // same type
                else
                {
                    proxy.Server = new TLString(Server);
                    var port = 0;
                    proxy.Port = int.TryParse(Port, out port) ? new TLInt(port) : new TLInt(0);
                    proxy.Username = new TLString(Username);
                    proxy.Password = new TLString(Password);

                    _proxy = proxy;
                    if (proxyConfig.SelectedIndex.Value == proxyConfig.Items.IndexOf(_proxy))
                    {
                        _transportService.SetProxyConfig(proxyConfig);
                        _transportService.Close();
                        MTProtoService.PingAsync(TLLong.Random(), result => { });
                    }

                    EventAggregator.Publish(new ProxyChangedEventArgs(proxyConfig));
                    NavigationService.GoBack();

                    return;
                }

            }
            else
            {
                TLMTProtoProxy proxy = null;
                // new proxy
                if (_proxy == null)
                {
                    proxy = new TLMTProtoProxy { CustomFlags = new TLLong(0) };
                    proxy.Server = new TLString(Server);
                    var port = 0;
                    proxy.Port = int.TryParse(Port, out port) ? new TLInt(port) : new TLInt(0);
                    proxy.Secret = new TLString(Secret);

                    _proxy = proxy;
                    proxyConfig.Items.Add(_proxy);
                    proxyConfig.SelectedIndex = new TLInt(proxyConfig.Items.IndexOf(_proxy));
                    proxyConfig.IsEnabled = TLBool.True;

                    _transportService.SetProxyConfig(proxyConfig);
                    _transportService.Close();
                    MTProtoService.PingAsync(TLLong.Random(), result => { });

                    EventAggregator.Publish(new ProxyChangedEventArgs(proxyConfig));
                    NavigationService.GoBack();

                    return;
                }

                proxy = _proxy as TLMTProtoProxy;
                // change type
                if (proxy == null)
                {
                    proxy = new TLMTProtoProxy { CustomFlags = new TLLong(0) };
                    proxy.Server = new TLString(Server);
                    var port = 0;
                    proxy.Port = int.TryParse(Port, out port) ? new TLInt(port) : new TLInt(0);
                    proxy.Secret = new TLString(Secret);

                    _proxy = proxy;
                    proxyConfig.Items.Add(_proxy);
                    proxyConfig.SelectedIndex = new TLInt(proxyConfig.Items.IndexOf(_proxy));

                    _transportService.SetProxyConfig(proxyConfig);
                    _transportService.Close();
                    MTProtoService.PingAsync(TLLong.Random(), result => { });

                    EventAggregator.Publish(new ProxyChangedEventArgs(proxyConfig));
                    NavigationService.GoBack();

                    return;
                }
                // same type
                else
                {
                    proxy.Server = new TLString(Server);
                    var port = 0;
                    proxy.Port = int.TryParse(Port, out port) ? new TLInt(port) : new TLInt(0);
                    proxy.Secret = new TLString(Secret);

                    _proxy = proxy;
                    if (proxyConfig.SelectedIndex.Value == proxyConfig.Items.IndexOf(_proxy))
                    {
                        _transportService.SetProxyConfig(proxyConfig);
                        _transportService.Close();
                        MTProtoService.PingAsync(TLLong.Random(), result => { });
                    }

                    EventAggregator.Publish(new ProxyChangedEventArgs(proxyConfig));
                    NavigationService.GoBack();

                    return;
                }
            }
        }

        public void Share()
        {
            if (SuppressSharing) return;
            if (string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(Port)) return;
            if (!IsSocks5Proxy && string.IsNullOrEmpty(Secret)) return;

            var view = GetView() as ProxyView;
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
                            ? prefix + GetProxyString()
                            : args.Comment + "\n\n" + prefix + GetProxyString();

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
            //StateService.Url = prefix + GetProxyString();
            //NavigationService.UriFor<ChooseDialogViewModel>().Navigate();
        }

        private string GetProxyString()
        {
            var proxyString = string.Empty;
            if (IsSocks5Proxy)
            {
                proxyString = string.Format("socks?server={0}&port={1}", Server, Port);
                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                {
                    proxyString += string.Format("&user={0}&pass={1}", Username, Password);
                }
            }
            else
            {
                proxyString = string.Format("proxy?server={0}&port={1}&secret={2}", Server, Port, Secret);
            }

            return proxyString;
        }
    }

    public class ProxyChangedEventArgs
    {
        public TLProxyConfigBase ProxyConfig { get; set; }

        public ProxyChangedEventArgs(TLProxyConfigBase proxyConfig)
        {
            ProxyConfig = proxyConfig;
        }
    }

    public class ProxyDataChangedEventArgs
    {
        public TLProxyDataBase ProxyData { get; set; }

        public ProxyDataChangedEventArgs(TLProxyDataBase proxyData)
        {
            ProxyData = proxyData;
        }
    }
}
