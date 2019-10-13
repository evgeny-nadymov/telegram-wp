// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace TelegramClient
{
    static class Constants
    {
#if PRIVATE_BETA || DEBUG
        public const string LogEmail = "sms@telegram.org";
#else
        public const string LogEmail = "sms@telegram.org";
#endif
        public const int TelegramNotificationsId = 777000;

        public const string DefaultMeUrlPrefix = "https://t.me/";

        public const string TelegramFaq = "https://telegram.org/faq";
        public const string TelegramPrivacyPolicy = "https://telegram.org/privacy";
        public const string TelegramShare = "https://telegram.org/dl";
        public const string TelegramTroubleshooting = "https://telegram.org/faq#troubleshooting";

        public const int VideoPreviewMaxSize = 90;//320;
        public const int VideoPreviewQuality = 87;

        public const int DocumentPreviewMaxSize = 90;
        public const int DocumentPreviewQuality = 87;

        public const int PhotoPreviewMaxSize = 90;
        public const int PhotoPreviewQuality = 87;

        public const string EmptyBackgroundString = "Empty";
        public const string LibraryBackgroundString = "Library";
        public const string AnimatedBackground1String = "AnimatedBackground1";

        public const int MaximumMessageLength = 4096;

        public const double GlueGeoPointsDistance = 50.0;

        public const double DefaultMessageContentWidth = 323.0;
        public const double DefaultMessageContentHeight = 150.0;
        public const double MaxStickerDimension = 196.0;
        public const double MaxGifDimension = 323.0;
        public const double DeafultInlineBotResultHeight = 118.0;

        public const int ShowHelpTimeInSeconds = 120;
        public const int DialogsSlice = 20;
        public const int MessagesSlice = 25;
        public const string IsAuthorizedKey = "IsAuthorized";
        public const string ConfigKey = "Config";
        public const string CurrentUserKey = "CurrentUser";
        public const string CurrentBackgroundKey = "CurrentBackground";
        public const string SendByEnterKey = "SendByEnter";


        public const string UnreadCountKey = "UnreadCountKey";
        public const string SettingsKey = "SettingsKey";
        public const string CommonNotifySettingsFileName = "CommonNotifySettings.xml";
        public const string DelayedContactsFileName = "PeopleHub.dat";

        public const string ScheduledAgentTaskName = "TelegramScheduledAgent";
        public const string ToastNotificationChannelName = "TelegramNotificationChannel";

        public const int MaxImportingContactsCount = 1300;
        public static string StaticGoogleMap = "https://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=16&size={2}x{3}&sensor=false&format=jpg&scale=2&language=en";

        public const double SetTypingIntervalInSeconds = 5.0;
        public const int SendCallDefaultTimeout = 90;
        public const int DefaultCodeLength = 5;
        public const int DefaultForwardingMessagesCount = 50;
        public const string ImportedPhonesFileName = "importedPhones.dat";
        public const string CachedServerFilesFileName = "cachedServerFiles.dat";
        public const string AllStickersFileName = "allStickers.dat";
        public const string MasksFileName = "masks.dat";
        public const string FeaturedStickersFileName = "featuredStickers.dat";
        public const string RecentStickersFileName = "recentStickers.dat";
        public const string ArchivedStickersFileName = "archivedStickers.dat";
        public const string WallpapersFileName = "wallpapers.dat";

        public const string TelegramFolderName = "Telegram";
        public const int UsernameMinLength = 5;
        public const int UsernameMaxLength = 32;
        public const double GetAllStickersInterval = 60.0 * 60.0;     // 60 min

        public const int FileSliceLength = 50;
        public const int PhotoVideoSliceLength = 48;    // 4 preview * 12 rows

        // FullHD
        public const double FullHDAppBarHeight = 60.0;
        public const double FullHDAppBarDifference = -12.0;

        //qHD
        public const double QHDAppBarHeight = 67.0;
        public const double QHDAppBarDifference = -5.0;

        public const double NotificationTimerInterval = 10.0;   // seconds
        public const string SnapshotsDirectoryName = "Snapshots";

        //passcode
        public const string AppCloseTimeKey = "AppCloseTime";
        public const string PasscodeKey = "PasscodeEnabled";
        public const string IsSimplePasscodeKey = "IsSimplePasscode";
        public const string IsPasscodeEnabledKey = "IsPasscodeEnabled";
        public const string PasscodeAutolockKey = "PasscodeAutolock";
        public const string PasscodeParamsFileName = "passcode_params.dat";
        public const int PasscodeHashIterations = 1000;

        //password
        public const int PasswordRecoveryCodeLength = 6;

        //passport
        public const uint DefaultPassportImageSize = 2048;
        public const uint MaxPassportFilesCount = 20;

        //notifications
        public const int WNSTokenType = 8;
        public const int MPNSTokenType = 3;
        public const int VoIPMPNSTokenType = 11;
        public const int NotificationInterval = 15; // Interval to count notifications (seconds)
        public const int UnmutedCount = 1; // Cont of notifications to show within interval 

        //unread messages
        public const int MinUnreadCountToShowSeparator = 2;

        //venues
        public const string FoursquireCategoryIconUrl = @"https://foursquare.com/img/categories_v2/{0}_{1}.png";

        //stickers
        public const string AddStickersLinkPlaceholder = @"https://t.me/addstickers/{0}";

        //usernames
        public const string UsernameLinkPlaceholder = @"https://t.me/{0}";

        //background tasks
        public const string PushNotificationsBackgroundTaskName = "PushNotificationsTask";
        public const string MessageSchedulerBackgroundTaskName = "SchedulerTask";
        public const string TimerMessageSchedulerBackgroundTaskName = "TimerSchedulerTask";
        public const string BackgroundDifferenceLoaderTaskName = "BackgroundDifferenceLoader";
        public const string InteractiveNotificationsBackgroundTaskName = "InteractiveNotificationsBackgroundTask";

        //message search
        public const int SearchMessagesSliceLimit = 5;

        //search
        public const string RecentSearchResultsFileName = "search_chats_recent.dat";
        public const string TopPeersFileName = "top_peers.dat";

        //channels
        public const string ChannelIntroFileName = "channel_intro.dat";

        //photo
        public const uint DefaultImageSize = 1280;

        //chat settings
        public const string ChatSettingsFileName = "chat_settings.dat";
        public const string SuppressInlineBotsHintFileName = "suppress_inline_bots_hint.dat";

        //contacts settings 
        public const string ContactsSettingsFileName = "contacts_settings.dat";

        // services
        public static int HttpDownloadersCount = 5;

        public static int MinSecretChatWithStickersLayer = 23;
        public static int MinSecretChatWithRepliesLayer = 45;
        public static int MinSecretChatWithCaptionsLayer = 45;
        public static int MinSecretChatWithVenuesLayer = 45;
        public static int MinSecretChatWithInlineBotsLayer = 45;
        public static int MinSecretChatWithExtendedKeyVisualizationLayer = 46;
        public static int MinSecretChatWithAudioAsDocumentsLayer = 46;
        public static int MinSecretChatWithRoundVideoLayer = 66;
        public static int MinSecretChatWithGroupedMediaLayer = 73;

        public const string InlineBotsNotificationFileName = "inlinebots_notificaiton.dat";
        public const string WebPagePreviewsFileName = "webpagepreviews_notificaiton.dat";
        public const int RecheckBotInlineGeoAccessInterval =
#if DEBUG
            10; // seconds
#else
            600; // seconds
#endif

        public const string SpambotUsername = "spambot";
        public const string InlineBotsFileName = "inline_bots.dat";

        // camera
        public const string CameraSettingsFileName = "camera_settings.dat";

        // photo picker
        public const string PhotoPickerSettingsFileName = "photo_picker_settings.dat";

        public const int DefaultTmpPasswordLifetime =
#if DEBUG
            60 * 5;
#else
            60 * 30;
#endif
        public const string TmpPasswordFileName = "tmp_password.dat";

        public const string CallsSecurityFileName = "calls_security.dat";

        public const string SavedCountFileName = "saved_count.dat";

        public const int MinSetsToAddFavedSticker = 5;

        public const double MinDistanceToUpdateLiveLocation = 20.0;

        public const int MaxCacheCapacity = 50;
        public const int MaxGroupedMediaCount = 10;

        // update
        public const string PreviewUpdateUri = "ms-windows-store://pdp/?productid=9P0F9KC5TSTT";
        public const string UpdateUri = "ms-windows-store://pdp/?productid=9WZDNCRDZHS0";

        public const string PassportConfigFileName = "passport_config.dat";
    }
}
