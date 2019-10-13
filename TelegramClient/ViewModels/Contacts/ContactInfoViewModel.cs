// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows;
using Caliburn.Micro;
using Org.BouncyCastle.Security;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Contacts
{
    public class ContactInfoViewModel : TelegramPropertyChangedBase
    {
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(propertyName);
            return true;
        }

        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(selectorExpression);
            return true;
        }
        
        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            set { SetField(ref _isWorking, value, () => IsWorking); }
        }

        public TLUserBase CurrentItem { get; protected set; }

        public TLString ContactPhone { get; protected set; }

        private readonly IStateService _stateService;

        public IStateService StateService
        {
            get { return _stateService; }
        }

        private readonly INavigationService _navigationService;

        private readonly IMTProtoService _mtProtoService;

        private readonly ICacheService _cacheService;

        private readonly ITelegramEventAggregator _eventAggregator; 

        private TimerSpan _selectedSpan;

        public TimerSpan SelectedSpan
        {
            get { return _selectedSpan; }
            set
            {
                _selectedSpan = value;

                if (_selectedSpan != null)
                {
                    if (_selectedSpan.Seconds == 0
                        || _selectedSpan.Seconds == int.MaxValue)
                    {
                        MuteUntil = _selectedSpan.Seconds;
                    }
                    else
                    {
                        var now = DateTime.Now;
                        var muteUntil = now.AddSeconds(_selectedSpan.Seconds);

                        MuteUntil = muteUntil < now ? 0 : TLUtils.DateToUniversalTimeTLInt(_mtProtoService.ClientTicksDelta, muteUntil).Value;
                    }
                }
            }
        }

        public IList<TimerSpan> Spans { get; protected set; }

        private int _muteUntil;

        public int MuteUntil
        {
            get { return _muteUntil; }
            set { SetField(ref _muteUntil, value, () => MuteUntil); }
        }

        private string _selectedSound;

        public string SelectedSound
        {
            get { return _selectedSound; }
            set { SetField(ref _selectedSound, value, () => SelectedSound); }
        }

        public List<string> Sounds { get; protected set; }

        private string _subtitle;

        public string Subtitle
        {
            get { return _subtitle; }
            set { SetField(ref _subtitle, value, () => Subtitle); }
        }

        public ContactInfoViewModel(IMTProtoService mtProtoService, ICacheService cacheService, IStateService stateService, ITelegramEventAggregator eventAggregator, INavigationService navigationService)
        {
            Spans = new List<TimerSpan>
            {
                new TimerSpan(AppResources.Enabled, string.Empty, 0, AppResources.Enabled),
                new TimerSpan(AppResources.HourNominativeSingular,  "1", (int)TimeSpan.FromHours(1.0).TotalSeconds, string.Format(AppResources.MuteFor, string.Format("{0} {1}", "1", AppResources.HourNominativeSingular).ToLowerInvariant())),
                new TimerSpan(AppResources.HourGenitivePlural, "8", (int)TimeSpan.FromHours(8.0).TotalSeconds, string.Format(AppResources.MuteFor, string.Format("{0} {1}", "8", AppResources.HourGenitivePlural).ToLowerInvariant())),
                new TimerSpan(AppResources.DayNominativePlural, "2", (int)TimeSpan.FromDays(2.0).TotalSeconds, string.Format(AppResources.MuteFor, string.Format("{0} {1}", "2", AppResources.DayNominativePlural).ToLowerInvariant())),
                new TimerSpan(AppResources.Disabled, string.Empty, int.MaxValue, AppResources.Disabled),
            };
            _selectedSpan = Spans[0];


            if (stateService.CurrentContact == null)
            {
                ShellViewModel.Navigate(navigationService);
                return;
            }

            CurrentItem = stateService.CurrentContact;
            stateService.CurrentContact = null;

            ContactPhone = stateService.CurrentContactPhone;
            stateService.CurrentContactPhone = null;

            _cacheService = cacheService;
            _eventAggregator = eventAggregator;
            _mtProtoService = mtProtoService;
            _stateService = stateService;
            _navigationService = navigationService;
        }

        #region Secret chats

        public Visibility ProgressVisibility { get { return IsWorking ? Visibility.Visible : Visibility.Collapsed; } }

        private TLString _a;
        private TLInt _g;
        private TLString _p;
        private TLString _ga;
        private volatile bool _invokeDelayedUserAction;

        public void CreateSecretChat()
        {
            var user = CurrentItem as TLUserBase;
            if (user == null) return;

            if (_a == null
                || _g == null
                || _p == null
                || _ga == null)
            {
                IsWorking = true;
                NotifyOfPropertyChange(() => ProgressVisibility);
                _invokeDelayedUserAction = true;
                return;
            }

            IsWorking = false;
            NotifyOfPropertyChange(() => ProgressVisibility);

            var random = new Random();
            var randomId = random.Next();

            _mtProtoService.RequestEncryptionAsync(user.ToInputUser(), new TLInt(randomId), _ga,
                encryptedChat =>
                {
                    var chatWaiting = encryptedChat as TLEncryptedChatWaiting;
                    if (chatWaiting == null) return;

                    chatWaiting.A = _a;
                    chatWaiting.P = _p;
                    chatWaiting.G = _g;

                    StateService.With = chatWaiting;
                    StateService.Participant = user;

                    Execute.BeginOnUIThread(() =>
                    {
                        StateService.RemoveBackEntries = true;
                        _navigationService.UriFor<SecretDialogDetailsViewModel>().Navigate();
                    });

                    _cacheService.SyncEncryptedChat(chatWaiting, result => _eventAggregator.Publish(result));

                    var message = new TLDecryptedMessageService17
                    {
                        RandomId = TLLong.Random(),
                        //RandomBytes = TLString.Random(Telegram.Api.Constants.MinRandomBytesLength),
                        ChatId = chatWaiting.Id,
                        Action = new TLDecryptedMessageActionEmpty(),
                        FromId = new TLInt(StateService.CurrentUserId),
                        Date = chatWaiting.Date,
                        Out = new TLBool(false),
                        Unread = new TLBool(false),
                        Status = MessageStatus.Read
                    };

                    _cacheService.SyncDecryptedMessage(message, chatWaiting, result => { });
                },
                error =>
                {
                    Execute.ShowDebugMessage("messages.requestEncryption error: " + error);
                });
        }

        private void CalculateSecretChatParamsAsync()
        {
            _mtProtoService.GetDHConfigAsync(new TLInt(0), new TLInt(0),
                result =>
                {
                    var dhConfig = (TLDHConfig)result;
                    if (!TLUtils.CheckPrime(dhConfig.P.Data, dhConfig.G.Value))
                    {
                        return;
                    }

                    var aBytes = new byte[256];
                    var random = new SecureRandom();
                    random.NextBytes(aBytes);
                    _a = TLString.FromBigEndianData(aBytes);
                    _p = dhConfig.P;
                    _g = dhConfig.G;
#if DEBUG
                    //Thread.Sleep(15000);
#endif
                    var gaBytes = MTProtoService.GetGB(aBytes, dhConfig.G, dhConfig.P);
                    _ga = TLString.FromBigEndianData(gaBytes);
                    if (_invokeDelayedUserAction)
                    {
                        _invokeDelayedUserAction = false;
                        CreateSecretChat();
                    }

                },
                error =>
                {
                    Execute.ShowDebugMessage("messages.getDhConfig error: " + error);
                });
        }
        #endregion
    }
}
