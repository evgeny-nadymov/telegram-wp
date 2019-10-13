// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
#if WP8
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
#endif
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Media
{
    public class DecryptedImageViewerViewModel : ViewAware
    {
        public bool IsInputTTL
        {
            get
            {
                var message = CurrentItem as TLDecryptedMessage;
                return message != null && !message.Out.Value && message.TTL.Value > 0;
            }
        }

        private IList<TLDecryptedMessage> _items = new List<TLDecryptedMessage>();

        private int _currentIndex;

        private TLDecryptedMessageBase _previousItem;

        public TLDecryptedMessageBase PreviousItem
        {
            get { return _previousItem; }
            set
            {
                _previousItem = value;
            }
        }

        private TLDecryptedMessageBase _currentItem;

        public TLDecryptedMessageBase CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem != value)
                {
                    _currentItem = value;
                    NotifyOfPropertyChange(() => CurrentItem);
                    NotifyOfPropertyChange(() => IsInputTTL);
                    NotifyOfPropertyChange(() => Title);
                }
            }
        }

        private TLDecryptedMessageBase _nextItem;

        public TLDecryptedMessageBase NextItem
        {
            get { return _nextItem; }
            set
            {
                _nextItem = value;
            }
        }

        public BindableCollection<TLDecryptedMessageBase> GroupedItems { get; set; }

        public string Title
        {
            get
            {
                var message = CurrentItem as TLDecryptedMessage;
                if (message != null && message.TTL.Value > 0)
                {
                    if (message.Media is TLDecryptedMessageMediaPhoto)
                    {
                        return AppResources.SecretPhoto;
                    }
                    if (message.Media is TLDecryptedMessageMediaVideo || message.IsVideo())
                    {
                        return AppResources.SecretVideo;
                    }
                }

                return string.Empty;
            }
        }

        public IStateService StateService { get; protected set; }

        public bool ShowOpenMediaListButton { get; protected set; }

        public SecretDialogDetailsViewModel DialogDetails { get; set; }

        public DecryptedImageViewerViewModel(IStateService stateService, bool showMediaButton = false)
        {
            StateService = stateService;

            ShowOpenMediaListButton = showMediaButton;
            GroupedItems = new BindableCollection<TLDecryptedMessageBase>();
        }

        public void Delete()
        {
            if (CurrentItem == null) return;
            if (DialogDetails == null) return;

            var currentItem = CurrentItem;
            DialogDetails.DeleteMessageWithCallback(true, (TLDecryptedMessage) CurrentItem,
                () => Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                {
                    if (CanSlideRight)
                    {
                        var view = GetView() as DecryptedImageViewerView;
                        if (view != null)
                        {
                            _items.RemoveAt(_currentIndex--);
                            view.SlideRight(0.0, () =>
                            {
                                view.SetControlContent(2, NextItem);
                                GroupedItems.Remove(currentItem);
                            });
                        }
                    }
                    else if (CanSlideLeft)
                    {
                        var view = GetView() as DecryptedImageViewerView;
                        if (view != null)
                        {
                            _items.RemoveAt(_currentIndex);
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
                }));
        }

        public void Forward()
        {
            if (CurrentItem == null) return;
            if (DialogDetails == null) return;

            //SecretDialogDetails.ForwardMessage(CurrentItem);
        }

        public void OpenMediaDetails()
        {
            if (DialogDetails == null) return;

            StateService.MediaTab = true;
            StateService.CurrentDecryptedMediaMessages = _items;
            DialogDetails.OpenPeerDetails();
        }

#if DEBUG
        //~ImageViewerViewModel()
        //{
        //    TLUtils.WritePerformance("++ImageViewerVM dstr");
        //}
#endif

        private bool _isOpen;

        public bool IsOpen { get { return _isOpen; } }

        public event EventHandler Open;

        protected virtual void RaiseOpen()
        {
            EventHandler handler = Open;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public void OpenViewer()
        {
            CurrentItem = StateService.CurrentDecryptedPhotoMessage;
            _items = StateService.CurrentDecryptedMediaMessages;
            if (_items != null)
            {
                _currentIndex = _items.IndexOf(StateService.CurrentDecryptedPhotoMessage);
                PreviousItem = _currentIndex + 1 < _items.Count ? _items[_currentIndex + 1] : null;
                NextItem = _currentIndex > 0 ? _items[_currentIndex - 1] : null;

                CreateGroupedItems(CurrentItem);
            }
            else
            {
                _currentIndex = -1;
                PreviousItem = null;
                NextItem = null;
                GroupedItems.Clear();
            }

            _isOpen = CurrentItem != null;
            NotifyOfPropertyChange(() => CurrentItem);
            NotifyOfPropertyChange(() => PreviousItem);
            NotifyOfPropertyChange(() => NextItem);
            NotifyOfPropertyChange(() => IsOpen);
            NotifyOfPropertyChange(() => CanZoom);

            StateService.CurrentDecryptedPhotoMessage = null;
            StateService.CurrentDecryptedMediaMessages = null;

            RaiseOpen();
        }

        private void CreateGroupedItems(TLDecryptedMessageBase currentItem)
        {
            var message73 = currentItem as TLDecryptedMessage73;
            if (message73 != null && message73.GroupedId != null)
            {
                var firstItem = GroupedItems.FirstOrDefault() as TLDecryptedMessage73;
                if (firstItem == null
                    || (firstItem.GroupedId != null
                        && firstItem.GroupedId.Value != message73.GroupedId.Value))
                {
                    var items = new List<TLDecryptedMessageBase>();
                    for (var i = _items.Count - 1; i >= 0; i--)
                    {
                        var item = _items[i] as TLDecryptedMessage73;
                        if (item != null
                            && item.GroupedId != null
                            && item.GroupedId.Value == message73.GroupedId.Value)
                        {
                            items.Add(item);
                        }
                    }

                    GroupedItems.IsNotifying = false;
                    GroupedItems.Clear();
                    if (items.Count > 1)
                    {
                        foreach (var item in items)
                        {
                            GroupedItems.Add(item);
                        }
                    }
                    GroupedItems.IsNotifying = true;
                    GroupedItems.Refresh();
                    var view = GetView() as DecryptedImageViewerView;
                    if (view != null)
                    {
                        view.ScrollTo(currentItem, 0.0);
                    }
                }
                else if (firstItem.GroupedId != null
                    && firstItem.GroupedId.Value == message73.GroupedId.Value)
                {
                    var view = GetView() as DecryptedImageViewerView;
                    if (view != null)
                    {
                        view.ScrollTo(currentItem, 0.0);
                    }
                }
                else
                {
                    GroupedItems.Clear();
                }
            }
            else
            {
                GroupedItems.Clear();
            }
        }

        public event EventHandler Close;

        protected virtual void RaiseClose()
        {
            EventHandler handler = Close;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }


        public void CloseViewer()
        {
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);

            RaiseClose();
        }

        private static void SavePhotoAsync(TLDecryptedMessageMediaPhoto mediaPhoto, Action<string> callback = null)
        {
            var location = mediaPhoto.Photo as TLEncryptedFile;
            if (location == null) return;

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                location.Id,
                location.DCId,
                location.AccessHash);

            Execute.BeginOnThreadPool(() => ImageViewerViewModel.SavePhoto(fileName, callback));
        }

        private static void SaveVideoAsync(TLDecryptedMessageMediaBase mediaBase)
        {
            var mediaDocument = mediaBase as TLDecryptedMessageMediaDocument45;
            if (mediaDocument != null && TLDecryptedMessageBase.IsVideo(mediaDocument))
            {
                var fileLocation = mediaDocument.File as TLEncryptedFile;
                if (fileLocation == null) return;

                var fileName = String.Format("{0}_{1}_{2}.mp4",
                    fileLocation.Id,
                    fileLocation.DCId,
                    fileLocation.AccessHash);

                Execute.BeginOnThreadPool(() => ImageViewerViewModel.SaveVideo(fileName));

                return;
            }

            var mediaVideo = mediaBase as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null)
            {
                var fileLocation = mediaVideo.File as TLEncryptedFile;
                if (fileLocation == null) return;

                var fileName = String.Format("{0}_{1}_{2}.mp4",
                    fileLocation.Id,
                    fileLocation.DCId,
                    fileLocation.AccessHash);

                Execute.BeginOnThreadPool(() => ImageViewerViewModel.SaveVideo(fileName));

                return;
            }
        }

