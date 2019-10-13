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
using System.Windows.Controls;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.EmojiPanel;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.Views;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class StickersViewModel : ItemsViewModelBase<TLStickerSetBase>, 
        Telegram.Api.Aggregator.IHandle<DownloadableItem>, 
        Telegram.Api.Aggregator.IHandle<UpdateStickerSetsOrderEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateStickerSetsEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateNewStickerSetEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateRemoveStickerSetEventArgs>,
        Telegram.Api.Aggregator.IHandle<UpdateReadFeaturedStickersEventArgs>
    {
        public StickersViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Status = AppResources.Loading;
        }

        private void UpdateShowStickersByEmojiSubtitle(ShowStickersByEmoji showStickers)
        {
            switch (showStickers)
            {
                case Telegram.Api.TL.ShowStickersByEmoji.AllSets:
                    ShowStickersByEmojiSubtitle = AppResources.AllSets;
                    break;
                case Telegram.Api.TL.ShowStickersByEmoji.MySets:
                    ShowStickersByEmojiSubtitle = AppResources.MySets;
                    break;
                case Telegram.Api.TL.ShowStickersByEmoji.None:
                    ShowStickersByEmojiSubtitle = AppResources.None;
                    break;
            }
        }

        private void UpdateFeaturedSetsString(int count)
        {
            if (count == 0)
            {
                FeaturedStickersSubtitle = string.Empty;
            }
            if (count > 0)
            {
                FeaturedStickersSubtitle = string.Format(AppResources.NewFeaturedStickers, count);
            }
        }

        private void UpdateArchivedSetsString(int count)
        {
            if (count == 0)
            {
                ArchivedStickersSubtitle = LowercaseConverter.Convert(AppResources.NoSets);
            }
            else if (count > 0)
            {
                ArchivedStickersSubtitle = Language.Declension(
                    count,
                    AppResources.SetNominativeSingular,
                    AppResources.SetNominativePlural,
                    AppResources.SetGenitiveSingular,
                    AppResources.SetGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }
        }

        private string _showStickersByEmojiSubtitle = " ";

        public string ShowStickersByEmojiSubtitle
        {
            get { return _showStickersByEmojiSubtitle; }
            set { SetField(ref _showStickersByEmojiSubtitle, value, () => ShowStickersByEmojiSubtitle); }
        }

        private string _featuredStickersSubtitle = " ";

        public string FeaturedStickersSubtitle
        {
            get { return _featuredStickersSubtitle; }
            set { SetField(ref _featuredStickersSubtitle, value, () => FeaturedStickersSubtitle); }
        }

        private string _archivedStickersSubtitle = " ";

        public string ArchivedStickersSubtitle
        {
            get { return _archivedStickersSubtitle; }
            set { SetField(ref _archivedStickersSubtitle, value, () => ArchivedStickersSubtitle); }
        }

        public void ForwardInAnimationComplete()
        {
            StateService.GetAllStickersAsync(cachedAllStickers => BeginOnUIThread(() =>
            {
                _allStickers = cachedAllStickers;

                var allStickers = _allStickers as TLAllStickers43;
                if (allStickers != null)
                {
                    UpdateShowStickersByEmojiSubtitle(allStickers.ShowStickersByEmoji);

                    UpdateSets(allStickers, () => UpdateAllStickersAsync(allStickers));
                    Status = string.Empty;
                }
                else
                {
                    UpdateAllStickersAsync(null);
                }
            }));

            StateService.GetFeaturedStickersAsync(cachedFeaturedStickers => BeginOnUIThread(() =>
            {
                if (cachedFeaturedStickers != null)
                {
                    UpdateFeaturedSetsString(cachedFeaturedStickers.Unread.Count);
                }

                _featuredStickers = cachedFeaturedStickers;
                
                var featuredStickersHash = _featuredStickers != null
                    ? _featuredStickers.HashValue
                    : new TLInt(0);

                MTProtoService.GetFeaturedStickersAsync(false, featuredStickersHash,
                    result => BeginOnUIThread(() =>
                    {
                        var featuredStickers = result as TLFeaturedStickers;
                        if (featuredStickers != null)
                        {
                            if (_featuredStickers != null)
                            {
                                _featuredStickers.Unread = featuredStickers.Unread;
                            }
                            UpdateFeaturedSetsString(featuredStickers.Unread.Count);
                        }
                    }),
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getFeaturedStickers error " + error);
                    });
            }));

            StateService.GetArchivedStickersAsync(cachedArchivedStickers => BeginOnUIThread(() =>
            {
                if (cachedArchivedStickers != null)
                {
                    UpdateArchivedSetsString(cachedArchivedStickers.Count.Value);
                }

                _archivedStickers = cachedArchivedStickers;

                MTProtoService.GetArchivedStickersAsync(false, new TLLong(0), new TLInt(0),
                    result => BeginOnUIThread(() =>
                    {
                        UpdateArchivedSetsString(result.Count.Value);
                    }),
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getArchivedStickers error " + error);
                    });
            }));
        }

        public void ReorderStickerSets()
        {
            if (_allStickers == null) return;
            if (_delayedItems != null && _delayedItems.Count > 0) return;

            var oldHash = TLUtils.ToTLInt(_allStickers.Hash);
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
                MTProtoService.ReorderStickerSetsAsync(false, order,
                    result => BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        var allStickers = _allStickers as TLAllStickers32;
                        if (allStickers != null)
                        {
                            allStickers.Hash = new TLString(newHash.ToString(CultureInfo.InvariantCulture));
                            allStickers.Sets = new TLVector<TLStickerSetBase>(Items);
                            var sets = new Dictionary<long, TLVector<TLDocumentBase>>();
                            for (var i = 0; i < allStickers.Documents.Count; i++)
                            {
                                var document22 = allStickers.Documents[i] as TLDocument22;
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
                            for (var i = 0; i < allStickers.Sets.Count; i++)
                            {
                                TLVector<TLDocumentBase> stickers;
                                if (sets.TryGetValue(allStickers.Sets[i].Id.Value, out stickers))
                                {
                                    foreach (var sticker in stickers)
                                    {
                                        documents.Add(sticker);
                                    }
                                }
                            }
                            allStickers.Documents = documents;

                            EmojiControl emojiControl;
                            if (EmojiControl.TryGetInstance(out emojiControl))
                            {
                                emojiControl.ReorderStickerSets();
                            }

                            StateService.SaveAllStickersAsync(allStickers);
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

        private TLAllStickers _allStickers;

        private TLFeaturedStickers _featuredStickers;

        private TLArchivedStickers _archivedStickers;

        private List<TLStickerSetBase> _delayedItems;

        private void UpdateAllStickersAsync(TLAllStickers cachedStickers)
        {
            var hash = cachedStickers != null ? cachedStickers.Hash : TLString.Empty;

            IsWorking = true;
            MTProtoService.GetAllStickersAsync(hash,
                result => BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage(result.ToString());

                    Status = string.Empty;
                    IsWorking = false;

                    var allStickers = result as TLAllStickers43;
                    if (allStickers != null)
                    {
                        Items.Clear();

                        var cachedStickers29 = cachedStickers as TLAllStickers29;
                        if (cachedStickers29 != null)
                        {
                            allStickers.ShowStickersTab = cachedStickers29.ShowStickersTab;
                            allStickers.RecentlyUsed = cachedStickers29.RecentlyUsed;
                            allStickers.Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
                        }
                        var cachedStickers43 = cachedStickers as TLAllStickers43;
                        if (cachedStickers43 != null)
                        {
                            allStickers.RecentStickers = cachedStickers43.RecentStickers;
                            allStickers.FavedStickers = cachedStickers43.FavedStickers;
                        }

                        cachedStickers = allStickers;
                        StateService.SaveAllStickersAsync(cachedStickers);

                        UpdateSets(allStickers, () => { });
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    Status = string.Empty;
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.getAllStickers error " + error);
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
                    shellViewModel.RemoveStickerSet(set, inputSet);

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

                    var allStickers43 = StateService.GetAllStickers() as TLAllStickers43;
                    TLMessagesStickerSet messagesSet = null;
                    if (allStickers43 != null)
                    {
                        messagesSet = TLUtils.RemoveStickerSet(allStickers43, set32);
                        StateService.SaveAllStickersAsync(allStickers43);
                    }

                    if (messagesSet != null)
                    {
                        var archivedStickers = StateService.GetArchivedStickers();
                        if (archivedStickers != null)
                        {
                            TLUtils.AddStickerSetCovered(archivedStickers, messagesSet, archivedStickers.SetsCovered, new TLStickerSetCovered{ Cover = messagesSet.Documents.FirstOrDefault() ?? new TLDocumentEmpty { Id = new TLLong(0) }, StickerSet = messagesSet.Set });
                            archivedStickers.Count.Value = archivedStickers.Sets.Count;

                            UpdateArchivedSetsString(archivedStickers.Count.Value);

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

        private void UpdateSets(TLAllStickers29 allStickers, System.Action callback)
        {
            _allStickers = allStickers;

            _emoticons.Clear();
            _stickerSets.Clear();

            for (var i = 0; i < allStickers.Documents.Count; i++)
            {
                var document22 = allStickers.Documents[i] as TLDocument22;
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
            _delayedItems = new List<TLStickerSetBase>();
            for (var i = 0; i < allStickers.Sets.Count; i++)
            {
                var set = allStickers.Sets[i];
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
                            _delayedItems.Add(set);
                        }
                    }
                }
            }

            AddItemsChunk(25, _delayedItems, callback);
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

        public void ShowStickersByEmoji()
        {
            var content = new StackPanel();

            var allStickers = _allStickers as TLAllStickers43;

            var allSets = new RadioButton { Content = AppResources.AllSets, IsChecked = allStickers != null && allStickers.ShowStickersByEmoji == Telegram.Api.TL.ShowStickersByEmoji.AllSets };
            var mySets = new RadioButton { Content = AppResources.MySets, IsChecked = allStickers != null && allStickers.ShowStickersByEmoji == Telegram.Api.TL.ShowStickersByEmoji.MySets };
            var none = new RadioButton { Content = AppResources.None, IsChecked = allStickers != null && allStickers.ShowStickersByEmoji == Telegram.Api.TL.ShowStickersByEmoji.None };

            content.Children.Add(allSets);
            content.Children.Add(mySets);
            content.Children.Add(none);

            ShellViewModel.ShowCustomMessageBox(string.Empty, AppResources.SuggestStickersByEmoji, AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                r =>
                {
                    if (r == CustomMessageBoxResult.RightButton)
                    {
                        if (allStickers != null)
                        {
                            if (allSets.IsChecked == true)
                            {
                                allStickers.ShowStickersByEmoji = Telegram.Api.TL.ShowStickersByEmoji.AllSets;
                            }
                            else if (mySets.IsChecked == true)
                            {
                                allStickers.ShowStickersByEmoji = Telegram.Api.TL.ShowStickersByEmoji.MySets;
                            }
                            else
                            {
                                allStickers.ShowStickersByEmoji = Telegram.Api.TL.ShowStickersByEmoji.None;
                            }

                            UpdateShowStickersByEmojiSubtitle(allStickers.ShowStickersByEmoji);

                            StateService.SaveAllStickersAsync(allStickers);
                        }
                    }
                },
                content);
        }

        public void OpenFeaturedStickers()
        {
            NavigationService.UriFor<FeaturedStickersViewModel>().Navigate();
        }

        public void OpenMasks()
        {
            NavigationService.UriFor<MasksViewModel>().Navigate();
        }

        public void OpenArchivedStickers()
        {
            NavigationService.UriFor<ArchivedStickersViewModel>().Navigate();
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

                    var allStickers = StateService.GetAllStickers();
                    if (allStickers != null)
                    {
                        var allStickers29 = allStickers as TLAllStickers29;
                        if (allStickers29 != null)
                        {
                            allStickers29.RecentlyUsed = new TLVector<TLRecentlyUsedSticker>();
                            EmojiControl emojiControl;
                            if (EmojiControl.TryGetInstance(out emojiControl))
                            {
                                emojiControl.ClearRecentStickers();
                            }
                            StateService.SaveAllStickersAsync(allStickers29);
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
            if (args.Masks) return;

            // ui thread
            UpdateSets(args.AllStickers, () => { });
        }

        public void Handle(UpdateStickerSetsEventArgs args)
        {
            if (args.Masks) return;

            // ui thread
            UpdateSets(args.AllStickers, () => { });
        }

        public void Handle(UpdateNewStickerSetEventArgs args)
        {
            if (args.Masks) return;

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

        public void Handle(UpdateReadFeaturedStickersEventArgs args)
        {
            // ui thread
            UpdateFeaturedSetsString(0);
        }
    }
}
