// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using Microsoft.Phone.Controls;

namespace TelegramClient.Controls
{
    /// <summary>
    /// Has
    /// <see cref="T:Microsoft.Phone.Controls.TransitionElement"/>s
    /// for the designer experiences.
    /// </summary>
    public class TelegramNavigationTransition : DependencyObject
    {
        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the backward
        /// <see cref="T:Microsoft.Phone.Controls.NavigationTransition"/>.
        /// </summary>
        public static readonly DependencyProperty BackwardProperty =
            DependencyProperty.Register("Backward", typeof(TransitionElement), typeof(TelegramNavigationTransition), null);

        /// <summary>
        /// The
        /// <see cref="T:System.Windows.DependencyProperty"/>
        /// for the forward
        /// <see cref="T:Microsoft.Phone.Controls.NavigationTransition"/>.
        /// </summary>
        public static readonly DependencyProperty ForwardProperty =
            DependencyProperty.Register("Forward", typeof(TransitionElement), typeof(TelegramNavigationTransition), null);

        /// <summary>
        /// The navigation transition will begin.
        /// </summary>
        public event RoutedEventHandler BeginTransition;

        /// <summary>
        /// The navigation transition has ended.
        /// </summary>
        public event RoutedEventHandler EndTransition;

        /// <summary>
        /// Gets or sets the backward
        /// <see cref="T:Microsoft.Phone.Controls.NavigationTransition"/>.
        /// </summary>
        public TransitionElement Backward
        {
            get
            {
                return (TransitionElement)GetValue(BackwardProperty);
            }
            set
            {
                SetValue(BackwardProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the forward
        /// <see cref="T:Microsoft.Phone.Controls.NavigationTransition"/>.
        /// </summary>
        public TransitionElement Forward
        {
            get
            {
                return (TransitionElement)GetValue(ForwardProperty);
            }
            set
            {
                SetValue(ForwardProperty, value);
            }
        }

        /// <summary>
        /// Triggers <see cref="E:Microsoft.Phone.Controls.NavigationTransition.BeginTransition"/>.
        /// </summary>
        internal void OnBeginTransition()
        {
            if (BeginTransition != null)
            {
                BeginTransition(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Triggers <see cref="E:Microsoft.Phone.Controls.NavigationTransition.EndTransition"/>.
        /// </summary>
        internal void OnEndTransition()
        {
            if (EndTransition != null)
            {
                EndTransition(this, new RoutedEventArgs());
            }
        }
    }
}
