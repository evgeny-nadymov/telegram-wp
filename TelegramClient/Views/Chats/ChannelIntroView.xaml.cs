// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using TelegramClient.Animation.Navigation;

namespace TelegramClient.Views.Chats
{
    public partial class ChannelIntroView
    {
        public ChannelIntroView()
        {
            InitializeComponent();

            AnimationContext = LayoutRoot;
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
    }
}