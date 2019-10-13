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
using System.Globalization;
using Caliburn.Micro;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Location;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Utils;

namespace TelegramClient.ViewModels.Dialogs
{
    public class LiveLocationBadgeViewModel : PropertyChangedBase
    {
        public TLMessage Message { get; set; }

        public IList<TLMessageBase> Messages { get; set; }

        public string Subtitle
        {
            get
            {
                if (IsChatListBadge)
                {
                    var messages = _liveLocationsService.Get();
                    if (messages.Count == 1)
                    {
                        var peerUser = messages[0].ToId as TLPeerUser;
                        if (peerUser != null)
                        {
                            var user = _cacheService.GetUser(peerUser.Id);
                            if (user != null)
                            {
                                return string.Format(" " + AppResources.AttachLiveLocationIsSharing, user.ShortName);
                            }
                        }

                        var peerChat = messages[0].ToId as TLPeerChat;
                        if (peerChat != null)
                        {
                            var chat = _cacheService.GetChat(peerChat.Id);
                            if (chat != null)
                            {
                                return string.Format(" " + AppResources.AttachLiveLocationIsSharing, chat.FullName);
                            }
                        }

                        var peerChannel = messages[0].ToId as TLPeerChannel;
                        if (peerChannel != null)
                        {
                            var chat = _cacheService.GetChat(peerChannel.Id);
                            if (chat != null)
                            {
                                return string.Format(" " + AppResources.AttachLiveLocationIsSharing, chat.FullName);
                            }
                        }

                        return string.Format(" " + AppResources.AttachLiveLocationIsSharing, Language.Declension(
                                1,
                                AppResources.ChatNominativeSingular,
                                AppResources.ChatNominativePlural,
                                AppResources.ChatGenitiveSingular,
                                AppResources.ChatGenitivePlural).ToLower(CultureInfo.CurrentUICulture));
                    }
                    if (messages.Count > 1)
                    {
                        return string.Format(" " + AppResources.AttachLiveLocationIsSharing, Language.Declension(
                                messages.Count,
                                AppResources.ChatNominativeSingular,
                                AppResources.ChatNominativePlural,
                                AppResources.ChatGenitiveSingular,
                                AppResources.ChatGenitivePlural).ToLower(CultureInfo.CurrentUICulture));
                    }

                    return string.Empty;
                }
                else
                {
                    if (Messages == null || Messages.Count == 0)
                    {
                        return string.Format(" - {0}", AppResources.You);
                    }

                    var usersIndex = new Dictionary<int, int>();
                    var shortNames = new List<string>();
                    foreach (var messageBase in Messages)
                    {
                        var message = messageBase as TLMessage70;
                        if (message != null && !message.Out.Value)
                        {
                            var mediaGeoLive = message.Media as TLMessageMediaGeoLive;
                            if (mediaGeoLive != null)
                            {
                                mediaGeoLive.Date = message.Date;
                                mediaGeoLive.EditDate = message.EditDate;
                                if (mediaGeoLive.Active)
                                {
                                    var user = message.From as TLUser;
                                    if (user != null && !usersIndex.ContainsKey(user.Index))
                                    {
                                        usersIndex[user.Index] = user.Index;
                                        shortNames.Add(user.ShortName);
                                    }
                                }
                            }
                        }
                    }

                    if (shortNames.Count == 1)
                    {
                        return string.Format(" - " + AppResources.SharingYouAndOtherName, shortNames[0]);
                    }

                    if (shortNames.Count > 1)
                    {
                        return string.Format(" - " + AppResources.SharingYouAndOtherName, Language.Declension(
                                shortNames.Count,
                                AppResources.CompanyNominativeSingular,
                                AppResources.CompanyNominativePlural,
                                AppResources.CompanyGenitiveSingular,
                                AppResources.CompanyGenitivePlural).ToLower(CultureInfo.CurrentUICulture));
                    }

                    return string.Format(" - {0}", AppResources.You);
                }
            }
        }

        public bool IsChatListBadge { get; protected set; }

        private readonly ILiveLocationService _liveLocationsService;

        private readonly ICacheService _cacheService;

        public LiveLocationBadgeViewModel(ILiveLocationService liveLocationsService, ICacheService cacheService, bool chatListBadge)
        {
            _liveLocationsService = liveLocationsService;
            _cacheService = cacheService;

            IsChatListBadge = chatListBadge;

            Messages = new ObservableCollection<TLMessageBase>();
        }

        public event EventHandler Closed;

        public virtual void RaiseClosed()
        {
            var handler = Closed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler OpenMessage;

        public virtual void RaiseOpenMessage()
        {
            var handler = OpenMessage;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public void UpdateLiveLocations(IList<TLMessageBase> messages)
        {
            Messages.Clear();
            foreach (var m in messages)
            {
                Messages.Add(m);
            }
            NotifyOfPropertyChange(() => Subtitle);
        }

        public void UpdateLiveLocation(TLMessage message)
        {
            var updated = false;
            for (var i = 0; i < Messages.Count; i++)
            {
                var m = Messages[i] as TLMessage70;
                if (m != null && m.Index == message.Index)
                {
                    Messages[i] = message;
                    updated = true;
                    break;
                }
            }

            if (!updated)
            {
                Messages.Add(message);
            }

            NotifyOfPropertyChange(() => Subtitle);
        }

        public void RemoveLiveLocations(IList<TLMessage> messages)
        {
            foreach (var m in messages)
            {
                Messages.Remove(m);
            }

            NotifyOfPropertyChange(() => Subtitle);
        }
    }
}
