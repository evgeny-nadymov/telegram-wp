// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telegram.Controls.Extensions;

namespace Telegram.Controls
{
    public class LazyItemsControl : ItemsControl, ICompression
    {
        private const string VerticalCompressionGroup = "VerticalCompression";
        private const string ScrollStatesGroup = "ScrollStates";

        private const string CompressionTopState = "CompressionTop";
        private const string CompressionBottomState = "CompressionBottom";
        private const string ScrollingState = "Scrolling";

        private ScrollViewer _scrollViewer;

        public LazyItemsControl()
        {
            Loaded += LazyItemsControl_Loaded;
        }

        private void LazyItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= LazyItemsControl_Loaded;

            _scrollViewer = this.FindChildOfType<ScrollViewer>();

            if (_scrollViewer != null)
            {
                var element = VisualTreeHelper.GetChild(_scrollViewer, 0) as FrameworkElement;
                if (element != null)
                {
                    var verticalGroup = FindVisualState(element, VerticalCompressionGroup);
                    var scrollStatesGroup = FindVisualState(element, ScrollStatesGroup);

                    if (verticalGroup != null)
                        verticalGroup.CurrentStateChanging += VerticalGroup_CurrentStateChanging;

                    if (scrollStatesGroup != null)
                        scrollStatesGroup.CurrentStateChanging += ScrollStateGroup_CurrentStateChanging;
                }

                var binding = new Binding("VerticalOffset") { Source = _scrollViewer };
                SetBinding(VerticalOffsetProperty, binding);
            }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset", typeof (double), typeof (LazyItemsControl), new PropertyMetadata(default(double), OnVerticalOffsetChanged));

        public double VerticalOffset
        {
            get { return (double) GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lazyItemsControl = (LazyItemsControl)d;

            lazyItemsControl.OnListenerChanged(d, e);
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

        private void OnListenerChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private static VisualStateGroup FindVisualState(FrameworkElement element, string stateName)
        {
            if (element == null)
                return null;

            var groups = VisualStateManager.GetVisualStateGroups(element);
            return groups.Cast<VisualStateGroup>().FirstOrDefault(group => group.Name == stateName);
        }
        
        public event EventHandler<CompressionEventArgs> Compression;

        protected virtual void RaiseCompression(CompressionEventArgs e)
        {
            var handler = Compression;
            if (handler != null) handler(this, e);
        }

        private void VerticalGroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == CompressionTopState)
            {
                RaiseCompression(new CompressionEventArgs(CompressionType.Top));
            }
            if (e.NewState.Name == CompressionBottomState)
            {
                RaiseCompression(new CompressionEventArgs(CompressionType.Bottom));
            }
        }

        private void ScrollStateGroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            IsScrolling = (e.NewState.Name == ScrollingState);
        }

        public static readonly DependencyProperty IsScrollingProperty = DependencyProperty.Register(
            "IsScrolling",
            typeof(bool),
            typeof(LazyItemsControl),
            new PropertyMetadata(false));

        public bool IsScrolling
        {
            get { return (bool)GetValue(IsScrollingProperty); }
            set { SetValue(IsScrollingProperty, value); }
        }
    }
}
