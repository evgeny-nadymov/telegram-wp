// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using System.Windows;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.TL;

namespace TelegramClient.ViewModels.Dialogs
{
    public class InlineBotResultsViewModel : TelegramPropertyChangedBase
    {
        public string SwitchPMString { get { return SwitchPM != null ? SwitchPM.Text.ToString() : null; } }

        public Visibility SwitchPMVisibility { get { return SwitchPM != null ? Visibility.Visible : Visibility.Collapsed; } }

        private TLInlineBotSwitchPM _switchPM;

        public TLInlineBotSwitchPM SwitchPM
        {
            get { return _switchPM; }
            set
            {
                if (_switchPM != value)
                {
                    _switchPM = value;
                    NotifyOfPropertyChange(() => SwitchPM);
                    NotifyOfPropertyChange(() => SwitchPMString);
                    NotifyOfPropertyChange(() => SwitchPMVisibility);
                }
            }
        }

        private bool _gallery;

        public bool Gallery
        {
            get { return _gallery; }
            set
            {
                if (_gallery != value)
                {
                    _gallery = value;
                    NotifyOfPropertyChange(() => Gallery);
                }
            }
        }

        private string _debug;

        public string Debug
        {
            get { return _debug; }
            set
            {
                if (_debug != value)
                {
                    _debug = value;
                    NotifyOfPropertyChange(() => Debug);
                }
            }
        }

        public ObservableCollection<TLBotInlineResultBase> Results { get; protected set; }

        private readonly System.Action _loadNextSliceAction;

        private readonly System.Action<TLInlineBotSwitchPM> _switchPMAction; 

        public InlineBotResultsViewModel(System.Action loadNextSliceAction, System.Action<TLInlineBotSwitchPM> switchPMAction)
        {
            _loadNextSliceAction = loadNextSliceAction;
            _switchPMAction = switchPMAction;

            Results = new ObservableCollection<TLBotInlineResultBase>();
        }

        public void LoadNextSlice()
        {
            _loadNextSliceAction.SafeInvoke();
        }

        public void Switch()
        {
            _switchPMAction.SafeInvoke(SwitchPM);
        }

        public void StartPlayers()
        {
            
        }
    }
}
