// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define NATIVE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Telegram.Api.Services;
using Telegram.Api.TL;

namespace Telegram.Api.Transport
{
    public class TransportService : ITransportService
    {
        public TransportService()
        {

        }

        private readonly object _proxyConfigSyncRoot = new object();

        private TLProxyConfigBase _proxyConfig;

        public TLProxyConfigBase GetProxyConfig()
        {
            if (_proxyConfig != null)
            {
                return _proxyConfig;
            }

            _proxyConfig = TLUtils.OpenObjectFromMTProtoFile<TLProxyConfigBase>(_proxyConfigSyncRoot, Constants.ProxyConfigFileName) ?? TLProxyConfigBase.Empty;

            _proxyConfig = _proxyConfig.ToLastProxyConfig();

            return _proxyConfig;
        }

        public void SetProxyConfig(TLProxyConfigBase proxyConfig)
        {
            _proxyConfig = proxyConfig;

            TLUtils.SaveObjectToMTProtoFile(_proxyConfigSyncRoot, Constants.ProxyConfigFileName, _proxyConfig);
        }

        private readonly Dictionary<string, ITransport> _cache = new Dictionary<string, ITransport>();

        private readonly Dictionary<string, ITransport> _fileCache = new Dictionary<string, ITransport>();

        private readonly Dictionary<string, ITransport> _specialCache = new Dictionary<string, ITransport>();

        public ITransport GetFileTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated)
        {
            var key = string.Format("{0} {1} {2} {3}", host, port, protocolDCId, type);
            if (_fileCache.ContainsKey(key))
            {
                isCreated = false;
                return _fileCache[key];
            }

#if WINDOWS_PHONE
            if (type == TransportType.Http)
            {
                var transport = new HttpTransport(host, MTProtoTransportType.File, GetProxyConfig());

                _fileCache.Add(key, transport);
                isCreated = true;
                return transport;
                //transport.SetAddress(host, port, () => callback(transport));
            }
            else
#endif
            {
                var transport =
#if WIN_RT
                    new TcpTransportWinRT(host, port, staticHost, staticPort, MTProtoTransportType.File, GetProxyConfig());
#elif NATIVE
 new NativeTcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.File, protocolDCId, protocolSecret, GetProxyConfig());
#else
                    new TcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.File, GetProxyConfig());
#endif
                transport.ConnectionLost += OnConnectionLost;
                TLUtils.WritePerformance(string.Format("  TCP: New file transport {0}:{1}", host, port));

                _fileCache.Add(key, transport);
                isCreated = true;

                Debug.WriteLine("  TCP: New transport {0}:{1}", host, port);
                return transport;
                //trasport.SetAddress(host, port, () => callback(trasport));
            }
        }

        private readonly Dictionary<string, ITransport> _fileCache2 = new Dictionary<string, ITransport>();

        public ITransport GetFileTransport2(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated)
        {
            var key = string.Format("{0} {1} {2} {3}", host, port, protocolDCId, type);
            if (_fileCache2.ContainsKey(key))
            {
                isCreated = false;
                return _fileCache2[key];
            }

#if WINDOWS_PHONE
            if (type == TransportType.Http)
            {
                var transport = new HttpTransport(host, MTProtoTransportType.File, GetProxyConfig());

                _fileCache2.Add(key, transport);
                isCreated = true;
                return transport;
                //transport.SetAddress(host, port, () => callback(transport));
            }
            else
#endif
            {
                var transport =
#if WIN_RT
                    new TcpTransportWinRT(host, port, staticHost, staticPort, MTProtoTransportType.File, GetProxyConfig());
#elif NATIVE
 new NativeTcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.File, protocolDCId, protocolSecret, GetProxyConfig());
#else
                    new TcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.File, GetProxyConfig());
