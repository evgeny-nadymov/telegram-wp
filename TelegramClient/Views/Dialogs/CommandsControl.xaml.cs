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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telegram.Api.TL;
using Telegram.EmojiPanel;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Resources;

namespace TelegramClient.Views.Dialogs
{
    public partial class CommandsControl
    {
        public static readonly DependencyProperty InlineProperty = DependencyProperty.Register(
            "Inline", typeof (bool), typeof (CommandsControl), new PropertyMetadata(default(bool)));

        public bool Inline
        {
            get { return (bool) GetValue(InlineProperty); }
            set { SetValue(InlineProperty, value); }
        }

        public static readonly DependencyProperty ReplyMarkupProperty = DependencyProperty.Register(
            "ReplyMarkup", typeof (TLReplyKeyboardBase), typeof (CommandsControl), new PropertyMetadata(OnReplyMarkupChanged));

        public TLReplyKeyboardBase ReplyMarkup
        {
            get { return (TLReplyKeyboardBase)GetValue(ReplyMarkupProperty); }
            set { SetValue(ReplyMarkupProperty, value); }
        }
        private static void OnReplyMarkupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commandsControl = (CommandsControl) d;
            commandsControl.UpdateMarkup((TLReplyKeyboardBase) e.NewValue);
        }

        private void UpdateMarkup(TLReplyKeyboardBase replyKeyboardBase)
        {
            if (replyKeyboardBase == null)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

#if DEBUG
            _debug.Text = replyKeyboardBase.ToString();
#endif
            var inlineMarkup = replyKeyboardBase as TLReplyInlineMarkup;
            if (Inline && inlineMarkup == null)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            var replyMarkup = replyKeyboardBase as TLReplyKeyboardMarkup;
            if (!Inline && replyMarkup == null)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            var replyKeyboardRows = replyKeyboardBase as IReplyKeyboardRows;
            if (replyKeyboardRows == null)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            if (Inline)
            {
                var inlineButtonMargin = 3.0;
                var inlineButtonHeight = 48.0;    // without margin

                var buttonRows = new StackPanel { Margin = new Thickness(-3.0) };
                foreach (var buttonRow in replyKeyboardRows.Rows)
                {
                    var grid = new Grid();

                    for (var i = 0; i < buttonRow.Buttons.Count; i++)
                    {
                        var button = CreateButton(buttonRow.Buttons[i], inlineButtonHeight, new Thickness(inlineButtonMargin), buttonRow.Buttons.Count < 4 ? 6.0 : 0.0, GetMaxTextLength(buttonRow.Buttons.Count));
                        Grid.SetColumn(button, i);

                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.Children.Add(button);
                    }

                    buttonRows.Children.Add(grid);
                }

                Visibility = Visibility.Visible;
                LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
                LayoutRoot.Children.Clear();
                LayoutRoot.Children.Add(buttonRows);
#if DEBUG
                LayoutRoot.Children.Add(_debug);
#endif
            }
            else
            {
                var buttonRows = new StackPanel { Margin = new Thickness(3.0, 3.0, 3.0, 0.0) };
                var height = EmojiControl.PortraitOrientationHeight;
                var buttonRowsHeight = height - buttonRows.Margin.Top - buttonRows.Margin.Bottom;
                var buttonMargin = 3.0;
                var buttonHeight = 78.0;    // without margin
                if (!replyMarkup.IsResizable
                    && buttonHeight * replyMarkup.Rows.Count < buttonRowsHeight)
                {
                    buttonHeight = buttonRowsHeight / replyKeyboardRows.Rows.Count - 2 * buttonMargin;
                }

                foreach (var buttonRow in replyKeyboardRows.Rows)
                {
                    var grid = new Grid();

                    for (var i = 0; i < buttonRow.Buttons.Count; i++)
                    {
                        var button = CreateButton(buttonRow.Buttons[i], buttonHeight, new Thickness(buttonMargin), buttonRow.Buttons.Count < 4? 6.0 : 0.0, GetMaxTextLength(buttonRow.Buttons.Count));
                        Grid.SetColumn(button, i);

                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.Children.Add(button);
                    }

                    buttonRows.Children.Add(grid);
                }

                LayoutRoot.MaxHeight = height;
                if (replyMarkup.IsResizable)
                {
                    LayoutRoot.ClearValue(HeightProperty);
                }
                else
                {
                    LayoutRoot.Height = height;
                }
                Visibility = Visibility.Visible;
                var scrollViewer = new ScrollViewer();
                scrollViewer.VerticalScrollBarVisibility = buttonHeight * replyKeyboardRows.Rows.Count > buttonRowsHeight
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Disabled;

                scrollViewer.Content = buttonRows;
                LayoutRoot.Children.Clear();
                LayoutRoot.Children.Add(scrollViewer);
#if DEBUG
                LayoutRoot.Children.Add(_debug);
#endif
            }
        }

