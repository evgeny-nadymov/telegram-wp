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
using System.Linq;
using System.Reflection;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL.Account;
using Telegram.Api.TL.Functions.Channels;
using Telegram.Api.TL.Functions.Contacts;
using Telegram.Api.TL.Functions.Messages;

namespace Telegram.Api.TL
{
    public class TLObjectGenerator
    {
        private static readonly Dictionary<Type, Func<TLObject>> _baredTypes =
            new Dictionary<Type, Func<TLObject>>
                {
                    {typeof (TLDouble), () => new TLDouble()},
                    {typeof (TLBool), () => new TLBool()},
                    {typeof (TLInt), () => new TLInt()},
                    {typeof (TLLong), () => new TLLong()},
                    {typeof (TLInt128), () => new TLInt128()},
                    {typeof (TLInt256), () => new TLInt256()},
                    {typeof (TLString), () => new TLString()},
                    {typeof (TLNonEncryptedMessage), () => new TLNonEncryptedMessage()},
                    {typeof (TLTransportMessage), () => new TLTransportMessage()},
                    {typeof (TLContainerTransportMessage), () => new TLContainerTransportMessage()},
                    {typeof (TLIpPort), () => new TLIpPort()},
                };

        private static readonly Dictionary<uint, Func<TLObject>> _clothedTypes =
            new Dictionary<uint, Func<TLObject>>
                {
                    {TLUpdateUserBlocked.Signature, () => new TLUpdateUserBlocked()},
                    {TLUpdateNotifySettings.Signature, () => new TLUpdateNotifySettings()},

                    {TLNotifyPeer.Signature, () => new TLNotifyPeer()},
                    {TLNotifyUsers.Signature, () => new TLNotifyUsers()},
                    {TLNotifyChats.Signature, () => new TLNotifyChats()},
                    {TLNotifyAll.Signature, () => new TLNotifyAll()},

                    {TLDecryptedMessageLayer.Signature, () => new TLDecryptedMessageLayer()},
                    {TLUpdateDCOptions.Signature, () => new TLUpdateDCOptions()},
                    
                    {TLDecryptedMessageMediaAudio.Signature, () => new TLDecryptedMessageMediaAudio()},
                    {TLDecryptedMessageMediaDocument.Signature, () => new TLDecryptedMessageMediaDocument()},

                    {TLInputMediaDocument.Signature, () => new TLInputMediaDocument()},
                    {TLInputMediaUploadedDocument.Signature, () => new TLInputMediaUploadedDocument()},
                    {TLInputMediaUploadedThumbDocument.Signature, () => new TLInputMediaUploadedThumbDocument()},

                    {TLInputMediaAudio.Signature, () => new TLInputMediaAudio()},
                    {TLInputMediaUploadedAudio.Signature, () => new TLInputMediaUploadedAudio()}, 
                
                    {TLInputDocument.Signature, () => new TLInputDocument()},
                    {TLInputDocumentEmpty.Signature, () => new TLInputDocumentEmpty()}, 
                
                    {TLInputAudio.Signature, () => new TLInputAudio()},
                    {TLInputAudioEmpty.Signature, () => new TLInputAudioEmpty()},

                    {TLMessageMediaAudio.Signature, () => new TLMessageMediaAudio()},
                    {TLMessageMediaDocument.Signature, () => new TLMessageMediaDocument()},

                    {TLAudioEmpty.Signature, () => new TLAudioEmpty()},
                    {TLAudio.Signature, () => new TLAudio()},

                    {TLDocumentEmpty.Signature, () => new TLDocumentEmpty()},
                    {TLDocument10.Signature, () => new TLDocument10()},

                    {TLUpdateChatParticipantAdd.Signature, () => new TLUpdateChatParticipantAdd()},
                    {TLUpdateChatParticipantDelete.Signature, () => new TLUpdateChatParticipantDelete()},

                    {TLInputEncryptedFileBigUploaded.Signature, () => new TLInputEncryptedFileBigUploaded()},
                    {TLInputFileBig.Signature, () => new TLInputFileBig()},

                    {TLDecryptedMessageActionSetMessageTTL.Signature, () => new TLDecryptedMessageActionSetMessageTTL()},
                    {TLDecryptedMessageActionReadMessages.Signature, () => new TLDecryptedMessageActionReadMessages()},
                    {TLDecryptedMessageActionDeleteMessages.Signature, () => new TLDecryptedMessageActionDeleteMessages()},
                    {TLDecryptedMessageActionScreenshotMessages.Signature, () => new TLDecryptedMessageActionScreenshotMessages()},
                    {TLDecryptedMessageActionFlushHistory.Signature, () => new TLDecryptedMessageActionFlushHistory()},
                    {TLDecryptedMessageActionNotifyLayer.Signature, () => new TLDecryptedMessageActionNotifyLayer()},

                    {TLDecryptedMessage.Signature, () => new TLDecryptedMessage()},
                    {TLDecryptedMessageService.Signature, () => new TLDecryptedMessageService()},

                    {TLUpdateNewEncryptedMessage.Signature, () => new TLUpdateNewEncryptedMessage()},
                    {TLUpdateEncryptedChatTyping.Signature, () => new TLUpdateEncryptedChatTyping()},
                    {TLUpdateEncryption.Signature, () => new TLUpdateEncryption()},
                    {TLUpdateEncryptedMessagesRead.Signature, () => new TLUpdateEncryptedMessagesRead()},
                    
                    {TLEncryptedChatEmpty.Signature, () => new TLEncryptedChatEmpty()},
                    {TLEncryptedChatWaiting.Signature, () => new TLEncryptedChatWaiting()},
                    {TLEncryptedChatRequested.Signature, () => new TLEncryptedChatRequested()},
                    {TLEncryptedChat.Signature, () => new TLEncryptedChat()},
                    {TLEncryptedChatDiscarded.Signature, () => new TLEncryptedChatDiscarded()},
                    
                    {TLInputEncryptedChat.Signature, () => new TLInputEncryptedChat()},

                    {TLInputEncryptedFileEmpty.Signature, () => new TLInputEncryptedFileEmpty()},
                    {TLInputEncryptedFileUploaded.Signature, () => new TLInputEncryptedFileUploaded()},
                    {TLInputEncryptedFile.Signature, () => new TLInputEncryptedFile()},

                    {TLInputEncryptedFileLocation.Signature, () => new TLInputEncryptedFileLocation()},

                    {TLEncryptedFileEmpty.Signature, () => new TLEncryptedFileEmpty()},
                    {TLEncryptedFile.Signature, () => new TLEncryptedFile()},

                    {TLEncryptedMessage.Signature, () => new TLEncryptedMessage()},
                    {TLEncryptedMessageService.Signature, () => new TLEncryptedMessageService()},

                    {TLDecryptedMessageMediaEmpty.Signature, () => new TLDecryptedMessageMediaEmpty()},
                    {TLDecryptedMessageMediaPhoto.Signature, () => new TLDecryptedMessageMediaPhoto()},
                    {TLDecryptedMessageMediaVideo.Signature, () => new TLDecryptedMessageMediaVideo()},
                    {TLDecryptedMessageMediaGeoPoint.Signature, () => new TLDecryptedMessageMediaGeoPoint()},
                    {TLDecryptedMessageMediaContact.Signature, () => new TLDecryptedMessageMediaContact()},
                    
                    {TLDHConfig.Signature, () => new TLDHConfig()},
                    {TLDHConfigNotModified.Signature, () => new TLDHConfigNotModified()},
                    
                    {TLSentEncryptedMessage.Signature, () => new TLSentEncryptedMessage()},
                    {TLSentEncryptedFile.Signature, () => new TLSentEncryptedFile()},

                    {TLMessageDetailedInfo.Signature, () => new TLMessageDetailedInfo()},
                    {TLMessageNewDetailedInfo.Signature, () => new TLMessageNewDetailedInfo()},
                    {TLMessagesAllInfo.Signature, () => new TLMessagesAllInfo()},

                    {TLUpdateNewMessage.Signature, () => new TLUpdateNewMessage()},
                    {TLUpdateMessageId.Signature, () => new TLUpdateMessageId()},
                    {TLUpdateReadMessages.Signature, () => new TLUpdateReadMessages()},
                    {TLUpdateDeleteMessages.Signature, () => new TLUpdateDeleteMessages()},
                    {TLUpdateRestoreMessages.Signature, () => new TLUpdateRestoreMessages()},
                    {TLUpdateUserTyping.Signature, () => new TLUpdateUserTyping()},
                    {TLUpdateChatUserTyping.Signature, () => new TLUpdateChatUserTyping()},
                    {TLUpdateChatParticipants.Signature, () => new TLUpdateChatParticipants()},
                    {TLUpdateUserStatus.Signature, () => new TLUpdateUserStatus()},
                    {TLUpdateUserName.Signature, () => new TLUpdateUserName()},
                    {TLUpdateUserPhoto.Signature, () => new TLUpdateUserPhoto()},
                    {TLUpdateContactRegistered.Signature, () => new TLUpdateContactRegistered()},
                    {TLUpdateContactLink.Signature, () => new TLUpdateContactLink()},
                    {TLUpdateActivation.Signature, () => new TLUpdateActivation()},
                    {TLUpdateNewAuthorization.Signature, () => new TLUpdateNewAuthorization()},

                    {TLDifferenceEmpty.Signature, () => new TLDifferenceEmpty()},
                    {TLDifference.Signature, () => new TLDifference()},
                    {TLDifferenceSlice.Signature, () => new TLDifferenceSlice()},

                    {TLUpdatesTooLong.Signature, () => new TLUpdatesTooLong()},
                    {TLUpdatesShortMessage.Signature, () => new TLUpdatesShortMessage()},
                    {TLUpdatesShortChatMessage.Signature, () => new TLUpdatesShortChatMessage()},
                    {TLUpdatesShort.Signature, () => new TLUpdatesShort()},
                    {TLUpdatesCombined.Signature, () => new TLUpdatesCombined()},
                    {TLUpdates.Signature, () => new TLUpdates()},

                    {TLFutureSalt.Signature, () => new TLFutureSalt()},
                    {TLFutureSalts.Signature, () => new TLFutureSalts()},

                    {TLGzipPacked.Signature, () => new TLGzipPacked()},
                    {TLState.Signature, () => new TLState()},

                    {TLFileTypeUnknown.Signature, () => new TLFileTypeUnknown()},
                    {TLFileTypeJpeg.Signature, () => new TLFileTypeJpeg()},
                    {TLFileTypeGif.Signature, () => new TLFileTypeGif()},
                    {TLFileTypePng.Signature, () => new TLFileTypePng()},
                    {TLFileTypeMp3.Signature, () => new TLFileTypeMp3()},
                    {TLFileTypeMov.Signature, () => new TLFileTypeMov()},
                    {TLFileTypePartial.Signature, () => new TLFileTypePartial()},
                    {TLFileTypeMp4.Signature, () => new TLFileTypeMp4()},
                    {TLFileTypeWebp.Signature, () => new TLFileTypeWebp()},


                    {TLFile.Signature, () => new TLFile()},
                    
                    {TLInputFileLocation.Signature, () => new TLInputFileLocation()},
                    {TLInputVideoFileLocation.Signature, () => new TLInputVideoFileLocation()},

                    {TLInviteText.Signature, () => new TLInviteText()},

                    {TLDHGenOk.Signature, () => new TLDHGenOk()},
                    {TLDHGenRetry.Signature, () => new TLDHGenRetry()},
                    {TLDHGenFail.Signature, () => new TLDHGenFail()},

                    {TLServerDHInnerData.Signature, () => new TLServerDHInnerData()},
                    {TLServerDHParamsFail.Signature, () => new TLServerDHParamsFail()},
                    {TLServerDHParamsOk.Signature, () => new TLServerDHParamsOk()},
                    {TLPQInnerData.Signature, () => new TLPQInnerData()},
                    {TLResPQ.Signature, () => new TLResPQ()},

                    {TLContactsBlocked.Signature, () => new TLContactsBlocked()},
                    {TLContactsBlockedSlice.Signature, () => new TLContactsBlockedSlice()},
                    {TLContactBlocked.Signature, () => new TLContactBlocked()},
                    
                    {TLImportedContacts.Signature, () => new TLImportedContacts()},
                    {TLImportedContact.Signature, () => new TLImportedContact()},

                    {TLInputContact.Signature, () => new TLInputContact()},

                    {TLContactStatus.Signature, () => new TLContactStatus()},

                    {TLForeignLinkUnknown.Signature, () => new TLForeignLinkUnknown()},
                    {TLForeignLinkRequested.Signature, () => new TLForeignLinkRequested()},
                    {TLForeignLinkMutual.Signature, () => new TLForeignLinkMutual()},

                    {TLMyLinkEmpty.Signature, () => new TLMyLinkEmpty()},
                    {TLMyLinkContact.Signature, () => new TLMyLinkContact()},
                    {TLMyLinkRequested.Signature, () => new TLMyLinkRequested()},

                    {TLLink.Signature, () => new TLLink()},

                    {TLUserFull.Signature, () => new TLUserFull()},
                    
                    {TLPhotos.Signature, () => new TLPhotos()},
                    {TLPhotosSlice.Signature, () => new TLPhotosSlice()},
                    {TLPhotosPhoto.Signature, () => new TLPhotosPhoto()},

                    {TLInputPeerNotifyEventsEmpty.Signature, () => new TLInputPeerNotifyEventsEmpty()},
                    {TLInputPeerNotifyEventsAll.Signature, () => new TLInputPeerNotifyEventsAll()},

                    {TLInputPeerNotifySettings.Signature, () => new TLInputPeerNotifySettings()},

                    {TLInputNotifyPeer.Signature, () => new TLInputNotifyPeer()},
                    {TLInputNotifyUsers.Signature, () => new TLInputNotifyUsers()},
                    {TLInputNotifyChats.Signature, () => new TLInputNotifyChats()},
                    {TLInputNotifyAll.Signature, () => new TLInputNotifyAll()},

                    {TLInputUserEmpty.Signature, () => new TLInputUserEmpty()},
                    {TLInputUserSelf.Signature, () => new TLInputUserSelf()},
                    {TLInputUserContact.Signature, () => new TLInputUserContact()},
                    {TLInputUserForeign.Signature, () => new TLInputUserForeign()},

                    {TLInputPhotoCropAuto.Signature, () => new TLInputPhotoCropAuto()},
                    {TLInputPhotoCrop.Signature, () => new TLInputPhotoCrop()},

                    {TLInputChatPhotoEmpty.Signature, () => new TLInputChatPhotoEmpty()},
                    {TLInputChatUploadedPhoto.Signature, () => new TLInputChatUploadedPhoto()},
                    {TLInputChatPhoto.Signature, () => new TLInputChatPhoto()},

                    {TLMessagesChatFull.Signature, () => new TLMessagesChatFull()},
                    {TLChatFull.Signature, () => new TLChatFull()},

                    {TLChatParticipant.Signature, () => new TLChatParticipant()},

                    {TLChatParticipantsForbidden.Signature, () => new TLChatParticipantsForbidden()},
                    {TLChatParticipants.Signature, () => new TLChatParticipants()},

                    {TLPeerNotifySettingsEmpty.Signature, () => new TLPeerNotifySettingsEmpty()},
                    {TLPeerNotifySettings.Signature, () => new TLPeerNotifySettings()},

                    {TLPeerNotifyEventsEmpty.Signature, () => new TLPeerNotifyEventsEmpty()},
                    {TLPeerNotifyEventsAll.Signature, () => new TLPeerNotifyEventsAll()},

                    {TLChats.Signature, () => new TLChats()},

                    {TLMessages.Signature, () => new TLMessages()},
                    {TLMessagesSlice.Signature, () => new TLMessagesSlice()},

                    {TLExportedAuthorization.Signature, () => new TLExportedAuthorization()},                   

                    {TLInputFile.Signature, () => new TLInputFile()},
                    {TLInputPhotoEmpty.Signature, () => new TLInputPhotoEmpty()},
                    {TLInputPhoto.Signature, () => new TLInputPhoto()},
                    {TLInputGeoPoint.Signature, () => new TLInputGeoPoint()}, 
                    {TLInputGeoPointEmpty.Signature, () => new TLInputGeoPointEmpty()},
                    {TLInputVideo.Signature, () => new TLInputVideo()}, 
                    {TLInputVideoEmpty.Signature, () => new TLInputVideoEmpty()},

                    {TLInputMediaEmpty.Signature, () => new TLInputMediaEmpty()},
                    {TLInputMediaUploadedPhoto.Signature, () => new TLInputMediaUploadedPhoto()},
                    {TLInputMediaPhoto.Signature, () => new TLInputMediaPhoto()},
                    {TLInputMediaGeoPoint.Signature, () => new TLInputMediaGeoPoint()}, 
                    {TLInputMediaContact.Signature, () => new TLInputMediaContact()},
                    {TLInputMediaUploadedVideo.Signature, () => new TLInputMediaUploadedVideo()},
                    {TLInputMediaUploadedThumbVideo.Signature, () => new TLInputMediaUploadedThumbVideo()},
                    {TLInputMediaVideo.Signature, () => new TLInputMediaVideo()},

                    {TLInputMessagesFilterEmpty.Signature, () => new TLInputMessagesFilterEmpty()},
                    {TLInputMessagesFilterPhoto.Signature, () => new TLInputMessagesFilterPhoto()},
                    {TLInputMessagesFilterVideo.Signature, () => new TLInputMessagesFilterVideo()},
                    {TLInputMessagesFilterPhotoVideo.Signature, () => new TLInputMessagesFilterPhotoVideo()},
                    {TLInputMessagesFilterPhotoVideoDocument.Signature, () => new TLInputMessagesFilterPhotoVideoDocument()},
                    {TLInputMessagesFilterDocument.Signature, () => new TLInputMessagesFilterDocument()},
                    {TLInputMessagesFilterAudio.Signature, () => new TLInputMessagesFilterAudio()}, 
                    {TLInputMessagesFilterAudioDocuments.Signature, () => new TLInputMessagesFilterAudioDocuments()}, 
                    {TLInputMessagesFilterUrl.Signature, () => new TLInputMessagesFilterUrl()}, 

                    {TLSentMessageLink.Signature, () => new TLSentMessageLink()},
                    {TLStatedMessage.Signature, () => new TLStatedMessage()},
                    {TLStatedMessageLink.Signature, () => new TLStatedMessageLink()},
                    {TLStatedMessages.Signature, () => new TLStatedMessages()},
                    {TLStatedMessagesLinks.Signature, () => new TLStatedMessagesLinks()},

                    {TLAffectedHistory.Signature, () => new TLAffectedHistory()},

                    {TLNull.Signature, () => new TLNull()},               

                    {TLBool.BoolTrue, () => new TLBool()},
                    {TLBool.BoolFalse, () => new TLBool()},
                    
                    {TLChatEmpty.Signature, () => new TLChatEmpty()},
                    {TLChat.Signature, () => new TLChat()},
                    {TLChatForbidden.Signature, () => new TLChatForbidden()},

                    {TLSentMessage.Signature, () => new TLSentMessage()},

                    {TLMessageEmpty.Signature, () => new TLMessageEmpty()},
                    {TLMessage.Signature, () => new TLMessage()},
                    {TLMessageForwarded.Signature, () => new TLMessageForwarded()},
                    {TLMessageService.Signature, () => new TLMessageService()},                   

                    {TLMessageMediaEmpty.Signature, () => new TLMessageMediaEmpty()},
                    {TLMessageMediaPhoto.Signature, () => new TLMessageMediaPhoto()},
                    {TLMessageMediaVideo.Signature, () => new TLMessageMediaVideo()},
                    {TLMessageMediaGeo.Signature, () => new TLMessageMediaGeo()},
                    {TLMessageMediaContact.Signature, () => new TLMessageMediaContact()},
                    {TLMessageMediaUnsupported.Signature, () => new TLMessageMediaUnsupported()},

                    {TLMessageActionEmpty.Signature, () => new TLMessageActionEmpty()},
                    {TLMessageActionChatCreate.Signature, () => new TLMessageActionChatCreate()},
                    {TLMessageActionChatEditTitle.Signature, () => new TLMessageActionChatEditTitle()},
                    {TLMessageActionChatEditPhoto.Signature, () => new TLMessageActionChatEditPhoto()},
                    {TLMessageActionChatDeletePhoto.Signature, () => new TLMessageActionChatDeletePhoto()},
                    {TLMessageActionChatAddUser.Signature, () => new TLMessageActionChatAddUser()},
                    {TLMessageActionChatDeleteUser.Signature, () => new TLMessageActionChatDeleteUser()},

                    {TLPhoto.Signature, () => new TLPhoto()},
                    {TLPhotoEmpty.Signature, () => new TLPhotoEmpty()},

                    {TLPhotoSize.Signature, () => new TLPhotoSize()},
                    {TLPhotoSizeEmpty.Signature, () => new TLPhotoSizeEmpty()},
                    {TLPhotoCachedSize.Signature, () => new TLPhotoCachedSize()},

                    {TLVideoEmpty.Signature, () => new TLVideoEmpty()},
                    {TLVideo.Signature, () => new TLVideo()},

                    {TLGeoPointEmpty.Signature, () => new TLGeoPointEmpty()},
                    {TLGeoPoint.Signature, () => new TLGeoPoint()},

                    {TLDialog.Signature, () => new TLDialog()},
                    {TLDialogs.Signature, () => new TLDialogs()},
                    {TLDialogsSlice.Signature, () => new TLDialogsSlice()},

                    {TLInputPeerEmpty.Signature, () => new TLInputPeerEmpty()},
                    {TLInputPeerSelf.Signature, () => new TLInputPeerSelf()},
                    {TLInputPeerContact.Signature, () => new TLInputPeerContact()},
                    {TLInputPeerForeign.Signature, () => new TLInputPeerForeign()},
                    {TLInputPeerChat.Signature, () => new TLInputPeerChat()},
                    
                    {TLPeerUser.Signature, () => new TLPeerUser()},
                    {TLPeerChat.Signature, () => new TLPeerChat()},

                    {TLUserStatusEmpty.Signature, () => new TLUserStatusEmpty()},
                    {TLUserStatusOnline.Signature, () => new TLUserStatusOnline()},
                    {TLUserStatusOffline.Signature, () => new TLUserStatusOffline()},

                    {TLChatPhotoEmpty.Signature, () => new TLChatPhotoEmpty()},
                    {TLChatPhoto.Signature, () => new TLChatPhoto()},
                    {TLUserProfilePhotoEmpty.Signature, () => new TLUserProfilePhotoEmpty()},
                    {TLUserProfilePhoto.Signature, () => new TLUserProfilePhoto()},
                    
                    {TLUserEmpty.Signature, () => new TLUserEmpty()},
                    {TLUserSelf.Signature, () => new TLUserSelf()},
                    {TLUserContact.Signature, () => new TLUserContact()},
                    {TLUserRequest.Signature, () => new TLUserRequest()},
                    {TLUserForeign.Signature, () => new TLUserForeign()},
                    {TLUserDeleted.Signature, () => new TLUserDeleted()},

                    {TLSentCode.Signature, () => new TLSentCode()},

                    {TLRPCResult.Signature, () => new TLRPCResult()},

                    {TLRPCError.Signature, () => new TLRPCError()},
                    {TLRPCReqError.Signature, () => new TLRPCReqError()},

                    {TLNewSessionCreated.Signature, () => new TLNewSessionCreated()},

                    {TLNearestDC.Signature, () => new TLNearestDC()},

                    {TLMessagesAcknowledgment.Signature, () => new TLMessagesAcknowledgment()},

                    {TLContainer.Signature, () => new TLContainer()},

                    {TLFileLocationUnavailable.Signature, () => new TLFileLocationUnavailable()},
                    {TLFileLocation.Signature, () => new TLFileLocation()},

                    {TLDCOption.Signature, () => new TLDCOption()},

                    {TLContacts.Signature, () => new TLContacts()},
                    {TLContactsNotModified.Signature, () => new TLContactsNotModified()},

                    {TLContact.Signature, () => new TLContact()},

                    {TLConfig.Signature, () => new TLConfig()},

                    {TLCheckedPhone.Signature, () => new TLCheckedPhone()},

                    {TLBadServerSalt.Signature, () => new TLBadServerSalt()},
                    {TLBadMessageNotification.Signature, () => new TLBadMessageNotification()},

                    {TLAuthorization.Signature, () => new TLAuthorization()},

                    {TLPong.Signature, () => new TLPong()},
                    {TLWallPaper.Signature, () => new TLWallPaper()},
                    {TLWallPaperSolid.Signature, () => new TLWallPaperSolid()},
                    
                    {TLSupport.Signature, () => new TLSupport()},

                    //16 layer
                    {TLSentAppCode.Signature, () => new TLSentAppCode()},

                    //17 layer
                    {TLSendMessageTypingAction.Signature, () => new TLSendMessageTypingAction()},
                    {TLSendMessageCancelAction.Signature, () => new TLSendMessageCancelAction()},
                    {TLSendMessageRecordVideoAction.Signature, () => new TLSendMessageRecordVideoAction()},
                    {TLSendMessageUploadVideoAction.Signature, () => new TLSendMessageUploadVideoAction()},
                    {TLSendMessageRecordAudioAction.Signature, () => new TLSendMessageRecordAudioAction()},
                    {TLSendMessageUploadAudioAction.Signature, () => new TLSendMessageUploadAudioAction()},
                    {TLSendMessageUploadPhotoAction.Signature, () => new TLSendMessageUploadPhotoAction()},
                    {TLSendMessageUploadDocumentAction.Signature, () => new TLSendMessageUploadDocumentAction()},
                    {TLSendMessageGeoLocationAction.Signature, () => new TLSendMessageGeoLocationAction()},
                    {TLSendMessageChooseContactAction.Signature, () => new TLSendMessageChooseContactAction()},                    
                    {TLUpdateUserTyping17.Signature, () => new TLUpdateUserTyping17()},
                    {TLUpdateChatUserTyping17.Signature, () => new TLUpdateChatUserTyping17()},                    
                    {TLMessage17.Signature, () => new TLMessage17()},
                    {TLMessageForwarded17.Signature, () => new TLMessageForwarded17()},
                    {TLMessageService17.Signature, () => new TLMessageService17()},

                    //17 layer encrypted
                    {TLDecryptedMessage17.Signature, () => new TLDecryptedMessage17()},          
                    {TLDecryptedMessageService17.Signature, () => new TLDecryptedMessageService17()},
                    {TLDecryptedMessageMediaAudio17.Signature, () => new TLDecryptedMessageMediaAudio17()},
                    {TLDecryptedMessageMediaVideo17.Signature, () => new TLDecryptedMessageMediaVideo17()},
                    {TLDecryptedMessageLayer17.Signature, () => new TLDecryptedMessageLayer17()},
                    {TLDecryptedMessageActionResend.Signature, () => new TLDecryptedMessageActionResend()},
                    {TLDecryptedMessageActionTyping.Signature, () => new TLDecryptedMessageActionTyping()},

                    //18 layer
                    {TLUpdateServiceNotification.Signature, () => new TLUpdateServiceNotification()},
                    {TLContactFound.Signature, () => new TLContactFound()},
                    {TLContactsFound.Signature, () => new TLContactsFound()},
                    {TLUserSelf18.Signature, () => new TLUserSelf18()},
                    {TLUserContact18.Signature, () => new TLUserContact18()},
                    {TLUserRequest18.Signature, () => new TLUserRequest18()},
                    {TLUserForeign18.Signature, () => new TLUserForeign18()},
                    {TLUserDeleted18.Signature, () => new TLUserDeleted18()},

                    //19 layer
                    {TLUserStatusRecently.Signature, () => new TLUserStatusRecently()},
                    {TLUserStatusLastWeek.Signature, () => new TLUserStatusLastWeek()},
                    {TLUserStatusLastMonth.Signature, () => new TLUserStatusLastMonth()},                    
                    {TLContactStatus19.Signature, () => new TLContactStatus19()},
                    {TLUpdatePrivacy.Signature, () => new TLUpdatePrivacy()},
                    {TLInputPrivacyKeyStatusTimestamp.Signature, () => new TLInputPrivacyKeyStatusTimestamp()},                   
                    {TLPrivacyKeyStatusTimestamp.Signature, () => new TLPrivacyKeyStatusTimestamp()},
                    {TLInputPrivacyValueAllowContacts.Signature, () => new TLInputPrivacyValueAllowContacts()},
                    {TLInputPrivacyValueAllowAll.Signature, () => new TLInputPrivacyValueAllowAll()},
                    {TLInputPrivacyValueAllowUsers.Signature, () => new TLInputPrivacyValueAllowUsers()},
                    {TLInputPrivacyValueDisallowContacts.Signature, () => new TLInputPrivacyValueDisallowContacts()},
                    {TLInputPrivacyValueDisallowAll.Signature, () => new TLInputPrivacyValueDisallowAll()},
                    {TLInputPrivacyValueDisallowUsers.Signature, () => new TLInputPrivacyValueDisallowUsers()},
                    {TLPrivacyValueAllowContacts.Signature, () => new TLPrivacyValueAllowContacts()},
                    {TLPrivacyValueAllowAll.Signature, () => new TLPrivacyValueAllowAll()},
                    {TLPrivacyValueAllowUsers.Signature, () => new TLPrivacyValueAllowUsers()},
                    {TLPrivacyValueDisallowContacts.Signature, () => new TLPrivacyValueDisallowContacts()},
                    {TLPrivacyValueDisallowAll.Signature, () => new TLPrivacyValueDisallowAll()},
                    {TLPrivacyValueDisallowUsers.Signature, () => new TLPrivacyValueDisallowUsers()},                   
                    {TLPrivacyRules.Signature, () => new TLPrivacyRules()},                  
                    {TLAccountDaysTTL.Signature, () => new TLAccountDaysTTL()},

                    //20 layer
                    {TLSentChangePhoneCode.Signature, () => new TLSentChangePhoneCode()},
                    {TLUpdateUserPhone.Signature, () => new TLUpdateUserPhone()},
                    
                    //20 layer encrypted
                    {TLEncryptedChat20.Signature, () => new TLEncryptedChat20()},
                    {TLDecryptedMessageActionRequestKey.Signature, () => new TLDecryptedMessageActionRequestKey()},
                    {TLDecryptedMessageActionAcceptKey.Signature, () => new TLDecryptedMessageActionAcceptKey()},
                    {TLDecryptedMessageActionAbortKey.Signature, () => new TLDecryptedMessageActionAbortKey()},
                    {TLDecryptedMessageActionCommitKey.Signature, () => new TLDecryptedMessageActionCommitKey()},
                    {TLDecryptedMessageActionNoop.Signature, () => new TLDecryptedMessageActionNoop()},
                    
                    //21 layer

                    //22 layer
                    {TLInputMediaUploadedDocument22.Signature, () => new TLInputMediaUploadedDocument22()},
                    {TLInputMediaUploadedThumbDocument22.Signature, () => new TLInputMediaUploadedThumbDocument22()},                 
                    {TLDocument22.Signature, () => new TLDocument22()},                  
                    {TLDocumentAttributeImageSize.Signature, () => new TLDocumentAttributeImageSize()},
                    {TLDocumentAttributeAnimated.Signature, () => new TLDocumentAttributeAnimated()},
                    {TLDocumentAttributeSticker.Signature, () => new TLDocumentAttributeSticker()},
                    {TLDocumentAttributeVideo.Signature, () => new TLDocumentAttributeVideo()},
                    {TLDocumentAttributeAudio.Signature, () => new TLDocumentAttributeAudio()},
                    {TLDocumentAttributeFileName.Signature, () => new TLDocumentAttributeFileName()},                  
                    {TLStickersNotModified.Signature, () => new TLStickersNotModified()},
                    {TLStickers.Signature, () => new TLStickers()},                
                    {TLStickerPack.Signature, () => new TLStickerPack()},                    
                    {TLAllStickersNotModified.Signature, () => new TLAllStickersNotModified()},
                    {TLAllStickers.Signature, () => new TLAllStickers()},

                    //23 layer
                    {TLDisabledFeature.Signature, () => new TLDisabledFeature()},
                    {TLConfig23.Signature, () => new TLConfig23()},
                    
                    //23 layer encrypted
                    {TLDecryptedMessageMediaExternalDocument.Signature, () => new TLDecryptedMessageMediaExternalDocument()},

                    //24 layer
                    {TLUpdateNewMessage24.Signature, () => new TLUpdateNewMessage24()},
                    {TLUpdateReadMessages24.Signature, () => new TLUpdateReadMessages24()},
                    {TLUpdateDeleteMessages24.Signature, () => new TLUpdateDeleteMessages24()},
                    {TLUpdatesShortMessage24.Signature, () => new TLUpdatesShortMessage24()},
                    {TLUpdatesShortChatMessage24.Signature, () => new TLUpdatesShortChatMessage24()},                    
                    {TLUpdateReadHistoryInbox.Signature, () => new TLUpdateReadHistoryInbox()},
                    {TLUpdateReadHistoryOutbox.Signature, () => new TLUpdateReadHistoryOutbox()},                  
                    {TLDialog24.Signature, () => new TLDialog24()},                    
                    {TLStatedMessages24.Signature, () => new TLStatedMessages24()},
                    {TLStatedMessagesLinks24.Signature, () => new TLStatedMessagesLinks24()},                    
                    {TLStatedMessage24.Signature, () => new TLStatedMessage24()},
                    {TLStatedMessageLink24.Signature, () => new TLStatedMessageLink24()},                    
                    {TLSentMessage24.Signature, () => new TLSentMessage24()},
                    {TLSentMessageLink24.Signature, () => new TLSentMessageLink24()},                   
                    {TLAffectedMessages.Signature, () => new TLAffectedMessages()},
                    {TLAffectedHistory24.Signature, () => new TLAffectedHistory24()},       
                    {TLMessageMediaUnsupported24.Signature, () => new TLMessageMediaUnsupported24()},                 
                    {TLChats24.Signature, () => new TLChats24()},                
                    {TLUserSelf24.Signature, () => new TLUserSelf24()},                 
                    {TLCheckedPhone24.Signature, () => new TLCheckedPhone24()},                   
                    {TLContactLinkUnknown.Signature, () => new TLContactLinkUnknown()},
                    {TLContactLinkNone.Signature, () => new TLContactLinkNone()},
                    {TLContactLinkHasPhone.Signature, () => new TLContactLinkHasPhone()},
                    {TLContactLink.Signature, () => new TLContactLink()},                   
                    {TLUpdateContactLink24.Signature, () => new TLUpdateContactLink24()},
                    {TLLink24.Signature, () => new TLLink24()},
                    {TLConfig24.Signature, () => new TLConfig24()},

                    //25 layer
                    {TLMessage25.Signature, () => new TLMessage25()},
                    {TLDocumentAttributeSticker25.Signature, () => new TLDocumentAttributeSticker25()},
                    {TLUpdatesShortMessage25.Signature, () => new TLUpdatesShortMessage25()},
                    {TLUpdatesShortChatMessage25.Signature, () => new TLUpdatesShortChatMessage25()},

                    //26 layer
                    {TLSentMessage26.Signature, () => new TLSentMessage26()},
                    {TLSentMessageLink26.Signature, () => new TLSentMessageLink26()},
                    {TLConfig26.Signature, () => new TLConfig26()},
                    {TLUpdateWebPage.Signature, () => new TLUpdateWebPage()},
                    {TLWebPageEmpty.Signature, () => new TLWebPageEmpty()},
                    {TLWebPagePending.Signature, () => new TLWebPagePending()},
                    {TLWebPage.Signature, () => new TLWebPage()},
                    {TLMessageMediaWebPage.Signature, () => new TLMessageMediaWebPage()},
                    {TLAccountAuthorization.Signature, () => new TLAccountAuthorization()},
                    {TLAccountAuthorizations.Signature, () => new TLAccountAuthorizations()},

                    //27 layer
                    {TLPassword.Signature, () => new TLPassword()},
                    {TLNoPassword.Signature, () => new TLNoPassword()},
                    {TLPasswordSettings.Signature, () => new TLPasswordSettings()},
                    {TLPasswordInputSettings.Signature, () => new TLPasswordInputSettings()},
                    {TLPasswordRecovery.Signature, () => new TLPasswordRecovery()},

                    //layer 28
                    {TLInputMediaUploadedPhoto28.Signature, () => new TLInputMediaUploadedPhoto28()},
                    {TLInputMediaPhoto28.Signature, () => new TLInputMediaPhoto28()},
                    {TLInputMediaUploadedVideo28.Signature, () => new TLInputMediaUploadedVideo28()},
                    {TLInputMediaUploadedThumbVideo28.Signature, () => new TLInputMediaUploadedThumbVideo28()},
                    {TLInputMediaVideo28.Signature, () => new TLInputMediaVideo28()},
                    {TLSendMessageUploadVideoAction28.Signature, () => new TLSendMessageUploadVideoAction28()},
                    {TLSendMessageUploadAudioAction28.Signature, () => new TLSendMessageUploadAudioAction28()},
                    {TLSendMessageUploadPhotoAction28.Signature, () => new TLSendMessageUploadPhotoAction28()},
                    {TLSendMessageUploadDocumentAction28.Signature, () => new TLSendMessageUploadDocumentAction28()},
                    {TLInputMediaVenue.Signature, () => new TLInputMediaVenue()},
                    {TLMessageMediaVenue.Signature, () => new TLMessageMediaVenue()},
                    {TLChatInviteEmpty.Signature, () => new TLChatInviteEmpty()},
                    {TLChatInviteExported.Signature, () => new TLChatInviteExported()},
                    {TLChatInviteAlready.Signature, () => new TLChatInviteAlready()},
                    {TLChatInvite.Signature, () => new TLChatInvite()},
                    {TLUpdateReadMessagesContents.Signature, () => new TLUpdateReadMessagesContents()},
                    {TLConfig28.Signature, () => new TLConfig28()},
                    {TLChatFull28.Signature, () => new TLChatFull28()},
                    {TLReceivedNotifyMessage.Signature, () => new TLReceivedNotifyMessage()},
                    {TLMessageActionChatJoinedByLink.Signature, () => new TLMessageActionChatJoinedByLink()},
                    {TLPhoto28.Signature, () => new TLPhoto28()},
                    {TLVideo28.Signature, () => new TLVideo28()},
                    {TLMessageMediaPhoto28.Signature, () => new TLMessageMediaPhoto28()},
                    {TLMessageMediaVideo28.Signature, () => new TLMessageMediaVideo28()},

                    //layer 29
                    {TLDocumentAttributeSticker29.Signature, () => new TLDocumentAttributeSticker29()},
                    {TLAllStickers29.Signature, () => new TLAllStickers29()},
                    {TLInputStickerSetEmpty.Signature, () => new TLInputStickerSetEmpty()},
                    {TLInputStickerSetId.Signature, () => new TLInputStickerSetId()},
                    {TLInputStickerSetShortName.Signature, () => new TLInputStickerSetShortName()},
                    {TLStickerSet.Signature, () => new TLStickerSet()},
                    {TLMessagesStickerSet.Signature, () => new TLMessagesStickerSet()},
                    
                    //layer 30
                    {TLDCOption30.Signature, () => new TLDCOption30()},

                    //layer 31
                    {TLChatFull31.Signature, () => new TLChatFull31()},
                    {TLMessage31.Signature, () => new TLMessage31()},
                    {TLAuthorization31.Signature, () => new TLAuthorization31()},
                    {TLUserFull31.Signature, () => new TLUserFull31()},
                    {TLUser.Signature, () => new TLUser()},
                    {TLBotCommand.Signature, () => new TLBotCommand()},
                    {TLBotInfoEmpty.Signature, () => new TLBotInfoEmpty()},
                    {TLBotInfo.Signature, () => new TLBotInfo()},
                    {TLKeyboardButton.Signature, () => new TLKeyboardButton()},
                    {TLKeyboardButtonRow.Signature, () => new TLKeyboardButtonRow()},
                    {TLReplyKeyboardMarkup.Signature, () => new TLReplyKeyboardMarkup()},
                    {TLReplyKeyboardHide.Signature, () => new TLReplyKeyboardHide()},
                    {TLReplyKeyboardForceReply.Signature, () => new TLReplyKeyboardForceReply()},

                    //layer 32
                    {TLAllStickers32.Signature, () => new TLAllStickers32()},
                    {TLStickerSet32.Signature, () => new TLStickerSet32()},
                    {TLDocumentAttributeAudio32.Signature, () => new TLDocumentAttributeAudio32()},

                    //layer 33
                    {TLInputPeerUser.Signature, () => new TLInputPeerUser()},
                    {TLInputUser.Signature, () => new TLInputUser()},
                    {TLPhoto33.Signature, () => new TLPhoto33()},
                    {TLVideo33.Signature, () => new TLVideo33()},
                    {TLAudio33.Signature, () => new TLAudio33()},
                    {TLAppChangelogEmpty.Signature, () => new TLAppChangelogEmpty()},
                    {TLAppChangelog.Signature, () => new TLAppChangelog()},

                    //layer 34
                    {TLMessageEntityUnknown.Signature, () => new TLMessageEntityUnknown()},
                    {TLMessageEntityMention.Signature, () => new TLMessageEntityMention()},
                    {TLMessageEntityHashtag.Signature, () => new TLMessageEntityHashtag()},
                    {TLMessageEntityBotCommand.Signature, () => new TLMessageEntityBotCommand()},
                    {TLMessageEntityUrl.Signature, () => new TLMessageEntityUrl()},
                    {TLMessageEntityEmail.Signature, () => new TLMessageEntityEmail()},
                    {TLMessageEntityBold.Signature, () => new TLMessageEntityBold()},
                    {TLMessageEntityItalic.Signature, () => new TLMessageEntityItalic()},
                    {TLMessageEntityCode.Signature, () => new TLMessageEntityCode()},
                    {TLMessageEntityPre.Signature, () => new TLMessageEntityPre()},
                    {TLMessageEntityTextUrl.Signature, () => new TLMessageEntityTextUrl()},
                    {TLMessage34.Signature, () => new TLMessage34()},
                    {TLSentMessage34.Signature, () => new TLSentMessage34()},
                    {TLUpdatesShortMessage34.Signature, () => new TLUpdatesShortMessage34()},
                    {TLUpdatesShortChatMessage34.Signature, () => new TLUpdatesShortChatMessage34()},

                    //layer 35
                    {TLWebPage35.Signature, () => new TLWebPage35()},

                    //layer 36
                    {TLInputMediaUploadedVideo36.Signature, () => new TLInputMediaUploadedVideo36()},
                    {TLInputMediaUploadedThumbVideo36.Signature, () => new TLInputMediaUploadedThumbVideo36()},
                    {TLMessage36.Signature, () => new TLMessage36()},
                    {TLUpdatesShortSentMessage.Signature, () => new TLUpdatesShortSentMessage()},

                    //layer 37
                    {TLChatParticipantsForbidden37.Signature, () => new TLChatParticipantsForbidden37()},
                    {TLUpdateChatParticipantAdd37.Signature, () => new TLUpdateChatParticipantAdd37()},
                    {TLUpdateWebPage37.Signature, () => new TLUpdateWebPage37()},

                    //layer 40
                    {TLInputPeerChannel.Signature, () => new TLInputPeerChannel()},
                    {TLPeerChannel.Signature, () => new TLPeerChannel()},
                    {TLChat40.Signature, () => new TLChat40()},
                    {TLChatForbidden40.Signature, () => new TLChatForbidden40()},
                    {TLChannel.Signature, () => new TLChannel()},
                    {TLChannelForbidden.Signature, () => new TLChannelForbidden()},
                    {TLChannelFull.Signature, () => new TLChannelFull()},
                    {TLChannelParticipants40.Signature, () => new TLChannelParticipants40()},
                    {TLMessage40.Signature, () => new TLMessage40()},
                    {TLMessageService40.Signature, () => new TLMessageService40()},
                    {TLMessageActionChannelCreate.Signature, () => new TLMessageActionChannelCreate()},
                    {TLDialogChannel.Signature, () => new TLDialogChannel()},
                    {TLChannelMessages.Signature, () => new TLChannelMessages()},
                    {TLUpdateChannelTooLong.Signature, () => new TLUpdateChannelTooLong()},
                    {TLUpdateChannel.Signature, () => new TLUpdateChannel()},
                    {TLUpdateChannelGroup.Signature, () => new TLUpdateChannelGroup()},
                    {TLUpdateNewChannelMessage.Signature, () => new TLUpdateNewChannelMessage()},
                    {TLUpdateReadChannelInbox.Signature, () => new TLUpdateReadChannelInbox()},
                    {TLUpdateDeleteChannelMessages.Signature, () => new TLUpdateDeleteChannelMessages()},
                    {TLUpdateChannelMessageViews.Signature, () => new TLUpdateChannelMessageViews()},                
                    {TLUpdatesShortMessage40.Signature, () => new TLUpdatesShortMessage40()},
                    {TLUpdatesShortChatMessage40.Signature, () => new TLUpdatesShortChatMessage40()},
                    {TLContactsFound40.Signature, () => new TLContactsFound40()},
                    //{TLInputChatEmpty.Signature, () => new TLInputChatEmpty()},     // delete
                    //{TLInputChat.Signature, () => new TLInputChat()},   // delete
                    {TLInputChannel.Signature, () => new TLInputChannel()}, 
                    {TLInputChannelEmpty.Signature, () => new TLInputChannelEmpty()},
                    {TLMessageRange.Signature, () => new TLMessageRange()},
                    {TLMessageGroup.Signature, () => new TLMessageGroup()},
                    {TLChannelDifferenceEmpty.Signature, () => new TLChannelDifferenceEmpty()},
                    {TLChannelDifferenceTooLong.Signature, () => new TLChannelDifferenceTooLong()},
                    {TLChannelDifference.Signature, () => new TLChannelDifference()},
                    {TLChannelMessagesFilterEmpty.Signature, () => new TLChannelMessagesFilterEmpty()},
                    {TLChannelMessagesFilter.Signature, () => new TLChannelMessagesFilter()},
                    {TLChannelMessagesFilterCollapsed.Signature, () => new TLChannelMessagesFilterCollapsed()},
                    {TLResolvedPeer.Signature, () => new TLResolvedPeer()},
                    {TLChannelParticipant.Signature, () => new TLChannelParticipant()},
                    {TLChannelParticipantSelf.Signature, () => new TLChannelParticipantSelf()},
                    {TLChannelParticipantModerator.Signature, () => new TLChannelParticipantModerator()},
                    {TLChannelParticipantEditor.Signature, () => new TLChannelParticipantEditor()},
                    {TLChannelParticipantKicked.Signature, () => new TLChannelParticipantKicked()},
                    {TLChannelParticipantCreator.Signature, () => new TLChannelParticipantCreator()},
                    {TLChannelParticipantsRecent.Signature, () => new TLChannelParticipantsRecent()},
                    {TLChannelParticipantsAdmins.Signature, () => new TLChannelParticipantsAdmins()},
                    {TLChannelParticipantsKicked.Signature, () => new TLChannelParticipantsKicked()},        
                    {TLChannelRoleEmpty.Signature, () => new TLChannelRoleEmpty()},
                    {TLChannelRoleModerator.Signature, () => new TLChannelRoleModerator()},
                    {TLChannelRoleEditor.Signature, () => new TLChannelRoleEditor()},
                    {TLChannelParticipants.Signature, () => new TLChannelParticipants()},
                    {TLChannelsChannelParticipant.Signature, () => new TLChannelsChannelParticipant()},
                    {TLChatInvite40.Signature, () => new TLChatInvite40()},
                    
                    {TLChatParticipantCreator.Signature, () => new TLChatParticipantCreator()},
                    {TLChatParticipantAdmin.Signature, () => new TLChatParticipantAdmin()},
                    {TLChatParticipants40.Signature, () => new TLChatParticipants40()},
                    {TLUpdateChatAdmins.Signature, () => new TLUpdateChatAdmins()},
                    {TLUpdateChatParticipantAdmin.Signature, () => new TLUpdateChatParticipantAdmin()},

                    // layer 41
                    {TLConfig41.Signature, () => new TLConfig41()},
                    
                    {TLMessageActionChatMigrateTo.Signature, () => new TLMessageActionChatMigrateTo()},
                    {TLMessageActionChatDeactivate.Signature, () => new TLMessageActionChatDeactivate()},
                    {TLMessageActionChatActivate.Signature, () => new TLMessageActionChatActivate()},
                    {TLMessageActionChannelMigrateFrom.Signature, () => new TLMessageActionChannelMigrateFrom()},

                    {TLChannelParticipantsBots.Signature, () => new TLChannelParticipantsBots()},
                    {TLChat41.Signature, () => new TLChat41()},
                    {TLChannelFull41.Signature, () => new TLChannelFull41()},
                    {TLMessageActionChatAddUser41.Signature, () => new TLMessageActionChatAddUser41()},

                    // layer 42
                    {TLTermsOfService.Signature, () => new TLTermsOfService()},
                    
                    {TLInputReportReasonSpam.Signature, () => new TLInputReportReasonSpam()},
                    {TLInputReportReasonViolence.Signature, () => new TLInputReportReasonViolence()},
                    {TLInputReportReasonPornography.Signature, () => new TLInputReportReasonPornography()},
                    {TLInputReportReasonOther.Signature, () => new TLInputReportReasonOther()},
                    
                    // layer 43
                    {TLUpdateNewStickerSet.Signature, () => new TLUpdateNewStickerSet()},
                    {TLUpdateStickerSetsOrder.Signature, () => new TLUpdateStickerSetsOrder()},
                    {TLUpdateStickerSets.Signature, () => new TLUpdateStickerSets()},
                    {TLAllStickers43.Signature, () => new TLAllStickers43()},

                    // layer 44
                    {TLInputMediaGifExternal.Signature, () => new TLInputMediaGifExternal()},
                    {TLUser44.Signature, () => new TLUser44()},
                    {TLChannel44.Signature, () => new TLChannel44()},
                    {TLInputMessagesFilterGif.Signature, () => new TLInputMessagesFilterGif()},
                    {TLUpdateSavedGifs.Signature, () => new TLUpdateSavedGifs()},
                    {TLConfig44.Signature, () => new TLConfig44()},
                    {TLFoundGif.Signature, () => new TLFoundGif()},
                    {TLFoundGifCached.Signature, () => new TLFoundGifCached()},
                    {TLFoundGifs.Signature, () => new TLFoundGifs()},
                    {TLSavedGifsNotModified.Signature, () => new TLSavedGifsNotModified()},
                    {TLSavedGifs.Signature, () => new TLSavedGifs()},
                    
                    // layer 45
                    {TLInputMediaUploadedDocument45.Signature, () => new TLInputMediaUploadedDocument45()},
                    {TLInputMediaUploadedThumbDocument45.Signature, () => new TLInputMediaUploadedThumbDocument45()},
                    {TLInputMediaDocument45.Signature, () => new TLInputMediaDocument45()},
                    {TLUser45.Signature, () => new TLUser45()},
                    {TLMessage45.Signature, () => new TLMessage45()},
                    {TLMessageMediaDocument45.Signature, () => new TLMessageMediaDocument45()},
                    {TLUpdateBotInlineQuery.Signature, () => new TLUpdateBotInlineQuery()},
                    {TLUpdatesShortMessage45.Signature, () => new TLUpdatesShortMessage45()},
                    {TLUpdatesShortChatMessage45.Signature, () => new TLUpdatesShortChatMessage45()},

                    {TLInputBotInlineMessageMediaAuto.Signature, () => new TLInputBotInlineMessageMediaAuto()},
                    {TLInputBotInlineMessageText.Signature, () => new TLInputBotInlineMessageText()},
                    {TLInputBotInlineResult.Signature, () => new TLInputBotInlineResult()},
                    {TLBotInlineMessageMediaAuto.Signature, () => new TLBotInlineMessageMediaAuto()},
                    {TLBotInlineMessageText.Signature, () => new TLBotInlineMessageText()},
                    //{TLBotInlineMediaResultDocument.Signature, () => new TLBotInlineMediaResultDocument()},
                    {TLBotInlineMediaResultPhoto.Signature, () => new TLBotInlineMediaResultPhoto()},
                    {TLBotInlineResult.Signature, () => new TLBotInlineResult()},
                    {TLBotResults.Signature, () => new TLBotResults()},
                    
                    // layer 46
                    {TLDocumentAttributeAudio46.Signature, () => new TLDocumentAttributeAudio46()},
                    {TLInputMessagesFilterVoice.Signature, () => new TLInputMessagesFilterVoice()},
                    {TLInputMessagesFilterMusic.Signature, () => new TLInputMessagesFilterMusic()},
                    {TLInputPrivacyKeyChatInvite.Signature, () => new TLInputPrivacyKeyChatInvite()},
                    {TLPrivacyKeyChatInvite.Signature, () => new TLPrivacyKeyChatInvite()},

                    // layer 48
                    {TLMessage48.Signature, () => new TLMessage48()},
                    {TLInputPeerNotifySettings48.Signature, () => new TLInputPeerNotifySettings48()},
                    {TLPeerNotifySettings48.Signature, () => new TLPeerNotifySettings48()},
                    {TLUpdateEditChannelMessage.Signature, () => new TLUpdateEditChannelMessage()},
                    {TLUpdatesShortMessage48.Signature, () => new TLUpdatesShortMessage48()},
                    {TLUpdatesShortChatMessage48.Signature, () => new TLUpdatesShortChatMessage48()},
                    {TLConfig48.Signature, () => new TLConfig48()},
                    {TLExportedMessageLink.Signature, () => new TLExportedMessageLink()},
                    {TLMessageFwdHeader.Signature, () => new TLMessageFwdHeader()},
                    {TLMessageEditData.Signature, () => new TLMessageEditData()},

                    // layer 49
                    {TLChannel49.Signature, () => new TLChannel49()},
                    {TLChannelFull49.Signature, () => new TLChannelFull49()},
                    {TLMessageService49.Signature, () => new TLMessageService49()},
                    {TLMessageActionPinMessage.Signature, () => new TLMessageActionPinMessage()},
                    {TLPeerSettings.Signature, () => new TLPeerSettings()},
                    {TLUserFull49.Signature, () => new TLUserFull49()},
                    {TLUpdateChannelTooLong49.Signature, () => new TLUpdateChannelTooLong49()},
                    {TLUpdateChannelPinnedMessage.Signature, () => new TLUpdateChannelPinnedMessage()},
                    {TLBotInfo49.Signature, () => new TLBotInfo49()},

                    // layer 50
                    {TLSentCode50.Signature, () => new TLSentCode50()},
                    {TLCodeTypeSms.Signature, () => new TLCodeTypeSms()},
                    {TLCodeTypeCall.Signature, () => new TLCodeTypeCall()},
                    {TLCodeTypeFlashCall.Signature, () => new TLCodeTypeFlashCall()},
                    {TLSentCodeTypeApp.Signature, () => new TLSentCodeTypeApp()},
                    {TLSentCodeTypeSms.Signature, () => new TLSentCodeTypeSms()},
                    {TLSentCodeTypeCall.Signature, () => new TLSentCodeTypeCall()},
                    {TLSentCodeTypeFlashCall.Signature, () => new TLSentCodeTypeFlashCall()},

                    // layer 51
                    {TLUpdateBotCallbackQuery.Signature, () => new TLUpdateBotCallbackQuery()},
                    {TLUpdateInlineBotCallbackQuery.Signature, () => new TLUpdateInlineBotCallbackQuery()},
                    {TLUpdateBotInlineQuery51.Signature, () => new TLUpdateBotInlineQuery51()},
                    {TLUpdateBotInlineSend.Signature, () => new TLUpdateBotInlineSend()},
                    {TLUpdateEditMessage.Signature, () => new TLUpdateEditMessage()},

                    {TLKeyboardButtonUrl.Signature, () => new TLKeyboardButtonUrl()},
                    {TLKeyboardButtonCallback.Signature, () => new TLKeyboardButtonCallback()},
                    {TLKeyboardButtonRequestPhone.Signature, () => new TLKeyboardButtonRequestPhone()},
                    {TLKeyboardButtonRequestGeoLocation.Signature, () => new TLKeyboardButtonRequestGeoLocation()},
                    {TLKeyboardButtonSwitchInline.Signature, () => new TLKeyboardButtonSwitchInline()},

                    {TLBotCallbackAnswer.Signature, () => new TLBotCallbackAnswer()},
                    {TLReplyInlineMarkup.Signature, () => new TLReplyInlineMarkup()},

                    {TLInputBotInlineMessageMediaAuto51.Signature, () => new TLInputBotInlineMessageMediaAuto51()},
                    {TLInputBotInlineMessageText51.Signature, () => new TLInputBotInlineMessageText51()},
                    {TLInputBotInlineMessageMediaGeo.Signature, () => new TLInputBotInlineMessageMediaGeo()},
                    {TLInputBotInlineMessageMediaVenue.Signature, () => new TLInputBotInlineMessageMediaVenue()},
                    {TLInputBotInlineMessageMediaContact.Signature, () => new TLInputBotInlineMessageMediaContact()},
                    
                    {TLInputBotInlineResultPhoto.Signature, () => new TLInputBotInlineResultPhoto()},
                    {TLInputBotInlineResultDocument.Signature, () => new TLInputBotInlineResultDocument()},
                    
                    {TLBotInlineMessageMediaAuto51.Signature, () => new TLBotInlineMessageMediaAuto51()},
                    {TLBotInlineMessageText51.Signature, () => new TLBotInlineMessageText51()},              
                    {TLBotInlineMessageMediaGeo.Signature, () => new TLBotInlineMessageMediaGeo()},
                    {TLBotInlineMessageMediaVenue.Signature, () => new TLBotInlineMessageMediaVenue()},
                    {TLBotInlineMessageMediaContact.Signature, () => new TLBotInlineMessageMediaContact()},
                    
                    {TLBotInlineMediaResult.Signature, () => new TLBotInlineMediaResult()},
                    {TLInputBotInlineMessageId.Signature, () => new TLInputBotInlineMessageId()},
                    
                    {TLBotResults51.Signature, () => new TLBotResults51()},
                    {TLInlineBotSwitchPM.Signature, () => new TLInlineBotSwitchPM()},

                    // layer 52
                    {TLConfig52.Signature, () => new TLConfig52()},
                    {TLMessageEntityMentionName.Signature, () => new TLMessageEntityMentionName()},
                    {TLInputMessageEntityMentionName.Signature, () => new TLInputMessageEntityMentionName()},
                    {TLPeerDialogs.Signature, () => new TLPeerDialogs()},
                    {TLTopPeer.Signature, () => new TLTopPeer()},
                    {TLTopPeerCategoryBotsPM.Signature, () => new TLTopPeerCategoryBotsPM()},
                    {TLTopPeerCategoryBotsInline.Signature, () => new TLTopPeerCategoryBotsInline()},
                    {TLTopPeerCategoryCorrespondents.Signature, () => new TLTopPeerCategoryCorrespondents()},
                    {TLTopPeerCategoryGroups.Signature, () => new TLTopPeerCategoryGroups()},
                    {TLTopPeerCategoryChannels.Signature, () => new TLTopPeerCategoryChannels()},
                    {TLTopPeerCategoryPeers.Signature, () => new TLTopPeerCategoryPeers()},
                    {TLTopPeersNotModified.Signature, () => new TLTopPeersNotModified()},
                    {TLTopPeers.Signature, () => new TLTopPeers()},

                    // layer 53
                    {TLChannelFull53.Signature, () => new TLChannelFull53()},
                    {TLDialog53.Signature, () => new TLDialog53()},
                    {TLChannelMessages53.Signature, () => new TLChannelMessages53()},
                    {TLUpdateDraftMessage.Signature, () => new TLUpdateDraftMessage()},
                    {TLChannelDifferenceTooLong53.Signature, () => new TLChannelDifferenceTooLong53()},
                    {TLInputMessagesFilterChatPhotos.Signature, () => new TLInputMessagesFilterChatPhotos()},
                    {TLUpdateReadChannelOutbox.Signature, () => new TLUpdateReadChannelOutbox()},
                    {TLDraftMessageEmpty.Signature, () => new TLDraftMessageEmpty()},
                    {TLDraftMessage.Signature, () => new TLDraftMessage()},
                    {TLChannelForbidden53.Signature, () => new TLChannelForbidden53()},
                    {TLMessageActionClearHistory.Signature, () => new TLMessageActionClearHistory()},

                    // layer 54
                    {TLConfig54.Signature, () => new TLConfig54()},
                    {TLFeaturedStickersNotModified.Signature, () => new TLFeaturedStickersNotModified()},
                    {TLFeaturedStickers.Signature, () => new TLFeaturedStickers()},
                    {TLUpdateReadFeaturedStickers.Signature, () => new TLUpdateReadFeaturedStickers()},
                    {TLBotCallbackAnswer54.Signature, () => new TLBotCallbackAnswer54()},
                    {TLDocument54.Signature, () => new TLDocument54()},
                    {TLInputDocumentFileLocation54.Signature, () => new TLInputDocumentFileLocation54()},
                    {TLRecentStickersNotModified.Signature, () => new TLRecentStickersNotModified()},
                    {TLRecentStickers.Signature, () => new TLRecentStickers()},
                    {TLUpdateRecentStickers.Signature, () => new TLUpdateRecentStickers()},
                    {TLChatInvite54.Signature, () => new TLChatInvite54()},
                    {TLStickerSetInstallResult.Signature, () => new TLStickerSetInstallResult()},
                    {TLStickerSetInstallResultArchive.Signature, () => new TLStickerSetInstallResultArchive()},
                    {TLArchivedStickers.Signature, () => new TLArchivedStickers()},
                    {TLStickerSetCovered.Signature, () => new TLStickerSetCovered()},

                    // layer 55
                    {TLInputMediaPhotoExternal.Signature, () => new TLInputMediaPhotoExternal()},
                    {TLInputMediaDocumentExternal.Signature, () => new TLInputMediaDocumentExternal()},
                    {TLAuthorization55.Signature, () => new TLAuthorization55()},
                    {TLUpdateConfig.Signature, () => new TLUpdateConfig()},
                    {TLUpdatePtsChanged.Signature, () => new TLUpdatePtsChanged()},                   
                    {TLConfig55.Signature, () => new TLConfig55()},
                    {TLKeyboardButtonSwitchInline55.Signature, () => new TLKeyboardButtonSwitchInline55()},
                    
                    // layer 56
                    {TLUpdateBotCallbackQuery56.Signature, () => new TLUpdateBotCallbackQuery56()},
                    {TLUpdateInlineBotCallbackQuery56.Signature, () => new TLUpdateInlineBotCallbackQuery56()},               
                    {TLUpdateStickerSetsOrder56.Signature, () => new TLUpdateStickerSetsOrder56()},
                    {TLStickerSetMultiCovered.Signature, () => new TLStickerSetMultiCovered()},                   
                    {TLInputMediaUploadedPhoto56.Signature, () => new TLInputMediaUploadedPhoto56()},
                    {TLInputMediaUploadedDocument56.Signature, () => new TLInputMediaUploadedDocument56()},
                    {TLInputMediaUploadedThumbDocument56.Signature, () => new TLInputMediaUploadedThumbDocument56()},
                    {TLInputStickeredMediaPhoto.Signature, () => new TLInputStickeredMediaPhoto()},
                    {TLInputStickeredMediaDocument.Signature, () => new TLInputStickeredMediaDocument()},
                    {TLPhoto56.Signature, () => new TLPhoto56()},
                    {TLMaskCoords.Signature, () => new TLMaskCoords()},
                    {TLDocumentAttributeSticker56.Signature, () => new TLDocumentAttributeSticker56()},
                    {TLDocumentAttributeHasStickers.Signature, () => new TLDocumentAttributeHasStickers()},

                    // layer 57
                    {TLInputMediaGame.Signature, () => new TLInputMediaGame()},
                    {TLInputGameId.Signature, () => new TLInputGameId()},
                    {TLInputGameShortName.Signature, () => new TLInputGameShortName()},
                    {TLGame.Signature, () => new TLGame()},
                    {TLHighScore.Signature, () => new TLHighScore()},
                    {TLHighScores.Signature, () => new TLHighScores()},
                    {TLInputBotInlineMessageGame.Signature, () => new TLInputBotInlineMessageGame()},
                    {TLInputBotInlineResultGame.Signature, () => new TLInputBotInlineResultGame()},
                    {TLKeyboardButtonGame.Signature, () => new TLKeyboardButtonGame()},
                    {TLMessageActionGameScore.Signature, () => new TLMessageActionGameScore()},
                    {TLMessageMediaGame.Signature, () => new TLMessageMediaGame()},
                    
                    // layer 58
                    {TLUserFull58.Signature, () => new TLUserFull58()},
                    {TLChatsSlice.Signature, () => new TLChatsSlice()},
                    {TLUpdateChannelWebPage.Signature, () => new TLUpdateChannelWebPage()},
                    {TLDifferenceTooLong.Signature, () => new TLDifferenceTooLong()},
                    {TLBotResults58.Signature, () => new TLBotResults58()},
                    {TLBotCallbackAnswer58.Signature, () => new TLBotCallbackAnswer58()},

                    // layer 59
                    {TLChatsSlice59.Signature, () => new TLChatsSlice59()},
                    {TLUpdateServiceNotification59.Signature, () => new TLUpdateServiceNotification59()},
                    {TLWebPage59.Signature, () => new TLWebPage59()},
                    {TLWebPageNotModified.Signature, () => new TLWebPageNotModified()},
                    {TLAppChangelog59.Signature, () => new TLAppChangelog59()},
                    
                    {TLTextEmpty.Signature, () => new TLTextEmpty()},
                    {TLTextPlain.Signature, () => new TLTextPlain()},
                    {TLTextBold.Signature, () => new TLTextBold()},
                    {TLTextItalic.Signature, () => new TLTextItalic()},
                    {TLTextUnderline.Signature, () => new TLTextUnderline()},
                    {TLTextStrike.Signature, () => new TLTextStrike()},
                    {TLTextFixed.Signature, () => new TLTextFixed()},
                    {TLTextUrl.Signature, () => new TLTextUrl()},
                    {TLTextEmail.Signature, () => new TLTextEmail()},
                    {TLTextConcat.Signature, () => new TLTextConcat()},

                    {TLPageBlockUnsupported.Signature, () => new TLPageBlockUnsupported()},
                    {TLPageBlockTitle.Signature, () => new TLPageBlockTitle()},
                    {TLPageBlockSubtitle.Signature, () => new TLPageBlockSubtitle()},
                    {TLPageBlockAuthorDate.Signature, () => new TLPageBlockAuthorDate()},
                    {TLPageBlockHeader.Signature, () => new TLPageBlockHeader()},
                    {TLPageBlockSubheader.Signature, () => new TLPageBlockSubheader()},
                    {TLPageBlockParagraph.Signature, () => new TLPageBlockParagraph()},
                    {TLPageBlockPreformatted.Signature, () => new TLPageBlockPreformatted()},
                    {TLPageBlockFooter.Signature, () => new TLPageBlockFooter()},
                    {TLPageBlockDivider.Signature, () => new TLPageBlockDivider()},
                    {TLPageBlockAnchor.Signature, () => new TLPageBlockAnchor()},
                    {TLPageBlockList.Signature, () => new TLPageBlockList()},
                    {TLPageBlockBlockquote.Signature, () => new TLPageBlockBlockquote()},
                    {TLPageBlockPullquote.Signature, () => new TLPageBlockPullquote()},
                    {TLPageBlockPhoto.Signature, () => new TLPageBlockPhoto()},
                    {TLPageBlockVideo.Signature, () => new TLPageBlockVideo()},
                    {TLPageBlockCover.Signature, () => new TLPageBlockCover()},
                    {TLPageBlockEmbed.Signature, () => new TLPageBlockEmbed()},
                    {TLPageBlockEmbedPost.Signature, () => new TLPageBlockEmbedPost()},
                    {TLPageBlockCollage.Signature, () => new TLPageBlockCollage()},
                    {TLPageBlockSlideshow.Signature, () => new TLPageBlockSlideshow()},
                    
                    {TLPagePart.Signature, () => new TLPagePart()},
                    {TLPageFull.Signature, () => new TLPageFull()},

                    // layer 60
                    {TLUpdatePhoneCall.Signature, () => new TLUpdatePhoneCall()},
                    {TLConfig60.Signature, () => new TLConfig60()},
                    {TLSendMessageGamePlayAction.Signature, () => new TLSendMessageGamePlayAction()},
                    {TLInputPrivacyKeyPhoneCall.Signature, () => new TLInputPrivacyKeyPhoneCall()},
                    {TLPrivacyKeyPhoneCall.Signature, () => new TLPrivacyKeyPhoneCall()},
                    {TLInputPhoneCall.Signature, () => new TLInputPhoneCall()},
                    {TLPhoneCallEmpty.Signature, () => new TLPhoneCallEmpty()},
                    {TLPhoneCallWaiting.Signature, () => new TLPhoneCallWaiting()},
                    {TLPhoneCallRequested.Signature, () => new TLPhoneCallRequested()},
                    {TLPhoneCall.Signature, () => new TLPhoneCall()},
                    {TLPhoneCallDiscarded.Signature, () => new TLPhoneCallDiscarded()},
                    {TLPhoneConnection.Signature, () => new TLPhoneConnection()},
                    {TLPhoneCallProtocol.Signature, () => new TLPhoneCallProtocol()},
                    {TLPhonePhoneCall.Signature, () => new TLPhonePhoneCall()},
                    
                    // layer 61
                    {TLUpdateDialogPinned.Signature, () => new TLUpdateDialogPinned()},
                    {TLUpdatePinnedDialogs.Signature, () => new TLUpdatePinnedDialogs()},
                    {TLConfig61.Signature, () => new TLConfig61()},
                    {TLPageBlockAuthorDate61.Signature, () => new TLPageBlockAuthorDate61()},
                    {TLPageBlockEmbed61.Signature, () => new TLPageBlockEmbed61()},
                    {TLPhoneCallDiscarded61.Signature, () => new TLPhoneCallDiscarded61()},
                    {TLPhoneConnection61.Signature, () => new TLPhoneConnection61()},
                    {TLPhoneCallDiscardReasonMissed.Signature, () => new TLPhoneCallDiscardReasonMissed()},
                    {TLPhoneCallDiscardReasonDisconnect.Signature, () => new TLPhoneCallDiscardReasonDisconnect()},
                    {TLPhoneCallDiscardReasonHangup.Signature, () => new TLPhoneCallDiscardReasonHangup()},
                    {TLPhoneCallDiscardReasonBusy.Signature, () => new TLPhoneCallDiscardReasonBusy()},
                    
                    // layer 62
                    {TLMessageActionPhoneCall.Signature, () => new TLMessageActionPhoneCall()},
                    {TLInputMessagesFilterPhoneCalls.Signature, () => new TLInputMessagesFilterPhoneCalls()},
                    {TLUpdateBotWebhookJSON.Signature, () => new TLUpdateBotWebhookJSON()},
                    {TLUpdateBotWebhookJSONQuery.Signature, () => new TLUpdateBotWebhookJSONQuery()},
                    {TLDataJSON.Signature, () => new TLDataJSON()},

                    // layer 63
                    {TLConfig63.Signature, () => new TLConfig63()},

                    // layer 64
                    {TLInputMediaInvoice.Signature, () => new TLInputMediaInvoice()},
                    {TLMessageMediaInvoice.Signature, () => new TLMessageMediaInvoice()},
                    {TLMessageActionPaymentSentMe.Signature, () => new TLMessageActionPaymentSentMe()},
                    {TLMessageActionPaymentSent.Signature, () => new TLMessageActionPaymentSent()},
                    {TLUpdateBotShippingQuery.Signature, () => new TLUpdateBotShippingQuery()},
                    {TLUpdateBotPrecheckoutQuery.Signature, () => new TLUpdateBotPrecheckoutQuery()},
                    {TLKeyboardButtonBuy.Signature, () => new TLKeyboardButtonBuy()},
                    
                    {TLLabeledPrice.Signature, () => new TLLabeledPrice()},
                    {TLInvoice.Signature, () => new TLInvoice()},
                    {TLPaymentCharge.Signature, () => new TLPaymentCharge()},
                    {TLPostAddress.Signature, () => new TLPostAddress()},
                    {TLPaymentRequestedInfo.Signature, () => new TLPaymentRequestedInfo()},
                    {TLPaymentSavedCredentialsCard.Signature, () => new TLPaymentSavedCredentialsCard()},
                    {TLWebDocument.Signature, () => new TLWebDocument()},
                    {TLInputWebDocument.Signature, () => new TLInputWebDocument()},
                    {TLInputWebFileLocation.Signature, () => new TLInputWebFileLocation()},
                    {TLWebFile.Signature, () => new TLWebFile()},
                    {TLPaymentForm.Signature, () => new TLPaymentForm()},
                    {TLValidatedRequestedInfo.Signature, () => new TLValidatedRequestedInfo()},
                    {TLPaymentResult.Signature, () => new TLPaymentResult()},
                    {TLPaymentVerificationNeeded.Signature, () => new TLPaymentVerificationNeeded()},
                    {TLPaymentReceipt.Signature, () => new TLPaymentReceipt()},
                    {TLSavedInfo.Signature, () => new TLSavedInfo()},
                    {TLInputPaymentCredentialsSaved.Signature, () => new TLInputPaymentCredentialsSaved()},
                    {TLInputPaymentCredentials.Signature, () => new TLInputPaymentCredentials()},
                    {TLTmpPassword.Signature, () => new TLTmpPassword()},
                    {TLShippingOption.Signature, () => new TLShippingOption()},
                    
                    {TLPhoneCallRequested64.Signature, () => new TLPhoneCallRequested64()},
                    {TLPhoneCallAccepted.Signature, () => new TLPhoneCallAccepted()},

                    // layer 65, 66
                    {TLUser66.Signature, () => new TLUser66()},
                    {TLInputMessagesFilterRoundVideo.Signature, () => new TLInputMessagesFilterRoundVideo()},
                    {TLInputMessagesFilterRoundVoice.Signature, () => new TLInputMessagesFilterRoundVoice()},
                    {TLFileCdnRedirect.Signature, () => new TLFileCdnRedirect()},
                    {TLSendMessageRecordRoundAction.Signature, () => new TLSendMessageRecordRoundAction()},
                    {TLSendMessageUploadRoundAction.Signature, () => new TLSendMessageUploadRoundAction()},
                    {TLSendMessageUploadRoundAction66.Signature, () => new TLSendMessageUploadRoundAction66()},
                    {TLDocumentAttributeVideo66.Signature, () => new TLDocumentAttributeVideo66()},
                    {TLPageBlockChannel.Signature, () => new TLPageBlockChannel()},
                    {TLCdnConfig.Signature, () => new TLCdnConfig()},
                    {TLCdnPublicKey.Signature, () => new TLCdnPublicKey()},
                    {TLCdnFile.Signature, () => new TLCdnFile()},
                    {TLCdnFileReuploadNeeded.Signature, () => new TLCdnFileReuploadNeeded()},

                    // layer 67
                    {TLUpdateLangPackTooLong.Signature, () => new TLUpdateLangPackTooLong()},
                    {TLUpdateLangPack.Signature, () => new TLUpdateLangPack()},
                    {TLConfig67.Signature, () => new TLConfig67()},
                    {TLLangPackString.Signature, () => new TLLangPackString()},
                    {TLLangPackStringPluralized.Signature, () => new TLLangPackStringPluralized()},
                    {TLLangPackStringDeleted.Signature, () => new TLLangPackStringDeleted()},
                    {TLLangPackDifference.Signature, () => new TLLangPackDifference()},
                    {TLLangPackLanguage.Signature, () => new TLLangPackLanguage()},

                    // layer 68
                    {TLChannel68.Signature, () => new TLChannel68()},
                    {TLChannelForbidden68.Signature, () => new TLChannelForbidden68()},
                    {TLChannelFull68.Signature, () => new TLChannelFull68()},
                    {TLChannelParticipantAdmin.Signature, () => new TLChannelParticipantAdmin()},
                    {TLChannelParticipantBanned.Signature, () => new TLChannelParticipantBanned()},
                    {TLChannelParticipantsKicked68.Signature, () => new TLChannelParticipantsKicked68()},
                    {TLChannelParticipantsBanned.Signature, () => new TLChannelParticipantsBanned()},
                    {TLChannelParticipantsSearch.Signature, () => new TLChannelParticipantsSearch()},
                    {TLTopPeerCategoryPhoneCalls.Signature, () => new TLTopPeerCategoryPhoneCalls()},
                    {TLPageBlockAudio.Signature, () => new TLPageBlockAudio()},
                    {TLPagePart68.Signature, () => new TLPagePart68()},
                    {TLPageFull68.Signature, () => new TLPageFull68()},
                    {TLChannelAdminRights.Signature, () => new TLChannelAdminRights()},
                    {TLChannelBannedRights.Signature, () => new TLChannelBannedRights()},                    
                    {TLChannelAdminLogEventActionChangeTitle.Signature, () => new TLChannelAdminLogEventActionChangeTitle()},
                    {TLChannelAdminLogEventActionChangeAbout.Signature, () => new TLChannelAdminLogEventActionChangeAbout()},
                    {TLChannelAdminLogEventActionChangeUsername.Signature, () => new TLChannelAdminLogEventActionChangeUsername()},
                    {TLChannelAdminLogEventActionChangePhoto.Signature, () => new TLChannelAdminLogEventActionChangePhoto()},
                    {TLChannelAdminLogEventActionToggleInvites.Signature, () => new TLChannelAdminLogEventActionToggleInvites()},
                    {TLChannelAdminLogEventActionToggleSignatures.Signature, () => new TLChannelAdminLogEventActionToggleSignatures()},
                    {TLChannelAdminLogEventActionUpdatePinned.Signature, () => new TLChannelAdminLogEventActionUpdatePinned()},
                    {TLChannelAdminLogEventActionEditMessage.Signature, () => new TLChannelAdminLogEventActionEditMessage()},
                    {TLChannelAdminLogEventActionDeleteMessage.Signature, () => new TLChannelAdminLogEventActionDeleteMessage()},
                    {TLChannelAdminLogEventActionParticipantJoin.Signature, () => new TLChannelAdminLogEventActionParticipantJoin()},
                    {TLChannelAdminLogEventActionParticipantLeave.Signature, () => new TLChannelAdminLogEventActionParticipantLeave()},
                    {TLChannelAdminLogEventActionParticipantInvite.Signature, () => new TLChannelAdminLogEventActionParticipantInvite()},
                    {TLChannelAdminLogEventActionParticipantToggleBan.Signature, () => new TLChannelAdminLogEventActionParticipantToggleBan()},
                    {TLChannelAdminLogEventActionParticipantToggleAdmin.Signature, () => new TLChannelAdminLogEventActionParticipantToggleAdmin()},         
                    {TLChannelAdminLogEvent.Signature, () => new TLChannelAdminLogEvent()},
                    {TLAdminLogResults.Signature, () => new TLAdminLogResults()},
                    {TLChannelAdminLogEventsFilter.Signature, () => new TLChannelAdminLogEventsFilter()},
                    
                    // layer 69
                    {TLPopularContact.Signature, () => new TLPopularContact()},
                    {TLImportedContacts69.Signature, () => new TLImportedContacts69()},

                    // layer 70
                    {TLInputMediaUploadedPhoto70.Signature, () => new TLInputMediaUploadedPhoto70()},
                    {TLInputMediaPhoto70.Signature, () => new TLInputMediaPhoto70()},
                    {TLInputMediaUploadedDocument70.Signature, () => new TLInputMediaUploadedDocument70()},
                    {TLInputMediaDocument70.Signature, () => new TLInputMediaDocument70()},
                    {TLInputMediaPhotoExternal70.Signature, () => new TLInputMediaPhotoExternal70()},
                    {TLInputMediaDocumentExternal70.Signature, () => new TLInputMediaDocumentExternal70()},
                    {TLMessage70.Signature, () => new TLMessage70()},
                    {TLMessageMediaPhoto70.Signature, () => new TLMessageMediaPhoto70()},
                    {TLMessageMediaDocument70.Signature, () => new TLMessageMediaDocument70()},
                    {TLMessageActionScreenshotTaken.Signature, () => new TLMessageActionScreenshotTaken()},
                    {TLFileCdnRedirect70.Signature, () => new TLFileCdnRedirect70()},
                    {TLMessageFwdHeader70.Signature, () => new TLMessageFwdHeader70()},
                    {TLCdnFileHash.Signature, () => new TLCdnFileHash()},

                    // layer 71
                    {TLChannelFull71.Signature, () => new TLChannelFull71()},
                    {TLDialog71.Signature, () => new TLDialog71()},
                    {TLContacts71.Signature, () => new TLContacts71()},
                    {TLInputMessagesFilterMyMentions.Signature, () => new TLInputMessagesFilterMyMentions()},
                    {TLUpdateFavedStickers.Signature, () => new TLUpdateFavedStickers()},
                    {TLUpdateChannelReadMessagesContents.Signature, () => new TLUpdateChannelReadMessagesContents()},
                    {TLUpdateContactsReset.Signature, () => new TLUpdateContactsReset()},
                    {TLConfig71.Signature, () => new TLConfig71()},
                    {TLChannelDifferenceTooLong71.Signature, () => new TLChannelDifferenceTooLong71()},
                    {TLChannelAdminLogEventActionChangeStickerSet.Signature, () => new TLChannelAdminLogEventActionChangeStickerSet()},
                    {TLFavedStickersNotModified.Signature, () => new TLFavedStickersNotModified()},
                    {TLFavedStickers.Signature, () => new TLFavedStickers()},

                    // layer 72
                    {TLInputMediaVenue72.Signature, () => new TLInputMediaVenue72()},
                    {TLInputMediaGeoLive.Signature, () => new TLInputMediaGeoLive()},
                    {TLChannelFull72.Signature, () => new TLChannelFull72()},
                    {TLMessageMediaVenue72.Signature, () => new TLMessageMediaVenue72()},
                    {TLMessageMediaGeoLive.Signature, () => new TLMessageMediaGeoLive()},
                    {TLMessageActionCustomAction.Signature, () => new TLMessageActionCustomAction()},
                    {TLInputMessagesFilterGeo.Signature, () => new TLInputMessagesFilterGeo()},
                    {TLInputMessagesFilterContacts.Signature, () => new TLInputMessagesFilterContacts()},
                    {TLUpdateChannelAvailableMessages.Signature, () => new TLUpdateChannelAvailableMessages()},
                    {TLConfig72.Signature, () => new TLConfig72()},
                    {TLBotResults72.Signature, () => new TLBotResults72()},
                    {TLInputPaymentCredentialsApplePay.Signature, () => new TLInputPaymentCredentialsApplePay()},
                    {TLInputPaymentCredentialsAndroidPay.Signature, () => new TLInputPaymentCredentialsAndroidPay()},
                    {TLChannelAdminLogEventActionTogglePreHistoryHidden.Signature, () => new TLChannelAdminLogEventActionTogglePreHistoryHidden()},
                    {TLRecentMeUrlUnknown.Signature, () => new TLRecentMeUrlUnknown()},
                    {TLRecentMeUrlUser.Signature, () => new TLRecentMeUrlUser()},
                    {TLRecentMeUrlChat.Signature, () => new TLRecentMeUrlChat()},
                    {TLRecentMeUrlChatInvite.Signature, () => new TLRecentMeUrlChatInvite()},
                    {TLRecentMeUrlStickerSet.Signature, () => new TLRecentMeUrlStickerSet()},
                    {TLRecentMeUrls.Signature, () => new TLRecentMeUrls()},
                    {TLChannelParticipantsNotModified.Signature, () => new TLChannelParticipantsNotModified()},
                    
                    // layer 73
                    {TLChannel73.Signature, () => new TLChannel73()},
                    {TLMessage73.Signature, () => new TLMessage73()},
                    {TLMessageFwdHeader73.Signature, () => new TLMessageFwdHeader73()},
                    {TLInputMediaInvoice73.Signature, () => new TLInputMediaInvoice73()},
                    {TLInputSingleMedia.Signature, () => new TLInputSingleMedia()},

                    // layer 74
                    {TLContactsFound74.Signature, () => new TLContactsFound74()},
                    {TLExportedMessageLink74.Signature, () => new TLExportedMessageLink74()},
                    {TLInputPaymentCredentialsAndroidPay74.Signature, () => new TLInputPaymentCredentialsAndroidPay74()},
                    
                    // layer 75
                    {TLInputMediaUploadedPhoto75.Signature, () => new TLInputMediaUploadedPhoto75()},
                    {TLInputMediaPhoto75.Signature, () => new TLInputMediaPhoto75()},
                    {TLInputMediaUploadedDocument75.Signature, () => new TLInputMediaUploadedDocument75()},
                    {TLInputMediaDocument75.Signature, () => new TLInputMediaDocument75()},
                    {TLInputMediaPhotoExternal75.Signature, () => new TLInputMediaPhotoExternal75()},
                    {TLInputMediaDocumentExternal75.Signature, () => new TLInputMediaDocumentExternal75()},
                    {TLMessageMediaPhoto75.Signature, () => new TLMessageMediaPhoto75()},
                    {TLMessageMediaDocument75.Signature, () => new TLMessageMediaDocument75()},
                    {TLInputBotInlineMessageMediaAuto75.Signature, () => new TLInputBotInlineMessageMediaAuto75()},
                    {TLBotInlineMessageMediaAuto75.Signature, () => new TLBotInlineMessageMediaAuto75()},
                    {TLInputSingleMedia75.Signature, () => new TLInputSingleMedia75()},

                    // layer 76
                    {TLChannel76.Signature, () => new TLChannel76()},
                    {TLDialogFeed.Signature, () => new TLDialogFeed()},
                    {TLUpdateDialogPinned76.Signature, () => new TLUpdateDialogPinned76()},
                    {TLUpdatePinnedDialogs76.Signature, () => new TLUpdatePinnedDialogs76()},
                    {TLUpdateReadFeed.Signature, () => new TLUpdateReadFeed()},
                    {TLStickerSet76.Signature, () => new TLStickerSet76()},
                    {TLRecentStickers76.Signature, () => new TLRecentStickers76()},
                    {TLFeedPosition.Signature, () => new TLFeedPosition()},
                    {TLInputDialogPeerFeed.Signature, () => new TLInputDialogPeerFeed()},
                    {TLInputDialogPeer.Signature, () => new TLInputDialogPeer()},
                    {TLDialogPeerFeed.Signature, () => new TLDialogPeerFeed()},
                    {TLDialogPeer.Signature, () => new TLDialogPeer()},
                    {TLWebAuthorization.Signature, () => new TLWebAuthorization()},
                    {TLWebAuthorizations.Signature, () => new TLWebAuthorizations()},
                    {TLInputMessageId.Signature, () => new TLInputMessageId()},
                    {TLInputMessageReplyTo.Signature, () => new TLInputMessageReplyTo()},
                    {TLInputMessagePinned.Signature, () => new TLInputMessagePinned()},
                    {TLInputSingleMedia76.Signature, () => new TLInputSingleMedia76()},
                    {TLMessageEntityPhone.Signature, () => new TLMessageEntityPhone()},
                    {TLMessageEntityCashtag.Signature, () => new TLMessageEntityCashtag()},
                    {TLFeedMessagesNotModified.Signature, () => new TLFeedMessagesNotModified()},
                    {TLFeedMessages.Signature, () => new TLFeedMessages()},
                    {TLFeedBroadcastsUngrouped.Signature, () => new TLFeedBroadcastsUngrouped()},
                    {TLFeedBroadcasts.Signature, () => new TLFeedBroadcasts()},
                    {TLFeedSourcesNotModified.Signature, () => new TLFeedSourcesNotModified()},
                    {TLFeedSources.Signature, () => new TLFeedSources()},
                    {TLMessageActionBotAllowed.Signature, () => new TLMessageActionBotAllowed()},
                    {TLPeerFeed.Signature, () => new TLPeerFeed()},
                    {TLInputPeerFeed.Signature, () => new TLInputPeerFeed()},
                    {TLConfig76.Signature, () => new TLConfig76()},
                    {TLFoundStickerSetsNotModified.Signature, () => new TLFoundStickerSetsNotModified()},
                    {TLFoundStickerSets.Signature, () => new TLFoundStickerSets()},
                    {TLFileHash.Signature, () => new TLFileHash()},
                    {TLFileCdnRedirect76.Signature, () => new TLFileCdnRedirect76()},
                    {TLInputBotInlineResult76.Signature, () => new TLInputBotInlineResult76()},
                    {TLBotInlineResult76.Signature, () => new TLBotInlineResult76()},
                    {TLWebDocumentNoProxy.Signature, () => new TLWebDocumentNoProxy()},
                    
                    // layer 77
                    // layer 78
                    {TLDCOption78.Signature, () => new TLDCOption78()},
                    {TLConfig78.Signature, () => new TLConfig78()},
                    {TLInputClientProxy.Signature, () => new TLInputClientProxy()},
                    {TLProxyDataEmpty.Signature, () => new TLProxyDataEmpty()},
                    {TLProxyDataPromo.Signature, () => new TLProxyDataPromo()},

                    // layer 79
                    {TLStickers79.Signature, () => new TLStickers79()},
                    {TLPeerNotifySettings78.Signature, () => new TLPeerNotifySettings78()},
                    {TLInputPeerNotifySettings78.Signature, () => new TLInputPeerNotifySettings78()},
                    {TLBotInlineMessageMediaVenue78.Signature, () => new TLBotInlineMessageMediaVenue78()},
                    {TLInputBotInlineMessageMediaVenue78.Signature, () => new TLInputBotInlineMessageMediaVenue78()},
                    
                    // layer 80
                    {TLSentCode80.Signature, () => new TLSentCode80()},
                    {TLTermsOfService80.Signature, () => new TLTermsOfService80()},
                    {TLTermsOfServiceUpdateEmpty.Signature, () => new TLTermsOfServiceUpdateEmpty()},
                    {TLTermsOfServiceUpdate.Signature, () => new TLTermsOfServiceUpdate()},
                    
                    // layer 81
                    {TLInputSecureFileLocation.Signature, () => new TLInputSecureFileLocation()},
                    {TLMessageActionSecureValuesSentMe.Signature, () => new TLMessageActionSecureValuesSentMe()},
                    {TLMessageActionSecureValuesSent.Signature, () => new TLMessageActionSecureValuesSent()},
                    {TLNoPassword81.Signature, () => new TLNoPassword81()},
                    {TLPassword81.Signature, () => new TLPassword81()},
                    {TLPasswordSettings81.Signature, () => new TLPasswordSettings81()},
                    {TLPasswordInputSettings81.Signature, () => new TLPasswordInputSettings81()},
                    {TLInputSecureFileUploaded.Signature, () => new TLInputSecureFileUploaded()},
                    {TLInputSecureFile.Signature, () => new TLInputSecureFile()},
                    {TLSecureFileEmpty.Signature, () => new TLSecureFileEmpty()},
                    {TLSecureFile.Signature, () => new TLSecureFile()},
                    {TLSecureData.Signature, () => new TLSecureData()},
                    {TLSecurePlainPhone.Signature, () => new TLSecurePlainPhone()},
                    {TLSecurePlainEmail.Signature, () => new TLSecurePlainEmail()},
                    {TLSecureValueTypePersonalDetails.Signature, () => new TLSecureValueTypePersonalDetails()},
                    {TLSecureValueTypePassport.Signature, () => new TLSecureValueTypePassport()},
                    {TLSecureValueTypeDriverLicense.Signature, () => new TLSecureValueTypeDriverLicense()},
                    {TLSecureValueTypeIdentityCard.Signature, () => new TLSecureValueTypeIdentityCard()},
                    {TLSecureValueTypeInternalPassport.Signature, () => new TLSecureValueTypeInternalPassport()},
                    {TLSecureValueTypeAddress.Signature, () => new TLSecureValueTypeAddress()},
                    {TLSecureValueTypeUtilityBill.Signature, () => new TLSecureValueTypeUtilityBill()},
                    {TLSecureValueTypeBankStatement.Signature, () => new TLSecureValueTypeBankStatement()},
                    {TLSecureValueTypeRentalAgreement.Signature, () => new TLSecureValueTypeRentalAgreement()},
                    {TLSecureValueTypePassportRegistration.Signature, () => new TLSecureValueTypePassportRegistration()},
                    {TLSecureValueTypeTemporaryRegistration.Signature, () => new TLSecureValueTypeTemporaryRegistration()},             
                    {TLSecureValueTypePhone.Signature, () => new TLSecureValueTypePhone()},
                    {TLSecureValueTypeEmail.Signature, () => new TLSecureValueTypeEmail()},
                    {TLSecureValue.Signature, () => new TLSecureValue()},
                    {TLInputSecureValue.Signature, () => new TLInputSecureValue()},
                    {TLSecureValueHash.Signature, () => new TLSecureValueHash()},
                    {TLSecureValueErrorData.Signature, () => new TLSecureValueErrorData()},
                    {TLSecureValueErrorFrontSide.Signature, () => new TLSecureValueErrorFrontSide()},
                    {TLSecureValueErrorReverseSide.Signature, () => new TLSecureValueErrorReverseSide()},
                    {TLSecureValueErrorSelfie.Signature, () => new TLSecureValueErrorSelfie()},
                    {TLSecureValueErrorFile.Signature, () => new TLSecureValueErrorFile()},
                    {TLSecureValueErrorFiles.Signature, () => new TLSecureValueErrorFiles()},
                    {TLSecureCredentialsEncrypted.Signature, () => new TLSecureCredentialsEncrypted()},
                    {TLAuthorizationForm.Signature, () => new TLAuthorizationForm()},
                    {TLSentEmailCode.Signature, () => new TLSentEmailCode()},
                    {TLDeepLinkInfoEmpty.Signature, () => new TLDeepLinkInfoEmpty()},
                    {TLDeepLinkInfo.Signature, () => new TLDeepLinkInfo()},
                    {TLSavedPhoneContact.Signature, () => new TLSavedPhoneContact()},
                    {TLTakeout.Signature, () => new TLTakeout()},
                    
                    // layer 82
                    {TLInputTakeoutFileLocation.Signature, () => new TLInputTakeoutFileLocation()},
                    {TLAppUpdate.Signature, () => new TLAppUpdate()},
                    {TLNoAppUpdate.Signature, () => new TLNoAppUpdate()},
                    {TLInputMediaContact82.Signature, () => new TLInputMediaContact82()},
                    {TLMessageMediaContact82.Signature, () => new TLMessageMediaContact82()},
                    {TLGeoPoint82.Signature, () => new TLGeoPoint82()},
                    {TLDialogsNotModified.Signature, () => new TLDialogsNotModified()},
                    {TLUpdateDialogUnreadMark.Signature, () => new TLUpdateDialogUnreadMark()},
                    {TLConfig82.Signature, () => new TLConfig82()},
                    {TLInputBotInlineMessageMediaContact82.Signature, () => new TLInputBotInlineMessageMediaContact82()},
                    {TLBotInlineMessageMediaContact82.Signature, () => new TLBotInlineMessageMediaContact82()},
                    {TLDraftMessageEmpty82.Signature, () => new TLDraftMessageEmpty82()},
                    {TLWebDocument82.Signature, () => new TLWebDocument82()},
                    {TLInputWebFileGeoPointLocation.Signature, () => new TLInputWebFileGeoPointLocation()},
                    {TLInputReportReasonCopyright.Signature, () => new TLInputReportReasonCopyright()},
                    {TLTopPeersDisabled.Signature, () => new TLTopPeersDisabled()},
                   
                    // layer 83
                    {TLPassword83.Signature, () => new TLPassword83()},
                    {TLPasswordSettings83.Signature, () => new TLPasswordSettings83()},
                    {TLPasswordInputSettings83.Signature, () => new TLPasswordInputSettings83()},
                    {TLPasswordKdfAlgoUnknown.Signature, () => new TLPasswordKdfAlgoUnknown()},
                    {TLSecurePasswordKdfAlgoUnknown.Signature, () => new TLSecurePasswordKdfAlgoUnknown()},
                    {TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000.Signature, () => new TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000()},
                    {TLSecurePasswordKdfAlgoSHA512.Signature, () => new TLSecurePasswordKdfAlgoSHA512()},
                    {TLSecureSecretSettings.Signature, () => new TLSecureSecretSettings()},

                    // layer 84
                    {TLPassword84.Signature, () => new TLPassword84()},
                    {TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow.Signature, () => new TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow()},
                    {TLInputCheckPasswordEmpty.Signature, () => new TLInputCheckPasswordEmpty()},
                    {TLInputCheckPasswordSRP.Signature, () => new TLInputCheckPasswordSRP()},
                    
                    // layer 85
                    {TLSecureValue85.Signature, () => new TLSecureValue85()},
                    {TLInputSecureValue85.Signature, () => new TLInputSecureValue85()},
                    {TLSecureValueError.Signature, () => new TLSecureValueError()},
                    {TLSecureValueErrorTranslationFile.Signature, () => new TLSecureValueErrorTranslationFile()},
                    {TLSecureValueErrorTranslationFiles.Signature, () => new TLSecureValueErrorTranslationFiles()},
                    {TLAuthorizationForm85.Signature, () => new TLAuthorizationForm85()},
                    {TLSecureRequiredType.Signature, () => new TLSecureRequiredType()},
                    {TLSecureRequiredTypeOneOf.Signature, () => new TLSecureRequiredTypeOneOf()},
                    {TLPassportConfigNotModified.Signature, () => new TLPassportConfigNotModified()},
                    {TLPassportConfig.Signature, () => new TLPassportConfig()},
                    
                    //
                    {TLConfigSimple.Signature, () => new TLConfigSimple()},

                    //45 layer encrypted
                    {TLDecryptedMessage45.Signature, () => new TLDecryptedMessage45()},
                    {TLDecryptedMessageMediaPhoto45.Signature, () => new TLDecryptedMessageMediaPhoto45()},
                    {TLDecryptedMessageMediaVideo45.Signature, () => new TLDecryptedMessageMediaVideo45()},
                    {TLDecryptedMessageMediaDocument45.Signature, () => new TLDecryptedMessageMediaDocument45()},
                    {TLDecryptedMessageMediaVenue.Signature, () => new TLDecryptedMessageMediaVenue()},
                    {TLDecryptedMessageMediaWebPage.Signature, () => new TLDecryptedMessageMediaWebPage()},

                    //73 layer encrypted
                    {TLDecryptedMessage73.Signature, () => new TLDecryptedMessage73()},

                    // functions
                    {TLSendMessage.Signature, () => new TLSendMessage()},
                    {TLSendInlineBotResult.Signature, () => new TLSendInlineBotResult()},
                    {TLSendMedia.Signature, () => new TLSendMedia()},
                    {TLForwardMessage.Signature, () => new TLForwardMessage()},
                    {TLForwardMessages.Signature, () => new TLForwardMessages()},
                    {TLStartBot.Signature, () => new TLStartBot()},
                    {TLReadHistory.Signature, () => new TLReadHistory()},
                    {TLReadChannelHistory.Signature, () => new TLReadChannelHistory()},
                    {Functions.Messages.TLReadMessageContents.Signature, () => new Functions.Messages.TLReadMessageContents()},
                    {Functions.Channels.TLReadMessageContents.Signature, () => new Functions.Channels.TLReadMessageContents()},
                    
                    {TLSendEncrypted.Signature, () => new TLSendEncrypted()},
                    {TLSendEncryptedFile.Signature, () => new TLSendEncryptedFile()},
                    {TLSendEncryptedService.Signature, () => new TLSendEncryptedService()},
                    {TLReadEncryptedHistory.Signature, () => new TLReadEncryptedHistory()},
                    
                    {TLInitConnection.Signature, () => new TLInitConnection()},
                    {TLInitConnection67.Signature, () => new TLInitConnection67()},

                    // additional sigantures
                    {TLEncryptedDialog.Signature, () => new TLEncryptedDialog()},                   
                    {TLUserExtendedInfo.Signature, () => new TLUserExtendedInfo()},                   
                    {TLDecryptedMessageActionEmpty.Signature, () => new TLDecryptedMessageActionEmpty()},
                    {TLPeerEncryptedChat.Signature, () => new TLPeerEncryptedChat()},
                    {TLBroadcastChat.Signature, () => new TLBroadcastChat()},
                    {TLPeerBroadcast.Signature, () => new TLPeerBroadcast()},
                    {TLBroadcastDialog.Signature, () => new TLBroadcastDialog()},
                    {TLInputPeerBroadcast.Signature, () => new TLInputPeerBroadcast()},
                    {TLServerFile.Signature, () => new TLServerFile()},
                    {TLEncryptedChat17.Signature, () => new TLEncryptedChat17()},
                    {TLMessageActionUnreadMessages.Signature, () => new TLMessageActionUnreadMessages()},
                    {TLMessagesContainter.Signature, () => new TLMessagesContainter()},
                    {TLHashtagItem.Signature, () => new TLHashtagItem()},
                    {TLMessageActionContactRegistered.Signature, () => new TLMessageActionContactRegistered()},
                    {TLPasscodeParams.Signature, () => new TLPasscodeParams()},
                    {TLRecentlyUsedSticker.Signature, () => new TLRecentlyUsedSticker()},
                    {TLActionInfo.Signature, () => new TLActionInfo()},
                    {TLResultInfo.Signature, () => new TLResultInfo()},
                    {TLMessageActionMessageGroup.Signature, () => new TLMessageActionMessageGroup()},
                    {TLMessageActionChannelJoined.Signature, () => new TLMessageActionChannelJoined()},
                    {TLChatSettings.Signature, () => new TLChatSettings()},
                    {TLDocumentExternal.Signature, () => new TLDocumentExternal()},
                    {TLDecryptedMessagesContainter.Signature, () => new TLDecryptedMessagesContainter()},
                    {TLCameraSettings.Signature, () => new TLCameraSettings()},
                    {TLPhotoPickerSettings.Signature, () => new TLPhotoPickerSettings()},
                    {TLProxyConfig.Signature, () => new TLProxyConfig()},
                    {TLCallsSecurity.Signature, () => new TLCallsSecurity()},
                    {TLStickerSetEmpty.Signature, () => new TLStickerSetEmpty()},
                    {TLMessageMediaGroup.Signature, () => new TLMessageMediaGroup()},
                    {TLDecryptedMessageMediaGroup.Signature, () => new TLDecryptedMessageMediaGroup()},
                    {TLSecureFileUploaded.Signature, () => new TLSecureFileUploaded()},
                    {TLProxyConfig76.Signature, () => new TLProxyConfig76()},
                    {TLSocks5Proxy.Signature, () => new TLSocks5Proxy()},
                    {TLMTProtoProxy.Signature, () => new TLMTProtoProxy()},
                    {TLContactsSettings.Signature, () => new TLContactsSettings()},
                };

