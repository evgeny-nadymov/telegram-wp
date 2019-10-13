// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Users;

namespace Telegram.Api.Services
{
	public partial class MTProtoService
	{
        public void GetUsersAsync(TLVector<TLInputUserBase> id, Action<TLVector<TLUserBase>> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetUsers { Id = id };

            SendInformativeMessage<TLVector<TLUserBase>>("users.getUsers", obj, result =>
            {
                _cacheService.SyncUsers(result, callback);
            }, 
            faultCallback);
        }

        public void GetFullUserAsync(TLInputUserBase id, Action<TLUserFull> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetFullUser { Id = id };

            SendInformativeMessage<TLUserFull>("users.getFullUser", obj, userFull => _cacheService.SyncUser(userFull, callback.SafeInvoke), faultCallback);
        }

        public void SetSecureValueErrorsAsync(TLInputUserBase id, TLVector<TLSecureValueErrorBase> errors, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSetSecureValueErrors { Id = id, Errors = errors };

            SendInformativeMessage("users.setSecureValueErrors", obj, callback, faultCallback);
        }
	}
}
