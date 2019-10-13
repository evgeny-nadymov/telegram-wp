// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.EmojiPanel;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class MasksViewModel : ItemsViewModelBase<TLStickerSetBase>, 
        Telegram.Api.Aggregator.IHandle<DownloadableItem>, 
        Telegram.Api.Aggregator.IHandle<UpdateStickerSetsOrderEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateStickerSetsEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateNewStickerSetEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateRemoveStickerSetEventArgs>
    {
        public MasksViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Status = AppResources.Loading;
        }

        public void ForwardInAnimationComplete()
        {
            StateService.GetMasksAsync(cachedMasks => BeginOnUIThread(() =>
            {
                _masks = cachedMasks;

                var masks43 = _masks as TLAllStickers43;
                if (masks43 != null)
                {
                    UpdateSets(masks43, () => UpdateMasksAsync(masks43));
                    Status = string.Empty;
                }
                else
                {
                    UpdateMasksAsync(null);
                }
            }));
        }

        public void ReorderStickerSets()
        {
            if (_masks == null) return;

            var oldHash = TLUtils.ToTLInt(_masks.Hash);
            var newHash = TLUtils.GetAllStickersHash(Items);

            if (oldHash.Value != newHash)
            {
                var order = new TLVector<TLLong>();
                foreach (var item in Items)
                {
                    order.Add(item.Id);
                }

                Execute.ShowDebugMessage("ReorderStickers");
                IsWorking = true;
                MTProtoService.ReorderStickerSetsAsync(true, order,
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        var masks = _masks as TLAllStickers32;
                        if (masks != null)
                        {
                            masks.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));
                            masks.Sets = new TLVector<TLStickerSetBase>(Items);
                            var sets = new Dictionary<long, TLVector<TLDocumentBase>>();
                            for (var i = 0; i < masks.Documents.Count; i++)
                            {
                                var document22 = masks.Documents[i] as TLDocument22;
                                if (document22 != null)
                                {
                                    var stickerSetId = document22.StickerSet as TLInputStickerSetId;
                                    if (stickerSetId != null)
                                    {
                                        TLVector<TLDocumentBase> stickers;
                                        if (sets.TryGetValue(stickerSetId.Id.Value, out stickers))
                                        {
                                            stickers.Add(document22);
                                        }
                                        else
                                        {
                                            sets[stickerSetId.Id.Value] = new TLVector<TLDocumentBase>{ document22 };
                                        }
                                    }
                                }
                            }
                            var documents = new TLVector<TLDocumentBase>();
                            for (var i = 0; i < masks.Sets.Count; i++)
                            {
                                TLVector<TLDocumentBase> stickers;
                                if (sets.TryGetValue(masks.Sets[i].Id.Value, out stickers))
                                {
                                    foreach (var sticker in stickers)
                                    {
                                        documents.Add(sticker);
                                    }
                                }
                            }
                            masks.Documents = documents;

                            //EmojiControl emojiControl;
                            //if (EmojiControl.TryGetInstance(out emojiControl))
                            //{
                            //    emojiControl.ReorderStickerSets();
                            //}

                            StateService.SaveMasksAsync(masks);
                        }
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Execute.ShowDebugMessage("messages.reorderStickerSets error " + error);
                    }));
            }
        }

        protected override void OnActivate()
        {
            BrowserNavigationService.MentionNavigated += OnMentionNavigated;

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            BrowserNavigationService.MentionNavigated -= OnMentionNavigated;

            ReorderStickerSets();

            base.OnDeactivate(close);
        }

        private void OnMentionNavigated(object sender, TelegramMentionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Mention))
            {
                var usernameStartIndex = e.Mention.LastIndexOf("@", StringComparison.OrdinalIgnoreCase);
                if (usernameStartIndex != -1)
                {
                    var username = e.Mention.Substring(usernameStartIndex).TrimStart('@');

                    if (!string.IsNullOrEmpty(username))
                    {
                        TelegramViewBase.NavigateToUsername(MTProtoService, username, string.Empty, string.Empty, string.Empty);
                    }
                }
            }
            else if (e.UserId >= 0)
            {
                var user = CacheService.GetUser(new TLInt(e.UserId));
                if (user != null)
                {
                    TelegramViewBase.NavigateToUser(user, null, PageKind.Profile);
                }
            }
            else if (e.ChatId >= 0)
            {
                var chat = CacheService.GetChat(new TLInt(e.ChatId));
                if (chat != null)
                {
                    TelegramViewBase.NavigateToChat(chat, string.Empty);
                }
            }
            else if (e.ChannelId >= 0)
            {
                var channel = CacheService.GetChat(new TLInt(e.ChatId)) as TLChannel;
                if (channel != null)
                {
                    TelegramViewBase.NavigateToChat(channel, string.Empty);
                }
            }
        }

        private readonly Dictionary<string, TLVector<TLStickerItem>> _stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();

        private readonly Dictionary<long, string> _emoticons = new Dictionary<long, string>();

        private TLAllStickers _masks;

        private void UpdateMasksAsync(TLAllStickers cachedMasks)
        {
            var hash = cachedMasks != null ? cachedMasks.Hash : TLString.Empty;

            IsWorking = true;
            MTProtoService.GetMaskStickersAsync(hash,
                result => BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage(result.ToString());

                    Status = string.Empty;
                    IsWorking = false;

                    var masks = result as TLAllStickers43;
                    if (masks != null)
                    {
                        Items.Clear();

                        var cachedMasks29 = cachedMasks as TLAllStickers29;
                        if (cachedMasks29 != null)
                        {
                            masks.ShowStickersTab = cachedMasks29.ShowStickersTab;
                            masks.RecentlyUsed = cachedMasks29.RecentlyUsed;
                            masks.Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
                        }
                        var cachedMasks43 = cachedMasks as TLAllStickers43;
                        if (cachedMasks43 != null)
                        {
                            masks.RecentStickers = cachedMasks43.RecentStickers;
                        }

                        cachedMasks = masks;
                        StateService.SaveMasksAsync(cachedMasks);

                        UpdateSets(masks, () => { });
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    Status = string.Empty;
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.getMaskStickers error " + error);
                }));
        }

        public void Remove(TLStickerSet set)
        {
            if (set == null) return;
            var inputSet = new TLInputStickerSetId { Id = set.Id, AccessHash = set.AccessHash };

            IsWorking = true;
            MTProtoService.UninstallStickerSetAsync(inputSet,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Items.Remove(set);

                    var shellViewModel = IoC.Get<ShellViewModel>();
                    shellViewModel.RemoveMaskSet(set, inputSet);

                    MTProtoService.SetMessageOnTime(2.0, AppResources.StickersRemoved);
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.uninstallStickerSet error " + error);
                }));
        }

        public void Archive(TLStickerSet set)
        {
            var set32 = set as TLStickerSet32;
            if (set32 == null) return;

            var inputStickerSet = new TLInputStickerSetId { Id = set32.Id, AccessHash = set32.AccessHash };
            IsWorking = true;
            MTProtoService.InstallStickerSetAsync(inputStickerSet, TLBool.True,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    set32.Archived = true;
                    Items.Remove(set32);

                    var masks43 = StateService.GetMasks() as TLAllStickers43;
                    TLMessagesStickerSet messagesSet = null;
                    if (masks43 != null)
                    {
                        messagesSet = TLUtils.RemoveStickerSet(masks43, set32);
                        StateService.SaveMasksAsync(masks43);
                    }

                    if (messagesSet != null)
                    {
                        var archivedStickers = StateService.GetArchivedStickers();
                        if (archivedStickers != null)
                        {
                            TLUtils.AddStickerSetCovered(archivedStickers, messagesSet, archivedStickers.SetsCovered, new TLStickerSetCovered{ Cover = messagesSet.Documents.FirstOrDefault() ?? new TLDocumentEmpty { Id = new TLLong(0) }, StickerSet = messagesSet.Set });
                            archivedStickers.Count.Value = archivedStickers.Sets.Count;

                            StateService.SaveArchivedStickersAsync(archivedStickers);
                        }
                    }

                    //EventAggregator.Publish(new UpdateStickerSetsEventArgs(allStickers43));
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.installStickerSet error " + error);
                }));
        }

        public void CopyLink(TLStickerSet set)
        {
            if (set == null) return;

            var shortName = set.ShortName.ToString();
            if (string.IsNullOrEmpty(shortName)) return;

            var addStickersLink = string.Format(Constants.AddStickersLinkPlaceholder, shortName);

            Clipboard.SetText(addStickersLink);
            MTProtoService.SetMessageOnTime(2.0, AppResources.LinkCopiedToClipboard);
        }

        public void Share(TLStickerSet set)
        {
            if (set == null) return;

            var shortName = set.ShortName.ToString();
            if (string.IsNullOrEmpty(shortName)) return;

            var addStickersLink = string.Format(Constants.AddStickersLinkPlaceholder, shortName);

            StateService.ShareLink = addStickersLink;
            StateService.ShareMessage = addStickersLink;
            StateService.ShareCaption = AppResources.Share;
            NavigationService.UriFor<ShareViewModel>().Navigate();
        }

        private void UpdateSets(TLAllStickers29 masks, System.Action callback)
        {
            _masks = masks;

            _emoticons.Clear();
            _stickerSets.Clear();

            for (var i = 0; i < masks.Documents.Count; i++)
            {
                var document22 = masks.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    if (document22.StickerSet != null)
                    {
                        var setId = document22.StickerSet.Name;
                        TLVector<TLStickerItem> stickers;
                        if (_stickerSets.TryGetValue(setId, out stickers))
                        {
                            stickers.Add(new TLStickerItem { Document = document22 });
                        }
                        else
                        {
                            _stickerSets[setId] = new TLVector<TLStickerItem> { new TLStickerItem { Document = document22 } };
                        }
                    }
                }
            }

            Items.Clear();

            var firstChunkSize = 10;
            var count = 0;
            var delayedItems = new List<TLStickerSetBase>();
            for (var i = 0; i < masks.Sets.Count; i++)
            {
                var set = masks.Sets[i];
                var setName = set.Id.ToString();
                TLVector<TLStickerItem> stickers;
                if (_stickerSets.TryGetValue(setName, out stickers))
                {

                    var objects = new TLVector<TLObject>();
                    foreach (var sticker in stickers)
                    {
                        objects.Add(sticker);
                    }

                    set.Stickers = objects;
                    if (set.Stickers.Count > 0)
                    {
                        if (count < firstChunkSize)
                        {
                            Items.Add(set);
                            count++;
                        }
                        else
                        {
                            delayedItems.Add(set);
                        }
                    }
                }
            }

            AddItemsChunk(25, delayedItems, callback);
        }

        private void AddItemsChunk(int chunkSize, List<TLStickerSetBase> delayedItems, System.Action callback)
        {
            BeginOnUIThread(() =>
            {
                for (var i = 0; i < delayedItems.Count && i < chunkSize; i++)
                {
                    Items.Add(delayedItems[0]);
                    delayedItems.RemoveAt(0);
                }

                if (delayedItems.Count > 0)
                {
                    AddItemsChunk(25, delayedItems, callback);
                }
                else
                {
                    callback.SafeInvoke();
                }
            });
        }

        public void ClearRecentStickers()
        {
            IsWorking = true;
            MTProtoService.ClearRecentStickersAsync(
                false,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    MTProtoService.SetMessageOnTime(2.0, AppResources.Done);

                    var masks = StateService.GetMasks();
                    if (masks != null)
                    {
                        var masks29 = masks as TLAllStickers29;
                        if (masks29 != null)
                        {
                            masks29.RecentlyUsed = new TLVector<TLRecentlyUsedSticker>();
                            //EmojiControl emojiControl;
                            //if (EmojiControl.TryGetInstance(out emojiControl))
                            //{
                            //    emojiControl.ClearRecentStickers();
                            //}
                            StateService.SaveMasksAsync(masks29);
                        }
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.clearRecentStickers error " + error);
                }));
        }

        public void Handle(DownloadableItem item)
        {
            var sticker = item.Owner as TLStickerItem;
            if (sticker != null)
            {
                sticker.NotifyOfPropertyChange(() => sticker.Self);
            }
        }

        public void Handle(UpdateStickerSetsOrderEventArgs args)
        {
            if (!args.Masks) return;

            // ui thread
            UpdateSets(args.AllStickers, () => { });
        }

        public void Handle(UpdateStickerSetsEventArgs args)
        {
            if (!args.Masks) return;

            // ui thread
            UpdateSets(args.AllStickers, () => { });
        }

        public void Handle(UpdateNewStickerSetEventArgs args)
        {
            if (!args.Masks) return;

            // ui thread
            UpdateSets(args.AllStickers, () => { });
        }

        public void Handle(UpdateRemoveStickerSetEventArgs args)
        {
            // ui thread
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Id.Value == args.StickerSet.Id.Value)
                {
                    Items.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
