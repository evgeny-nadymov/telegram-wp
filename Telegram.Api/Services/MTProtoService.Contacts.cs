// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using Telegram.Api.TL.Functions.Contacts;

namespace Telegram.Api.Services
{
    public partial class MTProtoService
    {
        public void ResetSavedAsync(Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLResetSaved();

            SendInformativeMessage<TLBool>("contacts.resetSaved", obj, callback.SafeInvoke, faultCallback);
        }

        public void ResetTopPeerRatingAsync(TLTopPeerCategoryBase category, TLInputPeerBase peer, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLResetTopPeerRating { Category = category, Peer = peer };

            SendInformativeMessage<TLBool>("contacts.resetTopPeerRating", obj, callback.SafeInvoke, faultCallback);
        }

        public void GetTopPeersAsync(GetTopPeersFlags flags, TLInt offset, TLInt limit, TLInt hash, Action<TLTopPeersBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetTopPeers { Flags = new TLInt((int) flags), Offset  = offset, Limit = limit, Hash = hash };

            SendInformativeMessage<TLTopPeersBase>("contacts.getTopPeers", obj, result =>
            {
                var topPeers = result as TLTopPeers;
                if (topPeers != null)
                {
                    _cacheService.SyncUsersAndChats(topPeers.Users, topPeers.Chats,
                        tuple =>
                        {
                            topPeers.Users = tuple.Item1;
                            topPeers.Chats = tuple.Item2;
                            callback.SafeInvoke(result);
                        });
                }
                else
                {
                    callback.SafeInvoke(result);
                }
            }, faultCallback);
        }

        public void ResolveUsernameAsync(TLString username, Action<TLResolvedPeer> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLResolveUsername{ Username = username };

            SendInformativeMessage<TLResolvedPeer>("contacts.resolveUsername", obj,
                result =>
                {
                    _cacheService.SyncUsersAndChats(result.Users, result.Chats, 
                        tuple =>
                        {
                            result.Users = tuple.Item1;
                            result.Chats = tuple.Item2;
                            callback.SafeInvoke(result);
                        });
                }, 
                faultCallback);
        }

        public void GetStatusesAsync(Action<TLVector<TLContactStatusBase>> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetStatuses();

            SendInformativeMessage<TLVector<TLContactStatusBase>>("contacts.getStatuses", obj, 
                contacts =>
                {
                    _cacheService.SyncStatuses(contacts, callback);
                }, 
                faultCallback);
        }

        public void GetContactsAsync(TLInt hash, Action<TLContactsBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetContacts { Hash = hash };

            SendInformativeMessage<TLContactsBase>("contacts.getContacts", obj, result => _cacheService.SyncContacts(result, callback), faultCallback);
        }

        public void ImportContactsAsync(TLVector<TLInputContactBase> contacts, Action<TLImportedContacts> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLImportContacts { Contacts = contacts };

            SendInformativeMessage<TLImportedContacts>("contacts.importContacts", obj, result => _cacheService.SyncContacts(result, callback), faultCallback);
        }

        public void DeleteContactAsync(TLInputUserBase id, Action<TLLinkBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLDeleteContact { Id = id };

            SendInformativeMessage<TLLinkBase>("contacts.deleteContact", obj, result => _cacheService.SyncUserLink(result, callback), faultCallback);
        }

        public void DeleteContactsAsync(TLVector<TLInputUserBase> id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLDeleteContacts { Id = id };

            SendInformativeMessage<TLBool>("contacts.deleteContacts", obj,
                result =>
                {
                    foreach (var inputUser in id.OfType<IInputUserId>())
                    {
                        var user = _cacheService.GetUser(inputUser.UserId);
                        if (user != null && user.IsContact)
                        {
                            user.IsContact = false;
                            user.IsContactMutual = false;
                        }
                    }

                    _cacheService.Commit();

                    callback.SafeInvoke(result);
                },
                faultCallback);
        }

        public void BlockAsync(TLInputUserBase id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLBlock { Id = id };

            SendInformativeMessage("contacts.block", obj, callback, faultCallback);
        }

        public void UnblockAsync(TLInputUserBase id, Action<TLBool> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLUnblock { Id = id };

            SendInformativeMessage("contacts.unblock", obj, callback, faultCallback);
        }

        public void GetBlockedAsync(TLInt offset, TLInt limit, Action<TLContactsBlockedBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetBlocked { Offset = offset, Limit = limit };

            SendInformativeMessage("contacts.getBlocked", obj, callback, faultCallback);
        }

        public void SearchAsync(TLString q, TLInt limit, Action<TLContactsFoundBase> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLSearch { Q = q, Limit = limit };

            SendInformativeMessage("contacts.search", obj, callback, faultCallback);
        }

        public void GetSavedAsync(Action<TLVector<TLSavedPhoneContact>> callback, Action<TLRPCError> faultCallback = null)
        {
            var obj = new TLGetSaved();

            SendInformativeMessage("contacts.getSaved", obj, callback, faultCallback);
        }
    }
}
