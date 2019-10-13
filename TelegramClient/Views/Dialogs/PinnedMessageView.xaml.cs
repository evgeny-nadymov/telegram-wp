// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class PinnedMessageView
    {
        public PinnedMessageViewModel ViewModel
        {
            get { return DataContext as PinnedMessageViewModel; }
        }

        public PinnedMessageView()
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

            Loaded += (sender, args) =>
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void StartOpenStoryboard(object sender, RoutedEventArgs e)
        {
            Loaded -= StartOpenStoryboard;

            Deployment.Current.Dispatcher.BeginInvoke(() => OpenStoryboard.Begin());
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                if (!ViewModel.IsOpen)
                {
                    CloseStoryboard.Begin();
                }
            }
        }

        private void ButtonBase_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.RaiseUnpinMessage();
        }

        private void Message_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.RaiseOpenMessage();
        }

        private void CloseStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            ViewModel.RaiseClosed();
        }
    }
}
