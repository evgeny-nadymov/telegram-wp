// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace TelegramClient.Services
{
    /*public class WNSPushService : PushServiceBase
    {
        protected override string GetPushChannelUri()
        {
            return _pushChannel != null ? _pushChannel.Uri : null;
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
                _pushChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                
                //var hub = new NotificationHub("WNS" + Constants.ToastNotificationChannelName, 
                //    "Endpoint=sb://testdemopushhub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=z2Sj7sgwGpkvTyE/H5QyiffCpwCjV/PmJBY1h4WhXac=");

                //var result = await hub.RegisterNativeAsync(_pushChannel.Uri);

                //if (result.RegistrationId != null)
                //{
                //    Execute.ShowDebugMessage("Registration successful: " + result.RegistrationId);
                //}

                callback.SafeInvoke();
            });
        }


        public override void RegisterDeviceAsync()
        {
            
        }

        public override void UnregisterDeviceAsync(Action callback)
        {
            
        }
    }*/
}
