// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using PhoneVoIPApp.BackEnd;
using Telegram.Api;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.TL;

namespace PhoneVoIPApp.Agents
{
    internal class MTProtoUpdater : IMTProtoUpdater, IHandle<TLUpdateBase>
    {
        private static bool _logEnabled = true;

        private static readonly int _id = new Random().Next(999);

        private static void Log(string message, Action callback = null)
        {
            if (!_logEnabled) return;

            Telegram.Logs.Log.Write(string.Format("::MTProtoUpdater {0} {1}", _id, message), callback.SafeInvoke);
#if DEBUG
            //PushUtils.AddToast("push", message, string.Empty, string.Empty, null, null);
#endif
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

        public void Start(int pts, int date, int qts)
        {
            Log(string.Format("[MTProtoUpdater {0}] Start timer", GetHashCode()));

            CallInProgressAgentImpl.Suppress = true;
        }

        public void Stop()
        {
            Log(string.Format("[MTProtoUpdater {0}] Stop timer", GetHashCode()));

            CallInProgressAgentImpl.Suppress = false;
        }

        public void ReceivedCall(long id, long accessHash)
        {
        }

        public void DiscardCall(long id, long accessHash)
        {

        }

        public static void Handle(TLUpdateBase updateBase)
        {
            var updatePhoneCall = updateBase as TLUpdatePhoneCall;
            if (updatePhoneCall != null)
            {
                var phoneCallDiscarded = updatePhoneCall.PhoneCall as TLPhoneCallDiscarded61;
                //if (phoneCallDiscarded != null && Globals.Instance.CallController.CallId == phoneCallDiscarded.Id.Value)
                //{
                //    Globals.Instance.CallController.EndCall();
                //}
            }

            //Globals.Instance.CallController.HandleUpdatePhoneCall();
        }

        void IHandle<TLUpdateBase>.Handle(TLUpdateBase message)
        {
            Handle(message);
        }
    }
}
