// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Telegram.Api;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        private readonly Dictionary<string, TLMessageMediaBase> _webPagesCache = new Dictionary<string, TLMessageMediaBase>();

        private TLMessageBase _previousReply;

        private void SaveReply()
        {
            if (Reply != null && !IsWebPagePreview(Reply))
            {
                _previousReply = Reply;
            }
        }

        private void RestoreReply()
        {
            if (_previousReply != null)
            {
                Reply = _previousReply;
                _previousReply = null;
            }
            else
            {
                if (IsWebPagePreview(Reply))
                {
                    Reply = null;
                }
            }
        }

        private static bool IsWebPagePreview(TLMessageBase message)
        {
            var messagesContainer = message as TLMessagesContainter;
            if (messagesContainer != null)
            {
                return messagesContainer.WebPageMedia != null;
            }

            return false;
        }

        private TLMessage25 GetEditMessage(TLMessageBase messageBase)
        {
            TLMessage25 editMessage = null;
            var replyContainer = messageBase as TLMessagesContainter;
            if (replyContainer != null)
            {
                editMessage = replyContainer.EditMessage;
            }

            return editMessage;
        }

        private void GetWebPagePreviewAsync(string t)
        {
            if (t == null)
            {
                return;
            }

            CurrentDialog = CurrentDialog ?? CacheService.GetDialog(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));

            var dialog53 = CurrentDialog as TLDialog53;
            if (dialog53 != null)
            {
                var draft = dialog53.Draft as TLDraftMessage;
                if (draft != null && draft.NoWebpage && string.Equals(draft.Message.ToString().Trim(), t.Trim()))
                {
                    return;
                }
            }

            var text = t.Trim();
            TLMessageMediaBase webPageMedia;
            if (_webPagesCache.TryGetValue(text, out webPageMedia))
            {
                var webPageMessageMedia = webPageMedia as TLMessageMediaWebPage;
                if (webPageMessageMedia != null)
                {
                    var webPage = webPageMessageMedia.WebPage as TLWebPage;
                    if (webPage != null)
                    {
                        SaveReply();

                        Reply = new TLMessagesContainter { WebPageMedia = webPageMedia, EditMessage = GetEditMessage(Reply), };
                    }
                    else
                    {
                        RestoreReply();
                    }
                }

                return;
            }
            else
            {
                RestoreReply();
            }

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(1.0), () =>
            {
                if (!string.Equals(Text, text))
                {
                    return;
                }

                if (text.IndexOf(".", StringComparison.Ordinal) == -1)
                {
                    return;
                }

                Uri uri;
                var uriString = text.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? text
                    : "http://" + text;
                if (Uri.TryCreate(uriString, UriKind.Absolute, out uri))
                {
                    MTProtoService.GetWebPagePreviewAsync(new TLString(text),
                        result => Execute.BeginOnUIThread(() =>
                        {
                            _webPagesCache[text] = result;

                            if (!string.Equals(Text, text))
                            {
                                return;
                            }
                            var webPageMessageMedia = result as TLMessageMediaWebPage;
                            if (webPageMessageMedia != null)
                            {

                                var webPage = webPageMessageMedia.WebPage;
                                if (webPage is TLWebPage || webPage is TLWebPagePending)
                                {
                                    SaveReply();

                                    Reply = new TLMessagesContainter { WebPageMedia = result, EditMessage = GetEditMessage(Reply) };
                                }
                            }
                        }),
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.getWebPagePreview error " + error);
                        });
                }
            });
        }

        public bool SuppressOpenCommandsKeyboard { get; set; }

        public TLMessage31 ReplyMarkupMessage;

        private TLReplyKeyboardBase _replyMarkup;

        public TLReplyKeyboardBase ReplyMarkup
        {
            get { return _replyMarkup; }
            set { SetField(ref _replyMarkup, value, () => ReplyMarkup); }
        }

        private void SetReplyMarkup(TLMessage31 message, bool suppressOpenKeyboard = false)
        {
            if (Reply != null && message != null) return;

            if (message != null 
                && message.ReplyMarkup != null)
            {
                if (message.ReplyMarkup is TLReplyInlineMarkup) return;

                var replyMarkup = message.ReplyMarkup as TLReplyKeyboardMarkup;
                if (replyMarkup != null
                    && replyMarkup.IsPersonal
                    && !message.IsMention)
                {
                    return;
                }

                var keyboardHide = message.ReplyMarkup as TLReplyKeyboardHide;
                if (keyboardHide != null)
                {
                    if (ReplyMarkupMessage != null
                        && ReplyMarkupMessage.FromId.Value != message.FromId.Value)
                    {
                        return;
                    }
                }

                var forceReply = message.ReplyMarkup as TLReplyKeyboardForceReply;
                if (forceReply != null 
                    && !forceReply.HasResponse)
                {
                    ReplyMarkupMessage = null;
                    ReplyMarkup = null;
                    Reply = message;

                    return;
                }
            }

            SuppressOpenCommandsKeyboard = message != null && message.ReplyMarkup != null && suppressOpenKeyboard;
            ReplyMarkupMessage = message;
            ReplyMarkup = message != null? message.ReplyMarkup : null;
        }


        private TLMessageBase _reply;

        public TLMessageBase Reply
        {
            get { return _reply; }
            set
            {
                var notifyChanges = _reply != value;
                SetField(ref _reply, value, () => Reply);
                if (notifyChanges)
                {
                    NotifyOfPropertyChange(() => ReplyInfo);
                    //NotifyOfPropertyChange(() => CanSend);
                }
            }
        }

        public ReplyInfo ReplyInfo
        {
            get
            {
                if (_reply != null)
                {
                    return new ReplyInfo {Reply = _reply, ReplyToMsgId = _reply.Id};
                }

                return null;
            }
        }

        private Dictionary<string, IList<TLMessageBase>> _getHistoryCache = new Dictionary<string, IList<TLMessageBase>>();

        public TLMessageBase _previousScrollPosition;

        private void HighlightMessage(TLMessageBase message)
        {
            message.IsHighlighted = true;
            BeginOnUIThread(TimeSpan.FromSeconds(2.0), () =>
            {
                message.IsHighlighted = false;
            });
        }

        public void OpenEditMessage()
        {
            TLMessageBase message = null;
            var container = Reply as TLMessagesContainter;
            if (container != null) message = container.EditMessage;

            if (message == null) return;

            if (OpenMessage(Items[0] == message? null : Items[0], message.Id)) return;
        }

        public void OpenReply(TLMessageBase message)
        {
            if (message == null) return;

            var reply = message.Reply as TLMessageCommon;
            if (reply == null) return;
            if (reply.Index == 0) return;

            // migrated reply
            var channel = With as TLChannel;
            if (channel != null)
            {
                var replyPeerChat = reply.ToId as TLPeerChat;
                if (replyPeerChat != null)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var item = Items[i] as TLMessageCommon;
                        if (item != null)
                        {
                            var peerChat = item.ToId as TLPeerChat;
                            if (peerChat != null)
                            {
                                if (Items[i].Index == reply.Index)
                                {
                                    RaiseScrollTo(new ScrollToEventArgs(Items[i]));

                                    //waiting ScrollTo to complete
                                    BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                                    {
                                        HighlightMessage(Items[i]);

                                        _previousScrollPosition = message;
                                        ShowScrollToBottomButton();
                                    });

                                    return;
                                }
                            }
                        }
                    }

                    return;
                }
            }

            if (OpenMessage(message, reply.Id)) return;

            return;

            // load separated slice with reply
            Items.Clear();
            Items.Add(message.Reply);
            ShowScrollToBottomButton();
            _isFirstSliceLoaded = false;

            var key = string.Format("{0}", message.Reply.Index);
            IList<TLMessageBase> cachedMessage;
            if (_getHistoryCache.TryGetValue(key, out cachedMessage))
            {
                OpenReplyInternal(message.Reply, cachedMessage);
            }
            else
            {
                IsWorking = true;
                MTProtoService.GetHistoryAsync(
                    Stopwatch.StartNew(),
                    Peer,
                    TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                    false,
                    new TLInt(0), 
                    new TLInt(-15),
                    new TLInt(message.Reply.Index),
                    new TLInt(30),
                    result =>
                    {
                        ProcessMessages(result.Messages);
                        _getHistoryCache[key] = result.Messages;

                        BeginOnUIThread(() =>
                        {
                            OpenReplyInternal(message.Reply, result.Messages);
                            IsWorking = false;
                        });
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getHistory error " + error);
                        IsWorking = false;
                    });
            }
        }

        public bool OpenMessage(TLMessageBase previousMessage, TLInt messageId)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Index == messageId.Value)
                {
                    RaiseScrollTo(new ScrollToEventArgs(Items[i]));

                    //waiting ScrollTo to complete
                    BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                    {
                        HighlightMessage(Items[i]);

                        _previousScrollPosition = previousMessage;
                        if (_previousScrollPosition != null)
                        {
                            ShowScrollToBottomButton();
                        }
                    });
                    return true;
                }
            }

            LoadAndOpenMessage(previousMessage, messageId);

            return false;
        }

        private void LoadAndOpenMessage(TLMessageBase previousMessage, TLInt maxMessageId)
        {
            IsWorking = true;
            MTProtoService.GetHistoryAsync(Stopwatch.StartNew(),
                Peer,
                TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId),
                false,
                new TLInt(0), 
                new TLInt(-25),
                maxMessageId,
                new TLInt(Constants.MessagesSlice + 25),
                result =>
                {
#if WP8
                    var resultCount = result.Messages.Count;
                    ProcessMessages(result.Messages);

                    BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        SliceLoaded = true;

                        Items.Clear();

                        var firstSlice = new List<TLMessageBase>();
                        for (var i = 0; i < result.Messages.Count; i++)
                        {
                            var message = result.Messages[i];
                            if (message.Index > maxMessageId.Value)
                            {
                                firstSlice.Add(message);
                            }
                            else
                            {
                                if (!SkipMessage(message))
                                {
                                    Items.Add(message);
                                }
                            }
                        }

                        IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;

                        if (resultCount < Constants.MessagesSlice)
                        {
                            IsLastSliceLoaded = true;
                            LoadNextMigratedHistorySlice(Thread.CurrentThread.ManagedThreadId + " gh");
                        }

                        _isFirstSliceLoaded = false;

                        var highlightMessage = Items[0];
                        HoldScrollingPosition = true;
                        BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                        {
                            for(var i = firstSlice.Count - 1; i > 0; i--)
                            {
                                Items.Insert(0, firstSlice[i]);
                            }
                            HoldScrollingPosition = false;

                            HighlightMessage(highlightMessage);

                            _previousScrollPosition = previousMessage;
                            if (_previousScrollPosition != null)
                            {
                                ShowScrollToBottomButton();
                            }
                        });
                    });
