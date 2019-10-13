// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;

namespace TelegramClient.Views.Dialogs
{
    public partial class SearchUserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (SearchUserControl), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var searchUserControl = d as SearchUserControl;
            if (searchUserControl != null)
            {
                var from = (string) e.NewValue;
                searchUserControl.From.Text = (string) e.NewValue;

                if (string.IsNullOrEmpty(from))
                {
                    searchUserControl.Label.Text = "from:";
                }
                else
                {
                    searchUserControl.Label.Text = "from: ";
                }
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public SearchUserControl()
        {
            InitializeComponent();
        }
    }
}
