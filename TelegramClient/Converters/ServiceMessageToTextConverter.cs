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
using System.Text;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls.Utils;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views.Additional;
using Language = TelegramClient.Utils.Language;

namespace TelegramClient.Converters
{
    public class DecryptedServiceMessageToTextConverter : IValueConverter
    {
        private static readonly Dictionary<Type, Func<TLDecryptedMessageActionBase, int, string, string>> _actionsCache = new Dictionary<Type, Func<TLDecryptedMessageActionBase, int, string, string>>
        {
            { 
                typeof(TLDecryptedMessageActionAcceptKey), (action, fromUserId, fromUserFullName) =>
                {
#if DEBUG
                    return "TLDecryptedMessageActionAcceptKey exchange_id=" + ((TLDecryptedMessageActionAcceptKey)action).ExchangeId;
#endif
                    return string.Empty;
                } 
            },
            { 
                typeof(TLDecryptedMessageActionRequestKey), (action, fromUserId, fromUserFullName) =>
                {
#if DEBUG
                    return "TLDecryptedMessageActionRequestKey exchange_id=" + ((TLDecryptedMessageActionRequestKey)action).ExchangeId;
#endif
                    return string.Empty;
                } 
            },
            { 
                typeof(TLDecryptedMessageActionAbortKey), (action, fromUserId, fromUserFullName) =>
                {
#if DEBUG
                    return "TLDecryptedMessageActionAbortKey exchange_id=" + ((TLDecryptedMessageActionAbortKey)action).ExchangeId;
#endif
                    return string.Empty;
                } 
            },
            { 
                typeof(TLDecryptedMessageActionCommitKey), (action, fromUserId, fromUserFullName) =>
                {
#if DEBUG
                    return "TLDecryptedMessageActionCommitKey exchange_id=" + ((TLDecryptedMessageActionCommitKey)action).ExchangeId;
#endif
                    return string.Empty;
                } 
            },
            { 
                typeof(TLDecryptedMessageActionNoop), (action, fromUserId, fromUserFullName) =>
                {
#if DEBUG
                    return "TLDecryptedMessageActionNoop";
#endif
                    return string.Empty;
                } 
            },
            { 
                typeof(TLDecryptedMessageActionEmpty), (action, fromUserId, fromUserFullName) =>
                {
#if DEBUG
                    return AppResources.MessageActionEmpty;
#endif
                    return string.Empty;
                } 
            },
            {
                typeof(TLDecryptedMessageActionSetMessageTTL), (action, fromUserId, fromUserFullName) =>
                {
                    var currentUserId = IoC.Get<IStateService>().CurrentUserId;
                    var resourceActionSetMessageTTL = string.Format(AppResources.MessageActionSetMessageTTL, fromUserFullName, @"{0}");
                    if (currentUserId == fromUserId)
                    {
                        resourceActionSetMessageTTL = AppResources.MessageActionYouSetMessageTTL;
                    }

                    var seconds = ((TLDecryptedMessageActionSetMessageTTL)action).TTLSeconds.Value;

                    if (seconds == 0)
                    {
                        if (currentUserId == fromUserId)
                        {
                            return AppResources.MessageActionYouDisableMessageTTL;
                        }

                        return string.Format(AppResources.MessageActionDisableMessageTTL, fromUserFullName);
                    }

                    string secondsString;
                    if (seconds < 60)
                    {
                        secondsString = Utils.Language.Declension(seconds, 
                            AppResources.SecondNominativeSingular,
                            AppResources.SecondNominativePlural, 
                            AppResources.SecondGenitiveSingular,
                            AppResources.SecondGenitivePlural);
                    }
                    else if (seconds < 60 * 60)
                    {
                        secondsString = Utils.Language.Declension(seconds / 60,
                            AppResources.MinuteNominativeSingular,
                            AppResources.MinuteNominativePlural,
                            AppResources.MinuteGenitiveSingular,
                            AppResources.MinuteGenitivePlural);
                    }
                    else if (seconds < TimeSpan.FromHours(24.0).TotalSeconds)
                    {
                        secondsString = Utils.Language.Declension((int)(seconds / TimeSpan.FromHours(1.0).TotalSeconds),
                            AppResources.HourNominativeSingular,
                            AppResources.HourNominativePlural,
                            AppResources.HourGenitiveSingular,
                            AppResources.HourGenitivePlural);
                    }
                    else if (seconds < TimeSpan.FromDays(7.0).TotalSeconds)
                    {
                        secondsString = Utils.Language.Declension((int)(seconds / TimeSpan.FromDays(1.0).TotalSeconds),
                            AppResources.DayNominativeSingular,
                            AppResources.DayNominativePlural,
                            AppResources.DayGenitiveSingular,
                            AppResources.DayGenitivePlural);
                    }
                    else if (seconds == TimeSpan.FromDays(7.0).TotalSeconds)
                    {
                        secondsString = Utils.Language.Declension(1,
                            AppResources.WeekNominativeSingular,
                            AppResources.WeekNominativePlural,
                            AppResources.WeekGenitiveSingular,
                            AppResources.WeekGenitivePlural);
                    }
                    else
                    {
                        secondsString = Utils.Language.Declension(seconds,
                            AppResources.SecondNominativeSingular,
                            AppResources.SecondNominativePlural,
                            AppResources.SecondGenitiveSingular,
                            AppResources.SecondGenitivePlural);
                    }


                    return string.Format(resourceActionSetMessageTTL, secondsString.ToLowerInvariant());
                }
            },
            { typeof(TLDecryptedMessageActionScreenshotMessages), (action, fromUserId, fromUserFullName) =>
                {
                    var currentUserId = IoC.Get<IStateService>().CurrentUserId;
                    var resourceActionScreenshortMessage = string.Format(AppResources.MessageActionScreenshotMessages, fromUserFullName, @"{0}");
                    if (currentUserId == fromUserId)
                    {
                        resourceActionScreenshortMessage = AppResources.MessageActionYouScreenshotMessages;
                    }

                    return resourceActionScreenshortMessage;
                }
            },
            { 
                typeof(TLDecryptedMessageActionReadMessages), (action, fromUserId, fromUserFullName) =>
                {
#if DEBUG
                    return "TLDecryptedMessageActionReadMessages random_id=" + string.Join(", ", ((TLDecryptedMessageActionReadMessages)action).RandomIds);
#endif
                    return string.Empty;
                }
            },
            { 
                typeof(TLDecryptedMessageActionDeleteMessages), (action, fromUserId, fromUserFullName) => 
                {
#if DEBUG
                    return "TLDecryptedMessageActionDeleteMessages random_id=" + string.Join(", ", ((TLDecryptedMessageActionDeleteMessages)action).RandomIds);
#endif
                    return string.Empty;
                }
            },
            { 
                typeof(TLDecryptedMessageActionFlushHistory), (action, fromUserId, fromUserFullName) => 
                {
#if DEBUG
                    return "TLDecryptedMessageActionFlushHistory";
#endif
                    return string.Empty;
                }
            },
            { 
                typeof(TLDecryptedMessageActionNotifyLayer), (action, fromUserId, fromUserFullName) => 
                {
#if DEBUG
                    return "TLDecryptedMessageActionNotifyLayer layer=" + ((TLDecryptedMessageActionNotifyLayer)action).Layer;
#endif
                    return string.Empty;
                }
            },
        };

