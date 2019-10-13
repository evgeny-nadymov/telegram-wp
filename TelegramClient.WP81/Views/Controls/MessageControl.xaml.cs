// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Phone.Media.Devices;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.EmojiPanel;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Views.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Controls
{
    public partial class MessageControl
    {
        //~MessageControl()
        //{
            
        //}

        public static readonly DependencyProperty ShareButtonContextMenuProperty = DependencyProperty.Register(
            "ShareButtonContextMenu", typeof (ContextMenu), typeof (MessageControl), new PropertyMetadata(default(ContextMenu), OnShareButtonContextMenuChanged));

        private static void OnShareButtonContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as MessageControl;
            if (messageControl != null)
            {
                ContextMenuService.SetContextMenu(messageControl.ShareButton, e.NewValue as ContextMenu);
            }
        }

        public ContextMenu ShareButtonContextMenu
        {
            get { return (ContextMenu) GetValue(ShareButtonContextMenuProperty); }
            set { SetValue(ShareButtonContextMenuProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof (TLMessageBase), typeof (MessageControl), new PropertyMetadata(default(TLMessageBase), OnMessageChanged));

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as MessageControl;
            if (messageControl != null)
            {
                OnMessageChangedInternal(messageControl, e.NewValue as TLMessageBase);
            }
        }

        private static void OnMessageChangedInternal(MessageControl messageControl, TLMessageBase newMessage)
        {
            messageControl._isChannelMessage = false;

            messageControl.SetupBinding(newMessage);

            var messageCommon = newMessage as TLMessageCommon;
            if (messageCommon != null)
            {
                var showAsService = IsServiceMessage(messageCommon);
                if (!showAsService)
                {
                    messageControl.ToMessageTemplate(messageCommon);
                }
                else
                {
                    messageControl.ToServiceTemplate(messageCommon);
                }
            }
            else
            {
                messageControl.ToEmptyTemplate();
            }
        }

        private void ToMessageTemplate(TLMessageCommon message)
        {
            DrawBubble(message);
        }

        private static bool GetIsChannelMessage(TLMessageCommon messageCommon)
        {
            var message = messageCommon as TLMessage;
            if (message != null)
            {
                var message40 = message as TLMessage40;
                if (message40 != null)
                {
                    if (message40.FromId == null || message40.FromId.Value < 0) return true;

                    if (message40.ToId is TLPeerChannel) // with signatures
                    {
                        var channel = IoC.Get<ICacheService>().GetChat(message40.ToId.Id) as TLChannel;
                        if (channel != null && !channel.IsMegaGroup)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsDocument(TLMessage message)
        {
            if (message == null) return false;

            var mediaDocument = message.Media as TLMessageMediaDocument45;
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

        private static bool IsServiceMessage(TLMessageBase messageBase)
        {
            var messageService = messageBase as TLMessageService;
            var phoneCall = messageService != null && messageService.Action is TLMessageActionPhoneCall;
            var mediaExpired = messageBase != null && messageBase.IsExpired();
            var serviceMessage = (messageService != null && !phoneCall) || mediaExpired;

            return serviceMessage;
        }

        private static bool IsSelfInputMessage(TLMessageCommon messageCommon)
        {
            var message = messageCommon as TLMessage73;
            if (message != null && message.IsSelf())
            {
                var fwdHeader = message.FwdHeader as TLMessageFwdHeader73;
                if (fwdHeader != null)
                {
                    var user = fwdHeader.From as TLUser;
                    if (user == null || !user.IsSelf)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void DrawBubble(TLMessageCommon messageCommon)
        {
            //System.Diagnostics.Debug.WriteLine("DrawBubble id=" + messageCommon.Index);

            var isLightTheme = (Visibility) Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var message = messageCommon as TLMessage48;
            var messageService = messageCommon as TLMessageService;
            var showAsServiceMessage = IsServiceMessage(messageCommon);
            var unreadSeparator = messageService != null && messageService.Action is TLMessageActionUnreadMessages;
            var sticker = message != null && message.IsSticker();
            var photo = message != null && message.Media is TLMessageMediaPhoto;
            var grouped = message != null && message.Media is TLMessageMediaGroup;
            var roundVideo = message != null && message.IsRoundVideo();

            _isChannelMessage = GetIsChannelMessage(messageCommon);
            var output = GetOutput(messageCommon);

            FromLabel.Visibility = (message != null && !output && (message.ToId is TLPeerChannel || message.ToId is TLPeerChat || message.ToId is TLPeerBroadcast) && !sticker && !photo && !grouped) || IsSelfInputMessage(message)
                ? Visibility.Visible 
                : Visibility.Collapsed;

            Tile.Visibility = (message != null && !output && (message.ToId is TLPeerChannel || message.ToId is TLPeerChat || message.ToId is TLPeerBroadcast) && !_isChannelMessage) || IsSelfInputMessage(message)
                ? Visibility.Visible
                : Visibility.Collapsed;

            SetBackgroundAndBorder(messageCommon);

            Brush foreground;
            if (showAsServiceMessage)
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

            var beforeLastGridLength = output && !showAsServiceMessage
                ? new GridLength(1.0, GridUnitType.Star)
                : GridLength.Auto;

            var lastGridLength = output && !showAsServiceMessage
                ? GridLength.Auto
                : new GridLength(1.0, GridUnitType.Star);

            var bubbleGridColumn = output && !showAsServiceMessage
                ? 4
                : 3;

            var cornerGridColumn = output && !showAsServiceMessage
                ? 5
                : 2;

            var commandsGridColumn = bubbleGridColumn;

            Corner.Margin = output
                ? new Thickness(-1.0, 12.0, 0.0, 0.0)
                : new Thickness(0.0, 12.0, -1.0, 0.0);
            Corner.HorizontalAlignment = output
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right;
            PathScaleTransform.ScaleX = output ? -1.0 : 1.0;
            Corner.Visibility = showAsServiceMessage ? Visibility.Collapsed : Visibility.Visible;
            CornerBorder.Visibility = showAsServiceMessage ? Visibility.Collapsed : Visibility.Visible;
            if (unreadSeparator)
            {
                MainBorder.Margin = new Thickness(-18.0, 0.0, -18.0, 6.0);
                MainBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
                Panel.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else if (showAsServiceMessage)
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
            Grid.SetColumnSpan(MainBorder, showAsServiceMessage ? 2 : 1);
            InputMessage.TextAlignment = showAsServiceMessage ? TextAlignment.Center : TextAlignment.Left;            

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
            FwdFromGrid.Visibility = message != null && !message.IsSticker() && !message.IsRoundVideo() && !message.IsSelf() ? message.FwdFromPeerVisibility : Visibility.Collapsed;

            ViaBotGrid.Visibility = message != null ? message.ViaBotVisibility : Visibility.Collapsed;
            ReplyContent.Visibility = messageCommon != null ? messageCommon.ReplyVisibility : Visibility.Collapsed;
            if (FromLabel.Visibility == Visibility.Visible
                || FwdFromGrid.Visibility == Visibility.Visible
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
                && FwdFromGrid.Visibility == Visibility.Collapsed
                && ViaBotGrid.Visibility == Visibility.Collapsed
                && ReplyContent.Visibility == Visibility.Collapsed
                && message != null 
                && !TLString.IsNullOrEmpty(message.Message))
            {
                FromLabel.Margin = new Thickness(0.0, 2.0, 0.0, -4.0);
            }
            else if (FromLabel.Visibility == Visibility.Visible
                && FwdFromGrid.Visibility == Visibility.Collapsed
                && ViaBotGrid.Visibility == Visibility.Collapsed
                && messageCommon != null && messageCommon.ReplyInfo != null)
            {
                FromLabel.Margin = new Thickness(0.0, 2.0, 0.0, 6.0);
            }
            else
            {
                FromLabel.Margin = new Thickness(0.0, 2.0, 0.0, 0.0);
            }

            if (FromLabel.Visibility == Visibility.Collapsed
                && FwdFromGrid.Visibility == Visibility.Collapsed
                && ViaBotGrid.Visibility == Visibility.Collapsed
                && ReplyContent.Visibility == Visibility.Visible
                && messageCommon != null && messageCommon.ReplyInfo != null)
            {
                ReplyContent.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
            }
            else
            {
                ReplyContent.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
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

            AuthorLabel.Visibility = _isChannelMessage && message != null? message.AuthorVisibility : Visibility.Collapsed;
            ViewsGrid.Visibility = message != null ? message.ViewsVisibility : Visibility.Collapsed;

            Grid.SetColumn(Commands, commandsGridColumn);
            
            ShereButtonImage.Source = isLightTheme
                ? new BitmapImage(new Uri("/Images/Messages/channel.share.white.png", UriKind.Relative))
                : new BitmapImage(new Uri("/Images/Messages/channel.share.white.png", UriKind.Relative));

            ShareButton.Visibility = message != null
                && ((!sticker && message.ToId is TLPeerChannel && _isChannelMessage) || (message.Media is TLMessageMediaInvoice && !message.Out.Value) || IsSelfInputMessage(message))
                ? Visibility.Visible
                : Visibility.Collapsed;

            Status.Visibility = output? Visibility.Visible : Visibility.Collapsed;

            MessageGrid.MaxWidth = messageCommon != null ? messageCommon.MediaWidth : 12.0 + 311.0 + 12.0;

            Panel.Children.Remove(Header);
            MainItemGrid.Children.Remove(Header);
            if (showAsServiceMessage)
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

        private void SetBackgroundAndBorder(TLMessageCommon messageCommon)
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var message = messageCommon as TLMessage48;
            var messageService = messageCommon as TLMessageService;
            var phoneCall = messageService != null && messageService.Action is TLMessageActionPhoneCall;
            var mediaExpired = messageCommon != null && messageCommon.IsExpired();
            var serviceMessage = (messageService != null && !phoneCall) || mediaExpired;
            var sticker = messageCommon != null && messageCommon.IsSticker();
            var roundVideo = messageCommon != null && messageCommon.IsRoundVideo();
            var output = GetOutput(messageCommon);

            Brush border;
            if (serviceMessage)
            {
                border = (Brush)Resources["ServiceMessageBorderBrush"];
            }
            else if (sticker)
            {
                border = new SolidColorBrush(Colors.Transparent);
            }
            else if (roundVideo)
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
            if (serviceMessage)
            {
                background = (Brush)Resources["ServiceMessageBackgroundBrush"];
            }
            else if (sticker)
            {
                background = new SolidColorBrush(Colors.Transparent);
            }
            else if (roundVideo)
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

        private void ToServiceTemplate(TLMessageCommon messageCommon)
        {
            _isChannelMessage = false;
            FromLabel.Visibility = Visibility.Collapsed;
            Tile.Visibility = Visibility.Collapsed;

            DrawBubble(messageCommon);
        }

        private Binding _authorBinding;
        private Binding _viewsBinding;
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

        private void SetupBinding(TLMessageBase messageBase)
        {
            // exceptions:
            // 1) message with TTL and Document=null, Photo=null is service message (video has expired, photo has expired)
            // 2) service message with phone call is regular message

            var serviceMessage = messageBase as TLMessageService;
            var message = messageBase as TLMessage;

            if (serviceMessage != null)
            {
                SaveBinding(ref _inputMessageEntitiesBinding, InputMessage, TelegramRichTextBox.EntitiesProperty);
                SaveBinding(ref _inputMessageTextBinding, InputMessage, TelegramRichTextBox.TextProperty);
                SaveBinding(ref _commandsReplyMarkupBinding, Commands, CommandsControl.ReplyMarkupProperty);

                var isPhoneCall = serviceMessage.Action is TLMessageActionPhoneCall;
                if (!isPhoneCall)
                {
                    var serviceMessageToTextConverter = new ServiceMessageToTextConverter();
                    InputMessage.Text = (string)serviceMessageToTextConverter.Convert(serviceMessage.Self, null, "", null);
                }

                ClearValue(MediaProperty);
            }
            else if (message != null && message.IsExpired())   // will be set manually to service message 'video has expired'/'photo has expired'
            {
                SaveBinding(ref _inputMessageEntitiesBinding, InputMessage, TelegramRichTextBox.EntitiesProperty);
                SaveBinding(ref _inputMessageTextBinding, InputMessage, TelegramRichTextBox.TextProperty);
                RestoreBinding(ref _commandsReplyMarkupBinding, Commands, CommandsControl.ReplyMarkupProperty);

                var mediaPhoto = message.Media as TLMessageMediaPhoto70;
                var mediaDocument = message.Media as TLMessageMediaDocument70;
                if (mediaPhoto != null)
                {
                    InputMessage.Text = AppResources.MessageActionPhotoExpired;
                }
                else if (mediaDocument != null)
                {
                    InputMessage.Text = AppResources.MessageActionVideoExpired;
                }

                _suppressFooterReplacement = true;
                SetBinding(MediaProperty, new Binding("Media") { Source = messageBase, Mode = BindingMode.OneWay }); // Media field of type TLMessageService for phone calls is not acceptable for dependency property of type TLMessageMediaBase
                _suppressFooterReplacement = false;
            }
            else
            {
                RestoreBinding(ref _inputMessageEntitiesBinding, InputMessage, TelegramRichTextBox.EntitiesProperty); // there is no Entities field on TLMessageService with phone call
                RestoreBinding(ref _inputMessageTextBinding, InputMessage, TelegramRichTextBox.TextProperty);         // there is no Message field on TLMessageService with phone call
                RestoreBinding(ref _commandsReplyMarkupBinding, Commands, CommandsControl.ReplyMarkupProperty);      // there is no ReplyMarkup field on TLMessageService with phone call

                _suppressFooterReplacement = true;
                SetBinding(MediaProperty, new Binding("Media") { Source = messageBase, Mode = BindingMode.OneWay }); // Media field of type TLMessageService for phone calls is not acceptable for dependency property of type TLMessageMediaBase
                _suppressFooterReplacement = false;
            }

            var showAsServiceMessage = IsServiceMessage(messageBase);
            if (showAsServiceMessage)
            {
                //SaveBinding(ref _fwdFromLabelTextBinding, FwdFromLabel, Run.TextProperty);
                SaveBinding(ref _inputMessageVisibilityBinding, InputMessage, TelegramRichTextBox.VisibilityProperty);
                SaveBinding(ref _mediaContentControlContentBinding, MediaContentControl, ContentControl.ContentProperty);
                SaveBinding(ref _mediaContentControlContentTemplateBinding, MediaContentControl, ContentControl.ContentTemplateProperty);
                SaveBinding(ref _viewsBinding, ViewsLabel, TextBlock.TextProperty);
                SaveBinding(ref _authorBinding, AuthorLabel, TextBlock.TextProperty);
                SaveBinding(ref _editLabelVisibilityBinding, EditLabel, TextBlock.VisibilityProperty);

                _suppressFooterReplacement = true;
                ClearValue(MediaCaptionProperty);
                _suppressFooterReplacement = false;

                ClearValue(MediaUnreadProperty);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                RestoreBinding(ref _inputMessageVisibilityBinding, InputMessage, TelegramRichTextBox.VisibilityProperty);
                RestoreBinding(ref _mediaContentControlContentBinding, MediaContentControl, ContentControl.ContentProperty);
                RestoreBinding(ref _mediaContentControlContentTemplateBinding, MediaContentControl, ContentControl.ContentTemplateProperty);
                RestoreBinding(ref _viewsBinding, ViewsLabel, TextBlock.TextProperty);
                RestoreBinding(ref _authorBinding, AuthorLabel, TextBlock.TextProperty);
                RestoreBinding(ref _editLabelVisibilityBinding, EditLabel, TextBlock.VisibilityProperty);

                _suppressFooterReplacement = true;
                if (message != null)
                {
                    var mediaCaption = message.Media as IMediaCaption;
                    if (mediaCaption != null)
                    {
                        var mediaCaptionBinding = new Binding("Message") { Source = message, Mode = BindingMode.OneWay };
                        SetBinding(MediaCaptionProperty, mediaCaptionBinding);
                    }
                }
                _suppressFooterReplacement = false;

                var message70 = messageBase as TLMessage70;
                if (message70 != null && message70.HasTTL())
                {
                    var notListenedBinding = new Binding("TTLMediaExpired") { Source = message, Mode = BindingMode.OneWay };
                    SetBinding(MediaUnreadProperty, notListenedBinding);
                }
            }
        }

        private bool _suppressFooterReplacement;

        public string MediaCaption
        {
            get { return (string)GetValue(MediaCaptionProperty); }
            set { SetValue(MediaCaptionProperty, value); }
        }

        public static readonly DependencyProperty MediaCaptionProperty = DependencyProperty.Register(
            "MediaCaption", typeof (string), typeof (MessageControl), new PropertyMetadata(default(string), OnMediaCaptionChanged));

        private static void OnMediaCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as MessageControl;
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

        public static readonly DependencyProperty MediaUnreadProperty = DependencyProperty.Register(
            "MediaUnread", typeof(bool), typeof(MessageControl), new PropertyMetadata(default(bool), OnMediaUnreadChanged));

        private static void OnMediaUnreadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as MessageControl;
            if (messageControl != null)
            {
                var message = messageControl.Message as TLMessage;
                if (message != null && message.IsExpired())
                {
                    var oldMediaUnread = (bool) e.OldValue;
                    var newMediaUnread = (bool) e.NewValue;
                    if (!oldMediaUnread && newMediaUnread)
                    {
                        OnMessageChangedInternal(messageControl, messageControl.Message);
                    }
                }
            }
        }

        public bool MediaUnread
        {
            get { return (bool) GetValue(MediaUnreadProperty); }
            set { SetValue(MediaUnreadProperty, value); }
        }

        protected TLMessageMediaBase Media
        {
            get { return (TLMessageMediaBase)GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }

        public static readonly DependencyProperty MediaProperty = DependencyProperty.Register(
            "Media", typeof (TLMessageMediaBase), typeof (MessageControl), new PropertyMetadata(default(TLMessageMediaBase), OnMediaChanged));

        private static void OnMediaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageControl = d as MessageControl;
            if (messageControl != null)
            {
                if (!messageControl._suppressFooterReplacement)
                {
                    var oldWebPage = e.OldValue as TLMessageMediaWebPage;
                    var newWebPage = e.NewValue as TLMessageMediaWebPage;
                    if (oldWebPage != null
                        || newWebPage != null)
                    {
                        messageControl.SetFooter(messageControl.Message);
                    }
                }
            }
        }

        private bool GetOutput(TLMessageCommon message)
        {
            if (IsSelfInputMessage(message))
            {
                return false;
            }

            return message != null && message.Out.Value && !_isChannelMessage;
        }

        private void SetFooter(TLMessageBase messageBase)
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

            var messageCommon = messageBase as TLMessageCommon;
            var message = messageBase as TLMessage48;
            var service = messageBase as TLMessageService;
            var phoneCall = service != null && service.Action is TLMessageActionPhoneCall;
            var showAsServiceMessage = IsServiceMessage(messageBase);
            var output = GetOutput(messageCommon);

            var hasCaption = message != null && !TLString.IsNullOrEmpty(message.Message);
            var isGroupedMedia = message != null && message.Media is TLMessageMediaGroup;
            var isPhoto = message != null && message.Media is TLMessageMediaPhoto;
            var isVideo = message != null && message.IsVideo();
            var isGeo = message != null && message.Media is TLMessageMediaGeo && !(message.Media is TLMessageMediaGeoLive);
            var isVenue = message != null && message.Media is TLMessageMediaVenue;
            var isGeoLive = message != null && message.Media is TLMessageMediaGeoLive;
            var isGif = message != null && message.IsGif() && !(message.Media is TLMessageMediaGame) && !(message.Media is TLMessageMediaWebPage);
            var isDocument = message != null && IsDocument(message);
            var isVoice = message != null && message.IsVoice();
            var isRoundVideo = message != null && message.IsRoundVideo();
            var isSticker = message != null && message.IsSticker();
            var isWebPage = message != null && message.Media is TLMessageMediaWebPage;
            var isEmptyMedia = message != null && (message.Media == null || message.Media is TLMessageMediaEmpty);
            var isUnsupported = message != null && message.Media is TLMessageMediaUnsupported;

            //var isGif = message != null && message.IsGif(); 
            var isShortFooter = IsShortFooter(message, isGroupedMedia, isPhoto, isVideo, isGeo, isGeoLive, isGif, isDocument, isVoice, isSticker, isWebPage, isEmptyMedia, isUnsupported);

            Brush background;
            if (showAsServiceMessage)
            {
                background = (Brush)Resources["ServiceMessageBackgroundBrush"];
            }
            if (isSticker)
            {
                background = new SolidColorBrush(Colors.Transparent);
            }
            if (isRoundVideo)
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

            Brush footerForeground;
            if (isSticker)
            {
                footerForeground = isLightTheme
                    ? (Brush)Resources["StickerFooterSubtleBrushLight"]
                    : (Brush)Resources["StickerFooterSubtleBrushDark"];
            }
            else if (isRoundVideo)
            {
                footerForeground = isLightTheme
                    ? (Brush)Resources["StickerFooterSubtleBrushLight"]
                    : (Brush)Resources["StickerFooterSubtleBrushDark"];
            }
            else if (isShortFooter && !hasCaption && (isGroupedMedia || isPhoto || (isGeo && !isVenue) || isVideo || isGif))
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
            else if (isRoundVideo)
            {
                footerBackground = isLightTheme
                    ? (Brush)Resources["StickerFooterBackgroundBrushLight"]
                    : (Brush)Resources["StickerFooterBackgroundBrushDark"];
            }
            else if (isShortFooter && !hasCaption && (isGroupedMedia || isPhoto || isVideo || (isGeo && !isVenue) || isGif))
            {
                footerBackground = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
            }
            else
            {
                footerBackground = new SolidColorBrush(Colors.Transparent);
            }

            FooterContent.Foreground = footerForeground;
            FooterContentGrid.Background = footerBackground;
            Footer.MaxWidth = messageCommon != null ? messageCommon.MediaWidth : 12.0 + 311.0 + 12.0;
            Status.Fill = footerForeground;
            ViewsIcon.Stroke = footerForeground;

            if (messageCommon != null && messageCommon.Reply != null && (isGroupedMedia || isPhoto || isVideo || isGeo || isGif))
            {
                MediaContentControl.Margin = new Thickness(12.0, 3.0, 12.0, 0.0);
            }
            else
            {
                MediaContentControl.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
            }

            // setup message and media position
            MessageGrid.Visibility = message != null && !TLString.IsNullOrEmpty(message.Message) || showAsServiceMessage
                ? Visibility.Visible
                : Visibility.Collapsed;
            Panel.Children.Remove(MessageGrid);
            Panel.Children.Remove(MediaGrid);
            if (message != null && (message.Media is TLMessageMediaWebPage || message.Media is TLMessageMediaEmpty) || showAsServiceMessage)
            {
                MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
                MediaGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);

                Panel.Children.Add(MessageGrid);
                Panel.Children.Add(MediaGrid);
            }
            else
            {
                MessageGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
                MediaGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);

                Panel.Children.Add(MediaGrid);
                Panel.Children.Add(MessageGrid);
            }

            // setup footer position
            Panel.Children.Remove(Footer);
            MediaGrid.Children.Remove(Footer);
            MessageGrid.Children.Remove(Footer);
            if (showAsServiceMessage)
            {
                // remove footer
                MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 6.0);
            }
            else if (phoneCall)
            {
                MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
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
                else if (isEmptyMedia || hasCaption)
                {
                    Footer.Margin = new Thickness(0.0, -1.0, 0.0, -11.0);
                    Footer.Background = new SolidColorBrush(Colors.Transparent);
                    Footer.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Footer.VerticalAlignment = VerticalAlignment.Bottom;
                    MessageGrid.Children.Add(Footer);
                    MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 11.0);
                }
                else if (isGeoLive)
                {
                    Footer.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
                    Footer.Background = new SolidColorBrush(Colors.Transparent);
                    Footer.HorizontalAlignment = HorizontalAlignment.Right;
                    Footer.VerticalAlignment = VerticalAlignment.Bottom;
                    //MediaGrid.Children.Add(Footer);
                    MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
                }
                else if (isGroupedMedia || isPhoto || isVideo || isGeo || isGif || isVoice || isDocument)
                {
                    Footer.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
                    Footer.Background = new SolidColorBrush(Colors.Transparent);
                    Footer.HorizontalAlignment = HorizontalAlignment.Right;
                    Footer.VerticalAlignment = VerticalAlignment.Bottom;
                    MediaGrid.Children.Add(Footer);
                    MessageGrid.Margin = new Thickness(0.0, 6.0, 0.0, 11.0);
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
                && !TLString.IsNullOrEmpty(message.Message)
                && !(message.Media is TLMessageMediaWebPage))
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
                if (_isChannelMessage
                    && message.AuthorVisibility == Visibility.Visible
                    && message.Author != null)
                {
                    footerBuilder.Append(string.Format("{0} ", message.Author));
                }
                if (message.ViewsVisibility == Visibility.Visible)
                {
                    footerBuilder.Append(string.Format("vc {0}, ",
                        new MessageViewsConverter().Convert(message.Views, null, null, null)));
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

        private static bool IsShortFooter(TLMessage48 message, bool isGrouped, bool isPhoto, bool isVideo, bool isGeo, bool isGeoLive, bool isGif, bool isDocument, bool isVoice, bool isSticker, bool isWebPage, bool isEmptyMedia, bool isUnsupported)
        {
            if (message != null)
            {
                if (isUnsupported)
                {
                    return false;
                }

                if (isWebPage)
                {
                    return false;
                }

                if (isGrouped || isPhoto || isVideo || isGeo || isGif)
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
                
                if (isDocument || isVoice || isGeoLive)
                {
                    var clientDelta = IoC.Get<IMTProtoService>().ClientTicksDelta;
                    //var utc0SecsLong = message.Date.Value * 4294967296 - clientDelta;
                    var utc0SecsInt = message.Date.Value - clientDelta / 4294967296.0;

                    var dateTime = Telegram.Api.Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);
                    if (dateTime.Date.AddDays(365) < DateTime.Now.Date)
                        return false;

                    if (message.AuthorVisibility == Visibility.Visible
                        && message.ToId is TLPeerChannel)
                    {
                        var channel = IoC.Get<ICacheService>().GetChat(message.ToId.Id) as TLChannel;
                        if (channel != null && !channel.IsMegaGroup)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private void ToEmptyTemplate()
        {
            
        }

        public TLMessageBase Message
        {
            get { return (TLMessageBase) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        private static int _count;

        public MessageControl()
        {
            InitializeComponent();

            //System.Diagnostics.Debug.WriteLine("MessageControl " + _count++);
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

        private void ShareButton_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            RaiseShareButtonClick(sender, e);
        }

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

    public class NonBreakingStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        public static object Convert(object value)
        {
            var str = value as string;
            if (str != null)
            {
                return str.Replace(' ', '\xA0').Replace("-", "\u2011");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
