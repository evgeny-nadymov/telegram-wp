// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define NO_RIBBON
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using TelegramClient.Behaviors;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Media;
using GestureEventArgs = TelegramClient.Controls.GestureListener.GestureEventArgs;

namespace TelegramClient.Views.Media
{
    public partial class ProfilePhotoViewerView
    {
        public ProfilePhotoViewerViewModel ViewModel
        {
            get { return DataContext as ProfilePhotoViewerViewModel; }
        }

        private readonly ApplicationBarMenuItem _savePhotoMenuItem = new ApplicationBarMenuItem { Text = AppResources.Save };

        private readonly ApplicationBarMenuItem _deletePhotoMenuItem = new ApplicationBarMenuItem { Text = AppResources.Delete };

        private readonly ApplicationBarMenuItem _sharePhotoMenuItem = new ApplicationBarMenuItem { Text = AppResources.Share + "..." };

        private readonly ApplicationBarMenuItem _setPhotoMenuItem = new ApplicationBarMenuItem { Text = AppResources.Set, IsEnabled = false };

        private bool _runOnce;

        private PanAndZoomBehavior _panAndZoomBehavior;

        public ProfilePhotoViewerView()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;

            _savePhotoMenuItem.Click += (o, e) => ViewModel.SavePhoto();
            _deletePhotoMenuItem.Click += (o, e) => ViewModel.DeletePhoto();
            _sharePhotoMenuItem.Click += (o, e) => ViewModel.SharePhoto();
            _setPhotoMenuItem.Click += (o, e) => ViewModel.SetPhoto();

            BuildLocalizedAppBar();

            OptimizeFullHD();

            _runOnce = true;

            Loaded += (o, e) =>
            {
                var behaviorCollection = Interaction.GetBehaviors(Control2);
                if (behaviorCollection.FirstOrDefault(x => x is PanAndZoomBehavior) == null)
                {
                    var panAndZoomBehavior = new PanAndZoomBehavior();
                    panAndZoomBehavior.MaxZoom = 5.0;
                    panAndZoomBehavior.CanZoom = true;
                    panAndZoomBehavior.DoubleTap += PanAndZoom_OnDoubleTap;

                    behaviorCollection.Add(panAndZoomBehavior);

                    _panAndZoomBehavior = panAndZoomBehavior;
                }

                SetLocalizedAppBarButtons();

                if (_runOnce && ViewModel.CurrentItem != null)
                {
                    _runOnce = false;
                    BeginOpenStoryboard();
                }

                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };

            Unloaded += (o, e) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void PanAndZoom_OnDoubleTap(object sender, GestureEventArgs e)
        {
            _tapHandled = true;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.CurrentItem))
            {
                _setPhotoMenuItem.IsEnabled = ViewModel.CanSlideRight;

                if (ViewModel.IsChatViewer) return;

                if (ApplicationBar != null)
                {
                    if (ViewModel.IsSelfViewer)
                    {
                        ApplicationBar.MenuItems.Remove(_deletePhotoMenuItem);
                        if (!ViewModel.CanSlideRight)
                        {
                            ApplicationBar.MenuItems.Insert(1, _deletePhotoMenuItem);
                        }
                    }
                }
            } 
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                if (ViewModel.IsOpen)
                {
                    BeginOpenStoryboard();
                }
                else
                {
                    BeginCloseStoryboard();
                }
            }
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar == null)
            {
                ApplicationBar = new ApplicationBar { Opacity = 0.0 };
                ApplicationBar.BackgroundColor = Colors.Black;
                ApplicationBar.ForegroundColor = Colors.White;
                ApplicationBar.StateChanged += (o, e) => { ApplicationBar.Opacity = e.IsMenuVisible ? 0.9999 : 0.0; };
            }
        }

        private void OptimizeFullHD()
        {
#if WP8
            var appBar = ApplicationBar;
            if (appBar == null)
            {
                appBar = new ApplicationBar();
            }

            var appBarDefaultSize = appBar.DefaultSize;
            var appBarDifference = appBarDefaultSize - 72.0;

            AppBarPlaceholder.Height = appBarDefaultSize;
            AppBarPlaceholder2.Height = appBarDefaultSize;
            ApplicationPanel.Margin = new Thickness(0.0, 0.0, 0.0, -appBarDefaultSize);
#if !NO_RIBBON
            OpenApplicationPanelAnimationStartKeyFrame.Value = appBarDefaultSize + Canvas.Height;
            CloseApplicationPanelAnimationStopKeyFrame.Value = appBarDefaultSize + Canvas.Height;
            ApplicationPanelTransform.TranslateY = appBarDefaultSize + Canvas.Height;
#else
            OpenApplicationPanelAnimationStartKeyFrame.Value = appBarDefaultSize;
            CloseApplicationPanelAnimationStopKeyFrame.Value = appBarDefaultSize;
            ApplicationPanelTransform.TranslateY = appBarDefaultSize;
#endif
            
            //TopAppBarPlaceholder.Height = appBarDefaultSize;
            //OpenTopApplicationPanelAnimationStartKeyFrame.Value = -appBarDefaultSize;
            //CloseTopApplicationPanelAnimationStopKeyFrame.Value = -appBarDefaultSize;
            //TopApplicationPanelTransform.TranslateY = -appBarDefaultSize;
#endif
        }

        private void SetLocalizedAppBarButtons()
        {
            ApplicationBar.MenuItems.Clear();
            ApplicationBar.MenuItems.Add(_savePhotoMenuItem);

            if (ViewModel.IsChatViewer) return;

            if (ViewModel.IsSelfViewer) ApplicationBar.MenuItems.Add(_deletePhotoMenuItem);
#if WP8
            ApplicationBar.MenuItems.Add(_sharePhotoMenuItem);
#endif
            ApplicationBar.MenuItems.Add(_setPhotoMenuItem);
        }

        private CloseDirection? _direction;

        private void BeginCloseStoryboard()
        {
            SystemTray.IsVisible = true;
            ApplicationBar.IsVisible = false;

            var direction = _direction ?? CloseDirection.Down;
            var duration = _direction != null ? TimeSpan.FromSeconds(0.15) : TimeSpan.FromSeconds(0.25);
            var easingFunction = _direction != null ? null : new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = ImagesGrid.ActualHeight / 2 + rootFrameHeight / 2;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = direction == CloseDirection.Down ? translateYTo : -translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
            opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = 0 });
            Storyboard.SetTarget(opacityImageAniamtion, BackgroundBorder);
            Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityImageAniamtion);

            storyboard.Begin();
            storyboard.Completed += (o, args) =>
            {
                Visibility = Visibility.Collapsed;
                _direction = null;
            };

            CloseApplicationPanelAnimation.Begin();
        }

        private void BeginOpenStoryboard()
        {
            Control1.Visibility = Visibility.Collapsed;
            Control1.Content = ViewModel.PreviousItem;
            Grid.SetColumn(Item1, 2);
            Control2.Visibility = Visibility.Visible;
            Control2.Content = ViewModel.CurrentItem;
            Grid.SetColumn(Item2, 1);
            Control3.Visibility = Visibility.Collapsed;
            Control3.Content = ViewModel.NextItem;
            Grid.SetColumn(Item3, 0);

            SetPanAndZoom();

            SystemTray.IsVisible = false;
            ApplicationBar.IsVisible = true;

            if (_panAndZoomBehavior != null)
            {
                _panAndZoomBehavior.CurrentScaleX = 1.0;
                _panAndZoomBehavior.CurrentScaleY = 1.0;
            }

            var transparentBlack = Colors.Black;
            transparentBlack.A = 0;

            Visibility = Visibility.Visible;
            ImagesGrid.Opacity = 1.0;
            ImagesGrid.RenderTransform = new CompositeTransform();
            BackgroundBorder.Opacity = 1.0;


            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            ((CompositeTransform)ImagesGrid.RenderTransform).TranslateY = translateYTo;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            storyboard.Completed += (sender, args) =>
            {
                Control1.Visibility = Visibility.Visible;
                Control2.Visibility = Visibility.Visible;
                Control3.Visibility = Visibility.Visible;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    OpenApplicationPanelAnimation.Begin();
                    //OpenTopApplicationPanelAnimation.Begin();
                });
            };
            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private bool _tapHandled;

        private void PanAndZoom_OnClose(object sender, PanAndZoomCloseEventArgs args)
        {
            _direction = args.VerticalChange > 0 ? CloseDirection.Down : CloseDirection.Up;

            ViewModel.CloseViewer();
        }

        private bool? _verticalDrag;
        private bool _slideAnimating;

        private void ImagesGrid_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;

            if (_panAndZoomBehavior.CurrentScaleX > 1.0) return;
            if (_slideAnimating) return;

            _verticalDrag = null;
        }

        private void ImagesGrid_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;

            if (_panAndZoomBehavior.CurrentScaleX > 1.0) return;
            if (_slideAnimating) return;

            if (_verticalDrag == null)
            {
                _verticalDrag = Math.Abs(e.DeltaManipulation.Translation.Y) >
                                  Math.Abs(e.DeltaManipulation.Translation.X);
            }
            if (_verticalDrag == false)
            {
                var transform = ImagesGrid.RenderTransform as CompositeTransform;
                if (transform == null) return;

                if (e.CumulativeManipulation.Translation.X < 0.0
                    && ViewModel.CanSlideLeft)
                {
                    transform.TranslateX += e.DeltaManipulation.Translation.X;
                }
                else if (e.CumulativeManipulation.Translation.X > 0.0
                    && ViewModel.CanSlideRight)
                {
                    transform.TranslateX += e.DeltaManipulation.Translation.X;
                }

                var percent = transform.TranslateX / 480.0;
#if !NO_RIBBON
                Ribbon.Move(percent);
#endif

                transform.TranslateX = PanAndZoomBehavior.Clamp(transform.TranslateX, -480.0 - 12.0, 480.0 + 12.0);
                _panAndZoomBehavior.IsEnabled = transform.TranslateX == 0.0;
            }
            else
            {
                var transform = ImagesGrid.RenderTransform as CompositeTransform;
                if (transform == null) return;

                var translate = new CompositeTransform
                {
                    TranslateX = 0,
                    TranslateY = e.CumulativeManipulation.Translation.Y
                };

                var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
                var deltaY = Math.Abs(translate.TranslateY + translate.ScaleY * transform.TranslateY + (translate.ScaleY - 1) * (transform.CenterY - translate.CenterY));
                var opacity = (rootFrameHeight - deltaY) / rootFrameHeight;
                var backgroundBrush = (SolidColorBrush)BackgroundBorder.Background;
                var backgroundColor = backgroundBrush.Color;
                backgroundColor.A = (byte)(opacity * byte.MaxValue);

                BackgroundBorder.Opacity = opacity;

                ImagesGrid.RenderTransform = translate;
                _panAndZoomBehavior.IsEnabled = translate.TranslateY == 0.0;
            }
        }

        private void ImagesGrid_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;

            if (_panAndZoomBehavior.CurrentScaleX > 1.0) return;
            if (_slideAnimating) return;

            _panAndZoomBehavior.IsEnabled = true;
            if (_verticalDrag == true)
            {
                var transform = ImagesGrid.RenderTransform as CompositeTransform;
                if (transform == null) return;

                var hasVelocity = e.FinalVelocities.LinearVelocity.X > 1000.0;
                var minTranslateY = 100.0;

                if (Math.Abs(transform.TranslateY) > minTranslateY || hasVelocity && Math.Abs(transform.TranslateY) > 0.0)
                {
                    PanAndZoom_OnClose(this, new PanAndZoomCloseEventArgs { VerticalChange = e.TotalManipulation.Translation.Y });
                    return;
                }

                var storyboard = new Storyboard();

                var translateAnimation = new DoubleAnimationUsingKeyFrames();
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } });
                Storyboard.SetTarget(translateAnimation, ImagesGrid);
                Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation);

                storyboard.Begin();

                BackgroundBorder.Opacity = 1.0;
            }
            else if (_verticalDrag == false)
            {
                var transform = ImagesGrid.RenderTransform as CompositeTransform;
                if (transform == null) return;

                var hasVelocity = Math.Abs(e.FinalVelocities.LinearVelocity.X) > 1000.0;
                var minTranslateX = 160.0;

                if (transform.TranslateX > minTranslateX || hasVelocity && transform.TranslateX > 0.0)
                {
                    SlideRight(Math.Abs(e.FinalVelocities.LinearVelocity.X));
                }
                else if (transform.TranslateX < -minTranslateX || hasVelocity && transform.TranslateX < 0.0)
                {
                    SlideLeft(Math.Abs(e.FinalVelocities.LinearVelocity.X));
                }
                else
                {
                    SlideBack();
                    transform.TranslateX = 0.0;
                }
            }
        }

        public void SlideRight(double velocity, System.Action callback = null)
        {
            if (velocity == 0.0)
            {
                velocity = 480.0 + 12.0;
            }

            var transform = ImagesGrid.RenderTransform as CompositeTransform;
            if (transform == null) return;

            var translationX = 480.0 + 12.0;
            var duration = PanAndZoomBehavior.Clamp((translationX - transform.TranslateX) / velocity, 0.15, 0.35);
#if !NO_RIBBON
            Ribbon.ScrollPrevious(duration);
#endif
            var storyboard = new Storyboard();
            var translateAnimation = new DoubleAnimation();
            translateAnimation.To = translationX;
            translateAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            Storyboard.SetTarget(translateAnimation, transform);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("TranslateX"));
            storyboard.Children.Add(translateAnimation);
            storyboard.Begin();

            //MainGrid.IsHitTestVisible = false;
            _slideAnimating = true;
            storyboard.Completed += (sender, args) =>
            {
                //Deployment.Current.Dispatcher.BeginInvoke(() =>
                //{
                    //MainGrid.IsHitTestVisible = true;
                    _slideAnimating = false;
                //});
                transform.TranslateX = 0.0;

                Grid.SetColumn(Item1, Grid.GetColumn(Item1) + 1 > 2 ? 0 : Grid.GetColumn(Item1) + 1);
                Grid.SetColumn(Item2, Grid.GetColumn(Item2) + 1 > 2 ? 0 : Grid.GetColumn(Item2) + 1);
                Grid.SetColumn(Item3, Grid.GetColumn(Item3) + 1 > 2 ? 0 : Grid.GetColumn(Item3) + 1);

                SetPanAndZoom();

                ViewModel.SlideRight();

                SetControlContent(0, ViewModel.NextItem);

                callback.SafeInvoke();
            };
        }

        public void SlideLeft(double velocity, System.Action callback = null)
        {
            if (velocity == 0.0)
            {
                velocity = 480.0 + 12.0;
            }

            var transform = ImagesGrid.RenderTransform as CompositeTransform;
            if (transform == null) return;

            var translationX = 480.0 + 12.0;
            var duration = PanAndZoomBehavior.Clamp((translationX + transform.TranslateX) / velocity, 0.15, 0.35);
#if !NO_RIBBON
            Ribbon.ScrollNext(duration);
#endif
            var storyboard = new Storyboard();
            var translateAnimation = new DoubleAnimation();
            translateAnimation.To = -translationX;
            translateAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            Storyboard.SetTarget(translateAnimation, transform);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("TranslateX"));
            storyboard.Children.Add(translateAnimation);
            storyboard.Begin();

            //MainGrid.IsHitTestVisible = false;
            _slideAnimating = true;
            storyboard.Completed += (sender, args) =>
            {
                //Deployment.Current.Dispatcher.BeginInvoke(() =>
                //{
                    //MainGrid.IsHitTestVisible = true;
                    _slideAnimating = false;
                //});
                transform.TranslateX = 0.0;
                Grid.SetColumn(Item1, Grid.GetColumn(Item1) - 1 < 0 ? 2 : Grid.GetColumn(Item1) - 1);
                Grid.SetColumn(Item2, Grid.GetColumn(Item2) - 1 < 0 ? 2 : Grid.GetColumn(Item2) - 1);
                Grid.SetColumn(Item3, Grid.GetColumn(Item3) - 1 < 0 ? 2 : Grid.GetColumn(Item3) - 1);

                SetPanAndZoom();

                ViewModel.SlideLeft();

                SetControlContent(2, ViewModel.PreviousItem);

                callback.SafeInvoke();
            };
        }

        public void SetControlContent(int column, TLPhotoBase content)
        {
            if (Grid.GetColumn(Item1) == column)
            {
                Control1.Content = content;
            }
            else if (Grid.GetColumn(Item2) == column)
            {
                Control2.Content = content;
            }
            else if (Grid.GetColumn(Item3) == column)
            {
                Control3.Content = content;
            }
        }

        private void SlideBack()
        {
            var transform = ImagesGrid.RenderTransform as CompositeTransform;
            if (transform == null) return;

            var duration = 0.1;

            var storyboard = new Storyboard();
            var translateAnimation = new DoubleAnimation();
            translateAnimation.To = 0.0;
            translateAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            Storyboard.SetTarget(translateAnimation, transform);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("TranslateX"));
            storyboard.Children.Add(translateAnimation);
            storyboard.Begin();