#if WP8
        public void Save()
        {
            var message = CurrentItem as TLDecryptedMessage;
            if (message == null) return;

            var mediaPhoto = message.Media as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                SavePhotoAsync(mediaPhoto);
                return;
            }

            var mediaDocument = message.Media as TLDecryptedMessageMediaDocument45;
            if (mediaDocument != null && message.IsVideo())
            {
                SaveVideoAsync(mediaDocument);
                return;
            }

            var mediaVideo = message.Media as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null)
            {
                SaveVideoAsync(mediaVideo);
                return;
            }
        }
#else
        public void Save()
        {

        }
#endif

        public void Share()
        {
#if WP8
            var message = CurrentItem as TLDecryptedMessage;
            if (message == null) return;

            var mediaPhoto = message.Media as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                var location = mediaPhoto.Photo as TLEncryptedFile;
                if (location == null) return;

                var fileName = String.Format("{0}_{1}_{2}.jpg",
                    location.Id,
                    location.DCId,
                    location.AccessHash);

                var dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (o, args) =>
                {
                    var request = args.Request;

                    request.Data.Properties.Title = "media";
                    request.Data.Properties.ApplicationName = "Telegram Messenger";

                    var deferral = request.GetDeferral();

                    try
                    {
                        var fileToShare = FileUtils.GetLocalFile(fileName);//this.GetImageFileAsync("Sample.jpg");
                        if (fileToShare == null) return;
                        //var storageFileToShare = await this.GetStorageFileForImageAsync("Sample.jpg");

                        //request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromStream(fileToShare);
                        //request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(fileToShare));


                        // On Windows Phone, share StorageFile instead of Bitmaps
                        request.Data.SetStorageItems(new List<StorageFile> { fileToShare });
                    }
                    finally
                    {
                        deferral.Complete();
                    }
                };
                DataTransferManager.ShowShareUI();
            }
