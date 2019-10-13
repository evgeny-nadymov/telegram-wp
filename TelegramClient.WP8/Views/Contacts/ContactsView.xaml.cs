// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;

namespace TelegramClient.Views.Contacts
{
    public partial class ContactsView
    {
        public ContactsView()
        {
            App.Log("start ContactsView.ctor");

            InitializeComponent();

            App.Log("stop ContactsView.ctor");
        }

        public FrameworkElement TapedItem;

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            TapedItem = (FrameworkElement)sender;

            var userBase = TapedItem.DataContext as TLUserBase;
            if (userBase == null) return;

            if (!(TapedItem.RenderTransform is CompositeTransform))
            {
                TapedItem.RenderTransform = new CompositeTransform();
            }

            var listBoxItem = TapedItem.FindParentOfType<ListBoxItem>();

            ShellView.StartContinuumForwardOutAnimation(TapedItem, listBoxItem);

            Loaded += (o, args) =>
            {

            };
        }
    }
}