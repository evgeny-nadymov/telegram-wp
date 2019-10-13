// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using Telegram.Controls.VirtualizedView;
using Telegram.EmojiPanel.Controls.Emoji;
using Telegram.EmojiPanel.Controls.Utilites;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Dialogs;
using TelegramClient.Views.Media;
using Execute = Telegram.Api.Helpers.Execute;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class MasksView
    {
        public MasksViewModel ViewModel
        {
            get { return DataContext as MasksViewModel; }
        }

        public MasksView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void StickerSet_OnTap(object sender, GestureEventArgs e)
        {
            //return;
            OpenStickerSet(sender, e);
        }

        private void OpenStickerSet(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var stickerSet = element.DataContext as TLStickerSet;
            if (stickerSet == null) return;

            Execute.BeginOnUIThread(() =>
                ShowStickerSetMessageBox(false, true, stickerSet, prompt =>
                {
                    if (prompt == PopUpResult.Ok)
                    {
                        ViewModel.Remove(stickerSet);
                    }
                }));
        }

        private void Remove_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;
            if (menuItem != null)
            {
                var stickerSet = menuItem.DataContext as TLStickerSet32;
                if (stickerSet != null)
                {
                    menuItem.Visibility = stickerSet.Official && ! stickerSet.Masks
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
            }
        }

        private bool _once;

        private void ForwardTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                _once = true;
                ViewModel.ForwardInAnimationComplete();
            }
            //var journalEntry = NavigationService.BackStack.FirstOrDefault();
            //if (journalEntry != null && journalEntry.Source != null)
            //{
                
            //}
        }

        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.ReorderStickerSets();
        }
    }
}