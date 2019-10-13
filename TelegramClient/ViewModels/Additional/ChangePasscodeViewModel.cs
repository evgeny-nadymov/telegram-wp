// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class ChangePasscodeViewModel : ViewModelBase
    {
        private string _passcode;

        public string Passcode
        {
            get { return _passcode; }
            set
            {
                SetField(ref _passcode, value, () => Passcode);
                NotifyOfPropertyChange(() => IsPasscodeValid);
            }
        }

        private string _confirmPasscode;

        public string ConfirmPasscode
        {
            get { return _confirmPasscode; }
            set
            {
                SetField(ref _confirmPasscode, value, () => ConfirmPasscode);
                NotifyOfPropertyChange(() => IsPasscodeValid);
            }
        }

        private PasscodeTypeItem _selectedPasscodeType;

        public PasscodeTypeItem SelectedPasscodeType
        {
            get { return _selectedPasscodeType; }
            set
            {
                SetField(ref _selectedPasscodeType, value, () => SelectedPasscodeType);
                NotifyOfPropertyChange(() => IsPasscodeValid);
                NotifyOfPropertyChange(() => Simple);
            }
        }

        public bool Simple
        {
            get { return _selectedPasscodeType != null && _selectedPasscodeType.Type == PasscodeType.Pin; }
        }

        public IList<PasscodeTypeItem> PasscodeTypes { get; set; } 

        public bool IsPasscodeValid
        {
            get
            {
                return
                    CheckPasscode(Passcode)
                    && string.Equals(Passcode, ConfirmPasscode);
            }
        }

        public bool CheckPasscode(string passcode)
        {
            if (string.IsNullOrEmpty(passcode))
            {
                return false;
            }

            if (Simple)
            {
                return PasscodeUtils.CheckSimple(passcode);
            }

            return true;
        }

        private int _selectedAutolockTimeout;

        public ChangePasscodeViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            PasscodeTypes = new List<PasscodeTypeItem>();
            PasscodeTypes.Add(new PasscodeTypeItem(AppResources.Pin, PasscodeType.Pin));
            PasscodeTypes.Add(new PasscodeTypeItem(AppResources.Passcode, PasscodeType.Passcode));

            _selectedPasscodeType = PasscodeTypes.First();
            _selectedAutolockTimeout = StateService.SelectedAutolockTimeout;
            StateService.SelectedAutolockTimeout = 0;
        }

        public void Done()
        {
            if (!IsPasscodeValid) return;

            var isSimplePasscode = _selectedPasscodeType != null && _selectedPasscodeType.Type == PasscodeType.Pin;
            if (isSimplePasscode)
            {
                isSimplePasscode = PasscodeUtils.CheckSimple(Passcode);
            }

            PasscodeUtils.SetParams(Passcode, isSimplePasscode, _selectedAutolockTimeout);
            UpdateDeviceLockedAsync();
            NavigationService.GoBack();
        }

        private void UpdateDeviceLockedAsync()
        {
            var shellViewModel = IoC.Get<ShellViewModel>();
            if (shellViewModel != null)
            {
                shellViewModel.UpdateDeviceLockedAsync();
            }
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }
    }

    public class PasscodeTypeItem
    {
        public string Caption { get; set; }

        public PasscodeType Type { get; set; }

        public PasscodeTypeItem(string caption, PasscodeType type)
        {
            Caption = caption;
            Type = type;
        }
    }

    public enum PasscodeType
    {
        Pin,
        Passcode
    }
}
