// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DanielVaughan.WindowsPhone7Unleashed;
using Telegram.Controls.Extensions;

namespace Telegram.Controls
{
    public enum CompressionType { Top, Bottom, Left, Right };

    public interface ICompression
    {
        event EventHandler<CompressionEventArgs> Compression;
    }

    public class LazyListBox : ListBox, ICompression
    {
        public static readonly DependencyProperty KeepScrollingPositionProperty = DependencyProperty.Register(
            "KeepScrollingPosition", typeof (bool), typeof (LazyListBox), new PropertyMetadata(default(bool)));

        public bool KeepScrollingPosition
        {
            get { return (bool) GetValue(KeepScrollingPositionProperty); }
            set { SetValue(KeepScrollingPositionProperty, value); }
        }

        public bool SuppressVerticalOffsetListener { get; set; }

        public static readonly DependencyProperty IsHorizontalProperty = DependencyProperty.Register(
            "IsHorizontal", typeof (bool), typeof (LazyListBox), new PropertyMetadata(default(bool), OnIsHorizontalChanged));

        private static void OnIsHorizontalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as LazyListBox;
            if (listBox != null)
            {
                var isHorizontal = (bool) e.NewValue;
                if (isHorizontal)
                {
                    listBox.ToHorizontalOrientation();
                }
                else
                {
                    listBox.ToVerticalOrientation();
                }
            }
        }

        public bool IsHorizontal
        {
            get { return (bool) GetValue(IsHorizontalProperty); }
            set { SetValue(IsHorizontalProperty, value); }
        }

