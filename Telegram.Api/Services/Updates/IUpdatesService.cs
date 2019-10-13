// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;

namespace Telegram.Api.Services.Updates
{
    public delegate void GetDifferenceAction(TLInt pts, TLInt date, TLInt qts, Action<TLDifferenceBase> callback, Action<TLRPCError> faultCallback);
    public delegate void GetDHConfigAction(TLInt version, TLInt randomLength, Action<TLDHConfigBase> callback, Action<TLRPCError> faultCallback);
    public delegate void AcceptEncryptionAction(TLInputEncryptedChat peer, TLString gb, TLLong keyFingerprint, Action<TLEncryptedChatBase> callback, Action<TLRPCError> faultCallback);
    public delegate void SendEncryptedServiceAction(TLInputEncryptedChat peer, TLLong randomkId, TLString data, Action<TLSentEncryptedMessage> callback, Action<TLRPCError> faultCallback);
    public delegate void UpdateChannelAction(TLInt channelId, Action<TLMessagesChatFull> callback, Action<TLRPCError> faultCallback);
    public delegate void GetFullChatAction(TLInt chatId, Action<TLMessagesChatFull> callback, Action<TLRPCError> faultCallback);
    public delegate void GetFullUserAction(TLInputUserBase userId, Action<TLUserFull> callback, Action<TLRPCError> faultCallback);
    public delegate void GetPinnedDialogsAction(Action<TLPeerDialogs> callback, Action<TLRPCError> faultCallback = null);
    public delegate void GetChannelMessagesAction(TLInputChannelBase channelId, TLVector<TLInputMessageBase> id, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback);
    public delegate void GetMessagesAction(TLVector<TLInputMessageBase> id, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
    public delegate void GetPeerDialogsAction(TLInputPeerBase peer, Action<TLPeerDialogs> callback, Action<TLRPCError> faultCallback = null);
    public delegate void GetPromoDialogAction(TLInputPeerBase peer, Action<TLPeerDialogs> callback, Action<TLRPCError> faultCallback = null);

    public delegate void SetMessageOnTimeAtion(double seconds, string message);

    public interface IUpdatesService
    {
        bool ProcessUpdateInternal(TLUpdateBase update, bool notifyNewMessage = true);

        void CancelUpdating();

        IList<ExceptionInfo> SyncDifferenceExceptions { get; }
        //void IncrementClientSeq();

        Func<TLInt> GetCurrentUserId { get; set; }

        Action<Action<TLState>, Action<TLRPCError>> GetStateAsync { get; set; }
        GetDHConfigAction GetDHConfigAsync { get; set; }
        GetDifferenceAction GetDifferenceAsync { get; set; }
        AcceptEncryptionAction AcceptEncryptionAsync { get; set; }
        SendEncryptedServiceAction SendEncryptedServiceAsync { get; set; }
        SetMessageOnTimeAtion SetMessageOnTimeAsync { get; set; }
        Action<TLLong> RemoveFromQueue { get; set; }
        UpdateChannelAction UpdateChannelAsync { get; set; }
        GetFullChatAction GetFullChatAsync { get; set; }
        GetFullUserAction GetFullUserAsync { get; set; }
        GetChannelMessagesAction GetChannelMessagesAsync { get; set; }
        GetPinnedDialogsAction GetPinnedDialogsAsync { get; set; }
        GetMessagesAction GetMessagesAsync { get; set; }
        GetPeerDialogsAction GetPeerDialogsAsync { get; set; }
        GetPromoDialogAction GetPromoDialogAsync { get; set; }

        void SetInitState();

        TLInt ClientSeq { get; }
        void SetState(TLInt seq, TLInt pts, TLInt qts, TLInt date, TLInt unreadCount, string caption, bool cleanupMissingCounts = false);
        void SetState(IMultiPts multiPts, string caption);
        void ProcessTransportMessage(TLTransportMessage transportMessage);
        void ProcessUpdates(TLUpdatesBase updates, bool notifyNewMessages = false);

        void LoadStateAndUpdate(long acceptedCallId, Action callback);
        void SaveState();
        TLState GetState();
        void ClearState();

        void SaveStateSnapshot(string toDirectoryName);
        void LoadStateSnapshot(string fromDirectoryName);

        event EventHandler<DCOptionsUpdatedEventArgs> DCOptionsUpdated;
    }
}
