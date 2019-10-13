// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Additional;
using Telegram.Controls.Extensions;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class AddChatParticipantConfirmationView
    {
        public AddChatParticipantConfirmationViewModel ViewModel
        {
            get { return DataContext as AddChatParticipantConfirmationViewModel; }
        }

        private PhoneApplicationPage _page;

        private ScrollViewer _scrollViewer;

        public AddChatParticipantConfirmationView()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (_page == null)
                    {
                        _page = this.FindParentOfType<PhoneApplicationPage>();
                    }

                    if (_page != null)
                    {
                        _scrollViewer = _page.FindChildOfType<ScrollViewer>();

                        _page.BackKeyPress += ParentPage_OnBackKeyPressed;
                    }

                    ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                });
            };

            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;

                if (_page != null)
                {
                    _page.BackKeyPress -= ParentPage_OnBackKeyPressed;
                }
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                if (_page != null)
                {
                    var scrollViewer = _scrollViewer;
                    if (scrollViewer != null)
                    {
                        scrollViewer.IsHitTestVisible = !ViewModel.IsOpen;
                    }

                    var appBar = _page.ApplicationBar;
                    if (appBar != null)
                    {
                        appBar.IsVisible = !ViewModel.IsOpen;
                    }
                }
            }
        }

        private void ParentPage_OnBackKeyPressed(object sender, CancelEventArgs e)
        {
            if (ViewModel.IsOpen)
            {
                ViewModel.Close(MessageBoxResult.Cancel);
                e.Cancel = true;
            }
        }

        private void LayoutRoot_OnTap(object sender, GestureEventArgs args)
        {
            ViewModel.Close(MessageBoxResult.Cancel);
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Close(MessageBoxResult.OK);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Close(MessageBoxResult.Cancel);
        }

        private void ContentPanel_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void CloseStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            
        }
    }
}
