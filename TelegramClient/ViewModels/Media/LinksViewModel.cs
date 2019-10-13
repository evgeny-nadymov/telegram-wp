// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Phone.Tasks;
using Telegram.Api;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services.Cache.EventArgs;
using Telegram.EmojiPanel;
using TelegramClient.Helpers;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Media
{
    public class LinksViewModel<T> : LinksViewModelBase<T>,
        Telegram.Api.Aggregator.IHandle<TLMessageCommon>,
        ISliceLoadable
        where T : IInputPeer
    {
        public override TLInputMessagesFilterBase InputMessageFilter
        {
            get { return new TLInputMessagesFilterUrl(); }
        }

        public ObservableCollection<TimeKeyGroup<TLMessageBase>> Files { get; set; }

        public string EmptyListImageSource
        {
            get { return  "/Images/Messages/link.png"; }
        }

        public LinksViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Files = new ObservableCollection<TimeKeyGroup<TLMessageBase>>();
            Status = AppResources.Loading;
            IsEmptyList = false;
            Items = new ObservableCollection<TLMessage>();

            DisplayName = LowercaseConverter.Convert(AppResources.SharedLinks);
            EventAggregator.Subscribe(this);

            PropertyChanged += (o, e) =>
            {
                if (Property.NameEquals(e.PropertyName, () => IsSelectionEnabled))
                {
                    if (!IsSelectionEnabled)
                    {
                        foreach (var item in Items)
                        {
                            item.IsSelected = false;
                        }
                    }
                }
            };
        }

        protected override void OnInitialize()
        {
            BeginOnThreadPool(LoadNextSlice);

            base.OnInitialize();
        }

        private bool _isLoadingNextSlice;

        public void LoadNextSlice()
        {
            if (_isLoadingNextSlice) return;

            if (CurrentItem is TLBroadcastChat && !(CurrentItem is TLChannel))
            {
                Status = string.Empty;
                if (Items.Count == 0)
                {
                    IsEmptyList = true;
                    NotifyOfPropertyChange(() => IsEmptyList);
                }
                return;
            }

            var channel = CurrentItem as TLChannel;
            if (channel != null && channel.MigratedFromChatId != null)
            {
                if (IsLastSliceLoaded)
                {
                    LoadNextMigratedHistorySlice();
                    return;
                }

                var lastMessage = Items.LastOrDefault() as TLMessageCommon;
                if (lastMessage != null
                    && lastMessage.ToId is TLPeerChat)
                {
                    LoadNextMigratedHistorySlice();
                    return;
                }
            }

            if (IsLastSliceLoaded)
            {
                return;
            }

            var maxId = 0;
            var lastItem = Items.LastOrDefault();
            if (lastItem != null)
            {
                maxId = lastItem.Index;
            }

            IsWorking = true;
            _isLoadingNextSlice = true;
            MTProtoService.SearchAsync(
                CurrentItem.ToInputPeer(),
                TLString.Empty,
                null,
                InputMessageFilter,
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(maxId), 
                new TLInt(Constants.FileSliceLength),
                new TLInt(0),
                messages =>
                {
                    LinkUtils.ProcessLinks(messages.Messages, _mediaWebPagesCache);

                    BeginOnUIThread(() =>
                    {
                        Status = string.Empty;
                        IsWorking = false;
                        _isLoadingNextSlice = false;

                        AddMessages(messages.Messages.ToList());

                        if (messages.Messages.Count < Constants.PhotoVideoSliceLength)
                        {
                            IsLastSliceLoaded = true;
                            LoadNextMigratedHistorySlice();
                        }

                        IsEmptyList = Items.Count == 0;
                        NotifyOfPropertyChange(() => IsEmptyList);
                    });
                },
                error =>
                {
                    Status = string.Empty;
                    IsWorking = false;
                    _isLoadingNextSlice = false;

                    Execute.ShowDebugMessage("messages.search error " + error);
                });
        }

        private bool _isLastMigratedHistorySliceLoaded;

        private bool _isLoadingNextMigratedHistorySlice;

        private void LoadNextMigratedHistorySlice()
        {
            var channel = CurrentItem as TLChannel;
            if (channel == null || channel.MigratedFromChatId == null) return;

            if (_isLastMigratedHistorySliceLoaded) return;

            if (_isLoadingNextMigratedHistorySlice) return;

            var maxMessageId = int.MaxValue;
            for (var i = 0; i < Items.Count; i++)
            {
                var messageCommon = Items[i] as TLMessageCommon;
                if (messageCommon == null) continue;

                var peerChat = messageCommon.ToId as TLPeerChat;
                if (peerChat == null) continue;

                if (Items[i].Index != 0
                    && Items[i].Index < maxMessageId)
                {
                    maxMessageId = Items[i].Index;
                }
            }

            if (maxMessageId == int.MaxValue)
            {
                maxMessageId = channel.MigratedFromMaxId != null ? channel.MigratedFromMaxId.Value : 0;
            }

            _isLoadingNextMigratedHistorySlice = true;
            IsWorking = true;
            MTProtoService.SearchAsync(
                new TLInputPeerChat { ChatId = channel.MigratedFromChatId },
                TLString.Empty,
                null,
                InputMessageFilter,
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(maxMessageId), 
                new TLInt(Constants.FileSliceLength),
                new TLInt(0),
                result =>
                {
                    LinkUtils.ProcessLinks(result.Messages, _mediaWebPagesCache);

                    BeginOnUIThread(() =>
                    {
                        _isLoadingNextMigratedHistorySlice = false;
                        IsWorking = false;
                        Status = string.Empty;

                        if (result.Messages.Count < Constants.MessagesSlice)
                        {
                            _isLastMigratedHistorySliceLoaded = true;
                        }

                        AddMessages(result.Messages);

                        IsEmptyList = Items.Count == 0;
                        NotifyOfPropertyChange(() => IsEmptyList);
                    });
                },
                error => BeginOnUIThread(() =>
                {
                    _isLoadingNextMigratedHistorySlice = false;
                    IsWorking = false;
                    Status = string.Empty;

                    Execute.ShowDebugMessage("messages.getHistory error " + error);
                }));
        }

        protected override void DeleteLinksInternal(IList<TLMessageBase> messages)
        {
            BeginOnUIThread(() =>
            {
                for (var i = 0; i < messages.Count; i++)
                {
                    for (var j = 0; j < Files.Count; j++)
                    {
                        for (var k = 0; k < Files[j].Count; k++)
                        {
                            if (Files[j][k].Index == messages[i].Index)
                            {
                                Files[j].RemoveAt(k);
                                break;
                            }
                        }

                        if (Files[j].Count == 0)
                        {
                            Files.RemoveAt(j--);

                            if (Files.Count == 0)
                            {
                                Files.Clear();
                            }
                        }
                    }
                    messages[i].IsSelected = false;
                    Items.Remove(messages[i]);
                    if (Items.Count == 0)
                    {
                        IsEmptyList = true;
                        NotifyOfPropertyChange(() => IsEmptyList);
                    }
                }
            });
        }

        private void InsertMessages(IEnumerable<TLMessageBase> messages)
        {
            foreach (var messageBase in messages)
            {
                var message = messageBase as TLMessage;
                if (message == null)
                {
                    continue;
                }

                var date = TLUtils.ToDateTime(message.Date);

                var yearMonthKey = new DateTime(date.Year, date.Month, 1);
                var timeKeyGroup = Files.FirstOrDefault(x => x.Key == yearMonthKey);
                if (timeKeyGroup != null)
                {
                    timeKeyGroup.Insert(0, message);
                }
                else
                {
                    Files.Insert(0, new TimeKeyGroup<TLMessageBase>(yearMonthKey) { message });
                }

                Items.Insert(0, message);
            }
        }

        private void AddMessages(IEnumerable<TLMessageBase> messages)
        {
            foreach (var messageBase in messages)
            {
                var message = messageBase as TLMessage;
                if (message == null)
                {
                    continue;
                }

                var date = TLUtils.ToDateTime(message.Date);

                var yearMonthKey = new DateTime(date.Year, date.Month, 1);
                var timeKeyGroup = Files.FirstOrDefault(x => x.Key == yearMonthKey);
                if (timeKeyGroup != null)
                {
                    timeKeyGroup.Add(message);
                }
                else
                {
                    Files.Add(new TimeKeyGroup<TLMessageBase>(yearMonthKey) { message });
                }

                Items.Add(message);
            }
        }

        public void Manage()
        {
            IsSelectionEnabled = !IsSelectionEnabled;
        }

        public override void Search()
        {
            StateService.CurrentInputPeer = CurrentItem;
            var source = new List<TLMessageBase>(Items.Count);
            foreach (var item in Items)
            {
                source.Add(item);
            }

            StateService.Source = source;
            NavigationService.UriFor<SearchLinksViewModel>().Navigate();
        }

        public void Handle(TLMessageCommon message)
        {
            if (message == null) return;

            if (message.ToId is TLPeerUser)
            {
                var user = CurrentItem as TLUserBase;
                if (user != null)
                {
                    if (!message.Out.Value
                        && user.Index == message.FromId.Value)
                    {
                        InsertMessage(message);
                    }
                    else if (message.Out.Value
                        && user.Index == message.ToId.Id.Value)
                    {
                        InsertMessage(message);
                    }
                }
            }
            else if (message.ToId is TLPeerChat)
            {
                var chat = CurrentItem as TLChatBase;
                if (chat != null)
                {
                    if (chat.Index == message.ToId.Id.Value)
                    {
                        InsertMessage(message);
                    }
                }
            }
        }

        private void InsertMessage(TLMessageCommon message)
        {
            var messagesWithLinks = LinkUtils.ProcessLinks(new List<TLMessageBase> { message }, _mediaWebPagesCache);
            if (messagesWithLinks.Count > 0)
            {
                BeginOnUIThread(() =>
                {
                    InsertMessages(messagesWithLinks);

                    Status = string.Empty;
                    if (Items.Count == 0)
                    {
                        IsEmptyList = true;
                        NotifyOfPropertyChange(() => IsEmptyList);
                    }
                });
            }
        }
    }

    public abstract class LinksViewModelBase<T> : ItemsViewModelBase<TLMessage>,
        Telegram.Api.Aggregator.IHandle<DownloadableItem>,
        Telegram.Api.Aggregator.IHandle<TLUpdateWebPage>,
        Telegram.Api.Aggregator.IHandle<MessagesRemovedEventArgs> 
        where T : IInputPeer
    {
        public abstract TLInputMessagesFilterBase InputMessageFilter { get; }

        public T CurrentItem { get; set; }

        public bool IsEmptyList { get; protected set; }

        private bool _isSelectionEnabled;

        public bool IsSelectionEnabled
        {
            get { return _isSelectionEnabled; }
            set { SetField(ref _isSelectionEnabled, value, () => IsSelectionEnabled); }
        }

        public void ChangeGroupActionStatus()
        {
            var selectedItemsCount = Items.Count(x => x.IsSelected);
            SetSelectedCountAction.SafeInvoke(selectedItemsCount);

            NotifyOfPropertyChange(() => IsGroupActionEnabled);
        }

        public bool IsGroupActionEnabled
        {
            get { return Items.Any(x => x.IsSelected); }
        }

        public Action<int> SetSelectedCountAction;

        protected LinksViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {

        }

        public abstract void Search();

        public void DeleteMessage(TLMessageBase message)
        {
            if (message == null) return;

            var messages = new List<TLMessageBase> { message };

            var owner = CurrentItem as TLObject;

            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                var messageCommon = message as TLMessageCommon;
                if (messageCommon != null)
                {
                    if (messageCommon.ToId is TLPeerChat)
                    {
                        DialogDetailsViewModel.DeleteMessages(MTProtoService, false, null, null, messages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
                        return;
                    }
                }

                DialogDetailsViewModel.DeleteChannelMessages(MTProtoService, channel, null, null, messages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
                return;
            }

            if (CurrentItem is TLBroadcastChat)
            {
                DeleteMessagesInternal(owner, null, messages);
                return;
            }

            if ((message.Id == null || message.Id.Value == 0) && message.RandomIndex != 0)
            {
                DeleteMessagesInternal(owner, null, messages);
                return;
            }

            DialogDetailsViewModel.DeleteMessages(MTProtoService, false, null, null, messages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
        }

        private void DeleteMessagesInternal(TLObject owner, TLMessageBase lastMessage, IList<TLMessageBase> messages)
        {
            var ids = new TLVector<TLInt>();
            for (int i = 0; i < messages.Count; i++)
            {
                ids.Add(messages[i].Id);
            }

            // duplicate: deleting performed through updates
            CacheService.DeleteMessages(ids);

            DeleteLinksInternal(messages);

            EventAggregator.Publish(new DeleteMessagesEventArgs { Owner = owner, Messages = messages });
        }

        protected virtual void DeleteLinksInternal(IList<TLMessageBase> messages) { }

        public void DeleteMessages()
        {
            if (Items == null) return;

            var messages = new List<TLMessageBase>();
            foreach (var item in Items.Where(x => x.IsSelected))
            {
                messages.Add(item);
            }

            if (messages.Count == 0) return;

            var randomItems = new List<TLMessageBase>();
            var items = new List<TLMessageBase>();

            TLMessageBase lastItem = null;
            for (var i = 0; i < Items.Count; i++)
            {
                var message = Items[i];
                if (message.IsSelected)
                {
                    if (message.Index == 0 && message.RandomIndex != 0)
                    {
                        randomItems.Add(message);
                        lastItem = null;
                    }
                    else if (message.Index != 0)
                    {
                        items.Add(message);
                        lastItem = null;
                    }
                }
                else
                {
                    if (lastItem == null)
                    {
                        lastItem = message;
                    }
                }
            }

            if (randomItems.Count == 0 && items.Count == 0)
            {
                return;
            }

            IsSelectionEnabled = false;

            var owner = CurrentItem as TLObject;

            var channel = CurrentItem as TLChannel;
            if (channel != null)
            {
                var chatMessages = new List<TLMessageBase>();
                var channelMessages = new List<TLMessageBase>();
                if (channel.MigratedFromChatId != null)
                {
                    foreach (var item in items)
                    {
                        var message = item as TLMessageCommon;
                        if (message != null && message.ToId is TLPeerChat)
                        {
                            chatMessages.Add(message);
                        }
                        else
                        {
                            channelMessages.Add(message);
                        }
                    }
                }
                if (chatMessages.Count > 0)
                {
                    DialogDetailsViewModel.DeleteChannelMessages(MTProtoService, channel, lastItem, null, channelMessages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
                    DialogDetailsViewModel.DeleteMessages(MTProtoService, false, lastItem, null, chatMessages, null, (result1, result2) => DeleteMessagesInternal(owner, result1, result2));

                    return;
                }

                DialogDetailsViewModel.DeleteChannelMessages(MTProtoService, channel, lastItem, randomItems, items, (result1, result2) => DeleteMessagesInternal(owner, result1, result2), (result1, result2) => DeleteMessagesInternal(owner, result1, result2));

                return;
            }

            if (CurrentItem is TLBroadcastChat)
            {
                DeleteMessagesInternal(owner, null, randomItems);
                DeleteMessagesInternal(owner, null, items);
                return;
            }

            DialogDetailsViewModel.DeleteMessages(MTProtoService, false, null, randomItems, items, (result1, result2) => DeleteMessagesInternal(owner, result1, result2), (result1, result2) => DeleteMessagesInternal(owner, result1, result2));
        }

        public void ForwardMessage(TLMessageBase message)
        {
            if (message == null) return;

            DialogDetailsViewModel.ForwardMessagesCommon(new List<TLMessageBase> { message }, StateService, NavigationService);
        }

        public void ForwardMessages()
        {
            if (Items == null) return;

            var messages = new List<TLMessageBase>();
            foreach (var item in Items.Where(x => x.IsSelected))
            {
                messages.Add(item);
            }

            if (messages.Count == 0) return;

            IsSelectionEnabled = false;

            DialogDetailsViewModel.ForwardMessagesCommon(messages, StateService, NavigationService);
        }

        public void OpenMedia(TLMessage message)
        {
            if (message == null) return;

            var mediaWebPage = message.Media as TLMessageMediaWebPage;
            if (mediaWebPage != null)
            {
                var webPage = mediaWebPage.WebPage as TLWebPage;
                if (webPage != null)
                {
                    var url = webPage.Url.ToString();
                    OpenLink(url);
                }
            }
        }

        public void OpenLink(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (url.ToLowerInvariant().Contains("telegram.me")
                    || url.ToLowerInvariant().Contains("t.me"))
                {
                    DialogDetailsViewModel.OnTelegramLinkActionCommon(MTProtoService, StateService, new TelegramEventArgs{ Uri = url }, null, null);
                }
                else
                {
                    var task = new WebBrowserTask();
                    task.URL = HttpUtility.UrlEncode(url);
                    task.Show();
                }
            }
        }

        public void Handle(DownloadableItem item)
        {
            var webPage = item.Owner as TLWebPage;
            if (webPage != null)
            {
                var messages = Items;
                foreach (var m in messages)
                {
                    var media = m.Media as TLMessageMediaWebPage;
                    if (media != null && media.WebPage == webPage)
                    {
                        media.NotifyOfPropertyChange(() => media.Photo);
                        media.NotifyOfPropertyChange(() => media.Self);
                        break;
                    }
                }
            }
        }

        public void Handle(MessagesRemovedEventArgs args)
        {
            var with = CurrentItem as TLObject;
            if (with == args.Dialog.With && args.Messages != null)
            {
                DeleteLinksInternal(args.Messages);
            }
        }

        protected readonly List<TLMessageMediaWebPage> _mediaWebPagesCache = new List<TLMessageMediaWebPage>();

        public void Handle(TLUpdateWebPage updateWebPage)
        {
            Execute.BeginOnUIThread(() =>
            {
                for (var i = 0; i < _mediaWebPagesCache.Count; i++)
                {
                    var mediaWebPage = _mediaWebPagesCache[i];
                    if (mediaWebPage.WebPage.Id.Value == updateWebPage.WebPage.Id.Value)
                    {
                        mediaWebPage.WebPage = updateWebPage.WebPage;

                        foreach (var item in Items)
                        {
                            var itemMediaWebPage = item.Media as TLMessageMediaWebPage;
                            if (itemMediaWebPage != null
                                && itemMediaWebPage.WebPage.Id.Value == mediaWebPage.WebPage.Id.Value)
                            {
                                item.NotifyOfPropertyChange(() => item.Self);
                            }
                        }

                        _mediaWebPagesCache.RemoveAt(i--);
                    }
                }
            });
        }
    }

    public static class LinkUtils
    {
        public static List<TLMessageBase> ProcessLinks(IList<TLMessageBase> messages, IList<TLMessageMediaWebPage> mediaWebPagesCache)
        {
            const string linkPattern = "(https?:\\/\\/)?(([A-Za-zА-Яа-яЁё0-9@][A-Za-zА-Яа-яЁё0-9@\\-_\\.]*[A-Za-zА-Яа-яЁё0-9@])(\\/([A-Za-zА-Яа-я0-9@\\-_#%&?+\\/\\.=;:~]*[^\\.\\,;\\(\\)\\?<\\&\\s:])?)?)";
            const string ipv4Pattern = @"([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}"; 
                //"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.)\\{3\\}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
            var messagesWithLinks = new List<TLMessageBase>();
            foreach (var messageBase in messages)
            {
                var message = messageBase as TLMessage;
                if (message != null)
                {
                    var text = message.Message.ToString();
                    message.Links = new List<string>();
                    foreach (Match m in Regex.Matches(text, linkPattern, RegexOptions.IgnoreCase))
                    {
                        var url = GetUrl(m);
                        if (url != null)
                        {
                            message.Links.Add(url);
                        }
                    }

                    foreach (Match m in Regex.Matches(text, ipv4Pattern, RegexOptions.IgnoreCase))
                    {
                        message.Links.Add("http://" + m.Value);
                    }

                    var mediaEmpty = message.Media as TLMessageMediaEmpty;
                    if (mediaEmpty != null)
                    {
                        if (message.Links.Count > 0)
                        {
                            var title = GetWebPageTitle(message.Links[0]);
                            message.WebPageTitle = title;
                        }
                    }

                    var mediaWebPage = message.Media as TLMessageMediaWebPage;
                    if (mediaWebPage != null)
                    {
                        var webPage = mediaWebPage.WebPage as TLWebPage;
                        if (webPage != null)
                        {
                            if (message.Links.Count == 0)
                            {
                                message.Links.Add(webPage.Url.ToString());
                            }
                        }

                        var webPageEmpty = mediaWebPage.WebPage as TLWebPageEmpty;
                        if (webPageEmpty != null)
                        {
                            if (message.Links.Count > 0)
                            {
                                var title = GetWebPageTitle(message.Links[0]);
                                message.WebPageTitle = title;
                            }
                        }

                        var webPagePending = mediaWebPage.WebPage as TLWebPagePending;
                        if (webPagePending != null)
                        {
                            mediaWebPagesCache.Add(mediaWebPage);

                            if (message.Links.Count > 0)
                            {
                                var title = GetWebPageTitle(message.Links[0]);
                                message.WebPageTitle = title;
                            }
                        }
                    }

                    if (message.Links.Count > 0)
                    {
                        messagesWithLinks.Add(messageBase);
                    }
                    else
                    {
                        
                    }
                }
            }
            return messagesWithLinks;
        }

        private static string GetWebPageTitle(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                if (!string.IsNullOrEmpty(uri.Host))
                {
                    var parts = uri.Host.Split('.');
                    if (parts.Length >= 2)
                    {
                        return Language.CapitalizeFirstLetter(parts[parts.Length - 2]);
                    }
                }

            }

            return null;
        }

        private static string GetUrl(Match m)
        {
            var protocol = (m.Groups.Count > 1) ? m.Groups[1].Value : "http://";
            if (protocol == string.Empty) protocol = "http://";

            var url = (m.Groups.Count > 2) ? m.Groups[2].Value : string.Empty;
            var domain = (m.Groups.Count > 3) ? m.Groups[3].Value : string.Empty;

            if (domain.IndexOf(".") == -1 || domain.IndexOf("..") != -1) return null;
            if (url.IndexOf("@") != -1) return null;

            var topDomain = domain.Split('.').LastOrDefault();
            if (topDomain.Length > 5 ||
                !("guru,info,name,aero,arpa,coop,museum,mobi,travel,xxx,asia,biz,com,net,org,gov,mil,edu,int,tel,ac,ad,ae,af,ag,ai,al,am,an,ao,aq,ar,as,at,au,aw,az,ba,bb,bd,be,bf,bg,bh,bi,bj,bm,bn,bo,br,bs,bt,bv,bw,by,bz,ca,cc,cd,cf,cg,ch,ci,ck,cl,cm,cn,co,cr,cu,cv,cx,cy,cz,de,dj,dk,dm,do,dz,ec,ee,eg,eh,er,es,et,eu,fi,fj,fk,fm,fo,fr,ga,gd,ge,gf,gg,gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gu,gw,gy,hk,hm,hn,hr,ht,hu,id,ie,il,im,in,io,iq,ir,is,it,je,jm,jo,jp,ke,kg,kh,ki,km,kn,kp,kr,kw,ky,kz,la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly,ma,mc,md,me,mg,mh,mk,ml,mm,mn,mo,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,mz,na,nc,ne,nf,ng,ni,nl,no,np,nr,nu,nz,om,pa,pe,pf,pg,ph,pk,pl,pm,pn,pr,ps,pt,pw,py,qa,re,ro,ru,rw,sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn,so,sr,st,su,sv,sy,sz,tc,td,tf,tg,th,tj,tk,tl,tm,tn,to,tp,tr,tt,tv,tw,tz,ua,ug,uk,um,us,uy,uz,va,vc,ve,vg,vi,vn,vu,wf,ws,ye,yt,yu,za,zm,zw,рф,cat,pro"
                    .Split(',').Contains(topDomain))) return null;

            return (protocol + url);
        }

    }
}
