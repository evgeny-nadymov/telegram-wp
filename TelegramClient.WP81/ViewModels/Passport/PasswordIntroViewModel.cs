using System.Collections.Generic;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Passport
{
    public class PasswordIntroViewModel : ViewModelBase
    {
        private readonly TLPasswordBase _passwordBase;

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly IList<TLSecureValue> _secureValues;

        public PasswordIntroViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _passwordBase = StateService.Password;
            StateService.Password = null;

            _authorizationForm = StateService.AuthorizationForm;
            StateService.AuthorizationForm = null;

            _secureValues = StateService.SecureValues;
            StateService.SecureValues = null;
        }

        public void CreatePassword()
        {
            StateService.RemoveBackEntry = true;
            StateService.Password = _passwordBase;
            StateService.AuthorizationForm = _authorizationForm;
            StateService.SecureValues = _secureValues;
            NavigationService.UriFor<ChangePasswordViewModel>().Navigate();
        }
    }
}
