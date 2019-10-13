// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Telegram.Api;
using Telegram.Api.TL;

namespace TelegramClient.Helpers.TemplateSelectors
{
    public class DecryptedWebPageMediaTemplateSelector : IValueConverter
    {
        public DataTemplate EmptyTemplate { get; set; }

        public DataTemplate WebPageTextTemplate { get; set; }

        public DataTemplate WebPageTemplate { get; set; }

        public DataTemplate WebPageSmallPhotoTemplate { get; set; }

        public DataTemplate WebPagePhotoTemplate { get; set; }

        public DataTemplate WebPagePendingTemplate { get; set; }

        public DataTemplate WebPageGifTemplate { get; set; }

        public DataTemplate UnsupportedTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var webPageMedia = value as TLDecryptedMessageMediaWebPage;
            if (webPageMedia != null)
            {
                var webPageEmpty = webPageMedia.WebPage as TLWebPageEmpty;
                if (webPageEmpty != null)
                {
                    return EmptyTemplate;
                }

                var webPagePending = webPageMedia.WebPage as TLWebPagePending;
                if (webPagePending != null)
                {
                    return EmptyTemplate;
                }

                var webPage = webPageMedia.WebPage as TLWebPage;
                if (webPage != null)
                {
                    var webPage35 = webPage as TLWebPage35;
                    if (webPage35 != null)
                    {
                        if (TLMessageBase.IsGif(webPage35.Document))
                        {
                            return WebPageGifTemplate;
                        }
                    }

                    if (webPage.Photo != null
                        && webPage.Type != null)
                    {
                        if (MediaTemplateSelector.IsWebPagePhotoTemplate(webPage))
                        {
                            return WebPagePhotoTemplate;
                        }

                        return WebPageSmallPhotoTemplate;
                    }

                    return WebPageTemplate;
                }

                return WebPageTextTemplate;
            }

