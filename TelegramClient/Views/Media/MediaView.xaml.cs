using TelegramClient.ViewModels.Media;

namespace TelegramClient.Views.Media
{
    public partial class MediaView
    {
        public MediaView()
        {
            InitializeComponent();
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ((ISliceLoadable)DataContext).LoadNextSlice();
        }
    }
}