// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeniy Nadymov, 2013-2018.
// 
using System.Windows;
using System.Windows.Controls;

namespace Telegram.Controls
{
    public class ValidationTextBox : TextBox
    {
        public ValidationTextBox()
        {
            DefaultStyleKey = typeof(ValidationTextBox);
        }

        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register(
            "HasError", typeof (bool), typeof (ValidationTextBox), new PropertyMetadata(default(bool), OnHasErrorChanged));

        private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var validationTextBox = (ValidationTextBox)d;
            if ((bool)e.NewValue)
            {
                VisualStateManager.GoToState(validationTextBox, "Invalid", true);
            }
            else
            {
                VisualStateManager.GoToState(validationTextBox, "Valid", true);
            }
        }

        public bool HasError
        {
            get { return (bool) GetValue(HasErrorProperty); }
            set { SetValue(HasErrorProperty, value); }
        }
    }
}
