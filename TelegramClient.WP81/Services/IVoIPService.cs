// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.TL;

namespace TelegramClient.Services
{
    public interface IVoIPService
    {
        long AcceptedCallId { get; set; }
        TLPhoneCallBase Call { get; }
        TLInt UserId { get; }
        void StartOutgoingCall(TLUser user, Action<TLLong> callback);
        void AcceptIncomingCall(TLPhoneCallRequested64 requestedCall);
        string GetDebugString();
        string GetDebugLog(long callId);
        string GetVersion();
        void HangUp();
        string[] GetEmojis();
        void SwitchSpeaker(bool external);
        void Mute(bool muted);
        int GetSignalBarsCount();
    }
}
