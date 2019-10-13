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
using System.Linq;
using System.Windows;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using TelegramClient.Helpers;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Media
{
    public class FilesViewModel<T> : FilesViewModelBase<T> where T : IInputPeer
    {
        public override TLInputMessagesFilterBase InputMessageFilter
        {
            get { return new TLInputMessagesFilterDocument(); }
        }

        public string EmptyListImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/Messages/file.white-WXGA.png";
                }

                return "/Images/Messages/file.black-WXGA.png";
            }
        }

        public FilesViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            DisplayName = LowercaseConverter.Convert(AppResources.SharedFiles);
        }

        protected override void OnInitialize()
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

            base.OnInitialize();
        }

        protected override bool SkipMessage(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message == null)
            {
                return true;
            }

            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument == null)
            {
                return true;
            }

            var document = mediaDocument.Document as TLDocument;
            if (document == null)
            {
                return true;
            }

            if (message.IsSticker()
                || document.FileName.ToString().EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (message.IsGif())
            {
                return true;
            }
            
            if (message.IsMusic())
            {
                return true;
            }

            if (message.IsVoice())
            {
                return true;
            }

            if (message.IsVideo())
            {
                return true;
            }

            return false;
        }
    }

    public abstract class FilesViewModelBase<T> : ItemsViewModelBase<TLMessage>,
        Telegram.Api.Aggregator.IHandle<DownloadableItem>, 
        Telegram.Api.Aggregator.IHandle<DeleteMessagesEventArgs>,
        ISliceLoadable
        where T : IInputPeer
    {
        public abstract TLInputMessagesFilterBase InputMessageFilter { get; }

        private bool _isSelectionEnabled;

        public bool IsSelectionEnabled
        {
            get { return _isSelectionEnabled; }
            set { SetField(ref _isSelectionEnabled, value, () => IsSelectionEnabled); }
        }

        public bool IsGroupActionEnabled
        {
            get { return Items.Any(x => x.IsSelected); }
        }

        public ObservableCollection<TimeKeyGroup<TLMessageBase>> Files { get; set; } 

        public bool IsEmptyList { get; protected set; }

        public T CurrentItem { get; set; }

        private IDocumentFileManager _downloadDocumentFileManager;

        private IDocumentFileManager DownloadDocumentFileManager
        {
            get { return _downloadDocumentFileManager ?? (_downloadDocumentFileManager = IoC.Get<IDocumentFileManager>()); }
        }

        public AnimatedImageViewerViewModel AnimatedImageViewer { get; protected set; }

        public Action<int> SetSelectedCountAction;

        public FilesViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Files = new ObservableCollection<TimeKeyGroup<TLMessageBase>>();

            IsEmptyList = false;
            Items = new ObservableCollection<TLMessage>();

            EventAggregator.Subscribe(this);

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => IsSelectionEnabled))
            {
                if (!IsSelectionEnabled)
                {
                    foreach (var item in Items)
                    {
                        item.IsSelected = false;
                    }
                }
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

                var lastMessage = Items.LastOrDefault() as TLMessageCommon;
                if (lastMessage != null
                    && lastMessage.ToId is TLPeerChat)
                {
                    LoadNextMigratedHistorySlice();
                    return;
                }
            }

            if (IsLastSliceLoaded)
            {
                return;
            }

            var maxId = 0;
            var lastItem = Items.LastOrDefault();
            if (lastItem != null)
            {
                maxId = lastItem.Index;
            }

            IsWorking = true;
            _isLoadingNextSlice = true;
            MTProtoService.SearchAsync(
                CurrentItem.ToInputPeer(),
                TLString.Empty,
                null,
                InputMessageFilter,
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(maxId), 
                new TLInt(Constants.FileSliceLength),
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
                error =>
                {
                    Status = string.Empty;
                    IsWorking = false;
                    _isLoadingNextSlice = false;

                    Execute.ShowDebugMessage("messages.search error " + error);
                });
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
            for (var i = 0; i < Items.Count; i++)
            {
                var messageCommon = Items[i] as TLMessageCommon;
                if (messageCommon == null) continue;

                var peerChat = messageCommon.ToId as TLPeerChat;
                if (peerChat == null) continue;

                if (Items[i].Index != 0
                    && Items[i].Index < maxMessageId)
                {
                    maxMessageId = Items[i].Index;
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
                InputMessageFilter,
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(maxMessageId), 
                new TLInt(Constants.FileSliceLength),
                new TLInt(0),
                result => BeginOnUIThread(() =>
                {
                    _isLoadingNextMigratedHistorySlice = false;
                    IsWorking = false;
                    Status = string.Empty;

                    if (result.Messages.Count < Constants.MessagesSlice)
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

        public void ChangeGroupActionStatus()
        {
            var selectedItemsCount = Items.Count(x => x.IsSelected);
            SetSelectedCountAction.SafeInvoke(selectedItemsCount);

            NotifyOfPropertyChange(() => IsGroupActionEnabled);
        }

        public void Manage()
        {
            IsSelectionEnabled = !IsSelectionEnabled;
        }

        protected abstract bool SkipMessage(TLMessageBase message);

        protected void AddMessages(IList<TLMessageBase> messages)
        {
            foreach (var messageBase in messages)
            {
                if (SkipMessage(messageBase))
                {
                    continue;
                }

                var date = TLUtils.ToDateTime(((TLMessage)messageBase).Date);
                var yearMonthKey = new DateTime(date.Year, date.Month, 1);
                var timeKeyGroup = Files.FirstOrDefault(x => x.Key == yearMonthKey);
                if (timeKeyGroup != null)
                {
                    timeKeyGroup.Add(messageBase);
                }
                else
                {
                    Files.Add(new TimeKeyGroup<TLMessageBase>(yearMonthKey) { messageBase });
                }

                Items.Add(messageBase);
            }
        }

        public void DeleteMessage(TLMessageBase message)
        {
            if (message == null) return;

            var messages = new List<TLMessageBase> { message };

            var owner = CurrentItem as TLObject;

            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                var messageCommon = message as TLMessageCommon;
                if (messageCommon != null)
                {
                    if (messageCommon.ToId is TLPeerChat)
                    {
                        DialogDetailsViewModel.DeleteMessages(MTProtoService, false, null, null, messages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
                        return;
                    }
                }

                DialogDetailsViewModel.DeleteChannelMessages(MTProtoService, channel, null, null, messages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
                return;
            }

            if (CurrentItem is TLBroadcastChat)
            {
                DeleteMessagesInternal(owner, null, messages);
                return;
            }

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
                    for (var j = 0; j < Files.Count; j++)
                    {
                        for (var k = 0; k < Files[j].Count; k++)
                        {
                            if (Files[j][k].Index == messages[i].Index)
                            {
                                Files[j].RemoveAt(k);
                                break;
                            }
                        }
                    }
                    messages[i].IsSelected = false;
                    Items.Remove(messages[i]);
                }
            });

            EventAggregator.Publish(new DeleteMessagesEventArgs { Owner = owner, Messages = messages });
        }

        public void DeleteMessages()
        {
            if (Items == null) return;

            var messages = new List<TLMessageBase>();
            foreach (var item in Items.Where(x => x.IsSelected))
            {
                messages.Add(item);
            }

            if (messages.Count == 0) return;

            var randomItems = new List<TLMessageBase>();
            var items = new List<TLMessageBase>();

            TLMessageBase lastItem = null;
            for (var i = 0; i < Items.Count; i++)
            {
                var message = Items[i];
                if (message.IsSelected)
                {
                    if (message.Index == 0 && message.RandomIndex != 0)
                    {
                        randomItems.Add(message);
                        lastItem = null;
                    }
                    else if (message.Index != 0)
                    {
                        items.Add(message);
                        lastItem = null;
                    }
                }
                else
                {
                    if (lastItem == null)
                    {
                        lastItem = message;
                    }
                }
            }

            if (randomItems.Count == 0 && items.Count == 0)
            {
                return;
            }

            IsSelectionEnabled = false;

            var owner = CurrentItem as TLObject;

            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                var chatMessages = new List<TLMessageBase>();
                var channelMessages = new List<TLMessageBase>();
                if (channel.MigratedFromChatId != null)
                {
                    foreach (var item in items)
                    {
                        var message = item as TLMessageCommon;
                        if (message != null && message.ToId is TLPeerChat)
                        {
                            chatMessages.Add(message);
                        }
                        else
                        {
                            channelMessages.Add(message);
                        }
                    }
                }
                if (chatMessages.Count > 0)
                {
                    DialogDetailsViewModel.DeleteChannelMessages(MTProtoService, channel, lastItem, null, channelMessages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
                    DialogDetailsViewModel.DeleteMessages(MTProtoService, false, lastItem, null, chatMessages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));

                    return;
                }

                DialogDetailsViewModel.DeleteChannelMessages(MTProtoService, channel, lastItem, randomItems, items, (result1, result2) => DeleteMessagesInternal(owner, result1, result2), (result1, result2) => DeleteMessagesInternal(owner, result1, result2));

                return;
            }

            if (CurrentItem is TLBroadcastChat)
            {
                DeleteMessagesInternal(owner, null, randomItems);
                DeleteMessagesInternal(owner, null, items);

                return;
            }

            DialogDetailsViewModel.DeleteMessages(MTProtoService, false, null, randomItems, items, (result1, result2) => DeleteMessagesInternal(owner, result1, result2), (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
        }

        public void ForwardMessages()
        {
            if (Items == null) return;

            var messages = new List<TLMessageBase>();
            foreach (var item in Items.Where(x => x.IsSelected))
            {
                messages.Add(item);
            }

            if (messages.Count == 0) return;

            IsSelectionEnabled = false;

            DialogDetailsViewModel.ForwardMessagesCommon(messages, StateService, NavigationService);
        }

        public void ForwardMessage(TLMessageBase message)
        {
            if (message == null) return;

            DialogDetailsViewModel.ForwardMessagesCommon(new List<TLMessageBase>{ message }, StateService, NavigationService);
        }

        public void SaveMedia(TLMessage message)
        {
            if (message == null) return;

#if WP81
            DialogDetailsViewModel.SaveMediaCommon(message);
#endif
        }

#if WP8
        public async void OpenMedia(TLMessage message)
#else
        public void OpenMedia(TLMessage message)
#endif
        {
            if (message == null) return;

            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                DialogDetailsViewModel.OpenDocumentCommon(message, StateService, DownloadDocumentFileManager, () => { });
            }
        }

        public void Search()
        {
            StateService.CurrentInputPeer = CurrentItem;
            var source = new List<TLMessageBase>(Items.Count);
            foreach (var item in Items)
            {
                source.Add(item);
            }

            StateService.Source = source;
            NavigationService.UriFor<SearchFilesViewModel>().Navigate();
        }

        public void CancelDocumentDownloading(TLMessageMediaDocument mediaDocument)
        {
            BeginOnThreadPool(() =>
            {
                BeginOnUIThread(() =>
                {
                    var message = Items.FirstOrDefault(x => x.Media == mediaDocument);

                    DownloadDocumentFileManager.CancelDownloadFileAsync(message);

                    mediaDocument.IsCanceled = true;
                    mediaDocument.LastProgress = mediaDocument.DownloadingProgress;
                    mediaDocument.DownloadingProgress = 0.0;
                });
            });
        }

        public void Handle(DownloadableItem item)
        {
            var document = item.Owner as TLDocument;
            if (document != null)
            {
                BeginOnUIThread(() =>
                {
                    var messages = Items;
                    foreach (var m in messages)
                    {
                        var media = m.Media as TLMessageMediaDocument;
                        if (media != null && TLDocumentBase.DocumentEquals(media.Document, document))
                        {
                            media.NotifyOfPropertyChange(() => media.Document);
                            break;
                        }
                    }
                });
            }

            var message = item.Owner as TLMessage;
            if (message != null)
            {
                var mediaDocument1 = message.Media as TLMessageMediaDocument;
                if (mediaDocument1 == null) return;

                BeginOnUIThread(() =>
                {
                    foreach (var m in Items)
                    {
                        var mediaDocument2 = m.Media as TLMessageMediaDocument;
                        if (mediaDocument2 != null && TLDocumentBase.DocumentEquals(mediaDocument1.Document, mediaDocument2.Document))
                        {
                            m.Media.IsCanceled = false;
                            m.Media.LastProgress = 0.0;
                            m.Media.DownloadingProgress = 0.0;
                            m.Media.NotifyOfPropertyChange(() => m.Media.Self); // update download icon for documents
                            m.NotifyOfPropertyChange(() => m.Self);
                            m.Media.IsoFileName = item.IsoFileName;
                        }
                    }
                });
            }
        }

        public void Handle(DeleteMessagesEventArgs args)
        {
            var owner = CurrentItem as TLObject;
            if (owner == null) return;

            if (owner == args.Owner)
            {
                BeginOnUIThread(() =>
                {
                    for (var j = 0; j < args.Messages.Count; j++)
                    {
                        for (var i = 0; i < Items.Count; i++)
                        {
                            if (Items[i].Index == args.Messages[j].Index)
                            {
                                Items.RemoveAt(i);
                                break;
                            }
                        }
                    }
                });
            }
        }
    }

    public class TimeKeyGroup<T> : ObservableCollection<T>
    {
        public DateTime Key { get; private set; }

        public string KeyString { get { return Key.ToString("MMMM yyyy"); } }

        public TimeKeyGroup(DateTime key)
        {
            Key = key;
        }
    }

    public interface ISliceLoadable
    {
        void LoadNextSlice();
    }

    public class DeleteMessagesEventArgs
    {
        public TLObject Owner { get; set; }

        public IList<TLMessageBase> Messages { get; set; }
    }
}
