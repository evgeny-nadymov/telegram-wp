// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL;

namespace TelegramClient.Services
{
    public abstract class PushServiceBase : IPushService
    {
        protected abstract TLInt TokenType { get; }

        protected readonly IMTProtoService Service;

        protected PushServiceBase(IMTProtoService service)
        {
            Service = service;
        }

        public abstract string GetPushChannelUri();

        private string _lastRegisteredUri;

        private readonly object _lastRegisteredUriRoot = new object();

        public virtual void RegisterDeviceAsync(Action callback)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var channelUri = GetPushChannelUri();

                if (string.IsNullOrEmpty(channelUri))
                {
                    Telegram.Logs.Log.Write("PushServiceBase.RegisterDeviceAsync channelUri=null");
                    return;
                }

                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (!isAuthorized)
                {
                    Telegram.Logs.Log.Write("PushServiceBase.RegisterDeviceAsync isAuthorized=false");
                    return;
                }

                lock (_lastRegisteredUriRoot)
                {
                    if (string.Equals(_lastRegisteredUri, channelUri))
                    {
                        Telegram.Logs.Log.Write("PushServiceBase.RegisterDeviceAsync lastRegisteredUri=channelUri " + channelUri);
                        return;
                    }
                }

                Service.RegisterDeviceAsync(
                    TokenType,
                    new TLString(channelUri),
                    result =>
                    {
                        lock (_lastRegisteredUriRoot)
                        {
                            _lastRegisteredUri = channelUri;
                        }

                        //Execute.ShowDebugMessage("account.registerDevice result " + result);
                        TLUtils.WriteLine("account.registerDevice result " + result);

                        callback.SafeInvoke();
                    },
                    error =>
                    {
                        //Execute.ShowDebugMessage("account.registerDevice error " + error);
                        TLUtils.WriteLine("account.registerDevice error " + error);
                    });
            });
        }

        public virtual void UnregisterDeviceAsync(Action callback)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var channelUri = GetPushChannelUri();

                if (string.IsNullOrEmpty(channelUri))
                {
                    Telegram.Logs.Log.Write("PushServiceBase.UnregisterDeviceAsync channelUri=null");
                    callback.SafeInvoke();
                    return;
                }

                Service.UnregisterDeviceAsync(
                    TokenType,
                    new TLString(channelUri),
                    result =>
                    {
                        lock (_lastRegisteredUriRoot)
                        {
                            _lastRegisteredUri = null;
                        }
                        TLUtils.WriteLine("account.unregisterDevice result " + result);
                        callback.SafeInvoke();
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("account.unregisterDevice error " + error);
                        TLUtils.WriteLine("account.unregisterDevice error " + error);
                        callback.SafeInvoke();
                    });
            });
        }
    }
}