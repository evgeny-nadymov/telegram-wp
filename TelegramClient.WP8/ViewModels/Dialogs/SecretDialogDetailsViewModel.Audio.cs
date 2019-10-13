// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections;
using System.IO;
using System.IO.IsolatedStorage;
using Windows.Storage;
using Caliburn.Micro;
using Telegram.Api.TL;
using TelegramClient.Views.Controls;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
        private void SendAudioInternal(TLObject obj)
        {
            var message = GetDecryptedMessage(obj);
            if (message == null) return;

            var mediaDocument = message.Media as TLDecryptedMessageMediaDocument45;
            if (mediaDocument != null)
            {
                var fileLocation = mediaDocument.File as TLEncryptedFile;
                if (fileLocation == null) return;

                var fileName = String.Format("audio{0}_{1}.mp3",
                    fileLocation.Id,
                    fileLocation.AccessHash);

                byte[] data;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fileStream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                    {
                        data = new byte[fileStream.Length];
                        fileStream.Read(data, 0, data.Length);
                    }
                }

                var encryptedBytes = Telegram.Api.Helpers.Utils.AesIge(data, mediaDocument.Key.Data, mediaDocument.IV.Data, true);
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var file = storage.OpenFile("encrypted." + fileName, FileMode.Create, FileAccess.Write))
                    {
                        file.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }
                }

                UploadFileManager.UploadFile(fileLocation.Id, obj, encryptedBytes);

                return;
            }

            var mediaAudio = message.Media as TLDecryptedMessageMediaAudio;
            if (mediaAudio != null)
            {
                var fileLocation = mediaAudio.File as TLEncryptedFile;
                if (fileLocation == null) return;

                var fileName = String.Format("audio{0}_{1}.mp3",
                    fileLocation.Id,
                    fileLocation.AccessHash);

                byte[] data;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fileStream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                    {
                        data = new byte[fileStream.Length];
                        fileStream.Read(data, 0, data.Length);
                    }
                }

                var encryptedBytes = Telegram.Api.Helpers.Utils.AesIge(data, mediaAudio.Key.Data, mediaAudio.IV.Data, true);
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var file = storage.OpenFile("encrypted." + fileName, FileMode.Create, FileAccess.Write))
                    {
                        file.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }
                }

                UploadFileManager.UploadFile(fileLocation.Id, obj, encryptedBytes);

                return;
            }
        }

        public void SendAudio(AudioEventArgs args)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            if (string.IsNullOrEmpty(args.OggFileName)) return;

            var dcId = TLInt.Random();
            var id = TLLong.Random();
            var accessHash = TLLong.Random();

            var oggFileName = String.Format("audio{0}_{1}.mp3", id, accessHash);
            var wavFileName = Path.GetFileNameWithoutExtension(oggFileName) + ".wav";

            long size = 0;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storage.MoveFile(args.OggFileName, oggFileName);
                using (var file = storage.OpenFile(oggFileName, FileMode.Open, FileAccess.Read))
                {
                    size = file.Length;
                }

                var wavStream = Wav.GetWavAsMemoryStream(args.PcmStream, 16000, 1, 16);
                using (var file = new IsolatedStorageFileStream(wavFileName, FileMode.OpenOrCreate, storage))
                {
                    wavStream.Seek(0, SeekOrigin.Begin);
                    wavStream.CopyTo(file);
                    file.Flush();
                }
            }

            var fileLocation = new TLEncryptedFile
            {
                Id = id,
                AccessHash = accessHash,
                DCId = dcId,
                Size = new TLInt((int)size),
                KeyFingerprint = new TLInt(0),
                FileName = new TLString(Path.GetFileName(oggFileName))
            };

            var keyIV = GenerateKeyIV();
            TLDecryptedMessageMediaBase decryptedMediaAudio;
            var encryptedChat17 = chat as TLEncryptedChat17;
            if (encryptedChat17 != null)
            {
                if (encryptedChat17.Layer.Value >= Constants.MinSecretChatWithAudioAsDocumentsLayer)
                {
                    var opus = new TelegramClient_Opus.WindowsPhoneRuntimeComponent();
                    var bytes = opus.GetWaveform(ApplicationData.Current.LocalFolder.Path + "\\" + oggFileName);
                    var resultSamples = bytes.Length;
                    var bites2 = new BitArray(5 * bytes.Length);
                    var count = 0;
                    for (var i = 0; i < bytes.Length; i++)
                    {
                        var result = bytes[i];
                        var bit1 = result >> 0 & 0x1;
                        var bit2 = result >> 1 & 0x1;
                        var bit3 = result >> 2 & 0x1;
                        var bit4 = result >> 3 & 0x1;
                        var bit5 = result >> 4 & 0x1;
                        bites2[count] = Convert.ToBoolean(bit1);
                        bites2[count + 1] = Convert.ToBoolean(bit2);
                        bites2[count + 2] = Convert.ToBoolean(bit3);
                        bites2[count + 3] = Convert.ToBoolean(bit4);
                        bites2[count + 4] = Convert.ToBoolean(bit5);
                        count = count + 5;
                    }

                    var bytesCount = (resultSamples * 5) / 8 + (((resultSamples * 5) % 8) == 0 ? 0 : 1);
                    var waveformBytes = new byte[bytesCount];
                    bites2.CopyTo(waveformBytes, 0);
                    var waveform = waveformBytes != null ? TLString.FromBigEndianData(waveformBytes) : TLString.Empty;

                    var audioAttribute = new TLDocumentAttributeAudio46
                    {
                        Flags = new TLInt((int)DocumentAttributeAudioFlags.Voice),
                        Duration = new TLInt((int)args.Duration)
                    };

                    if (waveformBytes != null)
                    {
                        audioAttribute.Waveform = waveform;
                    }

                    var attributes = new TLVector<TLDocumentAttributeBase>
                    {
                        audioAttribute
                    };

                    decryptedMediaAudio = new TLDecryptedMessageMediaDocument45
                    {
                        Thumb = TLString.Empty,
                        ThumbW = new TLInt(0),
                        ThumbH = new TLInt(0),
                        MimeType = new TLString("audio/ogg"),
                        Size = new TLInt((int)size),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,
                        Attributes = attributes,
                        Caption = TLString.Empty,
                        NotListened = true,

                        File = fileLocation,

                        UploadingProgress = 0.001
                    };
                }
                else
                {
                    decryptedMediaAudio = new TLDecryptedMessageMediaAudio17
                    {
                        Duration = new TLInt((int)args.Duration),
                        MimeType = new TLString("audio/ogg"),
                        Size = new TLInt((int)size),
                        Key = keyIV.Item1,
                        IV = keyIV.Item2,

                        UserId = new TLInt(StateService.CurrentUserId),
                        File = fileLocation,

                        UploadingProgress = 0.001
                    };
                }
            }
            else
            {
                decryptedMediaAudio = new TLDecryptedMessageMediaAudio
                {
                    Duration = new TLInt((int)args.Duration),
                    //MimeType = new TLString("audio/ogg"),
                    Size = new TLInt((int)size),
                    Key = keyIV.Item1,
                    IV = keyIV.Item2,

                    UserId = new TLInt(StateService.CurrentUserId),
                    File = fileLocation,

                    UploadingProgress = 0.001
                };
            }

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, decryptedMediaAudio, chat, true);

            //var message45 = decryptedTuple.Item1 as TLDecryptedMessage45;
            //if (message45 != null && message45.IsVoice())
            //{
            //    message45.NotListened = true;
            //}

            BeginOnUIThread(() =>
            {
                InsertSendingMessage(decryptedTuple.Item1);
                NotifyOfPropertyChange(() => DescriptionVisibility);
            });

            BeginOnThreadPool(() =>
                CacheService.SyncDecryptedMessage(decryptedTuple.Item1, chat,
                    cachedMessage => SendAudioInternal(decryptedTuple.Item2)));
        }
    }
}
