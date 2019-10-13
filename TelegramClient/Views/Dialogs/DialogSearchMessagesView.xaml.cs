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
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class DialogSearchMessagesView
    {
        public DialogSearchMessagesViewModel ViewModel
        {
            get { return DataContext as DialogSearchMessagesViewModel; }
        }

        private bool _once;

        public DialogSearchMessagesView()
        {
            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            //BackgroundBorder.Background = isLightTheme
            //    ? (Brush)Resources["HintsBackgroundBrushLight"]
            //    : (Brush)Resources["HintsBackgroundBrushDark"];
            Border.Background = isLightTheme
                ? (Brush)Resources["InputBorderBrushLight"]
                : (Brush)Resources["InputBorderBrushDark"];

            SearchLabel.Visibility = Visibility.Collapsed;

            OpenStoryboard.Completed += (sender, args) =>
            {
                Text.Focus();
            };

            CloseStoryboard.Completed += (sender, args) =>
            {
                ViewModel.From = null;
                Text.Text = string.Empty;
                if (_searchUserControl == null)
                {
                    _searchUserControl = Text.FindChildOfType<SearchUserControl>();
                }

                if (_searchUserControl.Visibility == Visibility.Visible)
                {
                    _searchUserControl.Visibility = Visibility.Collapsed;
                    _searchUserControl.Text = string.Empty;
                }

                ViewModel.From = null;
                ViewModel.IsSearchDownEnabled = false;
                ViewModel.IsSearchUpEnabled = false;
            };

            Loaded += (sender, args) =>
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;

                if (!_once)
                {
                    _once = true;
                    if (ViewModel.IsOpen)
                    {
                        OpenStoryboard.Begin();
                    }
                }
            };

            Unloaded += (sender, args) =>
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
                    OpenStoryboard.Begin();
                }
                else
                {
                    ViewModel.Hints.Clear();
                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                    {
                        CloseStoryboard.Begin();
                    });
                }
            }
        }

        private void ButtonBase_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Close();
        }

        private void ButtonUp_OnClick(object sender, GestureEventArgs gestureEventArgs)
        {
            SearchControl.Focus();
            ViewModel.Up();
        }

        private void ButtonDown_OnClick(object sender, GestureEventArgs gestureEventArgs)
        {
            SearchControl.Focus();
            ViewModel.Down();
        }

        private void CalendarButton_OnClick(object sender, GestureEventArgs e)
        {
            var datePickerPage = new DatePickerPage { Height = 239.0, Value = ViewModel.Date ?? DateTime.Now.Date, Margin = new Thickness(0.0, -34.0, 0.0, -6.0)};
            string leftButtonContent = null;
            if (ViewModel.Date != null && ViewModel.Date != DateTime.Now.Date)
            {
                leftButtonContent = AppResources.Clear.ToLowerInvariant();
            }
            ShellViewModel.ShowCustomMessageBox(null, null, AppResources.JumpToDate.ToLowerInvariant(), leftButtonContent,
                result =>
                {
                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        var selector = GetChildOfType<LoopingSelector>(datePickerPage);
                        var value = ((DateTimeWrapper)selector.DataSource.SelectedItem).DateTime;

                        ViewModel.Date = value;
                        ViewModel.Search();
                    }
                    else if (result == CustomMessageBoxResult.LeftButton)
                    {
                        ViewModel.Date = null;
                    }
                },
                datePickerPage);
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }


        private SearchUserControl _searchUserControl;

        private void ButtonUser_OnClick(object sender, GestureEventArgs e)
        {
            if (_searchUserControl == null)
            {
                _searchUserControl = Text.FindChildOfType<SearchUserControl>();
            }

            _searchUserControl.Text = "";
            _searchUserControl.Visibility = Visibility.Visible;
            Text.Focus();

            ViewModel.From = null;
            ViewModel.GetUsers();
        }

        private void Text_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (string.IsNullOrEmpty(ViewModel.Text))
                {
                    if (_searchUserControl == null)
                    {
                        _searchUserControl = Text.FindChildOfType<SearchUserControl>();
                    }

                    if (_searchUserControl.Visibility == Visibility.Visible)
                    {
                        _searchUserControl.Visibility = Visibility.Collapsed;
                    }

                    ViewModel.From = null;
                    ViewModel.Hints.Clear();
                }
            }
        }

        private void UserHint_OnTap(object sender, GestureEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem != null)
            {
                var user = listBoxItem.DataContext as TLUserBase;
                if (user != null)
                {
                    if (_searchUserControl == null)
                    {
                        _searchUserControl = Text.FindChildOfType<SearchUserControl>();
                    }

                    _searchUserControl.Text = NonBreakingStringConverter.Convert(user.ShortName) as string;

                    ViewModel.From = user;

                    ViewModel.Hints.Clear();
                    ViewModel.Text = string.Empty;
                    Text.Focus();
                    ViewModel.Search();
                }
            }
        }

        private void Text_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchUserControl == null)
            {
                _searchUserControl = Text.FindChildOfType<SearchUserControl>();
            }

            if (_searchUserControl.Visibility == Visibility.Visible
                && ViewModel.From == null)
            {
                if (!string.Equals(ViewModel.Text, Text.Text))
                {
                    ViewModel.Text = Text.Text;
                    ViewModel.SearchUser();
                }
            }
            else
            {
                if (!string.Equals(ViewModel.Text, Text.Text))
                {
                    ViewModel.Text = Text.Text;
                    ViewModel.Search();
                }
            }
        }

        private void Text_OnGotFocus(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
