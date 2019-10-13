// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Media;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using TelegramClient.Converters;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class PlayerView
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof (TLMessage), typeof (PlayerView), new PropertyMetadata(default(TLMessage), OnMessageChanged));

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var playerView = d as PlayerView;
            if (playerView != null)
            {
                playerView.Title.Text = string.Empty;

                var message = e.NewValue as TLMessage;
                if (message != null)
                {
                    var user = message.From as TLUserBase;
                    if (user != null)
                    {
                        playerView.Title.Text = user.FullName2;
                    }
                    var chat = message.From as TLChatBase;
                    if (chat != null)
                    {
                        playerView.Title.Text = chat.FullName2;
                    }

                    var converter = Application.Current.Resources["MessageDateTimeConverter"] as TLIntToDateTimeConverter;
                    if (converter != null)
                    {
                        var date = converter.Convert(message.Date, null, null, null);
                        playerView.Subtitle.Text = date != null ? date.ToString() : string.Empty;
                    }
                }
            }
        }

        public TLMessage Message
        {
            get { return (TLMessage) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public PlayerView()
        {
            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                Border.Background = (Brush)Resources["InputBorderBrushLight"];
            }
            else
            {
                Border.Background = (Brush)Resources["InputBorderBrushDark"];
            }

            UserActionLabel.Visibility = Visibility.Collapsed;

            Loaded += StartOpenStoryboard;
        }

        private void StartOpenStoryboard(object sender, RoutedEventArgs e)
        {
            Loaded -= StartOpenStoryboard;

            Deployment.Current.Dispatcher.BeginInvoke(() => OpenStoryboard.Begin());
        }

        private Action _closeCallback;

        public void Close(Action callback = null)
        {
            _closeCallback = callback;

            CloseStoryboard.Begin();
        }

        private void CloseStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            _closeCallback.SafeInvoke();
        }

        public event EventHandler Closed;

        protected virtual void RaiseClosed()
        {
            var handler = Closed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler Paused;

        protected virtual void RaisePaused()
        {
            EventHandler handler = Paused;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler Resumed;

        protected virtual void RaiseResumed()
        {
            EventHandler handler = Resumed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void CloseButton_OnClick(object sender, GestureEventArgs e)
        {
            Title.Text = "Closed";
            RaiseClosed();
        }

        private void Message_OnTap(object sender, GestureEventArgs e)
        {
            
        }

        public void Pause()
        {
            PlayerToggleButton.IsChecked = false;
        }

        public void Resume()
        {
            PlayerToggleButton.IsChecked = true;
        }

        private void PlayerToggleButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlayerToggleButton.IsChecked == true)
            {
                RaiseResumed();
            }
            else
            {
                RaisePaused();
            }
        }
    }
}
