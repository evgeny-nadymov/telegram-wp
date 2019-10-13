// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Extensions;
using TelegramClient.Services;
using TelegramClient.Themes.Default.Templates;
using TelegramClient.ViewModels.Additional;
using TelegramClient.Views.Additional;
using TelegramClient.Views.Calls;
using TelegramClient.Views.Controls;

namespace TelegramClient.Controls
{
    public class TelegramTransitionFrame : TransitionFrame
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(TelegramTransitionFrame), new PropertyMetadata(default(string)));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private UIElement _blockingProgress;

        private Border _clientArea;

        private StackPanel _blockingPanel;

        private LockscreenView _passcodePanel;

        //private TextBlock _debugInfo;

        //public TextBlock DebugInfo { get { return _debugInfo; } }

        //private ListBox _debugList;

        //public ListBox DebugList { get { return _debugList; } }

        private MediaElement _element;

        public MediaElement Element { get { return _element; } }

        private ContentControl _callPlaceholder;

        private ContentControl _blockingPlaceholder;

        private Border _player;

        public TelegramTransitionFrame()
        {
            DefaultStyleKey = typeof(TelegramTransitionFrame);

            Loaded += OnLoaded;

            Navigating += OnNavigating;

            //AddOrRemoveEventHandler();
        }

        #region Handle Software buttons

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= this.OnUnloaded;
            this.AddOrRemoveEventHandler(remove: true);
        }

        private bool AddOrRemoveEventHandler(bool remove = false)
        {
            var ei = GetType().GetEvents();
            var pi = GetType().GetProperties();

            var evInfo = GetType().GetEvent("NavigationBarVisibilityChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (evInfo != null)
            {
                var method = evInfo.AddMethod;
                if (remove)
                {
                    method = evInfo.RemoveMethod;
                }
                method.Invoke(this, new EventHandler[] { OnNavBarVisibilityChanged });
                Unloaded += OnUnloaded;
                return true;
            }
            return false;
        }

        private void UpdateMargin(double occludedHeight)
        {
            Margin = new Thickness(0, 0, 0, occludedHeight);
        }

        private void OnNavBarVisibilityChanged(object sender, System.EventArgs e)
        {
            var occludedHeightProp = e.GetType().GetProperties().SingleOrDefault(p => p.Name == "OccludedHeight");
            if (occludedHeightProp != null)
            {
                var occludedHeight = (double)occludedHeightProp.GetValue(e);
                UpdateMargin(occludedHeight);
            }
        }
        #endregion



        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_passcodePanel != null
                && _passcodePanel.Visibility == Visibility.Visible
                && e.IsCancelable)
            {
                e.Cancel = true;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _blockingProgress = GetTemplateChild("BlockingProgress") as Border;
            _clientArea = GetTemplateChild("ClientArea") as Border;
            _blockingPanel = GetTemplateChild("BlockingPanel") as StackPanel;
            _passcodePanel = GetTemplateChild("PasscodePanel") as LockscreenView;
            //_debugInfo = GetTemplateChild("DebugInfo") as TextBlock;
            _element = GetTemplateChild("Element") as MediaElement;
            if (_element != null)
            {
                _element.MediaOpened += PlayerOnMediaOpened;
                _element.MediaFailed += PlayerOnMediaFailed;
                _element.MediaEnded += PlayerOnMediaEnded;
            }
            _callPlaceholder = GetTemplateChild("CallPlaceholder") as ContentControl;
            _blockingPlaceholder = GetTemplateChild("BlockingPlaceholder") as ContentControl;
            _player = GetTemplateChild("Player") as Border;
            if (_player != null)
            {
                _player.ManipulationDelta += PlayerOnManipulationDelta;
                _player.ManipulationCompleted += PlayerOnManipulationCompleted;
            }

            //#if !DEBUG
            //if (_debugInfo != null) _debugInfo.Visibility = Visibility.Collapsed;
            //if (_debugList != null) _debugList.Visibility = Visibility.Collapsed;
            //#endif

            if (_returnToCallControl != null)
            {
                _callPlaceholder.Content = _returnToCallControl;
            }

            if (PasscodeUtils.IsLockscreenRequired)
            {
                OpenLockscreen();
            }
        }



        private void PlayerOnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_player.Visibility == Visibility.Visible)
            {
                var position = _player.TransformToVisual(Application.Current.RootVisual).Transform(new Point(80.0, 80.0));

                System.Diagnostics.Debug.WriteLine(position.X + " " + position.Y);

                var toX = 0.0;
                var toY = 0.0;

                var transform = _player.RenderTransform as TranslateTransform;
                if (transform != null)
                {
                    if (position.X < 40.0)
                    {
                        toX = transform.X - 160.0;
                    }
                    else if (position.X > 440.0)
                    {
                        toX = transform.X + 160.0;
                    }
                    else if (position.Y < 40.0)
                    {
                        toY = transform.Y - 160.0;
                    }
                    else if (position.Y > 760.0)
                    {
                        toY = transform.Y + 160.0;
                    }

                    if (Math.Abs(toY) > 0.001 || Math.Abs(toX) > 0.001)
                    {
                        var storyborad = new Storyboard();
                        if (Math.Abs(toY) > 0.001)
                        {
                            var translateYAnimaiton = new DoubleAnimation { To = toY, Duration = TimeSpan.FromSeconds(0.15), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 } };
                            Storyboard.SetTarget(translateYAnimaiton, transform);
                            Storyboard.SetTargetProperty(translateYAnimaiton, new PropertyPath("Y"));
                            storyborad.Children.Add(translateYAnimaiton);
                        }
                        else if (Math.Abs(toX) > 0.001)
                        {
                            var translateYAnimaiton = new DoubleAnimation { To = toX, Duration = TimeSpan.FromSeconds(0.15), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 } };
                            Storyboard.SetTarget(translateYAnimaiton, transform);
                            Storyboard.SetTargetProperty(translateYAnimaiton, new PropertyPath("X"));
                            storyborad.Children.Add(translateYAnimaiton);
                        }
                        storyborad.Completed += (o, args) =>
                        {
                            _player.Visibility = Visibility.Collapsed;
                            _player.RenderTransform = null;

                            PlayerOnMediaEnded(_element, new RoutedEventArgs());
                        };

                        _element.Stop();
                        Telegram.Api.Helpers.Execute.BeginOnUIThread(storyborad.Begin);
                    }
                }
            }
        }

        private void PlayerOnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var translateTransform = _player.RenderTransform as TranslateTransform;
            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform();
                _player.RenderTransform = translateTransform;
            }

            translateTransform.X += e.DeltaManipulation.Translation.X;
            translateTransform.Y += e.DeltaManipulation.Translation.Y;

            e.Handled = true;
        }

        private void PlayerOnMediaEnded(object sender, RoutedEventArgs e)
        {
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                var player = mediaElement.Tag as GifPlayerControl;
                if (player != null)
                {
                    player.OnMediaEnded();
                    return;
                }

                var audioPlayer = mediaElement.Tag as MessagePlayerControl;
                if (audioPlayer != null)
                {
                    audioPlayer.OnMediaEnded();
                    return;
                }
            }
        }

        private void PlayerOnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                var player = mediaElement.Tag as GifPlayerControl;
                if (player != null)
                {
                    player.OnMediaFailed(e);
                    return;
                }

                var audioPlayer = mediaElement.Tag as MessagePlayerControl;
                if (audioPlayer != null)
                {
                    audioPlayer.OnMediaFailed(e);
                    return;
                }
            }
        }

        private void PlayerOnMediaOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                var player = mediaElement.Tag as GifPlayerControl;
                if (player != null)
                {
                    player.OnMediaOpened();
                    return;
                }

                var audioPlayer = mediaElement.Tag as MessagePlayerControl;
                if (audioPlayer != null)
                {
                    audioPlayer.OnMediaOpened();
                    return;
                }
            }
        }

        private TranslateTransform _frameTransform;

        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register(
            "RootFrameTransformProperty", typeof(double), typeof(TelegramTransitionFrame), new PropertyMetadata(OnRootFrameTransformChanged));

        private static void OnRootFrameTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as TelegramTransitionFrame;
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

        private IList<object> _buttons = new List<object>();
        private IList<object> _menuItems = new List<object>();

        private bool _removeApplicationBar;
        private double _previousOpacity;
        private Color _previousColor;
        private bool _isSystemTrayVisible;

        public bool IsPasscodeActive
        {
            get { return _passcodePanel != null && _passcodePanel.Visibility == Visibility.Visible; }
        }

        private bool _stateExists;

        public void OpenLockscreen()
        {
            if (_passcodePanel != null && _clientArea != null)
            {
                if (_passcodePanel.DataContext == null)
                {
                    var viewModel = new LockscreenViewModel();
                    _passcodePanel.DataContext = viewModel;
                    viewModel.PasscodeIncorrect += _passcodePanel.OnPasscodeIncorrect;
                }
                _passcodePanel.Visibility = Visibility.Visible;
                var page = Content as PhoneApplicationPage;
                if (page != null)
                {
                    _passcodePanel.ParentPage = page;
                    SetRootFrameBinding();
                    page.IsHitTestVisible = false;

                    if (!_stateExists)
                    {
                        _stateExists = true;
                        _isSystemTrayVisible = SystemTray.IsVisible;
                        SystemTray.IsVisible = false;
                        if (page.ApplicationBar != null)
                        {
                            if (_buttons.Count == 0)
                            {
                                foreach (var button in page.ApplicationBar.Buttons)
                                {
                                    _buttons.Add(button);
                                }
                            }

                            if (_menuItems.Count == 0)
                            {
                                foreach (var menuItem in page.ApplicationBar.MenuItems)
                                {
                                    _menuItems.Add(menuItem);
                                }
                            }
                            //page.ApplicationBar.IsVisible = false;
                            page.ApplicationBar.Buttons.Clear();
                            page.ApplicationBar.MenuItems.Clear();
                        }
                        else
                        {
                            page.ApplicationBar = new ApplicationBar();
                            //page.ApplicationBar.IsVisible = false;
                            _removeApplicationBar = true;
                        }
                        _previousColor = page.ApplicationBar.BackgroundColor;
                        _previousOpacity = page.ApplicationBar.Opacity;
                        page.ApplicationBar.Opacity = 1.0;
                        page.ApplicationBar.BackgroundColor = Colors.Transparent;
                    }
                }
                _passcodePanel.FocusPasscode();
            }
        }

        public void CloseLockscreen()
        {
            if (_passcodePanel != null && _clientArea != null)
            {
                _passcodePanel.Visibility = Visibility.Collapsed;

                var page = Content as PhoneApplicationPage;
                if (page != null)
                {
                    RemoveRootFrameBinding();
                    page.IsHitTestVisible = true;
                    _stateExists = false;
                    SystemTray.IsVisible = _isSystemTrayVisible;
                    if (_removeApplicationBar)
                    {
                        page.ApplicationBar = null;
                        _removeApplicationBar = false;
                    }

                    if (page.ApplicationBar != null)
                    {
                        page.ApplicationBar.Buttons.Clear();
                        page.ApplicationBar.MenuItems.Clear();
                        foreach (var button in _buttons)
                        {
                            page.ApplicationBar.Buttons.Add(button);
                        }
                        foreach (var menuItem in _menuItems)
                        {
                            page.ApplicationBar.MenuItems.Add(menuItem);
                        }

                        _buttons.Clear();
                        _menuItems.Clear();

                        page.ApplicationBar.BackgroundColor = _previousColor;
                        page.ApplicationBar.Opacity = _previousOpacity;
                        //page.ApplicationBar.IsVisible = true;
                    }
                }
            }
        }

        public bool IsLockScreenOpen()
        {
            return _passcodePanel != null && _passcodePanel.Visibility == Visibility.Visible;
        }

        public void OpenBlockingProgress()
        {
            if (_blockingProgress != null && _clientArea != null)
            {
                _clientArea.IsHitTestVisible = false;
                _blockingProgress.Visibility = Visibility.Visible;
                _blockingPanel.Visibility = Visibility.Visible;
            }
        }

        public void CloseBlockingProgress()
        {
            if (_blockingProgress != null && _clientArea != null)
            {
                _clientArea.IsHitTestVisible = true;
                _blockingProgress.Visibility = Visibility.Collapsed;
                _blockingPanel.Visibility = Visibility.Collapsed;
            }
        }

        public bool IsBlockingProgressOpen()
        {
            return _blockingProgress != null && _blockingProgress.Visibility == Visibility.Visible;
        }

        private ReturnToCallControl _returnToCallControl;

        public ContentControl CallPlaceholder
        {
            get { return _callPlaceholder; }
        }

        public void ShowCallPlaceholder(System.Action callback)
        {
            if (_callPlaceholder == null
                || _callPlaceholder.Content != null)
            {
                _returnToCallControl = new ReturnToCallControl();
                _returnToCallControl.Tap += (o, e) =>
                {
                    callback.SafeInvoke();
                };
                return;
            }

            var returnToCallControl = new ReturnToCallControl();
            returnToCallControl.Tap += (o, e) =>
            {
                callback.SafeInvoke();
            };

            _callPlaceholder.Content = returnToCallControl;
        }

        public void HideCallPlaceholder()
        {
            _returnToCallControl = null;
            if (_callPlaceholder != null)
            {
                _callPlaceholder.Content = null;
            }
        }

        private UpdateAppControl _updateAppControl;

        public ContentControl BlockingPlaceholder
        {
            get { return _blockingPlaceholder; }
        }

        public void ShowBlockingPlaceholder(System.Action callback)
        {
            var updateAppControl = new UpdateAppControl();
            updateAppControl.TapBottomMenu += (o, e) =>
            {
                callback.SafeInvoke();
            };

            _blockingPlaceholder.Content = updateAppControl;
            _blockingPlaceholder.Visibility = Visibility.Visible;
        }

        public void HideBlockingPlaceholder()
        {
            _updateAppControl = null;
            if (_blockingPlaceholder != null)
            {
                _blockingPlaceholder.Content = null;
                _blockingPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        public bool IsPlayerVisible
        {
            get { return _player != null && _player.Visibility == Visibility.Visible; }
        }

        public void ShowPlayer(Brush brush)
        {
            if (_player == null) return;

            _player.Visibility = Visibility.Visible;
            _player.Background = brush;
        }

        public void HidePlayer()
        {
            _player.Visibility = Visibility.Collapsed;
            _player.Background = null;
        }
    }
}
