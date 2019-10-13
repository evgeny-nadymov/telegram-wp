// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace TelegramClient.Services
{
    public interface IPushService
    {
        string GetPushChannelUri();
        void RegisterDeviceAsync(System.Action callback);
        void UnregisterDeviceAsync(System.Action callback);
    }
}
