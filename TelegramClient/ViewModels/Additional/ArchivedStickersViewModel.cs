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
    public class ArchivedStickersViewModel : ItemsViewModelBase<TLStickerSetBase>//,
        //Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        //Telegram.Api.Aggregator.IHandle<UpdateStickerSetsOrderEventArgs>,
        //Telegram.Api.Aggregator.IHandle<UpdateStickerSetsEventArgs>,
        //Telegram.Api.Aggregator.IHandle<UpdateNewStickerSetEventArgs>
    {
        public ArchivedStickersViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Status = AppResources.Loading;
        }

        public void ForwardInAnimationComplete()
        {
            StateService.GetArchivedStickersAsync(result => BeginOnUIThread(() =>
            {
                if (result != null)
                {
                    _messagesStickerSets = result.MessagesStickerSets;

                    UpdateSets(result);
                    Status = result.Sets.Count > 0? string.Empty : AppResources.Loading;
                }

                UpdateStickersAsync(result);
            }));
        }

        private void UpdateStickersAsync(TLArchivedStickers cachedStickers)
        {
            IsWorking = true;
            MTProtoService.GetArchivedStickersAsync(true, new TLLong(0), new TLInt(100), 
                result => BeginOnUIThread(() =>
                {
                    Status = result.Sets.Count > 0 ? string.Empty : AppResources.NoSetsHere;
                    IsWorking = false;

                    var archivedStickers = result;
                    if (archivedStickers != null)
                    {
                        _messagesStickerSets = archivedStickers.MessagesStickerSets;

                        Items.Clear();

                        cachedStickers = archivedStickers;
                        StateService.SaveArchivedStickersAsync(cachedStickers);

                        UpdateSets(archivedStickers);
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    Status = string.Empty;
                    IsWorking = false;

                    Execute.ShowDebugMessage("messages.getArchivedStickers error " + error);
                }));
        }

        public void LoadNextSlice()
        {
            //if (IsWorking) return;

            //IsWorking = true;
            //MTProtoService.GetArchivedStickersAsync(new TLLong(0), new TLInt(100),
            //    result => BeginOnUIThread(() =>
            //    {
            //        Status = string.Empty;
            //        IsWorking = false;

            //        var archivedStickers = result;
            //        if (archivedStickers != null)
            //        {
            //            _messagesStickerSets = archivedStickers.MessagesStickerSets;

            //            Items.Clear();

            //            cachedStickers = archivedStickers;
            //            StateService.SaveArchivedStickersAsync(cachedStickers);

            //            UpdateSets(archivedStickers);
            //        }
            //    }),
            //    error => BeginOnUIThread(() =>
            //    {
            //        Status = string.Empty;
            //        IsWorking = false;
            //        Execute.ShowDebugMessage("messages.getArchivedStickers error " + error);
            //    }));
        }

        private readonly Dictionary<string, TLVector<TLStickerItem>> _stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();

        private readonly Dictionary<long, string> _emoticons = new Dictionary<long, string>();

        private IStickers _stickers;

        private TLVector<TLMessagesStickerSet> _messagesStickerSets;

        private void UpdateSets(IStickers iStickers)
        {
            _stickers = iStickers;

            _emoticons.Clear();
            _stickerSets.Clear();

            //for (var i = 0; i < iStickers.Packs.Count; i++)
            //{
            //    var emoticon = iStickers.Packs[i].Emoticon.ToString();
            //    foreach (var document in iStickers.Packs[i].Documents)
            //    {
            //        _emoticons[document.Value] = emoticon;
            //    }
            //}

            for (var i = 0; i < iStickers.Documents.Count; i++)
            {
                var document22 = iStickers.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    //string emoticon;
                    //if (_emoticons.TryGetValue(document22.Id.Value, out emoticon))
                    //{
                    //    document22.Emoticon = emoticon;
                    //}

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
            for (var i = 0; i < iStickers.Sets.Count; i++)
            {
                var set = iStickers.Sets[i];

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

            var messagesStickerSet = _messagesStickerSets.FirstOrDefault(x => x.Set.Id.Value == set.Id.Value);
            if (messagesStickerSet == null) return;

            var stickerSetExists = set.Installed && !set.Archived;
            var inputStickerSet = new TLInputStickerSetId{ Id = set.Id, AccessHash = set.AccessHash };
            if (!stickerSetExists)
            {
                MTProtoService.InstallStickerSetAsync(inputStickerSet, TLBool.False,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        Items.Remove(set);

                        var archivedStickers = StateService.GetArchivedStickers();

                        var resultArchive = result as TLStickerSetInstallResultArchive;
                        if (resultArchive != null)
                        {
                            Execute.BeginOnUIThread(() => TelegramViewBase.ShowArchivedStickersMessageBox(resultArchive));

                            var allStickers = StateService.GetAllStickers() as TLAllStickers43;
                            if (allStickers != null)
                            {
                                for (var i = 0; i < resultArchive.Sets.Count; i++)
                                {
                                    TLUtils.RemoveStickerSet(allStickers, resultArchive.Sets[i]);
                                }

                                StateService.SaveAllStickersAsync(allStickers);
                            }

                            if (archivedStickers != null)
                            {
                                for (var i = resultArchive.MessagesStickerSets.Count - 1; i >= 0; i--)
                                {
                                    TLUtils.AddStickerSetCovered(archivedStickers, resultArchive.MessagesStickerSets[i], archivedStickers.SetsCovered, resultArchive.SetsCovered[i]);

                                    for (var j = 0; j < _messagesStickerSets.Count; j++)
                                    {
                                        if (_messagesStickerSets[j].Set.Id.Value == resultArchive.MessagesStickerSets[i].Set.Id.Value)
                                        {
                                            _messagesStickerSets.RemoveAt(j);
                                            break;
                                        }
                                    }

                                    _messagesStickerSets.Insert(0, resultArchive.MessagesStickerSets[i]);
                                }
                            }
                        }
                        
                        if (archivedStickers != null)
                        {
                            TLUtils.RemoveStickerSetCovered(archivedStickers, set, archivedStickers.SetsCovered);

                            StateService.SaveArchivedStickersAsync(archivedStickers);
                        }

                        if (resultArchive != null)
                        {
                            UpdateSets(archivedStickers);
                        }
                        else
                        {
                            set.Installed = true;
                            var set76 = set as TLStickerSet76;
                            if (set76 != null)
                            {
                                set76.InstalledDate = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now);
                            }
                            set.NotifyOfPropertyChange(() => set.Installed);

                            set.Archived = false;
                            set.NotifyOfPropertyChange(() => set.Archived);
                        }

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
                                Execute.ShowDebugMessage("messages.installStickerSet error " + error);
                            }
                        }
                        else
                        {
                            Execute.ShowDebugMessage("messages.installStickerSet error " + error);
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

                        set.Archived = false;
                        set.NotifyOfPropertyChange(() => set.Archived);

                        var shellViewModel = IoC.Get<ShellViewModel>();
                        shellViewModel.RemoveStickerSet(set, inputStickerSet);

                        var eventAggregator = EventAggregator;
                        eventAggregator.Publish(new UpdateRemoveStickerSetEventArgs(set));

                        MTProtoService.SetMessageOnTime(2.0, AppResources.StickersRemoved);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        Execute.ShowDebugMessage("messages.uninstallStickerSet error " + error);
                    }));
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
