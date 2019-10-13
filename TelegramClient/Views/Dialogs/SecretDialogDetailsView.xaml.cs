using System;
using System.ComponentModel;
using System.Diagnostics;
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
using TelegramClient.Views.Controls;
using TelegramClient.Views.Media;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class SecretDialogDetailsView
    {
        public SecretDialogDetailsViewModel ViewModel
        {
            get { return DataContext as SecretDialogDetailsViewModel; }
        }

        private readonly Stopwatch _timer;

        #region ApplicationBar
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
            ApplicationBar.IsVisible = ViewModel.IsApplicationBarVisible && !ViewModel.IsChooseAttachmentOpen;
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

        public SecretDialogDetailsView()
        {
            _timer = System.Diagnostics.Stopwatch.StartNew();

            InitializeComponent();

            _sendButton.Click += (sender, args) => ViewModel.Send();
            _attachButton.Click += (sender, args) =>
            {
                EmojiPlaceholder.Visibility = Visibility.Collapsed;
                Self.Focus();
                ViewModel.Attach();
            };
            _manageButton.Click += (sender, args) => SwitchToSelectionMode();
            _deleteButton.Click += (sender, args) =>
            {
                ViewModel.DeleteMessages();
                SwitchToNormalMode();
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
                        Items.Focus(); 
                        
                        EmojiPlaceholder.Opacity = 1.0;
                        EmojiPlaceholder.Height = EmojiControl.PortraitOrientationHeight;
                    }
                    else
                    {
                        EmojiPlaceholder.Visibility = Visibility.Collapsed;

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
                            EmojiPlaceholder.Visibility = Visibility.Visible;
                        });
                    });
                }
            };

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void SwitchToSelectionMode()
        {
            Items.Focus();
            ViewModel.IsSelectionEnabled = true;
            //EmojiPlaceholder.Visibility = Visibility.Collapsed;


            ApplicationBar.Buttons.Clear();
            //ApplicationBar.Buttons.Add(_forwardButton);
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

            if (!ViewModel.StateService.IsEmptyBackground)
            {
                var color = Colors.White;
                color.A = 254;
                SystemTray.ForegroundColor = color;
            }

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            ViewModel.ScrollToBottom += OnViewModelScrollToBottom;

            if (ViewModel.IsApplicationBarVisible)
            {
                BuildLocalizedAppBar();
            }
            else if (ApplicationBar != null)
            {
                ApplicationBar.IsVisible = ViewModel.IsApplicationBarVisible && !ViewModel.IsChooseAttachmentOpen;
            }

            RunAnimation();
            Stopwatch = _timer.Elapsed.ToString();
        }

        private void OnViewModelScrollToBottom(object sender, System.EventArgs e)
        {
            if (ViewModel.Items.Count > 0)
            {
                Items.ScrollIntoView(ViewModel.Items[0]);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            RemoveRootFrameBinding();

            ViewModel.ScrollToBottom -= OnViewModelScrollToBottom;
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        private Binding _visibilityBinding;

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

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsApplicationBarVisible)
                && ViewModel.IsApplicationBarVisible)
            {
                BuildLocalizedAppBar();
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
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsApplicationBarVisible))
            {
                ApplicationBar.IsVisible = ViewModel.IsApplicationBarVisible && !ViewModel.IsChooseAttachmentOpen;
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.CanSend))
            {
                _sendButton.IsEnabled = ViewModel.CanSend;
            }
        }

        private IApplicationBar _prevApplicationBar;

        private void OnImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer.IsOpen))
            {
                Items.IsHitTestVisible = !ViewModel.ChooseAttachment.IsOpen && (ViewModel.ImageViewer == null || !ViewModel.ImageViewer.IsOpen);

                if (ViewModel.ImageViewer != null
                    && ViewModel.ImageViewer.IsOpen)
                {
                    _prevApplicationBar = ApplicationBar;
                    ContentPanel.Visibility = Visibility.Collapsed;
                    ApplicationBar = ((DecryptedImageViewerView)ImageViewer.Content).ApplicationBar;
                }
                else
                {
                    ContentPanel.Visibility = Visibility.Visible;
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
                Items.IsHitTestVisible = !ViewModel.ChooseAttachment.IsOpen && (ViewModel.ImageViewer == null || !ViewModel.ImageViewer.IsOpen);
            }
        }

        private bool _isForwardInAnimation;
        private bool _isBackwardInAnimation;
        private bool _isBackwardOutAnimation;
        private bool _fromExternalUri;
        private readonly Uri _externalUri = new Uri(@"app://external/");

        private void RunAnimation()
        {
            if (_isForwardInAnimation)
            {
                _isForwardInAnimation = false;

                var storyboard = new Storyboard();
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
                    Items.Visibility = Visibility.Collapsed;
                    storyboard.Completed += (o, e) =>
                    {
                        MessagesCache.Visibility = Visibility.Collapsed;
                        Items.Visibility = Visibility.Visible;
                        ViewModel.OnForwardInAnimationComplete();
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
        }

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
                if (_fromExternalUri)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        Thread.Sleep(500);
                        Deployment.Current.Dispatcher.BeginInvoke(() => ViewModel.OnBackwardInAnimationComplete());
                    });
                }
                else
                {
                    _isBackwardInAnimation = true;
                }
                _fromExternalUri = false;
            }

            base.OnNavigatedTo(e);
        }

        public static readonly DependencyProperty BitmapProperty =
            DependencyProperty.Register("Bitmap", typeof(WriteableBitmap), typeof(SecretDialogDetailsView), new PropertyMetadata(default(WriteableBitmap)));

        public WriteableBitmap Bitmap
        {
            get { return (WriteableBitmap)GetValue(BitmapProperty); }
            set { SetValue(BitmapProperty, value); }
        }

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

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.OnNavigatedFrom();
            //MediaControl.Content = null;
            //MessagePlayerControl.Stop();

            if (e.Uri == _externalUri)
            {
                _fromExternalUri = true;
            }
            else
            {
                _fromExternalUri = false;
                //ThreadPool.QueueUserWorkItem(state =>
                {
                    //Thread.Sleep(100);
                    //Execute.BeginOnUIThread(() =>
                    {
                        var timer = new Stopwatch();
                        timer.Start();
                        var width = (int)Items.RenderSize.Width;
                        var height = (int)Items.RenderSize.Height;
                        var writeableBitmap = new WriteableBitmap(width, height);
                        writeableBitmap.Render(Items, new CompositeTransform { ScaleX = 1.0, ScaleY = 1.0 });
                        writeableBitmap.Invalidate();
                        Bitmap = writeableBitmap;
                        MessagesCache.Visibility = Visibility.Visible;
                        Items.Visibility = Visibility.Collapsed;
                        var elapsed = timer.Elapsed;
                    }//);
                }//);
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
            if (ViewModel.ImageViewer != null
                && ViewModel.ImageViewer.IsOpen)
            {
                ViewModel.ImageViewer.CloseViewer();
                e.Cancel = true;

                return;
            }

            if (ViewModel.IsSelectionEnabled)
            {
                SwitchToNormalMode();
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
                e.Cancel = true;
                ViewModel.ChooseAttachment.Close();
                return;
            }

            _isBackwardOutAnimation = true;

            try
            {
                if (Items.Visibility == Visibility.Visible)
                {
                    var writeableBitmap = new WriteableBitmap(Items, null);
                    ViewModel.Chat.SetBitmap(writeableBitmap);
                }
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("WritableBitmap exception " + ex);
            }
            MessagesCache.Visibility = Visibility.Visible;
            Items.Visibility = Visibility.Collapsed;

            RunAnimation();
        }

        private void NavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            if (_isBackwardInAnimation)
            {
                _isBackwardInAnimation = false;

                Items.Visibility = Visibility.Visible;
                MessagesCache.Visibility = Visibility.Collapsed;
                ViewModel.OnBackwardInAnimationComplete();
            }
        }

        private void UIElement_OnHold(object sender, GestureEventArgs e)
        {

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

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            
        }

        private void InputMessage_OnGotFocus(object sender, RoutedEventArgs e)
        {
            EmojiPlaceholder.Visibility = Visibility.Visible;
            EmojiPlaceholder.Opacity = 0.0;
            EmojiPlaceholder.Height = EmojiControl.PortraitOrientationHeight;

            if (ViewModel.Items.Count == 0)
            {
                Description.Visibility = Visibility.Collapsed;
            }
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

            if (ViewModel.Items.Count == 0)
            {
                Description.Visibility = Visibility.Visible;
            }
        }

        private void NavigationOutTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            //Items.Visibility = Visibility.Collapsed;
        }

        private void SecretPhotoPlaceholder_OnElapsed(object sender, System.EventArgs e)
        {
            var control = sender as FrameworkElement;
            if (control == null) return;
            ViewModel.DeleteMessage(control.DataContext as TLDecryptedMessageMediaPhoto);
            SecretImageViewer.Visibility = Visibility.Collapsed;
            //MessageBox.Show("Elapsed");
        }

        private void SecretPhotoPlaceholder_OnStartTimer(object sender, System.EventArgs e)
        {
            var uielement = sender as FrameworkElement;
            if (uielement != null)
            {
                var decryptedMessage = uielement.DataContext as TLDecryptedMessageMediaPhoto;
                if (decryptedMessage != null)
                {
                    var result = ViewModel.OpenSecretPhoto(decryptedMessage);
                    if (result)
                    {
                        SecretImageViewer.Visibility = Visibility.Visible;
                        ApplicationBar.IsVisible = false;
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

        private void SecretImageViewer_OnMouseLeave(object sender, MouseEventArgs e)
        {
            SecretImageViewer.Visibility = Visibility.Collapsed;
            ApplicationBar.IsVisible = true;
        }

        private void SecretImageViewer_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SecretImageViewer.Visibility = Visibility.Collapsed;
            ApplicationBar.IsVisible = true;
        }

        private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            ViewModel.DeleteMessage(element.DataContext as TLDecryptedMessage);
        }

        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            ViewModel.CopyMessage(element.DataContext as TLDecryptedMessage);
        }

        private void Items_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (ViewModel.SliceLoaded) return;

            ViewModel.LoadNextSlice();
        }
    }
}