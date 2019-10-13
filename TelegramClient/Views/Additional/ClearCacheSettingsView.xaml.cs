// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Additional;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class ClearCacheSettingsView : UserControl
    {
        public ClearCacheSettingsViewModel ViewModel
        {
            get { return DataContext as ClearCacheSettingsViewModel; }
        }

        private bool _once;

        public ClearCacheSettingsView()
        {
            InitializeComponent();

            Loaded += (o, e) =>
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
            Unloaded += (o, e) => ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                var frame = Bootstrapper.PhoneFrame;
                if (frame != null)
                {
                    var currentPage = frame.Content as PhoneApplicationPage;
                    if (currentPage != null && currentPage.ApplicationBar != null)
                    {
                        currentPage.ApplicationBar.IsVisible = !ViewModel.IsOpen;
                    }
                }

                if (ViewModel.IsOpen)
                {
                    //OpenContactItem.Visibility = ViewModel.OpenContactVisibility;
                    OpenStoryboard.Begin();
                }
                else
                {
                    CloseStoryboard.Begin();
                }
            }
        }

        //private void LayoutRoot_OnTap(object sender, GestureEventArgs e)
        //{
        //    ViewModel.Close();
        //}
        private void LayoutRoot_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.Close();
        }

        private void ContentPanel_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }
    }
}
