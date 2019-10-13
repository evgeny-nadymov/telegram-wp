// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;

namespace TelegramClient.ViewModels.Additional
{
    public class MassDeleteReportSpamViewModel : TelegramPropertyChangedBase
    {
        private bool _deleteMessages;

        public bool DeleteMessages
        {
            get { return _deleteMessages; }
            set
            {
                if (_deleteMessages != value)
                {
                    _deleteMessages = value;
                    NotifyOfPropertyChange(() => DeleteMessages);
                }
            }
        }
        
        private bool _banUser;

        public bool BanUser
        {
            get { return _banUser; }
            set
            {
                if (_banUser != value)
                {
                    _banUser = value;
                    NotifyOfPropertyChange(() => BanUser);
                }
            }
        }

        private bool _reportSpam;

        public bool ReportSpam
        {
            get { return _reportSpam; }
            set
            {
                if (_reportSpam != value)
                {
                    _reportSpam = value;
                    NotifyOfPropertyChange(() => ReportSpam);
                }
            }
        }

        private bool _deleteAllMessages;

        public bool DeleteAllMessages
        {
            get { return _deleteAllMessages; }
            set
            {
                if (_deleteAllMessages != value)
                {
                    _deleteAllMessages = value;
                    NotifyOfPropertyChange(() => DeleteAllMessages);
                }
            }
        }

        public string DeleteAllFromString
        {
            get
            {
                var user = _with as TLUserBase;
                if (user != null)
                {
                    return string.Format(AppResources.DeleteAllFrom.ToLowerInvariant(), user.FullName);
                }

                return null;
            }
        }

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

        public bool CanDone
        {
            get { return DeleteMessages || ReportSpam || BanUser || DeleteAllMessages; }
        }

        private readonly TLObject _with;

        public MassDeleteReportSpamViewModel(TLObject with)
        {
            _with = with;

            PropertyChanged += (sender, e) =>
            {
                if (Property.NameEquals(e.PropertyName, () => DeleteMessages)
                    || Property.NameEquals(e.PropertyName, () => ReportSpam)
                    || Property.NameEquals(e.PropertyName, () => BanUser)
                    || Property.NameEquals(e.PropertyName, () => DeleteAllMessages))
                {
                    NotifyOfPropertyChange(() => CanDone);
                }
            };
        }

        public void Open()
        {
            IsOpen = true;
            DeleteMessages = true;
            BanUser = false;
            ReportSpam = false;
            DeleteAllMessages = false;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void Cancel()
        {
            Close();
        }

        public void Done()
        {
            if (!CanDone) return;
            
            Close();

            RaiseCompleted();
        }

        public event EventHandler Completed;

        protected virtual void RaiseCompleted()
        {
            var handler = Completed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }
    }
}
