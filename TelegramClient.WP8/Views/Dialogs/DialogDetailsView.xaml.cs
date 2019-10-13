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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Controls.Extensions;
using Telegram.EmojiPanel;
using Telegram.EmojiPanel.Controls.Emoji;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Aggregator;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using TelegramClient.Controls;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Media;
using TelegramClient_Native;
using Action = System.Action;
using AudioEventArgs = TelegramClient.Views.Controls.AudioEventArgs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using StickerSelectedEventArgs = Telegram.EmojiPanel.Controls.Emoji.StickerSelectedEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class DialogDetailsView : IDialogDetailsView
    {
        public DialogDetailsViewModel ViewModel
        {
            get { return DataContext as DialogDetailsViewModel; }
        }

        public void MoveCurretToEnd()
        {
            if (FocusManager.GetFocusedElement() != InputMessage) return;

            InputMessage.SelectionStart = InputMessage.Text.Length;
            InputMessage.SelectionLength = 0;
        }

        private readonly AppBarButton _doneButton = new AppBarButton
        {
            Text = AppResources.Done,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        private readonly MenuItem _manageButton = new MenuItem
        {
            Header = AppResources.Select
        };

        private readonly MenuItem _searchMenuItem = new MenuItem
        {
            Header = AppResources.Search,
        };

        private readonly MenuItem _pinToStartMenuItem = new MenuItem
        {
            Header = AppResources.PinToStart
        };

        private readonly MenuItem _shareMyContactInfoMenuItem = new MenuItem
        {
            Header = AppResources.ShareMyContactInfo
        };

        private readonly MenuItem _helpMenuItem = new MenuItem
        {
            Header = AppResources.Help
        };

        private readonly MenuItem _reportSpamMenuItem = new MenuItem
        {
            Header = AppResources.ReportSpam
        };

        private readonly MenuItem _callMenuItem = new MenuItem
        {
            Header = AppResources.Call
        };

        private readonly MenuItem _debugMenuItem = new MenuItem
        {
            Header = "Debug"
        };

        private EmojiControl _emojiKeyboard;

        private TranslateTransform _frameTransform;

        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register(
            "RootFrameTransformProperty", typeof(double), typeof(DialogDetailsView), new PropertyMetadata(OnRootFrameTransformChanged));

        private static void OnRootFrameTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as DialogDetailsView;
            if (view != null)
            {
                view._frameTransform.Y = 0;
            }
        }

        public double RootFrameTransform
        {
            get { return (double)GetValue(RootFrameTransformProperty); }
            set { SetValue(RootFrameTransformProperty, value); }
        }

        private void SetRootFrameBinding()
        {
            var frame = (Frame)Application.Current.RootVisual;
            _frameTransform = ((TranslateTransform)((TransformGroup)frame.RenderTransform).Children[0]);
            var binding = new Binding("Y")
            {
                Source = _frameTransform
            };
            SetBinding(RootFrameTransformProperty, binding);
        }

        private void RemoveRootFrameBinding()
        {
            ClearValue(RootFrameTransformProperty);
        }

        private void CloseEmojiPlaceholder()
        {
            InputMessage.ClosePlaceholder();

            if (_emojiKeyboard != null)
            {
                _emojiKeyboard.ClosePreview();
                _emojiKeyboard.ReloadStickerSprites();
            }
        }

        private void OpenCommandsPlaceholder()
        {
            CommandsControl.Visibility = Visibility.Visible;
            InputMessage.KeyboardButtonImageString = GetReplyKeyboardImageString();
        }

        private void CloseCommandsPlaceholder()
        {
            CommandsControl.Visibility = Visibility.Collapsed;
            InputMessage.KeyboardButtonImageString = GetReplyKeyboardImageString();
        }

        private string GetReplyKeyboardImageString()
        {
            if (ViewModel != null)
            {
                var replyMarkup = ViewModel.ReplyMarkup as TLReplyKeyboardMarkup;
                if (replyMarkup != null)
                {
                    if (CommandsControl.Visibility == Visibility.Visible)
                    {
                        return "/Images/W10M/ic_keyboard_2x.png";
                    }

                    return "/Images/W10M/ic_botkeyboard_2x.png";
                }

                if (ViewModel.HasBots)
                {
                    return "/Images/W10M/ic_commands_2x.png";
                }
            }

            return null;
        }


        private void InputMessage_OnEmojiClick(object sender, System.EventArgs e)
        {
            OnSmileButtonClick();
        }

        private void InputMessage_OnSendClick(object sender, System.EventArgs e)
        {
            if (ViewModel.IsEditingEnabled)
            {
                ViewModel.SaveMessage();
            }
            else
            {
                ViewModel.Send();
            }
        }

        private void InputMessage_OnAttachClick(object sender, System.EventArgs e)
        {
            OpenEmptyDialogPlaceholder();
            CloseEmojiPlaceholder();
            CloseCommandsPlaceholder();
            SetCaptionVisibility(Visibility.Visible);
            if (_focusedElement == InputMessage)
            {
                ChooseAttachment.Focus();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(.35), () => ViewModel.Attach());
            }
            else
            {
                ChooseAttachment.Focus();
                ViewModel.Attach();
            }
        }

        private object _previousFocusedElement;

        private object _focusedElement;

        public DialogDetailsView()
        {
            ShellViewModel.WriteTimer("View start ctor");

            InitializeComponent();

            CaptionBorder.Background = ShellView.CaptionBrush;

            ShellViewModel.WriteTimer("View start ctor InitializeComponent");

            //Full HD
            OptimizeFullHD();

            ShellViewModel.WriteTimer("View start ctor OptimizeFullHD");

            _doneButton.Click += (sender, args) => ViewModel.SaveMessage();
            _cancelButton.Click += (sender, args) => ViewModel.CancelSaveMessage();

            _manageButton.Click += (sender, args) => ViewModel.IsSelectionEnabled = true;

            _pinToStartMenuItem.Click += (sender, args) => ViewModel.PinToStart();
            _shareMyContactInfoMenuItem.Click += (sender, args) => ViewModel.InvokeUserAction();
            _helpMenuItem.Click += (sender, args) => ViewModel.Help();
            _searchMenuItem.Click += (sender, args) => ViewModel.Search();
            _reportSpamMenuItem.Click += (sender, args) => ViewModel.ReportSpam();
            _callMenuItem.Click += (sender, args) => ViewModel.Call();
            _debugMenuItem.Click += (sender, args) =>
            {
                var aggregator = IoC.Get<ITelegramEventAggregator>();

                aggregator.Publish(new UpdateCompletedEventArgs());
            };

            ShellViewModel.WriteTimer("View start ctor set appbar");

            GotFocus += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine(args.OriginalSource);
                _previousFocusedElement = _focusedElement;
                _focusedElement = args.OriginalSource;
            };

            var viewportChangedEvents = Observable.FromEventPattern<ViewportChangedEventArgs>(
                keh => { MessagesList.ViewportChanged += keh; },
                keh => { MessagesList.ViewportChanged -= keh; });

            _viewportChangedSubscription = viewportChangedEvents
                .Sample(TimeSpan.FromSeconds(1.0))
                .ObserveOnDispatcher()
                .Subscribe(e => MessagesList_OnViewportChanged());

            Loaded += InitializeMTProtoService;

            Loaded += (sender, args) =>
            {
                if (_viewportChangedSubscription == null)
                {
                    _viewportChangedSubscription = viewportChangedEvents
                        .Sample(TimeSpan.FromSeconds(1.0))
                        .ObserveOnDispatcher()
                        .Subscribe(e => MessagesList_OnViewportChanged());
                }

                ShellViewModel.WriteTimer("View start Loaded");
                ViewModel.OnLoaded();
#if LOG_NAVIGATION
                TLUtils.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "DV Loaded", LogSeverity.Error);
#endif
                InputMessage.KeyboardButtonImageString = GetReplyKeyboardImageString();
                InputMessage.KeyboardButtonVisibility = GetReplyKeyboardButtonVisibility();
                InputMessage.MuteButtonVisibility = GetMuteButtonVisibility();

                SetRootFrameBinding();

                RunAnimation();

                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () =>
                {
                    if (ViewModel.StateService.FocusOnInputMessage)
                    {
                        ViewModel.StateService.FocusOnInputMessage = false;
                        if (!ViewModel.IsAppBarCommandVisible)
                        {
                            InputMessage.Focus();
                        }
                    }
                });

                if (ViewModel.ChooseAttachment != null)
                {
                    ViewModel.ChooseAttachment.PropertyChanged += OnChooseAttachmentPropertyChanged;
                }
                if (ViewModel.ImageViewer != null)
                {
                    ViewModel.ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
                }
                if (ViewModel.MultiImageEditor != null)
                {
                    ViewModel.MultiImageEditor.PropertyChanged += OnMultiImageEditorPropertyChanged;
                }
                if (ViewModel.ImageEditor != null)
                {
                    ViewModel.ImageEditor.PropertyChanged += OnImageEditorPropertyChanged;
                }
                if (ViewModel.AnimatedImageViewer != null)
                {
                    ViewModel.AnimatedImageViewer.PropertyChanged += OnAnimatedImageViewerPropertyChanged;
                }
                if (ViewModel.VideoEditor != null)
                {
                    ViewModel.VideoEditor.PropertyChanged += OnVideoEditorPropertyChanged;
                }
                if (ViewModel.LocationPicker != null)
                {
                    ViewModel.LocationPicker.PropertyChanged += OnLocationPickerPropertyChanged;
                }
                if (ViewModel.ContactPicker != null)
                {
                    ViewModel.ContactPicker.PropertyChanged += OnContactPickerPropertyChanged;
                }
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                ViewModel.ScrollToBottom += OnViewModelScrollToBottom;
                ViewModel.ScrollTo += OnViewModelScrollTo;

                BuildLocalizedAppBar();

                InputMessage.AudioRecorded += OnAudioRecorded;      //AudioRecorder.AudioRecorded += OnAudioRecorded;
                InputMessage.RecordStarted += OnRecordStarted;      //AudioRecorder.RecordStarted += OnRecordStarted;
                InputMessage.RecordingAudio += OnRecordingAudio;    //AudioRecorder.RecordingAudio += OnRecordingAudio;
                InputMessage.RecordCanceled += OnRecordCanceled;    //AudioRecorder.RecordCanceled += OnRecordCanceled;

                BrowserNavigationService.TelegramLinkAction += ViewModel.OnTelegramLinkAction;
                BrowserNavigationService.MentionNavigated += ViewModel.OnMentionNavigated;
                BrowserNavigationService.SearchHashtag += ViewModel.OnSearchHashtag;
                BrowserNavigationService.InvokeCommand += ViewModel.OnInvokeCommand;
                BrowserNavigationService.OpenGame += ViewModel.OnOpenGame;
                BrowserNavigationService.OpenPhone += ViewModel.OnOpenPhone;

                GifPlayerControl.MediaStateChanged += OnGifPlayerControlMediaStateChanged;

                ShellViewModel.WriteTimer("View end Loaded");
            };

            Unloaded += (sender, args) =>
            {
                if (_viewportChangedSubscription != null)
                {
                    _viewportChangedSubscription.Dispose();
                    _viewportChangedSubscription = null;
                }

#if LOG_NAVIGATION
                TLUtils.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "DV Unloaded", LogSeverity.Error);
