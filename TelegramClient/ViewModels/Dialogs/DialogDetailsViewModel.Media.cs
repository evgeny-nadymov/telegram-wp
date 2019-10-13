// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using Id3;
using Microsoft.Phone.BackgroundAudio;
using Telegram.Api;
using Telegram.Api.Extensions;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.EmojiPanel;
using TelegramClient.Services;
using TelegramClient.Views.Dialogs;
#if WP8
using Windows.Storage;
using Windows.System;
#endif
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public void OpenInvoice(TLMessageMediaInvoice mediaInvoice)
        {
            var message = Items.OfType<TLMessage48>().FirstOrDefault(x => x.Media == mediaInvoice);
            if (message != null)
            {
                var replyMarkup = message.ReplyMarkup as TLReplyInlineMarkup;
                if (replyMarkup != null)
                {
                    TLKeyboardButtonBuy buyButton = null;
                    foreach (var row in replyMarkup.Rows)
                    {
                        foreach (var button in row.Buttons)
                        {
                            buyButton = button as TLKeyboardButtonBuy;
                            if (buyButton != null) break;

                        }
                    }

                    if (buyButton != null)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            View.CreateBitmapCache(() =>
                            {
                                Execute.BeginOnUIThread(() => Send(message, buyButton));
                            });
                        });
                    }
                }
            }
        }

        private void SendMedia()
        {
            if (StateService.RecordedVideo != null)
            {
                var recordedVideo = StateService.RecordedVideo;
                StateService.RecordedVideo = null;
                SendVideo(recordedVideo);
                return;
            }

#if WP8

#if MULTIPLE_PHOTOS
            if (App.Photos != null)
            {
                var photos = App.Photos;
                App.Photos = null;
                SendPhoto(photos);
            }
#endif

            if (App.Video != null)
            {
                var video = App.Video;
                App.Video = null;
                SendVideo(video);
            }
#endif

            if (StateService.Photo != null)
            {
                var photo = StateService.Photo;
                StateService.Photo = null;

                //SendPhoto
                SendPhoto(photo);
                //var fileId = new TLFile{}
                //UploadFileManager.UploadFile(fielId)
                return;
            }

            if (StateService.Document != null)
            {
                var document = StateService.Document;
                StateService.Document = null;

                //SendPhoto
                SendDocument(document);
                //var fileId = new TLFile{}
                //UploadFileManager.UploadFile(fielId)
                return;
            }

            SendSharedContact();
        }

        private static TTLQueue _ttlQueue;

        public static void AddToTTLQueue(TLMessage70 message, TTLParams ttlParams, Action<TLMessage70> callback)
        {
            if (message == null) return;
            if (ttlParams == null) return;

            if (_ttlQueue == null)
            {
                _ttlQueue = new TTLQueue();
            }

            _ttlQueue.Add(message, ttlParams, callback);
        }

#if WP8
        public async void OpenMedia(TLMessageBase messageBase)
#else
        public void OpenMedia(TLMessageBase messageBase)
