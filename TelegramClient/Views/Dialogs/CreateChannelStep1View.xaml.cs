// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TelegramClient.Helpers;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.Animation.Navigation;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Controls;

namespace TelegramClient.Views.Dialogs
{

    public partial class CreateChannelStep1View
    {
        public CreateChannelStep1ViewModel ViewModel
        {
            get { return DataContext as CreateChannelStep1ViewModel; }
        }

        private readonly AppBarButton _nextButton = new AppBarButton
        {
            Text = AppResources.Next,
            IsEnabled = false,
            IconUri = new Uri("/Images/ApplicationBar/appbar.next.png", UriKind.Relative)
        };

        public CreateChannelStep1View()
        {
            InitializeComponent();

            AnimationContext = LayoutRoot;

            _nextButton.Click += (sender, args) => ViewModel.Next();

            Loaded += (sender, args) =>
            {
                BuildLocalizedAppBar();
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };

            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.CanCreateChannel))
            {
                _nextButton.IsEnabled = ViewModel.CanCreateChannel;
            }
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

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_nextButton);
        }

        private void WhatIsChannel_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowChannelHint();
        }



        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups().ToList();
            var popup = popups.FirstOrDefault();
            if (popup != null)
            {
                e.Cancel = true;

                var multiplePhotoChooser = popup.Child as OpenPhotoPicker;
                if (multiplePhotoChooser != null)
                {
                    multiplePhotoChooser.TryClose();
                }

                var cropControl = popup.Child as CropControl;
                if (cropControl != null)
                {
                    cropControl.TryClose();
                }

                return;
            }

            base.OnBackKeyPress(e);
        }
    }
}