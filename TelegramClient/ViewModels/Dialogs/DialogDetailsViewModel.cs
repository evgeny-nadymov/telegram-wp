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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.EmojiPanel;
using TelegramClient.Views;
using TelegramClient.Views.Dialogs;
#if WP8
using Windows.Storage;
#endif
using Caliburn.Micro;
using Microsoft.Xna.Framework.Media;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.Location;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel : ItemsViewModelBase<TLMessageBase>
    {
        private IDialogDetailsView _view;

        public IDialogDetailsView View
        {
            get
            {
                _view = _view ?? GetView() as IDialogDetailsView;

                return _view;
            }
        }

        public List<object> TestItems { get; set; }

        public Visibility MuteButtonVisibility
        {
            get
            {
                var channel = With as TLChannel44;

                return channel != null && channel.IsBroadcast && (channel.Creator || channel.IsEditor)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public string MuteButtonImageSource
        {
            get
            {
                var channel = With as TLChannel44;

                return channel != null && channel.IsBroadcast && (channel.Creator || channel.IsEditor) && channel.Silent
                    ? "/Images/W10M/ic_unmutechannel_2x.png"
                    : "/Images/W10M/ic_mutechannel_2x.png";
            }
        }

        public bool HasBots
        {
            get
            {
                var user = With as TLUser;
                if (user != null && user.IsBot)
                {
                    return true;
                }

                var channel = With as TLChannel;
                if (channel != null && channel.IsBroadcast)
                {
                    return false;
                }

                var chat = With as TLChatBase;
                if (chat != null && chat.BotInfo != null)
                {
                    return chat.BotInfo.Count > 0;
                }

                return false;
            }
        }

        public bool IsSingleBot
        {
            get
            {
                var user = With as TLUser;
                if (user != null && user.IsBot)
                {
                    return true;
                }

                var chat = With as TLChatBase;
                if (chat != null && chat.BotInfo != null)
                {
                    var channel = chat as TLChannel;
                    if (channel != null && channel.IsMegaGroup)
                    {
                        return false;
                    }

                    return chat.BotInfo.Count == 1;
                }

                return false;
            }
        }

        public bool IsAppBarCommandVisible
        {
            get
            {
                ShellViewModel.WriteTimer("  IsAppBarCommandVisible start");

                var chatBase = With as TLChatBase;
                if (chatBase != null)
                {
                    var channel = With as TLChannel;
                    if (channel != null)// && channel.IsBroadcast)
                    {
                        if (channel.IsMegaGroup)
                        {
                            if (channel.IsPublic && channel.Left.Value)
                            {
                                return true;
                            }

                            return false;
                        }
                        else
                        {
                            if (channel.Left.Value)
                            {
                                return true;
                            }

                            if (!channel.Creator && !channel.IsEditor && !channel.IsModerator)
                            {
                                return true;
                            }
                        }
                    }

                    if (IsChannelForbidden)
                    {
                        return true;
                    }

                    if (IsChatForbidden)
                    {
                        return true;
                    }

                    if (IsChatDeactivated)
                    {
                        return true;
                    }
                }

                var userBase = With as TLUserBase;
                if (userBase != null)
                {
                    if (userBase.Blocked != null && userBase.Blocked.Value)
                    {
                        return true;
                    }

                    var user = With as TLUser;
                    var bot = _bot as TLUser;
                    if (user != null && bot != null && bot.IsBot && !string.IsNullOrEmpty(bot.AccessToken))
                    {
                        return true;
                    }

                    if (user != null && user.IsBot && Items.Count == 0 && LazyItems.Count == 0)
                    {
                        return true;
                    }
                }

                var channels = With as TLVector<TLChatBase>;
                if (channels != null)
                {
                    return true;
                }

                //System.Diagnostics.Debug.WriteLine("  IsAppBarCommandVisible stop elapsed=" + ShellViewModel.Timer.Elapsed);
                return false;
            }
        }

        public string AppBarCommandString
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel IsAppBarCommandString start");
                if (IsChannel)
                {
                    var channel = (TLChannel)With;

                    if (channel.Left.Value && channel.IsPublic)
                    {
                        return AppResources.Join.ToUpperInvariant();
                    }

                    var notifySettings = channel.NotifySettings as TLPeerNotifySettings;
                    if (notifySettings != null)
                    {
                        var muteUntil = notifySettings.MuteUntil != null ? notifySettings.MuteUntil.Value > 0 : !StateService.GetNotifySettings().GroupAlert;
                        return muteUntil ? AppResources.Unmute.ToUpperInvariant() : AppResources.Mute.ToUpperInvariant();
                    }

                    return AppResources.Mute.ToUpperInvariant();
                }

                if (IsChannelForbidden)
                {
                    return AppResources.Delete.ToUpperInvariant();
                }

                if (IsChatForbidden)
                {
                    return AppResources.Delete.ToUpperInvariant();
                }

                if (IsChatDeactivated)
                {
                    return AppResources.Delete.ToUpperInvariant();
                }

                if (IsBotStarting)
                {
                    return AppResources.Start.ToUpperInvariant();
                }

                if (IsUserBlocked)
                {
                    if (IsBot)
                    {
                        return AppResources.Restart.ToUpperInvariant();
                    }

                    return AppResources.UnblockContact.ToUpperInvariant();
                }

                var channels = With as TLVector<TLChatBase>;
                if (channels != null)
                {
                    return AppResources.ShowNext.ToUpperInvariant();
                }

                ShellViewModel.WriteTimer("DialogDetailsViewModel IsAppBarCommandString stop");
                return string.Empty;
            }
        }

        public bool IsGroupActionEnabled
        {
            get { return UngroupEnumerator(Items).Any(x => x.IsSelected); }
        }

        public string ScrollButtonImageSource
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel ScrollButtonImageSource");

                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/ApplicationBar/appbar.next.light.png";
                }

                return "/Images/ApplicationBar/appbar.next.png";
            }
        }

        public Brush ReplyBackgroundBrush
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (!isLightTheme)
                {
                    if (StateService.IsEmptyBackground)
                    {
                        return (Brush)Application.Current.Resources["PhoneChromeBrush"];
                    }
                }
                var color = Colors.Black;
                color.A = 102;
                return new SolidColorBrush(color);
            }
        }

        public Brush WatermarkForeground
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    if (StateService.IsEmptyBackground)
                    {
                        var color = Colors.Black;
                        color.A = 153;
                        return new SolidColorBrush(color);
                    }
                }

                return (Brush)Application.Current.Resources["PhoneContrastForegroundBrush"];
            }
        }

        public TLFavedStickers FavedStickers { get; protected set; }

        public TLAllStickers Stickers { get; protected set; }

        public TLStickerPack GetStickerPack(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            if (Stickers == null) return null;
            if (Stickers.Packs == null) return null;

            for (var i = 0; i < Stickers.Packs.Count; i++)
            {
                if (Stickers.Packs[i].Emoticon != null
                    && Stickers.Packs[i].Emoticon.ToString() == text)
                {
                    return Stickers.Packs[i];
                }
            }

            return null;
        }

        public TLStickerPack GetFeaturedStickerPack(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var featuredStickers = StateService.GetFeaturedStickers();
            if (featuredStickers == null) return null;
            if (featuredStickers.Packs == null) return null;

            for (var i = 0; i < featuredStickers.Packs.Count; i++)
            {
                if (featuredStickers.Packs[i].Emoticon != null
                    && featuredStickers.Packs[i].Emoticon.ToString() == text)
                {
                    return featuredStickers.Packs[i];
                }
            }

            return null;
        }

        public string EmptyDialogImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                var user = With as TLUser;
                if (user != null && user.IsSelf)
                {
                    if (isLightTheme)
                    {
                        return "/Images/Messages/chat.cloudstorage-white-WXGA.png";
                    }

                    return "/Images/Messages/chat.cloudstorage-WXGA.png.png";
                }

                if (isLightTheme)
                {
                    return "/Images/Messages/chat.nomessages-white-WXGA.png";
                }

                return "/Images/Messages/chat.nomessages-WXGA.png";
            }
        }

        private bool _isSelectionEnabled;

        public bool IsSelectionEnabled
        {
            get { return _isSelectionEnabled; }
            set
            {
                SetField(ref _isSelectionEnabled, value, () => IsSelectionEnabled);

                if (!value)
                {
                    foreach (var item in Items)
                    {
                        item.IsSelected = false;
                        var message73 = item as TLMessage73;
                        if (message73 != null)
                        {
                            var mediaGroup = message73.Media as TLMessageMediaGroup;
                            if (mediaGroup != null)
                            {
                                foreach (var m in mediaGroup.Group)
                                {
                                    m.IsSelected = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ChangeSelection(TLMessageBase messageBase)
        {
            if (messageBase == null) return;

            messageBase.IsSelected = !messageBase.IsSelected;

            var message = messageBase as TLMessage73;
            if (message != null && message.GroupedId != null)
            {
                var mediaGroup = message.Media as TLMessageMediaGroup;
                if (mediaGroup != null)
                {
                    foreach (var m in mediaGroup.Group)
                    {
                        m.IsSelected = messageBase.IsSelected;
                    }
                }
            }

            NotifyOfPropertyChange(() => IsGroupActionEnabled);
        }

        private string _text = "";

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    NotifyOfPropertyChange(() => Text);
                    //NotifyOfPropertyChange(() => CanSend);
                }
            }
        }

        private void SaveDraftAsync(TLInputPeerBase peer, TLDraftMessageBase draft)
        {
            CurrentDialog = CurrentDialog ?? CacheService.GetDialog(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));

            var dialog53 = CurrentDialog as TLDialog53;
            if (dialog53 != null)
            {
                if (draft.DraftEquals(dialog53.Draft)) return;
            }

            MTProtoService.SaveDraftAsync(peer, draft,
                result =>
                {
                    if (dialog53 != null)
                    {
                        dialog53.Draft = draft;

                        EventAggregator.Publish(new TLUpdateDraftMessage { Peer = dialog53.Peer, Draft = dialog53.Draft });
                    }

                    Execute.ShowDebugMessage(string.Format("messages.saveDraft {0} result={1}", draft, result));
                },
                error =>
                {
                    Execute.ShowDebugMessage("messages.saveDraft error " + error);
                });
        }

        private void GetDraftAsync(Action<TLDraftMessageBase> callback)
        {
            CurrentDialog = CurrentDialog ?? CacheService.GetDialog(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));

            TLDraftMessageBase draft = null;
            var dialog53 = CurrentDialog as TLDialog53;
            if (dialog53 != null)
            {
                draft = dialog53.Draft;
            }

            callback.SafeInvoke(draft);
        }

        private readonly object _unsendedTextRoot = new object();

        private void SaveUnsendedTextAsync(string text)
        {
            BeginOnThreadPool(() =>
            {
                var inputPeer = With as IInputPeer;
                if (inputPeer == null) return;

                if (IsEditingEnabled) return;

                text = text ?? string.Empty;

                var noWebpage = false;
                TLMessageCommon reply = null;
                var messageCommon = Reply as TLMessageCommon;
                if (messageCommon != null)
                {
                    reply = messageCommon;
                }

                var messagesContainer = Reply as TLMessagesContainter;
                if (messagesContainer != null && messagesContainer.WebPageMedia != null)
                {
                    messageCommon = _previousReply as TLMessageCommon;
                    if (messageCommon != null)
                    {
                        reply = messageCommon;
                    }
                }
                else
                {
                    if (_webPagesCache.ContainsKey(text.Trim()))
                    {
                        noWebpage = true;
                    }
                }

                string processedText;
                var entities = GetEntities(text, out processedText);

                TLDraftMessageBase draft = new TLDraftMessage
                {
                    Flags = new TLInt(0),
                    NoWebpage = noWebpage,
                    ReplyToMsgId = reply != null ? reply.Id : null,
                    Message = new TLString(processedText),
                    Entities = entities != null && entities.Count > 0 ? new TLVector<TLMessageEntityBase>(entities) : null,
                    Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now)
                };
                if (draft.IsEmpty())
                {
                    draft = new TLDraftMessageEmpty82
                    {
                        Flags = new TLInt(0),
                        Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now)
                    };
                }

                SaveDraftAsync(inputPeer.ToInputPeer(), draft);

                FileUtils.Delete(_unsendedTextRoot, inputPeer.GetUnsendedTextFileName());

                //TLUtils.SaveObjectToMTProtoFile(_unsendedTextRoot, inputPeer.GetUnsendedTextFileName(), new TLString(text));
            });
        }

        private void LoadUnsendedTextAsync(Action<TLDraftMessageBase> callback)
        {
            BeginOnThreadPool(() =>
            {
                var inputPeer = With as IInputPeer;
                if (inputPeer == null) return;

                GetDraftAsync(draftBase =>
                {
                    callback.SafeInvoke(draftBase);
                });
            });
        }

        private bool _isEmptyDialog;

        public bool IsEmptyDialog
        {
            get { return _isEmptyDialog; }
            set
            {
                SetField(ref _isEmptyDialog, value, () => IsEmptyDialog);
            }
        }

        private Typing _typing;

        public Typing Typing
        {
            get { return _typing; }
            set { SetField(ref _typing, value, () => Typing); }
        }

        private string _subtitle;

        public string Subtitle
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel Subtitle");
                return _subtitle;
            }
            set { SetField(ref _subtitle, value, () => Subtitle); }
        }

        private TLInputPeerBase _peer;

        public TLInputPeerBase Peer
        {
            get { return _peer; }
            set { SetField(ref _peer, value, () => Peer); }
        }

        public TLObject With { get; set; }

        private IUploadFileManager _uploadFileManager;

        private IUploadFileManager UploadFileManager
        {
            get { return _uploadFileManager ?? (_uploadFileManager = IoC.Get<IUploadFileManager>()); }
        }

        private IUploadDocumentFileManager _uploadDocumentFileManager;

        private IUploadDocumentFileManager UploadDocumentFileManager
        {
            get { return _uploadDocumentFileManager ?? (_uploadDocumentFileManager = IoC.Get<IUploadDocumentFileManager>()); }
        }

        private IUploadVideoFileManager _uploadVideoFileManager;

        public IUploadVideoFileManager UploadVideoFileManager
        {
            get { return _uploadVideoFileManager ?? (_uploadVideoFileManager = IoC.Get<IUploadVideoFileManager>()); }
        }

        private IUploadAudioFileManager _uploadAudioFileManager;

        public IUploadAudioFileManager UploadAudioFileManager
        {
            get { return _uploadAudioFileManager ?? (_uploadAudioFileManager = IoC.Get<IUploadAudioFileManager>()); }
        }

        private IFileManager _downloadFileManager;

        public IFileManager DownloadFileManager
        {
            get { return _downloadFileManager ?? (_downloadFileManager = IoC.Get<IFileManager>()); }
        }

        private IVideoFileManager _downloadVideoFileManager;

        private IVideoFileManager DownloadVideoFileManager
        {
            get { return _downloadVideoFileManager ?? (_downloadVideoFileManager = IoC.Get<IVideoFileManager>()); }
        }

        private IAudioFileManager _downloadAudioFileManager;

        private IAudioFileManager DownloadAudioFileManager
        {
            get { return _downloadAudioFileManager ?? (_downloadAudioFileManager = IoC.Get<IAudioFileManager>()); }
        }

        private IDocumentFileManager _downloadDocumentFileManager;

        private IDocumentFileManager DownloadDocumentFileManager
        {
            get { return _downloadDocumentFileManager ?? (_downloadDocumentFileManager = IoC.Get<IDocumentFileManager>()); }
        }

        public void BackwardInAnimationComplete()
        {
            if (StateService.UpdateSubtitle)
            {
                StateService.UpdateSubtitle = false;
                Subtitle = GetSubtitle();
            }

            if (StateService.ShowScrollDownButton)
            {
                StateService.ShowScrollDownButton = false;
                Execute.BeginOnUIThread(() =>
                {
                    View.ShowScrollToBottomButton();
                });
            }

            BeginOnThreadPool(() =>
            {
                InputTypingManager.Start();

                BeginOnUIThread(() =>
                {
                    NotifyOfPropertyChange(() => With);
                    if (StateService.RemoveBackEntry)
                    {
                        StateService.RemoveBackEntry = false;
                        NavigationService.RemoveBackEntry();
                    }
                });


                SendMedia();
                ReadHistoryAsync();
            });
        }

        private bool _isForwardInAnimationComplete;

        private bool _updateItemsAndReadHistory;

        public void OnLoaded()
        {

        }

        private bool _loadResultHistory;

        public void ForwardInAnimationComplete()
        {
            //var shellViewModel = IoC.Get<ShellViewModel>();
            //if (shellViewModel != null)
            //{
            //    var shellView = shellViewModel.GetView() as ShellView;
            //    if (shellView != null)
            //    {
            //        shellView.NextUri = new Uri("DialogDetailsView.xaml", UriKind.Relative);
            //    }
            //}

            //System.Diagnostics.Debug.WriteLine("ForwardInAnimationComplete");
            if (_loadResultHistory)
            {
                _loadResultHistory = false;

                if (Items.Count == 1)
                {
                    LoadResultHistory(Items[0]);
                    ReadHistoryAsync();
                }
            }

            if (_messageIdSlice != null)
            {
                var messageSlice = _messageIdSlice;

                _messageIdSlice = null;
                _isUpdated = true;
                _messageId = null;
                ContinueLoadResultHistory(messageSlice.Item2, messageSlice.Item1, messageSlice.Item3.Messages);
            }

            _isForwardInAnimationComplete = true;
            System.Diagnostics.Debug.WriteLine("FwdInAnimationComplete _messages=" + (_messages == null ? "null" : _messages.Count.ToString()));
            AddMessagesWithCache(_messages);

            if (_isFirstSliceLoaded)
            {
                if (LazyItems.Count == 0)
                {
                    UpdateReplyMarkup(Items);
                }
            }

            StartSwitchPMBotWithParam();
            SendForwardMessages();
            SendSharedContact();
            try
            {
                SendLogs();
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }

            if (_delayedMessages != null)
            {
                AddMessagesAndReadHistory(_delayedPosition, _delayedMessages);
                _delayedMessages = null;
            }
            _delayedPosition = -1;

            if (ChooseAttachment == null)
            {
                ChooseAttachment = new ChooseAttachmentViewModel(With, OpenInlineBot, SendDocument, SendVideo, SendPhoto, SendLocation, OpenContact, CacheService, EventAggregator, NavigationService, StateService);
                NotifyOfPropertyChange(() => ChooseAttachment);
            }

            var channel = With as TLChannel49;
            if (channel != null && channel.PinnedMsgId != null && channel.PinnedMsgId.Value > 0)
            {
                ShowPinnedMessage(channel);
            }

            ShowPromoNotification();

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            if (StateService.RemoveBackEntries)
            {
                var backEntry = NavigationService.BackStack.FirstOrDefault();
                while (backEntry != null
                    && !backEntry.Source.ToString().EndsWith("ShellView.xaml")
                    && !IsFirstEntryFromPeopleHub(backEntry, NavigationService.BackStack)
                    && !IsFirstEntryFromTelegramUrl(backEntry, NavigationService.BackStack))
                {
                    NavigationService.RemoveBackEntry();
                    backEntry = NavigationService.BackStack.FirstOrDefault();
                }


                StateService.RemoveBackEntries = false;
            }

            if (_updateItemsAndReadHistory)
            {
                _updateItemsAndReadHistory = false;

                //((BindableCollection<TLMessageBase>)Items).AddRange(_secondSlice);

                if (_isFirstSliceLoaded)
                {
                    UpdateReplyMarkup(Items);
                }
                //BeginOnUIThread(TimeSpan.FromSeconds(2.0), () =>
                //{
                UpdateItemsAsync(0, 0, Constants.MessagesSlice, false);
                ReadHistoryAsync();
                //});
            }

            BeginOnThreadPool(() =>
            {
                InputTypingManager.Start();

                //ReadHistoryAsync(); Read on LazyItems Population complete
                GetFullInfo();
            });
        }

        private void ShowPromoNotification()
        {
            var dialog71 = CurrentDialog as TLDialog71;
            if (dialog71 != null && dialog71.IsPromo && !dialog71.PromoNotification)
            {
                dialog71.PromoNotification = true;
                ShellViewModel.ShowCustomMessageBox(
                    AppResources.ProxySponsorAbout,
                    AppResources.AppName,
                    AppResources.Ok, null,
                    dismissed =>
                    {

                    });
            }
        }

        private void AddMessagesAndReadHistory(int startPosition, TLVector<TLMessageBase> cachedMessages)
        {
            //if (startPosition > 1)
            //{
            //    ScrollToBottomVisibility = Visibility.Visible;
            //}

            BeginOnUIThread(() =>
            {
                for (var i = startPosition; i < cachedMessages.Count; i++)
                {
                    var message = cachedMessages[i];
                    Items.Add(message);
                }

                HoldScrollingPosition = true;
                BeginOnUIThread(() =>
                {
                    for (var i = 0; i < startPosition - 1; i++)
                    {
                        Items.Insert(i, cachedMessages[i]);
                    }
                    HoldScrollingPosition = false;
                    BeginOnUIThread(() =>
                    {
                        SuppressHideScrollToBottom = false;
                    });
                });

            });

            ReadHistoryAsync();
        }

        public static bool IsFirstEntryFromTelegramUrl(JournalEntry backEntry, IEnumerable<JournalEntry> backStack)
        {
            if (backEntry.Source.ToString().StartsWith("/Protocol?encodedLaunchUri"))
            {
                if (backStack != null && backStack.Count() == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFirstEntryFromPeopleHub(JournalEntry backEntry, IEnumerable<JournalEntry> backStack)
        {
            if (backEntry.Source.ToString().StartsWith("/PeopleExtension?action"))
            {
                if (backStack != null && backStack.Count() == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private void SendSharedContact()
        {
            if (StateService.SharedContact != null)
            {
                var contact = StateService.SharedContact;
                StateService.SharedContact = null;
                SendContact(contact);
                return;
            }

            if (StateService.SharedContactMedia != null)
            {
                var contactMedia = StateService.SharedContactMedia;
                StateService.SharedContactMedia = null;
                ContinueSendContact(contactMedia);
                return;
            }
        }

        public TLDialogBase CurrentDialog;

        private ImageViewerViewModel _imageViewer;

        public ImageViewerViewModel ImageViewer
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel ImageViewer");
                return _imageViewer;
            }
            protected set { _imageViewer = value; }
        }

        public AnimatedImageViewerViewModel _animatedImageViewer;

        public AnimatedImageViewerViewModel AnimatedImageViewer
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel AnimatedImageViewer");
                return _animatedImageViewer;
            }
            protected set { _animatedImageViewer = value; }
        }

        public ChooseAttachmentViewModel _chooseAttachment;

        public ChooseAttachmentViewModel ChooseAttachment
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel ChooseAttachment");
                return _chooseAttachment;
            }
            protected set { _chooseAttachment = value; }
        }

        public bool IsChooseAttachmentOpen { get { return ChooseAttachment != null && ChooseAttachment.IsOpen; } }

        public void NavigateToShellViewModel()
        {
            ShellViewModel.Navigate(NavigationService);
        }

        private readonly TLUserBase _bot;

        private readonly int _post;

        public ICollectionView FilteredItems { get; set; }

        private IUploadService _uploadService;

        public bool SuppressHideScrollToBottom { get; set; }

        public DialogDetailsViewModel(IUploadService uploadService,
            ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {

            ShellViewModel.WriteTimer("DialogDetailsViewModel start ctor");

            _uploadService = uploadService;

            TestItems = new List<object>();
            _channelScheduler = new Timer(OnChannelSchedulerTick, this, Timeout.Infinite, Timeout.Infinite);

            //Items = new ObservableCollection<TLMessageBase>();
            //Items.CollectionChanged += (o, e) =>
            //{
            //System.Diagnostics.Debug.WriteLine("action={0} newItems={1} oldItems={2}", e.Action, e.NewItems, e.OldItems);
            //};
            //FilteredItems = new CollectionViewSource {Source = Items}.View;
            //FilteredItems.Filter += (item) =>
            //{
            //    var serviceMessage = item as TLMessageService;
            //    return serviceMessage == null || !(serviceMessage.Action is TLMessageActionClearHistory);
            //};

            With = GetParticipant();
            StateService.With = null;
            if (With == null)
            {
                return;
            }

            if (StateService.Url != null)
            {
                With.ClearBitmap();
            }

            if (StateService.Dialog != null)
            {
                CurrentDialog = StateService.Dialog;
                StateService.Dialog = null;
            }

            var dialog71 = CurrentDialog as TLDialog71;
            if (dialog71 != null)
            {
                _mentionsCounter = dialog71.UnreadMentionsCount.Value;

                var liveLocationService = IoC.Get<ILiveLocationService>();
                var message = liveLocationService.Get(dialog71.Peer, mtProtoService.CurrentUserId);

                if (message != null)
                {
                    LiveLocationBadge = new LiveLocationBadgeViewModel(IoC.Get<ILiveLocationService>(), IoC.Get<ICacheService>(), false) { Message = message };
                    LiveLocationBadge.OpenMessage += OpenLiveLocationBadge;
                    LiveLocationBadge.Closed += CloseLiveLocationBadge;
                }
            }

            var post = StateService.Post;
            StateService.Post = null;
            Int32.TryParse(post, out _post);

            if (StateService.MessageId != null)
            {
                With.ClearBitmap();
                _messageId = StateService.MessageId;
                StateService.MessageId = null;
            }

            var accessToken = StateService.AccessToken;
            StateService.AccessToken = null;
            if (StateService.Bot != null)
            {
                _bot = StateService.Bot;
                _bot.AccessToken = accessToken;
                StateService.Bot = null;

                var chat = With as TLChat;
                if (chat != null)
                {
                    MTProtoService.AddChatUserAsync(chat.Id, _bot.ToInputUser(), new TLInt(0),
                        statedMessage =>
                        {
                            var updates = statedMessage as TLUpdates;
                            if (updates != null)
                            {
                                var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                                if (updateNewMessage != null)
                                {
                                    Handle(updateNewMessage.Message as TLMessageCommon);
                                }

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    Execute.BeginOnUIThread(AppBarCommand);
                                }
                            }
                        },
                        error => BeginOnUIThread(() =>
                        {
                            if (error.TypeEquals(ErrorType.PEER_FLOOD))
                            {
                                //MessageBox.Show(AppResources.PeerFloodAddContact, AppResources.Error, MessageBoxButton.OK);
                                ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodAddContact, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                                    result =>
                                    {
                                        if (result == CustomMessageBoxResult.RightButton)
                                        {
                                            TelegramViewBase.NavigateToUsername(MTProtoService, Constants.SpambotUsername, null, null, null);
                                        }
                                    });
                            }
                            else if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                                && error.TypeEquals(ErrorType.USER_ALREADY_PARTICIPANT))
                            {
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    AppBarCommand();
                                }
                            }

                            Execute.ShowDebugMessage("messages.addChatUser error " + error);
                        }));
                }

                var channel = With as TLChannel;
                if (channel != null)
                {
                    MTProtoService.InviteToChannelAsync(channel.ToInputChannel(), new TLVector<TLInputUserBase> { _bot.ToInputUser() },
                        statedMessage =>
                        {
                            var updates = statedMessage as TLUpdates;
                            if (updates != null)
                            {
                                var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                                if (updateNewMessage != null)
                                {
                                    Handle(updateNewMessage.Message as TLMessageCommon);
                                }

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    Execute.BeginOnUIThread(AppBarCommand);
                                }
                            }
                        },
                        error => BeginOnUIThread(() =>
                        {
                            if (error.TypeEquals(ErrorType.PEER_FLOOD))
                            {
                                //MessageBox.Show(AppResources.PeerFloodAddContact, AppResources.Error, MessageBoxButton.OK);
                                ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodAddContact, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                                    result =>
                                    {
                                        if (result == CustomMessageBoxResult.RightButton)
                                        {
                                            TelegramViewBase.NavigateToUsername(MTProtoService, Constants.SpambotUsername, null, null, null);
                                        }
                                    });
                            }
                            else if (error.TypeEquals(ErrorType.USERS_TOO_MUCH))
                            {
                                MessageBox.Show(AppResources.UsersTooMuch, AppResources.Error, MessageBoxButton.OK);
                            }
                            else if (error.TypeEquals(ErrorType.USER_CHANNELS_TOO_MUCH))
                            {
                                MessageBox.Show(AppResources.UserChannelsTooMuch, AppResources.Error, MessageBoxButton.OK);
                            }
                            else if (error.TypeEquals(ErrorType.BOTS_TOO_MUCH))
                            {
                                MessageBox.Show(AppResources.BotsTooMuch, AppResources.Error, MessageBoxButton.OK);
                            }
                            else if (error.TypeEquals(ErrorType.USER_NOT_MUTUAL_CONTACT))
                            {
                                MessageBox.Show(AppResources.UserNotMutualContact, AppResources.Error, MessageBoxButton.OK);
                            }
                            else if (error.TypeEquals(ErrorType.USER_ALREADY_PARTICIPANT))
                            {
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    AppBarCommand();
                                }
                            }

                            Execute.ShowDebugMessage("messages.addChatUser error " + error);
                        }));
                }
            }
            else
            {
                var user = With as TLUser;
                if (user != null)
                {
                    user.AccessToken = accessToken;
                }
            }

            if (UserActionViewModel.IsRequired(With))
            {
                UserAction = new UserActionViewModel((TLUserBase)With);
                UserAction.InvokeUserAction += (sender, args) => InvokeUserAction();
                UserAction.InvokeUserAction2 += (sender, args) => InvokeUserAction2();
            }
            //return;



            //подписываем на события только, если смогли восстановиться после tombstoning
            EventAggregator.Subscribe(this);

            var inputPeer = With as IInputPeer;
            if (inputPeer != null)
            {
                _peer = inputPeer.ToInputPeer();
            }
            else
            {
                _peer = new TLInputPeerFeed { ChatId = new TLInt(1) };
            }
            _subtitle = GetSubtitle();

            PropertyChanged += (sender, args) =>
            {
                //if (_debugTimer != null) System.Diagnostics.Debug.WriteLine("start property changed " + _debugTimer.Elapsed);

                ShellViewModel.WriteTimer(args.PropertyName);

                if (Property.NameEquals(args.PropertyName, () => Text))
                {
                    if (IsEditingEnabled)
                    {
                        if (!string.IsNullOrEmpty(Text))
                        {
                            if (_editedMessage != null
                                && (_editedMessage.Media is TLMessageMediaEmpty || _editedMessage.Media is TLMessageMediaWebPage))
                            {
                                GetWebPagePreviewAsync(Text);
                            }

                            string searchText;

                            var searchByUsernames = SearchByUsernames(Text, out searchText);
                            if (searchByUsernames)
                            {
                                GetUsernameHints(searchText);
                            }
                            else
                            {
                                ClearUsernameHints();
                            }
                        }
                        else
                        {
                            RestoreReply();

                            ClearUsernameHints();
                        }

                        return;
                    }

                    if (!string.IsNullOrEmpty(Text))
                    {
                        GetWebPagePreviewAsync(Text);

                        string searchText;

                        var searchByStickers = SearchByStickers(Text, out searchText);
                        if (searchByStickers)
                        {
                            GetStickerHints(searchText);
                        }
                        else
                        {
                            ClearStickerHints();
                        }

                        var searchInlineBotResults = SearchInlineBotResults(Text, out searchText);
                        if (searchInlineBotResults)
                        {
                            GetInlineBotResults(searchText);
                        }
                        else
                        {
                            ClearInlineBotResults();
                        }

                        var searchByUsernames = SearchByUsernames(Text, out searchText);
                        if (searchByUsernames)
                        {
                            GetUsernameHints(searchText);
                        }
                        else
                        {
                            ClearUsernameHints();
                        }

                        var searchByCommands = SearchByCommands(Text, out searchText);
                        if (searchByCommands)
                        {
                            GetCommandHints(searchText);
                        }
                        else
                        {
                            ClearCommandHints();
                        }

                        //var searchByHashtags = SearchByHashtags(Text, out searchText);

                        //if (searchByHashtags)
                        //{
                        //    GetHashtagHints(searchText);
                        //}
                        //else
                        //{
                        //    HashtagHints.Clear();
                        //}
                    }
                    else
                    {
                        RestoreReply();

                        ClearStickerHints();
                        ClearInlineBotResults();
                        ClearUsernameHints();
                        ClearHashtagHints();
                        ClearCommandHints();
                    }

                    if (_suppressTyping)
                    {
                        _suppressTyping = false;
                        return;
                    }
                    else
                    {
                        TextTypingManager.SetTyping();
                    }
                }
                else if (Property.NameEquals(args.PropertyName, () => With))
                {
                    NotifyOfPropertyChange(() => IsAppBarCommandVisible);
                    NotifyOfPropertyChange(() => AppBarCommandString);
                }
                //if (_debugTimer != null) System.Diagnostics.Debug.WriteLine("end property changed " + _debugTimer.Elapsed);
            };
            LoadUnsendedTextAsync(draft =>
            {
                GetUnreadMentionsAsync();

                if (StateService.Url != null)
                {
                    var text = StateService.Url.Trim().TrimStart('@');
                    StateService.Url = null;

                    if (StateService.UrlText != null)
                    {
                        text = text + Environment.NewLine + StateService.UrlText;
                        StateService.UrlText = null;
                    }

                    Text = text;
                }
                else if (StateService.WebLink != null)
                {
                    Text = StateService.WebLink.ToString();
                    StateService.WebLink = null;
                }
                else if (StateService.SwitchInlineButton != null)
                {
                    var userName = StateService.SwitchInlineButton.Bot as IUserName;
                    if (userName != null && !TLString.IsNullOrEmpty(userName.UserName))
                    {
                        _currentInlineBot = StateService.SwitchInlineButton.Bot;

                        Text = string.Format("@{0} {1}", userName.UserName, StateService.SwitchInlineButton.Query);
                    }

                    StateService.SwitchInlineButton = null;
                }
                else
                {
                    UpdateDraftMessage(draft);

                    if (Text == null) return;

                    var trimmedText = Text.TrimStart();
                    if (trimmedText.StartsWith("@"))
                    {
                        var words = trimmedText.Split(' ');
                        if (words.Length >= 1)
                        {
                            if (BrowserNavigationService.IsValidUsername(words[0]))
                            {
                                if (trimmedText.Length > words[0].Length)
                                {
                                    ResolveUsername(words[0], trimmedText.Substring(words[0].Length + 1));
                                }
                            }
                        }
                    }
                }
            });

            //return;

            SendSharedPhoto();

            BeginOnThreadPool(() =>
            {
                GetAllStickersAsync();

                if (StateService.Message != null)
                {
                    var message = StateService.Message;
                    StateService.Message = null;

                    ProcessMessages(new List<TLMessageBase> { message });

                    Execute.BeginOnUIThread(() =>
                    {
                        _isUpdated = true;
                        _isFirstSliceLoaded = false;

                        Items.Clear();
                        Items.Add(message);

                        _loadResultHistory = true;
                    });

                    return;
                }

#if WP8
                var isLocalServiceMessage = false;
                var dialog = CurrentDialog as TLDialog;
                if (dialog != null)
                {
                    var topMessage = dialog.TopMessage;
                    isLocalServiceMessage = IsLocalServiceMessage(topMessage);  // ContactRegistered update
                }

                if (CurrentDialog != null
                    && StateService.ForwardMessages == null
                    && CurrentDialog.UnreadCount.Value >= Constants.MinUnreadCountToShowSeparator
                    && !isLocalServiceMessage)
                {
                    var unreadCount = CurrentDialog.UnreadCount.Value;

                    if (unreadCount > 0)
                    {
                        if (With != null)
                        {
                            With.Bitmap = null;
                        }
                        var cachedMessages = CacheService.GetHistory(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));

                        ProcessMessages(cachedMessages);

                        if (cachedMessages.Count >= unreadCount && cachedMessages.Count > 10)
                        {
                            BeginOnUIThread(() =>
                            {
                                var startPosition = 0;
                                for (var i = 0; i < cachedMessages.Count; i++)
                                {
                                    var message = cachedMessages[i] as TLMessageCommon;
                                    if (message != null && !message.Unread.Value)
                                    {
                                        break;
                                    }
                                    startPosition++;
                                }
                                startPosition--;

                                var items = new TLVector<TLMessageBase>();
                                foreach (var message in cachedMessages)
                                {
                                    items.Add(message);
                                }

                                IsFirstSliceLoaded = true;
                                if (startPosition >= 0)
                                {
                                    AddUnreadHistory(startPosition, items);
                                }
                                else
                                {
                                    AddHistory(items.Items);
                                    CurrentDialog.UnreadCount = new TLInt(0);
                                }
                            });
                        }
                        else
                        {
                            var channels = With as TLVector<TLChatBase>;
                            if (channels != null)
                            {
                                var unreadSlice = 10;
                                var offset = Math.Max(0, unreadCount - unreadSlice);
                                IsWorking = true;
                                MTProtoService.GetFeedAsync(
                                    false,
                                    new TLInt(1),
                                    null,
                                    new TLInt(0),
                                    new TLInt(20),
                                    null,
                                    null,
                                    new TLInt(0),
                                    result =>
                                    {
                                        ProcessMessages(result.Messages);

                                        BeginOnUIThread(() =>
                                        {
                                            IsWorking = false;
                                            var startPosition = offset == 0 ? unreadCount - 1 : unreadSlice - 1;

                                            IsFirstSliceLoaded = false;
                                            AddUnreadHistory(startPosition, result.Messages);
                                        });
                                    },
                                    error =>
                                    {
                                        Execute.ShowDebugMessage("messages.getHistory error " + error);
                                        IsWorking = false;
                                    });
                            }
                            else
                            {
                                var unreadSlice = 10;
                                var offset = Math.Max(0, unreadCount - unreadSlice);
                                IsWorking = true;
                                MTProtoService.GetHistoryAsync(Stopwatch.StartNew(),
                                    Peer,
                                    TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                                    offset < Constants.MessagesSlice,
                                    new TLInt(0),
                                    new TLInt(offset),
                                    new TLInt(0),
                                    new TLInt(20),
                                    result =>
                                    {
                                        ProcessMessages(result.Messages);

                                        BeginOnUIThread(() =>
                                        {
                                            IsWorking = false;
                                            var startPosition = offset == 0 ? unreadCount - 1 : unreadSlice - 1;

                                            IsFirstSliceLoaded = false;
                                            AddUnreadHistory(startPosition, result.Messages);
                                        });
                                    },
                                    error =>
                                    {
                                        Execute.ShowDebugMessage("messages.getHistory error " + error);
                                        IsWorking = false;
                                    });
                            }
                        }

                        return;
                    }
                }
#endif

                IList<TLMessageBase> messages = new List<TLMessageBase>();
                if (_post > 0)
                {
                    // history exists

                    messages = CacheService.GetHistory(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), _post, TelegramClient.Constants.MessagesSlice);
                    if (messages.Count > 0)
                    {
                        With.Bitmap = null;
                        ProcessMessages(messages);
                        AddHistory(messages);
                        BeginOnUIThread(() =>
                        {
                            HighlightMessage(messages[0]);
                            //LoadPreviousSlice();
                        });

                        _isFirstSliceLoaded = false;
                        // load from server

                        return;
                    }
                }
                else if (_messageId != null)
                {
                    LoadResultHistory(_messageId);
                    return;
                }

                foreach (var message in GetHistory())
                {
                    if (!SkipMessage(message))
                    {
                        messages.Add(message);
                    }
                }
                ProcessMessages(messages);
                AddHistory(messages);
            });

            ShellViewModel.WriteTimer("DialogDetailsViewModel stop ctor");
        }

        public void AppendMigratedHistory(IList<TLMessageBase> messages)
        {
            var channel = With as TLChannel;
            if (channel != null)
            {
                var migratedFromChatId = channel.MigratedFromChatId;
                if (migratedFromChatId == null)
                {
                    var lastServiceMessage = (messages != null ? messages.LastOrDefault() : null) as TLMessageService;
                    if (lastServiceMessage != null)
                    {
                        var migratedFromAction = lastServiceMessage.Action as TLMessageActionChannelMigrateFrom;
                        if (migratedFromAction != null)
                        {
                            migratedFromChatId = migratedFromAction.ChatId;
                            channel.MigratedFromChatId = migratedFromChatId;
                        }
                    }
                }

                if (migratedFromChatId != null)
                {
                    var lastMessage = messages != null ? messages.OfType<TLMessageService>().LastOrDefault() : null;
                    var clearHistoryMessage = messages != null ? messages.OfType<TLMessageService>().FirstOrDefault(x => x.Action is TLMessageActionClearHistory) : null;
                    var noneClearHistoryMessage = messages != null ? messages.FirstOrDefault(x => !(x is TLMessageService) || !(((TLMessageService)x).Action is TLMessageActionClearHistory)) : null;

                    if ((lastMessage != null && lastMessage.Action is TLMessageActionChannelMigrateFrom) || (clearHistoryMessage != null && noneClearHistoryMessage != null))
                    {
                        var chatMessages = CacheService.GetHistory(new TLPeerChat { Id = migratedFromChatId });
                        if (chatMessages.Count == 0)
                        {
                            var dialog71 = CurrentDialog as TLDialog71;
                            if (dialog71 != null && dialog71.MigratedHistory != null)
                            {
                                chatMessages = dialog71.MigratedHistory;
                            }
                        }

                        foreach (var message in chatMessages)
                        {
                            if (!SkipMessage(message))
                            {
                                messages.Add(message);
                            }
                        }

                        if (chatMessages.Count <= 1)
                        {
                            LoadNextMigratedHistorySlice("GetHistory");
                        }
                    }
                }
            }
        }

        public IList<TLMessageBase> GetHistory()
        {
            var messages = CacheService.GetHistory(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));
            if (messages.Count == 0)
            {
                var dialog = CurrentDialog as TLDialog;
                if (dialog != null)
                {
                    messages = dialog.Messages;
                }
            }

            AppendMigratedHistory(messages);

            return messages;
        }

        private bool SkipMessage(TLMessageBase messageBase)
        {
            var serviceMessage = messageBase as TLMessageService;
            if (serviceMessage == null) return false;

            var channel = With as TLChannel;
            if (channel != null && (channel.MigratedFromChatId != null))
            {
                var chatMigrateTo = serviceMessage.Action as TLMessageActionChatMigrateTo;
                if (chatMigrateTo != null)
                {
                    return true;
                }
            }

            var clearHistory = serviceMessage.Action as TLMessageActionClearHistory;
            if (clearHistory != null)
            {
                return true;
            }

            return false;
        }

        private IList<TLMessageBase> _messages;

        private void AddMessagesWithCache(IList<TLMessageBase> messages)
        {
            if (!_isForwardInAnimationComplete)
            {
                _messages = messages;
                return;
            }

            if (_messages == null)
            {
                View.CloseBitmapCache();

                return;
            }

            const int firstSliceCount = 13;
            var firstSlice = new List<TLMessageBase>();
            var secondSlice = new List<TLMessageBase>();
            for (var i = 0; i < messages.Count; i++)
            {
                if (i < firstSliceCount)
                {
                    firstSlice.Add(messages[i]);
                }
                else
                {
                    secondSlice.Add(messages[i]);
                }
            }

            ((BindableCollection<TLMessageBase>)Items).AddRange(firstSlice);

            View.CloseBitmapCache();

            NotifyOfPropertyChange(() => IsAppBarCommandVisible);

            BeginOnUIThread(() =>
            {
                ((BindableCollection<TLMessageBase>)Items).AddRange(secondSlice);

                if (_isForwardInAnimationComplete)
                {
                    if (_isFirstSliceLoaded)
                    {
                        UpdateReplyMarkup(Items);
                    }

                    BeginOnUIThread(() =>
                    {
                        UpdateItemsAsync(0, 0, Constants.MessagesSlice, false);
                        ReadHistoryAsync();
                    });
                }
                else
                {
                    _updateItemsAndReadHistory = true;
                }
            });
        }

        private void AddHistory(IList<TLMessageBase> messages)
        {
            if (messages.Count == 0)
            {
                IsEmptyDialog = With != null && With.Bitmap == null;

                BeginOnUIThread(() =>
                {
                    if (_isForwardInAnimationComplete)
                    {
                        UpdateItemsAsync(0, 0, Constants.MessagesSlice, false);
                        ReadHistoryAsync();
                    }
                    else
                    {
                        _updateItemsAndReadHistory = true;
                    }
                });
            }
            else
            {
                //if (With.Bitmap != null)
                //{
                //    AddMessagesWithCache(messages);
                //}
                //else
                {
                    LazyItems.Clear();
                    const int firstSliceCount = 13;
                    var isAnimated = With.Bitmap == null && messages.Count > 1;
                    var secondSlice = new List<TLMessageBase>();
                    for (var i = 0; i < messages.Count; i++)
                    {
                        if (i < firstSliceCount)
                        {
                            messages[i]._isAnimated = isAnimated;
                            LazyItems.Add(messages[i]);
                        }
                        else
                        {
                            secondSlice.Add(messages[i]);
                        }
                    }

                    NotifyOfPropertyChange(() => IsAppBarCommandVisible);

                    if (LazyItems.Count == 0)
                    {
                        BeginOnUIThread(() =>
                        {
                            if (_isForwardInAnimationComplete)
                            {
                                UpdateItemsAsync(0, 0, Constants.MessagesSlice, false);
                                ReadHistoryAsync();
                            }
                            else
                            {
                                _updateItemsAndReadHistory = true;
                            }
                        });
                    }
                    else
                    {
                        BeginOnUIThread(() => PopulateItems(() =>
                        {
                            foreach (var item in secondSlice)
                            {
                                Items.Add(item);
                            }

                            if (_isForwardInAnimationComplete)
                            {
                                if (_isFirstSliceLoaded)
                                {
                                    UpdateReplyMarkup(Items);
                                }

                                BeginOnUIThread(() =>
                                {
                                    UpdateItemsAsync(0, 0, Constants.MessagesSlice, false);
                                    ReadHistoryAsync();
                                });
                            }
                            else
                            {
                                _updateItemsAndReadHistory = true;
                            }
                        }));
                    }

                }
            }
        }

        public bool StartGifPlayers { get; set; }

        private void AddUnreadHistory(int startPosition, TLVector<TLMessageBase> messages)
        {
            Items.Clear();
            _isUpdated = true;

            if (startPosition < messages.Count)
            {
                var message = messages[startPosition++];
                Items.Add(message);

                var separator = new TLMessageService17
                {
                    FromId = new TLInt(StateService.CurrentUserId),
                    ToId = TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                    Status = With is TLBroadcastChat
                            ? MessageStatus.Broadcast
                            : MessageStatus.Sending,
                    Out = new TLBool { Value = true },
                    Unread = new TLBool(true),
                    Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                    Action = new TLMessageActionUnreadMessages(),
                    //IsAnimated = true,
                    RandomId = TLLong.Random()
                };
                Items.Add(separator);
            }


            if (startPosition > 1)
            {
                SuppressHideScrollToBottom = true;
                BeginOnUIThread(() =>
                {
                    var unreadMessage = Items.FirstOrDefault();
                    var dialog71 = CurrentDialog as TLDialog71;
                    if (unreadMessage != null && dialog71 != null && dialog71.UnreadMentions != null)
                    {
                        var unreadMention = dialog71.UnreadMentions.LastOrDefault();
                        if (unreadMention != null && unreadMention.Index == unreadMessage.Index)
                        {
                            ReadNextMention();
                        }
                    }

                    ShowScrollToBottomButton();
                });
            }

            var forwardInAnimationComplete = _delayedPosition == -1;
            if (forwardInAnimationComplete)
            {
                AddMessagesAndReadHistory(startPosition, messages);
            }
            else
            {
                _delayedPosition = startPosition;
                _delayedMessages = messages;
            }

            IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && messages.Count == 0;
        }

        private void ShowScrollToBottomButton()
        {
            if (View != null) View.ShowScrollToBottomButton();
        }

        private void HideScrollToBottomButton()
        {
            if (View != null) View.HideScrollToBottomButton();
        }

        private TLVector<TLMessageBase> _delayedMessages;
        private int _delayedPosition;

        private TLObject GetParticipant()
        {
            if (StateService.With == null)
            {
                if (!string.IsNullOrEmpty(StateService.ChatId))
                {
                    var chatIdString = StateService.ChatId;
                    StateService.ChatId = null;
                    int chatId;
                    try
                    {
                        chatId = Convert.ToInt32(chatIdString);
                    }
                    catch (Exception e)
                    {
                        NavigateToShellViewModel();
                        return null;
                    }
                    var chat = CacheService.GetChat(new TLInt(chatId));
                    if (chat != null)
                    {
                        return chat;
                    }

                    NavigateToShellViewModel();
                    return null;
                }

                if (!string.IsNullOrEmpty(StateService.UserId))
                {
                    var userIdString = StateService.UserId;
                    StateService.UserId = null;

                    int userId;
                    try
                    {
                        userId = Convert.ToInt32(userIdString);
                    }
                    catch (Exception e)
                    {
                        NavigateToShellViewModel();
                        return null;
                    }
                    var user = CacheService.GetUser(new TLInt(userId));
                    if (user != null)
                    {
                        return user;
                    }

                    NavigateToShellViewModel();
                    return null;
                }

                if (!string.IsNullOrEmpty(StateService.BroadcastId))
                {
                    var userIdString = StateService.BroadcastId;
                    StateService.BroadcastId = null;

                    int broadcastId;
                    try
                    {
                        broadcastId = Convert.ToInt32(userIdString);
                    }
                    catch (Exception e)
                    {
                        NavigateToShellViewModel();
                        return null;
                    }
                    var broadcast = CacheService.GetBroadcast(new TLInt(broadcastId));
                    if (broadcast != null)
                    {
                        return broadcast;
                    }

                    NavigateToShellViewModel();
                    return null;
                }

                NavigateToShellViewModel();
                return null;
            }

            return StateService.With;
        }

        public void GetAllStickersAsync()
        {
            StateService.GetAllStickersAsync(cachedStickers =>
            {
                Stickers = cachedStickers;

                var cachedStickers43 = cachedStickers as TLAllStickers43;
                if (cachedStickers43 != null
                    && cachedStickers43.FavedStickers == null)
                {
                    MTProtoService.GetFavedStickersAsync(new TLInt(0),
                        result =>
                        {
                            var favedStickers = result as TLFavedStickers;
                            if (favedStickers != null)
                            {
                                cachedStickers43.FavedStickers = favedStickers;
                                StateService.SaveAllStickersAsync(cachedStickers43);
                            }
                        },
                        error =>
                        {

                        });
                }

                var featuredStickers = StateService.GetFeaturedStickers();
                if (featuredStickers == null)
                {
                    MTProtoService.GetFeaturedStickersAsync(true, new TLInt(0),
                        result =>
                        {
                            featuredStickers = result as TLFeaturedStickers;
                            if (featuredStickers != null)
                            {
                                StateService.SaveFeaturedStickersAsync(featuredStickers);
                            }
                        },
                        error =>
                        {

                        });
                }

                var cachedStickers29 = cachedStickers as TLAllStickers29;
                if (cachedStickers29 != null
                    && cachedStickers29.Date != null
                    && cachedStickers29.Date.Value != 0)
                {
                    var date = TLUtils.ToDateTime(cachedStickers29.Date);
                    if (date < DateTime.Now.AddSeconds(Constants.GetAllStickersInterval))
                    {
                        return;
                    }
                }

                var hash = cachedStickers != null ? cachedStickers.Hash ?? TLString.Empty : TLString.Empty;

                MTProtoService.GetAllStickersAsync(hash,
                    result =>
                    {
                        var allStickers = result as TLAllStickers43;
                        if (allStickers != null)
                        {
                            if (cachedStickers29 != null)
                            {
                                allStickers.ShowStickersTab = cachedStickers29.ShowStickersTab;
                                allStickers.RecentlyUsed = cachedStickers29.RecentlyUsed;
                                allStickers.Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
                            }

                            if (cachedStickers43 != null)
                            {
                                allStickers.RecentStickers = cachedStickers43.RecentStickers;
                                allStickers.FavedStickers = cachedStickers43.FavedStickers;
                            }
                            Stickers = allStickers;
                            cachedStickers = allStickers;
                            StateService.SaveAllStickersAsync(cachedStickers);
                        }
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getAllStickers error " + error);
                    });
            });
        }