#endif
        {
            if (messageBase == null) return;
            if (messageBase.Status == MessageStatus.Failed
                || messageBase.Status == MessageStatus.Sending) return;

            var serviceMessage = messageBase as TLMessageService;
            if (serviceMessage != null)
            {
                var editPhotoAction = serviceMessage.Action as TLMessageActionChatEditPhoto;
                if (editPhotoAction != null)
                {
                    var photo = editPhotoAction.Photo;
                    if (photo != null)
                    {
                        //StateService.CurrentPhoto = photo;
                        //StateService.CurrentChat = With as TLChatBase;
                        //NavigationService.UriFor<ProfilePhotoViewerViewModel>().Navigate();
                    }
                }

                var phoneCallAction = serviceMessage.Action as TLMessageActionPhoneCall;
                if (phoneCallAction != null)
                {
                    TLUser user = null;
                    if (serviceMessage.Out.Value)
                    {
                        user = CacheService.GetUser(serviceMessage.ToId.Id) as TLUser;
                    }
                    else
                    {
                        user = serviceMessage.From as TLUser;
                    }

                    ShellViewModel.StartVoiceCall(user, IoC.Get<IVoIPService>(), IoC.Get<ICacheService>());
                }

                return;
            }

            var message = messageBase as TLMessage;
            if (message == null) return;

            var mediaPhoto = message.Media as TLMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                if (!message.Out.Value && message.HasTTL())
                {
                    var ttlMessageMedia = mediaPhoto as ITTLMessageMedia;
                    if (ttlMessageMedia != null && ttlMessageMedia.TTLParams == null)
                    {
                        ttlMessageMedia.TTLParams = new TTLParams
                        {
                            StartTime = DateTime.Now,
                            IsStarted = true,
                            Total = ttlMessageMedia.TTLSeconds.Value
                        };
                        mediaPhoto.NotifyOfPropertyChange(() => ttlMessageMedia.TTLParams);

                        AddToTTLQueue(message as TLMessage70, ttlMessageMedia.TTLParams,
                            result =>
                            {
                                SplitGroupedMessages(new List<TLInt> { message.Id });
                            });
                    }

                    ReadMessageContents(message as TLMessage25);
                }

                var photo = mediaPhoto.Photo as TLPhoto;
                if (photo != null)
                {
                    var width = 311.0;

                    TLPhotoSize size = null;
                    var sizes = photo.Sizes.OfType<TLPhotoSize>();
                    foreach (var photoSize in sizes)
                    {
                        if (size == null
                            || Math.Abs(width - size.W.Value) > Math.Abs(width - photoSize.W.Value))
                        {
                            size = photoSize;
                        }
                    }

                    if (size != null)
                    {
                        var location = size.Location as TLFileLocation;
                        if (location != null)
                        {
                            var fileName = String.Format("{0}_{1}_{2}.jpg",
                                location.VolumeId,
                                location.LocalId,
                                location.Secret);

                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (!store.FileExists(fileName))
                                {
                                    message.Media.IsCanceled = false;
                                    message.Media.DownloadingProgress = 0.01;
                                    Execute.BeginOnThreadPool(() => DownloadFileManager.DownloadFile(location, photo, size.Size));

                                    return;
                                }
                            }
                        }
                    }
                }

                if (mediaPhoto.IsCanceled)
                {
                    mediaPhoto.IsCanceled = false;
                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.Photo);
                    mediaPhoto.NotifyOfPropertyChange(() => mediaPhoto.Self);

                    return;
                }

                StateService.CurrentPhotoMessage = message;
                StateService.CurrentMediaMessages = message.HasTTL()
                    ? new List<TLMessage> { message }
                    : UngroupEnumerator(Items).OfType<TLMessage>()
                        .Where(x =>
                        {
                            if (TLMessageBase.HasTTL(x.Media))
                            {
                                return false;
                            }

                            return x.Media is TLMessageMediaPhoto || x.Media is TLMessageMediaVideo || x.IsVideo();
                        })
                        .ToList();

                OpenImageViewer();

                return;
            }

            var mediaWebPage = message.Media as TLMessageMediaWebPage;
            if (mediaWebPage != null)
            {
                var webPage = mediaWebPage.WebPage as TLWebPage;
                if (webPage != null && webPage.Type != null)
                {
                    if (webPage.Type != null)
                    {
                        var type = webPage.Type.ToString();
                        if (string.Equals(type, "photo", StringComparison.OrdinalIgnoreCase))
                        {
                            StateService.CurrentPhotoMessage = message;

                            OpenImageViewer();

                            return;
                        }
                        if (string.Equals(type, "video", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!TLString.Equals(webPage.SiteName, new TLString("youtube"), StringComparison.OrdinalIgnoreCase)
                                && !TLString.IsNullOrEmpty(webPage.EmbedUrl))
                            {
                                var launcher = new MediaPlayerLauncher
                                {
                                    Location = MediaLocationType.Data,
                                    Media = new Uri(webPage.EmbedUrl.ToString(), UriKind.Absolute),
                                    Orientation = MediaPlayerOrientation.Portrait
                                };
                                launcher.Show();
                                return;
                            }

                            if (!TLString.IsNullOrEmpty(webPage.Url))
                            {
                                var webBrowserTask = new WebBrowserTask();
                                webBrowserTask.URL = HttpUtility.UrlEncode(webPage.Url.ToString());
                                webBrowserTask.Show();
                            }

                            return;
                        }
                        if (string.Equals(type, "gif", StringComparison.OrdinalIgnoreCase))
                        {
                            var webPage35 = webPage as TLWebPage35;
                            if (webPage35 != null)
                            {
                                var document = webPage35.Document as TLDocument;
                                if (document == null) return;

                                var documentLocalFileName = document.GetFileName();
                                var store = IsolatedStorageFile.GetUserStoreForApplication();
#if WP81
                                var documentFile = mediaWebPage.File ?? await GetStorageFile(mediaWebPage);
#endif

                                if (!store.FileExists(documentLocalFileName)
#if WP81
 && documentFile == null
#endif
)
                                {
                                    mediaWebPage.IsCanceled = false;
                                    mediaWebPage.DownloadingProgress = mediaWebPage.LastProgress > 0.0 ? mediaWebPage.LastProgress : 0.001;
                                    DownloadDocumentFileManager.DownloadFileAsync(document.FileName, document.DCId, document.ToInputFileLocation(), message, document.Size,
                                        progress =>
                                        {
                                            if (progress > 0.0)
                                            {
                                                mediaWebPage.DownloadingProgress = progress;
                                            }
                                        });
                                }
                            }

                            return;
                        }
                    }
                }
            }

            var mediaGame = message.Media as TLMessageMediaGame;
            if (mediaGame != null)
            {
                var game = mediaGame.Game;
                if (game != null)
                {
                    var document = game.Document as TLDocument;
                    if (document == null) return;

                    var documentLocalFileName = document.GetFileName();
                    var store = IsolatedStorageFile.GetUserStoreForApplication();
#if WP81
                    var documentFile = mediaGame.File ?? await GetStorageFile(mediaGame);
#endif

                    if (!store.FileExists(documentLocalFileName)
#if WP81
 && documentFile == null
#endif
)
                    {
                        mediaGame.IsCanceled = false;
                        mediaGame.DownloadingProgress = mediaGame.LastProgress > 0.0 ? mediaGame.LastProgress : 0.001;
                        DownloadDocumentFileManager.DownloadFileAsync(document.FileName, document.DCId, document.ToInputFileLocation(), message, document.Size,
                            progress =>
                            {
                                if (progress > 0.0)
                                {
                                    mediaGame.DownloadingProgress = progress;
                                }
                            });
                    }
                }

                return;
            }

            var mediaGeo = message.Media as TLMessageMediaGeo;
            if (mediaGeo != null)
            {
                OpenLocation(message);

                return;
            }

            var mediaContact = message.Media as TLMessageMediaContact;
            if (mediaContact != null)
            {
                var phoneNumber = mediaContact.PhoneNumber.ToString();

                if (mediaContact.UserId.Value == 0)
                {
                    OpenPhoneContact(mediaContact);
                }
                else
                {
                    var user = mediaContact.User;

                    OpenMediaContact(mediaContact.UserId, user, new TLString(phoneNumber));
                }
                return;
            }

            var mediaVideo = message.Media as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                var video = mediaVideo.Video as TLVideo;
                if (video == null) return;

                var videoFileName = video.GetFileName();
                var store = IsolatedStorageFile.GetUserStoreForApplication();
