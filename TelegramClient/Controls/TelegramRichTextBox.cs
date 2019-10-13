// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telegram.Api.TL;

namespace Telegram.EmojiPanel
{
    public class TelegramRichTextBox : Control
    {
        private Run _footerRun;

        public static readonly DependencyProperty FooterFontSizeProperty = DependencyProperty.Register(
            "FooterFontSize", typeof (double), typeof (TelegramRichTextBox), new PropertyMetadata(18.87, OnFooterFontSizeChanged));

        private static void OnFooterFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramRichTextBox = d as TelegramRichTextBox;
            if (telegramRichTextBox != null)
            {
                if (telegramRichTextBox._footerRun != null)
                {
                    telegramRichTextBox._footerRun.Text = (string)e.NewValue;
                }
            }
        }

        public double FooterFontSize
        {
            get { return (double) GetValue(FooterFontSizeProperty); }
            set { SetValue(FooterFontSizeProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            "Footer", typeof (string), typeof (TelegramRichTextBox), new PropertyMetadata(default(string), OnFooterChanged));

        private static void OnFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var telegramRichTextBox = d as TelegramRichTextBox;
            if (telegramRichTextBox != null)
            {
                if (telegramRichTextBox._footerRun != null)
                {
                    telegramRichTextBox._footerRun.Text = (string)e.NewValue;
                }
            }
        }

        public string Footer
        {
            get { return (string) GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty EntitiesProperty = DependencyProperty.Register(
            "Entities", typeof (IList<TLMessageEntityBase>), typeof (TelegramRichTextBox), new PropertyMetadata(default(IList<TLMessageEntityBase>)));

        public IList<TLMessageEntityBase> Entities
        {
            get { return (IList<TLMessageEntityBase>) GetValue(EntitiesProperty); }
            set { SetValue(EntitiesProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof (TextWrapping), typeof (TelegramRichTextBox), new PropertyMetadata(TextWrapping.Wrap));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping) GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof (TextTrimming), typeof (TelegramRichTextBox), new PropertyMetadata(default(TextTrimming)));

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming) GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof (TextAlignment), typeof (TelegramRichTextBox), new PropertyMetadata(TextAlignment.Left, OnTextAlignmentChanged));

        private static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TelegramRichTextBox)d;
            if (textBox != null && textBox._stackPanel != null)
            {
                foreach (var child in textBox._stackPanel.Children)
                {
                    var richTextBox = child as RichTextBox;
                    if (richTextBox != null)
                    {
                        richTextBox.TextAlignment = (TextAlignment) e.NewValue;
                    }
                }
            }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment) GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public void SetForeground(Brush foreground)
        {
            if (_stackPanel != null)
            {
                foreach (var child in _stackPanel.Children)
                {
                    var richTextBox = child as RichTextBox;
                    if (richTextBox != null)
                    {
                        richTextBox.Foreground = foreground;
                    }
                }
            }
        }

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

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TelegramRichTextBox source = (TelegramRichTextBox)d;
            string value = (string)e.NewValue;
            //if (value.Length <= 2)
            //{
            //    value += "TTT";
            //}
            //System.Diagnostics.Debug.WriteLine("oldText={0} newText={1} foreground={2} textAlignment={3}", e.OldValue, e.NewValue, ((SolidColorBrush)source.Foreground).Color, source.TextAlignment);
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

            //System.Diagnostics.Debug.WriteLine("{0} ParseText", GetHashCode());
            // Clear previous TextBlocks
            _stackPanel.Children.Clear();

            var suppressParsing = BrowserNavigationService.GetSuppressParsing(this);
            var message = DataContext as TLMessageBase ?? BrowserNavigationService.GetMessage(this);
            var decryptedMessage = DataContext as TLDecryptedMessageBase;
            var fitIn2000Pixels = CheckFitInMaxRenderHeight(value);

            if (fitIn2000Pixels)
            {
                var textBlock = GetTextBlock();
                BrowserNavigationService.SetSuppressParsing(textBlock, suppressParsing);
                BrowserNavigationService.SetMessage(textBlock, message);
                BrowserNavigationService.SetDecryptedMessage(textBlock, decryptedMessage);
                BrowserNavigationService.SetAddFooter(textBlock, true);
                BrowserNavigationService.SetText(textBlock, value);
                _stackPanel.Children.Add(textBlock);

                _footerRun = GetFooter(textBlock);
            }
            else
            {
                ParseLineExtended(value);
            }
        }

        private Run GetFooter(RichTextBox textBlock)
        {
            var run = textBlock.Tag as Run;
            if (run != null)
            {
                run.Text = Footer;
                run.FontSize = FooterFontSize;
            }

            return run;
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

            var suppressParsing = BrowserNavigationService.GetSuppressParsing(this);
            var message = DataContext as TLMessageBase;
            var decryptedMessage = DataContext as TLDecryptedMessageBase;

            var leftSide = allText.Substring(0, cutIndex + 1);
            allText = allText.Substring(cutIndex + 1);
            var isLastTextBlock = allText.Length <= 0;
            var textBlock = GetTextBlock();
            BrowserNavigationService.SetSuppressParsing(textBlock, suppressParsing);
            BrowserNavigationService.SetMessage(textBlock, message);
            BrowserNavigationService.SetDecryptedMessage(textBlock, decryptedMessage);
            if (isLastTextBlock)
            {
                BrowserNavigationService.SetAddFooter(textBlock, true);
            }
            BrowserNavigationService.SetText(textBlock, leftSide);
            _stackPanel.Children.Add(textBlock);

            _footerRun = GetFooter(textBlock);

            if (!isLastTextBlock)
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

            textBlock.TextAlignment = TextAlignment;
            textBlock.TextWrapping = TextWrapping;
            
            textBlock.IsReadOnly = true;
            textBlock.FontSize = FontSize * TextScaleFactor;
            textBlock.FontFamily = FontFamily;
            textBlock.HorizontalContentAlignment = HorizontalContentAlignment;
            textBlock.Foreground = Foreground;
            textBlock.Padding = new Thickness(0, 0, 0, 0);
            textBlock.Margin = new Thickness(0, 0, 0, 0);
            textBlock.FlowDirection = FlowDirection;

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
