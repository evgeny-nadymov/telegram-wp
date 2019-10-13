// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telegram.Api.TL;
using TelegramClient.Converters;

namespace TelegramClient.Views.Controls
{
    public partial class GroupedMessageControl
    {
        //~GroupedMessageControl()
        //{
            
        //}

        public static readonly DependencyProperty MediaProperty = DependencyProperty.Register(
            "Media", typeof(IMessageMediaGroup), typeof(GroupedMessageControl), new PropertyMetadata(default(IMessageMediaGroup), OnMediaChanged));

        public IMessageMediaGroup Media
        {
            get { return (IMessageMediaGroup)GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }

        private static void OnMediaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as GroupedMessageControl;
            if (control != null)
            {
                var oldMedia = e.OldValue as IMessageMediaGroup;
                if (oldMedia != null)
                {
                    oldMedia.Calculate -= control.OnMediaCalculate;
                }

                control.UpdatePhotoLayout(e.NewValue as IMessageMediaGroup);

                var newMedia = e.NewValue as IMessageMediaGroup;
                if (newMedia != null)
                {
                    newMedia.Calculate += control.OnMediaCalculate;
                }
            }
        }

        private void OnMediaCalculate(object sender, System.EventArgs e)
        {
            UpdatePhotoLayout(Media as IMessageMediaGroup);
        }

        private StackPanel _previousPanel;

