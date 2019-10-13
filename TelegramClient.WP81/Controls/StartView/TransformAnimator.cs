// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TelegramClient.Controls.StartView
{
    /// <summary>
    /// A utility for animating a horizontal translation value.
    /// </summary>
    internal sealed class TransformAnimator
    {
        /// <summary>
        /// Single static instance of a PropertyPath with string path "X".
        /// </summary>
        private static readonly PropertyPath TranslateXPropertyPath = new PropertyPath("X");

        /// <summary>
        /// The Storyboard instance for the animation.
        /// </summary>
        private readonly Storyboard _sbRunning = new Storyboard();

        /// <summary>
        /// The DoubleAnimation instance for a running animation.
        /// </summary>
        private readonly DoubleAnimation _daRunning = new DoubleAnimation();

        /// <summary>
        /// The target translate transform instance.
        /// </summary>
        private TranslateTransform _transform;

        /// <summary>
        /// A one-time action for the current GoTo statement only. Cleared if
        /// GoTo is called before the action runs.
        /// </summary>
        private Action _oneTimeAction;

        /// <summary>
        /// Initializes a new instance of the TransformAnimator class.
        /// </summary>
        /// <param name="translateTransform">TranslateTransform instance.</param>
        public TransformAnimator(TranslateTransform translateTransform)
        {
            Debug.Assert(translateTransform != null);
            _transform = translateTransform;

            _sbRunning.Completed += OnCompleted;
            _sbRunning.Children.Add(_daRunning);
            Storyboard.SetTarget(_daRunning, _transform);
            Storyboard.SetTargetProperty(_daRunning, TranslateXPropertyPath);
        }

        /// <summary>
        /// Gets the current offset value from the translate transform object.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping the public API available.")]
        public double CurrentOffset
        {
            get { return _transform.X; }
        }

        /// <summary>
        /// Targets a new horizontal offset over a specified duration.
        /// </summary>
        /// <param name="targetOffset">The target offset value.</param>
        /// <param name="duration">The duration for the animation.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping the public API available.")]
        public void GoTo(double targetOffset, Duration duration)
        {
            GoTo(targetOffset, duration, null, null);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping the public API available.")]
        public void GoTo(double targetOffset, Duration duration, Action completionAction)
        {
            GoTo(targetOffset, duration, null, completionAction);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping the public API available.")]
        public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction)
        {
            GoTo(targetOffset, duration, easingFunction, null);
        }

        public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction, Action completionAction)
        {
            _daRunning.To = targetOffset;
            _daRunning.Duration = duration;
            _daRunning.EasingFunction = easingFunction;
            _sbRunning.Begin();
            _sbRunning.SeekAlignedToLastTick(TimeSpan.Zero);
            _oneTimeAction = completionAction;
        }

        /// <summary>
        /// Updates the easing function of the double animation.
        /// </summary>
        /// <param name="ease">The easing funciton, if any, to use.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping existing implementation.")]
        public void UpdateEasingFunction(IEasingFunction ease)
        {
            if (_daRunning != null && _daRunning.EasingFunction != ease)
            {
                _daRunning.EasingFunction = ease;
            }
        }

        /// <summary>
        /// Immediately updates the duration of the running double animation.
        /// </summary>
        /// <param name="duration">The new duration value to use.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping the public API available.")]
        public void UpdateDuration(Duration duration)
        {
            if (_daRunning != null)
            {
                _daRunning.Duration = duration;
            }
        }

        /// <summary>
        /// Handles animation completed
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void OnCompleted(object sender, System.EventArgs e)
        {
            // Make sure the action is called when the animation is Stopped
            // The complete event is triggered when it changes to Filling
            // This is also called before the state has been changed so the state
            // is still Active when switching to Filling
            Action action = _oneTimeAction;
            if (action != null && _sbRunning.GetCurrentState() != ClockState.Active)
            {
                _oneTimeAction = null;
                action();
            }
        }

        /// <summary>
        /// Ensures and creates if needed the animator for an element. Will also
        /// verify that a translate transform is present.
        /// </summary>
        /// <param name="targetElement">The target element.</param>
        /// <param name="animator">The animator reference.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping the public API available.")]
        public static void EnsureAnimator(FrameworkElement targetElement, ref TransformAnimator animator)
        {
            if (animator == null)
            {
                TranslateTransform transform = TransformAnimator.GetTranslateTransform(targetElement);
                if (transform != null)
                {
                    animator = new TransformAnimator(transform);
                }
            }
            if (animator == null)
            {
                throw new InvalidOperationException("The animation system could not be prepared for the target element.");
            }
        }

        /// <summary>
        /// Find a translate transform for the container or create one.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>Returns the TranslateTransform reference.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Keeping the public API available.")]
        public static TranslateTransform GetTranslateTransform(UIElement container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            TranslateTransform transform = container.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                if (container.RenderTransform == null)
                {
                    transform = new TranslateTransform();
                    container.RenderTransform = transform;
                }
                else if (container.RenderTransform is TransformGroup)
                {
                    TransformGroup g = container.RenderTransform as TransformGroup;
                    transform = (from t in g.Children
                                 where t is TranslateTransform
                                 select (TranslateTransform)t).FirstOrDefault();
                    if (transform == null)
                    {
                        transform = new TranslateTransform();
                        g.Children.Add(transform);
                    }
                }
                else
                {
                    TransformGroup tg = new TransformGroup();
                    var existing = container.RenderTransform;
                    container.RenderTransform = null;
                    tg.Children.Add(existing);
                    transform = new TranslateTransform();
                    tg.Children.Add(transform);
                    container.RenderTransform = tg;
                }
            }
            return transform;
        }
    }
}