// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Feed
{
    public class FeedViewModel : ItemsViewModelBase<TLChatBase> 
    {
        public TLVector<TLChatBase> CurrentItem { get; set; }

        private string _subtitle;

        public string Subtitle
        {
            get { return _subtitle; }
            set { SetField(ref _subtitle, value, () => Subtitle); }
        }

        public FeedViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            CurrentItem = stateService.CurrentFeed;
            stateService.CurrentFeed = null;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            UpdateSubtitles();
        }

        public void ForwardInAnimationComplete()
        {
            var channels = CurrentItem;
            if (channels != null)
            {
                UpdateChannels(channels, UpdateItems);
            }
        }

        private void UpdateItems()
        {
            
        }

        private void UpdateChannels(TLVector<TLChatBase> channels, System.Action callback)
        {
            const int firstSliceCount = 3;
            var secondSlice = new List<TLChatBase>();
            for (var i = 0; i < channels.Count; i++)
            {
                if (i < firstSliceCount)
                {
                    //users[i].IsAdmin = false;
                    Items.Add(channels[i]);
                }
                else
                {
                    secondSlice.Add(channels[i]);
                }
            }

            Execute.BeginOnUIThread(() =>
            {
                foreach (var user in secondSlice)
                {
                    //user.IsAdmin = false;
                    Items.Add(user);
                }
                callback.SafeInvoke();
            });
        }

        public void ViewChannel(TLChatBase channel)
        {
            if (channel == null) return;

            StateService.With = channel;
            StateService.RemoveBackEntries = true;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        public void UpdateSubtitles()
        {
            Subtitle = GetChatSubtitle();
        }

        private string GetChatSubtitle()
        {
            var channels = CurrentItem;
            if (channels != null)
            {
                var channelsCount = channels.Count;

                return Language.Declension(
                    channelsCount,
                    AppResources.ChannelNominativeSingular,
                    AppResources.ChannelNominativePlural,
                    AppResources.ChannelGenitiveSingular,
                    AppResources.ChannelGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }

            return null;
        }
    }
}
