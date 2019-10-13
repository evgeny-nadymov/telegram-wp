// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public static class TLConstructors
    {
        public const uint TLUpdateUserBlocked = 0x80ece81a;
        public const uint TLUpdateNotifySettings = 0xbec268ef;
        public const uint TLNotifyPeer = 0x9fd40bd8;
        public const uint TLNotifyUsers = 0xb4c83b4c;
        public const uint TLNotifyChats = 0xc007cec3;
        public const uint TLNotifyAll = 0x74d07c60;
        public const uint TLDecryptedMessageActionReadMessages = 0xc4f40be;
        public const uint TLDecryptedMessageActionDeleteMessages = 0x65614304;
        public const uint TLDecryptedMessageActionScreenshotMessages = 0x8ac1f475;
        public const uint TLDecryptedMessageActionFlushHistory = 0x6719e45c;
        public const uint TLDecryptedMessageActionNotifyLayer = 0xf3048883;
        public const uint TLDecryptedMessageLayer = 0x99a438cf;
        public const uint TLSupport = 0x17c6b5f6;
        public const uint TLDecryptedMessageMediaAudio = 0x6080758f;
        public const uint TLDecryptedMessageMediaDocument = 0xb095434b;
        public const uint TLInputAudioFileLocation = 0x74dc404d;
        public const uint TLInputDocumentFileLocation = 0x4e45abe9;
        public const uint TLInputMediaUploadedDocument = 0x34e794bd;
        public const uint TLInputMediaUploadedThumbDocument = 0x3e46de5d;
        public const uint TLInputMediaDocument = 0xd184e841;
        public const uint TLInputMediaAudio = 0x89938781;
        public const uint TLInputMediaUploadedAudio = 0x4e498cab;
        public const uint TLInputAudio = 0x77d440ff;
        public const uint TLInputAudioEmpty = 0xd95adc84;
        public const uint TLInputDocument = 0x18798952;
        public const uint TLInputDocumentEmpty = 0x72f0eaae;
        public const uint TLMessageMediaAudio = 0xc6b68300;
        public const uint TLMessageMediaDocument = 0x2fda2204;
        public const uint TLAudioEmpty = 0x586988d8;
        public const uint TLAudio = 0xc7ac6496;
        public const uint TLDocumentEmpty = 0x36f8c871;
        public const uint TLDocument = 0x9efc6326;
        public const uint TLUpdateChatParticipantAdd = 0x3a0eeb22;
        public const uint TLUpdateChatParticipantDelete = 0x6e5f8c22;
        public const uint TLInputEncryptedFileBigUploaded = 0x2dc173c8;
        public const uint TLInputFileBig = 0xfa4f0bb5;
        public const uint TLDecryptedMessage = 0x1f814f1f;
        public const uint TLDecryptedMessageService = 0xaa48327d;
        public const uint TLUpdateNewEncryptedMessage = 0x12bcbd9a;
        public const uint TLUpdateEncryptedChatTyping = 0x1710f156;
        public const uint TLUpdateEncryption = 0xb4a2e88d;
        public const uint TLUpdateEncryptedMessagesRead = 0x38fe25b7;
        public const uint TLEncryptedChatEmpty = 0xab7ec0a0;
        public const uint TLEncryptedChatWaiting = 0x3bf703dc;
        public const uint TLEncryptedChatRequested = 0xc878527e;
        public const uint TLEncryptedChat = 0xfa56ce36;
        public const uint TLEncryptedChatDiscarded = 0x13d6dd27;
        public const uint TLInputEncryptedChat = 0xf141b5e1;
        public const uint TLInputEncryptedFileEmpty = 0x1837c364;
        public const uint TLInputEncryptedFileUploaded = 0x64bd0306;
        public const uint TLInputEncryptedFile = 0x5a17b5e5;
        public const uint TLInputEncryptedFileLocation = 0xf5235d55;
        public const uint TLEncryptedFileEmpty = 0xc21f497e;
        public const uint TLEncryptedFile = 0x4a70994c;
        public const uint TLEncryptedMessage = 0xed18c118;
        public const uint TLEncryptedMessageService = 0x23734b06;
        public const uint TLDecryptedMessageActionSetMessageTTL = 0xa1733aec;
        public const uint TLDecryptedMessageMediaEmpty = 0x089f5c4a;
        public const uint TLDecryptedMessageMediaPhoto = 0x32798a8c;
        public const uint TLDecryptedMessageMediaVideo = 0x4cee6ef3;
        public const uint TLDecryptedMessageMediaGeoPoint = 0x35480a59;
        public const uint TLDecryptedMessageMediaContact = 0x588a0a97;
        public const uint TLDHConfig = 0x2c221edd;
        public const uint TLDHConfigNotModified = 0xc0e24635;
        public const uint TLSentEncryptedMessage = 0x560f8935;
        public const uint TLSentEncryptedFile = 0x9493ff32;
        public const uint TLRPCAnswerUnknown = 0x5e2ad36e;
        public const uint TLRPCAnswerDroppedRunning = 0xcd78e586;
        public const uint TLRPCAnswerDropped = 0xa43ad8b7;
        public const uint TLMessageDetailedInfo = 0x276d3ec6;
        public const uint TLMessageNewDetailedInfo = 0x809db6df;
        public const uint TLMessagesAllInfo = 0x8cc0d131;
        public const uint TLInvokeAfterMsg = 0xcb9f372d;
        public const uint TLDifferenceEmpty = 0x5d75a138;
        public const uint TLDifference = 0xf49ca0;
        public const uint TLDifferenceSlice = 0xa8fb1981;
        public const uint TLUpdateNewMessage = 0x013abdb3;
        public const uint TLUpdateMessageId = 0x4e90bfd6;
        public const uint TLUpdateReadMessages = 0xc6649e31;
        public const uint TLUpdateDeleteMessages = 0xa92bfe26;
        public const uint TLUpdateRestoreMessages = 0xd15de04d;
        public const uint TLUpdateUserTyping = 0x6baa8508;
        public const uint TLUpdateChatUserTyping = 0x3c46cfe6;
        public const uint TLUpdateChatParticipants = 0x07761198;
        public const uint TLUpdateUserStatus = 0x1bfbd823;
        public const uint TLUpdateUserName = 0xa7332b73;
        public const uint TLUpdateUserPhoto = 0x95313b0c;
        public const uint TLUpdateContactRegistered = 0x2575bbb9;
        public const uint TLUpdateContactLink = 0x51a48a9a;
        public const uint TLUpdateActivation = 0x6f690963;
        public const uint TLUpdateNewAuthorization = 0x8f06529a;
        public const uint TLUpdateDCOptions = 0x8e5e9873;
        public const uint TLUpdatesTooLong = 0xe317af7e;
        public const uint TLUpdateShortMessage = 0xd3f45784;
        public const uint TLUpdateShortChatMessage = 0x2b2fbd4e;
        public const uint TLUpdateShort = 0x78d4dec1;
        public const uint TLUpdatesCombined = 0x725b04c3;
        public const uint TLUpdates = 0x74ae4240;
        public const uint TLFutureSalt = 0x0949d9dc;
        public const uint TLFutureSalts = 0xae500895;
        public const uint TLGzipPacked = 0x3072cfa1;
        public const uint TLState = 0xa56c2a3e;
        public const uint TLFileTypeUnknown = 0xaa963b05;
        public const uint TLFileTypeJpeg = 0x7efe0e;
        public const uint TLFileTypeGif = 0xcae1aadf;
        public const uint TLFileTypePng = 0x0a4f63c0;
        public const uint TLFileTypeMp3 = 0x528a0677;
        public const uint TLFileTypeMov = 0x4b09ebbc;
        public const uint TLFileTypePartial = 0x40bc6f52;
        public const uint TLFileTypeMp4 = 0xb3cea0e4;
        public const uint TLFileTypeWebp = 0x1081464c;
        public const uint TLFile = 0x096a18d5;
        public const uint TLInputFileLocation = 0x14637196;
        public const uint TLInputVideoFileLocation = 0x3d0364ec;
        public const uint TLInviteText = 0x18cb9f78;
        public const uint TLDHGenOk = 0x3bcbf734;
        public const uint TLDHGenRetry = 0x46dc1fb9;
        public const uint TLDHGenFail = 0xa69dae02;
        public const uint TLServerDHInnerData = 0xb5890dba;
        public const uint TLServerDHParamsFail = 0x79cb045d;
        public const uint TLServerDHParamsOk = 0xd0e8075c;
        public const uint TLPQInnerData = 0x83c95aec;
        public const uint TLPQInnerDataDC = 0xa9f55f95;
        public const uint TLResPQ = 0x05162463;
        public const uint TLContactsBlocked = 0x1c138d15;
        public const uint TLContactsBlockedSlice = 0x900802a1;
        public const uint TLContactBlocked = 0x561bc879;
        public const uint TLImportedContacts = 0xad524315;
        public const uint TLImportedContact = 0xd0028438;
        public const uint TLInputContact = 0xf392b7f4;
        public const uint TLContactStatus = 0xaa77b873;
        public const uint TLForeignLinkUnknown = 0x133421f8;
        public const uint TLForeignLinkRequested = 0xa7801f47;
        public const uint TLForeignLinkMutual = 0x1bea8ce1;
        public const uint TLMyLinkEmpty = 0xd22a1c60;
        public const uint TLMyLinkRequested = 0x6c69efee;
        public const uint TLMyLinkContact = 0xc240ebd9;
        public const uint TLLink = 0xeccea3f5;
        public const uint TLUserFull = 0x771095da;
        public const uint TLPhotos = 0x8dca6aa5;
        public const uint TLPhotosSlice = 0x15051f54;
        public const uint TLPhotosPhoto = 0x20212ca8;
        public const uint TLInputPeerNotifyEventsEmpty = 0xf03064d8;
        public const uint TLInputPeerNotifyEventsAll = 0xe86a2c74;
        public const uint TLInputPeerNotifySettings = 0x46a2ce98;
        public const uint TLInputNotifyPeer = 0xb8bc5b0c;
        public const uint TLInputNotifyUsers = 0x193b4417;
        public const uint TLInputNotifyChats = 0x4a95e84e;
        public const uint TLInputNotifyAll = 0xa429b886;
        public const uint TLInputUserEmpty = 0xb98886cf;
        public const uint TLInputUserSelf = 0xf7c1b13f;
        public const uint TLInputUserContact = 0x86e94f65;
        public const uint TLInputUserForeign = 0x655e74ff;
        public const uint TLInputPhotoCropAuto = 0xade6b004;
        public const uint TLInputPhotoCrop = 0xd9915325;
        public const uint TLInputChatPhotoEmpty = 0x1ca48f57;
        public const uint TLInputChatUploadedPhoto = 0x94254732;
        public const uint TLInputChatPhoto = 0xb2e1bf08;
        public const uint TLMessagesChatFull = 0xe5d7d19c;
        public const uint TLChatFull = 0x630e61be;
        public const uint TLChatParticipant = 0xc8d7493e;
        public const uint TLChatParticipantsForbidden = 0x0fd2bb8a;
        public const uint TLChatParticipants = 0x7841b415;
        public const uint TLPeerNotifySettingsEmpty = 0x70a68512;
        public const uint TLPeerNotifySettings = 0x8d5e11ee;
        public const uint TLPeerNotifyEventsEmpty = 0xadd53cb3;
        public const uint TLPeerNotifyEventsAll = 0x6d1ded88;
        public const uint TLChats = 0x8150cbd8;
        public const uint TLMessages = 0x8c718e87;
        public const uint TLMessagesSlice = 0x0b446ae3;
        public const uint TLExportedAuthorization = 0xdf969c2d;
        public const uint TLInputFile = 0xf52ff27f;
        public const uint TLInputPhotoEmpty = 0x1cd7bf0d;
        public const uint TLInputPhoto = 0xfb95c6c4;
        public const uint TLInputGeoPointEmpty = 0xe4c123d6;
        public const uint TLInputGeoPoint = 0xf3b7acc9;
        public const uint TLInputVideoEmpty = 0x5508ec75;
        public const uint TLInputVideo = 0xee579652;
        public const uint TLInputMediaEmpty = 0x9664f57f;
        public const uint TLInputMediaUploadedPhoto = 0x2dc53a7d;
        public const uint TLInputMediaPhoto = 0x8f2ab2ec;
        public const uint TLInputMediaGeoPoint = 0xf9c44144;
        public const uint TLInputMediaContact = 0xa6e45987;
        public const uint TLInputMediaUploadedVideo = 0x133ad6f6;
        public const uint TLInputMediaUploadedThumbVideo = 0x9912dabf;
        public const uint TLInputMediaVideo = 0x7f023ae6;
        public const uint TLInputMessageFilterEmpty = 0x57e2f66c;
        public const uint TLInputMessageFilterPhoto = 0x9609a51c;
        public const uint TLInputMessageFilterVideo = 0x9fc00e65;
        public const uint TLInputMessageFilterPhotoVideo = 0x56e9f0e4;
        public const uint TLInputMessageFilterPhotoVideoDocument = 0xd95e73bb;
        public const uint TLInputMessageFilterDocument = 0x9eddf188;
        public const uint TLInputMessageFilterAudio = 0xcfc87522;
        public const uint TLInputMessageFilterAudioDocuments = 0x5afbf764;
        public const uint TLInputMessageFilterUrl = 0x7ef0dd87;
        public const uint TLStatedMessage = 0xd07ae726;
        public const uint TLStatedMessageLink = 0xa9af2881;
        public const uint TLStatedMessages = 0x969478bb;
        public const uint TLStatedMessagesLinks = 0x3e74f5c6;
        public const uint TLAffectedHistory = 0xb7de36f2;
        public const uint TLNull = 0x56730bcc;
        public const uint TLChatEmpty = 0x9ba2d800;
        public const uint TLChat = 0x6e9c9bc7;
        public const uint TLChatForbidden = 0xfb0ccc41;
        public const uint TLSentMessage = 0xd1f4d35c;
        public const uint TLSentMessageLink = 0xe9db4a3f;
        public const uint TLMessageEmpty = 0x83e5de54;
        public const uint TLMessage = 0x22eb6aba;
        public const uint TLMessageForwarded = 0x05f46804;
        public const uint TLMessageService = 0x9f8d60bb;
        public const uint TLMessageMediaEmpty = 0x3ded6320;
        public const uint TLMessageMediaPhoto = 0xc8c45a2a;
        public const uint TLMessageMediaVideo = 0xa2d24290;
        public const uint TLMessageMediaGeo = 0x56e0d474;
        public const uint TLMessageMediaContact = 0x5e7d2f39;
        public const uint TLMessageMediaUnsupported = 0x29632a36;
        public const uint TLMessageActionEmpty = 0xb6aef7b0;
        public const uint TLMessageActionChatCreate = 0xa6638b9a;
        public const uint TLMessageActionChatEditTitle = 0xb5a1ce5a;
        public const uint TLMessageActionChatEditPhoto = 0x7fcb13a8;
        public const uint TLMessageActionChatDeletePhoto = 0x95e3fbef;
        public const uint TLMessageActionChatAddUser = 0x5e3cfc4b;
        public const uint TLMessageActionChatDeleteUser = 0xb2ae9b0c;
        public const uint TLPhotoEmpty = 0x2331b22d;
        public const uint TLPhoto = 0x22b56751;
        public const uint TLPhotoSizeEmpty = 0x0e17e23c;
        public const uint TLPhotoSize = 0x77bfb61b;
        public const uint TLPhotoCachedSize = 0xe9a734fa;
        public const uint TLVideoEmpty = 0xc10658a8;
        public const uint TLVideo = 0x388fa391;
        public const uint TLGeoPointEmpty = 0x1117dd5f;
        public const uint TLGeoPoint = 0x2049d70c;
        public const uint TLDialogs = 0x15ba6c40;
        public const uint TLDialogsSlice = 0x71e094f3;
        public const uint TLDialog = 0xab3a99ac;
        public const uint TLInputPeerEmpty = 0x7f3b18ea;
        public const uint TLInputPeerSelf = 0x7da07ec9;
        public const uint TLInputPeerContact = 0x1023dbe8;
        public const uint TLInputPeerForeign = 0x9b447325;
        public const uint TLInputPeerChat = 0x179be863;
        public const uint TLPeerUser = 0x9db1bc6d;
        public const uint TLPeerChat = 0xbad0e5bb;
        public const uint TLVector = 0x1cb5c415;
        public const uint TLUserStatusEmpty = 0x09d05049;
        public const uint TLUserStatusOnline = 0xedb93949;
        public const uint TLUserStatusOffline = 0x8c703f;
        public const uint TLChatPhotoEmpty = 0x37c1011c;
        public const uint TLChatPhoto = 0x6153276a;
        public const uint TLUserProfilePhotoEmpty = 0x4f11bae1;
        public const uint TLUserProfilePhoto = 0xd559d8c8;
        public const uint TLUserEmpty = 0x200250ba;
        public const uint TLUserSelf = 0x720535ec;
        public const uint TLUserContact = 0xf2fb8319;
        public const uint TLUserRequest = 0x22e8ceb0;
        public const uint TLUserForeign = 0x5214c89d;
        public const uint TLUserDeleted = 0xb29ad7cc;
        public const uint TLSentCode = 0xefed51d9;
        public const uint TLRPCResult = 0xf35c6d01;
        public const uint TLRPCError = 0x2144ca19;
        public const uint TLRPCReqError = 0x7ae432f5;
        public const uint TLNewSessionCreated = 0x9ec20908;
        public const uint TLNearestDC = 0x8e1a1775;
        public const uint TLMessagesAcknowledgment = 0x62d6b459;
        public const uint TLContainer = 0x73f1f8dc;
        public const uint TLFileLocationUnavailable = 0x7c596b46;
        public const uint TLFileLocation = 0x53d69076;
        public const uint TLDCOption = 0x2ec2a43c;
        public const uint TLContacts = 0x6f8b8cb2;
        public const uint TLContactsNotModified = 0xb74ba9d2;
        public const uint TLContact = 0xf911c994;
        public const uint TLConfig = 0x2e54dd74;
        public const uint TLConfig23 = 0x7dae33e0; 
        public const uint TLCheckedPhone = 0xe300cc3b;
        public const uint TLBadServerSalt = 0xedab447b;
        public const uint TLBadMessageNotification = 0xa7eff811;
        public const uint TLAuthorization = 0xf6b673a4;
        public const uint TLWallPaper = 0xccb03657;
        public const uint TLWallPaperSolid = 0x63117f24;
        public const uint TLPing = 0x7abe77ec;
        public const uint TLPong = 0x347773c5;
        public const uint TLPingDelayDisconnect = 0xf3427b8c;
        public const uint TLContactFound = 0xea879f95;
        public const uint TLContactsFound = 0x566000e;

        // layer 16
        public const uint TLSentAppCode = 0xe325edcf;

        // layer 17
        public const uint TLSendMessageTypingAction = 0x16bf744e;
        public const uint TLSendMessageCancelAction = 0xfd5ec8f5;
        public const uint TLSendMessageRecordVideoAction = 0xa187d66f;
        public const uint TLSendMessageUploadVideoAction = 0x92042ff7;
        public const uint TLSendMessageRecordAudioAction = 0xd52f73f7;
        public const uint TLSendMessageUploadAudioAction = 0xe6ac8a6f;
        public const uint TLSendMessageUploadPhotoAction = 0x990a3c1a;
        public const uint TLSendMessageUploadDocumentAction = 0x8faee98e;
        public const uint TLSendMessageGeoLocationAction = 0x176f8ba1;
        public const uint TLSendMessageChooseContactAction = 0x628cbc6f;
        public const uint TLUpdateUserTyping17 = 0x5c486927;
        public const uint TLUpdateChatUserTyping17 = 0x9a65ea1f;
        public const uint TLMessage17 = 0x567699b3;
        public const uint TLMessageForwarded17 = 0xa367e716;
        public const uint TLMessageService17 = 0x1d86f70e;

        // layer 17 encrypted
        public const uint TLDecryptedMessage17 = 0x204d3878;
        public const uint TLDecryptedMessageService17 = 0x73164160;
        public const uint TLDecryptedMessageMediaVideo17 = 0x524a415d;
        public const uint TLDecryptedMessageMediaAudio17 = 0x57e0a9cb;
        public const uint TLDecryptedMessageLayer17 = 0x1be31789;
        public const uint TLDecryptedMessageActionResend = 0x511110b0;
        public const uint TLDecryptedMessageActionTyping = 0xccb27641;

        // layer 18
        public const uint TLUpdateServiceNotification = 0x382dd3e4;
        public const uint TLUserSelf18 = 0x7007b451;
        public const uint TLUserContact18 = 0xcab35e18;
        public const uint TLUserRequest18 = 0xd9ccc4ef;
        public const uint TLUserForeign18 = 0x75cf7a8;
        public const uint TLUserDeleted18 = 0xd6016d7a;

        // layer 19
        public const uint TLUserStatusRecently = 0xe26f42f1;
        public const uint TLUserStatusLastWeek = 0x7bf09fc;
        public const uint TLUserStatusLastMonth = 0x77ebc742;
        public const uint TLContactStatus19 = 0xd3680c61;
        public const uint TLUpdatePrivacy = 0xee3b272a;
        public const uint TLInputPrivacyKeyStatusTimestamp = 0x4f96cb18;
        public const uint TLPrivacyKeyStatusTimestamp = 0xbc2eab30;
        public const uint TLInputPrivacyValueAllowContacts = 0xd09e07b;
        public const uint TLInputPrivacyValueAllowAll = 0x184b35ce;
        public const uint TLInputPrivacyValueAllowUsers = 0x131cc67f;
        public const uint TLInputPrivacyValueDisallowContacts = 0xba52007;
        public const uint TLInputPrivacyValueDisallowAll = 0xd66b66c9;
        public const uint TLInputPrivacyValueDisallowUsers = 0x90110467;
        public const uint TLPrivacyValueAllowContacts = 0xfffe1bac;
        public const uint TLPrivacyValueAllowAll = 0x65427b82;
        public const uint TLPrivacyValueAllowUsers = 0x4d5bbe0c;
        public const uint TLPrivacyValueDisallowContacts = 0xf888fa1a;
        public const uint TLPrivacyValueDisallowAll = 0x8b73e763;
        public const uint TLPrivacyValueDisallowUsers = 0xc7f49b7;
        public const uint TLPrivacyRules = 0x554abb6f;
        public const uint TLAccountDaysTTL = 0xb8d0afdf;

        // layer 20
        public const uint TLSentChangePhoneCode = 0xa4f58c4c;
        public const uint TLUpdateUserPhone = 0x12b9417b;

        // layer 20 encrypted
        public const uint TLDecryptedMessageActionRequestKey = 0xf3c9611b;
        public const uint TLDecryptedMessageActionAcceptKey = 0x6fe1735b;
        public const uint TLDecryptedMessageActionAbortKey = 0xdd05ec6b;
        public const uint TLDecryptedMessageActionCommitKey = 0xec2e0b9b;
        public const uint TLDecryptedMessageActionNoop = 0xa82fdd63;
        
        // layer 21

        // layer 22
        public const uint TLInputMediaUploadedDocument22 = 0xffe76b78;
        public const uint TLInputMediaUploadedThumbDocument22 = 0x41481486;
        public const uint TLDocument22 = 0xf9a39f4f;
        public const uint TLDocumentAttributeImageSize = 0x6c37c15c;
        public const uint TLDocumentAttributeAnimated = 0x11b58939;
        public const uint TLDocumentAttributeSticker = 0xfb0a5727;
        public const uint TLDocumentAttributeVideo = 0x5910cccb;
        public const uint TLDocumentAttributeAudio = 0x51448e5;
        public const uint TLDocumentAttributeFileName = 0x15590068;
        public const uint TLStickersNotModified = 0xf1749a22;
        public const uint TLStickers = 0x8a8ecd32;
        public const uint TLStickerPack = 0x12b299d4;
        public const uint TLAllStickersNotModified = 0xe86602c3;
        public const uint TLAllStickers = 0xdcef3102;

        // layer 23
        public const uint TLDisabledFeature = 0xae636f24;

        // layer 23 encrypted
        public const uint TLDecryptedMessageMediaExternalDocument = 0xfa95b0dd;

        // layer 24
        public const uint TLUpdateNewMessage24 = 0x1f2b0afd;
        public const uint TLUpdateReadMessages24 = 0x2e5ab668;
        public const uint TLUpdateDeleteMessages24 = 0xa20db0e5;
        public const uint TLUpdateShortMessage24 = 0xb87da3b1;
        public const uint TLUpdateShortChatMessage24 = 0x20e85ded;
        public const uint TLUpdateReadHistoryInbox = 0x9961fd5c;
        public const uint TLUpdateReadHistoryOutbox = 0x2f2f21bf;
        public const uint TLDialog24 = 0xc1dd804a;
        public const uint TLStatedMessages24 = 0x7d84b48;
        public const uint TLStatedMessagesLinks24 = 0x51be5d19;
        public const uint TLStatedMessage24 = 0x96240c6a;
        public const uint TLStatedMessageLink24 = 0x948a288;
        public const uint TLSentMessage24 = 0x900eac40;
        public const uint TLSentMessageLink24 = 0xe923400d;
        public const uint TLAffectedMessages = 0x84d19185;
        public const uint TLAffectedHistory24 = 0xb45c69d1;
        public const uint TLMessageMediaUnsupported24 = 0x9f84f49e;
        public const uint TLChats24 = 0x64ff9fd5;
        public const uint TLUserSelf24 = 0x1c60e608;
        public const uint TLCheckedPhone24 = 0x811ea28e;
        public const uint TLContactLinkUnknown = 0x5f4f9247;
        public const uint TLContactLinkNone = 0xfeedd3ad;
        public const uint TLContactLinkHasPhone = 0x268f3f59;
        public const uint TLContactLink = 0xd502c2d0;
        public const uint TLUpdateContactLink24 = 0x9d2e67c5;
        public const uint TLLink24 = 0x3ace484c;
        public const uint TLConfig24 = 0x3e6f732a;

        // layer 25
        public const uint TLMessage25 = 0xa7ab1991;
        public const uint TLDocumentAttributeSticker25 = 0x994c9882;
        public const uint TLUpdatesShortMessage25 = 0xed5c2127;
        public const uint TLUpdatesShortChatMessage25 = 0x52238b3c;

        // layer 26
        public const uint TLSentMessage26 = 0x4c3d47f3;
        public const uint TLSentMessageLink26 = 0x35a1a663;
        public const uint TLConfig26 = 0x68bac247;
        public const uint TLUpdateWebPage = 0x2cc36971;
        public const uint TLWebPageEmpty = 0xeb1477e8;
        public const uint TLWebPagePending = 0xc586da1c;
        public const uint TLWebPage = 0xa31ea0b5;
        public const uint TLMessageMediaWebPage = 0xa32dd600;
        public const uint TLAccountAuthorization = 0x7bf2e6f6;
        public const uint TLAccountAuthorizations = 0x1250abde;

        // layer 27
        public const uint TLNoPassword = 0x96dabc18;
        public const uint TLPassword = 0x7c18141c;
        public const uint TLPasswordSettings = 0xb7b72ab3;
        public const uint TLPasswordInputSettings = 0x86916deb; //0xbcfc532c;
        public const uint TLPasswordRecovery = 0x137948a5;

        // layer 28
        public const uint TLInvokeWithoutUpdates = 0xbf9459b7;
        public const uint TLInputMediaUploadedPhoto28 = 0xf7aff1c0;
        public const uint TLInputMediaPhoto28 = 0xe9bfb4f3;
        public const uint TLInputMediaUploadedVideo28 = 0xe13fd4bc;
        public const uint TLInputMediaUploadedThumbVideo28 = 0x96fb97dc;
        public const uint TLInputMediaVideo28 = 0x936a4ebd;
        public const uint TLSendMessageUploadVideoAction28 = 0xe9763aec;
        public const uint TLSendMessageUploadAudioAction28 = 0xf351d7ab;
        public const uint TLSendMessageUploadDocumentAction28 = 0xaa0cd9e4;
        public const uint TLSendMessageUploadPhotoAction28 = 0xd1d34a26;
        public const uint TLInputMediaVenue = 0x2827a81a;
        public const uint TLMessageMediaVenue = 0x7912b71f;
        public const uint TLReceivedNotifyMessage = 0xa384b779;
        public const uint TLChatInviteEmpty = 0x69df3769;
        public const uint TLChatInviteExported = 0xfc2e05bc;
        public const uint TLChatInviteAlready = 0x5a686d7c;
        public const uint TLChatInvite = 0xce917dcd;
        public const uint TLMessageActionChatJoinedByLink = 0xf89cf5e8;
        public const uint TLUpdateReadMessagesContents = 0x68c13933;
        public const uint TLChatFull28 = 0xcade0791;
        public const uint TLConfig28 = 0x4e32b894;
        public const uint TLMessageMediaPhoto28 = 0x3d8ce53d;
        public const uint TLMessageMediaVideo28 = 0x5bcf1675;
        public const uint TLPhoto28 = 0xc3838076;
        public const uint TLVideo28 = 0xee9f4a4d;

        // layer 29
        public const uint TLDocumentAttributeSticker29 = 0x3a556302;
        public const uint TLAllStickers29 = 0x5ce352ec;
        public const uint TLInputStickerSetEmpty = 0xffb62b95;
        public const uint TLInputStickerSetId = 0x9de7a269;
        public const uint TLInputStickerSetShortName = 0x861cc8a0;
        public const uint TLStickerSet = 0xa7a43b17;
        public const uint TLMessagesStickerSet = 0xb60a24a6;

        // layer 30
        public const uint TLDCOption30 = 0x5d8c6cc;

        // layer 31
        public const uint TLAuthorization31 = 0xff036af1;
        public const uint TLMessage31 = 0xc3060325;
        public const uint TLChatFull31 = 0x2e02a614;
        public const uint TLUserFull31 = 0x5a89ac5b;
        public const uint TLUser = 0x22e49072;
        public const uint TLBotCommand = 0xc27ac8c7;
        public const uint TLBotInfoEmpty = 0xbb2e37ce;
        public const uint TLBotInfo = 0x9cf585d;
        public const uint TLKeyboardButton = 0xa2fa4880;
        public const uint TLKeyboardButtonRow = 0x77608b83;
        public const uint TLReplyKeyboardMarkup = 0x3502758c;
        public const uint TLReplyKeyboardHide = 0xa03e5b85;
        public const uint TLReplyKeyboardForceReply = 0xf4108aa0;

        // layer 32
        public const uint TLDocumentAttributeAudio32 = 0xded218e0;
        public const uint TLAllStickers32 = 0xd51dafdb;
        public const uint TLStickerSet32 = 0xcd303b41;

        // layer 33
        public const uint TLInputPeerUser = 0x7b8e7de6;
        public const uint TLInputUser = 0xd8292816;
        public const uint TLPhoto33 = 0xcded42fe;
        public const uint TLVideo33 = 0xf72887d3;
        public const uint TLAudio33 = 0xf9e35055;
        public const uint TLAppChangelogEmpty = 0xaf7e0394;
        public const uint TLAppChangelog = 0x4668e6bd;

        // layer 34
        public const uint TLMessageEntityUnknown = 0xbb92ba95;
        public const uint TLMessageEntityMention = 0xfa04579d;
        public const uint TLMessageEntityHashtag = 0x6f635b0d;
        public const uint TLMessageEntityBotCommand = 0x6cef8ac7;
        public const uint TLMessageEntityUrl = 0x6ed02538;
        public const uint TLMessageEntityEmail = 0x64e475c2;
        public const uint TLMessageEntityBold = 0xbd610bc9;
        public const uint TLMessageEntityItalic = 0x826f8b60;
        public const uint TLMessageEntityCode = 0x28a20571;
        public const uint TLMessageEntityPre = 0x73924be0;
        public const uint TLMessageEntityTextUrl = 0x76a6d327;
        public const uint TLMessage34 = 0xf07814c8;
        public const uint TLSentMessage34 = 0x8a99d8e0;
        public const uint TLUpdatesShortMessage34 = 0x3f32d858;
        public const uint TLUpdatesShortChatMessage34 = 0xf9409b3d;

        // layer 35
        public const uint TLWebPage35 = 0xca820ed7;

        // layer 36
        public const uint TLInputMediaUploadedVideo36 = 0x82713fdf;
        public const uint TLInputMediaUploadedThumbVideo36 = 0x7780ddf9;
        public const uint TLMessage36 = 0x2bebfa86;
        public const uint TLUpdatesShortSentMessage = 0x11f1331c;

        // layer 37
        public const uint TLChatParticipantsForbidden37 = 0xfc900c2b;
        public const uint TLUpdateChatParticipantAdd37 = 0xea4b0e5c;
        public const uint TLUpdateWebPage37 = 0x7f891213;

        // layer 40
        public const uint TLInputPeerChannel = 0x20adaef8;
        public const uint TLPeerChannel = 0xbddde532;
        public const uint TLChat40 = 0x7312bc48;
        public const uint TLChatForbidden40 = 0x7328bdb;
        public const uint TLChannel = 0x678e9587; //0x1bcc63f2;
        public const uint TLChannelForbidden = 0x2d85832c;
        public const uint TLChannelFull = 0xfab31aa3; //0xf6945b65;
        public const uint TLChannelParticipants40 = 0xb561ad0c;
        public const uint TLMessage40 = 0x5ba66c13;
        public const uint TLMessageService40 = 0xc06b9607;
        public const uint TLMessageActionChannelCreate = 0x95d2ac92;
        public const uint TLMessageActionToggleComments = 0xf2863903;
        public const uint TLDialogChannel = 0x5b8496b2;
        public const uint TLChannelMessages = 0xbc0f17bc;
        public const uint TLUpdateChannelTooLong = 0x60946422;
        public const uint TLUpdateChannelGroup = 0xc36c1e3c;
        public const uint TLUpdateNewChannelMessage = 0x62ba04d9;
        public const uint TLUpdateReadChannelInbox = 0x4214f37f;// 0x87b87b7d;
        public const uint TLUpdateDeleteChannelMessages = 0xc37521c9;// 0x11da3046;
        public const uint TLUpdateChannelMessageViews = 0x98a12b4b;//0xf3349b09;
        public const uint TLUpdateChannel = 0xb6d45656;
        public const uint TLUpdatesShortMessage40 = 0xf7d91a46;
        public const uint TLUpdatesShortChatMessage40 = 0xcac7fdd2;
        public const uint TLContactsFound40 = 0x1aa1f784;
        //public const uint TLInputChatEmpty = 0xd9ff343c;
        //public const uint TLInputChat = 0x43a5b9c3;
        public const uint TLInputChannel = 0xafeb712e;// 0x30c6ce73;
        public const uint TLInputChannelEmpty = 0xee8c1e86;
        public const uint TLMessageRange = 0xae30253;
        public const uint TLMessageGroup = 0xe8346f53;
        public const uint TLChannelDifferenceEmpty = 0x3e11affb;
        public const uint TLChannelDifferenceTooLong = 0x5e167646;
        public const uint TLChannelDifference = 0x2064674e;
        public const uint TLChannelMessagesFilterEmpty = 0x94d42ee7;
        public const uint TLChannelMessagesFilter = 0xcd77d957;
        public const uint TLChannelMessagesFilterCollapsed = 0xfa01232e;
        public const uint TLResolvedPeer = 0x7f077ad9;
        public const uint TLChannelParticipant = 0x15ebac1d;
        public const uint TLChannelParticipantSelf = 0xa3289a6d; 
        public const uint TLChannelParticipantModerator = 0x91057fef;
        public const uint TLChannelParticipantEditor = 0x98192d61;
        public const uint TLChannelParticipantKicked = 0x8cc5e69a;
        public const uint TLChannelParticipantCreator = 0xe3e2e1f9;
        public const uint TLChannelParticipantsRecent = 0xde3f3c79;
        public const uint TLChannelParticipantsAdmins = 0xb4608969;
        public const uint TLChannelParticipantsKicked = 0x3c37bb7a;
        public const uint TLChannelRoleEmpty = 0xb285a0c6;
        public const uint TLChannelRoleModerator = 0x9618d975;
        public const uint TLChannelRoleEditor = 0x820bfe8c;
        public const uint TLChannelParticipants = 0xf56ee2a8;
        public const uint TLChannelsChannelParticipant = 0xd0d9b163;
        public const uint TLChatInvite40 = 0x93e99b60;
        public const uint TLChatParticipants40 = 0x3f460fed;
        public const uint TLChatParticipantCreator = 0xda13538a;
        public const uint TLChatParticipantAdmin = 0xe2d6e436;
        public const uint TLUpdateChatAdmins = 0x6e947941;
        public const uint TLUpdateChatParticipantAdmin = 0xb6901959;
        public const uint TLConfig41 = 0x6cb6e65e;
        public const uint TLMessageActionChatMigrateTo = 0x51bdb021;
        public const uint TLMessageActionChatDeactivate = 0x64ad20a8;
        public const uint TLMessageActionChatActivate = 0x40ad8cb2;
        public const uint TLMessageActionChannelMigrateFrom = 0xb055eaee;
        public const uint TLChannelParticipantsBots = 0xb0d1865b;
        public const uint TLChat41 = 0xd91cdd54;
        public const uint TLChannelFull41 = 0x9e341ddf;
        public const uint TLMessageActionChatAddUser41 = 0x488a7337;

        // layer 42
        public const uint TLInputReportReasonSpam = 0x58dbcab8;
        public const uint TLInputReportReasonViolence = 0x1e22c78d;
        public const uint TLInputReportReasonPornography = 0x2e59d922;
        public const uint TLInputReportReasonOther = 0xe1746d0a;
        public const uint TLTermsOfService = 0xf1ee3e90;

        // layer 43
        public const uint TLUpdateNewStickerSet = 0x688a30aa;
        public const uint TLUpdateStickerSetsOrder = 0xf0dfb451;
        public const uint TLUpdateStickerSets = 0x43ae3dec;
        public const uint TLAllStickers43 = 0xedfd405f;

        // layer 44
        public const uint TLInputMediaGifExternal = 0x4843b0fd;
        public const uint TLUser44 = 0x603539b4;
        public const uint TLChannel44 = 0x4b1b7506;
        public const uint TLInputMessagesFilterGif = 0xffc86587;
        public const uint TLUpdateSavedGifs = 0x9375341e;
        public const uint TLConfig44 = 0x6bbc5f8;
        public const uint TLFoundGif = 0x162ecc1f;
        public const uint TLFoundGifCached = 0x9c750409;
        public const uint TLFoundGifs = 0x450a1c0a;
        public const uint TLSavedGifsNotModified = 0xe8025ca2;
        public const uint TLSavedGifs = 0x2e0709a5;

        // layer 45
        public const uint TLInputMediaUploadedDocument45 = 0x1d89306d;
        public const uint TLInputMediaUploadedThumbDocument45 = 0xad613491;
        public const uint TLInputMediaDocument45 = 0x1a77f29c;
        public const uint TLUser45 = 0xd10d979a;
        public const uint TLMessage45 = 0xc992e15c;
        public const uint TLMessageMediaDocument45 = 0xf3e02ea8;
        public const uint TLUpdateBotInlineQuery = 0xc01eea08;
        public const uint TLUpdatesShortMessage45 = 0x13e4deaa;
        public const uint TLUpdatesShortChatMessage45 = 0x248afa62;
        public const uint TLInputBotInlineMessageMediaAuto = 0x2e43e587;
        public const uint TLInputBotInlineMessageText = 0xadf0df71;
        public const uint TLInputBotInlineResult = 0x2cbbe15a;
        public const uint TLBotInlineMessageMediaAuto = 0xfc56e87d;
        public const uint TLBotInlineMessageText = 0xa56197a9;
        public const uint TLBotInlineMediaResultDocument = 0xf897d33e;
        public const uint TLBotInlineMediaResultPhoto = 0xc5528587;
        public const uint TLBotInlineResult = 0x9bebaeb9;
        public const uint TLBotResults = 0x1170b0a3;

        // layer 46
        public const uint TLDocumentAttributeAudio46 = 0x9852f9c6;
        public const uint TLInputMessagesFilterVoice = 0x50f5c392;
        public const uint TLInputMessagesFilterMusic = 0x3751b49e;
        public const uint TLInputPrivacyKeyChatInvite = 0xbdfb0426;
        public const uint TLPrivacyKeyChatInvite = 0x500e6dfa;

        // layer 48
        public const uint TLMessage48 = 0xc09be45f;
        public const uint TLInputPeerNotifySettings48 = 0x38935eb2;
        public const uint TLPeerNotifySettings48 = 0x9acda4c0;
        public const uint TLUpdateEditChannelMessage = 0x1b3f4df7;
        public const uint TLUpdatesShortMessage48 = 0x914fbf11;
        public const uint TLUpdatesShortChatMessage48 = 0x16812688;
        public const uint TLConfig48 = 0x317ceef4;
        public const uint TLConfig54 = 0xf401a4bf;
        public const uint TLExportedMessageLink = 0x1f486803;
        public const uint TLMessageFwdHeader = 0xc786ddcb;
        public const uint TLMessageEditData = 0x26b5dde6;

        // layer 49
        public const uint TLPeerSettings = 0x818426cd;
        public const uint TLMessageService49 = 0x9e19a1f6;
        public const uint TLMessageActionPinMessage = 0x94bd38ed;
        public const uint TLChannel49 = 0xa14dca52;
        public const uint TLChannelFull49 = 0x97bee562;
        public const uint TLUserFull49 = 0x5932fc03;
        public const uint TLBotInfo49 = 0x98e81d3a;
        public const uint TLUpdateChannelPinnedMessage = 0x98592475;
        public const uint TLUpdateChannelTooLong49 = 0xeb0467fb;

        // layer 50
        public const uint TLSentCode50 = 0x5e002502;
        public const uint TLCodeTypeSms = 0x72a3158c;
        public const uint TLCodeTypeCall = 0x741cd3e3;
        public const uint TLCodeTypeFlashCall = 0x226ccefb;
        public const uint TLSentCodeTypeApp = 0x3dbb5986;
        public const uint TLSentCodeTypeSms = 0xc000bba2;
        public const uint TLSentCodeTypeCall = 0x5353e5a7;
        public const uint TLSentCodeTypeFlashCall = 0xab03c6d9;

        // layer 51
        public const uint TLUpdateBotCallbackQuery = 0xa68c688c;
        public const uint TLUpdateInlineBotCallbackQuery = 0x2cbd95af;
        public const uint TLUpdateBotInlineQuery51 = 0x54826690;
        public const uint TLUpdateBotInlineSend = 0xe48f964;
        public const uint TLUpdateEditMessage = 0xe40370a3;
        public const uint TLKeyboardButtonUrl = 0x258aff05;
        public const uint TLKeyboardButtonCallback = 0x683a5e46;
        public const uint TLKeyboardButtonRequestPhone = 0xb16a6c29;
        public const uint TLKeyboardButtonRequestGeoLocation = 0xfc796b3f;
        public const uint TLKeyboardButtonSwitchInline = 0xea1b7a14;
        public const uint TLBotCallbackAnswer = 0x1264f1c6;
        public const uint TLReplyInlineMarkup = 0x48a30254;
        public const uint TLInputBotInlineMessageMediaAuto51 = 0x292fed13;
        public const uint TLInputBotInlineMessageText51 = 0x3dcd7a87;
        public const uint TLInputBotInlineMessageMediaGeo = 0xf4a59de1;
        public const uint TLInputBotInlineMessageMediaVenue = 0xaaafadc8;
        public const uint TLInputBotInlineMessageMediaContact = 0x2daf01a7;
        public const uint TLInputBotInlineResultPhoto = 0xa8d864a7;
        public const uint TLInputBotInlineResultDocument = 0xfff8fdc4;
        public const uint TLBotInlineMessageMediaAuto51 = 0xa74b15b;
        public const uint TLBotInlineMessageText51 = 0x8c7f65e2;
        public const uint TLBotInlineMessageMediaGeo = 0x3a8fd8b8;
        public const uint TLBotInlineMessageMediaVenue = 0x4366232e;
        public const uint TLBotInlineMessageMediaContact = 0x35edb4d4;
        public const uint TLBotInlineMediaResult = 0x17db940b;
        public const uint TLInputBotInlineMessageId = 0x890c3d89;
        public const uint TLBotResults51 = 0x256709a6;
        public const uint TLInlineBotSwitchPM = 0x3c20629f;

        // layer 52
        public const uint TLConfig52 = 0xc9411388;
        public const uint TLMessageEntityMentionName = 0x352dca58;
        public const uint TLInputMessageEntityMentionName = 0x208e68c9;
        public const uint TLPeerDialogs = 0x3371c354;
        public const uint TLTopPeer = 0xedcdc05b;
        public const uint TLTopPeerCategoryBotsPM = 0xab661b5b;
        public const uint TLTopPeerCategoryBotsInline = 0x148677e2;
        public const uint TLTopPeerCategoryCorrespondents = 0x637b7ed;
        public const uint TLTopPeerCategoryGroups = 0xbd17a14a;
        public const uint TLTopPeerCategoryChannels = 0x161d9628;
        public const uint TLTopPeerCategoryPeers = 0xfb834291;
        public const uint TLTopPeersNotModified = 0xde266ef5;
        public const uint TLTopPeers = 0x70b772a8;

        // layer 53
        public const uint TLInputMessagesFilterChatPhotos = 0x3a20ecb8;
        public const uint TLUpdateReadChannelOutbox = 0x25d6c9c7;
        public const uint TLChannelFull53 = 0xc3d5512f;
        public const uint TLDialog53 = 0x66ffba14;
        public const uint TLChannelMessages53 = 0x99262e37;
        public const uint TLUpdateDraftMessage = 0xee2bb969;
        public const uint TLChannelDifferenceTooLong53 = 0x410dee07;
        public const uint TLChannelMessagesFilter53 = 0xcd77d957;
        public const uint TLDraftMessageEmpty = 0xba4baec5;
        public const uint TLDraftMessage = 0xfd8e711f;
        public const uint TLChannelForbidden53 = 0x8537784f;
        public const uint TLMessageActionClearHistory = 0x9fbab604;

        // layer 54
        public const uint TLFeaturedStickersNotModified = 0x4ede3cf;
        public const uint TLUpdateReadFeaturedStickers = 0x571d2742;
        public const uint TLBotCallbackAnswer54 = 0xb10df1fb;   //0x31fde6e4;
        public const uint TLInputDocumentFileLocation54 = 0x430f0724;
        public const uint TLDocument54 = 0x87232bc7;
        public const uint TLUpdateRecentStickers = 0x9a422c20;
        public const uint TLRecentStickersNotModified = 0xb17f890;
        public const uint TLRecentStickers = 0x5ce20970;
        public const uint TLChatInvite54 = 0xdb74f558;
        public const uint TLStickerSetInstallResult = 0x38641628;
        public const uint TLStickerSetInstallResultArchive = 0x35e410a8;
        public const uint TLFeaturedStickers = 0xf89d88e5;
        public const uint TLArchivedStickers = 0x4fcba9c8;
        public const uint TLStickerSetCovered = 0x6410a5d2;

        // layer 55
        public const uint TLInputMediaPhotoExternal = 0xb55f4f18;
        public const uint TLInputMediaDocumentExternal = 0xe5e9607c;
        public const uint TLAuthorization55 = 0xcd050916;
        public const uint TLUpdateConfig = 0xa229dd06;
        public const uint TLUpdatePtsChanged = 0x3354678f;
        public const uint TLConfig55 = 0x9a6b2e2a;
        public const uint TLKeyboardButtonSwitchInline55 = 0x568a748;

        // layer 56
        public const uint TLUpdateBotCallbackQuery56 = 0x81c5615f;
        public const uint TLUpdateInlineBotCallbackQuery56 = 0xd618a28b;
        public const uint TLUpdateStickerSetsOrder56 = 0xbb2d201;
        public const uint TLStickerSetMultiCovered = 0x3407e51b;
        public const uint TLInputMediaUploadedPhoto56 = 0x630c9af1;
        public const uint TLInputMediaUploadedDocument56 = 0xd070f1e9;
        public const uint TLInputMediaUploadedThumbDocument56 = 0x50d88cae;
        public const uint TLPhoto56 = 0x9288dd29;
        public const uint TLDocumentAttributeSticker56 = 0x6319d612;
        public const uint TLDocumentAttributeHasStickers = 0x9801d2f7;
        public const uint TLMaskCoords = 0xaed6dbb2;
        public const uint TLInputStickeredMediaPhoto = 0x4a992157;
        public const uint TLInputStickeredMediaDocument = 0x438865b;
        public const uint TLInputChatUploadedPhoto56 = 0x927c55b4;
        public const uint TLInputChatPhoto56 = 0x8953ad37;

        // layer 57
        public const uint TLInputMediaGame = 0xd33f43f3;
        public const uint TLInputGameId = 0x32c3e77;
        public const uint TLInputGameShortName = 0xc331e80a;
        public const uint TLGame = 0xbdf9653b;
        public const uint TLHighScore = 0x58fffcd0;
        public const uint TLHighScores = 0x9a3bfd99;
        public const uint TLInputBotInlineMessageGame = 0x4b425864;
        public const uint TLInputBotInlineResultGame = 0x4fa417f2;
        public const uint TLKeyboardButtonGame = 0x50f41ccf;
        public const uint TLMessageActionGameScore = 0x92a72876;
        public const uint TLMessageMediaGame = 0xfdb19008;

        // layer 58
        public const uint TLUserFull58 = 0xf220f3f;
        public const uint TLChatsSlice = 0x78f69146;
        public const uint TLUpdateChannelWebPage = 0x40771900;
        public const uint TLDifferenceTooLong = 0x4afe8f6d;
        public const uint TLBotResults58 = 0xccd3563d;
        public const uint TLBotCallbackAnswer58 = 0x36585ea4;

        // layer 59
        public const uint TLChatsSlice59 = 0x9cd81144;
        public const uint TLUpdateServiceNotification59 = 0xebe46819;
        public const uint TLWebPage59 = 0x5f07b4bc;
        public const uint TLWebPageNotModified = 0x85849473;
        public const uint TLAppChangelog59 = 0x2a137e7c;     
        public const uint TLTextEmpty = 0xdc3d824f;
        public const uint TLTextPlain = 0x744694e0;
        public const uint TLTextBold = 0x6724abc4;
        public const uint TLTextItalic = 0xd912a59c;
        public const uint TLTextUnderline = 0xc12622c4;
        public const uint TLTextStrike = 0x9bf8bb95;
        public const uint TLTextFixed = 0x6c3f19b9;
        public const uint TLTextUrl = 0x3c2884c1;
        public const uint TLTextEmail = 0xde5a0dd6;
        public const uint TLTextConcat = 0x7e6260d7;
        public const uint TLPageBlockUnsupported = 0x13567e8a;
        public const uint TLPageBlockTitle = 0x70abc3fd;
        public const uint TLPageBlockSubtitle = 0x8ffa9a1f;
        public const uint TLPageBlockAuthorDate = 0x3d5b64f2;
        public const uint TLPageBlockHeader = 0xbfd064ec;
        public const uint TLPageBlockSubheader = 0xf12bb6e1;
        public const uint TLPageBlockParagraph = 0x467a0766;
        public const uint TLPageBlockPreformatted = 0xc070d93e;
        public const uint TLPageBlockFooter = 0x48870999;
        public const uint TLPageBlockDivider = 0xdb20b188;
        public const uint TLPageBlockAnchor = 0xce0d37b0;
        public const uint TLPageBlockList = 0x3a58c7f4;
        public const uint TLPageBlockBlockquote = 0x263d7c26;
        public const uint TLPageBlockPullquote = 0x4f4456d3;
        public const uint TLPageBlockPhoto = 0xe9c69982;
        public const uint TLPageBlockVideo = 0xd9d71866;
        public const uint TLPageBlockCover = 0x39f23300;
        public const uint TLPageBlockEmbed = 0xd935d8fb;
        public const uint TLPageBlockEmbedPost = 0x292c7be9;
        public const uint TLPageBlockCollage = 0x8b31c4f;
        public const uint TLPageBlockSlideshow = 0x130c8963;
        public const uint TLPagePart = 0x8dee6c44;
        public const uint TLPageFull = 0xd7a19d69;

        // layer 60
        public const uint TLUpdatePhoneCall = 0xab0f6b1e;
        public const uint TLConfig60 = 0xb6735d71;
        public const uint TLSendMessageGamePlayAction = 0xdd6a8f48;
        public const uint TLInputPrivacyKeyPhoneCall = 0xfabadc5f;
        public const uint TLPrivacyKeyPhoneCall = 0x3d662b7b;
        public const uint TLInputPhoneCall = 0x1e36fded;
        public const uint TLPhoneCallEmpty = 0x5366c915;
        public const uint TLPhoneCallWaiting = 0x1b8f4ad1;
        public const uint TLPhoneCallRequested = 0x6c448ae8;
        public const uint TLPhoneCall = 0xffe6ab67;
        public const uint TLPhoneCallDiscarded = 0xcc3740bd;
        public const uint TLPhoneConnection = 0x6b7411c9;
        public const uint TLPhoneCallProtocol = 0xa2bb35cb;
        public const uint TLPhonePhoneCall = 0xec82e140;

        // layer 61
        public const uint TLUpdateDialogPinned = 0xd711a2cc;
        public const uint TLUpdatePinnedDialogs = 0xd8caf68d;
        public const uint TLConfig61 = 0x3af6fb5f;
        public const uint TLPageBlockAuthorDate61 = 0xbaafe5e0;
        public const uint TLPageBlockEmbed61 = 0xcde200d1;
        public const uint TLPhoneCallDiscarded61 = 0x50ca4de1;
        public const uint TLPhoneConnection61 = 0x9d4c17c0;
        public const uint TLPhoneCallDiscardReasonMissed = 0x85e42301;
        public const uint TLPhoneCallDiscardReasonDisconnect = 0xe095c1a0;
        public const uint TLPhoneCallDiscardReasonHangup = 0x57adc690;
        public const uint TLPhoneCallDiscardReasonBusy = 0xfaf7e8c9;

        // layer 62
        public const uint TLMessageActionPhoneCall = 0x80e11a7f;
        public const uint TLInputMessagesFilterPhoneCalls = 0x80c99768;
        public const uint TLUpdateBotWebhookJSON = 0x8317c0c3;
        public const uint TLUpdateBotWebhookJSONQuery = 0x9b9240a6;
        public const uint TLDataJSON = 0x7d748d04;

        // layer 63
        public const uint TLConfig63 = 0xcb601684;

        // layer 64
        public const uint TLInputMediaInvoice = 0x92153685;
        public const uint TLMessageMediaInvoice = 0x84551347;
        public const uint TLMessageActionPaymentSentMe = 0x8f31b327;
        public const uint TLMessageActionPaymentSent = 0x40699cd0;
        public const uint TLUpdateBotShippingQuery = 0xe0cdc940;
        public const uint TLUpdateBotPrecheckoutQuery = 0x5d2f3aa9;
        public const uint TLKeyboardButtonBuy = 0xafd93fbb;
        public const uint TLLabeledPrice = 0xcb296bf8;
        public const uint TLInvoice = 0xc30aa358;
        public const uint TLPaymentCharge = 0xea02c27e;
        public const uint TLPostAddress = 0x1e8caaeb;
        public const uint TLPaymentRequestedInfo = 0x909c3f94;
        public const uint TLPaymentSavedCredentialsCard = 0xcdc27a1f;
        public const uint TLWebDocument = 0xc61acbd8;
        public const uint TLInputWebDocument = 0x9bed434d;
        public const uint TLInputWebFileLocation = 0xc239d686;
        public const uint TLWebFile = 0x21e753bc;
        public const uint TLPaymentForm = 0x3f56aea3;
        public const uint TLValidatedRequestedInfo = 0xd1451883;
        public const uint TLPaymentResult = 0x4e5f810d;
        public const uint TLPaymentVerificationNeeded = 0x6b56b921;
        public const uint TLPaymentReceipt = 0x500911e1;
        public const uint TLSavedInfo = 0xfb8fe43c;
        public const uint TLInputPaymentCredentialsSaved = 0xc10eb2cf;
        public const uint TLInputPaymentCredentials = 0x3417d728;
        public const uint TLTmpPassword = 0xdb64fd34;
        public const uint TLShippingOption = 0xb6213cdf;

        public const uint TLPhoneCallRequested64 = 0x83761ce4;
        public const uint TLPhoneCallAccepted = 0x6d003d3f;

        // layer 65, 66
        public const uint TLUser66 = 0x2e13f4c3;
        public const uint TLInputMessagesFilterRoundVoice = 0x7a7c17a4;
        public const uint TLInputMessagesFilterRoundVideo = 0xb549da53;
        public const uint TLFileCdnRedirect = 0x1508485a;
        public const uint TLSendMessageRecordRoundAction = 0x88f27fbc;
        public const uint TLSendMessageUploadRoundAction = 0x243e1c66;
        public const uint TLSendMessageUploadRoundAction66 = 0xbb718624;
        public const uint TLDocumentAttributeVideo66 = 0xef02ce6;
        public const uint TLPageBlockChannel = 0xef1751b5;
        public const uint TLCdnFileReuploadNeeded = 0xeea8e46e;
        public const uint TLCdnFile = 0xa99fca4f;
        public const uint TLCdnPublicKey = 0xc982eaba;
        public const uint TLCdnConfig = 0x5725e40a;

        // layer 67
        public const uint TLUpdateLangPackTooLong = 0x10c2404b;
        public const uint TLUpdateLangPack = 0x56022f4d;
        public const uint TLConfig67 = 0x7feec888;
        public const uint TLLangPackString = 0xcad181f6;
        public const uint TLLangPackStringPluralized = 0x6c47ac9f;
        public const uint TLLangPackStringDeleted = 0x2979eeb2;
        public const uint TLLangPackDifference = 0xf385c1f6;
        public const uint TLLangPackLanguage = 0x117698f1;

        // layer 68
        public const uint TLChannel68 = 0xcb44b1c;
        public const uint TLChannelForbidden68 = 0x289da732;
        public const uint TLChannelFull68 = 0x95cb5f57;
        public const uint TLChannelParticipantAdmin = 0xa82fa898;
        public const uint TLChannelParticipantBanned = 0x222c1886;
        public const uint TLChannelParticipantsKicked68 = 0xa3b54985;
        public const uint TLChannelParticipantsBanned = 0x1427a5e1;
        public const uint TLChannelParticipantsSearch = 0x656ac4b;
        public const uint TLTopPeerCategoryPhoneCalls = 0x1e76a78c;
        public const uint TLPageBlockAudio = 0x31b81a7f;
        public const uint TLPagePart68 = 0x8e3f9ebe;
        public const uint TLPageFull68 = 0x556ec7aa;
        public const uint TLChannelAdminRights = 0x5d7ceba5;
        public const uint TLChannelBannedRights = 0x58cf4249;
        public const uint TLChannelAdminLogEventActionChangeTitle = 0xe6dfb825;
        public const uint TLChannelAdminLogEventActionChangeAbout = 0x55188a2e;
        public const uint TLChannelAdminLogEventActionChangeUsername = 0x6a4afc38;
        public const uint TLChannelAdminLogEventActionChangePhoto = 0xb82f55c3;
        public const uint TLChannelAdminLogEventActionToggleInvites = 0x1b7907ae;
        public const uint TLChannelAdminLogEventActionToggleSignatures = 0x26ae0971;
        public const uint TLChannelAdminLogEventActionUpdatePinned = 0xe9e82c18;
        public const uint TLChannelAdminLogEventActionEditMessage = 0x709b2405;
        public const uint TLChannelAdminLogEventActionDeleteMessage = 0x42e047bb;
        public const uint TLChannelAdminLogEventActionParticipantJoin = 0x183040d3;
        public const uint TLChannelAdminLogEventActionParticipantLeave = 0xf89777f2;
        public const uint TLChannelAdminLogEventActionParticipantInvite = 0xe31c34d8;
        public const uint TLChannelAdminLogEventActionParticipantToggleBan = 0xe6d83d7e;
        public const uint TLChannelAdminLogEventActionParticipantToggleAdmin = 0xd5676710;
        public const uint TLChannelAdminLogEvent = 0x3b5a3e40;
        public const uint TLAdminLogResults = 0xed8af74d;
        public const uint TLChannelAdminLogEventsFilter = 0xea107ae4;

        // layer 69
        public const uint TLImportedContacts69 = 0x77d01c3b;
        public const uint TLPopularContact = 0x5ce14175;

        // layer 70
        public const uint TLInputMediaUploadedPhoto70 = 0x2f37e231;
        public const uint TLInputMediaPhoto70 = 0x81fa373a;
        public const uint TLInputMediaUploadedDocument70 = 0xe39621fd;
        public const uint TLInputMediaDocument70 = 0x5acb668e;
        public const uint TLInputMediaPhotoExternal70 = 0x922aec1;
        public const uint TLInputMediaDocumentExternal70 = 0xb6f74335;
        public const uint TLMessage70 = 0x90dddc11;
        public const uint TLMessageMediaPhoto70 = 0xb5223b0f;
        public const uint TLMessageMediaDocument70 = 0x7c4414d3;
        public const uint TLMessageActionScreenshotTaken = 0x4792929b;
        public const uint TLFileCdnRedirect70 = 0xea52fe5a;
        public const uint TLMessageFwdHeader70 = 0xfadff4ac;
        public const uint TLCdnFileHash = 0x77eec38f;

        // layer 71
        public const uint TLChannelFull71 = 0x17f45fcf;
        public const uint TLDialog71 = 0xe4def5db;
        public const uint TLContacts71 = 0xeae87e42;
        public const uint TLInputMessagesFilterMyMentions = 0xc1f8e69a;
        public const uint TLUpdateFavedStickers = 0xe511996d;
        public const uint TLUpdateChannelReadMessagesContents = 0x89893b45;
        public const uint TLUpdateContactsReset = 0x7084a7be;
        public const uint TLConfig71 = 0x8df376a4;
        public const uint TLChannelDifferenceTooLong71 = 0x6a9d7b35;
        public const uint TLChannelAdminLogEventActionChangeStickerSet = 0xb1c3caa7;
        public const uint TLFavedStickersNotModified = 0x9e8fa6d3;
        public const uint TLFavedStickers = 0xf37f2f16;

        // layer 72
        public const uint TLInputMediaVenue72 = 0xc13d1c11;
        public const uint TLInputMediaGeoLive = 0x7b1a118f;
        public const uint TLChannelFull72 = 0x76af5481;
        public const uint TLMessageMediaVenue72 = 0x2ec0533f;
        public const uint TLMessageMediaGeoLive = 0x7c3c2609;
        public const uint TLMessageActionCustomAction = 0xfae69f56;
        public const uint TLInputMessagesFilterGeo = 0xe7026d0d;
        public const uint TLInputMessagesFilterContacts = 0xe062db83;
        public const uint TLUpdateChannelAvailableMessages = 0x70db6837;
        public const uint TLConfig72 = 0x9c840964;
        public const uint TLBotResults72 = 0x947ca848;
        public const uint TLInputPaymentCredentialsApplePay = 0xaa1c39f;
        public const uint TLInputPaymentCredentialsAndroidPay = 0x795667a6;
        public const uint TLChannelAdminLogEventActionTogglePreHistoryHidden = 0x5f5c95f1;
        public const uint TLRecentMeUrlUnknown = 0x46e1d13d;
        public const uint TLRecentMeUrlUser = 0x8dbc3336;
        public const uint TLRecentMeUrlChat = 0xa01b22f9;
        public const uint TLRecentMeUrlChatInvite = 0xeb49081d;
        public const uint TLRecentMeUrlStickerSet = 0xbc0a57dc;
        public const uint TLRecentMeUrls = 0xe0310d7;
        public const uint TLChannelParticipantsNotModified = 0xf0173fe9;

        // layer 73
        public const uint TLChannel73 = 0x450b7115;
        public const uint TLMessage73 = 0x44f9b43d;
        public const uint TLMessageFwdHeader73 = 0x559ebe6d;
        public const uint TLInputSingleMedia = 0x5eaa7809;
        public const uint TLInputMediaInvoice73 = 0xf4e096c3;
        public const uint TLDecryptedMessage73 = 0x91cc4674;

        // layer 74
        public const uint TLContactsFound74 = 0xb3134d9d;
        public const uint TLExportedMessageLink74 = 0x5dab1af4;
        public const uint TLInputPaymentCredentialsAndroidPay74 = 0xca05d50e;

        // layer 75
        public const uint TLInputMediaUploadedPhoto75 = 0x1e287d04;
        public const uint TLInputMediaPhoto75 = 0xb3ba0635;
        public const uint TLInputMediaUploadedDocument75 = 0x5b38c6c1;
        public const uint TLInputMediaDocument75 = 0x23ab23d2;
        public const uint TLInputMediaPhotoExternal75 = 0xe5bbfe1a;
        public const uint TLInputMediaDocumentExternal75 = 0xfb52dc99;
        public const uint TLMessageMediaPhoto75 = 0x695150d7;
        public const uint TLMessageMediaDocument75 = 0x9cb070d7;
        public const uint TLInputBotInlineMessageMediaAuto75 = 0x3380c786;
        public const uint TLBotInlineMessageMediaAuto75 = 0x764cf810;
        public const uint TLInputSingleMedia75 = 0x31bc3d25;

        // layer 76
        public const uint TLChannel76 = 0xc88974ac;
        public const uint TLDialogFeed = 0x36086d42;
        public const uint TLUpdateDialogPinned76 = 0x19d27f3c;
        public const uint TLUpdatePinnedDialogs76 = 0xea4cb65b;
        public const uint TLUpdateReadFeed = 0x6fa68e41;
        public const uint TLStickerSet76 = 0x5585a139;
        public const uint TLRecentStickers76 = 0x22f3afb3;
        public const uint TLFeedPosition = 0x5059dc73;
        public const uint TLFeedMessagesNotModified = 0x4678d0cf;
        public const uint TLFeedMessages = 0x55c3a1b1;
        public const uint TLFeedBroadcasts = 0x4f4feaf1;
        public const uint TLFeedBroadcastsUngrouped = 0x9a687cba;
        public const uint TLFeedSourcesNotModified = 0x88b12a17;
        public const uint TLFeedSources = 0x8e8bca3d;
        public const uint TLInputDialogPeerFeed = 0x2c38b8cf;
        public const uint TLInputDialogPeer = 0xfcaafeb7;
        public const uint TLDialogPeerFeed = 0xda429411;
        public const uint TLDialogPeer = 0xe56dbf05;
        public const uint TLWebAuthorization = 0xcac943f2;
        public const uint TLWebAuthorizations = 0xed56c9fc;
        public const uint TLInputMessageId = 0xa676a322;
        public const uint TLInputMessageReplyTo = 0xbad88395;
        public const uint TLInputMessagePinned = 0x86872538;
        public const uint TLInputSingleMedia76 = 0x1cc6e91f;
        public const uint TLMessageEntityPhone = 0x9b69e34b;
        public const uint TLMessageEntityCashtag = 0x4c4e743f;
        public const uint TLMessageActionBotAllowed = 0xabe9affe;
        public const uint TLConfig76 = 0x86b5778e;
        public const uint TLFoundStickerSetsNotModified = 0xd54b65d;
        public const uint TLFoundStickerSets = 0x5108d648;
        public const uint TLFileHash = 0x6242c773;
        public const uint TLFileCdnRedirect76 = 0xf18cda44;
        public const uint TLInputBotInlineResult76 = 0x88bf9319;
        public const uint TLBotInlineResult76 = 0x11965f3a;
        public const uint TLWebDocumentNoProxy = 0xf9c8bcc6;

        // layer 77
        // layer 78
        public const uint TLDCOption78 = 0x18b7a10d;
        public const uint TLConfig78 = 0xeb7bb160;
        public const uint TLInputClientProxy = 0x75588b3f;
        public const uint TLProxyDataEmpty = 0xe09e1fb8;
        public const uint TLProxyDataPromo = 0x2bf7ee23;

        // layer 79
        public const uint TLInputPeerNotifySettings78 = 0x9c3d198e;
        public const uint TLPeerNotifySettings78 = 0xaf509d20;
        public const uint TLStickers79 = 0xe4599bbd;
        public const uint TLInputBotInlineMessageMediaVenue78 = 0x417bbf11;
        public const uint TLBotInlineMessageMediaVenue78 = 0x8a86659c;

        // layer 80
        public const uint TLSentCode80 = 0x38faab5f;
        public const uint TLTermsOfService80 = 0x780a0310;
        public const uint TLTermsOfServiceUpdateEmpty = 0xe3309f7f;
        public const uint TLTermsOfServiceUpdate = 0x28ecf961;

        // layer 81
        public const uint TLInputSecureFileLocation = 0xcbc7ee28;       
        public const uint TLMessageActionSecureValuesSentMe = 0x1b287353;
        public const uint TLMessageActionSecureValuesSent = 0xd95c6154;
        public const uint TLNoPassword81 = 0x5ea182f6;
        public const uint TLPassword81 = 0xca39b447;
        public const uint TLPasswordSettings81 = 0x7bd9c3f1;
        public const uint TLPasswordInputSettings81 = 0x21ffa60d;
        public const uint TLInputSecureFileUploaded = 0x3334b0f0;
        public const uint TLInputSecureFile = 0x5367e5be;
        public const uint TLSecureFileEmpty = 0x64199744;
        public const uint TLSecureFile = 0xe0277a62;
        public const uint TLSecureData = 0x8aeabec3;
        public const uint TLSecurePlainPhone = 0x7d6099dd;
        public const uint TLSecurePlainEmail = 0x21ec5a5f;
        public const uint TLSecureValueTypePersonalDetails = 0x9d2a81e3;
        public const uint TLSecureValueTypePassport = 0x3dac6a00;
        public const uint TLSecureValueTypeDriverLicense = 0x6e425c4;
        public const uint TLSecureValueTypeIdentityCard = 0xa0d0744b;
        public const uint TLSecureValueTypeInternalPassport = 0x99a48f23;
        public const uint TLSecureValueTypeAddress = 0xcbe31e26;
        public const uint TLSecureValueTypeUtilityBill = 0xfc36954e;
        public const uint TLSecureValueTypeBankStatement = 0x89137c0d;
        public const uint TLSecureValueTypeRentalAgreement = 0x8b883488;
        public const uint TLSecureValueTypePassportRegistration = 0x99e3806a;
        public const uint TLSecureValueTypeTemporaryRegistration = 0xea02ec33;
        public const uint TLSecureValueTypePhone = 0xb320aadb;
        public const uint TLSecureValueTypeEmail = 0x8e3ca7ee;
        public const uint TLSecureValue = 0xb4b4b699;
        public const uint TLInputSecureValue = 0x67872e8;
        public const uint TLSecureValueHash = 0xed1ecdb0;
        public const uint TLSecureValueErrorData = 0xe8a40bd9;
        public const uint TLSecureValueErrorFrontSide = 0xbe3dfa;
        public const uint TLSecureValueErrorReverseSide = 0x868a2aa5;
        public const uint TLSecureValueErrorSelfie = 0xe537ced6;
        public const uint TLSecureValueErrorFile = 0x7a700873;
        public const uint TLSecureValueErrorFiles = 0x666220e9;
        public const uint TLSecureCredentialsEncrypted = 0x33f0ea47;
        public const uint TLAuthorizationForm = 0xcb976d53;
        public const uint TLSentEmailCode = 0x811f854f;
        public const uint TLDeepLinkInfoEmpty = 0x66afa166;
        public const uint TLDeepLinkInfo = 0x6a4ee832;
        public const uint TLSavedPhoneContact = 0x1142bd56;
        public const uint TLTakeout = 0x4dba4501;

        // layer 82
        public const uint TLInputTakeoutFileLocation = 0x29be5899;
        public const uint TLAppUpdate = 0x1da7158f;
        public const uint TLNoAppUpdate = 0xc45a6536;
        public const uint TLInvokeWithMessagesRange = 0x365275f2;
        public const uint TLInputMediaContact82 = 0xf8ab7dfb;
        public const uint TLMessageMediaContact82 = 0xcbf24940;
        public const uint TLGeoPoint82 = 0x296f104;
        public const uint TLDialogsNotModified = 0xf0e3e596;
        public const uint TLUpdateDialogUnreadMark = 0xe16459c3;
        public const uint TLConfig82 = 0x3213dbba;
        public const uint TLInputBotInlineMessageMediaContact82 = 0xa6edbffd;
        public const uint TLBotInlineMessageMediaContact82 = 0x18d1cdc2;
        public const uint TLDraftMessageEmpty82 = 0x1b0c841a;
        public const uint TLWebDocument82 = 0x1c570ed1;
        public const uint TLInputWebFileGeoPointLocation = 0x9f2221c9;
        public const uint TLInputReportReasonCopyright = 0x9b89f93a;
        public const uint TLTopPeersDisabled = 0xb52c939d;

        // layer 83
        public const uint TLPassword83 = 0x68873ba5;
        public const uint TLPasswordSettings83 = 0x9a5c33e5;
        public const uint TLPasswordInputSettings83 = 0xc23727c9;
        public const uint TLPasswordKdfAlgoUnknown = 0xd45ab096;
        public const uint TLSecurePasswordKdfAlgoUnknown = 0x4a8537;
        public const uint TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000 = 0xbbf2dda0;
        public const uint TLSecurePasswordKdfAlgoSHA512 = 0x86471d92;
        public const uint TLSecureSecretSettings = 0x1527bcac;

        // layer 84
        public const uint TLPassword84 = 0xad2641f8;
        public const uint TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow = 0x3a912d4a;
        public const uint TLInputCheckPasswordEmpty = 0x9880f658;
        public const uint TLInputCheckPasswordSRP = 0xd27ff082;

        // layer 85
        public const uint TLSecureValue85 = 0x187fa0ca;
        public const uint TLInputSecureValue85 = 0xdb21d0a7;
        public const uint TLSecureValueError = 0x869d758f;
        public const uint TLSecureValueErrorTranslationFile = 0xa1144770;
        public const uint TLSecureValueErrorTranslationFiles = 0x34636dd8;
        public const uint TLAuthorizationForm85 = 0xad2e1cd8;
        public const uint TLSecureRequiredType = 0x829d99da;
        public const uint TLSecureRequiredTypeOneOf = 0x27477b4;
        public const uint TLPassportConfigNotModified = 0xbfb9f457;
        public const uint TLPassportConfig = 0xa098d6af;

        //
        public const uint TLConfigSimple = 0xd997c3c5;

        // layer 45 encrypted
        public const uint TLDecryptedMessage45 = 0x36b091de;
        public const uint TLDecryptedMessageMediaPhoto45 = 0xf1fa8d78;
        public const uint TLDecryptedMessageMediaVideo45 = 0x970c8c0e;
        public const uint TLDecryptedMessageMediaDocument45 = 0x7afe8ae2;
        public const uint TLDecryptedMessageMediaVenue = 0x8a0df56f;
        public const uint TLDecryptedMessageMediaWebPage = 0xe50511d8;

        // additional signatures
        public const uint TLMessageActionChannelJoined = 0xffffff00;
        public const uint TLUserExtendedInfo = 0xffffff01;
        public const uint TLDialogSecret = 0xffffff02;
        public const uint TLDecryptedMessageActionEmpty = 0xffffff03;
        public const uint TLPeerEncryptedChat = 0xffffff04;
        public const uint TLBroadcastChat = 0xffffff05;
        public const uint TLPeerBroadcastChat = 0xffffff06;
        public const uint TLBroadcastDialog = 0xffffff07;
        public const uint TLInputPeerBroadcast = 0xffffff08;
        public const uint TLServerFile = 0xffffff09;
        public const uint TLMessageActionContactRegistered = 0xffffff0a;
        public const uint TLPasscodeParams = 0xffffff0b;
        public const uint TLRecentlyUsedSticker = 0xffffff0c;
        public const uint TLActionInfo = 0xffffff0d;
        public const uint TLResultInfo = 0xffffff0e;
        public const uint TLEncryptedChat20 = 0xffffff0f;
        public const uint TLEncryptedChat17 = 0xffffff10;
        public const uint TLMessageActionUnreadMessages = 0xffffff11;
        public const uint TLMessagesContainter = 0xffffff12;
        public const uint TLHashtagItem = 0xffffff13;
        public const uint TLMessageActionMessageGroup = 0xffffff14;
        public const uint TLChatSettings = 0xffffff15;
        public const uint TLDocumentExternal = 0xffffff16;
        public const uint TLDecryptedMessagesContainter = 0xffffff17;
        public const uint TLCameraSettings = 0xffffff18;
        public const uint TLPhotoPickerSettings = 0xffffff19;
        public const uint TLProxyConfig = 0xffffff1a;
        public const uint TLCallsSecurity = 0xffffff1b;
        public const uint TLStickerSetEmpty = 0xffffff1c;
        public const uint TLMessagesGroup = 0xffffff1d;
        public const uint TLMessageMediaGroup = 0xffffff1e;
        public const uint TLDecryptedMessageMediaGroup = 0xffffff1f;
        public const uint TLPeerFeed = 0xffffff20;
        public const uint TLInputPeerFeed = 0xffffff21;
        public const uint TLProxyConfig76 = 0xffffff22;
        public const uint TLSocks5Proxy = 0xffffff23;
        public const uint TLMTProtoProxy = 0xffffff24;
        public const uint TLContactsSettings = 0xffffff25;
        public const uint TLSecureFileUploaded = 0xffffff26;
    }
}
