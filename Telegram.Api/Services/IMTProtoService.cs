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
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Contacts;
using Telegram.Api.Transport;

namespace Telegram.Api.Services
{
    public interface IMTProtoService
    {
        TLEncryptedTransportMessage GetEncryptedTransportMessage(byte[] authKey, TLLong salt, TLObject obj);

#if DEBUG
        void CheckPublicConfig();
#endif

        TLInputPeerBase PeerToInputPeer(TLPeerBase peer);

        void Stop();

        void StartInitialize();

        void RemoveFromQueue(TLLong id);

        event EventHandler<TransportCheckedEventArgs> TransportChecked;

        string Message { get; }
        void SetMessageOnTime(double seconds, string message);

        ITransport GetActiveTransport();
        WindowsPhone.Tuple<int, int, int> GetCurrentPacketInfo();
        string GetTransportInfo();

        string Country { get; }
        event EventHandler<CountryEventArgs> GotUserCountry;

        // To remove multiple UpdateStatusAsync calls, it's prefer to invoke this method instead
        void RaiseSendStatus(SendStatusEventArgs e);

        TLInt CurrentUserId { get; set; }

        IList<HistoryItem> History { get; }

        void ClearHistory(string caption, bool createNewSession, bool syncFaultCallbacks = false, Exception e = null);

        long ClientTicksDelta { get; }

        /// <summary>
        /// Indicates that service has authKey
        /// </summary>
        //bool IsInitialized { get; }
        event EventHandler Initialized;
        event EventHandler<AuthorizationRequiredEventArgs> AuthorizationRequired;
        event EventHandler CheckDeviceLocked;
        event EventHandler ProxyDisabled;

        void SaveConfig();
        TLConfig LoadConfig();

        void GetStateAsync(Action<TLState> callback, Action<TLRPCError> faultCallback = null);
        void GetDifferenceAsync(TLInt pts, TLInt date, TLInt qts, Action<TLDifferenceBase> callback, Action<TLRPCError> faultCallback = null);
        void GetDifferenceWithoutUpdatesAsync(TLInt pts, TLInt date, TLInt qts, Action<TLDifferenceBase> callback, Action<TLRPCError> faultCallback = null);

