// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Phone;

namespace Telegram.Api.Services
{
    public partial class MTProtoService
    {
        public void GetCallConfigAsync(Action<TLDataJSON> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetCallConfig();

            SendInformativeMessage("phone.requestCall", obj, callback, faultCallback);
        }

        public void RequestCallAsync(TLInputUserBase userId, TLInt randomId, TLString gaHash, TLPhoneCallProtocol protocol, Action<TLPhonePhoneCall> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLRequestCall { UserId = userId, RandomId = randomId, GAHash = gaHash, Protocol = protocol };

            SendInformativeMessage("phone.requestCall", obj, callback, faultCallback);
        }

        public void AcceptCallAsync(TLInputPhoneCall peer, TLString gb, TLPhoneCallProtocol protocol, Action<TLPhonePhoneCall> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLAcceptCall { Peer = peer, GB = gb, Protocol = protocol };

            SendInformativeMessage("phone.acceptCall", obj, callback, faultCallback);
        }

        public void ConfirmCallAsync(TLInputPhoneCall peer, TLString ga, TLLong keyFingerprint, TLPhoneCallProtocol protocol, Action<TLPhonePhoneCall> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLConfirmCall { Peer = peer, GA = ga, KeyFingerprint = keyFingerprint, Protocol = protocol };

            SendInformativeMessage("phone.confirmCall", obj, callback, faultCallback);
        }

        public void ReceivedCallAsync(TLInputPhoneCall peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLReceivedCall { Peer = peer };

            SendInformativeMessage("phone.receivedCall", obj, callback, faultCallback);
        }

        public void DiscardCallAsync(TLInputPhoneCall peer, TLInt duration, TLPhoneCallDiscardReasonBase reason, TLLong connectionId, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLDiscardCall { Peer = peer, Duration = duration, Reason = reason, ConnectionId = connectionId };

            const string caption = "phone.discardCall";
            SendInformativeMessage<TLUpdatesBase>(caption, obj,
                result =>
                {
                    var multiPts = result as IMultiPts;
                    if (multiPts != null)
                    {
                        _updatesService.SetState(multiPts, caption);
                    }
                    else
                    {
                        ProcessUpdates(result, null, true);
                    }

                    callback.SafeInvoke(result);
                },
                faultCallback);
        }

        public void SetCallRatingAsync(TLInputPhoneCall peer, TLInt rating, TLString comment, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSetCallRating { Peer = peer, Rating = rating, Comment = comment };

            const string caption = "phone.setCallRating";
            SendInformativeMessage<TLUpdatesBase>(caption, obj,
                result =>
                {
                    var multiPts = result as IMultiPts;
                    if (multiPts != null)
                    {
                        _updatesService.SetState(multiPts, caption);
                    }
                    else
                    {
                        ProcessUpdates(result, null, true);
                    }

                    callback.SafeInvoke(result);
                },
                faultCallback);
        }

        public void SaveCallDebugAsync(TLInputPhoneCall peer, TLDataJSON debug, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSaveCallDebug { Peer = peer, Debug = debug };

            SendInformativeMessage("phone.saveCallDebug", obj, callback, faultCallback);
        }
    }
}
