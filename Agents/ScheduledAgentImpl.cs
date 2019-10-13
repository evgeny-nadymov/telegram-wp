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
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Microsoft.Phone.Networking.Voip;
using Microsoft.Phone.Scheduler;
using PhoneVoIPApp.BackEnd;
using Telegram.Api;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Connection;
using Telegram.Api.Services.Location;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Phone;
using Telegram.Api.Transport;

namespace PhoneVoIPApp.Agents
{
    public class ScheduledAgentImpl : ScheduledTaskAgent
    {
        private readonly Mutex _appOpenMutex = new Mutex(false, Constants.TelegramMessengerMutexName);

        private static bool _logEnabled = true;

        private static readonly int _id = new Random().Next(999);

        private static void Log(string message, Action callback = null)
        {
            if (!_logEnabled) return;

            Telegram.Logs.Log.WriteSync = true;
            Telegram.Logs.Log.Write(string.Format("::ScheduledAgentImpl {0} {1}", _id, message), callback);
#if DEBUG
            //PushUtils.AddToast("push", message, string.Empty, string.Empty, null, null);
#endif
        }

        public ScheduledAgentImpl()
        {
        }

        //private static void SetText(XmlDocument document, string caption, string message)
        //{
        //    var toastTextElements = document.GetElementsByTagName("text");
        //    toastTextElements[0].InnerText = caption ?? string.Empty;
        //    toastTextElements[1].InnerText = message ?? string.Empty;
        //}

        private static void SetText(XmlDocument document, string caption, string message)
        {
            var toastTextElements = document.GetElementsByTagName("text");
            toastTextElements[0].InnerText = caption ?? string.Empty;
            toastTextElements[1].InnerText = message ?? string.Empty;
        }

        protected override void OnInvoke(ScheduledTask task)
        {
            Debug.WriteLine("[ScheduledAgentImpl {0}] ScheduledAgentImpl has been invoked with argument of type {1}.", GetHashCode(), task.GetType());

            // Indicate that an agent has started running
            AgentHost.OnAgentStarted();

            Log(string.Format("start with argument of type {0}", task.GetType()));
            if (!_appOpenMutex.WaitOne(0))
            {
                Log("cancel");
                Complete();
                return;
            }
            _appOpenMutex.ReleaseMutex();

            var incomingCallTask = task as VoipHttpIncomingCallTask;
            if (incomingCallTask != null)
            {
                isIncomingCallAgent = true;

                var messageBody = HttpUtility.HtmlDecode(Encoding.UTF8.GetString(incomingCallTask.MessageBody, 0, incomingCallTask.MessageBody.Length));
                Notification pushNotification = null;
                try
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(messageBody ?? string.Empty)))
                    {
                        var xs = new XmlSerializer(typeof(Notification));
                        pushNotification = (Notification)xs.Deserialize(ms);
                    }
                }
                catch (Exception ex)
                {
                    Log(string.Format("cannot deserialize message_body={0}", messageBody));
#if DEBUG
                    var toastNotifier = ToastNotificationManager.CreateToastNotifier();

                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                    SetText(toastXml, "Notification Exception", string.Empty);

                    try
                    {
                        var toast = new ToastNotification(toastXml);
                        //RemoveToastGroup(group);
                        toastNotifier.Show(toast);
                    }
                    catch (Exception ex2)
                    {
                        Telegram.Logs.Log.Write(ex.ToString());
                    }
#endif
                }

                if (pushNotification != null)
                {
                    var rootObject = PushUtils2.GetRootObject<RootObject>(pushNotification.Data);
                    if (rootObject != null
                        && rootObject.data != null)
                    {
                        if (rootObject.data.custom != null
                            && rootObject.data.custom.from_id != null
                            && rootObject.data.custom.call_id != null
                            && rootObject.data.custom.call_ah != null)
                        {
                            var contactImage = PushUtils2.GetImageSource(rootObject.data.custom.mtpeer) ?? string.Empty;
                            var contactName = rootObject.data.loc_args != null && rootObject.data.loc_args.Length > 0
                                ? rootObject.data.loc_args[0]
                                : string.Empty;
                            Debug.WriteLine("[{0}] Incoming call from caller {1}, id {2}", incomingCallAgentId,
                                contactName, rootObject.data.custom.from_id);

                            long contactId = 0;
                            long callId = 0;
                            long callAccessHash = 0;
                            if (long.TryParse(rootObject.data.custom.from_id, out contactId)
                                && long.TryParse(rootObject.data.custom.call_id, out callId)
                                && long.TryParse(rootObject.data.custom.call_ah, out callAccessHash))
                            {
                                if (string.Equals(rootObject.data.loc_key, "PHONE_CALL_REQUEST",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    if (BackEnd.Globals.Instance.CallController.CallStatus == CallStatus.InProgress)
                                    {
                                        OnIncomingCallDialogDismissed(callId, callAccessHash, true);
                                        return;
                                    }

                                    // Initiate incoming call processing
                                    // If you want to pass in additional information such as pushNotification.Number, you can
                                    var incomingCallProcessingStarted =
                                        BackEnd.Globals.Instance.CallController.OnIncomingCallReceived(contactName,
                                            contactId, contactImage, callId, callAccessHash,
                                            OnIncomingCallDialogDismissed);
                                    if (incomingCallProcessingStarted)
                                    {
                                        // will Complete() at OnIncomingCallDialogDismissed
                                        return;
                                    }

                                    //PushUtils2.AddToast(rootObject, "Caption", "Message", "", "", "tag", "group");
                                }
                                else if (string.Equals(rootObject.data.loc_key, "PHONE_CALL_DECLINE",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    var currentCallId = BackEnd.Globals.Instance.CallController.CallId;
                                    if (currentCallId == callId)
                                    {
                                        Log(string.Format("PHONE_CALL_DECLINE CallController.EndCall call_id={0}", callId));
                                        var result = BackEnd.Globals.Instance.CallController.EndCall();
                                        Log(string.Format("PHONE_CALL_DECLINE CallController.EndCall call_id={0} result={1}", callId, result));
                                    }
                                }
                            }
                        }
                        else if (string.Equals(rootObject.data.loc_key, "GEO_LIVE_PENDING"))
                        {
                            ProcessLiveLocations();
                        }
                        else
                        {
                            //PushUtils2.UpdateToastAndTiles(rootObject);
                        }
                    }
                }

                Complete();
                return;
            }
            else
            {
                VoipKeepAliveTask keepAliveTask = task as VoipKeepAliveTask;
                if (keepAliveTask != null)
                {
                    this.isIncomingCallAgent = false;

                    // Refresh tokens, get new certs from server, etc.
                    BackEnd.Globals.Instance.DoPeriodicKeepAlive();
                    this.Complete();
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Unknown scheduled task type {0}", task.GetType()));
                }
            }
        }

