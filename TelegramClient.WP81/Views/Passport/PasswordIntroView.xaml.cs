using System.Windows;

namespace TelegramClient.Views.Passport
{
    public partial class PasswordIntroView
    {
        public PasswordIntroView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void ImageBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
            ImagePlaceholder.Opacity = 1.0;
        }
    }
}