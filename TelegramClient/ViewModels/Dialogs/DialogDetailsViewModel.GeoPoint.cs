// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Location;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public LiveLocationBadgeViewModel LiveLocationBadge { get; set; }

        public MapViewModel LocationPicker { get; set; }

        private void SendLocation()
        {
            var liveLocationsService = IoC.Get<ILiveLocationService>();
            var messageGeoLive = CurrentDialog != null ? liveLocationsService.Get(CurrentDialog.Peer, MTProtoService.CurrentUserId) : null;
            if (messageGeoLive != null)
            {
                var cachedMessage = CacheService.GetMessage(messageGeoLive.Id, With is TLChannel ? ((TLChannel)With).Id : null) as TLMessage;
                if (cachedMessage != null)
                {
                    messageGeoLive = cachedMessage;
                }
            }

            if (LocationPicker == null)
            {
                LocationPicker = IoC.Get<MapViewModel>();
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = null;
                LocationPicker.MessageGeoLive = messageGeoLive;
                LocationPicker.ContinueAction = ContinueSendLocation;
                LocationPicker.StopLiveLocationAction = StopLiveLocation;
                LocationPicker.UpdateLiveLocationsAction = UpdateLiveLocations;
                NotifyOfPropertyChange(() => LocationPicker);
            }
            else
            {
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = null;
                LocationPicker.MessageGeoLive = messageGeoLive;
                BeginOnUIThread(() => LocationPicker.OpenEditor());
            }
        }

        private void OpenLocation(TLMessage message)
        {
            var liveLocationsService = IoC.Get<ILiveLocationService>();
            var messageGeoLive = CurrentDialog != null ? liveLocationsService.Get(CurrentDialog.Peer, MTProtoService.CurrentUserId) : null;
            if (messageGeoLive != null)
            {
                var cachedMessage = CacheService.GetMessage(messageGeoLive.Id, With is TLChannel ? ((TLChannel) With).Id : null) as TLMessage;
                if (cachedMessage != null)
                {
                    messageGeoLive = cachedMessage;
                }
            }

            if (LocationPicker == null)
            {
                LocationPicker = IoC.Get<MapViewModel>();
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = message;
                LocationPicker.MessageGeoLive = messageGeoLive;
                LocationPicker.ContinueAction = ContinueSendLocation;
                LocationPicker.StopLiveLocationAction = StopLiveLocation;
                LocationPicker.UpdateLiveLocationsAction = UpdateLiveLocations;
                NotifyOfPropertyChange(() => LocationPicker);
            }
            else
            {
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = message;
                LocationPicker.MessageGeoLive = messageGeoLive;
                BeginOnUIThread(() => LocationPicker.OpenEditor());
            }
        }

        private void UpdateLiveLocations(Action<TLMessagesBase> callback, Action<TLRPCError> faultCallback = null)
        {
            MTProtoService.GetRecentLocationsAsync(Peer, new TLInt(int.MaxValue), new TLInt(0), 
                result => Execute.BeginOnUIThread(() =>
                {
                    if (LiveLocationBadge != null)
                    {
                        LiveLocationBadge.UpdateLiveLocations(result.Messages);
                    }

                    callback.SafeInvoke(result);
                }),
                faultCallback);
        }

        private void ContinueSendLocation(TLMessageMediaBase mediaBase)
        {
            var mediaVenue = mediaBase as TLMessageMediaVenue;
            if (mediaVenue != null)
            {
                SendVenue(mediaVenue);
                return;
            }

            var mediaGeoLive = mediaBase as TLMessageMediaGeoLive;
            if (mediaGeoLive != null)
            {
                SendLiveLocation(mediaGeoLive);
                return;
            }

            var mediaGeo = mediaBase as TLMessageMediaGeo;
            if (mediaGeo != null)
            {
                SendLocation(mediaGeo);
                return;
            }
        }

        private void SendVenue(TLMessageMediaVenue venue)
        {
            var message = GetMessage(TLString.Empty, venue);

            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        SendVenueInternal));
            });
        }

        private void SendVenueInternal(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage34;
            if (message == null) return;

            var mediaVenue = message.Media as TLMessageMediaVenue;
            if (mediaVenue == null) return;

            var geoPoint = mediaVenue.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var inputMediaVenue = new TLInputMediaVenue
            {
                Title = mediaVenue.Title,
                Address = mediaVenue.Address,
                Provider = mediaVenue.Provider,
                VenueId = mediaVenue.VenueId,
                GeoPoint = new TLInputGeoPoint { Lat = ((TLGeoPoint)mediaVenue.Geo).Lat, Long = ((TLGeoPoint)mediaVenue.Geo).Long }
            };

            message.InputMedia = inputMediaVenue;

            UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
        }

        private void SendLiveLocation(TLMessageMediaGeoLive mediaGeoLive)
        {
            var message = GetMessage(TLString.Empty, mediaGeoLive);

            mediaGeoLive.Date = message.Date;
            mediaGeoLive.From = message.From;

            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                LiveLocationBadge = new LiveLocationBadgeViewModel(IoC.Get<ILiveLocationService>(), IoC.Get<ICacheService>(), false) { Message = message };
                LiveLocationBadge.OpenMessage += OpenLiveLocationBadge;
                LiveLocationBadge.Closed += CloseLiveLocationBadge;

                NotifyOfPropertyChange(() => LiveLocationBadge);

                UpdateLiveLocations(
                    result => 
                    {

                    },
                    error =>
                    {
                        
                    });

                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        SendLiveLocationInternal));
            });
        }

        private void CloseLiveLocationBadge(object sender, System.EventArgs e)
        {
            if (LiveLocationBadge == null) return;
            var message = LiveLocationBadge.Message as TLMessage70;
            if (message == null) return;

            var confirmation = string.Empty;
            var chat = With as TLChatBase;
            var user = With as TLUserBase;
            if (chat != null)
            {
                confirmation = string.Format(AppResources.StopLiveLocationAlertToGroup, chat.FullName2);
            }
            else if (user != null)
            {
                confirmation = string.Format(AppResources.StopLiveLocationAlertToUser, user.FullName2);
            }

            if (string.IsNullOrEmpty(confirmation)) return;

            var result = MessageBox.Show(confirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;

            StopLiveLocation(message,
                () =>
                {
                    
                });
        }

        private void OpenLiveLocationBadge(object sender, System.EventArgs e)
        {
            if (LiveLocationBadge == null) return;

            var messageGeoLive = LiveLocationBadge.Message;
            if (messageGeoLive == null) return;

            var mediaGeoLive = LiveLocationBadge.Message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            OpenMedia(messageGeoLive);
        }

        private void SendLocation(TLMessageMediaGeo mediaGeo)
        {
            var message = GetMessage(TLString.Empty, mediaGeo);

            BeginOnUIThread(() =>
            {
                var previousMessage = InsertSendingMessage(message);
                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                BeginOnThreadPool(() =>
                    CacheService.SyncSendingMessage(
                        message, previousMessage,
                        SendLocationInternal));
            });
        }

        private void SendLocationInternal(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage34;
            if (message == null) return;

            var mediaGeo = message.Media as TLMessageMediaGeo;
            if (mediaGeo == null) return;

            var geoPoint = mediaGeo.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var inputMediaGeoPoint = new TLInputMediaGeoPoint
            {
                GeoPoint = new TLInputGeoPoint { Lat = geoPoint.Lat, Long = geoPoint.Long }
            };

            message.InputMedia = inputMediaGeoPoint;

            UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);
        }

        private DispatcherTimer _timer;

        private TLMessageBase _timerMessage;

        private void SendLiveLocationInternal(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage34;
            if (message == null) return;

            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var inputMediaGeoLive = new TLInputMediaGeoLive
            {
                GeoPoint = new TLInputGeoPoint { Lat = geoPoint.Lat, Long = geoPoint.Long },
                Period = mediaGeoLive.Period
            };

            message.InputMedia = inputMediaGeoLive;

            UploadService.SendMediaInternal(message, MTProtoService, StateService, CacheService);

            return;
            Execute.BeginOnUIThread(() =>
            {
                _timer = _timer ?? new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(5.0);
                _timer.Tick += GeoLive_OnTick;

                _timerMessage = message;
                _timer.Start();
            });
        }

        public void LiveLocationCompleted(TLObject obj)
        {
            var mediaGeoLive = obj as TLMessageMediaGeoLive;
            if (mediaGeoLive != null)
            {
                if (LiveLocationBadge != null)
                {
                    var message = LiveLocationBadge.Message;
                    if (message != null && message.Media == mediaGeoLive)
                    {
                        LiveLocationBadge = null;
                        NotifyOfPropertyChange(() => LiveLocationBadge);
                    }
                }

                mediaGeoLive.Period = new TLInt(0);
                mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);
            }
        }

        private void StopLiveLocation(TLMessage70 message, System.Action callback)
        {
            if (message == null) return;

            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var newGeoPoint = new TLGeoPointEmpty();

            var liveLocationsService = IoC.Get<ILiveLocationService>();

            liveLocationsService.UpdateAsync(message, newGeoPoint, result =>
                Execute.BeginOnUIThread(() =>
                {
                    LiveLocationBadge = null;
                    NotifyOfPropertyChange(() => LiveLocationBadge);

                    mediaGeoLive.Date = message.Date;
                    mediaGeoLive.EditDate = message.EditDate;
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error == null || error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        LiveLocationBadge = null;
                        NotifyOfPropertyChange(() => LiveLocationBadge);

                        mediaGeoLive.Date = message.Date;
                        mediaGeoLive.EditDate = message.EditDate;
                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                        mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);

                        callback.SafeInvoke();
                    }
                }));
        }

        private void GeoLive_OnTick(object sender, System.EventArgs e)
        {
            if (_timerMessage == null)
            {
                _timer.Stop();
                return;
            }

            var peer = With as IInputPeer;
            if (peer == null) return;

            var message = _timerMessage as TLMessage70;
            if (message == null) return;

            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
            if (mediaGeoLive == null) return;

            var geoPoint = mediaGeoLive.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var newGeoPoint = new TLGeoPoint { Lat = new TLDouble(geoPoint.Lat.Value - 0.001 / 6.0), Long = new TLDouble(geoPoint.Long.Value - 0.001538762512 / 6.0) };

            var liveLocationsService = IoC.Get<ILiveLocationService>();

            liveLocationsService.UpdateAsync(message, newGeoPoint, result =>
                Execute.BeginOnUIThread(() =>
                {
                    mediaGeoLive.EditDate = message.EditDate;
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                    mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);

                    StopEditMessage();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    _timer.Stop();
                }));
        }
    }
}
