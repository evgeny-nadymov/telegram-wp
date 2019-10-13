// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.TL;

namespace Telegram.Api.Services.DeviceInfo
{
    public class DeviceInfoService : IDeviceInfoService
    {
        public string Model { get; protected set; }
        public string AppVersion { get; protected set; }
        public string SystemVersion { get; protected set; }
        public bool IsBackground { get; protected set; }
        public string BackgroundTaskName { get; protected set; }
        public int BackgroundTaskId { get; protected set; }

        public DeviceInfoService(string model, string appVersion, string systemVersion, bool isBackground, string backgroundTaskName, int backgroundTaskId)
        {
            Model = model;
            AppVersion = appVersion;
            SystemVersion = systemVersion;
            IsBackground = isBackground;
            BackgroundTaskName = backgroundTaskName;
            BackgroundTaskId = backgroundTaskId;
        }

        public DeviceInfoService(TLInitConnection initConnection, bool isBackground, string backgroundTaskName, int backgroundTaskId)
        {
            Model = initConnection.DeviceModel.ToString();
            AppVersion = initConnection.AppVersion.ToString();
            SystemVersion = initConnection.SystemVersion.ToString();
            IsBackground = isBackground;
            BackgroundTaskName = backgroundTaskName;
            BackgroundTaskId = backgroundTaskId;
        }
    }
}
