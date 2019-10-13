// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
#if WNS_PUSH_SERVICE
using Windows.Networking.PushNotifications;
#endif
using Windows.UI.Xaml;
using Telegram.Api;
using Telegram.Api.Helpers;
using Telegram.Api.Extensions;
using Telegram.Api.TL;

namespace TelegramClient.Tasks
{
    public sealed class PushNotificationsBackgroundTask : IBackgroundTask
    {
        private readonly Mutex _appOpenMutex = new Mutex(false, Constants.TelegramMessengerMutexName);

        private static bool _logEnabled = true;

        private static readonly int _id = new Random().Next(999);

        private static void Log(string message, Action callback = null)
        {
            if (!_logEnabled) return;

            Telegram.Logs.Log.Write(string.Format("::PushNotificationsBackgroundTask {0} {1}", _id, message), callback.SafeInvoke);
#if DEBUG
            //PushUtils.AddToast("push", message, string.Empty, string.Empty, null, null);
#endif
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
#if WNS_PUSH_SERVICE
            Telegram.Logs.Log.WriteSync = true;

            var stopwatch = Stopwatch.StartNew();

            var rawNotification = taskInstance.TriggerDetails as RawNotification;
            var payload = rawNotification != null ? rawNotification.Content : null;
            var rootObject = payload != null ? PushUtils.GetRootObject(payload) : null;
            var data = rootObject != null ? rootObject.data : null;
            var locKey = data != null ? data.loc_key : null;

            Log(string.Format("start locKey={0}", locKey));
            if (!_appOpenMutex.WaitOne(0))
            {
                Log("cancel");

                return;
            }
            _appOpenMutex.ReleaseMutex();

            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
            if (!isAuthorized)
            {
                Log(string.Format("cancel isAuthorized=false\npayload={0}", payload));
                return;
            }

            try
            {
                var initConnection = GetInitConnection();
                if (initConnection != null)
                {
                    SystemVersion = initConnection.SystemVersion != null
                        ? initConnection.SystemVersion.ToString()
                        : string.Empty;
                    Log(string.Format(initConnection.ToString()));
                }
                else
                {
                    Log(string.Format("empty init_connection"));
                }
                //string[] supportedLanguages = { "de", "en", "es", "it", "nl", "pt" };
                //var language = initConnection != null ? initConnection.LangCode.ToString() : "en";
                //language = supportedLanguages.Contains(language) ? language : "en"; 
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "";               

                PushUtils.UpdateToastAndTiles(taskInstance.TriggerDetails as RawNotification);
            }
            catch (Exception ex)
            {
                Log(string.Format("ex={0}\npayload={1}", ex, payload));
            }

            Log(string.Format("stop elapsed={0}", stopwatch.Elapsed));
#endif
        }

        public static string SystemVersion { get; set; }

        private readonly object _initConnectionSyncRoot = new object();

        private TLInitConnection GetInitConnection()
        {
            return TLUtils.OpenObjectFromMTProtoFile<TLInitConnection>(_initConnectionSyncRoot, Constants.InitConnectionFileName);
        }
    }
}
