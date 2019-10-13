using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using Telegram.Api.Extensions;
using Execute = Telegram.Api.Helpers.Execute; 

namespace TelegramClient.Logs
{
    public class Log
    {
        public static bool IsEnabled
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        private static readonly object _fileSyncRoot = new object();

        public static void Write(string str)
        {
            if (!IsEnabled)
            {
                return;
            }

            Execute.BeginOnThreadPool(() =>
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                lock (_fileSyncRoot)
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.DirectoryExists(DirectoryName))
                        {
                            store.CreateDirectory(DirectoryName);
                        }

                        using (var file = store.OpenFile(Path.Combine(DirectoryName, FileName), FileMode.Append))
                        {
                            var bytes = Encoding.UTF8.GetBytes(string.Format("{0} {1}{2}", timestamp, str, Environment.NewLine));
                            file.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
            });
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
                lock (_fileSyncRoot)
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(Path.Combine(DirectoryName, FileName)))
                        {
                            store.CopyFile(Path.Combine(DirectoryName, FileName), fileName);
                        }
                    }
                }

                callback.SafeInvoke(fileName);
            });
        }
    }
}