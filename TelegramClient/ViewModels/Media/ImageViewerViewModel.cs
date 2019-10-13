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
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views;
#if WP8
using Windows.Storage;
#endif
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Media
{
    public class ImageViewerViewModel : ViewAware
    {
        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                if (value != _isWorking)
                {
                    _isWorking = value;
                    NotifyOfPropertyChange(() => IsWorking);
                }
            }
        }

        public bool Inverse { get; protected set; }

        private IList<TLMessage> _items = new List<TLMessage>(); 

        private int _currentIndex;

        public bool IsInputTTL
        {
            get
            {
                var message = CurrentItem as TLMessage;
                return message != null && !message.Out.Value && message.HasTTL();
            }
        }

        private TLMessageBase _previousItem;

        public TLMessageBase PreviousItem
        {
            get { return _previousItem; }
            set
            {
                _previousItem = value;
            }
        }

        private TLMessageBase _currentItem;

        public TLMessageBase CurrentItem
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

                    //CheckPhoto(value);
                }
            }
        }

        private TLMessageBase _nextItem;

        public TLMessageBase NextItem
        {
            get { return _nextItem; }
            set
            {
                _nextItem = value;
            }
        }

        public BindableCollection<TLMessageBase> GroupedItems { get; set; }

        public string Title
        {
            get
            {
                var message = CurrentItem as TLMessage;
                if (message != null && message.HasTTL())
                {
                    if (message.Media is TLMessageMediaPhoto)
                    {
                        return AppResources.SecretPhoto;
                    }
                    if (message.Media is TLMessageMediaVideo || message.IsVideo())
                    {
                        return AppResources.SecretVideo;
                    }
                }

                return string.Empty;
            }
        }

        public IStateService StateService { get; protected set; }

        private readonly IVideoFileManager _downloadVideoFileManager;

        public bool ShowOpenMediaListButton { get; protected set; }

        public DialogDetailsViewModel DialogDetails { get; set; }

        public ImageViewerViewModel(IStateService stateService, IVideoFileManager downloadVideoFileManager, bool inverse, bool showMediaButton = false)
        {
            Inverse = inverse;
            StateService = stateService;
            _downloadVideoFileManager = downloadVideoFileManager;

            ShowOpenMediaListButton = showMediaButton;
            GroupedItems = new BindableCollection<TLMessageBase>();
        }

        public void Delete()
        {
            if (CurrentItem == null) return;
            if (DialogDetails == null) return;

            var currentItem = CurrentItem;
            DialogDetails.DeleteMessageById(
                currentItem, 
                () =>
                {
                    Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                    {
                        if (CanSlideRight)
                        {
                            var view = GetView() as ImageViewerView;
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
                            var view = GetView() as ImageViewerView;
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
                    });
                });
        }

        public void Forward()
        {
            if (CurrentItem == null) return;

            DialogDetailsViewModel.ForwardMessagesCommon(new List<TLMessageBase>{ CurrentItem }, StateService, IoC.Get<INavigationService>());
        }

        public void OpenMediaDetails()
        {
            if (DialogDetails == null) return;

            StateService.MediaTab = true;
            DialogDetails.OpenPeerDetails();
        }

        public void OpenViewer()
        {
            CurrentItem = StateService.CurrentPhotoMessage;
            _items = StateService.CurrentMediaMessages;
            if (_items != null)
            {
                _currentIndex = _items.IndexOf(StateService.CurrentPhotoMessage);
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

            StateService.CurrentPhotoMessage = null;
            StateService.CurrentMediaMessages = null;

            RaiseOpen();
        }

        private void CreateGroupedItems(TLMessageBase currentItem)
        {
            var message73 = currentItem as TLMessage73;
            if (message73 != null && message73.GroupedId != null)
            {
                var firstItem = GroupedItems.FirstOrDefault() as TLMessage73;
                if (firstItem == null
                    || (firstItem.GroupedId != null
                        && firstItem.GroupedId.Value != message73.GroupedId.Value))
                {
                    var items = new List<TLMessageBase>();
                    for (var i = _items.Count - 1; i >= 0; i--)
                    {
                        var item = _items[i] as TLMessage73;
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
                    var view = GetView() as ImageViewerView;
                    if (view != null)
                    {
                        view.ScrollTo(currentItem, 0.0);
                    }
                }
                else if (firstItem.GroupedId != null
                    && firstItem.GroupedId.Value == message73.GroupedId.Value)
                {
                    var view = GetView() as ImageViewerView;
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

        public void CloseViewer()
        {
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);

            RaiseClose();
        }

        private bool _isOpen;

        public bool IsOpen { get { return _isOpen; } }

        public event EventHandler Open;

        protected virtual void RaiseOpen()
        {
            var handler = Open;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler Close;

        protected virtual void RaiseClose()
        {
            var handler = Close;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

#if WP8
        private static async void SaveFileAsync(string fileName, string fileExt, Action<string> callback = null)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(fileName))
                {
                    using (var fileStream = store.OpenFile(fileName, FileMode.Open))
                    {
                        var telegramFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(Constants.TelegramFolderName, CreationCollisionOption.OpenIfExists);
                        if (telegramFolder == null) return;

                        var ext = fileExt.StartsWith(".") ? fileExt : "." + fileExt;
                        var storageFile = await telegramFolder.CreateFileAsync(Guid.NewGuid() + ext, CreationCollisionOption.ReplaceExisting);

                        var stopwatch1 = Stopwatch.StartNew();
                        using (var storageStream = await storageFile.OpenStreamForWriteAsync())
                        {
                            fileStream.CopyTo(storageStream);
                        }

                        if (callback != null)
                        {
                            callback.Invoke(storageFile.Path);
                        }
                        else
                        {
                            var elapsed1 = stopwatch1.Elapsed;
                            //var stopwatch2 = Stopwatch.StartNew();
                            ////using (var storageStream = await storageFile.OpenStreamForWriteAsync())
                            //using (var storageStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                            //{
                            //    await RandomAccessStream.CopyAndCloseAsync(fileStream.AsInputStream(), storageStream.GetOutputStreamAt(0));
                            //}
                            //var elapsed2 = stopwatch2.Elapsed;
                            Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.SaveFileMessage
#if DEBUG
                                + "\n Time: " + elapsed1
                                //+ "\n Time2: " + elapsed2
#endif

                            ));
                        }
                    }
                }
            }
        }
#endif


#if WP8
        public static void SaveVideo(string fileName)
        {
            SaveFileAsync(fileName, "mp4");
        }
#else
        public static void SaveVideo(string fileName)
        {

        }
#endif

#if WP8
        public static void SavePhoto(string fileName, Action<string> callback = null)
        {
            SaveFileAsync(fileName, "jpg", callback);
        }
#else
        public static void SavePhoto(string photoFileName, Action<string> callback = null)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(photoFileName))
                {
                    using (var fileStream = store.OpenFile(photoFileName, FileMode.Open))
                    {
                        var photoUrl = Guid.NewGuid().ToString(); //.jpg will be added automatically            
                        var mediaLibrary = new MediaLibrary();
                        var photoFile = mediaLibrary.SavePicture(photoUrl, fileStream);

                        if (callback != null)
                        {
                            
                        }
                        else
                        {
                            Execute.BeginOnUIThread(() => MessageBox.Show(AppResources.SavePhotoMessage));
                        }
                    }
                }
            }
        }
