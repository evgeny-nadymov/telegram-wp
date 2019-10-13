// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public interface IUploadAudioFileManager
    {
        void UploadFile(TLLong fileId, TLObject owner, string fileName);
        void UploadFile(TLLong fileId, TLObject owner, string fileName, IList<UploadablePart> parts);
        void CancelUploadFile(TLLong fileId);
    }
}