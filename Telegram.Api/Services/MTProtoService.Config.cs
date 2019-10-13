// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Telegram.Api.Extensions;
using Telegram.Api.Services.Connection;
using Telegram.Api.TL;
using Telegram.Api.Transport;

namespace Telegram.Api.Services
{
    public partial class MTProtoService
    {
        public void SaveConfig()
        {
            _cacheService.SetConfig(_config);
        }

        public TLConfig LoadConfig()
        {
            throw new NotImplementedException();
        }

#if DEBUG
        public void CheckPublicConfig()
        {
            OnCheckConfig(null, null);
        }
#endif

        public void PingProxyAsync(TLProxyBase proxy, Action<TLInt> callback, Action<TLRPCError> faultCallback = null)
        {
            if (_activeTransport == null)
            {
                faultCallback.SafeInvoke(new TLRPCError { Code = new TLInt(404), Message = new TLString("ActiveTransport is null") });
                return;
            }

            var transport = GetSpecialTransportWithProxy(_activeTransport.Host, _activeTransport.Port, _activeTransport.Type, proxy, new TransportSettings
            {
                DcId = _activeTransport.DCId,
                Secret = _activeTransport.Secret,
                AuthKey = _activeTransport.AuthKey,
                Salt = TLLong.Random(),
                SessionId = TLLong.Random(),
                MessageIdDict = new Dictionary<long, long>(),
                SequenceNumber = 0,
                ClientTicksDelta = _activeTransport.ClientTicksDelta,
                PacketReceivedHandler = OnPacketReceivedByTransport
            });

            if (transport.AuthKey == null)
            {
                InitTransportAsync(transport,
                    tuple =>
                    {
                        lock (transport.SyncRoot)
                        {
                            transport.AuthKey = tuple.Item1;
                            transport.Salt = tuple.Item2;
                            transport.SessionId = tuple.Item3;

                            transport.IsInitializing = false;
                        }
                        var authKeyId = TLUtils.GenerateLongAuthKeyId(tuple.Item1);

                        lock (_authKeysRoot)
                        {
                            if (!_authKeys.ContainsKey(authKeyId))
                            {
                                _authKeys.Add(authKeyId, new AuthKeyItem { AuthKey = tuple.Item1, AutkKeyId = authKeyId });
                            }
                        }

                        PingSpecialTransportAsync(transport, callback, faultCallback);
                    },
                    error =>
                    {
                        faultCallback.SafeInvoke(error);
                    });
            }
            else
            {
                PingSpecialTransportAsync(transport, callback, faultCallback);
            }
        }

        private void PingSpecialTransportAsync(ITransport transport, Action<TLInt> callback, Action<TLRPCError> faultCallback = null)
        {
            // connect and ping
            var stopwatch = Stopwatch.StartNew();
            PingByTransportAsync(transport, TLLong.Random(),
                pong =>
                {
                    // ping and measure
                    var ping = transport.Ping > 0 ? transport.Ping : stopwatch.Elapsed.TotalMilliseconds;
                    _transportService.CloseSpecialTransport(transport);
                    callback.SafeInvoke(new TLInt((int)ping));
                },
                error =>
                {
                    _transportService.CloseSpecialTransport(transport);
                    faultCallback.SafeInvoke(error);
                });
        }

        private readonly IPublicConfigService _publicConfigService;

        private static void LogPublicConfig(string str)
        {
            Logs.Log.Write(string.Format("  MTProtoService.CheckConfig {0}", str));
            Debug.WriteLine("  MTProtoService.CheckConfig {0}", str);
        }

        private void PingAndUpdateTransportInfoAsync(TLDCOption78 dcOption, ITransport transport, Action callback, Action<TLRPCError> faultCallback = null)
        {
            LogPublicConfig(string.Format("Ping id={0} dc_id={1} ip={2} port={3} secret={4} proxy=[{5}]", transport.Id, transport.DCId, transport.Host, transport.Port, transport.Secret != null, transport.ProxyConfig));

            PingByTransportAsync(transport, TLLong.Random(),
                pong =>
                {
                    LogPublicConfig(string.Format("Ping completed id={0}", transport.Id));

                    LogPublicConfig("Close transport id=" + transport.Id);
                    _transportService.CloseSpecialTransport(transport);

                    LogPublicConfig(string.Format("Update info dc_id={0} ip={1} port={2} secret={3}", transport.DCId, transport.Host, transport.Port, transport.Secret != null));
                    UpdateTransportInfoAsync(dcOption, new TLString(transport.Host), new TLInt(transport.Port),
                        result =>
                        {
                            LogPublicConfig("Update info completed");
                            callback.SafeInvoke();
                        });
                },
                error =>
                {
                    LogPublicConfig(string.Format("Ping error id={0} error={1}", transport.Id, error));
                    _transportService.CloseSpecialTransport(transport);
                    faultCallback.SafeInvoke(error);
                });
        }

