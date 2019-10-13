using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Telegram.EmojiPanel
{
    public class TelegramRichTextBox : Control
    {
        public static readonly DependencyProperty MoreElementProperty = DependencyProperty.Register(
            "MoreElement", typeof (FrameworkElement), typeof (TelegramRichTextBox), new PropertyMetadata(default(FrameworkElement), OnMoreElementChanged));

        private static void OnMoreElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramRichTextBox = (TelegramRichTextBox) d;
            telegramRichTextBox.OnSizeChanged(null, null);
        }

        public FrameworkElement MoreElement
        {
            get { return (FrameworkElement) GetValue(MoreElementProperty); }
            set { SetValue(MoreElementProperty, value); }
        }

        public static readonly DependencyProperty TextScaleFactorProperty = DependencyProperty.Register(
            "TextScaleFactor", typeof (double), typeof (TelegramRichTextBox), new PropertyMetadata(1.0, OnFontScaleFactorChanged));

        private static void OnFontScaleFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TelegramRichTextBox)d;
            if (textBox != null && textBox._stackPanel != null)
            {
                foreach (var child in textBox._stackPanel.Children)
                {
                    var richTextBox = child as RichTextBox;
                    if (richTextBox != null)
                    {
                        richTextBox.FontSize = textBox._defaultFontSize*(double) e.NewValue;
                        foreach (var block in richTextBox.Blocks)
                        {
                            var paragraph = block as Paragraph;
                            if (paragraph != null)
                            {
                                foreach (var inline in paragraph.Inlines)
                                {
                                    var uiContainer = inline as InlineUIContainer;
                                    if (uiContainer != null)
                                    {
                                        var image = uiContainer.Child as Image;
                                        if (image != null)
                                        {
                                            var size = 27.0 * (double)e.NewValue;
                                            image.Height = size;
                                            image.Width = size;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private double _defaultFontSize;

        public double TextScaleFactor
        {
            get { return (double) GetValue(TextScaleFactorProperty); }
            set { SetValue(TextScaleFactorProperty, value); }
        }

        private StackPanel _stackPanel;
        private TextBlock measureTxt;

        public TelegramRichTextBox()
        {
            DefaultStyleKey = typeof(TelegramRichTextBox);

            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MoreElement != null)
            {
                if (ActualHeight > 0.0 && MaxHeight > 0.0 && ActualHeight >= MaxHeight)
                {
                    MoreElement.Visibility = Visibility.Visible;
                }
                else
                {
                    MoreElement.Visibility = Visibility.Collapsed;
                }
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(TelegramRichTextBox),
                new PropertyMetadata("", OnTextPropertyChanged));

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

        public static readonly DependencyProperty DateTextProperty = DependencyProperty.Register(
            "DateText", typeof (string), typeof (TelegramRichTextBox), new PropertyMetadata(default(string)));

        public string DateText
        {
            get { return (string) GetValue(DateTextProperty); }
            set { SetValue(DateTextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TelegramRichTextBox source = (TelegramRichTextBox)d;
            string value = (string)e.NewValue;
            source.ParseText(value);
        }


        public override void OnApplyTemplate()
        {
            _defaultFontSize = FontSize;
            _stackPanel = GetTemplateChild("StackPanel") as StackPanel;
            ParseText(Text);

            base.OnApplyTemplate();

        }

        private void ParseText(string value)
        {
            if (value == null)
            {
                value = "";
            }

            if (_stackPanel == null)
            {
                return;
            }
            // Clear previous TextBlocks
            _stackPanel.Children.Clear();


            bool fitIn2000Pixels = CheckFitInMaxRenderHeight(value);

            if (fitIn2000Pixels)
            {
                var textBlock = GetTextBlock();
                BrowserNavigationService.SetText(textBlock, value);
                _stackPanel.Children.Add(textBlock);
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
            this._stackPanel.Children.Add(textBlock);

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
            var textBlock = new RichTextBox();

            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.IsReadOnly = true;
            textBlock.FontSize = FontSize * TextScaleFactor;
            textBlock.FontFamily = FontFamily;
            textBlock.HorizontalContentAlignment = HorizontalContentAlignment;
            textBlock.Foreground = Foreground;
            textBlock.Padding = new Thickness(0, 0, 0, 0);

            return textBlock;
        }

    }

    public class Utils
    {
        public static readonly Regex HyperlinkRegex = new Regex("(?i)\\b(((?:https?://|www\\d{0,3}[.]|[a-z0-9.\\-]+[.][a-z]{2,4}/)(?:[^\\s()<>]+|\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\))+(?:\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))|([a-z0-9.\\-]+(\\.ru|\\.com|\\.net|\\.org|\\.us|\\.it|\\.co\\.uk)(?![a-z0-9]))|([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]*[a-zA-Z0-9-]+))");

        public static string GetFormattedLink(string link)
        {
            var flag = link.EndsWith(",");
            if (flag && link.Length > 1)
                link = link.Substring(0, link.Length - 1);
            if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                link = "http://" + link;

            return link;
        }

        public static Inline GetTextBlock(string text)
        {
            return new Run { Text = text };
        }

        public static Inline GetHyperlinkBlock(string text, string link)
        {
            var hyperlink = new Hyperlink();
            hyperlink.Inlines.Add(text);
            hyperlink.NavigateUri = new Uri(link, UriKind.Absolute);

            return hyperlink;
        }

        public static Inline GetEmojiBlock(string emoji)
        {
            var uiContainer = new InlineUIContainer();
            uiContainer.Child = new Image { Source = GetEmojiSource(emoji), Width = 30, Height = 30 };

            return uiContainer;
        }

        public static ImageSource GetEmojiSource(string emoji)
        {
            return new BitmapImage(new Uri(emoji));
        }

        public static Regex GetRegex(bool supportEmoji)
        {
            if (supportEmoji)
            {
                return HyperlinkRegex;
            }

            return HyperlinkRegex;
        }
    }
}
