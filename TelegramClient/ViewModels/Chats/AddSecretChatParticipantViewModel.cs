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
using Caliburn.Micro;
using Org.BouncyCastle.Security;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Logs;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Chats
{
    public class AddSecretChatParticipantViewModel : ItemsViewModelBase<TLUserBase>
    {
        public bool IsNotWorking { get { return !IsWorking; } }

        public Visibility ProgressVisibility { get { return IsWorking ? Visibility.Visible : Visibility.Collapsed; } }

        public AddSecretChatParticipantViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (isAuthorized)
                {
                    var contacts = CacheService.GetContacts()
                        .Where(x => !(x is TLUserEmpty) && x.Index != StateService.CurrentUserId)
                        .OrderBy(x => x.FullName)
                        .ToList();
                    
                    Status = string.Empty;

                    var count = 0;
                    const int firstSliceCount = 10;
                    var secondSlice = new List<TLUserBase>();
                    foreach (var contact in contacts)
                    {
                        if (count < firstSliceCount)
                        {
                            LazyItems.Add(contact);
                        }
                        else
                        {
                            secondSlice.Add(contact);
                        }
                        count++;
                    }

                    BeginOnUIThread(() => PopulateItems(() =>
                    {
                        foreach (var item in secondSlice)
                        {
                            Items.Add(item);
                        }
                    }));
                }

                GetDHConfig();
            });
        }

        private volatile bool _isGettingConfig;

        public void GetDHConfig()
        {
            if (_dhConfig != null) return;

            _isGettingConfig = true;
            MTProtoService.GetDHConfigAsync(new TLInt(0), new TLInt(0),
                result =>
                {
                    var dhConfig = result as TLDHConfig;
                    if (dhConfig == null) return;
                    if (!TLUtils.CheckPrime(dhConfig.P.Data, dhConfig.G.Value))
                    {
                        return;
                    }

                    var aBytes = new byte[256];
                    var random = new SecureRandom();
                    random.NextBytes(aBytes);
                    
                    var gaBytes = Telegram.Api.Services.MTProtoService.GetGB(aBytes, dhConfig.G, dhConfig.P);

                    dhConfig.A = TLString.FromBigEndianData(aBytes);
                    dhConfig.GA = TLString.FromBigEndianData(gaBytes);

                    _isGettingConfig = false;

                    Execute.BeginOnUIThread(() =>
                    {
                        _dhConfig = dhConfig;

                        if (_contact != null)
                        {
                            UserAction(_contact);
                        }
                    });
                },
                error =>
                {
                    _isGettingConfig = false;

                    IsWorking = false;
                    NotifyOfPropertyChange(() => IsNotWorking);
                    NotifyOfPropertyChange(() => ProgressVisibility);
                    Execute.ShowDebugMessage("messages.getDhConfig error: " + error);
                }); 
        }

        protected override void OnActivate()
        {
            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            base.OnActivate();
        }

        private TLDHConfig _dhConfig;
        private TLUserBase _contact;

        public TLUserBase Contact { get { return _contact; } }

        public void CancelSecretChat()
        {
            _contact = null;

            IsWorking = false;
            NotifyOfPropertyChange(() => IsNotWorking);
            NotifyOfPropertyChange(() => ProgressVisibility);
        }

        public void UserAction(TLUserBase user)
        {
            if (user == null) return;

            if (_dhConfig == null)
            {
                IsWorking = true;
                NotifyOfPropertyChange(() => IsNotWorking);
                NotifyOfPropertyChange(() => ProgressVisibility);
                _contact = user;

                //try to get dhConfig once again
                if (!_isGettingConfig)
                {
                    GetDHConfig();
                }

                return;
            }

            IsWorking = false;
            NotifyOfPropertyChange(() => IsNotWorking);
            NotifyOfPropertyChange(() => ProgressVisibility);

            _contact = null;
            CreateSecretChatCommon(user, _dhConfig, MTProtoService, CacheService, NavigationService, StateService, EventAggregator);
        }

        public static void CreateSecretChatCommon(TLUserBase user, TLDHConfig dhConfig, IMTProtoService mtProtoService, ICacheService cacheService, INavigationService navigationService, IStateService stateService, ITelegramEventAggregator eventAggregator)
        {
            if (user == null) return;

            var random = new Random();
            var randomId = random.Next();

            mtProtoService.RequestEncryptionAsync(user.ToInputUser(), new TLInt(randomId), dhConfig.GA,
                encryptedChat =>
                {
                    var chatWaiting = encryptedChat as TLEncryptedChatWaiting;

                    if (chatWaiting != null)
                    {
                        var action = new TLMessageActionChatCreate();
                        action.Title = TLString.Empty;
                        action.Users = new TLVector<TLInt> { Items = new List<TLInt> { user.Id } };

                        var dialog = new TLDialog53();
                        dialog.Flags = new TLInt(0);
                        dialog.ReadOutboxMaxId = new TLInt(0);
                        dialog.ReadInboxMaxId = new TLInt(0);
                        dialog.With = user;
                        dialog.Peer = new TLPeerUser { Id = user.Id };

                        var topMessage = new TLMessageService17();
                        topMessage.Date = chatWaiting.Date;
                        topMessage.FromId = chatWaiting.AdminId;
                        topMessage.ToId = new TLPeerUser { Id = user.Id };
                        topMessage.Out = new TLBool(true);
                        topMessage.Action = action;
                        topMessage.SetUnread(new TLBool(false));

                        chatWaiting.A = dhConfig.A;
                        chatWaiting.P = dhConfig.P;
                        chatWaiting.G = dhConfig.G;
                    }

                    stateService.RemoveBackEntry = true;
                    stateService.With = chatWaiting;
                    stateService.Participant = user;

                    Execute.BeginOnUIThread(() =>
                    {
                        stateService.AnimateTitle = true;
                        navigationService.UriFor<SecretDialogDetailsViewModel>().Navigate();
                    });


                    //syncing chat and message
                    cacheService.SyncEncryptedChat(chatWaiting, eventAggregator.Publish);

                    var message = new TLDecryptedMessageService17
                    {
                        RandomId = TLLong.Random(),
                        //RandomBytes = TLString.Random(Telegram.Api.Constants.MinRandomBytesLength),
                        ChatId = chatWaiting.Id,
                        Action = new TLDecryptedMessageActionEmpty(),
                        FromId = new TLInt(stateService.CurrentUserId),
                        Date = chatWaiting.Date,
                        Out = new TLBool(false),
                        Unread = new TLBool(false),
                        Status = MessageStatus.Read
                    };

                    cacheService.SyncDecryptedMessage(message, chatWaiting, result => { });
                },
                error =>
                {
                    Execute.ShowDebugMessage("messages.requestEncryption error " + error);
                });
        }

        public void Search()
        {
            StateService.RemoveBackEntry = true;
            StateService.NavigateToSecretChat = true;
            StateService.DHConfig = _dhConfig;
            NavigationService.UriFor<SearchContactsViewModel>().Navigate();
        }
    }
}
