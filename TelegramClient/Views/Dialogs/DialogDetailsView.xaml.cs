using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Animation.Navigation;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Media;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class DialogDetailsView
    {
        private DialogDetailsViewModel ViewModel
        {
            get { return DataContext as DialogDetailsViewModel; }
        }

        private readonly AppBarButton _sendButton = new AppBarButton
        {
            Text = AppResources.Send,
            IconUri = new Uri("/Images/ApplicationBar/appbar.send.text.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _attachButton = new AppBarButton
        {
            Text = AppResources.Attach,
            IconUri = new Uri("/Images/ApplicationBar/appbar.attach.png", UriKind.Relative)
        };

        private readonly AppBarButton _smileButton = new AppBarButton
        {
            Text = AppResources.Emoji,
            //IsEnabled = false,
            IconUri = new Uri("/Images/ApplicationBar/appbar.smile.png", UriKind.Relative)
        };

        private readonly AppBarButton _manageButton = new AppBarButton
        {
            Text = AppResources.Manage,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.manage.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _forwardButton = new AppBarButton
        {
            Text = AppResources.Forward,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.forwardmessage.png", UriKind.Relative)
        };

        private readonly AppBarButton _deleteButton = new AppBarButton
        {
            Text = AppResources.Delete,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.delete.png", UriKind.Relative)
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

        public DialogDetailsView()
        {
            InitializeComponent();

            AnimationContext = LayoutRoot;

            _sendButton.Click += (sender, args) => ViewModel.Send();
            _attachButton.Click += (sender, args) =>
            {
                EmojiPlaceholder.Visibility = Visibility.Collapsed;
                //DialogPhoto.Visibility = Visibility.Visible;
                //Title.Visibility = Visibility.Visible;

                ChooseAttachment.Focus();
                ViewModel.Attach();
            };
            _smileButton.Click += (sender, args) =>
            {
                if (_emojiKeyboard == null)
                {
                    _emojiKeyboard = EmojiControl.GetInstance();

                    _emojiKeyboard.BindTextBox(InputMessage);
                    EmojiPlaceholder.Content = _emojiKeyboard;
                    _emojiKeyboard.IsOpen = true;
                }

                if (EmojiPlaceholder.Visibility == Visibility.Visible)
                {
                    if (InputMessage == FocusManager.GetFocusedElement())
                    {
                        _smileButtonPressed = true;

                        EmojiPlaceholder.Opacity = 1.0;
                        EmojiPlaceholder.Height = EmojiControl.PortraitOrientationHeight;
                        Items.Focus();
                    }
                    else
                    {
                        EmojiPlaceholder.Visibility = Visibility.Collapsed;
                        //DialogPhoto.Visibility = Visibility.Visible;
                        //Title.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    var awaitKeyboardDown = false;
                    if (InputMessage == FocusManager.GetFocusedElement())
                    {
                        awaitKeyboardDown = true;
                        Items.Focus();
                    }

                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        if (awaitKeyboardDown)
                        {
                            Thread.Sleep(400);
                        }
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            EmojiPlaceholder.Opacity = 1.0;
                            EmojiPlaceholder.Visibility = Visibility.Visible;
                            //DialogPhoto.Visibility = Visibility.Collapsed;
                            //Title.Visibility = Visibility.Collapsed;
                        });
                    });
                }
            };
            _manageButton.Click += (sender, args) => SwitchToSelectionMode();
            _forwardButton.Click += (sender, args) =>
            {
                var selectedItems = ViewModel.Items.Where(x => x.Id != null && x.IsSelected).ToList();
                if (selectedItems.Count == 0) return;

                ViewModel.IsSelectionEnabled = false;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (Items.Visibility == Visibility.Visible)
                        {
                            var writeableBitmap = new WriteableBitmap(Items, null);
                            ViewModel.With.Bitmap = writeableBitmap;
                        }
                    }
                    catch (Exception ex)
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("WritableBitmap exception " + ex);
                    }

                    ViewModel.ForwardMessages(selectedItems);
                });
            };
            _deleteButton.Click += (sender, args) => ViewModel.DeleteMessages();

            Loaded += (sender, args) =>
            {
                SetRootFrameBinding();

                if (!ViewModel.StateService.IsEmptyBackground)
                {
                    var color = Colors.White;
                    color.A = 254;
                    SystemTray.ForegroundColor = color;
                }

                if (ViewModel.StateService.CurrentBackground != null)
                {
                    var color = Colors.White;
                    color.A = 254;
                    SystemTray.ForegroundColor = color;
                }

                if (ViewModel.With is TLBroadcastChat)
                {
                    _forwardButton.IsEnabled = false;
                }

                ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(300);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (ViewModel.StateService.FocusOnInputMessage)
                        {
                            ViewModel.StateService.FocusOnInputMessage = false;
                            InputMessage.Focus();
                        }
                    });
                });
                if (ViewModel.ChooseAttachment != null)
                {
                    ViewModel.ChooseAttachment.PropertyChanged += OnChooseAttachmentPropertyChanged;
                }
                if (ViewModel.ImageViewer != null)
                {
                    ViewModel.ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
                }
                if (ViewModel.ImageEditor != null)
                {
                    ViewModel.ImageEditor.PropertyChanged += OnImageEditorPropertyChanged;
                }
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                ViewModel.ScrollToBottom += OnViewModelScrollToBottom;

                BuildLocalizedAppBar();
                //MessageBox.Show("Time: " + timer.Elapsed);
            };

            Unloaded += (sender, args) =>
            {
                RemoveRootFrameBinding();

                if (ViewModel.ChooseAttachment != null)
                {
                    ViewModel.ChooseAttachment.PropertyChanged -= OnChooseAttachmentPropertyChanged;
                }
                if (ViewModel.ImageViewer != null)
                {
                    ViewModel.ImageViewer.PropertyChanged -= OnImageViewerPropertyChanged;
                }
                if (ViewModel.ImageEditor != null)
                {
                    ViewModel.ImageEditor.PropertyChanged -= OnImageEditorPropertyChanged;
                }
                ViewModel.ScrollToBottom -= OnViewModelScrollToBottom;
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelScrollToBottom(object sender, System.EventArgs e)
        {
            if (ViewModel.Items.Count > 0)
            {
                Items.ScrollIntoView(ViewModel.Items[0]);
            }
        }

        private void OnChooseAttachmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ChooseAttachment.IsOpen))
            {
                Items.IsHitTestVisible = !ViewModel.ChooseAttachment.IsOpen && (ViewModel.ImageViewer == null || !ViewModel.ImageViewer.IsOpen);

                return;
                // ApplicationBar скрывается в коде ChooseAttachmentView
                //if (ViewModel.ChooseAttachment.IsOpen)
                //{
                //    _prevApplicationBar = ApplicationBar;
                //    ApplicationBar = ((ChooseAttachmentView)ChooseAttachment.Content).ApplicationBar;
                //}
                //else
                //{
                //    if (_prevApplicationBar != null)
                //    {
                //        ApplicationBar = _prevApplicationBar;
                //    }
                //}
            }
        }

        private IApplicationBar _prevApplicationBar;

        private void OnImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer.IsOpen))
            {
                if (ViewModel.ImageViewer.IsOpen)
                {
                    _prevApplicationBar = ApplicationBar;
                    ApplicationBar = ((ImageViewerView)ImageViewer.Content).ApplicationBar;
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
                Items.IsHitTestVisible = !ViewModel.ImageEditor.IsOpen && (ViewModel.ChooseAttachment == null || !ViewModel.ChooseAttachment.IsOpen);

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

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ScrollToBottomVisibility))
            {
                if (ViewModel.ScrollToBottomVisibility == Visibility.Visible)
                {
                    //ShowScrollToBottomButton();
                }
                else
                {
                    //HideScrollToBottomButton();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.Reply))
            {
                if (ViewModel.Reply != null)
                {
                    if (ViewModel.Reply is TLMessagesContainter)
                    {

                    }
                    else
                    {
                        InputMessage.Focus();
                    }
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
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageEditor)
            && ViewModel.ImageEditor != null)
            {
                ViewModel.ImageEditor.PropertyChanged += OnImageEditorPropertyChanged;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.With))
            {
                ApplicationBar.IsVisible = !ViewModel.IsChatForbidden && !ViewModel.IsChooseAttachmentOpen;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.CanSend))
            {
                _sendButton.IsEnabled = ViewModel.CanSend;
            }
        }

        private void SwitchToSelectionMode()
        {
            Items.Focus();
            ViewModel.IsSelectionEnabled = true;
            EmojiPlaceholder.Visibility = Visibility.Collapsed;

            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(_forwardButton);
            ApplicationBar.Buttons.Add(_deleteButton);
        }

        private void SwitchToNormalMode()
        {
            ViewModel.IsSelectionEnabled = false;

            ApplicationBar.Buttons.Clear();

            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(_sendButton);
            ApplicationBar.Buttons.Add(_attachButton);
            ApplicationBar.Buttons.Add(_smileButton);
            ApplicationBar.Buttons.Add(_manageButton);
        }

        private bool _firstRun = true;

        private void BuildLocalizedAppBar()
        {
            if (!_firstRun) return;
            _firstRun = false;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.99;
            
            ApplicationBar.Buttons.Add(_sendButton);
            ApplicationBar.Buttons.Add(_attachButton);
            ApplicationBar.Buttons.Add(_smileButton);
            ApplicationBar.Buttons.Add(_manageButton);

            _sendButton.IsEnabled = ViewModel.CanSend;
            ApplicationBar.IsVisible = !ViewModel.IsChatForbidden && !ViewModel.IsChooseAttachmentOpen;
        }


