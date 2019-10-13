// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Telegram.Api;
using Telegram.Api.TL;
#if WINDOWS_PHONE
using System.Windows;
using System.IO.IsolatedStorage;
#else
using System.Threading.Tasks;
using TelegramClient.ViewModels.Additional;
using Windows.UI.Xaml;
using Windows.Storage;
#endif

namespace Telegram.Api.Helpers
{
#if WINDOWS_PHONE
    public static class SettingsHelper
    {
        private static readonly object SyncLock = new object();

        public static void CrossThreadAccess(Action<IsolatedStorageSettings> action)
        {
            lock (SyncLock)
            {
                try
                {
                    action(IsolatedStorageSettings.ApplicationSettings);
                }
                catch (Exception e)
                {
                    Execute.ShowDebugMessage("SettingsHelper.CrossThreadAccess" + e);
                }
            }
        }

        public static T GetValue<T>(string key)
        {
            T result;
            lock (SyncLock) // critical for wp7 devices
            {
                try
                {
                    if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(key, out result))
                    {
                        return result;
                    }

                    result = default(T);
                }
                catch (Exception e)
                {
                    Logs.Log.Write("SettingsHelper.GetValue " + e);
                    result = default(T);
                }
            }
            return result;
        }

        public static object GetValue(string key)
        {
            object result;
            lock (SyncLock) //critical for wp7 devices
            {
                try
                {
                    if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(key, out result))
                    {
                        return result;
                    }

                    result = null;
                }
                catch (Exception e)
                {
                    Logs.Log.Write("SettingsHelper.GetValue " + e);
                    result = null;
                }

            }
            return result;
        }

        private static readonly object _backgroundTaskSettingsSyncRoot = new object();

        public static void SetValue(string key, object value)
        {
            lock (SyncLock)
            {
                IsolatedStorageSettings.ApplicationSettings[key] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();

                //var backgroundSettings = new Dictionary<string, object>();
                //foreach (var settings in IsolatedStorageSettings.ApplicationSettings)
                //{
                //    if (settings.Value.GetType().Assembly.GetName().Name != "Telegram.Api")
                //    {
                //        continue;
                //    }

                //    backgroundSettings[settings.Key] = settings.Value;
                //}
                //SaveBackgroundSettingsAsync(_backgroundTaskSettingsSyncRoot, Constants.BackgroundTaskSettingsFileName, backgroundSettings);
            }
        }

        private static void SaveBackgroundSettingsAsync(object syncRoot, string fileName, Dictionary<string, object> settings)
        {
            Execute.BeginOnThreadPool(() =>
            {
                TLUtils.SaveObjectToFile(syncRoot, fileName, settings);
            });
        }

        public static void RemoveValue(string key)
        {
            lock (SyncLock)
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
            }
        }
    }