#if WP81
                var file = mediaVideo.File ?? await GetStorageFile(mediaVideo);
#endif


                if (!store.FileExists(videoFileName)
#if WP81
 && file == null
#endif
)
                {
                    mediaVideo.IsCanceled = false;
                    mediaVideo.DownloadingProgress = mediaVideo.LastProgress > 0.0 ? mediaVideo.LastProgress : 0.001;
                    DownloadVideoFileManager.DownloadFileAsync(
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
                    //ReadMessageContents(message);

#if WP81

                    //var localFile = await GetFileFromLocalFolder(videoFileName);
                    var videoProperties = await file.Properties.GetVideoPropertiesAsync();
                    var musicProperties = await file.Properties.GetMusicPropertiesAsync();

                    if (file != null)
                    {
                        Launcher.LaunchFileAsync(file);
                        return;
                    }


#endif

                    var launcher = new MediaPlayerLauncher
                    {
                        Location = MediaLocationType.Data,
                        Media = new Uri(videoFileName, UriKind.Relative)
                    };
                    launcher.Show();
                }
                return;
            }

            var mediaAudio = message.Media as TLMessageMediaAudio;
            if (mediaAudio != null)
            {
                var audio = mediaAudio.Audio as TLAudio;
                if (audio == null) return;

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                var audioFileName = audio.GetFileName();
                if (TLString.Equals(audio.MimeType, new TLString("audio/mpeg"), StringComparison.OrdinalIgnoreCase))
                {
                    if (!store.FileExists(audioFileName))
                    {
                        mediaAudio.IsCanceled = false;
                        mediaAudio.DownloadingProgress = mediaAudio.LastProgress > 0.0 ? mediaAudio.LastProgress : 0.001;
                        BeginOnThreadPool(() =>
                        {
                            DownloadAudioFileManager.DownloadFile(audio.DCId, audio.ToInputFileLocation(), message, audio.Size);
                        });
                    }
                    else
                    {
                        ReadMessageContents(message as TLMessage25);
                    }

                    return;
                }

                var wavFileName = Path.GetFileNameWithoutExtension(audioFileName) + ".wav";

                if (!store.FileExists(wavFileName))
                {
                    mediaAudio.IsCanceled = false;
                    mediaAudio.DownloadingProgress = mediaAudio.LastProgress > 0.0 ? mediaAudio.LastProgress : 0.001;
                    BeginOnThreadPool(() =>
                    {
                        DownloadAudioFileManager.DownloadFile(audio.DCId, audio.ToInputFileLocation(), message, audio.Size);
                    });
                }
                else
                {
                    ReadMessageContents(message as TLMessage25);
                }

                if (mediaAudio.IsCanceled)
                {
                    mediaAudio.IsCanceled = false;
                    mediaAudio.NotifyOfPropertyChange(() => mediaPhoto.ThumbSelf);

                    return;
                }

                return;
            }

            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var document = mediaDocument.Document as TLDocument22;
                if (document != null)
                {
                    if (TLMessageBase.IsSticker(document))
                    {
                        var inputStickerSet = document.StickerSet;
                        if (inputStickerSet != null && !(inputStickerSet is TLInputStickerSetEmpty))
                        {
                            AddToStickers(message);
                        }
                    }
                    else if (TLMessageBase.IsVoice(document))
                    {
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
                        var audioFileName = string.Format("audio{0}_{1}.mp3", document.Id, document.AccessHash);
                        if (TLString.Equals(document.MimeType, new TLString("audio/mpeg"), StringComparison.OrdinalIgnoreCase))
                        {
                            if (!store.FileExists(audioFileName))
                            {
                                mediaDocument.IsCanceled = false;
                                mediaDocument.DownloadingProgress = mediaDocument.LastProgress > 0.0 ? mediaDocument.LastProgress : 0.001;
                                BeginOnThreadPool(() =>
                                {
                                    DownloadAudioFileManager.DownloadFile(document.DCId, document.ToInputFileLocation(), message, document.Size);
                                });
                            }
                            else
                            {
                                ReadMessageContents(message as TLMessage25);
                            }

                            return;
                        }

                        var wavFileName = Path.GetFileNameWithoutExtension(audioFileName) + ".wav";

                        if (!store.FileExists(wavFileName))
                        {
                            mediaDocument.IsCanceled = false;
                            mediaDocument.DownloadingProgress = mediaDocument.LastProgress > 0.0 ? mediaDocument.LastProgress : 0.001;
                            BeginOnThreadPool(() =>
                            {
                                DownloadAudioFileManager.DownloadFile(document.DCId, document.ToInputFileLocation(), message, document.Size);
                            });
                        }
                        else
                        {
                            ReadMessageContents(message as TLMessage25);
                        }

                        if (mediaDocument.IsCanceled)
                        {
                            mediaDocument.IsCanceled = false;
                            mediaDocument.NotifyOfPropertyChange(() => mediaDocument.ThumbSelf);

                            return;
                        }
                    }
                    else if (TLMessageBase.IsVideo(document))
                    {
                        var video = mediaDocument.Document as TLDocument;
                        if (video == null) return;

                        var videoFileName = video.GetFileName();
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
#if WP81
                        var file = mediaDocument.File ?? await GetStorageFile(mediaDocument);
#endif


                        if (!store.FileExists(videoFileName)
#if WP81
 && file == null
#endif
)
                        {
                            mediaDocument.IsCanceled = false;
                            mediaDocument.DownloadingProgress = mediaDocument.LastProgress > 0.0
                                ? mediaDocument.LastProgress
                                : 0.001;
                            DownloadVideoFileManager.DownloadFileAsync(
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
                            if (!message.Out.Value && message.HasTTL())
                            {
                                var ttlMessageMedia = mediaDocument as ITTLMessageMedia;
                                if (ttlMessageMedia != null && ttlMessageMedia.TTLParams == null)
                                {
                                    ttlMessageMedia.TTLParams = new TTLParams
                                    {
                                        StartTime = DateTime.Now,
                                        IsStarted = true,
                                        Total = ttlMessageMedia.TTLSeconds.Value
                                    };
                                    mediaDocument.NotifyOfPropertyChange(() => ttlMessageMedia.TTLParams);

                                    AddToTTLQueue(message as TLMessage70, ttlMessageMedia.TTLParams,
                                    result =>
                                    {
                                        SplitGroupedMessages(new List<TLInt> { message.Id });
                                    });
                                }

                                ReadMessageContents(message as TLMessage25);
                            }
                            else if (message.IsRoundVideo())
                            {
                                ReadMessageContents(message as TLMessage25);
                            }
#if WP81
                            if (file != null)
                            {
                                Launcher.LaunchFileAsync(file);
                                return;
                            }
#endif

                            var launcher = new MediaPlayerLauncher
                            {
                                Location = MediaLocationType.Data,
                                Media = new Uri(videoFileName, UriKind.Relative)
                            };
                            launcher.Show();
                        }
                        return;
                    }
                    else
                    {
                        OpenDocumentCommon(message, StateService, DownloadDocumentFileManager,
                            () =>
                            {
                                StateService.CurrentPhotoMessage = message;

                                if (AnimatedImageViewer == null)
                                {
                                    AnimatedImageViewer = new AnimatedImageViewerViewModel(StateService);
                                    NotifyOfPropertyChange(() => AnimatedImageViewer);
                                }

                                Execute.BeginOnUIThread(() => AnimatedImageViewer.OpenViewer());
                            });
                    }
                }
                else
                {
                    OpenDocumentCommon(message, StateService, DownloadDocumentFileManager,
                        () =>
                        {
                            StateService.CurrentPhotoMessage = message;

                            if (AnimatedImageViewer == null)
                            {
                                AnimatedImageViewer = new AnimatedImageViewerViewModel(StateService);
                                NotifyOfPropertyChange(() => AnimatedImageViewer);
                            }

                            Execute.BeginOnUIThread(() => AnimatedImageViewer.OpenViewer());
                        });
                }

                return;
            }

            Execute.ShowDebugMessage("tap on media");
        }

        private void OpenImageViewer()
        {
            if (ImageViewer == null)
            {
                ImageViewer = new ImageViewerViewModel(StateService, DownloadVideoFileManager, false, true)
                {
                    DialogDetails = this
                };
                NotifyOfPropertyChange(() => ImageViewer);
            }
            BeginOnUIThread(() => ImageViewer.OpenViewer());
        }


