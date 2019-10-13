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