#endif
        }

        public bool CanZoom
        {
            get
            {
                //return true;
                return CurrentItem != null && ((TLDecryptedMessage)CurrentItem).Media is TLDecryptedMessageMediaPhoto;
            }
        }

        public bool CanSlideLeft
        {
            get { return _currentIndex > 0; }
        }

        public void SlideLeft()
        {
            if (!CanSlideLeft) return;

            var nextItem = _items[--_currentIndex];
            CurrentItem = nextItem;
            PreviousItem = _currentIndex + 1 < _items.Count ? _items[_currentIndex + 1] : null;
            NextItem = _currentIndex > 0 ? _items[_currentIndex - 1] : null;
            NotifyOfPropertyChange(() => PreviousItem);
            NotifyOfPropertyChange(() => NextItem);
            NotifyOfPropertyChange(() => CanZoom);

            CreateGroupedItems(CurrentItem);
        }

        public bool CanSlideRight
        {
            get { return _currentIndex < _items.Count - 1; }
        }

        public void SlideRight()
        {
            if (!CanSlideRight) return;

            var nextItem = _items[++_currentIndex];
            CurrentItem = nextItem;
            PreviousItem = _currentIndex + 1 < _items.Count ? _items[_currentIndex + 1] : null;
            NextItem = _currentIndex > 0 ? _items[_currentIndex - 1] : null;
            NotifyOfPropertyChange(() => PreviousItem);
            NotifyOfPropertyChange(() => NextItem);
            NotifyOfPropertyChange(() => CanZoom);

            CreateGroupedItems(CurrentItem);
        }

        public void OpenMedia()
        {
            var message = CurrentItem as TLDecryptedMessage;
            if (message == null) return;

            var mediaVideo = message.Media as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null)
            {
                var fileLocation = mediaVideo.File as TLEncryptedFile;
                if (fileLocation == null) return;

                var fileName = String.Format("{0}_{1}_{2}.mp4",
                    fileLocation.Id,
                    fileLocation.DCId,
                    fileLocation.AccessHash);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(fileName))
                    {
                        var launcher = new MediaPlayerLauncher();
                        launcher.Location = MediaLocationType.Data;
                        launcher.Media = new Uri(fileName, UriKind.Relative);
                        launcher.Show();
                    }
                    else
                    {
                        mediaVideo.DownloadingProgress = 0.001;
                        var fileManager = IoC.Get<IEncryptedFileManager>();
                        fileManager.DownloadFile(fileLocation, mediaVideo);

                        //DownloadVideoFileManager.DownloadFileAsync(mediaVideo.ToInputFileLocation(), message, mediaVideo.Size);
                    }
                }
            }
        }

        public void Handle(DownloadableItem item)
        {
            var message = item.Owner as TLDecryptedMessage;
            if (message != null && _items != null)
            {
                var messages = _items;
                foreach (var m in messages)
                {
                    var media = m.Media as TLDecryptedMessageMediaVideo;
                    if (media != null && m == item.Owner)
                    {
                        m.Media.DownloadingProgress = 0.0;
                        //m.Media.IsoFileName = item.IsoFileName;
                        //MessageBox.Show("Download video time: " + _downloadVideoStopwatch.Elapsed);
                        //media.NotifyOfPropertyChange(() => media.Video);
                        break;
                    }
                }
                return;
            }
        }

        public void CancelVideoDownloading()
        {
            var message = CurrentItem as TLDecryptedMessage;
            if (message == null) return;

            message.Media.DownloadingProgress = 0.0;
            var fileManager = IoC.Get<IEncryptedFileManager>();
            fileManager.CancelDownloadFile(message.Media);
            //DownloadVideoFileManager.CancelDownloadFileAsync(message);
        }

        //public void CancelVideoDownloading()
        //{
        //    var message = CurrentItem as TLDecryptedMessage;
        //    if (message == null) return;

        //    var mediaVideo = message.Media as TLDecryptedMessageMediaVideo;
        //    if (mediaVideo != null)
        //    {
        //        ThreadPool.QueueUserWorkItem(state =>
        //        {
        //            mediaVideo.DownloadingProgress = 0.0;
        //            _downloadVideoFileManager.CancelDownloadFileAsync(message);
        //        });
        //    }
        //}
    }
}
