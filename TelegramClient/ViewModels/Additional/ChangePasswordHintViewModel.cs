// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class ChangePasswordHintViewModel : ChangePasswordViewModelBase
    {
        private string _passwordHint;

        public string PasswordHint
        {
            get { return _passwordHint; }
            set
            {
                SetField(ref _passwordHint, value, () => PasswordHint);
                NotifyOfPropertyChange(() => CanChangePasswordHint);
            }
        }

        public ChangePasswordHintViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            var password = PasswordViewModel.TempNewPassword;
            if (!string.IsNullOrEmpty(password) && password.Length > 2)
            {
                PasswordHint = string.Format("{0}{1}{2}",
                    password[0],
                    new string('*', password.Length - 2),
                    password[password.Length - 1]);
            }
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

        public bool CanChangePasswordHint
        {
            get
            {
                return !string.Equals(PasswordViewModel.TempNewPassword, PasswordHint);
            }
        }

        public void ChangePasswordHint()
        {
            if (_passwordBase == null) return;
            if (_newPasswordSettings == null) return;
            if (!CanChangePasswordHint)
            {
                HasError = true;
                Error = AppResources.PasswordHintError;
                return;
            }

            HasError = false;
            Error = string.Empty;

            var newSettings = _newPasswordSettings;
            newSettings.Hint = new TLString(PasswordHint);

            var password = _passwordBase as TLPassword84;
            if (password != null && password.HasRecovery.Value)
            {
                UpdatePasswordSettings(password, newSettings);

                return;
            }

            StateService.Password = _passwordBase;
            StateService.NewPasswordSettings = newSettings;
            StateService.RemoveBackEntry = true;
            StateService.AuthorizationForm = _authorizationForm;
            StateService.SecureValues = _secureValues;
            NavigationService.UriFor<ChangePasswordEmailViewModel>().Navigate();
        }

        
    }
}
