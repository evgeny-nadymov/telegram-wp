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
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Contacts
{
    public class SecretContactDetailsViewModel : TelegramPropertyChangedBase, Telegram.Api.Aggregator.IHandle<TLEncryptedChatBase>
    {
        public Visibility VoiceCallVisibility
        {
            get
            {
                var user = CurrentItem as TLUser;
                if (user != null && !user.IsSelf && !user.IsBot)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public string GroupsInCommonSubtitle
        {
            get
            {
                var user = CurrentItem as TLUser45;
                if (user != null && user.CommonChatsCount != null && user.CommonChatsCount.Value > 0)
                {
                    return Utils.Language.Declension(
                    user.CommonChatsCount.Value,
                    AppResources.GroupNominativeSingular,
                    AppResources.GroupNominativePlural,
                    AppResources.GroupGenitiveSingular,
                    AppResources.GroupGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                }

                return string.Empty;
            }
        }

        public Visibility GroupsInCommonVisibility
        {
            get
            {
                var user = CurrentItem as TLUser45;
                if (user != null && user.CommonChatsCount != null && user.CommonChatsCount.Value > 0)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public string DisplayName { get { return AppResources.Info.ToLowerInvariant(); } }

        public TLUserBase CurrentItem { get; set; }

        private readonly IStateService _stateService;
        private readonly INavigationService _navigationService;
        private readonly ITelegramEventAggregator _eventAggregator;
        private readonly IMTProtoService _mtProtoService;
        private readonly ICacheService _cacheService;

        public TLString Key { get; set; }

        private TimerSpan _selectedSpan;

        public TimerSpan SelectedSpan
        {
            get { return _selectedSpan; }
            set
            {
                if (value != _selectedSpan)
                {
                    _selectedSpan = value;
                    NotifyOfPropertyChange(() => SelectedSpan);
                    SetTimerSpan(_selectedSpan);
                }
            }
        }

        public IList<TimerSpan> TimerSpans { get; protected set; } 

        private TLEncryptedChatBase _chat;

        public TLEncryptedChatBase Chat
        {
            get { return _chat; }
            set
            {
                _chat = value;
                SetChatTTL(_chat);
            }
        }

        public Visibility EncryptionKeyVisibility
        {
            get { return Chat is TLEncryptedChat ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string Subtitle
        {
            get
            {
                if (CurrentItem == null) return null;

                return UserStatusToStringConverter.Convert(CurrentItem.Status);
            }
        }

        public SecretContactDetailsViewModel(IMTProtoService mtProtoService, IStateService stateService, INavigationService navigationService, ITelegramEventAggregator eventAggregator, ICacheService cacheService)
        {
            _stateService = stateService;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;
            _mtProtoService = mtProtoService;
            _cacheService = cacheService;
            
            TimerSpans = new List<TimerSpan>
            {
                new TimerSpan(AppResources.OffMasculine, string.Empty, 0),
                new TimerSpan(AppResources.SecondNominativeSingular,  "1", 1),
                new TimerSpan(AppResources.SecondNominativePlural, "2", 2),
                new TimerSpan(AppResources.SecondNominativePlural, "3", 3),
                new TimerSpan(AppResources.SecondNominativePlural, "4", 4),
                new TimerSpan(AppResources.SecondGenitivePlural, "5", 5),
                new TimerSpan(AppResources.SecondGenitivePlural, "6", 6),
                new TimerSpan(AppResources.SecondGenitivePlural, "7", 7),
                new TimerSpan(AppResources.SecondGenitivePlural, "8", 8),
                new TimerSpan(AppResources.SecondGenitivePlural, "9", 9),
                new TimerSpan(AppResources.SecondGenitivePlural, "10", 10),
                new TimerSpan(AppResources.SecondGenitivePlural, "11", 11),
                new TimerSpan(AppResources.SecondGenitivePlural, "12", 12),
                new TimerSpan(AppResources.SecondGenitivePlural, "13", 13),
                new TimerSpan(AppResources.SecondGenitivePlural, "14", 14),
                new TimerSpan(AppResources.SecondGenitivePlural, "15", 15),
                new TimerSpan(AppResources.SecondGenitivePlural, "30", 30),
                new TimerSpan(AppResources.MinuteNominativeSingular, "1", 60),
                new TimerSpan(AppResources.HourNominativeSingular, "1", (int) TimeSpan.FromHours(1.0).TotalSeconds),
                new TimerSpan(AppResources.DayNominativeSingular, "1", (int) TimeSpan.FromDays(1.0).TotalSeconds),
                new TimerSpan(AppResources.WeekNominativeSingular, "1", (int) TimeSpan.FromDays(7.0).TotalSeconds),
            };

            _selectedSpan = TimerSpans.First();

            _eventAggregator.Subscribe(this);
        }

        public void Handle(TLEncryptedChatBase encryptedChat)
        {
            if (encryptedChat != null
                && Chat != null
                && encryptedChat.Id.Value == Chat.Id.Value)
            {
                Chat = encryptedChat;
                NotifyOfPropertyChange(() => EncryptionKeyVisibility);
            }
        }

        private void SetChatTTL(TLEncryptedChatBase chat)
        {
            if (chat != null && chat.MessageTTL != null)
            {
                var selectedSpan = TimerSpans.FirstOrDefault(x => x.Seconds == Chat.MessageTTL.Value);
                if (selectedSpan != null)
                {
                    _selectedSpan = selectedSpan;
                }
                else
                {
                    _selectedSpan = TimerSpans.First();
                }
            }
            else
            {
                _selectedSpan = TimerSpans.First();
            }
        }

        private void SetTimerSpan(TimerSpan selectedSpan)
        {
            var chat = Chat as TLEncryptedChat;
            if (chat == null) return;

            var action = new TLDecryptedMessageActionSetMessageTTL();
            action.TTLSeconds = new TLInt(selectedSpan.Seconds);

            var decryptedTuple = SecretDialogDetailsViewModel.GetDecryptedServiceMessageAndObject(action, chat, _mtProtoService.CurrentUserId, _cacheService);

            SecretDialogDetailsViewModel.SendEncryptedService(chat, decryptedTuple.Item2, _mtProtoService, _cacheService,
                result =>
                {
                    _eventAggregator.Publish(new SetMessagesTTLEventArgs { Chat = Chat, Message = decryptedTuple.Item1 });
                    Chat.MessageTTL = new TLInt(selectedSpan.Seconds);
                });
        }

        public void OpenEncryptionKey()
        {
            if (Key == null) return;

            _stateService.CurrentKey = Key;
            _stateService.CurrentContact = CurrentItem;
            _stateService.CurrentEncryptedChat = Chat;
            _navigationService.UriFor<EncryptionKeyViewModel>().Navigate();
        }

        public void OpenGroupsInCommon()
        {
            if (CurrentItem == null) return;

            _stateService.CurrentContact = CurrentItem;
            _navigationService.UriFor<GroupsInCommonViewModel>().Navigate();
        }

        public void StartVoiceCall()
        {
            ShellViewModel.StartVoiceCall(CurrentItem as TLUser, IoC.Get<IVoIPService>(), IoC.Get<ICacheService>());
        }

        public void Call()
        {
            var user = CurrentItem;

            if (user == null || user.Phone == null) return;

            var task = new PhoneCallTask();
            task.DisplayName = user.FullName;
            task.PhoneNumber = "+" + user.Phone;
            task.Show();
        }

        public void SelectTimerSpan()
        {
            _stateService.IsEncryptedTimer = true;
            _stateService.SelectedTimerSpan = SelectedSpan;
            _navigationService.UriFor<ChooseTTLViewModel>().Navigate();
        }
    }

    public class TimerSpan
    {
        public string SpanName { get; protected set; }

        public string SpanNumber { get; protected set; }

        public int Seconds { get; protected set; }

        public string Description { get; protected set; }

        public TimerSpan(string caption, string countCaption, int seconds, string description = null)
        {
            SpanName = caption;
            SpanNumber = countCaption;
            Seconds = seconds;
            Description = description;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", SpanNumber, SpanName);
        }
    }

    public class SetMessagesTTLEventArgs
    {
        public TLEncryptedChatBase Chat { get; set; }

        public TLDecryptedMessageService Message { get; set; }
    }
}