        private void UpdatePhotoLayout(IMessageMediaGroup media)
        {
            if (LayoutRoot.Children.Count > 0)
            {
                _previousPanel = LayoutRoot.Children.FirstOrDefault() as StackPanel;
            }
            LayoutRoot.Children.Clear();

            if (media != null && media.GroupCommon != null && media.GroupCommon.Count > 0)
            {
                var groupedMessages = new GroupedMessages();
                foreach (var messageBase in media.GroupCommon)
                {
                    var message = messageBase as TLMessage;
                    if (message != null)
                    {
                        if (message.IsExpired())
                        {
                            continue;
                        }

                        groupedMessages.Messages.Add(message);
                    }
                    var decryptedMessage = messageBase as TLDecryptedMessage;
                    if (decryptedMessage != null)
                    {
                        //if (decryptedMessage.IsExpired())
                        //{
                        //    continue;
                        //}

                        groupedMessages.Messages.Add(decryptedMessage);
                    }
                }

                if (groupedMessages.Messages.Count == 0)
                {
                    
                }
                else if (groupedMessages.Messages.Count == 1)
                {
                    var decryptedMessage = groupedMessages.Messages[0] as TLDecryptedMessage;
                    if (decryptedMessage != null)
                    {
                        var mediaPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                        if (mediaPhoto != null)
                        {
                            var stackPanel = new StackPanel { Background = new SolidColorBrush(Colors.Transparent) };

                            var converter = new PhotoToDimensionConverter();
                            var width = (double)converter.Convert(mediaPhoto, null, "Width", null);
                            var height = (double)converter.Convert(mediaPhoto, null, "Height", null);

                            var groupedMessagePosition = new GroupedMessagePosition();
                            groupedMessagePosition.Set(0, 0, 0, 0, (int)width, 1.0f, 0);
                            var position = new KeyValuePair<TLObject, GroupedMessagePosition>(groupedMessages.Messages[0], groupedMessagePosition);

                            var border = GetControl(_previousPanel, position, 1.0, height, 0.0, 0.0);

                            stackPanel.Children.Add(border);

                            if (stackPanel.Children.Count > 0)
                            {
                                LayoutRoot.Children.Add(stackPanel);
                            }
                        }

                        if (decryptedMessage.IsVideo())
                        {
                            var stackPanel = new StackPanel { Background = new SolidColorBrush(Colors.Transparent) };

                            var width = 230.0;
                            var height = 145.0;

                            var groupedMessagePosition = new GroupedMessagePosition();
                            groupedMessagePosition.Set(0, 0, 0, 0, (int)width, 1.0f, 0);
                            var position = new KeyValuePair<TLObject, GroupedMessagePosition>(groupedMessages.Messages[0], groupedMessagePosition);

                            var border = GetControl(_previousPanel, position, 1.0, height, 0.0, 0.0);

                            stackPanel.Children.Add(border);

                            if (stackPanel.Children.Count > 0)
                            {
                                LayoutRoot.Children.Add(stackPanel);
                            }
                        }
                    }

                    var message = groupedMessages.Messages[0] as TLMessage;
                    if (message != null)
                    {
                        var mediaPhoto = message.Media as TLMessageMediaPhoto;
                        if (mediaPhoto != null)
                        {
                            var stackPanel = new StackPanel { Background = new SolidColorBrush(Colors.Transparent) };

                            var converter = new PhotoToDimensionConverter();
                            var width = (double)converter.Convert(mediaPhoto, null, "Width", null);
                            var height = (double)converter.Convert(mediaPhoto, null, "Height", null);

                            var groupedMessagePosition = new GroupedMessagePosition();
                            groupedMessagePosition.Set(0, 0, 0, 0, (int)width, 1.0f, 0);
                            var position = new KeyValuePair<TLObject, GroupedMessagePosition>(groupedMessages.Messages[0], groupedMessagePosition);

                            var border = GetControl(_previousPanel, position, 1.0, height, 0.0, 0.0);

                            stackPanel.Children.Add(border);

                            if (stackPanel.Children.Count > 0)
                            {
                                LayoutRoot.Children.Add(stackPanel);
                            }
                        }

                        var mediaDocument = message.Media as TLMessageMediaDocument;
                        if (mediaDocument != null)
                        {
                            var stackPanel = new StackPanel { Background = new SolidColorBrush(Colors.Transparent) };

                            var width = 230.0;
                            var height = 145.0;

                            var groupedMessagePosition = new GroupedMessagePosition();
                            groupedMessagePosition.Set(0, 0, 0, 0, (int)width, 1.0f, 0);
                            var position = new KeyValuePair<TLObject, GroupedMessagePosition>(groupedMessages.Messages[0], groupedMessagePosition);

                            var border = GetControl(_previousPanel, position, 1.0, height, 0.0, 0.0);

                            stackPanel.Children.Add(border);

                            if (stackPanel.Children.Count > 0)
                            {
                                LayoutRoot.Children.Add(stackPanel);
                            }
                        }
                    }
                }
                else if (groupedMessages.Messages.Count > 0)
                {
                    var height = 320;
                    groupedMessages.Calculate(height);

                    var stackPanel = new StackPanel { Background = new SolidColorBrush(Colors.Transparent) };

                    var positions = groupedMessages.Positions.ToList();

                    for (var i = 0; i < positions.Count; i++)
                    {
                        var position = positions[i];

                        var top = 0.0;
                        var left = 0.0;

                        if (i > 0)
                        {
                            var pos = positions[i - 1];
                            // in one row
                            if (pos.Value.MinY == position.Value.MinY)
                            {
                                top = -(height * pos.Value.Height);

                                for (var j = i - 1; j >= 0; j--)
                                {
                                    pos = positions[j];
                                    if (pos.Value.MinY == position.Value.MinY)
                                    {
                                        left += pos.Value.Width;
                                    }
                                }
                            }
                            // in one column
                            else if (position.Value.SpanSize == groupedMessages.MaxSizeWidth)
                            {
                                left = position.Value.LeftSpanOffset;
                                // find common big message
                                KeyValuePair<TLObject, GroupedMessagePosition>? leftColumn = null;
                                for (var j = i - 1; j >= 0; j--)
                                {
                                    pos = positions[j];
                                    if (pos.Value.SiblingHeights != null)
                                    {
                                        leftColumn = pos;
                                        break;
                                    }
                                    else
                                    {
                                        top += (height * pos.Value.Height);
                                    }
                                }
                                // set top
                                if (leftColumn != null)
                                {
                                    top -= (leftColumn.Value.Value.Height * height);
                                }
                                else
                                {
                                    top = 0;
                                }
                            }
                        }

                        var border = GetControl(_previousPanel, position, 0.65, height, left, top);

                        stackPanel.Children.Add(border);
                    }

                    if (stackPanel.Children.Count > 0)
                    {
                        LayoutRoot.Children.Add(stackPanel);
                    }
                }
            }
        }

