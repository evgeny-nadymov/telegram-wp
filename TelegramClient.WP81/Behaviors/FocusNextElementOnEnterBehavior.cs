// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TelegramClient.Behaviors
{
    public class FocusNextElementOnEnterBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty NextControlProperty = DependencyProperty.Register(
            "NextControl", typeof (Control), typeof (FocusNextElementOnEnterBehavior), new PropertyMetadata(default(Control)));

        public Control NextControl
        {
            get { return (Control) GetValue(NextControlProperty); }
            set { SetValue(NextControlProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                if (NextControl != null)
                {
                    NextControl.Focus();
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyDown -= AssociatedObject_KeyDown;

            base.OnDetaching();
        }
    }
}