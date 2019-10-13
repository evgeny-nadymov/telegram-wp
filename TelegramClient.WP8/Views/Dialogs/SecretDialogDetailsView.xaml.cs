// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using Telegram.EmojiPanel;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Controls;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Media;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using StickerSelectedEventArgs = Telegram.EmojiPanel.Controls.Emoji.StickerSelectedEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class SecretDialogDetailsView : IDialogDetailsView
    {
        public SecretDialogDetailsViewModel ViewModel
        {
            get { return DataContext as SecretDialogDetailsViewModel; }
        }

        public void MoveCurretToEnd()
        {
            if (FocusManager.GetFocusedElement() != InputMessage) return;

            InputMessage.SelectionStart = InputMessage.Text.Length;
            InputMessage.SelectionLength = 0;
        }

        private readonly Stopwatch _timer;

        #region ApplicationBar

        private bool _firstRun = true;

        private readonly MenuItem _callButton = new MenuItem
        {
            Header = AppResources.Call
        };

        private readonly MenuItem _manageButton = new MenuItem
        {
            Header = AppResources.Select
        };

        private void BuildLocalizedAppBar()
        {
            if (!_firstRun) return;
            _firstRun = false;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.99;

            ContextMenu.Items.Add(_callButton);
            ContextMenu.Items.Add(_manageButton);

            ApplicationBar.IsVisible = false; //ViewModel.IsApplicationBarVisible && !ViewModel.IsChooseAttachmentOpen;
        }
        #endregion

        private EmojiControl _emojiKeyboard;

        private TranslateTransform _frameTransform;

        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register(
            "RootFrameTransformProperty", typeof(double), typeof(SecretDialogDetailsView), new PropertyMetadata(OnRootFrameTransformChanged));

        private static void OnRootFrameTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as SecretDialogDetailsView;
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

        }

        private void CloseCommandsPlaceholder()
        {

        }

        private TLAllStickers _allStickers;

        private object _previousFocusedElement;

        private object _focusedElement;

        public SecretDialogDetailsView()
        {
            _timer = System.Diagnostics.Stopwatch.StartNew();

            InitializeComponent();

            CaptionBorder.Background = ShellView.CaptionBrush;

            //Full HD
            OptimizeFullHD();

            _callButton.Click += (sender, args) => ViewModel.Call();
            _manageButton.Click += (sender, args) => ViewModel.IsSelectionEnabled = true;
            GotFocus += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine(args.OriginalSource);
                _previousFocusedElement = _focusedElement;
                _focusedElement = args.OriginalSource;
            };

            Loaded += InitializeMTProtoService;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void InitializeMTProtoService(object sender, RoutedEventArgs e)
        {
            Loaded -= InitializeMTProtoService;

            var mtProtoService = IoC.Get<IMTProtoService>();
            mtProtoService.StartInitialize(); 
        }

        private void OnSmileButtonClick()
        {
            if (_emojiKeyboard == null)
            {
                _emojiKeyboard = EmojiControl.GetInstance();

                _emojiKeyboard.SetGroupStickers(null, null);

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
                InputMessage.EmojiPlaceholder.Content = _emojiKeyboard;

                _emojiKeyboard.IsOpen = true;

                //_emojiKeyboard.SetHeight(InputBox.KeyboardHeight > 0.0 ? InputBox.KeyboardHeight : _emojiKeyboard.Height);
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {
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
                var awaitKeyboardDown = false;
                if (InputMessage == FocusManager.GetFocusedElement())
                {
                    awaitKeyboardDown = true;
                    MessagesList.Focus();
                }

                Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                {
                    if (awaitKeyboardDown)
                    {
                        Thread.Sleep(400);
                    }

                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                    {
                        InputMessage.OpenPlaceholder();

                        if (_emojiKeyboard != null)
                        {
                            _emojiKeyboard.OpenStickerSprites();
                        }

                        SetCaptionVisibility(Visibility.Collapsed);
                        CloseEmptyDialogPlaceholder();
                        CloseCommandsPlaceholder();
                    });
                });
            }
        }

        private void SetCaptionVisibility(Visibility visibility)
        {
            return;

            DialogPhoto.Visibility = visibility;
            Title.Visibility = visibility;
            ContextMenuIcon.Visibility = visibility;
            CaptionBorder.Height = visibility == Visibility.Visible ? 105.0 : 71.0;
        }

        private Binding _emptyPlaceholderVisibilityBinding;

        private void OpenEmptyDialogPlaceholder()
        {
            RestoreVisibilityBinding(Description, _emptyPlaceholderVisibilityBinding, Visibility.Collapsed);
        }

        private void CloseEmptyDialogPlaceholder()
        {
            _emptyPlaceholderVisibilityBinding = SaveVisibilityBinding(Description);
            Description.Visibility = Visibility.Collapsed;
        }
