// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;
using Language = TelegramClient.Utils.Language;

namespace TelegramClient.ViewModels.Additional
{
    public class GroupsViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<TLUpdatePrivacy>
    {
        private string _allowUsersSubtitle = AppResources.NoUsers;

        public string AllowUsersSubtitle
        {
            get { return _allowUsersSubtitle; }
            set { SetField(ref _allowUsersSubtitle, value, () => AllowUsersSubtitle); }
        }

        private Visibility _allowUsersVisibility;

        public Visibility AllowUsersVisibility
        {
            get { return _allowUsersVisibility; }
            set { SetField(ref _allowUsersVisibility, value, () => AllowUsersVisibility); }
        }

        private string _disallowUsersSubtitle = AppResources.NoUsers;

        public string DisallowUsersSubtitle
        {
            get { return _disallowUsersSubtitle; }
            set { SetField(ref _disallowUsersSubtitle, value, () => DisallowUsersSubtitle); }
        }

        private Visibility _disallowUsersVisibility;

        public Visibility DisallowUsersVisibility
        {
            get { return _disallowUsersVisibility; }
            set { SetField(ref _disallowUsersVisibility, value, () => DisallowUsersVisibility); }
        }

        private TLPrivacyRuleBase _selectedMainRule;

        public TLPrivacyRuleBase SelectedMainRule
        {
            get { return _selectedMainRule; }
            set { SetField(ref _selectedMainRule, value, () => SelectedMainRule); }
        }

        public List<TLPrivacyRuleBase> MainRules { get; set; }

        private TLPrivacyRules _rules;
        private TLPrivacyValueAllowUsers _selectedAllowUsers;
        private TLPrivacyValueDisallowUsers _selectedDisallowUsers;

        public GroupsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            _rules = StateService.PrivacyRules;
            StateService.PrivacyRules = null;

            MainRules = new List<TLPrivacyRuleBase>
            {
                new TLPrivacyValueAllowAll{ Label = AppResources.Everybody },
                new TLPrivacyValueAllowContacts{ Label = AppResources.MyContacts }
            };

            _selectedMainRule = GetSelectedMainRule(MainRules, _rules, MainRules[0], MainRules[1]);
            _selectedMainRule.IsChecked = true;
            _selectedAllowUsers = GetSelectedRule<TLPrivacyValueAllowUsers>(_rules) ?? new TLPrivacyValueAllowUsers { Users = new TLVector<TLInt>() };
            _selectedDisallowUsers = GetSelectedRule<TLPrivacyValueDisallowUsers>(_rules) ?? new TLPrivacyValueDisallowUsers { Users = new TLVector<TLInt>() };
            SwitchUsersVisibility(true);
            PropertyChanged += OnPropertyChanged;
        }

        protected override void OnActivate()
        {
            BeginOnThreadPool(() =>
            {
                if (StateService.UsersRule != null)
                {
                    var allowUsersRule = StateService.UsersRule as TLPrivacyValueAllowUsers;
                    if (allowUsersRule != null)
                    {
                        _selectedAllowUsers = allowUsersRule;

                        CleanupUsers(_selectedAllowUsers, _selectedDisallowUsers);
                    }
                    var disallowUsersRule = StateService.UsersRule as TLPrivacyValueDisallowUsers;
                    if (disallowUsersRule != null)
                    {
                        _selectedDisallowUsers = disallowUsersRule;

                        CleanupUsers(_selectedDisallowUsers, _selectedAllowUsers);
                    }
                    StateService.UsersRule = null;
                }

                UpdateSubtitles();
            });

            base.OnActivate();
        }

        private void CleanupUsers(IPrivacyValueUsersRule sourceList, IPrivacyValueUsersRule cleaningList)
        {
            var sourceDict = new Dictionary<int, int>();
            foreach (var userId in sourceList.Users)
            {
                sourceDict[userId.Value] = userId.Value;
            }

            for (var i = 0; i < cleaningList.Users.Count; i++)
            {
                if (sourceDict.ContainsKey(cleaningList.Users[i].Value))
                {
                    cleaningList.Users.RemoveAt(i--);
                }
            }
        }

        private void UpdateSubtitles()
        {
            var allowCount = _selectedAllowUsers.Users.Count;

            AllowUsersSubtitle = allowCount == 0
                ? AppResources.NoUsers
                : Language.Declension(
                    allowCount,
                    AppResources.UserNominativeSingular,
                    AppResources.UserNominativePlural,
                    AppResources.UserGenitiveSingular,
                    AppResources.UserGenitivePlural).ToLower(CultureInfo.CurrentUICulture);

            var disallowCount = _selectedDisallowUsers.Users.Count;

            DisallowUsersSubtitle = disallowCount == 0
                ? AppResources.NoUsers
                : Language.Declension(
                    disallowCount,
                    AppResources.UserNominativeSingular,
                    AppResources.UserNominativePlural,
                    AppResources.UserGenitiveSingular,
                    AppResources.UserGenitivePlural).ToLower(CultureInfo.CurrentUICulture);
        }

