// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TelegramClient.Views.Additional
{
    public partial class TelegramPasswordBox
    {
        public static readonly DependencyProperty SimpleProperty = DependencyProperty.Register(
            "Simple", typeof (bool), typeof (TelegramPasswordBox), new PropertyMetadata(OnSimpleChanged));

        private static void OnSimpleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramPasswordBox = d as TelegramPasswordBox;
            if (telegramPasswordBox != null)
            {
                telegramPasswordBox.SwitchMode((bool)e.NewValue);
            }
        }

        public bool Simple
        {
            get { return (bool) GetValue(SimpleProperty); }
            set { SetValue(SimpleProperty, value); }
        }

        private void SwitchMode(bool simple)
        {
            if (simple)
            {
                SwitchToPinMode();
            }
            else
            {
                SwitchToPasscodeMode();
            }
        }

        private void SwitchToPasscodeMode()
        {
            Password = string.Empty;
            PinPanel.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;

            var focusedElement = FocusManager.GetFocusedElement();
            if (focusedElement == PinTextBox)
            {
                PasswordBox.Focus();
            }
        }

        private void SwitchToPinMode()
        {
            Password = string.Empty;
            PinPanel.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;

            var focusedElement = FocusManager.GetFocusedElement();
            if (focusedElement == PasswordBox)
            {
                PinTextBox.Focus();
            }
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof (string), typeof (TelegramPasswordBox), new PropertyMetadata(string.Empty));

        public string Password
        {
            get { return (string) GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public TelegramPasswordBox()
        {
            InitializeComponent();

            SwitchMode(Simple);

            GotFocus += OnGotFocus;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (Simple)
            {
                PinTextBox.Focus();
            }
            else
            {
                PasswordBox.Focus();
            }
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            PinTextBox.Focus();
        }

        private void PasswordTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key == Key.Back || e.Key == Key.Enter)
            {
                return;
            }

            e.Handled = true;
        }

        public bool IsFocused
        {
            get
            {
                var focusedElement = FocusManager.GetFocusedElement();

                return focusedElement == PinTextBox || focusedElement == PasswordBox;
            }
        }

        public int Length
        {
            get { return Simple ? PinTextBox.Text.Length : PasswordBox.Password.Length; }
        }

        public event RoutedEventHandler PasswordChanged;

        protected virtual void RaisePasswordChanged(RoutedEventArgs e)
        {
            var handler = PasswordChanged;
            if (handler != null) handler(this, e);
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!Simple)
            {
                RaisePasswordChanged(e);
            }
        }

        private void PinTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Simple)
            {
                RaisePasswordChanged(e);
            }
        }

        private string _clipboardText;

        private void PinTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(string.Empty);

            PinTextBlock.Foreground = (Brush)Application.Current.Resources["PhoneTextBoxForegroundBrush"];
        }

        private void PinTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            PinTextBlock.Foreground = PinTextBox.Foreground;
        }
    }
}
