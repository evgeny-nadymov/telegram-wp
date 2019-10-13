// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Tasks;

namespace Telegram.Controls
{
    public class ScrollableTextBlock : Control
    {
        private const int MaxSymbolsChunk = 1024;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof (string),
            typeof (ScrollableTextBlock),
            new PropertyMetadata(OnTextPropertyChanged));

        private TextBlock _measureText;

        private StackPanel _stackPanel;

        public ScrollableTextBlock()
        {
            DefaultStyleKey = typeof (ScrollableTextBlock);
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScrollableTextBlock) d).ParseText((string) e.NewValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _stackPanel = GetTemplateChild("StackPanel") as StackPanel;
            ParseText(Text);
        }

        private void ParseText(string value)
        {
            if (string.IsNullOrEmpty(value) || _stackPanel == null)
                return;
            _stackPanel.Children.Clear();
            var maxTextSize = GetMaxTextSize();
            if (value.Length < maxTextSize)
            {
                AddTextChunk(value);
            }
            else
                SplitText(value);
        }

        private static readonly Regex HyperlinkRegex = new Regex("(?i)\\b(((?:https?://|www\\d{0,3}[.]|[a-z0-9.\\-]+[.][a-z]{2,4}/)(?:[^\\s()<>]+|\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\))+(?:\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))|([a-z0-9.\\-]+(\\.ru|\\.com|\\.net|\\.org|\\.us|\\.it|\\.co\\.uk)(?![a-z0-9]))|([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]*[a-zA-Z0-9-]+))");
    

        private void AddTextChunk(string value)
        {
            var index = 0;
            foreach (Match match in HyperlinkRegex.Matches(value))
            {
                string text;
                if (match.Index != index)
                {
                    text = value.Substring(index, match.Index - index);
                    _stackPanel.Children.Add(GetTextElement(text));
                }

                var link = match.Value;
                var flag = link.EndsWith(",");
                if (flag && link.Length > 1)
                    link = link.Substring(0, link.Length - 1);
                if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    link = "http://" + link;

                text = match.Value;
                _stackPanel.Children.Add(GeHyperlinkElement(text, link));

                index = match.Index + match.Length;
            }
            if (index < value.Length)
            {
                var textElement = GetTextElement(value.Substring(index));
                _stackPanel.Children.Add(textElement);
            }
        }

        private FrameworkElement GeHyperlinkElement(string text, string uri)
        {
            var textBlock = GetTextBlock();
            textBlock.Text = text;
            textBlock.Tap += (sender, args) =>
            {
                if (textBlock.Text.Contains("@"))
                {
                    var task = new EmailComposeTask();
                    task.To = uri.ToLowerInvariant().Replace("http://", string.Empty);
                    task.Show();

                }
                else
                {
                    var task = new WebBrowserTask();
                    task.URL = HttpUtility.UrlEncode(uri); 
                    task.Show();
                }
                
            };
            textBlock.TextDecorations = TextDecorations.Underline;
            var border = GetBackgroundBorder();
            border.Child = textBlock;

            return border;

            //var hyperlink = GetHyperlinkButton();
            //var content = GetTextBlock();
            //content.Text = text;
            //hyperlink.Content = content;
            //hyperlink.NavigateUri = new Uri(uri, UriKind.Absolute);
            //var border = GetBackgroundBorder();
            //border.Child = hyperlink;

            //return border;
        }

        private FrameworkElement GetTextElement(string text)
        {
            var textBlock = GetTextBlock();
            textBlock.Text = text;
            var border = GetBackgroundBorder();
            border.Child = textBlock;

            return border;
        }

        private static readonly Dictionary<char, char> Delimiters = new Dictionary<char, char>
        {
            {' ', ' '},
            {'.', ' '},
            {',', ' '},
            {'!', ' '},
            {'?', ' '},
            {':', ' '},
            {';', ' '},
            {']', ' '},
            {')', ' '},
            {'}', ' '},
        }; 

        private void SplitText(string text)
        {
            while (true)
            {
                var symbolsCount = Math.Min(MaxSymbolsChunk, text.Length);
                
                var stepsBack = 0;
                if (symbolsCount != text.Length)
                {
                    if (!Delimiters.ContainsKey(text[symbolsCount - 1]))
                    {
                        for (var i = 1; i < 24 && (symbolsCount - 1 - i) >= 0; i++)
                        {
                            if (Delimiters.ContainsKey(text[symbolsCount - 1 - i]))
                            {
                                stepsBack += i;
                                break;
                            }
                        }
                    }
                }
                symbolsCount -= stepsBack;

                var currentChunk = text.Substring(0, symbolsCount);

                AddTextChunk(currentChunk);

                var nextChunk = text.Substring(symbolsCount, text.Length - symbolsCount);
                if (nextChunk.Length > 0)
                {
                    text = nextChunk;
                    continue;
                }
                break;
            }
        }

        private Size MeasureString(string text)
        {
            if (_measureText == null)
                _measureText = GetTextBlock();
            _measureText.Text = text;
            return new Size(_measureText.ActualWidth, _measureText.ActualHeight);
        }

        private int GetMaxTextSize()
        {
            var size = MeasureString("W");
            return (int) (Width/size.Width)*(int) (2048.0/size.Height)/2;
        }

        private TextBlock GetTextBlock()
        {
            return new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                Foreground = Foreground,
            };
        }

        private HyperlinkButton GetHyperlinkButton()
        {
            //var resources = new generic();

            //Uri resourceLocater = new Uri("/Telegram.Controls;component/Themes/generic.xaml", UriKind.Relative);
            //ResourceDictionary resourceDictionary = (ResourceDictionary)Application.LoadComponent(resourceLocater);
            //groupStyle.ContainerStyle = resourceDictionary["GroupHeaderStyle"] as Style; 

            return new HyperlinkButton
            {
                //Style = (Style) resources["HyperlinkButtonWrappingStyle"],
                ClickMode = ClickMode.Release,
                TargetName = "_blank",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0.0),
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                Foreground = Foreground,
            };
        }

        private Border GetBackgroundBorder()
        {
            return new Border
            {
                Background = Background,
                Margin = new Thickness(0.0, -2.0, 0.0, 0.0),
                Padding = new Thickness(Padding.Left, 0.0, Padding.Right, 0.0),
                //BorderBrush = new SolidColorBrush(Colors.Blue),
                //BorderThickness = new Thickness(1.0)
            };
        }
    }
}