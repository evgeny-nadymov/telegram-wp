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
using System.Text;
using Windows.Storage;
using Telegram.Api.TL;
using TelegramClient.Views.Controls;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public void SendAudio(AudioEventArgs args)
        {
            Telegram.Logs.Log.Write("DialogDetailsViewModel.SendAudio file_name=" + args.OggFileName);

            if (string.IsNullOrEmpty(args.OggFileName)) return;

            Telegram.Logs.Log.Write("DialogDetailsViewModel.SendAudio check_disable_feature file_name=" + args.OggFileName);

            var id = TLLong.Random();
            var accessHash = TLLong.Random();

            var oggFileName = string.Format("audio{0}_{1}.mp3", id, accessHash);
            var wavFileName = Path.GetFileNameWithoutExtension(oggFileName) + ".wav";

            long size = 0;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storage.MoveFile(args.OggFileName, oggFileName);
                using (var file = storage.OpenFile(oggFileName, FileMode.Open, FileAccess.Read))
                {
                    size = file.Length;
                }

                var wavStream = args.PcmStream.GetWavAsMemoryStream(16000, 1, 16);
                using (var file = new IsolatedStorageFileStream(wavFileName, FileMode.OpenOrCreate, storage))
                {
                    wavStream.Seek(0, SeekOrigin.Begin);
                    wavStream.CopyTo(file);
                    file.Flush();
                }
            }

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

            var document = new TLDocument54
            {
                Id = id,
                AccessHash = accessHash,
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                //Duration = new TLInt((int)args.Duration),
                MimeType = new TLString("audio/ogg"),
                Size = new TLInt((int)size),
                Thumb = new TLPhotoSizeEmpty { Type = TLString.Empty },
                DCId = new TLInt(0),
                Version = new TLInt(0),
                Attributes = attributes
            };

            var channel = With as TLChannel;
            var isChannel = channel != null && !channel.IsMegaGroup;

            var media = new TLMessageMediaDocument75 { Flags = new TLInt(0), Document = document, Caption = TLString.Empty, IsoFileName = oggFileName, NotListened = !isChannel };

            var message = GetMessage(TLString.Empty, media);

            message.NotListened = !isChannel;

            Telegram.Logs.Log.Write(string.Format("DialogDetailsViewModel.SendAudio start sending file_name={0} rnd_id={1}", args.OggFileName, message.RandomId));
            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                message.NotifyOfPropertyChange(() => message.Media);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                       message, previousMessage,
                       m => SendAudioInternal(message, args)));
            });
        }

        private void SendAudioInternal(TLMessage message, AudioEventArgs args = null)
        {
            Telegram.Logs.Log.Write("DialogDetailsViewModel.SendAudioInternal rnd_id=" + message.RandomId);

            var documentMedia = message.Media as TLMessageMediaDocument;
            if (documentMedia != null)
            {
                var fileName = documentMedia.IsoFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                var document = documentMedia.Document as TLDocument;
                if (document == null)
                {
                    return;
                }

                if (args != null)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("parts");
                    foreach (var part in args.Parts)
                    {
                        stringBuilder.AppendLine(string.Format("file_part={0} position={1} count={2} status={3}", part.FilePart, part.Position, part.Count, part.Status));
                    }

                    Telegram.Logs.Log.Write("DialogDetailsViewModel.SendAudioInternal uploading rnd_id=" + message.RandomId + Environment.NewLine + stringBuilder);

                    var fileId = args.FileId ?? TLLong.Random();
                    message.Media.FileId = fileId;
                    message.Media.UploadingProgress = 0.001;
                    UploadAudioFileManager.UploadFile(fileId, message, fileName, args.Parts);
                }
                else
                {
                    var fileId = TLLong.Random();
                    message.Media.FileId = fileId;
                    message.Media.UploadingProgress = 0.001;
                    UploadAudioFileManager.UploadFile(fileId, message, fileName);
                }

                return;
            }

            var audioMedia = message.Media as TLMessageMediaAudio;
            if (audioMedia != null)
            {
                var fileName = audioMedia.IsoFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                var audio = audioMedia.Audio as TLAudio;
                if (audio == null)
                {
                    return;
                }

                if (args != null)
                {
                    var fileId = args.FileId ?? TLLong.Random();
                    message.Media.FileId = fileId;
                    message.Media.UploadingProgress = 0.001;
                    UploadAudioFileManager.UploadFile(fileId, message, fileName, args.Parts);
                }
                else
                {
                    var fileId = TLLong.Random();
                    message.Media.FileId = fileId;
                    message.Media.UploadingProgress = 0.001;
                    UploadAudioFileManager.UploadFile(fileId, message, fileName);
                }

                return;
            }
        }
    }
}
