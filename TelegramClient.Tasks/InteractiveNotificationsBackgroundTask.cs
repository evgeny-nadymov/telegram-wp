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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Telegram.Api;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Connection;
using Telegram.Api.Services.DeviceInfo;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Channels;
using Telegram.Api.TL.Functions.Messages;
using Telegram.Api.Transport;
using Windows.Foundation.Metadata;


namespace TelegramClient.Tasks
{
    public sealed class InteractiveNotificationsBackgroundTask : IBackgroundTask
    {
        private readonly Mutex _appOpenMutex = new Mutex(false, Constants.TelegramMessengerMutexName);

        private bool _logEnabled = true;

        private void Log(string message, Action callback = null)
        {
            if (!_logEnabled) return;

            Telegram.Logs.Log.Write(string.Format("::InteractiveNotificationsBackgroundTask {0} {1}", _id, message), callback.SafeInvoke);
#if DEBUG
            //PushUtils.AddToast("scheduler", message, string.Empty, string.Empty, null, null);
#endif
        }

        private static readonly int _id = new Random().Next(999);

        private readonly object _initConnectionSyncRoot = new object();

        private TLInitConnection GetInitConnection()
        {
            return TLUtils.OpenObjectFromMTProtoFile<TLInitConnection>(_initConnectionSyncRoot, Constants.InitConnectionFileName) ??
                new TLInitConnection
                {
                    DeviceModel = new TLString("unknown"),
                    AppVersion = new TLString("background task"),
                    SystemVersion = new TLString("10.0.0.0")
                };
        }

        private readonly object _actionInfoSyncRoot = new object();

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var details = taskInstance.TriggerDetails as IToastNotificationActionTriggerDetail;
            if (details == null) return;
            if (string.IsNullOrEmpty(details.Argument)) return;
            object message;
            if (!details.UserInput.TryGetValue("message", out message)) return;

            var inputPeer = GetInputPeer(details.Argument);
            if (inputPeer == null) return;

            var msgId = GetMsgId(details.Argument);

            Telegram.Logs.Log.WriteSync = true;

            taskInstance.Canceled += OnTaskCanceled;
            var deferral = taskInstance.GetDeferral();

            var stopwatch = Stopwatch.StartNew();

            var task = taskInstance.Task;
            var name = task != null ? task.Name : null;
            Log("start " + name);
            if (!_appOpenMutex.WaitOne(0))
            {
                Log("cancel", deferral.Complete);

                return;
            }
            _appOpenMutex.ReleaseMutex();
            Log("release mutex");

            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
            if (!isAuthorized)
            {
                Log("cancel isAuthorized=false", deferral.Complete);

                return;
            }
            Log("isAuthorized=true");


            if (details.Argument.StartsWith("action=reply"))
            {
                SendReply(inputPeer, new TLString(message.ToString()), new TLInt(int.MaxValue));
            }
            else if (details.Argument.StartsWith("action=mute"))
            {
                UpdateNotifySettings(inputPeer, new TLInt(1 * 60 * 60));
            }
            else if (details.Argument.StartsWith("action=disable"))
            {
                UpdateNotifySettings(inputPeer, new TLInt(int.MaxValue));
            }

            Log("stop " + stopwatch.Elapsed, deferral.Complete);
        }

