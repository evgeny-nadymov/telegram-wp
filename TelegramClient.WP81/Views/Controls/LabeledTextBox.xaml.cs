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

namespace TelegramClient.Views.Controls
{
    public partial class LabeledTextBox
    {
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(string), typeof(LabeledTextBox), new PropertyMetadata(default(string), OnErrorChanged));

        private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var labeledTextBox = d as LabeledTextBox;
            if (labeledTextBox != null)
            {
                labeledTextBox.ErrorTextBlock.Text = (string)e.NewValue;
                labeledTextBox.ErrorTextBlock.Visibility = string.IsNullOrEmpty(labeledTextBox.ErrorTextBlock.Text)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public string Error
        {
            get { return (string) GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof (string), typeof (LabeledTextBox), new PropertyMetadata(default(string), OnLabelChanged));

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var labeledTextBox = d as LabeledTextBox;
            if (labeledTextBox != null)
            {
                labeledTextBox.TextBlock.Text = (string) e.NewValue;
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (LabeledTextBox), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var labeledTextBox = d as LabeledTextBox;
            if (labeledTextBox != null)
            {
                labeledTextBox.TextBox.Text = (string)e.NewValue;
                labeledTextBox.TextBlock.Visibility = string.IsNullOrEmpty((string) e.NewValue)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Label
        {
            get { return (string) GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public InputScope InputScope
        {
            get { return TextBox.InputScope; }
            set { TextBox.InputScope = value; }
        }

        public int MaxLength
        {
            get { return TextBox.MaxLength; }
            set { TextBox.MaxLength = value; }
        }

        public int SelectionStart
        {
            get { return TextBox.SelectionStart; }
            set { TextBox.SelectionStart = value; }
        }

        public TextWrapping TextWrapping
        {
            get { return TextBox.TextWrapping; }
            set { TextBox.TextWrapping = value; }
        }

        public bool AcceptReturn
        {
            get { return TextBox.AcceptsReturn; }
            set { TextBox.AcceptsReturn = value; }
        }

        public TextBox Input { get { return TextBox; } }

        public LabeledTextBox()
        {
            InitializeComponent();

            GotFocus += (o, e) =>
            {
                TextBox.Focus();
                if (!string.IsNullOrEmpty(TextBox.Text))
                {
                    TextBox.SelectionStart = TextBox.Text.Length;
                }
            };
        }

        public void SetTextBox(TextBox textBox)
        {
            var inputScope = TextBox.InputScope;
            var maxLength = TextBox.MaxLength;
            LayoutRoot.Children.Remove(TextBox);
            TextBox.GotFocus -= TextBox_OnGotFocus;
            TextBox.LostFocus -= TextBox_OnLostFocus;
            TextBox = textBox;
            LayoutRoot.Children.Insert(0, textBox);
            textBox.GotFocus += TextBox_OnGotFocus;
            textBox.LostFocus += TextBox_OnLostFocus;
            textBox.Style = (Style) Resources["TextBoxStyle1"];
            textBox.InputScope = inputScope;
            textBox.MaxLength = maxLength;
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock.Visibility = string.IsNullOrEmpty(TextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            TextBlock.Foreground = (Brush)Application.Current.Resources["PhoneTextBoxForegroundBrush"];
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Text = TextBox.Text;
            TextBlock.Visibility = string.IsNullOrEmpty(TextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            TextBlock.Foreground = (Brush) Application.Current.Resources["PhoneForegroundBrush"];
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock.Visibility = string.IsNullOrEmpty(TextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
    }
}
