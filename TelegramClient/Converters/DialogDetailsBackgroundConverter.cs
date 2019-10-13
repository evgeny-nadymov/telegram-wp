// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Converters
{
    public class DialogDetailsBackgroundConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ImageTemplateProperty = DependencyProperty.Register(
            "ImageTemplate", typeof (DataTemplate), typeof (DialogDetailsBackgroundConverter), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ImageTemplate
        {
            get { return (DataTemplate) GetValue(ImageTemplateProperty); }
            set { SetValue(ImageTemplateProperty, value); }
        }

        public static readonly DependencyProperty AnimatedTemplateProperty = DependencyProperty.Register(
            "AnimatedTemplate", typeof (DataTemplate), typeof (DialogDetailsBackgroundConverter), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate AnimatedTemplate
        {
            get { return (DataTemplate) GetValue(AnimatedTemplateProperty); }
            set { SetValue(AnimatedTemplateProperty, value); }
        }

        public static readonly DependencyProperty EmptyTemplateProperty = DependencyProperty.Register(
            "EmptyTemplate", typeof (DataTemplate), typeof (DialogDetailsBackgroundConverter), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate EmptyTemplate
        {
            get { return (DataTemplate) GetValue(EmptyTemplateProperty); }
            set { SetValue(EmptyTemplateProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //System.Diagnostics.Debug.WriteLine("DialogDetailsBackgroundConverter elapsed=" + ShellViewModel.Timer.Elapsed);
            var background = value as BackgroundItem;
            if (background == null || background.Name == "Empty")
            {
                return ImageTemplate;
            }

            if (background.Name == Constants.AnimatedBackground1String)
            {
                return AnimatedTemplate;
            }

            return ImageTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
