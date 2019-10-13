// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Help;
using Telegram.Api.TL.Functions.Updates;

namespace Telegram.Api.Services
{
	public partial class MTProtoService
	{
        public void GetStateAsync(Action<TLState> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetState();

            SendInformativeMessage("updates.getState", obj, callback, faultCallback);
        }

        public void GetStateWithoutUpdatesAsync(Action<TLState> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLInvokeWithoutUpdates {Object = new TLGetState()};

            SendInformativeMessage("updates.getState", obj, callback, faultCallback);
        }

        public void GetDifferenceAsync(TLInt pts, TLInt date, TLInt qts, Action<TLDifferenceBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetDifference { Flags = new TLInt(0), Pts = pts, Date = date, Qts = qts };

            SendInformativeMessage("updates.getDifference", obj, callback, faultCallback);
        }

        public void GetDifferenceWithoutUpdatesAsync(TLInt pts, TLInt date, TLInt qts, Action<TLDifferenceBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetDifference { Flags = new TLInt(0), Pts = pts, Date = date, Qts = qts };

            SendInformativeMessage("updates.getDifference", new TLInvokeWithoutUpdates{Object = obj}, callback, faultCallback);
        }
	}
}
