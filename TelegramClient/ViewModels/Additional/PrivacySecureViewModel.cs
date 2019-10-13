// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Passport;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class PrivacySecurityViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<TLUpdateUserBlocked>, Telegram.Api.Aggregator.IHandle<TLUpdatePrivacy>
    {
        private bool _isSecretChatsEnabled;

        public bool IsSecretChatsEnabled
        {
            get { return _isSecretChatsEnabled; }
            set { SetField(ref _isSecretChatsEnabled, value, () => IsSecretChatsEnabled); }
        }

        private int _blockedUsersCount;

        private string _blockedUsersSubtitle = " ";

        public string BlockedUsersSubtitle
        {
            get { return _blockedUsersSubtitle; }
            set { SetField(ref _blockedUsersSubtitle, value, () => BlockedUsersSubtitle); }
        }

        private TLPrivacyRules _lastSeenPrivacyRules;

        private string _lastSeenSubtitle = " ";

        public string LastSeenSubtitle
        {
            get { return _lastSeenSubtitle; }
            set { SetField(ref _lastSeenSubtitle, value, () => LastSeenSubtitle); }
        }

        private TLPrivacyRules _phoneCallsPrivacyRules;

        private string _phoneCallsSubtitle = " ";

        public string PhoneCallsSubtitle
        {
            get { return _phoneCallsSubtitle; }
            set { SetField(ref _phoneCallsSubtitle, value, () => PhoneCallsSubtitle); }
        }

        private TLPrivacyRules _chatInvitePrivacyRules;

        private string _groupsSubtitle = " ";

        public string GroupsSubtitle
        {
            get { return _groupsSubtitle; }
            set { SetField(ref _groupsSubtitle, value, () => GroupsSubtitle); }
        }

        private int _accountDaysTTL;

        private string _accountSelfDestructsSubtitle = " ";

        public string AccountSelfDestructsSubtitle
        {
            get { return _accountSelfDestructsSubtitle; }
            set { SetField(ref _accountSelfDestructsSubtitle, value, () => AccountSelfDestructsSubtitle); }
        }

        public PrivacySecurityViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);
        }

        private bool _secretChatsEnabled;

        protected override void OnActivate()
        {
            base.OnActivate();

            MTProtoService.GetBlockedAsync(new TLInt(0), new TLInt(int.MaxValue),
                result =>
                {
                    var contacts = result as TLContactsBlocked;
                    if (contacts != null)
                    {
                        var count = contacts.Blocked.Count;

                        UpdateBlockedUsersString(count);
                    }
                },
                error => Execute.ShowDebugMessage("contacts.getBlocked error " + error));

            MTProtoService.GetAccountTTLAsync(
                result =>
                {
                    var days = result.Days.Value;

                    UpdateAccountTTLString(days);
                },
                error => Execute.ShowDebugMessage("account.getAccountTTL error " + error));

            MTProtoService.GetPrivacyAsync(new TLInputPrivacyKeyStatusTimestamp(),
                result =>
                {
                    LastSeenSubtitle = GetPrivacyString(result, out _lastSeenPrivacyRules);
                },
                error => Execute.ShowDebugMessage("account.getPrivacy error " + error));

            MTProtoService.GetPrivacyAsync(new TLInputPrivacyKeyPhoneCall(),
                result =>
                {
                    PhoneCallsSubtitle = GetPrivacyString(result, out _phoneCallsPrivacyRules);
                },
                error => Execute.ShowDebugMessage("account.getPrivacy error " + error));

            MTProtoService.GetPrivacyAsync(new TLInputPrivacyKeyChatInvite(),
                result =>
                {
                    GroupsSubtitle = GetPrivacyString(result, out _chatInvitePrivacyRules);
                },
                error => Execute.ShowDebugMessage("account.getPrivacy error " + error));

            Execute.BeginOnThreadPool(() =>
            {
                var isSecretChatEnabled = TLUtils.OpenObjectFromMTProtoFile<TLBool>(SecretChatsViewModel.LinkPreviewsSyncRoot, Constants.WebPagePreviewsFileName);

                if (isSecretChatEnabled == null || !isSecretChatEnabled.Value)
                {
                    _secretChatsEnabled = true;
                    IsSecretChatsEnabled = true;
                }
                else
                {
                    _secretChatsEnabled = false;
                }
            });
        }

        private string GetPrivacyString(TLPrivacyRules rules, out TLPrivacyRules currentRules)
        {
            currentRules = rules;

            TLPrivacyRuleBase mainRule = null;
            var mainRuleString = string.Empty;
            var minusCount = 0;
            var plusCount = 0;
            foreach (var rule in rules.Rules)
            {
                if (rule is TLPrivacyValueAllowAll)
                {
                    mainRule = rule;
                    mainRuleString = AppResources.Everybody;
                }

                if (rule is TLPrivacyValueAllowContacts)
                {
                    mainRule = rule;
                    mainRuleString = AppResources.MyContacts;
                }

                if (rule is TLPrivacyValueDisallowAll)
                {
                    mainRule = rule;
                    mainRuleString = AppResources.Nobody;
                }

                if (rule is TLPrivacyValueDisallowUsers)
                {
                    minusCount += ((TLPrivacyValueDisallowUsers)rule).Users.Count;
                }

                if (rule is TLPrivacyValueAllowUsers)
                {
                    plusCount += ((TLPrivacyValueAllowUsers)rule).Users.Count;
                }
            }

            if (mainRule == null)
            {
                mainRule = new TLPrivacyValueDisallowAll();
                mainRuleString = AppResources.Nobody;
            }

            var countStrings = new List<string>();
            if (minusCount > 0)
            {
                countStrings.Add("-" + minusCount);
            }
            if (plusCount > 0)
            {
                countStrings.Add("+" + plusCount);
            }
            if (countStrings.Count > 0)
            {
                mainRuleString += string.Format(" ({0})", string.Join(", ", countStrings));
            }

            return mainRuleString;
        }

        public void OpenBlockedUsers()
        {
            NavigationService.UriFor<BlockedContactsViewModel>().Navigate();
        }

        private void UpdateBlockedUsersString(int count)
        {
            _blockedUsersCount = count;
            if (count == 0)
            {
                BlockedUsersSubtitle = AppResources.NoUsers;
            }
            else if (count > 0)
            {
                BlockedUsersSubtitle = Language.Declension(
                    count,
                    AppResources.UserNominativeSingular,
                    AppResources.UserNominativePlural,
                    AppResources.UserGenitiveSingular,
                    AppResources.UserGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
            }
        }

        private void UpdateAccountTTLString(int days)
        {
            _accountDaysTTL = days;
            if (days >= 365)
            {
                var years = days / 365;
                var yearsString = Language.Declension(
                    years,
                    AppResources.YearNominativeSingular,
                    AppResources.YearNominativePlural,
                    AppResources.YearGenitiveSingular,
                    AppResources.YearGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                AccountSelfDestructsSubtitle = string.Format("{0} {1}", AppResources.IfYouAreAwayFor, yearsString);
            }
            else
            {
                var months = days / 30;

                var monthsString = Language.Declension(
                    months,
                    AppResources.MonthNominativeSingular,
                    AppResources.MonthNominativePlural,
                    AppResources.MonthGenitiveSingular,
                    AppResources.MonthGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
                AccountSelfDestructsSubtitle = string.Format("{0} {1}", AppResources.IfYouAreAwayFor, monthsString);
            }
        }

        public void Sessions()
        {
            NavigationService.UriFor<SessionsViewModel>().Navigate();
        }

        public void LoggedIn()
        {
            NavigationService.UriFor<LoggedInViewModel>().Navigate();
        }

        public void Contacts()
        {
            StateService.GetContactsSettings();
            NavigationService.UriFor<ContactsSecurityViewModel>().Navigate();
        }

        public void LastSeen()
        {
            StateService.PrivacyRules = _lastSeenPrivacyRules;
            NavigationService.UriFor<LastSeenViewModel>().Navigate();
        }

        public void CallsPrivacy()
        {
            StateService.PrivacyRules = _phoneCallsPrivacyRules;
            NavigationService.UriFor<CallsPrivacyViewModel>().Navigate();
        }

        public void CallsSecurity()
        {
            CacheService.GetConfigAsync(
                config =>
                {
                    var config82 = config as TLConfig82;
                    var defaultP2PContacts = config82 == null || config82.DefaultP2PContacts;
                    StateService.GetCallsSecurity(defaultP2PContacts);

                    NavigationService.UriFor<CallsSecurityViewModel>().Navigate();
                });
        }

        public void SecretChats()
        {
            StateService.LinkPreviews = !_secretChatsEnabled;
            NavigationService.UriFor<SecretChatsViewModel>().Navigate();
        }

        public void Groups()
        {
            StateService.PrivacyRules = _chatInvitePrivacyRules;
            NavigationService.UriFor<GroupsViewModel>().Navigate();
        }

        public void AccountSelfDestructs()
        {
            StateService.AccountDaysTTL = _accountDaysTTL;
            NavigationService.UriFor<AccountSelfDestructsViewModel>().Navigate();
        }

        public void Passcode()
        {
            if (!PasscodeUtils.IsEnabled)
            {
                NavigationService.UriFor<PasscodeViewModel>().Navigate();
            }
            else
            {
                NavigationService.UriFor<EnterPasscodeViewModel>().Navigate();
            }
        }

        public void TwoStepVerification()
        {
            if (IsWorking) return;

            IsWorking = true;
            MTProtoService.GetPasswordAsync(
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (result.HasPassword)
                    {
                        StateService.Password = result;
                        NavigationService.UriFor<EnterPasswordViewModel>().Navigate();
                        return;
                    }

                    StateService.Password = result;
                    NavigationService.UriFor<PasswordViewModel>().Navigate();
                }),
                error =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("account.getPassword error " + error);
                });
        }

        public void Payments()
        {
            var content = new StackPanel();

            var shippingInfo = new CheckBox { Content = AppResources.ShippingInfo, IsChecked = true };
            var paymentInfo = new CheckBox { Content = AppResources.PaymentInfo, IsChecked = true };

            content.Children.Add(shippingInfo);
            content.Children.Add(paymentInfo);

            ShellViewModel.ShowCustomMessageBox(string.Empty, string.Empty, AppResources.Clear.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                r =>
                {
                    if (r == CustomMessageBoxResult.RightButton)
                    {
                        var credentials = paymentInfo.IsChecked.HasValue && paymentInfo.IsChecked.Value;
                        var info = shippingInfo.IsChecked.HasValue && shippingInfo.IsChecked.Value;

                        if (!info && !credentials) return;

                        IsWorking = true;
                        MTProtoService.ClearSavedInfoAsync(credentials, info,
                            result => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                            }));
                    }
                },
                content);
        }

        public void Passport()
        {
            if (IsWorking) return;

            var passportConfig = StateService.GetPassportConfig();
            var passportConfigHash = passportConfig != null ? passportConfig.Hash : new TLInt(0);

            MTProtoService.GetPassportDataAsync(
                (result1, result2) => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (result1.HasPassword)
                    {
                        StateService.Password = result1;
                        StateService.SecureValues = result2;
                        NavigationService.UriFor<Passport.EnterPasswordViewModel>().Navigate();
                        return;
                    }

                    if (!result1.HasPassword)
                    {
                        if (!TLString.IsNullOrEmpty(result1.EmailUnconfirmedPattern))
                        {
                            StateService.Password = result1;
                            StateService.SecureValues = result2;
                            NavigationService.UriFor<PasswordViewModel>().Navigate();
                        }
                        else
                        {
                            StateService.Password = result1;
                            StateService.SecureValues = result2;
                            NavigationService.UriFor<PasswordIntroViewModel>().Navigate();
                        }
                        return;
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false; 
                    Execute.ShowDebugMessage("passport.container error " + error);
                }));
        }

        public void DeleteAllCloudDrafts()
        {
            ShellViewModel.ShowCustomMessageBox(
                AppResources.DeleteAllCloudDraftsConfirmation, AppResources.AppName,
                AppResources.Delete, AppResources.Cancel,
                dismissed =>
                {
                    MTProtoService.ClearAllDraftsAsync(
                        result => Execute.BeginOnUIThread(() =>
                        {
                            var dialogs = CacheService.GetDialogs();
                            foreach (var dialogBase in dialogs)
                            {
                                var dialog = dialogBase as TLDialog71;
                                if (dialog != null)
                                {
                                    dialog.Draft = null;
                                    dialog.NotifyOfPropertyChange(() => dialog.Draft);
                                }
                            }
                        }));
                });
        }

        public void Handle(TLUpdateUserBlocked update)
        {
            var count = _blockedUsersCount;
            if (update.Blocked.Value)
            {
                _blockedUsersCount++;
                count++;
            }
            else if (count > 0)
            {
                _blockedUsersCount--;
                count--;
            }

            UpdateBlockedUsersString(count);
        }

        public void Handle(TLUpdatePrivacy privacy)
        {
            var result = new TLPrivacyRules { Rules = privacy.Rules };
            if (privacy.Key is TLPrivacyKeyStatusTimestamp)
            {
                LastSeenSubtitle = GetPrivacyString(result, out _lastSeenPrivacyRules);
            }
            else if (privacy.Key is TLPrivacyKeyPhoneCall)
            {
                PhoneCallsSubtitle = GetPrivacyString(result, out _phoneCallsPrivacyRules);
            }
            else if (privacy.Key is TLPrivacyKeyChatInvite)
            {
                GroupsSubtitle = GetPrivacyString(result, out _chatInvitePrivacyRules);
            }
            else
            {
                Execute.ShowDebugMessage("PrivacySecureViewModel.Handle TLUpdatePrivacy unknown key=" + privacy.Key);
            }
        }
    }
}
