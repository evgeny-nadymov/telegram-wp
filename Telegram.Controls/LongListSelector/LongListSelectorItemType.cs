// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Controls.LongListSelector
{
    /// <summary>
    /// Describes different items.
    /// </summary>
    public enum LongListSelectorItemType
    {
        /// <summary>
        /// Indicates an unknown item type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Represents a standard list item.
        /// </summary>
        Item,

        /// <summary>
        /// Represents a group header.
        /// </summary>
        GroupHeader,

        /// <summary>
        /// Represents a group footer.
        /// </summary>
        GroupFooter,

        /// <summary>
        /// Represents a list header.
        /// </summary>
        ListHeader,

        /// <summary>
        /// Represents a list footer.
        /// </summary>
        ListFooter,
    }
}
