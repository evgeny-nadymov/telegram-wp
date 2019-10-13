// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Coding4Fun.Toolkit.Controls;
using Coding4Fun.Toolkit.Controls.Binding;
using Coding4Fun.Toolkit.Controls.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Telegram.Controls.Notifications
{
    // this code has been modified from the orginal code
    // from Kevin Marshall's post 
    // http://blogs.claritycon.com/kevinmarshall/2010/10/13/wp7-page-transitions-sample/

    public class DialogService
    {
        public enum AnimationTypes
        {
            Slide,
            SlideHorizontal,
            Swivel,
            SwivelHorizontal,
            Fade
        }

        private const string SlideUpStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.Y)"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""150""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""0"" To=""1"" Duration=""0:0:0.350"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        private const string SlideHorizontalInStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.X)"" >
                    <EasingDoubleKeyFrame KeyTime=""0"" Value=""-150""/>
                    <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
                        <EasingDoubleKeyFrame.EasingFunction>
                            <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                        </EasingDoubleKeyFrame.EasingFunction>
                    </EasingDoubleKeyFrame>
                </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""0"" To=""1"" Duration=""0:0:0.350"" >
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        private const string SlideHorizontalOutStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.X)"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""150"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""1"" To=""0"" Duration=""0:0:0.25"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        private const string SlideDownStoryboard = @"
        <Storyboard  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(TranslateTransform.Y)"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""150"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty=""(UIElement.Opacity)"" From=""1"" To=""0"" Duration=""0:0:0.25"">
                <DoubleAnimation.EasingFunction>
                    <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>";

        private const string SwivelInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation 
				To="".5""
                Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.CenterOfRotationY)"" />
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationX)"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""-30""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.35"" Value=""0"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                <DiscreteDoubleKeyFrame KeyTime=""0"" Value=""1"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        private const string SwivelOutStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation BeginTime=""0:0:0"" Duration=""0"" 
                                Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.CenterOfRotationY)"" 
                                To="".5""/>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Projection).(PlaneProjection.RotationX)"">
                <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                <EasingDoubleKeyFrame KeyTime=""0:0:0.25"" Value=""45"">
                    <EasingDoubleKeyFrame.EasingFunction>
                        <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                    </EasingDoubleKeyFrame.EasingFunction>
                </EasingDoubleKeyFrame>
            </DoubleAnimationUsingKeyFrames>
            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                <DiscreteDoubleKeyFrame KeyTime=""0"" Value=""1"" />
                <DiscreteDoubleKeyFrame KeyTime=""0:0:0.267"" Value=""0"" />
            </DoubleAnimationUsingKeyFrames>
        </Storyboard>";

        private const string FadeInStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation 
				Duration=""0:0:0.2"" 
				Storyboard.TargetProperty=""(UIElement.Opacity)"" 
                To=""1""/>
        </Storyboard>";

        private const string FadeOutStoryboard =
        @"<Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
            <DoubleAnimation 
				Duration=""0:0:0.2""
				Storyboard.TargetProperty=""(UIElement.Opacity)"" 
                To=""0""/>
        </Storyboard>";

        private Panel _popupContainer;
        private Frame _rootFrame;
        private PhoneApplicationPage _page;
        private Grid _childPanel;
        private Grid _overlay;

        public bool IsOverlayApplied
        {
            get { return _isOverlayApplied; }
            set { _isOverlayApplied = value; }
        }
        private bool _isOverlayApplied = true;

        public FrameworkElement Child { get; set; }
        public AnimationTypes AnimationType { get; set; }
        public TimeSpan MainBodyDelay { get; set; }

        public double VerticalOffset { get; set; }
        internal double ControlVerticalOffset { get; set; }
        public bool BackButtonPressed { get; set; }

        public Brush BackgroundBrush { get; set; }

        internal bool IsOpen { get; set; }
        protected internal bool IsBackKeyOverride { get; set; }

        public event EventHandler Closed;
        public event EventHandler Opened;

        // set this to prevent the dialog service from closing on back click
        public bool HasPopup { get; set; }

        internal PhoneApplicationPage Page
        {
            get { return _page ?? (_page = RootFrame.GetFirstLogicalChildByType<PhoneApplicationPage>(false)); }
        }

        internal Frame RootFrame
        {
            get { return _rootFrame ?? (_rootFrame = ApplicationSpace.RootFrame); }
        }

        internal Panel PopupContainer
        {
            get
            {
                if (_popupContainer == null)
                {
                    //var popups = RootFrame.GetLogicalChildrenByType<Popup>(false).Where(x => x.IsOpen);

                    //if (popups.Any())
                    //{
                    //    for (var i = 0; i < popups.Count(); i++)
                    //    {
                    //        var child = popups.ElementAt(i).Child as Panel;

                    //        if (child == null)
                    //            continue;

                    //        _popupContainer = child;
                    //        break;
                    //    }
                    //}
                    //else
                    {
                        var presenters = RootFrame.GetLogicalChildrenByType<ContentPresenter>(false);

                        for (var i = 0; i < presenters.Count(); i++)
                        {
                            var panels = presenters.ElementAt(i).GetLogicalChildrenByType<Panel>(false);

                            if (!panels.Any())
                                continue;

                            _popupContainer = panels.First();
                            break;
                        }
                    }

                }

                return _popupContainer;
            }
        }

        public DialogService()
        {
            AnimationType = AnimationTypes.Slide;

            BackButtonPressed = false;
        }

        bool _deferredShowToLoaded;
        private void InitializePopup()
        {
            // Add overlay which is the size of RootFrame
            _childPanel = CreateGrid();

            if (IsOverlayApplied)
            {
                _overlay = CreateGrid();
                PreventScrollBinding.SetIsEnabled(_overlay, true);
            }

            ApplyOverlayBackground();

            // Initialize popup to draw the context menu over all controls
            if (PopupContainer != null)
            {
                if (_overlay != null)
                    PopupContainer.Children.Add(_overlay);

                PopupContainer.Children.Add(_childPanel);
                _childPanel.Children.Add(Child);
            }
            else
            {
                _deferredShowToLoaded = true;
                RootFrame.Loaded += RootFrameDeferredShowLoaded;
            }
        }

        internal void ApplyOverlayBackground()
        {
            if (IsOverlayApplied && BackgroundBrush != null)
                _overlay.Background = BackgroundBrush;
        }

        private Grid CreateGrid()
        {
            var grid = new Grid { Name = Guid.NewGuid().ToString() };

            Grid.SetColumnSpan(grid, int.MaxValue);
            Grid.SetRowSpan(grid, int.MaxValue);

            grid.Opacity = 0;

            CalculateVerticalOffset(grid);

            return grid;
        }

        internal void CalculateVerticalOffset()
        {
            CalculateVerticalOffset(_childPanel);
        }

        internal void CalculateVerticalOffset(Panel panel)
        {
            if (panel == null)
                return;

            var sysTrayVerticalOffset = 0;

            if (SystemTray.IsVisible && SystemTray.Opacity < 1 && SystemTray.Opacity > 0)
            {
                sysTrayVerticalOffset += 32;
            }

            panel.Margin = new Thickness(0, VerticalOffset + sysTrayVerticalOffset + ControlVerticalOffset, 0, 0);
        }

        void RootFrameDeferredShowLoaded(object sender, RoutedEventArgs e)
        {
            RootFrame.Loaded -= RootFrameDeferredShowLoaded;
            _deferredShowToLoaded = false;

            Show();
        }

        protected internal void SetAlignmentsOnOverlay(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            if (_childPanel != null)
            {
                _childPanel.HorizontalAlignment = horizontalAlignment;
                _childPanel.VerticalAlignment = verticalAlignment;
            }
        }

        private static readonly object Lockobj = new object();
        /// <summary>
        /// Shows the context menu.
        /// </summary>
        public void Show()
        {
            lock (Lockobj)
            {
                Page.BackKeyPress -= OnBackKeyPress;

                IsOpen = true;

                InitializePopup();

                if (_deferredShowToLoaded)
                    return;

                if (!IsBackKeyOverride)
                    Page.BackKeyPress += OnBackKeyPress;

                Page.NavigationService.Navigated += OnNavigated;

                RunShowStoryboard(_overlay, AnimationTypes.Fade);
                RunShowStoryboard(_childPanel, AnimationType, MainBodyDelay);

                if (Opened != null)
                    Opened.Invoke(this, null);

            }
        }

        private void RunShowStoryboard(UIElement grid, AnimationTypes animation)
        {
            RunShowStoryboard(grid, animation, TimeSpan.MinValue);
        }

        private void RunShowStoryboard(UIElement grid, AnimationTypes animation, TimeSpan delay)
        {
            if (grid == null)
                return;

            Storyboard storyboard;
            switch (animation)
            {
                case AnimationTypes.SlideHorizontal:
                    storyboard = XamlReader.Load(SlideHorizontalInStoryboard) as Storyboard;
                    grid.RenderTransform = new TranslateTransform();
                    break;

                case AnimationTypes.Slide:
                    storyboard = XamlReader.Load(SlideUpStoryboard) as Storyboard;
                    grid.RenderTransform = new TranslateTransform();
                    break;
                case AnimationTypes.Fade:
                    storyboard = XamlReader.Load(FadeInStoryboard) as Storyboard;
                    break;
                case AnimationTypes.Swivel:
                case AnimationTypes.SwivelHorizontal:
                default:
                    storyboard = XamlReader.Load(SwivelInStoryboard) as Storyboard;
                    grid.Projection = new PlaneProjection();
                    break;
            }

            if (storyboard != null)
            {
                foreach (var storyboardAnimation in storyboard.Children)
                {
                    if (!(storyboardAnimation is DoubleAnimationUsingKeyFrames))
                        continue;

                    var doubleKey = storyboardAnimation as DoubleAnimationUsingKeyFrames;

                    foreach (var frame in doubleKey.KeyFrames)
                    {
                        frame.KeyTime = KeyTime.FromTimeSpan(frame.KeyTime.TimeSpan.Add(delay));
                    }
                }

                Page.Dispatcher.BeginInvoke(() =>
                {
                    foreach (var t in storyboard.Children)
                        Storyboard.SetTarget(t, grid);

                    storyboard.Begin();
                });
            }
        }

        void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.IsNavigationInitiator) //current app initialized navigation?
                Hide();
        }

        public void Hide()
        {
            if (!IsOpen)
                return;

            if (Page != null)
            {
                Page.BackKeyPress -= OnBackKeyPress;
                Page.NavigationService.Navigated -= OnNavigated;

                _page = null;
            }

            RunHideStoryboard(_overlay, AnimationTypes.Fade);
            RunHideStoryboard(_childPanel, AnimationType);
        }

        void RunHideStoryboard(Grid grid, AnimationTypes animation)
        {
            if (grid == null)
                return;

            Storyboard storyboard;

            switch (animation)
            {
                case AnimationTypes.SlideHorizontal:
                    storyboard = XamlReader.Load(SlideHorizontalOutStoryboard) as Storyboard;
                    break;
                case AnimationTypes.Slide:
                    storyboard = XamlReader.Load(SlideDownStoryboard) as Storyboard;
                    break;
                case AnimationTypes.Fade:
                    storyboard = XamlReader.Load(FadeOutStoryboard) as Storyboard;
                    break;
                case AnimationTypes.Swivel:
                case AnimationTypes.SwivelHorizontal:
                default:
                    storyboard = XamlReader.Load(SwivelOutStoryboard) as Storyboard;
                    break;
            }

            try
            {
                if (storyboard != null)
                {
                    storyboard.Completed += HideStoryboardCompleted;

                    foreach (var t in storyboard.Children)
                        Storyboard.SetTarget(t, grid);

                    storyboard.Begin();
                }
            }
            catch (Exception)
            {
                // chances are user nav'ed away
                // attempting to be extremely robust here
                // if this fails, go straight to complete
                // and attempt to remove it from the visual tree
                HideStoryboardCompleted(null, null);
            }
        }

        void HideStoryboardCompleted(object sender, EventArgs e)
        {
            IsOpen = false;

            try
            {
                if (PopupContainer != null && PopupContainer.Children != null)
                {
                    if (_overlay != null)
                        PopupContainer.Children.Remove(_overlay);

                    PopupContainer.Children.Remove(_childPanel);
                }

                _childPanel.Children.Clear();
            }
            catch
            {
                // chances are user nav'ed away
                // attempting to be extremely robust here
                // if this fails, go straight to complete
                // and attempt to remove it from the visual tree
            }

            try
            {
                if (Closed != null)
                    Closed(this, null);

            }
            catch
            {
                // chances are user nav'ed away
                // attempting to be extremely robust here
                // if this fails, go straight to complete
                // and attempt to remove it from the visual tree
            }
        }

        public void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (HasPopup)
            {
                e.Cancel = true;
                return;
            }

            if (IsOpen)
            {
                e.Cancel = true;
                BackButtonPressed = true;
                Hide();
            }
        }
    }
}
