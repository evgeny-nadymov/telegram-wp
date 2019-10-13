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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Controls.LongListSelector.Common;

namespace Telegram.Controls.LongListSelector
{
    /// <summary>
    /// Partial definition of LongListSelector. Includes group view code.
    /// </summary>
    public partial class LongListSelector : Control
    {
        private PhoneApplicationPage _page;

        private bool _systemTrayVisible;
        private bool _applicationBarVisible;
        private Border _border;
        private LongListSelectorItemsControl _itemsControl;
        private Popup _groupSelectorPopup;

        private static double _screenWidth = Application.Current.Host.Content.ActualWidth;
        private static double _screenHeight = Application.Current.Host.Content.ActualHeight;

        private void OpenPopup()
        {
            SaveSystemState(true, false);
            BuildPopup();
            AttachToPageEvents();
            _groupSelectorPopup.IsOpen = true;

            // This has to happen eventually anyway, and this forces the ItemsControl to
            // expand it's template, populate it's items etc.
            UpdateLayout();
        }

        private void popup_Opened(object sender, EventArgs e)
        {
            SafeRaise.Raise(GroupViewOpened, this, () => { return new GroupViewOpenedEventArgs(_itemsControl); });
        }

        /// <summary>
        /// Closes the group popup.
        /// </summary>
        /// <param name="selectedGroup">The selected group.</param>
        /// <param name="raiseEvent">Should the GroupPopupClosing event be raised.</param>
        /// <returns>True if the event was not raised or if it was raised and e.Handled is false.</returns>
        private bool ClosePopup(object selectedGroup, bool raiseEvent)
        {
            if (raiseEvent)
            {
                GroupViewClosingEventArgs args = null;

                SafeRaise.Raise(GroupViewClosing, this, () => { return args = new GroupViewClosingEventArgs(_itemsControl, selectedGroup); });

                if (args != null && args.Cancel)
                {
                    return false;
                }
            }

            if (_groupSelectorPopup != null)
            {
                RestoreSystemState();
                _groupSelectorPopup.IsOpen = false;
                DetachFromPageEvents();
                _groupSelectorPopup.Child = null;
                _border = null;
                _itemsControl = null;
                _groupSelectorPopup = null;
            }

            return true;
        }

        private Thickness _borderPadding;

        private void BuildPopup()
        {
            _groupSelectorPopup = new Popup();
            _groupSelectorPopup.Opened += popup_Opened;

            // Support the background color jumping through. Note that the
            // alpha channel will be ignored, unless it is a purely transparent
            // value (such as when a user uses Transparent as the background
            // on the control).
            SolidColorBrush background = Background as SolidColorBrush;
            Color bg = (Color)Resources["PhoneBackgroundColor"];
            if (background != null
                && background.Color != null
                && background.Color.A > 0)
            {
                bg = background.Color;
            }
            _border = new Border
            {
                Background = new SolidColorBrush(
                    Color.FromArgb(0xa0, bg.R, bg.G, bg.B))
            };

            // Prevents touch events from bubbling up for most handlers.
            _border.ManipulationStarted += ((o, e) => e.Handled = true);
            _border.ManipulationCompleted += ((o, e) => e.Handled = true);
            _border.ManipulationDelta += ((o, e) => e.Handled = true);

            var gestureHandler = new EventHandler<System.Windows.Input.GestureEventArgs>((o, e) => e.Handled = true);
            _border.Tap += gestureHandler;
            _border.DoubleTap += gestureHandler;
            _border.Hold += gestureHandler;
            _borderPadding = _border.Padding;

            _itemsControl = new LongListSelectorItemsControl();
            _itemsControl.ItemTemplate = GroupItemTemplate;
            _itemsControl.ItemsPanel = GroupItemsPanel;
            _itemsControl.ItemsSource = ItemsSource;

            _itemsControl.GroupSelected += itemsControl_GroupSelected;

            _groupSelectorPopup.Child = _border;
            ScrollViewer sv = new ScrollViewer() { HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };

            _itemsControl.HorizontalAlignment = HorizontalAlignment.Center;
            _itemsControl.Margin = new Thickness(0, 12, 0, 0);
            _border.Child = sv;
            sv.Content = _itemsControl;

            SetItemsControlSize();
        }

