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
using System.Windows.Documents;
using Microsoft.Phone.Shell;

namespace TelegramClient.Views.Chats
{
    public partial class ConvertToSupergroupView
    {
        public ConvertToSupergroupView()
        {
            InitializeComponent();

            OptimizeFullHD();
        }

        private void OptimizeFullHD()
        {
            var appBar = new ApplicationBar();
            var appBarDefaultSize = appBar.DefaultSize;

            //ConvertToSupergroupPanel.Height = appBarDefaultSize;
        }

        public static readonly DependencyProperty FormattedLinkedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedLinkedText", typeof (string), typeof (ConvertToSupergroupView), new PropertyMetadata(default(string), OnFormattedLinkedTextChanged));

        public static void SetFormattedLinkedText(DependencyObject element, string value)
        {
            element.SetValue(FormattedLinkedTextProperty, value);
        }

        public static string GetFormattedLinkedText(DependencyObject element)
        {
            return (string) element.GetValue(FormattedLinkedTextProperty);
        }

        private static void OnFormattedLinkedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var r = d as RichTextBox;
            if (r != null)
            {
                var text = e.NewValue as string;
                if (text != null)
                {
                    var splittedText = text.Split(new[] { "__" }, StringSplitOptions.None);
                    var paragraph = new Paragraph();
                    for (var i = 0; i < splittedText.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            var underline = new Run();
                            //bold.FontWeight = FontWeights.SemiBold;
                            underline.Text = splittedText[i];
                            underline.TextDecorations = TextDecorations.Underline;
                            paragraph.Inlines.Add(underline);
                        }
                        else
                        {
                            paragraph.Inlines.Add(splittedText[i]);
                        }
                    }
                    r.Blocks.Clear();
                    r.Blocks.Add(paragraph);
                }
            }
        }

        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedText", typeof(string), typeof(ConvertToSupergroupView), new PropertyMetadata(default(string), OnFormattedTextChanged));

        public static void SetFormattedText(DependencyObject element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject element)
        {
            return (string)element.GetValue(FormattedTextProperty);
        }

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var r = d as RichTextBox;
            if (r != null)
            {
                var text = e.NewValue as string;
                if (text != null)
                {
                    var splittedText = text.Split(new []{"**"}, StringSplitOptions.None);
                    var paragraph = new Paragraph();
                    for (var i = 0; i < splittedText.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            var bold = new Run();
                            bold.FontWeight = FontWeights.SemiBold;
                            bold.Text = splittedText[i];
                            paragraph.Inlines.Add(bold);
                        }
                        else
                        {
                            paragraph.Inlines.Add(splittedText[i]);
                        }
                    }
                    r.Blocks.Clear();
                    r.Blocks.Add(paragraph);
                }
            }
        }
    }
}