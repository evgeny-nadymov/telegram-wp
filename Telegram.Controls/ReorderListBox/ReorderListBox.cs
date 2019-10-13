// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
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
using Microsoft.Phone.Controls;
using Telegram.Controls;

namespace ReorderListBox
{
    /// <summary>
    /// Extends ListBox to enable drag-and-drop reorder within the list.
    /// </summary>
    [TemplatePart(Name = ReorderListBox.ScrollViewerPart, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ReorderListBox.DragIndicatorPart, Type = typeof(Image))]
    [TemplatePart(Name = ReorderListBox.DragInterceptorPart, Type = typeof(Canvas))]
    [TemplatePart(Name = ReorderListBox.RearrangeCanvasPart, Type = typeof(Canvas))]
    public class ReorderListBox : ListBox
    {
        #region Template part name constants

        public const string ScrollViewerPart = "ScrollViewer";
        public const string DragIndicatorPart = "DragIndicator";
        public const string DragInterceptorPart = "DragInterceptor";
        public const string RearrangeCanvasPart = "RearrangeCanvas";

        #endregion

        private const string VerticalCompressionGroup = "VerticalCompression";
        private const string HorizontalCompressionGroup = "HorizontalCompression";
        private const string ScrollStatesGroup = "ScrollStates";

        private const string ScrollViewerScrollingVisualState = "Scrolling";
        private const string ScrollViewerNotScrollingVisualState = "NotScrolling";

        private const string IsReorderEnabledPropertyName = "IsReorderEnabled";

        #region Private fields

        private double dragScrollDelta;
        private Panel itemsPanel;
        private ScrollViewer scrollViewer;
        private Canvas dragInterceptor;
        private Image dragIndicator;
        private object dragItem;
        private ReorderListBoxItem dragItemContainer;
        private bool isDragItemSelected;
        private Rect dragInterceptorRect;
        private int dropTargetIndex;
        private Canvas rearrangeCanvas;
        private Queue<KeyValuePair<Action, Duration>> rearrangeQueue;

        #endregion

        /// <summary>
        /// Creates a new ReorderListBox and sets the default style key.
        /// The style key is used to locate the control template in Generic.xaml.
        /// </summary>
        public ReorderListBox()
        {
            this.DefaultStyleKey = typeof(ReorderListBox);

            ItemContainerGenerator.ItemsChanged += (sender, args) =>
            {

            };
        }

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate", typeof (DataTemplate), typeof (ReorderListBox), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate) GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register(
            "FooterTemplate", typeof (DataTemplate), typeof (ReorderListBox), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate) GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        #region IsReorderEnabled DependencyProperty

        public static readonly DependencyProperty IsReorderEnabledProperty = DependencyProperty.Register(
            ReorderListBox.IsReorderEnabledPropertyName, typeof(bool), typeof(ReorderListBox),
            new PropertyMetadata(false, (d, e) => ((ReorderListBox)d).OnIsReorderEnabledChanged(e)));

        /// <summary>
        /// Gets or sets a value indicating whether reordering is enabled in the listbox.
        /// This also controls the visibility of the reorder drag-handle of each listbox item.
        /// </summary>
        public bool IsReorderEnabled
        {
            get
            {
                return (bool)this.GetValue(ReorderListBox.IsReorderEnabledProperty);
            }
            set
            {
                this.SetValue(ReorderListBox.IsReorderEnabledProperty, value);
            }
        }

