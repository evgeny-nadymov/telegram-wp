// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Telegram.Api;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls.Utils;
using TelegramClient.Resources;
using Language = TelegramClient.Utils.Language;

namespace TelegramClient.Converters
{
    public class DeclensionConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty NominativeSingularProperty = DependencyProperty.Register(
            "NominativeSingular", typeof (string), typeof (DeclensionConverter), new PropertyMetadata(default(string)));

        public string NominativeSingular
        {
            get { return (string) GetValue(NominativeSingularProperty); }
            set { SetValue(NominativeSingularProperty, value); }
        }

        public static readonly DependencyProperty NominativePluralProperty = DependencyProperty.Register(
            "NominativePlural", typeof (string), typeof (DeclensionConverter), new PropertyMetadata(default(string)));

        public string NominativePlural
        {
            get { return (string) GetValue(NominativePluralProperty); }
            set { SetValue(NominativePluralProperty, value); }
        }

        public static readonly DependencyProperty GenitiveSingularProperty = DependencyProperty.Register(
            "GenitiveSingular", typeof (string), typeof (DeclensionConverter), new PropertyMetadata(default(string)));

        public string GenitiveSingular
        {
            get { return (string) GetValue(GenitiveSingularProperty); }
            set { SetValue(GenitiveSingularProperty, value); }
        }

        public static readonly DependencyProperty GenitivePluralProperty = DependencyProperty.Register(
            "GenitivePlural", typeof (string), typeof (DeclensionConverter), new PropertyMetadata(default(string)));