        public void ToHorizontalOrientation()
        {
            if (_stackPanel != null)
            {
                _stackPanel.Orientation = Orientation.Horizontal;
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        public void ToVerticalOrientation()
        {
            if (_stackPanel != null)
            {
                _stackPanel.Orientation = Orientation.Vertical;
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        private const string VerticalCompressionGroup = "VerticalCompression";
        private const string HorizontalCompressionGroup = "HorizontalCompression";
        private const string ScrollStatesGroup = "ScrollStates";
        private const string NoHorizontalCompressionState = "NoHorizontalCompression";
        private const string CompressionRightState = "CompressionRight";
        private const string CompressionLeftState = "CompressionLeft";
        private const string NoVerticalCompressionState = "NoVerticalCompression";
        private const string CompressionTopState = "CompressionTop";
        private const string CompressionBottomState = "CompressionBottom";
        private const string ScrollingState = "Scrolling";

        public double PanelVerticalOffset { get; set; }

        public double PanelViewPortHeight { get; set; }

        private VirtualizingStackPanel _stackPanel;

        private ScrollViewer _scrollViewer;

        public ScrollViewer Scroll
        {
            get { return _scrollViewer; }
        }

        protected bool IsBouncy;

        private bool _isInitialized;

        public LazyListBox()
        {
            Loaded += ListBox_Loaded;
            //Unloaded += ListBox_Unloaded;
        }

        //~LazyListBox()
        //{
            
        //}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset", typeof(double), typeof(LazyListBox), new PropertyMetadata(default(double), OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lazyListBox = d as LazyListBox;
            if (lazyListBox != null)
            {
                lazyListBox.OnListenerChanged(lazyListBox, new BindingChangedEventArgs(e));
            }
        }

        public double VerticalOffset
        {
            get { return (double) GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        private void OnListenerChanged(object sender, BindingChangedEventArgs e)
        {
            if (_prevVerticalOffset >= _scrollViewer.VerticalOffset) return;
            if (_scrollViewer.VerticalOffset == 0.0 && _scrollViewer.ScrollableHeight == 0.0) return;

            _prevVerticalOffset = _scrollViewer.VerticalOffset;
            var atBottom = _scrollViewer.VerticalOffset >= _scrollViewer.ScrollableHeight * CloseToEndPercent;

            if (atBottom)
            {
                RaiseCloseToEnd();
            }
        }

        public void StopScrolling()
        {
            //stop scrolling


            var offset = _stackPanel.VerticalOffset;

            if (_scrollViewer != null)
            {
                _scrollViewer.InvalidateScrollInfo();
                _scrollViewer.ScrollToVerticalOffset(offset);
                VisualStateManager.GoToState(_scrollViewer, "NotScrolling", true);
            }
        }

        public void ScrollToBeginning()
        {
            _scrollViewer.ScrollToBeginnig(new Duration(TimeSpan.FromSeconds(0.3)));
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
                return;
 
            _isInitialized = true;
            
            AddHandler(ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(ListBox_ManipulationCompleted), true);

            _scrollViewer = this.FindChildOfType<ScrollViewer>();
 
            if (_scrollViewer != null)
            {
                _stackPanel = _scrollViewer.FindChildOfType<VirtualizingStackPanel>();

                if (IsHorizontal)
                {
                    ToHorizontalOrientation();
                }
                else
                {
                    ToVerticalOrientation();
                }
                // Visual States are always on the first child of the control template 
                var element = VisualTreeHelper.GetChild(_scrollViewer, 0) as FrameworkElement;
                if (element != null)
                {
                    var verticalGroup = FindVisualState(element, VerticalCompressionGroup);
                    var horizontalGroup = FindVisualState(element, HorizontalCompressionGroup);
                    var scrollStatesGroup = FindVisualState(element, ScrollStatesGroup); 

                    if (verticalGroup != null)
                        verticalGroup.CurrentStateChanging += VerticalGroup_CurrentStateChanging;
                    if (horizontalGroup != null)
                        horizontalGroup.CurrentStateChanging += HorizontalGroup_CurrentStateChanging;
                    if (scrollStatesGroup != null)
                        scrollStatesGroup.CurrentStateChanging += ScrollStateGroup_CurrentStateChanging;
                }


                if (!SuppressVerticalOffsetListener)
                {
                    var binding = new Binding("VerticalOffset") { Source = _scrollViewer };
                    SetBinding(VerticalOffsetProperty, binding);
                }
            }
        }

        private double _closeToEndPercent = 0.7;

        public double CloseToEndPercent
        {
            get { return _closeToEndPercent; }
            set { _closeToEndPercent = value; }
        }

        private double _prevVerticalOffset;

        public event EventHandler<EventArgs> CloseToEnd;

        protected virtual void RaiseCloseToEnd()
        {
            var handler = CloseToEnd;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void ScrollStateGroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            IsScrolling = (e.NewState.Name == ScrollingState);
        }

        public event EventHandler<ScrollingStateChangedEventArgs> ScrollingStateChanged;

        protected virtual void RaiseScrollingStateChanged(ScrollingStateChangedEventArgs e)
        {
            var handler = ScrollingStateChanged;
            if (handler != null) handler(this, e);
        }

        public static readonly DependencyProperty IsScrollingProperty = DependencyProperty.Register(
            "IsScrolling",
            typeof(bool),
            typeof(LazyListBox),
            new PropertyMetadata(false, IsScrollingPropertyChanged));

        public bool IsScrolling
        {
            get { return (bool)GetValue(IsScrollingProperty); }
            set { SetValue(IsScrollingProperty, value); }
        }

        static void IsScrollingPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var listbox = source as LazyListBox;
            if (listbox == null) return;

            listbox.RaiseScrollingStateChanged(new ScrollingStateChangedEventArgs((bool) e.OldValue, (bool) e.NewValue));
        }

        #region Compression
        public event EventHandler<CompressionEventArgs> Compression;

        protected virtual void RaiseCompression(CompressionEventArgs e)
        {
            var handler = Compression;
            if (handler != null) handler(this, e);
        }

        private void HorizontalGroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == CompressionLeftState)
            {
                IsBouncy = true;
                RaiseCompression(new CompressionEventArgs(CompressionType.Left));
            }

            if (e.NewState.Name == CompressionRightState)
            {
                IsBouncy = true;
                RaiseCompression(new CompressionEventArgs(CompressionType.Right));
            }
            if (e.NewState.Name == NoHorizontalCompressionState)
            {
                IsBouncy = false;
            }
        }
 
        private void VerticalGroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == CompressionTopState)
            {
                IsBouncy = true;
                RaiseCompression(new CompressionEventArgs(CompressionType.Top));
            }
            if (e.NewState.Name == CompressionBottomState)
            {
                IsBouncy = true;
                RaiseCompression(new CompressionEventArgs(CompressionType.Bottom));
            }
            if (e.NewState.Name == NoVerticalCompressionState)
                IsBouncy = false;
        }
 
