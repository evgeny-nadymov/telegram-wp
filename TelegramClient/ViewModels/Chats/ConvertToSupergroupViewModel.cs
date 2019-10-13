// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Chats
{
    public class ConvertToSupergroupViewModel : ViewModelBase
    {
        private TLChatBase CurrentItem { get; set; }

        public ConvertToSupergroupViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            CurrentItem = StateService.CurrentChat;
            StateService.CurrentChat = null;
        }

        public void ConvertToSupergroup()
        {
            if (CurrentItem == null) return;
            if (IsWorking) return;

            var confirmation = MessageBox.Show(AppResources.ConvertToSupergroupConfirmation, AppResources.Warning, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            IsWorking = true;
            MTProtoService.MigrateChatAsync(
                CurrentItem.Id,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var updates = result as TLUpdates;
                    if (updates != null)
                    {
                        var channel = updates.Chats.FirstOrDefault(x => x is TLChannel) as TLChannel;
                        if (channel != null)
                        {
                            var migratedFromMaxId = new TLInt(0);
                            var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                            if (updateNewMessage != null)
                            {
                                migratedFromMaxId = updateNewMessage.Message.Id;
                            }
                            channel.MigratedFromChatId = CurrentItem.Id;
                            channel.MigratedFromMaxId = migratedFromMaxId;

                            var addedChannel = CacheService.GetDialog(new TLPeerChannel { Id = channel.Id });
                            if (addedChannel != null)
                            {
                                EventAggregator.Publish(new DialogAddedEventArgs(addedChannel));
                            }
                            var removedChat = CacheService.GetDialog(new TLPeerChat { Id = CurrentItem.Id });
                            if (removedChat != null)
                            {
                                EventAggregator.Publish(new DialogRemovedEventArgs(removedChat));
                            }

                            StateService.With = channel;
                            StateService.RemoveBackEntries = true;
                            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
                        }
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.migrateChat error " + error);
                }));
        }
    }
}