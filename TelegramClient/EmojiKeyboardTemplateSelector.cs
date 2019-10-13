using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TelegramClient.Views.Dialogs;

namespace TelegramClient
{
    public class EmojiKeyboardTemplateSelector : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty EmojiKeyboardTemplateProperty =
            DependencyProperty.Register("EmojiKeyboardTemplate", typeof (DataTemplate), typeof (EmojiKeyboardTemplateSelector), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate EmojiKeyboardTemplate
        {
            get { return (DataTemplate) GetValue(EmojiKeyboardTemplateProperty); }
            set { SetValue(EmojiKeyboardTemplateProperty, value); }
        }

        public static readonly DependencyProperty EmptyTemplateProperty =
            DependencyProperty.Register("EmptyTemplate", typeof (DataTemplate), typeof (EmojiKeyboardTemplateSelector), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate EmptyTemplate
        {
            get { return (DataTemplate) GetValue(EmptyTemplateProperty); }
            set { SetValue(EmptyTemplateProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is EmojiKeyboard ? EmojiKeyboardTemplate : EmptyTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}