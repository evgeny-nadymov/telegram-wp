// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Input;
using Telegram.Api.TL;
using Telegram.Controls;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Search
{
    public partial class SearchVenuesView
    {
        public SearchVenuesViewModel ViewModel
        {
            get { return DataContext as SearchVenuesViewModel; }
        }

        public SearchVenuesView()
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

        private void Venue_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var mediaVenue = frameworkElement.DataContext as TLMessageMediaVenue;
                if (mediaVenue != null)
                {
                    ViewModel.AttachVenue(mediaVenue);
                }
            }
        }
    }
}