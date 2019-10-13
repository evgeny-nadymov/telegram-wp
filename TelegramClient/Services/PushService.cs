// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
#if WP81 && WNS_PUSH_SERVICE
using Windows.Networking.PushNotifications;
#endif
using Microsoft.Phone.Notification;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL;

namespace TelegramClient.Services
{
#if WP81 && WNS_PUSH_SERVICE
    public class WNSPushService : PushServiceBase
    {
        protected override TLInt TokenType
        {
            get { return new TLInt(Constants.WNSTokenType); }
        }

        private PushNotificationChannel _pushChannel;

        public WNSPushService(IMTProtoService service) : base(service)
        {
            LoadOrCreateChannelAsync();
        }

        private void LoadOrCreateChannelAsync(Action callback = null)
        {
            Execute.BeginOnThreadPool(async () =>
            {
                try
                {
                    var pushChannel = HttpNotificationChannel.Find(Constants.ToastNotificationChannelName);

                    if (pushChannel != null)
                    {
                        pushChannel.UnbindToShellTile();
                        pushChannel.UnbindToShellToast();
                    }
                }
                catch (Exception ex)
                {
                    Telegram.Logs.Log.Write("WNSPushService start creating channel ex " + ex);

                    Execute.ShowDebugMessage("WNSPushService.LoadOrCreateChannelAsync ex " + ex);
                }

                Telegram.Logs.Log.Write("WNSPushService start creating channel");
                _pushChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                _pushChannel.PushNotificationReceived += OnPushNotificationReceived;
                Telegram.Logs.Log.Write("WNSPushService stop creating channel");

                callback.SafeInvoke();
            });
        }

        private void OnPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            //Execute.ShowDebugMessage("WNSPushService.OnPushNotificationReceived " + args);
        }

        public override string GetPushChannelUri()
        {
            return _pushChannel != null ? _pushChannel.Uri : null;
        }
    }
#endif

    public class PushService : PushServiceBase
    {
        protected override TLInt TokenType
        {
            get { return new TLInt(Constants.VoIPMPNSTokenType); }
        }

        public override string GetPushChannelUri()
        {
            return _pushChannel != null && _pushChannel.ChannelUri != null ? _pushChannel.ChannelUri.ToString() : null;
        }

        private HttpNotificationChannel _pushChannel;

        public PushService(IMTProtoService service) : base(service)
        {
            LoadOrCreateChannelAsync();
        }

        private void LoadOrCreateChannelAsync(Action callback = null)
        {
            Execute.BeginOnThreadPool(() =>
            {
                _pushChannel = HttpNotificationChannel.Find(Constants.ToastNotificationChannelName);
                Telegram.Logs.Log.Write(string.Format("PushService.Find channelUri=" + (_pushChannel != null ? _pushChannel.ChannelUri : null)));

                if (_pushChannel == null)
                {
                    try
                    {
                        _pushChannel = new HttpNotificationChannel(Constants.ToastNotificationChannelName);
                        Telegram.Logs.Log.Write(string.Format("PushService.Create channelUri=" + (_pushChannel != null ? _pushChannel.ChannelUri : null)));

                        _pushChannel.HttpNotificationReceived += OnHttpNotificationReceived;
                        _pushChannel.ChannelUriUpdated += OnChannelUriUpdated;
                        _pushChannel.ErrorOccurred += OnErrorOccurred;
                        _pushChannel.ShellToastNotificationReceived += OnShellToastNotificationReceived;
                        _pushChannel.ConnectionStatusChanged += OnConnectionStatusChanged;

                        _pushChannel.Open();
                        _pushChannel.BindToShellToast();
                        _pushChannel.BindToShellTile();
                        Telegram.Logs.Log.Write(string.Format("PushService.OpenBind channelUri=" + (_pushChannel != null ? _pushChannel.ChannelUri : null)));
                    }
                    catch (Exception e)
                    {
                        TLUtils.WriteException("PushService", e);
                    }

                    if (_pushChannel != null && _pushChannel.ChannelUri != null)
                    {
                        Debug.WriteLine(_pushChannel.ChannelUri.ToString());
                    }
                }
                else
                {
                    try
                    {
                        _pushChannel.HttpNotificationReceived += OnHttpNotificationReceived;
                        _pushChannel.ChannelUriUpdated += OnChannelUriUpdated;
                        _pushChannel.ErrorOccurred += OnErrorOccurred;
                        _pushChannel.ShellToastNotificationReceived += OnShellToastNotificationReceived;
                        _pushChannel.ConnectionStatusChanged += OnConnectionStatusChanged;

                        if (!_pushChannel.IsShellToastBound)
                        {
                            _pushChannel.BindToShellToast();
                        }
                        if (!_pushChannel.IsShellTileBound)
                        {
                            _pushChannel.BindToShellTile();
                        }
                    }
                    catch (Exception e)
                    {
                        TLUtils.WriteException("PushService", e);
                    }
                }

                callback.SafeInvoke();
            });
        }

        private void OnConnectionStatusChanged(object sender, NotificationChannelConnectionEventArgs e)
        {
            Telegram.Logs.Log.Write(string.Format("PushService.OnConnectionStatusChanged status={0}", e.ConnectionStatus));
        }

        private void OnHttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {

        }

        private void OnShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            
        }

        private void OnErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            var message = string.Format("A push notification {0} error occurred.  {1} ({2}) {3}", e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData);
            Execute.ShowDebugMessage(message);
            TLUtils.WriteLine(message);

            Telegram.Logs.Log.Write(string.Format("PushService.OnErrorOccurred message={0}", message));
            LoadOrCreateChannelAsync(() => RegisterDeviceAsync(() => { }));
        }

        private void OnChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            Debug.WriteLine(e.ChannelUri.ToString());

            Telegram.Logs.Log.Write(string.Format("PushService.OnChannelUriUpdated\nnewUri={0}\noldUri={1}", e.ChannelUri, (_pushChannel != null ? _pushChannel.ChannelUri : null)));
            RegisterDeviceAsync(() => { });
        }
    }
}
