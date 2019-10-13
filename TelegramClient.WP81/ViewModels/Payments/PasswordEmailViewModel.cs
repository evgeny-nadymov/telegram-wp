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
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Payments
{
    public class PasswordEmailViewModel : ChangePasswordViewModelBase
    {
        private string _password;

        public string Password
        {
            get { return _password; }
            set { SetField(ref _password, value, () => Password); }
        }

        private string _confirmPassword;

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { SetField(ref _confirmPassword, value, () => ConfirmPassword); }
        }

        private string _recoveryEmail;

        public string RecoveryEmail
        {
            get { return _recoveryEmail; }
            set { SetField(ref _recoveryEmail, value, () => RecoveryEmail); }
        }

        public PasswordEmailViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler,
            IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService,
            ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {

        }

        public void Create()
        {
            if (_passwordBase == null) return;

            HasError = false;
            Error = " ";

            if (string.IsNullOrEmpty(_password))
            {
                HasError = true;
                Error = "invalid_password";
                return;
            }

            if (!string.Equals(_password, _confirmPassword))
            {
                HasError = true;
                Error = "invalid_confirmpassword";
                return;
            }

            if (!ChangePasswordEmailViewModel.IsValidEmail(_recoveryEmail))
            {
                HasError = true;
                Error = "invalid_email";
                return;
            }

            var password84 = _passwordBase as TLPassword84;
            if (password84 == null) return;

            var newAlgo = password84.NewAlgo as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (newAlgo == null) return;

            Utils.Password.AddClientSalt(newAlgo);

            var newSettings = new TLPasswordInputSettings83
            {
                Flags = new TLInt(0),
                NewAlgo = newAlgo,
                NewPasswordHash = null,
                Hint = TLString.Empty,
                Email = new TLString(RecoveryEmail),
                NewPassword = _password
            };

            UpdatePasswordSettings(password84, newSettings);
        }
    }
}
