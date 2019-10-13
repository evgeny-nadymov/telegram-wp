// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public interface IAudioFileManager
    {
        void DownloadFile(TLInt dcId, TLInputFileLocationBase file, TLObject owner, TLInt fileSize, System.Action<DownloadableItem> callback = null);
        void CancelDownloadFile(TLObject owner);
    }
}