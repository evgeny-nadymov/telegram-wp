// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Media;

namespace TelegramClient.Views.Dialogs
{
    public partial class StickerHintsView
    {
        public StickerHintsView()
        {
            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            BackgroundBorder.Background = isLightTheme
                ? (Brush)Resources["HintsBackgroundBrushLight"]
                : (Brush)Resources["HintsBackgroundBrushDark"];
        }

        //private IList<TLStickerItem> _delayedStickers;

        private void OpenStickersStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            //if (_delayedStickers != null && _stickers != null)
            //{
            //    for (var i = 0; i < _delayedStickers.Count; i++)
            //    {
            //        _stickers.Add(_delayedStickers[i]);
            //    }
            //    _delayedStickers = null;
            //}
        }
    }
}