#if DEBUG
        ~SecretDialogDetailsView()
        {
            System.Diagnostics.Debug.WriteLine("~SecretDialogDetailsView");
        }
#endif

        private void OptimizeFullHD()
        {
            //var appBar = new ApplicationBar();
            //var appBarDefaultSize = appBar.DefaultSize;

            //WaitingBar.Height = appBarDefaultSize;
        }

        private void OnViewModelScrollToBottom(object sender, System.EventArgs e)
        {
            if (ViewModel.Items.Count > 0)
            {
                MessagesList.ScrollToItem(ViewModel.Items[0]);
            }
        }

        private void OnViewModelScrollTo(object sender, ScrollToEventArgs e)
        {
            if (ViewModel.Items.Count > 0)
            {
                MessagesList.ScrollToItem(e.DecryptedMessage);
            }
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
                LayoutRoot.Children.Add(_selectionCaption);
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
                //((TranslateTransform)CaptionGrid.RenderTransform).Y = 0.0;
                //CaptionGrid.Opacity = 1.0;
                //DialogPhoto.Visibility = Visibility.Collapsed;
                //Caption.Visibility = Visibility.Collapsed;
                //ContextMenuIcon.Visibility = Visibility.Collapsed;
            };

            //DialogPhoto.Visibility = Visibility.Collapsed;
            //Caption.Visibility = Visibility.Collapsed;
            //ContextMenuIcon.Visibility = Visibility.Collapsed;
            var count = SecretDialogDetailsViewModel.UngroupEnumerator(ViewModel.Items).Count(x => x.IsSelected);
            var noneTTLCount = ViewModel.Items.Count(x => x.IsSelected);

            _selectionCaption.Text = string.Format(count == 1 ? AppResources.ItemsSelectedSingular : AppResources.ItemsSelectedPlural, count).ToUpperInvariant();
            _selectionCaption.Visibility = Visibility.Visible;

            ApplicationBar.Buttons.Clear();
            Execute.BeginOnUIThread(() =>
            {
                InputMessage.IsDeleteActionVisible = true;

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

        private Visibility GetMuteButtonVisibility()
        {
            return Visibility.Collapsed;
        }

        private void SwitchToNormalMode()
        {
            InputMessage.SwitchToNormalMode();
        }

        public static readonly DependencyProperty StopwatchProperty =
            DependencyProperty.Register("Stopwatch", typeof(string), typeof(SecretDialogDetailsView), new PropertyMetadata(default(string)));

        public string Stopwatch
        {
            get { return (string)GetValue(StopwatchProperty); }
            set { SetValue(StopwatchProperty, value); }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            SetRootFrameBinding();

            InputMessage.AudioRecorded += OnAudioRecorded;
            InputMessage.RecordStarted += OnRecordStarted;
            InputMessage.RecordCanceled += OnRecordCanceled;

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            ViewModel.ScrollToBottom += OnViewModelScrollToBottom;
            ViewModel.ScrollTo += OnViewModelScrollTo;

            GifPlayerControl.MediaStateChanged += OnGifPlayerControlMediaStateChanged;

            if (ViewModel.MultiImageEditor != null)
            {
                ViewModel.MultiImageEditor.PropertyChanged += OnMultiImageEditorPropertyChanged;
            }
            if (ViewModel.LocationPicker != null)
            {
                ViewModel.LocationPicker.PropertyChanged += OnLocationPickerPropertyChanged;
            }

            if (ViewModel.IsApplicationBarVisible)
            {
                BuildLocalizedAppBar();
            }
            else if (ApplicationBar != null)
            {
                ApplicationBar.IsVisible = false;//ViewModel.IsApplicationBarVisible && !ViewModel.IsChooseAttachmentOpen;
            }

            RunAnimation();
            Stopwatch = _timer.Elapsed.ToString();
        }

        private void OnGifPlayerControlMediaStateChanged(object sender, MediaStateChangedEventArgs args)
        {
            switch (args.State)
            {
                case GifPlayerControlState.Opening:

                    break;
                case GifPlayerControlState.Opened:

                    break;
                case GifPlayerControlState.Failed:
                    break;
                case GifPlayerControlState.Paused:
                    break;
                case GifPlayerControlState.Resumed:
                    break;
                case GifPlayerControlState.Ended:
                    ResumeChatPlayers();
                    break;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            RemoveRootFrameBinding();

            InputMessage.AudioRecorded -= OnAudioRecorded;
            InputMessage.RecordStarted -= OnRecordStarted;
            InputMessage.RecordCanceled -= OnRecordCanceled;

            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            ViewModel.ScrollToBottom -= OnViewModelScrollToBottom;
            ViewModel.ScrollTo -= OnViewModelScrollTo;

            GifPlayerControl.MediaStateChanged -= OnGifPlayerControlMediaStateChanged;

            if (ViewModel.MultiImageEditor != null)
            {
                ViewModel.MultiImageEditor.PropertyChanged -= OnMultiImageEditorPropertyChanged;
            }
            if (ViewModel.LocationPicker != null)
            {
                ViewModel.LocationPicker.PropertyChanged -= OnLocationPickerPropertyChanged;
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

        private void OnLocationPickerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.LocationPicker.IsOpen))
            {
                MessagesList.IsHitTestVisible = IsItemsHitTestVisible();
            }
        }

        private bool IsItemsHitTestVisible()
        {
            return (ViewModel.ChooseAttachment == null || !ViewModel.ChooseAttachment.IsOpen)
                    && (ViewModel.ImageViewer == null || !ViewModel.ImageViewer.IsOpen)
                    && (ViewModel.MultiImageEditor == null || !ViewModel.MultiImageEditor.IsOpen)
                    //&& (ViewModel.AnimatedImageViewer == null || !ViewModel.AnimatedImageViewer.IsOpen)
                    //&& (ViewModel.VideoEditor == null || !ViewModel.VideoEditor.IsOpen)
                    && (ViewModel.LocationPicker == null || !ViewModel.LocationPicker.IsOpen);
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
                //element.Visibility = defaultValue;
            }
        }

        private void OnRecordCanceled(object sender, System.EventArgs e)
        {

        }

        private void OnRecordStarted(object sender, System.EventArgs e)
        {

        }

        private void OnAudioRecorded(object sender, AudioEventArgs e)
        {
            var viewModel = ViewModel;
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => viewModel.SendAudio(e));
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsGroupActionEnabled))
            {
                var count = SecretDialogDetailsViewModel.UngroupEnumerator(ViewModel.Items).Count(x => x.IsSelected);
                var noneTTLCount = ViewModel.Items.Count(x => x.IsSelected);
                if (_selectionCaption != null)
                {
                    _selectionCaption.Text = string.Format(count == 1 ? AppResources.ItemsSelectedSingular : AppResources.ItemsSelectedPlural, count).ToUpperInvariant();
                }

                InputMessage.IsGroupActionEnabled = new Tuple<bool, bool>(count > 0, count > 0 && noneTTLCount == count);
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.Reply))
            {
                if (ViewModel.Reply != null)
                {
                    if (ViewModel.Reply is TLDecryptedMessagesContainter)
                    {
                        return;
                    }

                    InputMessage.Focus();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsApplicationBarVisible)
                && ViewModel.IsApplicationBarVisible)
            {
                BuildLocalizedAppBar();
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
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.LocationPicker)
                && ViewModel.LocationPicker != null)
            {
                ViewModel.LocationPicker.PropertyChanged += OnLocationPickerPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsApplicationBarVisible))
            {
                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = false;//ViewModel.IsApplicationBarVisible && !ViewModel.IsChooseAttachmentOpen;
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsAppBarCommandVisible))
            {
                if (ViewModel.IsAppBarCommandVisible)
                {
                    InputMessage_OnLostFocus(this, null);
                    if (ViewModel.ChooseAttachment != null && ViewModel.ChooseAttachment.IsOpen)
                    {
                        ViewModel.ChooseAttachment.Close();
                        ApplicationBar.IsVisible = false;
                    }
                    _prevApplicationBar = null;
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.StartGifPlayers))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Items_OnManipulationStateChanged(this, System.EventArgs.Empty);
                });
            }
        }

        private void Items_OnBegin(object sender, System.EventArgs e)
        {
            if (MessagesList.Viewport.Bounds.Y == 0.0) return;

            HideScrollToBottomButton();
        }

        public bool IsScrollToBottomButtonVisible
        {
            get { return ScrollButton.Visibility == Visibility.Visible; }
        }

        public void HideMentionButton()
        {
            
        }

        public void HideScrollToBottomButton()
        {
            if (ScrollButton.Visibility == Visibility.Collapsed) return;

            //ScrollToBottomButton.Visibility = Visibility.Collapsed;
            //return;

            var storyboard = new Storyboard();
            var continuumScrollToBottomButton = new DoubleAnimationUsingKeyFrames();
            continuumScrollToBottomButton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            continuumScrollToBottomButton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
            Storyboard.SetTarget(continuumScrollToBottomButton, ScrollButton);
            Storyboard.SetTargetProperty(continuumScrollToBottomButton, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(continuumScrollToBottomButton);

            var continuumLayoutRootOpacity = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
            };
            Storyboard.SetTarget(continuumLayoutRootOpacity, ScrollButton);
            Storyboard.SetTargetProperty(continuumLayoutRootOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(continuumLayoutRootOpacity);
            storyboard.Completed += (sender, args) =>
            {
                ScrollButton.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin();
            //Telegram.Api.Helpers.Execute.BeginOnUIThread(() => storyboard.Begin());
        }

        public void ShowMentionButton()
        {

        }

        public void ShowScrollToBottomButton()
        {
            if (ScrollButton.Visibility == Visibility.Visible) return;

            ScrollButton.Visibility = Visibility.Visible;
            ScrollButton.Opacity = 0.0;

            var storyboard = new Storyboard();
            var continuumScrollToBottomButton = new DoubleAnimationUsingKeyFrames();
            continuumScrollToBottomButton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 150.0 });
            continuumScrollToBottomButton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
            Storyboard.SetTarget(continuumScrollToBottomButton, ScrollButton);
            Storyboard.SetTargetProperty(continuumScrollToBottomButton, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(continuumScrollToBottomButton);

            var continuumLayoutRootOpacity = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
            };
            Storyboard.SetTarget(continuumLayoutRootOpacity, ScrollButton);
            Storyboard.SetTargetProperty(continuumLayoutRootOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(continuumLayoutRootOpacity);

            storyboard.Begin();
            //Telegram.Api.Helpers.Execute.BeginOnUIThread(() => storyboard.Begin());
        }

        private IApplicationBar _prevApplicationBar;

        private void OnImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer.IsOpen))
            {
                MessagesList.IsHitTestVisible = !ViewModel.ChooseAttachment.IsOpen && (ViewModel.ImageViewer == null || !ViewModel.ImageViewer.IsOpen);

                if (ViewModel.ImageViewer != null
                    && ViewModel.ImageViewer.IsOpen)
                {
                    _prevApplicationBar = ApplicationBar;
                    ApplicationBar = ((DecryptedImageViewerView)ImageViewer.Content).ApplicationBar;
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

        private void OnChooseAttachmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ChooseAttachment.IsOpen))
            {
                MessagesList.IsHitTestVisible = !ViewModel.ChooseAttachment.IsOpen && (ViewModel.ImageViewer == null || !ViewModel.ImageViewer.IsOpen);
            }
        }

        private bool _isForwardInAnimation;
        private bool _isBackwardInAnimation;
        private bool _fromExternalUri;
        private readonly Uri _externalUri = new Uri(@"app://external/");

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

        private void RunAnimation()
        {
            if (_isForwardInAnimation)
            {
                _isForwardInAnimation = false;

                if (ViewModel.Chat.Bitmap != null)
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

                var continuumLayoutRootOpacity = new DoubleAnimation { From = 0.0, To = 1.0, Duration = TimeSpan.FromSeconds(0.25), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } };
                Storyboard.SetTarget(continuumLayoutRootOpacity, LayoutRoot);
                Storyboard.SetTargetProperty(continuumLayoutRootOpacity, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(continuumLayoutRootOpacity);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    LayoutRoot.Opacity = 1.0;
                    storyboard.Completed += (o, e) =>
                    {

                        //Items.Opacity = 0.0;
                        MessagesList.Visibility = Visibility.Visible;
                        MessagesCache.Visibility = Visibility.Collapsed;
                        ViewModel.OnForwardInAnimationComplete();
                        Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () =>
                        {
                            Items_OnManipulationStateChanged(this, System.EventArgs.Empty);
                        });
                        //Deployment.Current.Dispatcher.BeginInvoke(() => CloseCacheStoryboard.Begin());
                    };
                    storyboard.Begin();
                });
            }
            else if (_isBackwardOutAnimation)
            {
                _isBackwardOutAnimation = false;

                var storyboard = new Storyboard();

                var translateAnimation = new DoubleAnimationUsingKeyFrames();
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                Storyboard.SetTarget(translateAnimation, LayoutRoot);
                Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation);

                var opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
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
                        ViewModel.OnBackwardInAnimationComplete();

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

        private void OpenPeerDetails_OnTap(object sender, GestureEventArgs args)
        {
            //if (ViewModel.With is TLChatForbidden)
            //{
            //    return;
            //}

            //if (CommandsControl.Visibility == Visibility.Visible)
            //{
            //    OpenEmptyDialogPlaceholder();
            //    CloseCommandsPlaceholder();
            //    SetCaptionVisibility(Visibility.Visible);
            //}

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
            {
                OpenEmptyDialogPlaceholder();
                CloseEmojiPlaceholder();
                SetCaptionVisibility(Visibility.Visible);
            }

            StopPlayersAndCreateBitmapCache(() => ViewModel.OpenPeerDetails());
        }

        public void StopPlayersAndCreateBitmapCache(System.Action callback = null)
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

        public void CreateBitmapCache(System.Action callback = null)
        {
            try
            {
                if (MessagesList.Visibility == Visibility.Visible)
                {
                   // var stopwatch = Stopwatch.StartNew();
                    var writeableBitmap = new WriteableBitmap(MessagesList, null);
                    //var elapsed1 = stopwatch.Elapsed;
                    ViewModel.Chat.SetBitmap(writeableBitmap);
                    //var elapsed2 = stopwatch.Elapsed;
                    //MessageBox.Show("create bitmap render=" + elapsed1 + " set=" + elapsed2);
                }
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("WritableBitmap exception " + ex);
            }

            callback.SafeInvoke();
        }

        private bool _suppressCancel = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.OnNavigatedTo();
            //MediaControl.Content = MessagePlayerControl.Player;

            // этот код выполняется до того, как происходит отрисовка экрана
            // нельзя ставить сюда долгие операции
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
                    ViewModel.OnBackwardInAnimationComplete();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        StartGifPlayers();
                    });
                }
                _fromExternalUri = false;
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
        }

        private void OnSettingsButtonClick(object sender, System.EventArgs eventArgs)
        {
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

        private bool _isBackwardOutAnimation;
        private bool _isForwardOutAnimation;

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
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

            if (e.Uri.OriginalString.EndsWith("EditVideoView.xaml")
                || e.Uri.OriginalString.EndsWith("MapView.xaml")
                || e.Uri.OriginalString.EndsWith("ContactView.xaml")
                || e.Uri.OriginalString.EndsWith("ChatView.xaml")
                || e.Uri.OriginalString.EndsWith("ProfilePhotoViewerView.xaml")
                || e.Uri.OriginalString.EndsWith("SearchShellView.xaml")
                || e.Uri.OriginalString.EndsWith("ChooseDialogView.xaml")
                || e.Uri.OriginalString.EndsWith("StickersView.xaml")
                )
            {
                CreateBitmapCache();
                MessagesList.Visibility = Visibility.Collapsed;
                MessagesCache.Visibility = Visibility.Visible;
                //e.Cancel = true;
                //Deployment.Current.Dispatcher.BeginInvoke(() => NavigationService.Navigate(e.Uri));
                //return;
            }
            else
            {
                //TransitionService.SetNavigationOutTransition(Self, OutTransition);
            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.OnNavigatedFrom();

            MessagePlayerControl.Stop();
            GifPlayerControl.StopVideo();

            if (e.Uri == _externalUri)
            {
                _fromExternalUri = true;
            }
            else
            {
                _fromExternalUri = false;
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (!NavigationService.BackStack.Any())
            {
                e.Cancel = true;
                ViewModel.NavigateToShellViewModel();

                return;
            }

            base.OnBackKeyPress(e);
        }

        private void SecretDialogDetailsView_OnBackKeyPress(object sender, CancelEventArgs e)
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

                return;
            }

            if (ViewModel.ImageViewer != null
                && ViewModel.ImageViewer.IsOpen)
            {
                ViewModel.ImageViewer.CloseViewer();
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

            if (ViewModel.IsSelectionEnabled)
            {
                ViewModel.IsSelectionEnabled = false;
                ApplicationBar.IsVisible = false;//!ViewModel.IsAppBarCommandVisible;
                e.Cancel = true;

                return;
            }

            if (_emojiKeyboard != null && _emojiKeyboard.IsPreviewMenuOpened)
            {
                _emojiKeyboard.ClosePreviewMenu();
                e.Cancel = true;
                
                return;
            }

            if (InputMessage.EmojiPlaceholder.Visibility == Visibility.Visible)
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
                e.Cancel = true;
                ViewModel.ChooseAttachment.Close();
                ResumeChatPlayers();

                return;
            }

            if (_lastContextMenu != null && _lastContextMenu.IsOpen)
            {
                _lastContextMenu.IsOpen = false;
                e.Cancel = true;

                return;
            }

            _isBackwardOutAnimation = true;

            try
            {
                if (MessagesList.Visibility == Visibility.Visible)
                {
                    var writeableBitmap = new WriteableBitmap(MessagesList, null);
                    ViewModel.Chat.SetBitmap(writeableBitmap);
                }
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("WritableBitmap exception " + ex);
            }
            MessagesCache.Visibility = Visibility.Visible;
            MessagesList.Visibility = Visibility.Collapsed;

            RunAnimation();
        }

        private void InputMessage_OnGotFocus(object sender, RoutedEventArgs e)
        {
            SetCaptionVisibility(Visibility.Collapsed);
            CloseCommandsPlaceholder();
            CloseEmptyDialogPlaceholder();

            PauseChatPlayers();
        }

        private bool _smileButtonPressed;

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

            var serviceMessage = menu.DataContext as TLDecryptedMessageService;
            if (serviceMessage != null)
            {
                menu.IsOpen = false;
                return;
            }

            _lastContextMenu = sender as ContextMenu;
        }

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

        private void UIElement_OnHold(object sender, GestureEventArgs e)
        {

        }

        private void ScrollButton_OnClick(object sender, RoutedEventArgs e)
        {
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
                if (BrowserNavigationService.IsValidUsername(text))
                {
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.ResolveUsername(text));
                }
            }
        }

        private void NavigationOutTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            //Items.Visibility = Visibility.Collapsed;
        }

        private void SecretPhotoPlaceholder_OnElapsed(object sender, System.EventArgs e)
        {
            return;

            var element = sender as FrameworkElement;
            if (element != null)
            {
                var decryptedMessage = element.DataContext as TLDecryptedMessage;
                if (decryptedMessage != null)
                {
                    var decryptedMessageMediaPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                    if (decryptedMessageMediaPhoto != null)
                    {
                        ViewModel.DeleteMessage(false, decryptedMessageMediaPhoto);
                        //SecretImageViewer.Visibility = Visibility.Collapsed;
                        //ApplicationBar.IsVisible = false;
                    }
                }
            }
        }

        private void SecretPhotoPlaceholder_OnStartTimer(object sender, System.EventArgs e)
        {
            return;

            var element = sender as FrameworkElement;
            if (element != null)
            {
                var decryptedMessage = element.DataContext as TLDecryptedMessage;
                if (decryptedMessage != null)
                {
                    var decryptedMessageMediaPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                    if (decryptedMessageMediaPhoto != null)
                    {
                        var result = ViewModel.OpenSecretPhoto(decryptedMessageMediaPhoto);
                        if (result)
                        {
                            //SecretImageViewer.Visibility = Visibility.Visible;
                            //ApplicationBar.IsVisible = false;
                        }
                    }
                }
            }
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
//            var uielement = sender as FrameworkElement;
//            if (uielement != null)
//            {
//                var decryptedMessage = uielement.DataContext as TLDecryptedMessage17;
//                if (decryptedMessage != null)
//                {
//                    var result = ViewModel.OpenSecretPhoto(decryptedMessage);
//                    if (result)
//                    {
//                        SecretImageViewer.Visibility = Visibility.Visible;
//                        ApplicationBar.IsVisible = false;
//                    }
//                }
//            }
        }

        private void Items_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (ViewModel.SliceLoaded) return;

            ViewModel.LoadNextSlice();
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

            //if (ViewModel != null)
            //{
            //    var chatForbidden = ViewModel.With as TLChatForbidden;
            //    var chat = ViewModel.With as TLChat;

            //    var isForbidden = chatForbidden != null || (chat != null && chat.Left.Value);
            //    if (isForbidden)
            //    {
            //        return Visibility.Collapsed;
            //    }
            //}

            return Visibility.Visible;
        }

        private void InputMessage_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InputMessage.KeyboardButtonVisibility = GetReplyKeyboardButtonVisibility();
            InputMessage.MuteButtonVisibility = GetMuteButtonVisibility();
        }

        private Visibility GetReplyKeyboardButtonVisibility()
        {
            return Visibility.Collapsed;
        }

        private void MorePanel_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var message = frameworkElement.DataContext as TLDecryptedMessage;
            if (message == null) return;

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

        private void InlineBotResults_OnTap(object sender, GestureEventArgs e)
        {
            InputMessage.Focus();

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var result = frameworkElement.DataContext as TLBotInlineResultBase;
                if (result != null)
                {
                    GifPlayerControl.StopInlineBotActivePlayers();
                    ViewModel.SendBotInlineResult(result);
                }
            }
        }

        private void InlineBotResults_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueInlineBotResults();
        }

        private void StickerHints_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement == null) return;

            var stickerItem = frameworkElement.DataContext as TLStickerItem;
            if (stickerItem == null) return;

            var document22 = stickerItem.Document as TLDocument22;
            if (document22 == null) return;

            ViewModel.SendSticker(document22);
        }

        private void StickerHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueStickerHints();
        }

        private void ContextMenu_OnLoaded(object sender, RoutedEventArgs e)
        {
            var contextMenu = (ContextMenu) sender;
            if (contextMenu != null)
            {
                var decryptedMessage = contextMenu.DataContext as TLDecryptedMessage;
                if (decryptedMessage != null)
                {

                }
            }
        }

        private void ReplyMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                menuItem.Visibility = Visibility.Collapsed;

                var chat = ViewModel.Chat as TLEncryptedChat17;
                if (chat != null && chat.Layer.Value >= Constants.MinSecretChatWithRepliesLayer)
                {
                    menuItem.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddToStickers_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                var decryptedMessage = menuItem.DataContext as TLDecryptedMessage;
                if (decryptedMessage != null && decryptedMessage.IsSticker())
                {
                    var mediaExternalDocument = decryptedMessage.Media as TLDecryptedMessageMediaExternalDocument;
                    if (mediaExternalDocument != null)
                    {
                        var stickerSet = mediaExternalDocument.StickerSet;
                        if (stickerSet != null && !(stickerSet is TLInputStickerSetEmpty))
                        {
                            var allStickers = IoC.Get<IStateService>().GetAllStickers() as TLAllStickers29;
                            if (allStickers != null)
                            {
                                var set = allStickers.Sets.FirstOrDefault(x => string.Equals(x.ShortName.ToString(), stickerSet.Name.ToString(), StringComparison.Ordinal));
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
                var message = item.DataContext as TLDecryptedMessage;
                if (message != null && message == e.DecryptedMessage && message.IsGif())
                {
                    var media = message.Media;
                    if ((media != null && media.AutoPlayGif == true) || (settings != null && settings.AutoPlayGif && media != null && media.AutoPlayGif != false))
                    {
                        var gifPlayer = item.FindChildOfType<GifPlayerControl>();
                        if (gifPlayer != null)
                        {
                            gifPlayer.Start();
                        }
                        return;
                    }
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
                var message = item.DataContext as TLDecryptedMessage;
                if (message != null && message.IsGif())
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

            var message = frameworkElement.DataContext as TLDecryptedMessage45;
            if (message == null) return;

            var viaBotName = message.ViaBotName;
            if (viaBotName == null) return;

            SetInlineBotCommon(viaBotName.ToString());
        }

        public void SetInlineBot(TLUserBase bot)
        {
            var user = bot as TLUser;
            if (user == null || !user.IsInlineBot || user.UserName == null) return;

            SetInlineBotCommon(user.UserName.ToString());
        }

        private void SetInlineBotCommon(string userName)
        {
            InputMessage.Text = string.Format("@{0} ", userName);
            InputMessage.Focus();
            InputMessage.SelectionStart = InputMessage.Text.Length;
            InputMessage.SelectionLength = 0;

            var text = InputMessage.Text.Trim();
            if (BrowserNavigationService.IsValidUsername(text))
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.ResolveUsername(text));
            }
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
            ViewModel.DeleteChat();
        }

        private void InputMessage_OnOpenReplyButtonClick(object sender, System.EventArgs e)
        {
            
        }

        private void InputMessage_OnDeleteReplyButtonClick(object sender, System.EventArgs e)
        {
            ViewModel.DeleteReply();
        }

        private void InputMessage_OnDeleteButtonClick(object sender, System.EventArgs e)
        {
            ViewModel.DeleteMessages();
        }

        private void InputMessage_OnForwardButtonClick(object sender, System.EventArgs e)
        {
            
        }

        private void InputMessage_OnCancelSelectionButtonClick(object sender, System.EventArgs e)
        {
            ViewModel.IsSelectionEnabled = false;
        }

        private void InputMessage_OnSendClick(object sender, System.EventArgs e)
        {
            ViewModel.Send();
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

        private void InputMessage_OnMuteButtonClick(object sender, System.EventArgs e)
        {
            
        }

        private void InputMessage_OnKeyboardButtonClick(object sender, System.EventArgs e)
        {
            
        }

        private void InputMessage_OnEmojiClick(object sender, System.EventArgs e)
        {
            OnSmileButtonClick();
        }

        private void CopyMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Collapsed;

            var message = element.DataContext as TLDecryptedMessage;
            if (message != null)
            {
                if (!TLString.IsNullOrEmpty(message.Message))
                {
                    element.Visibility = Visibility.Visible;
                    return;
                }

                var mediaCaption = message.Media as IMediaCaption;
                if (mediaCaption != null && !TLString.IsNullOrEmpty(mediaCaption.Caption))
                {
                    element.Visibility = Visibility.Visible;
                    return;
                }
            }
        }

        private void DeleteMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void SelectMessages_OnClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var message = frameworkElement.DataContext as TLDecryptedMessageBase;
                if (message != null && message.SelectionVisibility == Visibility.Visible)
                {
                    ViewModel.ChangeSelection(message);
                }
            }

            ViewModel.IsSelectionEnabled = true;
        }

        public void ScrollTo(TLObject obj)
        {
            MessagesList.ScrollToItem(obj);
        }

        private void ResendMessage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            element.Visibility = Visibility.Collapsed;
            var message = element.DataContext as TLDecryptedMessageBase;
            if (message != null)
            {
                element.Visibility = message.Status == MessageStatus.Failed ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void MessageControl_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (FocusManager.GetFocusedElement() == InputMessage.Input)
            {
                return;
            }
            if (MessagesList.Viewport.ManipulationState != ManipulationState.Idle)
            {
                return;
            }

            var messageControl = sender as DecryptedMessageControl;
            if (messageControl != null)
            {
                var decryptedMessageService = messageControl.DataContext as TLDecryptedMessageService;
                if (decryptedMessageService != null)
                {
                    return;
                }
            }
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
            if (MessagesList.Viewport.ManipulationState != ManipulationState.Idle)
            {
                return;
            }

            var messageControl = sender as DecryptedMessageControl;
            if (messageControl != null)
            {
                var decryptedMessageService = messageControl.DataContext as TLDecryptedMessageService;
                if (decryptedMessageService != null)
                {
                    return;
                }

                var translationX = -e.CumulativeManipulation.Translation.X;

                if (translationX < 0.0)
                {
                    translationX = 0.0;
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
            if (FocusManager.GetFocusedElement() == InputMessage.Input)
            {
                return;
            }
            if (MessagesList.Viewport.ManipulationState != ManipulationState.Idle)
            {
                return;
            }

            var messageControl = sender as DecryptedMessageControl;
            if (messageControl != null)
            {
                var decryptedMessageService = messageControl.DataContext as TLDecryptedMessageService;
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
                            var messageBase = messageControl.DataContext as TLDecryptedMessageBase;
                            var innerMessage = GetInnerMessage(e, messageBase);
                            if (innerMessage != null)
                            {
                                messageBase = innerMessage;
                            }

                            if (messageBase == null) return;

                            ViewModel.ReplyMessage(messageBase);
                            InputMessage.FocusInput();
                        }
                    };
                }
            }
        }

        private TLDecryptedMessageBase GetInnerMessage(ManipulationCompletedEventArgs e, TLDecryptedMessageBase messageBase)
        {
            var message = messageBase as TLDecryptedMessage73;
            if (message != null)
            {
                var mediaGroup = message.Media as TLDecryptedMessageMediaGroup;
                if (mediaGroup != null)
                {
                    var source = e.OriginalSource as FrameworkElement;
                    if (source != null)
                    {
                        var media = source.DataContext as TLDecryptedMessageMediaBase;
                        for (var i = 0; i < mediaGroup.Group.Count; i++)
                        {
                            var m = mediaGroup.Group[i] as TLDecryptedMessage;
                            if (m != null && m.Media == media)
                            {
                                return m;
                            }
                        }
                    }
                }
            }

            return messageBase;
        }

        private void MessageControl_OnTapMedia(object sender, GestureEventArgs e)
        {
            var messageControl = sender as DecryptedMessageControl;
            if (messageControl != null)
            {
                var message = GetInnerMessage(messageControl, e);
                if (message != null)
                {
                    ViewModel.OpenMedia(message);
                }
            }
        }

        private TLDecryptedMessageBase _innerMessage;

        private void MessageControl_OnHold(object sender, GestureEventArgs e)
        {
            var messageControl = sender as DecryptedMessageControl;
            if (messageControl != null)
            {
                _innerMessage = GetInnerMessage(messageControl, e);
            }
        }

        private TLDecryptedMessage GetInnerMessage(FrameworkElement element, GestureEventArgs e)
        {
            var groupedMessage = element.DataContext as TLDecryptedMessage73;
            if (groupedMessage != null)
            {
                var messageMediaGroup = groupedMessage.Media as TLDecryptedMessageMediaGroup;
                if (messageMediaGroup != null)
                {
                    var point = e.GetPosition(Application.Current.RootVisual);
                    var elements = VisualTreeHelper.FindElementsInHostCoordinates(point, MessagesList);
                    var mediaPhotoControl = elements.OfType<IMediaControl>().FirstOrDefault();
                    if (mediaPhotoControl != null)
                    {
                        var message = messageMediaGroup.Group.OfType<TLDecryptedMessage>().FirstOrDefault(x => x.Media == mediaPhotoControl.Media);
                        if (message != null)
                        {
                            return message;
                        }
                    }
                }
            }

            return element.DataContext as TLDecryptedMessage;
        }

        private void SelectionBorder_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var groupedMessage = element.DataContext as TLDecryptedMessage73;
                if (groupedMessage != null)
                {
                    var selectionChanged = false;

                    var messageMediaGroup = groupedMessage.Media as TLDecryptedMessageMediaGroup;
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
                    ViewModel.ChangeSelection(element.DataContext as TLDecryptedMessageBase);
                }
            }
        }

        private void DeleteMessage_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var message = element.DataContext as TLDecryptedMessageBase;
                if (message != null)
                {
                    ViewModel.DeleteMessage(true, message);
                }
            }
        }
    }
}