// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Additional;
using Execute = Telegram.Api.Helpers.Execute;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public partial class StickersView
    {
        public StickersViewModel ViewModel
        {
            get { return DataContext as StickersViewModel; }
        }

        public StickersView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        public void CreateBitmapCache()
        {
            var bitmap = new WriteableBitmap(ItemsContentControl, null);

            Image.Source = bitmap;
            Image.Visibility = Visibility.Visible;
            Items.Visibility = Visibility.Collapsed;
        }

        private WriteableBitmap _bitmap;

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //base.OnBackKeyPress(e);

            //if (_bitmap == null)
            //{
            //    _bitmap = new WriteableBitmap(ItemsContentControl, null);
            //    e.Cancel = true;
            
            //    Image.Source = _bitmap;
            //    Image.Visibility = Visibility.Visible;
            //    Items.Visibility = Visibility.Collapsed;
            //    ItemsContentControl.Content = null;
            //    Deployment.Current.Dispatcher.BeginInvoke(() =>
            //    {
            //        NavigationService.GoBack();
            //    });
            //}
            //else
            //{
            //    Image.Source = _bitmap;
            //    Image.Visibility = Visibility.Visible;
            //    Items.Visibility = Visibility.Collapsed;
            //    ItemsContentControl.Content = null;
            //}
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //if (e.NavigationMode == NavigationMode.Forward)
            //{
            //    Items.Visibility = Visibility.Collapsed;
            //}

            //Execute.BeginOnUIThread(TimeSp() =>
            //{
                
            //});

            //Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    if (e.NavigationMode == NavigationMode.Back)
            //    {
            //        LayoutRoot.Children.Clear();
            //    }
            //});
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
                    menuItem.Visibility = stickerSet.Official
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
        }

        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.ReorderStickerSets();
        }
    }
}