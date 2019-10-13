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
using System.Windows;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Resources;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel
    {
        private int _mentionsCounter;

        public int MentionsCounter
        {
            get { return _mentionsCounter; }
            set { SetField(ref _mentionsCounter, value, () => MentionsCounter); }
        }

        private TLMessageBase _currentMention;

        private void GetUnreadMentionsAsync()
        {
            var dialog = CurrentDialog as TLDialog71;
            if (dialog == null) return;
            if (dialog.UnreadMentionsCount.Value <= 0) return;

            var peer = With as IInputPeer;
            if (peer != null)
            {
                MTProtoService.GetUnreadMentionsAsync(
                    peer.ToInputPeer(), 
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(0),
                    result => Execute.BeginOnUIThread(() =>
                    {
                        dialog.UnreadMentionsCount = new TLInt(result.Messages.Count);
                        dialog.UnreadMentions = result.Messages;
                        MentionsCounter = dialog.UnreadMentionsCount.Value;

                        if (MentionsCounter == 0)
                        {
                            Execute.BeginOnUIThread(() =>
                            {
                                View.HideMentionButton();
                            });
                        }
                    }),
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getUnreadMentions error " + error);
                    });
            }
        }

        public void ClearUnreadMentions()
        {
            var dialog = CurrentDialog as TLDialog71;
            if (dialog == null) return;
            if (dialog.UnreadMentions == null || dialog.UnreadMentions.Count == 0)
            {
                dialog.UnreadMentionsCount = new TLInt(0);
                dialog.NotifyOfPropertyChange(() => dialog.UnreadMentionsCount);

                return;
            }

            var confirmation = MessageBox.Show(AppResources.ClearUnreadMentionsConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK) return;

            MentionsCounter = 0;
            Execute.BeginOnUIThread(() =>
            {
                View.HideMentionButton();
            });

            var channel = dialog.With as TLChannel68;
            if (channel != null)
            {
                MTProtoService.ReadMessageContentsAsync(channel.ToInputChannel(), new TLVector<TLInt>(dialog.UnreadMentions.Select(x => x.Id).ToList()),
                    result => Execute.BeginOnUIThread(() =>
                    {
                        dialog.UnreadMentionsCount = new TLInt(0);
                        dialog.NotifyOfPropertyChange(() => dialog.UnreadMentionsCount);

                        var mentionsCache = dialog.UnreadMentions.ToDictionary(x => x.Index);
                        dialog.UnreadMentions.Clear();

                        foreach (var item in Items)
                        {
                            if (mentionsCache.ContainsKey(item.Index))
                            {
                                var message = item as TLMessage25;
                                if (message != null && message.IsVoice())
                                {
                                    message.SetListened();
                                    message.Media.NotListened = false;
                                    message.Media.NotifyOfPropertyChange(() => message.Media.NotListened);
                                }
                            }
                        }
                    }));
            }
            else
            {
                MTProtoService.ReadMessageContentsAsync(new TLVector<TLInt>(dialog.UnreadMentions.Select(x => x.Id).ToList()),
                    result => Execute.BeginOnUIThread(() =>
                    {
                        dialog.UnreadMentionsCount = new TLInt(0);
                        dialog.NotifyOfPropertyChange(() => dialog.UnreadMentionsCount);
                        var mentionsCache = dialog.UnreadMentions.ToDictionary(x => x.Index);
                        dialog.UnreadMentions.Clear();

                        foreach (var item in Items)
                        {
                            if (mentionsCache.ContainsKey(item.Index))
                            {
                                var message = item as TLMessage25;
                                if (message != null && message.IsVoice())
                                {
                                    message.SetListened();
                                    message.Media.NotListened = false;
                                    message.Media.NotifyOfPropertyChange(() => message.Media.NotListened);
                                }
                            }
                        }
                    }));
            }
        }

        public void ReadMentions(int maxId)
        {
            if (maxId <= 0) return;

            var dialog = CurrentDialog as TLDialog71;
            if (dialog == null) return;
            if (dialog.UnreadMentions == null) return;
            if (dialog.UnreadMentions.Count == 0) return;

            var mentions = new List<TLMessageBase>();
            for (int i = dialog.UnreadMentions.Count - 1; i >= 0; i--)
            {
                var message = dialog.UnreadMentions[i] as TLMessage;
                if (message != null && message.IsVoice()) continue;


                if (dialog.UnreadMentions[i].Index <= maxId)
                {
                    mentions.Add(dialog.UnreadMentions[i]);
                }
                else
                {
                    break;
                }
            }

            if (mentions.Count > 0)
            {
                MentionsCounter = Math.Max(0, MentionsCounter - mentions.Count);

                if (MentionsCounter == 0)
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        View.HideMentionButton();
                    });
                }

                var channel = dialog.With as TLChannel68;
                if (channel != null)
                {
                    MTProtoService.ReadMessageContentsAsync(channel.ToInputChannel(), new TLVector<TLInt>(mentions.Select(x => x.Id).ToList()),
                        result => Execute.BeginOnUIThread(() =>
                        {
                            dialog.UnreadMentionsCount = new TLInt(Math.Max(0, dialog.UnreadMentions.Count - mentions.Count));
                            dialog.NotifyOfPropertyChange(() => dialog.UnreadMentionsCount);
                            foreach (var mention in mentions)
                            {
                                dialog.UnreadMentions.Remove(mention);
                            }
                        }));
                }
                else
                {
                    MTProtoService.ReadMessageContentsAsync(new TLVector<TLInt>(mentions.Select(x => x.Id).ToList()),
                        result => Execute.BeginOnUIThread(() =>
                        {
                            dialog.UnreadMentionsCount = new TLInt(Math.Max(0, dialog.UnreadMentions.Count - mentions.Count));
                            dialog.NotifyOfPropertyChange(() => dialog.UnreadMentionsCount);
                            foreach (var mention in mentions)
                            {
                                dialog.UnreadMentions.Remove(mention);
                            }
                        }));
                }
            }
        }

        public void ReadNextMention()
        {
            var dialog = CurrentDialog as TLDialog71;
            if (dialog == null) return;
            if (dialog.UnreadMentions == null) return;
            if (dialog.UnreadMentions.Count == 0) return;

            _currentMention = GetNextMention(_currentMention, dialog.UnreadMentions);
            if (_currentMention == null) return;

            var message = _currentMention as TLMessage;
            if (message != null && message.IsVoice())
            {
                Execute.BeginOnUIThread(() =>
                {
                    OpenMessage(null, _currentMention.Id);
                });
                return;
            }

            MentionsCounter = Math.Max(0, MentionsCounter - 1);

            if (MentionsCounter == 0)
            {
                Execute.BeginOnUIThread(() =>
                {
                    if (_currentMention != null && _currentMention.Index == dialog.TopMessageId.Value)
                    {
                        View.HideScrollToBottomButton();
                    }
                    else
                    {
                        View.HideMentionButton();
                    }
                });
            }

            Execute.BeginOnUIThread(() =>
            {
                OpenMessage(null, _currentMention.Id);
            });

            var channel = dialog.With as TLChannel68;
            if (channel != null)
            {
                MTProtoService.ReadMessageContentsAsync(channel.ToInputChannel(), new TLVector<TLInt>{ _currentMention.Id },
                    result => Execute.BeginOnUIThread(() =>
                    {
                        dialog.UnreadMentionsCount = new TLInt(Math.Max(0, dialog.UnreadMentions.Count - 1));
                        dialog.NotifyOfPropertyChange(() => dialog.UnreadMentionsCount);
                        dialog.UnreadMentions.Remove(_currentMention);
                    }));
            }
            else
            {
                MTProtoService.ReadMessageContentsAsync(new TLVector<TLInt> { _currentMention.Id },
                    result => Execute.BeginOnUIThread(() =>
                    {
                        dialog.UnreadMentionsCount = new TLInt(Math.Max(0, dialog.UnreadMentions.Count - 1));
                        dialog.NotifyOfPropertyChange(() => dialog.UnreadMentionsCount);
                        dialog.UnreadMentions.Remove(_currentMention);
                    }));
            }
        }

        private TLMessageBase GetNextMention(TLMessageBase currentMention, TLVector<TLMessageBase> mentions)
        {
            if (mentions == null) return null;
            if (currentMention == null) return mentions.LastOrDefault();

            for (var i = mentions.Count - 1; i >= 0; i--)
            {
                if (mentions[i].Index == currentMention.Index)
                {
                    if (i >= 1)
                    {
                        return mentions[i - 1];
                    }
                    break;
                }
            }

            if (mentions.Count > 0)
            {
                return mentions.LastOrDefault();
            }

            return null;
        }

        private void ListenMention(int id)
        {
            var dialog71 = CurrentDialog as TLDialog71;
            if (dialog71 != null && dialog71.UnreadMentions != null)
            {
                for (var i = 0; i < dialog71.UnreadMentions.Count; i++)
                {
                    if (id == dialog71.UnreadMentions[i].Index)
                    {
                        dialog71.UnreadMentions.RemoveAt(i);
                        dialog71.UnreadMentionsCount = new TLInt(Math.Max(0, dialog71.UnreadMentions.Count - 1));
                        dialog71.NotifyOfPropertyChange(() => dialog71.UnreadMentionsCount);
                        MentionsCounter = dialog71.UnreadMentionsCount.Value;
                        
                        if (MentionsCounter == 0)
                        {
                            Execute.BeginOnUIThread(() =>
                            {
                                View.HideMentionButton();
                            });
                        }
                        break;
                    }
                }
            }
        }
    }
}
