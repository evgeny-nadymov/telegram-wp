using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls.Primitives;

namespace TelegramClient.Controls.StartView
{
    /// <summary>
    /// Creates a panoramic view of items that can be panned side-to-side, similar to the Start screen.
    /// </summary>
    [TemplatePart(Name = PanningTransformName, Type = typeof(TranslateTransform))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(StartViewItem))]
    public class StartView : TemplatedItemsControl<StartViewItem>, ISupportInitialize
    {
        internal static readonly Duration Immediately = TimeSpan.Zero;
        private static readonly Duration DefaultDuration = TimeSpan.FromMilliseconds(300);
        private static readonly Duration FlickDuration = DefaultDuration;
        private static readonly Duration SnapDuration = DefaultDuration;
        private static readonly Duration PanDuration = TimeSpan.FromMilliseconds(300);

        private const string PanningTransformName = "PanningTransform";

        private readonly IEasingFunction _easingFunction = new ExponentialEase { Exponent = 5 };

        private int _cumulativeDragDelta;
        private int _effectiveDragDelta;
        private int _flickDirection;
        private int _targetOffset;
        private bool _dragged;
        private bool _adjustSelectedRequested;
        private bool _suppressSelectionChangedEvent;
        private bool _loaded;
        private TransformAnimator _animator;
        private bool _suppressAnimation;
        private bool _ignorePropertyChange;
        private bool _isDesignTime;
        private InitializingData _initializingData;

        /// <summary>
        /// Initializes a new instance of the StartView class.
        /// </summary>
        public StartView()
        {
            DefaultStyleKey = typeof(StartView);

            GestureHelper gestureHelper = GestureHelper.Create(this, true);
            gestureHelper.GestureStart += (sender, args) => GestureStart();
            gestureHelper.HorizontalDrag += (sender, args) => HorizontalDrag(args);
            gestureHelper.Flick += (sender, args) => Flick(args);
            gestureHelper.GestureEnd += (sender, args) => GestureEnd();

            SizeChanged += OnSizeChanged;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _isDesignTime = DesignerProperties.IsInDesignTool;
        }

        internal StartViewPanel Panel { get; set; }

        internal int ItemsWidth { get; set; }

        internal int ViewportWidth { get; private set; }

        internal int ViewportHeight { get; private set; }

        private TranslateTransform PanningTransform { get; set; }

        private int ActualOffset
        {
            get { return PanningTransform == null ? 0 : (int)PanningTransform.X; }
        }

        private int SelectionOffset
        {
            get
            {
                StartViewItem container = GetContainer(SelectedItem);
                if (container != null)
                {
                    return -container.StartPosition;
                }
                return 0;
            }
        }

        private bool IsInit
        {
            get { return _initializingData != null; }
        }

        /// <summary>
        /// Identifies the SelectedItem dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(StartView),
            new PropertyMetadata(null, (d, e) => ((StartView)d).OnSelectedItemChanged(e)));

        /// <summary>
        /// Gets the selected item.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Object"/>.
        /// </returns>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the SelectedIndex dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(StartView),
            new PropertyMetadata(-1, (d, e) => ((StartView)d).OnSelectedIndexChanged(e)));

        /// <summary>
        /// Gets the selected index.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Int32"/>.
        /// </returns>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Event that is invoked when selection changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// Handles the application of a new Template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PanningTransform = GetTemplateChild(PanningTransformName) as TranslateTransform;

            _animator = PanningTransform != null ? new TransformAnimator(PanningTransform) : null;
        }

        /// <summary>
        /// Handles the measurement of the control.
        /// </summary>
        /// 
        /// <returns>
        /// Desired size.
        /// </returns>
        /// <param name="availableSize">Available size.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Application.Current.Host.Content.ActualWidth > 0)
            {
                ViewportWidth = !double.IsInfinity(availableSize.Width) ? (int)availableSize.Width : (int)Application.Current.Host.Content.ActualWidth;
                ViewportHeight = !double.IsInfinity(availableSize.Height) ? (int)availableSize.Height : (int)Application.Current.Host.Content.ActualHeight;
            }
            else
            {
                ViewportWidth = (int)Math.Min(availableSize.Width, 480);
                ViewportHeight = (int)Math.Min(availableSize.Height, 800);
            }

            base.MeasureOverride(new Size(double.PositiveInfinity, ViewportHeight));

            if (double.IsInfinity(availableSize.Width))
            {
                availableSize.Width = ViewportWidth;
            }

            if (double.IsInfinity(availableSize.Height))
            {
                availableSize.Height = ViewportHeight;
            }

            return availableSize;
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
            finalSize.Width = DesiredSize.Width;
            base.ArrangeOverride(finalSize);
            return finalSize;
        }

        /// <summary>
        /// When the items have changed, we need to adjust the selection.
        /// </summary>
        /// <param name="e">The changed item.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (!IsInit)
            {
                RequestAdjustSelection();
            }
        }

        internal void RequestAdjustSelection()
        {
            if (_adjustSelectedRequested)
            {
                return;
            }

            LayoutUpdated += LayoutUpdatedAdjustSelection;
            _adjustSelectedRequested = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _loaded = false;
        }

        private void LayoutUpdatedAdjustSelection(object sender, System.EventArgs e)
        {
            _adjustSelectedRequested = false;
            LayoutUpdated -= LayoutUpdatedAdjustSelection;
            AdjustSelection();
        }

        private void AdjustSelection()
        {
            if (_isDesignTime)
            {
                if (_loaded)
                {
                    _targetOffset = 0;
                    GoTo(_targetOffset, Immediately);
                }
            }
            else
            {
                object oldSelectedItem = SelectedItem;
                object newSelectedItem = null;

                if (Panel != null && Panel.VisibleChildren.Count > 0)
                {
                    if (oldSelectedItem == null)
                    {
                        newSelectedItem = GetItem(Panel.VisibleChildren[0]);
                    }
                    else
                    {
                        StartViewItem oldContainer = GetContainer(oldSelectedItem);
                        newSelectedItem = oldContainer == null || !Panel.VisibleChildren.Contains(oldContainer) ? GetItem(Panel.VisibleChildren[0]) : oldSelectedItem;
                    }
                }
                else
                {
                    _targetOffset = 0;
                    GoTo(_targetOffset, Immediately);
                }

                SetSelectionInternal(newSelectedItem);

                StartViewItem newContainer = GetContainer(newSelectedItem);
                if (newContainer != null)
                {
                    _targetOffset = -newContainer.StartPosition;
                    GoTo(_targetOffset, Immediately);
                }
            }
        }

        private void GestureStart()
        {
            _targetOffset = ActualOffset;
            _flickDirection = 0;
            _cumulativeDragDelta = 0;
            _effectiveDragDelta = 0;
            _dragged = false;
        }

        private void HorizontalDrag(DragEventArgs args)
        {
            if (_flickDirection == 0)
            {
                _cumulativeDragDelta = (int)args.CumulativeDistance.X;
                _effectiveDragDelta += (int)args.DeltaDistance.X;
                _targetOffset += (int)args.DeltaDistance.X;
                if (Math.Abs(_cumulativeDragDelta) <= ViewportWidth)
                {
                    if (_effectiveDragDelta > 0 && SelectedIndex == 0 || _effectiveDragDelta < 0 && SelectedIndex == Items.Count - 1)
                    {
                        _effectiveDragDelta = 0;
                        _targetOffset = ActualOffset;
                        return;
                    }

                    _dragged = true;
                    GoTo(_targetOffset, PanDuration);
                }
            }
        }

        private void Flick(FlickEventArgs e)
        {
            if (e.Angle == 180)
            {
                _flickDirection = -1;
            }
            else if (e.Angle == 0)
            {
                _flickDirection = 1;
            }
        }

        private void GestureEnd()
        {
            if (_flickDirection == 0)
            {
                if (_dragged)
                {
                    int snapTo;
                    int newDirection;
                    StartViewItem newSelection;

                    Panel.GetSnapOffset(_targetOffset, ViewportWidth, Math.Sign(_cumulativeDragDelta), out snapTo, out newDirection, out newSelection);

                    object newSelectedItem = GetItem(newSelection);
                    if (newSelectedItem != null)
                    {
                        _suppressAnimation = true;
                        SelectedItem = newSelectedItem;
                        _suppressAnimation = false;
                    }

                    GoTo(snapTo, SnapDuration);
                }
            }
            else
            {
                ProcessFlick();
            }
        }

        private void ProcessFlick()
        {
            if (_flickDirection != 0)
            {
                StartViewPanel.ItemStop previous;
                StartViewPanel.ItemStop current;
                StartViewPanel.ItemStop next;

                Panel.GetStops(SelectionOffset, ItemsWidth, out previous, out current, out next);

                if (previous == current && current == next && next == null)
                {
                    return;
                }

                if (_flickDirection < 0 && next == null || _flickDirection > 0 && previous == null)
                {
                    if (current != null)
                    {
                        GoTo(-current.Position, Immediately);
                    }

                    return;
                }

                _targetOffset = _flickDirection < 0 ? -next.Position : -previous.Position;

                _suppressAnimation = true;
                SelectedItem = GetItem(_flickDirection < 0 ? next.Item : previous.Item);
                _suppressAnimation = false;

                GoTo(_targetOffset, FlickDuration);
            }
        }

        private void GoTo(int offset, Duration duration, Action completionAction)
        {
            if (_animator != null)
            {
                _animator.GoTo(offset, duration, _easingFunction, completionAction);
            }
        }

        private void GoTo(int offset)
        {
            GoTo(offset, null);
        }

        private void GoTo(int offset, Action completionAction)
        {
            int delta = Math.Abs(ActualOffset - offset);
            GoTo(offset, TimeSpan.FromMilliseconds(delta * 2), completionAction);
        }

        private void GoTo(int offset, Duration duration)
        {
            GoTo(offset, duration, null);
        }

        private void SetSelectionInternal(object selectedItem)
        {
            _suppressSelectionChangedEvent = true;
            SelectedItem = selectedItem;
            _suppressSelectionChangedEvent = false;
        }

        private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs args)
        {
            if (IsInit)
            {
                return;
            }

            if (_ignorePropertyChange)
            {
                _ignorePropertyChange = false;
                return;
            }

            if (!_isDesignTime)
            {
                if (args.NewValue == null && Items.Count > 0)
                {
                    _ignorePropertyChange = true;
                    SelectedItem = args.OldValue;
                    throw new ArgumentException("SelectedItem");
                }
                else if (args.NewValue != null && !Items.Contains(args.NewValue))
                {
                    _ignorePropertyChange = true;
                    SelectedItem = args.OldValue;
                    return;
                }
            }

            SelectedIndex = Items.IndexOf(args.NewValue);

            if (_suppressSelectionChangedEvent)
            {
                return;
            }

            SafeRaise.Raise<SelectionChangedEventArgs>(SelectionChanged, this, (() =>
            {
                object[] unselected;
                if (args.OldValue != null)
                {
                    unselected = new object[1] { args.OldValue };
                }
                else
                {
                    unselected = new object[0];
                }

                object[] selected;
                if (args.NewValue != null)
                {
                    selected = new object[1] { args.NewValue };
                }
                else
                {
                    selected = new object[0];
                }

                return new SelectionChangedEventArgs(unselected, selected);
            }));

            if (!_suppressAnimation)
            {
                GoTo(SelectionOffset, _loaded ? DefaultDuration : Immediately);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs args)
        {
            if (IsInit)
            {
                return;
            }

            if (_ignorePropertyChange)
            {
                _ignorePropertyChange = false;
                return;
            }

            int newSelectedIndex = (int)args.NewValue;
            int itemsCount = Items.Count;

            if (newSelectedIndex >= 0 && newSelectedIndex < itemsCount)
            {
                SelectedItem = Items[newSelectedIndex];
            }
            else if (newSelectedIndex == -1 && itemsCount == 0)
            {
                SelectedItem = null;
            }
            else if (!_isDesignTime)
            {
                _ignorePropertyChange = true;
                SelectedIndex = (int)args.OldValue;
                throw new ArgumentOutOfRangeException("SelectedIndex");
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewportWidth = (int)e.NewSize.Width;
            ViewportHeight = (int)e.NewSize.Height;
            ItemsWidth = (int)Panel.ActualWidth;
        }

        void ISupportInitialize.BeginInit()
        {
            _initializingData = new InitializingData
            {
                InitialItem = SelectedItem,
                InitialIndex = SelectedIndex
            };
        }

        void ISupportInitialize.EndInit()
        {
            if (_initializingData == null)
            {
                throw new InvalidOperationException();
            }

            int selectedIndex = SelectedIndex;
            object selectedItem = SelectedItem;

            if (_initializingData.InitialIndex != selectedIndex)
            {
                SelectedIndex = _initializingData.InitialIndex;
                _initializingData = null;
                SelectedIndex = selectedIndex;
            }
            else if (!ReferenceEquals(_initializingData.InitialItem, selectedItem))
            {
                SelectedItem = _initializingData.InitialItem;
                _initializingData = null;
                SelectedItem = selectedItem;
            }

            _initializingData = null;
        }

        private class InitializingData
        {
            public int InitialIndex;
            public object InitialItem;
        }
    }
}
