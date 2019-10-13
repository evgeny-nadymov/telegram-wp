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
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Location;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Media;
using TelegramClient.Views.Search;

namespace TelegramClient.ViewModels.Media
{
    public class MapViewModel : Screen
    {
        public TLObject FooterMessage
        {
            get
            {
                var message = MessageGeo as TLMessage70;
                if (message != null)
                {
                    var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                    if (mediaGeoLive != null)
                    {
                        var chat = With as TLChat;
                        // forbidden group
                        if (chat != null && chat.IsForbidden)
                        {
                            return new TLMessageEmpty { Id = new TLInt(0) };
                        }
                        var channel = With as TLChannel;
                        if (channel != null)
                        {
                            // forbidden megagroup
                            if (channel.IsMegaGroup && channel.IsForbidden)
                            {
                                return new TLMessageEmpty { Id = new TLInt(0) };
                            }
                            // can't post to channel
                            if (!channel.IsMegaGroup && !(channel.IsEditor || channel.Creator))
                            {
                                return new TLMessageEmpty { Id = new TLInt(0) };
                            }
                        }

                        return MessageGeoLive;
                    }
                }

                return MessageGeo;
            }
        }

        public TLObject With { get; set; }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyOfPropertyChange(() => Status);
                }
            }
        }

        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                if (_isWorking != value)
                {
                    _isWorking = value;
                    NotifyOfPropertyChange(() => IsWorking);
                }
            }
        }

        private TLObject _messageGeo;

        public TLObject MessageGeo
        {
            get { return _messageGeo; }
            set
            {
                if (_messageGeo != value)
                {
                    _messageGeo = value;
                    NotifyOfPropertyChange(() => MessageGeo);
                    NotifyOfPropertyChange(() => FooterMessage);
                }
            }
        }

        private TLMessageBase _messageGeoLive;

        public TLMessageBase MessageGeoLive
        {
            get { return _messageGeoLive; }
            set
            {
                if (_messageGeoLive != value)
                {
                    _messageGeoLive = value;
                    NotifyOfPropertyChange(() => MessageGeoLive);
                    NotifyOfPropertyChange(() => FooterMessage);
                }
            }
        }

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            protected set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    NotifyOfPropertyChange(() => IsOpen);
                }
            }
        }

        public string PoweredBy
        {
            get { return "Foursquare"; }
        }

        private readonly INavigationService _navigationService;

        private readonly ICacheService _cacheService;

        public IStateService StateService { get; protected set; }

        public Action<TLMessageMediaBase> ContinueAction { get; set; }

        public Action<TLMessage70, System.Action> StopLiveLocationAction { get; set; }

        public Action<Action<TLMessagesBase>, Action<TLRPCError>> UpdateLiveLocationsAction { get; set; }

        public MapViewModel(ICacheService cacheService, IStateService stateService, INavigationService navigationService)
        {
            Venues = new ObservableCollection<TLMessageMediaVenue>();
            LiveLocations = new List<TLMessageBase>();

            StateService = stateService;
            _cacheService = cacheService;
            _navigationService = navigationService;
        }

        ~MapViewModel()
        {

        }

        protected override void OnActivate()
        {
            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                _navigationService.RemoveBackEntry();
            }

            if (StateService.MediaMessage != null)
            {
                MessageGeo = StateService.MediaMessage;
                StateService.MediaMessage = null;
            }

            if (StateService.DecryptedMediaMessage != null)
            {
                var geoPoint = StateService.DecryptedMediaMessage.Media as TLDecryptedMessageMediaGeoPoint;
                if (geoPoint == null) return;

                MessageGeo = new TLMessage17
                {
                    Media = new TLMessageMediaGeo
                    {
                        Geo = new TLGeoPoint
                        {
                            Lat = geoPoint.Lat,
                            Long = geoPoint.Long
                        }
                    },
                    FromId = StateService.DecryptedMediaMessage.FromId,
                };
                StateService.DecryptedMediaMessage = null;
            }

            base.OnActivate();
        }

        public void Cancel()
        {
            _navigationService.GoBack();
        }

        public void AttachVenue(TLMessageMediaVenue mediaVenue)
        {
            if (mediaVenue == null) return;

            CloseEditor(true);
            ContinueAction.SafeInvoke(mediaVenue);
        }

        public void AttchLocation(GeoCoordinate location)
        {
            if (location == null) return;
            if (location.Latitude == 0.0 && location.Longitude == 0.0) return;

            var mediaGeo = new TLMessageMediaGeo
            {
                Geo = new TLGeoPoint { Lat = new TLDouble(location.Latitude), Long = new TLDouble(location.Longitude) }
            };

            CloseEditor();
            ContinueAction.SafeInvoke(mediaGeo);
        }

        public void AttachGeoLive(GeoCoordinate location, TimerSpan timer)
        {
            if (location == null) return;
            if (location.Latitude == 0.0 && location.Longitude == 0.0) return;

            var mediaGeoLive = new TLMessageMediaGeoLive
            {
                Geo = new TLGeoPoint { Lat = new TLDouble(location.Latitude), Long = new TLDouble(location.Longitude) },
                Period = new TLInt(timer.Seconds)
            };

            CloseEditor();
            ContinueAction.SafeInvoke(mediaGeoLive);
        }

        public void ShowMapsDirections()
        {
#if WP8
            if (MessageGeo == null) return;

            var message = MessageGeo as TLMessage;
            if (message != null)
            {
                var mediaVenue = message.Media as TLMessageMediaVenue;
                if (mediaVenue != null)
                {
                    var label = mediaVenue.Title.ToString();
                    if (string.IsNullOrEmpty(label)) return;

                    var geoPoint = mediaVenue.Geo as TLGeoPoint;
                    if (geoPoint == null) return;

                    var task = new MapsDirectionsTask
                    {
                        End = new LabeledMapLocation(label, new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value))
                    };

                    task.Show();

                    return;
                }

                var mediaGeo = message.Media as TLMessageMediaGeo;
                if (mediaGeo != null)
                {
                    var label = GetLabel();
                    if (string.IsNullOrEmpty(label)) return;

                    var geoPoint = mediaGeo.Geo as TLGeoPoint;
                    if (geoPoint == null) return;

                    var task = new MapsDirectionsTask
                    {
                        End = new LabeledMapLocation(label, new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value))
                    };

                    task.Show();
                }
            }

            var decryptedMessage = MessageGeo as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                var mediaVenue = decryptedMessage.Media as TLDecryptedMessageMediaVenue;
                if (mediaVenue != null)
                {
                    var label = mediaVenue.Title.ToString();
                    if (string.IsNullOrEmpty(label)) return;

                    var geoPoint = mediaVenue.Geo as TLGeoPoint;
                    if (geoPoint == null) return;

                    var task = new MapsDirectionsTask
                    {
                        End = new LabeledMapLocation(label, new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value))
                    };

                    task.Show();

                    return;
                }

                var mediaGeo = decryptedMessage.Media as TLDecryptedMessageMediaGeoPoint;
                if (mediaGeo != null)
                {
                    var label = GetLabel();
                    if (string.IsNullOrEmpty(label)) return;

                    var geoPoint = mediaGeo.Geo as TLGeoPoint;
                    if (geoPoint == null) return;

                    var task = new MapsDirectionsTask
                    {
                        End = new LabeledMapLocation(label, new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value))
                    };

                    task.Show();
                }
            }