        public static TimeSpan ElapsedClothedTypes;

        public static TimeSpan ElapsedBaredTypes;

        public static TimeSpan ElapsedVectorTypes;

        public static T GetObject<T>(byte[] bytes, int position) where T : TLObject
        {

            //var stopwatch = Stopwatch.StartNew();

            // bared types


            var stopwatch2 = Stopwatch.StartNew();
            try
            {

                if (_baredTypes.ContainsKey(typeof (T)))
                {
                    return (T) _baredTypes[typeof (T)].Invoke();
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }
            finally
            {
                ElapsedBaredTypes += stopwatch2.Elapsed;
            }

            var stopwatch = Stopwatch.StartNew();
            uint signature = 0;
            try
            {
                // clothed types
                //var signatureBytes = bytes.SubArray(position, 4);
                //Array.Reverse(signatureBytes);
                signature = BitConverter.ToUInt32(bytes, position);
                Func<TLObject> getInstance;


                // exact matching
                if (_clothedTypes.TryGetValue(signature, out getInstance))
                {
                    return (T)getInstance.Invoke();
                }


                //// matching with removed leading 0
                //while (signature.StartsWith("0"))
                //{
                //    signature = signature.Remove(0, 1);
                //    if (_clothedTypes.TryGetValue("#" + signature, out getInstance))
                //    {
                //        return (T)getInstance.Invoke();
                //    }
                //}
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }
            finally
            {
                ElapsedClothedTypes += stopwatch.Elapsed;
            }




            var stopwatch3 = Stopwatch.StartNew();
            //throw new Exception("Signature exception");
            try
            {
                // TLVector
                if (bytes.StartsWith(position, TLConstructors.TLVector))
                {
                    

                    //TODO: remove workaround for TLRPCRESULT: TLVECTOR<TLINT>
                    if (typeof (T) == typeof (TLObject))
                    {
                        Func<TLObject> getObject;
                        var internalSignature = BitConverter.ToUInt32(bytes, position + 8);
                        var length = BitConverter.ToInt32(bytes, position + 4);
                        if (length > 0)
                        {
                            if (_clothedTypes.TryGetValue(internalSignature, out getObject))
                            {
                                var obj = getObject.Invoke();
                                if (obj is TLUserBase)
                                {
                                    return (T)Activator.CreateInstance(typeof(TLVector<TLUserBase>));
                                }
                            }
                        }

                        if (bytes.StartsWith(position + 8, TLConstructors.TLStickerSet)
                            || bytes.StartsWith(position + 8, TLConstructors.TLStickerSet32))
                        {
                            return (T)Activator.CreateInstance(typeof(TLVector<TLStickerSetBase>));
                        }
                        else if (bytes.StartsWith(position + 8, TLConstructors.TLContactStatus19)
                            || bytes.StartsWith(position + 8, TLConstructors.TLContactStatus))
                        {
                            return (T)Activator.CreateInstance(typeof(TLVector<TLContactStatusBase>));
                        }
                        else if (bytes.StartsWith(position + 8, TLConstructors.TLWallPaper)
                            || bytes.StartsWith(position + 8, TLConstructors.TLWallPaperSolid))
                        {
                            return (T)Activator.CreateInstance(typeof(TLVector<TLWallPaperBase>));
                        }
                        else if (bytes.StartsWith(position + 8, TLConstructors.TLStickerSetCovered)
                            || bytes.StartsWith(position + 8, TLConstructors.TLStickerSetMultiCovered))
                        {
                            return (T)Activator.CreateInstance(typeof(TLVector<TLStickerSetCoveredBase>));
                        }
                        else if (bytes.StartsWith(position + 8, TLConstructors.TLSecureValue)
                            || bytes.StartsWith(position + 8, TLConstructors.TLSecureValue85))
                        {
                            return (T)Activator.CreateInstance(typeof(TLVector<TLSecureValue>));
                        }
                        TLUtils.WriteLine("TLVecto<TLInt>  hack ", LogSeverity.Error);
                        return (T) Activator.CreateInstance(typeof(TLVector<TLInt>));
                    }
                    else
                    {
                        return (T) Activator.CreateInstance(typeof (T));
                    }
                }

            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }
            finally
            {
                ElapsedVectorTypes += stopwatch3.Elapsed;
            }

            var signatureBytes = BitConverter.GetBytes(signature);
            Array.Reverse(signatureBytes);
            var signatureString = BitConverter.ToString(signatureBytes).Replace("-", string.Empty).ToLowerInvariant();
            if (typeof (T) == typeof (TLObject))
            {
                var error = string.Format("  ERROR TLObjectGenerator: Cannot find signature #{0} ({1})\n\n{2}", signatureString, signature, GetStackTrace());
                TLUtils.WriteLine(error, LogSeverity.Error);
                Logs.Log.Write(error);
                Execute.ShowDebugMessage(error);
            }
            else
            {
                var error = string.Format("  ERROR TLObjectGenerator: Incorrect signature #{0} ({1}) for type {2}\n\n{3}", signatureString, signature, typeof(T), GetStackTrace());
                TLUtils.WriteLine(error, LogSeverity.Error);
                Logs.Log.Write(error);
                Execute.ShowDebugMessage(error);
            }

            return null;
        }

        public static string GetStackTrace()
        {
            try
            {
                var type = typeof(Environment);
                foreach (var p in type.GetRuntimeProperties())
                {
                    if (p.Name == "StackTrace")
                    {
                        var v = p.GetValue(null, null);

                        return v != null ? v.ToString() : null;
                    }
                }
            }
            catch
            {
                
            }

            return null;
        }

        public static T GetNullableObject<T>(Stream input) where T : TLObject
        {
            // clothed types
            var signatureBytes = new byte[4];
            input.Read(signatureBytes, 0, 4);
            uint signature = BitConverter.ToUInt32(signatureBytes, 0);

            if (signature == TLNull.Signature) return null;

            input.Position = input.Position - 4;
            return GetObject<T>(input);
        }

        public static T GetObject<T>(Stream input) where T : TLObject
        {
            //var startPosition = input.Position;
            //var stopwatch = Stopwatch.StartNew();

            // bared types


            var stopwatch2 = Stopwatch.StartNew();
            try
            {

                if (_baredTypes.ContainsKey(typeof(T)))
                {
                    return (T)_baredTypes[typeof(T)].Invoke();
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }
            finally
            {
                ElapsedBaredTypes += stopwatch2.Elapsed;
            }

            var stopwatch = Stopwatch.StartNew();
            uint signature = 0;
            try
            {
                // clothed types
                var signatureBytes = new byte[4];
                input.Read(signatureBytes, 0, 4);
                signature = BitConverter.ToUInt32(signatureBytes, 0);
                Func<TLObject> getInstance;


                // exact matching
                if (_clothedTypes.TryGetValue(signature, out getInstance))
                {
                    return (T)getInstance.Invoke();
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }
            finally
            {

                ElapsedClothedTypes += stopwatch.Elapsed;
            }




            var stopwatch3 = Stopwatch.StartNew();
            //throw new Exception("Signature exception");
            try
            {
                // TLVector
                if (signature == TLConstructors.TLVector)
                {
                    //TODO: remove workaround for TLRPCRESULT: TLVECTOR<TLINT>
                    if (typeof(T) == typeof(TLObject))
                    {
                        TLUtils.WriteLine("TLVecto<TLInt>  hack ", LogSeverity.Error);
                        return (T)Activator.CreateInstance(typeof(TLVector<TLInt>));
                    }
                    else
                    {
                        return (T)Activator.CreateInstance(typeof(T));
                    }
                }

            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }
            finally
            {
                ElapsedVectorTypes += stopwatch3.Elapsed;
            }

            var bytes = BitConverter.GetBytes(signature);
            Array.Reverse(bytes);
            var signatureString = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
            if (typeof(T) == typeof(TLObject))
            {
                var error = string.Format("  ERROR TLObjectGenerator FromStream: Cannot find signature #{0} ({1})\n\n{2}", signatureString, signature, GetStackTrace());
                TLUtils.WriteLine(error, LogSeverity.Error);
                Execute.ShowDebugMessage(error);
            }
            else
            {
                var error = string.Format("  ERROR TLObjectGenerator FromStream: Incorrect signature #{0} ({1}) for type {2}\n\n{3}", signatureString, signature, typeof(T), GetStackTrace());

                //var count = 0;
                //while (input.Position < input.Length)
                //{
                //    input.Position = startPosition + count;
                //    var signatureBytes = new byte[4];
                //    input.Read(signatureBytes, 0, 4);
                //    signature = BitConverter.ToUInt32(signatureBytes, 0);
                //    Func<TLObject> getInstance;
                //    if (_clothedTypes.TryGetValue(signature, out getInstance))
                //    {
                //        var instance = getInstance.Invoke();
                //    }
                //    count++;
                //}

                TLUtils.WriteLine(error, LogSeverity.Error);
                Execute.ShowDebugMessage(error);
            }

            return null;
        }
    }
}
