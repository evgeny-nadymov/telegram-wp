// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using Telegram.Api.TL;

namespace Telegram.Api.Services.Location
{
    public interface ILiveLocationService
    {
        void Load();

        void UpdateAll();

        void LoadAndUpdateAllAsync();

        void Add(TLMessage70 messageBase);

        void UpdateAsync(TLMessage70 message, TLGeoPointBase geoPoint, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);

        TLMessage Get(TLPeerBase peer, TLInt fromId);

        IList<TLMessage> Get();

        void Clear();

        void StopAllAsync();
    }
}