        public static string Convert(TLDecryptedMessageService serviceMessage)
        {
            var fromId = serviceMessage.FromId;
            var fromUser = IoC.Get<ICacheService>().GetUser(fromId);
            var fromUserFullName = fromUser != null ? fromUser.FullName : AppResources.User;

            var action = serviceMessage.Action;
            if (action != null && _actionsCache.ContainsKey(action.GetType()))
            {
                return _actionsCache[action.GetType()](action, fromId.Value, fromUserFullName);
            }

#if DEBUG
            return serviceMessage.GetType().Name;
#endif

            return AppResources.MessageActionEmpty;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var serviceMessage = value as TLDecryptedMessageService;
            if (serviceMessage != null)
            {
                return Convert(serviceMessage);
            }

            return AppResources.MessageActionEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceMessageToTextConverter : IValueConverter
    {
        public static TLGame GetGame(TLMessageService message)
        {
            var reply = message.Reply as TLMessage31;
            if (reply != null)
            {
                var mediaGame = reply.Media as TLMessageMediaGame;
                if (mediaGame != null)
                {
                    return mediaGame.Game;
                }
            }

            return null;
        }

        public static TLKeyboardButtonGame GetKeyboardButtonGame(TLMessageService message)
        {
            var reply = message.Reply as TLMessage31;
            if (reply != null)
            {
                if (reply.ReplyMarkup != null)
                {
                    var replyKeyboardMarkup = reply.ReplyMarkup as TLReplyInlineMarkup;
                    if (replyKeyboardMarkup != null)
                    {
                        foreach (var row in replyKeyboardMarkup.Rows)
                        {
                            foreach (var button in row.Buttons)
                            {
                                var keyboardButtonGame = button as TLKeyboardButtonGame;
                                if (keyboardButtonGame != null)
                                {
                                    return keyboardButtonGame;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static readonly Dictionary<Type, Func<TLMessageBase, TLMessageActionBase, int, string, bool, bool, string>> _actionsCache = new Dictionary<Type, Func<TLMessageBase, TLMessageActionBase, int, string, bool, bool, string>>
        { 
            { typeof(TLMessageActionEmpty), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) => AppResources.MessageActionEmpty },
            { typeof(TLMessageActionSecureValuesSent), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
            {
                var messageActionSecureValuesSent = action as TLMessageActionSecureValuesSent;
                if (messageActionSecureValuesSent != null)
                {
                    var values = new List<string>();
                    foreach (var type in messageActionSecureValuesSent.Types)
                    {
                        values.Add(SecureRequiredTypeToCaptionConverter.Convert(type));
                    }

                    return string.Format(AppResources.MessageActionSecureValuesSent, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName), string.Join(", ", values));
                }

                return string.Empty;
            } 
            },
            { typeof(TLMessageActionSecureValuesSentMe), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
            {
                return string.Empty;
            } 
            },
            { typeof(TLMessageActionBotAllowed), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
            {
                var messageActionBotAllowed = action as TLMessageActionBotAllowed;
                if (messageActionBotAllowed != null)
                {
                    return string.Format(AppResources.MessageActionBotAllowed, messageActionBotAllowed.Domain);
                }

                return string.Empty;
            } 
            },
            { typeof(TLMessageActionCustomAction), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
            {
                var messageActionCustomAction = action as TLMessageActionCustomAction;
                if (messageActionCustomAction != null)
                {
                    return messageActionCustomAction.Message.ToString();
                }

                return string.Empty;
            } 
            },
            { typeof(TLMessageActionPaymentSent), (message, action, fromfromUserId, fromUserFullName, useActiveLinks, noName) => GetPaymentSentString(action, message, fromUserFullName) },
            { typeof(TLMessageActionPaymentSentMe), (message, action, fromfromUserId, fromUserFullName, useActiveLinks, noName) => GetPaymentSentString(action, message, fromUserFullName) },
            { typeof(TLMessageActionPhoneCall), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
            {
                var duration = ((TLMessageActionPhoneCall)action).Duration;
                var reason = ((TLMessageActionPhoneCall)action).Reason;

                var messageService = message as TLMessageService;
                if (messageService != null)
                {
                    var messageDateTimeConverter = (TLIntToDateTimeConverter)Application.Current.Resources["MessageDateTimeConverter"];
                    var durationString = string.Empty;
                    if (duration != null)
                    {
                        var durationTimeSpan = TimeSpan.FromSeconds(duration.Value);
                        if (durationTimeSpan.TotalSeconds > 60.0)
                        {
                            durationString =
                                Language.Declension(
                                    (int) durationTimeSpan.TotalMinutes,
                                    AppResources.MinuteNominativeSingular,
                                    AppResources.MinuteNominativePlural,
                                    AppResources.MinuteGenitiveSingular,
                                    AppResources.MinuteGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                        }
                        else
                        {
                            durationString = 
                                Language.Declension(
                                    durationTimeSpan.Seconds,
                                    AppResources.SecondNominativeSingular,
                                    AppResources.SecondNominativePlural,
                                    AppResources.SecondGenitiveSingular,
                                    AppResources.SecondGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                        }
                    }

                    if (duration != null)
                    {
                        return messageService.Out.Value
                            ? string.Format(AppResources.MessageActionOutgoingDurationCall, durationString)
                            : string.Format(AppResources.MessageActionIncomingDurationCall, durationString);
                    }

                    var missed = reason as TLPhoneCallDiscardReasonMissed;
                    if (missed != null)
                    {
                        if (messageService.Out.Value)
                        {
                            return AppResources.MessageActionCanceledCall;
                        }
                        else
                        {
                            return AppResources.MessageActionMissedCall;
                        }
                    }

                    var busy2 = reason as TLPhoneCallDiscardReasonBusy;
                    if (busy2 != null)
                    {
                        if (messageService.Out.Value)
                        {
                            return AppResources.MessageActionOutgoingCall;
                        }
                        else
                        {
                            return AppResources.MessageActionDeclinedCall;
                        }
                    }

                    return messageService.Out.Value
                        ? AppResources.MessageActionOutgoingCall
                        : AppResources.MessageActionIncomingCall;    
                }

                return null;
            } 
            },
            { typeof(TLMessageActionGameScore), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
            {
                var score = ((TLMessageActionGameScore)action).Score.Value;
                var isPlural = score == 0 || score > 0;
                var user = IoC.Get<ICacheService>().GetUser(new TLInt(fromUserId)) as TLUser;
                var userFullName = GetUserFullName(user, useActiveLinks, noName);

                var messageService = message as TLMessageService;
                if (messageService != null)
                {
                    var game = GetGame(messageService);
                    if (game != null)
                    {
                        var gameTitle = game.Title.ToString();
                        var reply = messageService.Reply as TLMessageCommon;
                        if (reply != null)
                        {
                            gameTitle = GetGameFullNameString(gameTitle, messageService.Index, messageService.ToId, useActiveLinks);
                        }

                        if (user != null)
                        {
                            if (user.IsSelf)
                            {
                                return string.Format(isPlural ? AppResources.YourScoredAtGamePlural : AppResources.YourScoredAtGame, GetBoldString(score.ToString(CultureInfo.InvariantCulture), useActiveLinks), gameTitle);
                            }

                            return string.Format(isPlural ? AppResources.UserScoredAtGamePlural : AppResources.UserScoredAtGame, userFullName, GetBoldString(score.ToString(CultureInfo.InvariantCulture), useActiveLinks), gameTitle);
                        }

                        return string.Format(isPlural ? AppResources.UserScoredAtGamePlural : AppResources.UserScoredAtGame, AppResources.UserNominativeSingular, GetBoldString(score.ToString(CultureInfo.InvariantCulture), useActiveLinks), gameTitle);
                    }

                    if (user != null)
                    {
                        if (user.IsSelf)
                        {
                            return string.Format(isPlural ? AppResources.YourScoredPlural : AppResources.YourScored, score);
                        }

                        return string.Format(isPlural ? AppResources.UserScoredPlural : AppResources.UserScored, userFullName, score);
                    }
                }

                return string.Format(isPlural ? AppResources.UserScoredPlural : AppResources.UserScored, AppResources.UserNominativeSingular, score);
            } 
            },
            { typeof(TLMessageActionChatCreate), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) => string.Format(AppResources.MessageActionChatCreate, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName), ((TLMessageActionChatCreate) action).Title) },
            //{ typeof(TLMessageActionChannelCreate), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) => string.Format(AppResources.MessageActionChannelCreate, GetFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName), ((TLMessageActionChannelCreate) action).Title) },
            { typeof(TLMessageActionChatEditPhoto), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) => string.Format(AppResources.MessageActionChatEditPhoto, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName)) },
            { typeof(TLMessageActionChatEditTitle), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) => string.Format(AppResources.MessageActionChatEditTitle, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName), ((TLMessageActionChatEditTitle) action).Title) },
            { typeof(TLMessageActionChatDeletePhoto), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) => string.Format(AppResources.MessageActionChatDeletePhoto, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName)) },
            { typeof(TLMessageActionChatAddUser), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var userId = ((TLMessageActionChatAddUser)action).UserId;
                    var user = IoC.Get<ICacheService>().GetUser(userId);
                    var userFullName = GetUserFullName(user, useActiveLinks, noName);

                    if (userId.Value == fromUserId)
                    {
                        return string.Format(AppResources.MessageActionChatAddSelf, userFullName);
                    }

                    return string.Format(AppResources.MessageActionChatAddUser, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName), userFullName);
                }
            },
            { typeof(TLMessageActionChatAddUser41), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var users = ((TLMessageActionChatAddUser41)action).Users;

                    var userFullName = new List<string>();
                    foreach (var userId in users)
                    {
                        var user = IoC.Get<ICacheService>().GetUser(userId);
                        if (user != null)
                        {
                            userFullName.Add(GetUserFullName(user, useActiveLinks, noName));
                        }
                    }

                    if (users.Count == 1
                        && users[0].Value == fromUserId)
                    {
                        return string.Format(AppResources.MessageActionChatAddSelf, string.Join(", ", userFullName));
                    }

                    return string.Format(AppResources.MessageActionChatAddUser, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName), string.Join(", ", userFullName));
                }
            },
            { typeof(TLMessageActionScreenshotTaken), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var currentUserId = IoC.Get<IStateService>().CurrentUserId;
                    var resourceActionScreenshortMessage = string.Format(AppResources.MessageActionScreenshotMessages, fromUserFullName, @"{0}");
                    if (currentUserId == fromUserId)
                    {
                        resourceActionScreenshortMessage = AppResources.MessageActionYouScreenshotMessages;
                    }

                    return resourceActionScreenshortMessage;
                }
            },
            { typeof(TLMessageActionChatDeleteUser), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var userId = ((TLMessageActionChatDeleteUser)action).UserId;
                    var user = IoC.Get<ICacheService>().GetUser(userId);
                    var userFullName = GetUserFullName(user, useActiveLinks, noName);

                    if (userId.Value == fromUserId)
                    {
                        if (fromUserId == IoC.Get<IStateService>().CurrentUserId)
                        {
                            return AppResources.MessageActionLeftGroupSelf;
                        }

                        return string.Format(AppResources.MessageActionUserLeftGroup, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName));
                    }

                    return string.Format(AppResources.MessageActionChatDeleteUser, GetUserFullNameString(fromUserFullName, fromUserId, useActiveLinks, noName), userFullName);
                }
            },
            { typeof(TLMessageActionUnreadMessages), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) => AppResources.UnreadMessages.ToLowerInvariant() },
            { typeof(TLMessageActionContactRegistered), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var userId = ((TLMessageActionContactRegistered)action).UserId;
                    var user = IoC.Get<ICacheService>().GetUser(userId);
                    var userFullName = user != null ? user.FirstName.ToString() : AppResources.User;

                    if (string.IsNullOrEmpty(userFullName) && user != null)
                    {
                        userFullName = user.FullName;
                    }

                    return string.Format(AppResources.ContactRegistered, userFullName);
                }
            },


            { typeof(TLMessageActionChatJoinedByLink), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var userId = new TLInt(fromUserId);
                    var user = IoC.Get<ICacheService>().GetUser(userId);
                    var userFullName = user != null ? GetUserFullName(user, useActiveLinks, noName) : AppResources.User;

                    if (string.IsNullOrEmpty(userFullName) && user != null)
                    {
                        userFullName = user.FullName;
                    }

                    return string.Format(AppResources.MessageActionChatJoinedByLink, userFullName);
                }
            },
            { typeof(TLMessageActionMessageGroup), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var count = ((TLMessageActionMessageGroup) action).Group.Count.Value;

                    return Language.Declension(
                        count,
                        AppResources.CommentNominativeSingular,
                        AppResources.CommentNominativePlural,
                        AppResources.CommentGenitiveSingular,
                        AppResources.CommentGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                }
            },
            { typeof(TLMessageActionChatMigrateTo), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var channelId = ((TLMessageActionChatMigrateTo)action).ChannelId;
                    var channel = IoC.Get<ICacheService>().GetChat(channelId) as TLChannel;
                    var channelFullName = channel != null ? channel.FullName : string.Empty;

                    return string.Format(AppResources.MessageActionChatMigrateTo, GetChannelFullNameString(channelFullName, channelId.Value, useActiveLinks));
                }
            },
            { typeof(TLMessageActionChannelMigrateFrom), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    var chatId = ((TLMessageActionChannelMigrateFrom)action).ChatId;
                    var chat = IoC.Get<ICacheService>().GetChat(chatId);
                    var chatFullName = chat != null ? chat.FullName : string.Empty;

                    return string.Format(AppResources.MessageActionChannelMigrateFrom, GetChatFullNameString(chatFullName, chatId.Value, useActiveLinks));
                }
            },
            { typeof(TLMessageActionChatActivate), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    return AppResources.MessageActionChatActivate;
                }
            },
            { typeof(TLMessageActionChatDeactivate), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
                    return AppResources.MessageActionChatDeactivate;
                }
            },
            { typeof(TLMessageActionClearHistory), (message, action, fromUserId, fromUserFullName, useActiveLinks, noName) =>
                {
#if DEBUG
                    return string.Format("{0}", action != null ? action.GetType().Name : AppResources.MessageActionEmpty);
#endif
                    return string.Empty;
                }
            },
        };

        private static string GetPaymentSentString(TLMessageActionBase action, TLMessageBase message, string fromUserFullName)
        {
            var actionPaymentSent = action as TLMessageActionPaymentSentBase;
            if (actionPaymentSent != null)
            {
                var serviceMessage49 = message as TLMessageService49;
                if (serviceMessage49 != null)
                {
                    var replyToMsgId = serviceMessage49.ReplyToMsgId;
                    if (replyToMsgId != null)
                    {
                        TLChannel channel = null;
                        if (serviceMessage49.ToId is TLPeerChannel)
                        {
                            channel = IoC.Get<ICacheService>().GetChat(serviceMessage49.ToId.Id) as TLChannel;
                        }

                        var reply =
                            IoC.Get<ICacheService>()
                                .GetMessage(serviceMessage49.ReplyToMsgId, channel != null ? channel.Id : null) as TLMessage;
                        if (reply != null)
                        {
                            var mediaInvoice = reply.Media as TLMessageMediaInvoice;
                            if (mediaInvoice != null)
                            {
                                return string.Format(AppResources.MessageActionPaymentSentFor,
                                    (actionPaymentSent.TotalAmount.Value / Math.Pow(10.0, Currency.GetPow(actionPaymentSent.Currency.ToString())) + " " + Currency.GetSymbol(actionPaymentSent.Currency.ToString())),
                                    fromUserFullName, mediaInvoice.Title);
                            }
                        }
                    }

                    return string.Format(AppResources.MessageActionPaymentSent,
                        (actionPaymentSent.TotalAmount.Value / Math.Pow(10.0, Currency.GetPow(actionPaymentSent.Currency.ToString())) + " " + Currency.GetSymbol(actionPaymentSent.Currency.ToString())), fromUserFullName);
                }
            }

            return null;
        }

        private static string GetBoldString(string score, bool useActiveLinks)
        {
            if (!useActiveLinks)
            {
                return score;
            }

            return '\a' + "bold" + '\b' + score + '\a';
        }

        private static string GetGameFullNameString(string fullName, int msgId, TLPeerBase toId, bool useActiveLinks)
        {
            if (!useActiveLinks)
            {
                return fullName;
            }

            var channelIdString = toId is TLPeerChannel ? "&channel_id=" + toId.Id.Value : string.Empty;

            return '\a' + "tlg://?action=game&msg_id=" + msgId + channelIdString + '\b' + fullName + '\a';
        }

        private static string GetUserFullName(TLUserBase user, bool useActiveLinks, bool noName)
        {
            if (user == null) return AppResources.User;

            return GetUserFullNameString(user.FullName2, user.Index, useActiveLinks, noName);
        }

        private static string GetUserFullNameString(string fullName, int userId, bool useActiveLinks, bool noName)
        {
            if (noName)
            {
                return string.Empty;
            }

            if (!useActiveLinks)
            {
                return fullName;
            }

            return '\a' + "tlg://?action=profile&user_id=" + userId + '\b' + fullName + '\a';
        }

        private static string GetChatFullNameString(string fullName, int userId, bool useActiveLinks)
        {
            return fullName;

            if (!useActiveLinks)
            {
                return fullName;
            }

            return '\a' + "tlg://?action=profile&chat_id=" + userId + '\b' + fullName + '\a';
        }

        private static string GetChannelFullNameString(string fullName, int userId, bool useActiveLinks)
        {
            return fullName;

            if (!useActiveLinks)
            {
                return fullName;
            }

            return '\a' + "tlg://?action=profile&channel_id=" + userId + '\b' + fullName + '\a';
        }

        public static string Convert(TLMessageService serviceMessage, bool useActiveLinks = false, bool noName = false)
        {

            var fromId = serviceMessage.FromId;
            var fromUser = IoC.Get<ICacheService>().GetUser(fromId);
            var fromUserFullName = fromUser != null ? fromUser.FullName2 : AppResources.User;

            //var stateService = IoC.Get<IStateService>();
            //if (fromId.Value == stateService.CurrentUserId)
            //{
            //    fromUserFullName = AppResources.You;
            //}

            var action = serviceMessage.Action;

            if (serviceMessage.ToId is TLPeerChannel)
            {
                var channel = IoC.Get<ICacheService>().GetChat(serviceMessage.ToId.Id) as TLChannel;
                var isMegaGroup = channel != null && channel.IsMegaGroup;

                var actionPinMessage = action as TLMessageActionPinMessage;
                if (actionPinMessage != null)
                {
                    var serviceMessage49 = serviceMessage as TLMessageService49;
                    if (serviceMessage49 != null)
                    {
                        var replyToMsgId = serviceMessage49.ReplyToMsgId;
                        if (replyToMsgId != null && channel != null)
                        {
                            var reply = IoC.Get<ICacheService>().GetMessage(serviceMessage49.ReplyToMsgId, channel.Id) as TLMessage;
                            if (reply != null)
                            {
                                if (!isMegaGroup && fromUser == null)
                                {
                                    useActiveLinks = false;
                                    fromUserFullName = channel.FullName;
                                }

                                var mediaGame = reply.Media as TLMessageMediaGame;
                                if (mediaGame != null)
                                {
                                    return string.Format(AppResources.MessageActionPinGame, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }

                                var text = reply.Message.ToString();
                                if (text.Length > 0)
                                {
                                    if (text.Length > 20)
                                    {
                                        return string.Format(AppResources.MessageActionPinText, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName), text.Substring(0, 20).Replace("\r\n", "\n").Replace("\n", " ") + "...");
                                    }

                                    return string.Format(AppResources.MessageActionPinText, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName), text);
                                }

                                var mediaPhoto = reply.Media as TLMessageMediaPhoto;
                                if (mediaPhoto != null)
                                {
                                    return string.Format(AppResources.MessageActionPinPhoto, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }

                                var mediaDocument = reply.Media as TLMessageMediaDocument;
                                if (mediaDocument != null)
                                {
                                    if (TLMessageBase.IsSticker(mediaDocument.Document))
                                    {
                                        return string.Format(AppResources.MessageActionPinSticker, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                    }

                                    if (TLMessageBase.IsVoice(mediaDocument.Document))
                                    {
                                        return string.Format(AppResources.MessageActionPinVoiceMessage, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                    }

                                    if (TLMessageBase.IsMusic(mediaDocument.Document))
                                    {
                                        return string.Format(AppResources.MessageActionPinTrack, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                    }

                                    if (TLMessageBase.IsVideo(mediaDocument.Document))
                                    {
                                        return string.Format(AppResources.MessageActionPinVideo, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                    }

                                    if (TLMessageBase.IsGif(mediaDocument.Document))
                                    {
                                        return string.Format(AppResources.MessageActionPinGif, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                    }

                                    return string.Format(AppResources.MessageActionPinFile, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }

                                var mediaContact = reply.Media as TLMessageMediaContact;
                                if (mediaContact != null)
                                {
                                    return string.Format(AppResources.MessageActionPinContact, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }

                                var mediaGeoLive = reply.Media as TLMessageMediaGeoLive;
                                if (mediaGeoLive != null)
                                {
                                    return string.Format(AppResources.MessageActionPinGeoLive, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }

                                var mediaGeo = reply.Media as TLMessageMediaGeo;
                                if (mediaGeo != null)
                                {
                                    return string.Format(AppResources.MessageActionPinMap, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }

                                var mediaAudio = reply.Media as TLMessageMediaAudio;
                                if (mediaAudio != null)
                                {
                                    return string.Format(AppResources.MessageActionPinVoiceMessage, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }

                                var mediaVideo = reply.Media as TLMessageMediaVideo;
                                if (mediaVideo != null)
                                {
                                    return string.Format(AppResources.MessageActionPinVideo, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                                }
                            }
                        }

                        return string.Format(AppResources.MessageActionPinMessage, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName));
                    }
                }

                var actionChatAddUser41 = action as TLMessageActionChatAddUser41;
                if (actionChatAddUser41 != null)
                {
                    var users = ((TLMessageActionChatAddUser41)action).Users;

                    var userFullName = new List<string>();
                    foreach (var userId in users)
                    {
                        var user = IoC.Get<ICacheService>().GetUser(userId);
                        if (user != null)
                        {
                            userFullName.Add(GetUserFullName(user, useActiveLinks, noName));
                        }
                    }

                    if (users.Count == 1
                        && users[0].Value == fromId.Value)
                    {
                        if (fromId.Value == IoC.Get<IStateService>().CurrentUserId)
                        {
                            return AppResources.MessageActionChatJoinSelf;
                        }

                        return string.Format(AppResources.MessageActionChatJoin, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName), string.Join(", ", userFullName));
                    }

                    return string.Format(AppResources.MessageActionChatAddUser, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName), string.Join(", ", userFullName));
                }

                var actionChannelCreate = action as TLMessageActionChannelCreate;
                if (actionChannelCreate != null)
                {
                    return isMegaGroup
                        ? string.Format(AppResources.MessageActionChatCreate, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName), ((TLMessageActionChannelCreate)action).Title)
                        : string.Format(AppResources.MessageActionChannelCreate, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName), ((TLMessageActionChannelCreate) action).Title);
                }

                var actionChatEditPhoto = action as TLMessageActionChatEditPhoto;
                if (actionChatEditPhoto != null)
                {
                    return isMegaGroup
                        ? string.Format(AppResources.MessageActionChatEditPhoto, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName)) 
                        : AppResources.MessageActionChannelEditPhoto;
                }

                var actionChatDeletePhoto = action as TLMessageActionChatDeletePhoto;
                if (actionChatDeletePhoto != null)
                {
                    return isMegaGroup
                        ? string.Format(AppResources.MessageActionChatDeletePhoto, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName)) 
                        : AppResources.MessageActionChannelDeletePhoto;
                }

                var actionChantEditTitle = action as TLMessageActionChatEditTitle;
                if (actionChantEditTitle != null)
                {
                    return isMegaGroup
                        ? string.Format(AppResources.MessageActionChatEditTitle, GetUserFullNameString(fromUserFullName, fromId.Value, useActiveLinks, noName), actionChantEditTitle.Title)
                        : string.Format(AppResources.MessageActionChannelEditTitle, actionChantEditTitle.Title);
                }
            }
            if (action != null && _actionsCache.ContainsKey(action.GetType()))
            {
                return _actionsCache[action.GetType()](serviceMessage, action, fromId.Value, fromUserFullName, useActiveLinks, noName);
            }

#if DEBUG
            return string.Format("{0} msg_id={1}", action != null ? action.ToString() : AppResources.MessageActionEmpty, serviceMessage.Id);
#endif

            return AppResources.MessageActionEmpty;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var serviceMessage = value as TLMessageService;
            if (serviceMessage != null)
            {
                var useActiveLinks = false;
                var noUser = false;
                var p = parameter as string;
                if (p != null)
                {
                    useActiveLinks = !p.StartsWith("nolinks", StringComparison.OrdinalIgnoreCase);
                    noUser = p.EndsWith("nouser", StringComparison.OrdinalIgnoreCase);
                } 
                return Convert(serviceMessage, useActiveLinks, noUser).Trim(' ');
            }

            return AppResources.MessageActionEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