        private static bool IsVideo(TLObject obj)
        {
            var message = obj as TLMessage;
            if (message != null && message.IsVideo())
            {
                return true;
            }

            var decryptedMessage = obj as TLDecryptedMessage;
            if (decryptedMessage != null && decryptedMessage.IsVideo())
            {
                return true;
            }

            return false;
        }

        private static UIElement GetControl(StackPanel previousPanel, KeyValuePair<TLObject, GroupedMessagePosition> position, double scale, double height, double left, double top)
        {
            if (IsVideo(position.Key))
            {
                return GetVideoControl(previousPanel, position, scale, height, left, top);
            }

            return GetPhotoControl(previousPanel, position, scale, height, left, top);
        }

        private static UIElement GetVideoControl(StackPanel previousPanel, KeyValuePair<TLObject, GroupedMessagePosition> position, double scale, double height, double left, double top)
        {
            TLObject media = null;
            var message = position.Key as TLMessage;
            if (message != null)
            {
                media = message.Media;
            }
            var decryptedMessage = position.Key as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                media = decryptedMessage.Media;
            }

            var element = GetCachedControl<MediaVideoControl>(previousPanel, media);
            var created = false;
            if (element == null)
            {
                created = true;
                element = new MediaVideoControl();
                element.DataContext = media;
            }

            var margin = 1.0;

            element.Stretch = Stretch.UniformToFill;
            element.ProgressScale = scale;
            element.HorizontalAlignment = HorizontalAlignment.Left;
            element.VerticalAlignment = VerticalAlignment.Top;
            element.Width = position.Value.Width - 2.0 * margin;
            element.Height = height * position.Value.Height - 2 * margin;
            element.Margin = new Thickness(left + margin, top + margin, 0.0 + margin, 0.0 + margin);

            if (!created) return element;

            Caliburn.Micro.Message.SetAttach(element, "[Event CancelUploading] = [Action CancelUploading($DataContext)]; [Event CancelDownloading] = [Action CancelVideoDownloading($DataContext)]");

            var mediaBinding = new Binding("");
            element.SetBinding(MediaVideoControl.MediaProperty, mediaBinding);

            if (message != null && message.HasTTL()
                || decryptedMessage != null && decryptedMessage.TTL.Value > 0)
            {
                var previewSourceBinding = new Binding("ThumbSelf");
                previewSourceBinding.Converter = new PhotoToThumbConverter { Secret = true };
                element.SetBinding(MediaVideoControl.SourceProperty, previewSourceBinding);
            }
            else
            {
                var sourceBinding = new Binding("ThumbSelf");
                sourceBinding.Converter = new PhotoToThumbConverter();
                element.SetBinding(MediaVideoControl.SourceProperty, sourceBinding);
            }

            var downloadIconVisibilityBinding = new Binding("Self");
            downloadIconVisibilityBinding.Converter = new DownloadMediaToVisibilityConverter();
            element.SetBinding(MediaVideoControl.DownloadIconVisibilityProperty, downloadIconVisibilityBinding);

            var downloadingProgressBinding = new Binding("DownloadingProgress");
            element.SetBinding(MediaVideoControl.DownloadingProgressProperty, downloadingProgressBinding);

            var uploadingProgressBinding = new Binding("UploadingProgress");
            element.SetBinding(MediaVideoControl.UploadingProgressProperty, uploadingProgressBinding);

            var isSelectedBinding = new Binding("IsSelected") { Source = position.Key };
            element.SetBinding(MediaVideoControl.IsSelectedProperty, isSelectedBinding);