        private void CheckAndUpdateTransportInfoInternalAsync(TLDCOption78 dcOption, ITransport transport, Action callback, Action<TLRPCError> faultCallback = null)
        {
            if (transport.AuthKey == null)
            {
                InitTransportAsync(transport,
                    tuple =>
                    {
                        LogPublicConfig(string.Format("Init transport completed id={0}", transport.Id));
                        lock (transport.SyncRoot)
                        {
                            transport.AuthKey = tuple.Item1;
                            transport.Salt = tuple.Item2;
                            transport.SessionId = tuple.Item3;

                            transport.IsInitializing = false;
                        }
                        var authKeyId = TLUtils.GenerateLongAuthKeyId(tuple.Item1);

                        lock (_authKeysRoot)
                        {
                            if (!_authKeys.ContainsKey(authKeyId))
                            {
                                _authKeys.Add(authKeyId, new AuthKeyItem { AuthKey = tuple.Item1, AutkKeyId = authKeyId });
                            }
                        }

                        PingAndUpdateTransportInfoAsync(dcOption, transport, callback, faultCallback);
                    },
                    error =>
                    {
                        LogPublicConfig(string.Format("Init transport error id={0} error={1}", transport.Id, error));
                    });
            }
            else
            {
                PingAndUpdateTransportInfoAsync(dcOption, transport, callback, faultCallback);
            }
        }

        public void CheckAndUpdateTransportInfoAsync(TLInt dcId, TLString host, TLInt port, Action callback, Action<TLRPCError> faultCallback = null)
        {
            LogPublicConfig(string.Format("CheckTransport dc_id={0} host={1} port={2}", dcId, host, port));

            if (dcId == null) return;
            if (TLString.IsNullOrEmpty(host)) return;
            if (port == null) return;

            var dcOption = TLUtils.GetDCOption(_config, dcId);

            var transport = GetSpecialTransport(host.ToString(), port.Value, Type, new TransportSettings
            {
                DcId = dcId.Value,
                Secret = TLUtils.ParseSecret(dcOption),
                AuthKey = dcOption != null ? dcOption.AuthKey : null,
                Salt = dcOption != null ? dcOption.Salt : TLLong.Random(),
                SessionId = TLLong.Random(),
                MessageIdDict = new Dictionary<long, long>(),
                SequenceNumber = 0,
                ClientTicksDelta = dcOption != null ? dcOption.ClientTicksDelta : 0,
                PacketReceivedHandler = OnPacketReceivedByTransport
            });

            CheckAndUpdateTransportInfoInternalAsync(dcOption as TLDCOption78, transport, callback, faultCallback);
        }

