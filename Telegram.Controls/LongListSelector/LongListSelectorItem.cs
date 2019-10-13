// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Diagnostics.CodeAnalysis;

namespace Telegram.Controls.LongListSelector
{
    /// <summary>
    /// Holds information about an item for use in the LongListSelector.
    /// </summary>
    public class LongListSelectorItem
    {
        /// <summary>
        /// Gets or sets the item type.
        /// </summary>
        public LongListSelectorItemType ItemType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the associated group for the item.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Assists in debugging.")]
        public object Group
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the underlying item instance.
        /// </summary>
        public object Item
        {
            get;
            set;
        }
    }
}
