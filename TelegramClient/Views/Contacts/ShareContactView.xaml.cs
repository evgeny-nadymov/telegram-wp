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
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Search;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Contacts
{
    public partial class ShareContactView
    {
        public ShareContactViewModel ViewModel
        {
            get { return DataContext as ShareContactViewModel; }
        }

        public ShareContactView()
        {
            InitializeComponent();

            LayoutRoot.Opacity = 0.0;

            Caption.Background = ShellView.CaptionBrush;

            Loaded += OnLoadedOnce;
            Loaded += (o, e) =>
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
            Unloaded += (o, e) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
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

        private void BeginOpenStoryboard()
        {
            if (ViewModel.PhoneContact != null)
            {
                LayoutRoot.Opacity = 1.0;
                LayoutRoot.Visibility = Visibility.Visible;
                Caption.Visibility = Visibility.Collapsed;
                Title.Visibility = Visibility.Collapsed;
                ViewModel.ViewAction();
                return;
            }

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            var storyboard = new Storyboard();
            var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(translateAnimaiton, Transform);
            Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("TranslateY"));
            storyboard.Children.Add(translateAnimaiton);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                LayoutRoot.Opacity = 1.0;
                LayoutRoot.Visibility = Visibility.Visible;
                storyboard.Begin();
            });

            storyboard.Completed += (sender, args) =>
            {
                //Map.Visibility = System.Windows.Visibility.Visible;
            };
        }

        private void BeginCloseStoryboard()
        {
            // OnUnloaded(null, null);

            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, Transform);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("TranslateY"));
            storyboard.Children.Add(translateImageAniamtion);

            storyboard.Completed += (sender, args) =>
            {
                LayoutRoot.Visibility = Visibility.Collapsed;
                //ViewModel.RestoreParentHitTest(true);
            };
            storyboard.Begin();
        }

        private void OnLoadedOnce(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoadedOnce;

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.OpenEditor());
        }

        private void SearchButton_OnTap(object sender, GestureEventArgs e)
        {
            var view = new SearchSharedContactsView();
            var viewModel = IoC.Get<SearchSharedContactsViewModel>();
            viewModel.Source = ViewModel.Source;
            viewModel.AttachAction = user =>
            {
                ViewModel.UserAction(user);
            };
            view.DataContext = viewModel;

            SearchPlaceholder.Content = view;
        }

        private ShareContactDetailsView _contactDetailsView;

        public void OpenContactDetails(TLUserBase contact, ShareContactDetailsMode mode)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                MorePanel.Visibility = Visibility.Collapsed;
                AppBarPanel.Visibility = Visibility.Collapsed;
                SearchPlaceholder.Content = null;

                if (_contactDetailsView == null)
                {
                    var contactDetailsViewModel = new ShareContactDetailsViewModel(contact)
                    {
                        Mode = mode,
                        Callback = ViewModel.UserActionContinue
                    };

                    _contactDetailsView = new ShareContactDetailsView();
                    _contactDetailsView.ClosePivotAction = visibility =>
                    {
                        Items.IsHitTestVisible = visibility == Visibility.Visible;
                        AppBarPanel.Visibility = visibility;
                    };
                    ViewModelBinder.Bind(contactDetailsViewModel, _contactDetailsView, null);

                    ContactDetailsContentControl.Visibility = Visibility.Visible;
                    ContactDetailsContentControl.Content = _contactDetailsView;
                }
                else
                {
                    var contactDetailsViewModel = _contactDetailsView.DataContext as ShareContactDetailsViewModel;
                    if (contactDetailsViewModel != null)
                    {
                        contactDetailsViewModel.SetContact(contact);
                    }
                    ContactDetailsContentControl.Visibility = Visibility.Visible;
                    _contactDetailsView.BeginOpenStoryboard();
                }
            });
        }

        private void ShareContactView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (_contactDetailsView != null && ContactDetailsContentControl.Visibility == Visibility.Visible)
            {
                _contactDetailsView.BeginCloseStoryboard(() =>
                {
                    Main.Focus();
                    ContactDetailsContentControl.Visibility = Visibility.Collapsed;
                });
                e.Cancel = true;
                return;
            }
        }
    }
}