#endif

                RemoveRootFrameBinding();

                if (ViewModel.ChooseAttachment != null)
                {
                    ViewModel.ChooseAttachment.PropertyChanged -= OnChooseAttachmentPropertyChanged;
                }
                if (ViewModel.ImageViewer != null)
                {
                    ViewModel.ImageViewer.PropertyChanged -= OnImageViewerPropertyChanged;
                }
                if (ViewModel.MultiImageEditor != null)
                {
                    ViewModel.MultiImageEditor.PropertyChanged -= OnMultiImageEditorPropertyChanged;
                }
                if (ViewModel.ImageEditor != null)
                {
                    ViewModel.ImageEditor.PropertyChanged -= OnImageEditorPropertyChanged;
                }
                if (ViewModel.AnimatedImageViewer != null)
                {
                    ViewModel.AnimatedImageViewer.PropertyChanged -= OnAnimatedImageViewerPropertyChanged;
                }
                if (ViewModel.VideoEditor != null)
                {
                    ViewModel.VideoEditor.PropertyChanged -= OnVideoEditorPropertyChanged;
                }
                if (ViewModel.LocationPicker != null)
                {
                    ViewModel.LocationPicker.PropertyChanged -= OnLocationPickerPropertyChanged;
                }
                if (ViewModel.ContactPicker != null)
                {
                    ViewModel.ContactPicker.PropertyChanged -= OnContactPickerPropertyChanged;
                }
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                ViewModel.ScrollToBottom -= OnViewModelScrollToBottom;
                ViewModel.ScrollTo -= OnViewModelScrollTo;

                InputMessage.AudioRecorded -= OnAudioRecorded;      //AudioRecorder.AudioRecorded -= OnAudioRecorded;
                InputMessage.RecordStarted -= OnRecordStarted;      //AudioRecorder.RecordStarted -= OnRecordStarted;
                InputMessage.RecordingAudio -= OnRecordingAudio;    //AudioRecorder.RecordingAudio -= OnRecordingAudio;
                InputMessage.RecordCanceled -= OnRecordCanceled;    //AudioRecorder.RecordCanceled -= OnRecordCanceled;

                BrowserNavigationService.TelegramLinkAction -= ViewModel.OnTelegramLinkAction;
                BrowserNavigationService.MentionNavigated -= ViewModel.OnMentionNavigated;
                BrowserNavigationService.SearchHashtag -= ViewModel.OnSearchHashtag;
                BrowserNavigationService.InvokeCommand -= ViewModel.OnInvokeCommand;
                BrowserNavigationService.OpenGame -= ViewModel.OnOpenGame;
                BrowserNavigationService.OpenPhone -= ViewModel.OnOpenPhone;

                GifPlayerControl.MediaStateChanged -= OnGifPlayerControlMediaStateChanged;
            };

            ShellViewModel.WriteTimer("View stop ctor");
        }

        private void OnGifPlayerControlMediaStateChanged(object sender, MediaStateChangedEventArgs args)
        {
            switch (args.State)
            {
                case GifPlayerControlState.Opening:
                    {
                        var player = Player.Content as PlayerView;
                        if (player == null)
                        {
                            player = new PlayerView();
                            player.Closed += (o, eventArgs) =>
                            {
                                var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                                if (frame != null)
                                {
                                    frame.HidePlayer();
                                }
                                player.Close(() =>
                                {
                                    GifPlayerControl.StopVideo();

                                    Player.Content = null;
                                    ResumeChatPlayers();
                                });
                            };
                            player.Paused += (o, eventArgs) =>
                            {
                                var gifPlayer = MessagePlayerControl.Player.Tag as GifPlayerControl;
                                if (gifPlayer != null)
                                {
                                    if (gifPlayer.Media == player.Message.Media)
                                    {
                                        gifPlayer.ToggleVideoPlay();
                                    }
                                }
                            };
                            player.Resumed += (o, eventArgs) =>
                            {
                                var gifPlayer = MessagePlayerControl.Player.Tag as GifPlayerControl;
                                if (gifPlayer != null)
                                {
                                    if (gifPlayer.Media == player.Message.Media)
                                    {
                                        gifPlayer.ToggleVideoPlay();
                                    }
                                }
                            };

                            Player.Content = player;
                        }

                        player.Message = ViewModel.Items.OfType<TLMessage>().FirstOrDefault(x => x.Media == args.Media);

                        player.Resume();
                        var frame1 = Application.Current.RootVisual as TelegramTransitionFrame;
                        if (frame1 != null)
                        {
                            frame1.HidePlayer();
                        }

                        break;
                    }
                case GifPlayerControlState.Opened:

                    break;
                case GifPlayerControlState.Failed:

                    break;
                case GifPlayerControlState.Paused:
                    {
                        var player = Player.Content as PlayerView;
                        if (player != null)
                        {
                            player.Pause();
                        }
                        break;
                    }
                case GifPlayerControlState.Resumed:
                    {
                        var player = Player.Content as PlayerView;
                        if (player != null)
                        {
                            player.Resume();
                        }
                        break;
                    }
                case GifPlayerControlState.Ended:
                    {
                        var player = Player.Content as PlayerView;
                        if (player != null)
                        {
                            var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                            if (frame != null)
                            {
                                frame.HidePlayer();
                            }

                            player.Close(() =>
                            {
                                Player.Content = null;
                                ResumeChatPlayers();
                            });
                        }
                        else
                        {
                            ResumeChatPlayers();
                        }
                        break;
                    }
            }
        }

        private void InitializeMTProtoService(object sender, RoutedEventArgs e)
        {
            Loaded -= InitializeMTProtoService;

            var mtProtoService = IoC.Get<IMTProtoService>();
            mtProtoService.StartInitialize();
        }

        private TLAllStickers _allStickers;

        private void OnSmileButtonClick()
        {
            if (_emojiKeyboard == null)
            {
                _emojiKeyboard = EmojiControl.GetInstance();

                var channel68 = ViewModel.With as TLChannel68;
                if (channel68 != null)
                {
                    _emojiKeyboard.SetGroupStickers(channel68, channel68.StickerSet);
                }
                else
                {
                    _emojiKeyboard.SetGroupStickers(null, null);
                }

                _allStickers = _allStickers ?? IoC.Get<IStateService>().GetAllStickers();
                var isStickersPanelVisible = true;
                //var allStickers = _allStickers as TLAllStickers43;
                //if (allStickers != null && allStickers.ShowStickersTab != null)
                //{
                //    isStickersPanelVisible = allStickers.ShowStickersTab.Value;
                //}
                _emojiKeyboard.BindTextBox(InputMessage.InnerTextBox, isStickersPanelVisible);
                _emojiKeyboard.StickerSelected += OnStickerSelected;
                _emojiKeyboard.StickerSetAdded += OnStickerSetAdded;
                _emojiKeyboard.SettingsButtonClick += OnSettingsButtonClick;
                _emojiKeyboard.OpenFullScreen += OnOpenFullScreen;
                _emojiKeyboard.CloseFullScreen += OnCloseFullScreen;
                InputMessage.EmojiPlaceholder.Content = _emojiKeyboard;

                _emojiKeyboard.IsOpen = true;

                //_emojiKeyboard.SetHeight(InputBox.KeyboardHeight > 0.0 ? InputBox.KeyboardHeight : _emojiKeyboard.Height);
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {
                //System.Diagnostics.Debug.WriteLine("Keyboard");

                if (InputMessage.EmojiPlaceholder.Opacity == 0.0)
                {
                    _smileButtonPressed = true;

                    InputMessage.OpenPlaceholder();

                    if (_emojiKeyboard != null)
                    {
                        _emojiKeyboard.OpenStickerSprites();
                    }
                }
                else
                {
                    InputMessage.InnerTextBox.Focus();
                }
            }
            else
            {
                InputMessage.OpenPlaceholder();

                if (_emojiKeyboard != null)
                {
                    _emojiKeyboard.OpenStickerSprites();
                }

                SetCaptionVisibility(Visibility.Collapsed);
                CloseEmptyDialogPlaceholder();
                CloseCommandsPlaceholder();
            }
        }

        private void OptimizeFullHD()
        {
            //var appBar = new ApplicationBar();
            //var appBarDefaultSize = appBar.DefaultSize;

            //AppBarCommandPlaceholder.Height = appBarDefaultSize;
        }

        private void OnViewModelScrollToBottom(object sender, System.EventArgs e)
        {
            if (ViewModel.Items.Count > 0)
            {
                MessagesList.ScrollToItem(ViewModel.Items[0]);

                HideScrollToBottomButton();
            }
        }

        private void OnViewModelScrollTo(object sender, ScrollToEventArgs e)
        {
            if (ViewModel.Items.Count > 0)
            {
                MessagesList.ScrollToItem(e.Message);
            }
        }

        private Binding _visibilityBinding;
        private double _previousAudioRecorderMinHeight;

        private static Binding SaveVisibilityBinding(FrameworkElement element)
        {
            var visibilityExpression = element.GetBindingExpression(VisibilityProperty);
            if (visibilityExpression != null)
            {
                return visibilityExpression.ParentBinding;
            }

            return null;
        }

        private static void RestoreVisibilityBinding(FrameworkElement element, Binding binding, Visibility defaultValue)
        {
            if (binding != null)
            {
                element.SetBinding(VisibilityProperty, binding);
            }
            else
            {
                element.Visibility = defaultValue;
            }
        }

        private void OnRecordCanceled(object sender, System.EventArgs e)
        {
            //AudioRecorder.MinHeight = _previousAudioRecorderMinHeight;
            //RestoreVisibilityBinding(InputMessage, _visibilityBinding, Visibility.Visible);
            //InputMessage.AdditionalButtons.Visibility = Visibility.Visible;

            ViewModel.AudioTypingManager.CancelTyping();
        }

        private void OnRecordStarted(object sender, System.EventArgs e)
        {
            //_visibilityBinding = SaveVisibilityBinding(InputMessage);
            //_previousAudioRecorderMinHeight = AudioRecorder.MinHeight;

            //AudioRecorder.MinHeight = InputMessage.ActualHeight;
            //InputMessage.Visibility = Visibility.Collapsed;
            //InputMessage.AdditionalButtons.Visibility = Visibility.Collapsed;
        }

        private void OnRecordingAudio(object sender, System.EventArgs e)
        {
            ViewModel.AudioTypingManager.SetTyping();
        }

        private void OnAudioRecorded(object sender, AudioEventArgs e)
        {
            //AudioRecorder.MinHeight = _previousAudioRecorderMinHeight;
            //RestoreVisibilityBinding(InputMessage, _visibilityBinding, Visibility.Visible);
            //InputMessage.AdditionalButtons.Visibility = Visibility.Visible;

            // чтобы быстро обновить Visibility InputMessage переносим все остальное в фон
            var viewModel = ViewModel;
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => viewModel.SendAudio(e));
        }

        public void OpenBitmapCache()
        {
            MessagesCache.Visibility = Visibility.Visible;
            MessagesList.Visibility = Visibility.Collapsed;
        }

        public void CloseBitmapCache()
        {
            MessagesCache.Visibility = Visibility.Collapsed;
            MessagesList.Visibility = Visibility.Visible;
        }

        private Storyboard _forwardInStoryboard;

        private void RunAnimation()
        {
            if (_isForwardInAnimation)
            {
                _isForwardInAnimation = false;

                if (ViewModel.With.Bitmap != null)
                {
                    MessagesList.Visibility = Visibility.Collapsed;
                }

                var storyboard = new Storyboard();

                if (ViewModel != null
                    && ViewModel.StateService.AnimateTitle)
                {
                    ViewModel.StateService.AnimateTitle = false;

                    var continuumElementX = new DoubleAnimationUsingKeyFrames();
                    continuumElementX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 130.0 });
                    continuumElementX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                    Storyboard.SetTarget(continuumElementX, Title);
                    Storyboard.SetTargetProperty(continuumElementX, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                    storyboard.Children.Add(continuumElementX);

                    var continuumElementY = new DoubleAnimationUsingKeyFrames();
                    continuumElementY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -40.0 });
                    continuumElementY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
                    Storyboard.SetTarget(continuumElementY, Title);
                    Storyboard.SetTargetProperty(continuumElementY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                    storyboard.Children.Add(continuumElementY);
                }

                var continuumLayoutRootY = new DoubleAnimationUsingKeyFrames();
                continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 150.0 });
                continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(continuumLayoutRootY, LayoutRoot);
                Storyboard.SetTargetProperty(continuumLayoutRootY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(continuumLayoutRootY);

                var continuumLayoutRootOpacity = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
                };
                Storyboard.SetTarget(continuumLayoutRootOpacity, LayoutRoot);
                Storyboard.SetTargetProperty(continuumLayoutRootOpacity, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(continuumLayoutRootOpacity);

                _forwardInStoryboard = storyboard;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    LayoutRoot.Opacity = 1.0;
                    storyboard.Completed += (o, e) =>
                    {
                        if (_backKeyPressed) return;

                        ViewModel.ForwardInAnimationComplete();

                        Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () =>
                        {
                            Items_OnManipulationStateChanged(this, System.EventArgs.Empty);
                        });
                    }
                    ;
                    storyboard.Begin();
                });
            }
            else if (_isBackwardOutAnimation)
            {
                _isBackwardOutAnimation = false;

                LayoutRoot.CacheMode = new BitmapCache();

                var storyboard = new Storyboard();

                var translateAnimation = new DoubleAnimationUsingKeyFrames();
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                Storyboard.SetTarget(translateAnimation, LayoutRoot);
                Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation);

                var opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
                Storyboard.SetTarget(opacityAnimation, LayoutRoot);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(opacityAnimation);

                storyboard.Begin();
            }
            else if (_isBackwardInAnimation)
            {
                _isBackwardInAnimation = false;

                var storyboard = TelegramTurnstileAnimations.GetAnimation(LayoutRoot, TurnstileTransitionMode.BackwardIn);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    LayoutRoot.Opacity = 1.0;
                    MessagesCache.Visibility = Visibility.Visible;
                    MessagesList.Visibility = Visibility.Collapsed;
                    storyboard.Completed += (o, e) =>
                    {
                        MessagesCache.Visibility = Visibility.Collapsed;
                        MessagesList.Visibility = Visibility.Visible;
                        ViewModel.BackwardInAnimationComplete();

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            StartGifPlayers();
                        });
                    };
                    storyboard.Begin();
                });
            }
        }

        private void StartGifPlayers()
        {
            Items_OnManipulationStateChanged(this, System.EventArgs.Empty);
            var inlineBotResultsView = InlineBotResults.FindChildOfType<InlineBotResultsView>();
            if (inlineBotResultsView != null)
            {
                inlineBotResultsView.StartActivePlayers();
            }
        }

#if DEBUG
        ~DialogDetailsView()
        {
            System.Diagnostics.Debug.WriteLine("~DialogDetailsView");
        }
#endif

        private bool _inputMessageDisabled;
        private bool _focusInputMessage;

        private void OpenPeerDetails_OnTap(object sender, GestureEventArgs args)
        {
            if (ViewModel.With is TLChatForbidden)
            {
                return;
            }

            if (CommandsControl.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseCommandsPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseEmojiPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            StopPlayersAndCreateBitmapCache(() => ViewModel.OpenPeerDetails());
        }

        public void StopPlayersAndCreateBitmapCache(Action callback = null)
        {
            GifPlayerControl.StopActivePlayers();
            GifPlayerControl.StopInlineBotActivePlayers();

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                CreateBitmapCache();
                MessagesCache.Visibility = Visibility.Visible;
                MessagesList.Visibility = Visibility.Collapsed;

                callback.SafeInvoke();
            });
        }

        public void CreateBitmapCache(Action callback = null)
        {
            try
            {
                if (MessagesList.Visibility == Visibility.Visible)
                {
                    var stopwatch = Stopwatch.StartNew();
                    var writeableBitmap = new WriteableBitmap(MessagesList, null);
                    ViewModel.With.SetBitmap(writeableBitmap);
                }
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("WritableBitmap exception " + ex);
            }

            callback.SafeInvoke();
        }

        private void MorePanel_OnTap(object sender, GestureEventArgs args)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var message = frameworkElement.DataContext as TLMessage;
            if (message == null) return;

            if (CommandsControl.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseCommandsPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseEmojiPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            // чтобы клавиатура успела опуститься
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                CreateBitmapCache();
                MessagesCache.Visibility = Visibility.Visible;
                MessagesList.Visibility = Visibility.Collapsed;

                ViewModel.OpenCropedMessage(message);
            });
        }

        private bool _isBackwardOutAnimation;
        private bool _isBackwardInAnimation;
        private bool _isForwardInAnimation;
        private bool _isForwardOutAnimation;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ShellViewModel.WriteTimer("OnNavigatedTo start");
            ViewModel.OnNavigatedTo();
#if LOG_NAVIGATION
            TLUtils.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + string.Format("DV OnNavigatedTo Mode={0} Uri={1}", e.NavigationMode, e.Uri), LogSeverity.Error);
