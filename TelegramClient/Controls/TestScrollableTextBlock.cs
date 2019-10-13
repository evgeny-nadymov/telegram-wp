// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;

namespace Telegram.EmojiPanel
{
    public class TestScrollableTextBlock : Control
    {
        private StackPanel stackPanel;
        private TextBlock measureTxt;

        public TestScrollableTextBlock()
        {
            DefaultStyleKey = typeof(TestScrollableTextBlock);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(TestScrollableTextBlock),
                new PropertyMetadata("", OnTextPropertyChanged));


        public static readonly DependencyProperty LineHeightProperty
            = DependencyProperty.Register(
            "LineHeight",
            typeof(double),
            typeof(TestScrollableTextBlock), null);

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //TestScrollableTextBlock source = (TestScrollableTextBlock)d;
            //string value = (string)e.NewValue;
            //source.ParseText(value);
        }


        public override void OnApplyTemplate()
        {
            stackPanel = GetTemplateChild("StackPanel") as StackPanel;
            ParseText(Text);

            base.OnApplyTemplate();
            
        }

        private void ParseText(string value)
        {
            if (value == null)
            {
                value = "";
            }

            if (this.stackPanel == null)
            {
                return;
            }
            // Clear previous TextBlocks
            this.stackPanel.Children.Clear();


            bool fitIn2000Pixels = CheckFitInMaxRenderHeight(value);

            if (fitIn2000Pixels)
            {
                RichTextBox textBlock = this.GetTextBlock();
                BrowserNavigationService.SetText(textBlock, value);
                this.stackPanel.Children.Add(textBlock);
            }
            else
            {
                ParseLineExtended(value);
            }
        }

        private readonly int MAX_STR_LENGTH = 1100;

        private void ParseLineExtended(string allText)
        {
            if (string.IsNullOrEmpty(allText))
                return;


            int cutIndex = MAX_STR_LENGTH;
            if (cutIndex >= allText.Length)
                cutIndex = allText.Length - 1;

            var endOfSentenceIndAfterCut = allText.IndexOf(".", cutIndex);

            if (endOfSentenceIndAfterCut >= 0 && endOfSentenceIndAfterCut - cutIndex < 200)
            {
                cutIndex = endOfSentenceIndAfterCut;
            }
            else
            {
                var whiteSpaceIndAfterCut = allText.IndexOf(' ', cutIndex);

                if (whiteSpaceIndAfterCut >= 0 && whiteSpaceIndAfterCut - cutIndex < 100)
                {
                    cutIndex = whiteSpaceIndAfterCut;
                }
            }

            // add all whitespaces before cut
            while (cutIndex + 1 < allText.Length &&
                   allText[cutIndex + 1] == ' ')
            {
                cutIndex++;
            }

            string leftSide = allText.Substring(0, cutIndex + 1);
            RichTextBox textBlock = this.GetTextBlock();
            BrowserNavigationService.SetText(textBlock, leftSide);
            this.stackPanel.Children.Add(textBlock);

            allText = allText.Substring(cutIndex + 1);

            if (allText.Length > 0)
            {
                ParseLineExtended(allText);
            }
        }

        private bool CheckFitInMaxRenderHeight(string value)
        {
            return value.Length <= MAX_STR_LENGTH;
        }

        private RichTextBox GetTextBlock()
        {
            RichTextBox textBlock = new RichTextBox();
            textBlock.TextWrapping = TextWrapping.Wrap;

            textBlock.IsReadOnly = true;
            textBlock.FontSize = this.FontSize;
            textBlock.FontFamily = this.FontFamily;
            textBlock.HorizontalContentAlignment = this.HorizontalContentAlignment;
            textBlock.Foreground = Foreground;
            textBlock.Padding = new Thickness(-12, 0, 0, 0);

            return textBlock;
        }
    }
}
