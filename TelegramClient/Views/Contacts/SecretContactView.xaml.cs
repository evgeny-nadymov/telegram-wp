// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Shell;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.Views.Media;

namespace TelegramClient.Views.Contacts
{
    public partial class SecretContactView
    {
        private IApplicationBar _prevAppBar;

        public SecretContactViewModel ViewModel
        {
            get { return DataContext as SecretContactViewModel; }
        }

        public SecretContactView()
        {
            var timer = Stopwatch.StartNew();

            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                Caption.Background = ShellView.CaptionBrush;
            }

            OptimizeFullHD();

            Loaded += (sender, args) =>
            {
                TimerString.Text = timer.Elapsed.ToString();

                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
        }

        private void OptimizeFullHD()
        {
#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            //if (!isFullHD) return;
#endif
            Items.HeaderTemplate = (DataTemplate)Application.Current.Resources["FullHDPivotHeaderTemplate"];
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer)
                && ViewModel.ImageViewer != null)
            {
                ViewModel.ImageViewer.PropertyChanged += OnImageViewerPropertyChanged;
            }
        }

        private void OnImageViewerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.ImageViewer.IsOpen))
            {
                Items.IsHitTestVisible = !ViewModel.ImageViewer.IsOpen;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (ViewModel.ImageViewer != null && ViewModel.ImageViewer.IsOpen)
            {
                ViewModel.ImageViewer.CloseViewer();
                e.Cancel = true;
                return;
            }

            base.OnBackKeyPress(e);
        }

        private void NavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            
        }
    }
}