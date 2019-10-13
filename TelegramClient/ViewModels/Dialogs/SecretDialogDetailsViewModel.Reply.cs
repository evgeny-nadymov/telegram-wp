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
using System.Windows.Media;
using Telegram.Api;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
        public Brush ReplyBackgroundBrush
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (!isLightTheme)
                {
                    if (StateService.IsEmptyBackground)
                    {
                        return (Brush)Application.Current.Resources["PhoneChromeBrush"];
                    }
                }
                var color = Colors.Black;
                color.A = 102;
                return new SolidColorBrush(color);
            }
        }

        private readonly Dictionary<string, TLMessageMediaBase> _webPagesCache = new Dictionary<string, TLMessageMediaBase>();

        private TLDecryptedMessageBase _previousReply;

        private void SaveReply()
        {
            if (Reply != null && !IsWebPagePreview(Reply))
            {
                _previousReply = Reply;
            }
        }

        private static bool IsWebPagePreview(TLDecryptedMessageBase message)
        {
            var messagesContainer = message as TLDecryptedMessagesContainter;
            if (messagesContainer != null)
            {
                return messagesContainer.WebPageMedia != null;
            }

            return false;
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

        private readonly object _webPagePreviewsSyncRoot = new object();

        private bool? _isWebPagePreviewEnabled;

        private bool CheckWebPagePreviewsNotification()
        {
            if (_isWebPagePreviewEnabled.HasValue)
            {
                return _isWebPagePreviewEnabled.Value;
            }

            if (!_isWebPagePreviewEnabled.HasValue)
            {
                var result = TLUtils.OpenObjectFromMTProtoFile<TLBool>(_webPagePreviewsSyncRoot, Constants.WebPagePreviewsFileName);
                if (result == null)
                {
                    var messageBoxResult = MessageBox.Show(AppResources.LInkPreviewsNotification, AppResources.Confirm, MessageBoxButton.OKCancel);
                    _isWebPagePreviewEnabled = messageBoxResult == MessageBoxResult.OK;

                    TLUtils.SaveObjectToMTProtoFile(_webPagePreviewsSyncRoot, Constants.WebPagePreviewsFileName, new TLBool(_isWebPagePreviewEnabled.Value));
                }
                else
                {
                    _isWebPagePreviewEnabled = result.Value;
                }
            }

            return _isWebPagePreviewEnabled.Value;
        }

        private void GetWebPagePreviewAsync(string t)
        {
            if (t == null)
            {
                return;
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

                        Reply = new TLDecryptedMessagesContainter { WebPageMedia = webPageMedia };
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
                    if (!CheckWebPagePreviewsNotification()) return;

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

                                    Reply = new TLDecryptedMessagesContainter { WebPageMedia = result };
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


        public void OpenReply(TLDecryptedMessageBase message)
        {
            if (message == null) return;

            var reply = message.Reply;
            if (reply == null) return;
            if (reply.RandomIndex == 0) return;

            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].RandomIndex == reply.RandomIndex)
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

        private void HighlightMessage(TLDecryptedMessageBase message)
        {
            message.IsHighlighted = true;
            BeginOnUIThread(TimeSpan.FromSeconds(2.0), () =>
            {
                message.IsHighlighted = false;
            });
        }

        public void ProcessMessages(IList<TLDecryptedMessageBase> messages)
        {
            ProcessGroupedMessages(messages);

            var replyToMsgIds = new TLVector<TLLong>();
            var replyToMsgs = new List<TLDecryptedMessageBase>();
            for (var i = 0; i < messages.Count; i++)
            {
                var decryptedMessage = messages[i] as TLDecryptedMessage;
                if (decryptedMessage != null
                    && decryptedMessage.TTL != null
                    && decryptedMessage.TTL.Value > 0
                    && decryptedMessage.Media != null)
                {
                    decryptedMessage.Media.TTLSeconds = decryptedMessage.TTL;
                }

                var decryptedMessage45 = messages[i] as TLDecryptedMessage45;
                if (decryptedMessage45 != null)
                {

                    if (decryptedMessage45.ReplyToRandomMsgId != null)
                    {
                        var replyToRandomMsgId = decryptedMessage45.ReplyToRandomMsgId;
                        if (replyToRandomMsgId != null
                            && replyToRandomMsgId.Value != 0)
                        {
                            var reply = CacheService.GetDecryptedMessage(Chat.Id, replyToRandomMsgId);
                            if (reply != null)
                            {
                                messages[i].Reply = reply;
                            }
                            else
                            {
                                replyToMsgIds.Add(replyToRandomMsgId);
                                replyToMsgs.Add(decryptedMessage45);
                            }
                        }
                    }


                    if (decryptedMessage45.NotListened)
                    {
                        if (decryptedMessage45.Media != null)
                        {
                            decryptedMessage45.Media.Out = decryptedMessage45.Out.Value;
                            decryptedMessage45.Media.NotListened = true;
                        }
                    }

                    var decryptedMediaWebPage = decryptedMessage45.Media as TLDecryptedMessageMediaWebPage;
                    if (decryptedMediaWebPage != null)
                    {
                        if (decryptedMediaWebPage.WebPage == null && !TLString.IsNullOrEmpty(decryptedMediaWebPage.Url))
                        {
                            MTProtoService.GetWebPagePreviewAsync(decryptedMediaWebPage.Url,
                                result => Execute.BeginOnUIThread(() =>
                                {
                                    var mediaWebPage = result as TLMessageMediaWebPage;
                                    if (mediaWebPage != null)
                                    {
                                        decryptedMediaWebPage.WebPage = mediaWebPage.WebPage;
                                        decryptedMessage45.NotifyOfPropertyChange(() => decryptedMessage45.MediaSelf);
                                    }
                                }),
                                error =>
                                {
                                    Execute.ShowDebugMessage("messages.getWebPagePreview error " + error);
                                });
                        }
                    }
                }
            }
        }

        private void ProcessGroupedMessages(IList<TLDecryptedMessageBase> messages)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                var firstMessage = messages[i] as TLDecryptedMessage73;
                if (firstMessage != null && firstMessage.GroupedId != null)
                {
                    var mediaPhoto = firstMessage.Media as TLDecryptedMessageMediaPhoto;
                    if (mediaPhoto != null || firstMessage.IsVideo())
                    {
                        var groupCount = GetGroupLength(messages, firstMessage.GroupedId, i + 1);
                        if (groupCount > 0)
                        {
                            var groupedMessage = ReplaceWithGroup(messages, i, groupCount + 1);
                        }
                    }
                }
            }
        }

        private static TLDecryptedMessage ReplaceWithGroup(IList<TLDecryptedMessageBase> messages, int position, int length)
        {
            var message = messages[position + length - 1] as TLDecryptedMessage73;
            if (message != null)
            {
                var group = new TLVector<TLDecryptedMessageBase>();
                for (var i = 0; i < length; i++)
                {
                    group.Insert(0, messages[position]);
                    messages.RemoveAt(position);
                }

                var mediaGroup = new TLDecryptedMessageMediaGroup { Group = group };

                var groupedMessage = new TLDecryptedMessage73
                {
                    Flags = new TLInt(0),
                    Out = message.Out,
                    Unread = message.Unread,
                    RandomId = message.RandomId,
                    FromId = message.FromId,
                    //ToId = message.ToId,
                    //FwdHeader = message.FwdHeader,
                    ViaBotName = message.ViaBotName,
                    ReplyToRandomMsgId = message.ReplyToRandomMsgId,
                    Date = message.Date,
                    TTL = new TLInt(0), //message.TTL,
                    Message = TLString.Empty,
                    Media = mediaGroup,
                    //ReplyMarkup = message.ReplyMarkup,
                    Entities = new TLVector<TLMessageEntityBase>(),
                    //Views = message.Views,
                    //EditDate = message.EditDate,
                    //PostAuthor = message.PostAuthor,
                    GroupedId = message.GroupedId,
                    Status = message.Status
                };

                //if (groupedMessage.FromId != null) groupedMessage.SetFromId();
                if (groupedMessage.Media != null) groupedMessage.SetMedia();

                messages.Insert(position, groupedMessage);

                return groupedMessage;
            }

            return null;
        }

        private static int GetGroupLength(IList<TLDecryptedMessageBase> messages, TLLong groupId, int start)
        {
            var count = 0;
            for (var i = start; i < messages.Count; i++)
            {
                var message = messages[i] as TLDecryptedMessage73;
                if (message != null
                    && message.GroupedId != null
                    && message.GroupedId.Value == groupId.Value
                    //&& !message.IsExpired()
                    && (message.Media is TLDecryptedMessageMediaPhoto || message.IsVideo()))
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

        private TLDecryptedMessageBase _reply;

        public TLDecryptedMessageBase Reply
        {
            get { return _reply; }
            set
            {
                var notifyChanges = _reply != value;
                SetField(ref _reply, value, () => Reply);
                if (notifyChanges)
                {
                    NotifyOfPropertyChange(() => ReplyInfo);
                    NotifyOfPropertyChange(() => CanSend);
                }
            }
        }

        public ReplyInfo ReplyInfo
        {
            get
            {
                if (_reply != null)
                {
                    return new ReplyInfo { Reply = _reply, ReplyToRandomMsgId = _reply.RandomId };
                }

                return null;
            }
        }

        public void ReplyMessage(TLDecryptedMessageBase message)
        {
            if (message == null) return;
            if (message.RandomIndex == 0) return;

            //var message31 = message as TLMessage31;
            //if (message31 != null && !message31.Out.Value)
            //{
            //    var fromId = message31.FromId;
            //    var user = CacheService.GetUser(fromId) as TLUser;
            //    if (user != null && user.IsBot)
            //    {
            //        SetReplyMarkup(message31);
            //    }
            //}

            Reply = message;
        }

        public void DeleteReply()
        {
            //var message31 = Reply as TLMessage31;
            //if (message31 != null)
            //{
            //    if (message31.ReplyMarkup != null)
            //    {
            //        message31.ReplyMarkup.HasResponse = true;
            //    }
            //}

            if (_previousReply != null)
            {
                Reply = _previousReply;
                _previousReply = null;
            }
            else
            {
                //if (_replyMarkupMessage == Reply)
                //{
                //    SetReplyMarkup(null);
                //}
                Reply = null;
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

        public TLAllStickers Stickers { get; protected set; }

        public TLStickerPack GetStickerPack(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            if (Stickers == null) return null;
            if (Stickers.Packs == null) return null;

            for (var i = 0; i < Stickers.Packs.Count; i++)
            {
                if (Stickers.Packs[i].Emoticon != null
                    && Stickers.Packs[i].Emoticon.ToString() == text)
                {
                    return Stickers.Packs[i];
                }
            }

            return null;
        }

        public TLStickerPack GetFeaturedStickerPack(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var featuredStickers = StateService.GetFeaturedStickers();
            if (featuredStickers == null) return null;
            if (featuredStickers.Packs == null) return null;

            for (var i = 0; i < featuredStickers.Packs.Count; i++)
            {
                if (featuredStickers.Packs[i].Emoticon != null
                    && featuredStickers.Packs[i].Emoticon.ToString() == text)
                {
                    return featuredStickers.Packs[i];
                }
            }

            return null;
        }

        public void GetAllStickersAsync()
        {
            StateService.GetAllStickersAsync(cachedStickers =>
            {
                Stickers = cachedStickers;

                var cachedStickers43 = cachedStickers as TLAllStickers43;
                if (cachedStickers43 != null
                    && cachedStickers43.FavedStickers == null)
                {
                    MTProtoService.GetFavedStickersAsync(new TLInt(0),
                        result =>
                        {
                            var favedStickers = result as TLFavedStickers;
                            if (favedStickers != null)
                            {
                                cachedStickers43.FavedStickers = favedStickers;
                                StateService.SaveAllStickersAsync(cachedStickers43);
                            }
                        },
                        error =>
                        {

                        });
                }

                var featuredStickers = StateService.GetFeaturedStickers();
                if (featuredStickers == null)
                {
                    MTProtoService.GetFeaturedStickersAsync(true, new TLInt(0),
                        result =>
                        {
                            featuredStickers = result as TLFeaturedStickers;
                            if (featuredStickers != null)
                            {
                                StateService.SaveFeaturedStickersAsync(featuredStickers);
                            }
                        },
                        error =>
                        {

                        });
                }

                var cachedStickers29 = cachedStickers as TLAllStickers29;
                if (cachedStickers29 != null
                    && cachedStickers29.Date != null
                    && cachedStickers29.Date.Value != 0)
                {
                    var date = TLUtils.ToDateTime(cachedStickers29.Date);
                    if (
                        date < DateTime.Now.AddSeconds(Constants.GetAllStickersInterval))
                    {
                        return;
                    }
                }

                var hash = cachedStickers != null ? cachedStickers.Hash ?? TLString.Empty : TLString.Empty;

                MTProtoService.GetAllStickersAsync(hash,
                    result =>
                    {
                        var allStickers = result as TLAllStickers43;
                        if (allStickers != null)
                        {
                            if (cachedStickers29 != null)
                            {
                                allStickers.ShowStickersTab = cachedStickers29.ShowStickersTab;
                                allStickers.RecentlyUsed = cachedStickers29.RecentlyUsed;
                                allStickers.Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
                            }
                            if (cachedStickers43 != null)
                            {
                                allStickers.RecentStickers = cachedStickers43.RecentStickers;
                                allStickers.FavedStickers = cachedStickers43.FavedStickers;
                            }
                            Stickers = allStickers;
                            cachedStickers = allStickers;
                            StateService.SaveAllStickersAsync(cachedStickers);
                        }
                    },
                    error =>
                    {
                        Execute.ShowDebugMessage("messages.getAllStickers error " + error);
                    });
            });
        }

        private readonly Dictionary<string, IList<TLStickerItem>> _cachedStickerResults = new Dictionary<string, IList<TLStickerItem>>();

        private const int MaxResults = 5;
        private void GetStickerHints(string text)
        {
            var stickersCache = new Dictionary<long, long>();
            var stickers = new List<TLStickerItem>();
            var currentStickerText = text.Trim();

            var allStickers43 = StateService.GetAllStickers() as TLAllStickers43;
            var key = string.Format("{0}\ashowStickersByEmoji={1}", text, allStickers43 != null ? allStickers43.ShowStickersByEmoji : ShowStickersByEmoji.AllSets);
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
                    /*var featuredPack = GetFeaturedStickerPack(currentStickerText);
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
                    }*/

                    // 5. search on server side
                    MTProtoService.GetStickersAsync(new TLString(currentStickerText), new TLInt(0),
                        result => Execute.BeginOnUIThread(() =>
                        {
                            var stickersResult = result as TLStickers79;
                            if (stickersResult != null)
                            {
                                if (!string.IsNullOrEmpty(Text))
                                {
                                    string searchText;
                                    var searchByStickers = SearchStickerHints(Text, out searchText);
                                    if (searchByStickers)
                                    {
                                        if (string.Equals(currentStickerText, searchText))
                                        {
                                            if (StickerHints.Hints.Count == MaxResults)
                                            {
                                                IList<TLStickerItem> cachedResult;
                                                if (_cachedStickerResults.TryGetValue(key, out cachedResult))
                                                {
                                                    for (var i = 0; i < stickersResult.Stickers.Count; i++)
                                                    {
                                                        var item = new TLStickerItem { Document = stickersResult.Stickers[i] };

                                                        cachedResult.Add(item);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                IList<TLStickerItem> cachedResult;
                                                if (_cachedStickerResults.TryGetValue(key, out cachedResult))
                                                {
                                                    for (var i = 0; i < stickersResult.Stickers.Count; i++)
                                                    {
                                                        var item = new TLStickerItem { Document = stickersResult.Stickers[i] };

                                                        cachedResult.Add(item);
                                                        StickerHints.Hints.Add(item);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            Execute.ShowDebugMessage("messages.getStickers error " + error);
                        }));
                }
            }

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

        private static bool SearchStickerHints(string text, out string searchText)
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
                var searchByStickers = SearchStickerHints(Text, out searchText);

                if (searchByStickers)
                {
                    CreateStickerHints();

                    if (StickerHints.Hints.Count == MaxResults)
                    {
                        var allStickers43 = StateService.GetAllStickers() as TLAllStickers43;
                        var key = string.Format("{0}\ashowStickersByEmoji={1}", searchText, allStickers43 != null ? allStickers43.ShowStickersByEmoji : ShowStickersByEmoji.AllSets);
                        IList<TLStickerItem> cachedResult;
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
    }
}
