// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Windows;
using BugSense;
using Telegram.Api.TL;

namespace TelegramClient
{
    class BugSenseWrapper
    {
        private static readonly List<Tuple<Exception, string, NotificationOptions>> _delayedErrors = new List<Tuple<Exception, string, NotificationOptions>>();

        private static readonly object _bugSenseSyncRoot = new object();

        private static bool _isInitialized;

        private BugSenseWrapper()
        {
            
        }

        public static void LogError(Exception ex, string comment = null, NotificationOptions options = null)
        {
            lock (_bugSenseSyncRoot)
            {
                if (!_isInitialized)
                {
                    _delayedErrors.Add(new Tuple<Exception, string, NotificationOptions>(ex, comment, options));
                    return;
                }
            }

            try
            {
                BugSenseHandler.Instance.LogError(ex, comment, options);
            }
            catch (Exception e)
            {
                Telegram.Logs.Log.Write("BugSenseWrapper\n" + e);
            }
        }

        public static void Init()
        {
#if PRIVATE_BETA
            const string apiKey = "b6f57378";
#else
            const string apiKey = "e715f5e8";
#endif
            BugSenseHandler.Instance.Init(Application.Current, apiKey, new NotificationOptions { Type = enNotificationType.None });
            BugSenseHandler.Instance.UnhandledException += (sender, args) =>
            {
                TLUtils.WriteLine(args.ExceptionObject.ToString(), LogSeverity.Error);

                args.Handled = true;
            };

            lock (_bugSenseSyncRoot)
            {
                _isInitialized = true;
                foreach (var error in _delayedErrors)
                {
                    try
                    {
                        BugSenseHandler.Instance.LogError(error.Item1, error.Item2, error.Item3);
                    }
                    catch (Exception ex)
                    {
                        Telegram.Logs.Log.Write("BugSenseWrapper delayed\n" + ex);
                    }
                }
                _delayedErrors.Clear();
            }
        }
    }
}
