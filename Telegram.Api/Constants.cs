// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
//#define TEST_SERVER

namespace Telegram.Api
{
    public static class Constants
    {
        public const int ApiId = https://core.telegram.org/api/obtaining_api_id
        public const string ApiHash = https://core.telegram.org/api/obtaining_api_id

#if TEST_SERVER
        public const int FirstServerDCId = 1;
        public const int FirstServerPort = 443;
        public const string FirstServerIpAddress =
            "149.154.175.40";       // dc1
            //"149.154.167.40";     // dc2
            //"149.154.175.117";    // dc3
        public const bool IsTestServer = true;
#else
        public const int FirstServerDCId = 2;   // [1, 2, 3, 4, 5]
        public const int FirstServerPort = 443;
        public const string FirstServerIpAddress =
            //"149.154.175.50";     // dc1
            "149.154.167.51";       // dc2
            //"174.140.142.6";      // dc3
            //"149.154.167.90";     // dc4
            //"149.154.171.5";      // dc5
        public const bool IsTestServer = false;
#endif

        public const int SupportedLayer = 85;
        public const int MinSecretSupportedLayer = 46;
        public const int SecretSupportedLayer = 73;

        public const int LongPollReattemptDelay = 5000;     //ms
        public const double MessageSendingInterval =
#if DEBUG
            300;   //seconds (5 minutes - 30 seconds(max delay: 25))
#else
            180;   //seconds (5 minutes - 30 seconds(max delay: 25))
#endif
        public const double ResendMessageInterval = 5.0;        //seconds
        public const int CommitDBInterval = 3;                  //seconds
        public const int GetConfigInterval = 60 * 60;           //seconds
        public const int TimeoutInterval = 25;                  //seconds 
        public const double DelayedTimeoutInterval = 45.0;      //seconds 
        public const double NonEncryptedTimeoutInterval = 15.0; //seconds   

        public const bool IsLongPollEnabled = false;
        public const int CachedDialogsCount = 20;
        public const int CachedMessagesCount = 25;

        public const int WorkersNumber = 4;
        public static int BigFileWorkersNumber = 4;

        public const string ConfigKey = "Config";
        public const string ConfigFileName = "config.xml";
        public static double CheckSendingMesagesInterval = 5.0; //seconds

        public static double CheckGetConfigInterval =
#if DEBUG
            10.0;
#else
            1 * 60.0;     //seconds (1 min)
#endif
        public static double CheckPingInterval = 20.0;          //seconds
        public static double UpdateStatusInterval = 2.0;
        public static int VideoUploadersCount = 3;
        public static int DocumentUploadersCount = 3;
        public static int AudioDownloadersCount = 3;
        public static int MaximumChunksCount = 3000;
        public static int DownloadedChunkSize = 32 * 1024;                  // 1MB % DownloadedChunkSize = 0 && DownloadedChunkSize % 1KB = 0
        public static int DownloadedBigChunkSize = 128 * 1024;              // 1MB % DownloadedChunkSize = 0 && DownloadedChunkSize % 1KB = 0
        public static ulong MaximumUploadedFileSize = 512 * 1024 * 3000;    // 1,5GB

        public static string StateFileName = "state.dat";
        public static string TempStateFileName = "temp_state.dat";
        public static string ActionQueueFileName = "action_queue.dat";
        public static string SentQueueIdFileName = "sent_queue_id.dat";

        public const string IsAuthorizedKey = "IsAuthorized";
        public const int StickerMaxSize = 256 * 1024;               // 256 KB
        public const int GifMaxSize = 10 * 1014 * 1024;             // 10 MB
        public const int AutoDownloadGifMaxSize = 2 * 1014 * 1024;  // 1 MB
        public const int SmallFileMaxSize = 32 * 1024;              //10 * 1024 * 1024;   // 10 MB

        public const string BackgroundTaskSettingsFileName = "background_task_settings.dat";
        public const string DifferenceFileName = "difference.dat";
        public const string TempDifferenceFileName = "temp_difference.dat";
        public const string DifferenceTimeFileName = "difference_time.dat";

        public const string TelegramMessengerMutexName = "TelegramMessenger";
        public const double DifferenceMinInterval = 10.0;       //seconds

        public const string InitConnectionFileName = "init_connection.dat";
        public const string DisableNotificationsFileName = "disable_notifications.dat";

        public const int MinRandomBytesLength = 15;

        public static int MinSecretChatWithExtendedKeyVisualizationLayer = 46;
        public static int MinSecretChatWithMTProto2Layer = 46;

        public const string ProxyConfigFileName = "proxy_config.dat";
        public const string CdnConfigFileName = "cdn_config.dat";

        public const string LiveLocationsFileName = "live_locations.dat";

        public const int CheckConfigTimeout =
#if DEBUG
            10;
#else
            7;
#endif
    }
}