#if DEBUG
        ///<summary>
        ///Add a finalizer to check for memory leaks
        ///</summary>
        //~DialogDetailsView()
        //{
        //    TLUtils.WritePerformance("DialogDetailsView finalizer");
        //}
#endif

        private void DialogDetailsView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (ViewModel == null) return;

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

            if (EmojiPlaceholder.Visibility == Visibility.Visible)
            {
                EmojiPlaceholder.Visibility = Visibility.Collapsed;
                e.Cancel = true;

                return;
            }

            if (ViewModel.ChooseAttachment != null
                && ViewModel.ChooseAttachment.IsOpen)
            {
                ViewModel.ChooseAttachment.Close();
                e.Cancel = true;

                return;
            }

            if (ViewModel.IsSelectionEnabled)
            {
                ViewModel.IsSelectionEnabled = false;
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

            try
            {
                if (Items.Visibility == Visibility.Visible)
                {
                    var writeableBitmap = new WriteableBitmap(Items, null);
                    ViewModel.With.SetBitmap(writeableBitmap);
                }
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("WritableBitmap exception " + ex);
            }

            RunAnimation();

            Items.DetachPropertyListener();
            ViewModel.CancelDownloading();
        }

        public void RunAnimation()
        {
            if (_isBackwardOutAnimation)
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
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ViewModel.LoadNextSlice();
        }

        //private readonly IDisposable _keyPressSubscription;

        //public void Dispose()
        //{
        //    _keyPressSubscription.Dispose();
        //}
        private void CopyMessage_OnClick(object sender, RoutedEventArgs e)
        {
            var message = ((MenuItem) sender).DataContext as TLMessage;

            ViewModel.CopyMessage(message);
        }


        private void UIElement_OnHold(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void SelectionBox_OnChecked(object sender, RoutedEventArgs e)
        {

        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardOut)
            {
                if (!string.IsNullOrEmpty(toOrFrom.OriginalString)
                    && toOrFrom.OriginalString.Contains("ShellView.xaml"))
                {
                    return null;
                }
            }

            if (animationType == AnimationType.NavigateForwardIn)
            {
                Items.Opacity = 0.0;
                Items.IsHitTestVisible = false;
                return GetContinuumAnimation(Title, animationType);
            }

            if (animationType == AnimationType.NavigateBackwardOut)
            {
                return null;
            }
            return base.GetAnimation(animationType, toOrFrom);

        }

        protected override void AnimationsComplete(AnimationType animationType)
        {

            base.AnimationsComplete(animationType);
            if (animationType == AnimationType.NavigateForwardIn)
            {
                Items.Opacity = 1.0;
                Items.IsHitTestVisible = true;
                MessagesCache.Visibility = Visibility.Collapsed;
                ViewModel.ForwardInAnimationComplete();
            }

            if (animationType == AnimationType.NavigateBackwardIn)
            {
                //ViewModel.BackwardInAnimationComplete();
            }
        }

        protected override bool IsPopupOpen()
        {
            return ViewModel.IsSelectionEnabled
                || (ViewModel.ChooseAttachment != null && ViewModel.ChooseAttachment.IsOpen)
                || (ViewModel.ImageViewer != null && ViewModel.ImageViewer.IsOpen)
                || EmojiPlaceholder.Visibility == Visibility.Visible;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.OnNavigatedTo();

            if (e.NavigationMode == NavigationMode.New)
            {
                //LayoutRoot.Opacity = 0.0;
                //_isForwardInAnimation = true;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (!_fromExternalUri)
                {
                    //LayoutRoot.Opacity = 0.0;
                    //_isBackwardInAnimation = true;
                    ViewModel.BackwardInAnimationComplete();
                }
                else
                {
                    ViewModel.BackwardInAnimationComplete();
                }
                _fromExternalUri = false;
            }
            else if (e.NavigationMode == NavigationMode.Forward && e.Uri != ExternalUri)
            {
                //_isForwardOutAnimation = true;
            }

            base.OnNavigatedTo(e);
        }

        private static readonly Uri ExternalUri = new Uri(@"app://external/");

        private bool _fromExternalUri;
        private bool _isBackwardOutAnimation;

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_emojiKeyboard != null)
            {
                // Destroy EmojiControl
                _emojiKeyboard.IsOpen = false;
                _emojiKeyboard.UnbindTextBox();
                EmojiPlaceholder.Content = null; // Remove from view
                EmojiPlaceholder.Visibility = Visibility.Collapsed;
                _emojiKeyboard = null;
            }


        //    if (e.Uri.OriginalString.EndsWith("VideoCaptureView.xaml")
        //        || e.Uri.OriginalString.EndsWith("MapView.xaml")
        //        || e.Uri.OriginalString.EndsWith("ShareContactView.xaml"))
        //    {

        //        OpenPeerDetails_OnTap(this, null);
        //    }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.OnNavigatedFrom();

            _fromExternalUri = e.Uri == ExternalUri;

            //image.Source = writeableBitmap;


            base.OnNavigatedFrom(e);
        }

        private void Items_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (ViewModel.SliceLoaded) return;

            ViewModel.LoadNextSlice();
        }

        private void InputMessage_OnGotFocus(object sender, RoutedEventArgs e)
        {
            EmojiPlaceholder.Visibility = Visibility.Visible;
            EmojiPlaceholder.Opacity = 0.0;
            EmojiPlaceholder.Height = EmojiControl.PortraitOrientationHeight;
        }

        private bool _smileButtonPressed;

        private void InputMessage_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!_smileButtonPressed)
            {
                if (EmojiPlaceholder.Visibility == Visibility.Visible)
                {
                    EmojiPlaceholder.Visibility = Visibility.Collapsed;
                }
            }
            _smileButtonPressed = false;

            if (EmojiPlaceholder.Visibility == Visibility.Collapsed)
            {
                //AudioRecorder.Visibility = Visibility.Visible;
                //DialogPhoto.Visibility = Visibility.Visible;
                //Title.Visibility = Visibility.Visible;
            }
        }


        private void UsernameHint_OnTap(object sender, GestureEventArgs e)
        {
            InputMessage.Focus();

            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var user = frameworkElement.DataContext as IUserName;
                if (user != null)
                {
                    var index = 0;
                    for (var i = InputMessage.Text.Length - 1; i >= 0; i--)
                    {
                        if (InputMessage.Text[i] == '@')
                        {
                            index = i;
                            break;
                        }
                    }

                    InputMessage.Text = string.Format("{0}{1} ", InputMessage.Text.Substring(0, index + 1), user.UserName);
                    InputMessage.SelectionStart = InputMessage.Text.Length;
                    InputMessage.SelectionLength = 0;
                }
            }
        }

        private void UsernameHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueUsernameHints();
        }
    }

    public class TLStickerItem : TLObject
    {
        public TLDocumentBase Document { get; set; }

        public TLStickerItem Self
        {
            get { return this; }
        }
    }
}