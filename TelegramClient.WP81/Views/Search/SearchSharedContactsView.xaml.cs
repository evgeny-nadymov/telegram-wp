// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows.Input;
using Telegram.Api.Helpers;
using Telegram.Controls;
using TelegramClient.ViewModels.Search;

namespace TelegramClient.Views.Search
{
    public partial class SearchSharedContactsView
    {
        public SearchSharedContactsViewModel ViewModel
        {
            get { return DataContext as SearchSharedContactsViewModel; }
        }

        public SearchSharedContactsView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (o, e) =>
            {
                Execute.BeginOnUIThread(TimeSpan.FromMilliseconds(500), () => SearchBox.Focus());
            };
        }

        private void Items_OnScrollingStateChanged(object sender, ScrollingStateChangedEventArgs e)
        {
            if (e.NewValue)
            {
                var focusElement = FocusManager.GetFocusedElement();
                if (focusElement == SearchBox)
                {
                    Self.Focus();
                }
            }
        }
    }
}