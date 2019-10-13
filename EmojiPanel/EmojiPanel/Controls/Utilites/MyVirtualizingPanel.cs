using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telegram.Controls.Utilities;

namespace Telegram.Controls.VirtualizedView
{
    public class MyVirtualizingPanel : Canvas
    {
        private const bool IsLogEnabled = false;

        private static void Log(string str)
        {
            if (IsLogEnabled)
            {
                Debug.WriteLine(str);
            }
        }


        private const double LoadUnloadThreshold = 500;
        private const double LoadedHeightUpwards = 300;
        private const double LoadedHeightDownwards = 900;
        private const double LoadedHeightDownwardsNotScrolling = 800;

        private bool _changingVerticalOffset = false;

        readonly DependencyProperty _listVerticalOffsetProperty = DependencyProperty.Register(
             "ListVerticalOffset",
             typeof(double),
             typeof(MyVirtualizingPanel),
             new PropertyMetadata(new PropertyChangedCallback(OnListVerticalOffsetChanged)));

        public double ListVerticalOffset
        {
            get { return (double) this.GetValue(_listVerticalOffsetProperty); }
            set { this.SetValue(_listVerticalOffsetProperty, value); }
        }

        private static void OnListVerticalOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (MyVirtualizingPanel) obj;
            control.OnListVerticalOffsetChanged();
        }

        private ScrollViewer _scrollViewer;
        bool _isScrolling = false;

        public ScrollViewer ScrollViewer
        {
            get { return _scrollViewer; }
        }

        public void InitializeWithScrollViewer(ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;
            EnsureBoundToScrollViewer();
        }

        protected void EnsureBoundToScrollViewer()
        {
            Binding binding = new Binding
            {
                Source = _scrollViewer,
                Path = new PropertyPath("VerticalOffset"),
                Mode = BindingMode.OneWay
            };
            this.SetBinding(_listVerticalOffsetProperty, binding);


        }

        bool _notReactToScroll = false;
        private double _savedDelta;
        //private DelayedExecutor _de = new DelayedExecutor(300);
        //internal void PrepareForScrollToBottom()
        //{
        //    _notReactToScroll = true;
        //    _savedDelta = DeltaOffset;
        //    // load in the end
        //    DeltaOffset = _scrollViewer.ExtentHeight - _scrollViewer.ViewportHeight - _scrollViewer.VerticalOffset;
        //    Debug.WriteLine("PrepareForScrollToBottom");
        //    PerformLoadUnload2(VirtualizableState.LoadedPartially, false);
        //    _de.AddToDelayedExecution(() =>
        //        {
        //            Execute.ExecuteOnUIThread(() => ScrollToBottomCompleted());
        //        });
        //}

        //internal void ScrollToBottomCompleted()
        //{
        //    _notReactToScroll = false;
        //    DeltaOffset = _savedDelta;
        //    PerformLoadUnload(VirtualizableState.LoadedFully);
        //    Debug.WriteLine("ScrolltoBottomCompleted");
        //}

