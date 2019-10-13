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
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Telegram.Api.Extensions;
using TelegramClient.Behaviors;

namespace TelegramClient.Views.Controls
{
    public partial class RibbonControl
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(RibbonControl), new PropertyMetadata(default(DataTemplate), OnItemTemplateChanged));

        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RibbonControl;
            if (control != null)
            {
                control.Items.ItemTemplate = e.NewValue as DataTemplate;
            }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate) GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IList), typeof(RibbonControl), new PropertyMetadata(default(IList), OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RibbonControl;
            if (control != null)
            {
                var collectionChangedOld = e.OldValue as INotifyCollectionChanged;
                if (collectionChangedOld != null)
                {
                    collectionChangedOld.CollectionChanged -= control.OnCollectionChanged;
                }

                control.Items.ItemsSource = e.NewValue as IList;
                control.Clear();

                var collectionChangedNew = e.NewValue as INotifyCollectionChanged;
                if (collectionChangedNew != null)
                {
                    collectionChangedNew.CollectionChanged += control.OnCollectionChanged;
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Clear();
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public RibbonControl()
        {
            InitializeComponent();

            Items.PrepareContainerForItem = PrepareContainerForItem;
        }

        private void PrepareContainerForItem(DependencyObject element, object item)
        {
            var contentPresenter = element as ContentPresenter;
            if (contentPresenter != null)
            {
                var index = Items.Items.IndexOf(item);
                contentPresenter.RenderTransform = new TranslateTransform { X = index * RibbonImageControl.MinExpandedWidth }; 

                var ribbonImageControl = GetChild<RibbonImageControl>(contentPresenter);
                if (ribbonImageControl != null)
                {
                    ribbonImageControl.Prepare();   
                }
            }
        }

        private int _selectedIndex = -1;

        private double _minTranslationX;

        private double _maxTranslationX;

        private double _translationX;

        private RibbonImageControl _previousImage;

        private ContentPresenter _previousContainer;

        private RibbonImageControl _currentImage;

        private ContentPresenter _currentContainer;

        private RibbonImageControl _nextImage;

        private ContentPresenter _nextContainer;

        public TranslateTransform Transform = new TranslateTransform();

        private void Clear()
        {
            _selectedIndex = -1;
            _minTranslationX = 0.0;
            _maxTranslationX = 0.0;
            _translationX = 0.0;
            _previousImage = null;
            _currentImage = null;
            _nextImage = null;
        }

        public bool ScrollTo(int index, double duration)
        {
            if (index == _selectedIndex)
            {
                AnimatePosition(_currentImage, _currentImage != null ? _currentImage.ExpandedWidth : RibbonImageControl.MinExpandedWidth, _toImage, RibbonImageControl.MinExpandedWidth, Transform, _translationX, duration);
                
                return true;
            }

            ContentPresenter fromContainer;
            ContentPresenter toContainer;
            var fromItem = FindAtIndex<RibbonImageControl>(Items, _selectedIndex, out fromContainer);
            var toItem = FindAtIndex<RibbonImageControl>(Items, index, out toContainer);

            if (toItem == null)
            {
                var translationX = index > _selectedIndex ? _minTranslationX : _maxTranslationX;
                AnimatePosition(fromItem, RibbonImageControl.MinExpandedWidth, null, 0, Transform, translationX, duration);

                return false;
            }

            _selectedIndex = index;

            _previousImage = FindAtIndex<RibbonImageControl>(Items, _selectedIndex - 1, out _previousContainer);
            _currentImage = toItem;
            _currentContainer = toContainer;
            _nextImage = FindAtIndex<RibbonImageControl>(Items, _selectedIndex + 1, out _nextContainer);

            _minTranslationX = -(_selectedIndex + 1) * RibbonImageControl.MinExpandedWidth - (_nextImage != null ? _nextImage.ExpandedWidth / 2.0 : RibbonImageControl.MaxExpandedWidth / 2.0);
            _maxTranslationX = -(_selectedIndex - 1) * RibbonImageControl.MinExpandedWidth - (_previousImage != null ? _previousImage.ExpandedWidth / 2.0 : RibbonImageControl.MinExpandedWidth - RibbonImageControl.MaxExpandedWidth / 2.0);
            _translationX = -(RibbonImageControl.MinExpandedWidth * index + toItem.ExpandedWidth / 2.0);

            AnimatePosition(fromItem, RibbonImageControl.MinExpandedWidth, toItem, toItem.ExpandedWidth, Transform, -(RibbonImageControl.MinExpandedWidth * index + toItem.ExpandedWidth / 2.0), duration);

            return true;
        }

        public bool ScrollNext(double duration)
        {
            return ScrollTo(_selectedIndex + 1, duration);
        }

        public bool ScrollPrevious(double duration)
        {
            return ScrollTo(_selectedIndex - 1, duration);
        }

        public bool ScrollBack(double duration)
        {
            return ScrollTo(_selectedIndex, duration);
        }

        private void AnimatePosition(RibbonImageControl currentControl, double currentWidth, RibbonImageControl nextControl, double nextWidth, TranslateTransform transform, double translateXAll, double duration)
        {
            var storyboard = new Storyboard();

//#if EXTENDED_LENGTH
            if (currentControl != null)
            {
                var currentWidthAnimation = new DoubleAnimation();
                currentWidthAnimation.To = currentWidth;
                currentWidthAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
                Storyboard.SetTarget(currentWidthAnimation, currentControl);
                Storyboard.SetTargetProperty(currentWidthAnimation, new PropertyPath("Width"));
                storyboard.Children.Add(currentWidthAnimation);
            }

            if (nextControl != null && nextControl != currentControl)
            {
                var nextWidthAnimation = new DoubleAnimation();
                nextWidthAnimation.To = nextWidth;
                nextWidthAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
                Storyboard.SetTarget(nextWidthAnimation, nextControl);
                Storyboard.SetTargetProperty(nextWidthAnimation, new PropertyPath("Width"));
                storyboard.Children.Add(nextWidthAnimation);
            }
//#endif

            //var translateXAnimation = new DoubleAnimation();
            //translateXAnimation.To = translateX;
            //translateXAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            //Storyboard.SetTarget(translateXAnimation, transform);
            //Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("X"));
            //storyboard.Children.Add(translateXAnimation);

            var x = 0.0;
            for (var i = 0; i < Items.Items.Count; i++)
            {
                ContentPresenter container;
                var image = FindAtIndex<RibbonImageControl>(Items, i, out container);
                if (container != null)
                {
                    var animation = new DoubleAnimation();
                    animation.To = x + translateXAll;
                    animation.Duration = new Duration(TimeSpan.FromSeconds(duration));
                    Storyboard.SetTarget(animation, container.RenderTransform);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("X"));
                    storyboard.Children.Add(animation);

                    //container.RenderTransform = new TranslateTransform { X = x };
                    //x += image.Width;

                    if (image == currentControl)
                    {
                        x += currentWidth;
                    }
                    else if (image == nextControl)
                    {
                        x += nextWidth;
                    }
                    else if (image != null)
                    {
                        x += image.Width;
                    }
                }
            }


            storyboard.Begin();
        }

        private RibbonImageControl _toImage;

        private ContentPresenter _toContainer;

        public void Move(double percent)
        {
            percent = PanAndZoomBehavior.Clamp(percent, -1.0, 1.0);

            var translateX = percent < 0.0
                ? _translationX - (_minTranslationX - _translationX) * percent
                : _translationX + (_maxTranslationX - _translationX) * percent;

            //Transform.X = translateX;

            if (_currentImage != null)
            {
                _currentImage.Width = _currentImage.ExpandedWidth - (_currentImage.ExpandedWidth - RibbonImageControl.MinExpandedWidth) * Math.Abs(percent);
            }

            _toImage = percent < 0.0 ? _nextImage : _previousImage;
            if (_toImage != null)
            {
                _toImage.Width = RibbonImageControl.MinExpandedWidth + (_toImage.ExpandedWidth - RibbonImageControl.MinExpandedWidth) * Math.Abs(percent);
            }

            _toContainer = percent < 0.0 ? _nextContainer : _previousContainer;

            var x = 0.0;
            for (var i = 0; i < Items.Items.Count; i++)
            {
                ContentPresenter container;
                var image = FindAtIndex<RibbonImageControl>(Items, i, out container);
                if (container != null)
                {
                    container.RenderTransform = new TranslateTransform { X = x + translateX };
                    x += image.Width;

                    //if (container == _toContainer)
                    //{
                    //    x += _toImage.Width;
                    //}
                    //else if (container == _currentContainer)
                    //{
                    //    x += _currentImage.Width;
                    //}
                    //else if (container != null)
                    //{
                    //    x += image.Width;
                    //}
                }
            }
        }

        private static T FindAtIndex<T>(ItemsControl itemsControl, int index, out ContentPresenter container) where T : DependencyObject
        {
            container = null;
            var item = index >= 0 && index < itemsControl.Items.Count ? itemsControl.Items[index] : null;
            if (item != null)
            {
                container = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as ContentPresenter;
                if (container != null)
                {
                    return GetChild<T>(container);
                }
            }
            return null;
        }

        private static T GetChild<T>(DependencyObject obj) where T : DependencyObject
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = child as T;
                if (result != null)
                {
                    return result;
                }
                result = GetChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
    public class CachingItemsControl : ItemsControl
    {
        private readonly Stack<DependencyObject> _cache =
            new Stack<DependencyObject>();

        protected override DependencyObject GetContainerForItemOverride()
        {
            return
                _cache.Count > 0
                ? _cache.Pop()
                : base.GetContainerForItemOverride();
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            _cache.Push(element);
        }

        public Action<DependencyObject, object> PrepareContainerForItem; 

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            PrepareContainerForItem.SafeInvoke(element, item);

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
