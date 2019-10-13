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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TelegramClient.Animation.Navigation;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class CreateChannelStep2View
    {
        public CreateChannelStep2ViewModel ViewModel
        {
            get { return DataContext as CreateChannelStep2ViewModel; }
        }

        private readonly AppBarButton _nextButton = new AppBarButton
        {
            Text = AppResources.Next,
            IconUri = new Uri("/Images/ApplicationBar/appbar.next.png", UriKind.Relative)
        };

        public CreateChannelStep2View()
        {
            InitializeComponent();

            AnimationContext = LayoutRoot;

            _nextButton.Click += (sender, args) => ViewModel.Next();

            Loaded += (sender, args) =>
            {
                BuildLocalizedAppBar();

                ViewModel.EmptyUserName += OnEmptyUserName;
            };

            Unloaded += (sender, args) =>
            {
                ViewModel.EmptyUserName -= OnEmptyUserName;
            };
        }

        private void OnEmptyUserName(object sender, System.EventArgs e)
        {
            UserName.Focus();
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

        private void CopyInvite_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyInvite();
        }

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            ContextMenuService.GetContextMenu((DependencyObject)sender).IsOpen = true;
        }

        private void ScrollViewer_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (FocusManager.GetFocusedElement() == UserName)
            {
                ScrollViewer.Focus();
            }
        }

        private void CreateChannelStep2View_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups().ToList();
            var popup = popups.FirstOrDefault();
            if (popup != null)
            {
                e.Cancel = true;
                return;
            }
        }
    }
}