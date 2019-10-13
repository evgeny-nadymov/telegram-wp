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
using Telegram.Controls.LongListSelector.Common;

namespace Telegram.Controls.LongListSelector
{
    /// <summary>
    /// Represents a ListBox with item-specific templates.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class TemplatedListBox : ListBox
    {
        /// <summary>
        /// Gets or sets the list header template.
        /// </summary>
        public DataTemplate ListHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the list footer template.
        /// </summary>
        public DataTemplate ListFooterTemplate { get; set; }

        /// <summary>
        /// Gets or sets the group header template.
        /// </summary>
        public DataTemplate GroupHeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the footer template.
        /// </summary>
        public DataTemplate GroupFooterTemplate { get; set; }

        /// <summary>
        /// Occurs when an item is about to be "realized".
        /// </summary>
        public event EventHandler<LinkUnlinkEventArgs> Link;
        
        /// <summary>
        /// Occurs when an item is about to be "un-realized".
        /// </summary>
        public event EventHandler<LinkUnlinkEventArgs> Unlink;

        /// <summary>
        /// Creates or identifies the element used to display a specified item.
        /// </summary>
        /// <returns>Returns the new container.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TemplatedListBoxItem();
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">Element used to display the specified item.</param>
        /// <param name="item">Specified item.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            DataTemplate template = null;
            LongListSelectorItem itemTuple = item as LongListSelectorItem;
            
            if (itemTuple != null)
            {
                switch (itemTuple.ItemType)
                {
                    case LongListSelectorItemType.ListHeader:
                        template = this.ListHeaderTemplate;
                        break;
                    case LongListSelectorItemType.ListFooter:
                        template = this.ListFooterTemplate;
                        break;
                    case LongListSelectorItemType.GroupHeader:
                        template = this.GroupHeaderTemplate;
                        break;
                    case LongListSelectorItemType.GroupFooter:
                        template = this.GroupFooterTemplate;
                        break;
                    case LongListSelectorItemType.Item:
                        template = this.ItemTemplate;
                        break;
                }

                TemplatedListBoxItem listBoxItem = (TemplatedListBoxItem)element;
                listBoxItem.Content = itemTuple.Item;
                listBoxItem.Tuple = itemTuple;
                listBoxItem.ContentTemplate = template;

                var result = listBoxItem.GetFirstLogicalChildByType<ContentPresenter>(true);
                var handler = Link;
                if (result != null && handler != null)
                {
                    handler(this, new LinkUnlinkEventArgs(result));
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, undoes the effects of the 
        /// PrepareContainerForItemOverride method.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            LongListSelectorItem itemTuple = item as LongListSelectorItem;

            if (itemTuple != null)
            {
                var result = ((FrameworkElement)element).GetFirstLogicalChildByType<ContentPresenter>(true);
                var handler = Unlink;
                if (result != null && handler != null)
                {
                    handler(this, new LinkUnlinkEventArgs(result));
                }
            }

            base.ClearContainerForItemOverride(element, item);
        }
    }
}
