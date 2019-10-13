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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Controls
{
    public partial class TelegramApplicationBar
    {
        public Brush BackgroundBrush
        {
            get { return LayoutRoot.Background; }
            set { LayoutRoot.Background = value; }
        }

        public Brush MorePanelBackgroundBrush
        {
            get { return _morePanelBrush; }
            set
            {
                _morePanelBrush = value;
                if (MorePanel != null)
                {
                    MorePanel.Background = _morePanelBrush;
                }
            }
        }

        public event EventHandler PanelOpened;

        protected virtual void RaisePanelOpened()
        {
            var handler = PanelOpened;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler PanelClosed;

        protected virtual void RaisePanelClosed()
        {
            EventHandler handler = PanelClosed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private static ApplicationBar _applicationBar;

        public static ApplicationBar ApplicationBar
        {
            get
            {
                if (_applicationBar == null)
                {
                    _applicationBar = new ApplicationBar();
                }

                return _applicationBar;
            }
        }

        public static double ApplicaitonBarDefaultSize1X
        {
            get { return 72.0; }
        }

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(
            "Buttons", typeof(IList), typeof(TelegramApplicationBar), new PropertyMetadata(default(IList), OnButtonsChanged));

        public IList Buttons
        {
            get { return (IList)GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramApplicationBar = d as TelegramApplicationBar;
            if (telegramApplicationBar != null)
            {
                var oldButtons = e.OldValue as IList;
                if (oldButtons != null)
                {
                    for (var i = oldButtons.Count - 1; i >= 0; i--)
                    {
                        var button = oldButtons[i] as FrameworkElement;
                        if (button != null)
                        {
                            button.Tap -= telegramApplicationBar.Button_OnTap;
                        }
                    }

                    var notifyCollectionChanged = oldButtons as INotifyCollectionChanged;
                    if (notifyCollectionChanged != null)
                    {
                        notifyCollectionChanged.CollectionChanged -= telegramApplicationBar.OnCollectionChanged;
                    }
                }

                telegramApplicationBar.LayoutRoot.Children.Clear();
                var newButtons = e.NewValue as IList;
                if (newButtons != null)
                {
                    for (var i = 0; i < newButtons.Count && i < 4; i++)
                    {
                        var button = newButtons[i] as FrameworkElement;
                        if (button != null)
                        {
                            button.Tap += telegramApplicationBar.Button_OnTap;

                            telegramApplicationBar.LayoutRoot.Children.Add(button);
                        }
                    }

                    var notifyCollectionChanged = newButtons as INotifyCollectionChanged;
                    if (notifyCollectionChanged != null)
                    {
                        notifyCollectionChanged.CollectionChanged += telegramApplicationBar.OnCollectionChanged;
                    }
                }
                telegramApplicationBar.LayoutRoot.Children.Add(telegramApplicationBar.MoreButton);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var child in LayoutRoot.Children)
                {
                    if (child == MoreButton) continue;

                    var button = child as FrameworkElement;
                    if (button != null)
                    {
                        button.Tap -= Button_OnTap;
                    }
                }

                LayoutRoot.Children.Clear();
                LayoutRoot.Children.Add(MoreButton);
            }

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    var button = oldItem as FrameworkElement;
                    if (button != null)
                    {
                        button.Tap -= Button_OnTap;

                        LayoutRoot.Children.Remove(button);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    var button = newItem as FrameworkElement;
                    if (button != null)
                    {
                        button.Tap += Button_OnTap;

                        LayoutRoot.Children.Add(button);
                    }
                }
            }
        }

        public static readonly DependencyProperty MorePanelProperty = DependencyProperty.Register(
            "MorePanel", typeof(Border), typeof(TelegramApplicationBar), new PropertyMetadata(default(Border), OnMorePanelChanged));

        private static void OnMorePanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramApplicationBar = d as TelegramApplicationBar;
            if (telegramApplicationBar != null)
            {
                var morePanel = e.NewValue as Border;
                if (morePanel != null)
                {
                    morePanel.Background = telegramApplicationBar._morePanelBrush;
                }
            }
        }

        public Border MorePanel
        {
            get { return (Border)GetValue(MorePanelProperty); }
            set { SetValue(MorePanelProperty, value); }
        }

        private void Button_OnTap(object sender, GestureEventArgs e)
        {
            HideLabels();
            Transform.Y = 0.0;
            if (MorePanel != null)
            {
                RaisePanelClosed();
                MorePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void HideLabels()
        {
            foreach (var child in LayoutRoot.Children)
            {
                var button = child as TelegramAppBarButton;
                if (button != null)
                {
                    button.HideLabel();
                }
            }
        }

        private void ShowLabels()
        {
            foreach (var child in LayoutRoot.Children)
            {
                var button = child as TelegramAppBarButton;
                if (button != null)
                {
                    button.ShowLabel();
                }
            }
        }

        private Brush _morePanelBrush;

        public TelegramApplicationBar()
        {
            InitializeComponent();

            SetValue(ButtonsProperty, new ObservableCollection<FrameworkElement>()); 

            var applicationBar = ApplicationBar;
            if (applicationBar.DefaultSize < ApplicaitonBarDefaultSize1X)
            {
                Height = applicationBar.DefaultSize + 18.0;
            }

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            LayoutRoot.Background = isLightTheme ? (SolidColorBrush)Resources["AppBarPanelLight"] : (SolidColorBrush)Resources["AppBarPanelDark"];
            _morePanelBrush = isLightTheme ? (SolidColorBrush)Resources["MorePanelLight"] : (SolidColorBrush)Resources["MorePanelDark"];
            if (MorePanel != null)
            {

                MorePanel.Background = _morePanelBrush; 
            }

            Loaded += OnApplicationBarLoaded;
            Unloaded += OnApplicationBarUnloaded;
        }

        private PhoneApplicationFrame _parentFrame;

        private void OnApplicationBarLoaded(object sender, RoutedEventArgs e)
        {
            _parentFrame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (_parentFrame != null)
            {
                _parentFrame.BackKeyPress += OnFrameBackKeyPressed;
            }
        }

        private void OnApplicationBarUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentFrame != null)
            {
                _parentFrame.BackKeyPress -= OnFrameBackKeyPressed;
            }
        }

        private void OnFrameBackKeyPressed(object sender, CancelEventArgs e)
        {
            if (MorePanel != null
                && MorePanel.Visibility == Visibility.Visible)
            {
                Close();

                e.Cancel = true;
                return;
            }
        }

        private void More_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                if (MorePanel.Visibility == Visibility.Visible)
                {
                    CloseMorePanel();
                }
                else
                {
                    OpenMorePanel();
                }
            });
        }

        private void OpenMorePanel()
        {
            if (MorePanel != null && MorePanel.ActualHeight == 0.0)
            {
                MorePanel.Visibility = Visibility.Visible;
                MorePanel.Opacity = 0.0;
                MorePanel.IsHitTestVisible = false;

                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    MorePanel.Opacity = 1.0;
                    MorePanel.IsHitTestVisible = true;

                    OpenMorePanelInternal();
                });
            }
            else
            {
                OpenMorePanelInternal();
            }
        }

        private void OpenMorePanelInternal()
        {
            RaisePanelOpened();

            var storyboard = new Storyboard();

            var translateAppBarPanelAnimation = new DoubleAnimationUsingKeyFrames();
            translateAppBarPanelAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
            translateAppBarPanelAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = -18.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
            Storyboard.SetTarget(translateAppBarPanelAnimation, Transform);
            Storyboard.SetTargetProperty(translateAppBarPanelAnimation, new PropertyPath("Y"));
            storyboard.Children.Add(translateAppBarPanelAnimation);

            if (MorePanel != null)
            {
                MorePanel.Visibility = Visibility.Visible;

                var continuumLayoutRootY = new DoubleAnimationUsingKeyFrames();
                continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = MorePanel.ActualHeight });
                continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = -18.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(continuumLayoutRootY, MorePanel);
                Storyboard.SetTargetProperty(continuumLayoutRootY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(continuumLayoutRootY);
            }

            ShowLabels();
            storyboard.Begin();
        }

        private void CloseMorePanel()
        {
            RaisePanelClosed();

            var storyboard = new Storyboard();

            var translateAppBarPanelAnimation = new DoubleAnimationUsingKeyFrames();
            translateAppBarPanelAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
            Storyboard.SetTarget(translateAppBarPanelAnimation, Transform);
            Storyboard.SetTargetProperty(translateAppBarPanelAnimation, new PropertyPath("Y"));
            storyboard.Children.Add(translateAppBarPanelAnimation);

            if (MorePanel != null)
            {
                var translateAnimation = new DoubleAnimationUsingKeyFrames();
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = MorePanel.ActualHeight, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
                Storyboard.SetTarget(translateAnimation, MorePanel);
                Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation);
            }

            storyboard.Begin();
            storyboard.Completed += (sender, args) =>
            {
                HideLabels();
                if (MorePanel != null) MorePanel.Visibility = Visibility.Collapsed;
            };
        }

        public void Close()
        {
            CloseMorePanel();
        }
    }
}