#endif
                transport.ConnectionLost += OnConnectionLost;
                TLUtils.WritePerformance(string.Format("  TCP: New file transport 2 {0}:{1}", host, port));

                _fileCache2.Add(key, transport);
                isCreated = true;

                Debug.WriteLine("  TCP: New transport {0}:{1}", host, port);
                return transport;
                //trasport.SetAddress(host, port, () => callback(trasport));
            }
        }

        public ITransport GetTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated)
        {
            var key = string.Format("{0} {1} {2} {3}", host, port, protocolDCId, type);
            if (_cache.ContainsKey(key))
            {
                isCreated = false;

#if LOG_REGISTRATION
                TLUtils.WriteLog(string.Format("Old transport {2} {0}:{1}", host, port, _cache[key].Id));
#endif
                return _cache[key];
            }

#if WINDOWS_PHONE
            if (type == TransportType.Http)
            {
                var transport = new HttpTransport(host, MTProtoTransportType.Main, GetProxyConfig());

                _cache.Add(key, transport);
                isCreated = true;
                return transport;
                //transport.SetAddress(host, port, () => callback(transport));
            }
            else
#endif
            {
                var transport =
#if WIN_RT
                    new TcpTransportWinRT(host, port, staticHost, staticPort, MTProtoTransportType.Main, GetProxyConfig());
#elif NATIVE
 new NativeTcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.Main, protocolDCId, protocolSecret, GetProxyConfig());
#else
                    new TcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.Main, GetProxyConfig());
#endif

                transport.Connecting += OnConnecting;
                transport.Connected += OnConnected;
                transport.ConnectionLost += OnConnectionLost;
                transport.CheckConfig += OnCheckConfig;

#if LOG_REGISTRATION
                TLUtils.WriteLog(string.Format("New transport {2} {0}:{1}", host, port, transport.Id));
#endif
                TLUtils.WritePerformance(string.Format("  TCP: New transport {0}:{1}", host, port));

                _cache.Add(key, transport);
                isCreated = true;

                Debug.WriteLine("  TCP: New transport {0}:{1}", host, port);
                return transport;
                //trasport.SetAddress(host, port, () => callback(trasport));
            }
        }

        public ITransport GetSpecialTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated)
        {
            var random = TLLong.Random();   // Important! To ping multiple connections to one proxy, will be closed after first ping otherwise
            var proxyConfig = GetProxyConfig();
            var proxy = proxyConfig != null ? proxyConfig.GetProxy() : null;
            var key = string.Format("{0} {1} {2} {3} {4} {5}", host, port, protocolDCId, type, random, proxy != null ? string.Format("{0}:{1}", proxy.Server, proxy.Port) : string.Empty);
            if (_specialCache.ContainsKey(key))
            {
                isCreated = false;

#if LOG_REGISTRATION
                TLUtils.WriteLog(string.Format("Old transport {2} {0}:{1}", host, port, _specialCache[key].Id));
#endif
                return _specialCache[key];
            }

#if WINDOWS_PHONE
            if (type == TransportType.Http)
            {
                var transport = new HttpTransport(host, MTProtoTransportType.Special, GetProxyConfig());

                _specialCache.Add(key, transport);
                isCreated = true;
                return transport;
                //transport.SetAddress(host, port, () => callback(transport));
            }
            else
#endif
            {
                var transport =
#if WIN_RT
                    new TcpTransportWinRT(host, port, staticHost, staticPort, MTProtoTransportType.Special, GetProxyConfig());
#elif NATIVE
 new NativeTcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.Special, protocolDCId, protocolSecret, GetProxyConfig());
#else
                    new TcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.Special, GetProxyConfig());
#endif
                transport.Connecting += OnConnecting;
                transport.Connected += OnConnected;
                transport.ConnectionLost += OnConnectionLost;
                transport.CheckConfig += OnCheckConfig;

#if LOG_REGISTRATION
                TLUtils.WriteLog(string.Format("New transport {2} {0}:{1}", host, port, transport.Id));
