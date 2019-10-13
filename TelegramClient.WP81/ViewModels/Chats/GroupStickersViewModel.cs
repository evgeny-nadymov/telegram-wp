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
using System.Net;
using Caliburn.Micro;
using Coding4Fun.Toolkit.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.Views;
using TelegramClient.Views.Chats;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Chats
{
    public class TLStickerSetNotFound : TLStickerSetBase
    {
        
    }

    public class GroupStickersViewModel : ItemsViewModelBase<TLStickerSetBase>
    {
        private string _text;

        public string Text
        {
            get { return _text; }
            set { SetField(ref _text, value, () => Text); }
        }

        private TLStickerSetBase _stickerSet;

        public TLStickerSetBase StickerSet
        {
            get { return _stickerSet; }
            set { SetField(ref _stickerSet, value, () => StickerSet); }
        }

        private TLChatBase _channel;

        private readonly Dictionary<string, TLVector<TLStickerItem>> _stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();

        private readonly Dictionary<long, string> _emoticons = new Dictionary<long, string>();

        private TLAllStickers _allStickers;

        public GroupStickersViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _channel = StateService.CurrentChat;
            var channel68 = _channel as TLChannel68;
            if (channel68 != null)
            {
                _stickerSet = channel68.StickerSet;
                _text = _stickerSet != null && !TLString.IsNullOrEmpty(_stickerSet.ShortName)
                    ? _stickerSet.ShortName.ToString()
                    : null;

                var stickerSet32 = StickerSet as TLStickerSet32;
                if (stickerSet32 != null && stickerSet32.Stickers == null)
                {
                    MTProtoService.GetStickerSetAsync(new TLInputStickerSetShortName{ ShortName = stickerSet32.ShortName},
                        result => Execute.BeginOnUIThread(() =>
                        {
                            var objects = new TLVector<TLObject>();
                            foreach (var sticker in result.Documents)
                            {
                                objects.Add(new TLStickerItem { Document = sticker });
                            }

                            stickerSet32.Stickers = objects;
                            stickerSet32.NotifyOfPropertyChange(() => stickerSet32.Stickers);
                        }),
                        error =>
                        {
                            
                        });
                }
            }
            StateService.CurrentChat = null;

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Text))
                {
                    if (string.IsNullOrEmpty(Text))
                    {
                        Remove(false);
                    }
                    else if (StickerSet is TLStickerSet32 && string.Equals(StickerSet.ShortName.ToString(), Text))
                    {
                        
                    }
                    else if (Text.Contains("/addstickers"))
                    {
                        var shortNameStartIndex = Text.TrimEnd('/').LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                        if (shortNameStartIndex != -1)
                        {
                            var shortName = Text.Substring(shortNameStartIndex).Replace("/", string.Empty);

                            if (!string.IsNullOrEmpty(shortName))
                            {
                                Text = shortName;
                                var view = GetView() as GroupStickersView;
                                if (view != null)
                                {
                                    view.MoveCursorToEnd();
                                }
                            }
                        }
                    }
                    else
                    {
                        var text = Text;
                        Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
                        {
                            if (string.Equals(text, Text))
                            {
                                SearchStickerSet(text);
                            }
                        });
                    }
                }
            };
        }

        public void Open()
        {
            var stickerSet = StickerSet as TLStickerSet32;
            if (stickerSet == null) return;

            TelegramViewBase.NavigateToStickers(MTProtoService, StateService, new TLInputStickerSetShortName{ ShortName = stickerSet.ShortName },
                () =>
                {
                    Items.Insert(0, stickerSet);
                },
                () =>
                {
                    for (var index = 0; index < Items.Count; index++)
                    {
                        var item = Items[index];
                        if (TLString.Equals(stickerSet.ShortName, item.ShortName, StringComparison.Ordinal))
                        {
                            Items.RemoveAt(index);
                            break;
                        }
                    }
                });
        }

        private void SearchStickerSet(string text)
        {
            IsWorking = true;
            MTProtoService.GetStickerSetAsync(new TLInputStickerSetShortName{ ShortName = new TLString(text)},
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var objects = new TLVector<TLObject>();
                    foreach (var sticker in result.Documents)
                    {
                        objects.Add(new TLStickerItem { Document = sticker });
                    }

                    result.Set.Stickers = objects;

                    Set(result.Set, false);
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Set(new TLStickerSetNotFound(), false);
                }));
        }

        public void Set(TLStickerSetBase set)
        {
            Set(set, true);
        }

        private void Set(TLStickerSetBase set, bool updateText)
        {
            StickerSet = set;
            set.IsSelected = true;
            set.NotifyOfPropertyChange(() => set.IsSelected);
            foreach (var item in Items)
            {
                if (TLString.Equals(StickerSet.ShortName, item.ShortName, StringComparison.OrdinalIgnoreCase))
                {
                    item.IsSelected = true;
                    item.NotifyOfPropertyChange(() => item.IsSelected);
                }
                else
                {
                    item.IsSelected = false;
                    item.NotifyOfPropertyChange(() => item.IsSelected);
                }
            }
            if (updateText) Text = set.ShortName.ToString();
        }

        public void Remove()
        {
            Remove(true);
        }

        private void Remove(bool clearText)
        {
            StickerSet = null;
            foreach (var item in Items)
            {
                if (StickerSet != item)
                {
                    item.IsSelected = false;
                    item.NotifyOfPropertyChange(() => item.IsSelected);
                }
            }
            if (clearText) Text = string.Empty;
        }

        public void Done()
        {
            var channel68 = _channel as TLChannel68;
            if (channel68 != null)
            {
                if (channel68.StickerSet != StickerSet)
                {
                    TLInputStickerSetBase inputStickerSet;
                    if (!(StickerSet is TLStickerSet32))
                    {
                        inputStickerSet = new TLInputStickerSetEmpty();
                    }
                    else
                    {
                        inputStickerSet = new TLInputStickerSetShortName{ ShortName = StickerSet.ShortName };
                    }
                    IsWorking = true;
                    MTProtoService.SetStickersAsync(channel68.ToInputChannel(), inputStickerSet,
                        result => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            channel68.StickerSet = StickerSet;
                            channel68.NotifyOfPropertyChange(() => channel68.StickerSet);
                            NavigationService.GoBack();
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                        }));
                }
            }
        }

        public void ForwardInAnimationComplete()
        {
            StateService.GetAllStickersAsync(cachedAllStickers => BeginOnUIThread(() =>
            {
                _allStickers = cachedAllStickers;

                var allStickers = _allStickers as TLAllStickers43;
                if (allStickers != null)
                {
                    UpdateSets(allStickers, () => UpdateAllStickersAsync(allStickers));
                    Status = string.Empty;
                }
                else
                {
                    UpdateAllStickersAsync(null);
                }
            }));
        }

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
            var delayedItems = new List<TLStickerSetBase>();
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

                    set.IsSelected = StickerSet != null && TLString.Equals(StickerSet.ShortName, set.ShortName, StringComparison.OrdinalIgnoreCase);
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
                    var stickerSet = delayedItems[i] as TLStickerSet;
                    if (stickerSet != null)
                    {
                        stickerSet.IsSelected = StickerSet != null && TLString.Equals(StickerSet.ShortName, stickerSet.ShortName, StringComparison.OrdinalIgnoreCase);
                    }

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
    }
}
