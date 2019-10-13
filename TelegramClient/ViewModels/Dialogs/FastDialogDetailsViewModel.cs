// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Extensions;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;

namespace TelegramClient.ViewModels.Dialogs
{
    public class FastDialogDetailsViewModel
    {
        public Brush WatermarkForeground
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    if (StateService.IsEmptyBackground)
                    {
                        var color = Colors.Black;
                        color.A = 153;
                        return new SolidColorBrush(color);
                    }
                }

                return (Brush)Application.Current.Resources["PhoneContrastForegroundBrush"];
            }
        }

        public IStateService StateService { get; protected set; }

        public ICacheService CacheService { get; protected set; }

        public ObservableCollection<TLMessageBase> Items { get; set; }

        public ObservableCollection<TLMessageBase> LazyItems { get; set; }
        
        public TLObject With { get; set; }

        public int FistSliceCount { get; set; }

        public string Subtitle { get; set; }

        public FastDialogDetailsViewModel(ICacheService cacheService, IStateService stateService)
        {
            StateService = stateService;
            CacheService = cacheService;
            With = StateService.With;
            StateService.With = null;
            var dialog = StateService.Dialog as TLDialog;
            if (dialog != null)
            {
                Items = new ObservableCollection<TLMessageBase>(dialog.Messages.Take(10));
                FistSliceCount = 15;
            }
            Subtitle = GetSubtitle();
        }

        public void OnLoaded()
        {
            //Items.AddRange(LazyItems);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (LazyItems != null)
                {
                    Items.AddRange(LazyItems);
                }
            });
        }

        private string GetSubtitle()
        {
            var channel = With as TLChannel;
            if (channel != null)
            {
                if (channel.ParticipantsCount != null)
                {
                    return Language.Declension(
                        channel.ParticipantsCount.Value,
                        AppResources.CompanyNominativeSingular,
                        AppResources.CompanyNominativePlural,
                        AppResources.CompanyGenitiveSingular,
                        AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                }

                if (channel.IsMegaGroup)
                {
                    return AppResources.Loading.ToLowerInvariant();
                }

                return channel.IsPublic ? AppResources.PublicChannel.ToLowerInvariant() : AppResources.PrivateChannel.ToLowerInvariant();
            }

            var user = With as TLUserBase;
            if (user != null)
            {
                return GetUserStatus(user);
            }

            var chat = With as TLChat;
            if (chat != null)
            {
                var participantsCount = chat.ParticipantsCount.Value;
                var onlineCount = chat.UsersOnline;
                var onlineString = onlineCount > 0 ? string.Format(", {0} {1}", chat.UsersOnline, AppResources.Online.ToLowerInvariant()) : string.Empty;

                var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
                var isCurrentUserOnline = currentUser != null && currentUser.Status is TLUserStatusOnline;
                if (participantsCount == 1 || (onlineCount == 1 && isCurrentUserOnline))
                {
                    onlineString = string.Empty;
                }

                return Language.Declension(
                    participantsCount,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture)
                    + onlineString;
            }

            var forbiddenChat = With as TLChatForbidden;
            if (forbiddenChat != null)
            {
                return LowercaseConverter.Convert(AppResources.YouWhereKickedFromTheGroup);
            }

            var broadcastChat = With as TLBroadcastChat;
            if (broadcastChat != null)
            {
                var participantsCount = broadcastChat.ParticipantIds.Count;
                var onlineParticipantsCount = 0;
                foreach (var participantId in broadcastChat.ParticipantIds)
                {
                    var participant = CacheService.GetUser(participantId);
                    if (participant != null && participant.Status is TLUserStatusOnline)
                    {
                        onlineParticipantsCount++;
                    }
                }

                var onlineString = onlineParticipantsCount > 0 ? string.Format(", {0} {1}", onlineParticipantsCount, AppResources.Online.ToLowerInvariant()) : string.Empty;

                return Language.Declension(
                    participantsCount,
                    AppResources.CompanyNominativeSingular,
                    AppResources.CompanyNominativePlural,
                    AppResources.CompanyGenitiveSingular,
                    AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture)
                    + onlineString;
            }

            return string.Empty;
        }

        public static string GetUserStatus(TLUserBase user)
        {
            if (user.Index == Constants.TelegramNotificationsId)
            {
                return AppResources.ServiceNotifications.ToLowerInvariant();
            }

            if (user.BotInfo is TLBotInfo)
            {
                return AppResources.Bot.ToLowerInvariant();
            }

            return UserStatusToStringConverter.Convert(user.Status);
        }
    }
}
