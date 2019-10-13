// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Views.Dialogs;

namespace TelegramClient.ViewModels.Dialogs
{
    public class DialogSearchMessagesViewModel : TelegramPropertyChangedBase
    {
        public string NavigationButtonImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
                return isLightTheme ? "/Images/ApplicationBar/appbar.next.light.png" : "/Images/ApplicationBar/appbar.next.png";
            }
        }

        public string UserButtonImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
                return isLightTheme ? "/Images/ApplicationBar/appbar.user.light.png" : "/Images/ApplicationBar/appbar.user.dark.png";
            }
        }

        public string CalendarButtonImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
                return isLightTheme ? "/Images/ApplicationBar/feature.calendar.light.png" : "/Images/ApplicationBar/feature.calendar.png";
            }
        }

        private bool _isSearchUpEnabled;

        public bool IsSearchUpEnabled
        {
            get { return _isSearchUpEnabled; }
            set
            {
                if (_isSearchUpEnabled != value)
                {
                    _isSearchUpEnabled = value;
                    NotifyOfPropertyChange(() => IsSearchUpEnabled);
                }
            }
        }

        private bool _isSearchDownEnabled;

        public bool IsSearchDownEnabled
        {
            get { return _isSearchDownEnabled; }
            set
            {
                if (_isSearchDownEnabled != value)
                {
                    _isSearchDownEnabled = value;
                    NotifyOfPropertyChange(() => IsSearchDownEnabled);
                }
            }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            protected set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    NotifyOfPropertyChange(() => IsOpen);
                }
            }
        }

        private DateTime? _date;

        public DateTime? Date
        {
            get { return _date; }
            set
            {
                if (value != _date)
                {
                    _date = value;
                    NotifyOfPropertyChange(() => DateBrush);
                }
            }
        }

        public Brush DateBrush
        {
            get
            {
                if (Date == null || Date == DateTime.Now.Date)
                {
                    return (Brush) Application.Current.Resources["PhoneForegroundBrush"];
                }

                return (Brush)Application.Current.Resources["PhoneAccentBrush"];
            }
        }

        private readonly Action<string, DateTime?, TLUserBase> _searchAction;

        private readonly Action<string> _searchUserAction;

        private readonly System.Action _upAction;

        private readonly System.Action _downAction;

        private readonly Func<IList<TLUserBase>> _getUsersFunc; 

        public void ResultLoaded(int current, int count)
        {
            IsSearchUpEnabled = count > 0 && current < count - 1;
            IsSearchDownEnabled = count > 0 && current > 0;
        }

        public ObservableCollection<TLUserBase> Hints { get; set; }

        public TLUserBase From { get; set; }

        private readonly TLObject _with;

        public Visibility UserButtonVisibility
        {
            get
            {
                var channel = _with as TLChannel;
                return channel != null && channel.IsMegaGroup ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public DialogSearchMessagesViewModel(TLObject with, Action<string, DateTime?, TLUserBase> searchAction, Action<string> searchUserAction, System.Action upAction, System.Action downAction, Func<IList<TLUserBase>> getUsersFunc)
        {
            _with = with;
            Hints = new ObservableCollection<TLUserBase>();

            _searchAction = searchAction;
            _searchUserAction = searchUserAction;
            _upAction = upAction;
            _downAction = downAction;
            _getUsersFunc = getUsersFunc;
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void Search()
        {
            _searchAction.SafeInvoke(Text, Date, From);
        }

        public void SearchUser()
        {
            _searchUserAction.SafeInvoke(Text);
        }

        public void Up()
        {
            _upAction.SafeInvoke();
        }

        public void Down()
        {
            _downAction.SafeInvoke();
        }

        public void GetUsers()
        {
            var users = _getUsersFunc.Invoke();
            Hints.Clear();
            foreach (var user in users)
            {
                Hints.Add(user);
            }
        }
    }
}