        void RegisterDeviceAsync(TLInt tokenType, TLString token, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void UnregisterDeviceAsync(TLInt tokenType, TLString token, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        

        void MessageAcknowledgments(TLVector<TLLong> ids);

        // auth
        void BindTempAuthKeyAsync(TLLong permAuthKeyId, TLLong nonce, TLInt expiresAt, TLString encryptedMessage, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SendCodeAsync(TLString phoneNumber, TLString currentNumber, Action<TLSentCodeBase> callback, Action<int> attemptFailed = null, Action<TLRPCError> faultCallback = null);
        void ResendCodeAsync(TLString phoneNumber, TLString phoneCodeHash, Action<TLSentCodeBase> callback, Action<TLRPCError> faultCallback = null);
        void CancelCodeAsync(TLString phoneNumber, TLString phoneCodeHash, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SignInAsync(TLString phoneNumber, TLString phoneCodeHash, TLString phoneCode, Action<TLAuthorization> callback, Action<TLRPCError> faultCallback = null);
        void CancelSignInAsync();
        void LogOutAsync(Action callback);
        void LogOutAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void LogOutTransportsAsync(Action callback, Action<List<TLRPCError>> faultCallback = null);
        void SignUpAsync(TLString phoneNumber, TLString phoneCodeHash, TLString phoneCode, TLString firstName, TLString lastName, Action<TLAuthorization> callback, Action<TLRPCError> faultCallback = null);
        void SendCallAsync(TLString phoneNumber, TLString phoneCodeHash, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
       
        void SearchAsync(TLInputPeerBase peer, TLString query, TLInputUserBase fromId, TLInputMessagesFilterBase filter, TLInt minDate, TLInt maxDate, TLInt addOffset, TLInt offsetId, TLInt limit, TLInt hash, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetDialogsAsync(Stopwatch timer, TLInt offsetDate, TLInt offsetId, TLInputPeerBase offsetPeer, TLInt limit, TLInt hash, Action<TLDialogsBase> callback, Action<TLRPCError> faultCallback = null);
        void GetHistoryAsync(Stopwatch timer, TLInputPeerBase inputPeer, TLPeerBase peer, bool sync, TLInt offsetDate, TLInt offset, TLInt maxId, TLInt limit, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void DeleteMessagesAsync(bool revoke, TLVector<TLInt> id, Action<TLAffectedMessages> callback, Action<TLRPCError> faultCallback = null);
        void DeleteHistoryAsync(bool justClear, TLInputPeerBase peer, TLInt offset, Action<TLAffectedHistory> callback, Action<TLRPCError> faultCallback = null);
        void ReadHistoryAsync(TLInputPeerBase peer, TLInt maxId, TLInt offset, Action<TLAffectedMessages> callback, Action<TLRPCError> faultCallback = null);
        void ReadMentionsAsync(TLInputPeerBase peer, Action<TLAffectedHistory24> callback, Action<TLRPCError> faultCallback = null);
        void ReadMessageContentsAsync(TLVector<TLInt> id, Action<TLAffectedMessages> callback, Action<TLRPCError> faultCallback = null);
        void GetFullChatAsync(TLInt chatId, Action<TLMessagesChatFull> callback, Action<TLRPCError> faultCallback = null);

        void SetTypingAsync(TLInputPeerBase peer, TLBool typing, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SetTypingAsync(TLInputPeerBase peer, TLSendMessageActionBase action, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        void GetContactsAsync(TLInt hash, Action<TLContactsBase> callback, Action<TLRPCError> faultCallback = null);     
        void ImportContactsAsync(TLVector<TLInputContactBase> contacts, Action<TLImportedContacts> callback, Action<TLRPCError> faultCallback = null);

        void BlockAsync(TLInputUserBase id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void UnblockAsync(TLInputUserBase id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null); 
        void GetBlockedAsync(TLInt offset, TLInt limit, Action<TLContactsBlockedBase> callback, Action<TLRPCError> faultCallback = null);

        void UpdateProfileAsync(TLString firstName, TLString lastName, TLString about, Action<TLUserBase> callback, Action<TLRPCError> faultCallback = null);
        void UpdateStatusAsync(TLBool offline, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        void GetFileAsync(TLInt dcId, TLInputFileLocationBase location, TLInt offset, TLInt limit, Action<TLFileBase> callback, Action<TLRPCError> faultCallback = null);
        void GetFileAsync(TLInputFileLocationBase location, TLInt offset, TLInt limit, Action<TLFileBase> callback, Action<TLRPCError> faultCallback = null);
        void SaveFilePartAsync(TLLong fileId, TLInt filePart, TLString bytes, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SaveBigFilePartAsync(TLLong fileId, TLInt filePart, TLInt fileTotalParts, TLString bytes, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        void GetNotifySettingsAsync(TLInputNotifyPeerBase peer, Action<TLPeerNotifySettingsBase> settings, Action<TLRPCError> faultCallback = null);
        void UpdateNotifySettingsAsync(TLInputNotifyPeerBase peer, TLInputPeerNotifySettings settings, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void ResetNotifySettingsAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        void UploadProfilePhotoAsync(TLInputFile file, Action<TLPhotosPhoto> callback, Action<TLRPCError> faultCallback = null);
        void UpdateProfilePhotoAsync(TLInputPhotoBase id, Action<TLPhotoBase> callback, Action<TLRPCError> faultCallback = null);

        void GetDHConfigAsync(TLInt version, TLInt randomLength, Action<TLDHConfigBase> result, Action<TLRPCError> faultCallback = null);
        void RequestEncryptionAsync(TLInputUserBase userId, TLInt randomId, TLString g_a, Action<TLEncryptedChatBase> callback, Action<TLRPCError> faultCallback = null);
        void AcceptEncryptionAsync(TLInputEncryptedChat peer, TLString gb, TLLong keyFingerprint, Action<TLEncryptedChatBase> callback, Action<TLRPCError> faultCallback = null);
        void SendEncryptedAsync(TLInputEncryptedChat peer, TLLong randomId, TLString data, Action<TLSentEncryptedMessage> callback, Action fastCallback, Action<TLRPCError> faultCallback = null);
        void SendEncryptedFileAsync(TLInputEncryptedChat peer, TLLong randomId, TLString data, TLInputEncryptedFileBase file, Action<TLSentEncryptedFile> callback, Action fastCallback, Action<TLRPCError> faultCallback = null);
        void SendEncryptedMultiMediaAsync(TLInputEncryptedChat peer, TLVector<TLLong> randomId, TLVector<TLString> data, TLVector<TLInputEncryptedFileBase> file, Action<TLVector<TLSentEncryptedFile>> callback, Action fastCallback, Action<TLRPCError> faultCallback = null);
        void ReadEncryptedHistoryAsync(TLInputEncryptedChat peer, TLInt maxDate, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SendEncryptedServiceAsync(TLInputEncryptedChat peer, TLLong randomId, TLString data, Action<TLSentEncryptedMessage> callback, Action<TLRPCError> faultCallback = null);
        void DiscardEncryptionAsync(TLInt chatId, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SetEncryptedTypingAsync(TLInputEncryptedChat peer, TLBool typing, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        void GetConfigInformationAsync(Action<string> callback);
        void GetTransportInformationAsync(Action<string> callback);
        void GetUserPhotosAsync(TLInputUserBase userId, TLInt offset, TLLong maxId, TLInt limit, Action<TLPhotosBase> callback, Action<TLRPCError> faultCallback = null);
        void GetNearestDCAsync(Action<TLNearestDC> callback, Action<TLRPCError> faultCallback = null);
        void GetSupportAsync(Action<TLSupport> callback, Action<TLRPCError> faultCallback = null);

        void ResetAuthorizationsAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SetInitState();

        void PingAsync(TLLong pingId, Action<TLPong> callback, Action<TLRPCError> faultCallback = null); 
        void PingDelayDisconnectAsync(TLLong pingId, TLInt disconnectDelay, Action<TLPong> callback, Action<TLRPCError> faultCallback = null);

        void SearchAsync(TLString q, TLInt limit, Action<TLContactsFoundBase> callback, Action<TLRPCError> faultCallback = null);
        void CheckUsernameAsync(TLString username, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void UpdateUsernameAsync(TLString username, Action<TLUserBase> callback, Action<TLRPCError> faultCallback = null);
        void GetAccountTTLAsync(Action<TLAccountDaysTTL> callback, Action<TLRPCError> faultCallback = null);
        void SetAccountTTLAsync(TLAccountDaysTTL ttl, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void DeleteAccountTTLAsync(TLString reason, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetPrivacyAsync(TLInputPrivacyKeyBase key, Action<TLPrivacyRules> callback, Action<TLRPCError> faultCallback = null);
        void SetPrivacyAsync(TLInputPrivacyKeyBase key, TLVector<TLInputPrivacyRuleBase> rules, Action<TLPrivacyRules> callback, Action<TLRPCError> faultCallback = null);
        void GetStatusesAsync(Action<TLVector<TLContactStatusBase>> callback, Action<TLRPCError> faultCallback = null);
        void UpdateTransportInfoAsync(TLDCOption78 dcOption, TLString ipAddress, TLInt port, Action<bool> callback);
        void CheckAndUpdateTransportInfoAsync(TLInt dcId, TLString host, TLInt port, Action callback, Action<TLRPCError> faultCallback = null);

        void ResolveUsernameAsync(TLString username, Action<TLResolvedPeer> callback, Action<TLRPCError> faultCallback = null);
        void SendChangePhoneCodeAsync(TLString phoneNumber, TLString currentNumber, Action<TLSentCodeBase> callback, Action<TLRPCError> faultCallback = null);
        void ChangePhoneAsync(TLString phoneNumber, TLString phoneCodeHash, TLString phoneCode, Action<TLUserBase> callback, Action<TLRPCError> faultCallback = null);
        void GetWallpapersAsync(Action<TLVector<TLWallPaperBase>> callback, Action<TLRPCError> faultCallback = null);
        void GetAllStickersAsync(TLString hash, Action<TLAllStickersBase> callback, Action<TLRPCError> faultCallback = null);
        void GetMaskStickersAsync(TLString hash, Action<TLAllStickersBase> callback, Action<TLRPCError> faultCallback = null);

        void UpdateDeviceLockedAsync(TLInt period, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        void GetSendingQueueInfoAsync(Action<string> callback);
        void GetSyncErrorsAsync(Action<ExceptionInfo, IList<ExceptionInfo>> callback);
        void GetMessagesAsync(TLVector<TLInputMessageBase> id, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        // users
        void GetFullUserAsync(TLInputUserBase id, Action<TLUserFull> callback, Action<TLRPCError> faultCallback = null);
        void GetUsersAsync(TLVector<TLInputUserBase> id, Action<TLVector<TLUserBase>> callback, Action<TLRPCError> faultCallback = null);
        void SetSecureValueErrorsAsync(TLInputUserBase id, TLVector<TLSecureValueErrorBase> errors, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        // messages
        void GetRecentLocationsAsync(TLInputPeerBase peer, TLInt limit, TLInt hash, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetFeaturedStickersAsync(bool full, TLInt hash, Action<TLFeaturedStickersBase> callback, Action<TLRPCError> faultCallback = null);
        void GetArchivedStickersAsync(bool full, TLLong offsetId, TLInt limit, Action<TLArchivedStickers> callback, Action<TLRPCError> faultCallback = null);
        void ReadFeaturedStickersAsync(TLVector<TLLong> id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetAllDraftsAsync(Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void SaveDraftAsync(TLInputPeerBase peer, TLDraftMessageBase draft, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetInlineBotResultsAsync(TLInputUserBase bot, TLInputPeerBase peer, TLInputGeoPointBase geoPoint, TLString query, TLString offset, Action<TLBotResults> callback, Action<TLRPCError> faultCallback = null);
        void SetInlineBotResultsAsync(TLBool gallery, TLBool pr, TLLong queryId, TLVector<TLInputBotInlineResult> results, TLInt cacheTime, TLString nextOffset, TLInlineBotSwitchPM switchPM, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SendInlineBotResultAsync(TLMessage45 message, Action<TLMessageCommon> callback, Action fastCallback, Action<TLRPCError> faultCallback = null);
        void GetDocumentByHashAsync(TLString sha256, TLInt size, TLString mimeType, Action<TLDocumentBase> callback, Action<TLRPCError> faultCallback = null);
        void SearchGifsAsync(TLString q, TLInt offset, Action<TLFoundGifs> callback, Action<TLRPCError> faultCallback = null);
        void GetSavedGifsAsync(TLInt hash, Action<TLSavedGifsBase> callback, Action<TLRPCError> faultCallback = null);
        void SaveGifAsync(TLInputDocumentBase id, TLBool unsave, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void ReorderStickerSetsAsync(bool masks, TLVector<TLLong> order, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SearchGlobalAsync(TLString query, TLInt offsetDate, TLInputPeerBase offsetPeer, TLInt offsetId, TLInt limit, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void ReportSpamAsync(TLInputPeerBase peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SendMessageAsync(TLMessage36 message, Action<TLMessageCommon> callback, Action fastCallback, Action<TLRPCError> faultCallback = null);
        void SendMediaAsync(TLInputPeerBase inputPeer, TLInputMediaBase inputMedia, TLMessage34 message, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void StartBotAsync(TLInputUserBase bot, TLString startParam, TLMessage25 message, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void SendBroadcastAsync(TLVector<TLInputUserBase> contacts, TLInputMediaBase inputMedia, TLMessage25 message, Action<TLUpdatesBase> callback, Action fastCallback, Action<TLRPCError> faultCallback = null);
        void ForwardMessageAsync(TLInputPeerBase peer, TLInt fwdMessageId, TLMessage25 message, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void ForwardMessagesAsync(TLInputPeerBase toPeer, TLVector<TLInt> id, IList<TLMessage25> messages, bool withMyScore, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void ForwardMessagesAsync(TLMessage25 commentMessage, TLInputPeerBase toPeer, TLVector<TLInt> id, IList<TLMessage25> messages, bool withMyScore, Action<TLUpdatesBase[]> callback, Action<TLRPCError> faultCallback = null);
        void CreateChatAsync(TLVector<TLInputUserBase> users, TLString title, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void EditChatTitleAsync(TLInt chatId, TLString title, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void EditChatPhotoAsync(TLInt chatId, TLInputChatPhotoBase photo, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void AddChatUserAsync(TLInt chatId, TLInputUserBase userId, TLInt fwdLimit, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void DeleteChatUserAsync(TLInt chatId, TLInputUserBase userId, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetWebPagePreviewAsync(TLString message, Action<TLMessageMediaBase> callback, Action<TLRPCError> faultCallback = null);
        void ExportChatInviteAsync(TLInt chatId, Action<TLExportedChatInvite> callback, Action<TLRPCError> faultCallback = null);
        void CheckChatInviteAsync(TLString hash, Action<TLChatInviteBase> callback, Action<TLRPCError> faultCallback = null);
        void ImportChatInviteAsync(TLString hash, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetStickerSetAsync(TLInputStickerSetBase stickerset, Action<TLMessagesStickerSet> callback, Action<TLRPCError> faultCallback = null);
        void InstallStickerSetAsync(TLInputStickerSetBase stickerset, TLBool archived, Action<TLStickerSetInstallResultBase> callback, Action<TLRPCError> faultCallback = null);
        void UninstallStickerSetAsync(TLInputStickerSetBase stickerset, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void HideReportSpamAsync(TLInputPeerBase peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetPeerSettingsAsync(TLInputPeerBase peer, Action<TLPeerSettings> callback, Action<TLRPCError> faultCallback = null);
        void GetBotCallbackAnswerAsync(TLInputPeerBase peer, TLInt messageId, TLString data, TLBool game, Action<TLBotCallbackAnswer> callback, Action<TLRPCError> faultCallback = null);
        void GetPromoDialogAsync(TLInputPeerBase peer, Action<TLPeerDialogs> callback, Action<TLRPCError> faultCallback = null);
        void GetRecentStickersAsync(bool attached, TLInt hash, Action<TLRecentStickersBase> callback, Action<TLRPCError> faultCallback = null);
        void ClearRecentStickersAsync(bool attached, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetUnusedStickersAsync(TLInt limit, Action<TLVector<TLStickerSetCoveredBase>> callback, Action<TLRPCError> faultCallback = null);
        void GetAttachedStickersAsync(bool full, TLInputStickeredMediaBase media, Action<TLArchivedStickers> callback, Action<TLRPCError> faultCallback = null);
        void GetCommonChatsAsync(TLInputUserBase user, TLInt maxId, TLInt limit, Action<TLChatsBase> callback, Action<TLRPCError> faultCallback = null);
        void GetWebPageAsync(TLString url, TLInt hash, Action<TLWebPageBase> callback, Action<TLRPCError> faultCallback = null);
        void GetPinnedDialogsAsync(Action<TLPeerDialogs> callback, Action<TLRPCError> faultCallback = null);
        void ReorderPinnedDialogsAsync(bool force, TLVector<TLInputDialogPeerBase> order, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void ToggleDialogPinAsync(bool pinned, TLPeerBase peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);  
        void GetFavedStickersAsync(TLInt hash, Action<TLFavedStickersBase> callback, Action<TLRPCError> faultCallback = null);
        void FaveStickerAsync(TLInputDocumentBase id, TLBool unfave, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetUnreadMentionsAsync(TLInputPeerBase peer, TLInt offsetId, TLInt addOffset, TLInt limit, TLInt maxId, TLInt minId, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void UploadMediaAsync(TLInputPeerBase inputPeer, TLInputMediaBase inputMedia, Action<TLMessageMediaBase> callback, Action<TLRPCError> faultCallback = null);
        void SendMultiMediaAsync(TLInputPeerBase inputPeer, TLVector<TLInputSingleMedia> inputMedia, TLMessage25 message, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetStickersAsync(TLString emoticon, TLInt hash, Action<TLStickersBase> callback, Action<TLRPCError> faultCallback = null);
        void ReportAsync(TLInputPeerBase peer, TLVector<TLInt> id, TLInputReportReasonBase reason, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SearchStickerSetsAsync(bool full, bool excludeFeatured, TLString q, TLInt hash, Action<TLFoundStickerSetsBase> callback, Action<TLRPCError> faultCallback = null);
        void GetPeerDialogsAsync(TLInputPeerBase peer, Action<TLPeerDialogs> callback, Action<TLRPCError> faultCallback = null);
        void MarkDialogUnreadAsync(bool unread, TLInputDialogPeer peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetDialogUnreadMarksAsync(Action<TLVector<TLDialogPeerBase>> callback, Action<TLRPCError> faultCallback = null);
        void ToggleTopPeersAsync(TLBool enabled, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void ClearAllDraftsAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        // contacts
        void DeleteContactAsync(TLInputUserBase id, Action<TLLinkBase> callback, Action<TLRPCError> faultCallback = null);
        void DeleteContactsAsync(TLVector<TLInputUserBase> id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetTopPeersAsync(GetTopPeersFlags flags, TLInt offset, TLInt limit, TLInt hash, Action<TLTopPeersBase> callback, Action<TLRPCError> faultCallback = null);
        void ResetTopPeerRatingAsync(TLTopPeerCategoryBase category, TLInputPeerBase peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void ResetSavedAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetSavedAsync(Action<TLVector<TLSavedPhoneContact>> callback, Action<TLRPCError> faultCallback = null);
        
        // channels
        void GetChannelHistoryAsync(string debugInfo, TLInputPeerBase inputPeer, TLPeerBase peer, bool sync, TLInt offset, TLInt maxId, TLInt limit, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);   
        void GetMessagesAsync(TLInputChannelBase inputChannel, TLVector<TLInputMessageBase> id, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void UpdateChannelAsync(TLInt channelId, Action<TLMessagesChatFull> callback, Action<TLRPCError> faultCallback = null);
        void EditAdminAsync(TLChannel channel, TLInputUserBase userId, TLChannelAdminRights adminRights, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void KickFromChannelAsync(TLChannel channel, TLInputUserBase userId, TLBool kicked, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetParticipantAsync(TLInputChannelBase inputChannel, TLInputUserBase userId, Action<TLChannelsChannelParticipant> callback, Action<TLRPCError> faultCallback = null);
        void GetParticipantsAsync(TLInputChannelBase inputChannel, TLChannelParticipantsFilterBase filter, TLInt offset, TLInt limit, TLInt hash, Action<TLChannelParticipantsBase> callback, Action<TLRPCError> faultCallback = null);
        void EditTitleAsync(TLChannel channel, TLString title, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void EditAboutAsync(TLChannel channel, TLString about, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void EditPhotoAsync(TLChannel channel, TLInputChatPhotoBase photo, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void JoinChannelAsync(TLChannel channel, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void LeaveChannelAsync(TLChannel channel, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void DeleteChannelAsync(TLChannel channel, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void InviteToChannelAsync(TLInputChannelBase channel, TLVector<TLInputUserBase> users, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetFullChannelAsync(TLInputChannelBase channel, Action<TLMessagesChatFull> callback, Action<TLRPCError> faultCallback = null);
        void CreateChannelAsync(TLInt flags, TLString title, TLString about, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void ExportInviteAsync(TLInputChannelBase channel, Action<TLExportedChatInvite> callback, Action<TLRPCError> faultCallback = null);
        void CheckUsernameAsync(TLInputChannelBase channel, TLString username, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void UpdateUsernameAsync(TLInputChannelBase channel, TLString username, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetChannelDialogsAsync(TLInt offset, TLInt limit, Action<TLDialogsBase> callback, Action<TLRPCError> faultCallback = null);
        void GetImportantHistoryAsync(TLInputChannelBase channel, TLPeerBase peer, bool sync, TLInt offsetId, TLInt addOffset, TLInt limit, TLInt maxId, TLInt minId, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void ReadHistoryAsync(TLChannel channel, TLInt maxId, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void DeleteMessagesAsync(TLInputChannelBase channel, TLVector<TLInt> id, Action<TLAffectedMessages> callback, Action<TLRPCError> faultCallback = null);
        void ToggleInvitesAsync(TLInputChannelBase channel, TLBool enabled, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void ExportMessageLinkAsync(TLInputChannelBase channel, TLInt id, Action<TLExportedMessageLink> callback, Action<TLRPCError> faultCallback = null);
        void ToggleSignaturesAsync(TLInputChannelBase channel, TLBool enabled, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetMessageEditDataAsync(TLInputPeerBase peer, TLInt id, Action<TLMessageEditData> callback, Action<TLRPCError> faultCallback = null);
        void EditMessageAsync(TLInputPeerBase peer, TLInt id, TLString message, TLVector<TLMessageEntityBase> entities, TLInputMediaBase media, TLReplyKeyboardBase replyMarkup, bool noWebPage, bool stopGeoLive, TLInputGeoPointBase geoPoint, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void UpdatePinnedMessageAsync(bool silent, TLInputChannelBase channel, TLInt id, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void ReportSpamAsync(TLInputChannelBase channel, TLInt userId, TLVector<TLInt> id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void DeleteUserHistoryAsync(TLChannel channel, TLInputUserBase userId, Action<TLAffectedHistory> callback, Action<TLRPCError> faultCallback = null);
        void GetAdminedPublicChannelsAsync(Action<TLChatsBase> callback, Action<TLRPCError> faultCallback = null);
        void ReadMessageContentsAsync(TLInputChannelBase channel, TLVector<TLInt> id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SetStickersAsync(TLInputChannelBase channel, TLInputStickerSetBase stickerset, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void TogglePreHistoryHiddenAsync(TLInputChannelBase channel, TLBool enabled, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void DeleteHistoryAsync(TLInputChannelBase channel, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SetFeedBroadcastsAsync(TLInt feedId, TLVector<TLInputChannelBase> channels, TLBool alsoNewlyJoined, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void ChangeFeedBroadcastAsync(TLInputChannelBase channel, TLInt feedId, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void GetFeedAsync(bool offsetToMaxReed, TLInt feedId, TLFeedPosition offsetPosition, TLInt addOffset, TLInt limit, TLFeedPosition maxPosition, TLFeedPosition minPosition, TLInt hash, Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null);
        void ReadFeedAsync(TLInt feedId, TLFeedPosition maxPosition, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);

        // updates
        void GetChannelDifferenceAsync(bool force, TLInputChannelBase inputChannel, TLChannelMessagesFilerBase filter, TLInt pts, TLInt limit, Action<TLChannelDifferenceBase> callback, Action<TLRPCError> faultCallback = null);

        // admins
        void ToggleChatAdminsAsync(TLInt chatId, TLBool enabled, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void EditChatAdminAsync(TLInt chatId, TLInputUserBase userId, TLBool isAdmin, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void DeactivateChatAsync(TLInt chatId, TLBool enabled, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void MigrateChatAsync(TLInt chatId, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);

        // account
        void ReportPeerAsync(TLInputPeerBase peer, TLInputReportReasonBase reason, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void DeleteAccountAsync(TLString reason, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetAuthorizationsAsync(Action<TLAccountAuthorizations> callback, Action<TLRPCError> faultCallback = null);
        void ResetAuthorizationAsync(TLLong hash, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetPasswordAsync(Action<TLPasswordBase> callback, Action<TLRPCError> faultCallback = null);
        void GetPasswordSettingsAsync(TLInputCheckPasswordBase password, Action<TLPasswordSettings> callback, Action<TLRPCError> faultCallback = null);
        void UpdatePasswordSettingsAsync(TLInputCheckPasswordBase password, TLPasswordInputSettings newSettings, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void CheckPasswordAsync(TLInputCheckPasswordBase password, Action<TLAuthorization> callback, Action<TLRPCError> faultCallback = null);
        void RequestPasswordRecoveryAsync(Action<TLPasswordRecovery> callback, Action<TLRPCError> faultCallback = null);
        void RecoverPasswordAsync(TLString code, Action<TLAuthorization> callback, Action<TLRPCError> faultCallback = null);
        void ConfirmPhoneAsync(TLString phoneCodeHash, TLString phoneCode, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SendConfirmPhoneCodeAsync(TLString hash, TLBool currentNumber, Action<TLSentCodeBase> callback, Action<TLRPCError> faultCallback = null);
        void GetTmpPasswordAsync(TLInputCheckPasswordBase password, TLInt period, Action<TLTmpPassword> callback, Action<TLRPCError> faultCallback = null);
        void GetWebAuthorizationsAsync(Action<TLWebAuthorizations> callback, Action<TLRPCError> faultCallback = null);
        void ResetWebAuthorizationAsync(TLLong hash, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void ResetWebAuthorizationsAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetAllSecureValuesAsync(Action<TLVector<TLSecureValue>> callback, Action<TLRPCError> faultCallback = null);
        void GetSecureValueAsync(TLVector<TLSecureValueTypeBase> types, Action<TLVector<TLSecureValue>> callback, Action<TLRPCError> faultCallback = null);
        void SaveSecureValueAsync(TLInputSecureValue value, TLLong secureSecretId, Action<TLSecureValue> callback, Action<TLRPCError> faultCallback = null);
        void DeleteSecureValueAsync(TLVector<TLSecureValueTypeBase> types, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetAuthorizationFormAsync(TLInt botId, TLString scope, TLString publicKey, Action<TLAuthorizationForm> callback, Action<TLRPCError> faultCallback = null);
        void GetAuthorizationFormAndPassportConfigAsync(TLInt botId, TLString scope, TLString publicKey, TLInt passportSettingsHash, Action<TLAuthorizationForm, TLPassportConfigBase> callback, Action<TLRPCError> faultCallback = null);
        void AcceptAuthorizationAsync(TLInt botId, TLString scope, TLString publicKey, TLVector<TLSecureValueHash> valueHashes, TLSecureCredentialsEncrypted credentials, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SendVerifyPhoneCodeAsync(TLString phoneNumber, TLBool currentNumber, Action<TLSentCodeBase> callback, Action<TLRPCError> faultCallback = null);
        void VerifyPhoneAsync(TLString phoneNumber, TLString phoneCodeHash, TLString phoneCode, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void SendVerifyEmailCodeAsync(TLString email, Action<TLSentEmailCode> callback, Action<TLRPCError> faultCallback = null);
        void VerifyEmailAsync(TLString email, TLString code, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void InitTakeoutSessionAsync(bool contacts, bool messageUsers, bool messageChats, bool messageMegagroups, bool messageChannels, bool files, TLInt fileMaxSize, TLLong takeoutId, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void GetPassportDataAsync(Action<TLPasswordBase, IList<TLSecureValue>> callback, Action<TLRPCError> faultCallback = null); 

        // help
        void GetAppChangelogAsync(TLString deviceModel, TLString systemVersion, TLString appVersion, TLString langCode, Action<TLAppChangelogBase> callback, Action<TLRPCError> faultCallback = null);
        void GetTermsOfServiceAsync(TLString countryISO2, Action<TLTermsOfService> callback, Action<TLRPCError> faultCallback = null);
        void GetCdnConfigAsync(Action<TLCdnConfig> callback, Action<TLRPCError> faultCallback = null);
        void GetProxyDataAsync(Action<TLProxyDataBase> callback, Action<TLRPCError> faultCallback = null);
        void GetDeepLinkInfoAsync(TLString path, Action<TLDeepLinkInfoBase> callback, Action<TLRPCError> faultCallback = null);
        void GetPassportConfigAsync(TLInt hash, Action<TLPassportConfigBase> callback, Action<TLRPCError> faultCallback = null);

        // upload
        void GetCdnFileAsync(TLInt dcId, TLString fileToken, TLInt offset, TLInt limit, Action<TLCdnFileBase> callback, Action<TLRPCError> faultCallback = null);
        void ReuploadCdnFileAsync(TLInt dcId, TLString fileToken, TLString requestToken, Action<TLVector<TLFileHash>> callback, Action<TLRPCError> faultCallback = null);

        // encrypted chats
        void RekeyAsync(TLEncryptedChatBase chat, Action<TLLong> callback);

        // phone
        void GetCallConfigAsync(Action<TLDataJSON> callback, Action<TLRPCError> faultCallback = null);
        void RequestCallAsync(TLInputUserBase userId, TLInt randomId, TLString gaHash, TLPhoneCallProtocol protocol, Action<TLPhonePhoneCall> callback, Action<TLRPCError> faultCallback = null);
        void AcceptCallAsync(TLInputPhoneCall peer, TLString gb, TLPhoneCallProtocol protocol, Action<TLPhonePhoneCall> callback, Action<TLRPCError> faultCallback = null);
        void ConfirmCallAsync(TLInputPhoneCall peer, TLString ga, TLLong keyFingerprint, TLPhoneCallProtocol protocol, Action<TLPhonePhoneCall> callback, Action<TLRPCError> faultCallback = null);
        void ReceivedCallAsync(TLInputPhoneCall peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        void DiscardCallAsync(TLInputPhoneCall peer, TLInt duration, TLPhoneCallDiscardReasonBase reason, TLLong connectionId, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void SetCallRatingAsync(TLInputPhoneCall peer, TLInt rating, TLString comment, Action<TLUpdatesBase> callback, Action<TLRPCError> faultCallback = null);
        void SaveCallDebugAsync(TLInputPhoneCall peer, TLDataJSON debug, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);
        
        // payments
        void GetPaymentReceiptAsync(TLInt msgId, Action<TLPaymentReceipt> callback, Action<TLRPCError> faultCallback = null);
        void GetPaymentFormAsync(TLInt msgId, Action<TLPaymentForm> callback, Action<TLRPCError> faultCallback = null);
        void SendPaymentFormAsync(TLInt msgId, TLString requestedInfoId, TLString shippingOptionId, TLInputPaymentCredentialsBase credentials, Action<TLPaymentResultBase> callback, Action<TLRPCError> faultCallback = null);
        void ValidateRequestedInfoAsync(bool save, TLInt msgId, TLPaymentRequestedInfo info, Action<TLValidatedRequestedInfo> callback, Action<TLRPCError> faultCallback = null);
        void GetSavedInfoAsync(Action<TLSavedInfo> callback, Action<TLRPCError> faultCallback = null);
        void ClearSavedInfoAsync(bool credentials, bool info, Action<TLBool> callback, Action<TLRPCError> faultCallback = null);

        // langpack
        void GetLangPackAsync(TLString langCode, Action<TLLangPackDifference> callback, Action<TLRPCError> faultCallback = null);
        void GetStringsAsync(TLString langCode, TLVector<TLString> keys, Action<TLVector<TLLangPackStringBase>> callback, Action<TLRPCError> faultCallback = null);
        void GetDifferenceAsync(TLInt fromVersion, Action<TLLangPackDifference> callback, Action<TLRPCError> faultCallback = null);
        void GetLanguagesAsync(Action<TLVector<TLLangPackLanguage>> callback, Action<TLRPCError> faultCallback = null);
        
        // proxy
        void PingProxyAsync(TLProxyBase proxy, Action<TLInt> callback, Action<TLRPCError> faultCallback = null);

        // background task
        void SendActionsAsync(List<TLObject> actions, Action<TLObject, TLObject> callback, Action<TLRPCError> faultCallback = null);
        void ClearQueue();
    }
}
