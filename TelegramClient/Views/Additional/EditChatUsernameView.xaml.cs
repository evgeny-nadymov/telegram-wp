// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeniy Nadymov, 2013-2018.
// 
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class EditChatUsernameView : IDisposable
    {
        private readonly IDisposable _keyPressSubscription;

        public static readonly DependencyProperty AppBarStateProperty =
            DependencyProperty.Register("AppBarState", typeof(string), typeof(EditChatUsernameView), new PropertyMetadata(OnAppBarStateChanged));

        private static void OnAppBarStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = (string)e.NewValue;

            var view = (EditChatUsernameView)d;

            if (state == "Working")
            {
                view._doneButton.IsEnabled = false;
                view._cancelButton.IsEnabled = false;
            }
            else
            {
                view._doneButton.IsEnabled = true;
                view._cancelButton.IsEnabled = true;
            }
        }

        public string AppBarState
        {
            get { return (string)GetValue(AppBarStateProperty); }
            set { SetValue(AppBarStateProperty, value); }
        }

        public EditChatUsernameViewModel ViewModel
        {
            get { return DataContext as EditChatUsernameViewModel; }
        }

        private readonly AppBarButton _doneButton = new AppBarButton
        {
            Text = AppResources.Done,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        public EditChatUsernameView()
        {
            InitializeComponent();

            var keyPressEvents = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                keh => { Username.TextChanged += keh; },
                keh => { Username.TextChanged -= keh; });

            _keyPressSubscription = keyPressEvents
                .Throttle(TimeSpan.FromSeconds(0.3))
                .ObserveOnDispatcher()
                .Subscribe(e => ViewModel.Check());

            _doneButton.Click += (sender, args) => ViewModel.Done();
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();

            Loaded += (sender, args) => BuildLocalizedAppBar();
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar == null)
            {
                ApplicationBar = new ApplicationBar();

                ApplicationBar.Buttons.Add(_doneButton);
                ApplicationBar.Buttons.Add(_cancelButton);
            }
        }

        public void Dispose()
        {
            _keyPressSubscription.Dispose();
        }
    }
}