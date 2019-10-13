// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Telegram.Controls
{
    /// <summary>
    /// Class that acts as a Mediator between a Storyboard animation and a
    /// Transform used by the Silverlight Toolkit's LayoutTransformer.
    /// </summary>
    /// <remarks>
    /// Works around an issue with the Silverlight platform where changes to
    /// properties of child Transforms assigned to a Transform property do not
    /// trigger the top-level property changed handler (as on WPF).
    /// </remarks>
    public class AnimationMediator : FrameworkElement
    {
        /// <summary>
        /// Gets or sets a reference to the LayoutTransformer to update.
        /// </summary>
        //public LayoutTransformer LayoutTransformer { get; set; }

        public static readonly DependencyProperty LayoutTransformerProperty =
            DependencyProperty.Register("LayoutTransformer", typeof (LayoutTransformer), typeof (AnimationMediator), new PropertyMetadata(default(LayoutTransformer)));

        public LayoutTransformer LayoutTransformer
        {
            get { return (LayoutTransformer) GetValue(LayoutTransformerProperty); }
            set { SetValue(LayoutTransformerProperty, value); }
        }

        /// <summary>
        /// Gets or sets the name of the LayoutTransformer to update.
        /// </summary>
        /// <remarks>
        /// This property is used iff the LayoutTransformer property is null.
        /// </remarks>
        public string LayoutTransformerName
        {
            get
            {
                return _layoutTransformerName;
            }
            set
            {
                _layoutTransformerName = value;
                // Force a new name lookup
                LayoutTransformer = null;
            }
        }
        private string _layoutTransformerName;

        /// <summary>
        /// Gets or sets the value being animated.
        /// </summary>
        public double AnimationValue
        {
            get { return (double)GetValue(AnimationValueProperty); }
            set { SetValue(AnimationValueProperty, value); }
        }
        public static readonly DependencyProperty AnimationValueProperty =
            DependencyProperty.Register(
                "AnimationValue",
                typeof(double),
                typeof(AnimationMediator),
                new PropertyMetadata(AnimationValuePropertyChanged));
        private static void AnimationValuePropertyChanged(
            DependencyObject o,
            DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine(e.NewValue);
            ((AnimationMediator)o).AnimationValuePropertyChanged();
        }
        private void AnimationValuePropertyChanged()
        {
            if (null == LayoutTransformer)
            {
                // No LayoutTransformer set; try to find it by LayoutTransformerName
                LayoutTransformer = FindName(LayoutTransformerName) as LayoutTransformer;
                if (null == LayoutTransformer)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "AnimationMediator was unable to find a LayoutTransformer named \"{0}\".",
                        LayoutTransformerName));
                }
            }
            // The Transform hasn't been updated yet; schedule an update to run after it has
            Dispatcher.BeginInvoke(() => LayoutTransformer.ApplyLayoutTransform());
        }
    }
}
