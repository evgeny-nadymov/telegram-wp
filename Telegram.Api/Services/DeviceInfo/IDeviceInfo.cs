// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.Services.DeviceInfo
{
    public interface IDeviceInfoService
    {
        string Model { get; }
        string AppVersion { get; }
        string SystemVersion { get; }
        bool IsBackground { get; }
        string BackgroundTaskName { get; }
        int BackgroundTaskId { get; }
    }
}
