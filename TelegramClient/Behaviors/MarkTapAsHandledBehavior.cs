// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TelegramClient.Behaviors
{
    public class HandledEventTrigger : System.Windows.Interactivity.EventTrigger
    {
        protected override void OnEvent(System.EventArgs eventArgs)
        {
            var routedEventArgs = eventArgs as GestureEventArgs;
            if (routedEventArgs != null)
                routedEventArgs.Handled = true;

            base.OnEvent(eventArgs);
        }
    }

    public class MarkTapAsHandledBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Tap += AssociatedObjectOnTap;

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Tap -= AssociatedObjectOnTap;
        }

        private void AssociatedObjectOnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            gestureEventArgs.Handled = true;
        }
    }
}