        private void OnCheckConfig(object sender, EventArgs e)
        {
            _publicConfigService.GetAsync(
                configSimple =>
                {
                    if (configSimple != null)
                    {
                        var now = TLUtils.DateToUniversalTimeTLInt(ClientTicksDelta, DateTime.Now);
                        if (configSimple.Expires.Value < now.Value || now.Value < configSimple.Date.Value)
                        {
                            LogPublicConfig(string.Format("Config expired date={0} expires={1} now={2}", configSimple.Date, configSimple.Expires, now));
                            return;
                        }

                        var dcId = configSimple.DCId;
                        var ipPort = configSimple.IpPortList.FirstOrDefault();
                        if (ipPort == null)
                        {
                            LogPublicConfig("ipPort is null");
                            return;
                        }

                        var dcOption = TLUtils.GetDCOption(_config, dcId);

                        var transport = GetSpecialTransport(ipPort.GetIpString(), ipPort.Port.Value, Type, new TransportSettings
                        {
                            DcId = dcId.Value,
                            Secret = null,  //ipPort.Secret
                            AuthKey = dcOption != null ? dcOption.AuthKey : null,
                            Salt = dcOption != null ? dcOption.Salt : TLLong.Random(),
                            SessionId = TLLong.Random(),
                            MessageIdDict = new Dictionary<long, long>(),
                            SequenceNumber = 0,
                            ClientTicksDelta = dcOption != null ? dcOption.ClientTicksDelta : 0,
                            PacketReceivedHandler = OnPacketReceivedByTransport
                        });

                        if (transport.AuthKey == null)
                        {
                            LogPublicConfig(string.Format("Init transport id={0} dc_id={1} ip={2} port={3} proxy=[{4}]", transport.Id, transport.DCId, transport.Host, transport.Port, transport.ProxyConfig));
                            InitTransportAsync(transport,
                                tuple =>
                                {
                                    LogPublicConfig(string.Format("Init transport completed id={0}", transport.Id));
                                    lock (transport.SyncRoot)
                                    {
                                        transport.AuthKey = tuple.Item1;
                                        transport.Salt = tuple.Item2;
                                        transport.SessionId = tuple.Item3;

                                        transport.IsInitializing = false;
                                    }
                                    var authKeyId = TLUtils.GenerateLongAuthKeyId(tuple.Item1);

                                    lock (_authKeysRoot)
                                    {
                                        if (!_authKeys.ContainsKey(authKeyId))
                                        {
                                            _authKeys.Add(authKeyId, new AuthKeyItem { AuthKey = tuple.Item1, AutkKeyId = authKeyId });
                                        }
                                    }

                                    CheckAndUpdateMainTransportAsync(transport);
                                },
                                error =>
                                {
                                    LogPublicConfig(string.Format("Init transport error id={0} error={1}", transport.Id, error));
                                });
                        }
                        else
                        {
                            CheckAndUpdateMainTransportAsync(transport);
                        }
                    }
                }
                ,
                error =>
                {
                    LogPublicConfig(string.Format("PublicConfigService.GetAsync error {0}", error));
                });
        }

        private void CheckAndUpdateMainTransportAsync(ITransport transport)
        {
            LogPublicConfig(string.Format("Get config from id={0} dc_id={1} ip={2} port={3} proxy=[{4}]", transport.Id, transport.DCId, transport.Host, transport.Port, transport.ProxyConfig));
            GetConfigByTransportAsync(transport,
                config =>
                {
                    LogPublicConfig(string.Format("Get config completed id={0}", transport.Id));

                    var dcId = new TLInt(_activeTransport.DCId);
                    var dcOption = TLUtils.GetDCOption(config, dcId) as TLDCOption78;
                    if (dcOption == null)
                    {
                        LogPublicConfig(string.Format("dcOption is null id={0}", transport.Id));
                        return;
                    }
                    if (string.Equals(_activeTransport.Host, dcOption.IpAddress.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        LogPublicConfig(string.Format("dcOption ip equals ip={0}", dcOption.IpAddress.ToString()));
                        return;
                    }
                    LogPublicConfig("Close transport id=" + transport.Id);
                    _transportService.CloseSpecialTransport(transport);

                    // replace main dc ip and port
                    transport = GetSpecialTransport(dcOption.IpAddress.ToString(), dcOption.Port.Value, Type, new TransportSettings
                    {
                        DcId = dcOption.Id.Value,
                        Secret = TLUtils.ParseSecret(dcOption),
                        AuthKey = _activeTransport.AuthKey,
                        Salt = _activeTransport.Salt,
                        SessionId = TLLong.Random(),
                        MessageIdDict = new Dictionary<long, long>(),
                        SequenceNumber = 0,
                        ClientTicksDelta = _activeTransport.ClientTicksDelta,
                        PacketReceivedHandler = OnPacketReceivedByTransport
                    });

                    CheckAndUpdateTransportInfoInternalAsync(dcOption, transport, null);
                    // reconnect
                },
                error2 =>
                {
                    LogPublicConfig(string.Format("Get config error id={0} error={1}", transport.Id, error2));

                    LogPublicConfig("Close transport id=" + transport.Id);
                    _transportService.CloseSpecialTransport(transport);
                });
        }
    }
}
