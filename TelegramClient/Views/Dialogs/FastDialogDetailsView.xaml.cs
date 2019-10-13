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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Controls;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class FastDialogDetailsView
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        private List<string> _logs = new List<string>();

        private IApplicationBar _previousAppBar;

        private PhoneApplicationPage _parentPage;

        public FastDialogDetailsViewModel ViewModel
        {
            get { return DataContext as FastDialogDetailsViewModel; }
        }

        private readonly AppBarButton _sendButton = new AppBarButton
        {
            Text = AppResources.Send,
            IconUri = new Uri("/Images/ApplicationBar/appbar.send.text.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _attachButton = new AppBarButton
        {
            Text = AppResources.Attach,
            IconUri = new Uri("/Images/ApplicationBar/appbar.attach.png", UriKind.Relative)
        };

        private readonly AppBarButton _smileButton = new AppBarButton
        {
            Text = AppResources.Emoji,
            IconUri = new Uri("/Images/ApplicationBar/appbar.smile.png", UriKind.Relative)
        };

        private readonly AppBarButton _manageButton = new AppBarButton
        {
            Text = AppResources.Manage,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.manage.rest.png", UriKind.Relative)
        };

        private readonly AppBarButton _forwardButton = new AppBarButton
        {
            Text = AppResources.Forward,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.forwardmessage.png", UriKind.Relative)
        };

        private readonly AppBarButton _deleteButton = new AppBarButton
        {
            Text = AppResources.Delete,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.delete.png", UriKind.Relative)
        };

        private readonly ApplicationBarMenuItem _searchMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.Search,
        };

        private readonly ApplicationBarMenuItem _pinToStartMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.PinToStart
        };

        private readonly ApplicationBarMenuItem _shareMyContactInfoMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.ShareMyContactInfo
        };

        private readonly ApplicationBarMenuItem _helpMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.Help
        };

        private readonly ApplicationBarMenuItem _reportSpamMenuItem = new ApplicationBarMenuItem
        {
            Text = AppResources.ReportSpam
        };

        private readonly ApplicationBarMenuItem _debugMenuItem = new ApplicationBarMenuItem
        {
            Text = "debug"
        };


        private bool _firstRun = true;
        private bool _isBackwardOutAnimation;
        private bool _isForwardInAnimation;

        private void BuildLocalizedAppBar()
        {
            if (!_firstRun) return;
            _firstRun = false;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.99;

            ApplicationBar.Buttons.Add(_sendButton);
            ApplicationBar.Buttons.Add(_attachButton);
            ApplicationBar.Buttons.Add(_smileButton);
            ApplicationBar.Buttons.Add(_manageButton);
            var broadcast = ViewModel.With as TLBroadcastChat;
            var channel = ViewModel.With as TLChannel;
            var chat = ViewModel.With as TLChat41;
            if (broadcast == null || (channel != null && channel.IsMegaGroup))
            {
                var addReportSpam = true;
                if (chat != null && chat.Creator)
                {
                    addReportSpam = false;
                }

                if (channel != null && channel.Creator)
                {
                    addReportSpam = false;
                }
                if (addReportSpam)
                {
                    ApplicationBar.MenuItems.Add(_reportSpamMenuItem);
                }
            }
            if (broadcast == null || (channel != null && channel.IsMegaGroup))
            {
                ApplicationBar.MenuItems.Add(_searchMenuItem);
            }
            var user = ViewModel.With as TLUser;
            if (user != null && user.IsBot)
            {
                ApplicationBar.MenuItems.Add(_helpMenuItem);
            }
            ApplicationBar.MenuItems.Add(_pinToStartMenuItem);

            var userBase = ViewModel.With as TLUserBase;
            if (userBase != null && userBase.IsForeign)
            {
                ApplicationBar.MenuItems.Add(_shareMyContactInfoMenuItem);
            }
#if DEBUG
            ApplicationBar.MenuItems.Add(_debugMenuItem);
#endif

            //_sendButton.IsEnabled = ViewModel.CanSend;
            //ApplicationBar.IsVisible = !ViewModel.IsAppBarCommandVisible && !ViewModel.IsChooseAttachmentOpen;
        }


        public FastDialogDetailsView()
        {
            InitializeComponent();
            //LayoutRoot.Opacity = 0.0;

            var appBar = new ApplicationBar();
            var appBarDefaultSize = appBar.DefaultSize;

            AppBarPlaceholder.Height = appBarDefaultSize;

            Loaded += (sender, args) =>
            {
                //_isForwardInAnimation = true;

                var elapsed = _stopwatch.Elapsed;
                _logs.Add("Elapsed=" + elapsed + " Count=" + ViewModel.FistSliceCount);

                Logs.Text = string.Join(Environment.NewLine, _logs);
                //MessageBox.Show(string.Join(Environment.NewLine, _logs));


                _parentPage = VisualTreeExtensions.FindParentOfType<PhoneApplicationPage>(this);
                if (_parentPage != null)
                {
                    _previousAppBar = _parentPage.ApplicationBar;

                    BuildLocalizedAppBar();
                    _parentPage.ApplicationBar = ApplicationBar;
                }
                //RunAnimation();
                ViewModel.OnLoaded();
            };

            Unloaded += (sender, args) =>
            {
                //_isBackwardOutAnimation = true;

                if (_parentPage != null)
                {
                    _parentPage.ApplicationBar = _previousAppBar;
                }
            };
        }


        private void RunAnimation(System.Action callback = null)
        {
            if (_isForwardInAnimation)
            {
                _isForwardInAnimation = false;

                var storyboard = new Storyboard();

                //if (ViewModel != null
                //    && ViewModel.StateService.AnimateTitle)
                //{
                //    ViewModel.StateService.AnimateTitle = false;

                //    var continuumElementX = new DoubleAnimationUsingKeyFrames();
                //    continuumElementX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 130.0 });
                //    continuumElementX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                //    Storyboard.SetTarget(continuumElementX, Title);
                //    Storyboard.SetTargetProperty(continuumElementX, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
                //    storyboard.Children.Add(continuumElementX);

                //    var continuumElementY = new DoubleAnimationUsingKeyFrames();
                //    continuumElementY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -40.0 });
                //    continuumElementY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
                //    Storyboard.SetTarget(continuumElementY, Title);
                //    Storyboard.SetTargetProperty(continuumElementY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                //    storyboard.Children.Add(continuumElementY);
                //}

                var continuumLayoutRootY = new DoubleAnimationUsingKeyFrames();
                continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 150.0 });
                continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.35), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
                Storyboard.SetTarget(continuumLayoutRootY, LayoutRoot);
                Storyboard.SetTargetProperty(continuumLayoutRootY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(continuumLayoutRootY);

                //var continuumLayoutRootOpacity = new DoubleAnimation
                //{
                //    From = 0.0,
                //    To = 1.0,
                //    Duration = TimeSpan.FromSeconds(0.25),
                //    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
                //};
                //Storyboard.SetTarget(continuumLayoutRootOpacity, LayoutRoot);
                //Storyboard.SetTargetProperty(continuumLayoutRootOpacity, new PropertyPath("(UIElement.Opacity)"));
                //storyboard.Children.Add(continuumLayoutRootOpacity);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    LayoutRoot.Opacity = 1.0;
                    //InputMessage.IsHitTestVisible = false;
                    //InputMessageFocusHolder.Visibility = Visibility.Visible;
                    //_inputMessageDisabled = true;
                    storyboard.Completed += (o, e) =>
                    {
                        ViewModel.OnLoaded();
                    };
                    storyboard.Begin();
                });
            }
            else if (_isBackwardOutAnimation)
            {
                _isBackwardOutAnimation = false;

                LayoutRoot.CacheMode = new BitmapCache();

                var storyboard = new Storyboard();

                var translateAnimation = new DoubleAnimationUsingKeyFrames();
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
                translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                Storyboard.SetTarget(translateAnimation, LayoutRoot);
                Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                storyboard.Children.Add(translateAnimation);

                var opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 1.0 });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
                Storyboard.SetTarget(opacityAnimation, LayoutRoot);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
                storyboard.Children.Add(opacityAnimation);

                storyboard.Begin();
                storyboard.Completed += (o, e) =>
                {
                    callback.SafeInvoke();
                };
            }
        }

        public void Close(System.Action callback)
        {
            _isBackwardOutAnimation = true;
            ViewModel.LazyItems.Clear();
            RunAnimation(callback);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var elapsed = _stopwatch.Elapsed;
            _logs.Add("OnNavigatedTo elapsed=" + elapsed);
        }

        private void MainItemGrid2_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void MorePanel_OnTap(object sender, GestureEventArgs e)
        {
            
        }

        private void UIElement_OnHold(object sender, GestureEventArgs e)
        {
            
        }

        private void FastDialogDetailsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded();
        }
    }
}