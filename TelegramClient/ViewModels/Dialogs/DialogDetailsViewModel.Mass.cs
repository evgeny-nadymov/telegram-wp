// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;
using TelegramClient.Views.Additional;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        public bool IsMassDeleteReportSpamEnabled(TLChannel channel, IList<TLMessageBase> randomItems, IList<TLMessageBase> items, out TLUserBase from)
        {
            from = null;

            if (!channel.Creator && !channel.IsEditor) return false;

            if (randomItems != null && randomItems.Count > 0) return false;

            var onlyServiceMessages = true;
            TLInt fromId = null;
            foreach (var messageBase in items)
            {
                var messageService = messageBase as TLMessageService;
                if (messageService == null)
                {
                    onlyServiceMessages = false;
                }
                var message = messageBase as TLMessageCommon;
                if (message != null)
                {
                    if (message.Out.Value) return false;
                    if (message.FromId == null) return false;
                    if (fromId != null && message.FromId.Value != fromId.Value) return false;

                    if (fromId == null)
                    {
                        fromId = message.FromId;
                    }
                }
            }

            if (onlyServiceMessages) return false;

            var user = CacheService.GetUser(fromId);
            if (user == null) return false;

            from = user;

            return true;
        }

        private TLMessageBase _localLastItem;

        private List<TLMessageBase> _localItems;

        private List<TLMessageBase> _localRandomItems;

        public void DeleteReportSpam(TLUserBase from, TLMessageBase lastItem, IList<TLMessageBase> randomItems, IList<TLMessageBase> items)
        {
            var channel = With as TLChannel;
            if (channel == null) return;

            _localLastItem = lastItem;
            _localItems = new List<TLMessageBase>(items ?? new List<TLMessageBase>());
            _localRandomItems = new List<TLMessageBase>(randomItems ?? new List<TLMessageBase>());

            var viewModel = new MassDeleteReportSpamViewModel(from);
            var view = new MassDeleteReportSpamView{ DataContext = viewModel };

            ShellViewModel.ShowCustomMessageBox(null, AppResources.Confirm,
                AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        if (viewModel.DeleteAllMessages)
                        {
                            DeleteUserHistoryAsync(channel, from.ToInputUser(),
                                result =>
                                {
                                    CacheService.DeleteUserHistory(new TLPeerChannel { Id = channel.Id }, new TLPeerUser { Id = from.Id });

                                    Execute.BeginOnUIThread(() =>
                                    {

                                        for (var i = 0; i < Items.Count; i++)
                                        {
                                            var messageCommon = Items[i] as TLMessageCommon;
                                            if (messageCommon != null
                                                && messageCommon.ToId is TLPeerChannel
                                                && messageCommon.FromId.Value == from.Index)
                                            {
                                                if (messageCommon.Index == 1)
                                                {
                                                    var message = messageCommon as TLMessageService;
                                                    if (message != null)
                                                    {
                                                        var channelMigrateFrom = message.Action as TLMessageActionChannelMigrateFrom;
                                                        if (channelMigrateFrom != null)
                                                        {
                                                            continue;
                                                        }
                                                    }
                                                }

                                                Items.RemoveAt(i--);
                                            }
                                        }

                                        if (Items.Count < 10)
                                        {
                                            var messages = GetHistory();
                                            ProcessMessages(messages);
                                            if (messages.Count > 0)
                                            {
                                                Items.Clear();
                                                foreach (var item in messages)
                                                {
                                                    Items.Add(item);
                                                }

                                                IsEmptyDialog = Items.Count == 0 && (_messages == null || _messages.Count == 0) && LazyItems.Count == 0;
                                            }
                                            else
                                            {
                                                _isLastMigratedHistorySliceLoaded = false;
                                                UpdateItemsAsync(0, 0, Constants.MessagesSlice, false);
                                            }
                                        }
                                    });
                                },
                                error =>
                                {
                                    Telegram.Api.Helpers.Execute.ShowDebugMessage("channels.deleteUserHistory error " + error);
                                });
                        }
                        else if (viewModel.DeleteMessages)
                        {
                            DeleteChannelMessages(MTProtoService, channel, _localLastItem, _localRandomItems, _localItems, DeleteMessagesInternal, DeleteMessagesInternal);
                        }
                        if (viewModel.BanUser)
                        {
                            MTProtoService.KickFromChannelAsync(channel, from.ToInputUser(), TLBool.True,
                                result =>
                                {

                                    var updates = result as TLUpdates;
                                    if (updates != null)
                                    {
                                        var updateNewMessage = updates.Updates.FirstOrDefault(x => x is TLUpdateNewChannelMessage) as TLUpdateNewChannelMessage;
                                        if (updateNewMessage != null)
                                        {
                                            EventAggregator.Publish(updateNewMessage.Message);
                                        }
                                    }

                                    Subtitle = GetSubtitle();
                                },
                                error =>
                                {
                                    Telegram.Api.Helpers.Execute.ShowDebugMessage("channels.kickFromChannel error " + error);
                                });
                        }
                        if (viewModel.ReportSpam)
                        {
                            MTProtoService.ReportSpamAsync(channel.ToInputChannel(), from.Id, new TLVector<TLInt>(_localItems.Select(x => x.Id).ToList()),
                                result =>
                                {

                                },
                                error =>
                                {
                                    Telegram.Api.Helpers.Execute.ShowDebugMessage("channels.reportSpam error " + error);
                                });
                        }
                    }
                },
                view);
        }

        private void DeleteUserHistoryAsync(TLChannel channel, TLInputUserBase userId, Action<TLAffectedHistory> callback, Action<TLRPCError> faultCallback = null)
        {
            MTProtoService.DeleteUserHistoryAsync(channel, userId,
                result => 
                {
                    if (result.Offset.Value > 0)
                    {
                        DeleteUserHistoryAsync(channel, userId, callback, faultCallback);
                    }
                    else
                    {
                        callback.SafeInvoke(result);
                    }
                },
                error =>
                {
                    Telegram.Api.Helpers.Execute.ShowDebugMessage("channels.deleteUserHistory error " + error);
                });
        }
    }
}