#endif
            //MediaControl.Content = MessagePlayerControl.Player;

            if (e.NavigationMode == NavigationMode.New)
            {
                LayoutRoot.Opacity = 0.0;
                _isForwardInAnimation = true;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (!_fromExternalUri)
                {
                    LayoutRoot.Opacity = 0.0;
                    _isBackwardInAnimation = true;
                }
                else
                {
                    ViewModel.BackwardInAnimationComplete();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        StartGifPlayers();

                        if (LiveLocationProgress.Controls.Count > 0)
                        {
                            foreach (var control in LiveLocationProgress.Controls)
                            {
                                control.Update();
                            }
                        }
                    });
                }
                _fromExternalUri = false;
            }
            else if (e.NavigationMode == NavigationMode.Forward && e.Uri != ExternalUri)
            {
                _isForwardOutAnimation = true;
            }
            else if (e.NavigationMode == NavigationMode.Reset)
            {
                if (_fromExternalUri)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        StartGifPlayers();
                    });
                }
            }

            base.OnNavigatedTo(e);

            ShellViewModel.WriteTimer("OnNavigatedTo stop");
        }

        private static readonly Uri ExternalUri = new Uri(@"app://external/");

        private bool _fromExternalUri;
        private bool _suppressNavigation;

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {

            if (CancelNavigatingFrom(e)) return;

#if LOG_NAVIGATION
            TLUtils.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + string.Format("DV OnNavigatingFrom Mode={0} Uri={1}", e.NavigationMode, e.Uri), LogSeverity.Error);
