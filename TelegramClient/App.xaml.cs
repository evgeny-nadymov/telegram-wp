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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
#if WP81
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.ApplicationModel.Activation;
#endif
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Passport;
using TelegramClient.Views.Dialogs;
using TelegramClient.Views.Passport;
#if WP8
using Windows.Storage;
#endif
namespace TelegramClient
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        //public PhoneApplicationFrame RootFrame { get; private set; }

        public static Stopwatch Timer = Stopwatch.StartNew();

        public static Stopwatch StartupStimer = Stopwatch.StartNew();

        public static void Log(string str)
        {
            Debug.WriteLine("{0} {1}", StartupStimer.Elapsed, str);
        }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            Log("App start .ctor");

            Telegram.Api.Helpers.Execute.IsForegroundApp = true;
            //var color = Colors.Magenta;
            //Resources.Remove("PhoneAccentColor");
            //Resources.Add("PhoneAccentColor", color);
            //((SolidColorBrush)Resources["PhoneAccentBrush"]).Color = color;

            // Standard Silverlight initialization
            InitializeComponent();

            //RootVisual = new PhoneApplicationFrame();

            Log("App start InitilizeComponent");

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.


                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            //Current.Host.Settings.EnableRedrawRegions = true;
            //Current.Host.Settings.EnableFrameRateCounter = true;
#if DEBUG
            Current.Host.Settings.EnableFrameRateCounter = true;
            //Current.Host.Settings.EnableRedrawRegions = true;
#endif
#if WP81
            PhoneApplicationService.Current.Activated += OnActivated;
            PhoneApplicationService.Current.ContractActivated += OnContractActivated;
