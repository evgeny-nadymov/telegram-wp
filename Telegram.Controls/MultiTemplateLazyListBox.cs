// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;

namespace Telegram.Controls
{
    public class MultiTemplateLazyListBox : LazyListBox
    {

        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register("TemplateSelector",
            typeof(ITemplateSelector), typeof(MultiTemplateLazyListBox),
            new PropertyMetadata(OnTemplateChanged));

        public ITemplateSelector TemplateSelector
        {
            get { return (ITemplateSelector)GetValue(TemplateSelectorProperty); }
            set { SetValue(TemplateSelectorProperty, value); }
        }

        private static void OnTemplateChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {

        }

        protected override void PrepareContainerForItemOverride(
            DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var listBoxItem = element as ListBoxItem;

            if (listBoxItem != null)
            {
                listBoxItem.ContentTemplate = TemplateSelector.SelectTemplate(item, this);
            }
        }
    }
}