            return UnsupportedTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DecryptedMediaTemplateSelector : IValueConverter
    {
        public DataTemplate StickerTemplate { get; set; }

        public DataTemplate EmptyTemplate { get; set; }

        public DataTemplate ContactTemplate { get; set; }

        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate SecretPhotoTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public DataTemplate GeoPointTemplate { get; set; }

        public DataTemplate VenueTemplate { get; set; }

        public DataTemplate DocumentTemplate { get; set; }

        public DataTemplate GifTemplate { get; set; }

        public DataTemplate AudioTemplate { get; set; }

        public DataTemplate WebPageTemplate { get; set; }

        public DataTemplate GroupTemplate { get; set; }

        public DataTemplate UnsupportedTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TLInt ttl = null;
            var useSecretPhotos = false;
            var decryptedMessage = value as TLDecryptedMessage;
            if (decryptedMessage != null)
            {
                ttl = decryptedMessage.TTL;
                useSecretPhotos = decryptedMessage is TLDecryptedMessage17;
                value = decryptedMessage.Media;
            }

            var emptyMedia = value as TLDecryptedMessageMediaEmpty;
            if (emptyMedia != null)
            {
                return EmptyTemplate;
            }

            var contactMedia = value as TLDecryptedMessageMediaContact;
            if (contactMedia != null)
            {
                return ContactTemplate;
            }

            var groupMedia = value as TLDecryptedMessageMediaGroup;
            if (groupMedia != null)
            {
                return GroupTemplate;
            }

            var photoMedia = value as TLDecryptedMessageMediaPhoto;
            if (photoMedia != null)
            {
                if (ttl != null && ttl.Value > 0 && ttl.Value <= 60.0 && useSecretPhotos)
                {
                    return SecretPhotoTemplate;
                }

                return PhotoTemplate;
            }

            var documentMedia = value as TLDecryptedMessageMediaDocument;
            if (documentMedia != null)
            {
                var document = documentMedia.Document as TLDecryptedMessageMediaDocument45;
                if (document != null)
                {
                    if (TLMessageBase.IsSticker(document, document.Size))
                    {
                        return StickerTemplate;
                    }

                    if (TLMessageBase.IsVoice(document, document.Size))
                    {
                        return AudioTemplate;
                    }

                    if (TLMessageBase.IsVideo(document, document.Size))
                    {
                        return VideoTemplate;
                    }

                    if (TLMessageBase.IsGif(document, document.Size))
                    {
                        return GifTemplate;
                    }
                }


                //var documentExternal = documentMedia.Document as TLDocumentExternal;
                //if (documentExternal != null)
                //{
                //    if (TLMessageBase.IsGif(documentExternal))
                //    {
                //        return GifTemplate;
                //    }
                //}

                return DocumentTemplate;
            }

            var documentExternal = value as TLDecryptedMessageMediaExternalDocument;
            if (documentExternal != null)
            {
                if (TLMessageBase.IsSticker(documentExternal, documentExternal.Size))
                {
                    return StickerTemplate;
                }
            }

            var videoMedia = value as TLDecryptedMessageMediaVideo;
            if (videoMedia != null)
            {
                return VideoTemplate;
            }

            var venueMedia = value as TLDecryptedMessageMediaVenue;
            if (venueMedia != null)
            {
                return VenueTemplate;
            }

            var geoPointMedia = value as TLDecryptedMessageMediaGeoPoint;
            if (geoPointMedia != null)
            {
                return GeoPointTemplate;
            }

            var audioMedia = value as TLDecryptedMessageMediaAudio;
            if (audioMedia != null)
            {
                return AudioTemplate;
            }

            var webPageMedia = value as TLDecryptedMessageMediaWebPage;
            if (webPageMedia != null)
            {
                var webPageEmpty = webPageMedia.WebPage as TLWebPageEmpty;
                if (webPageEmpty != null)
                {
                    return EmptyTemplate;
                }

                var webPagePending = webPageMedia.WebPage as TLWebPagePending;
                if (webPagePending != null)
                {
                    return EmptyTemplate;
                }

                var webPage = webPageMedia.WebPage as TLWebPage;
                if (webPage != null)
                {
                    return WebPageTemplate;
                }

                return EmptyTemplate;
            }

            return UnsupportedTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaTemplateSelector : IValueConverter
    {
        public DataTemplate CallTemplate { get; set; }

        public DataTemplate StickerTemplate { get; set; }

        public DataTemplate VenueTemplate { get; set; }

        public DataTemplate WebPageTemplate { get; set; }

        public DataTemplate WebPageSmallPhotoTemplate { get; set; }

        public DataTemplate WebPagePhotoTemplate { get; set; }

        public DataTemplate WebPagePendingTemplate { get; set; }

        public DataTemplate WebPageGifTemplate { get; set; }

        public DataTemplate EmptyTemplate { get; set; }

        public DataTemplate ContactTemplate { get; set; }

        public DataTemplate PhoneContactTemplate { get; set; }

        public DataTemplate SecretPhotoTemplate { get; set; }

        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate RoundVideoTemplate { get; set; }

        public DataTemplate SecretVideoTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public DataTemplate GeoLiveTemplate { get; set; }

        public DataTemplate GeoTemplate { get; set; }

        public DataTemplate DocumentTemplate { get; set; }

        public DataTemplate AudioTemplate { get; set; }

        public DataTemplate GifTemplate { get; set; }

        public DataTemplate GameTemplate { get; set; }

        public DataTemplate GameGifTemplate { get; set; }

        public DataTemplate InvoicePhotoTemplate { get; set; }

        public DataTemplate InvoiceTemplate { get; set; }

        public DataTemplate GroupTemplate { get; set; }

        public DataTemplate UnsupportedTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageService = value as TLMessageService;
            if (messageService != null)
            {
                return CallTemplate;
            }

            var emptyMedia = value as TLMessageMediaEmpty;
            if (emptyMedia != null)
            {
                return EmptyTemplate;
            }

            var contactMedia = value as TLMessageMediaContact;
            if (contactMedia != null)
            {
                if (contactMedia.UserId.Value <= 0)
                {
                    return PhoneContactTemplate;
                }

                return ContactTemplate;
            }

            var groupMedia = value as TLMessageMediaGroup;
            if (groupMedia != null)
            {
                return GroupTemplate;
            }

            var photoMedia = value as TLMessageMediaPhoto;
            if (photoMedia != null)
            {
                if (TLMessageBase.HasTTL(photoMedia) && photoMedia.Photo != null)
                {
                    return SecretPhotoTemplate;
                }

                return PhotoTemplate;
            }

            var documentMedia = value as TLMessageMediaDocument;
            if (documentMedia != null)
            {
                var document = documentMedia.Document as TLDocument22;
                if (document != null)
                {
                    if (TLMessageBase.HasTTL(documentMedia))
                    {
                        return SecretVideoTemplate;
                    }
                    if (TLMessageBase.IsSticker(document))
                    {
                        return StickerTemplate;
                    }
                    if (TLMessageBase.IsVoice(document))
                    {
                        return AudioTemplate;
                    }
                    if (TLMessageBase.IsRoundVideo(document))
                    {
                        return RoundVideoTemplate;
                    }
                    if (TLMessageBase.IsVideo(document))
                    {
                        return VideoTemplate;
                    }
                    if (TLMessageBase.IsGif(document))
                    {
                        return GifTemplate;
                    }
                }

                var documentExternal = documentMedia.Document as TLDocumentExternal;
                if (documentExternal != null)
                {
                    if (TLMessageBase.IsGif(documentExternal))
                    {
                        return GifTemplate;
                    }
                }

                return DocumentTemplate;
            }

            var videoMedia = value as TLMessageMediaVideo;
            if (videoMedia != null)
            {
                return VideoTemplate;
            }

            var geoLiveMedia = value as TLMessageMediaGeoLive;
            if (geoLiveMedia != null)
            {
                return GeoLiveTemplate;
            }

            var venueMedia = value as TLMessageMediaVenue;
            if (venueMedia != null)
            {
                return VenueTemplate;
            }

            var geoMedia = value as TLMessageMediaGeo;
            if (geoMedia != null)
            {
                return GeoTemplate;
            }

            var audioMedia = value as TLMessageMediaAudio;
            if (audioMedia != null)
            {
                return AudioTemplate;
            }

            var webPageMedia = value as TLMessageMediaWebPage;
            if (webPageMedia != null)
            {
                var webPageEmpty = webPageMedia.WebPage as TLWebPageEmpty;
                if (webPageEmpty != null)
                {
                    return EmptyTemplate;
                }

                var webPagePending = webPageMedia.WebPage as TLWebPagePending;
                if (webPagePending != null)
                {
                    return EmptyTemplate;
                }

                var webPage = webPageMedia.WebPage as TLWebPage;
                if (webPage != null)
                {
                    var webPage35 = webPage as TLWebPage35;
                    if (webPage35 != null)
                    {
                        if (TLMessageBase.IsGif(webPage35.Document))
                        {
                            return WebPageGifTemplate;
                        }
                    }

                    if (webPage.Photo != null
                        && webPage.Type != null)
                    {
                        if (IsWebPagePhotoTemplate(webPage))
                        {
                            return WebPagePhotoTemplate;
                        }

                        return WebPageSmallPhotoTemplate;
                    }
                }

                return WebPageTemplate;
            }

            var gameMedia = value as TLMessageMediaGame;
            if (gameMedia != null)
            {
                if (TLMessageBase.IsGif(gameMedia.Document))
                {
                    return GameGifTemplate;
                }

                return GameTemplate;
            }

            var invoiceMedia = value as TLMessageMediaInvoice;
            if (invoiceMedia != null)
            {
                if (invoiceMedia.Photo != null)
                {
                    return InvoicePhotoTemplate;
                }

                return InvoiceTemplate;
            }

            return UnsupportedTemplate;
        }

        public static bool IsWebPagePhotoTemplate(TLWebPage webPage)
        {
            if (webPage.Type != null)
            {
                var type = webPage.Type.ToString();
                if (string.Equals(type, "photo", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type, "video", StringComparison.OrdinalIgnoreCase)
                    || (webPage.SiteName != null && string.Equals(webPage.SiteName.ToString(), "twitter", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaGridTemplateSelector : IValueConverter
    {
        public DataTemplate EmptyTemplate { get; set; }

        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as TLMessage;
            if (message == null)
            {
                return EmptyTemplate;
            }

            value = message.Media;

            var emptyMedia = value as TLMessageMediaEmpty;
            if (emptyMedia != null)
            {
                return EmptyTemplate;
            }

            var photoMedia = value as TLMessageMediaPhoto;
            if (photoMedia != null)
            {
                return PhotoTemplate;
            }

            var documentMedia = value as TLMessageMediaDocument;
            if (documentMedia != null && TLMessageBase.IsVideo(documentMedia.Document))
            {
                return VideoTemplate;
            }

            var videoMedia = value as TLMessageMediaVideo;
            if (videoMedia != null)
            {
                return VideoTemplate;
            }

            return EmptyTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReplyTemplateSelector : IValueConverter
    {
        public DataTemplate GameTemplate { get; set; }

        public DataTemplate WebPageEmptyTemplate { get; set; }

        public DataTemplate WebPagePendingTemplate { get; set; }

        public DataTemplate WebPageTemplate { get; set; }

        public DataTemplate ForwardedMessagesTemplate { get; set; }

        public DataTemplate ForwardEmptyTemplate { get; set; }

        public DataTemplate ForwardTextTemplate { get; set; }

        public DataTemplate ForwardContactTemplate { get; set; }

        public DataTemplate ForwardInvoiceTemplate { get; set; }

        public DataTemplate ForwardGameTemplate { get; set; }

        public DataTemplate ForwardPhotoTemplate { get; set; }

        public DataTemplate ForwardVideoTemplate { get; set; }

        public DataTemplate ForwardRoundVideoTemplate { get; set; }

        public DataTemplate ForwardGeoPointTemplate { get; set; }

        public DataTemplate ForwardLiveGeoPointTemplate { get; set; }

        public DataTemplate ForwardDocumentTemplate { get; set; }

        public DataTemplate ForwardGifTemplate { get; set; }

        public DataTemplate ForwardStickerTemplate { get; set; }

        public DataTemplate ForwardAudioTemplate { get; set; }

        public DataTemplate ForwardVoiceMessageTemplate { get; set; }

        public DataTemplate ForwardUnsupportedTemplate { get; set; }

        public DataTemplate ReplyEmptyTemplate { get; set; }

        public DataTemplate ReplyLoadingTemplate { get; set; }

        public DataTemplate ReplyTextTemplate { get; set; }

        public DataTemplate ReplyContactTemplate { get; set; }

        public DataTemplate ReplyInvoiceTemplate { get; set; }

        public DataTemplate ReplyGameTemplate { get; set; }

        public DataTemplate ReplyPhotoTemplate { get; set; }

        public DataTemplate ReplyVideoTemplate { get; set; }

        public DataTemplate ReplyRoundVideoTemplate { get; set; }

        public DataTemplate ReplyGeoPointTemplate { get; set; }

        public DataTemplate ReplyLiveGeoPointTemplate { get; set; }

        public DataTemplate ReplyDocumentTemplate { get; set; }

        public DataTemplate ReplyGifTemplate { get; set; }

        public DataTemplate ReplyStickerTemplate { get; set; }

        public DataTemplate ReplyAudioTemplate { get; set; }

        public DataTemplate ReplyVoiceMessageTemplate { get; set; }

        public DataTemplate ReplyUnsupportedTemplate { get; set; }

        public DataTemplate ReplyServiceTextTemplate { get; set; }

        public DataTemplate ReplyServicePhotoTemplate { get; set; }

        public DataTemplate EditTextTemplate { get; set; }

        public DataTemplate EditContactTemplate { get; set; }

        public DataTemplate EditPhotoTemplate { get; set; }

        public DataTemplate EditVideoTemplate { get; set; }

        public DataTemplate EditRoundVideoTemplate { get; set; }

        public DataTemplate EditGeoPointTemplate { get; set; }

        public DataTemplate EditLiveGeoPointTemplate { get; set; }

        public DataTemplate EditDocumentTemplate { get; set; }

        public DataTemplate EditGifTemplate { get; set; }

        public DataTemplate EditStickerTemplate { get; set; }

        public DataTemplate EditAudioTemplate { get; set; }

        public DataTemplate EditVoiceMessageTemplate { get; set; }

        public DataTemplate EditUnsupportedTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var info = value as ReplyInfo;
            if (info == null)
            {
                if (value is TLMessageBase)
                {
                    return GetMessageTemplate(value as TLObject);
                }

                return ReplyUnsupportedTemplate;
            }

            if (info.Reply == null)
            {
                return ReplyLoadingTemplate;
            }

            var messagesContainter = info.Reply as TLMessagesContainter;
            if (messagesContainter != null)
            {
                return GetMessagesContainerTemplate(messagesContainter);
            }

            if (info.ReplyToMsgId == null || info.ReplyToMsgId.Value == 0)
            {
                return ReplyUnsupportedTemplate;
            }

            return GetMessageTemplate(info.Reply);
        }

        private DataTemplate GetMessageTemplate(TLObject reply)
        {
            var message = reply as TLMessage;
            if (message != null)
            {
                if (!TLString.IsNullOrEmpty(message.Message)
                    && (message.Media is TLMessageMediaEmpty || message.Media is TLMessageMediaWebPage))
                {
                    return ReplyTextTemplate;
                }

                var value = message.Media;

                if (value is TLMessageMediaInvoice)
                {
                    return ReplyInvoiceTemplate;
                }

                if (value is TLMessageMediaGame)
                {
                    return ReplyGameTemplate;
                }

                if (value is TLMessageMediaEmpty)
                {
                    return ReplyUnsupportedTemplate;
                }

                if (value is TLMessageMediaContact)
                {
                    return ReplyContactTemplate;
                }

                if (value is TLMessageMediaPhoto)
                {
                    return ReplyPhotoTemplate;
                }

                if (value is TLMessageMediaDocument)
                {
                    if (message.IsVoice())
                    {
                        return ReplyVoiceMessageTemplate;
                    }

                    if (message.IsRoundVideo())
                    {
                        return ReplyRoundVideoTemplate;
                    }

                    if (message.IsVideo())
                    {
                        return ReplyVideoTemplate;
                    }

                    if (message.IsGif())
                    {
                        return ReplyGifTemplate;
                    }

                    if (message.IsSticker())
                    {
                        return ReplyStickerTemplate;
                    }

                    return ReplyDocumentTemplate;
                }

                if (value is TLMessageMediaVideo)
                {
                    return ReplyVideoTemplate;
                }

                if (value is TLMessageMediaGeoLive)
                {
                    return ReplyLiveGeoPointTemplate;
                }

                if (value is TLMessageMediaGeo)
                {
                    return ReplyGeoPointTemplate;
                }

                if (value is TLMessageMediaAudio)
                {
                    return ReplyAudioTemplate;
                }
            }

            var messageService = reply as TLMessageService;
            if (messageService != null)
            {
                var serviceAction = messageService.Action;

                if (serviceAction is TLMessageActionChatEditPhoto)
                {
                    return ReplyServicePhotoTemplate;
                }

                return ReplyServiceTextTemplate;
            }

            var emptyMessage = reply as TLMessageEmpty;
            if (emptyMessage != null)
            {
                return ReplyEmptyTemplate;
            }

            return ReplyUnsupportedTemplate;
        }

        private DataTemplate GetMessagesContainerTemplate(TLMessagesContainter container)
        {
            if (container.WebPageMedia != null)
            {
                var webPageMedia = container.WebPageMedia as TLMessageMediaWebPage;
                if (webPageMedia != null)
                {
                    var webPagePending = webPageMedia.WebPage as TLWebPagePending;
                    if (webPagePending != null)
                    {
                        return WebPagePendingTemplate;
                    }

                    var webPage = webPageMedia.WebPage as TLWebPage;
                    if (webPage != null)
                    {
                        return WebPageTemplate;
                    }

                    var webPageEmpty = webPageMedia.WebPage as TLWebPageEmpty;
                    if (webPageEmpty != null)
                    {
                        return WebPageEmptyTemplate;
                    }
                }
            }

            if (container.FwdMessages != null)
            {
                if (container.FwdMessages.Count == 1)
                {
                    var fwdMessage = container.FwdMessages[0];
                    if (fwdMessage != null)
                    {
                        if (!TLString.IsNullOrEmpty(fwdMessage.Message)
                            && (fwdMessage.Media is TLMessageMediaEmpty || fwdMessage.Media is TLMessageMediaWebPage))
                        {
                            return ForwardTextTemplate;
                        }

                        var media = container.FwdMessages[0].Media;
                        if (media != null)
                        {
                            var mediaInvoice = media as TLMessageMediaInvoice;
                            if (mediaInvoice != null)
                            {
                                return ForwardInvoiceTemplate;
                            }

                            var mediaGame = media as TLMessageMediaGame;
                            if (mediaGame != null)
                            {
                                return ForwardGameTemplate;
                            }

                            var mediaPhoto = media as TLMessageMediaPhoto;
                            if (mediaPhoto != null)
                            {
                                return ForwardPhotoTemplate;
                            }

                            var mediaAudio = media as TLMessageMediaAudio;
                            if (mediaAudio != null)
                            {
                                return ForwardAudioTemplate;
                            }

                            var mediaDocument = media as TLMessageMediaDocument;
                            if (mediaDocument != null)
                            {
                                if (fwdMessage.IsVoice())
                                {
                                    return ForwardVoiceMessageTemplate;
                                }

                                if (fwdMessage.IsRoundVideo())
                                {
                                    return ForwardRoundVideoTemplate;
                                }

                                if (fwdMessage.IsVideo())
                                {
                                    return ForwardVideoTemplate;
                                }

                                if (fwdMessage.IsGif())
                                {
                                    return ForwardGifTemplate;
                                }

                                if (fwdMessage.IsSticker())
                                {
                                    return ForwardStickerTemplate;
                                }

                                return ForwardDocumentTemplate;
                            }

                            var mediaVideo = media as TLMessageMediaVideo;
                            if (mediaVideo != null)
                            {
                                return ForwardVideoTemplate;
                            }

                            var mediaGeoLive = media as TLMessageMediaGeoLive;
                            if (mediaGeoLive != null)
                            {
                                return ForwardLiveGeoPointTemplate;
                            }

                            var mediaGeo = media as TLMessageMediaGeo;
                            if (mediaGeo != null)
                            {
                                return ForwardGeoPointTemplate;
                            }

                            var mediaContact = media as TLMessageMediaContact;
                            if (mediaContact != null)
                            {
                                return ForwardContactTemplate;
                            }

                            var mediaEmpty = media as TLMessageMediaEmpty;
                            if (mediaEmpty != null)
                            {
                                return ForwardEmptyTemplate;
                            }

                            return ForwardUnsupportedTemplate;
                        }

                        var text = container.FwdMessages[0].Message;

                        if (!string.IsNullOrEmpty(text.ToString()))
                        {
                            return ForwardTextTemplate;
                        }
                    }
                }

                return ForwardedMessagesTemplate;
            }

            if (container.EditMessage != null)
            {
                var editMessage = container.EditMessage as TLMessage;
                if (editMessage != null)
                {
                    if (!TLString.IsNullOrEmpty(editMessage.Message)
                        && (editMessage.Media is TLMessageMediaEmpty || editMessage.Media is TLMessageMediaWebPage))
                    {
                        return EditTextTemplate;
                    }

                    var value = editMessage.Media;

                    if (value is TLMessageMediaEmpty)
                    {
                        return EditUnsupportedTemplate;
                    }

                    if (value is TLMessageMediaContact)
                    {
                        return EditContactTemplate;
                    }

                    if (value is TLMessageMediaPhoto)
                    {
                        return EditPhotoTemplate;
                    }

                    if (value is TLMessageMediaDocument)
                    {
                        if (editMessage.IsVoice())
                        {
                            return EditVoiceMessageTemplate;
                        }

                        if (editMessage.IsRoundVideo())
                        {
                            return EditRoundVideoTemplate;
                        }

                        if (editMessage.IsVideo())
                        {
                            return EditVideoTemplate;
                        }

                        if (editMessage.IsGif())
                        {
                            return EditGifTemplate;
                        }

                        if (editMessage.IsSticker())
                        {
                            return EditStickerTemplate;
                        }

                        return EditDocumentTemplate;
                    }

                    if (value is TLMessageMediaVideo)
                    {
                        return EditVideoTemplate;
                    }

                    if (value is TLMessageMediaGeoLive)
                    {
                        return EditLiveGeoPointTemplate;
                    }

                    if (value is TLMessageMediaGeo)
                    {
                        return EditGeoPointTemplate;
                    }

                    if (value is TLMessageMediaAudio)
                    {
                        return EditAudioTemplate;
                    }
                }

                return EditUnsupportedTemplate;
            }

            return ReplyUnsupportedTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DecryptedReplyTemplateSelector : IValueConverter
    {
        public DataTemplate WebPageEmptyTemplate { get; set; }

        public DataTemplate WebPagePendingTemplate { get; set; }

        public DataTemplate WebPageTemplate { get; set; }

        public DataTemplate ForwardedMessagesTemplate { get; set; }

        public DataTemplate ForwardEmptyTemplate { get; set; }

        public DataTemplate ForwardTextTemplate { get; set; }

        public DataTemplate ForwardContactTemplate { get; set; }

        public DataTemplate ForwardPhotoTemplate { get; set; }

        public DataTemplate ForwardVideoTemplate { get; set; }

        public DataTemplate ForwardGeoPointTemplate { get; set; }

        public DataTemplate ForwardDocumentTemplate { get; set; }

        public DataTemplate ForwardGifTemplate { get; set; }

        public DataTemplate ForwardStickerTemplate { get; set; }

        public DataTemplate ForwardAudioTemplate { get; set; }

        public DataTemplate ForwardVoiceMessageTemplate { get; set; }

        public DataTemplate ForwardUnsupportedTemplate { get; set; }

        public DataTemplate ReplyEmptyTemplate { get; set; }

        public DataTemplate ReplyLoadingTemplate { get; set; }

        public DataTemplate ReplyTextTemplate { get; set; }

        public DataTemplate ReplyWebPageTemplate { get; set; }

        public DataTemplate ReplyContactTemplate { get; set; }

        public DataTemplate ReplyPhotoTemplate { get; set; }

        public DataTemplate ReplySecretPhotoTemplate { get; set; }

        public DataTemplate ReplyVideoTemplate { get; set; }

        public DataTemplate ReplyGeoPointTemplate { get; set; }

        public DataTemplate ReplyDocumentTemplate { get; set; }

        public DataTemplate ReplyGifTemplate { get; set; }

        public DataTemplate ReplyStickerTemplate { get; set; }

        public DataTemplate ReplyAudioTemplate { get; set; }

        public DataTemplate ReplyVoiceMessageTemplate { get; set; }

        public DataTemplate ReplyUnsupportedTemplate { get; set; }

        public DataTemplate ReplyServiceTextTemplate { get; set; }

        public DataTemplate ReplyServicePhotoTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var info = value as ReplyInfo;
            if (info == null)
            {
                return ReplyUnsupportedTemplate;
            }

            if (info.Reply == null)
            {
                return null;
            }

            var messagesContainter = info.Reply as TLDecryptedMessagesContainter;
            if (messagesContainter != null)
            {
                return GetMessagesContainerTemplate(messagesContainter);
            }

            if (info.ReplyToRandomMsgId == null || info.ReplyToRandomMsgId.Value == 0)
            {
                return ReplyUnsupportedTemplate;
            }

            var messageService = info.Reply as TLDecryptedMessageService;
            if (messageService != null)
            {
                return ReplyServiceTextTemplate;
            }

            var message = info.Reply as TLDecryptedMessage;
            if (message != null)
            {
                var ttl = message.TTL;

                if (!TLString.IsNullOrEmpty(message.Message)
                    && (message.Media is TLDecryptedMessageMediaEmpty || message.Media is TLDecryptedMessageMediaWebPage))
                {
                    return ReplyTextTemplate;
                }

                value = message.Media;

                if (value is TLDecryptedMessageMediaWebPage)
                {
                    return ReplyWebPageTemplate;
                }

                if (value is TLDecryptedMessageMediaEmpty)
                {
                    return ReplyUnsupportedTemplate;
                }

                if (value is TLDecryptedMessageMediaContact)
                {
                    return ReplyContactTemplate;
                }

                if (value is TLDecryptedMessageMediaPhoto)
                {
                    if (ttl != null && ttl.Value > 0 && ttl.Value < 60)
                    {
                        return ReplySecretPhotoTemplate;
                    }

                    return ReplyPhotoTemplate;
                }

                if (value is TLDecryptedMessageMediaDocument)
                {
                    if (message.IsVoice())
                    {
                        return ReplyVoiceMessageTemplate;
                    }

                    if (message.IsVideo())
                    {
                        return ReplyVideoTemplate;
                    }

                    if (message.IsGif())
                    {
                        return ReplyGifTemplate;
                    }

                    return ReplyDocumentTemplate;
                }

                if (value is TLDecryptedMessageMediaExternalDocument)
                {
                    if (message.IsSticker())
                    {
                        return ReplyStickerTemplate;
                    }

                    return ReplyDocumentTemplate;
                }

                if (value is TLDecryptedMessageMediaVideo)
                {
                    return ReplyVideoTemplate;
                }

                if (value is TLDecryptedMessageMediaGeoPoint)
                {
                    return ReplyGeoPointTemplate;
                }

                if (value is TLDecryptedMessageMediaAudio)
                {
                    return ReplyAudioTemplate;
                }
            }

            return ReplyUnsupportedTemplate;
        }

        private DataTemplate GetMessagesContainerTemplate(TLDecryptedMessagesContainter container)
        {
            if (container.WebPageMedia != null)
            {
                var webPageMedia = container.WebPageMedia as TLMessageMediaWebPage;
                if (webPageMedia != null)
                {
                    var webPagePending = webPageMedia.WebPage as TLWebPagePending;
                    if (webPagePending != null)
                    {
                        return WebPagePendingTemplate;
                    }

                    var webPage = webPageMedia.WebPage as TLWebPage;
                    if (webPage != null)
                    {
                        return WebPageTemplate;
                    }

                    var webPageEmpty = webPageMedia.WebPage as TLWebPageEmpty;
                    if (webPageEmpty != null)
                    {
                        return WebPageEmptyTemplate;
                    }
                }
            }

            return ReplyUnsupportedTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InlineBotResultTemplateSelector : IValueConverter
    {
        public DataTemplate ResultTemplate { get; set; }

        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate PhotoResultTemplate { get; set; }

        public DataTemplate GifTemplate { get; set; }

        public DataTemplate GifResultTemplate { get; set; }

        public DataTemplate GeoResultTemplate { get; set; }

        public DataTemplate VenueResultTemplate { get; set; }

        public DataTemplate ContactResultTemplate { get; set; }

        public DataTemplate StickerResultTemplate { get; set; }

        public DataTemplate AudioResultTemplate { get; set; }

        public DataTemplate GameResultTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resultBase = value as TLBotInlineResultBase;
            if (resultBase != null)
            {
                if (resultBase.SendMessage is TLBotInlineMessageMediaContact)
                {
                    return ContactResultTemplate;
                }

                if (resultBase.SendMessage is TLBotInlineMessageMediaVenue)
                {
                    return VenueResultTemplate;
                }

                if (resultBase.SendMessage is TLBotInlineMessageMediaGeo)
                {
                    return GeoResultTemplate;
                }
            }

            var mediaResult = value as TLBotInlineMediaResult;
            if (mediaResult != null)
            {
                if (TLString.Equals(mediaResult.Type, new TLString("photo"), StringComparison.OrdinalIgnoreCase))
                {
                    return PhotoTemplate;
                }

                if (TLString.Equals(mediaResult.Type, new TLString("sticker"), StringComparison.OrdinalIgnoreCase))
                {
                    return StickerResultTemplate;
                }

                if (TLString.Equals(mediaResult.Type, new TLString("gif"), StringComparison.OrdinalIgnoreCase))
                {
                    return GifTemplate;
                }

                if (TLString.Equals(mediaResult.Type, new TLString("audio"), StringComparison.OrdinalIgnoreCase))
                {
                    return AudioResultTemplate;
                }

                if (TLString.Equals(mediaResult.Type, new TLString("voice"), StringComparison.OrdinalIgnoreCase))
                {
                    return AudioResultTemplate;
                }

                if (TLString.Equals(mediaResult.Type, new TLString("game"), StringComparison.OrdinalIgnoreCase))
                {
                    return GameResultTemplate;
                }
            }

            var result = value as TLBotInlineResult;
            if (result != null)
            {
                if (TLString.Equals(result.Type, new TLString("photo"), StringComparison.OrdinalIgnoreCase))
                {
                    return PhotoResultTemplate;
                }

                if (TLString.Equals(result.Type, new TLString("gif"), StringComparison.OrdinalIgnoreCase))
                {
                    return GifResultTemplate;
                }

                if (TLString.Equals(result.Type, new TLString("audio"), StringComparison.OrdinalIgnoreCase))
                {
                    return AudioResultTemplate;
                }

                if (TLString.Equals(result.Type, new TLString("voice"), StringComparison.OrdinalIgnoreCase))
                {
                    return AudioResultTemplate;
                }
            }

            return ResultTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PinnedMessageTemplateSelector : IValueConverter
    {
        public DataTemplate PinnedEmptyTemplate { get; set; }

        public DataTemplate PinnedTextTemplate { get; set; }

        public DataTemplate PinnedContactTemplate { get; set; }

        public DataTemplate PinnedInvoiceTemplate { get; set; }

        public DataTemplate PinnedGameTemplate { get; set; }

        public DataTemplate PinnedPhotoTemplate { get; set; }

        public DataTemplate PinnedVideoTemplate { get; set; }

        public DataTemplate PinnedRoundVideoTemplate { get; set; }

        public DataTemplate PinnedGeoPointTemplate { get; set; }

        public DataTemplate PinnedLiveGeoPointTemplate { get; set; }

        public DataTemplate PinnedDocumentTemplate { get; set; }

        public DataTemplate PinnedGifTemplate { get; set; }

        public DataTemplate PinnedStickerTemplate { get; set; }

        public DataTemplate PinnedAudioTemplate { get; set; }

        public DataTemplate PinnedVoiceMessageTemplate { get; set; }

        public DataTemplate PinnedClearHistoryTemplate { get; set; }

        public DataTemplate PinnedUnsupportedTemplate { get; set; }

        private DataTemplate GetPinnedMessageTemplate(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message != null)
            {
                if (!TLString.IsNullOrEmpty(message.Message)
                    && (message.Media is TLMessageMediaEmpty || message.Media is TLMessageMediaWebPage))
                {
                    return PinnedTextTemplate;
                }

                var value = message.Media;

                if (value is TLMessageMediaEmpty)
                {
                    return PinnedUnsupportedTemplate;
                }

                if (value is TLMessageMediaContact)
                {
                    return PinnedContactTemplate;
                }

                if (value is TLMessageMediaPhoto)
                {
                    return PinnedPhotoTemplate;
                }

                if (value is TLMessageMediaInvoice)
                {
                    return PinnedInvoiceTemplate;
                }

                if (value is TLMessageMediaGame)
                {
                    return PinnedGameTemplate;
                }

                if (value is TLMessageMediaDocument)
                {
                    if (message.IsVoice())
                    {
                        return PinnedVoiceMessageTemplate;
                    }

                    if (message.IsRoundVideo())
                    {
                        return PinnedVideoTemplate;
                    }

                    if (message.IsVideo())
                    {
                        return PinnedVideoTemplate;
                    }

                    if (message.IsGif())
                    {
                        return PinnedGifTemplate;
                    }

                    if (message.IsSticker())
                    {
                        return PinnedStickerTemplate;
                    }

                    return PinnedDocumentTemplate;
                }

                if (value is TLMessageMediaVideo)
                {
                    return PinnedVideoTemplate;
                }

                if (value is TLMessageMediaGeoLive)
                {
                    return PinnedLiveGeoPointTemplate;
                }

                if (value is TLMessageMediaGeo)
                {
                    return PinnedGeoPointTemplate;
                }

                if (value is TLMessageMediaAudio)
                {
                    return PinnedAudioTemplate;
                }
            }

            var serviceMessage = messageBase as TLMessageService;
            if (serviceMessage != null)
            {
                var clearHistoryAction = serviceMessage.Action as TLMessageActionClearHistory;
                if (clearHistoryAction != null)
                {
                    return PinnedClearHistoryTemplate;
                }
            }

            var emptyMessage = messageBase as TLMessageEmpty;
            if (emptyMessage != null)
            {
                return PinnedEmptyTemplate;
            }

            return PinnedUnsupportedTemplate;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as TLMessageBase;

            if (message != null)
            {
                return GetPinnedMessageTemplate(message);
            }

            return PinnedUnsupportedTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
