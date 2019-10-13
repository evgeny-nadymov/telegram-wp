// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class ClearCacheSettingsViewModel : TelegramPropertyChangedBase
    {

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    NotifyOfPropertyChange(() => IsOpen);
                }
            }
        }

        private readonly System.Action _clearCacheAction;

        public ClearCacheSettings Settings { get; set; }

        public ClearCacheSettingsViewModel(ClearCacheSettings settings, System.Action clearCacheAction)
        {
            _clearCacheAction = clearCacheAction;
            Settings = settings;
        }

        public void ClearCache()
        {
            _clearCacheAction.SafeInvoke();
            Close();
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }
    }
}
