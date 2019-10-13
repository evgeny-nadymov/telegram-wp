// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public ShareContactViewModel ContactPicker { get; set; }

        private void OpenContact()
        {
            //if (ContactPicker == null)
            {
                ContactPicker = IoC.Get<ShareContactViewModel>();
                ContactPicker.ContinueAction = ContinueSendContact;
                NotifyOfPropertyChange(() => ContactPicker);
            }
            //else
            //{
            //    BeginOnUIThread(() => ContactPicker.OpenEditor());
            //}
        }

        public void OpenPhoneContact(TLMessageMediaContact mediaContact)
        {
            StateService.PhoneContact = mediaContact;
            ContactPicker = IoC.Get<ShareContactViewModel>();
            ContactPicker.ContinueAction = ContinueSendContact;
            NotifyOfPropertyChange(() => ContactPicker);
        }

        private void SendContact(TLUserBase contact)
        {
            if (TLString.IsNullOrEmpty(contact.Phone))
            {
                var username = contact as IUserName;
                if (username != null && !TLString.IsNullOrEmpty(username.UserName))
                {
                    string accessToken = null;
                    var bot = contact as TLUser;
                    if (bot != null && bot.IsBot && !string.IsNullOrEmpty(bot.AccessToken))
                    {
                        accessToken = bot.AccessToken;
                        bot.AccessToken = null;
                    }

                    _text = string.Format(Constants.UsernameLinkPlaceholder, username.UserName);
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        _text += "?start=" + accessToken;
                    }
                    Send();

                    return;
                }

                return;
            }

            var media = new TLMessageMediaContact82
            {
                UserId = contact.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                PhoneNumber = contact.Phone,
                VCard = TLString.Empty
            };

            ContinueSendContact(media);
        }

        private void ContinueSendContact(TLMessageMediaContact media)
        {
            var message = GetMessage(TLString.Empty, media);

            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                BeginOnThreadPool(() =>
                CacheService.SyncSendingMessage(
                    message, previousMessage,
                    SendContactInternal));
            });
        }

        private void SendContactInternal(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage34;
            if (message == null) return;

            var mediaContact = message.Media as TLMessageMediaContact;
            if (mediaContact == null) return;

            var inputMediaContact = new TLInputMediaContact82
            {
                FirstName = mediaContact.FirstName,
                LastName = mediaContact.LastName,
                PhoneNumber = mediaContact.PhoneNumber,
                VCard = TLString.Empty
            };

            message.InputMedia = inputMediaContact;

            UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
        }
    }
}
