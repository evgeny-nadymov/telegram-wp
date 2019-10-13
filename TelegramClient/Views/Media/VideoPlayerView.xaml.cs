// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using TelegramClient.ViewModels.Media;

namespace TelegramClient.Views.Media
{
    public partial class VideoPlayerView
    {
        public VideoPlayerViewModel ViewModel
        {
            get { return DataContext as VideoPlayerViewModel; }
        }


        public VideoPlayerView()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var stream = new IsolatedStorageFileStream(ViewModel.IsoFileName, FileMode.Open, FileAccess.Read, FileShare.Read, store))
                    //using (var stream = new IsolatedStorageFileStream(ViewModel.IsoFileName, FileMode.Open, FileAccess.Read, store))
                    {
                        MediaElement.SetSource(stream);
                        //MediaElement.Play();
                    }
                }
            };
        }

        private void VideoPlayerView_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            // see whole code from original question.................

            //MediaElement.Visibility = Visibility.Collapsed;
            MediaElement.Source = null; // added this line
            MediaElement = null;

            //..................

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MediaElement.Play();
        }
    }
}