// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using TelegramClient.Views.Controls;

namespace TelegramClient.Behaviors
{
    public class UpdateTextBindingBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var binding = AssociatedObject.GetBindingExpression(TextBox.TextProperty);
            if (binding != null) binding.UpdateSource();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;

            base.OnDetaching();
        }
    }

    public class UpdatePasswordBindingBehavior : Behavior<PasswordBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PasswordChanged += AssociatedObject_PasswordChanged;
        }

        private void AssociatedObject_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var binding = AssociatedObject.GetBindingExpression(PasswordBox.PasswordProperty);
            if (binding != null) binding.UpdateSource();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= AssociatedObject_PasswordChanged;

            base.OnDetaching();
        }
    }

    public class UpdateLabeledTextBindingBehavior : Behavior<LabeledTextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Input.TextChanged += AssociatedObject_TextChanged;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            AssociatedObject.Text = AssociatedObject.Input.Text;

            var binding = AssociatedObject.GetBindingExpression(LabeledTextBox.TextProperty);
            if (binding != null) binding.UpdateSource();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Input.TextChanged -= AssociatedObject_TextChanged;

            base.OnDetaching();
        }
    }
}
