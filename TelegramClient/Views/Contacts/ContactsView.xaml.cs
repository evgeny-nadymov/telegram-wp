using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Animation.Navigation;

namespace TelegramClient.Views.Contacts
{
    public partial class ContactsView
    {
        public ContactsView()
        {
            InitializeComponent();
        }
        
        public FrameworkElement TapedItem;

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            TapedItem = (FrameworkElement) sender;

            if (!(TapedItem.DataContext is TLUserContact)) return;

            //foreach (var descendant in TapedItem.GetVisualDescendants().OfType<FrameworkElement>())
            //{
            //    if (AnimatedBasePage.GetIsAnimationTarget(descendant))
            //    {
            //        TapedItem = descendant;
            //        break;
            //    }
            //}

            if (!(TapedItem.RenderTransform is CompositeTransform))
            {
                TapedItem.RenderTransform = new CompositeTransform();
            }

            var tapedItemContainer = TapedItem.FindParentOfType<ListBoxItem>();
            if (tapedItemContainer != null)
            {
                tapedItemContainer = tapedItemContainer.FindParentOfType<ListBoxItem>();
            }

            ShellView.StartContinuumForwardOutAnimation(TapedItem, tapedItemContainer);
        }
    }
}