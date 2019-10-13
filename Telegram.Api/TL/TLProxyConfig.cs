// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum ProxyConfigCustomFlags
    {
        IsEnabled = 0x1,
        UseForCalls = 0x2
    }

    [Flags]
    public enum ProxyCustomFlags
    {
        Ping = 0x1,
        CheckTime = 0x2
    }

    public enum ProxyStatus
    {
        Available,
        Unavailable,
        Connecting
    }

    public abstract class TLProxyConfigBase : TLObject
    {
        public virtual TLBool IsEnabled { get; set; }

        public virtual TLBool UseForCalls { get; set; }

        public abstract bool IsEmpty { get; }

        public abstract TLProxyBase GetProxy();

        public abstract TLProxyConfigBase ToLastProxyConfig();

        public static TLProxyConfigBase Empty
        {
            get
            {
                var proxyConfig = new TLProxyConfig76
                {
                    CustomFlags = new TLLong(0),
                    IsEnabled = TLBool.False,
                    UseForCalls = TLBool.False,
                    SelectedIndex = new TLInt(-1),
                    Items = new TLVector<TLProxyBase>()
                };

                return proxyConfig;
            }
        }
    }

    public class TLProxyConfig : TLProxyConfigBase
    {
        public const uint Signature = TLConstructors.TLProxyConfig;

        public TLString Server { get; set; }

        public TLInt Port { get; set; }

        public TLString Username { get; set; }

        public TLString Password { get; set; }

        public override bool IsEmpty
        {
            get { return TLString.IsNullOrEmpty(Server) || Port.Value < 0; }
        }

        public override TLProxyBase GetProxy()
        {
            return IsEmpty
                ? null
                : new TLSocks5Proxy
                {
                    CustomFlags = new TLLong(0),
                    Server = Server,
                    Port = Port,
                    Username = Username,
                    Password = Password
                };
        }

        public override TLProxyConfigBase ToLastProxyConfig()
        {
            return new TLProxyConfig76
            {
                CustomFlags = new TLLong(0),
                IsEnabled = IsEnabled,
                SelectedIndex = new TLInt(0),
                Items = new TLVector<TLProxyBase>
                {
                    new TLSocks5Proxy
                    {
                        CustomFlags = new TLLong(0),
                        Server = Server,
                        Port = Port,
                        Username = Username,
                        Password = Password
                    }
                }
            };
        }

        public override TLObject FromStream(Stream input)
        {
            IsEnabled = GetObject<TLBool>(input);
            Server = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);
            Username = GetObject<TLString>(input);
            Password = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            IsEnabled.ToStream(output);
            Server.ToStream(output);
            Port.ToStream(output);
            Username.ToStream(output);
            Password.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLProxyConfig server={0} port={1} username={2} password={3}", Server, Port, Username, Password);
        }
    }

    public class TLProxyConfig76 : TLProxyConfigBase
    {
        public const uint Signature = TLConstructors.TLProxyConfig76;

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public override TLBool IsEnabled
        {
            get { return IsSet(CustomFlags, (int)ProxyConfigCustomFlags.IsEnabled) ? TLBool.True : TLBool.False; }
            set { SetUnset(ref _customFlags, value.Value, (int)ProxyConfigCustomFlags.IsEnabled); }
        }

        public override TLBool UseForCalls
        {
            get { return IsSet(CustomFlags, (int)ProxyConfigCustomFlags.UseForCalls) ? TLBool.True : TLBool.False; }
            set { SetUnset(ref _customFlags, value.Value, (int)ProxyConfigCustomFlags.UseForCalls); }
        }

        public TLInt SelectedIndex { get; set; }

        public TLVector<TLProxyBase> Items { get; set; }

        public override bool IsEmpty
        {
            get { return SelectedIndex.Value < 0 || SelectedIndex.Value > Items.Count - 1 || Items[SelectedIndex.Value].IsEmpty; }
        }

        public override TLProxyConfigBase ToLastProxyConfig()
        {
            return this;
        }

        public override TLProxyBase GetProxy()
        {
            return IsEmpty
                ? null
                : Items[SelectedIndex.Value];
        }

        public override TLObject FromStream(Stream input)
        {
            CustomFlags = GetObject<TLLong>(input);
            SelectedIndex = GetObject<TLInt>(input);
            Items = GetObject<TLVector<TLProxyBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            CustomFlags.ToStream(output);
            SelectedIndex.ToStream(output);
            Items.ToStream(output);
        }
    }

    public abstract class TLProxyBase : TLObject
    {
        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        protected TLInt _ping;

        public TLInt Ping
        {
            get { return _ping; }
            set
            {
                SetField(out _ping, value, ref _customFlags, (int)ProxyCustomFlags.Ping);
                NotifyOfPropertyChange(() => Ping);
            }
        }

        protected TLInt _checkTime;

        public TLInt CheckTime
        {
            get { return _checkTime; }
            set { SetField(out _checkTime, value, ref _customFlags, (int)ProxyCustomFlags.CheckTime); }
        }

        public TLString Server { get; set; }

        public TLInt Port { get; set; }

        protected ProxyStatus _proxyStatus;

        public ProxyStatus Status
        {
            get { return _proxyStatus; }
            set
            {
                SetField(ref _proxyStatus, value, () => Status);
                NotifyOfPropertyChange(() => Self);
            }
        }

        public virtual string About { get { return string.Format("{0}:{1}", Server, Port); } }

        public abstract bool IsEmpty { get; }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetField(ref _isSelected, value, () => IsSelected);
                NotifyOfPropertyChange(() => Self);
            }
        }

        public abstract bool ProxyEquals(TLProxyBase proxy);

        public static bool ProxyEquals(TLProxyBase proxyItem1, TLProxyBase proxyItem2)
        {
            if (proxyItem1 != null && proxyItem2 == null) return false;
            if (proxyItem1 == null && proxyItem2 == null) return false;
            if (proxyItem1 == null && proxyItem2 != null) return false;

            return proxyItem1.ProxyEquals(proxyItem2);
        }

        public TLProxyBase Self
        {
            get { return this; }
        }

        public abstract string GetUrl(string prefix);

        public abstract TLInputClientProxy ToInputProxy();
    }

    public class TLSocks5Proxy : TLProxyBase
    {
        public const uint Signature = TLConstructors.TLSocks5Proxy;

        public TLString Username { get; set; }

        public TLString Password { get; set; }

        public override bool IsEmpty
        {
            get { return TLString.IsNullOrEmpty(Server) || Port.Value < 0; }
        }

        public override bool ProxyEquals(TLProxyBase proxy)
        {
            var socks5Proxy = proxy as TLSocks5Proxy;
            if (socks5Proxy == null) return false;

            return
                TLString.Equals(Server, socks5Proxy.Server, StringComparison.OrdinalIgnoreCase)
                && Port.Value == socks5Proxy.Port.Value
                && TLString.Equals(Username, socks5Proxy.Username, StringComparison.Ordinal)
                && TLString.Equals(Password, socks5Proxy.Password, StringComparison.Ordinal);
        }

        public override TLObject FromStream(Stream input)
        {
            CustomFlags = GetObject<TLLong>(input);
            Server = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);
            Username = GetObject<TLString>(input);
            Password = GetObject<TLString>(input);

            Ping = GetObject<TLInt>(CustomFlags, (int)ProxyCustomFlags.Ping, null, input);
            CheckTime = GetObject<TLInt>(CustomFlags, (int)ProxyCustomFlags.CheckTime, null, input);
            Status = Ping != null ? ProxyStatus.Available : ProxyStatus.Unavailable;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            CustomFlags.ToStream(output);
            Server.ToStream(output);
            Port.ToStream(output);
            Username.ToStream(output);
            Password.ToStream(output);

            ToStream(output, Ping, CustomFlags, (int)ProxyCustomFlags.Ping);
            ToStream(output, CheckTime, CustomFlags, (int)ProxyCustomFlags.CheckTime);
        }

        public override string ToString()
        {
            return string.Format("TLSocks5Proxy server={0} port={1} username={2} password={3}", Server, Port, Username, Password);
        }

        public override string GetUrl(string prefix)
        {
            var proxyString = prefix + string.Format("socks?server={0}&port={1}", Server, Port);

            if (!TLString.IsNullOrEmpty(Username) && !TLString.IsNullOrEmpty(Password))
            {
                proxyString += string.Format("&user={0}&pass={1}", Username, Password);
            }

            return proxyString;
        }

        public override TLInputClientProxy ToInputProxy()
        {
            return null;
        }
    }

    public class TLMTProtoProxy : TLProxyBase
    {
        public const uint Signature = TLConstructors.TLMTProtoProxy;

        public TLString Secret { get; set; }

        public override bool IsEmpty
        {
            get { return TLString.IsNullOrEmpty(Server) || Port.Value < 0 || TLString.IsNullOrEmpty(Secret); }
        }

        public override bool ProxyEquals(TLProxyBase proxy)
        {
            var mtProtoProxy = proxy as TLMTProtoProxy;
            if (mtProtoProxy == null) return false;

            return
                TLString.Equals(Server, mtProtoProxy.Server, StringComparison.OrdinalIgnoreCase)
                && Port.Value == mtProtoProxy.Port.Value
                && TLString.Equals(Secret, mtProtoProxy.Secret, StringComparison.Ordinal);
        }

        public override TLObject FromStream(Stream input)
        {
            CustomFlags = GetObject<TLLong>(input);
            Server = GetObject<TLString>(input);
            Port = GetObject<TLInt>(input);
            Secret = GetObject<TLString>(input);

            Ping = GetObject<TLInt>(CustomFlags, (int)ProxyCustomFlags.Ping, null, input);
            CheckTime = GetObject<TLInt>(CustomFlags, (int)ProxyCustomFlags.CheckTime, null, input);
            Status = Ping != null ? ProxyStatus.Available : ProxyStatus.Unavailable;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            CustomFlags.ToStream(output);
            Server.ToStream(output);
            Port.ToStream(output);
            Secret.ToStream(output);

            ToStream(output, Ping, CustomFlags, (int)ProxyCustomFlags.Ping);
            ToStream(output, CheckTime, CustomFlags, (int)ProxyCustomFlags.CheckTime);
        }

        public override string ToString()
        {
            return string.Format("TLMTProtoProxy server={0} port={1} secret={2}", Server, Port, Secret);
        }

        public override string GetUrl(string prefix)
        {
            var proxyString = prefix + string.Format("proxy?server={0}&port={1}", Server, Port);

            if (!TLString.IsNullOrEmpty(Secret))
            {
                proxyString += string.Format("&secret={0}", Secret);
            }

            return proxyString;
        }

        public override TLInputClientProxy ToInputProxy()
        {
            return new TLInputClientProxy
            {
                Address = Server,
                Port = Port
            };
        }
    }
}
