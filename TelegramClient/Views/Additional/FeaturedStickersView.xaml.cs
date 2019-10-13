// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Additional;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class FeaturedStickersView
    {
        public FeaturedStickersViewModel ViewModel
        {
            get { return DataContext as FeaturedStickersViewModel; }
        }

        public FeaturedStickersView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private bool _once;

        private void ForwardTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                _once = true;
                ViewModel.ForwardInAnimationComplete();
            }
        }

        private void StickerSet_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var stickerSet = element.DataContext as TLStickerSet32;
            if (stickerSet == null) return;

            Execute.BeginOnUIThread(() =>
            {
                ShowStickerSetMessageBox(false, stickerSet.Installed, stickerSet, prompt =>
                {
                    if (prompt == PopUpResult.Ok)
                    {
                        ViewModel.AddRemoveStickerSet(stickerSet);
                    }
                });
            });
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu != null)
            {
                
            }
        }

        private void AddRemoveStickerSet_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var stickerSet = button.DataContext as TLStickerSet32;
                if (stickerSet != null)
                {
                    ViewModel.AddRemoveStickerSet(stickerSet);
                }
            }
        }

        private void Button_OnLoaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                Execute.ShowDebugMessage(button.ActualWidth.ToString());
            }
        }
    }
}