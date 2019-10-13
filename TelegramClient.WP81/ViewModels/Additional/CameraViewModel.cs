// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class CameraViewModel : ViewModelBase
    {
        private TLPrivacyRuleBase _selectedMainRule;

        public TLPrivacyRuleBase SelectedMainRule
        {
            get { return _selectedMainRule; }
            set { SetField(ref _selectedMainRule, value, () => SelectedMainRule); }
        }

        public List<TLPrivacyRuleBase> MainRules { get; set; }

        private TLCameraSettings _settings;

        public CameraViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _settings = StateService.GetCameraSettings();

            MainRules = new List<TLPrivacyRuleBase>
            {
                new TLPrivacyValueAllowAll{ Label = AppResources.InAppCamera },
                new TLPrivacyValueAllowContacts{ Label = AppResources.ExternalCamera }
            };

            _selectedMainRule = _settings.External ? MainRules[1] : MainRules[0];
            _selectedMainRule.IsChecked = true;

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        protected override void OnDeactivate(bool close)
        {
            _settings.External = SelectedMainRule == MainRules[1];
            StateService.SaveCameraSettings(_settings);

            base.OnDeactivate(close);
        }
    }
}
