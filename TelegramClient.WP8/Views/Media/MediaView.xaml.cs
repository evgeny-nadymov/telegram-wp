// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.ViewModels.Media;

namespace TelegramClient.Views.Media
{
    public partial class MediaView
    {
        public MediaViewModel<IInputPeer> ViewModel
        {
            get { return DataContext as MediaViewModel<IInputPeer>; }
        } 

        public MediaView()
        {
            InitializeComponent();

            // FullHD
            OptimizeFullHD();
        }

        ~MediaView()
        {
            
        }

        private void OptimizeFullHD()
        {
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            if (!isFullHD) return;

            //BottomAppBarPlaceholder.Height = new GridLength(Constants.FullHDAppBarHeight);
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ((ISliceLoadable)DataContext).LoadNextSlice();
        }

        private bool _once;

        private void Files_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_once) return;

            _once = true;
            ((ISliceLoadable)DataContext).LoadNextSlice();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private List<MessagesRow> _unrealizedRows = new List<MessagesRow>(); 

        private void LongListSelector_OnItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
            return;
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                var element = e.Container as FrameworkElement;
                if (element != null)
                {
                    var messagesRow = element.DataContext as MessagesRow;
                    if (messagesRow != null)
                    {
                        _unrealizedRows.Add(messagesRow);
                        //System.Diagnostics.Debug.WriteLine("Unrealized row " + ViewModel.Items.IndexOf(messagesRow));
                    }
                    return;
                    var images = FindVisualChildren<Image>(element);
                    foreach (var image in images)
                    {
                        if (image.Source == null)
                        {
                            var message = image.DataContext as TLMessage;
                            if (message != null)
                            {
                                var mediaPhoto = message.Media as TLMessageMediaPhoto;
                                if (mediaPhoto != null)
                                {
                                    var photo = mediaPhoto.Photo as TLPhoto;
                                    if (photo != null)
                                    {
                                        ViewModel.CancelDownloading(photo);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LongListSelector_OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            return;
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                var element = e.Container as FrameworkElement;
                if (element != null)
                {
                    var messagesRow = element.DataContext as MessagesRow;
                    if (messagesRow != null)
                    {
                        _unrealizedRows.Remove(messagesRow);

                        if (_unrealizedRows.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Invoke unrealized row " + _unrealizedRows.Count);
                        }
                    }
                    return;
                }
            }
        }
    }
}