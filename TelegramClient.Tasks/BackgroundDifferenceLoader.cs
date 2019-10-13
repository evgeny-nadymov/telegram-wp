// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Telegram.Api;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Connection;
using Telegram.Api.Services.DeviceInfo;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.Api.Transport;

namespace TelegramClient.Tasks
{
    public sealed class BackgroundDifferenceLoader : IBackgroundTask
    {
        private readonly Mutex _appOpenMutex = new Mutex(false, Constants.TelegramMessengerMutexName);

        private static bool _logEnabled = true;

        private static void Log(string message, Action callback = null)
        {
            if (!_logEnabled) return;

            Telegram.Logs.Log.Write(string.Format("::BackgroundDifferenceLoader {0} {1}", _id, message), callback.SafeInvoke);
#if DEBUG
            //PushUtils.AddToast("difference", string.Format("{0} {1}", _id, message), string.Empty, string.Empty, null, null);
#endif
        }

        private static readonly int _id = new Random().Next(999);

        private TLInitConnection GetInitConnection()
        {
            return TLUtils.OpenObjectFromMTProtoFile<TLInitConnection>(_initConnectionSyncRoot, Constants.InitConnectionFileName) ??
                new TLInitConnection
                {
                    DeviceModel = new TLString("unknown"),
                    AppVersion = new TLString("background task"),
                    SystemVersion = new TLString("8.10.0.0")
                };
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Telegram.Logs.Log.WriteSync = true;

            var stopwatch = Stopwatch.StartNew();

            if (!_appOpenMutex.WaitOne(0))
            {
                Log("cancel");

                return;
            }
            _appOpenMutex.ReleaseMutex();

            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
            if (!isAuthorized)
            {
                Log("cancel isAuthorized=false");
            }

            var deferral = taskInstance.GetDeferral();
            RunAsync(() =>
                {
                    Log(string.Format("stop elapsed={0}", stopwatch.Elapsed));
                    deferral.Complete();
                });
        }

        private readonly object _stateRoot = new object();

        private readonly object _initConnectionSyncRoot = new object();

        private void RunAsync(Action callback)
        {
            var deviceInfoService = new DeviceInfoService(GetInitConnection(), true, "BackgroundDifferenceLoader", _id);
            var eventAggregator = new TelegramEventAggregator();
            var cacheService = new InMemoryCacheService(eventAggregator);
            var updatesService = new UpdatesService(cacheService, eventAggregator);
            var transportService = new TransportService();
            var connectionService = new ConnectionService(deviceInfoService);
            var publicConfigService = new MockupPublicConfigService();

            var manualResetEvent = new ManualResetEvent(false);
            var mtProtoService = new MTProtoService(deviceInfoService, updatesService, cacheService, transportService, connectionService, publicConfigService);
            mtProtoService.Initialized += (o, e) =>
            {
                var lastTime = TLUtils.OpenObjectFromMTProtoFile<TLInt>(_differenceTimeSyncRoot, Constants.DifferenceTimeFileName);
                if (lastTime != null)
                {
                    var now = TLUtils.DateToUniversalTimeTLInt(mtProtoService.ClientTicksDelta, DateTime.Now);

                    if (lastTime.Value + Constants.DifferenceMinInterval > now.Value)
                    {
                        manualResetEvent.Set();
                        return;
                    }
                }

                var clientState = TLUtils.OpenObjectFromMTProtoFile<TLState>(_stateRoot, Constants.StateFileName);
                _results = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLDifference>>(_differenceFileSyncRoot, Constants.DifferenceFileName) ?? new TLVector<TLDifference>();
                var state = GetState(clientState, _results);

                if (state != null)
                {
                    GetDifferenceAsync(mtProtoService, state, () => manualResetEvent.Set());
                }
                else
                {
                    manualResetEvent.Set();
                }
            };
            mtProtoService.InitializationFailed += (o, e) =>
            {
                manualResetEvent.Set();
            };
            mtProtoService.Initialize();

#if DEBUG
            manualResetEvent.WaitOne(15000);
#else
            manualResetEvent.WaitOne(15000);
#endif

            callback.SafeInvoke();
        }

