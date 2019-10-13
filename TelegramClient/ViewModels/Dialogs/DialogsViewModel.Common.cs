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
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogsViewModel
    {
        public void Handle(TLUpdateContactLinkBase update)
        {
            var updateContactLink24 = update as TLUpdateContactLink24;
            if (updateContactLink24 != null && updateContactLink24.MyLink is TLContactLinkNone)
            {
                Execute.BeginOnUIThread(() =>
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var userBase = Items[i].With as TLUserBase;
                        if (userBase != null
                            && userBase.Index == update.UserId.Value)
                        {
                            Items[i].NotifyOfPropertyChange(() => Items[i].With);
                        }
                    }
                });
            }
        }

        public void Handle(TLUpdateServiceNotification serviceNotification)
        {
            if (serviceNotification.Popup.Value)
            {
                Execute.BeginOnUIThread(() => MessageBox.Show(serviceNotification.Message.ToString(), AppResources.AppName, MessageBoxButton.OK));
            }
            else
            {
                var fromId = new TLInt(Constants.TelegramNotificationsId);
                var telegramUser = CacheService.GetUser(fromId);
                if (telegramUser == null)
                {
                    return;
                }

                var message = GetServiceMessage(fromId, serviceNotification.Message, serviceNotification.Media);

                CacheService.SyncMessage(message, m => { });
            }
        }

        public void Handle(TLUpdateNewAuthorization newAuthorization)
        {
            var user = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
            if (user == null)
            {
                return;
            }

            var telegramUser = CacheService.GetUser(new TLInt(Constants.TelegramNotificationsId));
            if (telegramUser == null)
            {
                return;
            }

            var firstName = user.FirstName;
            var date = TLUtils.ToDateTime(newAuthorization.Date);

            var text = string.Format(AppResources.NewAuthorization,
                firstName,
                date.ToString("dddd"),
                date.ToString("M"),
                date.ToString("t"),
                newAuthorization.Device,
                newAuthorization.Location);

            var fromId = new TLInt(Constants.TelegramNotificationsId);

            var message = GetServiceMessage(fromId, new TLString(text), new TLMessageMediaEmpty(), newAuthorization.Date);

            CacheService.SyncMessage(message, m => { });
        }

        public void ChangeUnreadMark(TLDialogBase dialogBase)
        {
            var dialog71 = dialogBase as TLDialog71;
            if (dialog71 == null) return;

            if (dialog71.UnreadMark
                || dialog71.UnreadMentionsCount != null && dialog71.UnreadMentionsCount.Value > 0
                || dialog71.UnreadCount != null && dialog71.UnreadCount.Value > 0)
            {
                if (dialog71.UnreadMark)
                {
                    MTProtoService.MarkDialogUnreadAsync(false, new TLInputDialogPeer { Peer = MTProtoService.PeerToInputPeer(dialog71.Peer)}, 
                        result => BeginOnUIThread(() =>
                        {
                            dialog71.UnreadMark = !dialog71.UnreadMark;
                            dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMark);
                        }),
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.markDialogUnread error " + error);
                        });
                }

                var channel = dialog71.With as TLChannel;
                if (dialog71.UnreadCount != null && dialog71.UnreadCount.Value > 0)
                {
                    if (channel != null)
                    {
                        MTProtoService.ReadHistoryAsync(channel, new TLInt(int.MaxValue),
                            result => BeginOnUIThread(() =>
                            {
                                dialog71.UnreadCount = new TLInt(0);
                                dialog71.NotifyOfPropertyChange(() => dialog71.UnreadCount);
                            }),
                            error =>
                            {
                                Execute.ShowDebugMessage("channels.readHistory error " + error);
                            });
                    }
                    else
                    {
                        MTProtoService.ReadHistoryAsync(MTProtoService.PeerToInputPeer(dialog71.Peer),
                            new TLInt(int.MaxValue), new TLInt(0),
                            result => BeginOnUIThread(() =>
                            {
                                dialog71.UnreadCount = new TLInt(0);
                                dialog71.NotifyOfPropertyChange(() => dialog71.UnreadCount);
                            }),
                            error =>
                            {
                                Execute.ShowDebugMessage("messages.readHistory error " + error);
                            });
                    }
                }
                if (dialog71.UnreadMentionsCount != null && dialog71.UnreadMentionsCount.Value > 0)
                {
                    MTProtoService.ReadMentionsAsync(MTProtoService.PeerToInputPeer(dialog71.Peer),
                        result => BeginOnUIThread(() =>
                        {
                            dialog71.UnreadMentionsCount = new TLInt(0);
                            dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMentionsCount);
                        }),
                        error =>
                        {
                            Execute.ShowDebugMessage("messages.readMentions error " + error);
                        });
                }
            }
            else
            {
                MTProtoService.MarkDialogUnreadAsync(true, new TLInputDialogPeer { Peer = MTProtoService.PeerToInputPeer(dialog71.Peer) },
                    result => BeginOnUIThread(() =>
                    {
                        dialog71.UnreadMark = !dialog71.UnreadMark;
                        dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMark);
                    }),
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.markDialogUnread error " + error);
                    });
            }
        }

        private TLMessageBase GetServiceMessage(TLInt fromId, TLString text, TLMessageMediaBase media, TLInt date = null)
        {
            var message = TLUtils.GetMessage(
                    fromId,
                    new TLPeerUser { Id = new TLInt(StateService.CurrentUserId) },
                    MessageStatus.Confirmed,
                    TLBool.False,
                    TLBool.True,
                    date ?? TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                    text,
                    media,
                    TLLong.Random(),
                    new TLInt(0)
                );
            message.Id = new TLInt(0);

            return message;
        }

        private int _offset = 0;

        public void LoadNextSlice()
        {
            if (IsWorking
                || LazyItems.Count > 0
                || IsLastSliceLoaded
#if WP8
 || !_isUpdated
#endif
)
            {
                return;
            }

            var offset = _offset;
            var limit = Constants.DialogsSlice;
            //TLUtils.WriteLine(string.Format("{0} messages.getDialogs offset={1} limit={2}", DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture), offset, limit), LogSeverity.Error);

            var offsetDate = 0;
            var offsetId = 0;
            TLInputPeerBase offsetPeer = new TLInputPeerEmpty();
            var lastDialog = GetLastDialog(Items);
            if (lastDialog != null)
            {
                var lastMessage = lastDialog.TopMessage as TLMessageCommon;
                if (lastMessage != null)
                {
                    offsetDate = lastMessage.DateIndex;
                    offsetId = lastMessage.Index;
                    if (lastMessage.ToId is TLPeerUser)
                    {
                        offsetPeer = !lastMessage.Out.Value
                            ? DialogDetailsViewModel.PeerToInputPeer(new TLPeerUser { Id = lastMessage.FromId })
                            : DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                    }
                    else
                    {
                        offsetPeer = DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                    }
                }
            }

            IsWorking = true;
            //TLObject.LogNotify = true;
            //TelegramEventAggregator.LogPublish = true;
            var stopwatch = Stopwatch.StartNew();
            MTProtoService.GetDialogsAsync(stopwatch,
                new TLInt(offsetDate), 
                new TLInt(offsetId), 
                offsetPeer,
                new TLInt(limit),
                new TLInt(0),
                result =>
                {
                    //System.Diagnostics.Debug.WriteLine("messages.getDialogs end sync elapsed=" + stopwatch.Elapsed);

                    BeginOnUIThread(() =>
                    {
                        //System.Diagnostics.Debug.WriteLine("messages.getDialogs ui elapsed=" + stopwatch.Elapsed);
                        //TelegramEventAggregator.LogPublish = false;
                        //TLObject.LogNotify = false;
                        if (_offset != offset)
                        {
                            return;
                        }
                        _offset += Constants.DialogsSlice;

                        foreach (var dialog in result.Dialogs)
                        {
                            Items.Add(dialog);
                        }

                        ReorderDrafts(Items);

                        IsWorking = false;
                        IsLastSliceLoaded = result.Dialogs.Count < limit;
                        Status = LazyItems.Count > 0 || Items.Count > 0 ? string.Empty : Status;
                        //System.Diagnostics.Debug.WriteLine("messages.getDialogs end ui elapsed=" + stopwatch.Elapsed);
                        //TLUtils.WriteLine(string.Format("messages.getDialogs offset={0} limit={1} result={2}", offset, limit, result.Dialogs.Count), LogSeverity.Error);
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Status = string.Empty;
                    //TLUtils.WriteLine(string.Format("messages.getDialogs offset={0} limit={1} error={2}", offset, limit, error), LogSeverity.Error);
                    Execute.ShowDebugMessage("messages.getDialogs error " + error);
                }));
        }

        private static TLDialog GetLastDialog(IList<TLDialogBase> dialogs)
        {
            TLDialog lastDialog = null;
            var minDate = int.MaxValue;
            for (var i = 0; i < dialogs.Count; i++)
            {
                var dialog = dialogs[i] as TLDialog;
                if (dialog != null)
                {
                    var topMessageIndex = dialog.TopMessage != null ? dialog.TopMessage.Index : 0;
                    var dateIndex = dialog.GetDateIndex();
                    if (topMessageIndex > 0 && dateIndex < minDate)
                    {
                        minDate = dateIndex;
                        lastDialog = dialog;
                    }
                }
            }

            return lastDialog;
        }

        public bool OpenDialogDetails(TLDialogBase dialog)
        {
            ShellViewModel.StartNewTimer();
            //Execute.ShowDebugMessage("OpenDialogDetails");

            if (dialog == null)
            {
                Execute.ShowDebugMessage("OpenDialogDetails dialog=null");
                return false;
            }
            if (dialog.With == null)
            {
                Execute.ShowDebugMessage("OpenDialogDetails dialog.With=null");
                return false;
            }

            if (dialog.IsEncryptedChat)
            {
                var encryptedChat = CacheService.GetEncryptedChat(dialog.Peer.Id);

                var user = dialog.With as TLUserBase;
                if (user == null)
                {
                    Execute.ShowDebugMessage("OpenDialogDetails encrypted dialog.With=null");
                    return false;
                }

                var cachedUser = CacheService.GetUser(user.Id);
                StateService.Participant = cachedUser ?? user;
                StateService.With = encryptedChat;
                StateService.Dialog = dialog;
                StateService.AnimateTitle = true;
                NavigationService.UriFor<SecretDialogDetailsViewModel>().Navigate();
            }
            else
            {
                var settings = dialog.With as INotifySettings;
                if (settings != null)
                {
                    settings.NotifySettings = settings.NotifySettings ?? dialog.NotifySettings;
                }

                var currentBackground = IoC.Get<IStateService>().CurrentBackground;
                StateService.With = dialog.With;
                StateService.Dialog = dialog;
                StateService.AnimateTitle = true;
                NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
            }

            return true;
        }

        public Func<TLDialogBase, bool> FastCallback;

        public bool OpenFastDialogDetails(TLDialogBase dialog)
        {
            return FastCallback(dialog);

            //Execute.ShowDebugMessage("OpenDialogDetails");

        }

        public void DeleteAndStop(TLDialogBase dialog)
        {
            if (dialog == null) return;

            var user = dialog.With as TLUser;
            if (user == null || !user.IsBot) return;

            var confirmation = MessageBox.Show(AppResources.DeleteChatConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            IsWorking = true;
            MTProtoService.BlockAsync(user.ToInputUser(),
                blocked =>
                {
                    user.Blocked = TLBool.True;
                    CacheService.Commit();

                    DeleteHistoryAsync(false, user.ToInputPeer(),
                        result => BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            CacheService.DeleteDialog(dialog); // TODO : move this line to MTProtoService

                            if (dialog.With != null)
                            {
                                dialog.With.Bitmap = null;
                            }
                            Items.Remove(dialog);
                        }),
                        error => BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            Execute.ShowDebugMessage("messages.deleteHistory error " + error);
                        }));
                },
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("contacts.Block error " + error);
                }));
        }

        public void DeleteAndExit(TLDialogBase dialog)
        {
            if (dialog == null) return;
            if (dialog.Peer is TLPeerUser) return;

            var message = dialog.Peer is TLPeerEncryptedChat
                ? AppResources.DeleteChatConfirmation
                : AppResources.DeleteAndExitConfirmation;
            var result = MessageBox.Show(message, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;

            if (dialog.Peer is TLPeerBroadcast)
            {
                CacheService.DeleteDialog(dialog);
                UnpinFromStart(dialog);
                BeginOnUIThread(() => Items.Remove(dialog));

                return;
            }

            if (dialog.Peer is TLPeerEncryptedChat)
            {
                var encryptedChat = CacheService.GetEncryptedChat(dialog.Peer.Id);
                if (encryptedChat is TLEncryptedChatDiscarded)
                {
                    CacheService.DeleteDialog(dialog);
                    UnpinFromStart(dialog);
                    BeginOnUIThread(() => Items.Remove(dialog));
                }
                else
                {
                    IsWorking = true;
                    MTProtoService.DiscardEncryptionAsync(dialog.Peer.Id,
                    r =>
                    {
                        IsWorking = false;
                        CacheService.DeleteDialog(dialog);
                        UnpinFromStart(dialog);
                        BeginOnUIThread(() => Items.Remove(dialog));
                    },
                    error =>
                    {
                        IsWorking = false;
                        if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                            && error.Message.Value == "ENCRYPTION_ALREADY_DECLINED")
                        {
                            CacheService.DeleteDialog(dialog);
                            UnpinFromStart(dialog);
                            BeginOnUIThread(() => Items.Remove(dialog));
                        }

                        Execute.ShowDebugMessage("messages.discardEncryption error " + error);
                    });
                }

                return;
            }

            if (dialog.Peer is TLPeerChat)
            {
                DeleteAndExitDialogCommon(
                dialog.With as TLChatBase,
                MTProtoService,
                () =>
                {
                    CacheService.DeleteDialog(dialog);
                    UnpinFromStart(dialog);

                    BeginOnUIThread(() => Items.Remove(dialog));
                },
                error =>
                {
                    Execute.ShowDebugMessage("DeleteAndExitDialogCommon error " + error);
                });
                return;
            }
        }

        public static void DeleteAndExitDialogCommon(TLChatBase chatBase, IMTProtoService mtProtoService, System.Action callback, Action<TLRPCError> faultCallback = null)
        {
            if (chatBase == null) return;

            var inputPeer = chatBase.ToInputPeer();

            if (chatBase is TLChatForbidden)
            {
                DeleteHistoryAsync(
                    mtProtoService,
                    false,
                    inputPeer, new TLInt(0),
                    affectedHistory => callback.SafeInvoke(),
                    faultCallback.SafeInvoke);
            }
            else
            {
                var chat = chatBase as TLChat;
                var chat41 = chatBase as TLChat41;
                if (chat != null)
                {
                    if (chat.Left.Value || (chat41 != null && chat41.IsMigrated))
                    {
                        DeleteHistoryAsync(
                            mtProtoService,
                            false,
                            inputPeer, new TLInt(0),
                            affectedHistory => callback.SafeInvoke(),
                            faultCallback.SafeInvoke);
                    }
                    else
                    {
                        mtProtoService.DeleteChatUserAsync(
                            chat.Id, new TLInputUserSelf(),
                            statedMessage =>
                                DeleteHistoryAsync(
                                    mtProtoService,
                                    false,
                                    inputPeer, new TLInt(0),
                                    affectedHistory => callback.SafeInvoke(),
                                    faultCallback.SafeInvoke),
                            faultCallback.SafeInvoke);
                    }
                }
            }
        }

        public static void DeleteDialogCommon(TLUserBase userBase, IMTProtoService mtProtoService, System.Action callback, Action<TLRPCError> faultCallback = null)
        {
            if (userBase == null) return;

            var inputPeer = userBase.ToInputPeer();

            DeleteHistoryAsync(mtProtoService, false, inputPeer, new TLInt(0),
                result => callback.SafeInvoke(),
                faultCallback.SafeInvoke);
        }

        public void ClearHistory(TLDialogBase dialog)
        {
            var confirmation = MessageBox.Show(AppResources.ClearHistoryConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            if (dialog.Peer is TLPeerChannel)
            {
                var channel = (TLChannel)dialog.With;

                MTProtoService.DeleteHistoryAsync(channel.ToInputChannel(),
                    result =>
                    {
                        CacheService.ClearDialog(dialog.Peer);
                        BeginOnUIThread(() =>
                        {
                            if (dialog.With != null)
                            {
                                dialog.With.ClearBitmap();
                            }

                            dialog.NotifyOfPropertyChange(() => dialog.UnreadCount);
                            var dialog71 = dialog as TLDialog71;
                            if (dialog71 != null)
                            {
                                dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMentionsCount);
                            }
                        });
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("channels.deleteHistory error " + error);
                    });
            }
            else if (dialog.Peer is TLPeerUser)
            {
                var user = (TLUserBase)dialog.With;
                var inputPeer = user.ToInputPeer();

                DeleteHistoryAsync(true, inputPeer,
                    result =>
                    {
                        CacheService.ClearDialog(dialog.Peer);
                        BeginOnUIThread(() =>
                        {
                            if (dialog.With != null)
                            {
                                dialog.With.ClearBitmap();
                            }

                            dialog.NotifyOfPropertyChange(() => dialog.UnreadCount);
                            var dialog71 = dialog as TLDialog71;
                            if (dialog71 != null)
                            {
                                dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMentionsCount);
                            }
                        });
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.deleteHistory error " + error);
                    });
            }
            else if (dialog.Peer is TLPeerChat)
            {
                var chat = (TLChatBase)dialog.With;
                var inputPeer = chat.ToInputPeer();

                DeleteHistoryAsync(true, inputPeer,
                    result =>
                    {
                        CacheService.ClearDialog(dialog.Peer);
                        BeginOnUIThread(() =>
                        {
                            if (dialog.With != null)
                            {
                                dialog.With.ClearBitmap();
                            }
                            dialog.NotifyOfPropertyChange(() => dialog.UnreadCount);
                            var dialog71 = dialog as TLDialog71;
                            if (dialog71 != null)
                            {
                                dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMentionsCount);
                            }
                        });
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.deleteHistory error " + error);
                    });
            }
            else if (dialog.Peer is TLPeerEncryptedChat)
            {
                var chat = CacheService.GetEncryptedChat(dialog.Peer.Id) as TLEncryptedChat;
                if (chat == null) return;

                var flushHistoryAction = new TLDecryptedMessageActionFlushHistory();

                var decryptedTuple = SecretDialogDetailsViewModel.GetDecryptedServiceMessageAndObject(flushHistoryAction, chat, MTProtoService.CurrentUserId, CacheService);

                SecretDialogDetailsViewModel.SendEncryptedService(chat, decryptedTuple.Item2, MTProtoService,
                    CacheService,
                    sentEncryptedMessage =>
                    {
                        CacheService.ClearDecryptedHistoryAsync(chat.Id);
                        BeginOnUIThread(() =>
                        {
                            if (dialog.With != null)
                            {
                                dialog.With.ClearBitmap();
                            }

                            dialog.NotifyOfPropertyChange(() => dialog.UnreadCount);
                        });
                    });
            }
            else if (dialog.Peer is TLPeerBroadcast)
            {
                var broadcast = CacheService.GetBroadcast(dialog.Peer.Id);
                if (broadcast == null) return;

                CacheService.ClearBroadcastHistoryAsync(broadcast.Id);
            }
        }

        public void DeleteDialog(TLDialog dialog)
        {
            MessageBoxResult confirmation;

            if (dialog == null) return;

            var channel = dialog.With as TLChannel;
            var channelForbidden = dialog.With as TLChannelForbidden;
            if (channel != null || channelForbidden != null)
            {
                if (channelForbidden != null)
                {
                    CacheService.DeleteDialog(dialog);
                    UnpinFromStart(dialog);
                    EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                }

                if (channel != null)
                {
                    /*if (channel.Creator)
                    {
                        var confirmationString = channel.IsMegaGroup
                            ? AppResources.DeleteGroupConfirmation
                            : AppResources.DeleteChannelConfirmation;

                        confirmation = MessageBox.Show(confirmationString, AppResources.Confirm, MessageBoxButton.OKCancel);
                        if (confirmation != MessageBoxResult.OK) return;

                        IsWorking = true;
                        MTProtoService.DeleteChannelAsync(channel,
                            result => BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                CacheService.DeleteDialog(dialog);
                                UnpinFromStart(dialog);
                                EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                            }),
                            error => BeginOnUIThread(() =>
                            {
                                Execute.ShowDebugMessage("channels.deleteChannel error " + error);

                                IsWorking = false;

                                if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                                    && error.TypeEquals(ErrorType.CHANNEL_PRIVATE))
                                {
                                    CacheService.DeleteDialog(dialog);
                                    UnpinFromStart(dialog);
                                    EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                                }
                            }));
                    }
                    else*/
                    {
                        var confirmationString = channel.IsMegaGroup
                            ? AppResources.LeaveGroupConfirmation
                            : AppResources.LeaveChannelConfirmation;

                        confirmation = MessageBox.Show(confirmationString, AppResources.Confirm, MessageBoxButton.OKCancel);
                        if (confirmation != MessageBoxResult.OK) return;

                        IsWorking = true;
                        MTProtoService.LeaveChannelAsync(
                            channel,
                            result => BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                CacheService.DeleteDialog(dialog);
                                UnpinFromStart(dialog);
                                EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                if (error.TypeEquals(ErrorType.CHANNEL_PRIVATE)
                                    || error.TypeEquals(ErrorType.USER_NOT_PARTICIPANT))
                                {
                                    CacheService.DeleteDialog(dialog);
                                    UnpinFromStart(dialog);
                                    EventAggregator.Publish(new DialogRemovedEventArgs(dialog));
                                }

                                IsWorking = false;
                                Execute.ShowDebugMessage("cnannels.leaveChannel error " + error);
                            }));

                        return;
                    }
                }

                return;
            }


            if (dialog.With is TLChat) return;


            confirmation = MessageBox.Show(AppResources.DeleteChatConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            var user = (IInputPeer)dialog.With;
            if (user == null)
            {
                CacheService.DeleteDialog(dialog);
                Items.Remove(dialog);
                Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.NoDialogsHere : string.Empty;
                return;
            }

            var inputPeer = user.ToInputPeer();

            DeleteHistoryAsync(false, inputPeer,
                result =>
                {
                    CacheService.DeleteDialog(dialog); // TODO : move this line to MTProtoService
                    BeginOnUIThread(() =>
                    {
                        if (dialog.With != null)
                        {
                            dialog.With.Bitmap = null;
                        }
                        Items.Remove(dialog);
                        Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.NoDialogsHere : string.Empty;
                    });
                },
                error =>
                {
                    Execute.ShowDebugMessage("messages.deleteHistory error " + error);
                });
        }

        private void DeleteHistoryAsync(bool justClear, TLInputPeerBase peer, Action<TLAffectedHistory> callback, Action<TLRPCError> faultCallback = null)
        {
            DeleteHistoryAsync(MTProtoService, justClear, peer, new TLInt(0), callback, faultCallback);
        }

        private static void DeleteHistoryAsync(IMTProtoService mtProtoService, bool justClear, TLInputPeerBase peer, TLInt offset, Action<TLAffectedHistory> callback, Action<TLRPCError> faultCallback = null)
        {
            mtProtoService.DeleteHistoryAsync(justClear, peer, offset,
                affectedHistory =>
                {
                    if (affectedHistory.Offset.Value > 0)
                    {
                        DeleteHistoryAsync(mtProtoService, justClear, peer, affectedHistory.Offset, callback, faultCallback);
                    }
                    else
                    {
                        callback.SafeInvoke(affectedHistory);
                    }
                },
                faultCallback.SafeInvoke);
        }

        public void CreateDialog()
        {
            NavigationService.UriFor<ChooseParticipantsViewModel>().Navigate();
        }

        public void Search()
        {
            StateService.LoadedDialogs = new List<TLDialogBase>(Items);
            NavigationService.UriFor<SearchViewModel>().Navigate();
            //NavigationService.UriFor<SearchShellViewModel>().Navigate();
        }

        public void Handle(TLUpdateNotifySettings notifySettings)
        {
            var notifyPeer = notifySettings.Peer as TLNotifyPeer;
            if (notifyPeer != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var dialog = Items[i] as TLDialog;
                        if (dialog != null
                            && dialog.Peer != null
                            && dialog.Peer.Id.Value == notifyPeer.Peer.Id.Value
                            && dialog.Peer.GetType() == notifyPeer.Peer.GetType())
                        {
                            dialog.NotifyOfPropertyChange(() => dialog.NotifySettings);
                            dialog.NotifyOfPropertyChange(() => dialog.Self);
                            break;
                        }
                    }
                });
            }

            var notifyUsers = notifySettings.Peer as TLNotifyUsers;
            if (notifyUsers != null)
            {
                var peerNotifySettings = notifySettings.NotifySettings as TLPeerNotifySettings;
                if (peerNotifySettings != null)
                {
                    var settings = StateService.GetNotifySettings();
                    settings.ContactAlert = peerNotifySettings.MuteUntil == null || peerNotifySettings.MuteUntil.Value == 0;
                    settings.ContactMessagePreview = peerNotifySettings.ShowPreviews != null && peerNotifySettings.ShowPreviews.Value;
                    settings.ContactSound = peerNotifySettings.Sound != null ? peerNotifySettings.Sound.ToString() : "default";
                    StateService.SaveNotifySettingsAsync(settings);
                }
            }

            var notifyChats = notifySettings.Peer as TLNotifyChats;
            if (notifyChats != null)
            {
                var peerNotifySettings = notifySettings.NotifySettings as TLPeerNotifySettings;
                if (peerNotifySettings != null)
                {
                    var settings = StateService.GetNotifySettings();
                    settings.GroupAlert = peerNotifySettings.MuteUntil == null || peerNotifySettings.MuteUntil.Value == 0;
                    settings.GroupMessagePreview = peerNotifySettings.ShowPreviews != null && peerNotifySettings.ShowPreviews.Value;
                    settings.GroupSound = peerNotifySettings.Sound != null ? peerNotifySettings.Sound.ToString() : "default";
                    StateService.SaveNotifySettingsAsync(settings);
                }
            }
        }

        #region Tiles

        public static string GetTileNavigationParam(TLDialogBase dialog)
        {
            var user = dialog.With as TLUserBase;
            var chat = dialog.With as TLChatBase;
            var channel = dialog.With as TLChannel;
            var broadcast = dialog.With as TLBroadcastChat;
            if (user != null)
            {
                if (dialog is TLEncryptedDialog)
                {
                    return "Action=SecondaryTile&encrypteduser_id=" + ((TLUserBase)dialog.With).Id + "&encryptedchat_id=" + dialog.Peer.Id;
                }

                return "Action=SecondaryTile&from_id=" + user.Id;
            }

            if (channel != null)
            {
                return "Action=SecondaryTile&channel_id=" + channel.Id;
            }

            if (broadcast != null)
            {
                return "Action=SecondaryTile&broadcast_id=" + broadcast.Id;
            }

            if (chat != null)
            {
                return "Action=SecondaryTile&chat_id=" + chat.Id;
            }

            return null;
        }

        private static TLFileLocation GetTileImageLocation(TLDialogBase dialog)
        {
            var user = dialog.With as TLUserBase;
            var chat = dialog.With as TLChat;
            var channel = dialog.With as TLChannel;

            if (user != null)
            {
                var userProfilePhoto = user.Photo as TLUserProfilePhoto;
                if (userProfilePhoto != null)
                {
                    return userProfilePhoto.PhotoSmall as TLFileLocation;
                }
            }
            else if (chat != null)
            {
                var chatPhoto = chat.Photo as TLChatPhoto;
                if (chatPhoto != null)
                {
                    return chatPhoto.PhotoSmall as TLFileLocation;
                }
            }
            else if (channel != null)
            {
                var chatPhoto = channel.Photo as TLChatPhoto;
                if (chatPhoto != null)
                {
                    return chatPhoto.PhotoSmall as TLFileLocation;
                }
            }

            return null;
        }

        private static Uri GetTileImageUri(TLFileLocation location)
        {
            if (location == null) return null;

            var photoPath = String.Format("{0}_{1}_{2}.jpg", location.VolumeId, location.LocalId, location.Secret);

            var store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!string.IsNullOrEmpty(photoPath)
                && store.FileExists(photoPath))
            {
                const string imageFolder = @"\Shared\ShellContent";
                if (!store.DirectoryExists(imageFolder))
                {
                    store.CreateDirectory(imageFolder);
                }
                if (!store.FileExists(Path.Combine(imageFolder, photoPath)))
                {
                    store.CopyFile(photoPath, Path.Combine(imageFolder, photoPath));
                }

                return new Uri(@"isostore:" + Path.Combine(imageFolder, photoPath), UriKind.Absolute);
            }

            return null;
        }

        public void Group(TLDialogBase dialogBase)
        {
            var dialog = dialogBase as TLDialog;
            if (dialog == null) return;

            var channel = dialog.With as TLChannel76;
            if (channel == null) return;

            var feedId = channel.FeedId != null ? null : new TLInt(1);
            IsWorking = true;
            MTProtoService.ChangeFeedBroadcastAsync(channel.ToInputChannel(), feedId,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    //Handle(new TLUpdateDialogPinned76 { Peer = dialog53.Peer, Pinned = pinned });
                    channel.FeedId = feedId;
                    Execute.ShowDebugMessage(result.ToString());

                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }));
        }

        public void Pin(TLDialogBase dialog)
        {
            var dialog53 = dialog as TLDialog53;
            if (dialog53 != null)
            {
                var pinned = !dialog53.IsPinned;
                IsWorking = true;
                MTProtoService.ToggleDialogPinAsync(pinned, dialog53.Peer,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        Handle(new TLUpdateDialogPinned76 { Peer = new TLDialogPeer { Peer = dialog53.Peer }, Pinned = pinned });
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                    }));
            }
        }

        public void PinToStart(TLDialogBase dialog)
        {
            PinToStartCommon(dialog);
        }

        public static void PinToStartCommon(TLDialogBase dialog)
        {
            if (dialog == null) return;
            if (dialog.With == null) return;

            var tileNavigationParam = GetTileNavigationParam(dialog);

            try
            {
                var tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(tileNavigationParam));
                if (tile != null)
                {
                    tile.Delete();
                }
                else
                {
                    var title = DialogCaptionConverter.Convert(dialog.With);
                    var standartTileData = new StandardTileData { BackContent = AppResources.AppName, Title = title, BackTitle = title };

                    var imageLocation = GetTileImageLocation(dialog);

                    var imageUri = GetTileImageUri(imageLocation);
                    if (imageUri != null)
                    {
                        standartTileData.BackgroundImage = imageUri;
                    }
                    ShellTile.Create(new Uri("/Views/ShellView.xaml?" + tileNavigationParam, UriKind.Relative), standartTileData);
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage("Pin tile error " + ex);
            }
        }

        public static void UnpinFromStart(TLDialogBase dialog)
        {
            if (dialog == null) return;
            if (dialog.With == null) return;

            var tileNavigationParam = GetTileNavigationParam(dialog);

            var tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(tileNavigationParam));
            if (tile != null)
            {
                tile.Delete();
            }
        }

        #endregion

        public void OpenChatDetails(TLChatBase chat)
        {
            if (chat == null) return;

            StateService.With = chat;
            StateService.AnimateTitle = true;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }
    }
}
