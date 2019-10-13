// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows.Data;
using Caliburn.Micro;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Themes.Default.Templates;

namespace TelegramClient.Converters
{
    public class DialogToBriefInfoConverter : IValueConverter
    {
        public static string Convert(TLDecryptedMessageBase value, bool showContent)
        {
            var serviceMessage = value as TLDecryptedMessageService;
            if (serviceMessage != null)
            {
                if (serviceMessage.Action is TLDecryptedMessageActionEmpty)
                {
                    return AppResources.SecretChatCreated;
                }

                return DecryptedServiceMessageToTextConverter.Convert(serviceMessage);
            }

            var message = value as TLDecryptedMessage;
            if (message != null)
            {
                var canSendString = string.Empty;

                if (message.Status == MessageStatus.Failed)
                {
                    canSendString = string.Format("{0}: ", AppResources.SendingFailed);
                }

                var message73 = message as TLDecryptedMessage73;
                if (message73 != null && message73.GroupedId != null)
                {
                    return canSendString + AppResources.Album;
                }

                if (message.Media != null)
                {
                    var messageMediaWebPage = message.Media as TLDecryptedMessageMediaWebPage;
                    if (messageMediaWebPage != null)
                    {
                        return canSendString + messageMediaWebPage.Url;
                    }

                    if (message.Media is TLDecryptedMessageMediaDocument)
                    {
                        if (message.IsVoice())
                        {
                            return canSendString + AppResources.VoiceMessage;
                        }
                        
                        if (message.IsVideo())
                        {
                            return canSendString + AppResources.Video;
                        }

                        if (message.IsGif())
                        {
                            return canSendString + AppResources.Gif;
                        }

                        return canSendString + AppResources.Document;
                    }

                    if (message.Media is TLDecryptedMessageMediaContact)
                    {
                        return canSendString + AppResources.Contact;
                    }

                    if (message.Media is TLDecryptedMessageMediaGeoPoint)
                    {
                        return canSendString + AppResources.GeoPoint;
                    }

                    if (message.Media is TLDecryptedMessageMediaPhoto)
                    {
                        return canSendString + AppResources.Photo;
                    }

                    if (message.Media is TLDecryptedMessageMediaVideo)
                    {
                        return canSendString + AppResources.Video;
                    }

                    if (message.Media is TLDecryptedMessageMediaAudio)
                    {
                        return canSendString + AppResources.Audio;
                    }

                    if (message.Media is TLDecryptedMessageMediaExternalDocument)
                    {
                        if (message.IsSticker())
                        {
                            return canSendString + AppResources.Sticker;
                        }

                        return canSendString + AppResources.Document;
                    }
                }

                if (message.Message != null)
                {
                    if (showContent)
                    {
                        var str = message.Message != null? message.Message.ToString() : string.Empty;
                        return canSendString + str.Substring(0, Math.Min(str.Length, 40)).Replace("\r\n", "\n").Replace('\n', ' ');
                    }

                    return canSendString + AppResources.Message;
                }
            }

            return null;
        }

        public static string Convert(TLMessageBase value, bool showContent)
        {
            var emptyMessage = value as TLMessageEmpty;
            if (emptyMessage != null)
            {
                return AppResources.EmptyMessage;
            }

            //var forwardedMessage = value as TLMessageForwarded;
            //if (forwardedMessage != null)
            //{
            //    return AppResources.ForwardedMessage;
            //}

            var serviceMessage = value as TLMessageService;
            if (serviceMessage != null)
            {
                return ServiceMessageToTextConverter.Convert(serviceMessage);

                //return AppResources.ServiceMessage;
            }
               
            var message = value as TLMessage;
            if (message != null)
            {
                var canSendString = string.Empty;

                if (message.Status == MessageStatus.Failed)
                {
                    canSendString = string.Format("{0}: ", AppResources.SendingFailed);
                }

                var message73 = message as TLMessage73;
                if (message73 != null && message73.GroupedId != null)
                {
                    return canSendString + AppResources.Album;
                }

                if (message.Media != null)
                {
                    var mediaInvoice = message.Media as TLMessageMediaInvoice;
                    if (mediaInvoice != null)
                    {
                        var description = mediaInvoice.Description;
                        if (!TLString.IsNullOrEmpty(description))
                        {
                            return canSendString + description;
                        }

                        return canSendString + AppResources.Invoice;
                    }

                    var mediaGame = message.Media as TLMessageMediaGame;
                    if (mediaGame != null)
                    {
                        return canSendString + "🎮 " + mediaGame.Game.Title;
                    }

                    if (message.Media is TLMessageMediaDocument)
                    {
                        var captionString = string.Empty;
                        var str = message.Message != null ? message.Message.ToString().Replace("\r\n", "\n").Replace('\n', ' ') : string.Empty;
                        if (!string.IsNullOrEmpty(str))
                        {
                            captionString = ", " + str.Substring(0, Math.Min(str.Length, 40));
                        }

                        if (message.IsVoice())
                        {
                            return canSendString + AppResources.VoiceMessage + captionString;
                        }

                        if (message.IsRoundVideo())
                        {
                            return canSendString + AppResources.VideoMessage + captionString;
                        }

                        if (message.IsVideo())
                        {
                            return canSendString + AppResources.Video + captionString;
                        }

                        if (message.IsGif())
                        {
                            return canSendString + AppResources.Gif + captionString;
                        }

                        if (message.IsSticker())
                        {
                            return canSendString + AppResources.Sticker + captionString;
                        }

                        var mediaDocument = message.Media as TLMessageMediaDocument45;
                        if (mediaDocument != null)
                        {
                            var document = mediaDocument.Document as TLDocument22;
                            if (document != null && !string.IsNullOrEmpty(document.DocumentName))
                            {
                                return canSendString + document.DocumentName + captionString;
                            }
                        }

                        return canSendString + AppResources.Document + captionString;
                    }

                    if (message.Media is TLMessageMediaContact)
                    {
                        return canSendString + AppResources.Contact;
                    }

                    if (message.Media is TLMessageMediaGeoLive)
                    {
                        return canSendString + AppResources.LiveLocation;
                    }

                    if (message.Media is TLMessageMediaGeo)
                    {
                        return canSendString + AppResources.GeoPoint;
                    }

                    if (message.Media is TLMessageMediaPhoto)
                    {
                        if (!TLString.IsNullOrEmpty(message.Message))
                        {
                            var str = message.Message.ToString().Replace("\r\n", "\n").Replace('\n', ' ');
                            if (!string.IsNullOrEmpty(str))
                            {
                                return canSendString + AppResources.Photo + ", " + str.Substring(0, Math.Min(str.Length, 40));
                            }
                        }

                        return canSendString + AppResources.Photo;
                    }

                    if (message.Media is TLMessageMediaVideo)
                    {
                        return canSendString + AppResources.Video;
                    }

                    if (message.Media is TLMessageMediaAudio)
                    {
                        return canSendString + AppResources.Audio;
                    }

                    if (message.Media is TLMessageMediaUnsupportedBase)
                    {
                        return canSendString + AppResources.UnsupportedMedia;
                    }
                }

                if (message.Message != null)
                {
                    if (showContent)
                    {
                        var str = message.Message != null ? message.Message.ToString() : string.Empty;
                        return canSendString + str.Substring(0, Math.Min(str.Length, 40)).Replace("\r\n", "\n").Replace('\n', ' ');                  
                    }

                    return canSendString + AppResources.Message;
                }
            }

            return null;
        }

