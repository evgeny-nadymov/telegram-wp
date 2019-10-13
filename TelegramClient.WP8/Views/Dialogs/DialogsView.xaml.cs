// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Extensions;
using Telegram.Controls.Triggers;
using Telegram.Controls.VirtualizedView;
using Telegram.EmojiPanel.Controls.Utilites;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Dialogs
{
    public partial class DialogsView
    {
        public DialogsViewModel ViewModel
        {
            get { return (DialogsViewModel)DataContext; }
        }

        public DialogsView()
        {
            App.Log("start DialogsView.ctor");

            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            ((SolidColorBrush)Resources["PinnedBackground"]).Color = isLightTheme
                ? ((SolidColorBrush)Resources["PinnedBackgroundLight"]).Color
                : ((SolidColorBrush)Resources["PinnedBackgroundDark"]).Color;

            //VirtPanel.InitializeWithScrollViewer(CSV);
            //VirtPanel.CreateFunc = o =>
            //{
            //    return new DialogItem(o as TLDialogBase, 480.0, Resources["DialogTemplate"] as DataTemplate);
            //};

            App.Log("stop DialogsView.ctor");
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            ((DialogsViewModel)DataContext).LoadNextSlice();
        }

        public FrameworkElement TapedItem;

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            TapedItem = (FrameworkElement)sender;

            var tapedItemContainer = TapedItem.FindParentOfType<ListBoxItem>();

            var result = ViewModel.OpenDialogDetails(TapedItem.DataContext as TLDialogBase);
            if (result)
            {
                ShellView.StartContinuumForwardOutAnimation(TapedItem, tapedItemContainer);
            }
        }

        private void MainItemGrid_OnDoubleTap(object sender, GestureEventArgs gestureEventArgs)
        {
            TapedItem = (FrameworkElement)sender;

            var tapedItemContainer = TapedItem.FindParentOfType<ListBoxItem>();

            var result = ViewModel.OpenFastDialogDetails(TapedItem.DataContext as TLDialogBase);
            //if (result)
            //{
            //    ShellView.StartContinuumForwardOutAnimation(TapedItem, tapedItemContainer);

            //            }
        }

        private void DeleteAndStop_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var dialog = menuItem.DataContext as TLDialogBase;
            if (dialog == null) return;

            var user = dialog.With as TLUser;

            menuItem.Visibility = user != null && user.IsBot && (user.Blocked == null || !user.Blocked.Value)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ClearHistory_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var dialog = menuItem.DataContext as TLDialogBase;
            if (dialog == null) return;

            var channel = dialog.With as TLChannel;

            menuItem.Visibility = channel != null && (!channel.IsMegaGroup || !TLString.IsNullOrEmpty(channel.UserName))
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void Pin_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var dialog = menuItem.DataContext as TLDialog53;
            if (dialog == null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            var dialog71 = menuItem.DataContext as TLDialog71;
            if (dialog71 != null && dialog71.IsPromo)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            var messageService = dialog.TopMessage as TLMessageService;
            if (messageService != null && messageService.Action is TLMessageActionContactRegistered)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            var isPinned = dialog.IsPinned;
            if (!isPinned)
            {
                var config = IoC.Get<ICacheService>().GetConfig() as TLConfig61;
                if (config != null)
                {
                    var pinnedCount = ViewModel.Items.Count(x => x.IsPinned);
                    if (pinnedCount >= config.PinnedDialogsCountMax.Value)
                    {
                        menuItem.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (isPinned)
            {
                menuItem.Header = AppResources.UnpinDialog;
            }
            else
            {
                menuItem.Header = AppResources.PinDialog;
            }
        }

        private void Group_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            menuItem.Visibility = Visibility.Collapsed;
            return;

            var dialog = menuItem.DataContext as TLDialog;
            if (dialog == null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            var dialog71 = menuItem.DataContext as TLDialog71;
            if (dialog71 == null && dialog71.IsPromo)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            var channel = dialog.With as TLChannel76;
            if (channel == null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            menuItem.Header = channel.FeedId != null ? AppResources.Ungroup : AppResources.Group;
        }

        private void ChangeUnreadMark_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var dialog = menuItem.DataContext as TLDialog71;
            if (dialog == null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            menuItem.Header =
                dialog.UnreadMark
                || dialog.UnreadCount != null && dialog.UnreadCount.Value > 0
                || dialog.UnreadMentionsCount != null && dialog.UnreadMentionsCount.Value > 0 ?
                AppResources.MarkRead :
                AppResources.MarkUnread;
        }

        private void PinToStart_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var dialog = menuItem.DataContext as TLDialogBase;
            if (dialog == null) return;

            var tileNavigationParam = DialogsViewModel.GetTileNavigationParam(dialog);
            if (tileNavigationParam == null)
            {
                menuItem.Visibility = Visibility.Collapsed;
                return;
            }

            var tileExists = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(tileNavigationParam)) != null;
            if (tileExists)
            {
                menuItem.Header = AppResources.UnpinFromStart;
            }
            else
            {
                menuItem.Header = AppResources.PinToStart;
            }
        }

        private void ContextMenu_OnLoaded(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu == null) return;

            var dialog = contextMenu.DataContext as TLDialogBase;
            if (dialog == null) return;

            //var channel = dialog.With as TLChannel;

            //contextMenu.Visibility = channel != null && !channel.IsMegaGroup
            //    ? Visibility.Collapsed
            //    : Visibility.Visible;
        }

        private void DeleteDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            menuItem.Visibility = Visibility.Collapsed;

            var dialog = menuItem.DataContext as TLDialogBase;
            if (dialog == null) return;

            var dialog71 = menuItem.DataContext as TLDialog71;
            if (dialog71 != null && dialog71.IsPromo) return;

            var peerChannel = dialog.Peer as TLPeerChannel;
            if (peerChannel != null)
            {
                var channel = dialog.With as TLChannel49;
                if (channel != null)
                {
                    menuItem.Header = channel.IsMegaGroup ? AppResources.LeaveGroup : AppResources.LeaveChannel;
                }

                menuItem.Visibility = Visibility.Visible;
                return;
            }

            var peerUser = dialog.Peer as TLPeerUser;
            if (peerUser != null)
            {
                menuItem.Visibility = Visibility.Visible;
                return;
            }

            var peerChat = dialog.Peer as TLPeerChat;
            if (peerChat != null)
            {
                var isVisible = dialog.With is TLChatForbidden || dialog.With is TLChatEmpty;

                menuItem.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                return;
            }
        }

        private void DeleteAndExit_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            menuItem.Visibility = Visibility.Collapsed;

            var dialog = menuItem.DataContext as TLDialogBase;
            if (dialog == null) return;

            var peerChat = dialog.Peer as TLPeerChat;
            if (peerChat != null)
            {
                menuItem.Visibility = Visibility.Visible;
                return;
            }

            //var peerChannel = dialog.Peer as TLPeerChannel;
            //if (peerChannel != null)
            //{
            //    var channel = dialog.With as TLChannel;
            //    menuItem.Visibility = channel != null && channel.IsMegaGroup ? Visibility.Visible : Visibility.Collapsed;
            //    return;
            //}

            var peerEncryptedChat = dialog.Peer as TLPeerEncryptedChat;
            if (peerEncryptedChat != null)
            {
                menuItem.Header = AppResources.DeleteChat;
                menuItem.Visibility = Visibility.Visible;
                return;
            }

            var peerBroadcast = dialog.Peer as TLPeerBroadcast;
            if (peerBroadcast != null)
            {
                menuItem.Visibility = Visibility.Visible;
                return;
            }
        }

        private void MainItemGrid_OnTapCommon(object sender, GestureEventArgs e)
        {
            if (!ViewModel.TestMode)
            {
                MainItemGrid_OnTap(sender, e);
            }
            else
            {
                //Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                //{
                MainItemGrid_OnDoubleTap(sender, e);
                //});
            }
        }

        private DateTime? _lastInvokeTime;

        private void Items_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            //if (_lastInvokeTime != null && _lastInvokeTime.Value.AddSeconds(10.0) > DateTime.Now) return;

            //_lastInvokeTime = DateTime.Now;
            //ViewModel.LoadNextSlice();
            this.Focus();
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            MessageBox.Show("4 " + element.ActualHeight.ToString());
        }

        private bool _once = true;

        private void Items_OnManipulationStarted(object sender, ScrollingStateChangedEventArgs args)
        {
            if (!_once) return;
            if (!args.NewValue) return;

            _once = false;
            ViewModel.LoadNextSlice();
        }
    }

    public class DialogItem : VListItemBase
    {
        public DialogItem(TLDialogBase dialog, double panelWidth, DataTemplate template)
        {
            _fixedHeight = 96.0;

            var contentControl = new ContentControl { Width = panelWidth, Content = dialog, HorizontalContentAlignment = HorizontalAlignment.Stretch, ContentTemplate = template };

            //var binding = new Binding
            //{
            //    Mode = BindingMode.OneWay,
            //    Path = new PropertyPath("LineOne"),
            //    //Converter = new DefaultPhotoConverter(),
            //    //ConverterParameter = StickerHeight
            //};
            //var panelMargin = new Thickness(0.0, 0.0, 0.0, 17.0);
            //var panelActualWidth = panelWidth - panelMargin.Left - panelMargin.Right;
            //var stackPanel = new StackPanel{ Width = panelActualWidth, Margin = panelMargin, Background = new SolidColorBrush(Colors.Transparent) };
            //var firstTextBlock = new TextBlock {TextWrapping = TextWrapping.Wrap, Text = item.LineOne, Style = Application.Current.Resources["PhoneTextExtraLargeStyle"] as Style };
            //firstTextBlock.SetBinding(TextBlock.TextProperty, binding);
            //var secondTextBlock = new TextBlock
            //{
            //    TextWrapping = TextWrapping.Wrap,
            //    Text = item.LineTwo,
            //    Margin = new Thickness(12.0, -6.0, 12.0, 0.0),
            //    Style = Application.Current.Resources["PhoneTextSubtleStyle"] as Style
            //};
            //stackPanel.Children.Add(firstTextBlock);
            //stackPanel.Children.Add(secondTextBlock);
            var listBoxItem = new ListBoxItem { Content = contentControl, DataContext = dialog };
            //listBoxItem.Tap += Item_OnTap;

            Children.Add(listBoxItem);

            View.Width = panelWidth;
        }

        private double _fixedHeight;

        public override double FixedHeight
        {
            get { return _fixedHeight; }
            set { _fixedHeight = value; }
        }
    }

    public class CheckDialogTypeConverter : IValueConverter
    {
        public string Type { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(DialogsViewModel))
            {
                return value;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}