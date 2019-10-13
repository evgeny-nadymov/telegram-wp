// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Controls;
using Execute = Telegram.Api.Helpers.Execute;
using TaskResult = Microsoft.Phone.Tasks.TaskResult;

namespace TelegramClient.ViewModels.Chats
{
    public class EditChatViewModel : ItemDetailsViewModelBase, Telegram.Api.Aggregator.IHandle<UploadableItem>
    {
        public string TypeDescription
        {
            get { return !IsMegaGroup ? AppResources.ChannelType : AppResources.GroupType; }
        }

        public string PublicTypeDescription
        {
            get { return !IsMegaGroup ? AppResources.PublicChannel : AppResources.Public; }
        }

        public string PrivateTypeDescription
        {
            get { return !IsMegaGroup ? AppResources.PrivateChannel : AppResources.Private; }
        }

        public bool CanSetStickers
        {
            get
            {
                var channel = CurrentItem as TLChannel68;
                return channel != null && channel.CanSetStickers;
            }
        }

        private bool _notifyMembers = true;

        public bool NotifyMembers
        {
            get { return _notifyMembers; }
            set { SetField(ref _notifyMembers, value, () => NotifyMembers); }
        }

        private bool _signMessages;

        public bool SignMessages
        {
            get { return _signMessages; }
            set { SetField(ref _signMessages, value, () => SignMessages); }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { SetField(ref _title, value, () => Title); }
        }

        private string _about;

        public string About
        {
            get { return _about; }
            set { SetField(ref _about, value, () => About); }
        }

