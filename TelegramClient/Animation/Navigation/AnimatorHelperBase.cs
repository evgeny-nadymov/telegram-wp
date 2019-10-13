// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace TelegramClient.Animation.Navigation
{

    public enum AnimationType
    {
        PivotInLeft,
        PivotInRight,
        PivotOutLeft,
        PivotOutRight,
        NavigateBackwardIn,
        NavigateBackwardOut,
        NavigateForwardIn,
        NavigateForwardOut,
        Appear,
        Disappear
    }

    public abstract class AnimatorHelperBase
    {
        private Action _oneTimeAction;
        public Storyboard Storyboard { get; set; }

        // Methods
        protected AnimatorHelperBase()
        {

        }

        private void OnCompleted(object sender, System.EventArgs e)
        {
            Storyboard.Completed -= new EventHandler(OnCompleted);
            Action action = _oneTimeAction;
            if (action != null)
            {
                _oneTimeAction = null;
                action();
            }
        }

        public virtual void Begin(Action completionAction)
        {
            Storyboard.Stop();
            Storyboard.Begin();
            Storyboard.SeekAlignedToLastTick(TimeSpan.Zero);
            Storyboard.Completed += new EventHandler(OnCompleted);
            _oneTimeAction = completionAction;
        }

        public void SetTargets(Dictionary<string, FrameworkElement> targets, Storyboard sb)
        {
            foreach (var kvp in targets)
            {
                var timelines = sb.Children.Where(t => Storyboard.GetTargetName(t) == kvp.Key);
                foreach (Timeline t in timelines)
                    Storyboard.SetTarget(t, kvp.Value);
            }
        }

        public void SetTargets(Dictionary<string, FrameworkElement> targets)
        {
            SetTargets(targets, Storyboard);
        }

        public void SetTarget(FrameworkElement target)
        {
            foreach (Timeline t in Storyboard.Children)
                Storyboard.SetTarget(t, target);
        }

        public FrameworkElement RootElement { get; set; }
    }
}
