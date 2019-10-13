// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using TelegramClient.ViewModels.Contacts;
#if WP8
using Windows.Storage;
#endif
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Extensions;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
        private void SendMedia()
        {
            if (!string.IsNullOrEmpty(StateService.VideoIsoFileName))
            {
                var videoIsoFileName = StateService.VideoIsoFileName;
                var videoDuration = StateService.Duration;
                StateService.VideoIsoFileName = null;
                StateService.Duration = 0;
                SendVideo(videoIsoFileName, videoDuration);
                return;
            }

            if (StateService.Photo != null)
            {
                var photo = StateService.Photo;
                StateService.Photo = null;

                SendPhoto(photo);
                return;
            }

#if WP8 && MULTIPLE_PHOTOS
            if (App.Photos != null)
            {
                var photos = App.Photos;
                App.Photos = null;
                SendPhoto(photos);
            }
#endif

            if (StateService.Document != null)
            {
                var document = StateService.Document;
                StateService.Document = null;

                SendDocument(document);
                return;
            }
        }

        public void Attach()
        {
            BeginOnUIThread(() => ChooseAttachment.Open());
        }

        public bool OpenSecretPhoto(TLDecryptedMessageMediaPhoto mediaPhoto)
        {
            if (mediaPhoto == null) return false;

            TLDecryptedMessage17 message = null;
            for (var i = 0; i < Items.Count; i++)
            {
                var message17 = Items[i] as TLDecryptedMessage17;
                if (message17 != null && message17.Media == mediaPhoto)
                {
                    message = message17;
                    break;
                }
            }

            if (message == null) return false;
            if (message.Status == MessageStatus.Sending) return false;

            var result = false;
            if (!message.Out.Value)
            {
                if (message.TTL != null && message.TTL.Value > 0 && message.TTL.Value <= 60.0)
                {
                    if (mediaPhoto.TTLParams == null)
                    {
                        message.IsTTLStarted = true;
                        message.DeleteDate = new TLLong(DateTime.Now.Ticks + message.TTL.Value * TimeSpan.TicksPerSecond);
                        mediaPhoto.TTLParams = new TTLParams
                        {
                            StartTime = DateTime.Now,
                            IsStarted = true,
                            Total = message.TTL.Value
                        };
                        message.Unread = new TLBool(false);
                        message.Status = MessageStatus.Read;
                        CacheService.SyncDecryptedMessage(message, Chat, r =>
                        {
                            var chat = Chat as TLEncryptedChat;
                            if (chat == null) return;

                            var action = new TLDecryptedMessageActionReadMessages();
                            action.RandomIds = new TLVector<TLLong>{ message.RandomId };

                            var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);
#if DEBUG
                            Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                            SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                                sentEncryptedMessage =>
                                {

                                });

                        });
                    }

                    result = true;
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void OpenImageViewer()
        {
            if (ImageViewer == null)
            {
                ImageViewer = new DecryptedImageViewerViewModel(StateService, true)
                {
                    DialogDetails = this
                };
                NotifyOfPropertyChange(() => ImageViewer);
            }

            ImageViewer.OpenViewer();
        }

        private static DecryptedTTLQueue _ttlQueue;

        public static void AddToTTLQueue(TLDecryptedMessage message, TTLParams ttlParams, Action<TLDecryptedMessage> callback)
        {
            if (message == null) return;
            if (ttlParams == null) return;

            if (_ttlQueue == null)
            {
                _ttlQueue = new DecryptedTTLQueue();
            }

            _ttlQueue.Add(message, ttlParams, callback);
        }

#if WP8
        public async void OpenMedia(TLDecryptedMessage message)
#else
        public void OpenMedia(TLDecryptedMessage message)
#endif
        {
            if (message == null) return;
            if (message.Status == MessageStatus.Sending) return;
            if (message.Media.UploadingProgress > 0.0 && message.Media.UploadingProgress < 1.0) return;

            var mediaPhoto = message.Media as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null)
            {

                var location = mediaPhoto.File as TLEncryptedFile;
                if (location != null)
                {
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.Id,
                        location.DCId,
                        location.AccessHash);

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName))
                        {
                            message.Media.IsCanceled = false;
                            message.Media.DownloadingProgress = 0.01;
                            var fileManager = IoC.Get<IEncryptedFileManager>();
                            fileManager.DownloadFile(location, mediaPhoto);

                            return;
                        }
                    }
                }

                if (!message.Out.Value
                    && message.TTL != null
                    && message.TTL.Value > 0
                    && message.TTL.Value <= 60.0
                    && mediaPhoto.TTLParams == null)
                {
                    message.IsTTLStarted = true;
                    message.DeleteDate = new TLLong(DateTime.Now.Ticks + message.TTL.Value * TimeSpan.TicksPerSecond);
                    mediaPhoto.TTLParams = new TTLParams
                    {
                        StartTime = DateTime.Now,
                        IsStarted = true,
                        Total = message.TTL.Value
                    };

                    AddToTTLQueue(message, mediaPhoto.TTLParams,
                        result =>
                        {
                            DeleteMessage(false, message);
                            SplitGroupedMessages(new List<TLLong> { message.RandomId });
                        });

                    CacheService.SyncDecryptedMessage(message, Chat, r =>
                    {
                        var chat = Chat as TLEncryptedChat;
                        if (chat == null) return;

                        var action = new TLDecryptedMessageActionReadMessages();
                        action.RandomIds = new TLVector<TLLong> { message.RandomId };

                        var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);
#if DEBUG
                        Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                        SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                            sentEncryptedMessage =>
                            {

                            });
                    });
                }

                message.Unread = new TLBool(false);
                message.Status = MessageStatus.Read;
                CacheService.SyncDecryptedMessage(message, Chat, r => { });

                if (mediaPhoto.IsCanceled)
                {
                    mediaPhoto.IsCanceled = false;
                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.Photo);
                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.Self);

                    return;
                }

                StateService.CurrentDecryptedPhotoMessage = message;
                StateService.CurrentDecryptedMediaMessages = message.TTL.Value > 0? new List<TLDecryptedMessage>() :
                    UngroupEnumerator(Items)
                    .OfType<TLDecryptedMessage>()
                    .Where(x => x.TTL.Value == 0 && (x.Media is TLDecryptedMessageMediaPhoto || x.IsVideo()))
                    .ToList();

                OpenImageViewer();

                return;
            }

            var mediaGeo = message.Media as TLDecryptedMessageMediaGeoPoint;
            if (mediaGeo != null)
            {
                OpenLocation(message);

                return;
            }

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
                        var mediaVideo17 = mediaVideo as TLDecryptedMessageMediaVideo17;
                        if (mediaVideo17 != null)
                        {
                            if (!message.Out.Value)
                            {
                                if (message.TTL != null && message.TTL.Value > 0 && message.TTL.Value <= 60.0)
                                {
                                    if (mediaVideo17.TTLParams == null)
                                    {
                                        message.IsTTLStarted = true;
                                        message.DeleteDate = new TLLong(DateTime.Now.Ticks + Math.Max(mediaVideo17.Duration.Value + 1, message.TTL.Value) * TimeSpan.TicksPerSecond);
                                        mediaVideo17.TTLParams = new TTLParams
                                        {
                                            StartTime = DateTime.Now,
                                            IsStarted = true,
                                            Total = message.TTL.Value
                                        };
                                        message.Unread = new TLBool(false);
                                        message.Status = MessageStatus.Read;

                                        AddToTTLQueue(message, mediaVideo17.TTLParams,
                                            result =>
                                            {
                                                DeleteMessage(false, message);
                                                SplitGroupedMessages(new List<TLLong> { message.RandomId });
                                            });

                                        CacheService.SyncDecryptedMessage(message, Chat, r =>
                                        {
                                            var chat = Chat as TLEncryptedChat;
                                            if (chat == null) return;

                                            var action = new TLDecryptedMessageActionReadMessages();
                                            action.RandomIds = new TLVector<TLLong>{ message.RandomId };

                                            var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);

#if DEBUG
                                            Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                                            SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                                                sentEncryptedMessage =>
                                                {

                                                });

                                        });
                                    }
                                }
                            }
                        }

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
                    }
                }

                return;
            }

            var mediaAudio = message.Media as TLDecryptedMessageMediaAudio;
            if (mediaAudio != null)
            {
                var fileLocation = mediaAudio.File as TLEncryptedFile;
                if (fileLocation == null) return;

                var fileName = String.Format("audio{0}_{1}.wav",
                    fileLocation.Id,
                    fileLocation.AccessHash);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(fileName))
                    {
                        mediaAudio.DownloadingProgress = 0.001;
                        var fileManager = IoC.Get<IEncryptedFileManager>();
                        fileManager.DownloadFile(fileLocation, mediaAudio);
                    }
                    else
                    {
                        if (mediaAudio.IsCanceled)
                        {
                            mediaAudio.IsCanceled = false;
                            mediaAudio.NotifyOfPropertyChange(() => mediaAudio.ThumbSelf);

                            return;
                        }

                        var mediaAudio17 = mediaAudio as TLDecryptedMessageMediaAudio17;
                        if (mediaAudio17 != null)
                        {
                            if (!message.Out.Value)
                            {
                                if (message.TTL != null && message.TTL.Value > 0 && message.TTL.Value <= 60.0)
                                {
                                    if (mediaAudio17.TTLParams == null)
                                    {
                                        message.IsTTLStarted = true;
                                        message.DeleteDate = new TLLong(DateTime.Now.Ticks + Math.Max(mediaAudio17.Duration.Value + 1, message.TTL.Value) * TimeSpan.TicksPerSecond);
                                        mediaAudio17.TTLParams = new TTLParams
                                        {
                                            StartTime = DateTime.Now,
                                            IsStarted = true,
                                            Total = message.TTL.Value
                                        };
                                        message.Unread = new TLBool(false);
                                        message.Status = MessageStatus.Read;

                                        CacheService.SyncDecryptedMessage(message, Chat, r =>
                                        {
                                            var chat = Chat as TLEncryptedChat;
                                            if (chat == null) return;

                                            var action = new TLDecryptedMessageActionReadMessages();
                                            action.RandomIds = new TLVector<TLLong> { message.RandomId };

                                            var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);

#if DEBUG
                                            Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                                            SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                                                sentEncryptedMessage =>
                                                {

                                                });

                                        });
                                    }
                                }
                            }
                        }     
                    }
                }

                return;
            }

            var mediaDocument = message.Media as TLDecryptedMessageMediaDocument;
            if (mediaDocument != null)
            {
                if (message.IsVoice())
                {
                    var fileLocation = mediaDocument.File as TLEncryptedFile;
                    if (fileLocation == null) return;

                    var fileName = String.Format("audio{0}_{1}.wav",
                        fileLocation.Id,
                        fileLocation.AccessHash);

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName))
                        {
                            mediaDocument.DownloadingProgress = 0.001;
                            var fileManager = IoC.Get<IEncryptedFileManager>();
                            fileManager.DownloadFile(fileLocation, mediaDocument);
                        }
                        else
                        {
                            if (mediaDocument.IsCanceled)
                            {
                                mediaDocument.IsCanceled = false;
                                mediaDocument.NotifyOfPropertyChange(() => mediaDocument.ThumbSelf);

                                return;
                            }

                            var mediaDocument45 = mediaDocument as TLDecryptedMessageMediaDocument45;
                            if (mediaDocument45 != null)
                            {
                                if (!message.Out.Value)
                                {
                                    var message45 = message as TLDecryptedMessage45;
                                    if (message.TTL != null && message.TTL.Value > 0 && message.TTL.Value <= 60.0)
                                    {
                                        if (mediaDocument45.TTLParams == null)
                                        {
                                            message.IsTTLStarted = true;
                                            message.DeleteDate = new TLLong(DateTime.Now.Ticks + Math.Max(mediaDocument45.Duration.Value + 1, message.TTL.Value) * TimeSpan.TicksPerSecond);
                                            mediaDocument45.TTLParams = new TTLParams
                                            {
                                                StartTime = DateTime.Now,
                                                IsStarted = true,
                                                Total = message.TTL.Value
                                            };
                                            message.Unread = new TLBool(false);
                                            message.Status = MessageStatus.Read;

                                            CacheService.SyncDecryptedMessage(message, Chat, r =>
                                            {
                                                var chat = Chat as TLEncryptedChat;
                                                if (chat == null) return;

                                                var action = new TLDecryptedMessageActionReadMessages();
                                                action.RandomIds = new TLVector<TLLong> { message.RandomId };

                                                var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);

#if DEBUG
                                                Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                                                SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                                                    sentEncryptedMessage =>
                                                    {
                                                        if (message45 != null)
                                                        {
                                                            message45.SetListened();
                                                            message45.Media.NotListened = false;
                                                            message45.Media.NotifyOfPropertyChange(() => message45.Media.NotListened);

                                                            CacheService.Commit();
                                                        }
                                                    });

                                            });
                                        }
                                    }
                                    else
                                    {
                                        ReadMessageContents(message45);
                                    }
                                }
                            }
                        }
                    }

                    return;
                }
                else if (message.IsVideo())
                {
                    var fileLocation = mediaDocument.File as TLEncryptedFile;
                    if (fileLocation == null) return;

                    var fileName = String.Format("{0}_{1}_{2}.mp4",
                    fileLocation.Id,
                    fileLocation.DCId,
                    fileLocation.AccessHash);

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(fileName))
                        {
                            var mediaVideo17 = mediaDocument as TLDecryptedMessageMediaDocument45;
                            if (mediaVideo17 != null)
                            {
                                if (!message.Out.Value)
                                {
                                    if (message.TTL != null && message.TTL.Value > 0 && message.TTL.Value <= 60.0)
                                    {
                                        if (mediaVideo17.TTLParams == null)
                                        {
                                            message.IsTTLStarted = true;
                                            message.DeleteDate = new TLLong(DateTime.Now.Ticks + Math.Max(mediaVideo17.Duration.Value + 1, message.TTL.Value) * TimeSpan.TicksPerSecond);
                                            mediaVideo17.TTLParams = new TTLParams
                                            {
                                                StartTime = DateTime.Now,
                                                IsStarted = true,
                                                Total = message.TTL.Value
                                            };
                                            message.Unread = new TLBool(false);
                                            message.Status = MessageStatus.Read;

                                            AddToTTLQueue(message, mediaVideo17.TTLParams,
                                                result =>
                                                {
                                                    DeleteMessage(false, message);
                                                    SplitGroupedMessages(new List<TLLong> { message.RandomId });
                                                });

                                            CacheService.SyncDecryptedMessage(message, Chat, r =>
                                            {
                                                var chat = Chat as TLEncryptedChat;
                                                if (chat == null) return;

                                                var action = new TLDecryptedMessageActionReadMessages();
                                                action.RandomIds = new TLVector<TLLong> { message.RandomId };

                                                var decryptedTuple = GetDecryptedServiceMessageAndObject(action, chat, MTProtoService.CurrentUserId, CacheService);

#if DEBUG
                                                Execute.BeginOnUIThread(() => Items.Insert(0, decryptedTuple.Item1));
#endif
                                                SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService, CacheService,
                                                    sentEncryptedMessage =>
                                                    {

                                                    });

                                            });
                                        }
                                    }
                                }
                            }

                            var launcher = new MediaPlayerLauncher();
                            launcher.Location = MediaLocationType.Data;
                            launcher.Media = new Uri(fileName, UriKind.Relative);
                            launcher.Show();
                        }
                        else
                        {
                            mediaDocument.DownloadingProgress = 0.001;
                            var fileManager = IoC.Get<IEncryptedFileManager>();
                            fileManager.DownloadFile(fileLocation, mediaDocument);
                        }
                    }

                    return;
                }
                else
                {
                    var fileLocation = mediaDocument.File as TLEncryptedFile;
                    if (fileLocation == null) return;

                    var fileName = String.Format("{0}_{1}_{2}.{3}",
                        fileLocation.Id,
                        fileLocation.DCId,
                        fileLocation.AccessHash,
                        fileLocation.FileExt ?? mediaDocument.FileExt);

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(fileName))
                        {
#if WP8
                            StorageFile pdfFile = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                            Windows.System.Launcher.LaunchFileAsync(pdfFile);
#endif
                        }
                        else
                        {
                            mediaDocument.DownloadingProgress = 0.001;
                            var fileManager = IoC.Get<IEncryptedFileManager>();
                            fileManager.DownloadFile(fileLocation, mediaDocument);
                        }
                    }
                }

                return;
            }
