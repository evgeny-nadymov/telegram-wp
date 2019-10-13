// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class ChangePasswordEmailViewModel : ChangePasswordViewModelBase
    {
        public Visibility SkipRecoveryEmailVisibility
        {
            get { return _newPasswordSettings != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        private string _recoveryEmail;

        public string RecoveryEmail
        {
            get { return _recoveryEmail; }
            set
            {
                SetField(ref _recoveryEmail, value, () => RecoveryEmail);
                NotifyOfPropertyChange(() => CanChangeRecoveryEmail);
            }
        }

        public ChangePasswordEmailViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {

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

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,6})+)$");
            var match = regex.Match(email);

            return match.Success;
        }

        public bool CanChangeRecoveryEmail
        {
            get { return IsValidEmail(_recoveryEmail); }
        }

        public void ChangeRecoveryEmail()
        {
            if (IsWorking) return;
            if (!CanChangeRecoveryEmail) return;

            var password84 = _passwordBase as TLPassword84;
            if (password84 == null) return;

            TLPasswordInputSettings newSettings;
            if (_newPasswordSettings != null)
            {
                newSettings = _newPasswordSettings;
                newSettings.Email = new TLString(RecoveryEmail);
            }
            else
            {
                newSettings = new TLPasswordInputSettings83
                {
                    Flags = new TLInt(0),
                    Email = new TLString(RecoveryEmail)
                };
            }

           UpdatePasswordSettings(password84, newSettings);
        }

        public void SkipRecoveryEmail()
        {
            if (IsWorking) return;
            if (_newPasswordSettings == null) return;

            var password84 = _passwordBase as TLPassword84;
            if (password84 == null) return;

            var result = MessageBox.Show(AppResources.SkipRecoveryEmailHint, AppResources.AppName, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                
                UpdatePasswordSettings(password84, _newPasswordSettings);
            }
        }
    }
}
