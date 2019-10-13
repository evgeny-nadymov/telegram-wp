// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.EmojiPanel;
using TelegramClient.Converters;
using TelegramClient.Views.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Controls
{
    public partial class DecryptedMessageControl
    {

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof (TLDecryptedMessageBase), typeof (DecryptedMessageControl), new PropertyMetadata(default(TLDecryptedMessageBase), OnMessageChanged));

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as DecryptedMessageControl;
            if (messageControl != null)
            {
                messageControl._isChannelMessage = false;

                messageControl.UpdateBinding(e.NewValue as TLDecryptedMessageBase);
                //messageControl.ClearBinding(e.NewValue as TLDecryptedMessageBase);
                //messageControl.SetBinding(e.NewValue as TLDecryptedMessageBase);

                var messageBase = e.NewValue as TLDecryptedMessageBase;
                if (messageBase != null)
                {
                    var message = messageBase as TLDecryptedMessage;
                    var messageService = messageBase as TLDecryptedMessageService;
                    if (message != null)
                    {
                        messageControl.ToMessageTemplate(message);
                    }
                    else if (messageService != null)
                    {
                        messageControl.ToServiceTemplate(messageService);
                    }
                    else
                    {
                        messageControl.ToEmptyTemplate();
                    }
                }
                else
                {
                    messageControl.ToEmptyTemplate();
                }
            }
        }

        private void ToMessageTemplate(TLDecryptedMessage message)
        {
            DrawBubble(message);
        }

        private static bool GetIsChannelMessage(TLDecryptedMessageBase messageCommon)
        {
            return false;
        }

        private static bool IsDocument(TLDecryptedMessage message)
        {
            if (message == null) return false;

            var mediaDocument = message.Media as TLDecryptedMessageMediaDocument45;
            if (mediaDocument == null) return false;

            var document = mediaDocument.Document as IAttributes;
            if (document == null) return false;

            foreach (var attribute in document.Attributes)
            {
                if (attribute is TLDocumentAttributeAnimated
                    || attribute is TLDocumentAttributeSticker
                    || attribute is TLDocumentAttributeVideo
                    )
                {
                    return false;
                }

                var documentAttributeAudio = attribute as TLDocumentAttributeAudio46;
                if (documentAttributeAudio != null && documentAttributeAudio.Voice)
                {
                    return false;
                }

            }

            return true;
        }

        private bool GetOutput(TLDecryptedMessageBase message)
        {
            return message != null && message.Out.Value && !_isChannelMessage;
        }

        private void DrawBubble(TLDecryptedMessageBase messageCommon)
        {
            var isLightTheme = (Visibility) Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var message = messageCommon as TLDecryptedMessage;
            var messageService = messageCommon as TLDecryptedMessageService;
            var unreadSeparator = false;//messageService != null && messageService.Action is TLDecryptedMessageActionUnreadMessages;
            var sticker = message != null && message.IsSticker();
            _isChannelMessage = GetIsChannelMessage(messageCommon);
            var output = GetOutput(messageCommon);
            var grouped = message != null && message.Media is TLDecryptedMessageMediaGroup;

            FromLabel.Visibility = Visibility.Collapsed;
            Tile.Visibility = Visibility.Collapsed;

            SetBackgroundAndBorder(messageCommon);

            Brush foreground;
            if (messageService != null)
            {
                foreground = (Brush)Resources["ServiceMessageForegroundBrush"];
            }
            else
            {
                foreground = isLightTheme
                    ? (output
                        ? (Brush)Resources["OutputForegroundBrushLight"]
                        : (Brush)Resources["InputForegroundBrushLight"])
                    : (output
                        ? (Brush)Resources["OutputForegroundBrushDark"]
                        : (Brush)Resources["InputForegroundBrushDark"]);            
            }   

            Brush footerForeground;
            if (!sticker)
            {
                footerForeground = isLightTheme
                ? (output
                    ? (Brush)Resources["OutputSubtleBrushLight"]
                    : (Brush)Resources["InputSubtleBrushLight"])
                : (output
                    ? (Brush)Resources["OutputSubtleBrushDark"]
                    : (Brush)Resources["InputSubtleBrushDark"]);
            }
            else
            {
                footerForeground = isLightTheme
                    ? (Brush)Resources["StickerFooterSubtleBrushLight"]
                    : (Brush)Resources["StickerFooterSubtleBrushDark"];
            }

            Brush footerBackground;
            if (!sticker)
            {
                footerBackground = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                footerBackground = isLightTheme
                    ? (Brush)Resources["StickerFooterBackgroundBrushLight"]
                    : (Brush)Resources["StickerFooterBackgroundBrushDark"];
            }

            Foreground = foreground;
            InputMessage.SetForeground(foreground);

            var beforeLastGridLength = output && messageService == null 
                ? new GridLength(1.0, GridUnitType.Star)
                : GridLength.Auto;

            var lastGridLength = output && messageService == null 
                ? GridLength.Auto
                : new GridLength(1.0, GridUnitType.Star);

            var bubbleGridColumn = output && messageService == null 
                ? 4
                : 3;

            var cornerGridColumn = output && messageService == null 
                ? 5
                : 2;

            Corner.Margin = output
                ? new Thickness(-1.0, 12.0, 0.0, 0.0)
                : new Thickness(0.0, 12.0, -1.0, 0.0);
            Corner.HorizontalAlignment = output
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right;
            PathScaleTransform.ScaleX = output ? -1.0 : 1.0;
            Corner.Visibility = messageService != null ? Visibility.Collapsed : Visibility.Visible;
            CornerBorder.Visibility = messageService != null ? Visibility.Collapsed : Visibility.Visible;
            if (unreadSeparator)
            {
                MainBorder.Margin = new Thickness(-18.0, 0.0, -18.0, 6.0);
                MainBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
                Panel.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else if (messageService != null)
            {
                MainBorder.Margin = new Thickness(0.0, 0.0, 0.0, 6.0);
                MainBorder.HorizontalAlignment = HorizontalAlignment.Center;
                Panel.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                MainBorder.Margin = new Thickness(0.0, 0.0, 0.0, 6.0);
                MainBorder.HorizontalAlignment = HorizontalAlignment.Left;
                Panel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            Grid.SetColumnSpan(MainBorder, messageService != null ? 2 : 1);
            InputMessage.TextAlignment = messageService != null ? TextAlignment.Center : TextAlignment.Left;

            MainBorder.BorderThickness = output
                ? new Thickness(1.0, 1.0, 0.0, 1.0)
                : new Thickness(0.0, 1.0, 1.0, 1.0);
            CornerBorder.BorderThickness = output
                ? new Thickness(1.0, 0.0, 0.0, 0.0)
                : new Thickness(0.0, 0.0, 1.0, 0.0);

            BeforeLastColumn.Width = beforeLastGridLength;
            LastColumn.Width = lastGridLength;
            Grid.SetColumn(MainBorder, bubbleGridColumn);
            Grid.SetColumn(Corner, cornerGridColumn);
            Grid.SetColumn(CornerBorder, cornerGridColumn);

            Header.MaxWidth = messageCommon != null ? messageCommon.MediaWidth : 12.0 + 311.0 + 12.0;
            //FwdFromGrid.Visibility = Visibility.Collapsed;

            var message45 = message as TLDecryptedMessage45;
            ViaBotGrid.Visibility = message45 != null && !TLString.IsNullOrEmpty(message45.ViaBotName) ? Visibility.Visible : Visibility.Collapsed;
            ReplyContent.Visibility = message != null ? message.ReplyVisibility : Visibility.Collapsed;
            if (FromLabel.Visibility == Visibility.Visible
                //|| FwdFromGrid.Visibility == Visibility.Visible
                || ViaBotGrid.Visibility == Visibility.Visible
                || ReplyContent.Visibility == Visibility.Visible)
            {
                Header.Visibility = Visibility.Visible;
            }
            else
            {
                Header.Visibility = Visibility.Collapsed;
            }
            if (FromLabel.Visibility == Visibility.Visible
                //&& FwdFromGrid.Visibility == Visibility.Collapsed
                && ViaBotGrid.Visibility == Visibility.Collapsed
                && ReplyContent.Visibility == Visibility.Collapsed
                && message != null
                && !TLString.IsNullOrEmpty(message.Message))
            {
                FromLabel.Margin = new Thickness(0.0, 2.0, 0.0, -4.0);
            }
            else if (FromLabel.Visibility == Visibility.Visible
                //&& FwdFromGrid.Visibility == Visibility.Collapsed
                && ViaBotGrid.Visibility == Visibility.Collapsed
                && message != null && message.ReplyInfo != null)
            {
                FromLabel.Margin = new Thickness(0.0, 2.0, 0.0, 6.0);
            }
            else
            {
                FromLabel.Margin = new Thickness(0.0, 2.0, 0.0, 0.0);
            }
            CaptionPanel.Children.Remove(ViaBotGrid);
            FromLabelPanel.Children.Remove(ViaBotGrid);
            if (FromLabel.Visibility == Visibility.Visible
                && ViaBotGrid.Visibility == Visibility.Visible)
            {
                ViaBotGrid.Margin = FromLabel.Margin;
                FromLabel.Margin = new Thickness(FromLabel.Margin.Left, FromLabel.Margin.Top, FromLabel.Margin.Right + 6.0, FromLabel.Margin.Bottom);
                FromLabelPanel.Children.Add(ViaBotGrid);
            }
            else
            {
                ViaBotGrid.Margin = new Thickness(0.0, -6.0, 0.0, 0.0);
                CaptionPanel.Children.Insert(2, ViaBotGrid);
            }

            Status.Visibility = output? Visibility.Visible : Visibility.Collapsed;
            
            MessageGrid.MaxWidth = messageCommon != null ? messageCommon.MediaWidth : 12.0 + 311.0 + 12.0;

            Panel.Children.Remove(Header);
            MainItemGrid.Children.Remove(Header);
            if (messageService != null)
            {
                MessageGrid.Margin = new Thickness(0.0, 2.0, 0.0, 7.0);
            }
            else if (sticker)
            {
                ReplyContent.Foreground = footerForeground;
                ViaBotGrid.Foreground = new SolidColorBrush(Colors.White);
                Header.Background = footerBackground;
                Grid.SetRow(Header, 1);
                Grid.SetColumn(Header, output ? bubbleGridColumn - 1 : bubbleGridColumn + 1);
                Header.HorizontalAlignment = output ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                Header.Margin = new Thickness(6.0);
                MainItemGrid.Children.Add(Header);
            }
            else
            {
                ReplyContent.Foreground = foreground;
                ViaBotGrid.Foreground = (Brush) Application.Current.Resources["TelegramBadgeAccentBrush"];
                Header.SetValue(Grid.RowProperty, DependencyProperty.UnsetValue);
                Header.SetValue(Grid.ColumnProperty, DependencyProperty.UnsetValue);
                Header.HorizontalAlignment = HorizontalAlignment.Left;
                Header.Margin = new Thickness(0.0, 0.0, 0.0, -6.0);
                Panel.Children.Insert(0, Header);
            }

            SetFooter(messageCommon);
        }

        private void SetBackgroundAndBorder(TLDecryptedMessageBase messageCommon)
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var messageService = messageCommon as TLDecryptedMessageService;
            var sticker = messageCommon != null && messageCommon.IsSticker();
            var output = GetOutput(messageCommon);

            Brush border;
            if (messageService != null)
            {
                border = (Brush)Resources["ServiceMessageBorderBrush"];
            }
            else if (sticker)
            {
                border = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                border = isLightTheme
                    ? (output
                        ? (Brush)Resources["OutputBorderBrushLight"]
                        : (Brush)Resources["InputBorderBrushLight"])
                    : (output
                        ? (Brush)Resources["OutputBorderBrushDark"]
                        : (Brush)Resources["InputBorderBrushDark"]);
            }

            Brush background;
            if (messageService != null)
            {
                background = (Brush)Resources["ServiceMessageBackgroundBrush"];
            }
            else if (sticker)
            {
                background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                background = isLightTheme
                    ? (output
                        ? (Brush)Resources["OutputBackgroundBrushLight"]
                        : (Brush)Resources["InputBackgroundBrushLight"])
                    : (output
                        ? (Brush)Resources["OutputBackgroundBrushDark"]
                        : (Brush)Resources["InputBackgroundBrushDark"]);
            }

            Corner.Fill = background;
            Corner.Stroke = border;
            CornerBorder.BorderBrush = border;

            MainBorder.Background = background;
            MainBorder.BorderBrush = border;
            MorePanel.Background = background;
            Header.Background = background;
        }

        private bool _isChannelMessage;

        private void ToServiceTemplate(TLDecryptedMessageService messageService)
        {
            _isChannelMessage = false;
            FromLabel.Visibility = Visibility.Collapsed;
            Tile.Visibility = Visibility.Collapsed;

            var serviceMessageToTextConverter = new DecryptedServiceMessageToTextConverter();
            InputMessage.Text = (string)serviceMessageToTextConverter.Convert(messageService.Self, null, null, null);

            DrawBubble(messageService);

            //var unreadMessagesAction = messageService.Action as TLDecryptedMessageActionUnreadMessages;
            //if (unreadMessagesAction != null)
            //{
            //    ToUnreadMessagesTemplate();
            //}
        }

        private Binding _authorBinding;
        private Binding _viaBotNameBinding;
        private Binding _inputMessageTextBinding;
        private Binding _inputMessageEntitiesBinding;
        private Binding _inputMessageVisibilityBinding;
        private Binding _mediaContentControlContentBinding;
        private Binding _mediaContentControlContentTemplateBinding;
        private Binding _commandsReplyMarkupBinding;
        private Binding _editLabelVisibilityBinding;

        private void SaveBinding(ref Binding binding, FrameworkElement element, DependencyProperty dp)
        {
            if (binding == null)
            {
                var bindingExpression = element.GetBindingExpression(dp);
                if (bindingExpression != null)
                {
                    binding = bindingExpression.ParentBinding;
                }
                element.ClearValue(dp);
            }
        }

        private void RestoreBinding(ref Binding binding, FrameworkElement element, DependencyProperty dp)
        {
            if (binding != null)
            {
                element.SetBinding(dp, binding);
                binding = null;
            }
        }

        private void UpdateBinding(TLDecryptedMessageBase messageBase)
        {
            var serviceMessage = messageBase as TLDecryptedMessageService;
            if (serviceMessage != null)
            {
                //SaveBinding(ref _fwdFromLabelTextBinding, FwdFromLabel, Run.TextProperty);
                SaveBinding(ref _inputMessageVisibilityBinding, InputMessage, TelegramRichTextBox.VisibilityProperty);
                SaveBinding(ref _inputMessageEntitiesBinding, InputMessage, TelegramRichTextBox.EntitiesProperty);
                SaveBinding(ref _inputMessageTextBinding, InputMessage, TelegramRichTextBox.TextProperty);
                SaveBinding(ref _mediaContentControlContentBinding, MediaContentControl, ContentControl.ContentProperty);
                SaveBinding(ref _mediaContentControlContentTemplateBinding, MediaContentControl, ContentControl.ContentTemplateProperty);
                SaveBinding(ref _viaBotNameBinding, ViaBotGrid, TextBlock.TextProperty);
                //SaveBinding(ref _authorBinding, AuthorLabel, TextBlock.TextProperty);
                //SaveBinding(ref _editLabelVisibilityBinding, EditLabel, TextBlock.VisibilityProperty);
                //SaveBinding(ref _commandsReplyMarkupBinding, Commands, CommandsControl.ReplyMarkupProperty);


                //System.Diagnostics.Debug.WriteLine("ClearMediaBinding id=" + messageBase.Id);
                _suppressFooterReplacement = true;
                ClearValue(MediaProperty);
                ClearValue(MediaCaptionProperty);
                _suppressFooterReplacement = false;
                //SaveBinding(ref _mediaBinding, this, MediaProperty);
            }

            var message = messageBase as TLDecryptedMessage;
            if (message != null)
            {
                var stopwatch = Stopwatch.StartNew();
                RestoreBinding(ref _inputMessageVisibilityBinding, InputMessage, TelegramRichTextBox.VisibilityProperty);
                RestoreBinding(ref _inputMessageEntitiesBinding, InputMessage, TelegramRichTextBox.EntitiesProperty);
                RestoreBinding(ref _inputMessageTextBinding, InputMessage, TelegramRichTextBox.TextProperty);
                RestoreBinding(ref _mediaContentControlContentBinding, MediaContentControl, ContentControl.ContentProperty);
                RestoreBinding(ref _mediaContentControlContentTemplateBinding, MediaContentControl, ContentControl.ContentTemplateProperty);
                RestoreBinding(ref _viaBotNameBinding, ViaBotGrid, TextBlock.TextProperty);
                //RestoreBinding(ref _authorBinding, AuthorLabel, TextBlock.TextProperty);
                //RestoreBinding(ref _editLabelVisibilityBinding, EditLabel, TextBlock.VisibilityProperty);
                //RestoreBinding(ref _commandsReplyMarkupBinding, Commands, CommandsControl.ReplyMarkupProperty);

                //System.Diagnostics.Debug.WriteLine("RestoreBinding elapsed=" + stopwatch.Elapsed);

                _suppressFooterReplacement = true;
                var mediaBinding = new Binding("Media") { Source = message, Mode = BindingMode.OneWay };
                SetBinding(MediaProperty, mediaBinding);
                var mediaCaption = message.Media as IMediaCaption;
                if (mediaCaption != null)
                {
                    var mediaCaptionBinding = new Binding("Media.Caption") { Source = message, Mode = BindingMode.OneWay };
                    SetBinding(MediaCaptionProperty, mediaCaptionBinding);
                }
                _suppressFooterReplacement = false;
            }
        }

        private bool _suppressFooterReplacement;

        public static readonly DependencyProperty MediaCaptionProperty = DependencyProperty.Register(
            "MediaCaption", typeof (string), typeof (DecryptedMessageControl), new PropertyMetadata(default(string), OnMediaCaptionChanged));

        private static void OnMediaCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as DecryptedMessageControl;
            if (messageControl != null && !messageControl._suppressFooterReplacement)
            {
                var oldCaption = e.OldValue as string;
                var newCaption = e.NewValue as string;
                if (!string.Equals(oldCaption, newCaption, StringComparison.Ordinal))
                {
                    messageControl.SetFooter(messageControl.Message);
                }
            }
        }

        public string MediaCaption
        {
            get { return (string) GetValue(MediaCaptionProperty); }
            set { SetValue(MediaCaptionProperty, value); }
        }

        public static readonly DependencyProperty MediaProperty = DependencyProperty.Register(
            "Media", typeof (TLDecryptedMessageMediaBase), typeof (DecryptedMessageControl), new PropertyMetadata(default(TLDecryptedMessageMediaBase), OnMediaChanged));

        private static void OnMediaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as DecryptedMessageControl;
            if (messageControl != null && !messageControl._suppressFooterReplacement)
            {
                var oldWebPage = e.OldValue as TLDecryptedMessageMediaWebPage;
                var newWebPage = e.NewValue as TLDecryptedMessageMediaWebPage;
                if (oldWebPage != null
                    || newWebPage != null)
                {
                    messageControl.SetFooter(messageControl.Message);
                }
            }
        }

        private void SetFooter(TLDecryptedMessageBase messageBase)
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            
            var message = messageBase as TLDecryptedMessage;
            var service = messageBase as TLDecryptedMessageService;
            var output = GetOutput(messageBase);

            var isGroupedMedia = message != null && message.Media is TLDecryptedMessageMediaGroup;
            var isPhoto = message != null && message.Media is TLDecryptedMessageMediaPhoto;
            var isVideo = message != null && message.IsVideo();
            var isGeoPoint = message != null && message.Media is TLDecryptedMessageMediaGeoPoint;
            var isVenue = message != null && message.Media is TLDecryptedMessageMediaVenue;
            var isDocument = message != null && IsDocument(message);
            var isVoice = message != null && message.IsVoice();
            var isSticker = message != null && message.IsSticker();
            var isWebPage = message != null && message.Media is TLDecryptedMessageMediaWebPage;
            var isEmptyMedia = message != null && (message.Media == null || message.Media is TLDecryptedMessageMediaEmpty);

            //var isGif = message != null && message.IsGif();
            var isShortFooter = IsShortFooter(message, isGroupedMedia, isPhoto, isVideo, isGeoPoint, isDocument, isVoice, isSticker, isWebPage, isEmptyMedia);

            Brush background = new SolidColorBrush(Colors.Transparent);
            if (service != null)
            {
                background = (Brush)Resources["ServiceMessageBackgroundBrush"];
            }
            else if (!isSticker)
            {
                background = isLightTheme
                    ? (output
                        ? (Brush)Resources["OutputBackgroundBrushLight"]
                        : (Brush)Resources["InputBackgroundBrushLight"])
                    : (output
                        ? (Brush)Resources["OutputBackgroundBrushDark"]
                        : (Brush)Resources["InputBackgroundBrushDark"]);
            }

            Brush footerForeground;
            if (isSticker)
            {
                footerForeground = isLightTheme
                    ? (Brush)Resources["StickerFooterSubtleBrushLight"]
                    : (Brush)Resources["StickerFooterSubtleBrushDark"];
            }
            else if (isShortFooter && (isGroupedMedia || isPhoto || (isGeoPoint && !isVenue) || isVideo))
            {
                footerForeground = new SolidColorBrush(Colors.White);
            }
            else
            {
                footerForeground = isLightTheme
                ? (output
                    ? (Brush)Resources["OutputSubtleBrushLight"]
                    : (Brush)Resources["InputSubtleBrushLight"])
                : (output
                    ? (Brush)Resources["OutputSubtleBrushDark"]
                    : (Brush)Resources["InputSubtleBrushDark"]);
            }

            Brush footerBackground;
            if (isSticker)
            {
                footerBackground = isLightTheme
                    ? (Brush)Resources["StickerFooterBackgroundBrushLight"]
                    : (Brush)Resources["StickerFooterBackgroundBrushDark"];
            }
            else if (isShortFooter && (isGroupedMedia || isPhoto || (isGeoPoint && !isVenue) || isVideo))
            {
                footerBackground = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
            }
            else
            {
                footerBackground = new SolidColorBrush(Colors.Transparent);
            }

            FooterContent.Foreground = footerForeground;
            FooterContentGrid.Background = footerBackground;
            Footer.MaxWidth = message != null ? message.MediaWidth : 12.0 + 311.0 + 12.0;
            Status.Fill = footerForeground;
            //ViewsIcon.Stroke = footerForeground;

            if (messageBase != null && messageBase.Reply != null && (isGroupedMedia || isPhoto || isVideo))
            {
                MediaContentControl.Margin = new Thickness(12.0, 3.0, 12.0, 0.0);
            }
            else
            {
                MediaContentControl.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
            }
            
            // setup message and media position
            //MessageGrid.Visibility = message != null && !TLString.IsNullOrEmpty(message.Message) || showAsServiceMessage
            //    ? Visibility.Visible
            //    : Visibility.Collapsed;
            //Panel.Children.Remove(MessageGrid);
            //Panel.Children.Remove(MediaGrid);
            //if (message != null && (message.Media is TLMessageMediaWebPage || message.Media is TLMessageMediaEmpty) || showAsServiceMessage)
            //{
            //    MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
            //    MediaGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);

            //    Panel.Children.Add(MessageGrid);
            //    Panel.Children.Add(MediaGrid);
            //}
            //else
            //{
            //    MessageGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
            //    MediaGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);

            //    Panel.Children.Add(MediaGrid);
            //    Panel.Children.Add(MessageGrid);
            //}

            // setup footer position
            Panel.Children.Remove(Footer);
            MediaGrid.Children.Remove(Footer);
            MessageGrid.Children.Remove(Footer);
            if (service != null)
            {
                // remove footer
            }
            else if (!isShortFooter)
            {
                Footer.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
                Footer.Background = background;
                Footer.HorizontalAlignment = HorizontalAlignment.Stretch;
                Footer.VerticalAlignment = VerticalAlignment.Stretch;
                Panel.Children.Add(Footer);
                MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
            }
            else
            {
                if (isSticker)
                {
                    Footer.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
                    Footer.Background = new SolidColorBrush(Colors.Transparent);
                    Footer.HorizontalAlignment = HorizontalAlignment.Right;
                    Footer.VerticalAlignment = VerticalAlignment.Bottom;
                    MediaGrid.Children.Add(Footer);
                    MessageGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
                }
                else if (isEmptyMedia)
                {
                    Footer.Margin = new Thickness(0.0, -1.0, 0.0, -11.0);
                    Footer.Background = new SolidColorBrush(Colors.Transparent);
                    Footer.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Footer.VerticalAlignment = VerticalAlignment.Bottom;
                    MessageGrid.Children.Add(Footer);
                    MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 11.0);
                }
                else if (isGroupedMedia || isPhoto || isGeoPoint || isVideo || isVoice || isDocument)
                {
                    Footer.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
                    Footer.Background = new SolidColorBrush(Colors.Transparent);
                    Footer.HorizontalAlignment = HorizontalAlignment.Right;
                    Footer.VerticalAlignment = VerticalAlignment.Bottom;
                    MediaGrid.Children.Add(Footer);
                    MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
                }
                else
                {
                    Footer.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
                    Footer.Background = background;
                    Footer.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Footer.VerticalAlignment = VerticalAlignment.Stretch;
                    Panel.Children.Add(Footer);
                    MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
                }
            }

            if (message != null
                && (message.Media == null || message.Media is TLDecryptedMessageMediaEmpty))
            {
                var messageDateTimeConverter = (IValueConverter) Application.Current.Resources["MessageDateTimeConverter"];
                var dateText = messageDateTimeConverter.Convert(message.Date, null, null, null);

                var footerBuilder = new StringBuilder();
                if (FlowDirection == FlowDirection.RightToLeft)
                {
                    footerBuilder.Append("د");
                }
                else
                {
                    footerBuilder.Append("a");
                }
                footerBuilder.Append("/ " + dateText);
                if (message.Out.Value)
                {
                    footerBuilder.Append(" W");
                }

                InputMessage.Footer = footerBuilder.ToString();
            }
            else
            {
                InputMessage.Footer = string.Empty;
            }

            if (isVoice)
            {
                MediaContentControl.Foreground = footerForeground;
            }
            else
            {
                MediaContentControl.SetValue(Control.ForegroundProperty, DependencyProperty.UnsetValue);
            }
        }

        private static bool IsShortFooter(TLDecryptedMessage message, bool isGrouped, bool isPhoto, bool isVideo, bool isGeoPoint, bool isDocument, bool isVoice, bool isSticker, bool isWebPage, bool isEmptyMedia)
        {
            if (message != null)
            {
                var mediaCaption = message.Media as IMediaCaption;
                if (mediaCaption != null && !TLString.IsNullOrEmpty(mediaCaption.Caption))
                {
                    return false;
                }

                if (isWebPage)
                {
                    return false;
                }

                if (isGrouped || isPhoto || isVideo || isGeoPoint)
                {
                    return true;
                }
                
                if (isSticker)
                {
                    return true;
                }

                if (isEmptyMedia)
                {
                    return true;
                }

                if (isDocument || isVoice)
                {
                    return true;
                }
            }

            return false;
        }

        protected TLDecryptedMessageMediaBase Media
        {
            get { return (TLDecryptedMessageMediaBase) GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }

        private void ToUnreadMessagesTemplate()
        {

        }

        private void ToEmptyTemplate()
        {
            
        }

        public TLDecryptedMessageBase Message
        {
            get { return (TLDecryptedMessageBase) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        private static int _count;

        public DecryptedMessageControl()
        {
            InitializeComponent();

            //System.Diagnostics.Debug.WriteLine("ctor " + _count++);
        }

        public event EventHandler<GestureEventArgs> TapViaBot;

        protected virtual void RaiseTapViaBot(object sender, GestureEventArgs args)
        {
            var handler = TapViaBot;
            if (handler != null) handler(sender, args);
        }

        private void ViaBot_Tap(object sender, GestureEventArgs args)
        {
            RaiseTapViaBot(sender, args);
        }

        public event EventHandler<GestureEventArgs> TapMorePanel;

        protected virtual void RaiseTapMorePanel(object sender, GestureEventArgs args)
        {
            var handler = TapMorePanel;
            if (handler != null) handler(sender, args);
        }

        private void MorePanel_OnTap(object sender, GestureEventArgs args)
        {
            RaiseTapMorePanel(sender, args);
        }

        public event EventHandler<GestureEventArgs> TapUserTile;

        protected virtual void RaiseTapUserTile(object sender, GestureEventArgs args)
        {
            var handler = TapUserTile;
            if (handler != null) handler(sender, args);
        }

        private void Tile_OnTap(object sender, GestureEventArgs args)
        {
            RaiseTapUserTile(sender, args);
        }

        public event EventHandler<KeyboardButtonEventArgs> CommandsControlButtonClick;

        protected virtual void RaiseCommandsControlButtonClick(object sender, KeyboardButtonEventArgs e)
        {
            var handler = CommandsControlButtonClick;
            if (handler != null) handler(sender, e);
        }

        private void CommandsControl_OnButtonClick(object sender, KeyboardButtonEventArgs e)
        {
            RaiseCommandsControlButtonClick(sender, e);
        }

        public event EventHandler<RoutedEventArgs> ShareButtonClick;

        protected virtual void RaiseShareButtonClick(object sender, RoutedEventArgs e)
        {
            var handler = ShareButtonClick;
            if (handler != null) handler(sender, e);
        }

        private void ShareButton_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseShareButtonClick(sender, e);
        }

        #region Encrypted Timer

        public event EventHandler StartTimer;

        protected virtual void RaiseStartTimer()
        {
            var handler = StartTimer;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void SecretPhotoPlaceholder_OnStartTimer(object sender, System.EventArgs e)
        {
            RaiseStartTimer();
        }

        public event EventHandler Elapsed;

        protected virtual void RaiseElapsed()
        {
            var handler = Elapsed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void SecretPhotoPlaceholder_OnElapsed(object sender, System.EventArgs e)
        {
            RaiseElapsed();
        }
        #endregion

        public event EventHandler<GestureEventArgs> TapMedia;

        private void MediaContentControl_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            RaiseTapMedia(e);
        }

        protected virtual void RaiseTapMedia(GestureEventArgs e)
        {
            var handler = TapMedia;
            if (handler != null) handler(this, e);
        }
    }
}
