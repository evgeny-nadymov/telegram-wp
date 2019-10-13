using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.Views.Dialogs
{
    public partial class DialogsView
    {
        public DialogsViewModel ViewModel
        {
            get { return (DialogsViewModel)DataContext; }
        }

        public DialogsView()
        {
            InitializeComponent();
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ((DialogsViewModel)DataContext).LoadNextSlice();
        }

        public FrameworkElement TapedItem;

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            TapedItem = (FrameworkElement)sender;

            var tapedItemContainer = TapedItem.FindParentOfType<ListBoxItem>();

            var result = ViewModel.OpenDialogDetails(TapedItem.DataContext as TLDialogBase);
            if (result)
            {
                ShellView.StartContinuumForwardOutAnimation(TapedItem, tapedItemContainer);
            }
        }
    }
}