        public static bool ShowDraft(TLDialog53 dialog)
        {
            if (dialog == null) return false;

            var draft = dialog.Draft as TLDraftMessage;
            if (draft != null)
            {
                if (dialog.Peer is TLPeerChannel)
                {
                    var channel = dialog.With as TLChannel;
                    if (channel != null && channel.IsBroadcast && !channel.Creator && !channel.IsEditor)
                    {
                        return false;
                    }
                }

                var topMessage = dialog.TopMessage as TLMessageCommon;
                if (topMessage != null && !topMessage.Out.Value && topMessage.Unread.Value)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dialog = value as TLDialog;
            var broadcast = value as TLBroadcastDialog;
            if (dialog != null)
            {
                var dialog53 = dialog as TLDialog53;
                if (dialog53 != null)
                {
                    if (ShowDraft(dialog53))
                    {
                        var draft = dialog53.Draft as TLDraftMessage;
                        if (draft != null)
                        {
                            if (TLString.IsNullOrEmpty(draft.Message) 
                                && draft.ReplyToMsgId != null 
                                && draft.ReplyToMsgId.Value > 0)
                            {
                                return AppResources.Reply.ToLowerInvariant();
                            }

                            return draft.Message.ToString();
                        }
                    }
                }

                var message = dialog.TopMessage;
                if (message != null)
                {
                    return Convert(message, true);
                }
            }
            else if (broadcast != null)
            {
                var message = broadcast.TopMessage;
                if (message != null)
                {
                    return Convert(message, true);
                }
            }
            else
            {
                var encryptedDialog = value as TLEncryptedDialog;
                if (encryptedDialog != null)
                {
                    var chatId = encryptedDialog.Peer.Id;
                    var encryptedChat = IoC.Get<ICacheService>().GetEncryptedChat(chatId);
                    var chatWaiting = encryptedChat as TLEncryptedChatWaiting;
                    if (chatWaiting != null)
                    {
                        var participant = IoC.Get<ICacheService>().GetUser(chatWaiting.ParticipantId);
                        return string.Format(AppResources.WaitingForUserToGetOnline, participant.FirstName);
                    }

                    var chatDiscarded = encryptedChat as TLEncryptedChatDiscarded;
                    if (chatDiscarded != null)
                    {
                        return AppResources.SecretChatDiscarded;
                    }

                    var chatEmpty = encryptedChat as TLEncryptedChatEmpty;
                    if (chatEmpty != null)
                    {
                        return AppResources.EmptySecretChat;
                    }

                    var chat = encryptedChat as TLEncryptedChat;
                    if (chat != null)
                    {
                        if (TLUtils.IsDisplayedDecryptedMessage(encryptedDialog.TopMessage))
                        {
                            return Convert(encryptedDialog.TopMessage, true);
                        }

                        for (var i = 0; i < encryptedDialog.Messages.Count; i++)
                        {
                            if (TLUtils.IsDisplayedDecryptedMessage(encryptedDialog.Messages[i]))
                            {
                                return Convert(encryptedDialog.Messages[i], true);
                            }
                        }

                        var currentUserId = IoC.Get<IStateService>().CurrentUserId;
                        if (chat.AdminId.Value == currentUserId)
                        {
                            var cacheService = IoC.Get<ICacheService>();
                            var user = cacheService.GetUser(chat.ParticipantId);
                            if (user != null)
                            {
                                var userName = TLString.IsNullOrEmpty(user.FirstName) ? user.LastName : user.FirstName;
                                return string.Format(AppResources.UserJoinedYourSecretChat, userName);
                            }
                        }
                        else
                        {
                            return AppResources.YouJoinedTheSecretChat;
                        }

                        return AppResources.SecretChatCreated;
                    }

                    var chatRequested = encryptedChat as TLEncryptedChatRequested;
                    if (chatRequested != null)
                    {
                        return AppResources.SecretChatRequested;
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
}
