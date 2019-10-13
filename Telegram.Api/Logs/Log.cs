// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Execute = Telegram.Api.Helpers.Execute; 

namespace Telegram.Logs
{
    public class Log
    {
        public static bool IsPrivateBeta
        {
            get
            {
#if DEBUG
                return true;
#endif
#if WP81
                return Windows.ApplicationModel.Package.Current.Id.Name == "TelegramMessengerLLP.TelegramMessengerPreview";
#endif
                return true;
            }
        }

        public static bool WriteSync { get; set; }

        public static bool IsEnabled
        {
            get { return IsPrivateBeta; }
        }

        public static void Write(string str, Action callback = null)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (WriteSync)
            {
                WriteInternal(str, callback);
            }
            else
            {
                Execute.BeginOnThreadPool(() =>
                {
                    WriteInternal(str, callback);
                });
            }
        }

        public static void SyncWrite(string str, Action callback = null)
        {
            if (!IsEnabled)
            {
                return;
            }

            //if (WriteSync)
            {
                WriteInternal(str, callback);
            }
        }

        private static readonly object _fileSyncRoot = new object();

        private static void WriteInternal(string str, Action callback = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

            str = string.Format("{0} {1}{2}", timestamp, str, Environment.NewLine);
            using (var mutex = new Mutex(false, "Telegram.Log"))
            {
                mutex.WaitOne();
                FileUtils.Write(_fileSyncRoot, DirectoryName, FileName, str);
                mutex.ReleaseMutex();
            }
            callback.SafeInvoke();
        }

        private const string DirectoryName = "Logs";

        public static string FileName
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".txt"; }
        }

        public static void CopyTo(string fileName, Action<string> callback)
        {
            Execute.BeginOnThreadPool(() =>
            {
                using (var mutex = new Mutex(false, "Telegram.Log"))
                {
                    mutex.WaitOne();
                    FileUtils.CopyLog(_fileSyncRoot, DirectoryName, FileName, fileName, IsEnabled);
                    mutex.ReleaseMutex();
                }

                callback.SafeInvoke(fileName);
            });
        }

        public static void Clear(Action callback)
        {
            Execute.BeginOnThreadPool(() =>
            {
                using (var mutex = new Mutex(false, "Telegram.Log"))
                {
                    mutex.WaitOne();
                    FileUtils.Clear(_fileSyncRoot, DirectoryName);
                    mutex.ReleaseMutex();
                }

                callback.SafeInvoke();
            });
        }
    }
}