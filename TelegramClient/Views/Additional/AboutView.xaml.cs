// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Telegram.Api.Helpers;
using TelegramClient.Controls;

namespace TelegramClient.Views.Additional
{
    public partial class AboutView
    {
        public AboutView()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                RunAnimation();
            };
        }

        private bool _isForwardInAnimation;

        private void RunAnimation()
        {
            if (_isForwardInAnimation)
            {
                _isForwardInAnimation = false;
                var forwardInAnimation = TelegramTurnstileAnimations.GetAnimation(LayoutRoot, TurnstileTransitionMode.ForwardIn);
                Execute.BeginOnUIThread(forwardInAnimation.Begin);
            }
            else
            {
                LayoutRoot.Opacity = 1.0;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                LayoutRoot.Opacity = 0.0;
                _isForwardInAnimation = true;
            }

            base.OnNavigatedTo(e);
        }
    }
}