#elif WIN_RT
    public static class SettingsHelper
    {
        private static readonly object SyncLock = new object();

        public static void CrossThreadAccess(Action<Dictionary<string, object>> action)
        {
            lock (SyncLock)
            {
                try
                {
                    action(LocalSettings);
                }
                catch (Exception e)
                {
                    Execute.ShowDebugMessage("SettingsHelper.CrossThreadAccess" + e);
                }
            }
        }

        public static T GetValue<T>(string key)
        {
            object result;
            lock (SyncLock) // critical for wp7 devices
            {
                try
                {
                    if (LocalSettings.TryGetValue(key, out result))
                    {
                        return (T)result;
                    }

                    result = default(T);
                }
                catch (Exception e)
                {
                    Logs.Log.Write("SettingsHelper.GetValue " + e);
                    result = default(T);
                }
            }
            return (T)result;
        }

        public static object GetValue(string key)
        {
            object result;
            lock (SyncLock) //critical for wp7 devices
            {
                try
                {
                    if (LocalSettings.TryGetValue(key, out result))
                    {
                        return result;
                    }

                    result = null;
                }
                catch (Exception e)
                {
                    Logs.Log.Write("SettingsHelper.GetValue " + e);
                    result = null;
                }

            }
            return result;
        }

        public static void SetValue(string key, object value)
        {
            lock (SyncLock)
            {
                LocalSettings[key] = value;
            }
        }

        public static void RemoveValue(string key)
        {
            lock (SyncLock)
            {
                LocalSettings.Remove(key);
            }
        }

        private static Dictionary<string, object> _settings;

        public static Dictionary<string, object> LocalSettings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = GetValuesAsync().Result;
                }

                return _settings;
            }
        }

        public static async Task<Dictionary<string, object>> GetValuesAsync()
        {
            try
            {
                using (var fileStream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("__ApplicationSettings"))
                {
                    //var stringReader = new StreamReader(fileStream);
                    //var str = stringReader.ReadToEnd();

                    using (var streamReader = new StreamReader(fileStream))
                    {
                        var line = streamReader.ReadLine() ?? string.Empty;

                        var knownTypes = line.Split('\0')
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Select(Type.GetType)
                            .ToList();

                        ReplaceNonPclTypes(knownTypes);

                        fileStream.Position = line.Length + Environment.NewLine.Length;

                        var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), knownTypes);
                        return (Dictionary<string, object>)serializer.ReadObject(fileStream);
                    }
                }
            }
            catch(Exception ex)
            {
                Logs.Log.Write("SettingsHelper.GetValuesAsync exception " + ex);

                return new Dictionary<string, object>();
            }
        }

        private static void ReplaceNonPclTypes(List<Type> knownTypes)
        {
            for (var i = 0; i < knownTypes.Count; i++)
            {
                if (knownTypes[i].Name == typeof(TLConfig82).Name)
                {
                    knownTypes[i] = typeof(TLConfig82);
                }
                else if (knownTypes[i].Name == typeof(TLConfig78).Name)
                {
                    knownTypes[i] = typeof(TLConfig78);
                }
                else if (knownTypes[i].Name == typeof(TLConfig76).Name)
                {
                    knownTypes[i] = typeof(TLConfig76);
                }
                else if (knownTypes[i].Name == typeof(TLConfig72).Name)
                {
                    knownTypes[i] = typeof(TLConfig72);
                }
                else if (knownTypes[i].Name == typeof(TLConfig71).Name)
                {
                    knownTypes[i] = typeof(TLConfig71);
                }
                else if (knownTypes[i].Name == typeof(TLConfig67).Name)
                {
                    knownTypes[i] = typeof(TLConfig67);
                }
                else if (knownTypes[i].Name == typeof(TLConfig63).Name)
                {
                    knownTypes[i] = typeof(TLConfig63);
                }
                else if (knownTypes[i].Name == typeof(TLConfig61).Name)
                {
                    knownTypes[i] = typeof(TLConfig61);
                }
                else if (knownTypes[i].Name == typeof(TLConfig60).Name)
                {
                    knownTypes[i] = typeof(TLConfig60);
                }
                else if (knownTypes[i].Name == typeof(TLConfig55).Name)
                {
                    knownTypes[i] = typeof(TLConfig55);
                }
                else if (knownTypes[i].Name == typeof(TLConfig54).Name)
                {
                    knownTypes[i] = typeof(TLConfig54);
                }
                else if (knownTypes[i].Name == typeof(TLConfig52).Name)
                {
                    knownTypes[i] = typeof(TLConfig52);
                }
                else if (knownTypes[i].Name == typeof(TLConfig48).Name)
                {
                    knownTypes[i] = typeof(TLConfig48);
                }
                else if (knownTypes[i].Name == typeof(TLConfig44).Name)
                {
                    knownTypes[i] = typeof(TLConfig44);
                }
                else if (knownTypes[i].Name == typeof(TLConfig41).Name)
                {
                    knownTypes[i] = typeof(TLConfig41);
                }
                else if (knownTypes[i].Name == typeof(TLConfig28).Name)
                {
                    knownTypes[i] = typeof(TLConfig28);
                }
                else if (knownTypes[i].Name == typeof(BackgroundItem).Name)
                {
                    knownTypes[i] = typeof(BackgroundItem);
                }
            }
        }
    }
#endif
}

#if WIN_RT
namespace TelegramClient.ViewModels.Additional
{
    public class BackgroundItem { }
}
#endif