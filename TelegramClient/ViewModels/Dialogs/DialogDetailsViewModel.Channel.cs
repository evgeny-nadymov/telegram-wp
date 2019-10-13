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
using System.Windows.Media;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using TelegramClient.Converters;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public bool IsChannel
        {
            get { return With is TLChannel; }
        }

        public bool IsMegaGroup
        {
            get
            {
                var channel = With as TLChannel;
                return channel != null && channel.IsMegaGroup;
            }
        }

        private bool _isChannelMessage;

        public bool IsChannelMessage
        {
            get { return _isChannelMessage; }
            set
            {
                if (_isChannelMessage != value)
                {
                    _isChannelMessage = value;
                    NotifyOfPropertyChange(() => ChannelMessageBrush);
                }
            }
        }

        public Visibility ChannelVisibility
        {
            get { return IsChannel ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Brush ChannelMessageBrush
        {
            get
            {
                var brush = IsChannelMessage
                    ? (Brush)Application.Current.Resources["PhoneAccentBrush"]
                    : (Brush)Application.Current.Resources["PhoneChromeBrush"];
                return brush;
            }
        }

        private void CheckChannelMessage(TLMessage25 message)
        {
            var channel = With as TLChannel;
            if (IsChannelMessage || (channel != null && channel.IsBroadcast))
            {
                var message36 = message as TLMessage36;
                if (message36 != null)
                {
                    if (channel != null && !channel.Signatures)
                    {
                        message36.FromId = new TLInt(-1);
                    }
                    message36.Views = new TLInt(1);
                }
            }

            var channel44 = channel as TLChannel44;
            if (channel44 != null && channel44.IsBroadcast && channel44.Silent)
            {
                var message48 = message as TLMessage48;
                if (message48 != null && message48.Out.Value)
                {
                    message48.Silent = true;
                }
            }
        }

        public void OpenServiceMessage(TLMessageBase message)
        {
            var serviceMessage = message as TLMessageService17;
            if (serviceMessage != null)
            {
                var messageGroupAction = serviceMessage.Action as TLMessageActionMessageGroup;
                if (messageGroupAction != null)
                {
                    var channel = With as TLChannel;
                    if (channel == null) return;

                    var collapsed = false;
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var message40 = Items[i] as TLMessage40;
                        if (message40 != null && message40.Group == message)
                        {
                            Items.RemoveAt(i--);
                            collapsed = true;
                        }
                    }
                    if (collapsed)
                    {
                        //RaiseScrollTo(new ScrollToEventArgs(message));
                        return;
                    }

                    var participantIds = channel.ParticipantIds;
                    if (participantIds.Count == 0) return;

                    var participant = participantIds[0];

                    var index = Items.IndexOf(message);
                    var comments = new List<TLMessageCommon>();
                    var count = messageGroupAction.Group.Count.Value;
                    for(var i = 0; i < count; i++)
                    {
                        var comment = GetMessage(new TLString(i.ToString(CultureInfo.InvariantCulture)), new TLMessageMediaEmpty());
                        comment.FromId = participant;
                        comment.Out = TLBool.False;
                        var comment40 = comment as TLMessage40;
                        if (comment40 != null)
                        {
                            comment40.Group = message;
                        }
                        comments.Add(comment);
                    }

                    foreach (var comment in comments)
                    {
                        Items.Insert(index, comment);
                    }
                }
            }
        }

        public void AddComments()
        {
            var broadcast = With as TLBroadcastChat;
            if (broadcast == null) return;

            var broadcastPeer = new TLPeerBroadcast { Id = broadcast.Id };

            var count = new Random().Next(1, 5);

            var group = new TLMessageGroup
            {
                Count = new TLInt(count),
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                MaxId = new TLInt(int.MaxValue),
                MinId = new TLInt(0)
            };

            var action = new TLMessageActionMessageGroup
            {
                Group = group
            };

            var serviceMessage = new TLMessageService17
            {
                FromId = new TLInt(StateService.CurrentUserId),
                ToId = broadcastPeer,
                Status = MessageStatus.Confirmed,
                Out = new TLBool { Value = true },
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                RandomId = TLLong.Random(),
                Action = action
            };
            serviceMessage.SetUnread(TLBool.False);

            Items.Insert(0, serviceMessage);
            CacheService.SyncMessage(serviceMessage,
                message =>
                {
                    
                });
        }

        private void MergeGroupMessages(IList<TLMessageBase> messages)
        {
            var itemsToUpdate = new Dictionary<TLMessageBase, TLMessageBase>();
            foreach (var message in messages)
            {
                var message40 = message as TLMessage40;
                if (message40 != null)
                {
                    var group = message40.Group;
                    if (group != null)
                    {
                        for (var i = 0; i < Items.Count; i++)
                        {
                            if (group == Items[i])
                            {
                                var serviceMessage = Items[i] as TLMessageService17;
                                if (serviceMessage != null)
                                {
                                    var action = serviceMessage.Action as TLMessageActionMessageGroup;
                                    if (action != null)
                                    {
                                        action.Group.Count = new TLInt(action.Group.Count.Value-1);
                                        if (action.Group.Count.Value == 0)
                                        {
                                            Items.RemoveAt(i);
                                            CacheService.DeleteMessages(new TLVector<TLLong>{serviceMessage.RandomId});
                                        }
                                        itemsToUpdate[serviceMessage] = serviceMessage;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            foreach (var message in itemsToUpdate.Values)
            {
                message.NotifyOfPropertyChange(() => message.Self);
            }


            for (int i = 0; i < Items.Count; i++)
            {
                var serviceMessage1 = Items[i] as TLMessageService17;
                if (serviceMessage1 != null)
                {
                    var action1 = serviceMessage1.Action as TLMessageActionMessageGroup;
                    if (action1 != null)
                    {

                        if (Items.Count > i + 1)
                        {
                            var serviceMessage2 = Items[i + 1] as TLMessageService17;
                            if (serviceMessage2 != null)
                            {
                                var action2 = serviceMessage2.Action as TLMessageActionMessageGroup;
                                if (action2 != null)
                                {
                                    Items.RemoveAt(i);
                                    Items.RemoveAt(i);
                                    CacheService.DeleteMessages(new TLVector<TLLong>{serviceMessage2.RandomId});
                                    action1.Group.Count = new TLInt(action1.Group.Count.Value + action2.Group.Count.Value);
                                    action1.Group.MinId = new TLInt(Math.Min(action1.Group.MinId.Value, action2.Group.MinId.Value));
                                    action1.Group.MinId = new TLInt(Math.Min(action1.Group.MaxId.Value, action2.Group.MaxId.Value));

                                    Items.Insert(i, serviceMessage1);
                                    serviceMessage1.NotifyOfPropertyChange(() => serviceMessage1.Self);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