        private static TLState GetState(TLState clientState, TLVector<TLDifference> results)
        {
            var state = clientState;
            for (var i = 0; i < results.Count; i++)
            {
                var difference = results[i];
                if (difference != null)
                {
                    if (difference.State.Pts.Value < clientState.Pts.Value)
                    {
                        results.RemoveAt(i--);
                        continue;
                    }

                    if (difference.State.Pts.Value > state.Pts.Value)
                    {
                        state = difference.State;
                    }
                }
            }

            return state;
        }

        private void GetDifferenceAsync(IMTProtoService mtProtoService, TLState state, Action callback)
        {
            Log(string.Format("get_diff [{0}]", state));
            mtProtoService.GetDifferenceWithoutUpdatesAsync(state.Pts, state.Date, state.Qts,
                result =>
                {
                    var now = TLUtils.DateToUniversalTimeTLInt(mtProtoService.ClientTicksDelta, DateTime.Now);
                    TLUtils.SaveObjectToMTProtoFile(_differenceTimeSyncRoot, Constants.DifferenceTimeFileName, now);

                    var differenceEmpty = result as TLDifferenceEmpty;
                    if (differenceEmpty != null)
                    {
                        Log(string.Format("diff_empty date={0} seq={1}", differenceEmpty.Date, differenceEmpty.Seq));
            
                        //DeleteFile();

                        callback.SafeInvoke();
                    }

                    var difference = result as TLDifference;
                    if (difference != null)
                    {
                        SaveToFile(difference);

                        var differenceSlice = result as TLDifferenceSlice;
                        if (differenceSlice != null)
                        {
                            Log(string.Format("diff_slice [{0}]", differenceSlice.State));

                            GetDifferenceAsync(mtProtoService, differenceSlice.State, callback);
                        }
                        else
                        {
                            Log(string.Format("diff [{0}]", difference.State));

                            callback.SafeInvoke();
                        }
                    }
                },
                error =>
                {
                    Log(string.Format("diff_error={0}\n{1}", error, error.Exception));

                    callback.SafeInvoke();
                });
        }

        private readonly object _differenceFileSyncRoot = new object();

        private readonly object _differenceTimeSyncRoot = new object();

        private TLVector<TLDifference> _results;

        private void SaveToFile(TLDifference result)
        {
            if (result == null) return;

            AddResult(_results, result);

            TLUtils.SaveObjectToMTProtoFile(_differenceFileSyncRoot, Constants.DifferenceFileName, _results);
        }

        private void AddResult(TLVector<TLDifference> results, TLDifference result)
        {
            if (results.Count > 0)
            {
                var firstResult = results.FirstOrDefault();
                var usersCache = new Dictionary<int, TLUserBase>();
                var chatsCache = new Dictionary<int, TLChatBase>();

                foreach (var user in firstResult.Users)
                {
                    usersCache[user.Index] = user;
                }

                foreach (var chat in firstResult.Chats)
                {
                    chatsCache[chat.Index] = chat;
                }

                foreach (var user in result.Users)
                {
                    usersCache[user.Index] = user;
                }

                foreach (var chat in result.Chats)
                {
                    chatsCache[chat.Index] = chat;
                }

                result.Users = new TLVector<TLUserBase>();
                result.Chats = new TLVector<TLChatBase>();

                var users = new TLVector<TLUserBase>();
                foreach (var user in usersCache.Values)
                {
                    users.Add(user);
                }
                firstResult.Users = users;

                var chats = new TLVector<TLChatBase>();
                foreach (var chat in chatsCache.Values)
                {
                    chats.Add(chat);
                }
                firstResult.Chats = chats;
            }

            _results.Add(result);
        }
    }
}
