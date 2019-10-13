using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using EmojiPanel.Controls.Emoji;
using Microsoft.Phone.Controls;

namespace EmojiPanel
{
    public partial class MainPage
    {
        public EmojiControl EmojiInstance;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnSmileIconClick(object sender, EventArgs e)
        {
            if (EmojiInstance == null)
            {
                // Initialize EmojiControl
                EmojiInstance = EmojiControl.GetInstance();
                EmojiInstance.BindTextBox(InputTextBox);

                ContentPanel.Children.Add(EmojiInstance); // Add to view
            }

            EmojiInstance.IsOpen = !EmojiInstance.IsOpen;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (EmojiInstance == null) return;

            // Destroy EmojiControl
            EmojiInstance.IsOpen = false;
            EmojiInstance.UnbindTextBox();
            ContentPanel.Children.Remove(EmojiInstance); // Remove from view
            EmojiInstance = null;
        }

        private void ClearAllButtonClick(object sender, EventArgs e)
        {
            InputTextBox.Text = "";
        }
    }
}