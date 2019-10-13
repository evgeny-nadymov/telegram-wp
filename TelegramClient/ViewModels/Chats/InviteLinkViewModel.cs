// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Chats
{
    public class InviteLinkViewModel : ViewModelBase
    {
        private string _link;

        public string Link
        {
            get { return _link; }
            set { SetField(ref _link, value, () => Link); }
        }

        private readonly TLChatBase _chat;

        public InviteLinkViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            if (StateService.CurrentChat != null)
            {
                _chat = StateService.CurrentChat;
                StateService.CurrentChat = null;
            }

            if (_chat == null) return;

            var chatInviteExported = _chat.ExportedInvite as TLChatInviteExported;
            if (chatInviteExported != null)
            {
                _link = chatInviteExported.Link.ToString();
            }
            else
            {
                ExportChatInviteAsync();
            }
        }

        public void CopyLink()
        {
            if (string.IsNullOrEmpty(Link)) return;

            Clipboard.SetText(Link);
            MessageBox.Show(AppResources.CopyLinkHint);
        }

        public void RevokeLink()
        {
            if (_chat == null) return;

            var result = MessageBox.Show(AppResources.RevokeLinkConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                ExportChatInviteAsync();
            }
        }

        private void ExportChatInviteAsync()
        {
            var channel = _chat as TLChannel;
            if (channel != null)
            {

                IsWorking = true;
                MTProtoService.ExportInviteAsync(channel.ToInputChannel(),
                    chatInviteBase => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        var chatInviteExported = chatInviteBase as TLChatInviteExported;
                        if (chatInviteExported != null)
                        {
                            Link = chatInviteExported.Link.ToString();
                        }
                        else
                        {
                            Link = string.Empty;
                        }

                        _chat.ExportedInvite = chatInviteExported;
                        CacheService.Commit();
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                    }));

                return;
            }

            IsWorking = true;
            MTProtoService.ExportChatInviteAsync(_chat.Id,
                chatInviteBase => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    var chatInviteExported = chatInviteBase as TLChatInviteExported;
                    if (chatInviteExported != null)
                    {
                        Link = chatInviteExported.Link.ToString();
                    }
                    else
                    {
                        Link = string.Empty;
                    }

                    _chat.ExportedInvite = chatInviteExported;
                    CacheService.Commit();
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }));
        }

        public void ShareLink()
        {
            if (string.IsNullOrEmpty(Link)) return;

            StateService.ShareLink = Link;
            StateService.ShareMessage = Link;
            StateService.ShareCaption = AppResources.InviteFriends;
            NavigationService.UriFor<ShareViewModel>().Navigate();
        }
    }
}
