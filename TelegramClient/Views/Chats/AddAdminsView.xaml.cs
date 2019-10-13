// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Chats;

namespace TelegramClient.Views.Chats
{
    public partial class AddAdminsView: IDisposable
    {
        private readonly IDisposable _keyPressSubscription;

        private readonly AppBarButton _searchButton = new AppBarButton
        {
            Text = AppResources.Search,
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.search.rest.png", UriKind.Relative)
        };

        public AddAdminsViewModel ViewModel
        {
            get { return DataContext as AddAdminsViewModel; }
        }

        public AddAdminsView()
        {
            InitializeComponent();

            _searchButton.Click += (sender, args) =>
            {
                if (SearchPanel.Visibility == Visibility.Collapsed)
                {
                    OpenSearchPanel();
                }
                else
                {
                    CloseSearchPanel();
                }
            };

            var keyPressEvents = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                keh => { SearchBox.TextChanged += keh; },
                keh => { SearchBox.TextChanged -= keh; });

            _keyPressSubscription = keyPressEvents
                .Throttle(TimeSpan.FromSeconds(0.10))
                .ObserveOnDispatcher()
                .Subscribe(e => ViewModel.Search());

            BuildLocalizedAppBar();
        }

        private void OpenSearchPanel()
        {
            //SearchPanel.Visibility = Visibility.Visible;
            //TitlePanel.Opacity = 0.0;

            //SearchBox.Focus();

            var storyboard = new Storyboard();
            var searchBox = new DoubleAnimationUsingKeyFrames();
            searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -100.0 });
            searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
            Storyboard.SetTarget(searchBox, SearchPanel);
            Storyboard.SetTargetProperty(searchBox, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(searchBox);

            var searchBoxOpacity = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.15),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 }
            };
            Storyboard.SetTarget(searchBoxOpacity, SearchPanel);
            Storyboard.SetTargetProperty(searchBoxOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(searchBoxOpacity);

            var titlePanelOpacity = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.15),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 }
            };
            Storyboard.SetTarget(titlePanelOpacity, TitlePanel);
            Storyboard.SetTargetProperty(titlePanelOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(titlePanelOpacity);


            SearchPanel.Opacity = 0.0;
            SearchPanel.Visibility = Visibility.Visible;

            storyboard.Completed += (sender, args) => SearchBox.Focus();

            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (SearchPanel.Visibility == Visibility.Visible)
            {
                CloseSearchPanel();

                e.Cancel = true;
            }
        }

        private void CloseSearchPanel()
        {
            //SearchPanel.Visibility = Visibility.Collapsed;
            //TitlePanel.Opacity = 1.0;

            var storyboard = new Storyboard();

            var searchBox = new DoubleAnimationUsingKeyFrames();
            searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            searchBox.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = -100.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 } });
            Storyboard.SetTarget(searchBox, SearchPanel);
            Storyboard.SetTargetProperty(searchBox, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(searchBox);

            var searchBoxOpacity = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.15),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 }
            };
            Storyboard.SetTarget(searchBoxOpacity, SearchPanel);
            Storyboard.SetTargetProperty(searchBoxOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(searchBoxOpacity);

            var titlePanelOpacity = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.15),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3.0 }
            };
            Storyboard.SetTarget(titlePanelOpacity, TitlePanel);
            Storyboard.SetTargetProperty(titlePanelOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(titlePanelOpacity);

            storyboard.Completed += (sender, args) =>
            {
                SearchPanel.Visibility = Visibility.Collapsed;
                ViewModel.Text = string.Empty;
            };

            storyboard.Begin();
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_searchButton);
        }

        private void TelegramNavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            ViewModel.ForwardInAnimationComplete();
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            var checkBox = sender as FrameworkElement;
            if (checkBox != null)
            {
                var user = checkBox.DataContext as TLUserBase;
                if (user != null)
                {
                    ViewModel.EditChatAdmin(user);
                }
            }
        }

        public void Dispose()
        {
            _keyPressSubscription.Dispose();
        }
    }
}