        // This is a request to complete this agent
        protected override void OnCancel()
        {
            Debug.WriteLine("[{0}] Cancel requested.", this.isIncomingCallAgent ? ScheduledAgentImpl.incomingCallAgentId : ScheduledAgentImpl.keepAliveAgentId);
            this.Complete();
        }

        private readonly object _initConnectionSyncRoot = new object();

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

        // This method is called when the incoming call processing is complete
        private void OnIncomingCallDialogDismissed(long callId, long callAccessHash, bool rejected)
        {
            Debug.WriteLine("[IncomingCallAgent] Incoming call processing is now complete.");

            if (rejected)
            {
                var deviceInfoService = new Telegram.Api.Services.DeviceInfo.DeviceInfoService(GetInitConnection(), true, "BackgroundDifferenceLoader", 1);
                var cacheService = new MockupCacheService();
                var updatesService = new MockupUpdatesService();
                var transportService = new TransportService();
                var connectionService = new ConnectionService(deviceInfoService);
                var publicConfigService = new MockupPublicConfigService();

                var manualResetEvent = new ManualResetEvent(false);
                var mtProtoService = new MTProtoService(deviceInfoService, updatesService, cacheService, transportService, connectionService, publicConfigService);
                mtProtoService.Initialized += (o, e) =>
                {
                    var peer = new TLInputPhoneCall
                    {
                        Id = new TLLong(callId),
                        AccessHash = new TLLong(callAccessHash)
                    };

                    var getStateAction = new TLDiscardCall
                    {
                        Peer = peer,
                        Duration = new TLInt(0),
                        Reason = new TLPhoneCallDiscardReasonBusy(),
                        ConnectionId = new TLLong(0)
                    };
                    var actions = new List<TLObject> { getStateAction };

                    mtProtoService.SendActionsAsync(actions,
                        (request, result) =>
                        {
                            manualResetEvent.Set();
                        },
                        error =>
                        {
                            manualResetEvent.Set();
                        });
                };
                mtProtoService.InitializationFailed += (o, e) =>
                {
                    manualResetEvent.Set();
                };
                mtProtoService.Initialize();

#if DEBUG
                manualResetEvent.WaitOne();
#else
                manualResetEvent.WaitOne(TimeSpan.FromSeconds(10.0));
#endif

                mtProtoService.Stop();
            }

            this.Complete();
        }

        private void ProcessLiveLocations()
        {
            var deviceInfoService = new Telegram.Api.Services.DeviceInfo.DeviceInfoService(GetInitConnection(), true, "BackgroundDifferenceLoader", 1);
            var cacheService = new MockupCacheService();
            var updatesService = new MockupUpdatesService();
            var transportService = new TransportService();
            var connectionService = new ConnectionService(deviceInfoService);
            var publicConfigService = new MockupPublicConfigService();

            var manualResetEvent = new ManualResetEvent(false);
            var eventAggregator = new TelegramEventAggregator();
            var mtProtoService = new MTProtoService(deviceInfoService, updatesService, cacheService, transportService, connectionService, publicConfigService);
            mtProtoService.Initialized += (o, e) =>
            {
                var liveLocationsService = new LiveLocationService(mtProtoService, eventAggregator);

                liveLocationsService.Load();

                liveLocationsService.UpdateAll();

                manualResetEvent.Set();
            };
            mtProtoService.InitializationFailed += (o, e) =>
            {
                manualResetEvent.Set();
            };
            mtProtoService.Initialize();

            var timeout = 
#if DEBUG
                Timeout.InfiniteTimeSpan;
#else
                TimeSpan.FromSeconds(30.0);
#endif

            var result = manualResetEvent.WaitOne(timeout);
        }

        // Complete this agent.
        private void Complete()
        {
            Debug.WriteLine("[{0}] Calling NotifyComplete", this.isIncomingCallAgent ? ScheduledAgentImpl.incomingCallAgentId : ScheduledAgentImpl.keepAliveAgentId);

            Log(string.Format("[{0}] Calling NotifyComplete", this.isIncomingCallAgent ? ScheduledAgentImpl.incomingCallAgentId : ScheduledAgentImpl.keepAliveAgentId));

            // This agent is done
            base.NotifyComplete();
        }

        // Strings used in tracing
        private const string keepAliveAgentId = "KeepAliveAgent";
        private const string incomingCallAgentId = "IncomingCallAgent";

        // Indicates if this agent instance is handling an incoming call or not
        private bool isIncomingCallAgent;
    }
}
