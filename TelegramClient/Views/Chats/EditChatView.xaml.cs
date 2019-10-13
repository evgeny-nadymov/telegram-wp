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
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Chats;
using TelegramClient.Views.Controls;

namespace TelegramClient.Views.Chats
{
    public partial class EditChatView
    {
        public static readonly DependencyProperty AppBarStateProperty =
            DependencyProperty.Register("AppBarState", typeof (string), typeof (EditChatView), new PropertyMetadata(OnAppBarStateChanged));

        private static void OnAppBarStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = (string)e.NewValue;

            var view = (EditChatView) d;

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
            get { return (string) GetValue(AppBarStateProperty); }
            set { SetValue(AppBarStateProperty, value); }
        }

        public EditChatViewModel ViewModel { get { return DataContext as EditChatViewModel; }
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

        public EditChatView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            _doneButton.Click += (sender, args) => ViewModel.Done();
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();

            Loaded += (sender, args) => BuildLocalizedAppBar();
        }

        private void BuildLocalizedAppBar()
        {
            return;
            if (ApplicationBar == null)
            {
                ApplicationBar = new ApplicationBar();

                ApplicationBar.Buttons.Add(_doneButton);
                ApplicationBar.Buttons.Add(_cancelButton);
            }
        }

        private void TelegramNavigationTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            ViewModel.ForwardInAnimationComplete();
        }

        private void EditChatView_OnBackKeyPress(object sender, CancelEventArgs e)
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

        private void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }
    }
}