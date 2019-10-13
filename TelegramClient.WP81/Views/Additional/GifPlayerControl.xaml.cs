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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Caliburn.Micro;
using FFmpegInterop;
using Microsoft.Devices;
using Microsoft.Phone;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Controls;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;
using Buffer = System.Buffer;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Additional
{
    public partial class GifPlayerControl
    {
        #region Dispatcher Timer

        private static readonly DispatcherTimer _dispatcherTimer;

        static GifPlayerControl()
        {
            _dispatcherTimer = new DispatcherTimer{ Interval = TimeSpan.FromMilliseconds(50.0) };
            _dispatcherTimer.Tick += (sender, args) =>
            {
                //VibrateController.Default.Start(TimeSpan.FromMilliseconds(15.0));
               foreach (var player in ActivePlayers)
                {
                    player.UpdateFrame();
                }

                foreach (var inlinePlayer in InlineBotActivePlayers)
                {
                    inlinePlayer.UpdateFrame();
                }
            };
        }

        private void UpdateFrame()
        {
            var pixels = _frame;
            if (pixels != null)
            {
                _frame = null;

                try
                {
                    if (PlayButton.Visibility == Visibility.Visible) return;

                    var bitmap = Frame.Source as WriteableBitmap;
                    if (bitmap == null || bitmap.PixelWidth != _w || bitmap.PixelHeight != _h)
                    {
                        bitmap = new WriteableBitmap(_w, _h);

                        Frame.Source = bitmap;
                    }
                    for (var j = 0; j < bitmap.Pixels.Length; j++)
                    {
                        bitmap.Pixels[j] =
                            (pixels[j * 4 + 3] << 24) |     //b
                            (pixels[j * 4 + 2] << 0) |      //g
                            (pixels[j * 4 + 1] << 8) |      //r
                            (pixels[j * 4] << 16);          //a 

                        //bitmap.Pixels[j] = pixels[j];
                    }

                    bitmap.Invalidate();

                    if (Mode == GifPlayerMode.Normal)
                    {
                        //Debug.Text = string.Format("pts={0}\nptsDelta={1}\ntimeDelta={2}\nelapsed={3}", _pts, _ptsDelta, _timeDelta, _elapsed.TotalMilliseconds);
                    }
                }
                catch(Exception ex)
                {
                    Execute.ShowDebugMessage("UpdateFrame exception " + ex);
                }
            }
        }

        private static void StartUpdateFrames()
        {
            if (!_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Start();
            }
        }

        private static void StopUpdateFrames()
        {
            if (_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Stop();
            }
        }
        #endregion

        private static readonly IList<GifPlayerControl> _inlineBotActivePlayers = new List<GifPlayerControl>();

        public static IList<GifPlayerControl> InlineBotActivePlayers
        {
            get { return _inlineBotActivePlayers; }
        }

        public static void StopInlineBotActivePlayers()
        {
            var players = new List<GifPlayerControl>(_inlineBotActivePlayers);
            foreach (var player in players)
            {
                player.Stop();
            }
        }

        private static readonly IList<GifPlayerControl> _activePlayers = new List<GifPlayerControl>();

        public static IList<GifPlayerControl> ActivePlayers
        {
            get { return _activePlayers; }
        }

        public static void ResumeActivePlayers(IList<GifPlayerControl> activePlayers)
        {
            var players = new List<GifPlayerControl>(_activePlayers);
            _activePlayers.Clear();

            var stoppedPlayers = new List<GifPlayerControl>();

            foreach (var player in players)
            {
                if (!activePlayers.Contains(player))
                {
                    stoppedPlayers.Add(player);
                }
            }

            foreach (var player in activePlayers)
            {
                player.Resume();
                _activePlayers.Add(player);
            }

            var autoPlayGif = false;
            var stateService = IoC.Get<IStateService>();
            if (stateService != null)
            {
                var chatSettings = stateService.GetChatSettings();
                if (chatSettings != null)
                {
                    autoPlayGif = chatSettings.AutoPlayGif;
                }

                foreach (var player in stoppedPlayers)
                {
                    if (!autoPlayGif)
                    {
                        if ((player.Media != null && player.Media.AutoPlayGif == true))
                        {
                            autoPlayGif = true;
                        }
                    }

                    player.Stop(autoPlayGif);
                }
            }

            if (_activePlayers.Count > 0)
            {
                StartUpdateFrames();
            }
        }

        public static void PauseActivePlayers()
        {
            var players = new List<GifPlayerControl>(_activePlayers);
            if (players.Count > 0)
            {
                foreach (var player in players)
                {
                    player.Pause();
                }
            }

            StopUpdateFrames();
        }

        public static void StopActivePlayers(bool scroll = false)
        {
            var players = new List<GifPlayerControl>(_activePlayers);
            if (players.Count > 0)
            {
                var autoPlayGif = false;
                var stateService = IoC.Get<IStateService>();
                if (stateService != null)
                {
                    var chatSettings = stateService.GetChatSettings();
                    if (chatSettings != null)
                    {
                        autoPlayGif = chatSettings.AutoPlayGif;
                    }

                    foreach (var player in players)
                    {
                        if (!autoPlayGif)
                        {
                            if (player.Media != null && player.Media.AutoPlayGif == true)
                            {
                                autoPlayGif = true;
                            }
                        }

                        player.Stop(autoPlayGif);
                    }
                }
            }
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode", typeof (GifPlayerMode), typeof (GifPlayerControl), new PropertyMetadata(default(GifPlayerMode), OnModeChanged));

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var player = d as GifPlayerControl;
            if (player != null)
            {
                var mode = (GifPlayerMode) e.NewValue;
                if (mode == GifPlayerMode.InlineResult)
                {
                    player.Button.RenderTransform = new ScaleTransform { ScaleX = 0.5, ScaleY = 0.5, CenterX = 23.5, CenterY = 23.5 };
                    player.Play.Opacity = 0.0;
                    player.PlayCircle.Opacity = 0.0;
                    player.DownloadButton.Opacity = 0.0;
                    player.CancelDownloadButton.CancelVisibility = Visibility.Collapsed;
                }
                else if (mode == GifPlayerMode.RoundVideo)
                {
                    
                }
                else 
                {
                    player.Button.RenderTransform = null;
                }
            }
        }

        public GifPlayerMode Mode
        {
            get { return (GifPlayerMode) GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty UploadingProgressProperty = DependencyProperty.Register(
            "UploadingProgress", typeof(double), typeof(GifPlayerControl), new PropertyMetadata(default(double), OnUploadingProgressChanged));

        public double UploadingProgress
        {
            get { return (double) GetValue(UploadingProgressProperty); }
            set { SetValue(UploadingProgressProperty, value); }
        }

        private static void OnUploadingProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var player = d as GifPlayerControl;
            if (player == null) return;

            if (e.NewValue is double)
            {
                var progress = (double)e.NewValue;

                player.CancelDownloadButton.Value = progress;

                var isComplete = e.NewValue != null
                        && e.OldValue != null
                        && (double)e.OldValue > 0.0
                        && (double)e.OldValue < 1.0
                        && (double)e.NewValue == 0.0;
                if (isComplete)
                {
                    return;
                }

                var isVisible = progress > 0.0 && progress < 1.0;
                OnProgressChangedInternal(isVisible, player, progress);
            }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            "Progress", typeof (double), typeof (GifPlayerControl), new PropertyMetadata(default(double), OnProgressChanged));

        public double Progress
        {
            get { return (double) GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        private void CancelDownloadButton_OnCompleted(object sender, System.EventArgs e)
        {
            OnProgressChangedInternal(false, this, 0.0);
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var player = d as GifPlayerControl;
            if (player == null) return;
            
            if (e.NewValue is double)
            {
                var progress = (double)e.NewValue;

                player.CancelDownloadButton.Value = progress;

                var isVisible = progress > 0.0 && progress < 1.0;
                OnProgressChangedInternal(isVisible, player, progress);
            }
        }

        private static void OnProgressChangedInternal(bool isVisible, GifPlayerControl player, double progress)
        {
            if (!isVisible)
            {
                var localFileName = GetLocalFileName(player.Media);
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists(localFileName))
                    {
                        player.ChangeButtonState(GifButtonState.Download);
                    }
                    else
                    {
                        player.ChangeButtonState(GifButtonState.Play, "OnProgressChangedInternal1");
                    }
                }
            }
            else
            {
                player.ChangeButtonState(GifButtonState.Cancel);
            }

            if (!player._initialized && (progress == 0.0))
            {
                if (player.Mode == GifPlayerMode.Normal && !ActivePlayers.Contains(player))
                {
                    if (player.ActualHeight > 0.0 && player.ActualWidth > 0.0)
                    {
                        // new messages
                        var startNewGifPlayer = false;

                        var position = player.TransformToVisual(Application.Current.RootVisual).Transform(new Point(0.0, 0.0));
                        if (player.ActualHeight + position.Y >= 2.0/3.0*player.ActualHeight
                            &&
                            position.Y <=
                            ((FrameworkElement) Application.Current.RootVisual).ActualHeight - 2.0/3.0*player.ActualHeight)
                        {
                            startNewGifPlayer = true;
                            ActivePlayers.Add(player);
                        }

                        if (!startNewGifPlayer)
                        {
                            return;
                        }
                    }
                }

                if (player.Mode == GifPlayerMode.InlineResult)
                {
                    if (!IsScrolling)
                    {
                        player.Start();
                    }
                    else
                    {
                        player.ChangeButtonState(GifButtonState.Play, "OnProgressChangedInternal2");
                    }
                }
                else
                {
                    var stateService = IoC.Get<IStateService>();
                    if (stateService != null)
                    {
                        var chatSettings = stateService.GetChatSettings();
                        if (ManipulationState == ManipulationState.Idle
                            && (chatSettings != null && chatSettings.AutoPlayGif && player.Media != null && player.Media.AutoPlayGif != false)
                             || (player.Media != null && player.Media.AutoPlayGif == true))
                        {
                            player.Start();
                        }
                        else
                        {
                            var localFileName = GetLocalFileName(player.Media);
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (!store.FileExists(localFileName))
                                {
                                    player.ChangeButtonState(GifButtonState.Download);
                                }
                                else
                                {
                                    player.ChangeButtonState(GifButtonState.Play, "OnProgressChangedInternal3");
                                }
                            }
                        }
                    }
                }
            }
        }

        //public static async Task ExtractFirstFrame(IMediaGifBase mediaGifBase)
        //{
        //    if (mediaGifBase == null) return;

        //    var fileName = GetLocalFileName(mediaGifBase);

        //    var decoderParams = new int[3];
        //    var handler = FFmpegGifDecoder.CreateDecoder(Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName), decoderParams);
        //    if (handler != 0)
        //    {
        //        var w = decoderParams[0];
        //        var h = decoderParams[1];

        //        if (w > 0 && h > 0)
        //        {
        //            var frame = FFmpegGifDecoder.GetVideoFrame(handler, decoderParams);
        //            if (frame != null)
        //            {
        //                var pixels = new byte[frame.Length * sizeof(int)];
        //                Buffer.BlockCopy(frame, 0, pixels, 0, pixels.Length);

        //                var previewFileName = GetFrameFileName(mediaGifBase);
        //                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(previewFileName, CreationCollisionOption.ReplaceExisting);
        //                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
        //                {
        //                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
        //                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)w, (uint)h, 96.0, 96.0, pixels);
        //                    await encoder.FlushAsync();
        //                }

        //            }
        //        }

        //        FFmpegGifDecoder.DestroyDecoder(handler);
        //    }
        //}

        public static readonly DependencyProperty MediaProperty = DependencyProperty.Register(
            "Media", typeof(IMediaGifBase), typeof(GifPlayerControl), new PropertyMetadata(default(IMediaGifBase), OnMediaChanged));

        public IMediaGifBase Media
        {
            get { return (IMediaGifBase)GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }

        public static ManipulationState ManipulationState { get; set; }

        public static bool IsScrolling { get; set; }

        private static void OnMediaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var player = d as GifPlayerControl;
            if (player != null)
            {
                player.Stop();
                player.Release();

                var mediaBase = e.NewValue as IMediaGifBase;
                if (mediaBase != null)
                {
                    // preview
                    SetMediaPreview(player, mediaBase);

                    // round video playing
                    if (player.Mode == GifPlayerMode.RoundVideo)
                    {
                        if (MessagePlayerControl.Player.CurrentState == MediaElementState.Playing)
                        {
                            var gifPlayerControl = MessagePlayerControl.Player.Tag as GifPlayerControl;
                            if (gifPlayerControl != null
                                && gifPlayerControl.Media == mediaBase)
                            {
                                var videoBrush = new VideoBrush();
                                videoBrush.SetSource(MessagePlayerControl.Player);
                                player.MediaPlaceholder.Background = videoBrush;
                                player.MuteIcon.Visibility = Visibility.Collapsed;

                                MessagePlayerControl.Player.Tag = player;
                            }
                        }
                    }

                    // button state/downloading
                    var localFileName = GetLocalFileName(mediaBase);

                    if (!string.IsNullOrEmpty(localFileName))
                    {
                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (store.FileExists(localFileName))
                            {
                                var stateService = IoC.Get<IStateService>();
                                var chatSettings = stateService.GetChatSettings();
                                if (ManipulationState == ManipulationState.Idle
                                    && (chatSettings != null && chatSettings.AutoPlayGif && player.Media != null && player.Media.AutoPlayGif != false)
                                     || (player.Media != null && player.Media.AutoPlayGif == true))
                                {
                                    player.ChangeButtonState(GifButtonState.None, "OnMediaChanged");
                                }
                                else
                                {
                                    player.ChangeButtonState(GifButtonState.Play, "OnMediaChanged");
                                }

                                if (player.Mode == GifPlayerMode.Normal && !ActivePlayers.Contains(player))
                                {
                                    if (player.ActualHeight > 0.0 && player.ActualWidth > 0.0)
                                    {
                                        // new messages
                                        var startNewGifPlayer = false;

                                        var position = player.TransformToVisual(Application.Current.RootVisual).Transform(new Point(0.0, 0.0));
                                        if (player.ActualHeight + position.Y >= 2.0 / 3.0 * player.ActualHeight
                                            && position.Y <= ((FrameworkElement)Application.Current.RootVisual).ActualHeight - 2.0 / 3.0 * player.ActualHeight)
                                        {
                                            startNewGifPlayer = true;
                                            ActivePlayers.Add(player);
                                        }

                                        if (!startNewGifPlayer)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                player.ChangeButtonState(GifButtonState.Download);

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
                                var downloadGif = true;
                                if (chatSettings != null)
                                {
                                    if (with is TLUserBase && !chatSettings.AutoDownloadGifPrivateChats)
                                    {
                                        downloadGif = false;
                                    }

                                    if (with is TLChatBase && !chatSettings.AutoDownloadGifGroups)
                                    {
                                        downloadGif = false;
                                    }
                                }

                                if (downloadGif && CheckDocumentParams(mediaBase))
                                {
                                    if (player.Mode == GifPlayerMode.InlineResult)
                                    {
                                        return; // suppress autodownload on media changed for inline bots
                                    }

                                    DownloadGifAsync(mediaBase);
                                }
                            }

                            return;
                        }
                    }
                }
            }
        }

        private static string GetFrameFileName(IMediaGifBase mediaGifBase)
        {
            var frameFileName = string.Empty;

            var mediaGif = mediaGifBase as IMediaGif;
            if (mediaGif != null)
            {
                var document = mediaGif.Document as TLDocument;
                if (document != null)
                {
                    frameFileName = string.Format("frame{0}.jpg", document.Id);
                }

                var documentExternal = mediaGif.Document as TLDocumentExternal;
                if (documentExternal != null)
                {
                    frameFileName = string.Format("frame{0}.jpg", documentExternal.Id);
                }
            }

            var decryptedMediaGif = mediaGifBase as IDecryptedMediaGif;
            if (decryptedMediaGif != null)
            {
                var document = decryptedMediaGif.Document;
                if (document != null)
                {
                    var fileLocatioin = document.File as TLEncryptedFile;
                    if (fileLocatioin != null)
                    {
                        frameFileName = string.Format("frame{0}.jpg", fileLocatioin.Id);
                    }
                }
            }

            return frameFileName;
        }

        private static string GetLocalFileName(IMediaGifBase mediaGifBase)
        {
            var localFileName = string.Empty;

            var mediaGif = mediaGifBase as IMediaGif;
            if (mediaGif != null)
            {
                var document = mediaGif.Document as TLDocument;
                if (document != null)
                {
                    localFileName = document.GetFileName();
                }

                var documentExternal = mediaGif.Document as TLDocumentExternal;
                if (documentExternal != null)
                {
                    localFileName = documentExternal.GetFileName();
                }
            }

            var decryptedMediaGif = mediaGifBase as IDecryptedMediaGif;
            if (decryptedMediaGif != null)
            {
                var document = decryptedMediaGif.Document;
                if (document != null)
                {
                    localFileName = document.GetFileName();
                }
            }

            return localFileName;
        }

        private static bool CheckDocumentParams(IMediaGifBase mediaGifBase)
        {
            var mediaGif = mediaGifBase as IMediaGif;
            if (mediaGif != null)
            {
                var document = mediaGif.Document as TLDocument;
                if (document != null)
                {
                    return document.Size.Value <= Telegram.Api.Constants.AutoDownloadGifMaxSize;
                }
            }

            var decryptedMediaGif = mediaGifBase as IDecryptedMediaGif;
            if (decryptedMediaGif != null)
            {
                var document = decryptedMediaGif.Document;
                if (document != null)
                {
                    return document.Size.Value <= Telegram.Api.Constants.AutoDownloadGifMaxSize;
                }
            }

            return true;
        }

        private static void DownloadGifAsync(IMediaGifBase owner)
        {
            if (owner.Forbidden) return;

            var decryptedMediaGif = owner as IDecryptedMediaGif;
            if (decryptedMediaGif != null)
            {
                var encryptedFile = decryptedMediaGif.Document.File as TLEncryptedFile;
                if (encryptedFile != null)
                {
                    owner.DownloadingProgress = 0.01;

                    Execute.BeginOnThreadPool(() =>
                    {
                        var fileManager = IoC.Get<IEncryptedFileManager>();
                        fileManager.DownloadFile(
                            encryptedFile,
                            owner as TLObject,
                            async item =>
                            {
                                var fileName = item.IsoFileName;

                                var newFileName = String.Format("{0}_{1}_{2}.{3}",
                                    encryptedFile.Id,
                                    encryptedFile.DCId,
                                    encryptedFile.AccessHash,
                                    encryptedFile.FileExt ?? decryptedMediaGif.Document.FileExt);
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    byte[] buffer;
                                    using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                                    {
                                        buffer = new byte[file.Length];
                                        file.Read(buffer, 0, buffer.Length);
                                    }

                                    var decryptedBuffer = Telegram.Api.Helpers.Utils.AesIge(buffer, decryptedMediaGif.Document.Key.Data, decryptedMediaGif.Document.IV.Data, false);

                                    using (var file = store.OpenFile(newFileName, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        file.Write(decryptedBuffer, 0, decryptedBuffer.Length);
                                    }

                                    store.DeleteFile(fileName);
                                }

                                //await ExtractFirstFrame(owner);

                                Execute.BeginOnUIThread(() =>
                                {
                                    owner.IsCanceled = false;
                                    owner.LastProgress = 0.0;
                                    owner.DownloadingProgress = 0.0;
                                    owner.IsoFileName = newFileName;
                                });
                            });
                    });
                }

                return;
            }

            var mediaGif = owner as IMediaGif;
            if (mediaGif != null)
            {
                var documentBase = mediaGif.Document;

                var documentExternal = documentBase as TLDocumentExternal;
                if (documentExternal != null)
                {
                    owner.DownloadingProgress = 0.01;

                    var fileName = documentExternal.GetFileName();

                    var webClient = new WebClient();
                    webClient.OpenReadAsync(new Uri(documentExternal.ContentUrl.ToString(), UriKind.Absolute));
                    webClient.OpenReadCompleted += (sender2, args2) => Execute.BeginOnThreadPool(async () =>
                    {
                        if (args2.Cancelled)
                        {
                            Execute.BeginOnUIThread(() =>
                            {
                                owner.IsCanceled = false;
                                owner.LastProgress = 0.0;
                                owner.DownloadingProgress = 0.0;
                            });

                            Execute.ShowDebugMessage(args2.Cancelled.ToString());
                            return;
                        }
                        if (args2.Error != null)
                        {
                            var webException = args2.Error as WebException;
                            if (webException != null)
                            {
                                var response = webException.Response as HttpWebResponse;
                                if (response != null)
                                {
                                    if (response.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                        Execute.BeginOnUIThread(() =>
                                        {
                                            owner.Forbidden = true;
                                            owner.IsCanceled = false;
                                            owner.LastProgress = 0.0;
                                            owner.DownloadingProgress = 0.0;
                                        });
                                    }
                                    else if (response.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        Execute.BeginOnUIThread(TimeSpan.FromSeconds(5.0), () =>
                                        {
                                            owner.IsCanceled = false;
                                            owner.LastProgress = 0.0;
                                            owner.DownloadingProgress = 0.0;
                                        });
                                    }

                                    return;
                                }
                            }

                            Execute.BeginOnUIThread(() =>
                            {
                                owner.IsCanceled = false;
                                owner.LastProgress = 0.0;
                                owner.DownloadingProgress = 0.0;
                            });

                            Execute.ShowDebugMessage(args2.Error.ToString());
                            return;
                        }

                        try
                        {
                            using (var stream = args2.Result)
                            {
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    if (store.FileExists(fileName))
                                    {
                                        store.DeleteFile(fileName);
                                    }

                                    using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                                    {
                                        const int BUFFER_SIZE = 128 * 1024;
                                        var buf = new byte[BUFFER_SIZE];

                                        var bytesRead = 0;
                                        while ((bytesRead = stream.Read(buf, 0, BUFFER_SIZE)) > 0)
                                        {
                                            file.Write(buf, 0, bytesRead);
                                        }
                                    }
                                }
                            }

                            //await ExtractFirstFrame(owner);

                            Execute.BeginOnUIThread(() =>
                            {
                                owner.IsCanceled = false;
                                owner.LastProgress = 0.0;
                                owner.DownloadingProgress = 0.0;
                            });
                        }
                        catch (Exception ex)
                        {
                            Execute.BeginOnUIThread(() =>
                            {
                                owner.IsCanceled = false;
                                owner.LastProgress = 0.0;
                                owner.DownloadingProgress = 0.0;
                            });

                            Execute.ShowDebugMessage(ex.ToString());
                        }
                    });


                    return;
                }

                var document = documentBase as TLDocument;
                if (document != null)
                {
                    owner.DownloadingProgress = 0.01;

                    var fileManager = IoC.Get<IDocumentFileManager>();
                    fileManager.DownloadFileAsync(
                        document.FileName,
                        document.DCId,
                        document.ToInputFileLocation(),
                        owner as TLObject,
                        document.Size,
                        null,
                        async item =>
                        {
                            //await ExtractFirstFrame(owner);
                            
                            Execute.BeginOnUIThread(() =>
                            {
                                owner.IsCanceled = false;
                                owner.LastProgress = 0.0;
                                owner.DownloadingProgress = 0.0;
                                owner.IsoFileName = item.IsoFileName;
                            });
                        });

                    return;
                }
            }
        }

        private static void SetMediaPreview(GifPlayerControl player, IMediaGifBase mediaGifBase)
        {
            var frameFileName = GetFrameFileName(mediaGifBase);
            player._firstFrameFileName = frameFileName;
            player._extractFirstFrame = true;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(frameFileName))
                {
                    player._extractFirstFrame = false;
                    try
                    {
                        using (var stream = store.OpenFile(frameFileName, FileMode.Open, FileAccess.Read))
                        {
                            if (stream.Length > 0)
                            {
                                var bitmapImage = new BitmapImage();
                                //bitmapImage.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                                bitmapImage.SetSource(stream);

                                player.Thumb.Source = bitmapImage;
                                return;
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            var decryptedMediaGif = mediaGifBase as IDecryptedMediaGif;
            if (decryptedMediaGif != null)
            {
                var document = decryptedMediaGif.Document;
                if (document != null)
                {
                    var thumbCachedSize = document.Thumb;
                    if (thumbCachedSize != null 
                        && thumbCachedSize.Data != null 
                        && thumbCachedSize.Data.Length > 0
                        && document.ThumbW.Value > 0
                        && document.ThumbH.Value > 0)
                    {
                        BitmapImage preview;
                        try
                        {
                            var buffer = thumbCachedSize.Data;
                            var bitmap = PictureDecoder.DecodeJpeg(new MemoryStream(buffer));

                            new PhotoToThumbConverter().BlurBitmap(bitmap, false);

                            var blurredStream = new MemoryStream();
                            bitmap.SaveJpeg(blurredStream, document.ThumbW.Value, document.ThumbH.Value, 0, 100);

                            player.Thumb.Source = ImageUtils.CreateImage(blurredStream);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        player.Thumb.Source = null;
                    }
                }
            }

            var mediaGif = mediaGifBase as IMediaGif;
            if (mediaGif != null)
            {
                var documentBase = mediaGif.Document;

                var documentExternal = documentBase as TLDocumentExternal;
                if (documentExternal != null)
                {
                    if (!TLString.IsNullOrEmpty(documentExternal.ThumbUrl))
                    {
                        player.Thumb.Source = new BitmapImage(new Uri(documentExternal.ThumbUrl.ToString(), UriKind.Absolute));
                    }
                    else
                    {
                        player.Thumb.Source = null;
                    }

                    return;
                }

                var document = documentBase as TLDocument;
                if (document != null)
                {
                    var options = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation;
                    var thumbCachedSize = document.Thumb as TLPhotoCachedSize;
                    if (thumbCachedSize != null)
                    {
                        BitmapImage preview;
                        if (PhotoToThumbConverter.TryGetPhotoPreview(document.Id, out preview, options))
                        {
                            player.Thumb.Source = preview;
                        }
                        else
                        {
                            if (thumbCachedSize.Bytes != null
                                && thumbCachedSize.Bytes.Data != null
                                && thumbCachedSize.Bytes.Data.Length > 0
                                && thumbCachedSize.W.Value > 0
                                && thumbCachedSize.H.Value > 0)
                            {
                                try
                                {
                                    var buffer = thumbCachedSize.Bytes.Data;
                                    var bitmap = PictureDecoder.DecodeJpeg(new MemoryStream(buffer));

                                    new PhotoToThumbConverter().BlurBitmap(bitmap, false);

                                    var blurredStream = new MemoryStream();
                                    bitmap.SaveJpeg(blurredStream, thumbCachedSize.W.Value, thumbCachedSize.H.Value, 0, 100);

                                    var previewFileName = string.Format("preview{0}.jpg", document.Id);

                                    Execute.BeginOnThreadPool(() => PhotoToThumbConverter.SaveFile(previewFileName, blurredStream));

                                    player.Thumb.Source = ImageUtils.CreateImage(blurredStream);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            else
                            {
                                player.Thumb.Source = null;
                            }
                        }
                    }
                    else
                    {
                        var photoSize = document.Thumb as TLPhotoSize;
                        if (photoSize != null)
                        {
                            BitmapImage preview;
                            if (PhotoToThumbConverter.TryGetPhotoPreview(document.Id, out preview, options))
                            {
                                player.Thumb.Source = preview;
                            }
                            else
                            {
                                var fileLocation = photoSize.Location as TLFileLocation;
                                if (fileLocation != null)
                                {
                                    var previewFileName = string.Format("preview{0}.jpg", document.Id);
                                    var previewSourceFileName = String.Format("{0}_{1}_{2}.jpg",
                                        fileLocation.VolumeId,
                                        fileLocation.LocalId,
                                        fileLocation.Secret);

                                    if (!CreatePreview(player, mediaGif as TLObject, previewSourceFileName, previewFileName, photoSize))
                                    {
                                        var fileManager = IoC.Get<IFileManager>();
                                        fileManager.DownloadFile(fileLocation, mediaGif as TLObject, photoSize.Size,
                                            item => Execute.BeginOnUIThread(() =>
                                            {
                                                CreatePreview(player, item.Owner, previewSourceFileName, previewFileName, photoSize);
                                            }));
                                    }
                                }
                            }
                        }
                    }
            }


                return;
            }
        }

        private static bool CreatePreview(GifPlayerControl player, TLObject previewOwner, string previewSourceFileName, string previewFileName, TLPhotoSize photoSize)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(previewSourceFileName)) return false;

                try
                {
                    using (var stream = store.OpenFile(previewSourceFileName, FileMode.Open, FileAccess.Read))
                    {
                        if (stream.Length == 0) return false;

                        stream.Seek(0, SeekOrigin.Begin);

                        var bitmap = PictureDecoder.DecodeJpeg(stream);

                        new PhotoToThumbConverter().BlurBitmap(bitmap, false);

                        var blurredStream = new MemoryStream();
                        bitmap.SaveJpeg(blurredStream, photoSize.W.Value, photoSize.H.Value, 0, 100);

                        if (player.Media == previewOwner)
                        {
                            player.Thumb.Source = ImageUtils.CreateImage(blurredStream);
                        }

                        Execute.BeginOnThreadPool(() => PhotoToThumbConverter.SaveFile(previewFileName, blurredStream));
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        private static void InitializePlayer(IMediaGifBase mediaBase, GifPlayerControl player)
        {
            var mediaDocument = mediaBase;
            if (mediaDocument != null)
            {
                var fileName = GetLocalFileName(mediaDocument);
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (store.FileExists(fileName))
                        {
                            player.ChangeButtonState(GifButtonState.Play, "InitializePlayer");
                            player._decoderParams = new int[3];
                            try
                            {
                                lock (player._ffmpegLock)
                                {
                                    player._handler = FFmpegGifDecoder.CreateDecoder(Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName), player._decoderParams);
                                    if (player._handler != 0)
                                    {
                                        player._w = player._decoderParams[0];
                                        player._h = player._decoderParams[1];

                                        if (player._w > 0 && player._h > 0)
                                        {
                                            player._initialized = true;
                                        }
                                    }
                                }
                            }
                            catch (FileNotFoundException ex)
                            {
                                player.Width = 323.0;
                                player.Height = 150.0;
                                player.PlayButton.Opacity = 0.5;
                            }
                            catch (Exception ex)
                            {
                                player.Width = 323.0;
                                player.Height = 150.0;
                                player.PlayButton.Opacity = 0.5;
#if DEBUG
                                MessageBox.Show("OnFileNameChanged " + ex);
#endif
                            }
                        }
                        else
                        {
                            if (player.Progress > 0.0 && player.Progress < 1.0)
                            {
                                player.ChangeButtonState(GifButtonState.Cancel);
                            }
                            else
                            {
                                if (player.Mode == GifPlayerMode.InlineResult)
                                {
                                    if (mediaBase.Forbidden)
                                    {
                                        player.ChangeButtonState(GifButtonState.None);
                                    }
                                    else
                                    {
                                        DownloadGifAsync(mediaBase);

                                        player.ChangeButtonState(GifButtonState.Cancel);
                                    }

                                    return;
                                }

                                player.ChangeButtonState(GifButtonState.Download);
                            }
                        }
                    }
                }
            }
            
        }

        private int _handler;

        private readonly Timer _frameTimer;

        private int[] _decoderParams;

        private int _w;

        private int _h;

        public double _ptsDelta;

        public double _timeDelta;

        public double _pts;

        public TimeSpan _elapsed;

        private bool _isPlaying;

        private bool _initialized;

        private string _firstFrameFileName;

        private bool _extractFirstFrame;

        private byte[] _frame;

        private bool _destroyDecoder;

        private bool _pauseDecoder;

        private DateTime _startTime;

        private object _ffmpegLock = new object();

        public GifPlayerControl()
        {
            InitializeComponent();
            
            _frameTimer = new Timer(FrameTimer_OnTick, this, Timeout.Infinite, Timeout.Infinite);
        }

        public bool Start()
        {
            if (Mode == GifPlayerMode.InlineResult)
            {
                if (!_inlineBotActivePlayers.Contains(this))
                {
                    _inlineBotActivePlayers.Add(this);
                }
            }
            else
            {
                if (!_activePlayers.Contains(this))
                {
                    _activePlayers.Add(this);
                }
            }

            if (_activePlayers.Count > 0 || _inlineBotActivePlayers.Count > 0)
            {
                StartUpdateFrames();
            }

            if (!_initialized)
            {
                InitializePlayer(Media, this);
                if (!_initialized)
                {
                    if (Progress > 0.0 && Progress < 1.0)
                    {
                        return true;
                    }

                    return false;
                }
            }

            var pts = _ptsDelta == 0 ? 50 : _ptsDelta;
            _startTime = DateTime.Now;
            _frameTimer.Change(TimeSpan.FromMilliseconds(pts), Timeout.InfiniteTimeSpan);
            _isPlaying = true;

            _frame = null;
            ChangeButtonState(GifButtonState.None);

            return true;
        }

        private void Pause()
        {
            if (!_initialized) return;

            lock (_ffmpegLock)
            {
                _pauseDecoder = true;
            }
        }

        private void Resume()
        {
            if (!_initialized)
            {
                Start();
                return;
            }

            lock (_ffmpegLock)
            {
                _pauseDecoder = false;
            }
            _frameTimer.Change(TimeSpan.FromMilliseconds(1.0), Timeout.InfiniteTimeSpan);
            ChangeButtonState(GifButtonState.None);
        }

        public bool Stop(bool autoPlayGif = false)
        {
            if (Mode == GifPlayerMode.InlineResult)
            {
                _inlineBotActivePlayers.Remove(this);
            }
            else
            {
                _activePlayers.Remove(this);
            }

            if (_activePlayers.Count == 0 && _inlineBotActivePlayers.Count == 0)
            {
                StopUpdateFrames();
            }

            if (!_initialized) return false;

            lock (_ffmpegLock)
            {
                _destroyDecoder = true;
                _frameTimer.Change(TimeSpan.FromMilliseconds(1.0), Timeout.InfiniteTimeSpan);
            }
            ChangeButtonState(GifButtonState.Play, "Stop autoplay=" + autoPlayGif, autoPlayGif);

            Frame.Source = null;
            return true;
        }

        public void Release()
        {
            Frame.Source = null;
            PlayButton.Opacity = 1.0;
            Debug.Text = null;
            _isPlaying = false;
            _initialized = false;
            _w = 0;
            _h = 0;
            _pts = 0;
        }

        private void FrameTimer_OnTick(object state)
        {
            lock (_ffmpegLock)
            {
                if (_destroyDecoder)
                {
                    _destroyDecoder = false;
                    _pauseDecoder = false;
                    _isPlaying = false;

                    try
                    {
                        if (_handler != 0)
                        {
                            FFmpegGifDecoder.DestroyDecoder(_handler);
                            _handler = 0;
                        }
                        _initialized = false;
                    }
                    catch (Exception ex)
                    {
                        Execute.ShowDebugMessage("FrameTime_OnTick.DestroyDecoder exception\n" + ex);
                    }

                    return;
                }

                if (_pauseDecoder)
                {
                    _pauseDecoder = false;
                    _frameTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    return;
                }
            }

            var stopwatch = Stopwatch.StartNew();

            var frameParams = new int[3];
            lock (_ffmpegLock)
            {
                _frame = FFmpegGifDecoder.GetVideoFrame(_handler, frameParams);
            }

            if (_frame == null)
            {
                var pts = _pts;

                Execute.BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("GifPlayer.GetVideoFrame=null pts=" + pts + " frame_params=" + string.Join(",", frameParams));

                    Stop();
                });
                return;
            }

            _ptsDelta = 0.0;
            if (frameParams[2] <= 0)
            {
                _startTime = DateTime.Now;
                _ptsDelta = 1.0;
                _pts = 0.0;

                ExtractFirstFrame(_frame);
            }
            else
            {
                _pts = frameParams[2];
                _timeDelta = (DateTime.Now - _startTime).TotalMilliseconds;
                var delta = (frameParams[2] - _timeDelta);
                _ptsDelta = delta < 0.0 ? 1.0 : delta;
            }

            _elapsed = stopwatch.Elapsed;

            lock (_ffmpegLock)
            {
                _frameTimer.Change(TimeSpan.FromMilliseconds(_ptsDelta), Timeout.InfiniteTimeSpan);
            }
        }

        private void ExtractFirstFrame(byte[] frame)
        {
            if (!_extractFirstFrame) return;

            _extractFirstFrame = false;

            Execute.BeginOnThreadPool(() => SaveFirstFrame(frame));
        }

        private async void SaveFirstFrame(byte[] frame)
        {
            var fileName = _firstFrameFileName;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName) && frame.Length > 0)
                {
                    var pixels = new int[frame.Length / sizeof(int)];
                    for (var j = 0; j < pixels.Length; j++)
                    {
                        pixels[j] =
                            (frame[j * 4 + 3] << 24) +     //b
                            (frame[j * 4 + 2] << 0) +      //g
                            (frame[j * 4 + 1] << 8) +      //r
                            (frame[j * 4] << 16);          //a 

                        //bitmap.Pixels[j] = pixels[j];
                    }

                    Buffer.BlockCopy(pixels, 0, frame, 0, frame.Length);

                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)_w, (uint)_h, 96.0, 96.0, frame);
                        await encoder.FlushAsync();
                    }

                    Execute.BeginOnUIThread(() =>
                    {
                        SetMediaPreview(this, Media);
                    });
                }
            }
        }

        public void ToggleVideoPlay()
        {
            if (MediaPlaceholder.Background != null)
            {
                if (MessagePlayerControl.Player.CurrentState == MediaElementState.Playing)
                {
                    MessagePlayerControl.Player.Pause();
                    RaiseMediaStateChanged(Media, GifPlayerControlState.Paused);
                }
                else if (MessagePlayerControl.Player.CurrentState == MediaElementState.Paused)
                {
                    MessagePlayerControl.Player.Play();
                    RaiseMediaStateChanged(Media, GifPlayerControlState.Resumed);
                }
            }
        }

        private void LayoutRoot_OnTap(object sender, GestureEventArgs e)
        {
            if (Mode == GifPlayerMode.InlineResult) return;

            if (Mode == GifPlayerMode.RoundVideo)
            {
                // open external player for w10m
                if (System.Environment.OSVersion.Version.Major >= 10)
                {
                    return;
                }

                // open in-app player for wp8.1
                var mediaBase = Media;
                if (mediaBase != null)
                {
                    var player = MessagePlayerControl.Player.Tag as GifPlayerControl;
                    if (player != null
                        && player == this
                        && MediaPlaceholder.Background != null)
                    {
                        ToggleVideoPlay();

                        e.Handled = true;
                    }
                    else
                    {
                        var localFileName = GetLocalFileName(mediaBase);

                        if (!string.IsNullOrEmpty(localFileName))
                        {
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (store.FileExists(localFileName))
                                {
                                    PauseActivePlayers();

                                    if (player != null)
                                    {
                                        player.MuteIcon.Visibility = Visibility.Visible;
                                        player.MediaPlaceholder.Background = null;
                                    }

                                    MessagePlayerControl.Player.Tag = this;
                                    MessagePlayerControl.Player.Position = TimeSpan.FromSeconds(0.0);
                                    var videoBrush = new VideoBrush();
                                    videoBrush.SetSource(MessagePlayerControl.Player);
                                    MediaPlaceholder.Background = videoBrush;
                                    MuteIcon.Visibility = Visibility.Collapsed;

                                    var file = store.OpenFile(localFileName, FileMode.Open, FileAccess.Read);

                                    RaiseMediaStateChanged(Media, GifPlayerControlState.Opening);

                                    Execute.BeginOnUIThread(() =>
                                    {
                                        MessagePlayerControl.Player.SetSource(file);
                                        file.Dispose();
                                        //MessagePlayerControl.Player.MediaOpened += PlayerOnMediaOpened;
                                        //MessagePlayerControl.Player.MediaFailed += PlayerOnMediaFailed;
                                        //MessagePlayerControl.Player.MediaEnded += PlayerOnMediaEnded;
                                    });

                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }

                return;
            }

            if (Mode == GifPlayerMode.Normal)
            {
                if (_isPlaying)
                {
                    if (Media != null) Media.AutoPlayGif = false;
                    e.Handled = Stop();
                }
                else
                {
                    if (Media != null) Media.AutoPlayGif = true;
                    e.Handled = Start();
                }

                return;
            }
        }

        private void CancelDownloadButton_OnTap(object sender, GestureEventArgs e)
        {
            if (Mode == GifPlayerMode.InlineResult) return;

            e.Handled = true;
            var localFileName = GetLocalFileName(Media);
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(localFileName))
                {
                    ChangeButtonState(GifButtonState.Download);
                }
                else
                {
                    ChangeButtonState(GifButtonState.Play);
                }
            }
            RaiseCancelDownload();
        }

        private enum GifButtonState
        {
            Play,
            Download,
            Cancel,
            None
        }

        private GifButtonState? _lastState;

        private void ChangeButtonState(GifButtonState state, string place = null, bool autoPlayGif = false)
        {
            if (_lastState == state) return;

//#if DEBUG
            //Debug2.Text = state + " place=" + place;
//#endif
            _lastState = state;
            if (Mode == GifPlayerMode.RoundVideo)
            {
                MuteIcon.Visibility = MediaPlaceholder.Background == null ? Visibility.Visible : Visibility.Collapsed;
                PlayButton.Visibility = Visibility.Collapsed;
                Play.Visibility = Visibility.Collapsed;
                PlayCircle.Visibility = Visibility.Collapsed;
                DownloadButton.Visibility = state == GifButtonState.Download ? Visibility.Visible : Visibility.Collapsed;
                CancelDownloadButton.Visibility = state == GifButtonState.Cancel ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                MuteIcon.Visibility = Visibility.Collapsed;
                PlayButton.Visibility = state == GifButtonState.Play ? Visibility.Visible : Visibility.Collapsed;
                Play.Visibility = autoPlayGif ? Visibility.Collapsed : Visibility.Visible;
                PlayCircle.Visibility = autoPlayGif ? Visibility.Collapsed : Visibility.Visible;
                DownloadButton.Visibility = state == GifButtonState.Download ? Visibility.Visible : Visibility.Collapsed;
                CancelDownloadButton.Visibility = state == GifButtonState.Cancel ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public event EventHandler CancelDownload;

        protected virtual void RaiseCancelDownload()
        {
            var handler = CancelDownload;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private bool _suppressManipulationStarted = true; // in order to suppress manipulationstarted event on longlistselector

        public bool SuppressManipulationStarted
        {
            get { return _suppressManipulationStarted; }
            set { _suppressManipulationStarted = value; }
        }

        public Geometry FrameClip
        {
            get { return FrameGrid.Clip; }
            set
            {
                FrameGrid.Clip = value;
            }
        }

        private void LayoutRoot_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = SuppressManipulationStarted;
        }

        public void OnMediaEnded()
        {
            MediaPlaceholder.Background = null;
            MuteIcon.Visibility = Visibility.Visible;

            RaiseMediaStateChanged(Media, GifPlayerControlState.Ended);
        }

        public void OnMediaFailed(ExceptionRoutedEventArgs e)
        {
            MuteIcon.Visibility = Visibility.Visible;

            RaiseMediaStateChanged(Media, GifPlayerControlState.Failed);
        }

        public void OnMediaOpened()
        {
            MessagePlayerControl.Player.Play();

            RaiseMediaStateChanged(Media, GifPlayerControlState.Opened);
        }

        public static event EventHandler<MediaStateChangedEventArgs> MediaStateChanged;

        public static void RaiseMediaStateChanged(IMediaGifBase media, GifPlayerControlState state)
        {
            var handler = MediaStateChanged;
            if (handler != null) handler(null, new MediaStateChangedEventArgs(media, state));
        }

        public static void StopVideo()
        {
            var player = MessagePlayerControl.Player.Tag as GifPlayerControl;
            if (player != null)
            {
                if (MessagePlayerControl.Player.CurrentState == MediaElementState.Playing)
                {
                    MessagePlayerControl.Player.Stop();
                }

                player.MediaPlaceholder.Background = null;
                player.MuteIcon.Visibility = Visibility.Visible;

                RaiseMediaStateChanged(player.Media, GifPlayerControlState.Ended);
            }
        }
    }

    public enum GifPlayerMode
    {
        Normal,
        InlineResult,
        RoundVideo
    }

    public enum GifPlayerControlState
    {
        Opening,
        Opened,
        Failed,
        Ended,
        Paused,
        Resumed
    }

    public class MediaStateChangedEventArgs : System.EventArgs
    {
        public GifPlayerControlState State { get; protected set; }

        public IMediaGifBase Media { get; protected set; }

        public MediaStateChangedEventArgs(IMediaGifBase media, GifPlayerControlState state)
        {
            Media = media;
            State = state;
        }
    }
}
