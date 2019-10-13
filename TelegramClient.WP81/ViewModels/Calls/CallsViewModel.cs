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
using Caliburn.Micro;
using Coding4Fun.Toolkit.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Media;
using TelegramClient.ViewModels.Search;
using Execute = Caliburn.Micro.Execute;

namespace TelegramClient.ViewModels.Calls
{
    public class CallsViewModel : ItemsViewModelBase<TLDialogBase>
    {
        public bool FirstRun { get; set; }

        public CallsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            DisplayName = LowercaseConverter.Convert(AppResources.Calls);
            Status = AppResources.Loading;
            FirstRun = true;
        }

        public bool OpenDialogDetails(TLDialogBase dialog)
        {
            ShellViewModel.StartNewTimer();
            //Execute.ShowDebugMessage("OpenDialogDetails");

            if (dialog == null)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("OpenDialogDetails dialog=null");
                return false;
            }
            if (dialog.With == null)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("OpenDialogDetails dialog.With=null");
                return false;
            }

            if (dialog.IsEncryptedChat)
            {
                var encryptedChat = CacheService.GetEncryptedChat(dialog.Peer.Id);

                var user = dialog.With as TLUserBase;
                if (user == null)
                {
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("OpenDialogDetails encrypted dialog.With=null");
                    return false;
                }

                var cachedUser = CacheService.GetUser(user.Id);
                StateService.Participant = cachedUser ?? user;
                StateService.With = encryptedChat;
                StateService.Dialog = dialog;
                StateService.AnimateTitle = true;
                NavigationService.UriFor<SecretDialogDetailsViewModel>().Navigate();
            }
            else
            {
                var settings = dialog.With as INotifySettings;
                if (settings != null)
                {
                    settings.NotifySettings = settings.NotifySettings ?? dialog.NotifySettings;
                }

                StateService.With = dialog.With;
                StateService.AnimateTitle = true;
                NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
            }

            return true;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            //if (FirstRun)
            {
                LoadCalls();
            }
        }

        private volatile bool _isUpdated;

        private int _offset = 0;

        private void LoadCalls()
        {
            var timespan = Items.Count > 0 ? 1.0 : 0.0;

            BeginOnThreadPool(TimeSpan.FromSeconds(timespan), () =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                //if (!FirstRun)
                //{
                //    return;
                //}
                if (!isAuthorized)
                {
                    return;
                }

                FirstRun = false;

                Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.Loading : string.Empty;
                var limit = 50;//Constants.DialogsSlice;
                MTProtoService.SearchAsync(
                    new TLInputPeerEmpty(), 
                    TLString.Empty, 
                    null, 
                    new TLInputMessagesFilterPhoneCalls{ Flags = new TLInt(0) },
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(limit),
                    new TLInt(0),
                    result => 
                    {
                        CacheService.AddChats(result.Chats, results => { });
                        CacheService.AddUsers(result.Users, results => { });

                        var items = new List<TLDialogBase>();
                        var newMessages = result as TLMessages;
                        if (newMessages != null)
                        {
                            var usersCache = new Dictionary<int, TLUserBase>();
                            foreach (var user in newMessages.Users)
                            {
                                usersCache[user.Index] = user;
                            }

                            var chatsCache = new Dictionary<int, TLChatBase>();
                            foreach (var chat in newMessages.Chats)
                            {
                                chatsCache[chat.Index] = chat;
                            }

                            foreach (var message in newMessages.Messages.OfType<TLMessageCommon>())
                            {
                                var dialog = new TLDialog { TopMessage = message };
                                var peer = TLUtils.GetPeerFromMessage(message);
                                if (peer is TLPeerUser)
                                {
                                    TLUserBase user;
                                    if (!usersCache.TryGetValue(peer.Id.Value, out user))
                                    {
                                        continue;
                                    }
                                    dialog.With = user;
                                    items.Add(dialog);
                                }
                            }
                        }

                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                        {
                            IsLastSliceLoaded = result.Messages.Count < limit;
                            _offset = Constants.DialogsSlice;

                            _isUpdated = true;

                            const int maxMessagesSlice = 8;
                            Items.Clear();
                            for (var i = 0; i < items.Count; i++)
                            {
                                if (i < maxMessagesSlice)
                                {
                                    Items.Add(items[i]);
                                }
                                else
                                {
                                    LazyItems.Add(items[i]);
                                }
                            }

                            Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.NoCallsHere : string.Empty;

                            if (LazyItems.Count > 0)
                            {
                                BeginOnUIThread(() =>
                                {
                                    for (var i = 0; i < LazyItems.Count; i++)
                                    {
                                        Items.Add(LazyItems[i]);
                                    }
                                    LazyItems.Clear();
                                });
                            }
                        });
                    },
                    error => Execute.BeginOnUIThread(() =>
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.getDialogs error " + error);
                        //_isUpdated = true;
                        Status = string.Empty;
                    }));
            });

            base.OnInitialize();
        }

        public void LoadNextSlice()
        {
            
        }

        public void DeleteDialog(TLDialogBase dialogBase)
        {
            var dialog = dialogBase as TLDialog;
            if (dialog == null) return;

            var message = dialog.TopMessage as TLMessageCommon;
            if (message == null) return;

            var messages = new List<TLMessageBase> { message };

            var owner = message.Out.Value ? CacheService.GetUser(message.ToId.Id) : message.From;

            if ((message.Id == null || message.Id.Value == 0) && message.RandomIndex != 0)
            {
                DeleteMessagesInternal(owner, null, messages);
                return;
            }

            DialogDetailsViewModel.DeleteMessages(MTProtoService, false, null, null, messages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
        }

        private void DeleteMessagesInternal(TLObject owner, TLMessageBase lastMessage, IList<TLMessageBase> messages)
        {
            var ids = new TLVector<TLInt>();
            for (int i = 0; i < messages.Count; i++)
            {
                ids.Add(messages[i].Id);
            }

            // duplicate: deleting performed through updates
            CacheService.DeleteMessages(ids);

            BeginOnUIThread(() =>
            {
                for (var i = 0; i < messages.Count; i++)
                {
                    for (var j = 0; j < Items.Count; j++)
                    {
                        var dialog = Items[j] as TLDialog;
                        if (dialog != null
                            && dialog.TopMessage != null
                            && dialog.TopMessage.Id.Value == messages[i].Index)
                        {
                            Items.RemoveAt(j);
                            break;
                        }
                    }
                }
            });

            EventAggregator.Publish(new DeleteMessagesEventArgs { Owner = owner, Messages = messages });
        }
    }
}
