// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using TelegramClient.Controls;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.Views;
using Execute = Telegram.Api.Helpers.Execute;
using TypingTuple = Telegram.Api.WindowsPhone.Tuple<Telegram.Api.TL.TLDialogBase, TelegramClient.ViewModels.Dialogs.InputTypingManager>;
using TypingUser = Telegram.Api.WindowsPhone.Tuple<int, Telegram.Api.TL.TLSendMessageActionBase>;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogsViewModel : ItemsViewModelBase<TLDialogBase>,
        Telegram.Api.Aggregator.IHandle<TLUpdateContactLinkBase>,
        Telegram.Api.Aggregator.IHandle<TopMessageUpdatedEventArgs>,
        Telegram.Api.Aggregator.IHandle<DialogAddedEventArgs>,
        Telegram.Api.Aggregator.IHandle<DialogRemovedEventArgs>,
        Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        Telegram.Api.Aggregator.IHandle<UploadableItem>,
        Telegram.Api.Aggregator.IHandle<string>,
        Telegram.Api.Aggregator.IHandle<TLEncryptedChatBase>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserName>,
        Telegram.Api.Aggregator.IHandle<UpdateCompletedEventArgs>,
        Telegram.Api.Aggregator.IHandle<ChannelUpdateCompletedEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateNotifySettings>,
        Telegram.Api.Aggregator.IHandle<TLUpdateNewAuthorization>,
        Telegram.Api.Aggregator.IHandle<TLUpdateServiceNotification>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserTyping>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChatUserTyping>,
        Telegram.Api.Aggregator.IHandle<ClearCacheEventArgs>,
        Telegram.Api.Aggregator.IHandle<ClearLocalDatabaseEventArgs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEditMessage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEditChannelMessage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateDraftMessage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateChannel>,
        Telegram.Api.Aggregator.IHandle<TLUpdateDialogPinned76>,
        Telegram.Api.Aggregator.IHandle<TLUpdatePinnedDialogs>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEncryptedChatTyping>,
        Telegram.Api.Aggregator.IHandle<ProxyDataChangedEventArgs>
    {
        #region Workaround
        public Visibility EncryptedChatVisibility { get { return Visibility.Collapsed; } }

        public TLObject With { get { return null; } }

        public Visibility VerifiedVisibility { get { return Visibility.Collapsed; } }

        public TLDialogBase Self { get { return null; } }

        public TLMessageBase TopMessage { get { return null; } }

        public Brush MuteIconBackground { get { return new SolidColorBrush(Colors.White); } }

        public TLPeerNotifySettings NotifySettings { get { return null; } }

        public TLInt UnreadCount { get { return new TLInt(0); } }
        #endregion

        public bool FirstRun { get; set; }

        public ICollectionView FilteredItems { get; set; }

        public bool TestMode { get; set; }

        public DialogsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            App.Log("start DialogsViewModel.ctor");

            //Items = new ObservableCollection<TLDialogBase>();
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
                }

                return true;
            };

#if DEBUG
            Items.CollectionChanged += (sender, args) =>
            {
                TLDialog71 promoDialog = null;
                TLDialogBase firstDialog = Items.FirstOrDefault();
                if (args.NewItems != null)
                {
                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        var newItem = args.NewItems[i] as TLDialog71;
                        if (newItem != null && newItem.IsPromo)
                        {
                            promoDialog = newItem;
                            break;
                        }
                    }
                }

                if (promoDialog != null)
                {
                    var stackTrace = string.Format("Promo action={0} dialog={1} first_dialog={2}\n{3}", args.Action, promoDialog.Index, firstDialog != null ? firstDialog.Index.ToString() : null, TLObjectGenerator.GetStackTrace());
                    Execute.BeginOnUIThread(TimeSpan.FromSeconds(20.0), () =>
                    {
                        TLUtils.WriteException(":::Promo:::", new Exception(stackTrace));
                    });
                }
            };
