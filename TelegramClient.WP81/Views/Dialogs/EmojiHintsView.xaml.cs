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
    public partial class EmojiHintsView
    {
        public EmojiHintsView()
        {
            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            BackgroundBorder.Background = isLightTheme
                ? (Brush)Resources["HintsBackgroundBrushLight"]
                : (Brush)Resources["HintsBackgroundBrushDark"];
            Border.Background = isLightTheme
                ? (Brush)Resources["InputBorderBrushLight"]
                : (Brush)Resources["InputBorderBrushDark"];
        }
    }
}
