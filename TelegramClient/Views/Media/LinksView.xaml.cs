using System.Windows.Input;
using TelegramClient.ViewModels.Media;

namespace TelegramClient.Views.Media
{
    public partial class LinksView
    {
        public LinksView()
        {
            InitializeComponent();
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ((ISliceLoadable)DataContext).LoadNextSlice();
        }

        private void Files_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ((ISliceLoadable)DataContext).LoadNextSlice();
        }
    }
}