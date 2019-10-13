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
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Services;
using Microsoft.Phone.Tasks;
using TelegramClient.Views.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Media
{
    public class ProfilePhotoViewerViewModel : ViewAware
    {
        private readonly IList<TLPhotoBase> _items = new List<TLPhotoBase>();

        private int _currentIndex;

        private TLUserBase _currentContact;

        private TLChatBase _currentChat;

        public bool IsChatViewer
        {
            get { return _currentChat != null; }
        }

        public bool IsSelfViewer
        {
            get { return _currentContact != null && _currentContact.IsSelf; }
        }

        public TLPhotoBase PreviousItem { get; set; }

        private TLPhotoBase _currentItem;

        public TLPhotoBase CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem != value)
                {
                    _currentItem = value;
                    NotifyOfPropertyChange(() => CurrentItem);
                }
            }
        }

        public TLPhotoBase NextItem { get; set; }

        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                if (_isWorking != value)
                {
                    _isWorking = value;
                    NotifyOfPropertyChange(() => IsWorking);
                }
            }
        }

        public IStateService StateService { get; protected set; }

        public IMTProtoService MTProtoService { get; protected set; }

        public BindableCollection<TLPhotoBase> GroupedItems { get; set; }

        public ProfilePhotoViewerViewModel(IStateService stateService, IMTProtoService mtProtoService)
        {
            StateService = stateService;
            MTProtoService = mtProtoService;

            _currentContact = StateService.CurrentContact;
            _currentChat = StateService.CurrentChat;

            GroupedItems = new BindableCollection<TLPhotoBase>();
        }

        private void SetInitState()
        {
            CurrentItem = StateService.CurrentPhoto;
            StateService.CurrentPhoto = null;

            if (StateService.CurrentContact != null)
            {
                _currentContact = StateService.CurrentContact;
                StateService.CurrentContact = null;

                IsWorking = true;
                MTProtoService.GetUserPhotosAsync(_currentContact.ToInputUser(), new TLInt(0), new TLLong(0),
                    new TLInt(0),
                    photos => Execute.BeginOnUIThread(() =>
                    {
                        _currentIndex = 0;
                        _items.Clear();
                        foreach (var photo in photos.Photos)
                        {
                            _items.Add(photo);
                        }

                        GroupedItems.IsNotifying = false;
                        GroupedItems.Clear();
                        if (_items.Count > 1) GroupedItems.AddRange(_items);
                        GroupedItems.IsNotifying = true;
                        GroupedItems.Refresh();

                        PreviousItem = _currentIndex + 1 < _items.Count ? _items[_currentIndex + 1] : null;
                        NextItem = _currentIndex > 0 ? _items[_currentIndex - 1] : null;

                        var view = GetView() as ProfilePhotoViewerView;
                        if (view != null)
                        {
                            view.SetControlContent(0, NextItem);
                            view.SetControlContent(2, PreviousItem);
                            if (_items.Count > 0)
                            {
                                view.ScrollTo(_items[_currentIndex], 0.25);
                            }
                        }

                        IsWorking = false;
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Execute.ShowDebugMessage("photos.getUserPhotos error=" + error);
                    }));
            }
            else if (StateService.CurrentChat != null)
            {
                _currentChat = StateService.CurrentChat;
                StateService.CurrentChat = null;

                IsWorking = true;
                MTProtoService.SearchAsync(
                    _currentChat.ToInputPeer(),
                    TLString.Empty,
                    null,
                    new TLInputMessagesFilterChatPhotos(),
                    new TLInt(0),
                    new TLInt(0),
                    new TLInt(0),
                    new TLInt(0),
                    new TLInt(100),
                    new TLInt(0),
                    result => Execute.BeginOnUIThread(() =>
                    {
                        _items.Clear();
                        _currentIndex = -1;
                        for (var i = 0; i < result.Messages.Count; i++)
                        {
                            var messageService = result.Messages[i] as TLMessageService;
                            if (messageService != null)
                            {
                                var chatEditPhotoAction = messageService.Action as TLMessageActionChatEditPhoto;
                                if (chatEditPhotoAction != null)
                                {
                                    _items.Add(chatEditPhotoAction.Photo);

                                    var photo = chatEditPhotoAction.Photo as TLPhoto;
                                    var currentPhoto = CurrentItem as TLPhoto;
                                    var chatPhoto = CurrentItem as TLChatPhoto;
                                    if (photo != null
                                        && currentPhoto != null
                                        && photo.Id.Value == currentPhoto.Id.Value
                                        || (chatPhoto != null && ChatPhotoEquals(chatPhoto, photo)))
                                    {
                                        _currentIndex = _items.Count - 1;
                                    }
                                }
                            }
                        }

                        if (_currentIndex == -1)
                        {
                            if (_items.Count > 0)
                            {
                                _items.Insert(0, CurrentItem);
                                _currentIndex = 0;
                            }
                        }

                        GroupedItems.IsNotifying = false;
                        GroupedItems.Clear();
                        if (_items.Count > 1) GroupedItems.AddRange(_items);
                        GroupedItems.IsNotifying = true;
                        GroupedItems.Refresh();

                        PreviousItem = _currentIndex + 1 < _items.Count ? _items[_currentIndex + 1] : null;
                        NextItem = _currentIndex > 0 ? _items[_currentIndex - 1] : null;

                        var view = GetView() as ProfilePhotoViewerView;
                        if (view != null)
                        {
                            view.SetControlContent(0, NextItem);
                            view.SetControlContent(2, PreviousItem);
                            if (_items.Count > 0)
                            {
                                view.ScrollTo(_items[_currentIndex], 0.25);
                            }
                        }

                        IsWorking = false;
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Execute.ShowDebugMessage("messages.search error=" + error);
                    }));
            }
        }

        private static bool ChatPhotoEquals(TLChatPhoto chatPhoto, TLPhoto photo)
        {
            if (chatPhoto != null && photo == null) return false;
            if (chatPhoto == null && photo != null) return false;
            if (chatPhoto == null && photo == null) return false;

            var fileLocation1 = chatPhoto.PhotoSmall as TLFileLocation;
            var photoSize = photo.Sizes.Count > 0 ? photo.Sizes[0] as TLPhotoSize : null;
            var fileLocation2 = photoSize != null ? photoSize.Location as TLFileLocation : null;

            if (fileLocation1 != null && fileLocation2 == null) return false;
            if (fileLocation1 == null && fileLocation2 != null) return false;
            if (fileLocation1 == null && fileLocation2 == null) return false;

            if (fileLocation1.DCId.Value == fileLocation2.DCId.Value
                && fileLocation1.VolumeId.Value == fileLocation2.VolumeId.Value
                && fileLocation1.LocalId.Value == fileLocation2.LocalId.Value
                && fileLocation1.Secret.Value == fileLocation2.Secret.Value)
            {
                return true;
            }

            return false;
        }

        public void SavePhoto()
        {
            SavePhotoAsync();
        }

        private void SavePhotoAsync(Action<string> callback = null)
        {
            TLFileLocation location = null;
            var profilePhoto = CurrentItem as TLUserProfilePhoto;
            if (profilePhoto != null)
            {
                location = profilePhoto.PhotoBig as TLFileLocation;
            }

            var photo = CurrentItem as TLPhoto;
            if (photo != null)
            {
                TLPhotoSize size = null;
                var sizes = photo.Sizes.OfType<TLPhotoSize>();
                const double width = 640.0;
                foreach (var photoSize in sizes)
                {
                    if (size == null
                        || Math.Abs(width - size.W.Value) > Math.Abs(width - photoSize.W.Value))
                    {
                        size = photoSize;
                    }
                }
                if (size == null) return;

                location = size.Location as TLFileLocation;
            }

            var chatPhoto = CurrentItem as TLChatPhoto;
            if (chatPhoto != null)
            {
                location = chatPhoto.PhotoBig as TLFileLocation;
            }

            if (location == null) return;

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                location.VolumeId,
                location.LocalId,
                location.Secret);

            Execute.BeginOnThreadPool(() => ImageViewerViewModel.SavePhoto(fileName, callback));
        }

        public bool CanSlideRight
        {
            get { return _currentIndex > 0; }
        }

        public void SlideRight()
        {
            if (!CanSlideRight) return;

            var nextItem = _items[--_currentIndex];
            CurrentItem = nextItem;
            PreviousItem = _currentIndex + 1 < _items.Count ? _items[_currentIndex + 1] : null;
            NextItem = _currentIndex > 0 ? _items[_currentIndex - 1] : null;
            NotifyOfPropertyChange(() => PreviousItem);
            NotifyOfPropertyChange(() => NextItem);
        }

        public bool CanSlideLeft
        {
            get { return _currentIndex < _items.Count - 1; }
        }

        public void SlideLeft()
        {
            if (!CanSlideLeft) return;

            var nextItem = _items[++_currentIndex];
            CurrentItem = nextItem;
            PreviousItem = _currentIndex + 1 < _items.Count ? _items[_currentIndex + 1] : null;
            NextItem = _currentIndex > 0 ? _items[_currentIndex - 1] : null;
            NotifyOfPropertyChange(() => PreviousItem);
            NotifyOfPropertyChange(() => NextItem);
        }

        public void SharePhoto()
        {
#if WP8
            SavePhotoAsync(path =>
            {
                var task = new ShareMediaTask { FilePath = path };
                task.Show();
            });
#endif
        }

        public void DeletePhoto()
        {
            if (_currentContact == null || !_currentContact.IsSelf) return;
            if (CurrentItem == null) return;

            var currentItem = CurrentItem;
            IsWorking = true;
            MTProtoService.UpdateProfilePhotoAsync(new TLInputPhotoEmpty(),
                result =>
                {
                    Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                    {
                        IsWorking = false;

                        if (CanSlideLeft)
                        {
                            var view = GetView() as ProfilePhotoViewerView;
                            if (view != null)
                            {
                                _items.RemoveAt(_currentIndex--);
                                view.SlideLeft(0.0, () =>
                                {
                                    view.SetControlContent(0, PreviousItem);
                                    GroupedItems.Remove(currentItem);
                                });
                            }
                        }
                        else
                        {
                            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), CloseViewer);
                        }
                    });
                },
                error =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("photos.updateProfilePhoto error " + error);
                });
        }

        public void SetPhoto()
        {
            var photo = CurrentItem as TLPhoto;
            if (photo == null) return;

            TLPhotoSize size = null;
            var sizes = photo.Sizes.OfType<TLPhotoSize>();
            const double width = 800.0;
            foreach (var photoSize in sizes)
            {
                if (size == null
                    || Math.Abs(width - size.W.Value) > Math.Abs(width - photoSize.W.Value))
                {
                    size = photoSize;
                }
            }
            if (size == null) return;

            var location = size.Location as TLFileLocation;
            if (location == null) return;

            if (_currentContact != null)
            {
                IsWorking = true;
                MTProtoService.UpdateProfilePhotoAsync(new TLInputPhoto { Id = photo.Id, AccessHash = photo.AccessHash },
                    result =>
                    {
                        IsWorking = false;
                        _items.Insert(0, result);
                        _currentIndex++;
                        MTProtoService.GetUserPhotosAsync(_currentContact.ToInputUser(), new TLInt(1), new TLLong(0), new TLInt(1),
                            photos =>
                            {
                                var previousPhoto = photos.Photos.FirstOrDefault();

                                if (previousPhoto != null)
                                {
                                    _items.RemoveAt(1);
                                    _items.Insert(1, previousPhoto);
                                }
                            },
                            error =>
                            {
                                Execute.ShowDebugMessage("photos.getUserPhotos error " + error);
                            });
                    },
                    error =>
                    {
                        IsWorking = false;
                        Execute.ShowDebugMessage("photos.updateProfilePhoto error " + error);
                    });
            }
            else if (_currentChat != null)
            {

                var channel = _currentChat as TLChannel;
                if (channel != null)
                {
                    if (channel.Id != null)
                    {
                        IsWorking = true;
                        MTProtoService.EditPhotoAsync(
                            channel,
                            new TLInputChatPhoto56
                            {
                                Id = new TLInputPhoto { Id = photo.Id, AccessHash = photo.AccessHash }
                            },
                            result => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                var updates = result as TLUpdates;
                                if (updates != null)
                                {
                                    var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                                    if (updateNewMessage != null)
                                    {
                                        var serviceMessage = updateNewMessage.Message as TLMessageService;
                                        if (serviceMessage != null)
                                        {
                                            var chatEditPhotoAction = serviceMessage.Action as TLMessageActionChatEditPhoto;
                                            if (chatEditPhotoAction != null)
                                            {
                                                var newPhoto = chatEditPhotoAction.Photo as TLPhoto;
                                                if (newPhoto != null)
                                                {
                                                    _items.Insert(0, newPhoto);
                                                    _currentIndex++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                Execute.ShowDebugMessage("messages.editChatPhoto error " + error);
                            }));
                    }
                }

                var chat = _currentChat as TLChat;
                if (chat != null)
                {
                    IsWorking = true;
                    MTProtoService.EditChatPhotoAsync(
                        chat.Id,
                        new TLInputChatPhoto56
                        {
                            Id = new TLInputPhoto { Id = photo.Id, AccessHash = photo.AccessHash }
                        },
                        result => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            var updates = result as TLUpdates;
                            if (updates != null)
                            {
                                var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewMessage) as TLUpdateNewMessage;
                                if (updateNewMessage != null)
                                {
                                    var serviceMessage = updateNewMessage.Message as TLMessageService;
                                    if (serviceMessage != null)
                                    {
                                        var chatEditPhotoAction = serviceMessage.Action as TLMessageActionChatEditPhoto;
                                        if (chatEditPhotoAction != null)
                                        {
                                            var newPhoto = chatEditPhotoAction.Photo as TLPhoto;
                                            if (newPhoto != null)
                                            {
                                                _items.Insert(0, newPhoto);
                                                _currentIndex++;
                                            }
                                        }
                                    }
                                }
                            }
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            Execute.ShowDebugMessage("messages.editChatPhoto error " + error);
                        }));
                }
            }
        }

        public void OpenViewer()
        {
            SetInitState();
            _isOpen = CurrentItem != null;
            NotifyOfPropertyChange(() => CurrentItem);
            NotifyOfPropertyChange(() => IsOpen);
        }

        public void CloseViewer()
        {
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);
        }

        private bool _isOpen;

        public bool IsOpen { get { return _isOpen; } }
    }
}
