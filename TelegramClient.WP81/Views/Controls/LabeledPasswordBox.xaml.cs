// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;

namespace TelegramClient.Views.Controls
{
    public partial class LabeledPasswordBox
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof (string), typeof (LabeledPasswordBox), new PropertyMetadata(default(string), OnLabelChanged));

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var labeledTextBox = d as LabeledPasswordBox;
            if (labeledTextBox != null)
            {
                labeledTextBox.TextBlock.Text = (string) e.NewValue;
            }
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof (string), typeof (LabeledPasswordBox), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var labeledTextBox = d as LabeledPasswordBox;
            if (labeledTextBox != null)
            {
                labeledTextBox.TextBox.Password = (string)e.NewValue;
                labeledTextBox.TextBlock.Visibility = string.IsNullOrEmpty((string) e.NewValue)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public string Password
        {
            get { return (string) GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public string Label
        {
            get { return (string) GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public int MaxLength
        {
            get { return TextBox.MaxLength; }
            set { TextBox.MaxLength = value; }
        }

        public PasswordBox Input { get { return TextBox; } }

        public LabeledPasswordBox()
        {
            InitializeComponent();

            TextBox.LostFocus += (sender, args) =>
            {
                Password = Input.Password;
            };

            GotFocus += (o, e) =>
            {
                TextBox.Focus();
                if (!string.IsNullOrEmpty(TextBox.Password))
                {
                    TextBox.SelectAll();
                }
            };
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock.Visibility = Visibility.Collapsed;
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBlock.Visibility = string.IsNullOrEmpty(TextBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
    }
}
