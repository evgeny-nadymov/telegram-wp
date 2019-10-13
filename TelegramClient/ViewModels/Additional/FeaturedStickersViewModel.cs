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
using TelegramClient.Views;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class FeaturedStickersViewModel : ItemsViewModelBase<TLStickerSetBase>//,
        //Telegram.Api.Aggregator.IHandle<UpdateReadFeaturedStickersEventArgs>
        //Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        //Telegram.Api.Aggregator.IHandle<UpdateStickerSetsOrderEventArgs>,
        //Telegram.Api.Aggregator.IHandle<UpdateStickerSetsEventArgs>,
        //Telegram.Api.Aggregator.IHandle<UpdateNewStickerSetEventArgs>
    {
        public FeaturedStickersViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Status = AppResources.Loading;
        }

        protected override void OnDeactivate(bool close)
        {
            foreach (var item in Items)
            {
                item.Unread = false;
            }

            base.OnDeactivate(close);
        }

        public void ForwardInAnimationComplete()
        {
            StateService.GetFeaturedStickersAsync(result => BeginOnUIThread(() =>
            {
                if (result != null)
                {
                    UpdateSets(result, result.Unread);
                    Status = result.Sets.Count > 0 ? string.Empty : AppResources.Loading;
                }

                UpdateStickersAsync(result);
            }));
        }

        private void UpdateStickersAsync(TLFeaturedStickers cachedStickers)
        {
            var hash = cachedStickers != null ? cachedStickers.HashValue : new TLInt(0);

            IsWorking = true;
            MTProtoService.GetFeaturedStickersAsync(true, hash,
                result => BeginOnUIThread(() =>
                {
                    Status = string.Empty;
                    IsWorking = false;

                    var featuredStickers = result as TLFeaturedStickers;
                    if (featuredStickers != null)
                    {
                        Status = featuredStickers.Sets.Count > 0 ? string.Empty : AppResources.NoSetsHere;

                        Items.Clear();

                        cachedStickers = featuredStickers;
                        StateService.SaveFeaturedStickersAsync(cachedStickers);

                        UpdateSets(featuredStickers, featuredStickers.Unread);
                    }

                    ReadFeaturedStickersAsync(featuredStickers ?? cachedStickers);
                }),
                error => BeginOnUIThread(() =>
                {
                    Status = string.Empty;
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.getFeaturedStickers error " + error);
                }));
        }

        private void ReadFeaturedStickersAsync(TLFeaturedStickers featuredStickers)
        {
            if (featuredStickers == null) return;
            if (featuredStickers.Unread.Count == 0) return;

            MTProtoService.ReadFeaturedStickersAsync(
                featuredStickers.Unread,
                result2 => BeginOnUIThread(() =>
                {
                    featuredStickers.Unread = new TLVector<TLLong>();

                    StateService.SaveFeaturedStickersAsync(featuredStickers);

                    EventAggregator.Publish(new UpdateReadFeaturedStickersEventArgs(featuredStickers));
                }),
                error =>
                {
                    Execute.ShowDebugMessage("messages.readFeaturedStickers error " + error);
                });
        }

        private readonly Dictionary<string, TLVector<TLStickerItem>> _stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();

        private readonly Dictionary<long, string> _emoticons = new Dictionary<long, string>();

        private IStickers _stickers;

        private void UpdateSets(IStickers featuredStickers, IList<TLLong> unread)
        {
            _stickers = featuredStickers;

            _emoticons.Clear();
            _stickerSets.Clear();

            var unreadDict = new Dictionary<long, long>();
            foreach (var unreadId in unread)
            {
                unreadDict[unreadId.Value] = unreadId.Value;
            }

            for (var i = 0; i < featuredStickers.Documents.Count; i++)
            {
                var document22 = featuredStickers.Documents[i] as TLDocument22;
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
            for (var i = 0; i < featuredStickers.Sets.Count; i++)
            {
                var set = featuredStickers.Sets[i];
                if (unreadDict.ContainsKey(set.Id.Value))
                {
                    set.Unread = true;
                }

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

            BeginOnUIThread(() =>
            {
                foreach (var set in delayedItems)
                {
                    Items.Add(set);
                }
            });
        }

        public void AddRemoveStickerSet(TLStickerSet32 set)
        {
            if (set == null) return;

            var featuredStickers = _stickers as TLFeaturedStickers;
            if (featuredStickers == null) return;

            var messagesStickerSet = featuredStickers.MessagesStickerSets.FirstOrDefault(x => x.Set.Id.Value == set.Id.Value);
            if (messagesStickerSet == null) return;

            var stickerSetExists = set.Installed;
            var inputStickerSet = new TLInputStickerSetId{ Id = set.Id, AccessHash = set.AccessHash };
            if (!stickerSetExists)
            {
                MTProtoService.InstallStickerSetAsync(inputStickerSet, TLBool.False,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        var resultArchive = result as TLStickerSetInstallResultArchive;
                        if (resultArchive != null)
                        {
                            TelegramViewBase.ShowArchivedStickersMessageBox(resultArchive);
                        }

                        set.Installed = true;
                        var set76 = set as TLStickerSet76;
                        if (set76 != null)
                        {
                            set76.InstalledDate = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
                        }
                        set.NotifyOfPropertyChange(() => set.Installed);

                        var shellViewModel = IoC.Get<ShellViewModel>();
                        shellViewModel.Handle(new TLUpdateNewStickerSet { Stickerset = messagesStickerSet });

                        MTProtoService.SetMessageOnTime(2.0, AppResources.NewStickersAdded);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                        {
                            if (error.TypeEquals(ErrorType.STICKERSET_INVALID))
                            {
                                MessageBox.Show(AppResources.StickersNotFound, AppResources.Error, MessageBoxButton.OK);
                            }
                            else
                            {
                                Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                            }
                        }
                        else
                        {
                            Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                        }
                    }));
            }
            else
            {
                MTProtoService.UninstallStickerSetAsync(inputStickerSet,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        set.Installed = false;
                        var set76 = set as TLStickerSet76;
                        if (set76 != null)
                        {
                            set76.InstalledDate = null;
                        }
                        set.NotifyOfPropertyChange(() => set.Installed);

                        var shellViewModel = IoC.Get<ShellViewModel>();
                        shellViewModel.RemoveStickerSet(set, inputStickerSet);

                        var eventAggregator = EventAggregator;
                        eventAggregator.Publish(new UpdateRemoveStickerSetEventArgs(set));

                        MTProtoService.SetMessageOnTime(2.0, AppResources.StickersRemoved);
                    }),
                    error =>
                    Execute.BeginOnUIThread(
                    () => { Execute.ShowDebugMessage("messages.uninstallStickerSet error " + error); }));
            }
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
    }
}