        private int GetMaxTextLength(int columns)
        {
            if (columns == 1)
            {
                return 28;
            }
            else if (columns == 2)
            {
                return 12;
            }
            else if (columns == 3)
            {
                return 7;
            }

            return 7;
        }

        private FrameworkElement CreateButton(TLKeyboardButtonBase keyboardButton, double height, Thickness margin, double padding, int maxTextLength)
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var background = isLightTheme ? (Brush)Resources["ButtonLightBackground"] : (Brush)Resources["ButtonBackground"];

            var text = keyboardButton.Text.ToString();
            var buttonBuy = keyboardButton as TLKeyboardButtonBuy;
            if (buttonBuy != null)
            {
                var message = DataContext as TLMessage;
                if (message != null)
                {
                    var mediaInvoice = message.Media as TLMessageMediaInvoice;
                    if (mediaInvoice != null)
                    {
                        var receiptMsgId = mediaInvoice.ReceiptMsgId;
                        if (receiptMsgId != null)
                        {
                            text = AppResources.Receipt;
                        }
                    }
                }
            }

            if (text.Length > maxTextLength)
            {
                text = text.Substring(0, maxTextLength) + "...";
            }
            else
            {
                text = string.Format(" {0} ", text);
            }
            var textBox = new TelegramRichTextBox { MaxHeight = height, Margin = new Thickness(0.0, 0.0, 0.0, 0.0), Padding = new Thickness(0.0, 0.0, 0.0, 0.0), FontSize = 22, TextWrapping = TextWrapping.NoWrap };
            BrowserNavigationService.SetSuppressParsing(textBox, true);
            textBox.Text = text;
            textBox.Margin = new Thickness(-12.0 + padding, 0.0, -12.0, 0 + padding);
            textBox.FontSize = Inline ? 17.776 : textBox.FontSize;
            textBox.Foreground = Inline ? new SolidColorBrush(Colors.White) : textBox.Foreground; 

            var button = new Button();
            button.Style = (Style)Resources["CommandButtonStyle"];
            button.MaxHeight = height;
            button.Margin = margin;
            button.Background = Inline ? (Brush)Resources["ButtonInlineBackground"] : background;

            button.Content = textBox;
            button.DataContext = keyboardButton;
            button.Click += OnButtonClick;

            if (keyboardButton is TLKeyboardButtonUrl)
            {
                var imageSource = isLightTheme && !Inline ? "/Images/Messages/inline.openweb.light.png" : "/Images/Messages/inline.openweb.png";

                var grid = new Grid();
                grid.Children.Add(button);
                grid.Children.Add(new Image
                {
                    Width = 11.0,
                    Height = 11.0,
                    Margin = new Thickness(8.0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Source = new BitmapImage(new Uri(imageSource, UriKind.Relative))
                });

                return grid;
            }

            if (keyboardButton is TLKeyboardButtonSwitchInline)
            {
                var imageSource = isLightTheme && !Inline ? "/Images/Messages/inline.share.light.png" : "/Images/Messages/inline.share.png";

                var grid = new Grid();
                grid.Children.Add(button);
                grid.Children.Add(new Image
                {
                    Width = 13.0,
                    Height = 12.0,
                    Margin = new Thickness(8.0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Source = new BitmapImage(new Uri(imageSource, UriKind.Relative))
                });

                return grid;
            }

            return button;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var border = sender as FrameworkElement;
            if (border != null)
            {
                var keyboardButton = border.DataContext as TLKeyboardButtonBase;
                if (keyboardButton != null)
                {
                    RaiseButtonClick(new KeyboardButtonEventArgs { Button = keyboardButton, ReplyMarkup = ReplyMarkup });
                }
            }
        }

        public event EventHandler<KeyboardButtonEventArgs> ButtonClick;

        protected virtual void RaiseButtonClick(KeyboardButtonEventArgs e)
        {
            var handler = ButtonClick;
            if (handler != null) handler(this, e);
        }

        private TextBlock _debug;

        public CommandsControl()
        {
            InitializeComponent();

#if DEBUG
            _debug = new TextBlock{ IsHitTestVisible = false };
            LayoutRoot.Children.Add(_debug);
#endif
        }
    }

    public class KeyboardButtonEventArgs : System.EventArgs
    {
        public TLKeyboardButtonBase Button { get; set; }

        public TLReplyKeyboardBase ReplyMarkup { get; set; }
    }
}
