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
using System.Linq;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Media
{
    public class MediaViewModel<T> : ItemsViewModelBase<MessagesRow>, Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        ISliceLoadable
        where T : IInputPeer
    {
        public ObservableCollection<TimeKeyGroup<MessagesRow>> Media { get; set; } 

        public bool IsEmptyList { get; protected set; }

        public string EmptyListImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/Messages/media.white-WXGA.png";
                }

                return "/Images/Messages/media.black-WXGA.png";
            }
        }

        public T CurrentItem { get; set; }

        private readonly IFileManager _downloadFileManager;

        public ImageViewerViewModel ImageViewer { get; set; }

        private readonly IList<TLMessage> _items = new List<TLMessage>();

        public MediaViewModel(IFileManager downloadFileManager, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            IsEmptyList = false;
            Items = new ObservableCollection<MessagesRow>();
            Media = new ObservableCollection<TimeKeyGroup<MessagesRow>>();
            Status = AppResources.Loading;

            _downloadFileManager = downloadFileManager;

            DisplayName = LowercaseConverter.Convert(AppResources.Media);
            EventAggregator.Subscribe(this);
        }

        public void ForwardInAnimationComplete()
        {
            Status = string.Empty;  //AppResources.Loading;
            var limit = 25;
            var messages = CacheService.GetHistory(TLUtils.InputPeerToPeer(CurrentItem.ToInputPeer(), StateService.CurrentUserId), limit);

            BeginOnUIThread(() =>
            {
                Items.Clear();

                AddMessages(messages);

                var channel = CurrentItem as TLChannel;
                if (channel != null && channel.MigratedFromChatId != null)
                {
                    var lastMessage = messages != null ? messages.LastOrDefault() : null;
                    if (lastMessage != null && lastMessage.Index == 1)
                    {
                        IsLastSliceLoaded = true;
                        var chatMessages = CacheService.GetHistory(new TLPeerChat { Id = channel.MigratedFromChatId }, limit);

                        AddMessages(chatMessages);
                    }
                }

                Status = Items.Count > 0 ? string.Empty : Status;

                LoadNextSlice();
            });
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        private void AddMessages(IList<TLMessageBase> messages)
        {
            var isNewRow = false;
            var row = Items.LastOrDefault();
            if (row == null || row.IsFull())
            {
                row = new MessagesRow();
                isNewRow = true;
            }

            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i] as TLMessage;
                if (message == null) continue;
                if (message.HasTTL()) continue;

                if (message.Media is TLMessageMediaPhoto
                    || message.IsVideo())
                {
                    _items.Add(message);
                    if (!row.AddMessage(message))
                    {
                        if (isNewRow)
                        {
                            AddToTimeKeyCollection(row);
                            Items.Add(row);
                        }

                        row = new MessagesRow();
                        isNewRow = true;
                        row.AddMessage(message);
                    }
                }
            }

            if (isNewRow && !row.IsEmpty())
            {
                AddToTimeKeyCollection(row);
                Items.Add(row);
            }
        }

        private void AddToTimeKeyCollection(MessagesRow row)
        {
            var date = TLUtils.ToDateTime(row.Message1.Date);
            var yearMonthKey = new DateTime(date.Year, date.Month, 1);
            var timeKeyGroup = Media.FirstOrDefault(x => x.Key == yearMonthKey);
            if (timeKeyGroup != null)
            {
                timeKeyGroup.Add(row);
            }
            else
            {
                Media.Add(new TimeKeyGroup<MessagesRow>(yearMonthKey) {row});
            }
        }

        private bool _isLoadingNextSlice;

        public void LoadNextSlice()
        {
            if (_isLoadingNextSlice) return;

            if (CurrentItem is TLBroadcastChat && !(CurrentItem is TLChannel))
            {
                Status = string.Empty;
                if (Items.Count == 0)
                {
                    IsEmptyList = true;
                    NotifyOfPropertyChange(() => IsEmptyList);
                }
                return;
            }

            var channel = CurrentItem as TLChannel;
            if (channel != null && channel.MigratedFromChatId != null)
            {
                if (IsLastSliceLoaded)
                {
                    LoadNextMigratedHistorySlice();
                    return;
                }

                var lastMessage = _items.LastOrDefault() as TLMessageCommon;
                if (lastMessage != null
                    && lastMessage.ToId is TLPeerChat)
                {
                    LoadNextMigratedHistorySlice();
                    return;
                }
            }

            var maxId = 0;
            var lastItem = _items.LastOrDefault();
            if (lastItem != null)
            {
                maxId = lastItem.Index;
            }

            if (IsLastSliceLoaded) return;

            IsWorking = true;
            _isLoadingNextSlice = true;
            MTProtoService.SearchAsync(
                CurrentItem.ToInputPeer(),
                TLString.Empty,
                null,
                new TLInputMessagesFilterPhotoVideo(),
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(maxId), 
                new TLInt(Constants.PhotoVideoSliceLength),
                new TLInt(0),
                messages => BeginOnUIThread(() =>
                {
                    Status = string.Empty;
                    IsWorking = false;
                    _isLoadingNextSlice = false;

                    AddMessages(messages.Messages.ToList());

                    if (messages.Messages.Count < Constants.PhotoVideoSliceLength)
                    {
                        IsLastSliceLoaded = true;
                        LoadNextMigratedHistorySlice();
                    }
                    IsEmptyList = Items.Count == 0;
                    NotifyOfPropertyChange(() => IsEmptyList);
                }),
                error => BeginOnUIThread(() =>
                {
                    Status = string.Empty;
                    IsWorking = false;
                    _isLoadingNextSlice = false;

                    Execute.ShowDebugMessage("messages.search error " + error);
                }));
        }

        private bool _isLastMigratedHistorySliceLoaded;

        private bool _isLoadingNextMigratedHistorySlice;

        private void LoadNextMigratedHistorySlice()
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || channel.MigratedFromChatId == null) return;

            if (_isLastMigratedHistorySliceLoaded) return;

            if (_isLoadingNextMigratedHistorySlice) return;

            var maxMessageId = int.MaxValue;
            for (var i = 0; i < _items.Count; i++)
            {
                var messageCommon = _items[i] as TLMessageCommon;
                if (messageCommon == null) continue;

                var peerChat = messageCommon.ToId as TLPeerChat;
                if (peerChat == null) continue;

                if (_items[i].Index != 0
                    && _items[i].Index < maxMessageId)
                {
                    maxMessageId = _items[i].Index;
                }
            }

            if (maxMessageId == int.MaxValue)
            {
                maxMessageId = channel.MigratedFromMaxId != null ? channel.MigratedFromMaxId.Value : 0;
            }

            _isLoadingNextMigratedHistorySlice = true;
            IsWorking = true;
            MTProtoService.SearchAsync(
                new TLInputPeerChat { ChatId = channel.MigratedFromChatId },
                TLString.Empty,
                null,
                new TLInputMessagesFilterPhotoVideo(),
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(maxMessageId), 
                new TLInt(Constants.PhotoVideoSliceLength),
                new TLInt(0),
                result => BeginOnUIThread(() =>
                {
                    _isLoadingNextMigratedHistorySlice = false;
                    IsWorking = false;
                    Status = string.Empty;

                    if (result.Messages.Count < Constants.PhotoVideoSliceLength)
                    {
                        _isLastMigratedHistorySliceLoaded = true;
                    }

                    AddMessages(result.Messages);

                    IsEmptyList = Items.Count == 0;
                    NotifyOfPropertyChange(() => IsEmptyList);
                }),
                error => BeginOnUIThread(() =>
                {
                    _isLoadingNextMigratedHistorySlice = false;
                    IsWorking = false;
                    Status = string.Empty;

                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        public void OpenMedia(TLMessage message)
        {
            if (message == null) return;

            StateService.CurrentMediaMessages = _items;
            StateService.CurrentPhotoMessage = message;

            if (ImageViewer != null)
            {
                ImageViewer.OpenViewer();
            }
        }

        public void Handle(DownloadableItem item)
        {
            var photo = item.Owner as TLPhoto;
            if (photo != null)
            {
                var isUpdated = false;
                var messages = _items;
                foreach (var m in messages)
                {
                    var media = m.Media as TLMessageMediaPhoto;
                    if (media != null && media.Photo == photo)
                    {
                        media.NotifyOfPropertyChange(() => media.Photo);
                        media.NotifyOfPropertyChange(() => media.Self);
                        isUpdated = true;
                        break;
                    }
                }

                if (isUpdated) return;

                var serviceMessages = Items.OfType<TLMessageService>();
                foreach (var serviceMessage in serviceMessages)
                {
                    var editPhoto = serviceMessage.Action as TLMessageActionChatEditPhoto;
                    if (editPhoto != null && editPhoto.Photo == photo)
                    {
                        editPhoto.NotifyOfPropertyChange(() => editPhoto.Photo);
                        isUpdated = true;
                        break;
                    }
                }
            }

            var document = item.Owner as TLDocument22;
            if (document != null)
            {
                var messages = _items;
                foreach (var m in messages)
                {
                    var media = m.Media as TLMessageMediaDocument45;
                    if (media != null && media.Video == document)
                    {
                        media.NotifyOfPropertyChange(() => media.Video);
                        break;
                    }
                }
            }

            var video = item.Owner as TLVideo;
            if (video != null)
            {
                var messages = _items;
                foreach (var m in messages)
                {
                    var media = m.Media as TLMessageMediaVideo;
                    if (media != null && media.Video == video)
                    {
                        media.NotifyOfPropertyChange(() => media.Video);
                        break;
                    }
                }
            }

            var message = item.Owner as TLMessage;
            if (message != null)
            {
                var messages = _items;
                foreach (var m in messages)
                {
                    var mediaDocument = m.Media as TLMessageMediaDocument45;
                    if (mediaDocument != null && m == item.Owner)
                    {
                        m.Media.LastProgress = 0.0;
                        m.Media.DownloadingProgress = 0.0;
                        m.Media.IsoFileName = item.IsoFileName;
                        break;
                    }

                    var media = m.Media as TLMessageMediaVideo;
                    if (media != null && m == item.Owner)
                    {
                        m.Media.LastProgress = 0.0;
                        m.Media.DownloadingProgress = 0.0;
                        m.Media.IsoFileName = item.IsoFileName;
                        break;
                    }
                }
                return;
            }
        }

        public void CancelDownloading(TLPhotoBase photo)
        {
            _downloadFileManager.CancelDownloadFile(photo);
        }

        public void CancelDownloading()
        {
            BeginOnThreadPool(() =>
            {
                foreach (var item in _items)
                {
                    var mediaPhoto = item.Media as TLMessageMediaPhoto;
                    if (mediaPhoto != null)
                    {
                        CancelDownloading(mediaPhoto.Photo);
                    }
                }
            });
        }

        public void CancelDownloading(MessagesRow messageRow)
        {
        }
    }

    public class MessagesRow : TelegramPropertyChangedBase
    {
        public TLMessage Message1 { get; set; }

        public TLMessage Message2 { get; set; }

        public TLMessage Message3 { get; set; }

        public bool AddMessage(TLMessage message)
        {
            if (Message1 == null)
            {
                Message1 = message;
                NotifyOfPropertyChange(() => Message1);
                return true;
            }

            var date1 = TLUtils.ToDateTime(Message1.Date);
            var date = TLUtils.ToDateTime(message.Date);
            if (date1.Year != date.Year || date1.Month != date.Month)
            {
                return false;
            }

            if (Message2 == null)
            {
                Message2 = message;
                NotifyOfPropertyChange(() => Message2);
                return true;
            }

            if (Message3 == null)
            {
                Message3 = message;
                NotifyOfPropertyChange(() => Message3);
                return true;
            }

            return false;
        }

        public bool IsEmpty()
        {
            return Message1 == null;
        }

        public bool IsFull()
        {
            return Message3 != null;
        }

        public IEnumerable<TLMessage> Messages
        {
            get
            {
                if (Message1 != null) yield return Message1;
                if (Message2 != null) yield return Message2;
                if (Message3 != null) yield return Message3;
            }
        } 
    }
}
