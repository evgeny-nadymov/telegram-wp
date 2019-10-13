// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Services;
using Telegram.Api.TL;

namespace Telegram.Api.Transport
{
    public interface ITransportService
    {
        void SetProxyConfig(TLProxyConfigBase proxyConfig);
        TLProxyConfigBase GetProxyConfig();

        ITransport GetTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated);
        ITransport GetFileTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated);
        ITransport GetFileTransport2(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated);
        ITransport GetSpecialTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated);
        ITransport GetSpecialTransport(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, TLProxyBase proxy, out bool isCreated);


        void Close();
        void CloseTransport(ITransport transport);
        void CloseSpecialTransport(ITransport transport);

        event EventHandler<TransportEventArgs> TransportConnecting;
        event EventHandler<TransportEventArgs> TransportConnected;

        event EventHandler<TransportEventArgs> ConnectionLost;
        event EventHandler<TransportEventArgs> FileConnectionLost;
        event EventHandler<TransportEventArgs> SpecialConnectionLost;

        event EventHandler CheckConfig;
    }

    public delegate ITransport GetTransportFunc(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, out bool isCreated);

    public delegate ITransport GetTransportWithProxyFunc(string host, int port, string staticHost, int staticPort, TransportType type, short protocolDCId, byte[] protocolSecret, TLProxyBase proxy, out bool isCreated);
}