#if DEBUG
            MessageBox.Show("Tap on media");
#endif
        }

        public void OpenMediaContact(TLUserBase user)
        {
            OpenContactInternal(user, null);
        }

        public void OpenMediaContact(TLInt userId, TLUserBase user, TLString phoneNumber)
        {
            if (user == null)
            {
                MTProtoService.GetFullUserAsync(new TLInputUser { UserId = userId, AccessHash = new TLLong(0) },
                    userFull => OpenContactInternal(userFull.User, phoneNumber));
            }
            else
            {
                OpenContactInternal(user, phoneNumber);
            }
        }

        private void OpenContactInternal(TLUserBase user, TLString phoneNumber)
        {
            if (user == null) return;

            StateService.CurrentContact = user;
            StateService.CurrentContactPhone = phoneNumber;
            NavigationService.UriFor<ContactViewModel>().Navigate();
        }
    }

    public class DecryptedMessageExpiredEventArgs : System.EventArgs
    {
        public TLDecryptedMessage Message { get; set; }
    }

    public class DecryptedTTLQueue
    {
        private readonly List<Tuple<TLDecryptedMessage, TTLParams, Action<TLDecryptedMessage>>> _items = new List<Tuple<TLDecryptedMessage, TTLParams, Action<TLDecryptedMessage>>>();

        private readonly Timer _timer;

        public DecryptedTTLQueue()
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
                        item.Item3.SafeInvoke(message);
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

        private void SetTimer(Tuple<TLDecryptedMessage, TTLParams, Action<TLDecryptedMessage>> item)
        {
            var timeSpan = item.Item2.StartTime.AddSeconds(item.Item2.Total) > DateTime.Now ? item.Item2.StartTime.AddSeconds(item.Item2.Total) - DateTime.Now : TimeSpan.FromSeconds(0.0);
            _timer.Change(timeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Add(TLDecryptedMessage message, TTLParams ttlParams, Action<TLDecryptedMessage> callback)
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            var newItem = new Tuple<TLDecryptedMessage, TTLParams, Action<TLDecryptedMessage>>(message, ttlParams, callback);

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
}
