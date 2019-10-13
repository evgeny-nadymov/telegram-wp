// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;

namespace TelegramClient
{
    public class LongListSelectorEx : LongListSelector
    {
        public static readonly DependencyProperty IsSelectionEnabledProperty = DependencyProperty.Register(
            "IsSelectionEnabled", typeof (bool), typeof (LongListSelectorEx), new PropertyMetadata(OnIsSelectionEnabledChanged));

        private static void OnIsSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lls = (LongListSelectorEx) d;
            
            var projection = lls.Projection as PlaneProjection;
            var upDown = projection != null && projection.RotationZ == 180.0;
            var x = upDown ? -48.0 : 48.0;
            
            var viewport = lls.Viewport;
            if ((bool)e.NewValue)
            {
                var storyboard = new Storyboard();
                if (!(viewport.RenderTransform is CompositeTransform))
                {
                    viewport.RenderTransform = new CompositeTransform();
                }

                if (viewport.CacheMode == null)
                {
                    viewport.CacheMode = new BitmapCache();
                }

                var translateXAnimation = new DoubleAnimationUsingKeyFrames();
                translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
                translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.55), Value = x, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
                Storyboard.SetTarget(translateXAnimation, viewport);
                Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                storyboard.Children.Add(translateXAnimation);

                //storyboard.Begin();
                Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
            }
            else
            {
                var storyboard = new Storyboard();
                if (!(viewport.RenderTransform is CompositeTransform))
                {
                    viewport.RenderTransform = new CompositeTransform();
                }

                var translateXAnimation = new DoubleAnimationUsingKeyFrames();
                translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = x });
                translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.55), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
                Storyboard.SetTarget(translateXAnimation, viewport);
                Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                storyboard.Children.Add(translateXAnimation);

                //storyboard.Begin();
                Deployment.Current.Dispatcher.BeginInvoke(() => storyboard.Begin());
            }
        }

        public bool IsSelectionEnabled
        {
            get { return (bool) GetValue(IsSelectionEnabledProperty); }
            set { SetValue(IsSelectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsFirstSliceLoadedProperty = DependencyProperty.Register(
            "IsFirstSliceLoaded", typeof(bool), typeof(LongListSelectorEx), new PropertyMetadata(true));

        public bool IsFirstSliceLoaded
        {
            get { return (bool)GetValue(IsFirstSliceLoadedProperty); }
            set { SetValue(IsFirstSliceLoadedProperty, value); }
        }

        public int MeasureOverrideCount { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("LLS MeasureOverride " + MeasureOverrideCount);
                MeasureOverrideCount++;
                return base.MeasureOverride(availableSize);
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("LLS MeasureOverride catch " + MeasureOverrideCount);

#if DEBUG
                MessageBox.Show("LongListSelectorEx.MeasureOverride ex " + e);
#endif

                return base.MeasureOverride(availableSize);
            }
        }

        private int _knob = 1;

        public int Knob
        {
            get { return _knob; }
            set { _knob = value; }
        }

        public ScrollBar VerticalScrollBar { get; protected set; }

        public ViewportControl Viewport { get; protected set; }

        public LongListSelectorEx()
        {
            ItemRealized += OnItemRealized;
        }

        public override void OnApplyTemplate()
        {
            Viewport = (ViewportControl)GetTemplateChild("ViewportControl");

            

            Viewport.ViewportChanged += OnViewportChanged;
            Viewport.ManipulationStateChanged += OnManipulationStateChanged;

            base.OnApplyTemplate();
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static IEnumerable<T> GetChildrenOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i) as T;

                if (child != null) yield return child;
            }
        }

        private void OnManipulationStateChanged(object sender, ManipulationStateChangedEventArgs e)
        {
            
        }

        private double _fromBegin;

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            if (Viewport.Bounds.Y - Viewport.Viewport.Y == 0.0)
            {
                RaiseBegin();
            }

            var fromBegin = Viewport.Viewport.Y - Viewport.Bounds.Y;
            if (fromBegin >= 500.0 && _fromBegin < 500.0)
            {
                System.Diagnostics.Debug.WriteLine("RaiseShowScrollButton");
                RaiseShowScrollButton();
            }
            _fromBegin = fromBegin;

            if ((Viewport.Bounds.Height + Viewport.Bounds.Y) >= ActualHeight
                && (Viewport.Bounds.Height + Viewport.Bounds.Y) == (Viewport.Viewport.Height + Viewport.Viewport.Y))
            {
                //Telegram.Api.Helpers.Execute.ShowDebugMessage("CloseToEnd ActualHeight=" + ActualHeight + " Height+Y=" + (Viewport.Bounds.Height + Viewport.Bounds.Y));
                RaiseCloseToEnd();
            }

            RaiseViewportChanged(e);
            //System.Diagnostics.Debug.WriteLine("bounds={0} viewport={1}", Viewport.Bounds, Viewport.Viewport);
        }

        public bool IsHoldingScrollingPosition
        {
            get { return ListHeader != null; }
        }

        public void HoldScrollingPosition()
        {
            ListHeader = null;
        }

        public void UnholdScrollingPosition()
        {
            ListHeader = new Border { Visibility = Visibility.Collapsed };
        }

        public void ScrollToItem(object item)
        {
            if (ItemsSource.Count > 0 && ItemsSource[0] == item)
            {
                if (ItemsSource[0] == item && Viewport.Bounds.Y == Viewport.Viewport.Y)
                {
                    //MessageBox.Show("ScrollToBottom optimization");
                    return;
                }
            }

            ScrollTo(item);
        }

        public event EventHandler<ViewportChangedEventArgs> ViewportChanged;

        protected virtual void RaiseViewportChanged(ViewportChangedEventArgs e)
        {
            var handler = ViewportChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler ShowScrollButton;

        protected virtual void RaiseShowScrollButton()
        {
            var handler = ShowScrollButton;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler CloseToEnd;

        protected virtual void RaiseCloseToEnd()
        {
            var handler = CloseToEnd;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler CloseToBegin;

        protected virtual void RaiseCloseToBegin()
        {
            var handler = CloseToBegin;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler Begin;

        protected virtual void RaiseBegin()
        {
            var handler = Begin;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }


        public static readonly DependencyProperty DownButtonVisibilityProperty = DependencyProperty.Register(
            "DownButtonVisibility", typeof(Visibility), typeof(LongListSelectorEx), new PropertyMetadata(Visibility.Collapsed));

        public Visibility DownButtonVisibility
        {
            get { return (Visibility)GetValue(DownButtonVisibilityProperty); }
            set { SetValue(DownButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty PrevIndexProperty = DependencyProperty.Register(
            "PrevIndex", typeof(int), typeof(LongListSelectorEx), new PropertyMetadata(default(int)));

        public int PrevIndex
        {
            get { return (int)GetValue(PrevIndexProperty); }
            set { SetValue(PrevIndexProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
            "Index", typeof(int), typeof(LongListSelectorEx), new PropertyMetadata(default(int)));

        private Canvas _canvas;

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        private bool _check;

        private void OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (_check)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("OnItemRealized " + InView(e.Container));
            }
            //MessageBox.Show("ItemRealized");
            //OnViewportChanged(sender, new ViewportChangedEventArgs());

            var longListSelector = this;

            var item = e.Container.Content;


            var items = longListSelector.ItemsSource;
            var index = items.IndexOf(item);

            //if (items.Count >= Knob
            //    && e.Container.Content.Equals(longListSelector.ItemsSource[longListSelector.ItemsSource.Count - Knob]))
            //{
            //    InvokeActions(null);
            //}
            if (index > 20
                && IsFirstSliceLoaded
                && PrevIndex > index)
            {
                DownButtonVisibility = Visibility.Visible;
            }
            else
            {
                DownButtonVisibility = Visibility.Collapsed;
                //_prevIndex = 0;
            }

            PrevIndex = index;

            if (items.Count - index <= Knob)
            {
                if (ManipulationState != ManipulationState.Idle)
                {
                    RaiseCloseToEnd();
                    return;
                }
            }

            if (index <= Knob)
            {
                if (ManipulationState != ManipulationState.Idle)
                {
                    RaiseCloseToBegin();
                    return;
                }
            }

            if (LayoutMode == LongListSelectorLayoutMode.List)
            {
                var message = e.Container.Content as TLMessageBase;
                if (message == null) return;

                if (!message._isAnimated)
                {
                    e.Container.Opacity = 1.0;
                    return;
                }

                message._isAnimated = false;

                if (Visibility == Visibility.Collapsed) return;

                e.Container.Opacity = 0.0;

                if (e.Container.Tag != null && (bool) e.Container.Tag)
                {
                    StartLoadingAnimation(e.Container);
                    return;
                }
                e.Container.Loaded += OnContainerLoaded;
                e.Container.Unloaded += OnContainerUnloaded;
            }
        }

        private void OnContainerUnloaded(object sender, RoutedEventArgs e)
        {
            var container = (ContentPresenter)sender;
            container.Tag = false;
            container.Unloaded -= OnContainerUnloaded;
        }

        private void OnContainerLoaded(object sender, RoutedEventArgs e)
        {
            var container = (ContentPresenter)sender;
            container.Tag = true;
            container.Loaded -= OnContainerLoaded;
            
            StartLoadingAnimation(container);
        }

        private void StartLoadingAnimation(ContentPresenter container)
        {
            var message = container.Content as TLMessageBase;
            if (message!= null
                && message.Index == 160952)
            {
                TLUtils.WriteLine("startAnimation", LogSeverity.Error);
            }

            container.CacheMode = new BitmapCache();
            var storyboard = new Storyboard();
            var opacityAnimation = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromSeconds(1.0) };
            Storyboard.SetTarget(opacityAnimation, container);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        public IList<ContentPresenter> GetItemsInView()
        {
            if (_canvas == null)
            {
                var contentPresenter = GetChildOfType<ContentPresenter>(Viewport);
                var canvas = GetChildOfType<Canvas>(contentPresenter);
                _canvas = GetChildOfType<Canvas>(canvas);
            }

            var items = new List<ContentPresenter>();
            foreach (var item in GetChildrenOfType<ContentPresenter>(_canvas))
            {
                if (InView(item))items.Add(item);
                //var itemViewModel = item.DataContext as ItemViewModel;
                //if (itemViewModel != null)
                //{
                //    itemViewModel.IsSelected = isSelected;
                //}
                //Debug.WriteLine(Canvas.GetTop(item) + " " + item.ActualHeight + " " + item.DataContext);
            }

            return items;
        }

        private bool InView(ContentPresenter item)
        {
            var height = Viewport.Viewport.Height < ActualHeight ? ActualHeight : Viewport.Viewport.Height;
            var top = Canvas.GetTop(item);
            if (top >= (Viewport.Viewport.Y - item.ActualHeight / 3.0 * 2.0) && (top + item.ActualHeight <= Viewport.Viewport.Y + height + item.ActualHeight / 3.0 * 2.0))
            {
                return true;
            }

            return false;
        }
    }
}
