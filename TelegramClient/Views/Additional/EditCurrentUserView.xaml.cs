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
using TelegramClient.ViewModels.Additional;
using TelegramClient.Views.Controls;

namespace TelegramClient.Views.Additional
{
    public partial class EditCurrentUserView
    {
        public static readonly DependencyProperty AppBarStateProperty =
            DependencyProperty.Register("AppBarState", typeof(string), typeof(EditCurrentUserView), new PropertyMetadata(OnAppBarStateChanged));

        private static void OnAppBarStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = (string)e.NewValue;

            var view = (EditCurrentUserView)d;

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
            get { return (string)GetValue(AppBarStateProperty); }
            set { SetValue(AppBarStateProperty, value); }
        }

        public EditCurrentUserViewModel ViewModel
        {
            get { return DataContext as EditCurrentUserViewModel; }
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

        public EditCurrentUserView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            _doneButton.Click += (sender, args) => ViewModel.Done();
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();

            Loaded += (sender, args) => BuildLocalizedAppBar();
            BackKeyPress += EditCurrentUserView_BackKeyPress;
        }

        private void EditCurrentUserView_BackKeyPress(object sender, CancelEventArgs e)
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
        }

        private void BuildLocalizedAppBar()
        {
            //if (ApplicationBar == null)
            //{
            //    ApplicationBar = new ApplicationBar();

            //    ApplicationBar.Buttons.Add(_doneButton);
            //    ApplicationBar.Buttons.Add(_cancelButton);
            //}
        }

        private void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }
    }
}