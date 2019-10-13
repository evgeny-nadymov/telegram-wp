// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Media;
using Action = System.Action;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
        public MapViewModel LocationPicker { get; set; }

        private void SendLocation()
        {
            if (LocationPicker == null)
            {
                LocationPicker = IoC.Get<MapViewModel>();
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = null;
                LocationPicker.MessageGeoLive = new TLMessageEmpty();
                LocationPicker.ContinueAction = ContinueSendLocation;
                LocationPicker.StopLiveLocationAction = StopLiveLocation;
                NotifyOfPropertyChange(() => LocationPicker);
            }
            else
            {
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = null;
                LocationPicker.MessageGeoLive = new TLMessageEmpty();
                BeginOnUIThread(() => LocationPicker.OpenEditor());
            }
        }

        private void OpenLocation(TLDecryptedMessage message)
        {
            if (LocationPicker == null)
            {
                LocationPicker = IoC.Get<MapViewModel>();
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = message;
                LocationPicker.MessageGeoLive = new TLMessageEmpty();
                LocationPicker.ContinueAction = ContinueSendLocation;
                LocationPicker.StopLiveLocationAction = StopLiveLocation;
                NotifyOfPropertyChange(() => LocationPicker);
            }
            else
            {
                LocationPicker.LiveLocations.Clear();
                LocationPicker.With = With;
                LocationPicker.MessageGeo = message;
                LocationPicker.MessageGeoLive = new TLMessageEmpty();
                BeginOnUIThread(() => LocationPicker.OpenEditor());
            }
        }

        private void StopLiveLocation(TLMessage arg1, Action arg2)
        {
            
        }

        private void ContinueSendLocation(TLMessageMediaBase mediaBase)
        {
            var mediaVenue = mediaBase as TLMessageMediaVenue;
            if (mediaVenue != null)
            {
                var chat = Chat as TLEncryptedChat17;
                if (chat != null && chat.Layer.Value >= Constants.MinSecretChatWithVenuesLayer)
                {
                    SendVenue(mediaVenue);
                }
                else
                {
                    SendLocation(mediaVenue);
                }

                return;
            }

            var mediaGeo = mediaBase as TLMessageMediaGeo;
            if (mediaGeo != null)
            {
                SendLocation(mediaGeo);
                return;
            }
        }

        private void SendLocation(TLMessageMediaGeo mediaGeo)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            if (mediaGeo == null) return;

            var geoPoint = mediaGeo.Geo as TLGeoPoint;
            if (geoPoint == null) return;

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty, new TLDecryptedMessageMediaGeoPoint { Lat = geoPoint.Lat, Long = geoPoint.Long }, chat);

            BeginOnUIThread(() =>
            {
                InsertSendingMessage(decryptedTuple.Item1);
                RaiseScrollToBottom();
                NotifyOfPropertyChange(() => DescriptionVisibility);

                SendEncrypted(chat, decryptedTuple.Item2, MTProtoService, CacheService);
            });
        }

        private void SendVenue(TLMessageMediaVenue venue)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            var decryptedTuple = GetDecryptedMessageAndObject(TLString.Empty,
                new TLDecryptedMessageMediaVenue
                {
                    Lat = ((TLGeoPoint) venue.Geo).Lat,
                    Long = ((TLGeoPoint) venue.Geo).Long,
                    Title = venue.Title,
                    Address = venue.Address,
                    Provider = venue.Provider,
                    VenueId = venue.VenueId
                }, 
                chat);

            BeginOnUIThread(() =>
            {
                InsertSendingMessage(decryptedTuple.Item1);
                RaiseScrollToBottom();
                NotifyOfPropertyChange(() => DescriptionVisibility);

                SendEncrypted(chat, decryptedTuple.Item2, MTProtoService, CacheService);
            });
        }
    }
}
