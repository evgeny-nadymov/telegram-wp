// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
#if WP8
using Windows.Storage;
#endif
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public interface IUploadVideoFileManager
    {
        void UploadFile(TLLong fileId, TLObject owner, string fileName);
        void UploadFile(TLLong fileId, TLObject owner, string fileName, IList<UploadablePart> parts);

#if WP8
        void UploadFile(TLLong fileId, bool isGif, TLObject owner, StorageFile file);
#endif

        void CancelUploadFile(TLLong fileId);
    }
}