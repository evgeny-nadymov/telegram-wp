// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Storage;
using Windows.System;
using Caliburn.Micro;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Controls;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Dialogs;
using TelegramClient_Opus;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Controls
{
    public partial class MessagePlayerControl
    {
        private static MediaElement _player;

        public static MediaElement Player
        {
            get
            {
                if (_player == null)
                {
                    var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                    if (frame != null)
                    {
                        _player = frame.Element;
                    }
                }

                return _player;
            }
        } 

        private string _trackFileName;

        public static readonly DependencyProperty WaveformProperty = DependencyProperty.Register(
            "Waveform", typeof (TLString), typeof (MessagePlayerControl), new PropertyMetadata(OnWaveformChanged));

        private static void OnWaveformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MessagePlayerControl;
            if (control == null) return;

            var waveform = e.NewValue as TLString;
            
            List<double> barHeights = null;
            if (waveform != null && waveform.Data != null)
            {
                var bites = new BitArray(waveform.Data);
                barHeights = new List<double>();
                for (var i = 0; i <= bites.Length - 5; i = i + 5)
                {
                    var bit1 = Convert.ToInt32(bites[i]) << 0;
                    var bit2 = Convert.ToInt32(bites[i + 1]) << 1;
                    var bit3 = Convert.ToInt32(bites[i + 2]) << 2;
                    var bit4 = Convert.ToInt32(bites[i + 3]) << 3;
                    var bit5 = Convert.ToInt32(bites[i + 4]) << 4;

                    var result = bit1 | bit2 | bit3 | bit4 | bit5;

                    var barHeight = result / 31.0;

                    barHeights.Add(barHeight);
                }
            }

            control.PositionIndicator.Waveform = barHeights;
        }

        public TLString Waveform
        {
            get { return (TLString) GetValue(WaveformProperty); }
            set { SetValue(WaveformProperty, value); }
        }

        public static readonly DependencyProperty MediaProperty = DependencyProperty.Register(
            "Media", typeof (TLObject), typeof (MessagePlayerControl), new PropertyMetadata(default(TLObject), OnMediaPropertyChanged));

        private static void OnMediaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MessagePlayerControl;
            if (control == null) return;

            // deafult values

            var mediaDocument = e.NewValue as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var document = mediaDocument.Document as TLDocument22;
                if (document != null)
                {
                    var fileName = string.Format("audio{0}_{1}.wav", document.Id, document.AccessHash);
                    var isoFileName = Path.GetFileNameWithoutExtension(mediaDocument.IsoFileName) + ".wav";
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName) && !store.FileExists(isoFileName))
                        {
                            TLObject with = null;
                            var navigationService = IoC.Get<INavigationService>();
                            var dialogDetailsView = navigationService.CurrentContent as DialogDetailsView;
                            if (dialogDetailsView != null)
                            {
                                var dialogDetailsViewModel = dialogDetailsView.DataContext as DialogDetailsViewModel;
                                if (dialogDetailsViewModel != null)
                                {
                                    with = dialogDetailsViewModel.With;
                                }
                            }

                            var stateService = IoC.Get<IStateService>();
                            var chatSettings = stateService.GetChatSettings();
                            var downloadAudio = true;
                            if (chatSettings != null)
                            {
                                if (with is TLUserBase && !chatSettings.AutoDownloadAudioPrivateChats)
                                {
                                    downloadAudio = false;
                                }

                                if (with is TLChatBase && !chatSettings.AutoDownloadAudioGroups)
                                {
                                    downloadAudio = false;
                                }
                            }

                            if (downloadAudio)
                            {
                                mediaDocument.DownloadingProgress = 0.01;

                                Execute.BeginOnThreadPool(() =>
                                {
                                    var fileManager = IoC.Get<IAudioFileManager>();
                                    fileManager.DownloadFile(document.DCId, document.ToInputFileLocation(), mediaDocument,
                                        document.Size,
                                        item =>
                                        {
                                            Execute.BeginOnUIThread(() =>
                                            {
                                                ConvertAndSaveOpusToWav(control, mediaDocument);

                                                mediaDocument.IsCanceled = false;
                                                mediaDocument.LastProgress = 0.0;
                                                mediaDocument.DownloadingProgress = 0.0;
                                                mediaDocument.IsoFileName = item.IsoFileName;
                                                mediaDocument.NotifyOfPropertyChange(() => mediaDocument.ThumbSelf);
                                            });
                                        });
                                });
                            }

                            control.PlayerDownloadButton.Visibility = Visibility.Visible;
                            control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                            control.PlayerToggleButton.Visibility = Visibility.Visible;
                        }
                    }
                }

                return;
            }

            var mediaAudio = e.NewValue as TLMessageMediaAudio;
            if (mediaAudio != null)
            {
                var audio = mediaAudio.Audio as TLAudio33;
                if (audio != null)
                {
                    var fileName =  string.Format("audio{0}_{1}.wav", audio.Id, audio.AccessHash);
                    var isoFileName = Path.GetFileNameWithoutExtension(mediaAudio.IsoFileName) + ".wav";
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName) && !store.FileExists(isoFileName))
                        {
                            TLObject with = null;
                            var navigationService = IoC.Get<INavigationService>();
                            var dialogDetailsView = navigationService.CurrentContent as DialogDetailsView;
                            if (dialogDetailsView != null)
                            {
                                var dialogDetailsViewModel = dialogDetailsView.DataContext as DialogDetailsViewModel;
                                if (dialogDetailsViewModel != null)
                                {
                                    with = dialogDetailsViewModel.With;
                                }
                            }

                            var stateService = IoC.Get<IStateService>();
                            var chatSettings = stateService.GetChatSettings();
                            var downloadAudio = true;
                            if (chatSettings != null)
                            {
                                if (with is TLUserBase && !chatSettings.AutoDownloadAudioPrivateChats)
                                {
                                    downloadAudio = false;
                                }

                                if (with is TLChatBase && !chatSettings.AutoDownloadAudioGroups)
                                {
                                    downloadAudio = false;
                                }
                            }

                            if (downloadAudio)
                            {
                                mediaAudio.DownloadingProgress = 0.01;

                                Execute.BeginOnThreadPool(() =>
                                {
                                    var fileManager = IoC.Get<IAudioFileManager>();
                                    fileManager.DownloadFile(audio.DCId, audio.ToInputFileLocation(), mediaAudio,
                                        audio.Size,
                                        item =>
                                        {
                                            Execute.BeginOnUIThread(() =>
                                            {
                                                ConvertAndSaveOpusToWav(mediaAudio);

                                                mediaAudio.IsCanceled = false;
                                                mediaAudio.LastProgress = 0.0;
                                                mediaAudio.DownloadingProgress = 0.0;
                                                mediaAudio.IsoFileName = item.IsoFileName;
                                                mediaAudio.NotifyOfPropertyChange(() => mediaAudio.ThumbSelf);
                                            });
                                        });
                                });
                            }

                            control.PlayerDownloadButton.Visibility = Visibility.Visible;
                            control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                            control.PlayerToggleButton.Visibility = Visibility.Visible;
                        }
                    }
                }

                return;
            }

            var decryptedMediaDocument = e.NewValue as TLDecryptedMessageMediaDocument45;
            if (decryptedMediaDocument != null)
            {
                var fileLocation = decryptedMediaDocument.File as TLEncryptedFile;
                if (fileLocation != null)
                {
                    var fileName = string.Format("audio{0}_{1}.wav", fileLocation.Id, fileLocation.AccessHash);
                    //var isoFileName = Path.GetFileNameWithoutExtension(decryptedMediaAudio.IsoFileName) + ".wav";
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName))
                        {
                            TLObject with = null;
                            var navigationService = IoC.Get<INavigationService>();
                            var secretDialogDetailsView = navigationService.CurrentContent as SecretDialogDetailsView;
                            if (secretDialogDetailsView != null)
                            {
                                var secretDialogDetailsViewModel = secretDialogDetailsView.DataContext as SecretDialogDetailsViewModel;
                                if (secretDialogDetailsViewModel != null)
                                {
                                    with = secretDialogDetailsViewModel.With;
                                }
                            }

                            var stateService = IoC.Get<IStateService>();
                            var chatSettings = stateService.GetChatSettings();
                            var downloadAudio = true;
                            if (chatSettings != null)
                            {
                                if (with is TLUserBase && !chatSettings.AutoDownloadAudioPrivateChats)
                                {
                                    downloadAudio = false;
                                }

                                if (with is TLChatBase && !chatSettings.AutoDownloadAudioGroups)
                                {
                                    downloadAudio = false;
                                }
                            }

                            if (downloadAudio)
                            {
                                decryptedMediaDocument.DownloadingProgress = 0.01;

                                Execute.BeginOnThreadPool(() =>
                                {
                                    var fileManager = IoC.Get<IEncryptedFileManager>();
                                    fileManager.DownloadFile(fileLocation, decryptedMediaDocument,
                                        item =>
                                        {
                                            decryptedMediaDocument = item.Owner as TLDecryptedMessageMediaDocument45;
                                            if (decryptedMediaDocument == null) return;

                                            fileLocation = decryptedMediaDocument.File as TLEncryptedFile;
                                            if (fileLocation == null) return;

                                            fileName = item.IsoFileName;
                                            var decryptedFileName = String.Format("audio{0}_{1}.mp3",
                                                fileLocation.Id,
                                                fileLocation.AccessHash);
                                            using (var s = IsolatedStorageFile.GetUserStoreForApplication())
                                            {
                                                byte[] buffer;
                                                using (var file = s.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                                {
                                                    buffer = new byte[file.Length];
                                                    file.Read(buffer, 0, buffer.Length);
                                                }
                                                var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, decryptedMediaDocument.Key.Data, decryptedMediaDocument.IV.Data, false);

                                                using (var file = s.OpenFile(decryptedFileName, FileMode.OpenOrCreate, FileAccess.Write))
                                                {
                                                    file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                                                }

                                                s.DeleteFile(fileName);

                                            }

                                            Execute.BeginOnUIThread(() =>
                                            {
                                                ConvertAndSaveOpusToWav(decryptedMediaDocument);

                                                decryptedMediaDocument.DownloadingProgress = 0.0;
                                            });
                                        });
                                });
                            }

                            control.PlayerDownloadButton.Visibility = Visibility.Visible;
                            control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                            control.PlayerToggleButton.Visibility = Visibility.Visible;
                        }
                    }
                }

                return;
            }

            var decryptedMediaAudio = e.NewValue as TLDecryptedMessageMediaAudio;
            if (decryptedMediaAudio != null)
            {
                var fileLocation = decryptedMediaAudio.File as TLEncryptedFile;
                if (fileLocation != null)
                {
                    var fileName = string.Format("audio{0}_{1}.wav", fileLocation.Id, fileLocation.AccessHash);
                    //var isoFileName = Path.GetFileNameWithoutExtension(decryptedMediaAudio.IsoFileName) + ".wav";
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(fileName))
                        {
                            TLObject with = null;
                            var navigationService = IoC.Get<INavigationService>();
                            var secretDialogDetailsView = navigationService.CurrentContent as SecretDialogDetailsView;
                            if (secretDialogDetailsView != null)
                            {
                                var secretDialogDetailsViewModel = secretDialogDetailsView.DataContext as SecretDialogDetailsViewModel;
                                if (secretDialogDetailsViewModel != null)
                                {
                                    with = secretDialogDetailsViewModel.With;
                                }
                            }

                            var stateService = IoC.Get<IStateService>();
                            var chatSettings = stateService.GetChatSettings();
                            var downloadAudio = true;
                            if (chatSettings != null)
                            {
                                if (with is TLUserBase && !chatSettings.AutoDownloadAudioPrivateChats)
                                {
                                    downloadAudio = false;
                                }

                                if (with is TLChatBase && !chatSettings.AutoDownloadAudioGroups)
                                {
                                    downloadAudio = false;
                                }
                            }

                            if (downloadAudio)
                            {
                                decryptedMediaAudio.DownloadingProgress = 0.01;

                                Execute.BeginOnThreadPool(() =>
                                {
                                    var fileManager = IoC.Get<IEncryptedFileManager>();
                                    fileManager.DownloadFile(fileLocation, decryptedMediaAudio,
                                        item =>
                                        {
                                            decryptedMediaAudio = item.Owner as TLDecryptedMessageMediaAudio;
                                            if (decryptedMediaAudio == null) return;

                                            fileLocation = decryptedMediaAudio.File as TLEncryptedFile;
                                            if (fileLocation == null) return;

                                            fileName = item.IsoFileName;
                                            var decryptedFileName = String.Format("audio{0}_{1}.mp3",
                                                fileLocation.Id,
                                                fileLocation.AccessHash);
                                            using (var s = IsolatedStorageFile.GetUserStoreForApplication())
                                            {
                                                byte[] buffer;
                                                using (var file = s.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                                {
                                                    buffer = new byte[file.Length];
                                                    file.Read(buffer, 0, buffer.Length);
                                                }
                                                var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, decryptedMediaAudio.Key.Data, decryptedMediaAudio.IV.Data, false);

                                                using (var file = s.OpenFile(decryptedFileName, FileMode.OpenOrCreate, FileAccess.Write))
                                                {
                                                    file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                                                }

                                                s.DeleteFile(fileName);

                                            }

                                            Execute.BeginOnUIThread(() =>
                                            {
                                                ConvertAndSaveOpusToWav(decryptedMediaAudio);

                                                decryptedMediaAudio.DownloadingProgress = 0.0;
                                            });
                                        });
                                });
                            }

                            control.PlayerDownloadButton.Visibility = Visibility.Visible;
                            control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                            control.PlayerToggleButton.Visibility = Visibility.Visible;
                        }
                    }
                }

                return;
            }
        }

        public static void ConvertAndSaveOpusToWav(TLMessageMediaAudio mediaAudio)
        {
            if (mediaAudio == null) return;

            var audio = mediaAudio.Audio as TLAudio;
            if (audio != null)
            {
                var audioFileName = audio.GetFileName();
                var wavFileName = Path.GetFileNameWithoutExtension(audioFileName) + ".wav";
                
                ConvertAndSaveOputToWavCommon(audioFileName, wavFileName);
            }
        }

        public static void ConvertAndSaveOpusToWav(MessagePlayerControl control, TLMessageMediaDocument mediaDocument)
        {
            if (mediaDocument == null) return;

            var document = mediaDocument.Document as TLDocument22;
            if (document != null)
            {
                var audioFileName = string.Format("audio{0}_{1}.mp3", document.Id, document.AccessHash);
                var wavFileName = Path.GetFileNameWithoutExtension(audioFileName) + ".wav";

//#if DEBUG

//                var opus = new WindowsPhoneRuntimeComponent();
//                var waveformBytes = opus.GetWaveform(ApplicationData.Current.LocalFolder.Path + "\\" + audioFileName);


//                var mediaDocument45 = mediaDocument as TLMessageMediaDocument45;
//                if (mediaDocument45 != null)
//                {
//                    byte[] waveformBytes2 = new byte[100];
//                    var waveform = mediaDocument45.Waveform;
//                    if (waveform != null)
//                    {
//                        var bites = new BitArray(waveform.Data);
//                        var count = 0;
//                        for (var i = 0; i <= bites.Length - 5 && count < 100; )
//                        {
//                            var bit1 = Convert.ToByte(bites[i]) << 0;
//                            var bit2 = Convert.ToByte(bites[i + 1]) << 1;
//                            var bit3 = Convert.ToByte(bites[i + 2]) << 2;
//                            var bit4 = Convert.ToByte(bites[i + 3]) << 3;
//                            var bit5 = Convert.ToByte(bites[i + 4]) << 4;

//                            var result = bit1 | bit2 | bit3 | bit4 | bit5;

//                            waveformBytes2[count] = (byte)(result);
//                            i = i + 5;
//                            count++;
//                        }

//                        var bites2 = new BitArray(5*100);
//                        count = 0;
//                        for (var i = 0; i < waveformBytes2.Length; i++)
//                        {
//                            var result = waveformBytes2[i];
//                            var bit1 = result >> 0 & 0x1;
//                            var bit2 = result >> 1 & 0x1;
//                            var bit3 = result >> 2 & 0x1;
//                            var bit4 = result >> 3 & 0x1;
//                            var bit5 = result >> 4 & 0x1;
//                            bites2[count] = Convert.ToBoolean(bit1);
//                            bites2[count + 1] = Convert.ToBoolean(bit2);
//                            bites2[count + 2] = Convert.ToBoolean(bit3);
//                            bites2[count + 3] = Convert.ToBoolean(bit4);
//                            bites2[count + 4] = Convert.ToBoolean(bit5);
//                            count = count + 5;
//                        }

//                        for (var i = 0; i < 500; i++)
//                        {
//                            if (bites[i] != bites2[i])
//                            {
                                
//                            }
//                        }
//                    }
//                }
//#endif

                ConvertAndSaveOputToWavCommon(audioFileName, wavFileName);
            }
        }

        public static void ConvertAndSaveOpusToWav(TLDecryptedMessageMediaDocument mediaDocument)
        {
            if (mediaDocument == null) return;

            var fileLocation = mediaDocument.File as TLEncryptedFile;
            if (fileLocation == null) return;

            var audioFileName = String.Format("audio{0}_{1}.mp3", fileLocation.Id, fileLocation.AccessHash);
            var wavFileName = Path.GetFileNameWithoutExtension(audioFileName) + ".wav";

            ConvertAndSaveOputToWavCommon(audioFileName, wavFileName);
        }

        public static void ConvertAndSaveOpusToWav(TLDecryptedMessageMediaAudio mediaAudio)
        {
            if (mediaAudio == null) return;

            var fileLocation = mediaAudio.File as TLEncryptedFile;
            if (fileLocation == null) return;

            var audioFileName = String.Format("audio{0}_{1}.mp3", fileLocation.Id, fileLocation.AccessHash);
            var wavFileName = Path.GetFileNameWithoutExtension(audioFileName) + ".wav";

            ConvertAndSaveOputToWavCommon(audioFileName, wavFileName);
        }

        private static void ConvertAndSaveOputToWavCommon(string audioFileName, string wavFileName)
        {
#if WP8
            try
            {
                // critial not multitreaded code! 
                var component = new WindowsPhoneRuntimeComponent();
                var result = component.InitPlayer(ApplicationData.Current.LocalFolder.Path + "\\" + audioFileName);
                if (result == 1)
                {
                    var buffer = new byte[16*1024];
                    var args = new int[3];
                    var pcmStream = new MemoryStream();
                    while (true)
                    {
                        component.FillBuffer(buffer, buffer.Length, args);
                        var count = args[0];
                        var offset = args[1];
                        var endOfStream = args[2] == 1;

                        pcmStream.Write(buffer, 0, count);
                        if (endOfStream)
                        {
                            break;
                        }
                    }

                    var wavStream = Wav.GetWavAsMemoryStream(pcmStream, 48000, 1, 16);
                    using (var s = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var file = new IsolatedStorageFileStream(wavFileName, FileMode.OpenOrCreate, s))
                        {
                            wavStream.Seek(0, SeekOrigin.Begin);
                            wavStream.CopyTo(file);
                            file.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write("MessagePlayerControl.ConvertAndSaveOpusToWav " + ex);

                //Execute.ShowDebugMessage("MessagePlayerControl.ConvertAndSaveOpusToWav " + ex);
            }
#endif
        }

        public TLObject Media
        {
            get { return (TLObject) GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }

        public static readonly DependencyProperty DataContextWatcherProperty = DependencyProperty.Register(
            "DataContextWatcher", typeof(object), typeof(MessagePlayerControl), new PropertyMetadata(default(object), OnDataContextChanged));

        private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MessagePlayerControl;
            if (control != null)
            {
                control.Duration.Text = MessagePlayerUtils.GetDurationString(control.DataContext);
            }
        }

        public object DataContextWatcher
        {
            get { return GetValue(DataContextWatcherProperty); }
            set { SetValue(DataContextWatcherProperty, value); }
        }

        public static readonly DependencyProperty NotListenedProperty = DependencyProperty.Register(
            "NotListened", typeof (bool), typeof (MessagePlayerControl), new PropertyMetadata(OnNotListenedChanged));

        private static void OnNotListenedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MessagePlayerControl;
            if (control == null) return;

            if (e.NewValue is bool)
            {
                var isVisible = (bool)e.NewValue;
                
                control.NotListenedIndicator.Visibility = isVisible
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public bool NotListened
        {
            get { return (bool) GetValue(NotListenedProperty); }
            set { SetValue(NotListenedProperty, value); }
        }

        /// <summary>
        /// Downloading progress
        /// </summary>
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            "Progress", typeof (double), typeof (MessagePlayerControl), new PropertyMetadata(default(double), OnProgressChanged));

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MessagePlayerControl;
            if (control == null) return;

            if (e.NewValue is double)
            {
                bool isVisible;
                var progress = (double)e.NewValue;
                isVisible = progress > 0.0 && progress < 1.0;

                if (!isVisible)
                {
                    var mediaDocument = control.Media as TLMessageMediaDocument;
                    if (mediaDocument != null)
                    {
                        var document = mediaDocument.Document as TLDocument;
                        if (document != null)
                        {
                            var fileName = string.Format("audio{0}_{1}.wav", document.Id, document.AccessHash);
                            var isoFileName = Path.GetFileNameWithoutExtension(mediaDocument.IsoFileName) + ".wav";
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (!store.FileExists(fileName) && !store.FileExists(isoFileName))
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                                    control.PlayerDownloadButton.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Visible;
                                    control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }

                    var mediaAudio = control.Media as TLMessageMediaAudio;
                    if (mediaAudio != null)
                    {
                        var audio = mediaAudio.Audio as TLAudio;
                        if (audio != null)
                        {
                            var fileName = string.Format("audio{0}_{1}.wav", audio.Id, audio.AccessHash);
                            var isoFileName = Path.GetFileNameWithoutExtension(mediaAudio.IsoFileName) + ".wav";
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (!store.FileExists(fileName) && !store.FileExists(isoFileName))
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                                    control.PlayerDownloadButton.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Visible;
                                    control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }

                    var decryptedMediaDocument = control.Media as TLDecryptedMessageMediaDocument;
                    if (decryptedMediaDocument != null)
                    {
                        var fileLocation = decryptedMediaDocument.File as TLEncryptedFile;
                        if (fileLocation != null)
                        {
                            var fileName = string.Format("audio{0}_{1}.wav", fileLocation.Id, fileLocation.AccessHash);
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (!store.FileExists(fileName))
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                                    control.PlayerDownloadButton.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Visible;
                                    control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }

                    var decryptedMediaAudio = control.Media as TLDecryptedMessageMediaAudio;
                    if (decryptedMediaAudio != null)
                    {
                        var fileLocation = decryptedMediaAudio.File as TLEncryptedFile;
                        if (fileLocation != null)
                        {
                            var fileName = string.Format("audio{0}_{1}.wav", fileLocation.Id, fileLocation.AccessHash);
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (!store.FileExists(fileName))
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Collapsed;
                                    control.PlayerDownloadButton.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    control.PlayerToggleButton.Visibility = Visibility.Visible;
                                    control.PlayerDownloadButton.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                }
                else
                {
                    control.PlayerToggleButton.Visibility = isVisible
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                    control.PlayerDownloadButton.Visibility = isVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }

                control.CancelDownloadButton.Visibility = isVisible 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;

                control.DownloadButton.Visibility = isVisible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public double Progress
        {
            get { return (double) GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty UploadingProgressProperty = DependencyProperty.Register(
            "UploadingProgress", typeof (double), typeof (MessagePlayerControl), new PropertyMetadata(default(double), OnProgressChanged));

        public double UploadingProgress
        {
            get { return (double) GetValue(UploadingProgressProperty); }
            set { SetValue(UploadingProgressProperty, value); }
        }

        public MessagePlayerControl()
        {
            InitializeComponent();

            PositionIndicator.Value = 0.0;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.05) };
            _timer.Tick += UpdateState;

            //if (Player.Tag is MessagePlayerControl
            //    && Player.CurrentState == MediaElementState.Playing)
            //{
            //    // If audio was already playing when the app was launched, update the UI.
            //    //if (!_isManipulating)
            //    {
            //        UpdateState(null, null);
            //    }
            //}

            SetBinding(DataContextWatcherProperty, new Binding());
        }

        #region Timer

        private readonly DispatcherTimer _timer;

        private Storyboard _recentStoryboard;

        private bool _completeAnimation;

        private void UpdateState(object sender, System.EventArgs e)
        {
            if (Player.Source != null)
            {
                if (!_isManipulating)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(Player.Position.TotalSeconds);
                        if (PositionIndicator.Value > 0.0
                            && Player.Position.Ticks > 0
                            && (!Player.NaturalDuration.HasTimeSpan || Player.Position.Ticks < Player.NaturalDuration.TimeSpan.Ticks)
                            )
                        {
                            UpdateDurationString(Player.Position, "UpdateState");
                        }

                        var value = Player.Position.TotalSeconds;
                        if (PositionIndicator.Value > 0.0
                            && Player.Position.TotalSeconds == 0.0 && PlayerToggleButton.IsChecked == true)
                        {
                            if (_completeAnimation)
                            {
                                return;
                            }

                            value = PositionIndicator.Maximum;
                            _completeAnimation = true;
                        }

                        System.Diagnostics.Debug.WriteLine("value=" + value + " Player.Position.TotalSeconds=" + Player.Position.TotalSeconds + " complete=" + _completeAnimation);
                        PositionIndicator.Value = value;
                        if (_completeAnimation)
                        {
                            //return;
                            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                            {
                                PositionIndicator.Value = 0.0;
                                Duration.Text = MessagePlayerUtils.GetDurationString(PlayerToggleButton.DataContext);
                            });
                        }

                        //var storyboard = new Storyboard();
                        //var valueAnimation = new DoubleAnimationUsingKeyFrames();
                        //valueAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.1), Value = value });
                        //Storyboard.SetTarget(valueAnimation, PositionIndicator);
                        //Storyboard.SetTargetProperty(valueAnimation, new PropertyPath("(RangeBase.Value)"));
                        //storyboard.Children.Add(valueAnimation);
                        //if (completeAnimation)
                        //{
                        //    storyboard.Completed += (o, args) =>
                        //    {
                        //        PositionIndicator.Value = 0.0;
                        //        Duration.Text = MessagePlayerUtils.GetDurationString(PlayerToggleButton.DataContext);
                        //    };
                        //}
                        //storyboard.Begin();
                        //_recentStoryboard.Begin();
                        
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void UpdateDurationString(TimeSpan timeSpan, string from)
        {
            System.Diagnostics.Debug.WriteLine(from + " " + timeSpan);

            if (timeSpan.Hours > 0)
            {
                Duration.Text = timeSpan.ToString(@"h\:mm\:ss");
            }
            Duration.Text = timeSpan.ToString(@"m\:ss");
        }
        #endregion

        #region Player

        private void PlayerToggleButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Player.Tag != this)
            {
                ResetPlayer();
                Player.Tag = this;
            }

            var wavFileName = MessagePlayerUtils.GetWavFileName(PlayerToggleButton.DataContext);
            if (string.IsNullOrEmpty(wavFileName)) return;

            if (PlayerToggleButton.IsChecked == true)
            {

                if (Player.Source == null
                    || Path.GetFileName(Player.Source.OriginalString) != wavFileName)
                {
                    Player.Source = null;
                    _trackFileName = wavFileName;

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.FileExists(wavFileName))
                        {
                            PlayerToggleButton.IsChecked = false;
                            return;
                        }

                        using (var wavFile = store.OpenFile(wavFileName, FileMode.Open, FileAccess.Read))
                        {
                            Player.SetSource(wavFile);
                        }
                    }
                }
                else
                {
                    _trackFileName = wavFileName;

                    SetPosition();

                    _completeAnimation = false;
                    Player.Play();
                    _timer.Start();
                }

            }
            else
            {
                Player.Pause();
                _timer.Stop();
            }
        }

        private void SetPosition()
        {
            PositionIndicator.IsEnabled = true;
            var ratio = Player.NaturalDuration.TimeSpan.TotalSeconds/PositionIndicator.Maximum;
            var newValue = PositionIndicator.Value*ratio;
            PositionIndicator.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
            PositionIndicator.SmallChange = PositionIndicator.Maximum/10.0;
            PositionIndicator.LargeChange = PositionIndicator.Maximum/10.0;
            if (PositionIndicator.Value >= PositionIndicator.Maximum)
            {
                newValue = PositionIndicator.Maximum - 0.01;    //фикс, если установлено максимальное значение, то при вызове Play аудио не проигрывается и не вызывается OnMediaEnded. Плеер подвисает на конечной позиции
            }

            if (double.IsNaN(newValue) || double.IsInfinity(newValue))
            {
                newValue = 0.0;
            }

            PositionIndicator.Value = newValue;
            if (Player.CanSeek)
            {
                Player.Position = TimeSpan.FromSeconds(newValue);
            }
        }
        #endregion

        #region Binding to MediaElement

        private void ResetPlayer()
        {
            var playerControl = Player.Tag as MessagePlayerControl;
            if (playerControl != null)
            {
                playerControl._timer.Stop();
                playerControl.PlayerToggleButton.IsChecked = false;
                playerControl.PositionIndicator.Value = 0.0;
                playerControl.Duration.Text = MessagePlayerUtils.GetDurationString(playerControl.PlayerToggleButton.DataContext);

                UnbindFromPlayer(playerControl);
            }

            BindToPlayer();
        }

        private void BindToPlayer()
        {
            //Player.MediaOpened += OnMediaOpened;
            //Player.MediaEnded += OnMediaEnded;
            //Player.MediaFailed += OnMediaFailed;
            var gifPlayerControl = Player.Tag as GifPlayerControl;
            if (gifPlayerControl != null)
            {
                gifPlayerControl.OnMediaEnded();
            }

            Player.Tag = this;
        }

        private void UnbindFromPlayer(MessagePlayerControl control)
        {
            //Player.MediaOpened -= control.OnMediaOpened;
            //Player.MediaEnded -= control.OnMediaEnded;
            //Player.MediaFailed -= control.OnMediaFailed;

            Player.Tag = null;
        }

        public void OnMediaFailed(ExceptionRoutedEventArgs e)
        {
            Execute.ShowDebugMessage(e.ErrorException.ToString());

            _timer.Stop();
            PlayerToggleButton.IsChecked = false;
            PositionIndicator.Value = 0.0;
        }

        public void OnMediaOpened()
        {
            SetPosition();

            if (PlayerToggleButton.IsChecked == true)
            {
                Player.Play();
                _timer.Start();
            }
        }

        public void OnMediaEnded()
        {
            System.Diagnostics.Debug.WriteLine("OnMediaEneded");
            _timer.Stop();
            PlayerToggleButton.IsChecked = false;
            _completeAnimation = false;
            if (!_isManipulating)
            {
                //PositionIndicator.Value = 0.0;
                //Duration.Text = MessagePlayerUtils.GetDurationString(PlayerToggleButton.DataContext);
            }
        }

        #endregion

        #region Sliding
        private bool _isManipulating;

        private void Slider_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (!Player.CanSeek
                || Path.GetFileName(Player.Source.LocalPath) != _trackFileName)
            {
                e.Handled = true;
                return;
            }
            
            if (_recentStoryboard != null)
            {
                _recentStoryboard.Stop();
            }

            _isManipulating = true;
        }

        private void Slider_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var value = PositionIndicator.Value;
            _isManipulating = false;


            if (Player.Source != null
                && Player.CanSeek
                && Path.GetFileName(Player.Source.LocalPath) == _trackFileName)
            {
                if (value >= PositionIndicator.Maximum)
                {
                    value = PositionIndicator.Maximum - 0.01;
                }

                Player.Position = TimeSpan.FromSeconds(value);
                UpdateDurationString(Player.Position, "ManipulationCompleted");
            }
            else
            {
                PositionIndicator.Value = 0.0;
            }
        }

        private void Slider_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var timeSpan = TimeSpan.FromSeconds(PositionIndicator.Value);
            UpdateDurationString(timeSpan, "ManipulationDelta");
        }
        #endregion

        public static void Stop()
        {
            var player = Player.Tag as MessagePlayerControl;
            if (player != null)
            {
                if (MessagePlayerControl.Player.CurrentState == MediaElementState.Playing)
                {
                    MessagePlayerControl.Player.Stop();
                }

                player.OnMediaEnded(); 
            }          
        }

        private void CancelDownloadButton_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;

            RaiseCancelDownload();
        }

        public event EventHandler CancelDownload;

        protected virtual void RaiseCancelDownload()
        {
            var handler = CancelDownload;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }
    }

    public static class MessagePlayerUtils
    {
        public static string GetDurationString(object dataContext)
        {
            var mediaDocument = dataContext as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var document = mediaDocument.Document as TLDocument22;
                if (document != null)
                {
                    return document.DurationString;
                }
            }

            var mediaAudio = dataContext as TLMessageMediaAudio;
            if (mediaAudio != null)
            {
                var audio = mediaAudio.Audio as TLAudio;
                if (audio != null)
                {
                    return audio.DurationString;
                }
            }

            var decryptedMediaDocument = dataContext as TLDecryptedMessageMediaDocument45;
            if (decryptedMediaDocument != null)
            {
                return decryptedMediaDocument.DurationString;
            }

            var decryptedMediaAudio = dataContext as TLDecryptedMessageMediaAudio;
            if (decryptedMediaAudio != null)
            {
                return decryptedMediaAudio.DurationString;
            }

            return null;
        }

        public static string GetWavFileName(object dataContext)
        {
            var mediaDocument = dataContext as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var document = mediaDocument.Document as TLDocument;
                if (document != null)
                {
//                    if (TLString.Equals(document.MimeType, new TLString("audio/mpeg"), StringComparison.OrdinalIgnoreCase))
//                    {
//                        Execute.BeginOnThreadPool(async () =>
//                        {

//                            var audioFileName = document.GetFileName();
//#if WP81
//                            try
//                            {
//                                var documentFile = await ApplicationData.Current.LocalFolder.GetFileAsync(audioFileName);
//                                Launcher.LaunchFileAsync(documentFile);
//                            }
//                            catch (Exception ex)
//                            {
//                                Execute.ShowDebugMessage("LocalFolder.GetFileAsync docLocal exception \n" + ex);
//                            }
//#elif WP8
//                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(audioFileName);
//                        Launcher.LaunchFileAsync(file);
//                        return;
//#endif
//                        });
//                    }
                    return string.Format("audio{0}_{1}.wav", document.Id, document.AccessHash);
                }
            }

            var mediaAudio = dataContext as TLMessageMediaAudio;
            if (mediaAudio != null)
            {
                var audio = mediaAudio.Audio as TLAudio;
                if (audio != null)
                {
                    if (TLString.Equals(audio.MimeType, new TLString("audio/mpeg"), StringComparison.OrdinalIgnoreCase))
                    {
                        Execute.BeginOnThreadPool(async () =>
                        {

                            var audioFileName = audio.GetFileName();
#if WP81
                            try
                            {
                                var documentFile = await ApplicationData.Current.LocalFolder.GetFileAsync(audioFileName);
                                Launcher.LaunchFileAsync(documentFile);
                            }
                            catch (Exception ex)
                            {
                                Execute.ShowDebugMessage("LocalFolder.GetFileAsync docLocal exception \n" + ex);
                            }
#elif WP8
                        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(audioFileName);
                        Launcher.LaunchFileAsync(file);
                        return;
#endif
                        });
                    }
                    return string.Format("audio{0}_{1}.wav", audio.Id, audio.AccessHash);
                }
            }

            var decryptedMediaDocument = dataContext as TLDecryptedMessageMediaDocument;
            if (decryptedMediaDocument != null)
            {
                var file = decryptedMediaDocument.File as TLEncryptedFile;
                if (file != null)
                {
                    return string.Format("audio{0}_{1}.wav", file.Id, file.AccessHash);
                }
            }

            var decryptedMediaAudio = dataContext as TLDecryptedMessageMediaAudio;
            if (decryptedMediaAudio != null)
            {
                var file = decryptedMediaAudio.File as TLEncryptedFile;
                if (file != null)
                {
                    return string.Format("audio{0}_{1}.wav", file.Id, file.AccessHash);
                }
            }

            return null;
        }
    }
}