        public string GenitivePlural
        {
            get { return (string) GetValue(GenitivePluralProperty); }
            set { SetValue(GenitivePluralProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count;
            if (value is int)
            {
                count = (int) value;
            }
            else
            {
                return null;
            }

            return Language.Declension(
                count,
                NominativeSingular,
                NominativePlural,
                GenitiveSingular,
                GenitivePlural).ToLower(CultureInfo.CurrentUICulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvoiceToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mediaInvoice = value as TLMessageMediaInvoice;
            if (mediaInvoice == null) return null;

            if (mediaInvoice.ReceiptMsgId != null && mediaInvoice.ReceiptMsgId.Value >= 0)
            {
                return string.Format("{0} {1} {2}", mediaInvoice.TotalAmount.Value / Math.Pow(10.0, Currency.GetPow(mediaInvoice.Currency.ToString())), Currency.GetSymbol(mediaInvoice.Currency.ToString()), AppResources.Receipt).ToUpperInvariant();        
            }

            return string.Format("{0} {1} {2}", mediaInvoice.TotalAmount.Value / Math.Pow(10.0, Currency.GetPow(mediaInvoice.Currency.ToString())), Currency.GetSymbol(mediaInvoice.Currency.ToString()), mediaInvoice.Test ? AppResources.TestInvoice : AppResources.Invoice).ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageToWebPageCaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = value as TLMessage;
            if (message != null)
            {
                if (!string.IsNullOrEmpty(message.WebPageTitle))
                {
                    return message.WebPageTitle;
                }

                var mediaWebPage = message.Media as TLMessageMediaWebPage;
                if (mediaWebPage != null)
                {
                    var webPage = mediaWebPage.WebPage as TLWebPage;

                    if (webPage != null)
                    {
                        var caption = webPage.Title ?? webPage.SiteName;// ?? webPage.DisplayUrl;
                        if (!TLString.IsNullOrEmpty(caption))
                        {
                            return Language.CapitalizeFirstLetter(caption.ToString());
                            
                        }

                        caption = webPage.DisplayUrl;
                        if (!TLString.IsNullOrEmpty(caption))
                        {
                            var parts = caption.ToString().Split('.');
                            if (parts.Length >= 2)
                            {
                                return Language.CapitalizeFirstLetter(parts[parts.Length - 2]);
                            }
                        }
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WebPageToCaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var webPage = value as TLWebPage;

            if (webPage != null)
            {
                return webPage.SiteName?? webPage.Title ?? webPage.DisplayUrl;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WebPageToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var webPage = value as TLWebPage;

            if (webPage != null)
            {
                return webPage.Description ?? webPage.Title;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RecentItemToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var user = value as TLUserBase;
            if (user != null)
            {
                return TLUserBase.GetFirstName(user.FirstName, user.LastName, user.Phone);
            }

            var chat = value as TLChatBase;
            if (chat != null)
            {
                return chat.FullName;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageContainerToFromConverter : IValueConverter
    {
        public static string GetFirstName(TLMessage25 message)
        {
            var message48 = message as TLMessage48;
            if (message48 != null)
            {
                if (message48.FwdHeader != null)
                {
                    var cacheService = InMemoryCacheService.Instance;
                    var channelId = message48.FwdHeader.ChannelId;
                    if (channelId != null)
                    {
                        var channel = cacheService.GetChat(message48.FwdHeader.ChannelId) as TLChannel;
                        if (channel != null)
                        {
                            return channel.Title.ToString();
                        }
                    }
                    else
                    {
                        var userId = message48.FwdHeader.FromId;
                        if (userId != null)
                        {
                            var fromUser = cacheService.GetUser(message48.FwdHeader.FromId);
                            if (fromUser != null)
                            {
                                return TLUserBase.GetFirstName(fromUser.FirstName, fromUser.LastName, fromUser.Phone);
                            }
                        }
                    }
                }
            }

            if (message != null)
            {
                var fromUser = message.FwdFrom as TLUserBase;
                if (fromUser != null)
                {
                    return TLUserBase.GetFirstName(fromUser.FirstName, fromUser.LastName, fromUser.Phone);
                }

                var fromChat = message.FwdFrom as TLChannel;
                if (fromChat != null)
                {
                    return fromChat.Title.ToString();
                }

                var fromChatForbidden = message.FwdFrom as TLChannelForbidden;
                if (fromChatForbidden != null)
                {
                    return fromChatForbidden.Title.ToString();
                }
            }

            return null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messagesContainer = value as TLMessagesContainter;

            if (messagesContainer != null)
            {
                if (messagesContainer.FwdMessages != null)
                {
                    var usersCache = new Dictionary<int, TLMessage25>();
                    var channelsCache = new Dictionary<int, TLMessage25>();

                    for (var i = 0; i < messagesContainer.FwdMessages.Count; i++)
                    {
                        var message = messagesContainer.FwdMessages[i];
                        if (message.FwdFromId != null)
                        {
                            if (!usersCache.ContainsKey(message.FwdFromId.Value))
                            {
                                usersCache[message.FwdFromId.Value] = message;
                            }
                        }

                        var message48 = message as TLMessage48;
                        if (message48 != null)
                        {
                            if (message48.FwdHeader != null)
                            {
                                var channelId = message48.FwdHeader.ChannelId;
                                if (channelId != null)
                                {
                                    if (!channelsCache.ContainsKey(channelId.Value))
                                    {
                                        channelsCache[channelId.Value] = message;
                                    }
                                }
                                else
                                {
                                    var userId = message48.FwdHeader.FromId;
                                    if (userId != null)
                                    {
                                        if (!usersCache.ContainsKey(userId.Value))
                                        {
                                            usersCache[userId.Value] = message;
                                        }
                                    }
                                }
                            }
                        }

                        var message40 = message as TLMessage40;
                        if (message40 != null)
                        {
                            if (message40.FwdFromPeer != null)
                            {
                                var peerUser = message40.FwdFromPeer as TLPeerUser;
                                if (peerUser != null)
                                {
                                    if (!usersCache.ContainsKey(peerUser.Id.Value))
                                    {
                                        usersCache[peerUser.Id.Value] = message;
                                    }
                                }

                                var peerChannel = message40.FwdFromPeer as TLPeerChannel;
                                if (peerChannel != null)
                                {
                                    if (!channelsCache.ContainsKey(peerChannel.Id.Value))
                                    {
                                        channelsCache[peerChannel.Id.Value] = message;
                                    }
                                }
                            }
                        }
                    }
                    var count = usersCache.Count + channelsCache.Count;
                    if (count == 1)
                    {
                        return GetFirstName(messagesContainer.FwdMessages[0]);
                    } 
                    if (count == 2)
                    {
                        var list = usersCache.Values.Union(channelsCache.Values).ToList();

                        var message1 = list[0];
                        var message2 = list[1];

                        return string.Format("{0}, {1}", GetFirstName(message1), GetFirstName(message2));
                    }
                    if (count > 2)
                    {
                        return string.Format("{0} {1}", GetFirstName(messagesContainer.FwdMessages[0]), string.Format(AppResources.AndOthers, count - 1).ToLowerInvariant());
                    }
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageContainerToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messagesContainer = value as TLMessagesContainter;

            if (messagesContainer != null)
            {
                if (messagesContainer.FwdMessages != null)
                {
                    var count = messagesContainer.FwdMessages.Count;
                    if (count == 1)
                    {
                        var mediaPhoto = messagesContainer.FwdMessages[0].Media as TLMessageMediaPhoto;
                        if (mediaPhoto != null)
                        {
                            return AppResources.ForwardedPhotoNominativeSingular;
                        }

                        var mediaAudio = messagesContainer.FwdMessages[0].Media as TLMessageMediaAudio;
                        if (mediaAudio != null)
                        {
                            return AppResources.ForwardedAudioNominativeSingular;
                        }

                        var mediaDocument = messagesContainer.FwdMessages[0].Media as TLMessageMediaDocument;
                        if (mediaDocument != null)
                        {
                            if (messagesContainer.FwdMessages[0].IsVoice())
                            {
                                return AppResources.ForwardedVoiceMessageNominativeSingular;
                            }

                            if (messagesContainer.FwdMessages[0].IsVideo())
                            {
                                return AppResources.ForwardedVideoNominativeSingular;
                            }

                            if (messagesContainer.FwdMessages[0].IsGif())
                            {
                                return AppResources.ForwardedGifNominativeSingular;
                            }

                            if (messagesContainer.FwdMessages[0].IsSticker())
                            {
                                return AppResources.ForwardedStickerNominativeSingular;
                            }

                            return AppResources.ForwardedFileNominativeSingular;
                        }

                        var mediaVideo = messagesContainer.FwdMessages[0].Media as TLMessageMediaVideo;
                        if (mediaVideo != null)
                        {
                            return AppResources.ForwardedVideoNominativeSingular;
                        }

                        var mediaLocation = messagesContainer.FwdMessages[0].Media as TLMessageMediaGeo;
                        if (mediaLocation != null)
                        {
                            return AppResources.ForwardedLocationNominativeSingular;
                        }

                        var mediaContact = messagesContainer.FwdMessages[0].Media as TLMessageMediaContact;
                        if (mediaContact != null)
                        {
                            return AppResources.ForwardedContactNominativeSingular;
                        }

                        return AppResources.ForwardedMessage;
                    }
                    if (count > 1)
                    {
                        var sameMedia = true;
                        var media = messagesContainer.FwdMessages[0].Media;
                        for (var i = 1; i < messagesContainer.FwdMessages.Count; i++)
                        {
                            if (messagesContainer.FwdMessages[i].Media.GetType() != media.GetType())
                            {
                                sameMedia = false;
                                break;
                            }
                        }

                        if (sameMedia)
                        {
                            if (media is TLMessageMediaPhoto)
                            {
                                return Language.Declension(
                                    count,
                                    AppResources.ForwardedPhotoNominativeSingular,
                                    AppResources.ForwardedPhotoNominativePlural,
                                    AppResources.ForwardedPhotoGenitiveSingular,
                                    AppResources.ForwardedPhotoGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                            }

                            if (media is TLMessageMediaAudio)
                            {
                                return Language.Declension(
                                    count,
                                    AppResources.ForwardedAudioNominativeSingular,
                                    AppResources.ForwardedAudioNominativePlural,
                                    AppResources.ForwardedAudioGenitiveSingular,
                                    AppResources.ForwardedAudioGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                            }

                            if (media is TLMessageMediaDocument)
                            {
                                if (messagesContainer.FwdMessages[0].IsVoice())
                                {
                                    return Language.Declension(
                                       count,
                                       AppResources.ForwardedVoiceMessageNominativeSingular,
                                       AppResources.ForwardedVoiceMessageNominativePlural,
                                       AppResources.ForwardedVoiceMessageGenitiveSingular,
                                       AppResources.ForwardedVoiceMessageGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                                }

                                if (messagesContainer.FwdMessages[0].IsVideo())
                                {
                                    return Language.Declension(
                                       count,
                                       AppResources.ForwardedVideoNominativeSingular,
                                       AppResources.ForwardedVideoNominativePlural,
                                       AppResources.ForwardedVideoGenitiveSingular,
                                       AppResources.ForwardedVideoGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                                }

                                if (messagesContainer.FwdMessages[0].IsGif())
                                {
                                    return Language.Declension(
                                       count,
                                       AppResources.ForwardedGifNominativeSingular,
                                       AppResources.ForwardedGifNominativePlural,
                                       AppResources.ForwardedGifGenitiveSingular,
                                       AppResources.ForwardedGifGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                                }

                                if (messagesContainer.FwdMessages[0].IsSticker())
                                {
                                    return Language.Declension(
                                       count,
                                       AppResources.ForwardedStickerNominativeSingular,
                                       AppResources.ForwardedStickerNominativePlural,
                                       AppResources.ForwardedStickerGenitiveSingular,
                                       AppResources.ForwardedStickerGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                                }

                                return Language.Declension(
                                    count,
                                    AppResources.ForwardedFileNominativeSingular,
                                    AppResources.ForwardedFileNominativePlural,
                                    AppResources.ForwardedFileGenitiveSingular,
                                    AppResources.ForwardedFileGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                            }

                            if (media is TLMessageMediaVideo)
                            {
                                return Language.Declension(
                                    count,
                                    AppResources.ForwardedVideoNominativeSingular,
                                    AppResources.ForwardedVideoNominativePlural,
                                    AppResources.ForwardedVideoGenitiveSingular,
                                    AppResources.ForwardedVideoGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                            }

                            if (media is TLMessageMediaGeo)
                            {
                                return Language.Declension(
                                    count,
                                    AppResources.ForwardedLocationNominativeSingular,
                                    AppResources.ForwardedLocationNominativePlural,
                                    AppResources.ForwardedLocationGenitiveSingular,
                                    AppResources.ForwardedLocationGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                            }

                            if (media is TLMessageMediaContact)
                            {
                                return Language.Declension(
                                    count,
                                    AppResources.ForwardedContactNominativeSingular,
                                    AppResources.ForwardedContactNominativePlural,
                                    AppResources.ForwardedContactGenitiveSingular,
                                    AppResources.ForwardedContactGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                            }
                        }

                        return Language.Declension(
                            count,
                            AppResources.ForwardedMessageNominativeSingular,
                            AppResources.ForwardedMessageNominativePlural,
                            AppResources.ForwardedMessageGenitiveSingular,
                            AppResources.ForwardedMessageGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                    }
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
