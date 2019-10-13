// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using Windows.Networking;
using libtgnet;
using Telegram.Api.Extensions;
using Telegram.Api.TL;

namespace Telegram.Api.Transport
{
    public class NativeTcpTransport : TcpTransportBase
    {
        private readonly ConnectionSocketWrapper _wrapper;

        private bool _isConnected;

        private readonly List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>> _requests = new List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>>();

        public override ulong Ping
        {
            get
            {
                ulong ping;
                lock (SyncRoot)
                {
                    ping = _wrapper != null ? _wrapper.GetPing() : 0;
                }
                return ping;
            }
        }

        private ProxySettings _proxySettings;

        public NativeTcpTransport(string host, int port, string staticHost, int staticPort, MTProtoTransportType mtProtoType, short protocolDCId, byte[] protocolSecret, TLProxyConfigBase proxyConfig)
            : base(host, port, staticHost, staticPort, mtProtoType, proxyConfig)
        {
//            System.Diagnostics.Debug.WriteLine(
//                "  [NativeTcpTransport] .ctor begin host={0} port={1} static_host={2} static_port={3} type={4} protocol_dcid={5} protocol_secret={6} proxy={7}",
//                host, port, staticHost, staticPort, mtProtoType, protocolDCId, protocolSecret, proxyConfig);

            ActualHost = host;
            ActualPort = port;

            var ipv4 = true;
            ProxySettings proxySettings = null;
            if (proxyConfig != null && proxyConfig.IsEnabled.Value && !proxyConfig.IsEmpty)
            {
                var socks5Proxy = proxyConfig.GetProxy() as TLSocks5Proxy;
                if (socks5Proxy != null)
                {
                    try
                    {
                        ipv4 = new HostName(socks5Proxy.Server.ToString()).Type == HostNameType.Ipv4;
                    }
                    catch (Exception ex)
                    {

                    }
                    proxySettings = new ProxySettings
                    {
                        Type = ProxyType.Socks5,
                        Host = socks5Proxy.Server.ToString(),
                        Port = socks5Proxy.Port.Value,
                        Username = socks5Proxy.Username.ToString(),
                        Password = socks5Proxy.Password.ToString(),
                        IPv4 = ipv4
                    };

                    ActualHost = staticHost;
                    ActualPort = staticPort;
                    protocolSecret = null;
                    protocolDCId = protocolDCId;
                }
                var mtProtoProxy = proxyConfig.GetProxy() as TLMTProtoProxy;
                if (mtProtoProxy != null)
                {
                    try
                    {
                        ipv4 = new HostName(mtProtoProxy.Server.ToString()).Type == HostNameType.Ipv4;
                    }
                    catch (Exception ex)
                    {

                    }
                    proxySettings = new ProxySettings
                    {
                        Type = ProxyType.MTProto,
                        Host = mtProtoProxy.Server.ToString(),
                        Port = mtProtoProxy.Port.Value,
                        Secret = TLUtils.ParseSecret(mtProtoProxy.Secret),
                        IPv4 = ipv4
                    };

                    ActualHost = staticHost;
                    ActualPort = staticPort;
                }
            }

            try
            {
                ipv4 = new HostName(ActualHost).Type == HostNameType.Ipv4;
            }
            catch (Exception ex)
            {

            }

            var connectionSettings = new ConnectionSettings
            {
                Host = ActualHost,
                Port = ActualPort,
                IPv4 = ipv4,
                ProtocolDCId = protocolDCId,
                ProtocolSecret = protocolSecret
            };

            _proxySettings = proxySettings;

//            var proxyString = proxySettings == null
//                ? "null"
//                : string.Format("[host={0} port={1} ipv4={2} type={3} secret={4} username={5} password={6}]", 
//                proxySettings.Host, 
//                proxySettings.Port, 
//                proxySettings.IPv4, 
//                proxySettings.Type, 
//                proxySettings.Secret, 
//                proxySettings.Username, 
//                proxySettings.Password);
//            System.Diagnostics.Debug.WriteLine(
//                "  [NativeTcpTransport] .ctor end host={0} port={1} ipv4={2} protocol_dcid={3} protocol_secret={4} proxy={5}",
//                connectionSettings.Host, connectionSettings.Port, connectionSettings.IPv4, connectionSettings.ProtocolDCId, connectionSettings.ProtocolSecret, proxyString);

            _wrapper = new ConnectionSocketWrapper(connectionSettings, proxySettings);
            _wrapper.Closed += Wrapper_OnClosed;
            _wrapper.PacketReceived += Wrapper_OnPacketReceived;
        }

        private void Wrapper_OnPacketReceived(ConnectionSocketWrapper sender, byte[] data)
        {
            LastReceiveTime = DateTime.Now;

            StopCheckConfigTimer();
            RaiseConnectedAsync();
            lock (SyncRoot)
            {
                _isConnected = true;
            }

            Helpers.Execute.BeginOnThreadPool(() =>
            {
                RaisePacketReceived(new DataEventArgs(data));
            });
        }