        private void SetItemsControlSize()
        {
            Rect client = GetTransformedRect();
            if (_border != null)
            {
                _border.RenderTransform = GetTransform();

                _border.Width = client.Width;
                _border.Height = client.Height;
            }
        }

        private void itemsControl_GroupSelected(object sender, LongListSelector.GroupSelectedEventArgs e)
        {
            if (ClosePopup(e.Group, true))
            {
                ScrollToGroup(e.Group);
            }
        }

        private void AttachToPageEvents()
        {
            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                _page = frame.Content as PhoneApplicationPage;
                if (_page != null)
                {
                    _page.BackKeyPress += page_BackKeyPress;
                    _page.OrientationChanged += page_OrientationChanged;
                }
            }
        }

        private void DetachFromPageEvents()
        {
            if (_page != null)
            {
                _page.BackKeyPress -= page_BackKeyPress;
                _page.OrientationChanged -= page_OrientationChanged;
                _page = null;
            }
        }

        private void page_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            e.Cancel = true;
            ClosePopup(null, true);
        }

        void page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            SetItemsControlSize();
        }

        private static Rect GetTransformedRect()
        {
            bool isLandscape = IsLandscape(GetPageOrientation());

            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                var page = frame.Content as PhoneApplicationPage;

                //if (page != null)
                //{
                //    _screenHeight = page.ActualHeight;
                //    _screenWidth = page.ActualWidth;

                //    //return new Rect(0, 0, _screenWidth, _screenHeight);
                //}
                //else
                {
                    _screenWidth = Application.Current.Host.Content.ActualWidth;
                    _screenHeight = Application.Current.Host.Content.ActualHeight;
                }
                
            }
            else
            {
                _screenWidth = Application.Current.Host.Content.ActualWidth;
                _screenHeight = Application.Current.Host.Content.ActualHeight;
            }

            return new Rect(0, 0,
                isLandscape ? _screenHeight : _screenWidth,
                isLandscape ? _screenWidth : _screenHeight);
        }

        private Transform GetTransform()
        {
            PageOrientation orientation = GetPageOrientation();

            switch (orientation)
            {
                case PageOrientation.LandscapeLeft:
                case PageOrientation.Landscape:
                    _border.Padding = new Thickness(_borderPadding.Left  + 68.0, _borderPadding.Top, _borderPadding.Right, _borderPadding.Bottom);
                    return new CompositeTransform { Rotation = 90, TranslateX = _screenWidth };
                case PageOrientation.LandscapeRight:
                    _border.Padding = new Thickness(_borderPadding.Left, _borderPadding.Top, _borderPadding.Right + 68.0, _borderPadding.Bottom);
                    return new CompositeTransform { Rotation = -90, TranslateY = _screenHeight };
                default:
                    _border.Padding = _borderPadding;
                    return null;
            }
        }

        private static bool IsLandscape(PageOrientation orientation)
        {
            return orientation == PageOrientation.Landscape || orientation == PageOrientation.LandscapeLeft || orientation == PageOrientation.LandscapeRight;
        }

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

        private void SaveSystemState(bool newSystemTrayVisible, bool newApplicationBarVisible)
        {
            _systemTrayVisible = SystemTray.IsVisible;
            SystemTray.IsVisible = newSystemTrayVisible;

            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
                if (page != null && page.ApplicationBar != null)
                {
                    _applicationBarVisible = page.ApplicationBar.IsVisible;
                    page.ApplicationBar.IsVisible = newApplicationBarVisible;
                }
            }
        }

        private void RestoreSystemState()
        {
            SystemTray.IsVisible = _systemTrayVisible;

            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
                if (page != null && page.ApplicationBar != null)
                {
                    page.ApplicationBar.IsVisible = _applicationBarVisible;
                }
            }
        }
    }
}