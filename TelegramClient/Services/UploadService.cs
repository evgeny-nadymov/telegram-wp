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
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.Location;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views;
using TelegramClient.Views.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Services
{
    public class UploadService : IUploadService, Telegram.Api.Aggregator.IHandle<UploadableItem>
    {
        private IStateService _stateService;

        private IStateService StateService
        {
            get { return _stateService ?? (_stateService = IoC.Get<IStateService>()); }
        }

        private IMTProtoService _mtProtoService;

        private IMTProtoService MTProtoService
        {
            get { return _mtProtoService ?? (_mtProtoService = IoC.Get<IMTProtoService>()); }
        }

        private ICacheService _cacheService;

        private ICacheService CacheService
        {
            get { return _cacheService ?? (_cacheService = IoC.Get<ICacheService>()); }
        }

        public UploadService(ITelegramEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void Handle(UploadableItem item)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var mediaDocument = item.Owner as TLMessageMediaDocument;
                if (mediaDocument != null)
                {

                    var m = CacheService.GetSendingMessages()
                            .OfType<TLMessage>()
                            .FirstOrDefault(x => x.Media == mediaDocument);

                    if (m == null)
                    {
                        Execute.ShowDebugMessage("ShellViewModel.Handl UploadableItem sending message=null");
                        return;
                    }

                    try
                    {
                        var document = (TLDocument)((TLMessageMediaDocument)m.Media).Document;

                        var caption = document.FileName;
                        var uploadedThumb = new TLInputFile
                        {
                            Id = item.FileId,
                            MD5Checksum = new TLString(""),
                            Name = new TLString(caption + ".jpg"),
                            Parts = new TLInt(item.Parts.Count)
                        };

                        ((TLDocument)((TLMessageMediaDocument)m.Media).Document).ThumbInputFile = uploadedThumb;

                    }
                    catch (Exception e)
                    {

                    }
                }

                var mediaVideo = item.Owner as TLMessageMediaVideo;
                if (mediaVideo != null)
                {

                    var m = CacheService.GetSendingMessages()
                        .OfType<TLMessage>()
                        .FirstOrDefault(x => x.Media == mediaVideo);

                    if (m == null) return;

                    var caption = ((TLVideo)((TLMessageMediaVideo)m.Media).Video).Caption;
                    var uploadedThumb = new TLInputFile
                    {
                        Id = item.FileId,
                        //MD5Checksum = new TLString(MD5Core.GetHashString(item.Bytes).ToLowerInvariant()),
                        MD5Checksum = new TLString(""),
                        Name = new TLString(caption + ".jpg"),
                        Parts = new TLInt(item.Parts.Count)
                    };

                    ((TLVideo)((TLMessageMediaVideo)m.Media).Video).ThumbInputFile = uploadedThumb;
                }
            });

            var message = item.Owner as TLMessage34;
            if (message != null)
            {
                Telegram.Logs.Log.Write("ShellViewModel.HandleUploadableItem rnd_id=" + message.RandomId);
                HandleUploadableItemInternal(item, message);
            }

            var decryptedMessage = item.Owner as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                HandleUploadableEncryptedItemInternal(item, decryptedMessage);
            }

            var decryptedMessageLayer17 = item.Owner as TLDecryptedMessageLayer17;
            if (decryptedMessageLayer17 != null)
            {
                HandleUploadableEncryptedItemInternal(item, decryptedMessageLayer17);
            }
        }

        private void HandleUploadableItemInternal(UploadableItem item, TLMessage34 message)
        {
            Execute.BeginOnUIThread(() =>
            {
                if (item.FileNotFound)
                {
                    message.Media.UploadingProgress = 1.0;
                    message.Status = MessageStatus.Failed;

                    Telegram.Logs.Log.Write("ShellViewModel.HandleUploadableItemInternal item.FileNotFound");
                    return;
                }

                if (message.Status == MessageStatus.Failed)
                {
                    Telegram.Logs.Log.Write("ShellViewModel.HandleUploadableItemInternal message.Status==Failed rnd_id=" + message.RandomIndex);
                    return;
                }

                message.Media.UploadingProgress = 1.0;

                var audioMedia = message.Media as TLMessageMediaAudio;
                if (audioMedia != null)
                {
                    var audio = audioMedia.Audio as TLAudio;
                    if (audio == null)
                    {
                        Telegram.Logs.Log.Write("ShellViewModel.HandleUploadableItemInternal audio==null rnd_id=" + message.RandomIndex);
                        return;
                    }

                    var uploadedAudio = new TLInputMediaUploadedAudio
                    {
                        Duration = audio.Duration,
                        File = new TLInputFile
                        {
                            Id = item.FileId,
                            MD5Checksum = TLString.Empty,
                            Name = new TLString(audioMedia.IsoFileName),
                            Parts = new TLInt(item.Parts.Count)
                        },
                        MimeType = audio.MimeType
                    };

                    message.InputMedia = uploadedAudio;
                    SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    return;
                }

                var mediaDocument = message.Media as TLMessageMediaDocument70;
                if (mediaDocument != null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document == null)
                    {

                        Telegram.Logs.Log.Write("ShellViewModel.HandleUploadableItemInternal document==null rnd_id=" + message.RandomIndex);
                        return;
                    }

                    TLInputFileBase file;
                    if (item.IsSmallFile || message.IsVoice())
                    {
                        file = new TLInputFile
                        {
                            Id = item.FileId,
                            MD5Checksum = TLString.Empty,
                            Name = document.FileName,
                            Parts = new TLInt(item.Parts.Count)
                        };
                    }
                    else
                    {
                        file = new TLInputFileBig
                        {
                            Id = item.FileId,
                            Name = document.FileName,
                            Parts = new TLInt(item.Parts.Count)
                        };
                    }

                    TLInputMediaBase uploadedDocument;
                    if (document.ThumbInputFile != null)
                    {
                        uploadedDocument = new TLInputMediaUploadedDocument75
                        {
                            MD5Hash = new byte[0],
                            Flags = new TLInt(0),
                            Attributes = new TLVector<TLDocumentAttributeBase> { new TLDocumentAttributeFileName { FileName = document.FileName } },
                            MimeType = document.MimeType,
                            File = file,
                            Thumb = document.ThumbInputFile,
                            Caption = TLString.Empty,
                            Stickers = null,
                            TTLSeconds = mediaDocument.TTLSeconds
                        };
                    }
                    else
                    {
                        uploadedDocument = new TLInputMediaUploadedDocument75
                        {
                            MD5Hash = new byte[] { },
                            Flags = new TLInt(0),
                            Attributes = new TLVector<TLDocumentAttributeBase> { new TLDocumentAttributeFileName { FileName = document.FileName } },
                            MimeType = document.MimeType,
                            File = file,
                            Caption = TLString.Empty,
                            Stickers = null,
                            TTLSeconds = mediaDocument.TTLSeconds
                        };
                    }

                    var attributes = uploadedDocument as IAttributes;
                    if (attributes != null)
                    {
                        var document22 = document as TLDocument22;
                        if (document22 != null)
                        {
                            if (message.IsVoice())
                            {
                                var audioAttribute = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio) as TLDocumentAttributeAudio;
                                if (audioAttribute != null)
                                {
                                    attributes.Attributes.Add(audioAttribute);
                                }
                            }

                            if (message.IsMusic())
                            {
                                var audioAttribute = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio) as TLDocumentAttributeAudio;
                                if (audioAttribute != null)
                                {
                                    attributes.Attributes.Add(audioAttribute);
                                }
                            }

                            if (message.IsVideo())
                            {
                                var videoAttribute = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeVideo) as TLDocumentAttributeVideo;
                                if (videoAttribute != null)
                                {
                                    attributes.Attributes.Add(videoAttribute);
                                }
                            }
                        }
                    }

                    message.InputMedia = uploadedDocument;
                    SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    return;
                }

                var photoMedia = message.Media as TLMessageMediaPhoto70;
                if (photoMedia != null)
                {
                    var message73 = message as TLMessage73;
                    TLObject groupedMessage;
                    if (message73 != null && message73.GroupedId != null && _group.TryGetValue(message73.GroupedId.Value, out groupedMessage))
                    {
                        var groupedMessage73 = groupedMessage as TLMessage73;
                        if (groupedMessage73 != null)
                        {
                            var uploadedPhoto = new TLInputMediaUploadedPhoto75
                            {
                                Flags = new TLInt(0),
                                MD5Hash = new byte[] { },
                                File = new TLInputFile
                                {
                                    Id = item.FileId,
                                    MD5Checksum = TLString.Empty,
                                    Name = new TLString("file" + message73.RandomId + ".jpg"),
                                    Parts = new TLInt(item.Parts.Count)
                                },
                                Caption = TLString.Empty,
                                Stickers = GetStickers(message),
                                TTLSeconds = photoMedia.TTLSeconds
                            };

                            MTProtoService.UploadMediaAsync(DialogDetailsViewModel.PeerToInputPeer(message.ToId), uploadedPhoto,
                                result =>
                                {
                                    var mediaPhoto = result as TLMessageMediaPhoto70;
                                    if (mediaPhoto != null)
                                    {
                                        var photo = mediaPhoto.Photo as TLPhoto56;
                                        if (photo != null)
                                        {
                                            var inputPhoto = new TLInputPhoto
                                            {
                                                Id = photo.Id,
                                                AccessHash = photo.AccessHash
                                            };

                                            var inputMediaPhoto = new TLInputMediaPhoto75
                                            {
                                                Flags = new TLInt(0),
                                                Id = inputPhoto,
                                                Caption = TLString.Empty,
                                                TTLSeconds = photoMedia.TTLSeconds
                                            };

                                            var singleMedia = new TLInputSingleMedia76
                                            {
                                                Flags = new TLInt(0),
                                                MD5Hash = new byte[] { },
                                                RandomId = message73.RandomId,
                                                Media = inputMediaPhoto,
                                                Message = message.Message,
                                                Entities = message.Entities != null && message.Entities.Count > 0 ? message.Entities : null
                                            };

                                            var groupUploaded = false;
                                            var inputMedia = new TLVector<TLInputSingleMedia>();
                                            lock (groupedMessage)
                                            {
                                                message.InputMedia = singleMedia;
                                                var mediaGroup = groupedMessage73.Media as TLMessageMediaGroup;
                                                if (mediaGroup != null && mediaGroup.Group.OfType<TLMessage>().All(x => x.InputMedia != null))
                                                {
                                                    foreach (var m in mediaGroup.Group.OfType<TLMessage>())
                                                    {
                                                        singleMedia = m.InputMedia as TLInputSingleMedia76;
                                                        if (singleMedia == null)
                                                        {
                                                            break;
                                                        }
                                                        inputMedia.Add(singleMedia);
                                                    }
                                                    groupUploaded = true;
                                                }
                                            }

                                            if (groupUploaded)
                                            {
                                                SendMultiMediaInternal(inputMedia, (TLMessage25)groupedMessage, MTProtoService, StateService, CacheService);
                                            }
                                        }
                                    }
                                },
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    Telegram.Logs.Log.Write("ShellViewModel.UploadMediaAsync error=" + error);
                                    if (error.TypeEquals(ErrorType.PEER_FLOOD))
                                    {
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
                                        Execute.ShowDebugMessage("messages.uploadMedia error " + error);
                                    }
                                    if (groupedMessage73.Status == MessageStatus.Sending)
                                    {
                                        groupedMessage73.Status = groupedMessage73.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                                    }
                                }));
                        }
                    }
                    else
                    {
                        var uploadedPhoto = new TLInputMediaUploadedPhoto75
                        {
                            Flags = new TLInt(0),
                            MD5Hash = new byte[] { },
                            File = new TLInputFile
                            {
                                Id = item.FileId,
                                MD5Checksum = TLString.Empty,
                                Name = new TLString("file.jpg"),
                                Parts = new TLInt(item.Parts.Count)
                            },
                            Caption = TLString.Empty,
                            Stickers = GetStickers(message),
                            TTLSeconds = photoMedia.TTLSeconds
                        };
                        message.InputMedia = uploadedPhoto;
                        SendMediaInternal(message, MTProtoService, StateService, CacheService);
                    }
                }
            });
        }

        private TLVector<TLInputDocumentBase> GetStickers(TLMessage25 message)
        {
            var stickers = new TLVector<TLInputDocumentBase>();
            var message48 = message as TLMessage48;
            if (message48 != null)
            {
                if (message48.Documents != null)
                {
                    foreach (var documentBase in message48.Documents)
                    {
                        var document = documentBase as TLDocument;
                        if (document != null)
                        {
                            stickers.Add(new TLInputDocument{ Id = document.Id, AccessHash = document.AccessHash });
                        }
                    }

                    if (stickers.Count > 0)
                    {
                        return stickers;
                    }
                }
            }

            return null;
        }

        public static void SendMultiMediaInternal(TLVector<TLInputSingleMedia> inputMedia, TLMessage25 message, IMTProtoService mtProtoService, IStateService stateService, ICacheService cacheService)
        {
            Telegram.Logs.Log.Write("ShellViewModel.SendMultiMediaInternal rnd_id=" + message.RandomId);

            var inputPeer = DialogDetailsViewModel.PeerToInputPeer(message.ToId);

            TLPhotoSize[] photo = null;
            TLDocument[] document = null;
            var mediaGroup = message.Media as TLMessageMediaGroup;
            if (mediaGroup != null)
            {
                photo = new TLPhotoSize[mediaGroup.Group.Count];
                document = new TLDocument[mediaGroup.Group.Count];

                for (var i = 0; i < mediaGroup.Group.Count; i++)
                {
                    photo[i] = GetPhotoSize(mediaGroup.Group[i]);
                    document[i] = GetDocument(mediaGroup.Group[i]);
                }
            }

            Telegram.Logs.Log.Write("ShellViewModel.SendMultiMediaAsync rnd_id=" + message.RandomId);
            mtProtoService.SendMultiMediaAsync(
                inputPeer, inputMedia, message,
                updates =>
                {
                    Telegram.Logs.Log.Write("ShellViewModel.SendMultiMediaAsync result=" + updates);
                    if (mediaGroup != null && photo != null && document != null)
                    {
                        for (var i = 0; i < mediaGroup.Group.Count; i++)
                        {
                            ProcessSentPhoto(mediaGroup.Group[i], photo[i], stateService);
                            ProcessSentDocument(mediaGroup.Group[i], photo[i], document[i], stateService);
                        }
                    }
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    Telegram.Logs.Log.Write("ShellViewModel.SendMultiMediaAsync error=" + error);
                    if (error.TypeEquals(ErrorType.PEER_FLOOD))
                    {
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
                        Execute.ShowDebugMessage("messages.sendMultiMedia error " + error);
                    }
                    if (message.Status == MessageStatus.Sending)
                    {
                        message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                        if (mediaGroup != null && photo != null && document != null)
                        {
                            for (var i = 0; i < mediaGroup.Group.Count; i++)
                            {
                                mediaGroup.Group[i].Status = mediaGroup.Group[i].Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                            }
                        }
                    }
                }));
        }

        public static void SendMediaInternal(TLMessage34 message, IMTProtoService mtProtoService, IStateService stateService, ICacheService cacheService)
        {
            Telegram.Logs.Log.Write("ShellViewModel.SendMediaInternal rnd_id=" + message.RandomId);

            var inputPeer = DialogDetailsViewModel.PeerToInputPeer(message.ToId);

            if (inputPeer is TLInputPeerBroadcast && !(inputPeer is TLInputPeerChannel))
            {
                var broadcast = IoC.Get<ICacheService>().GetBroadcast(message.ToId.Id);
                var contacts = new TLVector<TLInputUserBase>();

                foreach (var participantId in broadcast.ParticipantIds)
                {
                    var contact = IoC.Get<ICacheService>().GetUser(participantId);
                    contacts.Add(contact.ToInputUser());
                }

                mtProtoService.SendBroadcastAsync(contacts, message.InputMedia, message,
                    result =>
                    {
                        message.Status = MessageStatus.Confirmed;
                    },
                    () =>
                    {
                        message.Status = MessageStatus.Confirmed;
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.sendBroadcast error " + error);
                        if (message.Status == MessageStatus.Broadcast)
                        {
                            message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                        }
                    });
            }
            else
            {
                var photoSize = GetPhotoSize(message);
                var document = GetDocument(message);
                Telegram.Logs.Log.Write("ShellViewModel.SendMediaAsync rnd_id=" + message.RandomId);
                mtProtoService.SendMediaAsync(
                    inputPeer, message.InputMedia, message,
                    updates =>
                    {
                        Telegram.Logs.Log.Write("ShellViewModel.SendMediaAsync result=" + updates);
                        ProcessSentPhoto(message, photoSize, stateService);
                        ProcessSentDocument(message, photoSize, document, stateService);
                        ProcessSentAudio(message);
                        ProcessSentGeoPoint(message);
                    },
                    error => Execute.BeginOnUIThread(() =>
                    {
                        Telegram.Logs.Log.Write("ShellViewModel.SendMediaAsync error=" + error);
                        if (error.TypeEquals(ErrorType.PEER_FLOOD))
                        {
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
                            Execute.ShowDebugMessage("messages.sendMedia error: " + error);
                        }
                        if (message.Status == MessageStatus.Sending)
                        {
                            message.Status = message.Index != 0 ? MessageStatus.Confirmed : MessageStatus.Failed;
                        }
                    }));
            }
        }

        private static void ProcessSentGeoPoint(TLMessage25 m)
        {
            var message70 = m as TLMessage70;
            if (message70 != null)
            {
                var mediaGeoLive = m.Media as TLMessageMediaGeoLive;
                if (mediaGeoLive != null)
                {
                    var liveLocationsService = IoC.Get<ILiveLocationService>();

                    liveLocationsService.Add(message70);
                } 
            }
        }

        private static TLDocument GetDocument(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message != null)
            {
                var mediaDocument = message.Media as TLMessageMediaDocument;
                if (mediaDocument != null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document != null)
                    {
                        return document;
                    }
                }
            }

            return null;
        }

        private static void ProcessSentPhoto(TLMessageBase messageBase, TLPhotoSize oldPhotoSize, IStateService stateService)
        {
            var message = messageBase as TLMessage;
            if (message != null && message.InputMedia != null && message.InputMedia.MD5Hash != null)
            {
                var mediaPhoto = message.Media as TLMessageMediaPhoto;
                if (mediaPhoto != null)
                {
                    var photo = mediaPhoto.Photo as TLPhoto;
                    if (photo != null)
                    {
                        Execute.BeginOnThreadPool(() =>
                        {
                            if (oldPhotoSize != null)
                            {
                                var newPhotoSizeM = photo.Sizes.FirstOrDefault(x => TLString.Equals(x.Type, new TLString("m"), StringComparison.OrdinalIgnoreCase)) as TLPhotoSize;
                                var newPhotoSizeX = photo.Sizes.FirstOrDefault(x => TLString.Equals(x.Type, new TLString("x"), StringComparison.OrdinalIgnoreCase)) as TLPhotoSize;

                                var oldFileLocation = string.Format("{0}_{1}_{2}.jpg", oldPhotoSize.Location.VolumeId, oldPhotoSize.Location.LocalId, oldPhotoSize.Location.Secret);
                                var newFileLocationM = newPhotoSizeM != null ? string.Format("{0}_{1}_{2}.jpg", newPhotoSizeM.Location.VolumeId, newPhotoSizeM.Location.LocalId, newPhotoSizeM.Location.Secret) : string.Empty;
                                var newFileLocationX = newPhotoSizeX != null ? string.Format("{0}_{1}_{2}.jpg", newPhotoSizeX.Location.VolumeId, newPhotoSizeX.Location.LocalId, newPhotoSizeX.Location.Secret) : string.Empty;
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(oldFileLocation))
                                    {
                                        if (!string.IsNullOrEmpty(newFileLocationM) && !string.Equals(oldFileLocation, newFileLocationM, StringComparison.OrdinalIgnoreCase)) store.CopyFile(oldFileLocation, newFileLocationM, true);
                                        if (!string.IsNullOrEmpty(newFileLocationX) && !string.Equals(oldFileLocation, newFileLocationX, StringComparison.OrdinalIgnoreCase)) store.CopyFile(oldFileLocation, newFileLocationX, true);
                                        store.DeleteFile(oldFileLocation);
                                    }
                                }
                            }
                        });

                        if (message.InputMedia.MD5Hash.Length > 0)
                        {
                            AddServerFileAsync(stateService, new TLServerFile
                            {
                                MD5Checksum = new TLLong(BitConverter.ToInt64(message.InputMedia.MD5Hash, 0)),
                                Media = new TLInputMediaPhoto75 { Flags = new TLInt(0), Id = new TLInputPhoto { Id = photo.Id, AccessHash = photo.AccessHash }, Caption = TLString.Empty }
                            });
                        }
                    }
                }
            }
        }

        private static void AddServerFileAsync(IStateService stateService, TLServerFile file)
        {
            stateService.GetServerFilesAsync(
                results =>
                {
                    results.Add(file);
                    stateService.SaveServerFilesAsync(results);
                });
        }

        private static void ProcessSentDocument(TLMessageBase messageBase, TLPhotoSize oldPhotoSize, TLDocument oldDocument, IStateService stateService)
        {
            var message = messageBase as TLMessage;
            if (message != null && message.InputMedia != null && message.InputMedia.MD5Hash != null)
            {
                var mediaDocument = message.Media as TLMessageMediaDocument;
                if (mediaDocument != null)
                {
                    var newDocument = mediaDocument.Document as TLDocument;
                    if (newDocument != null)
                    {
                        Execute.BeginOnThreadPool(() =>
                        {
                            if (message.IsGif())
                            {
                                if (oldDocument != null)
                                {
                                    var oldDocumentFileName = oldDocument.GetFileName();
                                    var newDocumentFileName = newDocument.GetFileName();
                                    if (!string.Equals(oldDocumentFileName, newDocumentFileName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                        {
                                            if (store.FileExists(oldDocumentFileName))
                                            {
                                                if (!string.IsNullOrEmpty(newDocumentFileName))
                                                {
                                                    store.CopyFile(oldDocumentFileName, newDocumentFileName, true);
                                                }

                                                Execute.BeginOnUIThread(() =>
                                                {
                                                    message.NotifyOfPropertyChange(() => message.Media);

                                                    //Execute.BeginOnThreadPool(TimeSpan.FromSeconds(2.0), () =>
                                                    //{
                                                    //    using (var store2 = IsolatedStorageFile.GetUserStoreForApplication())
                                                    //    {
                                                    //        store2.DeleteFile(oldDocumentFileName);
                                                    //    }
                                                    //});
                                                });
                                            }
                                        }
                                    }
                                }
                            }

                            if (oldPhotoSize != null)
                            {
                                var newPhotoSize = newDocument.Thumb as TLPhotoSize;

                                var oldFileLocation = string.Format("{0}_{1}_{2}.jpg", oldPhotoSize.Location.VolumeId, oldPhotoSize.Location.LocalId, oldPhotoSize.Location.Secret);
                                var newFileLocation = newPhotoSize != null ? string.Format("{0}_{1}_{2}.jpg", newPhotoSize.Location.VolumeId, newPhotoSize.Location.LocalId, newPhotoSize.Location.Secret) : string.Empty;
                                if (!string.Equals(oldFileLocation, newFileLocation, StringComparison.OrdinalIgnoreCase))
                                {
                                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                    {
                                        if (store.FileExists(oldFileLocation))
                                        {
                                            if (!string.IsNullOrEmpty(newFileLocation)) store.CopyFile(oldFileLocation, newFileLocation, true);
                                            store.DeleteFile(oldFileLocation);
                                        }
                                    }
                                }
                            }

                        });

                        if (message.InputMedia.MD5Hash.Length >= 8)
                        {
                            AddServerFileAsync(stateService, new TLServerFile
                            {
                                MD5Checksum = new TLLong(BitConverter.ToInt64(message.InputMedia.MD5Hash, 0)),
                                Media = new TLInputMediaDocument75 { Flags = new TLInt(0), Id = new TLInputDocument { Id = newDocument.Id, AccessHash = newDocument.AccessHash }, Caption = TLString.Empty }
                                //Media = m.InputMedia
                            });
                        }
                    }
                }
            }
        }

        private static void ProcessSentAudio(TLMessage message)
        {
            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null
                && (TLMessageBase.IsMusic(mediaDocument.Document) || TLMessageBase.IsVoice(mediaDocument.Document)))
            {

                // иначе в UI остается в качестве DataContext отправляетмый TLAudio с рандомным Id, AccessHash
                message.NotifyOfPropertyChange(() => message.Media);

                // rename local copy of uploaded media
                var sourceFileName = message.Media.IsoFileName;
                if (string.IsNullOrEmpty(sourceFileName)) return;

                var wavSourceFileName = Path.GetFileNameWithoutExtension(sourceFileName) + ".wav";

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(sourceFileName))
                    {
                        store.DeleteFile(sourceFileName);
                    }

                    if (store.FileExists(wavSourceFileName))
                    {
                        var audio = mediaDocument.Document as TLDocument;
                        if (audio != null)
                        {
                            var destinationFileName = string.Format("audio{0}_{1}.mp3", audio.Id, audio.AccessHash);//audio.GetFileName());

                            if (!string.IsNullOrEmpty(destinationFileName))
                            {
                                var wavDestinationFileName = Path.GetFileNameWithoutExtension(destinationFileName) + ".wav";
                                store.MoveFile(wavSourceFileName, wavDestinationFileName);
                            }
                        }
                    }
                }
                return;
            }

            var mediaAudio = message.Media as TLMessageMediaAudio;
            if (mediaAudio != null)
            {

                // иначе в UI остается в качестве DataContext отправляетмый TLAudio с рандомным Id, AccessHash
                message.NotifyOfPropertyChange(() => message.Media);

                // rename local copy of uploaded media
                var sourceFileName = message.Media.IsoFileName;
                if (string.IsNullOrEmpty(sourceFileName)) return;

                var wavSourceFileName = Path.GetFileNameWithoutExtension(sourceFileName) + ".wav";

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(sourceFileName))
                    {
                        store.DeleteFile(sourceFileName);
                    }

                    if (store.FileExists(wavSourceFileName))
                    {
                        var audio = mediaAudio.Audio as TLAudio;
                        if (audio != null)
                        {
                            var destinationFileName = audio.GetFileName();

                            if (!string.IsNullOrEmpty(destinationFileName))
                            {
                                var wavDestinationFileName = Path.GetFileNameWithoutExtension(destinationFileName) + ".wav";
                                store.MoveFile(wavSourceFileName, wavDestinationFileName);
                            }
                        }
                    }
                }
                return;
            }
        }

        private static TLPhotoSize GetPhotoSize(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message != null)
            {
                var mediaPhoto = message.Media as TLMessageMediaPhoto;
                if (mediaPhoto != null)
                {
                    var photo = mediaPhoto.Photo as TLPhoto;
                    if (photo != null)
                    {
                        return photo.Sizes.FirstOrDefault() as TLPhotoSize;
                    }
                }

                var mediaDocument = message.Media as TLMessageMediaDocument;
                if (mediaDocument != null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document != null)
                    {
                        return document.Thumb as TLPhotoSize;
                    }
                }
            }

            return null;
        }

        private void HandleUploadableEncryptedItemInternal(UploadableItem item, TLObject obj)
        {
            var message = SecretDialogDetailsViewModel.GetDecryptedMessage(obj);
            if (message == null) return;

            var mediaPhoto = message.Media as TLDecryptedMessageMediaPhoto;
            if (mediaPhoto != null)
            {
                mediaPhoto.UploadingProgress = 1.0;

                var fileLocation = mediaPhoto.File as TLEncryptedFile;
                if (fileLocation == null) return;

                message.InputFile = item.IsSmallFile ?
                    GetInputFile(item.FileId, new TLInt(item.Parts.Count), mediaPhoto.Key, mediaPhoto.IV) :
                    GetInputFileBig(item.FileId, new TLInt(item.Parts.Count), mediaPhoto.Key, mediaPhoto.IV);

                var chatId = message.ChatId;
                if (chatId == null) return;

                var chat = CacheService.GetEncryptedChat(chatId) as TLEncryptedChat;
                if (chat == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);

                    return;
                }

                var groupHandled = false;
                var message73 = message as TLDecryptedMessage73;
                if (message73 != null && message73.GroupedId != null)
                {
                    TLObject groupedMessage;
                    if (_group.TryGetValue(message73.GroupedId.Value, out groupedMessage))
                    {
                        var groupedMessage73 = groupedMessage as TLDecryptedMessage73;
                        if (groupedMessage73 != null)
                        {
                            groupHandled = true;

                            var mediaGroup = groupedMessage73.Media as TLDecryptedMessageMediaGroup;
                            if (mediaGroup != null && mediaGroup.Group.OfType<TLDecryptedMessage>().All(x => x.InputFile != null))
                            {
                                SecretDialogDetailsViewModel.SendEncryptedMultiMediaInternal(chat, groupedMessage73, MTProtoService, CacheService);
                            }
                        }
                    }
                }

                if (!groupHandled)
                {
                    SecretDialogDetailsViewModel.SendEncryptedMediaInternal(chat, obj, MTProtoService, CacheService);
                }
            }

            var mediaDocument = message.Media as TLDecryptedMessageMediaDocument;
            if (mediaDocument != null)
            {
                mediaDocument.UploadingProgress = 1.0;

                var fileLocation = mediaDocument.File as TLEncryptedFile;
                if (fileLocation == null) return;

                message.InputFile = item.IsSmallFile || message.IsVoice() ?
                    GetInputFile(item.FileId, new TLInt(item.Parts.Count), mediaDocument.Key, mediaDocument.IV) :
                    GetInputFileBig(item.FileId, new TLInt(item.Parts.Count), mediaDocument.Key, mediaDocument.IV);

                var chatId = message.ChatId;
                if (chatId == null) return;

                var chat = CacheService.GetEncryptedChat(chatId) as TLEncryptedChat;
                if (chat == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);

                    return;
                }

                var groupHandled = false;
                var message73 = message as TLDecryptedMessage73;
                if (message73 != null && message73.GroupedId != null)
                {
                    TLObject groupedMessage;
                    if (_group.TryGetValue(message73.GroupedId.Value, out groupedMessage))
                    {
                        var groupedMessage73 = groupedMessage as TLDecryptedMessage73;
                        if (groupedMessage73 != null)
                        {
                            groupHandled = true;

                            var mediaGroup = groupedMessage73.Media as TLDecryptedMessageMediaGroup;
                            if (mediaGroup != null && mediaGroup.Group.OfType<TLDecryptedMessage>().All(x => x.InputFile != null))
                            {
                                SecretDialogDetailsViewModel.SendEncryptedMultiMediaInternal(chat, groupedMessage73, MTProtoService, CacheService);
                            }
                        }
                    }
                }

                if (!groupHandled)
                {
                    SecretDialogDetailsViewModel.SendEncryptedMediaInternal(chat, obj, MTProtoService, CacheService);
                }
            }

            var mediaVideo = message.Media as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null)
            {
                mediaVideo.UploadingProgress = 1.0;

                var fileLocation = mediaVideo.File as TLEncryptedFile;
                if (fileLocation == null) return;

                message.InputFile = GetInputFileBig(item.FileId, new TLInt(item.Parts.Count), mediaVideo.Key, mediaVideo.IV);

                var chatId = message.ChatId;
                if (chatId == null) return;

                var chat = CacheService.GetEncryptedChat(chatId) as TLEncryptedChat;
                if (chat == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);

                    return;
                }

                SecretDialogDetailsViewModel.SendEncryptedMediaInternal(chat, obj, MTProtoService, CacheService);
            }

            var mediaAudio = message.Media as TLDecryptedMessageMediaAudio;
            if (mediaAudio != null)
            {
                mediaAudio.UploadingProgress = 1.0;

                var fileLocation = mediaAudio.File as TLEncryptedFile;
                if (fileLocation == null) return;

                message.InputFile = GetInputFile(item.FileId, new TLInt(item.Parts.Count), mediaAudio.Key, mediaAudio.IV);

                var chatId = message.ChatId;
                if (chatId == null) return;

                var chat = CacheService.GetEncryptedChat(chatId) as TLEncryptedChat;
                if (chat == null)
                {
                    message.Status = MessageStatus.Failed;
                    message.NotifyOfPropertyChange(() => message.Status);

                    return;
                }

                SecretDialogDetailsViewModel.SendEncryptedMediaInternal(chat, obj, MTProtoService, CacheService);
            }
        }

        private static TLInputEncryptedFileBase GetInputFile(TLLong fileId, TLInt partsCount, TLString key, TLString iv)
        {
            var keyData = key.Data;
            var ivData = iv.Data;
            var digest = Telegram.Api.Helpers.Utils.ComputeMD5(TLUtils.Combine(keyData, ivData));
            var fingerprint = new byte[4];
            var sub1 = digest.SubArray(0, 4);
            var sub2 = digest.SubArray(4, 4);
            for (var i = 0; i < 4; i++)
            {
                fingerprint[i] = (byte)(sub1[i] ^ sub2[i]);
            }

            var uploadedFile = new TLInputEncryptedFileUploaded
            {
                Id = fileId,
                MD5Checksum = TLString.Empty,
                KeyFingerprint = new TLInt(BitConverter.ToInt32(fingerprint, 0)),
                Parts = partsCount //new TLInt(item.Parts.Count)
            };

            return uploadedFile;
        }

        private static TLInputEncryptedFileBase GetInputFileBig(TLLong fileId, TLInt partsCount, TLString key, TLString iv)
        {
            var keyData = key.Data;
            var ivData = iv.Data;
            var digest = Telegram.Api.Helpers.Utils.ComputeMD5(TLUtils.Combine(keyData, ivData));
            var fingerprint = new byte[4];
            var sub1 = digest.SubArray(0, 4);
            var sub2 = digest.SubArray(4, 4);
            for (var i = 0; i < 4; i++)
            {
                fingerprint[i] = (byte)(sub1[i] ^ sub2[i]);
            }

            var uploadedFile = new TLInputEncryptedFileBigUploaded
            {
                Id = fileId,
                KeyFingerprint = new TLInt(BitConverter.ToInt32(fingerprint, 0)),
                Parts = partsCount
            };

            return uploadedFile;
        }

        private readonly Dictionary<long, TLObject> _group = new Dictionary<long, TLObject>();

        public void AddGroup(TLObject obj)
        {
            if (obj == null) return;

            var message = obj as TLMessage;
            if (message != null)
            {
                var mediaGroup = message.Media as TLMessageMediaGroup;
                if (mediaGroup != null)
                {
                    var m = mediaGroup.Group.FirstOrDefault() as TLMessage73;
                    if (m != null && m.GroupedId != null)
                    {
                        _group[m.GroupedId.Value] = message;
                    }
                }
            }

            var decryptedMessage = obj as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                var mediaGroup = decryptedMessage.Media as TLDecryptedMessageMediaGroup;
                if (mediaGroup != null)
                {
                    var m = mediaGroup.Group.FirstOrDefault() as TLDecryptedMessage73;
                    if (m != null && m.GroupedId != null)
                    {
                        _group[m.GroupedId.Value] = decryptedMessage;
                    }
                }
            }
        }

        public void Remove(TLObject obj)
        {
            if (obj == null) return;

            var message73 = obj as TLMessage73;
            if (message73 != null)
            {
                if (message73.GroupedId == null) return;

                TLObject groupedMessage;
                if (_group.TryGetValue(message73.GroupedId.Value, out groupedMessage))
                {
                    var groupedMessage73 = groupedMessage as TLMessage73;
                    if (groupedMessage73 != null)
                    {
                        var mediaGroup = groupedMessage73.Media as TLMessageMediaGroup;
                        if (mediaGroup != null)
                        {
                            mediaGroup.Group.Remove(message73);
                        }
                    }
                }
            }

            var decryptedMessage73 = obj as TLDecryptedMessage73;
            if (decryptedMessage73 != null)
            {
                if (decryptedMessage73.GroupedId == null) return;

                TLObject groupedMessage;
                if (_group.TryGetValue(decryptedMessage73.GroupedId.Value, out groupedMessage))
                {
                    var groupedMessage73 = groupedMessage as TLDecryptedMessage73;
                    if (groupedMessage73 != null)
                    {
                        var mediaGroup = groupedMessage73.Media as TLDecryptedMessageMediaGroup;
                        if (mediaGroup != null)
                        {
                            mediaGroup.Group.Remove(decryptedMessage73);
                        }
                    }
                }
            }
        }
    }
}