#endif

            EventAggregator.Subscribe(this);
            DisplayName = LowercaseConverter.Convert(AppResources.Dialogs);
            Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.Loading : string.Empty;

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
                            clearedDialogs.Add(dialog);
                            dialogsCache[dialog.Index] = dialog;
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
                            if (cachedDialog.Peer is TLPeerChannel && dialog.Peer is TLPeerChannel)
                            {
                                CacheService.DeleteDialog(dialog);
                                continue;
                            }
                        }
                    }

                    ReorderDrafts(clearedDialogs);

                    // load cache
                    BeginOnUIThread(() =>
                    {
                        Status = dialogs.Count == 0 ? AppResources.Loading : string.Empty;
                        Items.Clear();

                        const int maxDialogSlice = 8;
                        var importantCount = 0;
                        var count = 0;
                        for (var i = 0; i < clearedDialogs.Count && importantCount < maxDialogSlice; i++, count++)
                        {
                            Items.Add(clearedDialogs[i]);
                            var chat41 = clearedDialogs[i].With as TLChat41;
                            if (chat41 == null || !chat41.IsMigrated)
                            {
                                importantCount++;
                            }
                        }


                        if (count < clearedDialogs.Count)
                        {
                            BeginOnUIThread(() =>
                            {
                                for (var i = count; i < clearedDialogs.Count; i++)
                                {
                                    Items.Add(clearedDialogs[i]);
                                }

                                CheckPromoChannel();

                                var importantDialogsCount = Math.Max(Telegram.Api.Constants.CachedDialogsCount, Items.OfType<TLDialog>().Count());
                                UpdateItemsAsync(importantDialogsCount);
                            });
                        }
                        else
                        {
                            var importantDialogsCount = Math.Max(Telegram.Api.Constants.CachedDialogsCount, Items.OfType<TLDialog>().Count());
                            UpdateItemsAsync(importantDialogsCount);
                        }
                    });
                }
            });

            App.Log("stop DialogsViewModel.ctor");
        }

        public void CheckPromoChannel(bool force = false)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (!isAuthorized)
                {
                    return;
                }

                var updateProxyData = false;
                var proxyDataBase = CacheService.GetProxyData();
                var proxyDataPromo = proxyDataBase as TLProxyDataPromo;
                if (proxyDataBase == null)
                {
                    //Execute.ShowDebugMessage("GetPromoDialog");

                    MTProtoService.GetProxyDataAsync(
                        result =>
                        {
                            proxyDataPromo = result as TLProxyDataPromo;
                            if (proxyDataPromo != null)
                            {
                                MTProtoService.GetPromoDialogAsync(MTProtoService.PeerToInputPeer(proxyDataPromo.Peer),
                                    result2 =>
                                    {

                                    });
                            }
                        });

                    return;
                }

                if (proxyDataPromo != null)
                {
                    var now = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
                    var promoChannel = CacheService.GetChat(proxyDataPromo.Peer.Id) as TLChannel;
                    var promoDialog = CacheService.GetDialogs().OfType<TLDialog71>().FirstOrDefault(x => x.IsPromo && x.Peer.Id.Value == proxyDataPromo.Peer.Id.Value);
                    if (promoDialog == null && (promoChannel == null || promoChannel.Left.Value))
                    {
                        //Execute.ShowDebugMessage("GetPromoDialog");

                        MTProtoService.GetPromoDialogAsync(MTProtoService.PeerToInputPeer(proxyDataPromo.Peer),
                            result =>
                            {

                            });
                    }
                    else
//#if !DEBUG
                        if (proxyDataPromo.Expires.Value <= now.Value || force)
//#endif
                    {
                        //Execute.ShowDebugMessage("GetPromoDialog");

                        MTProtoService.GetProxyDataAsync(
                        result =>
                        {
                            proxyDataPromo = result as TLProxyDataPromo;
                            if (proxyDataPromo != null)
                            {
                                MTProtoService.GetPromoDialogAsync(MTProtoService.PeerToInputPeer(proxyDataPromo.Peer),
                                    result2 =>
                                    {

                                    });
                            }
                        });

                        return;
                    }
                }
            });
        }

        private static void ReorderDrafts(IList<TLDialogBase> dialogs)
        {
            for (var i = 0; i < dialogs.Count; i++)
            {
                var dialog53 = dialogs[i] as TLDialog53;
                if (dialog53 != null)
                {
                    var draft = dialog53.Draft as TLDraftMessage;
                    if (draft != null && dialog53.GetDateIndexWithDraft() > dialog53.GetDateIndex())
                    {
                        var dateWithDraft = dialog53.GetDateIndexWithDraft();
                        for (var j = 0; j < i; j++)
                        {
                            if (dateWithDraft >= dialogs[j].GetDateIndexWithDraft())
                            {
                                dialogs.RemoveAt(i);
                                dialogs.Insert(j, dialog53);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private volatile bool _isUpdated;

        private void UpdateItemsAsync(int limit)
        {
            IsWorking = true;

            var stopwatch = Stopwatch.StartNew();
            //TLObject.LogNotify = true;
            //TelegramEventAggregator.LogPublish = true;
            MTProtoService.GetDialogsAsync(stopwatch,
                new TLInt(0), 
                new TLInt(0), 
                new TLInputPeerEmpty(),
                new TLInt(limit),
                new TLInt(0),
                result =>
                {
                    //System.Diagnostics.Debug.WriteLine("messages.getDialogs elapsed=" + stopwatch.Elapsed);

                    // сортируем, т.к. при синхронизации, если есть отправляющиеся сообщений, то TopMessage будет замещен на них
                    // и начальная сортировка сломается
                    var orderedDialogs = new TLVector<TLDialogBase>(result.Dialogs.Count);
                    foreach (var orderedDialog in result.Dialogs.OrderByDescending(x => x.GetDateIndexWithDraft()))
                    {
                        orderedDialogs.Add(orderedDialog);
                    }
                    result.Dialogs = orderedDialogs;

                    //System.Diagnostics.Debug.WriteLine("messages.getDialogs ordering elapsed=" + stopwatch.Elapsed);

                    BeginOnUIThread(() =>
                    {
                        //System.Diagnostics.Debug.WriteLine("messages.getDialogs ui elapsed=" + stopwatch.Elapsed);
                        //TelegramEventAggregator.LogPublish = false;
                        //TLObject.LogNotify = false;
                        IsWorking = false;
                        IsLastSliceLoaded = result.Dialogs.Count < limit;

                        _offset = limit;

                        var needUpdate = Items.Count == 0;
                        var itemsCount = Items.Count;
                        int i = 0, j = 0;
                        var contactRegisteredDialogs = new List<TLDialog>();
                        var promoDialogs = new List<TLDialog>();
                        for (; i < result.Dialogs.Count && j < Items.Count; i++, j++)
                        {
                            if (itemsCount - 1 < i || result.Dialogs[i] != Items[j])
                            {
                                // skip "User joined Telegram!" message
                                var dialog = Items[j] as TLDialog;
                                if (dialog != null)
                                {
                                    var messageService = dialog.TopMessage as TLMessageService;
                                    if (messageService != null && messageService.Action is TLMessageActionContactRegistered)
                                    {
                                        i--;
                                        contactRegisteredDialogs.Add(dialog);
                                        continue;
                                    }
                                }

                                var dialog71 = Items[j] as TLDialog71;
                                if (dialog71 != null && dialog71.IsPromo)
                                {
                                    i--;
                                    promoDialogs.Add(dialog);
                                    continue;
                                }

                                var encryptedDialog = Items[j] as TLEncryptedDialog;
                                if (encryptedDialog != null)
                                {
                                    i--;
                                    continue;
                                }


                                needUpdate = true;
                                break;
                            }
                        }

                        if (i < j)
                        {
                            for (var k = i; k < j; k++)
                            {
                                if (k < result.Dialogs.Count)
                                {
                                    Items.Add(result.Dialogs[k]);
                                }
                            }
                        }

                        // load updated cache
                        Status = Items.Count == 0 && LazyItems.Count == 0 && result.Dialogs.Count == 0 ? AppResources.NoDialogsHere : string.Empty;

                        if (needUpdate)
                        {
                            var startIndex = 0;
                            foreach (var dialog in contactRegisteredDialogs)
                            {
                                for (var k = startIndex; k < result.Dialogs.Count; k++)
                                {
                                    if (dialog.GetDateIndexWithDraft() > result.Dialogs[k].GetDateIndexWithDraft())
                                    {
                                        result.Dialogs.Insert(k, dialog);
                                        startIndex = k;
                                        break;
                                    }
                                }
                            }

                            startIndex = 0;
                            foreach (var dialog in promoDialogs)
                            {
                                for (var k = startIndex; k < result.Dialogs.Count; k++)
                                {
                                    if (dialog.GetDateIndexWithDraft() > result.Dialogs[k].GetDateIndexWithDraft())
                                    {
                                        result.Dialogs.Insert(k, dialog);
                                        startIndex = k;
                                        break;
                                    }
                                }
                            }

                            var encryptedDialogs = Items.OfType<TLEncryptedDialog>();
                            startIndex = 0;
                            foreach (var encryptedDialog in encryptedDialogs)
                            {
                                for (var k = startIndex; k < result.Dialogs.Count; k++)
                                {
                                    if (encryptedDialog.GetDateIndexWithDraft() > result.Dialogs[k].GetDateIndexWithDraft())
                                    {
                                        result.Dialogs.Insert(k, encryptedDialog);
                                        startIndex = k;
                                        break;
                                    }
                                }
                            }

                            var broadcasts = Items.OfType<TLBroadcastDialog>();
                            startIndex = 0;
                            foreach (var broadcast in broadcasts)
                            {
                                for (var k = startIndex; k < result.Dialogs.Count; k++)
                                {
                                    if (broadcast.GetDateIndexWithDraft() > result.Dialogs[k].GetDateIndexWithDraft())
                                    {
                                        result.Dialogs.Insert(k, broadcast);
                                        startIndex = k;
                                        break;
                                    }
                                }
                            }

                            Items.Clear();
                            foreach (var dialog in result.Dialogs)
                            {
                                Items.Add(dialog);
                            }

                            IsLastSliceLoaded = false;
                            _isUpdated = true;
                        }
                        else
                        {
                            _isUpdated = true;
                        }
                        //System.Diagnostics.Debug.WriteLine("messages.getDialogs end ui elapsed=" + stopwatch.Elapsed);
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    _isUpdated = true;
                    Status = string.Empty;
                    IsWorking = false;
                }));
        }

        private bool _checkPromoChannel;

        protected override void OnActivate()
        {
            base.OnActivate();

            if (FirstRun)
            {
                OnInitialize();
            }

            if (_checkPromoChannel)
            {
                CheckPromoChannel();
            }
            else
            {
                _checkPromoChannel = true;
            }
        }

        protected override void OnInitialize()
        {
            BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (!FirstRun)
                {
                    return;
                }
                if (!isAuthorized)
                {
                    return;
                }

                FirstRun = false;

                Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.Loading : string.Empty;
                var limit = Constants.DialogsSlice;
                MTProtoService.GetDialogsAsync(Stopwatch.StartNew(),
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInputPeerEmpty(),
                    new TLInt(limit),
                    new TLInt(0),
                    dialogs => Execute.BeginOnUIThread(() =>
                    {
                        IsLastSliceLoaded = dialogs.Dialogs.Count < limit;
                        _offset = Constants.DialogsSlice;

                        _isUpdated = true;

                        const int maxDialogSlice = 8;
                        var importantCount = 0;
                        Items.Clear();
                        for (var i = 0; i < dialogs.Dialogs.Count; i++)
                        {
                            if (importantCount < maxDialogSlice)
                            {
                                Items.Add(dialogs.Dialogs[i]);
                            }
                            else
                            {
                                LazyItems.Add(dialogs.Dialogs[i]);
                            }

                            var chat41 = dialogs.Dialogs[i].With as TLChat41;
                            if (chat41 == null || !chat41.IsMigrated)
                            {
                                importantCount++;
                            }
                        }

                        Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.NoDialogsHere : string.Empty;

                        if (LazyItems.Count > 0)
                        {
                            BeginOnUIThread(() =>
                            {
                                for (var i = 0; i < LazyItems.Count; i++)
                                {
                                    Items.Add(LazyItems[i]);
                                }
                                LazyItems.Clear();

                                InvokeImportContacts();
                            });
                        }
                        else
                        {
                            InvokeImportContacts();
                        }
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        InvokeImportContacts();

                        Execute.ShowDebugMessage("messages.getDialogs error " + error);
                        _isUpdated = true;
                        Status = string.Empty;
                    }));
            });

            base.OnInitialize();
        }

        private void InvokeImportContacts()
        {
            var contacts = IoC.Get<ContactsViewModel>();
            contacts.Handle(new InvokeImportContacts());
        }

        #region Actions



        public FrameworkElement OpenDialogElement;

        public void SetOpenDialogElement(object element)
        {
            OpenDialogElement = element as FrameworkElement;
        }



        public override void RefreshItems()
        {
            UpdateItemsAsync(Constants.DialogsSlice);
        }

        #endregion

        public void Handle(TopMessageUpdatedEventArgs eventArgs)
        {
            eventArgs.Dialog.NotifyOfPropertyChange(() => eventArgs.Dialog.With);
            OnTopMessageUpdated(this, eventArgs);
        }

        public void Handle(TLUpdatePinnedDialogs update)
        {
            Handle(new UpdateCompletedEventArgs());
        }

        public void Handle(TLUpdateDialogPinned76 update)
        {
            var dialogPeer = update.Peer as TLDialogPeer;
            if (dialogPeer != null)
            {
                OnTopMessageUpdated(this, new TopMessageUpdatedEventArgs(dialogPeer.Peer) { NotifyPinned = true });
            }

            return;
        }

        public void Handle(TLUpdateDraftMessage update)
        {
            OnTopMessageUpdated(this, new TopMessageUpdatedEventArgs(update.Peer));
            return;

            Execute.BeginOnUIThread(() =>
            {
                TLDialog53 dialog53 = null;
                for (var i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Index == update.Peer.Id.Value)
                    {
                        dialog53 = Items[i] as TLDialog53;
                        if (dialog53 != null)
                        {
                            dialog53.Draft = update.Draft;

                            dialog53.NotifyOfPropertyChange(() => dialog53.Self);
                        }
                        Items.RemoveAt(i);
                        break;
                    }
                }

                if (dialog53 != null)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].GetDateIndexWithDraft() <= dialog53.GetDateIndexWithDraft())
                        {
                            Items.Insert(i, dialog53);
                            break;
                        }
                    }
                }
            });
        }

        public void Handle(ChannelUpdateCompletedEventArgs args)
        {
            var dialog = CacheService.GetDialog(new TLPeerChannel { Id = args.ChannelId });
            if (dialog != null)
            {
                var topMessage = dialog.Messages.FirstOrDefault();
                if (topMessage != null)
                {
                    Handle(new TopMessageUpdatedEventArgs(dialog, topMessage));
                }
            }
        }

        public void Handle(DialogAddedEventArgs eventArgs)
        {
            OnDialogAdded(this, eventArgs);
        }

        private void OnTopMessageUpdatedInternal(object sender, TopMessageUpdatedEventArgs e)
        {
            if (e.Dialog == null)
            {
                if (e.Peer != null)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Peer.GetType() == e.Peer.GetType()
                            && Items[i].Index == e.Peer.Id.Value)
                        {
                            e = new TopMessageUpdatedEventArgs(Items[i], (TLMessageBase)null);
                            break;
                        }
                    }
                }

                if (e.Dialog == null) return;
            }

            if (e.NotifyPinned) e.Dialog.NotifyOfPropertyChange(() => e.Dialog.IsPinned);

            int currentPosition;
            int newPosition;
            try
            {
                var chat = e.Dialog.With as TLChat;
                if (chat != null)
                {
                    var dialog = e.Dialog as TLDialog;
                    if (dialog != null)
                    {
                        var topMessage = dialog.TopMessage as TLMessageService;
                        if (topMessage != null)
                        {
                            var actionChatMigratedTo = topMessage.Action as TLMessageActionChatMigrateTo;
                            if (actionChatMigratedTo != null)
                            {
                                Items.Remove(e.Dialog);
                                return;
                            }

                            var actionChatDeleteUser = topMessage.Action as TLMessageActionChatDeleteUser;
                            if (actionChatDeleteUser != null)
                            {
                                if (actionChatDeleteUser.UserId.Value == StateService.CurrentUserId)
                                {
                                    Items.Remove(e.Dialog);
                                    return;
                                }
                            }
                        }
                    }
                }

                var channel = e.Dialog.With as TLChannel;
                if (channel != null)
                {
                    var dialog = e.Dialog as TLDialog;
                    if (dialog != null)
                    {
                        var topMessage = dialog.TopMessage as TLMessageService;
                        if (topMessage != null)
                        {
                            var actionChatDeleteUser = topMessage.Action as TLMessageActionChatDeleteUser;
                            if (actionChatDeleteUser != null)
                            {
                                if (actionChatDeleteUser.UserId.Value == StateService.CurrentUserId)
                                {
                                    Items.Remove(e.Dialog);
                                    return;
                                }
                            }
                        }
                    }
                }

                e.Dialog.Typing = null;

                currentPosition = Items.IndexOf(e.Dialog);

                newPosition = currentPosition;
                for (var i = 0; i < Items.Count; i++)
                {
                    if (// мигает диалог, если просто обновляется последнее сообщение, то номер становится на 1 больше
                        // и сначала удаляем, а потом вставляем на туже позицию
                        i != currentPosition
                        && Items[i].GetDateIndexWithDraft() <= e.Dialog.GetDateIndexWithDraft())
                    {
                        newPosition = i;
                        break;
                    }
                }

                if (currentPosition == -1
                    && currentPosition == newPosition)
                {
                    Execute.ShowDebugMessage("TLDialog with=" + e.Dialog.With + " curPos=newPos=-1 isLastSliceLoaded=" + IsLastSliceLoaded);

                    // channels not from current slice
                    if (IsLastSliceLoaded)
                    {
                        Items.Add(e.Dialog);
                        Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : Status;

                        return;
                    }
                    else
                    {
                        return;
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
                            var dialog71 = Items[currentPosition] as TLDialog71;
                            if (dialog71 != null)
                            {
                                dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMentionsCount);
                            }

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
                        && Items[Items.Count - 1].GetDateIndexWithDraft() > e.Dialog.GetDateIndexWithDraft())
                    {
                        Items.Remove(e.Dialog);
                    }

                    Items[currentPosition].NotifyOfPropertyChange(() => Items[currentPosition].Self);
                    Items[currentPosition].NotifyOfPropertyChange(() => Items[currentPosition].UnreadCount);
                    var dialog71 = Items[currentPosition] as TLDialog71;
                    if (dialog71 != null)
                    {
                        dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMentionsCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write(string.Format("DialogsViewModel.Handle OnTopMessageUpdatedEventArgs ex " + ex));
                throw ex;
            }
        }

        private void OnTopMessageUpdated(object sender, TopMessageUpdatedEventArgs e)
        {
            BeginOnUIThread(() =>
            {
                OnTopMessageUpdatedInternal(sender, e);
            });
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
                    if (Items[i].Index == e.Dialog.Index
                        && Items[i].Peer.GetType() == e.Dialog.Peer.GetType())
                    {
                        return;
                    }

                    if (Items[i] == e.Dialog)
                    {
                        return;
                    }

                    if (Items[i].GetDateIndexWithDraft() < dialog.GetDateIndexWithDraft())
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
                Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : Status;
            });
        }

        public void Handle(DialogRemovedEventArgs args)
        {
            BeginOnUIThread(() =>
            {
                var dialog = Items.FirstOrDefault(x => x.Index == args.Dialog.Index);

                if (dialog != null)
                {
                    Items.Remove(dialog);
                    Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.NoDialogsHere : string.Empty;
                }
            });
        }

        public void Handle(DownloadableItem item)
        {
            var photo = item.Owner as TLUserProfilePhoto;
            if (photo != null)
            {
                var user = CacheService.GetUser(photo);
                if (user != null)
                {
                    user.NotifyOfPropertyChange(() => user.Photo);
                }
                else
                {
                    Execute.ShowDebugMessage("Handle TLUserProfilePhoto user=null");
                }
                return;
            }

            var chatPhoto = item.Owner as TLChatPhoto;
            if (chatPhoto != null)
            {
                var chat = CacheService.GetChat(chatPhoto);
                if (chat != null)
                {
                    chat.NotifyOfPropertyChange(() => chat.Photo);
                    return;
                }

                var channel = CacheService.GetChannel(chatPhoto);
                if (channel != null)
                {
                    channel.NotifyOfPropertyChange(() => channel.Photo);
                    return;
                }

                Execute.ShowDebugMessage("Handle TLChatPhoto chat=null");

                return;
            }
        }

        public void Handle(string command)
        {
            if (string.Equals(command, Commands.LogOutCommand))
            {
                LazyItems.Clear();
                BeginOnUIThread(() => Items.Clear());
                Status = string.Empty;
                IsWorking = false;
            }
        }

        public void Handle(TLUpdateUserName userName)
        {
            Execute.BeginOnUIThread(() =>
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (Items[i].WithId == userName.UserId.Value
                        && Items[i].With is TLUserBase)
                    {
                        var user = (TLUserBase)Items[i].With;
                        user.FirstName = userName.FirstName;
                        user.LastName = userName.LastName;

                        var userWithUserName = user as IUserName;
                        if (userWithUserName != null)
                        {
                            userWithUserName.UserName = userName.UserName;
                        }

                        Items[i].NotifyOfPropertyChange(() => Items[i].With);
                        break;
                    }
                }
            });
        }

        public void Handle(UploadableItem item)
        {
            var userBase = item.Owner as TLUserBase;
            if (userBase != null && userBase.IsSelf)
            {
                MTProtoService.UploadProfilePhotoAsync(
                    new TLInputFile
                    {
                        Id = item.FileId,
                        MD5Checksum = new TLString(MD5Core.GetHashString(item.Bytes).ToLowerInvariant()),
                        Name = new TLString(Guid.NewGuid() + ".jpg"),
                        Parts = new TLInt(item.Parts.Count)
                    },
                    result =>
                    {
                        MTProtoService.GetFullUserAsync(new TLInputUserSelf(),
                            userFull => Execute.BeginOnUIThread(() =>
                            {
                                userFull.User.NotifyOfPropertyChange(() => userFull.User.Photo);
                            }),
                            error =>
                            {

                            });
                    },
                    error =>
                    {

                    });
                return;
            }

            var channel = item.Owner as TLChannel;
            if (channel != null)
            {
                if (channel.Id != null)
                {
                    MTProtoService.EditPhotoAsync(
                        channel,
                        new TLInputChatUploadedPhoto56
                        {
                            File = new TLInputFile
                            {
                                Id = item.FileId,
                                MD5Checksum = new TLString(MD5Core.GetHashString(item.Bytes).ToLowerInvariant()),
                                Name = new TLString("channelPhoto.jpg"),
                                Parts = new TLInt(item.Parts.Count)
                            }
                        },
                        result =>
                        {

                        },
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.editChatPhoto error " + error);
                        });
                }
            }

            var chat = item.Owner as TLChat;
            if (chat != null)
            {
                MTProtoService.EditChatPhotoAsync(
                    chat.Id,
                    new TLInputChatUploadedPhoto56
                    {
                        File = new TLInputFile
                        {
                            Id = item.FileId,
                            MD5Checksum = new TLString(MD5Core.GetHashString(item.Bytes).ToLowerInvariant()),
                            Name = new TLString("chatPhoto.jpg"),
                            Parts = new TLInt(item.Parts.Count)
                        }
                    },
                    result =>
                    {

                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.editChatPhoto error " + error);
                    });
            }
        }

        public void Handle(TLEncryptedChatBase chat)
        {
            Execute.BeginOnUIThread(() =>
            {
                int index = -1;
                TLDialogBase dialog = null;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Peer is TLPeerEncryptedChat
                        && Items[i].Peer.Id.Value == chat.Id.Value)
                    {
                        index = i;
                        dialog = Items[i];
                        break;
                    }
                }

                if (index != -1 && dialog != null)
                {
                    dialog.NotifyOfPropertyChange(() => dialog.Self);
                }
            });
        }

        public IList<TLDialogBase> UpdateCompletedDialogs { get; set; }

        public void Handle(UpdateCompletedEventArgs args)
        {
            var dialogs = CacheService.GetDialogs();

            var promoDialogs = dialogs.OfType<TLDialog71>().Where(x => x.IsPromo).ToList();
            if (promoDialogs.Count > 1)
            {
                var hasPromo = false;
                for (var i = 0; i < dialogs.Count; i++)
                {
                    var dialog71 = dialogs[i] as TLDialog71;
                    if (dialog71 != null && dialog71.IsPromo)
                    {
                        if (hasPromo)
                        {
                            dialogs.RemoveAt(i--);
                        }
                        else
                        {
                            hasPromo = true;
                        }
                    }
                }

                var error = new StringBuilder();
                error.AppendLine("promo_count=" + promoDialogs.Count);
                foreach (var promoDialog in promoDialogs)
                {
                    error.AppendLine(string.Format("id={0} hash={1}", promoDialog.Index, promoDialog.GetHashCode()));
                }

                Execute.ShowDebugMessage(error.ToString());
            }

            ReorderDrafts(dialogs);

            Execute.BeginOnUIThread(() =>
            {
                var rootFrame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (rootFrame != null)
                {
                    var shellView = rootFrame.Content as ShellView;
                    if (shellView == null)
                    {
                        UpdateCompletedDialogs = dialogs;
                    }
                    else
                    {
                        UpdateCompleted(dialogs);
                    }
                }
            });
        }

        public void UpdateCompleted(IList<TLDialogBase> dialogs)
        {
            var count = Items.Count == 0 ? dialogs.Count : Items.Count;

            ((BindableCollection<TLDialogBase>)Items).IsNotifying = false;
            Items.Clear();
            foreach (var dialog in dialogs.Take(count))
            {
                Items.Add(dialog);
            }
            ((BindableCollection<TLDialogBase>)Items).IsNotifying = true;
            ((BindableCollection<TLDialogBase>)Items).Refresh();

#if DEBUG
            if (Items.Count >= 2 && Items[0].Index == Items[1].Index)
            {
                Execute.ShowDebugMessage("UpdateCompleted same dialog");
            }
#endif

            Status = Items.Count > 0 || LazyItems.Count > 0 ? string.Empty : Status;
        }

        private void HandleTypingCommon(TLInt chatId, TLSendMessageActionBase action, Dictionary<int, TypingTuple> typingCache)
        {
            Execute.BeginOnUIThread(() =>
            {
                var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                if (frame != null)
                {
                    var shellView = frame.Content as ShellView;
                    if (shellView == null)
                    {
                        return;
                    }
                }

                TypingTuple tuple;
                if (!typingCache.TryGetValue(chatId.Value, out tuple))
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var dialog = Items[i] as TLEncryptedDialog;
                        if (dialog != null && dialog.Peer.Id.Value == chatId.Value)
                        {
                            tuple = new TypingTuple(dialog, new InputTypingManager(
                                users => Execute.BeginOnUIThread(() =>
                                {
                                    dialog.Typing = GetTyping(dialog.Peer, users, CacheService.GetUser, GetFullInfoAsync);
                                    dialog.NotifyOfPropertyChange(() => dialog.Self.Typing);
                                }),
                                () => Execute.BeginOnUIThread(() =>
                                {
                                    dialog.Typing = null;
                                    dialog.NotifyOfPropertyChange(() => dialog.Self.Typing);
                                })));
                            typingCache[chatId.Value] = tuple;
                            break;
                        }
                    }
                }

                if (tuple != null)
                {
                    if (action is TLSendMessageCancelAction)
                    {
                        tuple.Item2.RemoveTypingUser(tuple.Item1.WithId);
                    }
                    else
                    {
                        tuple.Item2.AddTypingUser(tuple.Item1.WithId, action);
                    }
                }
            });
        }

        private void HandleTypingCommon(TLUpdateTypingBase updateTyping, Dictionary<int, TypingTuple> typingCache)
        {
            Execute.BeginOnUIThread(() =>
            {
                var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                if (frame != null)
                {
                    var shellView = frame.Content as ShellView;
                    if (shellView == null)
                    {
                        return;
                    }
                }

                var updateChatUserTyping = updateTyping as TLUpdateChatUserTyping;
                var id = updateChatUserTyping != null ? updateChatUserTyping.ChatId : updateTyping.UserId;
                TypingTuple tuple;
                if (!typingCache.TryGetValue(id.Value, out tuple))
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (updateChatUserTyping == null
                            && Items[i].Peer is TLPeerUser
                            && Items[i].Peer.Id.Value == id.Value
                            || (updateChatUserTyping != null
                                && Items[i].Peer is TLPeerChat
                                && Items[i].Peer.Id.Value == id.Value)
                            || (updateChatUserTyping != null
                                && Items[i].Peer is TLPeerChannel
                                && Items[i].Peer.Id.Value == id.Value))
                        {
                            var dialog = Items[i] as TLDialog;
                            if (dialog != null)
                            {
                                tuple = new TypingTuple(dialog, new InputTypingManager(
                                    users => Execute.BeginOnUIThread(() =>
                                    {
                                        dialog.Typing = GetTyping(dialog.Peer, users, CacheService.GetUser, GetFullInfoAsync);
                                        dialog.NotifyOfPropertyChange(() => dialog.Self.Typing);
                                    }),
                                    () => Execute.BeginOnUIThread(() =>
                                    {
                                        dialog.Typing = null;
                                        dialog.NotifyOfPropertyChange(() => dialog.Self.Typing);
                                    })));
                                typingCache[id.Value] = tuple;
                            }
                            break;
                        }
                    }
                }

                if (tuple != null)
                {
                    TLSendMessageActionBase action = null;
                    var typingAction = updateTyping as IUserTypingAction;
                    if (typingAction != null)
                    {
                        action = typingAction.Action;
                    }

                    if (action is TLSendMessageCancelAction)
                    {
                        tuple.Item2.RemoveTypingUser(updateTyping.UserId.Value);
                    }
                    else
                    {
                        tuple.Item2.AddTypingUser(updateTyping.UserId.Value, action);
                    }
                }
            });
        }

        private readonly Dictionary<int, TypingTuple> _encryptedChatTypingCache = new Dictionary<int, TypingTuple>();

        public void Handle(TLUpdateEncryptedChatTyping encryptedChatTyping)
        {
            HandleTypingCommon(encryptedChatTyping.ChatId, new TLSendMessageTypingAction(), _encryptedChatTypingCache);
        }

        public void Handle(TLDecryptedMessageBase messageBase)
        {
            var serviceMessage = messageBase as TLDecryptedMessageService;
            if (serviceMessage != null)
            {
                var typingAction = serviceMessage.Action as TLDecryptedMessageActionTyping;
                if (typingAction != null)
                {
                    HandleTypingCommon(messageBase.ChatId, typingAction.Action, _encryptedChatTypingCache);
                }
            }
        }

        private readonly Dictionary<int, TypingTuple> _userTypingCache = new Dictionary<int, TypingTuple>();

        public void Handle(TLUpdateUserTyping userTyping)
        {
            HandleTypingCommon(userTyping, _userTypingCache);
        }

        private readonly Dictionary<int, TypingTuple> _chatUserTypingCache = new Dictionary<int, TypingTuple>();

        public void Handle(TLUpdateChatUserTyping chatUserTyping)
        {
            HandleTypingCommon(chatUserTyping, _chatUserTypingCache);
        }

        public static Typing GetTyping(TLPeerBase peer, IList<TypingUser> typingUsers, Func<TLInt, TLUserBase> getUser, Action<TLPeerBase> getFullInfoAction)
        {
            if (peer is TLPeerUser
                || peer is TLPeerEncryptedChat)
            {
                var typingUser = typingUsers.FirstOrDefault();
                if (typingUser != null)
                {
                    var action = typingUser.Item2;
                    if (action is TLSendMessageUploadPhotoAction)
                    {
                        return new Typing(TypingType.Upload, AppResources.SendingPhoto.ToLower(CultureInfo.InvariantCulture));
                    }
                    if (action is TLSendMessageRecordAudioAction)
                    {
                        return new Typing(TypingType.Record, AppResources.RecordingVoiceMessage.ToLower(CultureInfo.InvariantCulture));
                    }
                    if (action is TLSendMessageUploadAudioAction)
                    {
                        return new Typing(TypingType.Record, AppResources.SendingAudio.ToLower(CultureInfo.InvariantCulture));
                    }
                    if (action is TLSendMessageUploadDocumentAction)
                    {
                        return new Typing(TypingType.Upload, AppResources.SendingFile.ToLower(CultureInfo.InvariantCulture));
                    }
                    if (action is TLSendMessageRecordVideoAction)
                    {
                        return new Typing(TypingType.Upload, AppResources.RecordingVideo.ToLower(CultureInfo.InvariantCulture));
                    }
                    if (action is TLSendMessageUploadVideoAction)
                    {
                        return new Typing(TypingType.Upload, AppResources.SendingVideo.ToLower(CultureInfo.InvariantCulture));
                    }
                    if (action is TLSendMessageGamePlayAction)
                    {
                        return new Typing(TypingType.Text, AppResources.PlayingGame.ToLower(CultureInfo.InvariantCulture));
                    }
                }

                return new Typing(TypingType.Text, AppResources.Typing.ToLower(CultureInfo.InvariantCulture));
            }

            if (typingUsers.Count == 1)
            {
                var userId = new TLInt(typingUsers[0].Item1);
                var user = getUser(userId);
                if (user == null)
                {
                    getFullInfoAction.SafeInvoke(peer);

                    return null;
                }

                var userName = TLString.IsNullOrEmpty(user.FirstName) ? user.LastName : user.FirstName;
                var typingUser = typingUsers.FirstOrDefault();
                if (typingUser != null)
                {
                    var action = typingUser.Item2;
                    if (action is TLSendMessageUploadPhotoAction)
                    {
                        return new Typing(TypingType.Upload, string.Format("{0} {1}", userName, AppResources.IsSendingPhoto.ToLower(CultureInfo.InvariantCulture)));
                    }
                    if (action is TLSendMessageUploadAudioAction)
                    {
                        return new Typing(TypingType.Record, string.Format("{0} {1}", userName, AppResources.IsSendingAudio.ToLower(CultureInfo.InvariantCulture)));
                    }
                    if (action is TLSendMessageRecordAudioAction)
                    {
                        return new Typing(TypingType.Record, string.Format("{0} {1}", userName, AppResources.IsRecordingAudio.ToLower(CultureInfo.InvariantCulture)));
                    }
                    if (action is TLSendMessageUploadDocumentAction)
                    {
                        return new Typing(TypingType.Upload, string.Format("{0} {1}", userName, AppResources.IsSendingFile.ToLower(CultureInfo.InvariantCulture)));
                    }
                    if (action is TLSendMessageRecordVideoAction)
                    {
                        return new Typing(TypingType.Upload, string.Format("{0} {1}", userName, AppResources.IsRecordingVideo.ToLower(CultureInfo.InvariantCulture)));
                    }
                    if (action is TLSendMessageUploadVideoAction)
                    {
                        return new Typing(TypingType.Upload, string.Format("{0} {1}", userName, AppResources.IsSendingVideo.ToLower(CultureInfo.InvariantCulture)));
                    }
                    if (action is TLSendMessageGamePlayAction)
                    {
                        return new Typing(TypingType.Text, string.Format("{0} {1}", userName, AppResources.IsPlayingGame.ToLower(CultureInfo.InvariantCulture)));
                    }
                }

                return new Typing(TypingType.Text, string.Format("{0} {1}", userName, AppResources.IsTyping.ToLower(CultureInfo.InvariantCulture)));
            }

            if (typingUsers.Count <= 3)
            {
                var firstNames = new List<string>(typingUsers.Count);
                var missingUsers = new List<TLInt>();
                foreach (var typingUser in typingUsers)
                {
                    var user = getUser(new TLInt(typingUser.Item1));
                    if (user != null)
                    {
                        var userName = TLString.IsNullOrEmpty(user.FirstName) ? user.LastName : user.FirstName;
                        firstNames.Add(userName.ToString());
                    }
                    else
                    {
                        missingUsers.Add(new TLInt(typingUser.Item1));
                    }
                }

                if (missingUsers.Count > 0)
                {
                    getFullInfoAction.SafeInvoke(peer);

                    return null;
                }

                return new Typing(TypingType.Text, string.Format("{0} {1}", string.Join(", ", firstNames),
                    AppResources.AreTyping.ToLower(CultureInfo.InvariantCulture)));
            }

            return new Typing(TypingType.Text, string.Format("{0} {1}", Language.Declension(
                typingUsers.Count,
                AppResources.CompanyNominativeSingular,
                AppResources.CompanyNominativePlural,
                AppResources.CompanyGenitiveSingular,
                AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture),
                AppResources.AreTyping.ToLower(CultureInfo.InvariantCulture)));
        }

        private void GetFullInfoAsync(TLPeerBase peer)
        {
            var peerChat = peer as TLPeerChat;
            if (peerChat != null)
            {
                MTProtoService.GetFullChatAsync(peerChat.Id, result => { }, error => { });
            }
        }

        public void Handle(ClearCacheEventArgs args)
        {
            BeginOnUIThread(() =>
            {
                foreach (var item in Items)
                {
                    item.NotifyOfPropertyChange(() => item.With);
                }
            });
        }

        public void Handle(ClearLocalDatabaseEventArgs args)
        {
            BeginOnUIThread(() =>
            {
                foreach (var item in Items)
                {
                    if (item.With != null)
                    {
                        item.With.ClearBitmap();
                    }
                }
            });
        }

        public void Handle(TLUpdateEditMessage update)
        {
            var message = update.Message as TLMessageCommon;
            if (message == null) return;

            Execute.BeginOnUIThread(() =>
            {
                int index;
                if (message.ToId is TLPeerUser)
                {
                    index = message.Out.Value ? message.ToId.Id.Value : message.FromId.Value;
                }
                else
                {
                    index = message.ToId.Id.Value;
                }

                for (var i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Index == index
                        && Items[i].TopMessageId != null
                        && Items[i].TopMessageId.Value == message.Index)
                    {
                        Items[i].NotifyOfPropertyChange(() => Items[i].Self);
                    }
                }
            });
        }

        public void Handle(TLUpdateEditChannelMessage update)
        {
            var message = update.Message as TLMessageCommon;
            if (message == null) return;

            Execute.BeginOnUIThread(() =>
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Index == message.ToId.Id.Value
                        && Items[i].TopMessageId != null
                        && Items[i].TopMessageId.Value == message.Index)
                    {
                        Items[i].NotifyOfPropertyChange(() => Items[i].Self);
                    }
                }
            });
        }

        public void Handle(TLUpdateChannel update)
        {
            Execute.BeginOnUIThread(() =>
            {
                var dialog = Items.FirstOrDefault(x => x.Peer is TLPeerChannel && x.Peer.Id.Value == update.ChannelId.Value);
                if (dialog != null)
                {
                    dialog.NotifyOfPropertyChange(() => dialog.Self); // update draft on set current user as admin
                }
            });
        }

        public void Handle(ProxyDataChangedEventArgs args)
        {
            //Execute.BeginOnUIThread(() =>
            //{
            //    var dataPromo = args.ProxyData as TLProxyDataPromo;
            //    if (dataPromo != null && dataPromo.Channel != null)
            //    {
            //        Handle(new DialogAddedEventArgs(dataPromo.Channel));
            //    }
            //    else
            //    {
            //        var promoChannel = Items.OfType<TLDialog71>().FirstOrDefault(x => x.IsPromo);
            //        if (promoChannel != null)
            //        {
            //            Handle(new DialogRemovedEventArgs(promoChannel));
            //        }
            //    }
            //});
        }
    }
}
