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
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Search
{
    public class SearchMessagesViewModel : SearchItemsViewModelBase<TLDialog>, Telegram.Api.Aggregator.IHandle<DownloadableItem>
    {
        public List<TLMessageBase> ForwardMessages { get; set; } 

        public SearchMessagesViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new ObservableCollection<TLDialog>();
            DisplayName = LowercaseConverter.Convert(AppResources.Messages);

            EventAggregator.Subscribe(this);
        }

        private string _lastText;

        private Dictionary<int, TLMessage> _lastCache; 

        public string Text { get; set; }

        private int _offset;

        private bool _isLastSliceLoaded;

        private const int Limit = 40;

        public override void Search(string text)
        {
            _lastText = text;
            Status = string.Empty;
            Items.Clear();

            if (string.IsNullOrEmpty(text))
            {
                Status = AppResources.NoResults;
                return;
            }

            var messages = CacheService.GetMessages().OfType<TLMessage>();

            var cache = new Dictionary<int, TLMessage>();
            LazyItems.Clear();
            foreach (var message in messages)
            {
                if (message.Message.Value.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    var dialog = CacheService.GetDialog(message);
                    LazyItems.Add(new TLDialog24{With = dialog.With, TopMessage = message});
                    cache[message.Index] = message;
                }
            }

            IsWorking = true;
            _offset = 0;
            _isLastSliceLoaded = false;
            MTProtoService.SearchAsync(
                new TLInputPeerEmpty(), 
                new TLString(text), 
                null,
                new TLInputMessagesFilterEmpty(), 
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(_offset), 
                new TLInt(0), 
                new TLInt(Limit),
                new TLInt(0),
                result =>
                {
                    CacheService.AddChats(result.Chats, results => { });
                    CacheService.AddUsers(result.Users, results => { });

                    _offset += Limit;
                    IsWorking = false;

                    var newMessages = result as TLMessages;
                    if (newMessages != null)
                    {
                        foreach (var message in newMessages.Messages.OfType<TLMessageCommon>())
                        {
                            if (cache.ContainsKey(message.Index)) continue;

                            var dialog = new TLDialog {TopMessage = message};
                            var peer = TLUtils.GetPeerFromMessage(message);
                            if (peer is TLPeerUser)
                            {
                                var user = newMessages.Users.FirstOrDefault(x => x.Index == peer.Id.Value);
                                if (user == null)
                                {
                                    continue;
                                }
                                dialog.With = user;
                            }
                            else if (peer is TLPeerChat)
                            {
                                var chat = newMessages.Chats.FirstOrDefault(x => x.Index == peer.Id.Value);
                                if (chat == null)
                                {
                                    continue;
                                }

                                dialog.With = chat;
                            }
                            else if (peer is TLPeerChannel)
                            {
                                var channel = newMessages.Chats.FirstOrDefault(x => x.Index == peer.Id.Value);
                                if (channel == null)
                                {
                                    continue;
                                }

                                dialog.With = channel;
                            }
                            LazyItems.Add(dialog);
                        }
                    }

                    _lastCache = cache;
                    if (Items.Count == 0 && LazyItems.Count == 0) Status = AppResources.NoResults;

                    Deployment.Current.Dispatcher.BeginInvoke(PopulateItems);

                },
                error =>
                {
                    IsWorking = false;
                });

            Deployment.Current.Dispatcher.BeginInvoke(PopulateItems);
        }

        protected override void OnActivate()
        {
            if (_lastText == Text) return;

            Search(Text);

            base.OnActivate();
        }

        public void OpenDialogDetails(TLDialog dialog)
        {
            if (dialog == null) return;

            StateService.Message = dialog.TopMessage;
            StateService.RemoveBackEntry = true;
            StateService.With = dialog.With;
            StateService.ForwardMessages = ForwardMessages; 
            if (ForwardMessages != null)
            {
                StateService.RemoveBackEntries = true;
            }
            StateService.AnimateTitle = true;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        public void LoadNextSlice()
        {
            if (IsWorking) return;
            //if (LazyItems.Count > 0) return;
            if (_isLastSliceLoaded) return;

            var cache = _lastCache;
            var text = _lastText;
            IsWorking = true;
            MTProtoService.SearchAsync(
                new TLInputPeerEmpty(),
                new TLString(_lastText),
                null,
                new TLInputMessagesFilterEmpty(),
                new TLInt(0), 
                new TLInt(0), 
                new TLInt(_offset), 
                new TLInt(0), 
                new TLInt(Limit),
                new TLInt(0),
                result =>
                {
                    CacheService.AddChats(result.Chats, results => { });
                    CacheService.AddUsers(result.Users, results => { });

                    IsWorking = false;

                    if (text != _lastText) return;

                    _offset += Limit;
                    _isLastSliceLoaded = result.Messages.Count < Limit;

                    var dialogs = new List<TLDialog>(result.Messages.Count);
                    var newMessages = result as TLMessages;
                    if (newMessages != null)
                    {
                        foreach (var message in newMessages.Messages.OfType<TLMessageCommon>())
                        {
                            if (cache != null
                                && cache.ContainsKey(message.Index)) continue;

                            var dialog = new TLDialog { TopMessage = message };
                            var peer = TLUtils.GetPeerFromMessage(message);
                            if (peer is TLPeerUser)
                            {
                                var user = newMessages.Users.FirstOrDefault(x => x.Index == peer.Id.Value);
                                if (user == null)
                                {
                                    continue;
                                }
                                dialog.With = user;
                            }
                            else if (peer is TLPeerChat)
                            {
                                var chat = newMessages.Chats.FirstOrDefault(x => x.Index == peer.Id.Value);
                                if (chat == null)
                                {
                                    continue;
                                }

                                dialog.With = chat;
                            }
                            dialogs.Add(dialog);
                            //LazyItems.Add(dialog);
                        }
                    }

                    if (Items.Count == 0 && dialogs.Count == 0 && LazyItems.Count == 0) Status = AppResources.NoResults;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (var dialog in dialogs)
                        {
                            Items.Add(dialog);
                        }
                    });
                    //Deployment.Current.Dispatcher.BeginInvoke(PopulateItems);

                },
                error =>
                {
                    IsWorking = false;
                });
        }

        public void Handle(DownloadableItem item)
        {
            var photo = item.Owner as TLUserProfilePhoto;
            if (photo != null)
            {
                var dialog = Items.FirstOrDefault(x => x.With is TLUserBase && ((TLUserBase)x.With).Photo == photo);
                if (dialog != null)
                {
                    dialog.NotifyOfPropertyChange(() => dialog.With);
                }
                return;
            }

            var chatPhoto = item.Owner as TLChatPhoto;
            if (chatPhoto != null)
            {
                var dialog = Items.FirstOrDefault(x => x.With is TLChat && ((TLChat)x.With).Photo == chatPhoto);
                if (dialog != null)
                {
                    dialog.NotifyOfPropertyChange(() => dialog.With);
                }
                return;
            }
        }
    }
}