        private T GetSelectedRule<T>(TLPrivacyRules rules) where T : TLPrivacyRuleBase
        {
            T allowUsers = null;
            if (_rules != null)
            {
                allowUsers = (T)rules.Rules.FirstOrDefault(x => x is T);
            }

            return allowUsers;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => SelectedMainRule))
            {
                SwitchUsersVisibility(false);
            }
        }

        private void SwitchUsersVisibility(bool silent)
        {
            if (SelectedMainRule is TLPrivacyValueAllowAll)
            {
                _allowUsersVisibility = Visibility.Collapsed;
                _disallowUsersVisibility = Visibility.Visible;
            }

            if (SelectedMainRule is TLPrivacyValueAllowContacts)
            {
                _allowUsersVisibility = Visibility.Visible;
                _disallowUsersVisibility = Visibility.Visible;
            }

            if (SelectedMainRule is TLPrivacyValueDisallowAll)
            {
                _allowUsersVisibility = Visibility.Visible;
                _disallowUsersVisibility = Visibility.Collapsed;
            }

            if (!silent)
            {
                NotifyOfPropertyChange(() => AllowUsersVisibility);
                NotifyOfPropertyChange(() => DisallowUsersVisibility);
            }
        }

        public void OpenAllowUsers()
        {
            var allowUsersRule = _rules != null ? _rules.Rules.FirstOrDefault(x => x is TLPrivacyValueAllowUsers) : null;

            //if (allowUsersRule == null)
            //{
            //    NavigationService.UriFor<SelectMultipleUsersViewModel>().Navigate();
            //}
            //else
            {
                StateService.UsersRule = _selectedAllowUsers;
                NavigationService.UriFor<AllowUsersViewModel>().Navigate();
            }
        }

        public void OpenDisallowUsers()
        {
            var disallowUsersRule = _rules != null ? _rules.Rules.FirstOrDefault(x => x is TLPrivacyValueDisallowUsers) : null;

            //if (allowUsersRule == null)
            //{
            //    NavigationService.UriFor<SelectMultipleUsersViewModel>().Navigate();
            //}
            //else
            {
                StateService.UsersRule = _selectedDisallowUsers;
                NavigationService.UriFor<AllowUsersViewModel>().Navigate();
            }
        }

        private TLPrivacyRuleBase GetSelectedMainRule(List<TLPrivacyRuleBase> mainRules, TLPrivacyRules rules, TLPrivacyRuleBase defaultRule, TLPrivacyRuleBase noRules)
        {
            if (rules == null)
            {

            }
            else
            {
                foreach (var rule in rules.Rules)
                {
                    var mainRule = mainRules.FirstOrDefault(x => x.GetType() == rule.GetType());
                    if (mainRule != null)
                    {
                        return mainRule;
                    }
                }

                return noRules;
            }

            return defaultRule;
        }

        public void Done()
        {
            if (SelectedMainRule == null) return;

            var rules = new TLVector<TLInputPrivacyRuleBase>();

            if (_selectedDisallowUsers != null
                && _selectedDisallowUsers.Users != null
                && _selectedDisallowUsers.Users.Count > 0)
            {
                var inputDisallowUsers = (TLInputPrivacyValueDisallowUsers)_selectedDisallowUsers.ToInputRule();

                foreach (var userId in _selectedDisallowUsers.Users)
                {
                    var user = CacheService.GetUser(userId);

                    if (user != null)
                    {
                        inputDisallowUsers.Users.Add(user.ToInputUser());
                    }
                }

                rules.Add(inputDisallowUsers);
            }

            if (_selectedAllowUsers != null
                && _selectedAllowUsers.Users != null
                && _selectedAllowUsers.Users.Count > 0)
            {
                var inputAllowUsers = (TLInputPrivacyValueAllowUsers)_selectedAllowUsers.ToInputRule();

                foreach (var userId in _selectedAllowUsers.Users)
                {
                    var user = CacheService.GetUser(userId);

                    if (user != null)
                    {
                        inputAllowUsers.Users.Add(user.ToInputUser());
                    }
                }

                rules.Add(inputAllowUsers);
            }

            var inputMainRule = SelectedMainRule.ToInputRule();
            rules.Add(inputMainRule);

            IsWorking = true;
            MTProtoService.SetPrivacyAsync(new TLInputPrivacyKeyChatInvite(), 
                rules,
                result =>
                {
                    IsWorking = false;

                    //EventAggregator.Publish(new TLUpdatePrivacy{Key = new TLPrivacyKeyStatusTimestamp(), Rules = result.Rules});
                    BeginOnUIThread(() => NavigationService.GoBack());
                },
                error =>
                {
                    IsWorking = false;
                    if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK);
                    }

                    Execute.ShowDebugMessage("account.setPrivacy error " + error);
                });
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }

        public void Handle(TLUpdatePrivacy privacy)
        {
            if (privacy.Key is TLPrivacyKeyChatInvite)
            {
                _rules = new TLPrivacyRules { Rules = privacy.Rules };

                SelectedMainRule = GetSelectedMainRule(MainRules, _rules, MainRules[0], MainRules[1]);
                _selectedAllowUsers = GetSelectedRule<TLPrivacyValueAllowUsers>(_rules) ?? new TLPrivacyValueAllowUsers { Users = new TLVector<TLInt>() };
                _selectedDisallowUsers = GetSelectedRule<TLPrivacyValueDisallowUsers>(_rules) ?? new TLPrivacyValueDisallowUsers { Users = new TLVector<TLInt>() };
                SwitchUsersVisibility(false);

                UpdateSubtitles();
            }
        }
    }
}