        private void UpdateNotifySettings(TLInputPeerBase inputPeer, TLInt muteUntil)
        {
            var deviceInfoService = new DeviceInfoService(GetInitConnection(), true, "InteractiveNotificationsBackgroundTask", _id);
            var eventAggregator = new TelegramEventAggregator();
            var cacheService = new InMemoryCacheService(eventAggregator);
            var updatesService = new UpdatesService(cacheService, eventAggregator);
            var transportService = new TransportService();
            var connectionService = new ConnectionService(deviceInfoService);
            var publicConfigService = new MockupPublicConfigService();

            var manualResetEvent = new ManualResetEvent(false);
            Log("before init");
            var mtProtoService = new MTProtoService(deviceInfoService, updatesService, cacheService, transportService, connectionService, publicConfigService);
            mtProtoService.Initialized += (o, e) =>
            {
                Log("init completed");

                mtProtoService.GetNotifySettingsAsync(new TLInputNotifyPeer { Peer = inputPeer },
                    result =>
                    {
                        Log("getNotifySettings completed", () =>
                        {
                            var peerNotifySettings = result as TLPeerNotifySettings;
                            if (peerNotifySettings != null)
                            {
                                if (muteUntil.Value < int.MaxValue)
                                {
                                    muteUntil = TLUtils.DateToUniversalTimeTLInt(mtProtoService.ClientTicksDelta, DateTime.Now.AddSeconds(muteUntil.Value));
                                }

                                var inputPeerNotifySettings = new TLInputPeerNotifySettings78
                                {
                                    Flags = new TLInt(0),
                                    MuteUntil = muteUntil,
                                    Sound = peerNotifySettings.Sound,
                                };

                                mtProtoService.UpdateNotifySettingsAsync(new TLInputNotifyPeer { Peer = inputPeer },
                                    inputPeerNotifySettings,
                                    result2 =>
                                    {
                                        Log("setNotifySettings completed", () =>
                                        {
                                            manualResetEvent.Set();
                                        });
                                    },
                                    error2 =>
                                    {
                                        Log(string.Format("setNotifySettings error={0}\n{1}", error2, error2.Exception),
                                            async () =>
                                            {
                                                await Task.Delay(TimeSpan.FromSeconds(1.0));
                                                manualResetEvent.Set();
                                            });
                                    });
                            }
                            else
                            {
                                manualResetEvent.Set();
                            }
                        });
                    },
                    error =>
                    {
                        Log(string.Format("getNotifySettings error={0}\n{1}", error, error.Exception),
                            async () =>
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1.0));
                                manualResetEvent.Set();
                            });
                    });
            };
            mtProtoService.InitializationFailed += (o, e) =>
            {
                Log("init failed");

                manualResetEvent.Set();
            };
            mtProtoService.Initialize();
#if DEBUG
            manualResetEvent.WaitOne();
#else
            manualResetEvent.WaitOne(15000);
#endif
        }

        private void SendReply(TLInputPeerBase inputPeer, TLString message, TLInt msgId)
        {
            if (msgId == null) return;
            if (TLString.IsNullOrEmpty(message)) return;

            var actionInfo = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLActionInfo>>(_actionInfoSyncRoot, Constants.ActionQueueFileName) ?? new TLVector<TLActionInfo>();

            var count = actionInfo.Count;
            Log("send count=" + count);

            var peerChannel = inputPeer as TLInputPeerChannel;
            var readHistory = peerChannel != null
                ? (TLObject)new TLReadChannelHistory { Channel = new TLInputChannel { ChannelId = peerChannel.ChatId, AccessHash = peerChannel.AccessHash }, MaxId = msgId }
                : new TLReadHistory { Peer = inputPeer, MaxId = msgId };

            var readHistoryActionInfo = new TLActionInfo();
            readHistoryActionInfo.SendBefore = new TLInt(0);
            readHistoryActionInfo.Action = readHistory;

            actionInfo.Add(readHistoryActionInfo);

            var sendMessage = new TLSendMessage();
            sendMessage.Flags = new TLInt(0);
            sendMessage.Peer = inputPeer;
            sendMessage.Message = message;
            sendMessage.RandomId = TLLong.Random();

            var sendMessageActionInfo = new TLActionInfo();
            sendMessageActionInfo.SendBefore = new TLInt(0);
            sendMessageActionInfo.Action = sendMessage;

            actionInfo.Add(sendMessageActionInfo);

            TLUtils.SaveObjectToMTProtoFile(new object(), Constants.ActionQueueFileName, actionInfo);

            if (actionInfo.Count > 0)
            {
                var deviceInfoService = new DeviceInfoService(GetInitConnection(), true, "InteractiveNotificationsBackgroundTask", _id);
                var eventAggregator = new TelegramEventAggregator();
                var cacheService = new InMemoryCacheService(eventAggregator);
                var updatesService = new UpdatesService(cacheService, eventAggregator);
                var transportService = new TransportService();
                var connectionService = new ConnectionService(deviceInfoService);
                var publicConfigService = new MockupPublicConfigService();

                var manualResetEvent = new ManualResetEvent(false);
                Log("before init");
                var requestsToRemove = new List<TLObject>();
                var mtProtoService = new MTProtoService(deviceInfoService, updatesService, cacheService, transportService, connectionService, publicConfigService);
                mtProtoService.Initialized += async (o, e) =>
                {
                    Log("init completed");

                    var actionsString = new StringBuilder();
                    foreach (var info in actionInfo)
                    {
                        actionsString.AppendLine(info.ToString());
                    }
                    Log(actionsString.ToString());

                    var sendMessageActions = new List<TLObject>();
                    const int maxActionCount = 10;
                    var currentCount = 0;
                    foreach (var ai in actionInfo)
                    {
                        if (TLUtils.IsValidAction(ai.Action) && currentCount < maxActionCount)
                        {
                            currentCount++;
                            sendMessageActions.Add(ai.Action);
                        }
                    }

                    if (sendMessageActions.Count > 0)
                    {
                        mtProtoService.SendActionsAsync(sendMessageActions,
                            (request, result) => // will be invoked for each sent action
                            {
                                requestsToRemove.Add(request);
                                var sendingMessages = mtProtoService.SendingMessages;
                                Log("send completed count=" + sendingMessages, () =>
                                {
                                    if (sendingMessages == 0)
                                    {
                                        _clearActionInfoFile = true;

                                        manualResetEvent.Set();
                                    }
                                });
                            },
                            error =>
                            {
                                Log(string.Format("send error={0}\n{1}", error, error.Exception),
                                    async () =>
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds(1.0));
                                        manualResetEvent.Set();
                                    });
                            });
                    }
                    else
                    {
                        manualResetEvent.Set();
                    }
                };
                mtProtoService.InitializationFailed += (o, e) =>
                {
                    Log("init failed");

                    manualResetEvent.Set();
                };
                mtProtoService.Initialize();
