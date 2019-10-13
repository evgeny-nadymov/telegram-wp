using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TelegramClient.Controls.StartView
{
    /// <summary>
    /// Implements a custom Panel for the StartView control.
    /// </summary>
    public class StartViewPanel : Panel
    {
        private const int SnapThresholdDivisor = 2;

        private readonly List<StartViewItem> _visibleChildren = new List<StartViewItem>();
        private readonly List<ItemStop> _itemStops = new List<ItemStop>();
        private StartView _owner;
        private StartViewItem _selectedItem;

        internal IList<StartViewItem> VisibleChildren
        {
            get { return _visibleChildren; }
        }

        private StartView Owner
        {
            get { return _owner; }
            set
            {
                if (_owner != value)
                {
                    if (_owner != null)
                    {
                        _owner.Panel = null;
                    }

                    _owner = value;

                    if (_owner != null)
                    {
                        _owner.Panel = this;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the StartViewPanel class.
        /// </summary>
        public StartViewPanel()
        {
            SizeChanged += OnSizeChanged;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _owner = null;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Owner.ItemsWidth = (int)e.NewSize.Width;
        }

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// Desired size.
        /// </returns>
        /// <param name="availableSize">Available size.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_owner == null)
            {
                FindOwner();
            }

            Size desiredSize = new Size(0, availableSize.Height);

            int childWidth = Owner.ViewportWidth;
            int childHeight = (int)Math.Min(availableSize.Height, Owner.ViewportHeight);
            Size childSize = new Size(childWidth, childHeight);

            _visibleChildren.Clear();

            foreach (StartViewItem child in Children)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    _visibleChildren.Add(child);
                    child.Measure(childSize);
                    desiredSize.Width += childWidth;
                }
            }

            return desiredSize;
        }

        /// <summary>
        /// Handles the arrange pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// Render size.
        /// </returns>
        /// <param name="finalSize">Final size.</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _itemStops.Clear();
            double x = 0;
            Rect finalRect = new Rect(0, 0, 0, finalSize.Height);

            for (int index = 0; index < _visibleChildren.Count; ++index)
            {
                StartViewItem child = _visibleChildren[index];
                finalRect.X = child.StartPosition = (int)x;
                _itemStops.Add(new ItemStop(child, child.StartPosition));
                finalRect.Width = Owner.ViewportWidth;
                child.ItemWidth = (int)finalRect.Width;
                child.Arrange(finalRect);
                x += finalRect.Width;
            }

            Owner.RequestAdjustSelection();

            return finalSize;
        }

        private void GetItemsInView(int offset, int viewportWidth, out int leftIndex, out int leftInView, out int centerIndex, out int rightIndex, out int rightInView)
        {
            leftIndex = leftInView = centerIndex = rightIndex = rightInView = -1;

            int count = VisibleChildren.Count;
            if (count == 0)
            {
                return;
            }

            for (int index = 0; index < count; ++index)
            {
                StartViewItem child = _visibleChildren[index];

                int leftOffset = child.StartPosition + offset;
                int rightOffset = leftOffset + child.ItemWidth - 1;

                if (leftOffset <= 0 && rightOffset >= 0)
                {
                    leftIndex = index;
                    leftInView = Math.Min(viewportWidth, child.ItemWidth + leftOffset);
                }

                if (leftOffset < viewportWidth && rightOffset >= viewportWidth)
                {
                    rightIndex = index;
                    rightInView = Math.Min(viewportWidth, viewportWidth - leftOffset);
                }

                if (leftOffset > 0 && rightOffset < viewportWidth)
                {
                    centerIndex = index;
                }

                if (index == 0 && leftInView == -1)
                {
                    leftInView = leftOffset;
                }

                if (index == count - 1 && rightInView == -1)
                {
                    rightInView = viewportWidth - rightOffset - 1;
                }
            }
        }

        internal void GetStops(int offset, int totalWidth, out ItemStop previous, out ItemStop current, out ItemStop next)
        {
            next = current = previous = null;

            if (VisibleChildren.Count == 0)
            {
                return;
            }

            int nextIndex = -1;
            int currentIndex = -1;
            int previousIndex = -1;

            int position = -offset % totalWidth;
            int itemStopIndex = 0;

            foreach (ItemStop itemStop in _itemStops)
            {
                if (itemStop.Position < position)
                {
                    previousIndex = itemStopIndex;
                }
                else if (itemStop.Position > position)
                {
                    nextIndex = itemStopIndex;
                    break;
                }
                else if (itemStop.Position == position)
                {
                    currentIndex = itemStopIndex;
                }

                ++itemStopIndex;
            }

            if (previousIndex != -1)
            {
                previous = _itemStops[previousIndex];
            }

            if (currentIndex != -1)
            {
                current = _itemStops[currentIndex];
            }

            if (nextIndex != -1)
            {
                next = _itemStops[nextIndex];
            }
        }

        internal void GetSnapOffset(int offset, int viewportWidth, int direction, out int snapTo, out int newDirection, out StartViewItem newSelection)
        {
            int snapThreshold = viewportWidth / SnapThresholdDivisor;

            snapTo = offset;
            newDirection = direction;
            newSelection = _selectedItem;

            if (VisibleChildren.Count == 0)
            {
                return;
            }

            foreach (ItemStop itemStop in _itemStops)
            {
                if (itemStop.Position == -offset)
                {
                    newSelection = itemStop.Item;
                    return;
                }
            }

            int leftIndex;
            int leftInView;
            int centerIndex;
            int rightIndex;
            int rightInView;

            GetItemsInView(offset, viewportWidth, out leftIndex, out leftInView, out centerIndex, out rightIndex, out rightInView);

            if (leftIndex == rightIndex && leftIndex != -1)
            {
                newSelection = _selectedItem = _visibleChildren[leftIndex];
            }
            else
            {
                if (leftIndex == -1)
                {
                    leftIndex = _visibleChildren.Count - 1;
                }

                if (rightIndex == -1)
                {
                    rightIndex = 0;
                }

                int index;
                if (direction < 0)
                {
                    if (rightInView > snapThreshold)
                    {
                        index = GetBestIndex(centerIndex, rightIndex, leftIndex);
                        newDirection = -1;
                    }
                    else
                    {
                        index = GetBestIndex(leftIndex, centerIndex, rightIndex);
                        newDirection = 1;
                    }
                }
                else if (direction > 0)
                {
                    if (leftInView > snapThreshold)
                    {
                        index = StartViewPanel.GetBestIndex(leftIndex, centerIndex, rightIndex);
                        newDirection = 1;
                    }
                    else
                    {
                        index = StartViewPanel.GetBestIndex(centerIndex, rightIndex, leftIndex);
                        newDirection = -1;
                    }
                }
                else if (centerIndex != -1)
                {
                    index = centerIndex;
                    newDirection = -1;
                }
                else if (leftInView > rightInView)
                {
                    index = leftIndex;
                    newDirection = -1;
                }
                else
                {
                    index = rightIndex;
                    newDirection = 1;
                }

                _selectedItem = _visibleChildren[index];
                snapTo = -_selectedItem.StartPosition;
                newSelection = _selectedItem;
            }
        }

        private static int GetBestIndex(int n0, int n1, int n2)
        {
            if (n0 >= 0)
            {
                return n0;
            }

            if (n1 >= 0)
            {
                return n1;
            }

            if (n2 >= 0)
            {
                return n2;
            }

            throw new InvalidOperationException("No best index.");
        }

        private void FindOwner()
        {
            FrameworkElement frameworkElement = this;
            StartView owner;
            do
            {
                frameworkElement = (FrameworkElement)VisualTreeHelper.GetParent(frameworkElement);
                owner = frameworkElement as StartView;
            }
            while (frameworkElement != null && owner == null);
            Owner = owner;
        }

        internal class ItemStop
        {
            public ItemStop(StartViewItem item, int position)
            {
                Item = item;
                Position = position;
            }

            public int Position { get; private set; }

            public StartViewItem Item { get; private set; }
        }
    }
}
