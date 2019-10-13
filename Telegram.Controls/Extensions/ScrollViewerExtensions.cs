// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Telegram.Controls.Extensions
    {
        /// <summary>
        /// Provides useful extensions to ScrollViewer instances.
        /// 
        /// </summary>
        /// <QualityBand>Experimental</QualityBand>
        public static class ScrollViewerExtensions
        {
            /// <summary>
            /// The amount to scroll a ScrollViewer for a line change.
            /// 
            /// </summary>
            private const double LineChange = 16.0;
            /// <summary>
            /// Identifies the IsMouseWheelScrollingEnabled dependency property.
            /// 
            /// </summary>
            public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty;
            /// <summary>
            /// Identifies the VerticalOffset dependency property.
            /// 
            /// </summary>
            private static readonly DependencyProperty VerticalOffsetProperty;
            /// <summary>
            /// Identifies the HorizontalOffset dependency property.
            /// 
            /// </summary>
            private static readonly DependencyProperty HorizontalOffsetProperty;

            static ScrollViewerExtensions()
            {
                // ISSUE: method pointer
                ScrollViewerExtensions.IsMouseWheelScrollingEnabledProperty = DependencyProperty.RegisterAttached("IsMouseWheelScrollingEnabled", typeof(bool), typeof(ScrollViewerExtensions), new PropertyMetadata((object)false, new PropertyChangedCallback(OnIsMouseWheelScrollingEnabledPropertyChanged)));
                // ISSUE: method pointer
                ScrollViewerExtensions.VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerExtensions), new PropertyMetadata(new PropertyChangedCallback(OnVerticalOffsetPropertyChanged)));
                // ISSUE: method pointer
                ScrollViewerExtensions.HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerExtensions), new PropertyMetadata(new PropertyChangedCallback(OnHorizontalOffsetPropertyChanged)));
            }

            /// <summary>
            /// Gets a value indicating whether the ScrollViewer has enabled
            ///             scrolling via the mouse wheel.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param>
            /// <returns>
            /// A value indicating whether the ScrollViewer has enabled scrolling
            ///             via the mouse wheel.
            /// 
            /// </returns>
            public static bool GetIsMouseWheelScrollingEnabled(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                else
                    return (bool)viewer.GetValue(ScrollViewerExtensions.IsMouseWheelScrollingEnabledProperty);
            }

            /// <summary>
            /// Sets a value indicating whether the ScrollViewer will enable
            ///             scrolling via the mouse wheel.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><param name="value">A value indicating whether the ScrollViewer will enable scrolling
            ///             via the mouse wheel.
            ///             </param>
            public static void SetIsMouseWheelScrollingEnabled(this ScrollViewer viewer, bool value)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                viewer.SetValue(ScrollViewerExtensions.IsMouseWheelScrollingEnabledProperty, (value ? 1 : 0));
            }

            /// <summary>
            /// IsMouseWheelScrollingEnabledProperty property changed handler.
            /// 
            /// </summary>
            /// <param name="d">ScrollViewerExtensions that changed its IsMouseWheelScrollingEnabled.</param><param name="e">Event arguments.</param>
            private static void OnIsMouseWheelScrollingEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ScrollViewer scrollViewer = d as ScrollViewer;
                if ((bool)e.NewValue)
                {
                    // ISSUE: method pointer
                    scrollViewer.MouseWheel += new MouseWheelEventHandler(OnMouseWheel);
                }
                else
                {
                    // ISSUE: method pointer
                    scrollViewer.MouseWheel -= new MouseWheelEventHandler(OnMouseWheel);
                }
            }

            /// <summary>
            /// Handles the mouse wheel event.
            /// 
            /// </summary>
            /// <param name="sender">The ScrollViewer.</param><param name="e">Event arguments.</param>
            private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
            {
                ScrollViewer viewer = sender as ScrollViewer;
                Debug.Assert(viewer != null, "sender should be a non-null ScrollViewer!");
                Debug.Assert(e != null, "e should not be null!");
                if (e.Handled)
                    return;
                double offset = ScrollViewerExtensions.CoerceVerticalOffset(viewer, viewer.VerticalOffset - (double)e.Delta);
                viewer.ScrollToVerticalOffset(offset);
                e.Handled = true;
            }

            /// <summary>
            /// Gets the value of the VerticalOffset attached property for a specified ScrollViewer.
            /// 
            /// </summary>
            /// <param name="element">The ScrollViewer from which the property value is read.</param>
            /// <returns>
            /// The VerticalOffset property value for the ScrollViewer.
            /// </returns>
            private static double GetVerticalOffset(ScrollViewer element)
            {
                if (element == null)
                    throw new ArgumentNullException("element");
                else
                    return (double)element.GetValue(ScrollViewerExtensions.VerticalOffsetProperty);
            }

            /// <summary>
            /// Sets the value of the VerticalOffset attached property to a specified ScrollViewer.
            /// 
            /// </summary>
            /// <param name="element">The ScrollViewer to which the attached property is written.</param><param name="value">The needed VerticalOffset value.</param>
            private static void SetVerticalOffset(ScrollViewer element, double value)
            {
                if (element == null)
                    throw new ArgumentNullException("element");
                element.SetValue(ScrollViewerExtensions.VerticalOffsetProperty, (object)value);
            }

            /// <summary>
            /// VerticalOffsetProperty property changed handler.
            /// 
            /// </summary>
            /// <param name="dependencyObject">ScrollViewer that changed its VerticalOffset.</param><param name="eventArgs">Event arguments.</param>
            private static void OnVerticalOffsetPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
            {
                ScrollViewer scrollViewer = dependencyObject as ScrollViewer;
                if (scrollViewer == null)
                    throw new ArgumentNullException("dependencyObject");
                scrollViewer.ScrollToVerticalOffset((double)eventArgs.NewValue);
            }

            /// <summary>
            /// Gets the value of the HorizontalOffset attached property for a specified ScrollViewer.
            /// 
            /// </summary>
            /// <param name="element">The ScrollViewer from which the property value is read.</param>
            /// <returns>
            /// The HorizontalOffset property value for the ScrollViewer.
            /// </returns>
            private static double GetHorizontalOffset(ScrollViewer element)
            {
                if (element == null)
                    throw new ArgumentNullException("element");
                else
                    return (double)element.GetValue(ScrollViewerExtensions.HorizontalOffsetProperty);
            }

            /// <summary>
            /// Sets the value of the HorizontalOffset attached property to a specified ScrollViewer.
            /// 
            /// </summary>
            /// <param name="element">The ScrollViewer to which the attached property is written.</param><param name="value">The needed HorizontalOffset value.</param>
            private static void SetHorizontalOffset(ScrollViewer element, double value)
            {
                if (element == null)
                    throw new ArgumentNullException("element");
                element.SetValue(ScrollViewerExtensions.HorizontalOffsetProperty, (object)value);
            }

            /// <summary>
            /// HorizontalOffsetProperty property changed handler.
            /// 
            /// </summary>
            /// <param name="dependencyObject">ScrollViewer that changed its HorizontalOffset.</param><param name="eventArgs">Event arguments.</param>
            private static void OnHorizontalOffsetPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
            {
                ScrollViewer scrollViewer = dependencyObject as ScrollViewer;
                if (scrollViewer == null)
                    throw new ArgumentNullException("dependencyObject");
                scrollViewer.ScrollToHorizontalOffset((double)eventArgs.NewValue);
            }

            /// <summary>
            /// Coerce a vertical offset to fall within the vertical bounds of a
            ///             ScrollViewer.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><param name="offset">The vertical offset to coerce.</param>
            /// <returns>
            /// The coerced vertical offset that falls within the ScrollViewer's
            ///             vertical bounds.
            /// 
            /// </returns>
            private static double CoerceVerticalOffset(ScrollViewer viewer, double offset)
            {
                Debug.Assert(viewer != null, "viewer should not be null!");
                return Math.Max(Math.Min(offset, viewer.ExtentHeight), 0.0);
            }

            /// <summary>
            /// Coerce a horizontal offset to fall within the horizontal bounds of a
            ///             ScrollViewer.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><param name="offset">The horizontal offset to coerce.</param>
            /// <returns>
            /// The coerced horizontal offset that falls within the ScrollViewer's
            ///             horizontal bounds.
            /// 
            /// </returns>
            private static double CoerceHorizontalOffset(ScrollViewer viewer, double offset)
            {
                Debug.Assert(viewer != null, "viewer should not be null!");
                return Math.Max(Math.Min(offset, viewer.ExtentWidth), 0.0);
            }

            /// <summary>
            /// Scroll a ScrollViewer vertically by a given offset.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><param name="offset">The vertical offset to scroll.</param>
            private static void ScrollByVerticalOffset(ScrollViewer viewer, double offset)
            {
                Debug.Assert(viewer != null, "viewer should not be null!");
                offset += viewer.VerticalOffset;
                offset = ScrollViewerExtensions.CoerceVerticalOffset(viewer, offset);
                viewer.ScrollToVerticalOffset(offset);
            }

            /// <summary>
            /// Scroll a ScrollViewer horizontally by a given offset.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><param name="offset">The horizontal offset to scroll.</param>
            private static void ScrollByHorizontalOffset(ScrollViewer viewer, double offset)
            {
                Debug.Assert(viewer != null, "viewer should not be null!");
                offset += viewer.HorizontalOffset;
                offset = ScrollViewerExtensions.CoerceHorizontalOffset(viewer, offset);
                viewer.ScrollToHorizontalOffset(offset);
            }

            /// <summary>
            /// Scroll the ScrollViewer up by a line.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void LineUp(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByVerticalOffset(viewer, -16.0);
            }

            /// <summary>
            /// Scroll the ScrollViewer down by a line.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void LineDown(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByVerticalOffset(viewer, 16.0);
            }

            /// <summary>
            /// Scroll the ScrollViewer left by a line.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void LineLeft(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByHorizontalOffset(viewer, -16.0);
            }

            /// <summary>
            /// Scroll the ScrollViewer right by a line.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void LineRight(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByHorizontalOffset(viewer, 16.0);
            }

            /// <summary>
            /// Scroll the ScrollViewer up by a page.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void PageUp(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByVerticalOffset(viewer, -viewer.ViewportHeight);
            }

            /// <summary>
            /// Scroll the ScrollViewer down by a page.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void PageDown(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByVerticalOffset(viewer, viewer.ViewportHeight);
            }

            /// <summary>
            /// Scroll the ScrollViewer left by a page.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void PageLeft(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByHorizontalOffset(viewer, -viewer.ViewportWidth);
            }

            /// <summary>
            /// Scroll the ScrollViewer right by a page.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void PageRight(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                ScrollViewerExtensions.ScrollByHorizontalOffset(viewer, viewer.ViewportWidth);
            }

            /// <summary>
            /// Scroll the ScrollViewer to the top.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void ScrollToTop(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                viewer.ScrollToVerticalOffset(0.0);
            }

            /// <summary>
            /// Scroll the ScrollViewer to the bottom.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void ScrollToBottom(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                viewer.ScrollToVerticalOffset(viewer.ExtentHeight);
            }

            /// <summary>
            /// Scroll the ScrollViewer to the left.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void ScrollToLeft(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                viewer.ScrollToHorizontalOffset(0.0);
            }

            /// <summary>
            /// Scroll the ScrollViewer to the right.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception>
            public static void ScrollToRight(this ScrollViewer viewer)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                viewer.ScrollToHorizontalOffset(viewer.ExtentWidth);
            }

            /// <summary>
            /// Scroll the desired element into the ScrollViewer's viewport.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><param name="element">The element to scroll into view.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception><exception cref="T:System.ArgumentNullException"><paramref name="element"/> is null.
            ///             </exception>
            public static void ScrollIntoView(this ScrollViewer viewer, FrameworkElement element)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                if (element == null)
                    throw new ArgumentNullException("element");
                ScrollViewerExtensions.ScrollIntoView(viewer, element, 0.0, 112.0, TimeSpan.FromSeconds(0.25));
            }

            public static void ScrollToBeginnig(this ScrollViewer viewer, Duration duration)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");

                Storyboard storyboard = new Storyboard();
                ScrollViewerExtensions.SetVerticalOffset(viewer, viewer.VerticalOffset);
                ScrollViewerExtensions.SetHorizontalOffset(viewer, viewer.HorizontalOffset);
                DoubleAnimation doubleAnimation1 = new DoubleAnimation();
                doubleAnimation1.To = new double?(0.0);
                doubleAnimation1.Duration = duration;
                DoubleAnimation doubleAnimation2 = doubleAnimation1;
                DoubleAnimation doubleAnimation3 = new DoubleAnimation();
                doubleAnimation3.To = new double?(0.0);
                doubleAnimation3.Duration = duration;
                DoubleAnimation doubleAnimation4 = doubleAnimation3;
                Storyboard.SetTarget((Timeline)doubleAnimation2, (DependencyObject)viewer);
                Storyboard.SetTarget((Timeline)doubleAnimation4, (DependencyObject)viewer);
                Storyboard.SetTargetProperty((Timeline)doubleAnimation4, new PropertyPath((object)ScrollViewerExtensions.HorizontalOffsetProperty));
                Storyboard.SetTargetProperty((Timeline)doubleAnimation2, new PropertyPath((object)ScrollViewerExtensions.VerticalOffsetProperty));
                storyboard.Children.Add((Timeline)doubleAnimation2);
                storyboard.Children.Add((Timeline)doubleAnimation4);
                storyboard.Begin();
            }

            /// <summary>
            /// Scroll the desired element into the ScrollViewer's viewport.
            /// 
            /// </summary>
            /// <param name="viewer">The ScrollViewer.</param><param name="element">The element to scroll into view.</param><param name="horizontalMargin">The margin to add on the left or right.
            ///             </param><param name="verticalMargin">The margin to add on the top or bottom.
            ///             </param><param name="duration">The duration of the animation.</param><exception cref="T:System.ArgumentNullException"><paramref name="viewer"/> is null.
            ///             </exception><exception cref="T:System.ArgumentNullException"><paramref name="element"/> is null.
            ///             </exception>
            public static void ScrollIntoView(this ScrollViewer viewer, FrameworkElement element, double horizontalMargin, double verticalMargin, Duration duration)
            {
                if (viewer == null)
                    throw new ArgumentNullException("viewer");
                if (element == null)
                    throw new ArgumentNullException("element");
                Rect? boundsRelativeTo = VisualTreeExtensions.GetBoundsRelativeTo(element, (UIElement)viewer);
                if (!boundsRelativeTo.HasValue)
                    return;
                double verticalOffset = viewer.VerticalOffset;
                double num1 = 0.0;
                double viewportHeight = viewer.ViewportHeight;
                double num2 = boundsRelativeTo.Value.Bottom + verticalMargin;
                if (viewportHeight < num2)
                {
                    num1 = num2 - viewportHeight;
                    verticalOffset += num1;
                }
                double num3 = boundsRelativeTo.Value.Top - verticalMargin;
                if (num3 - num1 < 0.0)
                    verticalOffset -= num1 - num3;
                double horizontalOffset = viewer.HorizontalOffset;
                double num4 = 0.0;
                double viewportWidth = viewer.ViewportWidth;
                double num5 = boundsRelativeTo.Value.Right + horizontalMargin;
                if (viewportWidth < num5)
                {
                    num4 = num5 - viewportWidth;
                    horizontalOffset += num4;
                }
                double num6 = boundsRelativeTo.Value.Left - horizontalMargin;
                if (num6 - num4 < 0.0)
                    horizontalOffset -= num4 - num6;
                if (duration == (Duration)TimeSpan.Zero)
                {
                    viewer.ScrollToVerticalOffset(verticalOffset);
                    viewer.ScrollToHorizontalOffset(horizontalOffset);
                }
                else
                {
                    Storyboard storyboard = new Storyboard();
                    ScrollViewerExtensions.SetVerticalOffset(viewer, viewer.VerticalOffset);
                    ScrollViewerExtensions.SetHorizontalOffset(viewer, viewer.HorizontalOffset);
                    DoubleAnimation doubleAnimation1 = new DoubleAnimation();
                    doubleAnimation1.To = new double?(verticalOffset);
                    doubleAnimation1.Duration = duration;
                    DoubleAnimation doubleAnimation2 = doubleAnimation1;
                    DoubleAnimation doubleAnimation3 = new DoubleAnimation();
                    doubleAnimation3.To = new double?(verticalOffset);
                    doubleAnimation3.Duration = duration;
                    DoubleAnimation doubleAnimation4 = doubleAnimation3;
                    Storyboard.SetTarget((Timeline)doubleAnimation2, (DependencyObject)viewer);
                    Storyboard.SetTarget((Timeline)doubleAnimation4, (DependencyObject)viewer);
                    Storyboard.SetTargetProperty((Timeline)doubleAnimation4, new PropertyPath((object)ScrollViewerExtensions.HorizontalOffsetProperty));
                    Storyboard.SetTargetProperty((Timeline)doubleAnimation2, new PropertyPath((object)ScrollViewerExtensions.VerticalOffsetProperty));
                    storyboard.Children.Add((Timeline)doubleAnimation2);
                    storyboard.Children.Add((Timeline)doubleAnimation4);
                    storyboard.Begin();
                }
            }
        }
    }