#if DEBUG

        public void SendStatus()
        {
            MTProtoService.UpdateStatusAsync(new TLBool(false),
                result =>
                {
                    BeginOnUIThread(() => MessageBox.Show("SendStatus result: " + result.Value, "Result OK", MessageBoxButton.OK));
                },
                error =>
                {
                    BeginOnUIThread(() => MessageBox.Show("SendStatus error: " + error.Code + " " + error.Message, "Result ERROR", MessageBoxButton.OK));
                });
        }
#endif

        private void UpdateItemsAsync(int offset, int maxId, int count, bool isAnimated)
        {
            if (_post > 0)
            {
                return;
            }

            if (_messageId != null)
            {
                return;
            }

            if (IsBroadcast && !IsChannel)
            {
                return;
            }

            _isUpdating = true;
            IsWorking = true;

            TLObject.LogNotify = true;
            TelegramEventAggregator.LogPublish = true;

            ShellViewModel.StartNewTimer();

            ShellViewModel.WriteTimer("UpdateItems start");

            if (Peer is TLInputPeerFeed)
            {
                MTProtoService.GetFeedAsync(
                    false,
                    new TLInt(1),
                    null,
                    new TLInt(0),
                    new TLInt(Constants.MessagesSlice),
                    null,
                    null,
                    new TLInt(0),
                    result => BeginOnUIThread(() =>
                    {
                        UpdateItemsCompleted(offset, maxId, count, isAnimated, result);
                    }),
                    error => BeginOnUIThread(() =>
                    {
                        _loadingNextSlice = false;
                        IsWorking = false;
                        Status = string.Empty;
                        Execute.ShowDebugMessage("channels.getFeed error " + error);
                    }));
            }
            else
            {
                MTProtoService.GetHistoryAsync(null,
                    Peer,
                    TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                    true,
                    new TLInt(0),
                    new TLInt(0),
                    new TLInt(maxId),
                    new TLInt(count),
                    result =>
                    {
                        UpdateItemsCompleted(offset, maxId, count, isAnimated, result);
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getHistory error " + error);
                        _isUpdating = false;
                        _isUpdated = true;
                        IsWorking = false;
                    });
            }
        }

        private void UpdateItemsCompleted(int offset, int maxId, int count, bool isAnimated, TLMessagesBase result)
        {
            ShellViewModel.WriteTimer("UpdateItems callback");

            var resultCount = result.Messages.Count;
            ProcessMessages(result.Messages);

            BeginOnUIThread(() =>
            {
                var resultMessages = new List<TLMessageBase>();
                foreach (var message in result.Messages)
                {
                    if (!SkipMessage(message))
                    {
                        resultMessages.Add(message);
                    }
                }

                ShellViewModel.WriteTimer("UpdateItems start ui");

                TelegramEventAggregator.LogPublish = false;
                TLObject.LogNotify = false;

                // all history is new and has no messages with Index = 0
                var lastMessage = resultMessages.LastOrDefault();
                if (lastMessage != null)
                {
                    var lastId = lastMessage.Index;

                    var firstMessage = Items.FirstOrDefault(x => x.Index != 0);
                    var hasSendingMessages = Items.FirstOrDefault(x => x.Index == 0) != null;
                    if (firstMessage != null && !hasSendingMessages)
                    {
                        var firstId = firstMessage.Index;
                        if (lastId > firstId)
                        {
                            Items.Clear();
                        }
                    }
                }

                foreach (var message in resultMessages)
                {
                    message._isAnimated = isAnimated;
                }

                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && resultMessages.Count == 0;

                var checkGifPlayers = Items.Count < 5 && resultMessages.Count > 0;
                var checkGifStartPosition = Items.Count;
                var itemsCount = Items.Count;

                var channel = With as TLChannel;
                if (channel != null)
                {
                    var needUpdate = Items.Count < resultMessages.Count;

                    if (!needUpdate)
                    {
                        int i = 0, j = 0;
                        for (; i < resultMessages.Count && j < Items.Count; i++, j++)
                        {
                            if (itemsCount - 1 < i || resultMessages[i] != Items[j])
                            {
                                // skip "User joined Telegram!" message
                                var serviceMessage = Items[j] as TLMessageService;
                                if (serviceMessage != null)
                                {
                                    var unreadSeparator = serviceMessage.Action as TLMessageActionUnreadMessages;
                                    if (unreadSeparator != null)
                                    {
                                        i--;
                                        continue;
                                    }
                                }

                                var isSendingMessage = Items[j].Index == 0 && Items[i].RandomIndex != 0;
                                if (isSendingMessage)
                                {
                                    i--;
                                    continue;
                                }

                                needUpdate = true;
                                break;
                            }
                        }
                    }

                    if (needUpdate)
                    {
                        Items.Clear();
                        foreach (var message in resultMessages)
                        {
                            Items.Add(message);
                        }
                        var dialog = CurrentDialog as TLDialog;
                        if (dialog != null)
                        {
                            dialog.Messages.Clear();
                            foreach (var message in UngroupEnumerator(result.Messages))
                            {
                                dialog.Messages.Add(message);
                            }
                            var firstMessage = dialog.Messages.FirstOrDefault();
                            if (firstMessage != null)
                            {
                                dialog._topMessage = firstMessage;
                                dialog.TopMessageId = firstMessage.Id;

                                EventAggregator.Publish(new TopMessageUpdatedEventArgs(dialog, firstMessage));
                            }
                        }
                    }
                }
                else
                {
                    // handle one grouped messages
                    if (Items.Count == 1)
                    {
                        var message73 = Items[0] as TLMessage73;
                        if (message73 != null && message73.GroupedId != null)
                        {
                            Items.RemoveAt(0);
                        }
                    }

                    // remove tail
                    IList<TLMessageBase> removedItems;
                    MergeItems(Items, resultMessages, offset, maxId, count, out removedItems);
                }

                if (checkGifPlayers)
                {
                    var settings = StateService.GetChatSettings();
                    if (settings.AutoPlayGif)
                    {
                        for (var i = checkGifStartPosition; i < Items.Count && i < 5; i++)
                        {
                            if (Items[i].IsGif())
                            {
                                StartGifPlayers = true;
                                NotifyOfPropertyChange(() => StartGifPlayers);
                                break;
                            }
                        }
                    }
                }

                if (count > resultCount)
                {
                    IsLastSliceLoaded = true;
                    LoadNextMigratedHistorySlice("UpdateItemsAsync");
                }

                _isUpdating = false;
                _isUpdated = true;
                IsWorking = false;

                var bot = With as TLUser;
                if (itemsCount == 0 && Items.Count > 0 && bot != null && bot.IsBot)
                {
                    NotifyOfPropertyChange(() => IsAppBarCommandVisible);
                }
                ShellViewModel.WriteTimer("UpdateItems end ui");
            });
        }

        private int MergeItems(IList<TLMessageBase> current, IList<TLMessageBase> updated, int offset, int maxId, int count, out IList<TLMessageBase> removedItems)
        {
            TLInt migratedFromChatId = null;
            var channel = With as TLChannel;
            if (channel != null)
            {
                migratedFromChatId = channel.MigratedFromChatId;
            }

            var lastIndex = TLUtils.MergeItemsDesc(x => x.DateIndex, current, updated, offset, maxId, count, out removedItems, x => x.Index,
                m =>
                {
                    return IsLocalServiceMessage(m) || IsChatHistory(migratedFromChatId, m);
                });


            return lastIndex;
        }

        private static bool IsChatHistory(TLInt migratedFromChatId, TLMessageBase messageBase)
        {
            if (migratedFromChatId == null) return false;

            var message = messageBase as TLMessageCommon;
            if (message != null)
            {
                if (message.ToId is TLPeerChat)
                {
                    if (message.ToId.Id.Value == migratedFromChatId.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsLocalServiceMessage(TLMessageBase messageBase)
        {
            var messageService = messageBase as TLMessageService;
            if (messageService != null)
            {
                var action = messageService.Action;
                if (action is TLMessageActionContactRegistered)
                {
                    return true;
                }
            }

            return false;
        }

        private volatile bool _isUpdating;

        private volatile bool _isUpdated;

        public bool SliceLoaded { get; set; }

        private bool _loadingNextSlice;

        public void LoadNextSlice()
        {
            if (_loadMessageHistory)
            {
                return;
            }

            if (_messageId != null)
            {
                LoadResultHistory(_messageId);
                return;
            }

            if (IsBroadcast && !IsChannel)
            {
                return;
            }

            if (_loadingNextSlice)
            {
                return;
            }

            if (
#if WP8
_isUpdating ||
                !_isUpdated ||
#endif
 IsWorking || LazyItems.Count > 0) return;

            if (IsLastSliceLoaded)
            {
                LoadNextMigratedHistorySlice(Thread.CurrentThread.ManagedThreadId + " ilsl");
                return;
            }

            var channel = With as TLChannel;
            if (channel != null)
            {
                var lastMessage = Items.LastOrDefault() as TLMessageCommon;
                if (lastMessage != null
                    && lastMessage.ToId is TLPeerChat)
                {
                    LoadNextMigratedHistorySlice(Thread.CurrentThread.ManagedThreadId + " ch");
                    return;
                }
            }

            IsWorking = true;
            var maxMessageId = int.MaxValue;
            TLMessageCommon maxMessage = null;
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Index != 0
                    && Items[i].Index < maxMessageId)
                {
                    maxMessageId = Items[i].Index;
                    maxMessage = Items[i] as TLMessageCommon;
                }
            }

            if (maxMessageId == int.MaxValue)
            {
                maxMessageId = 0;
            }

            _loadingNextSlice = true;

            if (Peer is TLInputPeerFeed)
            {
                TLFeedPosition offsetPosition = null;
                if (maxMessage != null)
                {
                    offsetPosition = new TLFeedPosition
                    {
                        Date = maxMessage.Date,
                        Peer = maxMessage.ToId,
                        Id = maxMessage.Id
                    };
                }

                MTProtoService.GetFeedAsync(
                    false,
                    new TLInt(1),
                    offsetPosition,
                    new TLInt(0),
                    new TLInt(Constants.MessagesSlice),
                    null,
                    null,
                    new TLInt(0),
                    result =>
                    {
                        LoadNextSliceCompleted(result);
                    },
                    error => BeginOnUIThread(() =>
                    {
                        _loadingNextSlice = false;
                        IsWorking = false;
                        Status = string.Empty;
                        Execute.ShowDebugMessage("channels.getFeed error " + error);
                    }));
            }
            else
            {
                //TLObject.LogNotify = true;
                //var maxMessageId = maxMessage != null ? maxMessage.Index : 0;
                MTProtoService.GetHistoryAsync(Stopwatch.StartNew(),
                    Peer,
                    TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                    Items.Count < 1.5 * Constants.MessagesSlice,
                    new TLInt(0),
                    new TLInt(0),
                    new TLInt(maxMessageId),
                    new TLInt(Constants.MessagesSlice),
                    result =>
                    {
                        LoadNextSliceCompleted(result);
                    },
                    error => BeginOnUIThread(() =>
                    {
                        _loadingNextSlice = false;
                        IsWorking = false;
                        Status = string.Empty;
                        Execute.ShowDebugMessage("messages.getHistory error " + error);
                    }));
            }
        }

        private void LoadNextSliceCompleted(TLMessagesBase result)
        {
            var resultCount = result.Messages.Count;
            ProcessMessages(result.Messages);

            BeginOnUIThread(() =>
            {
                //TLObject.LogNotify = false;

                _loadingNextSlice = false;
                IsWorking = false;
                SliceLoaded = true;

                var nextSlice = new List<TLMessageBase>();
                foreach (var message in result.Messages)
                {
                    //message.IsAnimated = false;
                    if (!SkipMessage(message))
                    {
                        Items.Add(message);

                        var message31 = message as TLMessage31;
                        if (message31 != null && !message31.Out.Value)
                        {
                            if (message31.ReplyMarkup != null
                                && ReplyMarkup == null)
                            {
                                var fromId = message31.FromId;
                                var user = CacheService.GetUser(fromId) as TLUser;
                                if (user != null && user.IsBot)
                                {
                                    SetReplyMarkup(message31, true);
                                    break;
                                }
                            }
                        }
                    }
                }

                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                if (resultCount < Constants.MessagesSlice)
                {
                    IsLastSliceLoaded = true;
                    LoadNextMigratedHistorySlice(Thread.CurrentThread.ManagedThreadId + " gh");
                }
            });
        }

        private bool _isLastMigratedHistorySliceLoaded;

        private bool _isLoadingNextMigratedHistorySlice;

        private void LoadNextMigratedHistorySlice(string debugInfo)
        {
            var channel = With as TLChannel;
            if (channel == null || channel.MigratedFromChatId == null) return;

            if (_isLastMigratedHistorySliceLoaded) return;

            if (LazyItems.Count > 0) return;

            if (_isLoadingNextMigratedHistorySlice) return;

            var dialog71 = CurrentDialog as TLDialog71;
            if (dialog71 != null)
            {
                var firstItem = dialog71.Messages.FirstOrDefault();
                if (IsClearHistoryMessage(firstItem))
                {
                    return;
                }
            }

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
                maxMessageId = channel.MigratedFromMaxId != null ? channel.MigratedFromMaxId.Value : int.MaxValue;
            }

            var isFirstMigratedSlice = channel.MigratedFromMaxId != null && maxMessageId == channel.MigratedFromMaxId.Value;
            if (isFirstMigratedSlice)
            {
                if (dialog71 != null && dialog71.MigratedHistory != null)
                {
                    foreach (var message in dialog71.MigratedHistory)
                    {
                        if (!SkipMessage(message))
                        {
                            Items.Add(message);
                        }
                    }

                    IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                    return;
                }
            }

            _isLoadingNextMigratedHistorySlice = true;
            IsWorking = true;
            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(),
                new TLInputPeerChat { ChatId = channel.MigratedFromChatId },
                new TLPeerChat { Id = channel.MigratedFromChatId },
                (channel.MigratedFromMaxId != null && maxMessageId == channel.MigratedFromMaxId.Value) || maxMessageId == int.MaxValue,
                new TLInt(0),
                new TLInt(0),
                new TLInt(maxMessageId),
                new TLInt(Constants.MessagesSlice),
                result =>
                {
                    var resultCount = result.Messages.Count;
                    ProcessMessages(result.Messages);

                    if (isFirstMigratedSlice)
                    {
                        if (dialog71 != null)
                        {
                            dialog71.MigratedHistory = result.Messages;
                        }
                    }

                    BeginOnUIThread(() =>
                    {
                        _isLoadingNextMigratedHistorySlice = false;
                        IsWorking = false;

                        if (resultCount < Constants.MessagesSlice)
                        {
                            _isLastMigratedHistorySliceLoaded = true;
                        }
                        foreach (var message in result.Messages)
                        {
                            if (!SkipMessage(message))
                            {
                                Items.Add(message);
                            }
                        }

                        IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    _isLoadingNextMigratedHistorySlice = false;
                    IsWorking = false;
                    Status = string.Empty;
                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        private bool _isPreviousSliceLoading;

        private bool _isFirstSliceLoaded = true;

        public bool IsFirstSliceLoaded
        {
            get { return _isFirstSliceLoaded; }
            set { SetField(ref _isFirstSliceLoaded, value, () => IsFirstSliceLoaded); }
        }

        private bool _holdScrollingPosition;

        public bool HoldScrollingPosition
        {
            get { return _holdScrollingPosition; }
            set { SetField(ref _holdScrollingPosition, value, () => HoldScrollingPosition); }
        }



        private bool _isFirstMigratedHistorySliceLoaded;

        private bool _isLoadingFirstMigratedHistorySlice;

        private void LoadPreviousMigratedHistorySlice(string debugInfo)
        {
            var channel = With as TLChannel;
            if (channel == null || channel.MigratedFromChatId == null) return;

            if (_isFirstMigratedHistorySliceLoaded) return;

            if (IsWorking || LazyItems.Count > 0) return;

            if (_isLoadingFirstMigratedHistorySlice) return;

            var maxMessageId = 0;
            for (var i = 0; i < Items.Count; i++)
            {
                var messageCommon = Items[i] as TLMessageCommon;
                if (messageCommon == null) continue;

                var peerChat = messageCommon.ToId as TLPeerChat;
                if (peerChat == null) continue;

                if (Items[i].Index != 0
                    && Items[i].Index > maxMessageId)
                {
                    maxMessageId = Items[i].Index;
                }
            }

            if (maxMessageId == 0)
            {
                maxMessageId = channel.MigratedFromMaxId != null ? channel.MigratedFromMaxId.Value : 0;
            }

            _isLoadingFirstMigratedHistorySlice = true;
            IsWorking = true;
            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(),
                new TLInputPeerChat { ChatId = channel.MigratedFromChatId },
                new TLPeerChat { Id = channel.MigratedFromChatId },
                false,
                new TLInt(0),
                new TLInt(-Constants.MessagesSlice),
                new TLInt(maxMessageId),
                new TLInt(Constants.MessagesSlice),
                result =>
                {
                    var resultCount = result.Messages.Count;
                    ProcessMessages(result.Messages);

                    BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        _isLoadingFirstMigratedHistorySlice = false;
                        IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                        if (resultCount < Constants.MessagesSlice)
                        {
                            _isFirstMigratedHistorySliceLoaded = true;
                        }

                        HoldScrollingPosition = true;
                        for (var i = result.Messages.Count; i > 0; i--)
                        {
                            var message = result.Messages[i - 1];
                            if (message.Index > maxMessageId)
                            {
                                Items.Insert(0, result.Messages[i - 1]);
                            }
                        }
                        HoldScrollingPosition = false;
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    _isLoadingFirstMigratedHistorySlice = false;
                    IsWorking = false;
                    Status = string.Empty;
                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        public void LoadPreviousSlice(string str)
        {
            if (IsBroadcast && !IsChannel)
            {
                return;
            }

            if (_isPreviousSliceLoading
                || _isFirstSliceLoaded) return;


            var channel = With as TLChannel;
            if (channel != null)
            {
                if (!_isFirstMigratedHistorySliceLoaded)
                {
                    var firstMessage = Items.FirstOrDefault() as TLMessageCommon;
                    if (firstMessage != null
                        && firstMessage.ToId is TLPeerChat)
                    {
                        LoadPreviousMigratedHistorySlice(Thread.CurrentThread.ManagedThreadId + " ch");
                        return;
                    }
                }
            }


            _isPreviousSliceLoading = true;
            var maxMessageId = 1;
            TLMessageCommon maxMessage = null;
            for (var i = 0; i < Items.Count; i++)
            {
                if (channel != null && channel.MigratedFromChatId != null)
                {
                    var messageCommon = Items[i] as TLMessageCommon;
                    if (messageCommon != null && messageCommon.ToId is TLPeerChat)
                    {
                        continue;
                    }
                }

                if (Items[i].Index != 0
                    && Items[i].Index > maxMessageId)
                {
                    maxMessageId = Items[i].Index;
                    maxMessage = Items[i] as TLMessageCommon;
                }
            }
            if (Peer is TLInputPeerFeed)
            {
                TLFeedPosition offsetPosition = null;
                if (maxMessage != null)
                {
                    offsetPosition = new TLFeedPosition
                    {
                        Date = maxMessage.Date,
                        Peer = maxMessage.ToId,
                        Id = maxMessage.Id
                    };
                }

                MTProtoService.GetFeedAsync(
                    false,
                    new TLInt(1),
                    offsetPosition,
                    new TLInt(-Constants.MessagesSlice),
                    new TLInt(Constants.MessagesSlice),
                    null,
                    null,
                    new TLInt(0),
                    result =>
                    {
                        LoadPreviousSliceCompleted(result, maxMessageId);
                    },
                    error => BeginOnUIThread(() =>
                    {
                        _loadingNextSlice = false;
                        IsWorking = false;
                        Status = string.Empty;
                        Execute.ShowDebugMessage("channels.getFeed error " + error);
                    }));
            }
            else
            {
                IsWorking = true;
                MTProtoService.GetHistoryAsync(Stopwatch.StartNew(),
                    Peer,
                    TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                    false,
                    new TLInt(0),
                    new TLInt(-Constants.MessagesSlice),
                    new TLInt(maxMessageId),
                    new TLInt(Constants.MessagesSlice),
                    result =>
                    {
                        LoadPreviousSliceCompleted(result, maxMessageId);
                    },
                    error =>
                    {
                        IsWorking = false;
                        _isPreviousSliceLoading = false;
                        Status = string.Empty;
                        Execute.ShowDebugMessage("messages.getHistory error " + error);
                    });
            }
        }

        private void LoadPreviousSliceCompleted(TLMessagesBase result, int maxMessageId)
        {
            var resultCount = result.Messages.Count;
            ProcessMessages(result.Messages);

            BeginOnUIThread(() =>
            {
                IsWorking = false;
                _isPreviousSliceLoading = false;
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                if (resultCount < Constants.MessagesSlice)
                {
                    IsFirstSliceLoaded = true;
                }

                HoldScrollingPosition = true;
                for (var i = result.Messages.Count; i > 0; i--)
                {
                    var message = result.Messages[i - 1];
                    if (message.Index > maxMessageId)
                    {
                        Items.Insert(0, result.Messages[i - 1]);
                    }
                }
                HoldScrollingPosition = false;
            });
        }

        public void LoadPreviousSlice()
        {
            LoadPreviousSlice(string.Empty);
        }

        private void ReadHistoryAsync()
        {
            BeginOnThreadPool(() =>
            {
                var haveUnreadMessages = false;

                CurrentDialog = CurrentDialog ?? CacheService.GetDialog(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));

                if (CurrentDialog == null) return;

                var maxId = 0;
                TLMessageCommon maxMessage = null;
                var dialog = CurrentDialog as TLDialog;
                TLMessageCommon topMessage = null;
                if (dialog != null)
                {
                    topMessage = dialog.TopMessage as TLMessageCommon;
                    if (topMessage != null
                        && !topMessage.Out.Value
                        && topMessage.Unread.Value)
                    //&& !topMessage.IsAudioVideoMessage())
                    {
                        maxId = topMessage.Index;
                        maxMessage = topMessage;
                        haveUnreadMessages = true;
                    }
                }

                if (!haveUnreadMessages)
                {
                    for (var i = 0; i < 10 && i < Items.Count; i++)
                    {
                        var messageCommon = Items[i] as TLMessageCommon;
                        if (messageCommon != null
                            && !messageCommon.Out.Value
                            && messageCommon.Unread.Value)
                        //&& !messageCommon.IsAudioVideoMessage())
                        {
                            maxId = maxId > messageCommon.Index ? maxId : messageCommon.Index;
                            maxMessage = messageCommon;
                            haveUnreadMessages = true;
                            break;
                        }
                    }
                }

                if (!haveUnreadMessages)
                {
                    haveUnreadMessages = CurrentDialog.UnreadCount.Value > 0;
                }

                if (!haveUnreadMessages)
                {
                    CurrentDialog.NotifyOfPropertyChange(() => CurrentDialog.UnreadCount);
                    var dialog71 = CurrentDialog as TLDialog71;
                    if (dialog71 != null && dialog71.UnreadMark)
                    {
                        MTProtoService.MarkDialogUnreadAsync(
                            false, 
                            new TLInputDialogPeer{ Peer = Peer }, 
                            result => Execute.BeginOnUIThread(() =>
                            {
                                dialog71.UnreadMark = false;
                                dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMark);
                            }));
                    }

                    return;
                }

                SetRead(topMessage);

                var channels = With as TLVector<TLChatBase>;
                var channel = With as TLChannel;
                if (channels != null)
                {
                    return;
                    MTProtoService.ReadFeedAsync(new TLInt(1), new TLFeedPosition { Date = maxMessage.Date, Id = maxMessage.Id, Peer = maxMessage.ToId },
                        result =>
                        {
                            Execute.ShowDebugMessage("channels.readHistory result=" + result);
                        },
                        error =>
                        {
                            Execute.ShowDebugMessage("channels.readHistory error " + error);
                        });
                }
                else if (channel != null)
                {
                    MTProtoService.ReadHistoryAsync(channel, new TLInt(maxId),
                        result =>
                        {
                            Execute.ShowDebugMessage("channels.readHistory result=" + result);
                        },
                        error =>
                        {
                            Execute.ShowDebugMessage("channels.readHistory error " + error);
                        });
                }
                else
                {
                    MTProtoService.ReadHistoryAsync(Peer, new TLInt(maxId), new TLInt(0),
                        affectedHistory =>
                        {
                            //SetRead(topMessage, d => new TLInt(0));
                        },
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.readHistory error " + error);
                        });
                }
            });
        }

        private void ReadMessageContents(TLMessage25 message)
        {
            if (message == null) return;

            if (message.Index != 0 && !message.Out.Value && message.NotListened)
            {
                var channel = With as TLChannel;
                if (channel != null)
                {
                    MTProtoService.ReadMessageContentsAsync(channel.ToInputChannel(), new TLVector<TLInt> { message.Id },
                        result => Execute.BeginOnUIThread(() =>
                        {
                            message.SetListened();
                            message.Media.NotListened = false;
                            message.Media.NotifyOfPropertyChange(() => message.Media.NotListened);

                            if (message.IsMention)
                            {
                                ListenMention(message.Index);
                            }

                            CacheService.Commit();
                        }),
                        error => Execute.ShowDebugMessage("channels.readMessageContents error " + error));
                }
                else
                {
                    MTProtoService.ReadMessageContentsAsync(new TLVector<TLInt> { message.Id },
                        result => Execute.BeginOnUIThread(() =>
                        {
                            message.SetListened();
                            message.Media.NotListened = false;
                            message.Media.NotifyOfPropertyChange(() => message.Media.NotListened);

                            if (message.IsMention)
                            {
                                ListenMention(message.Index);
                            }

                            CacheService.Commit();
                        }),
                        error => Execute.ShowDebugMessage("messages.readMessageContents error " + error));
                }
            }
        }

        private void DeleteChannelInternal(TLInt channelId, bool suppressMessage = false)
        {
            BeginOnUIThread(() =>
            {
                if (!suppressMessage)
                {
                    MessageBox.Show(AppResources.GroupIsNoLongerAccessible, AppResources.Error, MessageBoxButton.OK);
                }

                var dialog = CurrentDialog ?? CacheService.GetDialog(new TLPeerChannel { Id = channelId });

                if (dialog != null)
                {
                    EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                    CacheService.DeleteDialog(dialog);
                    DialogsViewModel.UnpinFromStart(dialog);
                }
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    NavigateToShellViewModel();
                }
            });
        }

        public object _participantsSyncRoot = new object();

        private Timer _channelScheduler;

        public void StopChannelScheduler()
        {
            _channelScheduler.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnChannelSchedulerTick(object state)
        {
            if (!_isActive)
            {
                _channelScheduler.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            var channel = With as TLChannel;
            if (channel == null)
            {
                _channelScheduler.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            if (channel.IsMegaGroup)
            {
                MTProtoService.GetParticipantsAsync(channel.ToInputChannel(), new TLChannelParticipantsRecent(), new TLInt(0), new TLInt(200), new TLInt(0),
                    result2 =>
                    {
                        var channelParticipants = result2 as TLChannelParticipants;
                        if (channelParticipants != null)
                        {
                            channel.ChannelParticipants = channelParticipants;
                            TLUtils.SaveObjectToMTProtoFile(_participantsSyncRoot, channel.ChannelParticipantsFileName, result2);

                            Execute.BeginOnUIThread(() =>
                            {
                                Subtitle = GetSubtitle();
                            });
                        }
                    },
                    error =>
                    {
                        if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST)
                            && TLRPCError.TypeEquals(error, ErrorType.CHAT_ADMIN_REQUIRED))
                        {
                            channel.ChannelParticipants = null;
                            FileUtils.Delete(_participantsSyncRoot, channel.ChannelParticipantsFileName);
                        }
                    });
            }

            var pts = channel.Pts;
            if (pts == null)
            {
                var dialogChannel = CurrentDialog as IDialogPts;
                if (dialogChannel != null)
                {
                    pts = dialogChannel.Pts;
                }
            }

            if (pts != null)
            {
                pts = new TLInt(pts.Value - 10 > 0 ? pts.Value - 10 : 1);
                Execute.ShowDebugMessage("updates.getChannelDifference channel_id=" + channel.Index + " pts=" + pts);
                MTProtoService.GetChannelDifferenceAsync(false, channel.ToInputChannel(), new TLChannelMessagesFilterEmpty(), pts, new TLInt(10),
                    result =>
                    {
                        if (result.Timeout != null && result.Timeout.Value > 0)
                        {
                            _channelScheduler.Change(TimeSpan.FromSeconds(result.Timeout.Value), TimeSpan.FromMilliseconds(-1));
                        }
                        else
                        {
                            _channelScheduler.Change(Timeout.Infinite, Timeout.Infinite);
                        }
                    },
                    error =>
                    {
                        _channelScheduler.Change(Timeout.Infinite, Timeout.Infinite);
                    });
            }
        }

        private void GetFullInfo()
        {
            if (LiveLocationBadge != null)
            {
                UpdateLiveLocations(
                    result =>
                    {

                    },
                    error =>
                    {

                    });
            }

            if (Peer is TLInputPeerChannel)
            {
                var channelForbidden = With as TLChannelForbidden;
                if (channelForbidden != null)
                {
                    DeleteChannelInternal(channelForbidden.Id);
                    return;
                }

                var channel = With as TLChannel;
                if (channel == null)
                {
                    return;
                }

                OnChannelSchedulerTick(this);

                MTProtoService.GetFullChannelAsync(channel.ToInputChannel(),
                    result =>
                    {
                        channel.ChatPhoto = result.FullChat.ChatPhoto;
                        channel.Participants = result.FullChat.Participants;
                        channel.NotifySettings = result.FullChat.NotifySettings;
                        var channelFull = result.FullChat as TLChannelFull;
                        if (channelFull != null)
                        {
                            channel.ExportedInvite = channelFull.ExportedInvite;
                            channel.About = channelFull.About;
                            channel.ParticipantsCount = channelFull.ParticipantsCount;
                            channel.AdminsCount = channelFull.AdminsCount;
                            channel.KickedCount = channelFull.KickedCount;
                        }

                        var channelFull41 = result.FullChat as TLChannelFull41;
                        if (channelFull41 != null)
                        {
                            channel.MigratedFromChatId = channelFull41.MigratedFromChatId;
                            channel.MigratedFromMaxId = channelFull41.MigratedFromMaxId;
                            channel.BotInfo = channelFull41.BotInfo;
                            _cachedCommandResults.Clear();
                            _commands = null;
                        }

                        var channelFull49 = result.FullChat as TLChannelFull49;
                        if (channelFull49 != null)
                        {
                            var channel49 = channel as TLChannel49;
                            if (channel49 != null)
                            {
                                channel49.PinnedMsgId = channelFull49.PinnedMsgId;
                                if (channel49.HiddenPinnedMsgId != null)
                                {
                                    if (channel49.PinnedMsgId == null ||
                                        channel49.PinnedMsgId.Value != channel49.HiddenPinnedMsgId.Value)
                                    {
                                        channel49.HiddenPinnedMsgId = null;
                                    }
                                }
                            }
                        }

                        EventAggregator.Publish(channel);

                        // get participants

                        if (channelFull != null
                            && channelFull.CanViewParticipants)
                        {
                            var channelParticipants = TLUtils.OpenObjectFromMTProtoFile<TLChannelParticipants>(_participantsSyncRoot, channel.ChannelParticipantsFileName);
                            channel.ChannelParticipants = channelParticipants;

                            MTProtoService.GetParticipantsAsync(channel.ToInputChannel(), new TLChannelParticipantsRecent(), new TLInt(0), new TLInt(200), new TLInt(0),
                                result2 =>
                                {
                                    var channelParticipants2 = result2 as TLChannelParticipants;
                                    if (channelParticipants2 != null)
                                    {
                                        channelParticipants = channelParticipants2;
                                        channel.ChannelParticipants = channelParticipants2;
                                        TLUtils.SaveObjectToMTProtoFile(_participantsSyncRoot, channel.ChannelParticipantsFileName, result2);

                                        Execute.BeginOnUIThread(() =>
                                        {
                                            Subtitle = GetSubtitle();
                                        });
                                    }
                                },
                                error =>
                                {
                                    if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST)
                                        && TLRPCError.TypeEquals(error, ErrorType.CHAT_ADMIN_REQUIRED))
                                    {
                                        channel.ChannelParticipants = null;
                                        FileUtils.Delete(_participantsSyncRoot, channel.ChannelParticipantsFileName);
                                    }
                                });
                        }
                        else
                        {
                            FileUtils.Delete(_participantsSyncRoot, channel.ChannelParticipantsFileName);
                        }

                        BeginOnUIThread(() =>
                        {
                            var channel49 = channel as TLChannel49;
                            if (channel49 != null)
                            {
                                ShowPinnedMessage(channel49);
                            }

                            NotifyOfPropertyChange(() => HasBots);
                            NotifyOfPropertyChange(() => With);
                            Subtitle = GetSubtitle();

                            if (channel.IsKicked)
                            {
                                DeleteChannelInternal(channel.Id);
                            }
                        });
                    },
                    error => BeginOnUIThread(() =>
                    {
                        if (error.TypeEquals(ErrorType.CHANNEL_PRIVATE))
                        {
                            DeleteChannelInternal(channel.Id);
                        }
                    }));
            }
            else if (Peer is TLInputPeerBroadcast)
            {
                return;
            }
            else if (Peer is TLInputPeerChat)
            {
                var chat = With as TLChatBase;
                if (chat == null)
                {
                    return;
                }

                MTProtoService.GetFullChatAsync(chat.Id,
                    result =>
                    {
                        var newUsersCache = new Dictionary<int, TLUserBase>();
                        foreach (var user in result.Users)
                        {
                            newUsersCache[user.Index] = user;
                        }

                        chat.ChatPhoto = result.FullChat.ChatPhoto;
                        chat.Participants = result.FullChat.Participants;
                        chat.NotifySettings = result.FullChat.NotifySettings;
                        var chatFull28 = result.FullChat as TLChatFull28;
                        if (chatFull28 != null)
                        {
                            chat.ExportedInvite = chatFull28.ExportedInvite;
                        }
                        var chatFull31 = result.FullChat as TLChatFull31;
                        if (chatFull31 != null)
                        {
                            chat.BotInfo = chatFull31.BotInfo;
                            _cachedCommandResults.Clear();
                            _commands = null;
                            foreach (var botInfoBase in chatFull31.BotInfo)
                            {
                                var botInfo = botInfoBase as TLBotInfo;
                                if (botInfo != null)
                                {
                                    TLUserBase user;
                                    if (newUsersCache.TryGetValue(botInfo.UserId.Value, out user))
                                    {
                                        user.BotInfo = botInfo;
                                    }
                                }
                            }
                        }

                        var participants = result.FullChat.Participants as IChatParticipants;
                        if (participants != null)
                        {
                            var onlineUsers = 0;
                            foreach (var participant in participants.Participants)
                            {
                                var user = newUsersCache[participant.UserId.Value];
                                if (user.Status is TLUserStatusOnline)
                                {
                                    onlineUsers++;
                                }
                            }
                            chat.UsersOnline = onlineUsers;
                        }

                        BeginOnUIThread(() =>
                        {
                            NotifyOfPropertyChange(() => HasBots);
                            NotifyOfPropertyChange(() => With);
                            Subtitle = GetSubtitle();
                        });
                    });
            }
            else
            {
                var user = With as TLUserBase;
                if (user == null)
                {
                    return;
                }

                MTProtoService.GetFullUserAsync(user.ToInputUser(),
                    userFull =>
                    {
                        user.Link = userFull.Link;
                        user.ProfilePhoto = userFull.ProfilePhoto;
                        user.NotifySettings = userFull.NotifySettings;
                        user.Blocked = userFull.Blocked;
                        var userFull31 = userFull as TLUserFull31;
                        if (userFull31 != null)
                        {
                            user.BotInfo = userFull31.BotInfo;
                        }

                        var userFull49 = userFull as TLUserFull49;
                        if (userFull49 != null)
                        {
                            var user45 = user as TLUser45;
                            if (user45 != null) user45.About = userFull49.About;
                        }

                        var userFull58 = userFull as TLUserFull58;
                        if (userFull58 != null)
                        {
                            var user45 = user as TLUser45;
                            if (user45 != null) user45.CommonChatsCount = userFull58.CommonChatsCount;
                        }
                        BeginOnUIThread(() =>
                        {
                            NotifyOfPropertyChange(() => HasBots);
                            NotifyOfPropertyChange(() => With);
                            Subtitle = GetSubtitle();
                        });
                    });
            }
        }

        private void SendLogs()
        {
            if (StateService.LogFileName != null)
            {
                var logFileName = StateService.LogFileName;

#if WP8
                Execute.BeginOnThreadPool(async () =>
                {
                    var file = await GetFileFromLocalFolder(logFileName);
                    if (file != null)
                    {
                        SendDocument(file);
                    }
                });
#else

                SendDocument(logFileName);
#endif

                StateService.LogFileName = null;
            }
        }

        private async void SendSharedPhoto()
        {
            if (!string.IsNullOrEmpty(StateService.FileId))
            {
                var fileId = StateService.FileId;
                StateService.FileId = null;

                BeginOnUIThread(() =>
                {
                    var result = MessageBox.Show(AppResources.ForwardeMessageToThisChat, AppResources.Confirm, MessageBoxButton.OKCancel);
                    if (result != MessageBoxResult.OK) return;

                    BeginOnThreadPool(() =>
                    {
                        // Retrieve the photo from the media library using the FileID passed to the app.
                        var library = new MediaLibrary();
                        var photoFromLibrary = library.GetPictureFromToken(fileId);
                        var image = photoFromLibrary.GetImage();
                        var stream = new MemoryStream((int)image.Length);
                        image.CopyTo(stream);
                        var photo = new Photo
                        {
                            FileName = photoFromLibrary.Name,
                            Bytes = stream.ToArray(),
                            Width = photoFromLibrary.Width,
                            Height = photoFromLibrary.Height
                        };

                        SendPhoto(photo);
                    });
                });
            }
            else if (StateService.StorageItems != null)
            {
                var imageFiles = new List<StorageFile>();
                var videoFiles = new List<StorageFile>();
                foreach (var storageItem in StateService.StorageItems)
                {
                    var storageFile = storageItem as StorageFile;
                    if (storageFile != null)
                    {
                        var contentType = storageFile.ContentType;
                        if (string.Equals(contentType, "image/jpeg")
                            || string.Equals(contentType, "image/png")
                            || string.Equals(contentType, "image/bmp")
                            || string.Equals(contentType, "image/x-windows-bmp"))
                        //var properties = await storageFile.GetBasicPropertiesAsync();
                        //var videoProperties = await storageFile.Properties.GetVideoPropertiesAsync();
                        //var imageProperties = await storageFile.Properties.GetImagePropertiesAsync();
                        {
                            imageFiles.Add(storageFile);
                        }
                        else if (string.Equals(contentType, "video/mp4"))
                        {
                            videoFiles.Add(storageFile);
                        }
                    }
                }
                StateService.StorageItems = null;

                if (imageFiles.Count > 0)
                {
                    SendPhoto(imageFiles.AsReadOnly());
                }
                else if (videoFiles.Count > 0)
                {
                    SendVideo(videoFiles[0]);
                }
            }
        }

        public static IEnumerable<TLMessageBase> UngroupEnumerator(IEnumerable<TLMessageBase> source)
        {
            foreach (var messageBase in source)
            {
                var message = messageBase as TLMessage73;
                if (message != null && message.GroupedId != null)
                {
                    var mediaGroup = message.Media as TLMessageMediaGroup;
                    if (mediaGroup != null)
                    {
                        for (var i = mediaGroup.Group.Count - 1; i >= 0; i--)
                        {
                            yield return mediaGroup.Group[i];
                        }
                    }
                    else
                    {
                        yield return messageBase;
                    }
                }
                else
                {
                    yield return messageBase;
                }
            }
        }

        public TLMessagesContainter GetForwardMessagesContainer(List<TLMessageBase> messages, bool withMyScore, TLPeerBase toPeer = null)
        {
            TLLong groupedId = null;
            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i] as TLMessage73;
                if (message != null && message.GroupedId != null)
                {
                    if (i == 0)
                    {
                        groupedId = message.GroupedId;
                    }
                    else if (groupedId == null || groupedId.Value != message.GroupedId.Value)
                    {
                        groupedId = null;
                        break;
                    }
                }
                else
                {
                    groupedId = null;
                    break;
                }
            }

            var fwdMessages25 = new TLVector<TLMessage25>();
            foreach (var messageBase in UngroupEnumerator(messages))
            {
                var message = messageBase as TLMessage;
                if (message == null) continue;
                if (message.HasTTL()) continue;
                if (message.Index <= 0) continue;

                var fwdFromId = message.FromId;

                var messageForwarded = message as TLMessageForwarded;
                if (messageForwarded != null)
                {
                    fwdFromId = messageForwarded.FwdFromId;
                }

                var message25 = message as TLMessage25;
                if (message25 != null)
                {
                    if (message25.FwdFromId != null && message25.FwdFromId.Value != 0)
                    {
                        fwdFromId = message25.FwdFromId;
                    }
                }

                TLReplyKeyboardBase replyMarkup = null;
                var message31 = message as TLMessage31;
                if (message31 != null)
                {
                    if (message31.ReplyMarkup != null)
                    {
                        replyMarkup = message31.ReplyMarkup;
                    }
                }

                TLPeerBase fwdFromPeer = null;
                var message40 = message as TLMessage40;
                if (message40 != null)
                {
                    if (message40.FwdFromPeer != null)
                    {
                        fwdFromPeer = message40.FwdFromPeer;
                    }
                }

                TLMessageFwdHeader fwdHeader = null;
                var message48 = message as TLMessage48;
                if (message48 != null)
                {
                    if (message48.FwdHeader != null)
                    {
                        fwdHeader = message48.FwdHeader;
                    }
                }

                var fwdMessage = GetMessage(message.Message, GetForwardedMedia(message.Media)) as TLMessage73;
                if (fwdMessage == null) continue;

                if (toPeer != null)
                {
                    fwdMessage.ToId = toPeer;
                }

                if (replyMarkup != null)
                {
                    fwdMessage.ReplyMarkup = replyMarkup;
                }

                if (fwdHeader != null)
                {
                    fwdMessage.FwdHeader = fwdHeader;
                }
                else if (fwdFromPeer != null)
                {
                    fwdMessage.FwdHeader = GetFwdHeader(fwdFromPeer, message);
                    //fwdMessage.FwdFromPeer = fwdFromPeer;
                }
                else if (fwdFromId != null && fwdFromId.Value <= 0)
                {
                    fwdMessage.FwdHeader = GetFwdHeader(message.ToId, message);
                    //fwdMessage.FwdFromPeer = message.ToId;
                }
                else if (message.ToId is TLPeerChannel)
                {
                    var channel = CacheService.GetChat(message.ToId.Id) as TLChannel;
                    if (channel != null && channel.IsMegaGroup)
                    {
                        // megagroup
                        fwdMessage.FwdHeader = new TLMessageFwdHeader73 { Flags = new TLInt(0), FromId = message.FromId, Date = message.Date };
                        //fwdMessage.FwdFromPeer = new TLPeerUser { Id = message.FromId };
                    }
                    else
                    {
                        // channel
                        fwdMessage.FwdHeader = new TLMessageFwdHeader73 { Flags = new TLInt(0), FromId = message.FromId, Date = message.Date };

                        if (channel.IsBroadcast)
                        {
                            fwdMessage.FwdHeader.ChannelId = channel.Id;
                            fwdMessage.FwdHeader.ChannelPost = message.Id;
                        }
                    }
                }
                else
                {
                    fwdMessage.FwdHeader = new TLMessageFwdHeader73 { Flags = new TLInt(0), FromId = fwdFromId, Date = message.Date };
                    //fwdMessage.FwdFromPeer = new TLPeerUser{ Id = fwdFromId };
                }
                fwdMessage.FwdMessageId = message.Id;
                //fwdMessage.FwdDate = message.Date;
                fwdMessage.SetFwd();

                if (message.ToId is TLPeerChannel)
                {
                    var fromChannel = CacheService.GetChat(message.ToId.Id) as TLChannel;
                    if (fromChannel != null)
                    {
                        fwdMessage.FwdFromChannelPeer = fromChannel.ToInputPeer();
                    }

                    // author to fwd_from
                    if (fromChannel.IsBroadcast && message.FromId != null && message.FromId.Value >= 0)
                    {
                        fwdMessage.FwdHeader.FromId = message.FromId;
                    }
                }
                else if (message.ToId is TLPeerChat)
                {
                    var fromChat = CacheService.GetChat(message.ToId.Id);
                    if (fromChat != null)
                    {
                        fwdMessage.FwdFromChannelPeer = fromChat.ToInputPeer();
                    }
                }
                else
                {
                    var fromUser = CacheService.GetUser(message.Out.Value ? message.ToId.Id : message.FromId);
                    if (fromUser != null)
                    {
                        fwdMessage.FwdFromChannelPeer = fromUser.ToInputPeer();
                    }
                }

                if (fwdFromId != null
                    && fwdFromId.Value == StateService.CurrentUserId
                    && toPeer is TLPeerUser
                    && toPeer.Id.Value == StateService.CurrentUserId
                    && fwdMessage.FwdHeader != null
                    && fwdMessage.FwdHeader.ChannelId == null
                    && fwdMessage.FwdHeader.ChannelPost == null)
                {
                    fwdMessage.FwdHeader = null;
                }

                var message34 = message as TLMessage34;
                if (message34 != null)
                {
                    fwdMessage.Entities = message48.Entities;
                }

                if (message40 != null)
                {
                    fwdMessage.Views = message40.Views;
                }

                if (message48 != null)
                {
                    fwdMessage.ViaBotId = message48.ViaBotId;
                }

                var message73 = message as TLMessage73;
                if (message73 != null)
                {
                    fwdMessage.GroupedId = groupedId;
                }

                var mediaAudio = fwdMessage.Media as TLMessageMediaDocument45;
                if (mediaAudio != null && message.IsVoice())
                {
                    var fwdMediaAudio = new TLMessageMediaDocument75
                    {
                        Flags = new TLInt(0),
                        Document = mediaAudio.Document,
                        Caption = TLString.Empty,
                        IsoFileName = mediaAudio.IsoFileName
                    };

                    var channel = With as TLChannel;
                    fwdMediaAudio.NotListened = channel == null;
                    fwdMessage.NotListened = channel == null;

                    fwdMessage._media = fwdMediaAudio;
                }

                var mediaGame = fwdMessage.Media as TLMessageMediaGame;
                if (mediaGame != null)
                {
                    mediaGame.SourceMessage = fwdMessage;
                }

                fwdMessages25.Add(fwdMessage);
            }

            var container = new TLMessagesContainter
            {
                FwdMessages = fwdMessages25,
                WithMyScore = withMyScore
            };

            return container;
        }

        private void SendForwardMessages()
        {
            if (With is TLBroadcastChat && !(With is TLChannel)) return;

            var withMyScore = StateService.WithMyScore;
            StateService.WithMyScore = false;

            var messages = StateService.ForwardMessages;
            StateService.ForwardMessages = null;

            if (messages != null)
            {
                var container = GetForwardMessagesContainer(messages, withMyScore);

                if (container.FwdMessages.Count > 0)
                {
                    Reply = container;
                }
            }
        }

        private TLMessageMediaBase GetForwardedMedia(TLMessageMediaBase mediaBase)
        {
            var mediaGeoLive = mediaBase as TLMessageMediaGeoLive;
            if (mediaGeoLive != null)
            {
                return mediaGeoLive.ToMediaGeo();
            }

            return mediaBase;
        }

        private TLMessageFwdHeader GetFwdHeader(TLPeerBase peer, TLMessageCommon message)
        {
            var peerChannel = peer as TLPeerChannel;
            if (peerChannel != null)
            {
                var fwdHeader = new TLMessageFwdHeader73
                {
                    Flags = new TLInt(0),
                    ChannelId = peerChannel.Id,
                    ChannelPost = message.Id,
                    Date = message.Date
                };

                return fwdHeader;
            }

            return new TLMessageFwdHeader
            {
                Flags = new TLInt(0),
                FromId = peer.Id,
                Date = message.Date
            };
        }

        private static void SendForwardedMessages(IMTProtoService mtProtoService, TLInputPeerBase peer, TLMessageBase message)
        {
            var messagesContainer = message.Reply as TLMessagesContainter;
            if (messagesContainer != null)
            {
                SendForwardMessagesInternal(mtProtoService, peer, null, messagesContainer.FwdMessages, messagesContainer.WithMyScore);

                message.Reply = null;
            }
        }

        private void SendForwardMessageInternal(TLInt fwdMessageId, TLMessage25 message)
        {
            MTProtoService.ForwardMessageAsync(
                Peer, fwdMessageId,
                message,
                result =>
                {
                    message.Status = MessageStatus.Confirmed;
                },
                error => BeginOnUIThread(() =>
                {
                    if (error.TypeEquals(ErrorType.PEER_FLOOD))
                    {
                        //MessageBox.Show(AppResources.PeerFloodSendMessage, AppResources.Error, MessageBoxButton.OK);
                        ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodSendMessage, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                            result =>
                            {
                                if (result == CustomMessageBoxResult.RightButton)
                                {
                                    TelegramViewBase.NavigateToUsername(MTProtoService, Constants.SpambotUsername, null, null, null);
                                }
                            });
                    }
                    else
                    {
                        Execute.ShowDebugMessage("messages.forward error " + error);
                    }
                    Status = string.Empty;
                    if (message.Status == MessageStatus.Sending)
                    {
                        message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                    }
                }));
        }

        public static void SendForwardMessagesInternal(IMTProtoService mtProtoService, TLInputPeerBase toPeer, TLMessage25 comment, IList<TLMessage25> messages, bool withMyScore = false)
        {
            var noneChannelMessages = new List<TLMessage25>();
            var noneChannelMessagesId = new TLVector<TLInt>();
            var channelMessages = new List<TLMessage25>();
            var channelMessagesId = new TLVector<TLInt>();
            var messagesId = new TLVector<TLInt>();
            foreach (TLMessage40 message in messages)
            {
                messagesId.Add(message.FwdMessageId);

                var message48 = message as TLMessage48;
                if (message48 != null && message48.FwdHeader != null && message48.FwdHeader.ChannelId != null && message48.FwdHeader.ChannelPost != null)
                {
                    channelMessages.Add(message);
                    channelMessagesId.Add(message.FwdMessageId);
                }
                else if (message.FwdFromPeer is TLPeerChannel || message.FwdFromChannelPeer != null)
                {
                    channelMessages.Add(message);
                    channelMessagesId.Add(message.FwdMessageId);
                }
                else
                {
                    noneChannelMessages.Add(message);
                    noneChannelMessagesId.Add(message.FwdMessageId);
                }
            }

            if (noneChannelMessages.Count > 0 && channelMessages.Count > 0)
            {
                ForwardMessagesAsync(mtProtoService, toPeer, comment, noneChannelMessagesId, noneChannelMessages, withMyScore,
                    () => ForwardMessagesAsync(mtProtoService, toPeer, null, channelMessagesId, channelMessages, withMyScore));

                return;
            }

            ForwardMessagesAsync(mtProtoService, toPeer, comment, messagesId, messages, withMyScore);
        }

        private static void ForwardMessagesAsync(IMTProtoService mtProtoService, TLInputPeerBase toPeer, TLMessage25 comment, TLVector<TLInt> fwdMessageIds, IList<TLMessage25> messages, bool withMyScore, System.Action callback = null)
        {
            mtProtoService.ForwardMessagesAsync(
                comment,
                toPeer,
                fwdMessageIds,
                messages,
                withMyScore,
                result => Execute.BeginOnUIThread(() =>
                {
                    var firstMessage = messages.FirstOrDefault() as TLMessage73;
                    if (firstMessage != null && firstMessage.GroupedId != null)
                    {
                        IoC.Get<ITelegramEventAggregator>().Publish(new ForwardGroupedEventArgs { GroupedId = firstMessage.GroupedId, Messages = messages });
                    }

                    foreach (var message in messages)
                    {
                        var mediaGame = message.Media as TLMessageMediaGame;
                        if (mediaGame != null)
                        {
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.Message);
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.MessageVisibility);
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.DescriptionVisibility);

                            var message31 = message as TLMessage31;
                            if (message31 != null)
                            {
                                message31.NotifyOfPropertyChange(() => message31.ReplyMarkup);
                            }
                            var message48 = message as TLMessage48;
                            if (message48 != null)
                            {
                                message48.NotifyOfPropertyChange(() => message48.FwdFromPeerVisibility);
                            }
                            message.NotifyOfPropertyChange(() => message.ViaBotVisibility);
                            message.NotifyOfPropertyChange(() => message.ViaBot);
                            message.NotifyOfPropertyChange(() => message.Message);
                        }
                    }

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error.TypeEquals(ErrorType.PEER_FLOOD))
                    {
                        //MessageBox.Show(AppResources.PeerFloodSendMessage, AppResources.Error, MessageBoxButton.OK);
                        ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodSendMessage, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                            result =>
                            {
                                if (result == CustomMessageBoxResult.RightButton)
                                {
                                    TelegramViewBase.NavigateToUsername(mtProtoService, Constants.SpambotUsername, null, null, null);
                                }
                            });
                    }
                    else
                    {
                        Execute.ShowDebugMessage("messages.forwardMessages error " + error);
                    }
                    foreach (var message in messages)
                    {
                        if (message.Status == MessageStatus.Sending)
                        {
                            message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                        }
                    }
                }));
        }

        private bool _isOnline;

        public string RandomParam;

        private bool _isActive;

        protected override void OnActivate()
        {
            ShellViewModel.WriteTimer("DialogDetailsViewModel start OnActivate");

            base.OnActivate();

            ShellViewModel.WriteTimer("DialogDetailsViewModel stop OnActivate");
        }

        public void OnInvokeCommand(object sender, TelegramCommandEventArgs e)
        {
            var commandIndex = e.Command.LastIndexOf('/');
            if (commandIndex != -1)
            {
                var command = e.Command.Substring(commandIndex);

                if (With is TLChatBase)
                {
                    var message31 = e.Message as TLMessage31;
                    if (message31 == null && e.Media != null)
                    {
                        message31 = Items.FirstOrDefault(x => x is TLMessage31 && ((TLMessage31)x).Media == e.Media) as TLMessage31;
                    }

                    if (message31 != null && !message31.Out.Value)
                    {
                        var user = CacheService.GetUser(message31.FromId) as TLUser;
                        if (user != null && user.IsBot && !command.Contains("@") && (!IsSingleBot || IsChannel))
                        {
                            command += string.Format("@{0}", user.UserName);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(command))
                {
                    _text = command;
                    SendInternal(false, true);
                }
            }
        }

        public void OnOpenGame(object sender, TelegramGameEventArgs e)
        {
            var serviceMessage = e.Message as TLMessageService;
            if (serviceMessage == null) return;

            OpenReply(serviceMessage);

            return;

            var messageGameScoreAction = serviceMessage.Action as TLMessageActionGameScore;
            if (messageGameScoreAction != null)
            {
                var game = ServiceMessageToTextConverter.GetGame(serviceMessage);
                if (game != null)
                {
                    var bot = GetBot(serviceMessage.Reply);
                    if (bot == null) return;

                    OpenGame(serviceMessage.Reply, bot, game);
                    return;
                }
            }
        }

        public void OnSearchHashtag(object sender, TelegramHashtagEventArgs e)
        {
            var hashtagIndex = e.Hashtag.IndexOf('#');
            if (hashtagIndex != -1)
            {
                var hashtag = e.Hashtag.Substring(hashtagIndex);

                if (!string.IsNullOrEmpty(hashtag))
                {
                    TelegramViewBase.NavigateToHashtag(hashtag);
                }
            }
        }

        public void OnMentionNavigated(object sender, TelegramMentionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Mention))
            {
                var usernameStartIndex = e.Mention.LastIndexOf("@", StringComparison.OrdinalIgnoreCase);
                if (usernameStartIndex != -1)
                {
                    var username = e.Mention.Substring(usernameStartIndex).TrimStart('@');

                    if (!string.IsNullOrEmpty(username))
                    {
                        TelegramViewBase.NavigateToUsername(MTProtoService, username, string.Empty, string.Empty, string.Empty, PageKind.Profile);
                    }
                }
            }
            else if (e.UserId > 0)
            {
                var user = CacheService.GetUser(new TLInt(e.UserId));
                if (user != null)
                {
                    TelegramViewBase.NavigateToUser(user, null, PageKind.Profile);
                }
            }
            else if (e.ChatId > 0)
            {
                var chat = CacheService.GetChat(new TLInt(e.ChatId));
                if (chat != null)
                {
                    TelegramViewBase.NavigateToChat(chat, string.Empty);
                }
            }
            else if (e.ChannelId > 0)
            {
                var channel = CacheService.GetChat(new TLInt(e.ChannelId)) as TLChannel;
                if (channel != null)
                {
                    TelegramViewBase.NavigateToChat(channel, string.Empty);
                }
            }
        }

        public static void OnTelegramLinkActionCommon(IMTProtoService mtProtoService, IStateService stateService, TelegramEventArgs e, IUserName openedUsername, Action<string> callback)
        {
            if (e.Uri.Contains("joinchat"))
            {
                var hashStartIndex = e.Uri.TrimEnd('/').LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (hashStartIndex != -1)
                {
                    var hash = e.Uri.Substring(hashStartIndex).Replace("/", string.Empty);

                    if (!string.IsNullOrEmpty(hash))
                    {
                        TelegramViewBase.NavigateToInviteLink(mtProtoService, hash);
                    }
                }

                return;
            }

            if (e.Uri.Contains("addstickers"))
            {
                var shortNameStartIndex = e.Uri.TrimEnd('/').LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                if (shortNameStartIndex != -1)
                {
                    var shortName = e.Uri.Substring(shortNameStartIndex).Replace("/", string.Empty);

                    if (!string.IsNullOrEmpty(shortName))
                    {
                        var inputStickerSet = new TLInputStickerSetShortName { ShortName = new TLString(shortName) };
                        TelegramViewBase.NavigateToStickers(mtProtoService, stateService, inputStickerSet);
                    }
                }

                return;
            }

            var tempUri = HttpUtility.UrlDecode(e.Uri);

            Dictionary<string, string> uriParams = null;
            try
            {
                uriParams = TelegramUriMapper.ParseQueryString(tempUri);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage("Parse uri exception " + tempUri + ex);
            }
            PageKind pageKind;
            var accessToken = TelegramViewBase.GetAccessToken(uriParams, out pageKind);
            var post = TelegramViewBase.GetPost(uriParams);
            var game = TelegramViewBase.GetGame(uriParams);

            var uri = e.Uri.StartsWith("https://") || e.Uri.StartsWith("http://") ? e.Uri : "https://" + e.Uri;
            Uri telegramUri;
            if (Uri.TryCreate(uri, UriKind.Absolute, out telegramUri))
            {
                var segments = telegramUri.Segments;
                if (segments.Length >= 2)
                {
                    var username = segments[1].Replace("/", string.Empty);

                    if (string.IsNullOrEmpty(post))
                    {
                        if (segments.Length >= 3)
                        {
                            post = segments[2].Replace("/", string.Empty);
                        }
                    }
                    if (!string.IsNullOrEmpty(username))
                    {
                        if (string.Equals(username, "confirmphone", StringComparison.OrdinalIgnoreCase))
                        {
                            var phone = TelegramViewBase.GetPhone(uriParams);
                            var hash = TelegramViewBase.GetHash(uriParams);

                            TelegramViewBase.NavigateToConfirmPhone(mtProtoService, phone, hash);

                            return;
                        }
                        if (string.Equals(username, "socks", StringComparison.OrdinalIgnoreCase))
                        {
                            var server = TelegramViewBase.GetParam("server", uriParams);
                            var port = TelegramViewBase.GetParam("port", uriParams);
                            var user = TelegramViewBase.GetParam("user", uriParams);
                            var pass = TelegramViewBase.GetParam("pass", uriParams);

                            if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(port))
                            {
                                int portInt;
                                if (int.TryParse(port, out portInt)
                                    && portInt >= 0)
                                {
                                    TelegramViewBase.NavigateToSocksProxy(server, portInt, user, pass);
                                }
                            }

                            return;
                        }
                        if (string.Equals(username, "proxy", StringComparison.OrdinalIgnoreCase))
                        {
                            var server = TelegramViewBase.GetParam("server", uriParams);
                            var port = TelegramViewBase.GetParam("port", uriParams);
                            var secret = TelegramViewBase.GetParam("secret", uriParams);

                            if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(port))
                            {
                                int portInt;
                                if (int.TryParse(port, out portInt)
                                    && portInt >= 0
                                    && !string.IsNullOrEmpty(secret))
                                {
                                    TelegramViewBase.NavigateToMTProtoProxy(server, portInt, secret);
                                }
                            }

                            return;
                        }
                        //if (openedUsername != null
                        //    && TLString.Equals(new TLString(username), openedUsername.UserName, StringComparison.OrdinalIgnoreCase))
                        //{
                        //    callback.SafeInvoke(post);
                        //}
                        //else
                        {
                            TelegramViewBase.NavigateToUsername(mtProtoService, username, accessToken, post, game, pageKind);
                        }
                    }
                }
            }
        }

        public void OnTelegramLinkAction(object sender, TelegramEventArgs e)
        {
            OnTelegramLinkActionCommon(MTProtoService, StateService, e, With as IUserName,
                postString =>
                {
                    var channel = With as TLChannel;
                    if (channel != null)
                    {
                        int post;
                        if (int.TryParse(postString, out post))
                        {
                            OpenMessage(null, new TLInt(post));
                        }
                    }
                });
        }

        private bool _disableWatching;

        protected override void OnDeactivate(bool close)
        {
            ShellViewModel.WriteTimer("DialogDetailsViewModel start OnDeactivate");
            InputTypingManager.Stop();

            SaveUnsendedTextAsync(_text);

            if (_watcher != null)
            {
                _watcher.Stop();
                _disableWatching = true;
            }

            base.OnDeactivate(close);

            ShellViewModel.WriteTimer("DialogDetailsViewModel stop OnDeactivate");
        }

        private Typing GetTyping(IList<Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>> typingUsers)
        {
            return DialogsViewModel.GetTyping(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), typingUsers, CacheService.GetUser, null);
        }

        private string GetSubtitle()
        {
            Typing = null;

            var channel = With as TLChannel;
            if (channel != null)
            {
                if (channel.IsMegaGroup)
                {
                    System.Diagnostics.Debug.WriteLine("channel_participants=" + channel.ChannelParticipants + " participants_count=" + channel.ParticipantsCount);

                    if (channel.ChannelParticipants != null)
                    {
                        var config = CacheService.GetConfig();
                        if (config != null
                            && channel.ChannelParticipants.Count.Value <= config.ChatSizeMax.Value)
                        {
                            var participantsCount = channel.ChannelParticipants.Count.Value;
                            var onlineCount = channel.ChannelParticipants.Users.Count(x => x.Status is TLUserStatusOnline);
                            var onlineString = onlineCount > 0 ? string.Format(", {0} {1}", onlineCount, AppResources.Online.ToLowerInvariant()) : string.Empty;

                            var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
                            var isCurrentUserOnline = currentUser != null && currentUser.Status is TLUserStatusOnline;
                            if (participantsCount == 1 || (onlineCount == 1 && isCurrentUserOnline))
                            {
                                onlineString = string.Empty;
                            }

                            return Language.Declension(
                                participantsCount,
                                AppResources.CompanyNominativeSingular,
                                AppResources.CompanyNominativePlural,
                                AppResources.CompanyGenitiveSingular,
                                AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture)
                                + onlineString;
                        }
                    }

                    if (channel.ParticipantsCount != null)
                    {
                        return Language.Declension(
                            channel.ParticipantsCount.Value,
                            AppResources.CompanyNominativeSingular,
                            AppResources.CompanyNominativePlural,
                            AppResources.CompanyGenitiveSingular,
                            AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                    }

                    return AppResources.Loading.ToLowerInvariant();
                }

                if (channel.ParticipantsCount != null)
                {
                    return Language.Declension(
                        channel.ParticipantsCount.Value,
                        AppResources.CompanyNominativeSingular,
                        AppResources.CompanyNominativePlural,
                        AppResources.CompanyGenitiveSingular,
                        AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                }

                return channel.IsPublic ? AppResources.PublicChannel.ToLowerInvariant() : AppResources.PrivateChannel.ToLowerInvariant();
            }

            var user = With as TLUserBase;
            if (user != null)
            {
                return GetUserStatus(user);
            }

            var chat = With as TLChat;
            if (chat != null)
            {
                var participantsCount = chat.ParticipantsCount.Value;
                var onlineCount = chat.UsersOnline;
                var onlineString = onlineCount > 0 ? string.Format(", {0} {1}", chat.UsersOnline, AppResources.Online.ToLowerInvariant()) : string.Empty;

                var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
                var isCurrentUserOnline = currentUser != null && currentUser.Status is TLUserStatusOnline;
                if (participantsCount == 1 || (onlineCount == 1 && isCurrentUserOnline))
                {
                    onlineString = string.Empty;
                }

                return Language.Declension(
                    participantsCount,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture)
                    + onlineString;
            }

            var forbiddenChat = With as TLChatForbidden;
            if (forbiddenChat != null)
            {
                return LowercaseConverter.Convert(AppResources.YouWhereKickedFromTheGroup);
            }

            var broadcastChat = With as TLBroadcastChat;
            if (broadcastChat != null)
            {
                var participantsCount = broadcastChat.ParticipantIds.Count;
                var onlineParticipantsCount = 0;
                foreach (var participantId in broadcastChat.ParticipantIds)
                {
                    var participant = CacheService.GetUser(participantId);
                    if (participant != null && participant.Status is TLUserStatusOnline)
                    {
                        onlineParticipantsCount++;
                    }
                }

                var onlineString = onlineParticipantsCount > 0 ? string.Format(", {0} {1}", onlineParticipantsCount, AppResources.Online.ToLowerInvariant()) : string.Empty;

                return Language.Declension(
                    participantsCount,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture)
                    + onlineString;
            }

            var channels = With as TLVector<TLChatBase>;
            if (channels != null)
            {
                return Language.Declension(
                    channels.Count,
                    AppResources.ChannelNominativeSingular,
                    AppResources.ChannelNominativePlural,
                    AppResources.ChannelGenitiveSingular,
                    AppResources.ChannelGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }

            return string.Empty;
        }

        public static string GetUserStatus(TLUserBase userBase)
        {
            if (userBase.Index == Constants.TelegramNotificationsId)
            {
                return AppResources.ServiceNotifications.ToLowerInvariant();
            }

            var user = userBase as TLUser;
            if (user != null && user.IsBot)
            {
                return AppResources.Bot.ToLowerInvariant();
            }

            if (userBase.IsSelf)
            {
                return AppResources.ChatWithYourself.ToLowerInvariant();
            }

            return UserStatusToStringConverter.Convert(userBase.Status);
        }

        public void ShowLastSyncErrors(Action<string> callback = null)
        {
            MTProtoService.GetSyncErrorsAsync((syncMessageError, processDifferenceErrors) =>
            {
                var info = new StringBuilder();

                info.AppendLine("syncMessage last error: ");
                info.AppendLine(syncMessageError == null ? "none" : syncMessageError.ToString());
                info.AppendLine();
                info.AppendLine("syncDifference last error: ");
                if (processDifferenceErrors == null || processDifferenceErrors.Count == 0)
                {
                    info.AppendLine("none");
                }
                else
                {
                    foreach (var processDifferenceError in processDifferenceErrors)
                    {
                        info.AppendLine(processDifferenceError.ToString());
                    }
                }

                var infoString = info.ToString();
                Execute.BeginOnUIThread(() => MessageBox.Show(infoString));

                callback.SafeInvoke(infoString);
            });
        }

        public void ShowMessagesInfo(int limit = 15, Action<string> callback = null)
        {
            MTProtoService.GetSendingQueueInfoAsync(queueInfo =>
            {
                var info = new StringBuilder();

                info.AppendLine("Queue: ");
                info.AppendLine(queueInfo);

                var dialogMessages = Items.Take(limit);
                info.AppendLine("Dialog: ");
                var count = 0;
                foreach (var dialogMessage in dialogMessages)
                {
                    info.AppendLine("  " + count++ + " " + dialogMessage);
                }

                dialogMessages = CacheService.GetHistory(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId), limit);
                info.AppendLine();
                info.AppendLine("Database: ");
                count = 0;
                foreach (var dialogMessage in dialogMessages)
                {
                    info.AppendLine("  " + count++ + " " + dialogMessage);
                }
                var infoString = info.ToString();
                Execute.BeginOnUIThread(() => MessageBox.Show(infoString));

                callback.SafeInvoke(infoString);
            });
        }

        private void ShowConfigInfo(Action<string> callback)
        {
            MTProtoService.GetConfigInformationAsync(callback.SafeInvoke);
        }

        private void ShowTransportInfo(Action<string> callback)
        {
            MTProtoService.GetTransportInformationAsync(callback.SafeInvoke);
        }

        public void OnNavigatedTo()
        {
            _isActive = true;
            StateService.ActiveDialog = With;

            var messagesContainer = Reply as TLMessagesContainter;
            if (messagesContainer != null && messagesContainer.EditMessage != null)
            {
                _editMessageTimer.Start();
            }
        }

        public void OnNavigatedFrom()
        {
            _isActive = false;
            StateService.ActiveDialog = null;

            if (_editMessageTimer != null) _editMessageTimer.Stop();
        }

        private InputTypingManager _inputTypingManager;

        public InputTypingManager InputTypingManager
        {
            get
            {
                return _inputTypingManager =
                    _inputTypingManager ??
                    new InputTypingManager(
                        users =>
                        {
                            var typing = GetTyping(users);
                            Subtitle = typing != null ? typing.ToString() : null;
                            Typing = typing;
                        },
                        () =>
                        {
                            Subtitle = GetSubtitle();
                            Typing = null;
                        });
            }
        }

        private OutputTypingManager _textTypingManager;

        public OutputTypingManager TextTypingManager
        {
            get
            {
                return _textTypingManager =
                    _textTypingManager ??
                    new OutputTypingManager(Peer, Constants.SetTypingIntervalInSeconds,
                        action => MTProtoService.SetTypingAsync(_peer, action ?? new TLSendMessageTypingAction(), result => { }),
                        () => MTProtoService.SetTypingAsync(_peer, new TLSendMessageCancelAction(), result => { }));
            }
        }

        private OutputTypingManager _audioTypingManager;

        public OutputTypingManager AudioTypingManager
        {
            get
            {
                return _audioTypingManager =
                    _audioTypingManager ??
                    new OutputTypingManager(Peer, Constants.SetTypingIntervalInSeconds,
                        action => MTProtoService.SetTypingAsync(_peer, action ?? new TLSendMessageRecordAudioAction(), result => { }),
                        () => MTProtoService.SetTypingAsync(_peer, new TLSendMessageCancelAction(), result => { }));
            }
        }

        private OutputTypingManager _uploadTypingManager;

        public OutputTypingManager UploadTypingManager
        {
            get
            {
                return _uploadTypingManager =
                    _uploadTypingManager ??
                    new OutputTypingManager(Peer, Constants.SetTypingIntervalInSeconds,
                        action => MTProtoService.SetTypingAsync(_peer, action ?? new TLSendMessageTypingAction(), result => { }),
                        () => MTProtoService.SetTypingAsync(_peer, new TLSendMessageCancelAction(), result => { }));
            }
        }

        public TLMessage34 GetMessage(TLString text, TLMessageMediaBase media)
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

        public void ReportSpam()
        {
            if (Peer is TLInputPeerBroadcast && !(Peer is TLInputPeerChannel))
            {
                return;
            }

            var spamConfirmation = MessageBox.Show("Are you sure you want to report spam?", AppResources.AppName,
                MessageBoxButton.OKCancel);
            if (spamConfirmation != MessageBoxResult.OK) return;

            IsWorking = true;
            MTProtoService.ReportSpamAsync(Peer,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var chat = With as TLChatBase;
                    if (chat != null)
                    {
                        var confirmation = MessageBox.Show(AppResources.GroupConversationMarkedAsSpamConfirmation, AppResources.AppName, MessageBoxButton.OKCancel);
                        if (confirmation != MessageBoxResult.OK)
                        {
                            return;
                        }

                        DialogsViewModel.DeleteAndExitDialogCommon(chat, MTProtoService,
                            () => BeginOnUIThread(() =>
                            {
                                var dialog = CacheService.GetDialog(new TLPeerChat { Id = chat.Id });
                                DeleteDialogContinueCommon(dialog, StateService, EventAggregator, CacheService, NavigationService);
                            }),
                            error => BeginOnUIThread(() =>
                            {
                                Execute.ShowDebugMessage("DeleteAndExitDialogCommon error " + error);
                            }));

                        return;
                    }

                    var user = With as TLUserBase;
                    if (user != null)
                    {
                        var confirmation = MessageBox.Show(AppResources.ConversationMarkedAsSpamConfirmation, AppResources.AppName, MessageBoxButton.OKCancel);
                        if (confirmation != MessageBoxResult.OK)
                        {
                            return;
                        }

                        IsWorking = true;
                        MTProtoService.BlockAsync(user.ToInputUser(),
                            blocked => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                user.Blocked = TLBool.True;
                                CacheService.Commit();

                                DialogsViewModel.DeleteDialogCommon(user, MTProtoService,
                                    () => BeginOnUIThread(() =>
                                    {
                                        var dialog = CacheService.GetDialog(new TLPeerUser { Id = user.Id });
                                        DeleteDialogContinueCommon(dialog, StateService, EventAggregator, CacheService, NavigationService);
                                    }),
                                    error => BeginOnUIThread(() =>
                                    {
                                        Execute.ShowDebugMessage("DeleteDialogCommon error " + error);
                                    }));
                            }),
                            error => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                Execute.ShowDebugMessage("contacts.block error " + error);
                            }));

                        return;
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                }));
        }

        public static void DeleteDialogContinueCommon(TLDialogBase dialog, IStateService stateService, ITelegramEventAggregator eventAggregator, ICacheService cacheService, INavigationService navigationService)
        {
            if (dialog != null)
            {
                eventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                cacheService.DeleteDialog(dialog);
                DialogsViewModel.UnpinFromStart(dialog);
            }

            ShellViewModel.Navigate(navigationService);
        }

        private IList<TLUserBase> _mentions;

        public void AddMention(TLUserBase user)
        {
            _mentions = _mentions ?? new List<TLUserBase>();

            _mentions.Add(user);
        }

        public void ClearMentions()
        {
            if (_mentions == null) return;

            _mentions.Clear();
        }
    }

    public class InputTypingManager
    {
        private readonly object _typingUsersSyncRoot = new object();

        private readonly Dictionary<int, Telegram.Api.WindowsPhone.Tuple<DateTime, TLSendMessageActionBase>> _typingUsersCache = new Dictionary<int, Telegram.Api.WindowsPhone.Tuple<DateTime, TLSendMessageActionBase>>();

        private readonly Timer _typingUsersTimer;

        private readonly System.Action<IList<Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>>> _typingCallback;

        private readonly System.Action _callback;

        public InputTypingManager(System.Action<IList<Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>>> typingCallback, System.Action callback)
        {
            _typingUsersTimer = new Timer(UpdateTypingUsersCache, null, Timeout.Infinite, Timeout.Infinite);
            _typingCallback = typingCallback;
            _callback = callback;
        }


        private void StartTypingTimer(int dueTime)
        {
            if (_typingUsersTimer != null)
            {
                _typingUsersTimer.Change(dueTime, Timeout.Infinite);
                //TLUtils.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " Start TypingTimer " + dueTime, LogSeverity.Error);
            }
        }

        private void StopTypingTimer()
        {
            if (_typingUsersTimer != null)
            {
                _typingUsersTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //TLUtils.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " Stop TypingTimer ", LogSeverity.Error);
            }
        }

        private void UpdateTypingUsersCache(object state)
        {
            //#if !WIN_RT && DEBUG
            //            Microsoft.Devices.VibrateController.Default.Start(TimeSpan.FromMilliseconds(50));
            //#endif
            var now = DateTime.Now;
            var nextTime = DateTime.MaxValue;

            var typingUsers = new List<Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>>();
            lock (_typingUsersSyncRoot)
            {
                if (_typingUsersCache.Count == 0) return;

                var keys = new List<int>(_typingUsersCache.Keys);
                foreach (var key in keys)
                {
                    if (_typingUsersCache[key].Item1 <= now)
                    {
                        _typingUsersCache.Remove(key);
                    }
                    else
                    {
                        if (nextTime > _typingUsersCache[key].Item1)
                        {
                            nextTime = _typingUsersCache[key].Item1;
                        }
                        typingUsers.Add(new Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>(key, _typingUsersCache[key].Item2));
                    }
                }
            }

            if (typingUsers.Count > 0)
            {
                StartTypingTimer((int)(nextTime - now).TotalMilliseconds);
                _typingCallback.SafeInvoke(typingUsers);
            }
            else
            {
                StopTypingTimer();
                _callback.SafeInvoke();
            }
        }

        public void Start()
        {
            StartTypingTimer(0);
        }

        public void Stop()
        {
            StopTypingTimer();
        }

        public void AddTypingUser(int userId, TLSendMessageActionBase action)
        {
            var now = DateTime.Now;
            var nextTime = DateTime.MaxValue;
            var typingUsers = new List<Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>>();
            //lock here
            lock (_typingUsersSyncRoot)
            {
                _typingUsersCache[userId] = new Telegram.Api.WindowsPhone.Tuple<DateTime, TLSendMessageActionBase>(TillDate(now, action), action);

                foreach (var keyValue in _typingUsersCache)
                {
                    if (keyValue.Value.Item1 > now)
                    {
                        if (nextTime > keyValue.Value.Item1)
                        {
                            nextTime = keyValue.Value.Item1;
                        }
                        typingUsers.Add(new Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>(keyValue.Key, keyValue.Value.Item2));
                    }
                }
            }

            if (typingUsers.Count > 0)
            {
                StartTypingTimer((int)(nextTime - now).TotalMilliseconds);
                _typingCallback.SafeInvoke(typingUsers);
            }
            else
            {
                _callback.SafeInvoke();
            }
        }

        private static DateTime TillDate(DateTime now, TLSendMessageActionBase action)
        {
            return now.AddSeconds(6.0);
        }

        public void RemoveTypingUser(int userId)
        {
            var typingUsers = new List<Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>>();
            //lock here
            lock (_typingUsersSyncRoot)
            {
                _typingUsersCache.Remove(userId);

                foreach (var keyValue in _typingUsersCache)
                {
                    if (keyValue.Value.Item1 > DateTime.Now)
                    {
                        typingUsers.Add(new Telegram.Api.WindowsPhone.Tuple<int, TLSendMessageActionBase>(keyValue.Key, keyValue.Value.Item2));
                    }
                }
            }

            if (typingUsers.Count > 0)
            {
                _typingCallback.SafeInvoke(typingUsers);
            }
            else
            {
                _callback.SafeInvoke();
            }
        }
    }

    public class OutputTypingManager
    {
        public OutputTypingManager(TLInputPeerBase peer, double delay, Action<TLSendMessageActionBase> sendTyping, System.Action cancelTyping)
        {
            _peer = peer;
            _delay = delay;
            _sendTyping = sendTyping;
            _cancelTyping = cancelTyping;
        }

        private readonly Action<TLSendMessageActionBase> _sendTyping;

        private readonly System.Action _cancelTyping;

        private readonly TLInputPeerBase _peer;

        private readonly double _delay;

        private DateTime? _lastTypingTime;

        public void SetTyping(TLSendMessageActionBase action = null)
        {
            if (_peer is TLInputPeerBroadcast && !(_peer is TLInputPeerChannel))
            {
                return;
            }

            if (_lastTypingTime.HasValue
                && _lastTypingTime.Value.AddSeconds(_delay) > DateTime.Now)
            {
                return;
            }

            _lastTypingTime = DateTime.Now;

            _sendTyping.SafeInvoke(action);
        }

        public void CancelTyping()
        {
            _lastTypingTime = null;

            _cancelTyping.SafeInvoke();
        }
    }

    public class TTLQueue
    {
        private readonly List<Tuple<TLMessage70, TTLParams, Action<TLMessage70>>> _items = new List<Tuple<TLMessage70, TTLParams, Action<TLMessage70>>>();

        private readonly Timer _timer;

        public TTLQueue()
        {
            _timer = new Timer(Timer_OnTick);
        }

        private void Timer_OnTick(object state)
        {
            Execute.BeginOnUIThread(() =>
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    if (item.Item2.StartTime.AddSeconds(item.Item2.Total) < DateTime.Now)
                    {
                        _items.RemoveAt(i--);
                        var message = item.Item1;
                        message.SetListened();
                        var mediaPhoto = message.Media as TLMessageMediaPhoto70;
                        if (mediaPhoto != null)
                        {
                            mediaPhoto.Photo = null;
                            message.NotifyOfPropertyChange(() => message.TTLMediaExpired);
                            item.Item3.SafeInvoke(message);
                        }
                        var mediaDocument = message.Media as TLMessageMediaDocument70;
                        if (mediaDocument != null)
                        {
                            mediaDocument.Document = null;
                            message.NotifyOfPropertyChange(() => message.TTLMediaExpired);
                            item.Item3.SafeInvoke(message);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (_items.Count > 0)
                {
                    SetTimer(_items[0]);
                }
            });
        }

        private void SetTimer(Tuple<TLMessage70, TTLParams, Action<TLMessage70>> item)
        {
            var timeSpan = item.Item2.StartTime.AddSeconds(item.Item2.Total) > DateTime.Now ? item.Item2.StartTime.AddSeconds(item.Item2.Total) - DateTime.Now : TimeSpan.FromSeconds(0.0);
            _timer.Change(timeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Add(TLMessage70 message, TTLParams ttlParams, Action<TLMessage70> callback)
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            var newItem = new Tuple<TLMessage70, TTLParams, Action<TLMessage70>>(message, ttlParams, callback);

            var added = false;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Item2.StartTime.AddSeconds(_items[i].Item2.Total) >= ttlParams.StartTime.AddSeconds(ttlParams.Total))
                {
                    _items.Insert(i, newItem);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                _items.Add(newItem);
            }

            SetTimer(_items[0]);
        }
    }

    public class ForwardGroupedEventArgs
    {
        public TLLong GroupedId { get; set; }

        public IList<TLMessage25> Messages { get; set; }
    }
}