#endif

        private static void SavePhotoAsync(TLMessageMediaPhoto mediaPhoto, Action<string> callback = null)
        {
            var photo = mediaPhoto.Photo as TLPhoto;
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

            var fileName = String.Format("{0}_{1}_{2}.jpg",
                location.VolumeId,
                location.LocalId,
                location.Secret);

            Execute.BeginOnThreadPool(() => SavePhoto(fileName, callback));
        }

        private static void SaveVideoAsync(TLMessageMediaBase mediaBase)
        {
            var mediaDocument = mediaBase as TLMessageMediaDocument45;
            if (mediaDocument != null && TLMessageBase.IsVideo(mediaDocument.Document))
            {
                var video = mediaDocument.Video as TLDocument22;
                if (video == null) return;

                var fileName = video.GetFileName();

                Execute.BeginOnThreadPool(() => SaveVideo(fileName));

                return;
            }

            var mediaVideo = mediaBase as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                var video = mediaVideo.Video as TLVideo;
                if (video == null) return;

                var fileName = video.GetFileName();

                Execute.BeginOnThreadPool(() => SaveVideo(fileName));

                return;
            }
        }

        public void Save()
        {
            var message = CurrentItem as TLMessage;
            if (message == null) return;

            var mediaPhoto = message.Media as TLMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                SavePhotoAsync(mediaPhoto);
                return;
            }

            var mediaDocument = message.Media as TLMessageMediaDocument45;
            if (mediaDocument != null && message.IsVideo())
            {
                SaveVideoAsync(mediaDocument);
                return;
            }

            var mediaVideo = message.Media as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                SaveVideoAsync(mediaVideo);
                return;
            }
        }

        public void Share()
        {
#if WP8
            var message = CurrentItem as TLMessage;
            if (message == null) return;

            var mediaPhoto = message.Media as TLMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                var photo = mediaPhoto.Photo as TLPhoto;
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

                var fileName = //mediaPhoto.IsoFileName ?? 
                    String.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);
                
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

                return;

                SavePhotoAsync(mediaPhoto, path =>
                {
                    var task = new ShareMediaTask { FilePath = path };
                    task.Show();
                });
            }
