// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows.Controls;

namespace Telegram.Controls.LongListSelector
{
    /// <summary>
    /// The event args for the Link/Unlink events.
    /// </summary>
    public class LinkUnlinkEventArgs : EventArgs
    {
        /// <summary>
        /// Create new LinkUnlinkEventArgs.
        /// </summary>
        /// <param name="cp">The ContentPresenter.</param>
        public LinkUnlinkEventArgs(ContentPresenter cp)
        {
            ContentPresenter = cp;
        }

        /// <summary>
        /// The ContentPresenter which is displaying the item.
        /// </summary>
        public ContentPresenter ContentPresenter { get; private set; }
    }

    /// <summary>
    /// The GroupPopupOpened event args.
    /// </summary>
    public class GroupViewOpenedEventArgs : EventArgs
    {
        internal GroupViewOpenedEventArgs(ItemsControl itemsControl)
        {
            ItemsControl = itemsControl;
        }

        /// <summary>
        /// The ItemsControl containing the groups.
        /// </summary>
        public ItemsControl ItemsControl { get; private set; }
    }

    /// <summary>
    /// The GroupPopupClosing event args.
    /// </summary>
    public class GroupViewClosingEventArgs : EventArgs
    {
        internal GroupViewClosingEventArgs(ItemsControl itemsControl, object selectedGroup)
        {
            ItemsControl = itemsControl;
            SelectedGroup = selectedGroup;
        }

        /// <summary>
        /// The ItemsControl containing the groups.
        /// </summary>
        public ItemsControl ItemsControl { get; private set; }

        /// <summary>
        /// The selected group. Will be null if the back button was pressed.
        /// </summary>
        public object SelectedGroup { get; private set; }

        /// <summary>
        /// Set this to true if the application will handle the popup closing and scrolling to the group.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
