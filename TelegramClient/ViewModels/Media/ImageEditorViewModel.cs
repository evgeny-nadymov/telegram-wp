// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.TL;

namespace TelegramClient.ViewModels.Media
{
    public class ImageEditorViewModel : TelegramPropertyChangedBase
    {
        public Action<TLMessage34> ContinueAction { get; set; }

        public string Caption
        {
            get
            {
                var message = _currentItem as TLMessage;
                if (message != null)
                {
                    var media = message.Media as TLMessageMediaPhoto28;
                    if (media != null)
                    {
                        return message.Message.ToString();
                    }
                }

                return null;
            }
            set
            {
                var message = _currentItem as TLMessage;
                if (message != null)
                {
                    var media = message.Media as TLMessageMediaPhoto28;
                    if (media != null)
                    {
                        message.Message = new TLString(value);
                    }
                }
            }
        }

        private TLMessage34 _currentItem;

        public TLMessage34 CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem != value)
                {
                    _currentItem = value;
                    NotifyOfPropertyChange(() => CurrentItem);
                    NotifyOfPropertyChange(() => Caption);
                }
            }
        }

        private bool _isOpen;

        public bool IsOpen { get { return _isOpen; } }

        public void Done()
        {
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);

            ContinueAction.SafeInvoke(_currentItem);
        }

        public void Cancel()
        {
            CloseEditor();    
        }

        public void OpenEditor()
        {
            _isOpen = CurrentItem != null;
            NotifyOfPropertyChange(() => IsOpen);
        }

        public void CloseEditor()
        {
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);

            _currentItem = null;
        }
    }
}