#endif
        }

        public bool CanZoom
        {
            get
            {
                //return true;
                return CurrentItem != null && ((TLMessage) CurrentItem).Media is TLMessageMediaPhoto;
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
            //NotifyOfPropertyChange(() => PreviousItem);
            //NotifyOfPropertyChange(() => NextItem);
            //NotifyOfPropertyChange(() => CanZoom);

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
            //NotifyOfPropertyChange(() => PreviousItem);
            //NotifyOfPropertyChange(() => NextItem);
            //NotifyOfPropertyChange(() => CanZoom);

            CreateGroupedItems(CurrentItem);
        }

        public void OpenMedia()
        {
            var message = CurrentItem as TLMessage;
            if (message == null) return;

            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null && TLMessageBase.IsVideo(mediaDocument.Document))
            {
                var video = mediaDocument.Video as TLDocument22;
                if (video == null) return;

                if (string.IsNullOrEmpty(mediaDocument.IsoFileName))
                {
                    mediaDocument.IsCanceled = false;
                    mediaDocument.DownloadingProgress = mediaDocument.LastProgress > 0.0 ? mediaDocument.LastProgress : 0.001;
                    _downloadVideoFileManager.DownloadFileAsync(
                        video.DCId, video.ToInputFileLocation(), message, video.Size,
                        progress =>
                        {
                            if (progress > 0.0)
                            {
                                mediaDocument.DownloadingProgress = progress;
                            }
                        });
                }
                else
                {
                    var launcher = new MediaPlayerLauncher();
                    launcher.Location = MediaLocationType.Data;
                    launcher.Media = new Uri(mediaDocument.IsoFileName, UriKind.Relative);
                    launcher.Show();
                }
                return;
            }

            var mediaVideo = message.Media as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                var video = mediaVideo.Video as TLVideo;
                if (video == null) return;

                if (string.IsNullOrEmpty(mediaVideo.IsoFileName))
                {
                    mediaVideo.IsCanceled = false;
                    mediaVideo.DownloadingProgress = mediaVideo.LastProgress > 0.0 ? mediaVideo.LastProgress : 0.001;
                    _downloadVideoFileManager.DownloadFileAsync(
                        video.DCId, video.ToInputFileLocation(), message, video.Size,
                        progress =>
                        {
                            if (progress > 0.0)
                            {
                                mediaVideo.DownloadingProgress = progress;
                            }
                        });
                }
                else
                {
                    var launcher = new MediaPlayerLauncher();
                    launcher.Location = MediaLocationType.Data;
                    launcher.Media = new Uri(mediaVideo.IsoFileName, UriKind.Relative);
                    launcher.Show();
                }
                return;
            }
        }


        public void Handle(DownloadableItem item)
        {
            var message = item.Owner as TLMessage;
            if (message != null && _items != null)
            {
                var messages = _items;
                foreach (var m in messages)
                {
                    var mediaDocument = m.Media as TLMessageMediaDocument45;
                    if (mediaDocument != null && m == item.Owner)
                    {
                        m.Media.IsCanceled = false;
                        m.Media.DownloadingProgress = 0.0;
                        m.Media.IsoFileName = item.IsoFileName;
                        //MessageBox.Show("Download video time: " + _downloadVideoStopwatch.Elapsed);
                        //media.NotifyOfPropertyChange(() => media.Video);
                        break;
                    }

                    var media = m.Media as TLMessageMediaVideo;
                    if (media != null && m == item.Owner)
                    {
                        m.Media.IsCanceled = false;
                        m.Media.DownloadingProgress = 0.0;
                        m.Media.IsoFileName = item.IsoFileName;
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
            var message = CurrentItem as TLMessage;
            if (message == null) return;

            var mediaDocument = message.Media as TLMessageMediaDocument45;
            if (mediaDocument != null)
            {
                mediaDocument.IsCanceled = true;
                mediaDocument.LastProgress = mediaDocument.DownloadingProgress;
                mediaDocument.DownloadingProgress = 0.0;
                _downloadVideoFileManager.CancelDownloadFileAsync(message);
            }

            var mediaVideo = message.Media as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                mediaVideo.IsCanceled = true;
                mediaVideo.LastProgress = mediaVideo.DownloadingProgress;
                mediaVideo.DownloadingProgress = 0.0;
                _downloadVideoFileManager.CancelDownloadFileAsync(message);
            }
        }

        public void GetAttachedStickers()
        {
        }

        public TLArchivedStickers AttachedStickers { get; set; }

        public void AddRemoveStickerSet(TLStickerSet32 set)
        {
            if (set == null) return;

            //var featuredStickers = _stickers as TLFeaturedStickers;
            if (AttachedStickers == null) return;

            var messagesStickerSet = AttachedStickers.MessagesStickerSets.FirstOrDefault(x => x.Set.Id.Value == set.Id.Value);
            if (messagesStickerSet == null) return;

            var stickerSetExists = set.Installed;
            var inputStickerSet = new TLInputStickerSetId { Id = set.Id, AccessHash = set.AccessHash };
            if (!stickerSetExists)
            {
                IoC.Get<IMTProtoService>().InstallStickerSetAsync(inputStickerSet, TLBool.False,
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
                            set76.InstalledDate = TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now);
                        }
                        set.NotifyOfPropertyChange(() => set.Installed);

                        var shellViewModel = IoC.Get<ShellViewModel>();
                        shellViewModel.Handle(new TLUpdateNewStickerSet { Stickerset = messagesStickerSet });

                        IoC.Get<IMTProtoService>().SetMessageOnTime(2.0, AppResources.NewStickersAdded);
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
                IoC.Get<IMTProtoService>().UninstallStickerSetAsync(inputStickerSet,
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
                        if (!set.Masks)
                        {
                            shellViewModel.RemoveStickerSet(set, inputStickerSet);
                        }
                        else
                        {
                            shellViewModel.RemoveMaskSet(set, inputStickerSet);
                        }

                        var eventAggregator = IoC.Get<ITelegramEventAggregator>();
                        eventAggregator.Publish(new UpdateRemoveStickerSetEventArgs(set));

                        IoC.Get<IMTProtoService>().SetMessageOnTime(2.0, AppResources.StickersRemoved);
                    }),
                    error =>
                    Execute.BeginOnUIThread(
                    () => { Execute.ShowDebugMessage("messages.uninstallStickerSet error " + error); }));
            }
        }
    }
}
