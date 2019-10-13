// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Chats
{
    public class GroupsInCommonViewModel : ItemsViewModelBase<TLChatBase>, Telegram.Api.Aggregator.IHandle<DownloadableItem>
    {
        private readonly TLUserBase _currentUser;

        private readonly Dictionary<int, TLChatBase> _resultsDict = new Dictionary<int, TLChatBase>(); 

        public GroupsInCommonViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            _currentUser = StateService.CurrentContact;
            StateService.CurrentContact = null;

            Status = AppResources.Loading;

            BeginOnThreadPool(() =>
            {
                if (_currentUser == null) return;

                MTProtoService.GetCommonChatsAsync(_currentUser.ToInputUser(), new TLInt(0), new TLInt(100),
                    result => BeginOnUIThread(() =>
                    {
                        Status = result.Chats.Count > 0 ? string.Empty : AppResources.NoGroupsHere;

                        const int firstSliceCount = 11;
                        var firstSlice = new List<TLChatBase>();
                        var secondSlice = new List<TLChatBase>();

                        for (var i = 0; i < result.Chats.Count; i++)
                        {
                            _resultsDict[result.Chats[i].Index] = result.Chats[i];

                            if (i < firstSliceCount)
                            {
                                firstSlice.Add(result.Chats[i]);
                            }
                            else
                            {
                                secondSlice.Add(result.Chats[i]);
                            }
                        }

                        Items.Clear();
                        foreach (var chat in firstSlice)
                        {
                            Items.Add(chat);
                        }

                        if (secondSlice.Count > 0)
                        {
                            BeginOnUIThread(() =>
                            {
                                foreach (var chat in secondSlice)
                                {
                                    Items.Add(chat);
                                }
                            });
                        }
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        Status = AppResources.NoGroupsHere;
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.getCommonChats error=" + error);
                    }));
            });
        }

        public void ChatAction(TLChatBase chatBase)
        {
            if (chatBase == null) return;

            StateService.With = chatBase;
            StateService.RemoveBackEntries = true;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        public void Handle(DownloadableItem item)
        {
            
        }
    }
}
