// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telegram.Controls.Extensions;
#if WP8
using System.Windows.Navigation;
#endif
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using TelegramClient.Animation.Navigation;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.Views.Dialogs
{
    public partial class CreateBroadcastView : IDisposable
    {
// fast resume
#if WP8
        private bool _isFastResume;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Reset)
            {
                _isFastResume = true;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_isFastResume
                && e.NavigationMode == NavigationMode.New
                && e.Uri.OriginalString.EndsWith("ShellView.xaml"))
            {
                _isFastResume = false;
                e.Cancel = true;
                return;
            }

            base.OnNavigatingFrom(e);
        }
#endif


        private IDisposable _keyPressSubscription;

        public CreateDialogViewModel ViewModel
        {
            get { return DataContext as CreateDialogViewModel; }
        }

        private readonly AppBarButton _createChatButton = new AppBarButton
        {
            Text = AppResources.Create,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private bool _once;

        private TextBox _searchBox;

        public CreateBroadcastView()
        {
            InitializeComponent();

            AnimationContext = LayoutRoot;

            _createChatButton.Click += (sender, args) => ViewModel.Create();

            Loaded += (sender, args) =>
            {
                if (!_once)
                {
                    _once = true;

                    _searchBox = SelectedUsers.FindChildOfType<TextBox>();

                    if (_searchBox != null)
                    {
                        var keyPressEvents = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                            keh => { _searchBox.TextChanged += keh; },
                            keh => { _searchBox.TextChanged -= keh; });

                        _keyPressSubscription = keyPressEvents
                            .Throttle(TimeSpan.FromSeconds(0.1))
                            .ObserveOnDispatcher()
                            .Subscribe(e =>
                            {
                                SearchItems.Visibility = string.IsNullOrEmpty(_searchBox.Text.Trim()) ? Visibility.Collapsed : Visibility.Visible;

                                ViewModel.Search(_searchBox.Text);
                            });
                    }

                    BuildLocalizedAppBar();
                }
            };
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardIn
                || animationType == AnimationType.NavigateBackwardIn)
            {
                return new SwivelShowAnimator { RootElement = LayoutRoot };
            }

            return new SwivelHideAnimator { RootElement = LayoutRoot };
        }

        protected override void AnimationsComplete(AnimationType animationType)
        {
            if (animationType == AnimationType.NavigateForwardIn)
            {
                Title.Focus();
            }

            base.AnimationsComplete(animationType);
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_createChatButton);
        }

        public void Dispose()
        {
            _keyPressSubscription.Dispose();
        }

        private void Contacts_OnTap(object sender, GestureEventArgs e)
        {
            if (_searchBox != null)
            {
                _searchBox.Focus();
            }
        }

        private void SearchText_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_searchBox != null
                && _searchBox.SelectionStart == 0
                && _searchBox.SelectionLength == 0
                && e.Key == Key.Back)
            {
                ViewModel.DeleteLastUser();
            }
        }

        private void LongListSelector_OnScrollingStarted(object sender, System.EventArgs e)
        {
            var focusElement = FocusManager.GetFocusedElement();
            if (focusElement == _searchBox)
            {
                Self.Focus();
            }
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            SearchItems.Visibility = Visibility.Collapsed;

            var user = ((FrameworkElement)sender).DataContext as TLUserBase;
            if (user != null)
            {
                ViewModel.ChooseContact(user);
            }
        }
    }
}