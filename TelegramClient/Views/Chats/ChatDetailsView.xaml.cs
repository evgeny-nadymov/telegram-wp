using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Contacts;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Chats
{
    public partial class ChatDetailsView
    {
        public ChatDetailsViewModel ViewModel
        {
            get { return DataContext as ChatDetailsViewModel; }
        }

        public static readonly DependencyProperty TimerProperty = DependencyProperty.Register(
            "Timer", typeof (string), typeof (ChatDetailsView), new PropertyMetadata(default(string)));

        public string Timer
        {
            get { return (string) GetValue(TimerProperty); }
            set { SetValue(TimerProperty, value); }
        }

        public ChatDetailsView()
        {
            var timer = Stopwatch.StartNew();
            InitializeComponent();
            
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                NotificationsSwitch.Style = (Style)Application.Current.Resources["ProfileLightToggleSwitch"];
            }

            Loaded += (sender, args) =>
            {
                Timer = timer.Elapsed.ToString();
            };
        }
        
#if DEBUG
        //~ChatDetailsView()
        //{
        //    TLUtils.WritePerformance("++ChatDetailsV destr");
        //}
#endif

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            
            //ContextMenuService.GetContextMenu((DependencyObject)sender).IsOpen = true;
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var span = element.DataContext as TimerSpan;
            if (span == null) return;

            ViewModel.SelectSpan(span);
        }

        private void CopyLink_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyLink();
        }

        private void ToggleSwitch_OnChecked(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectSpan(ViewModel.Spans.First());
        }

        private void ToggleSwitch_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectSpan(ViewModel.Spans.Last());
        }

        private void DeleteMenuItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var user = element.DataContext as TLUserBase;
            if (user == null) return;

            element.Visibility = Visibility.Visible;

            var chat = ViewModel.CurrentItem as TLChat41;
            if (chat != null && chat.AdminsEnabled.Value && !chat.Admin.Value && !chat.Creator)
            {
                element.Visibility = Visibility.Collapsed;
                return;
            }

            var channel = ViewModel.CurrentItem as TLChannel;
            if (channel != null)
            {
                if (!channel.IsEditor && !channel.Creator)
                {
                    element.Visibility = Visibility.Collapsed;
                    return;
                }

                if (channel.ChannelParticipants != null)
                {
                    var participants = channel.ChannelParticipants.Participants;
                    var creator = participants.FirstOrDefault(x => x is TLChannelParticipantCreator);
                    if (creator != null && creator.UserId.Value == user.Index)
                    {
                        element.Visibility = Visibility.Collapsed;
                        return;
                    }

                }

                return;
            }

            
        }
    }
}