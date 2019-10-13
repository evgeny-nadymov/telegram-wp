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
using System.Threading;
using Microsoft.Phone.Networking.Voip;
using PhoneVoIPApp.BackEnd;
using Telegram.Api;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Connection;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Updates;
using Telegram.Api.Transport;

namespace PhoneVoIPApp.Agents
{
    /// <summary>
    /// An agent that is launched when the first call becomes active and is canceled when the last call ends.
    /// </summary>
    public class CallInProgressAgentImpl : VoipCallInProgressAgent
    {
        public static bool Suppress { get; set; }

        private bool _logEnabled = true;

        private void Log(string message, Action callback = null)
        {
            if (!_logEnabled) return;

            Telegram.Logs.Log.Write(string.Format("[CallInProgressAgentImpl] {0} {1}", GetHashCode(), message), callback.SafeInvoke);
#if DEBUG
            //PushUtils.AddToast("push", message, string.Empty, string.Empty, null, null);
#endif
        }

        private readonly object _initConnectionSyncRoot = new object();

        private TLInitConnection GetInitConnection()
        {
            return TLUtils.OpenObjectFromMTProtoFile<TLInitConnection>(_initConnectionSyncRoot, Constants.InitConnectionFileName) ??
                new TLInitConnection78
                {
                    Flags = new TLInt(0),
                    DeviceModel = new TLString("unknown"),
                    AppVersion = new TLString("background task"),
                    SystemVersion = new TLString("8.10.0.0")
                };
        }

        private static IMTProtoService _mtProtoService;

        private static IMTProtoService MTProtoService
        {
            get
            {
                return _mtProtoService;

            }
            set
            {
                _mtProtoService = value;

            }
        }

        private static ITransportService _transportService;

        private void InitializeServiceAsync(System.Action callback)
        {
            Debug.WriteLine("[CallInProgressAgentImpl {0}] _mtProtoService == null {1}", GetHashCode(), _mtProtoService == null);

            if (MTProtoService == null)
            {
                var deviceInfoService = new Telegram.Api.Services.DeviceInfo.DeviceInfoService(GetInitConnection(), true, "BackgroundDifferenceLoader", 1);
                var cacheService = new MockupCacheService();
                var updatesService = new MockupUpdatesService();

                _transportService = new TransportService();
                var connectionService = new ConnectionService(deviceInfoService);
                var publicConfigService = new MockupPublicConfigService();

                var mtProtoService = new MTProtoService(deviceInfoService, updatesService, cacheService, _transportService, connectionService, publicConfigService);
                mtProtoService.Initialized += (o, e) =>
                {
                    //Log(string.Format("[MTProtoUpdater {0}] Initialized", GetHashCode()));
                    Thread.Sleep(1000);
                    callback.SafeInvoke();
                };
                mtProtoService.InitializationFailed += (o, e) =>
                {
                    //Log(string.Format("[MTProtoUpdater {0}] InitializationFailed", GetHashCode()));
                };
                mtProtoService.Initialize();

                MTProtoService = mtProtoService;
            }
            else
            {
                callback.SafeInvoke();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CallInProgressAgentImpl()
            : base()
        {
            _timer = new Timer(OnTimer);
        }

        private Timer _timer;

        private void OnTimer(object state)
        {
            Log(string.Format("OnTimer call_id={0} suppress={1}", Globals.Instance.CallController != null ? Globals.Instance.CallController.CallId.ToString() : "null", Suppress));
            Debug.WriteLine("[CallInProgressAgentImpl {0}] OnTick.", GetHashCode());

            //InitializeServiceAsync(() =>
            //{
            //    var getStateAction = new TLGetState();
            //    var actions = new List<TLObject> { getStateAction };
            //    MTProtoService.SendActionsAsync(actions, (request, result) =>
            //        {
            //            Log("[CallInProgressAgentImpl] getState result=" + result);
            //        },
            //        error =>
            //        {
            //            Log("[CallInProgressAgentImpl] getState error=" + error);
            //        });
            //});
        }

        /// <summary>
        /// The first call has become active.
        /// </summary>
        protected override void OnFirstCallStarting()
        {
            Debug.WriteLine("[CallInProgressAgentImpl {0}] The first call has started.", GetHashCode());

            Log("Start timer");
            // Indicate that an agent has started running
            AgentHost.OnAgentStarted();
            _timer.Change(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0));
        }

        /// <summary>
        /// The last call has ended.
        /// </summary>
        protected override void OnCancel()
        {
            Debug.WriteLine("[CallInProgressAgentImpl {0}] The last call has ended. Calling NotifyComplete", GetHashCode());

            if (MTProtoService != null) MTProtoService.Stop();
            Log("Stop timer");
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            // This agent is done
            base.NotifyComplete();
        }
    }
}
