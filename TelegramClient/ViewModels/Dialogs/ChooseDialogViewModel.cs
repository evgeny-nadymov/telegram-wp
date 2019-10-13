// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Windows.Storage;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public class ChooseDialogViewModel : ItemsViewModelBase<TLDialogBase>,
        Telegram.Api.Aggregator.IHandle<TopMessageUpdatedEventArgs>,
        Telegram.Api.Aggregator.IHandle<DialogAddedEventArgs>,
        Telegram.Api.Aggregator.IHandle<DialogRemovedEventArgs>
    {

        public TLUserBase SharedContact { get; set; }

        public List<TLMessageBase> ForwardedMessages { get; set; }

        private string LogFileName { get; set; }

        private const int FirstSliceLength = 10;

        private readonly string _gameString;

        private readonly string _accessToken;

        private readonly TLUserBase _bot;

        private readonly Uri _webLink;

        private readonly IReadOnlyList<IStorageItem> _storageItems;

        private readonly string _url;

        private readonly string _text;

        private readonly TLKeyboardButtonSwitchInline _switchInlineButton;

        public ICollectionView FilteredItems { get; set; }

        public TLDialogBase CurrentUser { get; protected set; }

        public ChooseDialogViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            FilteredItems = new CollectionViewSource { Source = Items }.View;
            FilteredItems.Filter += item =>
            {
                var dialog = item as TLDialog;
                if (dialog != null)
                {
                    var chat = dialog.With as TLChat41;
                    if (chat != null)
                    {
                        return !chat.IsMigrated;
                    }

                    var user = dialog.With as TLUser;
                    if (user != null)
                    {
                        return !user.IsSelf;
                    }
                }

                var dialog71 = dialog as TLDialog71;
                if (dialog71 != null)
                {
                    return !dialog71.IsPromo;
                }

                return true;
            };

            EventAggregator.Subscribe(this);

            LogFileName = StateService.LogFileName;
            StateService.LogFileName = null;

            ForwardedMessages = StateService.ForwardMessages;
            StateService.ForwardMessages = null;

            SharedContact = StateService.SharedContact;
            StateService.SharedContact = null;

            _gameString = StateService.GameString;
            StateService.GameString = null;

            _accessToken = StateService.AccessToken;
            StateService.AccessToken = null;

            _bot = StateService.Bot;
            StateService.Bot = null;

            _webLink = StateService.WebLink;
            StateService.WebLink = null;

            _storageItems = StateService.StorageItems;
            StateService.StorageItems = null;

            _url = StateService.Url;
            StateService.Url = null;

            _text = StateService.UrlText;
            StateService.UrlText = null;

            _switchInlineButton = StateService.SwitchInlineButton;
            StateService.SwitchInlineButton = null;

            Status = AppResources.Loading;

            BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (isAuthorized)
                {
                    var dialogs = CacheService.GetDialogs();

                    var dialogsCache = new Dictionary<int, TLDialogBase>();
                    var clearedDialogs = new List<TLDialogBase>();
                    foreach (var dialog in dialogs)
                    {
                        if (!dialogsCache.ContainsKey(dialog.Index))
                        {
                            var user = dialog.With as TLUser;
                            if (user != null && user.IsSelf)
                            {
                                CurrentUser = dialog;
                            }

                            if (dialog is TLDialog || dialog is TLBroadcastDialog)
                            {
                                if (!SkipDialogForBot(_bot, dialog))
                                {
                                    clearedDialogs.Add(dialog);
                                }
                                dialogsCache[dialog.Index] = dialog;
                            }
                        }
                        else
                        {
                            var cachedDialog = dialogsCache[dialog.Index];
                            if (cachedDialog.Peer is TLPeerUser && dialog.Peer is TLPeerUser)
                            {
                                CacheService.DeleteDialog(dialog);
                                continue;
                            }
                            if (cachedDialog.Peer is TLPeerChat && dialog.Peer is TLPeerChat)
                            {
                                CacheService.DeleteDialog(dialog);
                                continue;
                            }
                        }
                    }

                    if (CurrentUser == null)
                    {
                        var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
                        if (currentUser != null)
                        {
                            var dialog = new TLDialog71
                            {
                                With = currentUser,
                                Flags = new TLInt(0),
                                Peer = new TLPeerUser { Id = currentUser.Id },
                                Messages = new ObservableCollection<TLMessageBase>(),
                                TopMessageId = new TLInt(0),
                                ReadInboxMaxId = new TLInt(0),
                                ReadOutboxMaxId = new TLInt(0),
                                UnreadCount = new TLInt(0),
                                UnreadMentionsCount = new TLInt(0),
                                NotifySettings = new TLPeerNotifySettings78 { Flags = new TLInt(0), MuteUntil = new TLInt(0), Sound = new TLString("Default") }
                            };
                            CurrentUser = dialog;
                        }
                    }

                    BeginOnUIThread(() =>
                    {
                        NotifyOfPropertyChange(() => CurrentUser);

                        foreach (var clearedDialog in clearedDialogs)
                        {
                            LazyItems.Add(clearedDialog);
                        }

                        var lastDialog = clearedDialogs.LastOrDefault(x => x.TopMessageId != null);
                        _maxId = lastDialog != null ? lastDialog.TopMessageId.Value : 0;

                        Status = LazyItems.Count == 0 ? AppResources.Loading : string.Empty;
                        var importantCount = 0;
                        var count = 0;
                        for (var i = 0; i < LazyItems.Count && importantCount < FirstSliceLength; i++, count++)
                        {
                            Items.Add(LazyItems[i]);
                            var chat41 = LazyItems[i].With as TLChat41;
                            if (chat41 == null || chat41.MigratedTo == null)
                            {
                                importantCount++;
                            }
                        }

                        BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
                        {
                            for (var i = count; i < LazyItems.Count; i++)
                            {
                                Items.Add(LazyItems[i]);
                            }
                            LazyItems.Clear();

                            LoadNextSlice();
                        });
                    });
                }

            });
        }

        public static bool SkipDialogForBot(TLUserBase bot, TLDialogBase dialog)
        {
            var chat = dialog.With as TLChat41;
            var channel = dialog.With as TLChannel;

            if (bot != null)
            {
                if (dialog is TLDialog)
                {
                    if (chat != null && !chat.IsMigrated)
                    {
                        return false;
                    }

                    if (channel != null && channel.IsMegaGroup && (channel.Creator || channel.IsEditor))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public TLMessage34 GetMessage(TLObject With, TLInputPeerBase Peer, TLString text, TLMessageMediaBase media)
        {
            var broadcast = With as TLBroadcastChat;
            var channel = With as TLChannel;
            var toId = channel != null
                ? new TLPeerChannel { Id = channel.Id }
                : broadcast != null
                ? new TLPeerBroadcast { Id = broadcast.Id }
                : TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId);

            var date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);

            var message = TLUtils.GetMessage(
                new TLInt(StateService.CurrentUserId),
                toId,
                broadcast != null && channel == null ? MessageStatus.Broadcast : MessageStatus.Sending,
                TLBool.True,
                TLBool.True,
                date,
                text,
                media,
                TLLong.Random(),
                new TLInt(0)
            );

            return message;
        }

        public bool ChooseDialog(TLObject with, bool animateTitle)
        {
            if (with == null) return false;

            if (ForwardedMessages != null)
            {
                var channel = with as TLChannel;
                if (channel != null && channel.IsBroadcast && !channel.Creator && !channel.IsEditor)
                {
                    MessageBox.Show(AppResources.PostToChannelError, AppResources.Error, MessageBoxButton.OK);

                    return false;
                }
            }

            var result = MessageBoxResult.OK;

            if (LogFileName != null)
            {
                result = MessageBox.Show(AppResources.ForwardMessagesToThisChat, AppResources.Confirm, MessageBoxButton.OKCancel);
            }

            if (_webLink != null || _storageItems != null || _gameString != null)
            {
                var fullName = string.Empty;
                var chat = with as TLChatBase;
                var user = with as TLUserBase;
                if (chat != null) fullName = chat.FullName2;
                if (user != null) fullName = user.FullName2;

                if (_gameString != null)
                {
                    result = MessageBox.Show(string.Format(AppResources.ShareGameWith, fullName), AppResources.Confirm, MessageBoxButton.OKCancel);
                }
                else
                {
                    result = MessageBox.Show(string.Format(AppResources.ShareWith, fullName), AppResources.Confirm, MessageBoxButton.OKCancel);
                }
            }

            if (_bot != null)
            {
                var chat = with as TLChat;
                var channel = with as TLChannel;
                if (chat == null && (channel == null || !channel.IsMegaGroup))
                {
                    return false;
                }

                var chatName = chat != null ? chat.FullName : channel.FullName;
                result = MessageBox.Show(string.Format(AppResources.AddBotToTheGroup, chatName), AppResources.Confirm, MessageBoxButton.OKCancel);
            }

            if (result != MessageBoxResult.OK)
            {
                return false;
            }

            if (_gameString != null)
            {
                var inputPeer = with as IInputPeer;
                if (inputPeer == null) return false;

                var mediaGame = new TLMessageMediaGame
                {
                    Game = new TLGame
                    {
                        Flags = new TLInt(0),
                        Id = new TLLong(0),
                        AccessHash = new TLLong(0),
                        ShortName = new TLString(_gameString),
                        Title = new TLString(_gameString),
                        Description = TLString.Empty,
                        Photo = new TLPhotoEmpty { Id = new TLLong(0) }
                    }
                };
                var message = GetMessage(with, inputPeer.ToInputPeer(), TLString.Empty, mediaGame);
                mediaGame.SourceMessage = message;
                IoC.Get<ICacheService>().SyncSendingMessage(message, null, m =>
                {
                    //IsWorking = true;
                    IoC.Get<IMTProtoService>().SendMediaAsync(inputPeer.ToInputPeer(), new TLInputMediaGame { Id = new TLInputGameShortName { ShortName = new TLString(_gameString), BotId = SharedContact.ToInputUser() } }, message,
                        updatesBase => Execute.BeginOnUIThread(() =>
                        {
                            var updates = updatesBase as TLUpdates;
                            if (updates != null)
                            {
                                var newChannelMessageUpdate = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                                if (newChannelMessageUpdate != null)
                                {
                                    var messageCommon = newChannelMessageUpdate.Message as TLMessageCommon;
                                    if (messageCommon != null)
                                    {
                                        var dialog = IoC.Get<ICacheService>().GetDialog(messageCommon);
                                        if (dialog != null)
                                        {
                                            IoC.Get<ITelegramEventAggregator>().Publish(new TopMessageUpdatedEventArgs(dialog, messageCommon));
                                        }
                                    }
                                }

                                var newMessageUpdate = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                                if (newMessageUpdate != null)
                                {
                                    var messageCommon = newMessageUpdate.Message as TLMessageCommon;
                                    if (messageCommon != null)
                                    {
                                        var dialog = IoC.Get<ICacheService>().GetDialog(messageCommon);
                                        if (dialog != null)
                                        {
                                            IoC.Get<ITelegramEventAggregator>().Publish(new TopMessageUpdatedEventArgs(dialog, messageCommon));
                                        }
                                    }
                                }
                            }

                            //IsWorking = false;
                            IoC.Get<INavigationService>().RemoveBackEntry();
                            IoC.Get<INavigationService>().GoBack();
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            //IsWorking = false;
                            Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.sendMedia error=" + error);
                        }));
                });

                return false;
            }

            StateService.With = with;
            StateService.ForwardMessages = ForwardedMessages;
            StateService.RemoveBackEntries = true;
            StateService.LogFileName = LogFileName;
            StateService.SharedContact = SharedContact;
            StateService.AccessToken = _accessToken;
            StateService.Bot = _bot;
            StateService.WebLink = _webLink;
            StateService.StorageItems = _storageItems;
            StateService.Url = _url;
            StateService.UrlText = _text;
            StateService.SwitchInlineButton = _switchInlineButton;
            StateService.AnimateTitle = animateTitle;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();

            return true;
        }

        public bool ChooseDialog(TLDialogBase dialog)
        {
            if (dialog == null) return false;
            if (dialog.With == null) return false;

            return ChooseDialog(dialog.With, true);
        }

        private int _maxId;

        public void LoadNextSlice()
        {
            if (LazyItems.Count > 0 || IsLastSliceLoaded || IsWorking)
            {
                return;
            }

            var offsetDate = 0;
            var offsetId = 0;
            TLInputPeerBase offsetPeer = new TLInputPeerEmpty();
            var lastDialog = Items.OfType<TLDialog>().Last(x => x.TopMessage != null && x.TopMessage.Index > 0);
            if (lastDialog != null)
            {
                var lastMessage = lastDialog.TopMessage as TLMessageCommon;
                if (lastMessage != null)
                {
                    offsetDate = lastMessage.DateIndex;
                    offsetId = lastMessage.Index;
                    if (lastMessage.ToId is TLPeerUser)
                    {
                        offsetPeer = !lastMessage.Out.Value
                            ? DialogDetailsViewModel.PeerToInputPeer(new TLPeerUser { Id = lastMessage.FromId })
                            : DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                    }
                    else
                    {
                        offsetPeer = DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                    }
                }
            }

            IsWorking = true;
            var offset = Items.Count;
            var limit = 30;
            MTProtoService.GetDialogsAsync(Stopwatch.StartNew(),
                new TLInt(offsetDate), 
                new TLInt(offsetId), 
                offsetPeer, 
                new TLInt(limit),
                new TLInt(0),
                result => Execute.BeginOnUIThread(() =>
                {
                    lastDialog = result.Dialogs.LastOrDefault(x => x.TopMessageId != null) as TLDialog;
                    if (lastDialog != null)
                    {
                        _maxId = lastDialog.TopMessageId.Value;
                    }

                    var itemsAdded = 0;
                    foreach (var dialog in result.Dialogs)
                    {
                        if (!SkipDialogForBot(_bot, dialog))
                        {
                            Items.Add(dialog);
                            itemsAdded++;
                        }
                    }

                    IsWorking = false;
                    IsLastSliceLoaded = result.Dialogs.Count < limit;
                    Status = LazyItems.Count > 0 || Items.Count > 0 ? string.Empty : Status;

                    if (itemsAdded < (Constants.DialogsSlice / 2))
                    {
                        LoadNextSlice();
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Status = string.Empty;
                }));
        }

        public void ForwardInAnimationComplete()
        {

        }

        public void Search()
        {
            StateService.SharedContact = SharedContact;
            StateService.LogFileName = LogFileName;
            StateService.ForwardMessages = ForwardedMessages;
            StateService.GameString = _gameString;
            StateService.AccessToken = _accessToken;
            StateService.Bot = _bot;
            StateService.WebLink = _webLink;
            StateService.StorageItems = _storageItems;
            StateService.Url = _url;
            StateService.UrlText = _text;
            StateService.SwitchInlineButton = _switchInlineButton;
            NavigationService.UriFor<SearchShellViewModel>().Navigate();
        }

        public void Handle(TopMessageUpdatedEventArgs eventArgs)
        {
            eventArgs.Dialog.NotifyOfPropertyChange(() => eventArgs.Dialog.With);
            OnTopMessageUpdated(this, eventArgs);
        }

        private void OnTopMessageUpdated(object sender, TopMessageUpdatedEventArgs e)
        {
            BeginOnUIThread(() =>
            {
                try
                {

                    e.Dialog.Typing = null;

                    var currentPosition = Items.IndexOf(e.Dialog);

                    var newPosition = currentPosition;
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (// мигает диалог, если просто обновляется последнее сообщение, то номер становится на 1 больше
                            // и сначала удаляем, а потом вставляем на туже позицию
                            i != currentPosition
                            && Items[i].GetDateIndex() <= e.Dialog.GetDateIndex())
                        {
                            newPosition = i;
                            break;
                        }
                    }

                    if (currentPosition != newPosition)
                    {
                        if (currentPosition >= 0
                            && currentPosition < newPosition)
                        {
                            // т.к. будем сначала удалять диалог а потом вставлять, то
                            // curPos + 1 = newPos - это вставка на тоже место и не имеет смысла
                            // Update: имеет, т.к. обновляется инфа о последнем сообщении
                            if (currentPosition + 1 == newPosition)
                            {
                                Items[currentPosition].NotifyOfPropertyChange(() => Items[currentPosition].Self);
                                Items[currentPosition].NotifyOfPropertyChange(() => Items[currentPosition].UnreadCount);
                                return;
                            }
                            Items.Remove(e.Dialog);
                            Items.Insert(newPosition - 1, e.Dialog);
                        }
                        else
                        {
                            Items.Remove(e.Dialog);
                            Items.Insert(newPosition, e.Dialog);
                        }
                    }
                    else
                    {
                        // удалили сообщение и диалог должен переместиться ниже загруженной части списка
                        if (!IsLastSliceLoaded
                            && Items.Count > 0
                            && Items[Items.Count - 1].GetDateIndex() > e.Dialog.GetDateIndex())
                        {
                            Items.Remove(e.Dialog);
                        }

                        Items[currentPosition].NotifyOfPropertyChange(() => Items[currentPosition].Self);
                        Items[currentPosition].NotifyOfPropertyChange(() => Items[currentPosition].UnreadCount);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }



        public void Handle(DialogAddedEventArgs eventArgs)
        {
            OnDialogAdded(this, eventArgs);
        }

        private void OnDialogAdded(object sender, DialogAddedEventArgs e)
        {
            var dialog = e.Dialog;
            if (dialog == null) return;

            BeginOnUIThread(() =>
            {
                var index = -1;
                for (var i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == e.Dialog)
                    {
                        return;
                    }

                    if (Items[i].GetDateIndex() < dialog.GetDateIndex())
                    {
                        index = i;
                        break;
                    }
                }

                if (e.Dialog.Peer is TLPeerChannel)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (e.Dialog.Peer.GetType() == Items[i].Peer.GetType()
                            && e.Dialog.Peer.Id.Value == Items[i].Peer.Id.Value)
                        {
                            Items.RemoveAt(i);
                            Execute.ShowDebugMessage("OnDialogAdded RemoveAt=" + i);
                            break;
                        }
                    }
                }

                if (index == -1)
                {
                    Items.Add(dialog);
                }
                else
                {
                    Items.Insert(index, dialog);
                }
                Status = Items.Count == 0 || LazyItems.Count == 0 ? string.Empty : Status;
            });
        }

        public void Handle(DialogRemovedEventArgs args)
        {
            BeginOnUIThread(() =>
            {
                if (args.Dialog.Peer is TLPeerChannel)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (args.Dialog.Peer.GetType() == Items[i].Peer.GetType()
                            && args.Dialog.Peer.Id.Value == Items[i].Peer.Id.Value)
                        {
                            Items.RemoveAt(i);
                            break;
                        }
                    }
                    return;
                }

                var dialog = Items.FirstOrDefault(x => x.Index == args.Dialog.Index);

                if (dialog != null)
                {
                    Items.Remove(dialog);
                }
            });
        }
    }
}