        private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "Scrolling")
            {
                _isScrolling = true;
            }
            else
            {
                _isScrolling = false;
                PerformLoadUnload(true);
            }
        }

        private static VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        public class ScrollPositionChangedEventAgrs : EventArgs
        {
            public double CurrentPosition { get; private set; }
            public double ScrollHeight { get; private set; }

            public ScrollPositionChangedEventAgrs(double currentPosition,
                double scrollHeight)
            {
                CurrentPosition = currentPosition;
                ScrollHeight = scrollHeight;
            }
        }

        public event EventHandler<ScrollPositionChangedEventAgrs> ScrollPositionChanged;

        private double _previousScrollOffset = 0;
        private DateTime _previousScrollOffsetChangedTime = DateTime.MinValue;
        private const double PixelsPerSecondThreshold = 200;

        private void OnListVerticalOffsetChanged()
        {
            if (_notReactToScroll) return;

            if (!_changingVerticalOffset)
            {

                var w = new Stopwatch();
                w.Start();
                PerformLoadUnload(true);
                w.Stop();

                Log("LOADUNLOAD performed in " + w.ElapsedMilliseconds);

                if (ScrollPositionChanged != null)
                {
                    ScrollPositionChanged(this, new ScrollPositionChangedEventAgrs(
                        _scrollViewer.VerticalOffset,
                        Height));
                }

                Log("Reported Offset: " + _scrollViewer.VerticalOffset);
            }
        }

        private bool DetermineIfScrollingIsFast()
        {
            var now = DateTime.Now;
            var result = false;
            if (_previousScrollOffsetChangedTime != DateTime.Now)
            {
                var scrolledPixels = Math.Abs(_scrollViewer.VerticalOffset - _previousScrollOffset);
                var timeInSeconds = (now - _previousScrollOffsetChangedTime).TotalSeconds;

                if (scrolledPixels != 0)
                {
                    var speedPixelsPerSecond = scrolledPixels / timeInSeconds;
                    Log(String.Format("Speed of scroll {0} ", speedPixelsPerSecond));

                    if (speedPixelsPerSecond > PixelsPerSecondThreshold)
                    {
                        result = true;
                    }
                }
            }

            _previousScrollOffsetChangedTime = now;
            _previousScrollOffset = _scrollViewer.VerticalOffset;
            return result;
        }

        private readonly List<VListItemBase> _virtItems = new List<VListItemBase>();

        // indexes of loaded items
        private Segment _loadedSegment = new Segment();

        // maps a point to its index in _virtItems
        // covers only points 0, LoadUnloadThreshold, 2*LoadUnloadThreshold, etc
        private readonly Dictionary<int, int> _thresholdPointIndexes = new Dictionary<int, int>();


        // do not change through this property
        public List<VListItemBase> VirtItems
        {
            get { return _virtItems; }
        }

        public MyVirtualizingPanel()
        {
            Loaded += MyVirtualizingPanel_Loaded;
        }

        void MyVirtualizingPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(_scrollViewer, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup group = FindVisualState(element, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += group_CurrentStateChanging;
                    }
                }
            }
        }

        public void AddItems(IEnumerable<VListItemBase> _itemsToBeAdded)
        {
            var sw = new Stopwatch();
            sw.Start();

            double topMargin = 0;

            if (_virtItems.Count > 0)
            {
                topMargin = _virtItems.Sum(vi => vi.FixedHeight);
            }

            foreach (var itemToBeAdded in _itemsToBeAdded)
            {
                itemToBeAdded.View.Margin = new Thickness(itemToBeAdded.Margin.Left, itemToBeAdded.Margin.Top + topMargin, itemToBeAdded.Margin.Right, itemToBeAdded.Margin.Bottom);

                _virtItems.Add(itemToBeAdded);

                var itemHeightIncludingMargin = itemToBeAdded.FixedHeight;

                List<int> coveredPoints = GetCoveredPoints(topMargin, topMargin + itemHeightIncludingMargin);

                foreach (var coveredPoint in coveredPoints)
                {
                    _thresholdPointIndexes[coveredPoint] = _virtItems.Count - 1; // index of the last
                }

                topMargin += itemHeightIncludingMargin;
            }

            PerformLoadUnload(true);

            Height = topMargin;

            sw.Stop();

            Log(String.Format("MyVirtualizingPanel.AddItems {0}", sw.ElapsedMilliseconds));
        }


        public void InsertRemoveItems(int index, List<VListItemBase> itemsToInsert, bool keepItemsBelowIndexFixed = false, VListItemBase itemToRemove = null)
        {
            bool needToAdjustScrollPositionAfterInsertion = false;

            if (keepItemsBelowIndexFixed)
            {
                double totalHeightOfAllItemsBeforeIndex = 0;

                for (int i = 0; i < index; i++)
                {
                    totalHeightOfAllItemsBeforeIndex += VirtItems[i].FixedHeight + VirtItems[i].Margin.Top + VirtItems[i].Margin.Bottom;
                }

                if (totalHeightOfAllItemsBeforeIndex < _scrollViewer.VerticalOffset + _scrollViewer.ViewportHeight)
                {
                    needToAdjustScrollPositionAfterInsertion = true;
                }
            }

            //  UnloadItemsInSegment(_loadedSegment);
            _loadedSegment = new Segment();

            var totalHeight = itemsToInsert.Sum(i => i.FixedHeight + i.Margin.Top + i.Margin.Bottom);

            _virtItems.InsertRange(index, itemsToInsert);

            if (itemToRemove != null)
            {
                itemToRemove.IsVLoaded = false;
                totalHeight -= itemToRemove.FixedHeight + itemToRemove.Margin.Top + itemToRemove.Margin.Bottom;
                _virtItems.Remove(itemToRemove);
            }


            RearrangeAllItems();


            if (needToAdjustScrollPositionAfterInsertion)
            {
                _changingVerticalOffset = true;
                //Debug.WriteLine("SCROLLING TO " + _scrollViewer.VerticalOffset + totalHeight + " scroll height : " + _scrollViewer.ExtentHeight);
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + totalHeight);
                _changingVerticalOffset = false;
            }

            PerformLoadUnload(false);
        }

        public void RemoveItem(VListItemBase itemToBeRemoved)
        {
            itemToBeRemoved.IsVLoaded = false;


            _virtItems.Remove(itemToBeRemoved);
            _loadedSegment = new Segment();
            RearrangeAllItems();

            PerformLoadUnload(true);
        }

        private void RearrangeAllItems()
        {
            double topMargin = 0;
            _thresholdPointIndexes.Clear();
            int ind = 0;
            foreach (var item in _virtItems)
            {
                item.View.Margin = new Thickness(item.Margin.Left, item.Margin.Top + topMargin, item.Margin.Right, item.Margin.Bottom);

                var itemHeightIncludingMargin = item.FixedHeight + item.Margin.Top + item.Margin.Bottom;

                List<int> coveredPoints = GetCoveredPoints(topMargin, topMargin + itemHeightIncludingMargin);

                foreach (var coveredPoint in coveredPoints)
                {
                    _thresholdPointIndexes[coveredPoint] = ind; // index of the last
                }

                topMargin += itemHeightIncludingMargin;
                ind++;
            }

            Height = topMargin;
            _scrollViewer.UpdateLayout();
        }

        private void PerformLoadUnload2(bool isToLoad, bool bypassUnload = false)
        {
            if (_virtItems.Count == 0)
                return;

            double currentOffset = GetRealOffset();

            int lowestLoadedInd = 0;
            int upperInd = 0;

            bool triggerLoading = false;

            if (isToLoad || _loadedSegment.IsEmpty)
            {
                triggerLoading = true;
            }
            else
            {
                lowestLoadedInd = _loadedSegment.LowerBound;
                upperInd = _loadedSegment.UpperBound;

                double topPoint = _virtItems[lowestLoadedInd].View.Margin.Top;
                double bottomPoint = _virtItems[upperInd].View.Margin.Top + _virtItems[upperInd].FixedHeight;

                if (currentOffset - topPoint < 500 ||
                    bottomPoint - currentOffset < 1500)
                {
                    triggerLoading = true;
                }
            }

            if (triggerLoading)
            {
                if (_scrollViewer.ExtentHeight < 3000 && _isScrolling)
                {
                    //Debug.WriteLine("Detected short scroll; loading all items");
                    // otherwise there are glitches in scrolling
                    lowestLoadedInd = 0;
                    upperInd = VirtItems.Count - 1;
                    isToLoad = true;
                }
                else
                {
                    var threshold = (int) Math.Floor((currentOffset - (currentOffset % LoadUnloadThreshold)));

                    int indexOfBaseItem = _thresholdPointIndexes.ContainsKey(threshold) ? _thresholdPointIndexes[threshold] : -1;

                    lowestLoadedInd = upperInd = indexOfBaseItem < 0 ? 0 : indexOfBaseItem;

                    double loadUpwards = LoadedHeightUpwards;
                    double loadDownwards = _isScrolling ? LoadedHeightDownwards : LoadedHeightDownwardsNotScrolling;

                    //if (_isScrolling)
                    //{
                    //    loadUpwards = LoadUpwardsWhenScrolling;
                    //    loadDownwards = LoadDownwardsWhenScrolling;
                    //}

                    // count up from the lower point on view
                    while (lowestLoadedInd > 0 && currentOffset - _virtItems[lowestLoadedInd].View.Margin.Top < loadUpwards)
                    {
                        lowestLoadedInd--;
                    }

                    while (upperInd < _virtItems.Count - 1 && _virtItems[upperInd].View.Margin.Top - currentOffset < loadDownwards)
                    {
                        upperInd++;
                    }

                }

                SetLoadedBounds(lowestLoadedInd, upperInd, isToLoad, bypassUnload);


                if (IsLogEnabled)
                {
                    string loadedIndexes = "Loaded indexes : ";
                    for (int i = 0; i < _virtItems.Count; i++)
                    {
                        if (_virtItems[i].IsVLoaded)
                        {
                            loadedIndexes += i + ",";
                        }
                    }

                    Log(loadedIndexes);
                }
            }

        }

        public double DeltaOffset
        {
            get;
            set;
        }

        private double GetRealOffset()
        {
            //// it might throw exception
            //try
            //{
            //    GeneralTransform childTransform = this.TransformToVisual(_listScrollViewer);

            //    var p = childTransform.Transform(new Point(0, 0));

            //    var delta = p.Y;
            //    Debug.WriteLine("DELTA offset =" + delta + "; VerticalOffset=" + _listScrollViewer.VerticalOffset);

            //    return -delta;
            //}
            //catch (Exception exc)
            //{
            //    return _listScrollViewer.VerticalOffset;
            //}

            return _scrollViewer.VerticalOffset + DeltaOffset;
        }

        private void PerformLoadUnload(bool isToLoad)
        {
            PerformLoadUnload2(isToLoad);
        }

        private void SetLoadedBounds(int lowerBoundInd, int upperBoundInd, bool isToLoad, bool bypassUnload = false)
        {
            var newLoadedSegment = new Segment(lowerBoundInd, upperBoundInd);

            Segment newMinusLoaded1;
            Segment newMinusLoaded2;
            Segment intersection;
            Segment loadedMinusNew1;
            Segment loadedMinusNew2;

            newLoadedSegment.CompareToSegment(_loadedSegment,
                out newMinusLoaded1,
                out newMinusLoaded2,
                out intersection,
                out loadedMinusNew1,
                out loadedMinusNew2);


            Log(String.Format("LoadedSegment:{0}, NewSegment:{1}, NewMinusLoaded1:{2}, NewMinusLoaded2:{3}, loadedMinusNew1:{4}, loadedMinusNew2:{5}",
                _loadedSegment,
                newLoadedSegment,
                newMinusLoaded1,
                newMinusLoaded2,
                loadedMinusNew1,
                loadedMinusNew2));

            if (isToLoad)
            {
                // ensure items are loaded fully for the whole segment
                LoadItemsInSegment(newLoadedSegment);
            }

            if (!bypassUnload)
            {
                UnloadItemsInSegment(loadedMinusNew1);
                UnloadItemsInSegment(loadedMinusNew2);
            }
            _loadedSegment = newLoadedSegment;

        }

        private void UnloadItemsInSegment(Segment segment)
        {
            for (int i = segment.LowerBound; i <= segment.UpperBound; i++)
            {
                var item = _virtItems[i];

                Children.Remove(item.View);

                item.IsVLoaded = false;
            }
        }

        private void LoadItemsInSegment(Segment segment)
        {
            for (int i = segment.LowerBound; i <= segment.UpperBound; i++)
            {
                var item = _virtItems[i];

                item.IsVLoaded = true;

                if (!Children.Contains(item.View))
                {
                    Children.Add(item.View);
                }
            }
        }

        private List<int> GetCoveredPoints(double from, double to)
        {

            var result = new List<int>();

            var candidate = from - (from % LoadUnloadThreshold);

            while (candidate <= to)
            {
                if (candidate >= from)
                {
                    result.Add((int) Math.Floor(candidate));
                }
                candidate += LoadUnloadThreshold;
            }

            return result;
        }

        public void ClearItems()
        {
            _virtItems.Clear();
            Children.Clear();
            _loadedSegment = new Segment();
            _thresholdPointIndexes.Clear();
            _scrollViewer.ScrollToVerticalOffset(0);
            Height = 0;
        }
    }
}