#else
                    ProcessReplies(result.Messages);

                    IsWorking = false;
                    SliceLoaded = true;
                    IsEmptyDialog = Items.Count == 0 && LazyItems.Count == 0;

                    foreach (var message in result.Messages)
                    {
                        message._isAnimated = false;
                        LazyItems.Add(message);
                    }

                    BeginOnUIThread(PopulateItems);
#endif
                },
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Status = string.Empty;
                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        private void OpenReplyInternal(TLMessageBase reply, IList<TLMessageBase> messages)
        {
            IsFirstSliceLoaded = false;

            var startPosition = 0;
            for (var i = 0; i < messages.Count; i++)
            {
                startPosition = i;
                if (messages[i].Index == reply.Index)
                {
                    break;
                }
            }

            for (var i = startPosition + 1; i < messages.Count; i++)
            {
                Items.Add(messages[i]);
            }

            HoldScrollingPosition = true;
            BeginOnUIThread(() =>
            {
                for (var i = 0; i < startPosition - 1; i++)
                {
                    Items.Insert(i, messages[i]);
                }
                HoldScrollingPosition = false;
            });
        }

        public void ReplyMessage(TLMessageBase message)
        {
            if (message == null) return;
            var messageService = message as TLMessageService;
            if (messageService != null)
            {
                var action = messageService.Action;
                if (action is TLMessageActionEmpty
                    || action is TLMessageActionUnreadMessages)
                {
                    return;
                }
            }
            if (message.Index <= 0) return;

            var message31 = message as TLMessage31;
            if (message31 != null && !message31.Out.Value)
            {
                var fromId = message31.FromId;
                var user = CacheService.GetUser(fromId) as TLUser;
                if (user != null && user.IsBot)
                {
                    SetReplyMarkup(message31);
                }
            }

            Reply = message;
        }

        public void DeleteReply()
        {
            var message31 = Reply as TLMessage31;
            if (message31 != null)
            {
                if (message31.ReplyMarkup != null)
                {
                    message31.ReplyMarkup.HasResponse = true;
                }
            }

            if (_previousReply != null)
            {
                Reply = _previousReply;
                _previousReply = null;
            }
            else
            {
                if (ReplyMarkupMessage == Reply)
                {
                    SetReplyMarkup(null);
                }
                Reply = null;
            }
        }

        private Dictionary<long, TLMessage> _group = new Dictionary<long, TLMessage>();

        public void ProcessMessages(IList<TLMessageBase> messages)
        {
            foreach (var messageBase in messages)
            {
                var message = messageBase as TLMessage48;
                if (message != null)
                {
                    var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                    if (mediaGeoLive != null)
                    {
                        mediaGeoLive.From = message.From;
                        mediaGeoLive.Date = message.Date;
                        mediaGeoLive.EditDate = message.EditDate;
                    }

                    var mediaGame = message.Media as TLMessageMediaGame;
                    if (mediaGame != null)
                    {
                        mediaGame.Message = message.Message;
                        mediaGame.SourceMessage = message;
                    }
                }

                var message70 = message as TLMessage70;
                if (message70 != null && !message70.NotListened && message70.HasTTL())
                {
                    var mediaPhoto = message70.Media as TLMessageMediaPhoto70;
                    if (mediaPhoto != null)
                    {
                        mediaPhoto.Photo = null;
                    }

                    var mediaDocument = message70.Media as TLMessageMediaDocument70;
                    if (mediaDocument != null)
                    {
                        mediaDocument.Document = null;
                    }

                    var mediaVenue = message70.Media as TLMessageMediaVenue72;
                    if (mediaVenue != null)
                    {
                        mediaVenue.User = message.From as TLUser;
                    }
                }
            }

            ProcessGroupedMessages(messages);

            var replyToMsgIds = new TLVector<TLInputMessageBase>();
            var replyToMsgs = new List<IReplyToMsgId>();
            for (var i = 0; i < messages.Count; i++)
            {
                var messageWithReply = messages[i] as IReplyToMsgId;
                var message25 = messages[i] as TLMessage25;
                var messageService49 = messages[i] as TLMessageService49;
                if (messageWithReply != null)
                {
                    var replyToMsgId = messageWithReply.ReplyToMsgId;
                    if (replyToMsgId != null
                        && replyToMsgId.Value != 0)
                    {
                        TLInt channelId = null;
                        var peerChannel = messageWithReply.ToId as TLPeerChannel;
                        if (peerChannel != null)
                        {
                            channelId = peerChannel.Id;
                        }

                        var reply = CacheService.GetMessage(replyToMsgId, channelId);
                        if (reply != null)
                        {
                            messages[i].Reply = reply;
                        }
                        else
                        {
                            replyToMsgIds.Add(new TLInputMessageId { Id = replyToMsgId });
                            replyToMsgs.Add(messageWithReply);
                        }
                    }

                    if (message25 != null
                        && message25.Media != null)
                    {
                        message25.Media.Out = message25.Out.Value;
                        message25.Media.NotListened = message25.NotListened;
                    }
                }
            }

            if (replyToMsgIds.Count > 0)
            {
                var channel = With as TLChannel;
                if (channel != null)
                {
                    var firstReplyToMsg = replyToMsgs.FirstOrDefault();
                    var peerChat = firstReplyToMsg != null ? firstReplyToMsg.ToId as TLPeerChat : null;
                    if (peerChat != null)
                    {
                        MTProtoService.GetMessagesAsync(
                            replyToMsgIds,
                            result =>
                            {
                                CacheService.AddChats(result.Chats, results => { });
                                CacheService.AddUsers(result.Users, results => { });

                                for (var i = 0; i < result.Messages.Count; i++)
                                {
                                    for (var j = 0; j < replyToMsgs.Count; j++)
                                    {
                                        var messageToReply = replyToMsgs[j];
                                        if (messageToReply != null
                                            && messageToReply.ReplyToMsgId.Value == result.Messages[i].Index)
                                        {
                                            replyToMsgs[j].Reply = result.Messages[i];
                                        }
                                    }
                                }

                                for (var i = 0; i < replyToMsgs.Count; i++)
                                {
                                    var obj = replyToMsgs[i] as TLObject;
                                    if (obj != null)
                                    {
                                        obj.NotifyOfPropertyChange(() => replyToMsgs[i].ReplyInfo);
                                    }
                                }
                            },
                            error =>
                            {
                                Execute.ShowDebugMessage("messages.getMessages error " + error);
                            });
                    }
                    else
                    {
                        MTProtoService.GetMessagesAsync(
                           channel.ToInputChannel(),
                           replyToMsgIds,
                           result =>
                           {
                               CacheService.AddChats(result.Chats, results => { });
                               CacheService.AddUsers(result.Users, results => { });

                               for (var i = 0; i < result.Messages.Count; i++)
                               {
                                   for (var j = 0; j < replyToMsgs.Count; j++)
                                   {
                                       var messageToReply = replyToMsgs[j];
                                       if (messageToReply != null
                                           && messageToReply.ReplyToMsgId.Value == result.Messages[i].Index)
                                       {
                                           replyToMsgs[j].Reply = result.Messages[i];
                                       }
                                   }
                               }

                               for (var i = 0; i < replyToMsgs.Count; i++)
                               {
                                   var obj = replyToMsgs[i] as TLObject;
                                   if (obj != null)
                                   {
                                       obj.NotifyOfPropertyChange(() => replyToMsgs[i].ReplyInfo);
                                   }
                               }
                           },
                           error =>
                           {
                               Execute.ShowDebugMessage("channels.getMessages error " + error);
                           });
                    }
                }
                else
                {
                    MTProtoService.GetMessagesAsync(
                        replyToMsgIds,
                        result =>
                        {
                            CacheService.AddChats(result.Chats, results => { });
                            CacheService.AddUsers(result.Users, results => { });

                            for (var i = 0; i < result.Messages.Count; i++)
                            {
                                for (var j = 0; j < replyToMsgs.Count; j++)
                                {
                                    var messageToReply = replyToMsgs[j];
                                    if (messageToReply != null
                                        && messageToReply.ReplyToMsgId.Value == result.Messages[i].Index)
                                    {
                                        replyToMsgs[j].Reply = result.Messages[i];
                                    }
                                }
                            }

                            for (var i = 0; i < replyToMsgs.Count; i++)
                            {
                                var obj = replyToMsgs[i] as TLObject;
                                if (obj != null)
                                {
                                    obj.NotifyOfPropertyChange(() => replyToMsgs[i].ReplyInfo);
                                }
                            }
                        },
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.getMessages error " + error);
                        });
                }
            }
        }

        private void ProcessGroupedMessages(IList<TLMessageBase> messages)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                var firstMessage = messages[i] as TLMessage73;
                if (firstMessage != null && firstMessage.GroupedId != null && !firstMessage.IsExpired())
                {
                    var mediaPhoto = firstMessage.Media as TLMessageMediaPhoto;
                    var mediaDocument = firstMessage.Media as TLMessageMediaDocument;

                    if (mediaPhoto != null || mediaDocument != null)
                    {
                        var groupCount = GetGroupLength(messages, firstMessage.GroupedId, i + 1);
                        if (groupCount > 0)
                        {
                            var groupedMessage = ReplaceWithGroup(messages, i, groupCount + 1);
                            _group[firstMessage.GroupedId.Value] = groupedMessage;
                        }
                    }
                }
            }
        }

        private static TLMessage ReplaceWithGroup(IList<TLMessageBase> messages, int position, int length)
        {
            var message = messages[position + length - 1] as TLMessage73;
            if (message != null)
            {
                var group = new TLVector<TLMessageBase>();
                for (var i = 0; i < length; i++)
                {
                    group.Insert(0, messages[position]);
                    messages.RemoveAt(position);
                }

                var mediaGroup = new TLMessageMediaGroup{ Group = group };
                
                var groupedMessage = new TLMessage73
                {
                    Flags = new TLInt(0),
                    Out = message.Out,
                    Unread = message.Unread,
                    Id = message.Id,
                    RandomId = message.RandomId,
                    FromId = message.FromId,
                    ToId = message.ToId,
                    FwdHeader = message.FwdHeader,
                    ViaBotId = message.ViaBotId,
                    ReplyToMsgId = message.ReplyToMsgId,
                    Date = message.Date,
                    Message = TLString.Empty,
                    _media = mediaGroup,
                    ReplyMarkup = message.ReplyMarkup,
                    Entities = new TLVector<TLMessageEntityBase>(),
                    Views = message.Views,
                    EditDate = message.EditDate,
                    PostAuthor = message.PostAuthor,
                    GroupedId = message.GroupedId,
                    _status = message.Status
                };

                if (groupedMessage.FromId != null) groupedMessage.SetFromId();
                if (groupedMessage._media != null) groupedMessage.SetMedia();

                messages.Insert(position, groupedMessage);

                return groupedMessage;
            }

            return null;
        }

        private static int GetGroupLength(IList<TLMessageBase> messages, TLLong groupId, int start)
        {
            var count = 0;
            for (var i = start; i < messages.Count; i++)
            {
                var message = messages[i] as TLMessage73;
                if (message != null 
                    && message.GroupedId != null 
                    && message.GroupedId.Value == groupId.Value
                    && !message.IsExpired()
                    && (message.Media is TLMessageMediaPhoto || message.Media is TLMessageMediaDocument))
                {
                    count++;
                }
                else
                {
                    return count;
                }
            }

            return count;
        }

        public CommandHintsViewModel CommandHints { get; protected set; }

        private void CreateCommandHints()
        {
            if (CommandHints == null)
            {
                CommandHints = new CommandHintsViewModel(With);
                NotifyOfPropertyChange(() => CommandHints);
            }
        }

        private void ClearCommandHints()
        {
            if (CommandHints != null)
            {
                CommandHints.Hints.Clear();
            }
        }

        private static bool IsValidCommandSymbol(char symbol)
        {
            if ((symbol >= 'a' && symbol <= 'z')
                || (symbol >= 'A' && symbol <= 'Z')
                || (symbol >= '0' && symbol <= '9')
                || symbol == '_')
            {
                return true;
            }

            return false;
        }

        private readonly Dictionary<string, IList<TLBotCommand>> _cachedCommandResults = new Dictionary<string, IList<TLBotCommand>>();

        private IList<TLBotCommand> _commands; 

        private IList<TLBotCommand> GetCommands()
        {
            if (_commands != null)
            {
                return _commands;
            }

            var user = With as TLUserBase;
            if (user != null)
            {
                if (user.BotInfo == null)
                {
                    return null;
                }

                var botInfo = user.BotInfo as TLBotInfo;
                if (botInfo != null)
                {
                    foreach (var command in botInfo.Commands)
                    {
                        command.Bot = user;
                    }

                    _commands = botInfo.Commands;
                }
            }

            var chat = With as TLChatBase;
            if (chat != null)
            {
                if (chat.BotInfo == null)
                {
                    return null;
                }

                var commands = new TLVector<TLBotCommand>();
                foreach (var botInfoBase in chat.BotInfo)
                {
                    var botInfo = botInfoBase as TLBotInfo;
                    if (botInfo != null)
                    {
                        user = CacheService.GetUser(botInfo.UserId);

                        foreach (var command in botInfo.Commands)
                        {
                            command.Bot = user;
                            commands.Add(command);
                        }
                    }
                }

                _commands = commands;
            }

            return _commands;
        } 

        private void GetCommandHints(string text)
        {
            var commands = GetCommands();

            if (text == null) return;
            text = text.TrimStart('/');

            if (commands == null)
            {
                GetFullInfo();
                return;
            }

            ClearCommandHints();

            IList<TLBotCommand> cachedResult;
            if (!_cachedCommandResults.TryGetValue(text, out cachedResult))
            {
                cachedResult = new List<TLBotCommand>();
                for (var i = 0; i < commands.Count; i++)
                {
                    var command = commands[i].Command.ToString();
                    if (!string.IsNullOrEmpty(command)
                        && (string.IsNullOrEmpty(text) || command.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
                    {
                        cachedResult.Add(commands[i]);
                    }
                }

                _cachedCommandResults[text] = cachedResult;
            }

            if (cachedResult.Count > 0)
            {
                CreateCommandHints();

                for (var i = 0; i < cachedResult.Count; i++)
                {
                    if (i == MaxResults) break;

                    CommandHints.Hints.Add(cachedResult[i]);
                }
            }
        }

        private static bool SearchByCommands(string text, out string searchText)
        {
            var symbol = '/';

            var searchByCommands = true;
            var commandIndex = -1;
            searchText = string.Empty;
            for (var i = text.Length - 1; i >= 0; i--)
            {
                if (text[i] == symbol)
                {
                    if (i == 0
                        || text[i - 1] == ' ')
                    {
                        commandIndex = i;
                    }
                    else
                    {
                        searchByCommands = false;
                    }
                    break;
                }

                if (!IsValidCommandSymbol(text[i]))
                {
                    searchByCommands = false;
                    break;
                }
            }

            if (searchByCommands)
            {
                if (commandIndex == -1)
                {
                    return false;
                }

                searchText = text.Substring(commandIndex).TrimStart(symbol);
            }

            return searchByCommands;
        }

        public void ContinueCommandHints()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                string searchText;
                var searchByCommands = SearchByCommands(Text, out searchText);

                if (searchByCommands)
                {
                    CreateCommandHints();

                    if (CommandHints.Hints.Count == MaxResults)
                    {
                        IList<TLBotCommand> cachedResult;
                        if (_cachedCommandResults.TryGetValue(searchText, out cachedResult))
                        {
                            for (var i = MaxResults; i < cachedResult.Count; i++)
                            {
                                CommandHints.Hints.Add(cachedResult[i]);
                            }
                        }
                    }
                }
            }
        }

        public StickerHintsViewModel StickerHints { get; protected set; }

        private void CreateStickerHints()
        {
            if (StickerHints == null)
            {
                StickerHints = new StickerHintsViewModel();
                NotifyOfPropertyChange(() => StickerHints);
            }
        }

        private void ClearStickerHints()
        {
            if (StickerHints != null)
            {
                StickerHints.Hints.Clear();
            }
        }

        private readonly Dictionary<string, IList<TLStickerItem>> _cachedStickerResults = new Dictionary<string, IList<TLStickerItem>>();

        private void GetStickerHints(string text)
        {
            var stickersCache = new Dictionary<long, long>();
            var stickers = new List<TLStickerItem>();
            var currentStickerText = text.Trim();

            var allStickers43 = StateService.GetAllStickers() as TLAllStickers43;
            if (allStickers43 != null)
            {
                if (allStickers43.ShowStickersByEmoji == ShowStickersByEmoji.AllSets
                    || allStickers43.ShowStickersByEmoji == ShowStickersByEmoji.MySets)
                {
                    var favedDict = new Dictionary<long, long>();
                    var favedStickers = allStickers43.FavedStickers;
                    if (favedStickers != null)
                    {
                        for (var i = 0; i < favedStickers.Documents.Count; i++)
                        {
                            favedDict[favedStickers.Documents[i].Index] = favedStickers.Documents[i].Index;
                        }
                    }

                    // 1. 5 most recently used (flag=[my sets, all sets])
                    var recentStickers = allStickers43.RecentStickers;
                    if (recentStickers != null)
                    {
                        const int maxRecentCount = 5;
                        var recentCount = 0;
                        for (var i = 0; i < recentStickers.Documents.Count && recentCount < maxRecentCount; i++)
                        {
                            var sticker = recentStickers.Documents[i] as TLDocument22;
                            if (sticker != null && sticker.Emoticon == text && !stickersCache.ContainsKey(sticker.Index) && !favedDict.ContainsKey(sticker.Index))
                            {
                                stickers.Add(new TLStickerItem { Document = sticker });
                                stickersCache[sticker.Index] = sticker.Index;
                                recentCount++;
                            }
                        }
                    }

                    // 2. faved stickers (flag=[my sets, all sets])
                    if (favedStickers != null)
                    {
                        for (var i = 0; i < favedStickers.Documents.Count; i++)
                        {
                            var sticker = favedStickers.Documents[i] as TLDocument22;
                            if (sticker != null && sticker.Emoticon == text && !stickersCache.ContainsKey(sticker.Index))
                            {
                                stickers.Add(new TLStickerItem { Document = sticker });
                                stickersCache[sticker.Index] = sticker.Index;
                            }
                        }
                    }

                    // 3. installed stickers (flag=[my sets, all sets])
                    var stickerPack = GetStickerPack(currentStickerText);
                    if (stickerPack != null)
                    {
                        for (var i = 0; i < stickerPack.Documents.Count; i++)
                        {
                            var sticker = Stickers.Documents.FirstOrDefault(x => x.Id.Value == stickerPack.Documents[i].Value);
                            if (sticker != null && !stickersCache.ContainsKey(sticker.Index))
                            {
                                stickers.Add(new TLStickerItem { Document = sticker });
                                stickersCache[sticker.Index] = sticker.Index;
                            }
                        }
                    }
                }

                if (allStickers43.ShowStickersByEmoji == ShowStickersByEmoji.AllSets)
                {
                    // 4. featured stickers (flag=[all sets])
                    var featuredPack = GetFeaturedStickerPack(currentStickerText);
                    if (featuredPack != null)
                    {
                        var featuredStickers = StateService.GetFeaturedStickers();
                        for (var i = 0; i < featuredPack.Documents.Count; i++)
                        {
                            var sticker = featuredStickers.Documents.FirstOrDefault(x => x.Id.Value == featuredPack.Documents[i].Value);
                            if (sticker != null && !stickersCache.ContainsKey(sticker.Index))
                            {
                                stickers.Add(new TLStickerItem { Document = sticker });
                                stickersCache[sticker.Index] = sticker.Index;
                            }
                        }
                    }

                    // 5. search on server side
                }
            }

            var key = string.Format("{0}\ashowStickersByEmoji={1}", text, allStickers43 != null ? allStickers43.ShowStickersByEmoji : ShowStickersByEmoji.AllSets);
            _cachedStickerResults[key] = stickers;

            ClearStickerHints();

            if (stickers.Count > 0)
            {
                CreateStickerHints();

                for (var i = 0; i < stickers.Count; i++)
                {
                    if (i == MaxResults) break;

                    StickerHints.Hints.Add(stickers[i]);
                }
            }
        }

        private static bool SearchByStickers(string text, out string searchText)
        {
            searchText = text.Trim();
            if (searchText.Length > 2)
            {
                return false;
            }

            return true;
        } 

        public void ContinueStickerHints()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                string searchText;
                var searchByStickers = SearchByStickers(Text, out searchText);

                if (searchByStickers)
                {
                    CreateStickerHints();

                    if (StickerHints.Hints.Count == MaxResults)
                    {
                        IList<TLStickerItem> cachedResult;
                        var allStickers43 = StateService.GetAllStickers() as TLAllStickers43;
                        var key = string.Format("{0}\ashowStickersByEmoji={1}", searchText, allStickers43 != null ? allStickers43.ShowStickersByEmoji : ShowStickersByEmoji.AllSets);
                        if (_cachedStickerResults.TryGetValue(key, out cachedResult))
                        {
                            for (var i = MaxResults; i < cachedResult.Count; i++)
                            {
                                StickerHints.Hints.Add(cachedResult[i]);
                            }
                        }
                    }
                }
            }
        }

        public UsernameHintsViewModel UsernameHints { get; protected set; }

        private void CreateUsernameHints()
        {
            if (UsernameHints == null)
            {
                UsernameHints = new UsernameHintsViewModel();
                NotifyOfPropertyChange(() => UsernameHints);
            }
        }

        private void ClearUsernameHints()
        {
            if (UsernameHints != null)
            {
                UsernameHints.Hints.Clear();
            }
        }

        private static bool IsValidUsernameSymbol(char symbol)
        {
            if ((symbol >= 'a' && symbol <= 'z')
                || (symbol >= 'A' && symbol <= 'Z')
                || (symbol >= '0' && symbol <= '9')
                || symbol == '_')
            {
                return true;
            }

            return false;
        }

        private IList<TLUserBase> GetUsernameHintsExternal(string text)
        {
            string searchText;
            var searchByUsernames = SearchByUsernames(text, out searchText);
            if (searchByUsernames)
            {
                GetUsernameHints(searchText);
                ClearUsernameHints();

                IList<TLUserBase> hints;
                if (_cachedUsernameResults.TryGetValue(searchText, out hints))
                {
                    return hints;
                }
            }

            return new List<TLUserBase>();
        }

        private readonly Dictionary<string, IList<TLUserBase>> _cachedUsernameResults = new Dictionary<string, IList<TLUserBase>>();

        private const int MaxResults = 10;

        private void GetUsernameHints(string text)
        {
            if (text == null) return;
            text = text.TrimStart('@');

            IList<TLUserBase> cachedResult = new List<TLUserBase>();
            var chat = With as TLChat;
            if (chat != null)
            {
                if (!GetChatUsernameHints(text, chat, out cachedResult)) return;
            }
            var channel = With as TLChannel;
            if (channel != null)
            {
                if (!GetChannelUsernameHints(text, channel, out cachedResult)) return;
            }
            var user = With as TLUserBase;
            if (user != null)
            {
                if (!GetParticipantUsernameHints(text, user, out cachedResult)) return;
            }

            ClearUsernameHints();

            if (cachedResult.Count > 0)
            {
                CreateUsernameHints();

                var addInlineBots = AddInlineBots();

                for (var i = 0; i < cachedResult.Count; i++)
                {
                    if (UsernameHints.Hints.Count == MaxResults) break;

                    if (!addInlineBots)
                    {
                        var bot = cachedResult[i] as TLUser;
                        if (bot != null && bot.IsInlineBot) continue;
                    }

                    UsernameHints.Hints.Add(cachedResult[i]);
                }
            }
        }

        private bool GetParticipantUsernameHints(string text, TLUserBase userBase, out IList<TLUserBase> cachedResult)
        {
            cachedResult = new List<TLUserBase>();

            var inlineBots = GetInlineBots();
            if (inlineBots == null)
            {
                return false;
            }

            if (!_cachedUsernameResults.TryGetValue(text, out cachedResult))
            {
                cachedResult = new List<TLUserBase>();
                var resultIndex = new Dictionary<int, int>();
                resultIndex[StateService.CurrentUserId] = StateService.CurrentUserId;

                for (var i = 0; i < inlineBots.Count; i++)
                {
                    var inlineBot = inlineBots[i];
                    if (!resultIndex.ContainsKey(inlineBot.Index))
                    {
                        var userName = inlineBot as IUserName;
                        if (userName != null)
                        {
                            var userNameValue = userName.UserName.ToString();

                            if (!string.IsNullOrEmpty(userNameValue)
                                && (string.IsNullOrEmpty(text) || userNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
                            {
                                cachedResult.Add(inlineBot);
                                resultIndex[inlineBot.Index] = inlineBot.Index;
                            }
                        }
                    }
                }

                _cachedUsernameResults[text] = cachedResult;
            }

            return true;
        }

        private bool GetChatUsernameHints(string text, TLChat chat, out IList<TLUserBase> cachedResult)
        {
            cachedResult = new List<TLUserBase>();

            if (chat.Participants == null)
            {
                GetFullInfo();

                return false;
            }

            var participants = chat.Participants as IChatParticipants;
            if (participants == null)
            {
                return false;
            }

            if (!_cachedUsernameResults.TryGetValue(text, out cachedResult))
            {
                cachedResult = new List<TLUserBase>();
                var resultIndex = new Dictionary<int, int>();
                resultIndex[StateService.CurrentUserId] = StateService.CurrentUserId;

                var inlineBots = GetInlineBots();
                if (inlineBots != null)
                {
                    var maxInlineBotsCount = 5;
                    for (var i = 0; i < inlineBots.Count; i++)
                    {
                        if (i == maxInlineBotsCount) break;

                        var inlineBot = inlineBots[i];
                        if (!resultIndex.ContainsKey(inlineBot.Index))
                        {
                            var userName = inlineBot as IUserName;
                            if (userName != null)
                            {
                                var userNameValue = userName.UserName.ToString();

                                if (!string.IsNullOrEmpty(userNameValue)
                                    && (string.IsNullOrEmpty(text) || userNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
                                {
                                    cachedResult.Add(inlineBot);
                                    resultIndex[inlineBot.Index] = inlineBot.Index;
                                }
                            }
                        }
                    }
                }

                for (var i = 0; i < participants.Participants.Count; i++)
                {
                    var user = CacheService.GetUser(participants.Participants[i].UserId);
                    if (!resultIndex.ContainsKey(user.Index))
                    {
                        var userName = user as IUserName;
                        if (userName != null)
                        {
                            var userNameValue = userName.UserName.ToString();
                            var firstNameValue = user.FirstName.ToString();
                            var lastNameValue = user.LastName.ToString();

                            if (string.IsNullOrEmpty(text) || userNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase) || firstNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase) || lastNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                            {
                                cachedResult.Add(user);
                                resultIndex[user.Index] = user.Index;
                            }
                        }
                    }
                }

                _cachedUsernameResults[text] = cachedResult;
            }

            return true;
        }

        private bool GetChannelUsernameHints(string text, TLChannel chat, out IList<TLUserBase> cachedResult)
        {
            cachedResult = new List<TLUserBase>();

            if (chat.ChannelParticipants == null)
            {
                GetFullInfo();

                return false;
            }

            var participants = chat.ChannelParticipants as TLChannelParticipants;
            if (participants == null)
            {
                return false;
            }

            if (!_cachedUsernameResults.TryGetValue(text, out cachedResult))
            {
                cachedResult = new List<TLUserBase>();
                var resultIndex = new Dictionary<int, int>();
                resultIndex[StateService.CurrentUserId] = StateService.CurrentUserId;

                var inlineBots = GetInlineBots();
                if (inlineBots != null)
                {
                    var maxInlineBotsCount = 5;
                    for (var i = 0; i < inlineBots.Count; i++)
                    {
                        if (i == maxInlineBotsCount) break;

                        var inlineBot = inlineBots[i];
                        if (!resultIndex.ContainsKey(inlineBot.Index))
                        {
                            var userName = inlineBot as IUserName;
                            if (userName != null)
                            {
                                var userNameValue = userName.UserName.ToString();

                                if (!string.IsNullOrEmpty(userNameValue)
                                    && (string.IsNullOrEmpty(text) || userNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
                                {
                                    cachedResult.Add(inlineBot);
                                    resultIndex[inlineBot.Index] = inlineBot.Index;
                                }
                            }
                        }
                    }
                }

                var usersCache = new Dictionary<int, TLUserBase>();
                foreach (var user in participants.Users)
                {
                    usersCache[user.Index] = user;
                }
                for (var i = 0; i < participants.Participants.Count; i++)
                {
                    TLUserBase user;
                    if (usersCache.TryGetValue(participants.Participants[i].UserId.Value, out user))
                    {
                        if (!resultIndex.ContainsKey(user.Index))
                        {
                            var userName = user as IUserName;
                            if (userName != null)
                            {
                                var userNameValue = userName.UserName.ToString();
                                var firstNameValue = user.FirstName.ToString();
                                var lastNameValue = user.LastName.ToString();

                                if (string.IsNullOrEmpty(text) || userNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase) || firstNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase) || lastNameValue.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                                {
                                    cachedResult.Add(user);
                                    resultIndex[user.Index] = user.Index;
                                }
                            }
                        }
                    }
                }

                _cachedUsernameResults[text] = cachedResult;
            }

            return true;
        }

        private static bool SearchByUsernames(string text, out string searchText)
        {
            var searchByUsernames = true;
            var usernameIndex = -1;
            searchText = string.Empty;
            for (var i = text.Length - 1; i >= 0; i--)
            {
                if (text[i] == '@')
                {
                    if (i == 0
                        || text[i - 1] == ' ')
                    {
                        usernameIndex = i;
                    }
                    else
                    {
                        searchByUsernames = false;
                    }
                    break;
                }

                if (!IsValidUsernameSymbol(text[i]))
                {
                    searchByUsernames = false;
                    break;
                }
            }

            if (searchByUsernames)
            {
                if (usernameIndex == -1)
                {
                    return false;
                }

                searchText = text.Substring(usernameIndex).TrimStart('@');
            }

            return searchByUsernames;
        } 

        public void ContinueUsernameHints()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                string searchText;
                var searchByUsernames = SearchByUsernames(Text, out searchText);

                if (searchByUsernames)
                {
                    CreateUsernameHints();
                            
                    if (UsernameHints.Hints.Count == MaxResults)
                    {
                        IList<TLUserBase> cachedResult;
                        if (_cachedUsernameResults.TryGetValue(searchText, out cachedResult))
                        {
                            var addInlineBots = AddInlineBots();

                            var lastItem = UsernameHints.Hints.LastOrDefault();
                            if (lastItem != null)
                            {
                                var lastIndex = cachedResult.IndexOf(lastItem);
                                if (lastIndex >= 0)
                                {
                                    for (var i = lastIndex + 1; i < cachedResult.Count; i++)
                                    {
                                        if (!addInlineBots)
                                        {
                                            var bot = cachedResult[i] as TLUser;
                                            if (bot != null && bot.IsInlineBot) continue;
                                        }

                                        UsernameHints.Hints.Add(cachedResult[i]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool AddInlineBots()
        {
            var addInlineBots = false;

            if (_editedMessage != null)
            {
                return false;
            }

            var originalText = Text.Trim().TrimStart('@');
            if (string.IsNullOrEmpty(originalText))
            {
                addInlineBots = true;
            }

            return addInlineBots;
        }

        public HashtagHintsViewModel HashtagHints { get; protected set; }

        public void CreateHashtagHints()
        {
            if (HashtagHints == null)
            {
                HashtagHints = new HashtagHintsViewModel();
                NotifyOfPropertyChange(() => HashtagHints);
            }
        }

        public void ClearHashtagHints()
        {
            if (HashtagHints != null)
            {
                HashtagHints.Hints.Clear();
            }
        }

        private static bool IsValidHashtagSymbol(char symbol)
        {
            if ((symbol >= 'a' && symbol <= 'z')
                || (symbol >= 'A' && symbol <= 'Z')
                || (symbol >= 'а' && symbol <= 'я')
                || (symbol >= 'А' && symbol <= 'Я')
                || (symbol >= '0' && symbol <= '9')
                || symbol == '_')
            {
                return true;
            }

            return false;
        }

        private readonly Dictionary<string, IList<TLHashtagItem>> _cachedHashtagResults = new Dictionary<string, IList<TLHashtagItem>>();

        private void GetHashtagHints(string text)
        {
            if (text == null) return;
            text = text.TrimStart('#');

            ClearHashtagHints();

            var hashtags = GetHashtagsFromFile();

            IList<TLHashtagItem> cachedResult;
            if (!_cachedHashtagResults.TryGetValue(text, out cachedResult))
            {
                cachedResult = new List<TLHashtagItem>();
                for (var i = 0; i < hashtags.Count; i++)
                {
                    var hashtagItem = hashtags[i];
                    if (hashtagItem != null)
                    {
                        var hashtag = hashtagItem.Hashtag;
                        if (hashtag != null)
                        {
                            var hashtagValue = hashtag.ToString();

                            if (!string.IsNullOrEmpty(hashtagValue)
                                && (string.IsNullOrEmpty(text) || hashtagValue.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
                            {
                                cachedResult.Add(hashtagItem);
                            }
                        }
                    }
                }

                _cachedHashtagResults[text] = cachedResult;
            }

            if (cachedResult.Count > 0)
            {
                CreateHashtagHints();

                for (var i = 0; i < cachedResult.Count; i++)
                {
                    if (i == MaxResults) break;
                    HashtagHints.Hints.Add(cachedResult[i]);
                }
            }
        }

        private static TLVector<TLHashtagItem> _hashtagItems = new TLVector<TLHashtagItem>
            {
                new TLHashtagItem {Hashtag = new TLString("test")},
                new TLHashtagItem {Hashtag = new TLString("wp")},
                new TLHashtagItem {Hashtag = new TLString("telegram")},
                //new TLHashtagItem {Hashtag = new TLString("test4")},
                //new TLHashtagItem {Hashtag = new TLString("test5")}
            };

        private static Dictionary<string, string> _hashtagItemsDict = new Dictionary<string, string>(); 

        private TLVector<TLHashtagItem> GetHashtagsFromFile()
        {
            var result = _hashtagItems;

            foreach (var hashtagItem in _hashtagItems)
            {
                var hashtag = hashtagItem.Hashtag.ToString();
                _hashtagItemsDict[hashtag] = hashtag;
            }

            return result;
        }

        private void AddHashtagsToFile(IList<TLHashtagItem> items)
        {
            bool clearCache = false;
            foreach (var item in items)
            {
                var hashtag = item.Hashtag.ToString();
                if (!_hashtagItemsDict.ContainsKey(hashtag))
                {
                    _hashtagItemsDict[hashtag] = hashtag;
                    _hashtagItems.Insert(0, item);
                    clearCache = true;
                }
            }

            if (clearCache)
            {
                _cachedHashtagResults.Clear();
            }
        }

        private void CheckHashcodes(string text)
        {
            var regexp = new Regex("(^|\\s)#[\\w@\\.]+", RegexOptions.IgnoreCase);

            var hashtags = new List<TLHashtagItem>();
            foreach (var match in regexp.Matches(text))
            {
                hashtags.Add(new TLHashtagItem(match.ToString().Trim().TrimStart('#')));
            }
            regexp.Matches(text);

            AddHashtagsToFile(hashtags);
        }

        private void ClearHashtagsFile()
        {
            _hashtagItems.Clear();
        }

        private static bool SearchByHashtags(string text, out string searchText)
        {
            var searchByHashtags = true;
            var hashtagIndex = -1;
            searchText = string.Empty;
            for (var i = text.Length - 1; i >= 0; i--)
            {
                if (text[i] == '#')
                {
                    if (i == 0
                        || text[i - 1] == ' ')
                    {
                        hashtagIndex = i;
                    }
                    else
                    {
                        searchByHashtags = false;
                    }
                    break;
                }

                if (!IsValidHashtagSymbol(text[i]))
                {
                    searchByHashtags = false;
                    break;
                }
            }


            if (searchByHashtags)
            {
                if (hashtagIndex == -1)
                {
                    return false;
                }

                searchText = text.Substring(hashtagIndex).TrimStart('#');
            }

            return searchByHashtags;
        }

        public void ContinueHashtagHints()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                string searchText;
                var searchByHashtags = SearchByHashtags(Text, out searchText);

                if (searchByHashtags)
                {
                    CreateHashtagHints();

                    if (HashtagHints.Hints.Count == MaxResults)
                    {
                        IList<TLHashtagItem> cachedResult;
                        if (_cachedHashtagResults.TryGetValue(searchText, out cachedResult))
                        {
                            for (var i = MaxResults; i < cachedResult.Count; i++)
                            {
                                HashtagHints.Hints.Add(cachedResult[i]);
                            }
                        }
                    }
                }
            }
        }

        public void ClearHashtags()
        {
            var result = MessageBox.Show("Clear search history?", AppResources.Confirm, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                _cachedHashtagResults.Clear();
                ClearHashtagHints();
                ClearHashtagsFile();
            }
        }

        //private TLMessageBase _pinnedMessage;

        //public TLMessageBase PinnedMessage
        //{
        //    get { return _pinnedMessage; }
        //    set { SetField(ref _pinnedMessage, value, () => PinnedMessage); }
        //}

        private PinnedMessageViewModel _pinnedMessage;

        public PinnedMessageViewModel PinnedMessage
        {
            get
            {
                ShellViewModel.WriteTimer("DialogDetailsViewModel PinnedMessage");
                return _pinnedMessage;
            }
            set { SetField(ref _pinnedMessage, value, () => PinnedMessage); }
        }

        public void PinMessage(TLMessageBase messageBase)
        {
            if (messageBase == null) return;
            var channel = With as TLChannel;
            if (channel == null) return;

            if (PinnedMessage != null
                && PinnedMessage.Message == messageBase)
            {
                if (channel.CanPinMessages)
                {
                    var result = channel.IsMegaGroup ? MessageBox.Show(AppResources.UnpinMessageConfirmation, AppResources.Confirm,  MessageBoxButton.OKCancel) : MessageBoxResult.OK;
                    if (result == MessageBoxResult.OK)
                    {
                        IsWorking = true;
                        MTProtoService.UpdatePinnedMessageAsync(false, channel.ToInputChannel(), new TLInt(0),
                            result2 => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                if (PinnedMessage != null) PinnedMessage.Close();
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                Execute.ShowDebugMessage("channels.updatePinnedMessage error " + error);
                            }));
                    }
                }
                else
                {
                    var channel49 = channel as TLChannel49;
                    if (channel49 != null)
                    {
                        channel49.HiddenPinnedMsgId = messageBase.Id;
                    }
                    if (PinnedMessage != null) PinnedMessage.Close();
                }
            }
            else
            {
                if (channel.IsMegaGroup)
                {
                    var textBlock = new TextBlock
                    {
                        IsHitTestVisible = false,
                        Margin = new Thickness(-18.0, 0.0, 12.0, 0.0),
                        Text = AppResources.NotifyAllMembers,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var notifyAllMembers = new CheckBox { IsChecked = true, IsHitTestVisible = false };
                    notifyAllMembers.SetValue(Control.FontSizeProperty, DependencyProperty.UnsetValue);

                    var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0.0, -2.0, 0.0, -20.0), Background = new SolidColorBrush(Colors.Transparent) };
                    panel.Tap += (sender, args) =>
                    {
                        notifyAllMembers.IsChecked = !notifyAllMembers.IsChecked;
                    };
                    panel.Children.Add(notifyAllMembers);
                    panel.Children.Add(textBlock);

                    ShellViewModel.ShowCustomMessageBox(AppResources.PinMessageConfirmation, AppResources.Confirm,
                        AppResources.Ok.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                        dismissed =>
                        {
                            if (dismissed == CustomMessageBoxResult.RightButton)
                            {
                                UpdatePinnedMessage(notifyAllMembers.IsChecked.Value, messageBase, channel);
                            }
                        },
                        panel);
                }
                else
                {
                    UpdatePinnedMessage(false, messageBase, channel);
                }
            }
        }

        private void UpdatePinnedMessage(bool notifyAllMembers, TLMessageBase messageBase, TLChannel channel)
        {
            IsWorking = true;
            MTProtoService.UpdatePinnedMessageAsync(!notifyAllMembers, channel.ToInputChannel(), messageBase.Id,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var updates = result as TLUpdates;
                    if (updates != null)
                    {
                        var newChannelMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                        if (newChannelMessage != null)
                        {
                            Handle(newChannelMessage.Message as TLMessageCommon);

                            EventAggregator.Publish(new TopMessageUpdatedEventArgs(CurrentDialog, newChannelMessage.Message));
                        }
                    }

                    PinnedMessage = new PinnedMessageViewModel(messageBase);
                    PinnedMessage.OpenMessage += OnOpenMessage;
                    PinnedMessage.UnpinMessage += OnUnpinMessage;
                    PinnedMessage.Closed += OnClosePinnedMessage;
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("channels.updatePinnedMessage error " + error);
                }));
        }

        private void OnClosePinnedMessage(object sender, System.EventArgs e)
        {
            PinnedMessage = null;
        }

        private void OnUnpinMessage(object sender, System.EventArgs e)
        {
            PinMessage(PinnedMessage.Message);
        }

        private void OnOpenMessage(object sender, System.EventArgs e)
        {
            OpenMessage(Items.FirstOrDefault(), PinnedMessage.Message.Id);
        }

        private void ShowPinnedMessage(TLChannel49 channel)
        {
            if (channel == null) return;
            if (channel.PinnedMsgId == null) return;
            if (PinnedMessage != null
                && PinnedMessage.Message != null
                && PinnedMessage.Message.Id.Value == channel.PinnedMsgId.Value) return;
            if (channel.HiddenPinnedMsgId != null
                && channel.HiddenPinnedMsgId.Value == channel.PinnedMsgId.Value) return;

            if (channel.PinnedMsgId.Value <= 0)
            {
                if (PinnedMessage != null) PinnedMessage.Close();
            }
            else
            {
                var messageBase = CacheService.GetMessage(channel.PinnedMsgId, channel.Id);
                if (messageBase != null)
                {
                    var isClearHistoryMessage = IsClearHistoryMessage(messageBase);
                    if (isClearHistoryMessage)
                    {
                        if (PinnedMessage != null) PinnedMessage.Close();

                        return;
                    }

                    PinnedMessage = new PinnedMessageViewModel(messageBase);
                    PinnedMessage.OpenMessage += OnOpenMessage;
                    PinnedMessage.UnpinMessage += OnUnpinMessage;
                    PinnedMessage.Closed += OnClosePinnedMessage;
                }
                else
                {
                    MTProtoService.GetMessagesAsync(channel.ToInputChannel(), new TLVector<TLInputMessageBase> { new TLInputMessageId { Id = channel.PinnedMsgId } },
                        result => Execute.BeginOnUIThread(() =>
                        {
                            messageBase = result.Messages.FirstOrDefault(x => x.Id.Value == channel.PinnedMsgId.Value);
                            if (messageBase != null)
                            {
                                var isClearHistoryMessage = IsClearHistoryMessage(messageBase);
                                if (isClearHistoryMessage)
                                {
                                    if (PinnedMessage != null) PinnedMessage.Close();

                                    return;
                                }

                                PinnedMessage = new PinnedMessageViewModel(messageBase);
                                PinnedMessage.OpenMessage += OnOpenMessage;
                                PinnedMessage.UnpinMessage += OnUnpinMessage;
                                PinnedMessage.Closed += OnClosePinnedMessage;
                            }
                        }),
                        error =>
                        {
                            Execute.ShowDebugMessage("channels.getMessages error " + error);
                        });
                }
            }
        }

        private bool IsClearHistoryMessage(TLMessageBase messageBase)
        {
            var serviceMessage = messageBase as TLMessageService;
            if (serviceMessage != null)
            {
                var clearHistoryAction = serviceMessage.Action as TLMessageActionClearHistory;
                if (clearHistoryAction != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
