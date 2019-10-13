// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Extensions;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Additional;

namespace TelegramClient.Views.Dialogs
{
    public partial class InlineBotResultsView
    {
        private TextBlock _debug;

        public InlineBotResultsViewModel ViewModel
        {
            get { return DataContext as InlineBotResultsViewModel; }
        }

        public InlineBotResultsView()
        {
            InitializeComponent();

            var applicationBar = new ApplicationBar();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            CaptionBorder.Background = isLightTheme
                ? (Brush)Resources["HintsBorderBrushLight"]
                : (Brush)Resources["HintsBorderBrushDark"];
            CaptionBorder.Height = applicationBar.DefaultSize;
            BackgroundBorder.Background = isLightTheme
                ? (Brush) Resources["HintsBackgroundBrushLight"]
                : (Brush) Resources["HintsBackgroundBrushDark"];
#if DEBUG
            _debug = new TextBlock();
            _debug.Foreground = new SolidColorBrush(Colors.Green);
            _debug.TextWrapping = TextWrapping.Wrap;
            _debug.VerticalAlignment = VerticalAlignment.Bottom;
            _debug.Margin = new Thickness(0.0);
            _debug.IsHitTestVisible = false;

            //InlineBotResultsPanel.Children.Add(_debug);
#endif
        }

        private void Results_OnCompression(object sender, CompressionEventArgs e)
        {
            if (Results.IsHorizontal && e.Type == CompressionType.Right)
            {
                ViewModel.LoadNextSlice();
            }
            else if (!Results.IsHorizontal && e.Type == CompressionType.Bottom)
            {
                ViewModel.LoadNextSlice();
            }
        }

        public void StartActivePlayers()
        {
            foreach (var item in Results.GetVisibleItems())
            {
                var gifPlayer = item.FindChildOfType<GifPlayerControl>();
                if (gifPlayer != null)
                {
                    gifPlayer.Start();
                }
            }
        }

        public void StopActivePlayers()
        {
            GifPlayerControl.StopInlineBotActivePlayers();
        }

        private void Results_OnScrollingStateChanged(object sender, ScrollingStateChangedEventArgs e)
        {
            GifPlayerControl.IsScrolling = e.NewValue;
#if DEBUG
            var count = GifPlayerControl.InlineBotActivePlayers.Count;
            var oldPlayers = GifPlayerControl.InlineBotActivePlayers.Where(x => x.Media != null).ToList();
            var oldItems = new List<string>();
            foreach (var mediaItem in oldPlayers)
            {
                var mediaGif = mediaItem.Media as IMediaGif;
                if (mediaGif != null && mediaGif.Document != null)
                {
                    oldItems.Add(mediaGif.Document.ShortId);
                }

                var decryptedMediaGif = mediaItem.Media as IDecryptedMediaGif;
                if (decryptedMediaGif != null && decryptedMediaGif.Document != null)
                {
                    oldItems.Add(decryptedMediaGif.Document.GetFileName());
                }
            }
#endif

            if (!e.NewValue)
            {
                StartActivePlayers();
            }
            else
            {
                StopActivePlayers();
            }

#if DEBUG
            var newPlayers = GifPlayerControl.InlineBotActivePlayers.Where(x => x.Media != null).ToList();
            var newItems = new List<string>();
            foreach (var mediaItem in newPlayers)
            {
                var mediaGif = mediaItem.Media as IMediaGif;
                if (mediaGif != null && mediaGif.Document != null)
                {
                    newItems.Add(mediaGif.Document.ShortId);
                }

                var decryptedMediaGif = mediaItem.Media as IDecryptedMediaGif;
                if (decryptedMediaGif != null && decryptedMediaGif.Document != null)
                {
                    newItems.Add(decryptedMediaGif.Document.GetFileName());
                }
            }

            if (_debug == null) return;

            _debug.Text = string.Format("old {0} {1}\nnew {2} {3}",
                count, string.Join(", ", oldItems),
                GifPlayerControl.InlineBotActivePlayers.Count, string.Join(", ", newItems));
#endif
        }

        private void Results_OnClear(object sender, System.EventArgs e)
        {
            StopActivePlayers();
        }

        private void Results_OnFirstSliceLoaded(object sender, System.EventArgs e)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(StartActivePlayers);
        }

        private void InlineBotResultsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Opacity = 1.0;
        }
    }
}
