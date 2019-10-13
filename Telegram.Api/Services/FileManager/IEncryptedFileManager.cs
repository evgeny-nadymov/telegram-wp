// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public interface IEncryptedFileManager
    {
        void DownloadFile(TLEncryptedFile file, TLObject owner, Action<DownloadableItem> callback = null);
        void CancelDownloadFile(TLObject owner);
    }
}
