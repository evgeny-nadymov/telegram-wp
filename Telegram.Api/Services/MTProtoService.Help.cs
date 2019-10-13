// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Threading;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Help;

namespace Telegram.Api.Services
{
    public partial class MTProtoService
    {
        private TLConfig __config = new TLConfig
        {
            DCOptions = new TLVector<TLDCOption>
            {
                new TLDCOption 
                { 
                    Id = new TLInt(Constants.FirstServerDCId),
                    IpAddress = new TLString(Constants.FirstServerIpAddress), 
                    Port = new TLInt(Constants.FirstServerPort) 
                }
            }
        };

        private TLConfig _config
        {
            get { return __config; }
            set
            {
                __config = value;
            }
        }

        public void GetConfigAsync(Action<TLConfig> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetConfig();

            Logs.Log.Write("help.getConfig");

            SendInformativeMessage<TLConfig>("help.getConfig", obj,
                result =>
                {
                    callback(result);
                },
                faultCallback);
        }

        private Timer _getConfigTimer;

        private volatile bool _isGettingConfig;

        private void CheckGetConfig(object state)
        {
            //TLUtils.WriteLine(DateTime.Now.ToLongTimeString() + ": Check Config on Thread " + Thread.CurrentThread.ManagedThreadId, LogSeverity.Error);

            if (_deviceInfo != null && _deviceInfo.IsBackground)
            {
                return;
            }

            if (_isGettingConfig)
            {
                return;
            }

            if (_activeTransport == null)
            {
                return;
            }

            if (_activeTransport.AuthKey == null)
            {
                return;
            }

            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
            if (!isAuthorized)
            {
                return;
            }

            var currentTime = TLUtils.DateToUniversalTimeTLInt(ClientTicksDelta, DateTime.Now);


            var config23 = _config as TLConfig23;
            if (config23 != null && config23.Expires != null && (config23.Expires.Value > currentTime.Value))
            {
                return;
            }

            //if (_config != null && _config.Date != null && Math.Abs(_config.Date.Value - currentTime.Value) < Constants.GetConfigInterval)
            //{
            //    return;
            //}

            //Execute.ShowDebugMessage("MTProtoService.CheckGetConfig GetConfig");

            _isGettingConfig = true;
            GetConfigAsync(
                result =>
                {
                    //TLUtils.WriteLine(DateTime.Now.ToLongTimeString() + ": help.getConfig", LogSeverity.Error);
                    _config = TLConfig.Merge(_config, result);
                    SaveConfig();
                    _isGettingConfig = false;
                },
                error =>
                {
                    _isGettingConfig = false;
                    //Execute.ShowDebugMessage("help.getConfig error: " + error);
                });
        }

        public void GetPassportConfigAsync(TLInt hash, Action<TLPassportConfigBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetPassportConfig { Hash = hash };

            SendInformativeMessage("help.getPassportConfig", obj, callback, faultCallback);
        }

        public void GetTermsOfServiceAsync(TLString countryISO2, Action<TLTermsOfService> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetTermsOfService { CountryISO2 = countryISO2 };

            SendInformativeMessage("help.getTermsOfService", obj, callback, faultCallback);
        }

        public void GetNearestDCAsync(Action<TLNearestDC> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetNearestDC();

            SendInformativeMessage("help.getNearestDc", obj, callback, faultCallback);
        }

        public void GetInviteTextAsync(TLString langCode, Action<TLInviteText> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetInviteText();

            SendInformativeMessage("help.getInviteText", obj, callback, faultCallback);
        }

        public void GetSupportAsync(Action<TLSupport> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetSupport();

            SendInformativeMessage("help.getSupport", obj, callback, faultCallback);
        }

        public void GetAppChangelogAsync(TLString deviceModel, TLString systemVersion, TLString appVersion, TLString langCode, Action<TLAppChangelogBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetAppChangelog();

            SendInformativeMessage("help.getAppChangelog", obj, callback, faultCallback);
        }

        public void GetCdnConfigAsync(Action<TLCdnConfig> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetCdnConfig();

            SendInformativeMessage<TLCdnConfig>("help.getCdnConfig", obj,
                result =>
                {
                    foreach (var publicKey in result.PublicKeys)
                    {
                        var fingerprint = Utils.GetRSAFingerprint(publicKey.PublicKey.ToString());
                        publicKey.PublicKeyFingerprint = new TLLong(fingerprint);
                    }

                    callback.SafeInvoke(result);
                },
                faultCallback);
        }

        public void GetDeepLinkInfoAsync(TLString path, Action<TLDeepLinkInfoBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetDeepLinkInfo{ Path = path };

            SendInformativeMessage("help.getDeepLinkInfo", obj, callback, faultCallback);
        }
	}
}
