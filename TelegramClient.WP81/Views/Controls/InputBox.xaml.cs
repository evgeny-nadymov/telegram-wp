// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Caliburn.Micro;
using Microsoft.Devices;
using Microsoft.Phone.Controls.Maps.Overlays;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient_Native;
using TelegramClient_Opus;
using Action = System.Action;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Controls
{
    public partial class InputBox
    {
        public Visibility ForwardButtonVisibility { get; set; }

        #region ReplyMarkup

        public static readonly DependencyProperty ReplyMarkupProperty = DependencyProperty.Register(
            "ReplyMarkup", typeof (TLReplyKeyboardBase), typeof (InputBox), new PropertyMetadata(default(TLReplyKeyboardBase), OnReplyMarkupChanged));

        private static void OnReplyMarkupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                //var replyKeyboardBase = e.NewValue as TLReplyKeyboardBase;
                //if (replyKeyboardBase != null)
                //{
                //    inputBox.Debug.Text = replyKeyboardBase + " " + DateTime.Now;
                //}
                //else
                //{
                //    inputBox.Debug.Text = "null";
                //}
            }
        }

        public TLReplyKeyboardBase ReplyMarkup
        {
            get { return (TLReplyKeyboardBase) GetValue(ReplyMarkupProperty); }
            set { SetValue(ReplyMarkupProperty, value); }
        }

        #endregion

        #region Reply/Forward/Edit

        private bool _isEditing;

        private bool _isForwarding;

        public event EventHandler OpenReplyButtonClick;

        protected virtual void RaiseOpenReply()
        {
            EventHandler handler = OpenReplyButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void OpenReply_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;

            SaveInputFocus();
            RaiseOpenReply();
        }

        public event EventHandler DeleteReplyButtonClick;

        protected virtual void RaiseDeleteReply()
        {
            EventHandler handler = DeleteReplyButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void DeleteReply_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;

            SaveInputFocus();
            RaiseDeleteReply();
        }

        public static readonly DependencyProperty ReplyTemplateProperty = DependencyProperty.Register(
            "ReplyTemplate", typeof (DataTemplate), typeof (InputBox), new PropertyMetadata(default(DataTemplate), OnReplyTemplateChanged));

        private static void OnReplyTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                inputBox.ReplyContentControl.ContentTemplate = e.NewValue as DataTemplate;
            }
        }

        public DataTemplate ReplyTemplate
        {
            get { return (DataTemplate) GetValue(ReplyTemplateProperty); }
            set { SetValue(ReplyTemplateProperty, value); }
        }

        public static readonly DependencyProperty ReplyProperty = DependencyProperty.Register(
            "Reply", typeof (object), typeof (InputBox), new PropertyMetadata(default(object), OnReplyChanged));

        private static void OnReplyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                inputBox.ReplyGrid.Visibility = e.NewValue != null && !inputBox.IsAppBarCommandVisible
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                inputBox.ReplyContentControl.Content = e.NewValue;

                inputBox._isEditing = false;
                inputBox._isForwarding = false;
                var messagesContainer = e.NewValue as TLMessagesContainter;
                if (messagesContainer != null)
                {
                    // forward
                    if (messagesContainer.FwdMessages != null)
                    {
                        inputBox._isForwarding = true;
                        inputBox.SendButtonImage.Source = inputBox.SendSource;
                        inputBox.SendButton.Visibility = System.Windows.Visibility.Visible;
                        inputBox.RecordButton.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    // edit
                    else if (messagesContainer.EditMessage != null)
                    {
                        inputBox._isEditing = true;
                        inputBox.AdditionalButtons.Visibility = Visibility.Collapsed;
                        inputBox.SendButtonImage.Source = inputBox.DoneSource;
                        inputBox.SendButton.Visibility = System.Windows.Visibility.Visible;
                        inputBox.RecordButton.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                    // reply
                else if (e.NewValue is TLMessageBase)
                {
                    //inputBox.AdditionalButtons.Visibility = Visibility.Collapsed;
                    //inputBox.SendButton.Source = inputBox.SendSource;
                }
                else
                {
                    inputBox.AdditionalButtons.Visibility = Visibility.Visible;
                    var textExists = !string.IsNullOrEmpty(inputBox.Input.Text);
                    inputBox.SetSendButtonSource(textExists);
                }
            }
        }

        public object Reply
        {
            get { return GetValue(ReplyProperty); }
            set { SetValue(ReplyProperty, value); }
        }

        #endregion


        #region ApplicationBar

        public event EventHandler AppBarCommandClick;

        protected virtual void RaiseAppBarCommandClick()
        {
            EventHandler handler = AppBarCommandClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private Button _appBarButton;

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof (string), typeof (InputBox), new PropertyMetadata(default(string), OnCommandChanged));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                if (inputBox._appBarButton == null)
                {
                    inputBox._appBarButton = new Button { Style = (Style) inputBox.Resources["ButtonStyle1"] };
                    inputBox._appBarButton.Click += (sender, args) => inputBox.RaiseAppBarCommandClick();
                    inputBox.AppBarCommandPlaceholder.Child = inputBox._appBarButton;

                    var scaledText = (ScaledText)Application.Current.Resources["ScaledText"];
                    var fontSize = scaledText.DefaultFontSize;

                    inputBox._appBarButton.FontSize = fontSize;
                }


                inputBox._appBarButton.Content = e.NewValue;
            }
        }

        public string Command
        {
            get { return (string) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty IsAppBarCommandVisibleProperty = DependencyProperty.Register(
            "IsAppBarCommandVisible", typeof (bool), typeof (InputBox), new PropertyMetadata(default(bool), OnIsAppBarCommandVisibleChanged));

        private static void OnIsAppBarCommandVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                var isAppBarCommandVisible = (bool) e.NewValue;

                //if (isAppBarCommandVisible)
                //{
                //    //var storyboard = inputBox.GetOpenStoryboard(inputBox.AppBarCommandTransform);
                //    //var storyboard2 = inputBox.GetCloseStoryboard(inputBox.InputBoxTranslate);
                //    //storyboard.Begin();
                //    //storyboard2.Begin();
                //}
                //else
                //{
                //    //var storyboard = inputBox.GetCloseStoryboard(inputBox.AppBarCommandTransform);
                //    //var storyboard2 = inputBox.GetOpenStoryboard(inputBox.InputBoxTranslate);
                //    //storyboard.Begin();
                //    //storyboard2.Begin();
                //}

                inputBox.ReplyGrid.Visibility = !isAppBarCommandVisible && inputBox.Reply != null ? Visibility.Visible : Visibility.Collapsed;
                inputBox.InputGrid.Visibility = isAppBarCommandVisible ? Visibility.Collapsed : Visibility.Visible;
                inputBox.AppBarCommandPlaceholder.Visibility = isAppBarCommandVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsAppBarCommandVisible
        {
            get { return (bool) GetValue(IsAppBarCommandVisibleProperty); }
            set { SetValue(IsAppBarCommandVisibleProperty, value); }
        }
        #endregion

        private BitmapSource _doneSource;

        private BitmapSource DoneSource
        {
            get
            {
                _doneSource = _doneSource ?? new BitmapImage(new Uri("/Images/W10M/ic_done_2x.png", UriKind.Relative));

                return _doneSource;
            }
        }

        private BitmapSource _emojiSource;

        private BitmapSource EmojiSource
        {
            get
            {
                _emojiSource = _emojiSource ?? new BitmapImage(new Uri("/Images/W10M/ic_emoji_2x.png", UriKind.Relative));

                return _emojiSource;
            }
        }

        private BitmapSource _keyboardSource;

        private BitmapSource KeyboardSource
        {
            get
            {
                _keyboardSource = _keyboardSource ?? new BitmapImage(new Uri("/Images/W10M/ic_keyboard_2x.png", UriKind.Relative));

                return _keyboardSource;
            }
        }

        private BitmapSource _sendSource;

        private BitmapSource SendSource
        {
            get
            {
                _sendSource = _sendSource ?? new BitmapImage(new Uri("/Images/W10M/ic_send_2x.png", UriKind.Relative));

                return _sendSource;
            }
        }

        private BitmapSource _microphoneSource;

        private BitmapSource MicrophoneSource
        {
            get
            {
                _microphoneSource = _microphoneSource ?? new BitmapImage(new Uri("/Images/W10M/ic_microphone_2x.png", UriKind.Relative));

                return _microphoneSource;
            }
        }

        public event RoutedEventHandler InputLostFocus;

        protected virtual void RaiseInputLostFocus(RoutedEventArgs e)
        {
            RoutedEventHandler handler = InputLostFocus;
            if (handler != null) handler(this, e);
        }

        public event RoutedEventHandler InputGotFocus;

        protected virtual void RaiseInputGotFocus(RoutedEventArgs args)
        {
            RoutedEventHandler handler = InputGotFocus;
            if (handler != null) handler(this, args);
        }

        public event KeyEventHandler InputKeyDown;

        protected virtual void RaiseInputKeyDown(KeyEventArgs e)
        {
            KeyEventHandler handler = InputKeyDown;
            if (handler != null) handler(this, e);
        }

        public event TextChangedEventHandler InputTextChanged;

        protected virtual void RaiseInputTextChanged(TextChangedEventArgs e)
        {
            TextChangedEventHandler handler = InputTextChanged;
            if (handler != null) handler(this, e);
        }

        public ContentControl EmojiPlaceholder { get { return KeyboardPlaceholder; } }

        public TextBox InnerTextBox { get { return Input; } }

        public int SelectionStart
        {
            get { return Input.SelectionStart; }
            set { Input.SelectionStart = value; }
        }

        public int SelectionLength
        {
            get { return Input.SelectionLength; }
            set { Input.SelectionLength = value; }
        }

        public Visibility MuteButtonVisibility
        {
            get { return MuteButtonBorder.Visibility; }
            set { MuteButtonBorder.Visibility = value; }
        }

        public Visibility KeyboardButtonVisibility
        {
            get { return KeyboardButtonBorder.Visibility; }
            set { KeyboardButtonBorder.Visibility = value; }
        }

        public static readonly DependencyProperty KeyboardButtonImageStringProperty = DependencyProperty.Register(
            "KeyboardButtonImageString", typeof (string), typeof (InputBox), new PropertyMetadata(default(string), OnKeyboardButtonImageStringChanged));

        private static void OnKeyboardButtonImageStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                var str = e.NewValue as string;
                inputBox.KeyboardButton.Source = str != null ? new BitmapImage(new Uri(str, UriKind.Relative)) : null;
            }
        }

        public string KeyboardButtonImageString
        {
            get { return (string) GetValue(KeyboardButtonImageStringProperty); }
            set { SetValue(KeyboardButtonImageStringProperty, value); }
        }

        public static readonly DependencyProperty MuteButtonImageSourceProperty = DependencyProperty.Register(
            "MuteButtonImageSource", typeof (ImageSource), typeof (InputBox), new PropertyMetadata(default(ImageSource), OnMuteButtonImageSourceChanged));

        private static void OnMuteButtonImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                inputBox.MuteButton.Source = (ImageSource) e.NewValue;
            }
        }

        public ImageSource MuteButtonImageSource
        {
            get { return (ImageSource) GetValue(MuteButtonImageSourceProperty); }
            set { SetValue(MuteButtonImageSourceProperty, value); }
        }

        public static readonly DependencyProperty TextScaleFactorProperty = DependencyProperty.Register(
            "TextScaleFactor", typeof(double), typeof(InputBox), new PropertyMetadata(default(double), OnTextScaleFactorChanged));

        private static void OnTextScaleFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                inputBox.Input.TextScaleFactor = (double)e.NewValue;
            }
        }

        public double TextScaleFactor
        {
            get { return (double)GetValue(TextScaleFactorProperty); }
            set { SetValue(TextScaleFactorProperty, value); }
        }

        public static readonly DependencyProperty InlineWatermarkProperty = DependencyProperty.Register(
            "InlineWatermark", typeof(string), typeof(InputBox), new PropertyMetadata(default(string), OnInlineWatermarkChanged));

        private static void OnInlineWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null)
            {
                inputBox.Input.InlineWatermark = e.NewValue as string;
            }
        }

        public string InlineWatermark
        {
            get { return (string)GetValue(InlineWatermarkProperty); }
            set { SetValue(InlineWatermarkProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(InputBox), new PropertyMetadata("", OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as InputBox;
            if (inputBox != null && e.NewValue != null)
            {
                inputBox.Input.Text = e.NewValue as string;
            }
        }

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty) ?? "";
            }
            set
            {
                var text = value ?? "";
                SetValue(TextProperty, text);
            }
        }

        public event EventHandler SendButtonClick;

        protected virtual void RaiseSendButtonClick()
        {
            var handler = SendButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler AttachButtonClick;

        protected virtual void RaiseAttachButtonClick()
        {
            var handler = AttachButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler MuteButtonClick;

        protected virtual void RaiseMuteButtonClick()
        {
            var handler = MuteButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler KeyboardButtonClick;

        protected virtual void RaiseKeyboardButtonClick()
        {
            var handler = KeyboardButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler EmojiButtonClick;

        protected virtual void RaiseEmojiButtonClick()
        {
            var handler = EmojiButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private Control _gotFocusControl;

        private bool _isFocused;

        private double _applicationBarDefaultSize = 72.0;

        private double _iconMargin = 20.0;

        private double _iconSize = 32.0;

        private double _iconLabelFontSize = 18.0;

        public InputBox()
        {
            InitializeComponent();

            if (DesignerProperties.IsInDesignTool) return;

            var applicationBar = new ApplicationBar();
            _applicationBarDefaultSize = applicationBar.DefaultSize;

            KeyboardHeight = EmojiControl.PortraitOrientationHeight;

            KeyboardPlaceholder.Visibility = Visibility.Collapsed;
            KeyboardPlaceholder.Height = KeyboardHeight;
            //var control = KeyboardPlaceholder.Content as FrameworkElement;
            //if (control != null)
            //{
            //    control.Height = KeyboardHeight;
            //}

            if (_applicationBarDefaultSize < 72.0)
            {
                var scaleFactor = _applicationBarDefaultSize / 72.0;
                _iconMargin = 20.0 * scaleFactor;
                _iconSize = _applicationBarDefaultSize - 2.0 * _iconMargin;
                _iconLabelFontSize = 18.0 * scaleFactor;
                RecordButtonIcon.Margin = new Thickness(20.0, _iconMargin, 20.0, _iconMargin);
                RecordButtonIcon.Width = _iconSize;
                RecordButtonIcon.Height = _iconSize;
                SendButtonImage.Margin = new Thickness(20.0, _iconMargin, 20.0, _iconMargin);
                SendButtonImage.Width = _iconSize;
                SendButtonImage.Height = _iconSize;
                KeyboardButton.Margin = new Thickness(20.0, _iconMargin, 20.0, _iconMargin);
                KeyboardButton.Width = _iconSize;
                KeyboardButton.Height = _iconSize;
                MuteButton.Margin = new Thickness(20.0, _iconMargin, 20.0, _iconMargin);
                MuteButton.Width = _iconSize;
                MuteButton.Height = _iconSize;
                AttachButton.Margin = new Thickness(20.0, _iconMargin, 20.0, _iconMargin);
                AttachButton.Width = _iconSize;
                AttachButton.Height = _iconSize;
                EmojiButtonImage.Margin = new Thickness(20.0, _iconMargin, 20.0, _iconMargin);
                EmojiButtonImage.Width = _iconSize;
                EmojiButtonImage.Height = _iconSize;
            }
            HintGrid.Height = _applicationBarDefaultSize;

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                TopBorder.Fill = (Brush)Resources["InputBorderBrushLight"];
            }
            else
            {
                TopBorder.Fill = (Brush)Resources["InputBorderBrushDark"];
                Input.CaretBrush = new SolidColorBrush(Colors.White);
            }

            var keyPressEvents = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                keh => { Input.TextChanged += keh; },
                keh => { Input.TextChanged -= keh; });

            _keyPressSubscription = keyPressEvents
                .Throttle(TimeSpan.FromSeconds(0.15))
                .ObserveOnDispatcher()
                .Subscribe(e => UpdateEmojiHints());

            Loaded += OnLoaded;
            Loaded += OnLoadedOnce;
            Unloaded += OnUnloaded;

            GotFocus += (sender, args) =>
            {
                _isFocused = true;
                _gotFocusControl = args.OriginalSource as Control;
            };
            LostFocus += (sender, args) =>
            {
                _isFocused = false;
            };
        }
#if DEBUG
        ~InputBox()
        {
            
        }
#endif

        private void OnLoadedOnce(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoadedOnce;

            Text = Input.Text;
            var textExists = !string.IsNullOrEmpty(Input.Text);

            Scroll.VerticalScrollBarVisibility = textExists ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            AttachButtonBorder.Visibility = textExists ? Visibility.Collapsed : Visibility.Visible;
            SetSendButtonSource(textExists);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_microphone != null) _microphone.BufferReady += Microphone_OnBufferReady;

            InputPane.GetForCurrentView().Showing += OnInputPaneShowing;
            InputPane.GetForCurrentView().Hiding += OnInputPaneHiding;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_microphone != null) _microphone.BufferReady -= Microphone_OnBufferReady;

            InputPane.GetForCurrentView().Showing -= OnInputPaneShowing;
            InputPane.GetForCurrentView().Hiding -= OnInputPaneHiding;
        }

        private void Input_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > e.PreviousSize.Height)
            {
                Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + e.NewSize.Height - e.PreviousSize.Height);
            }
        }

        private void Input_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressNotifyTextChanged)
            {
                _suppressNotifyTextChanged = false;
                return;
            }

            //RaiseHideEmojiHints();
            //UpdateEmojiHints();

            Text = Input.Text;
            var textExists = !string.IsNullOrEmpty(Input.Text);

            Scroll.VerticalScrollBarVisibility = textExists ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            AttachButtonBorder.Visibility = textExists ? Visibility.Collapsed : Visibility.Visible;
            SetSendButtonSource(textExists);

            RaiseInputTextChanged(e);
        }

        private EmojiSuggestionParams _parameters;

        public event EventHandler<EmojiHintsEventArgs> ShowEmojiHints;

        protected virtual void RaiseShowEmojiHints(EmojiHintsEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ShowEmojiHints selectionStart={0} text={1} parameters=[{2}]", Input.SelectionStart, Input.Text, e.Parameters);

            _parameters = e.Parameters;

            var handler = ShowEmojiHints;
            if (handler != null) handler(this, e);
        }

        public event EventHandler HideEmojiHints;

        protected virtual void RaiseHideEmojiHints()
        {
            //System.Diagnostics.Debug.WriteLine("HideEmojiHints selectionStart={0} text={1} parameters=[{2}]", Input.SelectionStart, Input.Text, _parameters);
            _parameters = null;

            var handler = HideEmojiHints;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void UpdateEmojiHints()
        {
            //RaiseHideEmojiHints();

            var index = SuggestionIndex(Input.Text, Input.SelectionStart, EmojiSuggestion.GetSuggestionMaxLength());
            if (index != -1)
            {
                var p = new EmojiSuggestionParams
                {
                    Text = Input.Text,
                    SelectionStart = Input.SelectionStart,
                    Index = index,
                    Length = Input.SelectionStart - index
                };

                var query = p.Text.Substring(p.Index + 1, p.Length - 1);
                if (!string.IsNullOrEmpty(query))
                {
                    Execute.BeginOnThreadPool(() =>
                    {
                        var stopwatch = Stopwatch.StartNew();
                        p.Results = EmojiSuggestion.GetSuggestions(query);
                        var elapsed = stopwatch.Elapsed;

                        Execute.BeginOnUIThread(() =>
                        {
                            if (p.Results != null
                                && string.Equals(Input.Text, p.Text, StringComparison.Ordinal)
                                && p.SelectionStart == Input.SelectionStart)
                            {
                                RaiseShowEmojiHints(new EmojiHintsEventArgs(p));
                            }
                            else
                            {
                                RaiseHideEmojiHints();
                            }
                        });
                    });
                }
                else
                {
                    RaiseHideEmojiHints();
                }
            }
            else
            {
                RaiseHideEmojiHints();
            }
        }

        private static bool IsSuggestionChar(char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || (ch == '_') || (ch == '-') || (ch == '+');
        }

        private int SuggestionIndex(string str, int start, int maxLength)
        {
            var index = start - 1;
            while (index >= 0 && ((start - maxLength) <= index))
            {
                if (str[index] == ':')
                {
                    if (index != start - 1
                        && (index == 0 || !IsSuggestionChar(str[index - 1])))
                    {
                        return index;
                    }
                }
                else if (!IsSuggestionChar(str[index]))
                {
                    return -1;
                }
                index--;
            }

            return -1;
        }

        private void Input_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_parameters != null
                && string.Equals(_parameters.Text, Input.Text, StringComparison.Ordinal)
                && _parameters.SelectionStart != Input.SelectionStart)
            {
                RaiseHideEmojiHints();
            }
        }

        private void SetSendButtonSource(bool textExists)
        {
            if (_isEditing)
            {
                SendButtonImage.Source = DoneSource;
            }
            else if (_isForwarding)
            {
                SendButtonImage.Source = SendSource;
            }
            else
            {
                SendButtonImage.Source = SendSource;
                if (textExists)
                {
                    SendButton.Visibility = Visibility.Visible;
                    RecordButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SendButton.Visibility = Visibility.Collapsed;
                    RecordButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void SaveInputFocus()
        {
            if (_isFocused
                && _gotFocusControl == Input)
            {
                Input.Focus();
            }
        }

        private void SendButton_OnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            SaveInputFocus();

            _suppressNotifyTextChanged = true;
            Input.Text = string.Empty;
            ReplyGrid.Visibility = Visibility.Collapsed;
            Scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            AttachButtonBorder.Visibility = Visibility.Visible;
            AdditionalButtons.Visibility = Visibility.Visible;
            _isEditing = false;
            _isForwarding = false;
            SetSendButtonSource(false);

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                RaiseSendButtonClick();
            });
        }

        private void AttachButton_OnTap(object sender, GestureEventArgs e)
        {
            RaiseAttachButtonClick();
        }

        private void KeyboardButton_OnTap(object sender, GestureEventArgs e)
        {
            RaiseKeyboardButtonClick();
        }

        private void MuteButton_OnTap(object sender, GestureEventArgs e)
        {
            SaveInputFocus();
            RaiseMuteButtonClick();
        }

        private bool _suppressHiding;
        private bool _suppressNotifyTextChanged;

        private void EmojiButton_OnTap(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            RaiseEmojiButtonClick();
        }

        private void OnInputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (KeyboardPlaceholder.Opacity == 1.0) return;

            KeyboardPlaceholder.Visibility = Visibility.Collapsed;
        }

        public static double KeyboardHeight { get; protected set; }

        private void OnInputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (!_isFocused || _gotFocusControl != Input) return;

            KeyboardHeight = 480.0 / args.OccludedRect.Width * args.OccludedRect.Height;

            KeyboardPlaceholder.Visibility = Visibility.Visible;
            KeyboardPlaceholder.Height = KeyboardHeight;
            //var control = KeyboardPlaceholder.Content as FrameworkElement;
            //if (control != null)
            //{
            //    control.Height = KeyboardHeight;
            //}
        }

        private void Input_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter
                && System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
            {
                RaiseSendButtonClick();
            }

            RaiseInputKeyDown(e);
        }

        private void Input_OnGotFocus(object sender, RoutedEventArgs e)
        {
            KeyboardPlaceholder.Visibility = Visibility.Visible;
            KeyboardPlaceholder.Opacity = 0.0;
            EmojiButtonImage.Source = EmojiSource;

            RaiseInputGotFocus(e);
        }

        private void Input_OnLostFocus(object sender, RoutedEventArgs e)
        {
            RaiseInputLostFocus(e);
        }

        public void OpenPlaceholder()
        {
            KeyboardPlaceholder.Visibility = Visibility.Visible;
            KeyboardPlaceholder.Opacity = 1.0;
            EmojiButtonImage.Source = KeyboardSource;
        }

        public void ClosePlaceholder()
        {
            KeyboardPlaceholder.Visibility = Visibility.Collapsed;
            EmojiButtonImage.Source = EmojiSource;

            if (IsFullScreen)
            {
                CloseFullScreen();
            }
        }

        public void FocusInput()
        {
            Input.Focus();
        }

        public void SwitchToSelectionMode()
        {
            if (InputBoxTranslate.Y == 72.0) return;

            if (_applicationBarBorder == null)
            {
                _applicationBarBorder = CreateApplicationBar();

                CommonGrid.Children.Insert(0, _applicationBarBorder);
            }

            _deleteButton.IsHitTestVisible = false;
            _deleteButton.Opacity = 0.5;

            _forwardButton.IsHitTestVisible = false;
            _forwardButton.Opacity = 0.5;
            _forwardButton.Visibility = ForwardButtonVisibility;

            _appBarOpened = false;
            _cancelLabel.Visibility = Visibility.Collapsed;
            _deleteLabel.Visibility = Visibility.Collapsed;
            _forwardLabel.Visibility = Visibility.Collapsed;

            var storyboard = GetCloseStoryboard(InputBoxTranslate, BackgroundBorder, _applicationBarBorder);
            storyboard.Begin();
            //LayoutRoot.Visibility = Visibility.Collapsed;
            //BackgroundBorder.Background = new SolidColorBrush(Colors.Transparent);
        }

        public event EventHandler CancelSelectionButtonClick;

        protected virtual void RaiseCancelSelectionButtonClick()
        {
            EventHandler handler = CancelSelectionButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler ForwardButtonClick;

        protected virtual void RaiseForwardButtonClick()
        {
            EventHandler handler = ForwardButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler DeleteButtonClick;

        protected virtual void RaiseDeleteButtonClick()
        {
            EventHandler handler = DeleteButtonClick;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private Border CreateApplicationBar()
        {
            var border = new Border();
            border.Height = 18.0 + _applicationBarDefaultSize;
            border.Margin = new Thickness(0.0, 0.0, 0.0, -18.0);
            border.VerticalAlignment = VerticalAlignment.Bottom;
            border.RenderTransform = new TranslateTransform{ Y = 0.0 };

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            border.Background = isLightTheme ? (SolidColorBrush)Resources["AppBarPanelLight"] : (SolidColorBrush)Resources["AppBarPanelDark"];

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72.0) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72.0) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72.0) });

            border.Child = grid;

            var moreButton = CreateButton("/Images/W10M/ic_more_2x.png", null, null).Item1;
            moreButton.Tap += (sender, args) =>
            {
                if (_appBarOpened)
                {
                    CloseMorePanel();
                }
                else
                {
                    OpenMorePanel();
                }

                _appBarOpened = !_appBarOpened;
            };
            Grid.SetColumn(moreButton, 4);
            grid.Children.Add(moreButton);

            var cancelButtonTuple = CreateButton("/Images/W10M/ic_cancel_2x.png", AppResources.Cancel, RaiseCancelSelectionButtonClick);
            _cancelLabel = cancelButtonTuple.Item2;
            Grid.SetColumn(cancelButtonTuple.Item1, 3);
            grid.Children.Add(cancelButtonTuple.Item1);

            var deleteButtonTuple = CreateButton("/Images/W10M/ic_delete_2x.png", AppResources.Delete, RaiseDeleteButtonClick);
            _deleteButton = deleteButtonTuple.Item1;
            _deleteLabel = deleteButtonTuple.Item2;
            Grid.SetColumn(_deleteButton, 2);
            grid.Children.Add(_deleteButton);

            var forwardButtonTuple = CreateButton("/Images/W10M/ic_share_2x.png", AppResources.Forward, RaiseForwardButtonClick);
            _forwardButton = forwardButtonTuple.Item1;
            _forwardLabel = forwardButtonTuple.Item2;
            Grid.SetColumn(_forwardButton, 1);
            grid.Children.Add(_forwardButton);

            return border;
        }

        private Tuple<FrameworkElement, TextBlock> CreateButton(string iconPath, string title, Action callback)
        {
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Colors.Transparent);

            var border = new Border();
            border.VerticalAlignment = VerticalAlignment.Top;
            border.Background = (Brush) Application.Current.Resources["PhoneForegroundBrush"];
            border.Margin = new Thickness(20.0, _iconMargin, 20.0, _iconMargin);
            border.Width = _iconSize;
            border.Height = _iconSize;
            var brush = new ImageBrush { ImageSource = new BitmapImage(new Uri(iconPath, UriKind.Relative)) };
            border.OpacityMask = brush;
            grid.Children.Add(border);

            TextBlock label = null;
            if (!string.IsNullOrEmpty(title))
            {
                label = new TextBlock();
                label.Text = title;
                label.VerticalAlignment = VerticalAlignment.Bottom;
                label.Visibility = Visibility.Collapsed;
                label.Margin = new Thickness(-12.0, 0.0, -12.0, 9.0);
                label.MaxWidth = 96.0;
                label.TextWrapping = TextWrapping.NoWrap;
                label.TextTrimming = TextTrimming.WordEllipsis;
                label.TextAlignment = TextAlignment.Center;
                label.FontSize = _iconLabelFontSize;
                label.IsHitTestVisible = false;

                grid.Children.Add(label);
            }

            border.Tap += (sender, args) => callback.SafeInvoke();

            return new Tuple<FrameworkElement, TextBlock>(grid, label);
        }

        public void SwitchToNormalMode()
        {
            if (InputBoxTranslate.Y == 0.0) return;

            var storyboard = GetOpenStoryboard(InputBoxTranslate, BackgroundBorder, _applicationBarBorder);
            storyboard.Begin();
            //LayoutRoot.Visibility = Visibility.Visible;
            //BackgroundBorder.Background = (Brush) Application.Current.Resources["PhoneBackgroundBrush"];
        }

        private static Storyboard GetOpenStoryboard(TranslateTransform transform, UIElement element, UIElement element2)
        {
            var storyboard = new Storyboard();

            var transformAnimaion = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromSeconds(0.2), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 } };
            Storyboard.SetTarget(transformAnimaion, transform);
            Storyboard.SetTargetProperty(transformAnimaion, new PropertyPath("Y"));
            storyboard.Children.Add(transformAnimaion);

            var opacityAnimaion = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion, element);
            Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion);

            var visibilityAnimation2 = new ObjectAnimationUsingKeyFrames();
            visibilityAnimation2.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = Visibility.Collapsed });
            Storyboard.SetTarget(visibilityAnimation2, element2);
            Storyboard.SetTargetProperty(visibilityAnimation2, new PropertyPath("Visibility"));
            storyboard.Children.Add(visibilityAnimation2);

            var transformAnimaion2 = new DoubleAnimationUsingKeyFrames();
            transformAnimaion2.KeyFrames.Add(new DiscreteDoubleKeyFrame{KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0});
            Storyboard.SetTarget(transformAnimaion2, element2.RenderTransform);
            Storyboard.SetTargetProperty(transformAnimaion2, new PropertyPath("Y"));
            storyboard.Children.Add(transformAnimaion2);

            var visibilityAnimation = new ObjectAnimationUsingKeyFrames();
            visibilityAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = Visibility.Visible });
            Storyboard.SetTarget(visibilityAnimation, element);
            Storyboard.SetTargetProperty(visibilityAnimation, new PropertyPath("Visibility"));
            storyboard.Children.Add(visibilityAnimation);

            return storyboard;
        }

        private static Storyboard GetCloseStoryboard(TranslateTransform transform, UIElement element, UIElement element2)
        {
            var storyboard = new Storyboard();
            
            var transformAnimaion = new DoubleAnimation { To = 72.0, Duration = TimeSpan.FromSeconds(0.2), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } };
            Storyboard.SetTarget(transformAnimaion, transform);
            Storyboard.SetTargetProperty(transformAnimaion, new PropertyPath("Y"));
            storyboard.Children.Add(transformAnimaion);

            var opacityAnimaion = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromSeconds(0.2) };
            Storyboard.SetTarget(opacityAnimaion, element);
            Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimaion);

            var visibilityAnimation2 = new ObjectAnimationUsingKeyFrames();
            visibilityAnimation2.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = Visibility.Visible });
            Storyboard.SetTarget(visibilityAnimation2, element2);
            Storyboard.SetTargetProperty(visibilityAnimation2, new PropertyPath("Visibility"));
            storyboard.Children.Add(visibilityAnimation2);

            var visibilityAnimation = new ObjectAnimationUsingKeyFrames();
            visibilityAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = Visibility.Collapsed });
            Storyboard.SetTarget(visibilityAnimation, element);
            Storyboard.SetTargetProperty(visibilityAnimation, new PropertyPath("Visibility"));
            storyboard.Children.Add(visibilityAnimation);

            return storyboard;
        }

        #region ApplicationBar

        private Border _applicationBarBorder;

        private FrameworkElement _deleteButton;

        private FrameworkElement _forwardButton;

        private TextBlock _deleteLabel;

        private TextBlock _forwardLabel;

        private TextBlock _cancelLabel;

        public Tuple<bool, bool> IsGroupActionEnabled
        {
            set
            {
                if (_deleteButton != null)
                {
                    _deleteButton.Opacity = value.Item1 ? 1.0 : 0.5;
                    _deleteButton.IsHitTestVisible = value.Item1;
                }

                if (_forwardButton != null)
                {
                    _forwardButton.Opacity = value.Item2 ? 1.0 : 0.5;
                    _forwardButton.IsHitTestVisible = value.Item2;
                }
            }
        }

        public bool IsDeleteActionVisible
        {
            set
            {
                if (_deleteButton != null)
                {
                    _deleteButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }

                if (_forwardButton != null)
                {
                    Grid.SetColumn(_forwardButton, value ? 1 : 2);
                }
            }
        }

        private bool _appBarOpened;

        private void OpenMorePanel()
        {
            var storyboard = new Storyboard();

            var translateAppBarPanelAnimation = new DoubleAnimationUsingKeyFrames();
            translateAppBarPanelAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = -16.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
            Storyboard.SetTarget(translateAppBarPanelAnimation, _applicationBarBorder);
            Storyboard.SetTargetProperty(translateAppBarPanelAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateAppBarPanelAnimation);

            _cancelLabel.Visibility = Visibility.Visible;
            _deleteLabel.Visibility = Visibility.Visible;
            _forwardLabel.Visibility = Visibility.Visible;

            storyboard.Begin();
        }

        private void CloseMorePanel()
        {
            var storyboard = new Storyboard();

            var translateAppBarPanelAnimation = new DoubleAnimationUsingKeyFrames();
            translateAppBarPanelAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            Storyboard.SetTarget(translateAppBarPanelAnimation, _applicationBarBorder);
            Storyboard.SetTargetProperty(translateAppBarPanelAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateAppBarPanelAnimation);

            storyboard.Begin();
            storyboard.Completed += (sender, args) =>
            {
                _cancelLabel.Visibility = Visibility.Collapsed;
                _deleteLabel.Visibility = Visibility.Collapsed;
                _forwardLabel.Visibility = Visibility.Collapsed;
            };
        }
        #endregion

        #region Recording

        public bool UploadFileDuringRecording { get; set; }

        private bool _isLogEnabled = false;

        private void Log(string str)
        {
            if (!_isLogEnabled) return;

            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("  HH:mm:ss.fff ") + str);
        }

        private DateTime? _lastUpdateTime;
        private DateTime? _lastTypingTime;
        private bool _isHintStoryboardPlaying;
        private bool _isSliding;
        private Microphone _microphone;
        private byte[] _buffer;
        private TimeSpan _duration;
        private TimeSpan _recordedDuration;
        private DateTime _startTime;
        private volatile bool _stopRequested;
        private volatile bool _cancelRequested;

        private MemoryStream _stream;
        private XnaAsyncDispatcher _asyncDispatcher;
        private string _fileName = "audio.mp3";

        private WindowsPhoneRuntimeComponent _component;

        protected WindowsPhoneRuntimeComponent Component
        {
            get
            {
                if (DesignerProperties.IsInDesignTool) return null;

                _component = _component ?? new WindowsPhoneRuntimeComponent();

                return _component;
            }
        }

        private long _uploadingLength;
        private volatile bool _isPartReady;
        private int _skipBuffersCount;
        private TLLong _fileId;
        private readonly List<UploadablePart> _uploadableParts = new List<UploadablePart>();

        private TextBlock Duration;

        private Grid SliderPanel;

        private StackPanel Slider;

        private Grid TimerPanel;

        private Grid CreateTimerPanel()
        {
            var timerPanel = new Grid { Margin = new Thickness(0.0, 2.0, 0.0, 0.0), Visibility = Visibility.Collapsed, Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"], MinWidth = 120.0, IsHitTestVisible = false };
            timerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            timerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            timerPanel.RenderTransform = new TranslateTransform();

            var scaledText = (ScaledText) Application.Current.Resources["ScaledText"];
            var fontSize = scaledText.DefaultFontSize;

            Duration = new TextBlock
            {
                CacheMode = new BitmapCache(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12.0, 0.0, 12.0, 3.0),
                Text = "0:00,0",
                FontSize = fontSize
            };
            Grid.SetColumn(Duration, 1);
            var border = new Ellipse
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20.0, 0.0, 0.0, 2.0),
                Width = 10.0,
                Height = 10.0,
                Fill = new SolidColorBrush(Color.FromArgb(255, 255, 83, 83))
            };
            Grid.SetColumn(border, 0);

            _recordingStoryboard = GetRecordingStoryboard(border);

            timerPanel.Children.Add(Duration);
            timerPanel.Children.Add(border);

            Grid.SetColumn(timerPanel, 0);

            return timerPanel;
        }

        private TranslateTransform _sliderTextTransform;

        private CompositeTransform _sliderPathTransform;

        private Grid CreateSliderPanel()
        {
            Brush fill;
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                fill = (Brush)Resources["InputForegroundBrushLight"];
            }
            else
            {
                fill = (Brush)Resources["InputForegroundBrushDark"];
            }

            var clipGrid = new Grid { Visibility = Visibility.Collapsed, IsHitTestVisible = false };
            Grid.SetColumn(clipGrid, 0);
            Grid.SetColumnSpan(clipGrid, 5);

            Slider = new StackPanel
            {
                CacheMode = new BitmapCache(),
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(6.0, 0.0, 0.0, 0.0),
                RenderTransform = new TranslateTransform()
            };

            var scaledText = (ScaledText)Application.Current.Resources["ScaledText"];
            var fontSize = scaledText.DefaultFontSize;

            _sliderTextTransform = new TranslateTransform();
            var textBlock = new TextBlock
            {
                Foreground = fill,
                VerticalAlignment = VerticalAlignment.Center,
                Text = AppResources.SlideToCancel,
                FontSize = fontSize,
                Margin = new Thickness(4.0, 0.0, 0.0, 3.0),
                RenderTransform = _sliderTextTransform,
            };

            _sliderPathTransform = new CompositeTransform{ Rotation = 45.0 };
            var path = new System.Windows.Shapes.Path
            {
                Stroke = fill,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(72.0, 0.0, 0.0, 0.0),
                StrokeThickness = 2.0,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _sliderPathTransform
            };
            var dataBinding = new System.Windows.Data.Binding { Source = "M 0,0 0,16 16,16" };
            path.SetBinding(System.Windows.Shapes.Path.DataProperty, dataBinding);

            Slider.Children.Add(path);
            Slider.Children.Add(textBlock);

            clipGrid.Children.Add(Slider);

            return clipGrid;
        }

        private void RecordButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveInputFocus();

            var microphoneState = _microphone != null ? _microphone.State : (MicrophoneState?)null;
            if (microphoneState == MicrophoneState.Started) return;
            if (_isHintStoryboardPlaying) return;

            Log(string.Format("microphone_state={0} storyboard_state={1}", microphoneState, _isHintStoryboardPlaying));

            if (Component == null) return;
            if (_asyncDispatcher == null)
            {
                _asyncDispatcher = new XnaAsyncDispatcher(TimeSpan.FromMilliseconds(50), OnTimerTick);
            }

            if (_microphone == null)
            {
                try
                {
                    _microphone = Microphone.Default;
                    _microphone.BufferReady += Microphone_OnBufferReady;
                }
                catch (Exception ex)
                {
                    TLUtils.WriteException(ex);

                    RecordButton.Opacity = 0.5;
                    RecordButton.IsHitTestVisible = false;

                    ShowMicrophonePrivacy();

                    return;
                }
            }

            try
            {
                _microphone.BufferDuration = TimeSpan.FromMilliseconds(120);
                _duration = _microphone.BufferDuration;
                _buffer = new byte[_microphone.GetSampleSizeInBytes(_microphone.BufferDuration)];
            }
            catch (Exception ex)
            {
                TLUtils.WriteException(ex);

                RecordButton.Opacity = 0.5;
                RecordButton.IsHitTestVisible = false;

                ShowMicrophonePrivacy();

                return;
            }

            if (_scalingStoryboard == null)
            {
                _scalingStoryboard = GetScalingStoryboard(VolumeEllipse);
            }

            if (TimerPanel == null)
            {
                TimerPanel = CreateTimerPanel();
                InputGrid.Children.Insert(3, TimerPanel);
            }

            if (SliderPanel == null)
            {
                SliderPanel = CreateSliderPanel();
                InputGrid.Children.Insert(3, SliderPanel);
            }

            _skipBuffersCount = 0;
            _fileId = TLLong.Random();
            _fileName = _fileId.Value + ".mp3";
            _isPartReady = true;
            _uploadingLength = 0;
            _uploadableParts.Clear();

            _isSliding = true;
            _stopRequested = false;
            _cancelRequested = false;

            RaiseRecordStarted();

            if (Duration != null) Duration.Text = "0:00,0";
            if (Slider != null) ((TranslateTransform)Slider.RenderTransform).X = 0.0;
            Component.StartRecord(ApplicationData.Current.LocalFolder.Path + "\\" + _fileName);

            _stream = new MemoryStream();
            _startTime = DateTime.Now;
            Vibrate();

            Execute.BeginOnUIThread(TimeSpan.FromMilliseconds(25.0), () =>
            {
                if (!_isSliding)
                {
                    if (_stopRequested)
                    {
                        _stopRequested = false;
                        _cancelRequested = false;

                        _isHintStoryboardPlaying = true;
                        HintStoryboard.Begin();
                        return;
                    }
                    Log("_isSliding=false return");
                    return;
                }

                if (Slider != null) SliderPanel.Visibility = Visibility.Visible;
                if (TimerPanel != null) TimerPanel.Visibility = Visibility.Visible;

                _asyncDispatcher.StartService(null);
                _microphone.Start();
                _recordingStoryboard.Begin();

                StartRecordingStoryboard();

                _bufferReady = false;

                Execute.BeginOnUIThread(TimeSpan.FromSeconds(2.0), () =>
                {
                    if (!_bufferReady)
                    {
                        RecordButton.Opacity = 0.5;
                        RecordButton.IsHitTestVisible = false;

                        ShowMicrophonePrivacy();

                        CancelRecording();

                        RaiseRecordCanceled();

                        _cancelRequested = false;
                        _microphone.Stop();
                        _recordingStoryboard.Stop();
                        _asyncDispatcher.StopService();
                        Component.StopRecord();
                    }
                });
            });
        }

        private static void ShowMicrophonePrivacy()
        {
            try
            {
                var result = Guide.BeginShowMessageBox(
                    AppResources.MicrophoneAccessDenied,
                    AppResources.MicrophoneAccessDeniedDescription,
                    new [] { AppResources.Ok, AppResources.Settings },
                    0,
                    MessageBoxIcon.Alert,
                    null,
                    null);

                result.AsyncWaitHandle.WaitOne();

                int? choice = Guide.EndShowMessageBox(result);

                if (choice == 1)
                {
                    Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-microphone"));
                }
            }
            catch (Exception ex)
            {
                TLUtils.WriteException(ex);
            }
        }

        private static void Vibrate()
        {
            if (Debugger.IsAttached) return;

            VibrateController.Default.Start(TimeSpan.FromMilliseconds(25));
        }

        private void RecordButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Log("RecordButton_OnMouseLeftButtonUp");
            StopRecording();
        }

        private void LayoutRoot_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!_isSliding) return;
            if (Slider == null) return;


            var transform = (TranslateTransform) Slider.RenderTransform;
            transform.X += e.DeltaManipulation.Translation.X;

            var scaleTransform = (ScaleTransform) CoverEllipse.RenderTransform;
            var minScale = 2.5;
            var scale = 5.0  + (5.0 - minScale) * transform.X / 75.0;
            if (scale < minScale) scale = minScale;
            if (scale > 5.0) scale = 5.0;
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;

            if (transform.X > 0)
            {
                transform.X = 0;
            }
            if (transform.X < -150)
            {
                //SliderTransform.X = 0;
                _isSliding = false;

                CancelRecordingStoryboard.Begin();
            }
        }

        private void LayoutRoot_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (e.OriginalSource == RecordButtonIcon)
            {
                if (!_isSliding)
                {
                    SaveInputFocus();
                    return;
                }

                if (!_cancelRequested && Hint.Visibility == Visibility.Collapsed)
                {
                    StopRecording();
                }
            }
        }

        private void OnTimerTick()
        {
            //if (_lastUpdateTime.HasValue && (DateTime.Now - _lastUpdateTime.Value).TotalMilliseconds < 50.0) return;

            _lastUpdateTime = DateTime.Now;

            if (Duration == null) return;

            var duration = DateTime.Now - _startTime;
            Duration.Text = duration.ToString(duration.TotalHours >= 1.0 ? @"h\:mm\:ss\,f" : @"m\:ss\,f");
        }

        private bool _bufferReady;

        private void Microphone_OnBufferReady(object sender, System.EventArgs e)
        {
            _bufferReady = true;
            //var duration = DateTime.Now - _startTime;
            //System.Diagnostics.Debug.WriteLine("OnBufferReady {0}", duration.ToString(@"h\:mm\:ss\.fff"));

            const int skipStartBuffersCount = 3;

            if (Component == null) return;

            var dataLength = _microphone.GetData(_buffer);
            if (_skipBuffersCount < skipStartBuffersCount)
            {
                _skipBuffersCount++;
                return;
            }

            var volume = GetCurrentVolume();
            var scale = 5.0 + volume / 100.0 * 10.0;
            var prevScale = ((ScaleTransform)VolumeEllipse.RenderTransform).ScaleX;
            if (prevScale < scale)
            {
                _scalingStoryboard.Stop();

                _scalingXKeyFrame.Value = scale;
                _scalingYKeyFrame.Value = scale;

                _scalingStoryboard.Begin();
            }

            //VolumeEllipseTransform.ScaleX = 5.0 + volume / 100.0 * 5.0;
            //VolumeEllipseTransform.ScaleY = 5.0 + volume / 100.0 * 5.0;

            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " -volume : " + volume + " scale : " + scale + " prevScale : " + prevScale); 

            const int frameLength = 1920;
            var partsCount = dataLength / frameLength;
            _stream.Write(_buffer, 0, _buffer.Length);
            for (var i = 0; i < partsCount; i++)
            {
                var count = frameLength * (i + 1) > _buffer.Length ? _buffer.Length - frameLength * i : frameLength;
                var result = Component.WriteFrame(_buffer.SubArray(frameLength * i, count), count);
            }

            if (_stopRequested || _cancelRequested)
            {
                _microphone.Stop();
                _recordingStoryboard.Stop();
                _asyncDispatcher.StopService();
                Component.StopRecord();

                if (UploadFileDuringRecording)
                {
                    UploadAudioFileAsync(true);
                }

                if (_stopRequested)
                {
                    if ((DateTime.Now - _startTime).TotalMilliseconds < 1000.0)
                    {
                        _stopRequested = false;
                        _cancelRequested = false;
                        //Log("HintStoryboard_OnCompleted._stopRequested=false");

                        _isHintStoryboardPlaying = true;
                        HintStoryboard.Begin();
                        return;
                    }

                    RaiseAudioRecorded(_stream, (DateTime.Now - _startTime - TimeSpan.FromTicks(_microphone.BufferDuration.Ticks * skipStartBuffersCount)).TotalSeconds, _fileName, _fileId, _uploadableParts);
                    return;
                }

                if (_cancelRequested)
                {
                    RaiseRecordCanceled();
                    return;
                }
            }
            else
            {
                var now = DateTime.Now;
                if (!_lastTypingTime.HasValue
                    || _lastTypingTime.Value.AddSeconds(1.0) < now)
                {
                    _lastTypingTime = DateTime.Now;
                    RaiseRecordingAudio();
                }

                if (UploadFileDuringRecording)
                {
                    UploadAudioFileAsync(false);
                }
            }
        }

        private float GetCurrentVolume()
        {
            double rms = 0;
            ushort byte1 = 0;
            ushort byte2 = 0;
            short value = 0;
            float volume = 0;
            rms = (short) (byte1 | (byte2 << 8));

            for (int i = 0; i < _buffer.Length - 1; i += 2)
            {
                byte1 = _buffer[i];
                byte2 = _buffer[i + 1];
                value = (short) (byte1 | (byte2 << 8));
                rms += Math.Pow(value, 2);
            }
            rms /= (double) (_buffer.Length/2);
            volume = (int) Math.Floor(Math.Sqrt(rms));
            if (volume > 10000)
            {
                volume = 10000;
            }
            else if (volume < 700)
            {
                volume = 0;
            }
            volume = (float) (volume/100);
            return volume;
        }

        private void UploadAudioFileAsync(bool isLastPart)
        {
            Execute.BeginOnThreadPool(() =>
            {
                if (!_isPartReady) return;

                _isPartReady = false;

                var uploadablePart = GetUploadablePart(_fileName, _uploadingLength, _uploadableParts.Count, isLastPart);
                if (uploadablePart == null)
                {
                    _isPartReady = true;
                    return;
                }

                _uploadableParts.Add(uploadablePart);
                _uploadingLength += uploadablePart.Count;

                //Execute.BeginOnUIThread(() => VibrateController.Default.Start(TimeSpan.FromSeconds(0.02)));

                if (!isLastPart)
                {
                    var mtProtoService = IoC.Get<IMTProtoService>();
                    mtProtoService.SaveFilePartAsync(_fileId, uploadablePart.FilePart,
                        TLString.FromBigEndianData(uploadablePart.Bytes),
                        result =>
                        {
                            if (result.Value)
                            {
                                uploadablePart.Status = PartStatus.Processed;
                            }
                        },
                        error => Execute.ShowDebugMessage("upload.saveFilePart error " + error));
                }

                _isPartReady = true;
            });
        }

        private static UploadablePart GetUploadablePart(string fileName, long position, int partId, bool isLastPart = false)
        {
            var fullFilePath = ApplicationData.Current.LocalFolder.Path + "\\" + fileName;
            var fi = new FileInfo(fullFilePath);
            if (!fi.Exists)
            {
                return null;
            }

            const int minPartLength = 1024;
            const int maxPartLength = 16 * 1024;

            var recordingLength = fi.Length - position;
            if (!isLastPart && recordingLength < minPartLength)
            {
                return null;
            }

            var subpartsCount = (int)recordingLength / minPartLength;
            var uploadingBufferSize = 0;
            if (isLastPart)
            {
                if (recordingLength > 0)
                {
                    uploadingBufferSize = Math.Min(maxPartLength, (int)recordingLength);
                }
            }
            else
            {
                uploadingBufferSize = Math.Min(maxPartLength, subpartsCount * minPartLength);
            }
            if (uploadingBufferSize == 0)
            {
                return null;
            }

            var uploadingBuffer = new byte[uploadingBufferSize];

            try
            {
                using (var fileStream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Position = position;
                    fileStream.Read(uploadingBuffer, 0, uploadingBufferSize);
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage("read file " + fullFilePath + " exception " + ex);
                return null;
            }

            return new UploadablePart(null, new TLInt(partId), uploadingBuffer, position, uploadingBufferSize);
        }

        private void HintStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            _isHintStoryboardPlaying = false;

            RaiseRecordCanceled();
        }

        private void CancelRecordingStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            CancelRecording();
        }

        private Storyboard _scalingStoryboard;
        private EasingDoubleKeyFrame _scalingXKeyFrame;
        private EasingDoubleKeyFrame _scalingYKeyFrame;

        private Storyboard GetScalingStoryboard(UIElement element)
        {
            var storyboard = new Storyboard();

            var scalingXAnimation = new DoubleAnimationUsingKeyFrames();
            _scalingXKeyFrame = new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 5.0 };
            scalingXAnimation.KeyFrames.Add(_scalingXKeyFrame);
            scalingXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.5), Value = 5.0 });
            Storyboard.SetTarget(scalingXAnimation, element);
            Storyboard.SetTargetProperty(scalingXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            storyboard.Children.Add(scalingXAnimation);

            var scalingYAnimation = new DoubleAnimationUsingKeyFrames();
            _scalingYKeyFrame = new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 5.0 };
            scalingYAnimation.KeyFrames.Add(_scalingYKeyFrame);
            scalingYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.5), Value = 5.0 });
            Storyboard.SetTarget(scalingYAnimation, element);
            Storyboard.SetTargetProperty(scalingYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(scalingYAnimation);

            return storyboard;
        }

        private Storyboard _recordingStoryboard;

        private IDisposable _keyPressSubscription;

        private Storyboard GetRecordingStoryboard(UIElement element)
        {
            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1.0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(1.0), Value = 0.0 });
            Storyboard.SetTarget(opacityAnimation, element);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimation);

            return storyboard;
        }

        private void StartRecordingStoryboard()
        {
            var storyboard = new Storyboard();

            var recordEllipseAnimation = new DoubleAnimationUsingKeyFrames();
            recordEllipseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 2.5 });
            recordEllipseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 5.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
            Storyboard.SetTarget(recordEllipseAnimation, CoverEllipse);
            Storyboard.SetTargetProperty(recordEllipseAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            storyboard.Children.Add(recordEllipseAnimation);

            var recordEllipseAnimation2 = new DoubleAnimationUsingKeyFrames();
            recordEllipseAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 2.5 });
            recordEllipseAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 5.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
            Storyboard.SetTarget(recordEllipseAnimation2, CoverEllipse);
            Storyboard.SetTargetProperty(recordEllipseAnimation2, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(recordEllipseAnimation2);

            var volumeEllipseAnimation = new DoubleAnimationUsingKeyFrames();
            volumeEllipseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 2.5 });
            volumeEllipseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 5.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
            Storyboard.SetTarget(volumeEllipseAnimation, VolumeEllipse);
            Storyboard.SetTargetProperty(volumeEllipseAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            storyboard.Children.Add(volumeEllipseAnimation);

            var volumeEllipseAnimation2 = new DoubleAnimationUsingKeyFrames();
            volumeEllipseAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 2.5 });
            volumeEllipseAnimation2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 5.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
            Storyboard.SetTarget(volumeEllipseAnimation2, VolumeEllipse);
            Storyboard.SetTargetProperty(volumeEllipseAnimation2, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(volumeEllipseAnimation2);

            var timerAnimation = new DoubleAnimationUsingKeyFrames();
            timerAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -100.0 });
            timerAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase{ EasingMode = EasingMode.EaseOut, Exponent = 5.0 }});
            Storyboard.SetTarget(timerAnimation, TimerPanel);
            Storyboard.SetTargetProperty(timerAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(timerAnimation);
            
            var sliderTextAnimation = new DoubleAnimationUsingKeyFrames();
            sliderTextAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.05), Value = 100.0 });
            sliderTextAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.30), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
            Storyboard.SetTarget(sliderTextAnimation, _sliderTextTransform);
            Storyboard.SetTargetProperty(sliderTextAnimation, new PropertyPath("X"));
            storyboard.Children.Add(sliderTextAnimation);

            var sliderPathAnimation = new DoubleAnimationUsingKeyFrames();
            sliderPathAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 100.0 });
            sliderPathAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } });
            Storyboard.SetTarget(sliderPathAnimation, _sliderPathTransform);
            Storyboard.SetTargetProperty(sliderPathAnimation, new PropertyPath("TranslateX"));
            storyboard.Children.Add(sliderPathAnimation);

            storyboard.Begin();
        }

        private void CancelRecording()
        {
            if (Slider != null) SliderPanel.Visibility = Visibility.Collapsed;
            if (TimerPanel != null) TimerPanel.Visibility = Visibility.Collapsed;

            if (!_stopRequested)
            {
                _cancelRequested = true;
            }
            _isSliding = false;
            _lastTypingTime = null;
        }

        private void StopRecording()
        {
            Vibrate();

            if (Slider != null) SliderPanel.Visibility = Visibility.Collapsed;
            if (TimerPanel != null) TimerPanel.Visibility = Visibility.Collapsed;
            _stopRequested = true;
            _isSliding = false;
            _lastTypingTime = null;
        }

        public event EventHandler<AudioEventArgs> AudioRecorded;

        protected virtual void RaiseAudioRecorded(MemoryStream stream, double duration, string fileName, TLLong fileId, IList<UploadablePart> parts)
        {
            SetInputVisibility(Visibility.Visible);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("parts");
            foreach (var part in parts)
            {
                stringBuilder.AppendLine(string.Format("file_part={0} position={1} count={2} status={3}", part.FilePart, part.Position, part.Count, part.Status));
            }

            Telegram.Logs.Log.Write(string.Format("AudioRecorderControl.AudioRecorded duration={0} file_name={1}\n{2}", duration, fileName, stringBuilder));

            var handler = AudioRecorded;
            if (handler != null) handler(this, new AudioEventArgs(stream, duration, fileName, fileId, parts));
        }

        public event EventHandler<System.EventArgs> RecordCanceled;

        protected virtual void RaiseRecordCanceled()
        {
            SetInputVisibility(Visibility.Visible);

            Telegram.Logs.Log.Write("AudioRecorderControl.AudioRecordCanceled");

            var handler = RecordCanceled;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<System.EventArgs> RecordStarted;

        protected virtual void RaiseRecordStarted()
        {
            SetInputVisibility(Visibility.Collapsed);
            Telegram.Logs.Log.Write("AudioRecorderControl.AudioRecordStarted");

            var handler = RecordStarted;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<System.EventArgs> RecordingAudio;

        protected virtual void RaiseRecordingAudio()
        {
            var handler = RecordingAudio;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void SetInputVisibility(Visibility visibility)
        {
            System.Diagnostics.Debug.WriteLine("SetInputVisibility " + visibility);
            //Scroll.IsHitTestVisible = visibility == Visibility.Visible;
            //EmojiButton.IsHitTestVisible = visibility == Visibility.Visible;
            //AdditionalButtons.IsHitTestVisible = visibility == Visibility.Visible;
            EmojiButton.Visibility = visibility;
            Scroll.Visibility = visibility;
            AdditionalButtons.Visibility = visibility;
            if (visibility == Visibility.Collapsed)
            {
                RecordButtonIcon.Background = new SolidColorBrush(Colors.White);
                VolumeEllipse.Visibility = Visibility.Visible;
                ((ScaleTransform) VolumeEllipse.RenderTransform).ScaleX = 2.5;
                ((ScaleTransform) VolumeEllipse.RenderTransform).ScaleY = 2.5;
                CoverEllipse.Visibility = Visibility.Visible;
                ((ScaleTransform) CoverEllipse.RenderTransform).ScaleX = 2.5;
                ((ScaleTransform) CoverEllipse.RenderTransform).ScaleY = 2.5;
            }
            else
            {
                RecordButtonIcon.Background = (Brush) Resources["InputForegroundBrushLight"];
                VolumeEllipse.Visibility = Visibility.Collapsed;
                CoverEllipse.Visibility = Visibility.Collapsed;
            }
            SaveInputFocus();
        }
        #endregion

        private void EmojiButton_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }

        public bool IsFullScreen { get; protected set; }

        public void OpenFullScreen(double addHeight)
        {
            if (IsFullScreen) return;

            addHeight = addHeight > 7.0 ? addHeight : 0.0;

            Margin = new Thickness(0.0, -7.0, 0.0, 0.0);
            KeyboardPlaceholder.Height = KeyboardPlaceholder.Height + addHeight;
            IsFullScreen = true;

            var emojiControl = KeyboardPlaceholder.Content as EmojiControl;
            if (emojiControl != null)
            {
                emojiControl.OpenSearch();
            }
        }

        public void CloseFullScreen()
        {
            Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
            KeyboardPlaceholder.Height = KeyboardHeight;
            IsFullScreen = false;

            var emojiControl = KeyboardPlaceholder.Content as EmojiControl;
            if (emojiControl != null)
            {
                emojiControl.CloseSearch();
            }
        }
    }

    public class EmojiHintsEventArgs
    {
        public EmojiSuggestionParams Parameters { get; protected set; }

        public EmojiHintsEventArgs(EmojiSuggestionParams parameters)
        {
            Parameters = parameters;
        }
    }

    public class EmojiSuggestionParams
    {
        public int Index { get; set; }

        public int Length { get; set; }

        public int SelectionStart { get; set; }

        public string Text { get; set; }

        public string Query { get; set; }

        public EmojiSuggestion[] Results { get; set; }

        public override string ToString()
        {
            return string.Format("text={0} selectionStart={1} query={2} results={3}", Text, SelectionStart, Query, Results != null ? Results.Length : 0);
        }
    }
}
