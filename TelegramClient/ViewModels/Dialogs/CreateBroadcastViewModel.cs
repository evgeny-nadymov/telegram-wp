// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Dialogs
{
    public class CreateBroadcastViewModel : CreateDialogViewModel
    {
        public CreateBroadcastViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {

        }

        public override void Create()
        {
            if (string.IsNullOrEmpty(Title))
            {
                MessageBox.Show(AppResources.PleaseEnterGroupSubject, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            var participants = new TLVector<TLInputUserBase>();
            foreach (var item in SelectedUsers)
            {
                participants.Add(item.ToInputUser());
            }

            if (participants.Count == 0)
            {
                MessageBox.Show(AppResources.PleaseChooseAtLeastOneParticipant, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            var broadcastChat = new TLBroadcastChat
            {
                Id = TLInt.Random(),
                Photo = new TLChatPhotoEmpty(),
                Title = new TLString(Title),
                ParticipantIds = new TLVector<TLInt> { Items = SelectedUsers.Select(x => x.Id).ToList() }
            };

            CacheService.SyncBroadcast(broadcastChat, result =>
            {
                var broadcastPeer = new TLPeerBroadcast {Id = broadcastChat.Id};
                var serviceMessage = new TLMessageService17
                {
                    FromId = new TLInt(StateService.CurrentUserId),
                    ToId = broadcastPeer,
                    Status = MessageStatus.Confirmed,
                    Out = new TLBool { Value = true },
                    Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                    //IsAnimated = true,
                    RandomId = TLLong.Random(),
                    Action = new TLMessageActionChatCreate
                    {
                        Title = broadcastChat.Title,
                        Users = broadcastChat.ParticipantIds
                    }
                };
                serviceMessage.SetUnread(new TLBool(false));

                CacheService.SyncMessage(serviceMessage, 
                    message =>
                    {
                        StateService.With = broadcastChat;
                        StateService.RemoveBackEntry = true;
                        NavigationService.UriFor<DialogDetailsViewModel>().Navigate(); 
                    });
  
            });
        }
    }
}