#endif
                TLUtils.WritePerformance(string.Format("  TCP: New transport {0}:{1}", host, port));

                _specialCache.Add(key, transport);
                isCreated = true;

                Debug.WriteLine("  TCP: New transport {0}:{1}", host, port);
                return transport;
                //trasport.SetAddress(host, port, () => callback(trasport));
            }
        }

        public ITransport GetSpecialTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, TLProxyBase proxy, out bool isCreated)
        {
            var random = TLLong.Random();   // Important! To ping multiple connections to one proxy, will be closed after first ping otherwise
            var key = string.Format("{0} {1} {2} {3} {4} {5}", host, port, protocolDCId, type, random, proxy != null ? string.Format("{0}:{1}", proxy.Server, proxy.Port) : string.Empty);
            if (_specialCache.ContainsKey(key))
            {
                isCreated = false;

#if LOG_REGISTRATION
                TLUtils.WriteLog(string.Format("Old transport {2} {0}:{1}", host, port, _specialCache[key].Id));
#endif
                return _specialCache[key];
            }

#if WINDOWS_PHONE
            if (type == TransportType.Http)
            {
                var transport = new HttpTransport(host, MTProtoTransportType.Special, GetProxyConfig());

                _specialCache.Add(key, transport);
                isCreated = true;
                return transport;
                //transport.SetAddress(host, port, () => callback(transport));
            }
            else
#endif
            {
                var proxyConfig = new TLProxyConfig76
                {
                    CustomFlags = new TLLong(0),
                    IsEnabled = TLBool.True,
                    SelectedIndex = new TLInt(0),
                    UseForCalls = TLBool.False,
                    Items = new TLVector<TLProxyBase> { proxy }
                };

                var transport =
#if WIN_RT
                    new TcpTransportWinRT(host, port, staticHost, staticPort, MTProtoTransportType.Special, proxyConfig);
#elif NATIVE
 new NativeTcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.Special, protocolDCId, protocolSecret, proxyConfig);
#else
                    new TcpTransport(host, port, staticHost, staticPort, MTProtoTransportType.Special, proxyConfig);
#endif
                transport.Connecting += OnConnecting;
                transport.Connected += OnConnected;
                transport.ConnectionLost += OnConnectionLost;
                transport.CheckConfig += OnCheckConfig;

#if LOG_REGISTRATION
                TLUtils.WriteLog(string.Format("New transport {2} {0}:{1}", host, port, transport.Id));
#endif
                TLUtils.WritePerformance(string.Format("  TCP: New transport {0}:{1}", host, port));

                _specialCache.Add(key, transport);
                isCreated = true;

                Debug.WriteLine("  TCP: New transport {0}:{1}", host, port);
                return transport;
                //trasport.SetAddress(host, port, () => callback(trasport));
            }
        }

        public event EventHandler CheckConfig;

        protected virtual void RaiseCheckConfig()
        {
            var handler = CheckConfig;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnCheckConfig(object sender, EventArgs e)
        {
            var transport = sender as ITransport;
            if (transport != null && transport.MTProtoType == MTProtoTransportType.Main)
            {
                Logs.Log.Write(string.Format("TransportService CheckConfig Transport=[dc_id={0} ip={1} port={2} proxy=[{3}]]", transport.DCId, transport.Host, transport.Port, transport.ProxyConfig));

                RaiseCheckConfig();
            }
        }

        public void Close()
        {
            var transports = new List<ITransport>(_cache.Values);
            foreach (var transport in transports)
            {
                transport.Connecting -= OnConnecting;
                transport.Connected -= OnConnected;
                transport.ConnectionLost -= OnConnectionLost;
                transport.Close();
            }
            _cache.Clear();

            var fileTransports = new List<ITransport>(_fileCache.Values);
            foreach (var transport in fileTransports)
            {
                transport.Connecting -= OnConnecting;
                transport.Connected -= OnConnected;
                transport.ConnectionLost -= OnConnectionLost;
                transport.Close();
            }
            _fileCache.Clear();

            var fileTransports2 = new List<ITransport>(_fileCache2.Values);
            foreach (var transport in fileTransports2)
            {
                transport.Connecting -= OnConnecting;
                transport.Connected -= OnConnected;
                transport.ConnectionLost -= OnConnectionLost;
                transport.Close();
            }
            _fileCache2.Clear();

            /*var specialTransports = new List<ITransport>(_specialCache.Values);
            foreach (var transport in specialTransports)
            {
                transport.Connecting -= OnConnecting;
                transport.Connected -= OnConnected;
                transport.ConnectionLost -= OnConnectionLost;
                transport.Close();
            }
            _specialCache.Clear();*/
        }

        public void CloseTransport(ITransport transport)
        {
            foreach (var value in _cache.Values.Where(x => string.Equals(x.Host, transport.Host, StringComparison.OrdinalIgnoreCase)))
            {
                value.Close();
                transport.Connecting -= OnConnecting;
                transport.Connected -= OnConnected;
                transport.ConnectionLost -= OnConnectionLost;
            }
            _cache.Remove(string.Format("{0} {1} {2}", transport.Host, transport.Port, transport.Type));

            foreach (var value in _fileCache.Values.Where(x => string.Equals(x.Host, transport.Host, StringComparison.OrdinalIgnoreCase)))
            {
                value.Close();
                transport.Connecting -= OnConnecting;
                transport.Connected -= OnConnected;
                transport.ConnectionLost -= OnConnectionLost;
            }
            _fileCache.Remove(string.Format("{0} {1} {2}", transport.Host, transport.Port, transport.Type));

            foreach (var value in _fileCache2.Values.Where(x => string.Equals(x.Host, transport.Host, StringComparison.OrdinalIgnoreCase)))
            {
                value.Close();
                transport.Connecting -= OnConnecting;
                transport.Connected -= OnConnected;
                transport.ConnectionLost -= OnConnectionLost;
            }
            _fileCache2.Remove(string.Format("{0} {1} {2}", transport.Host, transport.Port, transport.Type));
        }

        public void CloseSpecialTransport(ITransport transport)
        {
            transport.Connecting -= OnConnecting;
            transport.Connected -= OnConnected;
            transport.ConnectionLost -= OnConnectionLost;
            transport.Close();

            _specialCache.Remove(GetSpecialTransportKey(transport));
        }

        private static string GetSpecialTransportKey(ITransport transport)
        {
            var proxy = transport.ProxyConfig != null ? transport.ProxyConfig.GetProxy() : null;
            return string.Format("{0} {1} {2} {3}",
                transport.Host,
                transport.Port,
                transport.Type,
                proxy != null ? string.Format("{0}:{1}", proxy.Server, proxy.Port) : String.Empty);
        }

        public event EventHandler<TransportEventArgs> TransportConnecting;

        protected virtual void RaiseTransportConnecting(ITransport transport)
        {
            var handler = TransportConnecting;
            if (handler != null) handler(this, new TransportEventArgs { Transport = transport });
        }

        public void OnConnecting(object sender, EventArgs args)
        {
            RaiseTransportConnecting(sender as ITransport);
        }

        public event EventHandler<TransportEventArgs> TransportConnected;

        protected virtual void RaiseTransportConnected(ITransport transport)
        {
            var handler = TransportConnected;
            if (handler != null) handler(this, new TransportEventArgs { Transport = transport });
        }

        public void OnConnected(object sender, EventArgs args)
        {
            RaiseTransportConnected(sender as ITransport);
        }

        public event EventHandler<TransportEventArgs> ConnectionLost;

        protected virtual void RaiseConnectionLost(ITransport transport)
        {
            var handler = ConnectionLost;
            if (handler != null) handler(this, new TransportEventArgs { Transport = transport });
        }

        public event EventHandler<TransportEventArgs> FileConnectionLost;

        protected virtual void RaiseFileConnectionLost(ITransport transport)
        {
            var handler = FileConnectionLost;
            if (handler != null) handler(this, new TransportEventArgs { Transport = transport });
        }

        public event EventHandler<TransportEventArgs> SpecialConnectionLost;

        protected virtual void RaiseSpecialConnectionLost(ITransport transport)
        {
            var handler = SpecialConnectionLost;
            if (handler != null) handler(this, new TransportEventArgs { Transport = transport });
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            var transport = (ITransport)sender;
            if (transport.MTProtoType == MTProtoTransportType.File)
            {
                RaiseFileConnectionLost(sender as ITransport);
            }
            else if (transport.MTProtoType == MTProtoTransportType.Special)
            {
                RaiseSpecialConnectionLost(sender as ITransport);
            }
            else
            {
                RaiseConnectionLost(sender as ITransport);
            }
        }
    }

    public class TransportEventArgs : EventArgs
    {
        public ITransport Transport { get; set; }
    }
}