#endif
        }

        private string GetLabel()
        {
            var message = MessageGeo as TLMessage;
            if (message != null)
            {
                var fwdHeader = message.FwdFrom as TLMessageFwdHeader70;
                if (fwdHeader != null)
                {
                    var fwdChannel = fwdHeader.From as TLChatBase;
                    if (fwdChannel != null)
                    {
                        return fwdChannel.FullName2;
                    }

                    var fwdUser = fwdHeader.From as TLUserBase;
                    if (fwdUser != null)
                    {
                        return fwdUser.FullName2;
                    }
                }

                var user = message.From as TLUserBase;
                if (user != null)
                {
                    return user.FullName2;
                }

                var chat = message.From as TLChatBase;
                if (chat != null)
                {
                    return chat.FullName2;
                }
            }

            var decryptedMessage = MessageGeo as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                var user = decryptedMessage.From as TLUserBase;
                if (user != null)
                {
                    return user.FullName2;
                }
            }

            return string.Empty;
        }

        public ObservableCollection<TLMessageMediaVenue> Venues { get; set; }

        public TimerSpan TimerSpan { get; set; }

        private GeoCoordinate _previousLocaiton;

        private TLUserBase _foursquareBot;

        public void GetVenues(GeoCoordinate location)
        {
            if (_previousLocaiton != null)
            {
                if (_previousLocaiton.GetDistanceTo(location) < 100.0)
                {
                    return;
                }
            }

            if (_foursquareBot == null)
            {
                IoC.Get<IMTProtoService>().ResolveUsernameAsync(new TLString("foursquare"),
                    result =>
                    {
                        if (result.Peer is TLPeerUser && result.Users.Count > 0)
                        {
                            _foursquareBot = result.Users[0];
                            GetVenues(location);
                        }
                    });

                return;
            }
            IsWorking = true;
            Status = Venues.Count > 0 ? Status : AppResources.Loading;
            IoC.Get<IMTProtoService>().GetInlineBotResultsAsync(
                _foursquareBot.ToInputUser(),
                new TLInputPeerSelf(),
                new TLInputGeoPoint { Lat = new TLDouble(location.Latitude), Long = new TLDouble(location.Longitude) },
                TLString.Empty, TLString.Empty,
                result =>
                {
                    _previousLocaiton = location;

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
                            iconSource = new Uri(string.Format(Constants.FoursquireCategoryIconUrl, inlineMessageMediaVenue.VenueType, 64));
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
                        Venues.Clear();
                        foreach (var venue in venues)
                        {
                            Venues.Add(venue);
                        }

                        IsWorking = false;
                        Status = Venues.Count > 0 ? AppResources.NearbyPlaces : AppResources.NoResults;
                    });
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Venues.Clear();
                    Status = AppResources.NoResults;
                }));
        }

        public void OpenContactDetails()
        {
            var message = MessageGeo as TLMessage70;
            if (message != null)
            {
                if (message.Media is TLMessageMediaVenue) return;

                var fwdHeader = message.FwdHeader as TLMessageFwdHeader70;
                if (fwdHeader != null)
                {
                    var fwdChannel = fwdHeader.From as TLChatBase;
                    if (fwdChannel != null)
                    {
                        StateService.CurrentChat = fwdChannel;
                        _navigationService.UriFor<ChatViewModel>().Navigate();
                        return;
                    }

                    var fwdUser = fwdHeader.From as TLUserBase;
                    if (fwdUser != null)
                    {
                        StateService.CurrentContact = fwdUser;
                        _navigationService.UriFor<ContactViewModel>().Navigate();
                    }

                    return;
                }

                StateService.CurrentContact = message.From as TLUserBase;
                _navigationService.UriFor<ContactViewModel>().Navigate();
            }

            var decryptedMessage = MessageGeo as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                if (decryptedMessage.Media is TLDecryptedMessageMediaVenue) return;

                StateService.CurrentContact = decryptedMessage.From as TLUserBase;
                _navigationService.UriFor<ContactViewModel>().Navigate();
            }
        }

        public void SearchLocation(GeoCoordinate location)
        {
            StateService.GeoCoordinate = location;
            _navigationService.UriFor<SearchVenuesViewModel>().Navigate();
        }

        public void OpenEditor()
        {
            IsOpen = true;
        }

        public void CloseEditor(bool force = false)
        {
            var view = GetView() as MapView;
            if (view != null && !force)
            {
                var searchView = view.SearchPlaceholder.Content as SearchVenuesView;
                if (searchView != null)
                {
                    view.SearchPlaceholder.Content = null;
                    return;
                }
                if (view.MorePanel.Visibility == Visibility.Visible)
                {
                    view.AppBarPanel.Close();
                    return;
                }
            }

            IsOpen = false;
        }

        public IList<TLMessageBase> LiveLocations { get; set; }

        public void UpdateLiveLocations()
        {
            if (UpdateLiveLocationsAction == null) return;

            UpdateLiveLocationsAction(
                result =>
                {
                    var messages = result as TLMessages;
                    if (messages != null)
                    {
                        LiveLocations.Clear();
                        foreach (var m in messages.Messages)
                        {
                            LiveLocations.Add(m);
                        }

                        var mapView = GetView() as MapView;
                        if (mapView != null)
                        {
                            mapView.UpdateLiveLocations();
                        }
                    }
                },
                error =>
                {

                });
        }

        public void UpdateLiveLocation(TLMessage message)
        {
            if (MessageGeo == null) return;

            var mapView = GetView() as MapView;
            if (mapView != null)
            {
                mapView.UpdateLiveLocation(message as TLMessage48);
            }
        }

        public void StopLiveLocation()
        {
            var message = MessageGeoLive as TLMessage70;
            if (message == null) return;

            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            CloseEditor();
            StopLiveLocationAction.SafeInvoke(
                message,
                () =>
                {
                    NotifyOfPropertyChange(() => FooterMessage);
                });
        }

        public void OnPositionChanged(GeoPosition<GeoCoordinate> position)
        {
            if (!position.Location.IsUnknown)
            {
                var message = MessageGeoLive as TLMessage70;
                if (message != null)
                {
                    var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                    if (mediaGeoLive != null && mediaGeoLive.Active)
                    {
                        var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
                        if (geoPoint != null)
                        {
                            var oldLocation = new GeoCoordinate(geoPoint.Lat.Value, geoPoint.Long.Value);
                            if (oldLocation.GetDistanceTo(position.Location) >= Constants.MinDistanceToUpdateLiveLocation)
                            {
                                var liveLocationService = IoC.Get<ILiveLocationService>();
                                liveLocationService.UpdateAsync(message,
                                    new TLGeoPoint
                                    {
                                        Lat = new TLDouble(position.Location.Latitude),
                                        Long = new TLDouble(position.Location.Longitude)
                                    },
                                    result => Execute.BeginOnUIThread(() =>
                                    {
                                        mediaGeoLive.EditDate = message.EditDate;
                                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);
                                    }));
                            }
                        }
                    }
                }
            }
        }

        public void LocationSharingCompleted()
        {
            NotifyOfPropertyChange(() => MessageGeoLive);
            NotifyOfPropertyChange(() => FooterMessage);
        }

        public Action<bool> ParentHitTest;

        public void RestoreParentHitTest(bool restore)
        {
            ParentHitTest.SafeInvoke(restore);
        }
    }
}