        private void ListBox_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (IsBouncy)
                IsBouncy = false;
        }
 
        private static VisualStateGroup FindVisualState(FrameworkElement element, string stateName)
        {
            if (element == null)
                return null;
 
            var groups = VisualStateManager.GetVisualStateGroups(element);
            return groups.Cast<VisualStateGroup>().FirstOrDefault(group => group.Name == stateName);
        }
        #endregion


        public List<ListBoxItem> GetVisibleItems()
        {

            var items = new List<ListBoxItem>();

            //if (_stackPanel == null) return items;

            //var firstVisibleItem = IsHorizontal ? (int)_stackPanel.HorizontalOffset : (int)_stackPanel.VerticalOffset;
            //var visibleItemCount = IsHorizontal ? (int)_stackPanel.ViewportWidth : (int)_stackPanel.ViewportHeight;
            //for (int index = firstVisibleItem; index < firstVisibleItem + visibleItemCount + 1; index++)
            //{
            //    var item = ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
            //    if (item == null)
            //        continue;

            //    items.Add(item);
            //}

            //return items;

            foreach (var item in Items)
            {
                var listBoxItem = ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (IsInView(listBoxItem, this))
                {
                    items.Add(listBoxItem);
                }
                else if (items.Any())
                {
                    break;
                }
            }

            return items;
        }

        private static bool IsInView(FrameworkElement element, FrameworkElement container)
        {
            if (element == null) return false;

            var elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            return containerBounds.Contains(new Point(elementBounds.X, elementBounds.Y)) // topLeft point
                || containerBounds.Contains(new Point(elementBounds.X + elementBounds.Width, elementBounds.Y + elementBounds.Height)); // bottomRight point
        }

        public event EventHandler Clear;

        protected virtual void RaiseClear()
        {
            var handler = Clear;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler FirstSliceLoaded;

        protected virtual void RaiseFirstSliceLoaded()
        {
            var handler = FirstSliceLoaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<VerticalOffsetChangedEventArgs> VerticalOffsetChanged;

        protected virtual void RaiseVerticalOffsetChanged(VerticalOffsetChangedEventArgs e)
        {
            EventHandler<VerticalOffsetChangedEventArgs> handler = VerticalOffsetChanged;
            if (handler != null) handler(this, e);
        }

        private object _lastRemovedItem;

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (KeepScrollingPosition)
            {
                if (_scrollViewer != null && _scrollViewer.VerticalOffset > 0.0001)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        if (e.NewItems != null && e.NewItems.Count == 1 && e.NewStartingIndex == 0)
                        {
                            if (_lastRemovedItem != e.NewItems[0])
                            {
                                var nextOffset = Math.Min(_scrollViewer.VerticalOffset + 1.0, _scrollViewer.ScrollableHeight + 1.0);

                                Debug.WriteLine("VerticalOffset={0} ExtentHeight={1} ViewportHeight={2} ScrollableHeight={3}", _scrollViewer.VerticalOffset, _scrollViewer.ExtentHeight, _scrollViewer.ViewportHeight, _scrollViewer.ScrollableHeight);
                                RaiseVerticalOffsetChanged(new VerticalOffsetChangedEventArgs { Viewer = _scrollViewer });
                                _scrollViewer.ScrollToVerticalOffset(nextOffset);
                            }
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        if (e.OldItems != null && e.OldItems.Count == 1 &&
                            (e.OldStartingIndex) <= _scrollViewer.VerticalOffset)
                        {
                            _lastRemovedItem = e.OldItems[0];
                        }
                        else
                        {
                            _lastRemovedItem = null;
                        }
                    }
                }

                //if (_scrollViewer != null && _scrollViewer.VerticalOffset > 0.0001)
                //{
                //    if (e.Action == NotifyCollectionChangedAction.Add)
                //    {
                //        if (e.NewItems != null && e.NewStartingIndex == 0)
                //        {
                //            foreach (var newItem in e.NewItems)
                //            {
                //                _scrollViewer.ScrollToVerticalOffset(Math.Min(_scrollViewer.VerticalOffset + 1.0, _scrollViewer.ScrollableHeight));
                //            }
                //        }
                //    }
                //    else if (e.Action == NotifyCollectionChangedAction.Remove)
                //    {
                //        if (e.OldItems != null && e.OldStartingIndex == 0)
                //        {
                //            foreach (var oldItem in e.OldItems)
                //            {
                //                _scrollViewer.ScrollToVerticalOffset(Math.Max(_scrollViewer.VerticalOffset - 1.0, 0.0));
                //            }
                //        }
                //    }
                //}
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                RaiseClear();
            }
            else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && Items != null && Items.Count == e.NewItems.Count)
            {
                RaiseFirstSliceLoaded();
            }

            base.OnItemsChanged(e);
        }
    }

    public class VerticalOffsetChangedEventArgs : EventArgs
    {
        public ScrollViewer Viewer { get; set; }
    }

    public class ScrollingStateChangedEventArgs : EventArgs
    {
        public bool OldValue { get; private set; }

        public bool NewValue { get; private set; }

        public ScrollingStateChangedEventArgs(bool oldValue, bool newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
 
    public class CompressionEventArgs : EventArgs
    {
        public CompressionType Type { get; protected set; }
 
        public CompressionEventArgs(CompressionType type)
        {
            Type = type;
        }
    }
}