#if !NO_RIBBON
            Ribbon.ScrollBack(duration);
#endif
        }

        private void SetPanAndZoom()
        {
            Interaction.GetBehaviors(Control1).Clear();
            Interaction.GetBehaviors(Control2).Clear();
            Interaction.GetBehaviors(Control3).Clear();

            if (Grid.GetColumn(Item1) == 1)
            {
                var behaviors = Interaction.GetBehaviors(Control1);
                behaviors.Add(_panAndZoomBehavior);
                Control1.RenderTransform = new CompositeTransform();
            }
            else if (Grid.GetColumn(Item2) == 1)
            {
                var behaviors = Interaction.GetBehaviors(Control2);
                behaviors.Add(_panAndZoomBehavior);
                Control2.RenderTransform = new CompositeTransform();
            }
            else if (Grid.GetColumn(Item3) == 1)
            {
                var behaviors = Interaction.GetBehaviors(Control3);
                behaviors.Add(_panAndZoomBehavior);
                Control3.RenderTransform = new CompositeTransform();
            }
        }

        private void ImagesGrid_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.15), () =>
            {
                if (_tapHandled)
                {
                    _tapHandled = false;
                    return;
                }

                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = !ApplicationBar.IsVisible;
                }

                var animation = ApplicationPanel.Visibility == Visibility.Visible
                    ? CloseApplicationPanelAnimation
                    : OpenApplicationPanelAnimation;

                animation.Begin();
            });
        }

        private int _index;
        private double _duration;

        public void ScrollTo(TLPhotoBase currentItem, double duration)
        {
#if !NO_RIBBON
            var index = ViewModel.GroupedItems.IndexOf(currentItem);
            if (index != -1)
            {
                _index = index;
                _duration = duration;
                Ribbon.LayoutUpdated += Ribbon_LayoutUpdated;
            }
#endif
        }

        private void Ribbon_LayoutUpdated(object sender, System.EventArgs e)
        {
 #if !NO_RIBBON
            var result = Ribbon.ScrollTo(_index, _duration);
            System.Diagnostics.Debug.WriteLine("Ribbon.ScrollTo result=" + result);
            if (result)
            {
                Ribbon.LayoutUpdated -= Ribbon_LayoutUpdated;
            }
#endif
        }
    }
}