#endif
//#if WP8
//            ApplicationLifetimeObjects.Add(new XnaAsyncDispatcher(TimeSpan.FromMilliseconds(50)));
//#endif
            Windows.ApplicationModel.Core.CoreApplication.UnhandledErrorDetected += (sender, args) =>
            {
                try
                {
                    Telegram.Logs.Log.SyncWrite(args.UnhandledError.ToString());
#if DEBUG
                    Caliburn.Micro.Execute.OnUIThread(() => MessageBox.Show("UnhandledErrorDetected\n" + args.UnhandledError));
#endif
                }
                catch (Exception ex)
                {

                }

                if (!args.UnhandledError.Handled)
                {
                    try
                    {
                        args.UnhandledError.Propagate();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Telegram.Logs.Log.SyncWrite(args.UnhandledError.ToString());
#if DEBUG
                            Caliburn.Micro.Execute.OnUIThread(() => MessageBox.Show("UnhandledErrorDetected Propogate\n" + args.UnhandledError));
#endif
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
                //args.UnhandledError.Handled = true;
            };
            UnhandledException += (sender, args) =>
            {
                try
                {
                    Telegram.Logs.Log.SyncWrite(args.ExceptionObject.ToString());
#if DEBUG
                    Caliburn.Micro.Execute.OnUIThread(() => MessageBox.Show(args.ExceptionObject.ToString()));
#endif
                }
                catch (Exception ex)
                {

                }
                args.Handled = true;
            };

            Log("App stop .ctor");
        }

        public ChooseFileInfo ChooseFileInfo { get; set; }

#if WP81
        public ShareOperation ShareOperation { get; set; }
#endif

        private void OnActivated(object sender, ActivatedEventArgs e)
        {
            
        }

        public bool Offline { get; set; }
#if WP8
        public static IReadOnlyCollection<StorageFile> Photos { get; set; }

        public static StorageFile Video { get; set; }
#endif

#if WP81
        private void OnContractActivated(object sender, IActivatedEventArgs e)
        {
            var saveArgs = e as FileSavePickerContinuationEventArgs;
            if (saveArgs != null)
            {
                object from;
                if (saveArgs.ContinuationData != null
                    && saveArgs.ContinuationData.TryGetValue("From", out from))
                {
                    if (string.Equals(from, "DialogDetailsView"))
                    {
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => DialogDetailsViewModel.SaveFile(saveArgs.File));

                        return;
                    }
                }
            }

            var args = e as FileOpenPickerContinuationEventArgs;
            if (args != null)
            {
                object from;
                if (args.ContinuationData != null
                    && args.ContinuationData.TryGetValue("From", out from))
                {
                    if (string.Equals(from, "DialogDetailsView"))
                    {
                        var contentControl = RootVisual as ContentControl;
                        if (contentControl != null)
                        {
                            var dialogDetailsView = contentControl.Content as DialogDetailsView;
                            if (dialogDetailsView != null)
                            {
                                var dialogDetailsViewModel = dialogDetailsView.DataContext as DialogDetailsViewModel;
                                if (dialogDetailsViewModel != null)
                                {
                                    object type;
                                    if (!args.ContinuationData.TryGetValue("Type", out type))
                                    {
                                        type = "Document";
                                    }

                                    if (string.Equals(type, "Video"))
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => dialogDetailsViewModel.SendVideo(file));
                                    }
                                    else if (string.Equals(type, "Image"))
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        if (file != null)
                                        {
#if MULTIPLE_PHOTOS
                                            Photos = args.Files;
                                            return;
#endif

#if WP81
                                            Telegram.Api.Helpers.Execute.BeginOnThreadPool(async () =>
                                            {
                                                var randomStream = await file.OpenReadAsync();

                                                await ChooseAttachmentViewModel.Handle(IoC.Get<IStateService>(), randomStream, file.Name);

                                                //MessageBox.Show("OnContractActivated after handle");
                                                dialogDetailsViewModel.BackwardInAnimationComplete();
                                            });
#else
                                            Telegram.Api.Helpers.Execute.BeginOnThreadPool(async () =>
                                            {
                                                var randomStream = await file.OpenReadAsync();
                                                var chosenPhoto = randomStream.AsStreamForRead();

                                                //MessageBox.Show("OnContractActivated stream");
                                                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                                                {
                                                    ChooseAttachmentViewModel.Handle(IoC.Get<IStateService>(), chosenPhoto, file.Name);

                                                    //MessageBox.Show("OnContractActivated after handle");
                                                    dialogDetailsViewModel.BackwardInAnimationComplete();
                                                });
                                            });
#endif


                                        }
                                    }
                                    else
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => dialogDetailsViewModel.SendDocument(file));
                                    }

                                    return;
                                }
                            }
                        }
                    }
                    else if (string.Equals(from, "SecretDialogDetailsView"))
                    {
                        var contentControl = RootVisual as ContentControl;
                        if (contentControl != null)
                        {
                            var dialogDetailsView = contentControl.Content as SecretDialogDetailsView;
                            if (dialogDetailsView != null)
                            {
                                var secretDialogDetailsViewModel = dialogDetailsView.DataContext as SecretDialogDetailsViewModel;
                                if (secretDialogDetailsViewModel != null)
                                {
                                    object type;
                                    if (!args.ContinuationData.TryGetValue("Type", out type))
                                    {
                                        type = "Document";
                                    }

                                    if (string.Equals(type, "Video"))
                                    //{
                                    //    var file = args.Files.FirstOrDefault();
                                    //    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => dialogDetailsViewModel.EditVideo(file));
                                    //}
                                    //else
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => secretDialogDetailsViewModel.SendDocument(file));
                                    }
                                    else if (string.Equals(type, "Image"))
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        if (file != null)
                                        {
#if MULTIPLE_PHOTOS
                                            Photos = args.Files;
                                            return;
#endif
#if WP81
                                            Telegram.Api.Helpers.Execute.BeginOnThreadPool(async () =>
                                            {
                                                var randomStream = await file.OpenReadAsync();
                                                await ChooseAttachmentViewModel.Handle(IoC.Get<IStateService>(), randomStream, file.Name);
                                                secretDialogDetailsViewModel.OnBackwardInAnimationComplete();
                                            });
#else
                                            Telegram.Api.Helpers.Execute.BeginOnThreadPool(async () =>
                                            {
                                                var randomStream = await file.OpenReadAsync();
                                                var chosenPhoto = randomStream.AsStreamForRead();

                                                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                                                {
                                                    ChooseAttachmentViewModel.Handle(IoC.Get<IStateService>(), chosenPhoto, file.Name);
                                                });
                                            });
#endif
                                        }
                                    }
                                    else
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => secretDialogDetailsViewModel.SendDocument(file));
                                    }

                                    return;
                                }
                            }
                        }
                    }
                    else if (string.Equals(from, "ResidentialAddressView"))
                    {
                        var contentControl = RootVisual as ContentControl;
                        if (contentControl != null)
                        {
                            var view = contentControl.Content as ResidentialAddressView;
                            if (view != null)
                            {
                                object type;
                                if (!args.ContinuationData.TryGetValue("Type", out type))
                                {
                                    type = "Document";
                                }

                                if (type is string)
                                {
                                    var viewModel = view.DataContext as ResidentialAddressViewModel;
                                    if (viewModel != null)
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => viewModel.SendDocument(type.ToString(), file));

                                        return;
                                    }
                                }
                            }
                        }
                    }
                    else if (string.Equals(from, "PersonalDetailsView"))
                    {
                        var contentControl = RootVisual as ContentControl;
                        if (contentControl != null)
                        {
                            object type;
                            if (!args.ContinuationData.TryGetValue("Type", out type))
                            {
                                type = "Document";
                            }

                            if (type is string)
                            {
                                var view = contentControl.Content as PersonalDetailsView;
                                if (view != null)
                                {
                                    var viewModel = view.DataContext as PersonalDetailsViewModel;
                                    if (viewModel != null)
                                    {
                                        var file = args.Files.FirstOrDefault();
                                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => viewModel.SendDocument(type.ToString(), file));

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
#endif

#region Delayed Bugsense exceptions
        private readonly object _bugsenseSyncRoot = new object();

        public bool IsBugsenseInitialized { get; set; }
#endregion
    }

    public class ChooseFileInfo
    {
        public DateTime Time { get; set; }

        public ChooseFileInfo(DateTime time)
        {
            Time = time;
        }
    }
}