#if WP8
        public static async void OpenDocumentCommon(TLMessage message, IStateService stateService, IDocumentFileManager documentFileManager, System.Action openGifCallback)
#else
        public static void OpenDocumentCommon(TLMessage message, IStateService stateService, IDocumentFileManager documentFileManager, System.Action openGifCallback)
#endif
        {
            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                if (!string.IsNullOrEmpty(mediaDocument.IsoFileName))
                {

                }

                var document = mediaDocument.Document as TLDocument;
                if (document == null) return;

                var documentLocalFileName = document.GetFileName();
                var store = IsolatedStorageFile.GetUserStoreForApplication();
#if WP81
                var documentFile = mediaDocument.File ?? await GetStorageFile(mediaDocument);
#endif

                if (!store.FileExists(documentLocalFileName)
#if WP81
 && documentFile == null
#endif
)
                {

                    if (document.Size.Value == 0) return;

                    mediaDocument.IsCanceled = false;
                    mediaDocument.DownloadingProgress = mediaDocument.LastProgress > 0.0 ? mediaDocument.LastProgress : 0.001;
                    //_downloadVideoStopwatch = Stopwatch.StartNew();
                    //return;
                    documentFileManager.DownloadFileAsync(
                        document.FileName, document.DCId, document.ToInputFileLocation(), message, document.Size,
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
                    if (message.IsGif())
                    {
                        if (documentFile != null && File.Exists(documentFile.Path))
                        {
                            mediaDocument.DownloadingProgress = 0.001;
                            await documentFile.CopyAsync(ApplicationData.Current.LocalFolder, documentLocalFileName, NameCollisionOption.ReplaceExisting);
                            mediaDocument.DownloadingProgress = 0.0;
                        }

                        return;
                    }

                    if (documentLocalFileName.EndsWith(".gif")
                        || string.Equals(document.MimeType.ToString(), "image/gif", StringComparison.OrdinalIgnoreCase))
                    {
                        openGifCallback.SafeInvoke();

                        return;
                    }

                    if (documentLocalFileName.EndsWith(".mp3")
                        || string.Equals(document.MimeType.ToString(), "audio/mpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        var url = new Uri(documentLocalFileName, UriKind.Relative);
                        var title = document.FileName.ToString();
                        var performer = "Unknown Artist";
                        var readId3Tags = true;
#if WP81

                        try
                        {
                            var storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(documentLocalFileName);
                            var audioProperties = await storageFile.Properties.GetMusicPropertiesAsync();
                            title = audioProperties.Title;
                            performer = audioProperties.Artist;
                            readId3Tags = false;
                        }
                        catch (Exception ex) { }
#endif
#if WP81
                        if (documentFile == null)
                        {
                            try
                            {
                                documentFile = await ApplicationData.Current.LocalFolder.GetFileAsync(documentLocalFileName);
                            }
                            catch (Exception ex)
                            {
                                Execute.ShowDebugMessage("LocalFolder.GetFileAsync docLocal exception \n" + ex);
                            }
                        }
                        Launcher.LaunchFileAsync(documentFile);
                        return;
#elif WP8
                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(documentLocalFileName);
                        Launcher.LaunchFileAsync(file);
                        return;
#endif

                        //if (readId3Tags)
                        //{
                        //    if (store.FileExists(documentLocalFileName))
                        //    {
                        //        using (var file = store.OpenFile(documentLocalFileName, FileMode.Open, FileAccess.Read))
                        //        {
                        //            var mp3Stream = new Mp3Stream(file);
                        //            if (mp3Stream.HasTags)
                        //            {
                        //                var tag = mp3Stream.GetTag(Id3TagFamily.FileStartTag);
                        //                title = tag.Title;
                        //                performer = tag.Artists;
                        //            }
                        //        }
                        //    }
                        //}

                        //var track = BackgroundAudioPlayer.Instance.Track;
                        //if (track == null || track.Source != url)
                        //{
                        //    BackgroundAudioPlayer.Instance.Track = new AudioTrack(url, title, performer, null, null);
                        //}
                        //BackgroundAudioPlayer.Instance.Play();

                        return;
                    }
                    else
                    {
#if WP81
                        if (documentFile == null)
                        {
                            try
                            {
                                documentFile = await ApplicationData.Current.LocalFolder.GetFileAsync(documentLocalFileName);
                            }
                            catch (Exception ex)
                            {
                                Execute.ShowDebugMessage("LocalFolder.GetFileAsync docLocal exception \n" + ex);
                            }
                        }
                        Launcher.LaunchFileAsync(documentFile);
                        return;
#elif WP8
                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(documentLocalFileName);
                        Launcher.LaunchFileAsync(file);
                        return;
#endif
                    }
                }
                return;
            }
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

            if (View != null)
            {
                View.CreateBitmapCache(() => BeginOnUIThread(() =>
                {
                    StateService.CurrentContact = user;
                    StateService.CurrentContactPhone = phoneNumber;
                    NavigationService.UriFor<ContactViewModel>().Navigate();
                }));
            }
        }

        public void Attach()
        {
            if (ChooseAttachment == null)
            {
                ChooseAttachment = new ChooseAttachmentViewModel(With, OpenInlineBot, SendDocument, SendVideo, SendPhoto, SendLocation, OpenContact, CacheService, EventAggregator, NavigationService, StateService);
                NotifyOfPropertyChange(() => ChooseAttachment);
            }
            BeginOnUIThread(() => ChooseAttachment.Open());
        }
    }
}