#if DEBUG
                manualResetEvent.WaitOne();
#else
                manualResetEvent.WaitOne(15000);
#endif
                if (_clearActionInfoFile)
                {
                    Log("clear");
                    lock (_actionInfoSyncRoot)
                    {
                        var actions = actionInfo;

                        foreach (var o in requestsToRemove)
                        {
                            MTProtoService.RemoveActionInfoCommon(actions, o);
                        }

                        TLUtils.SaveObjectToMTProtoFile(_actionInfoSyncRoot, Constants.ActionQueueFileName, actions);
                    }
                }
            }
        }

        private static TLInt GetMsgId(string argument)
        {
            var parameters = argument.Split(' ');
            foreach (var parameter in parameters)
            {
                if (parameter.StartsWith("msg_id"))
                {
                    int msgId;
                    var split = parameter.Split('=');
                    if (split.Length == 2 && Int32.TryParse(split[1], out msgId))
                    {
                        return new TLInt(msgId);
                    }
                }
            }

            return null;
        }

        private static TLInputPeerBase GetInputPeer(string argument)
        {
            var parameters = argument.Split(' ');
            if (parameters.Length >= 2)
            {
                if (parameters[1].StartsWith("chat_id"))
                {
                    int chatId;
                    var split1 = parameters[1].Split('=');
                    if (split1.Length == 2 && Int32.TryParse(split1[1], out chatId))
                    {
                        return new TLInputPeerChat { ChatId = new TLInt(chatId) };
                    }
                }
                else if (parameters[1].StartsWith("from_id"))
                {
                    if (parameters.Length >= 3)
                    {
                        if (parameters[2].StartsWith("access_hash"))
                        {
                            int fromId;
                            long accessHash;
                            var split1 = parameters[1].Split('=');
                            var split2 = parameters[2].Split('=');
                            if (split1.Length == 2 && Int32.TryParse(split1[1], out fromId)
                                && split2.Length == 2 && Int64.TryParse(split2[1], out accessHash))
                            {
                                return new TLInputPeerUser { UserId = new TLInt(fromId), AccessHash = new TLLong(accessHash) };
                            }
                        }
                    }
                }
                else if (parameters[1].StartsWith("channel_id"))
                {
                    if (parameters.Length >= 3)
                    {
                        if (parameters[2].StartsWith("access_hash"))
                        {
                            int chatId;
                            long accessHash;
                            var split1 = parameters[1].Split('=');
                            var split2 = parameters[2].Split('=');
                            if (split1.Length == 2 && Int32.TryParse(split1[1], out chatId)
                                && split2.Length == 2 && Int64.TryParse(split2[1], out accessHash))
                            {
                                return new TLInputPeerChannel { ChatId = new TLInt(chatId), AccessHash = new TLLong(accessHash) };
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Log(string.Format("cancel reason={0}", reason));
        }

        private bool _clearActionInfoFile;
    }

    [Guid(2487554906, 14579, 17142, 150, 170, 121, 85, 176, 240, 61, 162)]
    public interface IToastNotificationActionTriggerDetail
    {
        string Argument { get; }

        ValueSet UserInput { get; }
    }
}