        protected void OnIsReorderEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.dragInterceptor != null)
            {
                this.dragInterceptor.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }

            this.InvalidateArrange();
        }

        #endregion

        #region AutoScrollMargin DependencyProperty

        public static readonly DependencyProperty AutoScrollMarginProperty = DependencyProperty.Register(
            "AutoScrollMargin", typeof(int), typeof(ReorderListBox), new PropertyMetadata(32));

        private PhoneApplicationPage _page;

        /// <summary>
        /// Gets or sets the size of the region at the top and bottom of the list where dragging will
        /// cause the list to automatically scroll.
        /// </summary>
        public double AutoScrollMargin
        {
            get
            {
                return (int)this.GetValue(ReorderListBox.AutoScrollMarginProperty);
            }
            set
            {
                this.SetValue(ReorderListBox.AutoScrollMarginProperty, value);
            }
        }

        #endregion

        #region ItemsControl overrides

        /// <summary>
        /// Applies the control template, gets required template parts, and hooks up the drag events.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.scrollViewer = (ScrollViewer)this.GetTemplateChild(ReorderListBox.ScrollViewerPart);
            if (scrollViewer != null)
            {
                //var verticalGroup = FindVisualState(scrollViewer, VerticalCompressionGroup);
                //var horizontalGroup = FindVisualState(scrollViewer, HorizontalCompressionGroup);
                var scrollStatesGroup = FindVisualState(scrollViewer, ScrollStatesGroup);

                //if (verticalGroup != null)
                //    verticalGroup.CurrentStateChanging += VerticalGroup_CurrentStateChanging;
                //if (horizontalGroup != null)
                //    horizontalGroup.CurrentStateChanging += HorizontalGroup_CurrentStateChanging;
                if (scrollStatesGroup != null)
                    scrollStatesGroup.CurrentStateChanging += ScrollStateGroup_CurrentStateChanging;
            }
            this.dragInterceptor = this.GetTemplateChild(ReorderListBox.DragInterceptorPart) as Canvas;
            this.dragIndicator = this.GetTemplateChild(ReorderListBox.DragIndicatorPart) as Image;
            this.rearrangeCanvas = this.GetTemplateChild(ReorderListBox.RearrangeCanvasPart) as Canvas;

            if (this.scrollViewer != null && this.dragInterceptor != null && this.dragIndicator != null)
            {
                this.dragInterceptor.Visibility = this.IsReorderEnabled ? Visibility.Visible : Visibility.Collapsed;

                //AddHandler(ManipulationStartedEvent, new EventHandler<ManipulationStartedEventArgs>(OnReorderListBoxManipulationStarted), true);
                //AddHandler(ManipulationDeltaEvent, new EventHandler<ManipulationDeltaEventArgs>(dragInterceptor_ManipulationDelta), true);
                //AddHandler(ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(dragInterceptor_ManipulationCompleted), true);
                this.dragInterceptor.ManipulationStarted += this.dragInterceptor_ManipulationStarted;
                this.dragInterceptor.ManipulationDelta += this.dragInterceptor_ManipulationDelta;
                this.dragInterceptor.ManipulationCompleted += this.dragInterceptor_ManipulationCompleted;
            }
        }

        private void ScrollStateGroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            IsScrolling = (e.NewState.Name == ScrollViewerScrollingVisualState);
        }
        /// <summary>
        /// The event people can subscribe to
        /// </summary>
        public event EventHandler<ScrollingStateChangedEventArgs> ScrollingStateChanged;

        /// <summary>
        /// DependencyProperty that backs the <see cref="IsScrolling"/> property
        /// </summary>
        public static readonly DependencyProperty IsScrollingProperty = DependencyProperty.Register(
            "IsScrolling",
            typeof(bool),
            typeof(ReorderListBox),
            new PropertyMetadata(false, IsScrollingPropertyChanged));

        /// <summary>
        /// Whether the list is currently scrolling or not
        /// </summary>
        public bool IsScrolling
        {
            get { return (bool)GetValue(IsScrollingProperty); }
            set { SetValue(IsScrollingProperty, value); }
        }

        /// <summary>
        /// Handler for when the IsScrolling dependency property changes
        /// </summary>
        /// <param name="source">The object that has the property</param>
        /// <param name="e">Args</param>
        static void IsScrollingPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var listbox = source as ReorderListBox;
            if (listbox == null) return;

            // Call the virtual notification method for anyone who derives from this class
            var scrollingArgs = new ScrollingStateChangedEventArgs((bool)e.OldValue, (bool)e.NewValue);

            // Raise the event, if anyone is listening to it
            var handler = listbox.ScrollingStateChanged;
            if (handler != null)
                handler(listbox, scrollingArgs);
        }

        private static VisualStateGroup FindVisualState(FrameworkElement element, string stateName)
        {
            if (element == null)
                return null;

            var groups = VisualStateManager.GetVisualStateGroups(element);
            return groups.Cast<VisualStateGroup>().FirstOrDefault(group => group.Name == stateName);
        }

        //private void OnReorderListBoxManipulationStarted(object sender, ManipulationStartedEventArgs e)
        //{
        //    scrollViewer.IsHitTestVisible = false;
        //    dragInterceptor_ManipulationStarted(sender, e);
        //}

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ReorderListBoxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ReorderListBoxItem;
        }

        /// <summary>
        /// Ensures that a possibly-recycled item container (ReorderListBoxItem) is ready to display a list item.
        /// </summary>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            ReorderListBoxItem itemContainer = (ReorderListBoxItem)element;
            itemContainer.ApplyTemplate();  // Loads visual states.

            // Set this state before binding to avoid showing the visual transition in this case.
            string reorderState = this.IsReorderEnabled ?
                ReorderListBoxItem.ReorderEnabledState : ReorderListBoxItem.ReorderDisabledState;
            VisualStateManager.GoToState(itemContainer, reorderState, false);

            itemContainer.SetBinding(ReorderListBoxItem.IsReorderEnabledProperty,
                new Binding(ReorderListBox.IsReorderEnabledPropertyName) { Source = this });

            if (item == this.dragItem)
            {
                itemContainer.IsSelected = this.isDragItemSelected;
                VisualStateManager.GoToState(itemContainer, ReorderListBoxItem.DraggingState, false);

                if (this.dropTargetIndex >= 0)
                {
                    // The item's dragIndicator is currently being moved, so the item itself is hidden. 
                    itemContainer.Visibility = Visibility.Collapsed;
                    this.dragItemContainer = itemContainer;
                }
                else
                {
                    itemContainer.Opacity = 0;
                    this.Dispatcher.BeginInvoke(() => this.AnimateDrop(itemContainer, "prepareContainer"));
                }
            }
            else
            {
                VisualStateManager.GoToState(itemContainer, ReorderListBoxItem.NotDraggingState, false);
            }
        }

        /// <summary>
        /// Called when an item container (ReorderListBoxItem) is being removed from the list panel.
        /// This may be because the item was removed from the list or because the item is now outside
        /// the virtualization region (because ListBox uses a VirtualizingStackPanel as its items panel).
        /// </summary>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            ReorderListBoxItem itemContainer = (ReorderListBoxItem)element;
            if (itemContainer == this.dragItemContainer)
            {
                this.dragItemContainer.Visibility = Visibility.Visible;
                this.dragItemContainer = null;
                Debug.WriteLine("ClearContainerForItemOverride dragItemContainer=null");
            }
        }

        #endregion

        #region Drag & drop reorder

        internal static T GetFirstLogicalChildByType<T>(FrameworkElement parent, bool applyTemplates)
            where T : FrameworkElement
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            Queue<FrameworkElement> queue = new Queue<FrameworkElement>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                var elementAsControl = element as Control;
                if (applyTemplates && elementAsControl != null)
                {
                    elementAsControl.ApplyTemplate();
                }

                if (element is T && element != parent)
                {
                    return (T)element;
                }

                foreach (FrameworkElement visualChild in GetVisualChildren(element).OfType<FrameworkElement>())
                {
                    queue.Enqueue(visualChild);
                }
            }

            return null;
        }

        public static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject target)
        {
            int count = VisualTreeHelper.GetChildrenCount(target);
            if (count == 0)
            {
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    yield return VisualTreeHelper.GetChild(target, i);
                }
            }
            yield break;
        }

        private bool _isManipulating;
        private DateTime? _manipulationTimestamp;
        /// <summary>
        /// Called when the user presses down on the transparent drag-interceptor. Identifies the targed
        /// drag handle and list item and prepares for a drag operation.
        /// </summary>
        private void dragInterceptor_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            Debug.WriteLine("ManipulationDelta dropTargetIndex=" + dropTargetIndex + " dragItemContainer=" + (dragItemContainer != null ? dragItemContainer.Content : "null") + " dragItem=" + dragItem);
           
 
            _isManipulating = true;

            _manipulationTimestamp = DateTime.Now;
            var timestamp = _manipulationTimestamp;
            GeneralTransform interceptorTransform;
            List<UIElement> targetElements;
            var targetItemContainer = InterceptorTransform(e, out targetElements, out interceptorTransform);
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (!_isManipulating) return;
                    if (timestamp != _manipulationTimestamp) return;

                    _page = _page ?? GetFirstLogicalChildByType<PhoneApplicationPage>(Application.Current.RootVisual as FrameworkElement, false);
                    _page.NavigationService.Navigated += OnPageNavigated;
                    if (this.dragItem != null)
                    {
                        return;
                    }

                    if (this.itemsPanel == null)
                    {
                        ItemsPresenter scrollItemsPresenter = (ItemsPresenter) GetFirstLogicalChildByType<ItemsPresenter>(this.scrollViewer, false);
                        this.itemsPanel = (Panel)VisualTreeHelper.GetChild(scrollItemsPresenter, 0);
                    }

                    var newTargetItemContainer = InterceptorTransform(e, out targetElements, out interceptorTransform);
                    if (newTargetItemContainer != targetItemContainer)
                    {
                        return;
                    }

                    targetItemContainer = newTargetItemContainer;
                    if (targetItemContainer != null && targetElements.Contains(targetItemContainer.DragHandle))
                    {
                        VisualStateManager.GoToState(targetItemContainer, ReorderListBoxItem.DraggingState, true);

                        GeneralTransform targetItemTransform = targetItemContainer.TransformToVisual(this.dragInterceptor);
                        Point targetItemOrigin = targetItemTransform.Transform(new Point(0, 0));
                        Canvas.SetLeft(this.dragIndicator, targetItemOrigin.X);
                        Canvas.SetTop(this.dragIndicator, targetItemOrigin.Y);
                        this.dragIndicator.Width = targetItemContainer.RenderSize.Width;
                        this.dragIndicator.Height = targetItemContainer.RenderSize.Height;

                        this.dragItemContainer = targetItemContainer;
                        this.dragItem = this.dragItemContainer.Content;
                        this.isDragItemSelected = this.dragItemContainer.IsSelected;

                        dragInterceptorRect = interceptorTransform.TransformBounds(new Rect(new Point(0, 0), this.dragInterceptor.RenderSize));

                        this.dropTargetIndex = -1;
                        Debug.WriteLine("ManipulationStarted dropTargetIndex=" + dropTargetIndex);
                    }
                });
            });

        }

        private ReorderListBoxItem InterceptorTransform(ManipulationStartedEventArgs e, out List<UIElement> targetElements, out GeneralTransform interceptorTransform)
        {
            interceptorTransform = dragInterceptor.TransformToVisual(Application.Current.RootVisual);
            
            var targetPoint = interceptorTransform.Transform(e.ManipulationOrigin);
            targetPoint = GetHostCoordinates(targetPoint);

            targetElements = VisualTreeHelper.FindElementsInHostCoordinates(targetPoint, itemsPanel).ToList();

            return targetElements.OfType<ReorderListBoxItem>().FirstOrDefault();
        }

        private void OnPageNavigated(object sender, NavigationEventArgs e)
        {
            _isManipulating = false;
            _manipulationTimestamp = null;

            Debug.WriteLine("PageNavigated initiator=" + e.IsNavigationInitiator);

            dragInterceptor_ManipulationCompleted(sender, null);

            _page.NavigationService.Navigated -= OnPageNavigated;
        }

        /// <summary>
        /// Called when the user drags on (or from) the transparent drag-interceptor.
        /// Moves the item (actually a rendered snapshot of the item) according to the drag delta.
        /// </summary>
        private void dragInterceptor_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Debug.WriteLine("ManipulationDelta dropTargetIndex=" + dropTargetIndex + " dragItemContainer=" + (dragItemContainer != null ? dragItemContainer.Content : "null") + " dragItem=" + dragItem);
            

            if (e.PinchManipulation != null)
            {
                dragInterceptor_ManipulationCompleted(sender, null);

                return;
            }

            if (this.Items.Count <= 1 || this.dragItem == null)
            {
                if (_manipulationTimestamp != null)
                {
                    _isManipulating = false;
                    _manipulationTimestamp = null;
                }

                return;
            }

            if (this.dropTargetIndex == -1)
            {
                if (this.dragItemContainer == null)
                {
                    dragItem = null;
                    return;
                }

                // When the drag actually starts, swap out the item for the drag-indicator image of the item.
                // This is necessary because the item itself may be removed from the virtualizing panel
                // if the drag causes a scroll of considerable distance.
                Size dragItemSize = this.dragItemContainer.RenderSize;
                WriteableBitmap writeableBitmap = new WriteableBitmap(
                    (int)dragItemSize.Width, (int)dragItemSize.Height);

                // Swap states to force the transition to complete.
                VisualStateManager.GoToState(this.dragItemContainer, ReorderListBoxItem.NotDraggingState, false);
                VisualStateManager.GoToState(this.dragItemContainer, ReorderListBoxItem.DraggingState, false);
                writeableBitmap.Render(this.dragItemContainer, null);

                writeableBitmap.Invalidate();
                this.dragIndicator.Source = writeableBitmap;

                this.dragIndicator.Visibility = Visibility.Visible;
                this.dragItemContainer.Visibility = Visibility.Collapsed;

                if (this.itemsPanel.Children.IndexOf(this.dragItemContainer) < this.itemsPanel.Children.Count - 1)
                {
                    this.UpdateDropTarget(Canvas.GetTop(this.dragIndicator) + this.dragIndicator.Height + 1, false);
                }
                else
                {
                    this.UpdateDropTarget(Canvas.GetTop(this.dragIndicator) - 1, false);
                }
            }

            double dragItemHeight = this.dragIndicator.Height;

            TranslateTransform translation = (TranslateTransform)this.dragIndicator.RenderTransform;
            double top = Canvas.GetTop(this.dragIndicator);

            // Limit the translation to keep the item within the list area.
            // Use different targeting for the top and bottom edges to allow taller items to
            // move before or after shorter items at the edges.
            double y = top + e.CumulativeManipulation.Translation.Y;
            if (y < 0)
            {
                y = 0;
                this.UpdateDropTarget(0, true);
            }
            else if (y >= this.dragInterceptorRect.Height - dragItemHeight)
            {
                y = this.dragInterceptorRect.Height - dragItemHeight;
                this.UpdateDropTarget(this.dragInterceptorRect.Height - 1, true);
            }
            else
            {
                this.UpdateDropTarget(y + dragItemHeight / 2, true);
            }

            translation.Y = y - top;

            // Check if we're within the margin where auto-scroll needs to happen.
            bool scrolling = (this.dragScrollDelta != 0);
            double autoScrollMargin = this.AutoScrollMargin;
            if (autoScrollMargin > 0 && y < autoScrollMargin)
            {
                this.dragScrollDelta = y - autoScrollMargin;
                if (!scrolling)
                {
                    VisualStateManager.GoToState(this.scrollViewer, ReorderListBox.ScrollViewerScrollingVisualState, true);
                    this.Dispatcher.BeginInvoke(() => this.DragScroll());
                    return;
                }
            }
            else if (autoScrollMargin > 0 && y + dragItemHeight > this.dragInterceptorRect.Height - autoScrollMargin)
            {
                this.dragScrollDelta = (y + dragItemHeight - (this.dragInterceptorRect.Height - autoScrollMargin));
                if (!scrolling)
                {
                    VisualStateManager.GoToState(this.scrollViewer, ReorderListBox.ScrollViewerScrollingVisualState, true);
                    this.Dispatcher.BeginInvoke(() => this.DragScroll());
                    return;
                }
            }
            else
            {
                // We're not within the auto-scroll margin. This ensures any current scrolling is stopped.
                this.dragScrollDelta = 0;
            }
        }

        /// <summary>
        /// Called when the user releases a drag. Moves the item within the source list and then resets everything.
        /// </summary>
        private void dragInterceptor_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _isManipulating = false;
            _manipulationTimestamp = null;


            Debug.WriteLine("ManipulationCompleted dropTargetIndex=" + dropTargetIndex + " dragItemContainer=" + (dragItemContainer != null ? dragItemContainer.Content : "null") + " dragItem=" + dragItem);

            if (_page != null)
            {
                _page.NavigationService.Navigated -= OnPageNavigated;
            }

            if (this.dragItem == null)
            {
                return;
            }

            if (this.dropTargetIndex >= 0)
            {
                this.MoveItem(this.dragItem, this.dropTargetIndex);
            }
            else
            {

            }

            if (this.dragItemContainer != null)
            {
                this.dragItemContainer.Visibility = Visibility.Visible;
                this.dragItemContainer.Opacity = 0;
                this.AnimateDrop(this.dragItemContainer, "manipulationCompleted");
                this.dragItemContainer = null;
            }
            else
            {

            }

            this.dragScrollDelta = 0;
            this.dropTargetIndex = -1;
            //Debug.WriteLine("ManipulationCompleted dropTargetIndex=" + dropTargetIndex);
            this.ClearDropTarget();

        }

        /// <summary>
        /// Slides the drag indicator (item snapshot) to the location of the dropped item,
        /// then performs the visibility swap and removes the dragging visual state.
        /// </summary>
        private void AnimateDrop(ReorderListBoxItem itemContainer, string from)
        {
            GeneralTransform itemTransform = itemContainer.TransformToVisual(this.dragInterceptor);

            Rect itemRect = itemTransform.TransformBounds(new Rect(new Point(0, 0), itemContainer.RenderSize));
            double delta = Math.Abs(itemRect.Y - Canvas.GetTop(this.dragIndicator) - ((TranslateTransform)this.dragIndicator.RenderTransform).Y);
            if (itemRect.Height == 0.0 || (itemContainer.RenderSize.Width == 0.0 && itemContainer.RenderSize.Height == 0.0))
            {
                delta = 0.0;
            }
            if (delta > 0.0)
            {
                // Adjust the duration based on the distance, so the speed will be constant.
                TimeSpan duration = TimeSpan.FromSeconds(0.25 * delta / itemRect.Height);
                Debug.WriteLine("Duration=" + duration + " from=" + from + " delta=" + delta + " itemRect.Height=" + itemRect.Height + " dragIndicator.Y=" + ((TranslateTransform)this.dragIndicator.RenderTransform).Y + " itemRect.Y=" + itemRect.Y + " Canvas.GetTop=" + Canvas.GetTop(this.dragIndicator));
                if (duration.TotalSeconds > 1.0)
                {
                    // There was no need for an animation, so do the visibility swap right now.
                    this.dragItem = null;
                    itemContainer.Opacity = 1;
                    this.dragIndicator.Visibility = Visibility.Collapsed;
                    this.dragIndicator.Source = null;
                    VisualStateManager.GoToState(itemContainer, ReorderListBoxItem.NotDraggingState, true);
                }
                else
                {
                    Storyboard dropStoryboard = new Storyboard();
                    DoubleAnimation moveToDropAnimation = new DoubleAnimation();
                    Storyboard.SetTarget(moveToDropAnimation, this.dragIndicator.RenderTransform);
                    Storyboard.SetTargetProperty(moveToDropAnimation, new PropertyPath(TranslateTransform.YProperty));
                    moveToDropAnimation.To = itemRect.Y - Canvas.GetTop(this.dragIndicator);
                    moveToDropAnimation.Duration = duration;
                    dropStoryboard.Children.Add(moveToDropAnimation);

                    dropStoryboard.Completed += delegate
                    {
                        this.dragItem = null;
                        itemContainer.Opacity = 1;
                        this.dragIndicator.Visibility = Visibility.Collapsed;
                        this.dragIndicator.Source = null;
                        ((TranslateTransform)this.dragIndicator.RenderTransform).Y = 0;
                        VisualStateManager.GoToState(itemContainer, ReorderListBoxItem.NotDraggingState, true);

                    };
                    dropStoryboard.Begin();
                }
            }
            else
            {
                // There was no need for an animation, so do the visibility swap right now.
                this.dragItem = null;
                itemContainer.Opacity = 1;
                this.dragIndicator.Visibility = Visibility.Collapsed;
                this.dragIndicator.Source = null;
                VisualStateManager.GoToState(itemContainer, ReorderListBoxItem.NotDraggingState, true);
            }
        }

        /// <summary>
        /// Automatically scrolls for as long as the drag is held within the margin.
        /// The speed of the scroll is adjusted based on the depth into the margin.
        /// </summary>
        private void DragScroll()
        {
            if (this.dragScrollDelta != 0)
            {
                double scrollRatio = this.scrollViewer.ViewportHeight / this.scrollViewer.RenderSize.Height;
                double adjustedDelta = this.dragScrollDelta * scrollRatio;
                double newOffset = this.scrollViewer.VerticalOffset + adjustedDelta;
                this.scrollViewer.ScrollToVerticalOffset(newOffset);

                this.Dispatcher.BeginInvoke(() => this.DragScroll());

                double dragItemOffset = Canvas.GetTop(this.dragIndicator) +
                    ((TranslateTransform)this.dragIndicator.RenderTransform).Y +
                    this.dragIndicator.Height / 2;
                this.UpdateDropTarget(dragItemOffset, true);
            }
            else
            {
                VisualStateManager.GoToState(this.scrollViewer, ReorderListBox.ScrollViewerNotScrollingVisualState, true);
            }
        }

        /// <summary>
        /// Updates spacing (drop target indicators) surrounding the targeted region.
        /// </summary>
        /// <param name="dragItemOffset">Vertical offset into the items panel where the drag is currently targeting.</param>
        /// <param name="showTransition">True if the drop-indicator transitions should be shown.</param>
        private void UpdateDropTarget(double dragItemOffset, bool showTransition)
        {
            Point dragPoint = ReorderListBox.GetHostCoordinates(
                new Point(this.dragInterceptorRect.Left, this.dragInterceptorRect.Top + dragItemOffset));
            IEnumerable<UIElement> targetElements = VisualTreeHelper.FindElementsInHostCoordinates(dragPoint, this.itemsPanel);
            ReorderListBoxItem targetItem = targetElements.OfType<ReorderListBoxItem>().FirstOrDefault();
            if (targetItem != null)
            {
                GeneralTransform targetTransform = targetItem.DragHandle.TransformToVisual(this.dragInterceptor);
                Rect targetRect = targetTransform.TransformBounds(new Rect(new Point(0, 0), targetItem.DragHandle.RenderSize));
                double targetCenter = (targetRect.Top + targetRect.Bottom) / 2;

                int targetIndex = this.itemsPanel.Children.IndexOf(targetItem);
                int childrenCount = this.itemsPanel.Children.Count;
                bool after = dragItemOffset > targetCenter;

                ReorderListBoxItem indicatorItem = null;
                if (!after && targetIndex > 0)
                {
                    ReorderListBoxItem previousItem = (ReorderListBoxItem)this.itemsPanel.Children[targetIndex - 1];
                    if (previousItem.Tag as string == ReorderListBoxItem.DropAfterIndicatorState)
                    {
                        indicatorItem = previousItem;
                    }
                }
                else if (after && targetIndex < childrenCount - 1)
                {
                    ReorderListBoxItem nextItem = (ReorderListBoxItem)this.itemsPanel.Children[targetIndex + 1];
                    if (nextItem.Tag as string == ReorderListBoxItem.DropBeforeIndicatorState)
                    {
                        indicatorItem = nextItem;
                    }
                }
                if (indicatorItem == null)
                {
                    targetItem.DropIndicatorHeight = this.dragIndicator.Height;
                    string dropIndicatorState = after ?
                        ReorderListBoxItem.DropAfterIndicatorState : ReorderListBoxItem.DropBeforeIndicatorState;
                    VisualStateManager.GoToState(targetItem, dropIndicatorState, showTransition);
                    targetItem.Tag = dropIndicatorState;
                    indicatorItem = targetItem;
                }

                for (int i = targetIndex - 5; i <= targetIndex + 5; i++)
                {
                    if (i >= 0 && i < childrenCount)
                    {
                        ReorderListBoxItem nearbyItem = (ReorderListBoxItem)this.itemsPanel.Children[i];
                        if (nearbyItem != indicatorItem)
                        {
                            VisualStateManager.GoToState(nearbyItem, ReorderListBoxItem.NoDropIndicatorState, showTransition);
                            nearbyItem.Tag = ReorderListBoxItem.NoDropIndicatorState;
                        }
                    }
                }

                this.UpdateDropTargetIndex(targetItem, after);
            }
        }

        /// <summary>
        /// Updates the targeted index -- that is the index where the item will be moved to if dropped at this point.
        /// </summary>
        private void UpdateDropTargetIndex(ReorderListBoxItem targetItemContainer, bool after)
        {
            int dragItemIndex = this.Items.IndexOf(this.dragItem);
            int targetItemIndex = this.Items.IndexOf(targetItemContainer.Content);

            int newDropTargetIndex;
            if (targetItemIndex == dragItemIndex)
            {
                newDropTargetIndex = dragItemIndex;
            }
            else
            {
                if (dragItemIndex == -1)
                {
                    newDropTargetIndex = targetItemIndex + (after ? 1 : 0);
                    Debug.WriteLine("   Catched UpdateDropTargetIndex dropTargetIndex=-1 targetItemIndex=" + targetItemIndex + " dragItemIndex=" + dragItemIndex + " after=" + after);
                }
                else
                {
                    newDropTargetIndex = targetItemIndex + (after ? 1 : 0) - (targetItemIndex >= dragItemIndex ? 1 : 0);
                }
            }

            if (newDropTargetIndex != this.dropTargetIndex)
            {
                this.dropTargetIndex = newDropTargetIndex;
                if (dropTargetIndex == -1)
                {
                    Debug.WriteLine("UpdateDropTargetIndex dropTargetIndex=-1 targetItemIndex=" + targetItemIndex + " dragItemIndex=" + dragItemIndex + " after=" + after);
                }
                else
                {
                    Debug.WriteLine("UpdateDropTargetIndex dropTargetIndex=" + dropTargetIndex);
                }
            }
        }

        /// <summary>
        /// Hides any drop-indicators that are currently visible.
        /// </summary>
        private void ClearDropTarget()
        {
            foreach (ReorderListBoxItem itemContainer in this.itemsPanel.Children)
            {
                VisualStateManager.GoToState(itemContainer, ReorderListBoxItem.NoDropIndicatorState, false);
                itemContainer.Tag = null;
            }
        }

        /// <summary>
        /// Moves an item to a specified index in the source list.
        /// </summary>
        private bool MoveItem(object item, int toIndex)
        {
            object itemsSource = this.ItemsSource;

            System.Collections.IList sourceList = itemsSource as System.Collections.IList;
            if (!(sourceList is System.Collections.Specialized.INotifyCollectionChanged))
            {
                // If the source does not implement INotifyCollectionChanged, then there's no point in
                // changing the source because changes to it will not be synchronized with the list items.
                // So, just change the ListBox's view of the items.
                sourceList = this.Items;
            }

            int fromIndex = sourceList.IndexOf(item);
            if (fromIndex != toIndex)
            {
                double scrollOffset = this.scrollViewer.VerticalOffset;

                Debug.WriteLine("Move item " + item + " toIndex=" + toIndex + " fromIndex=" + fromIndex);
                sourceList.RemoveAt(fromIndex);
                sourceList.Insert(toIndex, item);

                if (fromIndex <= scrollOffset && toIndex > scrollOffset)
                {
                    // Correct the scroll offset for the removed item so that the list doesn't appear to jump.
                    this.scrollViewer.ScrollToVerticalOffset(scrollOffset - 1);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region View range detection

        /// <summary>
        /// Gets the indices of the first and last items in the view based on the current scroll position.
        /// </summary>
        /// <param name="includePartial">True to include items that are partially obscured at the top and bottom,
        /// false to include only items that are completely in view.</param>
        /// <param name="firstIndex">Returns the index of the first item in view (or -1 if there are no items).</param>
        /// <param name="lastIndex">Returns the index of the last item in view (or -1 if there are no items).</param>
        public void GetViewIndexRange(bool includePartial, out int firstIndex, out int lastIndex)
        {
            if (this.Items.Count > 0)
            {
                firstIndex = 0;
                lastIndex = this.Items.Count - 1;

                if (this.scrollViewer != null && this.Items.Count > 1)
                {
                    Thickness scrollViewerPadding = new Thickness(
                        this.scrollViewer.BorderThickness.Left + this.scrollViewer.Padding.Left,
                        this.scrollViewer.BorderThickness.Top + this.scrollViewer.Padding.Top,
                        this.scrollViewer.BorderThickness.Right + this.scrollViewer.Padding.Right,
                        this.scrollViewer.BorderThickness.Bottom + this.scrollViewer.Padding.Bottom);

                    GeneralTransform scrollViewerTransform = this.scrollViewer.TransformToVisual(
                        Application.Current.RootVisual);
                    Rect scrollViewerRect = scrollViewerTransform.TransformBounds(
                        new Rect(new Point(0, 0), this.scrollViewer.RenderSize));

                    Point topPoint = ReorderListBox.GetHostCoordinates(new Point(
                        scrollViewerRect.Left + scrollViewerPadding.Left,
                        scrollViewerRect.Top + scrollViewerPadding.Top));
                    IEnumerable<UIElement> topElements = VisualTreeHelper.FindElementsInHostCoordinates(
                        topPoint, this.scrollViewer);
                    ReorderListBoxItem topItem = topElements.OfType<ReorderListBoxItem>().FirstOrDefault();
                    if (topItem != null)
                    {
                        GeneralTransform itemTransform = topItem.TransformToVisual(Application.Current.RootVisual);
                        Rect itemRect = itemTransform.TransformBounds(new Rect(new Point(0, 0), topItem.RenderSize));

                        firstIndex = this.ItemContainerGenerator.IndexFromContainer(topItem);
                        if (!includePartial && firstIndex < this.Items.Count - 1 &&
                            itemRect.Top < scrollViewerRect.Top && itemRect.Bottom < scrollViewerRect.Bottom)
                        {
                            firstIndex++;
                        }
                    }

                    Point bottomPoint = ReorderListBox.GetHostCoordinates(new Point(
                        scrollViewerRect.Left + scrollViewerPadding.Left,
                        scrollViewerRect.Bottom - scrollViewerPadding.Bottom - 1));
                    IEnumerable<UIElement> bottomElements = VisualTreeHelper.FindElementsInHostCoordinates(
                        bottomPoint, this.scrollViewer);
                    ReorderListBoxItem bottomItem = bottomElements.OfType<ReorderListBoxItem>().FirstOrDefault();
                    if (bottomItem != null)
                    {
                        GeneralTransform itemTransform = bottomItem.TransformToVisual(Application.Current.RootVisual);
                        Rect itemRect = itemTransform.TransformBounds(
                            new Rect(new Point(0, 0), bottomItem.RenderSize));

                        lastIndex = this.ItemContainerGenerator.IndexFromContainer(bottomItem);
                        if (!includePartial && lastIndex > firstIndex &&
                            itemRect.Bottom > scrollViewerRect.Bottom && itemRect.Top > scrollViewerRect.Top)
                        {
                            lastIndex--;
                        }
                    }
                }
            }
            else
            {
                firstIndex = -1;
                lastIndex = -1;
            }
        }

        #endregion

        #region Rearrange

        /// <summary>
        /// Private helper class for keeping track of each item involved in a rearrange.
        /// </summary>
        private class RearrangeItemInfo
        {
            public object Item = null;
            public int FromIndex = -1;
            public int ToIndex = -1;
            public double FromY = Double.NaN;
            public double ToY = Double.NaN;
            public double Height = Double.NaN;
        }

        /// <summary>
        /// Animates movements, insertions, or deletions in the list. 
        /// </summary>
        /// <param name="animationDuration">Duration of the animation.</param>
        /// <param name="rearrangeAction">Performs the actual rearrange on the list source.</param>
        /// <remarks>
        /// The animations are as follows:
        ///   - Inserted items fade in while later items slide down to make space.
        ///   - Removed items fade out while later items slide up to close the gap.
        ///   - Moved items slide from their previous location to their new location.
        ///   - Moved items which move out of or in to the visible area also fade out / fade in while sliding.
        /// <para>
        /// The rearrange action callback is called in the middle of the rearrange process. That
        /// callback may make any number of changes to the list source, in any order. After the rearrange
        /// action callback returns, the net result of all changes will be detected and included in a dynamically
        /// generated rearrange animation.
        /// </para><para>
        /// Multiple calls to this method in quick succession will be automatically queued up and executed in turn
        /// to avoid any possibility of conflicts. (If simultaneous rearrange animations are desired, use a single
        /// call to AnimateRearrange with a rearrange action callback that does both operations.)
        /// </para>
        /// </remarks>
        public void AnimateRearrange(Duration animationDuration, Action rearrangeAction)
        {
            if (rearrangeAction == null)
            {
                throw new ArgumentNullException("rearrangeAction");
            }

            if (this.rearrangeCanvas == null)
            {
                throw new InvalidOperationException("ReorderListBox control template is missing " +
                    "a part required for rearrange: " + ReorderListBox.RearrangeCanvasPart);
            }

            if (this.rearrangeQueue == null)
            {
                this.rearrangeQueue = new Queue<KeyValuePair<Action, Duration>>();
                this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.VerticalOffset); // Stop scrolling.
                this.Dispatcher.BeginInvoke(() =>
                    this.AnimateRearrangeInternal(rearrangeAction, animationDuration));
            }
            else
            {
                this.rearrangeQueue.Enqueue(new KeyValuePair<Action, Duration>(rearrangeAction, animationDuration));
            }
        }

        /// <summary>
        /// Orchestrates the rearrange animation process.
        /// </summary>
        private void AnimateRearrangeInternal(Action rearrangeAction, Duration animationDuration)
        {
            // Find the indices of items in the view. Animations are optimzed to only include what is visible.
            int viewFirstIndex, viewLastIndex;
            this.GetViewIndexRange(true, out viewFirstIndex, out viewLastIndex);

            // Collect information about items and their positions before any changes are made.
            RearrangeItemInfo[] rearrangeMap = this.BuildRearrangeMap(viewFirstIndex, viewLastIndex);

            // Call the rearrange action callback which actually makes the changes to the source list.
            // Assuming the source list is properly bound, the base class will pick up the changes.
            rearrangeAction();

            this.rearrangeCanvas.Visibility = Visibility.Visible;

            // Update the layout (positions of all items) based on the changes that were just made.
            this.UpdateLayout();

            // Find the NEW last-index in view, which may have changed if the items are not constant heights
            // or if the view includes the end of the list.
            viewLastIndex = this.FindViewLastIndex(viewFirstIndex);

            // Collect information about the NEW items and their NEW positions, linking up to information
            // about items which existed before.
            RearrangeItemInfo[] rearrangeMap2 = this.BuildRearrangeMap2(rearrangeMap,
                viewFirstIndex, viewLastIndex);

            // Find all the movements that need to be animated.
            IEnumerable<RearrangeItemInfo> movesWithinView = rearrangeMap
                .Where(rii => !Double.IsNaN(rii.FromY) && !Double.IsNaN(rii.ToY));
            IEnumerable<RearrangeItemInfo> movesOutOfView = rearrangeMap
                .Where(rii => !Double.IsNaN(rii.FromY) && Double.IsNaN(rii.ToY));
            IEnumerable<RearrangeItemInfo> movesInToView = rearrangeMap2
                .Where(rii => Double.IsNaN(rii.FromY) && !Double.IsNaN(rii.ToY));
            IEnumerable<RearrangeItemInfo> visibleMoves =
                movesWithinView.Concat(movesOutOfView).Concat(movesInToView);

            // Set a clip rect so the animations don't go outside the listbox.
            this.rearrangeCanvas.Clip = new RectangleGeometry() { Rect = new Rect(new Point(0, 0), this.rearrangeCanvas.RenderSize) };

            // Create the animation storyboard.
            Storyboard rearrangeStoryboard = this.CreateRearrangeStoryboard(visibleMoves, animationDuration);
            if (rearrangeStoryboard.Children.Count > 0)
            {
                // The storyboard uses an overlay canvas with item snapshots.
                // While that is playing, hide the real items.
                this.scrollViewer.Visibility = Visibility.Collapsed;

                rearrangeStoryboard.Completed += delegate
                {
                    rearrangeStoryboard.Stop();
                    this.rearrangeCanvas.Children.Clear();
                    this.rearrangeCanvas.Visibility = Visibility.Collapsed;
                    this.scrollViewer.Visibility = Visibility.Visible;

                    this.AnimateNextRearrange();
                };

                this.Dispatcher.BeginInvoke(() => rearrangeStoryboard.Begin());
            }
            else
            {
                this.rearrangeCanvas.Visibility = Visibility.Collapsed;
                this.AnimateNextRearrange();
            }
        }

        /// <summary>
        /// Checks if there's another rearrange action waiting in the queue, and if so executes it next.
        /// </summary>
        private void AnimateNextRearrange()
        {
            if (this.rearrangeQueue.Count > 0)
            {
                KeyValuePair<Action, Duration> nextRearrange = this.rearrangeQueue.Dequeue();
                this.Dispatcher.BeginInvoke(() =>
                    this.AnimateRearrangeInternal(nextRearrange.Key, nextRearrange.Value));
            }
            else
            {
                this.rearrangeQueue = null;
            }
        }

        /// <summary>
        /// Collects information about items and their positions before any changes are made.
        /// </summary>
        private RearrangeItemInfo[] BuildRearrangeMap(int viewFirstIndex, int viewLastIndex)
        {
            RearrangeItemInfo[] map = new RearrangeItemInfo[this.Items.Count];

            for (int i = 0; i < map.Length; i++)
            {
                object item = this.Items[i];

                RearrangeItemInfo info = new RearrangeItemInfo()
                {
                    Item = item,
                    FromIndex = i,
                };

                // The precise item location is only important if it's within the view.
                if (viewFirstIndex <= i && i <= viewLastIndex)
                {
                    ReorderListBoxItem itemContainer = (ReorderListBoxItem)
                        this.ItemContainerGenerator.ContainerFromIndex(i);
                    if (itemContainer != null)
                    {
                        GeneralTransform itemTransform = itemContainer.TransformToVisual(this.rearrangeCanvas);
                        Point itemPoint = itemTransform.Transform(new Point(0, 0));
                        info.FromY = itemPoint.Y;
                        info.Height = itemContainer.RenderSize.Height;
                    }
                }

                map[i] = info;
            }

            return map;
        }

        /// <summary>
        /// Collects information about the NEW items and their NEW positions after changes were made.
        /// </summary>
        private RearrangeItemInfo[] BuildRearrangeMap2(RearrangeItemInfo[] map,
            int viewFirstIndex, int viewLastIndex)
        {
            RearrangeItemInfo[] map2 = new RearrangeItemInfo[this.Items.Count];

            for (int i = 0; i < map2.Length; i++)
            {
                object item = this.Items[i];

                // Try to find the same item in the pre-rearrange info.
                RearrangeItemInfo info = map.FirstOrDefault(rii => rii.ToIndex < 0 && rii.Item == item);
                if (info == null)
                {
                    info = new RearrangeItemInfo()
                    {
                        Item = item,
                    };
                }

                info.ToIndex = i;

                // The precise item location is only important if it's within the view.
                if (viewFirstIndex <= i && i <= viewLastIndex)
                {
                    ReorderListBoxItem itemContainer = (ReorderListBoxItem)
                        this.ItemContainerGenerator.ContainerFromIndex(i);
                    if (itemContainer != null)
                    {
                        GeneralTransform itemTransform = itemContainer.TransformToVisual(this.rearrangeCanvas);
                        Point itemPoint = itemTransform.Transform(new Point(0, 0));
                        info.ToY = itemPoint.Y;
                        info.Height = itemContainer.RenderSize.Height;
                    }
                }

                map2[i] = info;
            }

            return map2;
        }

        /// <summary>
        /// Finds the index of the last visible item by starting at the first index and
        /// comparing the bounds of each following item to the ScrollViewer bounds.
        /// </summary>
        /// <remarks>
        /// This method is less efficient than the hit-test method used by GetViewIndexRange() above,
        /// but it works when the controls haven't actually been rendered yet, while the other doesn't.
        /// </remarks>
        private int FindViewLastIndex(int firstIndex)
        {
            int lastIndex = firstIndex;

            GeneralTransform scrollViewerTransform = this.scrollViewer.TransformToVisual(
                Application.Current.RootVisual);
            Rect scrollViewerRect = scrollViewerTransform.TransformBounds(
                new Rect(new Point(0, 0), this.scrollViewer.RenderSize));

            while (lastIndex < this.Items.Count - 1)
            {
                ReorderListBoxItem itemContainer = (ReorderListBoxItem)
                    this.ItemContainerGenerator.ContainerFromIndex(lastIndex + 1);
                if (itemContainer == null)
                {
                    break;
                }

                GeneralTransform itemTransform = itemContainer.TransformToVisual(
                    Application.Current.RootVisual);
                Rect itemRect = itemTransform.TransformBounds(new Rect(new Point(0, 0), itemContainer.RenderSize));
                itemRect.Intersect(scrollViewerRect);
                if (itemRect == Rect.Empty)
                {
                    break;
                }

                lastIndex++;
            }

            return lastIndex;
        }

        /// <summary>
        /// Creates a storyboard to animate the visible moves of a rearrange.
        /// </summary>
        private Storyboard CreateRearrangeStoryboard(IEnumerable<RearrangeItemInfo> visibleMoves,
            Duration animationDuration)
        {
            Storyboard storyboard = new Storyboard();

            ReorderListBoxItem temporaryItemContainer = null;

            foreach (RearrangeItemInfo move in visibleMoves)
            {
                Size itemSize = new Size(this.rearrangeCanvas.RenderSize.Width, move.Height);

                ReorderListBoxItem itemContainer = null;
                if (move.ToIndex >= 0)
                {
                    itemContainer = (ReorderListBoxItem)this.ItemContainerGenerator.ContainerFromIndex(move.ToIndex);
                }
                if (itemContainer == null)
                {
                    if (temporaryItemContainer == null)
                    {
                        temporaryItemContainer = new ReorderListBoxItem();
                    }

                    itemContainer = temporaryItemContainer;
                    itemContainer.Width = itemSize.Width;
                    itemContainer.Height = itemSize.Height;
                    this.rearrangeCanvas.Children.Add(itemContainer);
                    this.PrepareContainerForItemOverride(itemContainer, move.Item);
                    itemContainer.UpdateLayout();
                }

                WriteableBitmap itemSnapshot = new WriteableBitmap((int)itemSize.Width, (int)itemSize.Height);
                itemSnapshot.Render(itemContainer, null);
                itemSnapshot.Invalidate();

                Image itemImage = new Image();
                itemImage.Width = itemSize.Width;
                itemImage.Height = itemSize.Height;
                itemImage.Source = itemSnapshot;
                itemImage.RenderTransform = new TranslateTransform();
                this.rearrangeCanvas.Children.Add(itemImage);

                if (itemContainer == temporaryItemContainer)
                {
                    this.rearrangeCanvas.Children.Remove(itemContainer);
                }

                if (!Double.IsNaN(move.FromY) && !Double.IsNaN(move.ToY))
                {
                    Canvas.SetTop(itemImage, move.FromY);
                    if (move.FromY != move.ToY)
                    {
                        DoubleAnimation moveAnimation = new DoubleAnimation();
                        moveAnimation.Duration = animationDuration;
                        Storyboard.SetTarget(moveAnimation, itemImage.RenderTransform);
                        Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(TranslateTransform.YProperty));
                        moveAnimation.To = move.ToY - move.FromY;
                        storyboard.Children.Add(moveAnimation);
                    }
                }
                else if (Double.IsNaN(move.FromY) != Double.IsNaN(move.ToY))
                {
                    if (move.FromIndex >= 0 && move.ToIndex >= 0)
                    {
                        DoubleAnimation moveAnimation = new DoubleAnimation();
                        moveAnimation.Duration = animationDuration;
                        Storyboard.SetTarget(moveAnimation, itemImage.RenderTransform);
                        Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(TranslateTransform.YProperty));

                        const double animationDistance = 200;
                        if (!Double.IsNaN(move.FromY))
                        {
                            Canvas.SetTop(itemImage, move.FromY);
                            if (move.FromIndex < move.ToIndex)
                            {
                                moveAnimation.To = animationDistance;
                            }
                            else if (move.FromIndex > move.ToIndex)
                            {
                                moveAnimation.To = -animationDistance;
                            }
                        }
                        else
                        {
                            Canvas.SetTop(itemImage, move.ToY);
                            if (move.FromIndex < move.ToIndex)
                            {
                                moveAnimation.From = -animationDistance;
                            }
                            else if (move.FromIndex > move.ToIndex)
                            {
                                moveAnimation.From = animationDistance;
                            }
                        }

                        storyboard.Children.Add(moveAnimation);
                    }

                    DoubleAnimation fadeAnimation = new DoubleAnimation();
                    fadeAnimation.Duration = animationDuration;
                    Storyboard.SetTarget(fadeAnimation, itemImage);
                    Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

                    if (Double.IsNaN(move.FromY))
                    {
                        itemImage.Opacity = 0.0;
                        fadeAnimation.To = 1.0;
                        Canvas.SetTop(itemImage, move.ToY);
                    }
                    else
                    {
                        itemImage.Opacity = 1.0;
                        fadeAnimation.To = 0.0;
                        Canvas.SetTop(itemImage, move.FromY);
                    }

                    storyboard.Children.Add(fadeAnimation);
                }
            }

            return storyboard;
        }

        #endregion

        #region Private utility methods

        /// <summary>
        /// Gets host coordinates, adjusting for orientation. This is helpful when identifying what
        /// controls are under a point.
        /// </summary>
        private static Point GetHostCoordinates(Point point)
        {
            PhoneApplicationFrame frame = (PhoneApplicationFrame)Application.Current.RootVisual;
            switch (frame.Orientation)
            {
                case PageOrientation.LandscapeLeft: return new Point(frame.RenderSize.Width - point.Y, point.X);
                case PageOrientation.LandscapeRight: return new Point(point.Y, frame.RenderSize.Height - point.X);
                default: return point;
            }
        }

        #endregion
    }
}