            var ttlParamsBinding = new Binding("TTLParams");
            element.SetBinding(MediaVideoControl.TTLParamsProperty, ttlParamsBinding);

            return element;
        }

        private static UIElement GetPhotoControl(StackPanel previousPanel, KeyValuePair<TLObject, GroupedMessagePosition> position, double scale, double height, double left, double top)
        {
            TLObject media = null;
            var message = position.Key as TLMessage;
            if (message != null)
            {
                media = message.Media;
            }
            var decryptedMessage = position.Key as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                media = decryptedMessage.Media;
            }

            var element = GetCachedControl<MediaPhotoControl>(previousPanel, media);
            var created = false;  
            if (element == null)
            {
                created = true;
                element = new MediaPhotoControl();
                element.DataContext = media;
            }

            var margin = 1.0;

            element.ProgressScale = scale;
            element.HorizontalAlignment = HorizontalAlignment.Left;
            element.VerticalAlignment = VerticalAlignment.Top;
            element.Width = position.Value.Width - 2.0 * margin;
            element.Height = height * position.Value.Height - 2 * margin;
            element.Margin = new Thickness(left + margin, top + margin, 0.0 + margin, 0.0 + margin);

            if (!created) return element;

            Caliburn.Micro.Message.SetAttach(element, "[Event CancelUploading] = [Action CancelUploading($DataContext)]; [Event CancelDownloading] = [Action CancelPhotoDownloading($DataContext)]");

            var mediaBinding = new Binding("");
            element.SetBinding(MediaPhotoControl.MediaProperty, mediaBinding);

            if (message != null && message.HasTTL()
                || decryptedMessage != null && decryptedMessage.TTL.Value > 0)
            {
                element.ClearValue(MediaPhotoControl.SourceProperty);
                //var sourceBinding = new Binding("Self");
                //sourceBinding.Converter = new DefaultPhotoConverter();
                //element.SetBinding(MediaPhotoControl.SourceProperty, sourceBinding);

                var previewSourceBinding = new Binding("ThumbSelf");
                previewSourceBinding.Converter = new PhotoToThumbConverter{ Secret = true };
                element.SetBinding(MediaPhotoControl.PreviewSourceProperty, previewSourceBinding);
            }
            else
            {
                var sourceBinding = new Binding("Self");
                sourceBinding.Converter = new DefaultPhotoConverter();
                sourceBinding.ConverterParameter = "311_Background";
                element.SetBinding(MediaPhotoControl.SourceProperty, sourceBinding);

                var previewSourceBinding = new Binding("ThumbSelf");
                previewSourceBinding.Converter = new PhotoToThumbConverter();
                element.SetBinding(MediaPhotoControl.PreviewSourceProperty, previewSourceBinding);
            }

            var downloadingProgressBinding = new Binding("DownloadingProgress");
            element.SetBinding(MediaPhotoControl.DownloadingProgressProperty, downloadingProgressBinding);

            var uploadingProgressBinding = new Binding("UploadingProgress");
            element.SetBinding(MediaPhotoControl.UploadingProgressProperty, uploadingProgressBinding);

            var isSelectedBinding = new Binding("IsSelected") { Source = position.Key };
            element.SetBinding(MediaPhotoControl.IsSelectedProperty, isSelectedBinding);

            var ttlParamsBinding = new Binding("TTLParams");
            element.SetBinding(MediaPhotoControl.TTLParamsProperty, ttlParamsBinding);

            return element;
        }

        private static T GetCachedControl<T>(StackPanel panel, TLObject dataContext) where T : FrameworkElement
        {
            if (panel != null)
            {
                for (var i = 0; i < panel.Children.Count; i++)
                {
                    var control = panel.Children[i] as T;
                    if (control != null && control.DataContext == dataContext)
                    {
                        panel.Children.RemoveAt(i);
                        return control;
                    }
                }
            }

            return null;
        }

        public GroupedMessageControl()
        {
            InitializeComponent();
        }
    }
}
