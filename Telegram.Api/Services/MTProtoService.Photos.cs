// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Photos;

namespace Telegram.Api.Services
{
    public partial class MTProtoService
    {
        public void UploadProfilePhotoAsync(TLInputFile file, Action<TLPhotosPhoto> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLUploadProfilePhoto { File = file };

            SendInformativeMessage("photos.uploadProfilePhoto", obj, callback, faultCallback);
        }

        public void UpdateProfilePhotoAsync(TLInputPhotoBase id, Action<TLPhotoBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLUpdateProfilePhoto{ Id = id };

            SendInformativeMessage("photos.updateProfilePhoto", obj, callback, faultCallback);
        }

        public void GetUserPhotosAsync(TLInputUserBase userId, TLInt offset, TLLong maxId, TLInt limit, Action<TLPhotosBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetUserPhotos { UserId = userId, Offset = offset, MaxId = maxId, Limit = limit };

            SendInformativeMessage("photos.getUserPhotos", obj, callback, faultCallback);
        }
    }
}
