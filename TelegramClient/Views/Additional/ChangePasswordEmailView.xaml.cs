// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Input;
using Windows.UI.ViewManagement;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class ChangePasswordEmailView
    {
        public ChangePasswordEmailViewModel ViewModel
        {
            get { return DataContext as ChangePasswordEmailViewModel; }
        }

        public ChangePasswordEmailView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) => RecoveryEmailLabel.Focus();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing += InputPane_Showing;
            InputPane.GetForCurrentView().Hiding += InputPane_Hiding;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing -= InputPane_Showing;
            InputPane.GetForCurrentView().Hiding -= InputPane_Hiding;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            SkipRecoveryEmailTransform.Y = 0.0;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var keyboardHeight = 480.0 / args.OccludedRect.Width * args.OccludedRect.Height;

            SkipRecoveryEmailTransform.Y = -keyboardHeight;
        }

        private void RecoveryEmail_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.ChangeRecoveryEmail();
            }
        }

        private void RecoveryEmail_OnGotFocus(object sender, RoutedEventArgs e)
        {
            SkipRecoveryEmailTransform.Y = - EmojiControl.PortraitOrientationHeight;
        }

        private void RecoveryEmail_OnLostFocus(object sender, RoutedEventArgs e)
        {
            SkipRecoveryEmailTransform.Y = 0;
        }
    }
}