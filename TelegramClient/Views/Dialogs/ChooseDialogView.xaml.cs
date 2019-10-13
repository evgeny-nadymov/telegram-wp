// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Search;

namespace TelegramClient.Views.Dialogs
{
    public partial class ChooseDialogView
    {
        public ChooseDialogViewModel ViewModel
        {
            get { return DataContext as ChooseDialogViewModel; }
        }

        private readonly ApplicationBarIconButton _searchButton = new ApplicationBarIconButton
        {
            Text = AppResources.Search,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.search.rest.png", UriKind.Relative)
        };

        public ChooseDialogView()
        {
            InitializeComponent();

            _searchButton.Click += (sender, args) =>
            {
                ViewModel.Search();
            };

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => BuildLocalizedAppBar();
        }

        //~ChooseDialogView()
        //{

        //}

        private void BuildLocalizedAppBar()
        {
            return;
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();

            ApplicationBar.Buttons.Add(_searchButton);
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            var tapedItem = sender as FrameworkElement;
            if (tapedItem == null) return;

            var dialog = tapedItem.DataContext as TLDialogBase;
            if (dialog == null) return;

            if (!(tapedItem.RenderTransform is CompositeTransform))
            {
                tapedItem.RenderTransform = new CompositeTransform();
            }

            var tapedItemContainer = tapedItem.FindParentOfType<ListBoxItem>();
            if (tapedItemContainer != null)
            {
                tapedItemContainer = tapedItemContainer.FindParentOfType<ListBoxItem>();
            }

            var result = ViewModel.ChooseDialog(dialog);
            if (result)
            {
                ShellView.StartContinuumForwardOutAnimation(tapedItem, tapedItemContainer, false);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (!e.Cancel)
            {
                if (e.NavigationMode == NavigationMode.New
                    && e.Uri.ToString().EndsWith("DialogDetailsView.xaml"))
                {
                    var storyboard = new Storyboard();

                    var translateAnimation = new DoubleAnimationUsingKeyFrames();
                    translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
                    translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                    Storyboard.SetTarget(translateAnimation, LayoutRoot);
                    Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
                    storyboard.Children.Add(translateAnimation);

                    var opacityAnimation = new DoubleAnimationUsingKeyFrames();
                    opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
                    opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
                    Storyboard.SetTarget(opacityAnimation, LayoutRoot);
                    Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
                    storyboard.Children.Add(opacityAnimation);

                    storyboard.Begin();
                }
            }
        }

        private void NavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            ViewModel.ForwardInAnimationComplete();
        }

        private SearchView _searchView;

        private void SearchButton_OnTap(object sender, GestureEventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                MorePanel.Visibility = Visibility.Collapsed;
                AppBarPanel.Visibility = Visibility.Collapsed;

                if (_searchView == null)
                {
                    var searchViewModel = new SearchViewModel(
                        IoC.Get<ICacheService>(), IoC.Get<ICommonErrorHandler>(),
                        IoC.Get<IStateService>(), IoC.Get<INavigationService>(),
                        IoC.Get<IMTProtoService>(), IoC.Get<ITelegramEventAggregator>())
                    {
                        SuppressMessagesSearch = true,
                        Watermark = AppResources.Search,
                        Callback = ViewModel.ChooseDialog
                    };

                    _searchView = new SearchView();
                    _searchView.ClosePivotAction = visibility =>
                    {
                        Items.IsHitTestVisible = visibility == Visibility.Visible;
                        AppBarPanel.Visibility = visibility;
                    };
                    ViewModelBinder.Bind(searchViewModel, _searchView, null);

                    SearchContentControl.Visibility = Visibility.Visible;
                    SearchContentControl.Content = _searchView;
                }
                else
                {
                    var searchViewModel = _searchView.DataContext as SearchViewModel;
                    if (searchViewModel != null)
                    {
                        searchViewModel.Text = string.Empty;
                        searchViewModel.NotifyOfPropertyChange(() => searchViewModel.Text);
                    }
                    SearchContentControl.Visibility = Visibility.Visible;
                    _searchView.BeginOpenStoryboard();
                }
            });
        }

        private void ChooseDialogView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (_searchView != null && SearchContentControl.Visibility == Visibility.Visible)
            {
                _searchView.BeginCloseStoryboard(() =>
                {
                    Main.Focus();
                    SearchContentControl.Visibility = Visibility.Collapsed;
                });
                e.Cancel = true;
                return;
            }
        }
    }
}