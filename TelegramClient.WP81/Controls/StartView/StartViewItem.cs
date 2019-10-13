// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows.Controls;

namespace TelegramClient.Controls.StartView
{
    /// <summary>
    /// Represents an item in a StartView control.
    /// </summary>
    public class StartViewItem : ContentControl
    {
        /// <summary>
        /// Initializes a new instance of the StartViewItem class.
        /// </summary>
        public StartViewItem()
        {
            DefaultStyleKey = typeof(StartViewItem);
        }

        internal int StartPosition { get; set; }

        internal int ItemWidth { get; set; }
    }
}