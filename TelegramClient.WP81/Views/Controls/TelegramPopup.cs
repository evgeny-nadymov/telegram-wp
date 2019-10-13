// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TelegramClient.Views.Controls
{
    public class TelegramMessageBox : ContentControl
    {
        /// <summary>
        /// Holds a weak reference to the message box that is currently displayed.
        /// </summary>
        private static WeakReference _currentInstance;

        /// <summary>
        /// The current screen width.
        /// </summary>
        private static readonly double _screenWidth = Application.Current.Host.Content.ActualWidth;

        /// <summary>
        /// The current screen height.
        /// </summary>
        private static readonly double _screenHeight = Application.Current.Host.Content.ActualHeight;

        /// <summary>
        /// The height of the system tray in pixels when the page
        /// is in portrait mode.
        /// </summary>
        private const double _systemTrayHeightInPortrait = 32.0;

        /// <summary>
        /// The width of the system tray in pixels when the page
        /// is in landscape mode.
        /// </summary>
        private const double _systemTrayWidthInLandscape = 72.0;

        /// <summary>
        /// Identifies whether the application bar and the system tray
        /// must be restored after the message box is dismissed. 
        /// </summary>
        private static bool _mustRestore = true;

        /// <summary>
        /// Left button template part.
        /// </summary>
        private Button _leftButton;

        /// <summary>
        /// Right button template part.
        /// </summary>
        private Button _rightButton;

        /// <summary>
        /// The popup used to display the message box.
        /// </summary>
        private Popup _popup;

        /// <summary>
        /// The child container of the popup.
        /// </summary>
        private Grid _container;

        /// <summary>
        /// The root visual of the application.
        /// </summary>
        private PhoneApplicationFrame _frame;

        /// <summary>
        /// The current application page.
        /// </summary>
        private PhoneApplicationPage _page;

        /// <summary>
        /// Identifies whether the application bar is visible or not before
        /// opening the message box.
        /// </summary>
        private bool _hasApplicationBar;

        /// <summary>
        /// The current color of the system tray.
        /// </summary>
        private Color _systemTrayColor;

        /// <summary>
        /// Called when the message is being dismissing.
        /// </summary>
        public event EventHandler<DismissingEventArgs> Dismissing;

        /// <summary>
        /// Called when the message box is dismissed.
        /// </summary>
        public event EventHandler<DismissedEventArgs> Dismissed;

        #region IsFullScreen DependencyProperty

        /// <summary>
        /// Gets or sets whether the message box occupies the whole screen.
        /// </summary>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        /// <summary>
        /// Identifies the IsFullScreen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(TelegramMessageBox), new PropertyMetadata(false, OnIsFullScreenPropertyChanged));

        /// <summary>
        /// Modifies the vertical alignment of the message box depending
        /// on whether it should occupy the full screen or not.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnIsFullScreenPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var target = (TelegramMessageBox)obj;

            if ((bool)e.NewValue)
            {
                target.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                target.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        #endregion

        /// <summary>
        /// Called when the back key is pressed. This event handler cancels
        /// the backward navigation and dismisses the message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void OnBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Dismiss(CustomMessageBoxResult.None, true);
        }

        /// <summary>
        /// Called when the application frame is navigating.
        /// This event handler dismisses the message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void OnNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Dismiss(CustomMessageBoxResult.None, false);
        }

        /// <summary>
        /// Initializes a new instance of the CustomMessageBox class.
        /// </summary>
        public TelegramMessageBox()
            : base()
        {
            //DefaultStyleKey = typeof(TelegramMessageBox);
        }

        /// <summary>
        /// Reveals the message box by inserting it into a popup and opening it.
        /// </summary>
        public void Show()
        {
            if (_popup != null)
            {
                if (_popup.IsOpen)
                {
                    return;
                }
            }

            LayoutUpdated += CustomMessageBox_LayoutUpdated;

            _frame = Application.Current.RootVisual as PhoneApplicationFrame;
            _page = _frame.Content as PhoneApplicationPage;

            // Change the color of the system tray if necessary.
            if (SystemTray.IsVisible)
            {
                // Cache the original color of the system tray.
                _systemTrayColor = SystemTray.BackgroundColor;

                // Change the color of the system tray to match the message box.
                if (Background is SolidColorBrush)
                {
                    SystemTray.BackgroundColor = ((SolidColorBrush)Background).Color;
                }
                else
                {
                    SystemTray.BackgroundColor = (Color)Application.Current.Resources["PhoneChromeColor"];
                }
            }

            // Hide the application bar if necessary.
            if (_page.ApplicationBar != null)
            {
                // Cache the original visibility of the system tray.
                _hasApplicationBar = _page.ApplicationBar.IsVisible;

                // Hide it.
                if (_hasApplicationBar)
                {
                    _page.ApplicationBar.IsVisible = false;
                }
            }
            else
            {
                _hasApplicationBar = false;
            }

            // Dismiss the current message box if there is any.
            if (_currentInstance != null)
            {
                _mustRestore = false;

                var target = _currentInstance.Target as TelegramMessageBox;

                if (target != null)
                {
                    _systemTrayColor = target._systemTrayColor;
                    _hasApplicationBar = target._hasApplicationBar;
                    target.Dismiss();
                }
            }

            _mustRestore = true;

            // Insert the overlay.
            Rectangle overlay = new Rectangle();
            Color backgroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            overlay.Fill = new SolidColorBrush(Color.FromArgb(0x99, backgroundColor.R, backgroundColor.G, backgroundColor.B));
            _container = new Grid();
            //_container.Children.Add(overlay);

            // Insert the message box.
            _container.Children.Add(this);

            // Create and open the popup.
            _popup = new Popup();
            _popup.Child = _container;
            SetSizeAndOffset();
            _popup.IsOpen = true;
            _currentInstance = new WeakReference(this);

            // Attach event handlers.
            if (_page != null)
            {
                _page.BackKeyPress += OnBackKeyPress;
                _page.OrientationChanged += OnOrientationChanged;
            }

            if (_frame != null)
            {
                _frame.Navigating += OnNavigating;
            }
        }

        /// <summary>
        /// Dismisses the message box.
        /// </summary>
        public void Dismiss()
        {
            Dismiss(CustomMessageBoxResult.None, true);
        }

        /// <summary>
        /// Dismisses the message box.
        /// </summary>
        /// <param name="source">
        /// The source that caused the dismission.
        /// </param>
        /// <param name="useTransition">
        /// If true, the message box is dismissed after swiveling
        /// backward and out.
        /// </param>
        private void Dismiss(CustomMessageBoxResult source, bool useTransition)
        {
            // Handle the dismissing event.
            var handlerDismissing = Dismissing;
            if (handlerDismissing != null)
            {
                DismissingEventArgs args = new DismissingEventArgs(source);
                handlerDismissing(this, args);

                if (args.Cancel)
                {
                    return;
                }
            }

            // Handle the dismissed event.
            var handlerDismissed = Dismissed;
            if (handlerDismissed != null)
            {
                DismissedEventArgs args = new DismissedEventArgs(source);
                handlerDismissed(this, args);
            }

            // Set the current instance to null.
            _currentInstance = null;

            // Cache this variable to avoid a race condition.
            bool restoreOriginalValues = _mustRestore;

            // Close popup.
            if (useTransition)
            {
                SwivelTransition backwardOut = new SwivelTransition { Mode = SwivelTransitionMode.BackwardOut };
                ITransition swivelTransition = backwardOut.GetTransition(this);
                swivelTransition.Completed += (s, e) =>
                {
                    swivelTransition.Stop();
                    ClosePopup(restoreOriginalValues);
                };
                Deployment.Current.Dispatcher.BeginInvoke(swivelTransition.Begin);
            }
            else
            {
                ClosePopup(restoreOriginalValues);
            }
        }

        /// <summary>
        /// Closes the pop up.
        /// </summary>
        private void ClosePopup(bool restoreOriginalValues)
        {
            if (_popup == null) return;

            // Remove the popup.
            _popup.IsOpen = false;
            _popup = null;

            if (_container != null)
            {
                _container.Children.Remove(this);
            }

            // If there is no other message box displayed.  
            if (restoreOriginalValues)
            {
                // Set the system tray back to its original 
                // color if necessary.
                if (SystemTray.IsVisible)
                {
                    SystemTray.BackgroundColor = _systemTrayColor;
                }

                // Bring the application bar if necessary.
                if (_hasApplicationBar)
                {
                    _hasApplicationBar = false;
                    _page.ApplicationBar.IsVisible = true;
                }
            }

            // Dettach event handlers.
            if (_page != null)
            {
                _page.BackKeyPress -= OnBackKeyPress;
                _page.OrientationChanged -= OnOrientationChanged;
                _page = null;
            }

            if (_frame != null)
            {
                _frame.Navigating -= OnNavigating;
                _frame = null;
            }
        }

        /// <summary>
        /// Called when the visual layout changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void CustomMessageBox_LayoutUpdated(object sender, System.EventArgs e)
        {
            SwivelTransition backwardIn = new SwivelTransition { Mode = SwivelTransitionMode.BackwardIn };
            ITransition swivelTransition = backwardIn.GetTransition(this);
            swivelTransition.Completed += (s1, e1) => swivelTransition.Stop();
            swivelTransition.Begin();
            LayoutUpdated -= CustomMessageBox_LayoutUpdated;
        }

        /// <summary>
        /// Called when the current page changes orientation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event information.</param>
        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            return;
            SetSizeAndOffset();
        }

        /// <summary>
        /// Sets The vertical and horizontal offset of the popup,
        /// as well as the size of its child container.
        /// </summary>
        private void SetSizeAndOffset()
        {
            // Set the size the container.
            Rect client = GetTransformedRect();
            if (_container != null)
            {
                _container.RenderTransform = GetTransform();

                _container.Width = client.Width;
                _container.Height = client.Height;
            }

            // Set the vertical and horizontal offset of the popup.
            if (SystemTray.IsVisible && _popup != null)
            {
                PageOrientation orientation = GetPageOrientation();

                switch (orientation)
                {
                    case PageOrientation.PortraitUp:
                        _popup.HorizontalOffset = 0.0;
                        _popup.VerticalOffset = _systemTrayHeightInPortrait;
                        _container.Height -= _systemTrayHeightInPortrait;
                        break;
                    case PageOrientation.LandscapeLeft:
                        _popup.HorizontalOffset = 0.0;
                        _popup.VerticalOffset = _systemTrayWidthInLandscape;
                        break;
                    case PageOrientation.LandscapeRight:
                        _popup.HorizontalOffset = 0.0;
                        _popup.VerticalOffset = 0.0;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets a rectangle that occupies the entire page.
        /// </summary>
        /// <returns>The width, height and location of the rectangle.</returns>
        private static Rect GetTransformedRect()
        {
            bool isLandscape = IsLandscape(GetPageOrientation());

            return new Rect(0, 0,
                isLandscape ? _screenHeight : _screenWidth,
                isLandscape ? _screenWidth : _screenHeight);
        }

        /// <summary>
        /// Determines whether the orientation is landscape.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <returns>True if the orientation is landscape.</returns>
        private static bool IsLandscape(PageOrientation orientation)
        {
            return (orientation == PageOrientation.Landscape) || (orientation == PageOrientation.LandscapeLeft) || (orientation == PageOrientation.LandscapeRight);
        }

        /// <summary>
        /// Gets a transform for popup elements based
        /// on the current page orientation.
        /// </summary>
        /// <returns>
        /// A composite transform determined by page orientation.
        /// </returns>
        private static Transform GetTransform()
        {
            PageOrientation orientation = GetPageOrientation();

            switch (orientation)
            {
                case PageOrientation.LandscapeLeft:
                case PageOrientation.Landscape:
                    return new CompositeTransform() { Rotation = 90, TranslateX = _screenWidth };
                case PageOrientation.LandscapeRight:
                    return new CompositeTransform() { Rotation = -90, TranslateY = _screenHeight };
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the current page orientation.
        /// </summary>
        /// <returns>
        /// The current page orientation.
        /// </returns>
        private static PageOrientation GetPageOrientation()
        {
            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (frame != null)
            {
                PhoneApplicationPage page = frame.Content as PhoneApplicationPage;

                if (page != null)
                {
                    return page.Orientation;
                }
            }

            return PageOrientation.None;
        }
    }
}