        public string DeleteChannelString
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && channel.IsMegaGroup
                    ? AppResources.DeleteGroup
                    : AppResources.DeleteChannel;
            }
        }

        public bool IsChannel { get { return CurrentItem is TLChannel; } }

        public bool IsMegaGroup 
        {
            get
            {
                var channel = CurrentItem as TLChannel44;
                return channel != null && channel.IsMegaGroup;
            }
        }

        public bool IsChannelAdmin
        {
            get
            {
                var channel = CurrentItem as TLChannel;
                return channel != null && channel.Creator;
            }
        }

        private bool _hiddenPrehistory;

        public bool HiddenPrehistory
        {
            get { return _hiddenPrehistory; }
            set { SetField(ref _hiddenPrehistory, value, () => HiddenPrehistory); }
        }

        private readonly IUploadFileManager _uploadManager;

        //public ObservableCollection<TLUserBase> Items { get; set; } 

        public EditChatViewModel(IUploadFileManager uploadManager, ICacheService cacheService,
            ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService,
            IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            _uploadManager = uploadManager;

            CurrentItem = StateService.CurrentChat;
            StateService.CurrentChat = null;
            var chat = CurrentItem as TLChat;
            if (chat != null)
            {
                Title = chat.Title.ToString();
            }
            var broadcastChat = CurrentItem as TLBroadcastChat;
            if (broadcastChat != null)
            {
                Title = broadcastChat.Title.ToString();
            }
            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                Title = channel.Title.ToString();
                About = channel.About != null ? channel.About.ToString() : string.Empty;
            }

            var channel44 = CurrentItem as TLChannel44;
            if (channel44 != null)
            {
                _signMessages = channel44.Signatures;
            }

            var channel68 = CurrentItem as TLChannel68;
            if (channel68 != null)
            {
                _hiddenPrehistory = channel68.HiddenPrehistory;
            }

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => SignMessages))
                {
                    if (channel44 == null) return;

                    MTProtoService.ToggleSignaturesAsync(channel44.ToInputChannel(), new TLBool(SignMessages),
                        result => Execute.BeginOnUIThread(() =>
                        {

                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            Execute.ShowDebugMessage("channels.toggleComments error " + error);
                        }));
                }
                else if (Property.NameEquals(args.PropertyName, () => HiddenPrehistory))
                {
                    if (channel44 == null) return;

                    MTProtoService.TogglePreHistoryHiddenAsync(channel44.ToInputChannel(), new TLBool(HiddenPrehistory),
                        result => Execute.BeginOnUIThread(() =>
                        {
                            MTProtoService.GetFullChannelAsync(channel44.ToInputChannel(), result2 =>
                            {
                                
                            });
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            Execute.ShowDebugMessage("channels.toggleComments error " + error);
                        }));
                }
            };
        }


        public void ForwardInAnimationComplete()
        {

        }

        public void EditPublicLink()
        {
            StateService.CurrentChat = CurrentItem as TLChatBase;
            NavigationService.UriFor<EditGroupTypeViewModel>().Navigate();
        }

        public void Done()
        {
            if (IsWorking) return;

            var chat = CurrentItem as TLChat;
            if (chat != null)
            {
                IsWorking = true;
                MTProtoService.EditChatTitleAsync(chat.Id, new TLString(Title),
                    statedMessage =>
                    {
                        IsWorking = false;

                        var updates = statedMessage as TLUpdates;
                        if (updates != null)
                        {
                            var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                            if (updateNewMessage != null)
                            {
                                EventAggregator.Publish(updateNewMessage.Message);
                                BeginOnUIThread(() => NavigationService.GoBack());
                            }
                        }

                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.editChatTitle error " + error);

                        IsWorking = false;
                        BeginOnUIThread(() => NavigationService.GoBack());
                    });
                return;
            }

            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                EditChannelAboutAsync(channel, new TLString(About),
                    () => EditChannelTitleAsync(channel, new TLString(Title), 
                        () => NavigationService.GoBack()));

                return;
            }

            var broadcastChat = CurrentItem as TLBroadcastChat;
            if (broadcastChat != null)
            {
                broadcastChat.Title = new TLString(Title);
                CacheService.SyncBroadcast(broadcastChat, result => BeginOnUIThread(() => NavigationService.GoBack()));
                return;
            }
        }

        public void DeleteChannel()
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || !channel.Creator) return;

            var confirmationString = channel.IsMegaGroup
                ? AppResources.DeleteGroupConfirmation
                : AppResources.DeleteChannelConfirmation;

            var confirmation = MessageBox.Show(confirmationString, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            IsWorking = true;
            MTProtoService.DeleteChannelAsync(channel, 
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    ContinueDeleteChannel(channel);
                }),
                error => BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("channels.deleteChannel error " + error);

                    IsWorking = false;

                    if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                        && error.TypeEquals(ErrorType.CHANNEL_PRIVATE))
                    {
                        ContinueDeleteChannel(channel);
                    }
                }));
        }

        private void ContinueDeleteChannel(TLChannel channel)
        {
            var dialog = CacheService.GetDialog(new TLPeerChannel {Id = channel.Id});
            
            if (dialog != null)
            {
                CacheService.DeleteDialog(dialog);
                DialogsViewModel.UnpinFromStart(dialog);
                EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
            }

            NavigationService.RemoveBackEntry();
            NavigationService.RemoveBackEntry();
            NavigationService.GoBack();
        }

        public void EditChannelTitleAsync(TLChannel channel, TLString title, System.Action callback)
        {
            if (TLString.Equals(title, channel.Title, StringComparison.Ordinal))
            {
                callback.SafeInvoke();
                return;
            }

            IsWorking = true;
            MTProtoService.EditTitleAsync(channel, title,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var updates = result as TLUpdates;
                    if (updates != null)
                    {
                        var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                        if (updateNewMessage != null)
                        {
                            EventAggregator.Publish(updateNewMessage.Message);
                            
                        }
                    } 

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("channels.editTitle error " + error);

                    IsWorking = false;

                    if (error.CodeEquals(ErrorCode.BAD_REQUEST) 
                        && error.TypeEquals(ErrorType.CHAT_NOT_MODIFIED))
                    {

                    } 
                    callback.SafeInvoke();
                }));
        }

        public void EditChannelAboutAsync(TLChannel channel, TLString about, System.Action callback)
        {
            if (TLString.Equals(about, channel.About, StringComparison.Ordinal))
            {
                callback.SafeInvoke();
                return;
            }

            IsWorking = true;
            MTProtoService.EditAboutAsync(channel, about,
                statedMessage => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    channel.About = about;
                    CacheService.Commit();

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("channels.editAbout error " + error);

                    IsWorking = false;

                    if (error.CodeEquals(ErrorCode.BAD_REQUEST) 
                        && error.TypeEquals(ErrorType.CHAT_ABOUT_NOT_MODIFIED))
                    {

                    }
                    callback.SafeInvoke();
                }));
        }

        public void ReplacePhoto()
        {
            if (CurrentItem is TLChat || CurrentItem is TLChannel)
            {
                EditChatActions.EditPhoto(photo =>
                {
                    var fileId = TLLong.Random();
                    IsWorking = true;
                    _uploadManager.UploadFile(fileId, CurrentItem, photo);
                });
            }
        }

        public void DeletePhoto()
        {
            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                //IsWorking = true;
                MTProtoService.EditPhotoAsync(channel, new TLInputChatPhotoEmpty(),
                    result => { },
                    error => Execute.BeginOnUIThread(() =>
                    {
                        //IsWorking = false;
                        Execute.ShowDebugMessage("channels.editPhoto error " + error);
                    }));
            }

            var chat = CurrentItem as TLChat;
            if (chat != null)
            {
                //IsWorking = true;
                MTProtoService.EditChatPhotoAsync(chat.Id, new TLInputChatPhotoEmpty(),
                    result => { },
                    error => Execute.BeginOnUIThread(() =>
                    {
                        //IsWorking = false;
                        Execute.ShowDebugMessage("messages.editChatPhoto error " + error);
                    }));
            }
        }

        public void AddManager()
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || channel.IsForbidden) return;

            StateService.IsInviteVisible = false;
            StateService.CurrentChat = channel;
            //StateService.RemovedUsers = Items;
            StateService.RequestForwardingCount = false;
            NavigationService.UriFor<AddChatParticipantViewModel>().Navigate();
        }

        public void GroupStickers()
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || channel.IsForbidden) return;

            StateService.CurrentChat = channel;
            NavigationService.UriFor<GroupStickersViewModel>().Navigate();
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }

        public void Handle(UploadableItem item)
        {
            if (item.Owner == CurrentItem)
            {
                IsWorking = false;
            }
        }
    }

    public static class EditChatActions
    {
        private static CropControl _cropControl;

        public static void EditPhoto(Action<byte[]> callback)
        {
            var photoPickerSettings = IoC.Get<IStateService>().GetPhotoPickerSettings();
            if (photoPickerSettings != null && photoPickerSettings.External)
            {
                var photoChooserTask = new PhotoChooserTask
                {
                    ShowCamera = true,
                    PixelHeight = 800,
                    PixelWidth = 800
                };

                photoChooserTask.Completed += (o, e) =>
                {
                    if (e.TaskResult == TaskResult.OK)
                    {
                        byte[] bytes;
                        var sourceStream = e.ChosenPhoto;
                        using (var memoryStream = new MemoryStream())
                        {
                            sourceStream.CopyTo(memoryStream);
                            bytes = memoryStream.ToArray();
                        }
                        callback.SafeInvoke(bytes);
                    }
                };

                photoChooserTask.Show();
            }
            else
            {
                ChooseAttachmentViewModel.OpenPhotoPicker(true, (result1, result2) =>
                {
                    Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.4), () =>
                    {
                        var frame = Application.Current.RootVisual as PhoneApplicationFrame;
                        PhoneApplicationPage page = null;
                        if (frame != null)
                        {
                            page = frame.Content as PhoneApplicationPage;
                            if (page != null)
                            {
                                var applicationBar = page.ApplicationBar;
                                if (applicationBar != null)
                                {
                                    applicationBar.IsVisible = false;
                                }
                            }
                        }

                        if (page == null) return;

                        var popup = new Popup();
                        var cropControl = new CropControl
                        {
                            Width = page.ActualWidth,
                            Height = page.ActualHeight
                        };
                        _cropControl = cropControl;
                        page.SizeChanged += PageOnSizeChanged;

                        cropControl.Close += (sender, args) =>
                        {
                            _cropControl = null;
                            popup.IsOpen = false;
                            popup.Child = null;

                            frame = Application.Current.RootVisual as PhoneApplicationFrame;
                            if (frame != null)
                            {
                                page = frame.Content as PhoneApplicationPage;
                                if (page != null)
                                {
                                    page.SizeChanged -= PageOnSizeChanged;
                                    var applicationBar = page.ApplicationBar;
                                    if (applicationBar != null)
                                    {
                                        applicationBar.IsVisible = true;
                                    }
                                }
                            }
                        };
                        cropControl.Crop += (sender, args) =>
                        {
                            callback.SafeInvoke(args.File);

                            cropControl.TryClose();
                        };
                        cropControl.SetFile(result1.FirstOrDefault(), result2.FirstOrDefault());

                        popup.Child = cropControl;
                        popup.IsOpen = true;
                    });
                });
            }
        }

        private static void PageOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_cropControl != null)
            {
                _cropControl.Width = e.NewSize.Width;
                _cropControl.Height = e.NewSize.Height;
            }
        }
    }
}
