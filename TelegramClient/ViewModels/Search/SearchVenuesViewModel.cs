// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Search
{
    public class SearchVenuesViewModel : ItemsViewModelBase<TLObject>
    {
        public bool IsNotWorking { get { return !IsWorking; } }

        private string _text;

        public string Text
        {
            get { return _text; }
            set { SetField(ref _text, value, () => Text); }
        }

        public GeoCoordinate Location { get; set; }

        public Action<TLMessageMediaVenue> AttachAction;

        public SearchVenuesViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new ObservableCollection<TLObject>();
            Status = AppResources.NoResults;

            if (StateService.GeoCoordinate != null)
            {
                Location = StateService.GeoCoordinate;
                StateService.GeoCoordinate = null;
            }

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Text))
                {
                    if (!string.IsNullOrEmpty(Text))
                    {
                        Search(Text);
                    }
                    else
                    {
                        Items.Clear();
                        Status = AppResources.NoResults;
                    }
                }
            };
        }

        public void AttachVenue(TLMessageMediaVenue venue)
        {
            if (venue == null) return;

            AttachAction.SafeInvoke(venue);
        }

        private readonly Dictionary<string, List<TLMessageMediaVenue>> _cache = new Dictionary<string, List<TLMessageMediaVenue>>();

        private TLUserBase _foursquareBot;

        public void Search(string inputText)
        {
            if (inputText == null)
            {
                return;
            }

            var text = inputText.Trim();

            Execute.BeginOnUIThread(TimeSpan.FromMilliseconds(300), () =>
            {
                if (!string.Equals(Text, text))
                {
                    return;
                }

                List<TLMessageMediaVenue> cachedResult;
                if (_cache.TryGetValue(text, out cachedResult))
                {
                    Items.Clear();
                    foreach (var venue in cachedResult)
                    {
                        Items.Add(venue);
                    }

                    IsWorking = false;
                    Status = Items.Count > 0 ? string.Empty : AppResources.NoResults;

                    return;
                }

                if (_foursquareBot == null)
                {
                    IoC.Get<IMTProtoService>().ResolveUsernameAsync(new TLString("foursquare"),
                        result =>
                        {
                            if (result.Peer is TLPeerUser && result.Users.Count > 0)
                            {
                                _foursquareBot = result.Users[0];
                                Search(inputText);
                            }
                        });

                    return;
                }
                IsWorking = true;
                Status = Items.Count > 0 ? Status : AppResources.Loading;
                IoC.Get<IMTProtoService>().GetInlineBotResultsAsync(
                    _foursquareBot.ToInputUser(),
                    new TLInputPeerSelf(),
                    new TLInputGeoPoint { Lat = new TLDouble(Location.Latitude), Long = new TLDouble(Location.Longitude) },
                    TLString.Empty, TLString.Empty,
                    result =>
                    {
                        var venues = new List<TLMessageMediaVenue>();
                        foreach (var inlineResult in result.Results)
                        {
                            var inlineMessageMediaVenue = inlineResult.SendMessage as TLBotInlineMessageMediaVenue78;
                            if (inlineMessageMediaVenue == null) continue;

                            Uri iconSource = null;
                            if (!TLString.IsNullOrEmpty(inlineMessageMediaVenue.VenueType))
                            {
                                // get icon with provided size (32, 44, 64 or 88)
                                // https://foursquare.com/img/categories_v2/food/icecream_bg_32.png
                                //
                                iconSource = new Uri(string.Format("https://foursquare.com/img/categories_v2/{0}_{1}.png", inlineMessageMediaVenue.VenueType, 64));
                            }

                            var mediaVenue = new TLMessageMediaVenue72
                            {
                                VenueId = inlineMessageMediaVenue.VenueId,
                                Title = inlineMessageMediaVenue.Title,
                                Address = inlineMessageMediaVenue.Address,
                                Provider = inlineMessageMediaVenue.Provider,
                                Geo = inlineMessageMediaVenue.Geo,
                                VenueType = inlineMessageMediaVenue.VenueType,
                                IconSource = iconSource
                            };

                            venues.Add(mediaVenue);
                        }

                        Execute.BeginOnUIThread(() =>
                        {
                            _cache[text] = venues;

                            if (!string.Equals(Text, text))
                            {
                                return;
                            }

                            Items.Clear();
                            foreach (var venue in venues)
                            {
                                Items.Add(venue);
                            }

                            IsWorking = false;
                            Status = Items.Count > 0 ? string.Empty : AppResources.NoResults;
                        });
                    },
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Items.Clear();
                        Status = AppResources.NoResults;
                    }));
            });
        }
    }
}