#endif
            GifPlayerControl.StopActivePlayers();
            GifPlayerControl.StopInlineBotActivePlayers();
            if (_emojiKeyboard != null)
            {
                // Destroy EmojiControl
                _emojiKeyboard.IsOpen = false;
                _emojiKeyboard.UnbindTextBox();
                _emojiKeyboard.StickerSelected -= OnStickerSelected;
                _emojiKeyboard.StickerSetAdded -= OnStickerSetAdded;
                _emojiKeyboard.SettingsButtonClick -= OnSettingsButtonClick;
                InputMessage.EmojiPlaceholder.Content = null; // Remove from view
                OpenEmptyDialogPlaceholder();
                CloseEmojiPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
                _emojiKeyboard = null;
            }

            var multiImageEditorView = MultiImageEditor.Content as MultiImageEditorView;
            if (multiImageEditorView != null)
            {
                multiImageEditorView.ClosePreview();
            }

            if (CommandsControl.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseCommandsPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            if (e.Uri.OriginalString.EndsWith("VideoCaptureView.xaml")
                || e.Uri.OriginalString.EndsWith("MapView.xaml")
                || e.Uri.OriginalString.EndsWith("ShareContactView.xaml")
                || e.Uri.OriginalString.EndsWith("ContactView.xaml")
                || e.Uri.OriginalString.EndsWith("ChatView.xaml")
                || e.Uri.OriginalString.EndsWith("Chat2View.xaml"))
            {

            }

            var originalString = e.Uri.OriginalString;
            var index = originalString.IndexOf('?');
            if (index != -1)
            {
                originalString = originalString.Substring(0, index);
            }

            if (originalString.EndsWith("ChatView.xaml")
                || originalString.EndsWith("ProfilePhotoViewerView.xaml")
                || originalString.EndsWith("SearchShellView.xaml")
                || originalString.EndsWith("ChooseDialogView.xaml")
                || originalString.EndsWith("StickersView.xaml")
                || originalString.EndsWith("CancelConfirmResetView.xaml"))
            {
                CreateBitmapCache();
                MessagesList.Visibility = Visibility.Collapsed;
                MessagesCache.Visibility = Visibility.Visible;
            }
            else if (e.Uri.OriginalString.EndsWith("ContactView.xaml")
                || originalString.EndsWith("EditVideoView.xaml")
                || originalString.EndsWith("MapView.xaml")
                || originalString.EndsWith("WebView.xaml")
                || originalString.EndsWith("ShippingInfoView.xaml")
                || originalString.EndsWith("ShippingMethodView.xaml")
                || originalString.EndsWith("WebCardInfoView.xaml")
                || originalString.EndsWith("CheckoutView.xaml"))
            {
                if (ViewModel.With.Bitmap != null)
                {
                    MessagesList.Visibility = Visibility.Collapsed;
                    MessagesCache.Visibility = Visibility.Visible;
                }
            }

            if (originalString.EndsWith("DialogDetailsView.xaml"))
            {
                //new TelegramNavigationOutTransition { Forward = new SlideTransition { Mode = SlideTransitionMode.SlideDownFadeOut } });
            }

            base.OnNavigatingFrom(e);
        }

        private void OnSettingsButtonClick(object sender, System.EventArgs e)
        {
            if (CommandsControl.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseCommandsPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseEmojiPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            // чтобы клавиатура успела опуститься
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                CreateBitmapCache();
                MessagesCache.Visibility = Visibility.Visible;
                MessagesList.Visibility = Visibility.Collapsed;

                ViewModel.OpenStickerSettings();
            });
        }

        private void SetCaptionVisibility(Visibility visibility)
        {
            return;

            DialogPhoto.Visibility = visibility;
            Title.Visibility = visibility;
            ContextMenuIcon.Visibility = visibility;
            CaptionBorder.Height = visibility == Visibility.Visible ? 105.0 : 71.0;
        }

        private bool CancelNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.Uri.OriginalString.StartsWith("/Views/Dialogs/DialogDetailsView.xaml"))
            {
                if (ViewModel.With == ViewModel.StateService.With)
                {
                    var channel = ViewModel.With as TLChannel;
                    if (channel != null)
                    {
                        var postString = ViewModel.StateService.Post;
                        ViewModel.StateService.With = null;
                        ViewModel.StateService.Post = null;
                        ViewModel.StateService.RemoveBackEntries = false;

                        int post;
                        if (Int32.TryParse(postString, out post))
                        {
                            ViewModel.OpenMessage(null, new TLInt(post));
                        }

                        e.Cancel = true;
                        return true;
                    }

                    var user = ViewModel.With as TLUser;
                    if (user != null && user.IsBot)
                    {
                        var accessToken = ViewModel.StateService.AccessToken;
                        ViewModel.StateService.With = null;
                        ViewModel.StateService.AccessToken = null;
                        ViewModel.StateService.RemoveBackEntries = false;
                        ViewModel.StateService.Bot = null;

                        user.AccessToken = accessToken;
                        ViewModel.StartSwitchPMBotWithParam();

                        e.Cancel = true;
                        return true;
                    }
                }
            }
            else if (e.Uri.OriginalString.StartsWith("/Views/ShellView.xaml"))
            {
                if (e.Uri.OriginalString.Contains("from_id"))
                {
                    var user = ViewModel.With as TLUserBase;
                    if (user != null)
                    {
                        try
                        {
                            var uriParams = TelegramUriMapper.ParseQueryString(e.Uri.OriginalString);
                            var fromId = Convert.ToInt32(uriParams["from_id"]);
                            if (user.Index == fromId)
                            {
                                e.Cancel = true;
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                if (e.Uri.OriginalString.Contains("chat_id"))
                {
                    var chat = ViewModel.With as TLChatBase;
                    if (chat != null)
                    {
                        try
                        {
                            var uriParams = TelegramUriMapper.ParseQueryString(e.Uri.OriginalString);
                            var chatId = Convert.ToInt32(uriParams["chat_id"]);
                            if (chat.Index == chatId)
                            {
                                e.Cancel = true;
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                if (e.Uri.OriginalString.Contains("channel_id"))
                {
                    var chat = ViewModel.With as TLChatBase;
                    if (chat != null)
                    {
                        try
                        {
                            var uriParams = TelegramUriMapper.ParseQueryString(e.Uri.OriginalString);
                            var chatId = Convert.ToInt32(uriParams["channel_id"]);
                            if (chat.Index == chatId)
                            {
                                e.Cancel = true;
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            return false;
        }

        private void OnStickerSetAdded(object sender, StickerSetAddedEventArgs e)
        {
            if (e.Set == null) return;
        }

        private void OnStickerSelected(object sender, StickerSelectedEventArgs e)
        {
            if (e.Sticker == null) return;

            var document22 = e.Sticker.Document as TLDocument22;
            if (document22 == null) return;

            ViewModel.SendSticker(document22);

            //Execute.BeginOnUIThread(() =>
            {
                if (InputMessage.IsFullScreen)
                {
                    InputMessage.CloseFullScreen();
                }
            }
            //);
        }

        private double _messageListHeight;

        private void OnOpenFullScreen(object sender, System.EventArgs e)
        {
            InputMessage.OpenFullScreen(MessagesList.ActualHeight);
            //MessagesList.Visibility = Visibility.Collapsed;
        }

        private void OnCloseFullScreen(object sender, System.EventArgs e)
        {
            InputMessage.CloseFullScreen();
            //MessagesList.Visibility = Visibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.OnNavigatedFrom();
#if LOG_NAVIGATION
            TLUtils.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + string.Format("DV OnNavigatedFrom Mode={0} Uri={1}", e.NavigationMode, e.Uri), LogSeverity.Error);
#endif
            _fromExternalUri = e.Uri == ExternalUri;

            //if (_fromExternalUri)
            //{
            //    foreach (var control in LiveLocationProgress.Controls)
            //    {
            //        control.StopTimer();
            //    }
            //}

            MessagePlayerControl.Stop();
            GifPlayerControl.StopVideo();

            base.OnNavigatedFrom(e);
        }

        private void OnChooseAttachmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ChooseAttachment.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();
            }
        }

        private bool IsItemsHitTestVisible()
        {
            return (ViewModel.ChooseAttachment == null || !ViewModel.ChooseAttachment.IsOpen)
                    && (ViewModel.ImageViewer == null || !ViewModel.ImageViewer.IsOpen)
                    && (ViewModel.MultiImageEditor == null || !ViewModel.MultiImageEditor.IsOpen)
                    && (ViewModel.AnimatedImageViewer == null || !ViewModel.AnimatedImageViewer.IsOpen)
                    && (ViewModel.VideoEditor == null || !ViewModel.VideoEditor.IsOpen)
                    && (ViewModel.LocationPicker == null || !ViewModel.LocationPicker.IsOpen)
                    && (ViewModel.ContactPicker == null || !ViewModel.ContactPicker.IsOpen);
        }

        private IApplicationBar _prevApplicationBar;

        private void OnImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();
                //MessagesList.Visibility = MessagesList.IsHitTestVisible ? Visibility.Visible : Visibility.Collapsed;

                if (ViewModel.ImageViewer.IsOpen)
                {
                    var message70 = ViewModel.ImageViewer.CurrentItem as TLMessage70;
                    IsScreenCaptureEnabled = message70 == null || !message70.HasTTL();

                    _prevApplicationBar = ApplicationBar;
                    ApplicationBar = ((ImageViewerView)ImageViewer.Content).ApplicationBar;
                }
                else
                {
                    IsScreenCaptureEnabled = true;

                    if (_prevApplicationBar != null)
                    {
                        ApplicationBar = _prevApplicationBar;
                    }
                }
            }
        }

        private void OnMultiImageEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.MultiImageEditor.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();

                if (ViewModel.MultiImageEditor.IsOpen)
                {
                    _prevApplicationBar = ApplicationBar;
                    ApplicationBar = ((MultiImageEditorView)MultiImageEditor.Content).ApplicationBar;
                }
                else
                {
                    if (_prevApplicationBar != null)
                    {
                        ApplicationBar = _prevApplicationBar;
                    }
                }
            }
        }

        private void OnImageEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageEditor.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();

                if (ViewModel.ImageEditor.IsOpen)
                {
                    _prevApplicationBar = ApplicationBar;
                    ApplicationBar = ((ImageEditorView)ImageEditor.Content).ApplicationBar;
                }
                else
                {
                    if (_prevApplicationBar != null)
                    {
                        ApplicationBar = _prevApplicationBar;
                    }
                }
            }
        }

        private void OnAnimatedImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.AnimatedImageViewer.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();

                if (ViewModel.AnimatedImageViewer.IsOpen)
                {
                    _prevApplicationBar = ApplicationBar;
                    ApplicationBar = ((AnimatedImageViewerView)AnimatedImageViewer.Content).ApplicationBar;
                }
                else
                {
                    if (_prevApplicationBar != null)
                    {
                        ApplicationBar = _prevApplicationBar;
                    }
                }
            }
        }

        private void OnVideoEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.VideoEditor.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();
            }
        }

        private void OnLocationPickerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.LocationPicker.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();
            }
        }

        private void OnContactPickerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ContactPicker.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("VM changed " + e.PropertyName);

            //if (ViewModel._debugTimer != null) System.Diagnostics.Debug.WriteLine("start view property changed " + ViewModel._debugTimer.Elapsed);

            if (Property.NameEquals(e.PropertyName, () => ViewModel.Text))
            {

            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsAppBarCommandVisible))
            {
                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = false;//!ViewModel.IsAppBarCommandVisible && !ViewModel.IsChooseAttachmentOpen && !ViewModel.IsMassDeleteReportSpamOpen;
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.HasBots))
            {
                InputMessage.KeyboardButtonVisibility = GetReplyKeyboardButtonVisibility();
                InputMessage.KeyboardButtonImageString = GetReplyKeyboardImageString();
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.ReplyMarkup))
            {
                InputMessage.KeyboardButtonVisibility = GetReplyKeyboardButtonVisibility();
                InputMessage.KeyboardButtonImageString = GetReplyKeyboardImageString();

                var replyMarkup = ViewModel.ReplyMarkup as TLReplyKeyboardMarkup;
                if (replyMarkup != null)
                {
                    if (ViewModel.SuppressOpenCommandsKeyboard)
                    {
                        ViewModel.SuppressOpenCommandsKeyboard = false;
                        OpenEmptyDialogPlaceholder();
                        CloseCommandsPlaceholder();

                        return;
                    }

                    if (!replyMarkup.HasResponse)
                    {
                        if (!ViewModel.IsAppBarCommandVisible)
                        {
                            CloseEmojiPlaceholder();
                            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                            {
                                SelfView.Focus();
                                OpenCommandsPlaceholder();
                                CloseEmptyDialogPlaceholder();
                                SetCaptionVisibility(Visibility.Collapsed);
                            });
                        }
                    }
                    else
                    {
                        OpenEmptyDialogPlaceholder();
                        CloseCommandsPlaceholder();
                    }
                }

                var keyboardHide = ViewModel.ReplyMarkup as TLReplyKeyboardHide;
                if (keyboardHide != null)
                {
                    SetCaptionVisibility(Visibility.Visible);
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsGroupActionEnabled))
            {
                var count = DialogDetailsViewModel.UngroupEnumerator(ViewModel.Items).Count(x => x.IsSelected);
                var noneTTLCount = DialogDetailsViewModel.UngroupEnumerator(ViewModel.Items).Count(x => x.IsSelected && !x.HasTTL());
                if (_selectionCaption != null)
                {
                    _selectionCaption.Text = string.Format(count == 1 ? AppResources.ItemsSelectedSingular : AppResources.ItemsSelectedPlural, count).ToUpperInvariant();
                }

                InputMessage.IsGroupActionEnabled = new Tuple<bool, bool>(count > 0, count > 0 && noneTTLCount == count);
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.HoldScrollingPosition))
            {
                if (ViewModel.HoldScrollingPosition)
                {
                    MessagesList.HoldScrollingPosition();
                }
                else
                {
                    MessagesList.UnholdScrollingPosition();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsSelectionEnabled))
            {
                if (ViewModel.IsSelectionEnabled)
                {
                    SwitchToSelectionMode();
                }
                else
                {
                    SwitchToNonSelectionMode();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsEditingEnabled))
            {
                if (ViewModel.IsEditingEnabled)
                {
                    SwitchToEditingMode();
                }
                else
                {
                    SwitchToNormalMode();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.ChooseAttachment)
               && ViewModel.ChooseAttachment != null)
            {
                ViewModel.ChooseAttachment.PropertyChanged += OnChooseAttachmentPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer)
                && ViewModel.ImageViewer != null)
            {
                ViewModel.ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.MultiImageEditor)
                && ViewModel.MultiImageEditor != null)
            {
                ViewModel.MultiImageEditor.PropertyChanged += OnMultiImageEditorPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageEditor)
                && ViewModel.ImageEditor != null)
            {
                ViewModel.ImageEditor.PropertyChanged += OnImageEditorPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.AnimatedImageViewer)
                && ViewModel.AnimatedImageViewer != null)
            {
                ViewModel.AnimatedImageViewer.PropertyChanged += OnAnimatedImageViewerPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.VideoEditor)
                && ViewModel.VideoEditor != null)
            {
                ViewModel.VideoEditor.PropertyChanged += OnVideoEditorPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.LocationPicker)
                && ViewModel.LocationPicker != null)
            {
                ViewModel.LocationPicker.PropertyChanged += OnLocationPickerPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.ContactPicker)
            && ViewModel.ContactPicker != null)
            {
                ViewModel.ContactPicker.PropertyChanged += OnContactPickerPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.With))
            {
                ViewModel.ChangeUserAction();
                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = false;//!ViewModel.IsAppBarCommandVisible && !ViewModel.IsChooseAttachmentOpen && !ViewModel.IsMassDeleteReportSpamOpen;
                }
                ChangeShareInfo();
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.StartGifPlayers))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Items_OnManipulationStateChanged(this, System.EventArgs.Empty);
                });
            }
            //if (ViewModel._debugTimer != null) System.Diagnostics.Debug.WriteLine("end view property changed " + ViewModel._debugTimer.Elapsed);
        }

        private void ChangeShareInfo()
        {
            if (ApplicationBar == null) return;

            var userBase = ViewModel.With as TLUserBase;
            if (userBase != null && userBase.IsForeign)
            {
                ContextMenu.Items.Remove(_shareMyContactInfoMenuItem);
                ContextMenu.Items.Insert(0, _shareMyContactInfoMenuItem);
            }
            else
            {
                ContextMenu.Items.Remove(_shareMyContactInfoMenuItem);
            }
        }

        private void SwitchToEditingMode()
        {
            //InputMessage.IsEditing = true;

            //ApplicationBar.Buttons.Clear();

            //InputMessage.Padding = new Thickness(11.0, 2.0, 36.0, 2.0);

            Execute.BeginOnUIThread(() =>
            {
                InputMessage.FocusInput();
                InputMessage.SelectionStart = InputMessage.Text.Length;
                InputMessage.SelectionLength = 0;
                //ApplicationBar.Buttons.Add(_doneButton);
                //ApplicationBar.Buttons.Add(_cancelButton);
            });
        }

        private TextBlock _selectionCaption;

        private void SwitchToSelectionMode()
        {
            InputMessage.SwitchToSelectionMode();

            MessagesList.Focus();
            OpenEmptyDialogPlaceholder();
            CloseEmojiPlaceholder();
            CloseCommandsPlaceholder();
            SetCaptionVisibility(Visibility.Visible);

            if (_selectionCaption == null)
            {
                _selectionCaption = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(18.0, 52.0, 18.0, 21.0),
                    Foreground = new SolidColorBrush(Colors.White),
                    RenderTransform = new TranslateTransform(),
                    Style = (Style)Application.Current.Resources["ApplicationTitleStyle"]
                };

                ContentPanel.Children.Add(_selectionCaption);
            }

            CaptionGrid.RenderTransform = new TranslateTransform();

            var storyboard = new Storyboard();

            var transformAnimaion2 = new DoubleAnimation { From = 0.0, To = -72.0, Duration = TimeSpan.FromSeconds(0.2), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } };
            Storyboard.SetTarget(transformAnimaion2, CaptionGrid);
            Storyboard.SetTargetProperty(transformAnimaion2, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(transformAnimaion2);

            var opacityAnimaion2 = new DoubleAnimation { From = 1.0, To = 0.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion2, CaptionGrid);
            Storyboard.SetTargetProperty(opacityAnimaion2, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion2);

            var opacityAnimaion = new DoubleAnimation { From = 0.0, To = 1.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion, _selectionCaption);
            Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion);

            storyboard.Begin();
            storyboard.Completed += (o, e) =>
            {
                //((TranslateTransform) CaptionGrid.RenderTransform).Y = 0.0;
                //CaptionGrid.Opacity = 1.0;
                //DialogPhoto.Visibility = Visibility.Collapsed;
                //Caption.Visibility = Visibility.Collapsed;
                //ContextMenuIcon.Visibility = Visibility.Collapsed;
            };

            //DialogPhoto.Visibility = Visibility.Collapsed;
            //Caption.Visibility = Visibility.Collapsed;
            //ContextMenuIcon.Visibility = Visibility.Collapsed;
            var count = DialogDetailsViewModel.UngroupEnumerator(ViewModel.Items).Count(x => x.IsSelected);
            var noneTTLCount = ViewModel.Items.Count(x => x.IsSelected && !x.HasTTL());

            _selectionCaption.Text = string.Format(count == 1 ? AppResources.ItemsSelectedSingular : AppResources.ItemsSelectedPlural, count).ToUpperInvariant();
            _selectionCaption.Visibility = Visibility.Visible;

            ApplicationBar.Buttons.Clear();
            Execute.BeginOnUIThread(() =>
            {
                var channel = ViewModel.With as TLChannel;

                InputMessage.IsDeleteActionVisible = channel == null || channel.Creator;
                InputMessage.IsGroupActionEnabled = new Tuple<bool, bool>(count > 0, count > 0 && noneTTLCount == count);
                ApplicationBar.IsVisible = false;
            });
        }

        private void SwitchToNonSelectionMode()
        {
            InputMessage.SwitchToNormalMode();

            InputMessage.MuteButtonVisibility = GetMuteButtonVisibility();

            var storyboard = new Storyboard();

            var transformAnimaion2 = new DoubleAnimation { From = -72.0, To = 0.0, Duration = TimeSpan.FromSeconds(0.2), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } };
            Storyboard.SetTarget(transformAnimaion2, CaptionGrid);
            Storyboard.SetTargetProperty(transformAnimaion2, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(transformAnimaion2);

            var opacityAnimaion2 = new DoubleAnimation { From = 0.0, To = 1.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion2, CaptionGrid);
            Storyboard.SetTargetProperty(opacityAnimaion2, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion2);

            var opacityAnimaion = new DoubleAnimation { From = 1.0, To = 0.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion, _selectionCaption);
            Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion);

            storyboard.Begin();
            storyboard.Completed += (o, e) =>
            {
                _selectionCaption.Opacity = 1.0;
                _selectionCaption.Visibility = Visibility.Collapsed;
            };

            DialogPhoto.Visibility = Visibility.Visible;
            Caption.Visibility = Visibility.Visible;
            ContextMenuIcon.Visibility = Visibility.Visible;

            ApplicationBar.Buttons.Clear();
            ApplicationBar.IsVisible = false;
        }

        private void SwitchToNormalMode()
        {
            InputMessage.SwitchToNormalMode();

            InputMessage.MuteButtonVisibility = GetMuteButtonVisibility();

            DialogPhoto.Visibility = Visibility.Visible;
            Caption.Visibility = Visibility.Visible;
            ContextMenuIcon.Visibility = Visibility.Visible;
            if (_selectionCaption != null) _selectionCaption.Visibility = Visibility.Collapsed;

            //InputMessage.IsEditing = false;
            ApplicationBar.Buttons.Clear();
            ApplicationBar.IsVisible = false;
        }

        private bool _firstRun = true;

        private void BuildLocalizedAppBar()
        {
            if (!_firstRun) return;
            _firstRun = false;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.99;

            var broadcast = ViewModel.With as TLBroadcastChat;
            var channel = ViewModel.With as TLChannel;
            var chat = ViewModel.With as TLChat41;
            var user = ViewModel.With as TLUser;

            if (user != null && !user.IsBot && !user.IsSelf)
            {
                ContextMenu.Items.Add(_callMenuItem);
            }

            //ContextMenu.Items.Add(_debugMenuItem);

            ContextMenu.Items.Add(_manageButton);
            if (broadcast == null || channel != null)
            {
                var addReportSpam = true;
                if (chat != null && chat.Creator)
                {
                    addReportSpam = false;
                }

                if (user != null && user.IsSelf)
                {
                    addReportSpam = false;
                }

                if (channel != null && channel.Creator)
                {
                    addReportSpam = false;
                }
                if (addReportSpam)
                {
                    ContextMenu.Items.Add(_reportSpamMenuItem);
                }
            }
            if (broadcast == null || channel != null)
            {
                ContextMenu.Items.Add(_searchMenuItem);
            }

            if (user != null && user.IsBot)
            {
                ContextMenu.Items.Add(_helpMenuItem);
            }
            ContextMenu.Items.Add(_pinToStartMenuItem);

            var userBase = ViewModel.With as TLUserBase;
            if (userBase != null && userBase.IsForeign)
            {
                ContextMenu.Items.Add(_shareMyContactInfoMenuItem);
            }

            ApplicationBar.IsVisible = false; //!ViewModel.IsAppBarCommandVisible && !ViewModel.IsChooseAttachmentOpen && !ViewModel.IsMassDeleteReportSpamOpen;
        }

        private ShareMessagePicker _shareMessagePicker;

        private WeakEventListener<DialogDetailsView, object, NavigatingCancelEventArgs> _weakEventListener;

        public void OpenShareMessagePicker(string link, Action<PickDialogEventArgs> callback = null)
        {
            var isVisible = false;
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            PhoneApplicationPage page = null;
            if (frame != null)
            {
                page = frame.Content as PhoneApplicationPage;
                if (page != null)
                {
                    page.IsHitTestVisible = false;
                    var applicationBar = page.ApplicationBar;
                    if (applicationBar != null)
                    {
                        isVisible = applicationBar.IsVisible;
                        applicationBar.IsVisible = false;
                    }
                }

                var weakEventListener = new WeakEventListener<DialogDetailsView, object, NavigatingCancelEventArgs>(this, frame);
                frame.Navigating += weakEventListener.OnEvent;

                weakEventListener.OnEventAction = (view, o, args) =>
                {
                    view.Frame_Navigating(o, args);
                };
                weakEventListener.OnDetachAction = (listener, source) =>
                {
                    var f = source as PhoneApplicationFrame;
                    if (f != null)
                    {
                        f.Navigating -= listener.OnEvent;
                    }
                };

                _weakEventListener = weakEventListener;
            }

            if (page == null) return;

            var popup = new Popup();
            var sharePicker = new ShareMessagePicker
            {
                Width = page.ActualWidth,
                Height = page.ActualHeight,
                Link = link
            };
            _shareMessagePicker = sharePicker;
            page.SizeChanged += Page_SizeChanged;

            sharePicker.Close += (sender, args) =>
            {
                _shareMessagePicker = null;
                _weakEventListener.Detach();
                _weakEventListener = null;

                popup.IsOpen = false;
                popup.Child = null;

                frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    page = frame.Content as PhoneApplicationPage;
                    if (page != null)
                    {
                        page.SizeChanged -= Page_SizeChanged;
                        page.IsHitTestVisible = true;
                        var applicationBar = page.ApplicationBar;
                        if (applicationBar != null)
                        {
                            applicationBar.IsVisible = isVisible;
                        }
                    }
                }
            };
            _shareMessagePicker.Pick += (sender, args) =>
            {
                callback.SafeInvoke(args);
            };

            popup.Child = sharePicker;
            popup.IsOpen = true;
        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_shareMessagePicker != null)
            {
                _shareMessagePicker.ForceClose();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void DialogDetailsView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups().ToList();
            var popup = popups.FirstOrDefault();
            if (popup != null)
            {
                e.Cancel = true;

                var multiplePhotoPicker = popup.Child as OpenPhotoPicker;
                if (multiplePhotoPicker != null)
                {
                    multiplePhotoPicker.TryClose();
                }

                var shareMessagePicker = popup.Child as ShareMessagePicker;
                if (shareMessagePicker != null)
                {
                    shareMessagePicker.TryClose();
                }

                return;
            }

            if (_lastMessagePrompt != null
                && _lastMessagePrompt.IsOpen)
            {
                _lastMessagePrompt.Hide();
                e.Cancel = true;

                return;
            }

            if (ViewModel == null) return;

            if (ViewModel.SearchMessages != null
                && ViewModel.SearchMessages.IsOpen)
            {
                ViewModel.SearchMessages.Close();
                e.Cancel = true;

                return;
            }

            if (ViewModel.MultiImageEditor != null
                && ViewModel.MultiImageEditor.IsOpen)
            {
                ViewModel.MultiImageEditor.CloseEditor();
                e.Cancel = true;

                return;
            }

            if (ViewModel.LocationPicker != null
                && ViewModel.LocationPicker.IsOpen)
            {
                ViewModel.LocationPicker.CloseEditor();
                e.Cancel = true;

                return;
            }

            if (ViewModel.ContactPicker != null
                && ViewModel.ContactPicker.IsOpen)
            {
                ViewModel.ContactPicker.CloseEditor();
                e.Cancel = true;

                return;
            }

            if (ViewModel.VideoEditor != null
                && ViewModel.VideoEditor.IsOpen)
            {
                ViewModel.VideoEditor.CloseEditor();
                e.Cancel = true;

                return;
            }

            if (ViewModel.ImageEditor != null
                && ViewModel.ImageEditor.IsOpen)
            {
                ViewModel.ImageEditor.CloseEditor();
                e.Cancel = true;

                return;
            }

            if (ViewModel.ImageViewer != null
                && ViewModel.ImageViewer.IsOpen)
            {
                ViewModel.ImageViewer.CloseViewer();
                e.Cancel = true;

                return;
            }

            if (ViewModel.AnimatedImageViewer != null
                && ViewModel.AnimatedImageViewer.IsOpen)
            {
                ViewModel.AnimatedImageViewer.CloseViewer();
                e.Cancel = true;

                return;
            }

            if (_lastContextMenu != null && _lastContextMenu.IsOpen)
            {
                _lastContextMenu.IsOpen = false;
                e.Cancel = true;

                return;
            }

            if (_emojiKeyboard != null && _emojiKeyboard.IsPreviewMenuOpened)
            {
                _emojiKeyboard.ClosePreviewMenu();
                e.Cancel = true;

                return;
            }

            if (InputMessage.IsFullScreen)
            {
                InputMessage.CloseFullScreen();
                e.Cancel = true;

                return;
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible
                || CommandsControl.Visibility == Visibility.Visible)
            {
                SelfView.Focus();
                OpenEmptyDialogPlaceholder();
                CloseEmojiPlaceholder();
                CloseCommandsPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
                e.Cancel = true;

                return;
            }

            if (ViewModel.ChooseAttachment != null
                && ViewModel.ChooseAttachment.IsOpen)
            {
                ViewModel.ChooseAttachment.Close();
                ResumeChatPlayers();

                e.Cancel = true;

                return;
            }

            if (ViewModel.IsEditingEnabled)
            {
                ViewModel.StopEditMessage();
                ApplicationBar.IsVisible = false;//!ViewModel.IsAppBarCommandVisible;
                e.Cancel = true;

                return;
            }

            if (ViewModel.IsSelectionEnabled)
            {
                ViewModel.IsSelectionEnabled = false;
                ApplicationBar.IsVisible = false;//!ViewModel.IsAppBarCommandVisible;
                e.Cancel = true;

                return;
            }

            if (!NavigationService.BackStack.Any())
            {
                e.Cancel = true;
                ViewModel.NavigateToShellViewModel();

                return;
            }

            _isBackwardOutAnimation = true;

            CreateBitmapCache();
            MessagesCache.Visibility = Visibility.Visible;
            MessagesList.Visibility = Visibility.Collapsed;
            var result = ContentPanel.Children.Remove(MessagesList);

            ShellViewModel.WriteTimer("DialogDetailsView OnBackKeyPress");

            _backKeyPressed = true;

            if (_forwardInStoryboard != null && _forwardInStoryboard.GetCurrentState() != ClockState.Stopped)
            {
                _forwardInStoryboard.Stop();
            }

            RunAnimation();

            LiveLocationProgress.Controls.Clear();
            ViewModel.StopChannelScheduler();
            ViewModel.CancelDownloading();
        }

        private void UIElement_OnHold(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void MentionButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ReadNextMention();
            return;
            GifPlayerControl.PauseActivePlayers();
            GifPlayerControl.StopInlineBotActivePlayers();

            HideScrollToBottomButton();

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                if (ViewModel.Items.Count > 0)
                {
                    ViewModel.ProcessScroll();
                }

                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    Items_OnManipulationStateChanged(sender, System.EventArgs.Empty);
                });
            });
        }

        private void ScrollButton_OnClick(object sender, RoutedEventArgs e)
        {
            GifPlayerControl.PauseActivePlayers();
            GifPlayerControl.StopInlineBotActivePlayers();

            HideScrollToBottomButton();

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                if (ViewModel.Items.Count > 0)
                {
                    ViewModel.ProcessScroll();
                }

                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    Items_OnManipulationStateChanged(sender, System.EventArgs.Empty);
                });
            });
        }

        private void InputMessage_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ViewModel.StateService.SendByEnter)
                {
                    ViewModel.Send();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Space)
            {
                var text = InputMessage.Text.Trim();
                if (text.Length <= Constants.UsernameMaxLength && BrowserNavigationService.IsValidUsername(text))
                {
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.ResolveUsername(text));
                }
            }
            else if (e.Key == Key.T)
            {
                var text = (InputMessage.Text + "t").Trim();
                if (text.Length <= Constants.UsernameMaxLength && text.EndsWith("bot", StringComparison.OrdinalIgnoreCase) && BrowserNavigationService.IsValidUsername(text))
                {
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.ResolveUsername(text));
                }
            }
            else if (e.Key == Key.Back)
            {
                if (InputMessage.Text.Length == 0) return;

                var text = InputMessage.Text.Substring(0, InputMessage.Text.Length - 1).Trim();
                if (text.Length <= Constants.UsernameMaxLength && text.EndsWith("bot", StringComparison.OrdinalIgnoreCase) && BrowserNavigationService.IsValidUsername(text))
                {
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.ResolveUsername(text));
                }
            }
        }

        private ContextMenu _lastContextMenu;

        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            var menu = (ContextMenu)sender;
            var owner = (FrameworkElement)menu.Owner;

            if (owner.DataContext != menu.DataContext)
            {
                menu.DataContext = owner.DataContext;
            }

            if (_innerMessage != null && _innerMessage != menu.DataContext)
            {
                menu.DataContext = _innerMessage;
            }

            var serviceMessage = menu.DataContext as TLMessageService;
            if (serviceMessage != null)
            {
                menu.IsOpen = false;
                return;
            }

            _lastContextMenu = sender as ContextMenu;
        }

        private void InputMessage_OnGotFocus(object sender, RoutedEventArgs e)
        {
            SetCaptionVisibility(Visibility.Collapsed);
            CloseCommandsPlaceholder();
            CloseEmptyDialogPlaceholder();

            PauseChatPlayers();
        }

        private bool _smileButtonPressed;

        private void InputMessage_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!_smileButtonPressed)
            {
                if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
                {
                    CloseEmojiPlaceholder();
                }
            }
            _smileButtonPressed = false;

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Collapsed)
            {
                SetCaptionVisibility(Visibility.Visible);
            }

            OpenEmptyDialogPlaceholder();

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                ResumeChatPlayers();
            });
        }

        private Binding _emptyPlaceholderVisibilityBinding;

        private void OpenEmptyDialogPlaceholder()
        {
            var user = ViewModel.With as TLUser;
            if (user != null && user.IsSelf)
            {
                RestoreVisibilityBinding(EmptyDialogPlaceholder, _emptyPlaceholderVisibilityBinding, Visibility.Collapsed);
            }
        }

        private void CloseEmptyDialogPlaceholder()
        {
            var user = ViewModel.With as TLUser;
            if (user != null && user.IsSelf)
            {
                _emptyPlaceholderVisibilityBinding = SaveVisibilityBinding(EmptyDialogPlaceholder);
                EmptyDialogPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private bool _once = true;

        private void Items_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (ViewModel.SliceLoaded) return;
            if (ViewModel.IsSelectionEnabled) return;

            ViewModel.LoadNextSlice();
            ViewModel.LoadPreviousSlice();
        }

        private void InputMessage_OnTap(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_inputMessageDisabled)
            {
                _focusInputMessage = true;
            }
        }

        private void InputMessage_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InputMessage.KeyboardButtonVisibility = GetReplyKeyboardButtonVisibility();
            InputMessage.MuteButtonVisibility = GetMuteButtonVisibility();

            if (ViewModel.IsEditingEnabled) return;
        }

        private void StickerHints_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement == null) return;

            var stickerItem = frameworkElement.DataContext as TLStickerItem;
            if (stickerItem == null) return;

            var document22 = stickerItem.Document as TLDocument22;
            if (document22 == null) return;

            if (_emojiKeyboard != null)
            {
                _emojiKeyboard.UpdateRecentStickers(new StickerSelectedEventArgs { Sticker = stickerItem });
            }

            ViewModel.SendSticker(document22);
        }

        private ObservableCollection<TLStickerItem> _stickers;

        private Visibility GetMuteButtonVisibility()
        {
            if (InputMessage.Text.Length > 0)
            {
                return Visibility.Collapsed;
            }

            if (ViewModel == null) return Visibility.Collapsed;

            if (ViewModel.IsEditingEnabled) return Visibility.Collapsed;

            var channel = ViewModel.With as TLChannel44;

            return channel != null && channel.IsBroadcast && (channel.Creator || channel.IsEditor)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private Visibility GetReplyKeyboardButtonVisibility()
        {
            if (InputMessage.Text.Length > 0)
            {
                return Visibility.Collapsed;
            }

            if (ViewModel != null)
            {
                var replyMarkup = ViewModel.ReplyMarkup as TLReplyKeyboardMarkup;
                if (replyMarkup == null)
                {
                    if (!ViewModel.HasBots)
                    {
                        return Visibility.Collapsed;
                    }
                }
            }

            return Visibility.Visible;
        }

        private Visibility GetAudioRecorderVisibility()
        {
            if (FocusManager.GetFocusedElement() == InputMessage)
            {
                return Visibility.Collapsed;
            }

            if (InputMessage.Text.Length > 0)
            {
                return Visibility.Collapsed;
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }

            if (CommandsControl.Visibility == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }

            if (ViewModel != null)
            {
                if (ViewModel.IsEditingEnabled)
                {
                    return Visibility.Collapsed;
                }

                var chatForbidden = ViewModel.With as TLChatForbidden;
                var chat = ViewModel.With as TLChat;

                var isForbidden = chatForbidden != null || (chat != null && chat.Left.Value);
                if (isForbidden)
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        private void EmojiHint_OnTap(object sender, GestureEventArgs e)
        {
            var view = EmojiHints.Content as EmojiHintsView;
            if (view != null)
            {
                EmojiSuggestionParams parameters = null;
                var viewModel = view.DataContext as EmojiHintsViewModel;
                if (viewModel != null)
                {
                    parameters = viewModel.Parameters;
                    viewModel.SetParameters(null);
                }

                if (parameters != null)
                {
                    var frameworkElement = e.OriginalSource as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        var suggestion = frameworkElement.DataContext as EmojiSuggestion;
                        if (suggestion != null)
                        {
                            var text = InputMessage.Text;
                            if (string.Equals(InputMessage.Text, parameters.Text, StringComparison.Ordinal)
                                && InputMessage.SelectionStart == parameters.SelectionStart)
                            {
                                var firstPart = text.Substring(0, parameters.Index);
                                var middlePart = suggestion.Emoji;
                                var lastPart = text.Substring(parameters.Index + parameters.Length);
                                InputMessage.Text = firstPart + middlePart + lastPart;
                                InputMessage.SelectionStart = firstPart.Length + middlePart.Length;
                                InputMessage.FocusInput();

                                Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                                {
                                    var emoji = new EmojiDataItem(suggestion.Emoji, 1);
                                    emoji.Uri = EmojiDataItem.BuildUri(emoji.String);
                                    EmojiData.AddToRecents(emoji);
                                });
                            }
                        }
                    }
                }
            }
        }

        private void CommandHint_OnTap(object sender, GestureEventArgs e)
        {
            InputMessage.FocusInput();

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var botCommand = frameworkElement.DataContext as TLBotCommand;
                if (botCommand != null)
                {
                    var command = !ViewModel.IsSingleBot
                        ? string.Format("{0}@{1}", botCommand.Command, ((IUserName)botCommand.Bot).UserName)
                        : botCommand.Command.ToString();

                    InputMessage.Text = string.Empty;
                    Execute.BeginOnUIThread(() => ViewModel.Send(new TLString("/" + command)));
                }
            }
        }

        private void InlineBotResults_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var result = frameworkElement.DataContext as TLBotInlineResultBase;
                if (result != null)
                {
                    if (_previousFocusedElement == InputMessage)
                    {
                        InputMessage.FocusInput();
                    }

                    GifPlayerControl.StopInlineBotActivePlayers();
                    ViewModel.SendBotInlineResult(result);
                }
                else
                {
                    TelegramTransitionService.SetNavigationOutTransition(this, null);
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                    {
                        var textBlock = frameworkElement as TextBlock;
                        if (textBlock != null)
                        {
                            if (_previousFocusedElement == InputMessage)
                            {
                                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(.15), () =>
                                {
                                    ViewModel.Switch(ViewModel.InlineBotResults.SwitchPM);
                                });
                            }
                            else
                            {
                                ViewModel.Switch(ViewModel.InlineBotResults.SwitchPM);
                            }
                        }
                    });
                }
            }
        }

        private void UsernameHint_OnTap(object sender, GestureEventArgs e)
        {
            InputMessage.FocusInput();

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var user = frameworkElement.DataContext as IUserName;
                if (user != null)
                {
                    var user45 = user as TLUser45;
                    var userBase = user as TLUserBase;
                    if (user45 != null && user45.IsInlineBot)
                    {
                        ViewModel._currentInlineBot = user45;
                    }

                    var index = 0;
                    for (var i = InputMessage.Text.Length - 1; i >= 0; i--)
                    {
                        if (InputMessage.Text[i] == '@')
                        {
                            index = i;
                            break;
                        }
                    }

                    if (TLString.IsNullOrEmpty(user.UserName))
                    {
                        if (userBase != null)
                        {
                            ViewModel.AddMention(userBase);

                            InputMessage.Text = string.Format("{0}({1})", InputMessage.Text.Substring(0, index + 1), userBase.FullName);
                            InputMessage.SelectionStart = InputMessage.Text.Length - userBase.FullName.Length - 1;
                            InputMessage.SelectionLength = userBase.FullName.Length;
                        }
                    }
                    else
                    {
                        InputMessage.Text = string.Format("{0}{1} ", InputMessage.Text.Substring(0, index + 1), user.UserName);
                        InputMessage.SelectionStart = InputMessage.Text.Length;
                        InputMessage.SelectionLength = 0;
                    }

                    if (user45 != null && user45.IsInlineBot)
                    {
                        ViewModel.SetBotInlinePlaceholder();
                    }
                }
            }
        }

        private void HashtagHint_OnTap(object sender, GestureEventArgs e)
        {
            InputMessage.FocusInput();

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var hashtag = frameworkElement.DataContext as TLHashtagItem;
                if (hashtag != null)
                {
                    var index = 0;
                    for (var i = InputMessage.Text.Length - 1; i >= 0; i--)
                    {
                        if (InputMessage.Text[i] == '#')
                        {
                            index = i;
                            break;
                        }
                    }

                    InputMessage.Text = string.Format("{0}{1} ", InputMessage.Text.Substring(0, index + 1), hashtag.Hashtag);
                    InputMessage.SelectionStart = InputMessage.Text.Length;
                    InputMessage.SelectionLength = 0;
                }
            }
        }

        private void StickerHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueStickerHints();
        }

        private void UsernameHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueUsernameHints();
        }

        private void InlineBotResults_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueInlineBotResults();
        }

        private void CommandHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueCommandHints();
        }

        private void HashtagHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueHashtagHints();
        }

        private void EmojiHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            //ViewModel.ContinueEmojiHints();
        }

        private void Items_OnBegin(object sender, System.EventArgs e)
        {
            // repeat messages on first load with unread history
            if (!ViewModel.IsFirstSliceLoaded && ViewModel._previousScrollPosition != null)
            {
                ViewModel.LoadPreviousSlice("Items_OnBegin");
            }
            else
            {
                if (!ViewModel.SuppressHideScrollToBottom)
                {
                    ViewModel.MentionsCounter = 0;
                }
                HideScrollToBottomButton();
            }
        }

        public bool IsScrollToBottomButtonVisible
        {
            get { return ScrollButton.Visibility == Visibility.Visible; }
        }

        public void HideMentionButton()
        {
            if (MentionButton.Visibility == Visibility.Collapsed) return;
            if (ViewModel.SuppressHideScrollToBottom) return;

            var storyboard = new Storyboard();

            if (MentionButton.Visibility == Visibility.Visible)
            {
                var translateAnimation1 = new DoubleAnimationUsingKeyFrames();
                translateAnimation1.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
                translateAnimation1.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(translateAnimation1, MentionButton);
                Storyboard.SetTargetProperty(translateAnimation1, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation1);

                var opacityAnimation1 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
                };
                Storyboard.SetTarget(opacityAnimation1, MentionButton);
                Storyboard.SetTargetProperty(opacityAnimation1, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(opacityAnimation1);
            }

            storyboard.Completed += (sender, args) =>
            {
                MentionButton.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin();
        }

        public void HideScrollToBottomButton()
        {
            if (ScrollButton.Visibility == Visibility.Collapsed
                && MentionButton.Visibility == Visibility.Collapsed) return;
            if (ViewModel.SuppressHideScrollToBottom) return;

            var storyboard = new Storyboard();
            if (ScrollButton.Visibility == Visibility.Visible)
            {
                var translateAnimation2 = new DoubleAnimationUsingKeyFrames();
                translateAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
                translateAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(translateAnimation2, ScrollButton);
                Storyboard.SetTargetProperty(translateAnimation2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation2);

                var opacityAnimation2 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
                };
                Storyboard.SetTarget(opacityAnimation2, ScrollButton);
                Storyboard.SetTargetProperty(opacityAnimation2, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(opacityAnimation2);

                var visibilityAnimation2 = new ObjectAnimationUsingKeyFrames();
                visibilityAnimation2.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = Visibility.Collapsed });
                Storyboard.SetTarget(visibilityAnimation2, ScrollButton);
                Storyboard.SetTargetProperty(visibilityAnimation2, new PropertyPath("(UIElement.Visibility)"));
                storyboard.Children.Add(visibilityAnimation2);
            }

            if (ViewModel.MentionsCounter == 0 && MentionButton.Visibility == Visibility.Visible)
            {
                var translateAnimation1 = new DoubleAnimationUsingKeyFrames();
                translateAnimation1.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
                translateAnimation1.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(translateAnimation1, MentionButton);
                Storyboard.SetTargetProperty(translateAnimation1, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation1);

                var opacityAnimation1 = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
                };
                Storyboard.SetTarget(opacityAnimation1, MentionButton);
                Storyboard.SetTargetProperty(opacityAnimation1, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(opacityAnimation1);

                var visibilityAnimation = new ObjectAnimationUsingKeyFrames();
                visibilityAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = Visibility.Collapsed });
                Storyboard.SetTarget(visibilityAnimation, MentionButton);
                Storyboard.SetTargetProperty(visibilityAnimation, new PropertyPath("(UIElement.Visibility)"));
                storyboard.Children.Add(visibilityAnimation);
            }

            storyboard.Completed += (sender, args) =>
            {
                ViewModel.Counter = 0;
            };

            storyboard.Begin();
        }

        public void ShowMentionButton()
        {
            if (MentionButton.Visibility == Visibility.Visible) return;
            if (ViewModel.SuppressHideScrollToBottom) return;
            var dialog71 = ViewModel.CurrentDialog as TLDialog71;
            if (dialog71 != null && (dialog71.UnreadMentions == null || dialog71.UnreadMentionsCount.Value == 0)) return;

            MentionButton.Visibility = Visibility.Visible;
            MentionButton.Opacity = 0.0;

            var storyboard = new Storyboard();
            var translateAnimation = new DoubleAnimationUsingKeyFrames();
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 150.0 });
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
            Storyboard.SetTarget(translateAnimation, MentionButton);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateAnimation);

            var opacityAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
            };
            Storyboard.SetTarget(opacityAnimation, MentionButton);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            storyboard.Begin();
        }

        public void ShowScrollToBottomButton()
        {
            var storyboard = new Storyboard();
            if (ScrollButton.Visibility == Visibility.Collapsed)
            {
                ScrollButton.Visibility = Visibility.Visible;
                ScrollButton.Opacity = 0.0;

                var translateAnimation2 = new DoubleAnimationUsingKeyFrames();
                translateAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 150.0 });
                translateAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(translateAnimation2, ScrollButton);
                Storyboard.SetTargetProperty(translateAnimation2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation2);

                var opacityAnimation2 = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
                };
                Storyboard.SetTarget(opacityAnimation2, ScrollButton);
                Storyboard.SetTargetProperty(opacityAnimation2, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(opacityAnimation2);
            }

            if (MentionButton.Visibility == Visibility.Collapsed && ViewModel.MentionsCounter > 0)
            {
                MentionButton.Visibility = Visibility.Visible;
                MentionButton.Opacity = 0.0;

                var translateAnimation = new DoubleAnimationUsingKeyFrames();
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 150.0 });
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(translateAnimation, MentionButton);
                Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation);

                var opacityAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
                };
                Storyboard.SetTarget(opacityAnimation, MentionButton);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(opacityAnimation);
            }

            if (storyboard.Children.Count > 0)
            {
                storyboard.Begin();
            }
        }

        private static Storyboard _storyboard;
        private static UIElement _element;

        public static readonly DependencyProperty AnimatedVisibilityProperty =
            DependencyProperty.RegisterAttached("AnimatedVisibility", typeof(bool), typeof(DialogDetailsView),
                new PropertyMetadata(OnAnimagedVisibilityChanged));

        private StickerSpriteItem _stickerSpriteItem;

        private bool _backKeyPressed;

        private IDisposable _viewportChangedSubscription;

        private static void OnAnimagedVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element != null)
            {
                if (_storyboard != null)
                {

                    _storyboard.Stop();
                }
                if (_element != null)
                {
                    _element.Opacity = 0.0;
                    _element.Visibility = Visibility.Collapsed;
                }

                if ((bool)e.NewValue)
                {

                    element.Opacity = 1.0;
                    element.Visibility = Visibility.Visible;
                }
                else
                {
                    var storyboard = new Storyboard();
                    var continuumLayoutRootOpacity = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(1.0),
                    };
                    Storyboard.SetTarget(continuumLayoutRootOpacity, element);
                    Storyboard.SetTargetProperty(continuumLayoutRootOpacity, new PropertyPath("(UIElement.Opacity)"));
                    storyboard.Children.Add(continuumLayoutRootOpacity);

                    storyboard.Begin();
                    storyboard.Completed += (sender, args) =>
                    {
                        _storyboard = null;
                        _element = null;
                    };

                    _storyboard = storyboard;
                    _element = element;
                }
            }
        }


        public static bool GetAnimatedVisibility(UIElement element)
        {
            return (bool)element.GetValue(AnimatedVisibilityProperty);
        }

        public static void SetAnimatedVisibility(UIElement element, bool value)
        {
            element.SetValue(AnimatedVisibilityProperty, value);
        }

        private void HashtagHintsPanel_OnHold(object sender, GestureEventArgs e)
        {
            ViewModel.ClearHashtags();
        }

        private void CommandsControl_OnButtonClick(object sender, KeyboardButtonEventArgs e)
        {
            TLMessageBase message = null;
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                message = frameworkElement.DataContext as TLMessageBase;
            }

            ViewModel.Send(message, e.Button);

            if (e.ReplyMarkup != null
                && e.ReplyMarkup == ViewModel.ReplyMarkup
                && e.ReplyMarkup.IsSingleUse)
            {
                InputMessage.FocusInput();
            }
        }

        private void InputMessage_OnMuteButtonClick(object sender, System.EventArgs e)
        {
            var channel = ViewModel.With as TLChannel44;
            if (channel != null)
            {
                channel.Silent = !channel.Silent;
                ViewModel.NotifyOfPropertyChange(() => ViewModel.MuteButtonImageSource);

                if (InputMessageHintPlaceholder.Content == null)
                {
                    var control = new InputMessageHint();
                    control.Closed += OnInputMessageHintClosed;

                    InputMessageHintPlaceholder.Content = control;
                }

                var inputMessageHint = InputMessageHintPlaceholder.Content as InputMessageHint;
                if (inputMessageHint != null)
                {
                    inputMessageHint.Hint = channel.Silent ? AppResources.MuteChannelPostHint : AppResources.UnmuteChannelPostHint;
                }
            }
        }

        private void OnInputMessageHintClosed(object sender, System.EventArgs e)
        {
            var control = sender as InputMessageHint;
            if (control != null)
            {
                control.Closed -= OnInputMessageHintClosed;
            }

            InputMessageHintPlaceholder.Content = null;
        }

        private void InputMessage_OnKeyboardButtonClick(object sender, System.EventArgs e)
        {
            var replyKeyboard = ViewModel.ReplyMarkup as TLReplyKeyboardMarkup;

            if (ViewModel.HasBots && replyKeyboard == null)
            {
                InputMessage.Text = "/";
                InputMessage.SelectionStart = 1;
                InputMessage.FocusInput();

                return;
            }

            if (_emojiKeyboard != null
                && InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {

                if (replyKeyboard != null)
                {
                    //OpenEmptyDialogPlaceholder();
                    CloseEmojiPlaceholder();

                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() => OpenCommandsPlaceholder());

                    return;
                }
            }

            if (CommandsControl.Visibility == Visibility.Visible)
            {
                if (_emojiKeyboard != null
                    && InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
                {
                    OpenEmptyDialogPlaceholder();
                    CloseCommandsPlaceholder();
                    SetCaptionVisibility(Visibility.Visible);
                }
                else
                {
                    CloseCommandsPlaceholder();
                    InputMessage.FocusInput();
                    //Telegram.Api.Helpers.Execute.BeginOnUIThread(() => InputMessage.FocusInput());
                }
            }
            else
            {
                if (replyKeyboard != null)
                {
                    CloseEmptyDialogPlaceholder();
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() => OpenCommandsPlaceholder());
                    SetCaptionVisibility(Visibility.Collapsed);
                }
            }
        }

        private void AddToFavedStickers_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                var message = menuItem.DataContext as TLMessage;
                if (message != null && message.IsSticker())
                {
                    var mediaDocument = message.Media as TLMessageMediaDocument;
                    if (mediaDocument == null) return;

                    var document = mediaDocument.Document as TLDocument22;
                    if (document != null)
                    {
                        var inputStickerSet = document.StickerSet;
                        if (inputStickerSet != null && !(inputStickerSet is TLInputStickerSetEmpty))
                        {
                            var allStickers = ViewModel.Stickers as TLAllStickers43;
                            if (allStickers != null && allStickers.FavedStickers != null)
                            {
                                if (allStickers.Sets.Count >= Constants.MinSetsToAddFavedSticker || allStickers.FavedStickers.Documents.Count > 0)
                                {
                                    var exists = allStickers.FavedStickers.Documents.FirstOrDefault(x => x.Id.Value == document.Id.Value);
                                    if (exists != null)
                                    {
                                        menuItem.Visibility = Visibility.Visible;
                                        menuItem.Header = AppResources.DeleteFromFavorites;
                                    }
                                    else
                                    {
                                        menuItem.Visibility = Visibility.Visible;
                                        menuItem.Header = AppResources.AddToFavorites;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddToStickers_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                var message = menuItem.DataContext as TLMessage;
                if (message != null && message.IsSticker())
                {
                    var mediaDocument = message.Media as TLMessageMediaDocument;
                    if (mediaDocument == null) return;

                    var document = mediaDocument.Document as TLDocument22;
                    if (document != null)
                    {
                        var inputStickerSet = document.StickerSet;
                        if (inputStickerSet != null && !(inputStickerSet is TLInputStickerSetEmpty))
                        {
                            var allStickers = ViewModel.Stickers as TLAllStickers29;
                            if (allStickers != null)
                            {
                                var set =
                                    allStickers.Sets.FirstOrDefault(
                                        x => x.Id.Value.ToString() == inputStickerSet.Name.ToString());
                                if (set != null)
                                {
                                    menuItem.Visibility = Visibility.Collapsed;
                                    menuItem.Header = AppResources.ViewStickers;
                                }
                                else
                                {
                                    menuItem.Visibility = Visibility.Visible;
                                    menuItem.Header = AppResources.AddToStickers;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void StickerContextMenu_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                element.Visibility = Visibility.Visible;
            }
        }

        private void ServiceMessageContextMenu_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as ContextMenu;
            if (element != null)
            {
                if (ViewModel.IsAppBarCommandVisible)
                {
                    element.Visibility = Visibility.Collapsed;
                    element.IsOpen = false;

                    return;
                }
            }
        }

        private void DeleteMenuItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var message = element.DataContext as TLMessage40;
                if (message != null && message.Out.Value)
                {
                    element.Visibility = Visibility.Visible;
                    return;
                }

                var channel = ViewModel.With as TLChannel;
                if (channel != null
                    && (channel.Creator || channel.IsEditor))
                {
                    element.Visibility = Visibility.Visible;
                    return;
                }

                element.Visibility = Visibility.Collapsed;
            }
        }

        private void ReplyMenuItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = ViewModel.IsAppBarCommandVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ForwardMenuItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var broadcast = ViewModel.With as TLBroadcastChat;
            var channel = ViewModel.With as TLChannel;
            if (broadcast != null
                && channel == null)
            {
                element.Visibility = Visibility.Collapsed;
                return;
            }

            element.Visibility = Visibility.Visible;
        }

        private void MoreMenuItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = ViewModel.IsAppBarCommandVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MoreMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.IsSelectionEnabled = true;
            ApplicationBar.IsVisible = false;// true;
        }

        private void ReportMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Collapsed;
            var message = element.DataContext as TLMessageCommon;
            var user = ViewModel.With as TLUser;
            var channel = ViewModel.With as TLChannel;

            if (message != null
                && !message.Out.Value
                && (user != null && user.IsBot || channel != null))
            {
                element.Visibility = Visibility.Visible;
            }
        }

        private void ResendMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Collapsed;
            var message = element.DataContext as TLMessageCommon;
            if (message != null)
            {
                element.Visibility = message.Status == MessageStatus.Failed ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ReplyMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Visible;

            if (ViewModel.IsAppBarCommandVisible)
            {
                element.Visibility = Visibility.Collapsed;
                return;
            }

            var message = element.DataContext as TLMessage;
            if (message != null && message.IsExpired())
            {
                element.Visibility = Visibility.Collapsed;
                return;
            }

            var channel = ViewModel.With as TLChannel;
            if (channel != null && channel.MigratedFromChatId != null)
            {
                var messageCommon = element.DataContext as TLMessageCommon;
                if (messageCommon != null)
                {
                    if (messageCommon.ToId is TLPeerChat)
                    {
                        element.Visibility = messageCommon.ToId.Id.Value == channel.MigratedFromChatId.Value ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
        }

        private void PinMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as MenuItem;
            if (element == null) return;

            element.Visibility = Visibility.Collapsed;

            var channel = ViewModel.With as TLChannel;
            if (channel != null && channel.CanPinMessages)
            {
                var message = element.DataContext as TLMessageCommon;
                if (message != null)
                {
                    if (message.ToId is TLPeerChannel)
                    {
                        element.Visibility = Visibility.Visible;
                        element.Header = ViewModel.PinnedMessage != null && ViewModel.PinnedMessage.Message == message ? AppResources.UnpinMessage : AppResources.PinMessage;
                    }
                }
            }
        }
        private void EditMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Collapsed;

            var channel = ViewModel.With as TLChannel76;
            var user = ViewModel.With as TLUser66;
            var message = element.DataContext as TLMessage48;

            if (message != null
                && message.FwdHeader == null
                && message.ViaBotId == null
                && (message.Out.Value || (channel != null && channel.Creator && channel.IsEditor))
                && (message.Media is IMediaCaption || message.Media is TLMessageMediaWebPage || message.Media is TLMessageMediaEmpty))
            {
                if (message.IsExpired())
                {
                    return;
                }

                if (message.IsVoice())
                {
                    return;
                }

                if (message.IsSticker())
                {
                    return;
                }

                var time = TLUtils.DateToUniversalTimeTLInt(ViewModel.MTProtoService.ClientTicksDelta, DateTime.Now);
                var config = IoC.Get<ICacheService>().GetConfig() as TLConfig48;
                if (config != null && config.EditTimeLimit != null && (message.DateIndex + config.EditTimeLimit.Value) < time.Value)
                {
                    // channel admins with 'pin message' right can edit as long as possible
                    if (channel != null
                        && channel.AdminRights != null
                        && channel.AdminRights.PinMessages)
                    {

                    }
                    else if (user != null && user.IsSelf)
                    {

                    }
                    else
                    {
                        return;
                    }
                }

                element.Visibility = Visibility.Visible;
            }
        }

        private void DeleteMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Visible;

            var channel = ViewModel.With as TLChannel;
            if (channel != null)
            {
                var message = element.DataContext as TLMessageCommon;
                if (message != null)
                {
                    if (message.Index == 1 && message.ToId is TLPeerChannel)
                    {
                        element.Visibility = Visibility.Collapsed;
                    }

                    if (!channel.Creator && !channel.IsEditor)
                    {
                        if (message.FromId.Value != IoC.Get<IStateService>().CurrentUserId)
                        {
                            element.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void ForwardMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            element.Visibility = Visibility.Collapsed;

            if (ViewModel == null)
            {
                return;
            }

            var message = element.DataContext as TLMessage;
            if (message != null && message.HasTTL())
            {
                return;
            }

            element.Visibility = ViewModel.IsBroadcast && !ViewModel.IsChannel ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CopyMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Collapsed;

            var message = element.DataContext as TLMessage;
            if (message != null)
            {
                if (!TLString.IsNullOrEmpty(message.Message))
                {
                    element.Visibility = Visibility.Visible;
                    return;
                }
            }
        }

        private void EmojiMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility =
#if DEBUG
 Visibility.Visible;
#else
                Visibility.Collapsed;
#endif
        }

        private void SaveMedia_OnLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            frameworkElement.Visibility = Visibility.Collapsed;

            var message = frameworkElement.DataContext as TLMessage;
            if (message == null) return;
            if (message.HasTTL() || message.IsSticker() || message.IsRoundVideo() || message.IsGif()) return;

#if WP8
            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                var file = mediaDocument.File;

                if (file == null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document != null)
                    {
                        var localFileName = document.GetFileName() ?? string.Empty;
                        var globalFileName = mediaDocument.IsoFileName ?? string.Empty;
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
                        if (store.FileExists(localFileName)
                            || store.FileExists(globalFileName)
#if WP81
 || File.Exists(globalFileName)
#endif
)
                        {
                            frameworkElement.Visibility = Visibility.Visible;
                            return;
                        }
                    }
                }

                frameworkElement.Visibility = file != null ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            var mediaVideo = message.Media as TLMessageMediaVideo;
            if (mediaVideo != null)
            {
                var file = mediaVideo.File;

                if (file == null)
                {
                    var video = mediaVideo.Video as TLVideo;
                    if (video != null)
                    {
                        var localFileName = video.GetFileName() ?? string.Empty;
                        var globalFileName = mediaVideo.IsoFileName ?? string.Empty;
                        var store = IsolatedStorageFile.GetUserStoreForApplication();
                        if (store.FileExists(localFileName)
                            || store.FileExists(globalFileName)
#if WP81
 || File.Exists(globalFileName)
#endif
)
                        {
                            frameworkElement.Visibility = Visibility.Visible;
                            return;
                        }
                    }
                }

                frameworkElement.Visibility = file != null ? Visibility.Visible : Visibility.Collapsed;
                return;
            }
#endif
        }

        private void MessagesList_OnViewportChanged()
        {
            //return;
            System.Diagnostics.Debug.WriteLine("ViewportChanged date={0}", DateTime.Now.TimeOfDay);
            if (Player.Content != null)
            {
                var gifPlayer = MessagePlayerControl.Player.Tag as GifPlayerControl;
                if (gifPlayer != null && gifPlayer.Mode == GifPlayerMode.RoundVideo)
                {
                    var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                    if (frame != null)
                    {
                        var itemsInView = MessagesList.GetItemsInView();
                        foreach (var item in itemsInView)
                        {
                            var message = item.DataContext as TLMessage;
                            if (message != null && message.Media == gifPlayer.Media)
                            {
                                frame.HidePlayer();
                                return;
                            }
                        }

                        var videoBrush = new VideoBrush();
                        videoBrush.SetSource(MessagePlayerControl.Player);

                        frame.ShowPlayer(videoBrush);
                    }
                }
            }
            var dialog = ViewModel.CurrentDialog as TLDialog71;
            if (dialog != null && dialog.UnreadMentions != null && dialog.UnreadMentions.Count > 0)
            {
                var itemsInView = MessagesList.GetItemsInView();
                var maxId = 0;
                foreach (var item in itemsInView)
                {
                    var message = item.DataContext as TLMessage;
                    if (message != null)
                    {
                        maxId = Math.Max(maxId, message.Index);
                    }
                }

                if (maxId > 0)
                {
                    ViewModel.ReadMentions(maxId);
                }
            }
        }

        private void Items_OnItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
            var message = e.Container.Content as TLMessage;
            if (message != null)
            {

                TLFileLocation profilePhotoFileLocation = null;

                var photoMedia = message.Media as TLMessageMediaPhoto;
                if (photoMedia != null)
                {
                    var photo = photoMedia.Photo as TLPhoto;
                    if (photo != null)
                    {
                        var viewModel = ViewModel;
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            viewModel.DownloadFileManager.CancelDownloadFile(photo);
                            if (profilePhotoFileLocation != null) viewModel.DownloadFileManager.CancelDownloadFile(profilePhotoFileLocation);
                        });
                    }
                }
            }
        }

        public void StartChatPlayer(StartGifPlayerEventArgs e)
        {
            if (MessagesList.ManipulationState != ManipulationState.Idle) return;
            if (ViewModel.ChooseAttachment != null && ViewModel.ChooseAttachment.IsOpen) return;
            if (MessagePlayerControl.Player != null
                && MessagePlayerControl.Player.CurrentState == MediaElementState.Playing
                && MessagePlayerControl.Player.Tag is GifPlayerControl) return;

            var settings = ViewModel.StateService.GetChatSettings();
            var newItems = MessagesList.GetItemsInView();

            foreach (var item in newItems)
            {
                var message = item.DataContext as TLMessage;
                if (message != null && message == e.Message && (message.IsGif() || message.IsRoundVideo()))
                {
                    var media = message.Media;
                    if ((media != null && media.AutoPlayGif == true) || (settings != null && settings.AutoPlayGif && media != null && media.AutoPlayGif != false))
                    {
                        var gifPlayer = item.FindChildOfType<GifPlayerControl>();
                        if (gifPlayer != null)
                        {
                            gifPlayer.Start();
                        }
                    }
                    return;
                }
            }
        }

        public void ResumeChatPlayers()
        {
            if (MessagesList.ManipulationState != ManipulationState.Idle) return;
            if (ViewModel.ChooseAttachment != null && ViewModel.ChooseAttachment.IsOpen) return;
            if (MessagePlayerControl.Player != null
                && MessagePlayerControl.Player.CurrentState == MediaElementState.Playing
                && MessagePlayerControl.Player.Tag is GifPlayerControl) return;
            if (FocusManager.GetFocusedElement() == InputMessage.Input) return;
            if (ViewModel.InlineBotResults != null && ViewModel.InlineBotResults.Results.Count > 0) return;

            var settings = ViewModel.StateService.GetChatSettings();
            var newItems = MessagesList.GetItemsInView();

            var activePlayers = new List<GifPlayerControl>();
            foreach (var item in newItems)
            {
                var message = item.DataContext as TLMessage;
                if (message != null && (message.IsGif() || message.IsRoundVideo()))
                {
                    var media = message.Media;
                    if ((media != null && media.AutoPlayGif == true) || (settings != null && settings.AutoPlayGif && media != null && media.AutoPlayGif != false))
                    {
                        var gifPlayer = item.FindChildOfType<GifPlayerControl>();
                        if (gifPlayer != null)
                        {
                            activePlayers.Add(gifPlayer);
                        }
                    }
                }
            }

            GifPlayerControl.ResumeActivePlayers(activePlayers);
        }

        public void PauseChatPlayers()
        {
            GifPlayerControl.PauseActivePlayers();
        }

        private void Items_OnManipulationStateChanged(object sender, System.EventArgs e)
        {
            GifPlayerControl.ManipulationState = MessagesList.ManipulationState;
            if (MessagesList.ManipulationState == ManipulationState.Idle)
            {
                ResumeChatPlayers();
            }
            else
            {
                PauseChatPlayers();
            }
#if DEBUG
            var count = GifPlayerControl.ActivePlayers.Count;
            Debug.Text = string.Format("{0} {1}", count, GifPlayerControl.ActivePlayers.Count);
#endif
        }

        private void ViaBot_Tap(object sender, GestureEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var message = frameworkElement.DataContext as TLMessage45;
            if (message == null) return;

            SetInlineBot(message.ViaBot);
        }

        public void SetInlineBot(TLUserBase bot)
        {
            var user = bot as TLUser;
            if (user == null || !user.IsInlineBot) return;

            InputMessage.Text = string.Format("@{0} ", user.UserName);
            InputMessage.FocusInput();
            InputMessage.SelectionStart = InputMessage.Text.Length;
            InputMessage.SelectionLength = 0;

            ViewModel.CurrentInlineBot = user;
            ViewModel.GetInlineBotResults(string.Empty);
        }

        private void ForwardMessage_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var message = element.DataContext as TLMessageBase;
            // find common grouped message to resend
            var message73 = message as TLMessage73;
            if (message73 != null
                && message73.GroupedId != null)
            {
                var mediaGroup = message73.Media as TLMessageMediaGroup;
                if (mediaGroup == null)
                {
                    var groupedMessage = ViewModel.FindGroupedMessage(message73) as TLMessage45;
                    if (groupedMessage != null)
                    {
                        message = groupedMessage;
                    }
                }
            }

            if (message == null) return;
            var selectedItems = new List<TLMessageBase> { message };
            if (selectedItems.Count == 0) return;

            ViewModel.FastForwardMessages(selectedItems);
        }

        private void ShareButton_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var message = element.DataContext as TLMessageBase;
            if (message == null) return;

            var message73 = message as TLMessage73;
            if (message73 != null)
            {
                var fwdHeader = message73.FwdHeader as TLMessageFwdHeader73;
                if (fwdHeader != null && fwdHeader.SavedFromPeer != null && fwdHeader.SavedFromMsgId != null)
                {
                    TLObject with = null;
                    var peerUser = fwdHeader.SavedFromPeer as TLPeerUser;
                    var peerChat = fwdHeader.SavedFromPeer as TLPeerChat;
                    var peerChannel = fwdHeader.SavedFromPeer as TLPeerChannel;
                    if (peerUser != null)
                    {
                        with = IoC.Get<ICacheService>().GetUser(peerUser.Id);
                    }
                    else if (peerChat != null)
                    {
                        with = IoC.Get<ICacheService>().GetChat(peerChat.Id);
                    }
                    else if (peerChannel != null)
                    {
                        with = IoC.Get<ICacheService>().GetChat(peerChannel.Id);
                    }

                    if (with == null)
                    {
                        return;
                    }

                    Execute.BeginOnUIThread(() =>
                    {
                        IoC.Get<IStateService>().With = with;
                        IoC.Get<IStateService>().MessageId = fwdHeader.SavedFromMsgId;
                        IoC.Get<IStateService>().RemoveBackEntries = true;
                        IoC.Get<INavigationService>().Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                    });

                    return;
                }
            }

            var selectedItems = new List<TLMessageBase> { message };
            if (selectedItems.Count == 0) return;

            ViewModel.FastForwardMessages(selectedItems);
        }

        private void EmojiMessage_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var message = element.DataContext as TLMessage48;
            if (message == null) return;

            var fwdHeader = message.FwdHeader;
            MessageBox.Show(
                string.Format("TLMessageFwdHeader flags={0} from_id={1} date={2} channel_id={3} channel_post={4}",
                    fwdHeader.Flags, fwdHeader.FromId, fwdHeader.Date, fwdHeader.ChannelId, fwdHeader.ChannelPost));
            return;

            var resultString = string.Empty;
            var text = message.Message.ToString();
            var textEnumerator = StringInfo.GetTextElementEnumerator(text);

            var symbols = new List<string>();
            while (textEnumerator.MoveNext())
            {
                var text2 = textEnumerator.GetTextElement();
                var bytes2 = Encoding.BigEndianUnicode.GetBytes(text2);
                var bytesStr2 = BrowserNavigationService.ConvertToHexString(bytes2);

                symbols.Add(bytesStr2);
            }

            var dict = new Dictionary<string, string>();
            var builder = new StringBuilder();
            for (var i = 0; i < symbols.Count; )
            {
                builder.AppendLine(string.Format("_dict[\"{0}\"] = \"{0}\";", symbols[i] + symbols[i + 1]));
                dict[symbols[i]] = symbols[i];
                i += 2;

                if (i % 20 == 0)
                {
                    builder.AppendLine();
                }
            }
            builder.AppendLine();
            foreach (var item in dict)
            {
                builder.AppendLine(item.Value);
            }

            MessageBox.Show(builder.ToString());
            Clipboard.SetText(builder.ToString());
        }

        private void ShowUserProfile_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var message = element.DataContext as TLMessage;
            if (message == null) return;

            Execute.BeginOnUIThread(() =>
            {
                CreateBitmapCache();
                MessagesList.Visibility = Visibility.Collapsed;
                MessagesCache.Visibility = Visibility.Visible;
                ViewModel.ShowUserProfile(message);
            });
        }

        private void CopyLink_OnLoaded(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu == null) return;

            var message = contextMenu.DataContext as TLMessageBase;
            if (message == null)
            {
                contextMenu.IsOpen = false;
                return;
            }
            if (message.Index == 0)
            {
                contextMenu.IsOpen = false;
                return;
            }

            var channel = ViewModel.With as TLChannel;
            if (channel == null)
            {
                contextMenu.IsOpen = false;
                return;
            }

            if (TLString.IsNullOrEmpty(channel.UserName))
            {
                contextMenu.IsOpen = false;
                return;
            }
        }

        private void CopyLink_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var message = element.DataContext as TLMessageBase;
            if (message == null) return;
            if (message.Index == 0) return;

            var selectedItems = new List<TLMessageBase> { message };
            if (selectedItems.Count == 0) return;

            var channel = ViewModel.With as TLChannel;
            if (channel == null) return;
            if (TLString.IsNullOrEmpty(channel.UserName)) return;

            Clipboard.SetText(String.Format(Constants.UsernameLinkPlaceholder + "/{1}", channel.UserName, message.Id));
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            Telegram.Api.Helpers.Execute.ShowDebugMessage("ActualHeight=" + element.ActualWidth);
        }

        private void InputMessage_OnDeleteReplyButtonClick(object sender, System.EventArgs eventArgs)
        {
            var container = ViewModel.Reply as TLMessagesContainter;
            if (container != null)
            {
                var editMessage = container.EditMessage;
                if (editMessage != null)
                {
                    if (container.WebPageMedia != null)
                    {
                        ViewModel.DeleteReply();

                        return;
                    }

                    ViewModel.CancelSaveMessage();

                    return;
                }
            }

            ViewModel.DeleteReply();
        }

        private void KeepFocusedInputMessage()
        {
            if (_focusedElement == InputMessage)
            {
                InputMessage.FocusInput();
            }
        }

        private void InputMessage_OnOpenReplyButtonClick(object sender, System.EventArgs eventArgs)
        {
            ViewModel.OpenEditMessage();
        }

        private void MessagesList_OnShowScrollButton(object sender, System.EventArgs e)
        {
            ShowScrollToBottomButton();
        }

        private void MessagesList_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Telegram.Api.Helpers.Execute.ShowDebugMessage("MessagesList.OnSizeChanged");
        }

        private void ContextMenu_OnHold(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void ContextMenu_OnTap(object sender, GestureEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void InputMessage_OnAppBarCommandClick(object sender, System.EventArgs e)
        {
            ViewModel.AppBarCommand();
        }

        private void InputMessage_OnCancelSelectionButtonClick(object sender, System.EventArgs e)
        {
            ViewModel.IsSelectionEnabled = false;
        }

        private void InputMessage_OnForwardButtonClick(object sender, System.EventArgs e)
        {
            var selectedItems = DialogDetailsViewModel.UngroupEnumerator(ViewModel.Items).Where(x => x.Index > 0 && x.IsSelected).ToList();
            if (selectedItems.Count == 0) return;

            ViewModel.IsSelectionEnabled = false;

            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.55), () =>  // waiting to complete animation
            {
                CreateBitmapCache();
                MessagesList.Visibility = Visibility.Collapsed;
                MessagesCache.Visibility = Visibility.Visible;
                ViewModel.ForwardMessages(selectedItems);
            });
        }

        private void SelectMessages_OnClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var message = frameworkElement.DataContext as TLMessageBase;
                if (message != null && message.SelectionVisibility == Visibility.Visible)
                {
                    ViewModel.ChangeSelection(message);
                }
            }

            ViewModel.IsSelectionEnabled = true;
        }

        private void InputMessage_OnDeleteButtonClick(object sender, System.EventArgs e)
        {
            ViewModel.DeleteMessages();
        }

        private void InputMessage_OnShowEmojiHints(object sender, EmojiHintsEventArgs e)
        {
            var emojiHintsView = EmojiHints.Content as EmojiHintsView;
            if (emojiHintsView == null)
            {
                emojiHintsView = new EmojiHintsView { DataContext = new EmojiHintsViewModel() };
                EmojiHints.Content = emojiHintsView;
            }

            var emojiHintsViewModel = emojiHintsView.DataContext as EmojiHintsViewModel;
            if (emojiHintsViewModel != null)
            {
                emojiHintsViewModel.SetParameters(e.Parameters);
            }
        }

        private void InputMessage_OnHideEmojiHints(object sender, System.EventArgs e)
        {
            var emojiHintsView = EmojiHints.Content as EmojiHintsView;
            if (emojiHintsView != null)
            {
                var emojiHintsViewModel = emojiHintsView.DataContext as EmojiHintsViewModel;
                if (emojiHintsViewModel != null)
                {
                    emojiHintsViewModel.SetParameters(null);
                }
            }
        }

        private void SelectMessages_OnLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.Visibility = Visibility.Visible;

                var message = frameworkElement.DataContext as TLMessageBase;
                if (message != null && message.IsExpired())
                {
                    frameworkElement.Visibility = Visibility.Collapsed;
                    return;
                }
            }
        }

        public void ScrollTo(TLObject obj)
        {
            MessagesList.ScrollToItem(obj);
        }

        private void MentionButton_OnHold(object sender, GestureEventArgs e)
        {
            ViewModel.ClearUnreadMentions();
        }

        private void SelectionBorder_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var groupedMessage = element.DataContext as TLMessage73;
                if (groupedMessage != null)
                {
                    var selectionChanged = false;

                    var messageMediaGroup = groupedMessage.Media as TLMessageMediaGroup;
                    if (messageMediaGroup != null)
                    {
                        var message = GetInnerMessage(element, e);
                        if (message != null)
                        {
                            ViewModel.ChangeSelection(message);

                            selectionChanged = true;
                            if (messageMediaGroup.Group.All(x => x.IsSelected) && !groupedMessage.IsSelected
                                || messageMediaGroup.Group.All(x => !x.IsSelected) && groupedMessage.IsSelected)
                            {
                                selectionChanged = false;
                            }
                        }
                    }

                    if (!selectionChanged)
                    {
                        ViewModel.ChangeSelection(groupedMessage);
                    }
                }
                else
                {
                    ViewModel.ChangeSelection(element.DataContext as TLMessageBase);
                }
            }
        }

        private void MessageControl_OnTapMedia(object sender, GestureEventArgs e)
        {
            var messageControl = sender as MessageControl;
            if (messageControl != null)
            {
                var message = GetInnerMessage(messageControl, e);
                if (message != null)
                {
                    ViewModel.OpenMedia(message);
                }
            }
        }

        private TLMessageBase _innerMessage;

        private void MessageControl_OnHold(object sender, GestureEventArgs e)
        {
            var messageControl = sender as MessageControl;
            if (messageControl != null)
            {
                _innerMessage = GetInnerMessage(messageControl, e);
            }
        }

        private TLMessageBase GetInnerMessage(FrameworkElement element, GestureEventArgs e)
        {
            var groupedMessage = element.DataContext as TLMessage73;
            if (groupedMessage != null)
            {
                var messageMediaGroup = groupedMessage.Media as TLMessageMediaGroup;
                if (messageMediaGroup != null)
                {
                    var point = e.GetPosition(Application.Current.RootVisual);
                    var elements = VisualTreeHelper.FindElementsInHostCoordinates(point, MessagesList);
                    var mediaPhotoControl = elements.OfType<IMediaControl>().FirstOrDefault();
                    if (mediaPhotoControl != null)
                    {
                        var message = messageMediaGroup.Group.OfType<TLMessage>().FirstOrDefault(x => x.Media == mediaPhotoControl.Media);
                        if (message != null)
                        {
                            return message;
                        }
                    }
                }
            }

            return element.DataContext as TLMessageBase;
        }

        private void MessageControl_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (FocusManager.GetFocusedElement() == InputMessage.Input)
            {
                return;
            }

            var messageControl = sender as MessageControl;
            if (messageControl != null)
            {
                var messageService = messageControl.DataContext as TLMessageService;
                if (messageService != null)
                {
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine("  Manipulation started");
            _skipFirstManipulationDelta = true;
            e.Handled = true;
        }

        private bool _skipFirstManipulationDelta;

        private void MessageControl_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_skipFirstManipulationDelta)
            {
                _skipFirstManipulationDelta = false;
                return;
            }

            if (FocusManager.GetFocusedElement() == InputMessage.Input)
            {
                return;
            }

            var messageControl = sender as MessageControl;
            if (messageControl != null)
            {
                var decryptedMessageService = messageControl.DataContext as TLMessageService;
                if (decryptedMessageService != null)
                {
                    return;
                }

                var transform = messageControl.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    return;
                }

                var translationX = transform.X - e.DeltaManipulation.Translation.X;

                if (translationX < -100.0)
                {
                    translationX = -100.0;
                }
                else if (translationX > 100.0)
                {
                    translationX = 100.0;
                }

                if (messageControl.CacheMode == null) messageControl.CacheMode = new BitmapCache();
                messageControl.RenderTransform = new TranslateTransform
                {
                    X = translationX
                };

                e.Handled = true;
            }
        }

        private void MessageControl_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _skipFirstManipulationDelta = false;
            System.Diagnostics.Debug.WriteLine("  Manipulation completed State={0}", MessagesList.Viewport.ManipulationState);
            if (FocusManager.GetFocusedElement() == InputMessage.Input)
            {
                return;
            }

            var messageControl = sender as MessageControl;
            if (messageControl != null)
            {
                var decryptedMessageService = messageControl.DataContext as TLMessageService;
                if (decryptedMessageService != null)
                {
                    return;
                }
                var transform = messageControl.RenderTransform as TranslateTransform;
                if (transform != null)
                {
                    var translateX = transform.X;

                    var storyboard = new Storyboard();
                    var doubleAnimation = new DoubleAnimation();
                    doubleAnimation.To = 0.0;
                    doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.15));
                    Storyboard.SetTarget(doubleAnimation, messageControl.RenderTransform);
                    Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("X"));
                    storyboard.Children.Add(doubleAnimation);

                    storyboard.Begin();
                    storyboard.Completed += (o, args) =>
                    {
                        messageControl.CacheMode = null;
                        if (translateX >= 75.0)
                        {
                            var messageBase = messageControl.DataContext as TLMessageBase;
                            var innerMessage = GetInnerMessage(e, messageBase);
                            if (innerMessage != null)
                            {
                                messageBase = innerMessage;
                            }

                            if (messageBase == null) return;

                            ViewModel.ReplyMessage(messageBase);
                            InputMessage.FocusInput();
                        }
                        else if (translateX <= -75.0)
                        {
                            var messageBase = messageControl.DataContext as TLMessageBase;

                            if (messageBase == null) return;

                            ViewModel.FastForwardMessages(new List<TLMessageBase> { messageBase });
                            SelfView.Focus();
                        }
                    };
                }
            }
        }

        private TLMessageBase GetInnerMessage(ManipulationCompletedEventArgs e, TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage73;
            if (message != null)
            {
                var mediaGroup = message.Media as TLMessageMediaGroup;
                if (mediaGroup != null)
                {
                    var source = e.OriginalSource as FrameworkElement;
                    if (source != null)
                    {
                        var media = source.DataContext as TLMessageMediaBase;
                        for (int i = 0; i < mediaGroup.Group.Count; i++)
                        {
                            var m = mediaGroup.Group[i] as TLMessage;
                            if (m != null && m.Media == media)
                            {
                                return m;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void Items_OnItemRealized(object sender, ItemRealizationEventArgs e)
        {

        }
    }

    public interface IDialogDetailsView
    {
        void ScrollTo(TLObject item);

        void OpenBitmapCache();

        void CloseBitmapCache();

        void SetInlineBot(TLUserBase bot);

        void MoveCurretToEnd();

        bool IsScrollToBottomButtonVisible { get; }

        void HideMentionButton();

        void ShowMentionButton();

        void HideScrollToBottomButton();

        void ShowScrollToBottomButton();

        void CreateBitmapCache(Action action);

        void StopPlayersAndCreateBitmapCache(Action action);

        void ResumeChatPlayers();

        void PauseChatPlayers();

        void StartChatPlayer(StartGifPlayerEventArgs args);
    }

    public class TLStickerItem : TLObject
    {
        public TLDocumentBase Document { get; set; }

        public TLStickerItem Self
        {
            get { return this; }
        }

        public override string ToString()
        {
            return string.Format("TLStickerItem document={0}", Document);
        }
    }
}