        private void Wrapper_OnClosed(ConnectionSocketWrapper sender)
        {
            RaiseConnectionLost();
        }

        ~NativeTcpTransport()
        {

        }

        private void LOG(string message)
        {
            System.Diagnostics.Debug.WriteLine("NativeTcpTransport " + Host + " " + message);
        }

        public override void SendPacketAsync(string caption, byte[] data, Action<bool> callback, Action<TcpTransportResult> faultCallback = null)
        {
            var now = DateTime.Now;
            if (!FirstSendTime.HasValue)
            {
                FirstSendTime = now;
            }

            Helpers.Execute.BeginOnThreadPool(() =>
            {
                var isConnected = false;
                var isConnecting = false;
                var result = -1;

                lock (SyncRoot)
                {
                    isConnected = _isConnected;
                    if (!isConnected)
                    {
                        _requests.Add(new Tuple<byte[], Action<bool>, Action<TcpTransportResult>>(data, callback, faultCallback));
                        isConnecting = _requests.Count == 1;
                    }
                }

                if (isConnected)
                {
                    lock (SyncRoot)
                    {
                        try
                        {
                            result = _wrapper.SendPacket(data);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    if (result > 0 && result < data.Length)
                    {
                        Helpers.Execute.ShowDebugMessage(string.Format("NativeTransport Send req={0} sent={1}", data.Length, result));
                        callback.SafeInvoke(true);
                    }
                    else if (result > 0)
                    {
                        callback.SafeInvoke(true);
                    }
                    else
                    {
                        faultCallback.SafeInvoke(new TcpTransportResult(new Exception("NativeTCPTransport error=" + result)));
                    }

                    return;
                }

                if (isConnecting)
                {
                    RaiseConnectingAsync();
                    result = -1;

                    try
                    {
                        //LOG("Connect start");
                        result = _wrapper.Connect();
                        //LOG("Connect end");
                    }
                    catch (Exception ex)
                    {

                    }

                    isConnected = result > 0;
                    if (!isConnected)
                    {
                        List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>> requests;
                        lock (SyncRoot)
                        {
                            _isConnected = false;
                            requests = new List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>>(_requests);
                            _requests.Clear();
                        }

                        if (requests.Count > 0)
                        {
                            for (var i = 0; i < requests.Count; i++)
                            {
                                requests[i].Item3.SafeInvoke(new TcpTransportResult(new Exception("NativeTCPTransport connect error=" + result)));
                            }
                        }
                    }
                    else
                    {
                        Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            try
                            {
                                _wrapper.StartReceive();
                            }
                            catch (Exception ex)
                            {

                            }
                        });

                        List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>> requests;
                        lock (SyncRoot)
                        {
                            _isConnected = true;
                            requests = new List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>>(_requests);
                            _requests.Clear();
                        }

                        if (requests.Count > 0)
                        {
                            for (var i = 0; i < requests.Count; i++)
                            {
                                lock (SyncRoot)
                                {
                                    try
                                    {
                                        result = _wrapper.SendPacket(requests[i].Item1);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                if (result > 0 && result < requests[i].Item1.Length)
                                {
                                    Helpers.Execute.ShowDebugMessage(string.Format("NativeTransport Send req={0} sent={1}", requests[i].Item1.Length, result));
                                    requests[i].Item2.SafeInvoke(true);
                                }
                                else if (result > 0)
                                {
                                    requests[i].Item2.SafeInvoke(true);
                                }
                                else
                                {
                                    requests[i].Item3.SafeInvoke(new TcpTransportResult(new Exception("NativeTCPTransport error=" + result)));
                                }
                            }
                        }
                    }
                }
            });
        }

        public override void Close()
        {
            WRITE_LOG(string.Format("Close socket {2} {0}:{1}", Host, Port, Id));

            List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>> requests;

            lock (SyncRoot)
            {
                if (Closed)
                {
                    return;
                }
                try
                {
                    _wrapper.Closed -= Wrapper_OnClosed;
                    _wrapper.PacketReceived -= Wrapper_OnPacketReceived;
                    _wrapper.Close();
                }
                catch (Exception ex)
                {
                    Helpers.Execute.ShowDebugMessage("NativeTCPTransport.Close ex " + ex);
                }
                Closed = true;
                requests = new List<Tuple<byte[], Action<bool>, Action<TcpTransportResult>>>(_requests);
                _requests.Clear();
                StopCheckConfigTimer();
            }

            if (requests.Count > 0)
            {
                for (var i = 0; i < requests.Count; i++)
                {
                    requests[i].Item3.SafeInvoke(new TcpTransportResult(new Exception("NativeTCPTransport closed")));
                }
            }
        }

        public override string GetTransportInfo()
        {
            return string.Empty;
        }
    }
}