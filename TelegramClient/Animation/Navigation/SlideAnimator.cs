// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Telegram.Controls.Extensions;

namespace TelegramClient.Animation.Navigation
{
    public class SlideAnimator : AnimatorHelperBase
    {
        public override void Begin(Action completionAction)
        {
            if (this.PrepareElement(RootElement))
            {
                Storyboard.Stop();
                base.SetTarget(RootElement);
            }

            base.Begin(completionAction);
        }

        private bool PrepareElement(UIElement element)
        {
            element.GetTransform<CompositeTransform>(TransformCreationMode.CreateOrAddAndIgnoreMatrix);

            return true;
        }
    }

    public class SlideUpAnimator : SlideAnimator
    {
        private static Storyboard _storyboard;

        public SlideUpAnimator()
            : base()
        {
            if (_storyboard == null)
                _storyboard = XamlReader.Load(Storyboards.SlideUpFadeInStoryboard) as Storyboard;
            Storyboard = _storyboard;
        }
    }

    public class SlideDownAnimator : SlideAnimator
    {
        private static Storyboard _storyboard;

        public SlideDownAnimator()
            : base()
        {
            if (_storyboard == null)
                _storyboard = XamlReader.Load(Storyboards.SlideDownFadeOutStoryboard) as Storyboard;

            Storyboard = _storyboard;
        }
    }

    public class SlideDownSwivelShowAnimator : SlideAnimator
    {
        
    }
}
